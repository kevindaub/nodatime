#region Copyright and license information
// Copyright 2001-2009 Stephen Colebourne
// Copyright 2009-2010 Jon Skeet
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

using NodaTime.Fields;
using NodaTime.Periods;

namespace NodaTime.Format
{
    /// <summary>
    /// Factory that creates complex instances of PeriodFormatter via method calls.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Period formatting is performed by the <see cref="PeriodFormatter"/> class.
    /// Three classes provide factory methods to create formatters, and this is one.
    /// The others are <see cref="PeriodFormats"/> and <see cref="IsoPeriodFormats"/>.
    /// </para>
    /// <para>
    /// PeriodFormatterBuilder is used for constructing formatters which are then
    /// used to print or parse. The formatters are built by appending specific fields
    /// or other formatters to an instance of this builder.
    /// </para>
    /// <para>
    /// For example, a formatter that prints years and months, like "15 years and 8 months",
    /// can be constructed as follows:
    /// <code>
    /// * PeriodFormatter yearsAndMonths = new PeriodFormatterBuilder()
    ///     .PrintZeroAlways()
    ///     .AppendYears()
    ///     .AppendSuffix(" year", " years")
    ///     .AppendSeparator(" and ")
    ///     .PrintZeroRarely()
    ///     .AppendMonths()
    ///     .AppendSuffix(" month", " months")
    ///     .ToFormatter();
    /// </code>
    /// </para>
    /// <para>
    /// PeriodFormatterBuilder itself is mutable and not thread-safe, but the
    /// formatters that it builds are thread-safe and immutable.
    /// </para>
    /// </remarks>
    public class PeriodFormatterBuilder
    {
        #region Internal classes

        private enum PrintZeroSetting
        {
            RarelyFirst = 1,
            RarelyLast,
            IfSupported,
            Always,
            Never
        }

        private enum FormatterDurationFieldType
        {
            Years,
            Months,
            Weeks,
            Days,
            HalfDays,
            Hours,
            Minutes,
            Seconds,
            Milliseconds,
            SecondsMilliseconds,
            SecondsMillisecondsOptional
        }

        private const int MaxField = (int)FormatterDurationFieldType.SecondsMillisecondsOptional + 1;

        /// <summary>
        /// Defines a formatted field's prefix or suffix text.
        /// This can be used for fields such as 'n hours' or 'nH' or 'Hour:n'.
        /// </summary>
        internal interface IPeriodFieldAffix
        {
            int CalculatePrintedLength(int value);

            void PrintTo(StringBuilder stringBuilder, int value);

            void PrintTo(TextWriter textWriter, int value);

            /// <summary>
            /// 
            /// </summary>
            /// <param name="periodString"></param>
            /// <param name="position"></param>
            /// <returns>new position after parsing affix, or ~position of failure</returns>
            int Parse(string periodString, int position);

            /// <summary>
            /// 
            /// </summary>
            /// <param name="periodstring"></param>
            /// <param name="position"></param>
            /// <returns>position where affix starts, or original ~position if not found</returns>
            int Scan(string periodstring, int position);
        }

        /// <summary>
        /// Implements an affix where the text does not vary by the amount.
        /// </summary>
        internal class SimpleAffix : IPeriodFieldAffix
        {
            private readonly string text;

            public SimpleAffix(string text)
            {
                this.text = text;
            }
             
            #region IPeriodFieldAffix Members

            public int CalculatePrintedLength(int value)
            {
                return text.Length;
            }

            public void PrintTo(StringBuilder stringBuilder, int value)
            {
                stringBuilder.Append(text);
            }

            public void PrintTo(TextWriter textWriter, int value)
            {
                textWriter.Write(text);
            }

            public int Parse(string periodString, int position)
            {
                return FormatUtils.MatchSubstring(periodString, position, text);
            }

            public int Scan(string periodString, int position)
            {
                for (int startAt = position; startAt < periodString.Length; startAt++)
                {
                    if (FormatUtils.MatchSubstring(periodString, startAt, text) > 0)
                        return startAt;

                    // Only allow number characters to be skipped in search of suffix.
                    switch (periodString[startAt]) 
                    {
                        case '0': case '1': case '2': case '3': case '4':
                        case '5': case '6': case '7': case '8': case '9':
                        case '.': case ',': case '+': case '-':
                            continue;
                        default:
                            return ~position;
                    }
                }
                return ~position;
            }

            #endregion
        }

        /// <summary>
        /// Implements an affix where the text varies by the amount of the field.
        /// Only singular (1) and plural (not 1) are supported.
        /// </summary>
        internal class PluralAffix : IPeriodFieldAffix
        {
            private readonly string singularText;
            private readonly string pluralText;

            public PluralAffix(string singularText, string pluralText)
            {
                this.singularText = singularText;
                this.pluralText = pluralText;
            }

            #region IPeriodFieldAffix Members

            public int CalculatePrintedLength(int value)
            {
                return (value == 1 ? singularText : pluralText).Length;
            }

            public void PrintTo(StringBuilder stringBuilder, int value)
            {
                stringBuilder.Append(value == 1 ? singularText : pluralText);
            }

            public void PrintTo(TextWriter textWriter, int value)
            {
                textWriter.Write(value == 1 ? singularText : pluralText);
            }

            public int Parse(string periodString, int position)
            {
                string firstToCheck;
                string secondToCheck;
                ArrangeByLength(out firstToCheck, out secondToCheck);

                int newPosition;
                if ((newPosition = FormatUtils.MatchSubstring(periodString, position, firstToCheck)) > 0)
                {
                    return newPosition;
                }
                return FormatUtils.MatchSubstring(periodString, position, secondToCheck);
            }

            public int Scan(string periodString, int position)
            {
                string firstToCheck;
                string secondToCheck;
                ArrangeByLength(out firstToCheck, out secondToCheck);

                for (int startAt = position; startAt < periodString.Length; startAt++)
                {
                    int newPosition;
                    if ((newPosition = FormatUtils.MatchSubstring(periodString, startAt, firstToCheck)) > 0)
                    {
                        return startAt;
                    }
                    if ((newPosition = FormatUtils.MatchSubstring(periodString, startAt, secondToCheck)) > 0)
                    {
                        return startAt;
                    }
                }
                return ~position;
            }

            private void ArrangeByLength(out string firstToCheck, out string secondToCheck)
            {
                if (singularText.Length > pluralText.Length)
                {
                    firstToCheck = singularText;
                    secondToCheck = pluralText;
                }
                else
                {
                    firstToCheck = pluralText;
                    secondToCheck = singularText;
                }
            }

            #endregion
        }

        /// <summary>
        /// Builds a composite affix by merging two other affix implementations.
        /// </summary>
        internal class CompositeAffix : IPeriodFieldAffix
        {
            readonly IPeriodFieldAffix left;
            readonly IPeriodFieldAffix right;

            public CompositeAffix(IPeriodFieldAffix left, IPeriodFieldAffix right)
            {
                this.left = left;
                this.right = right;
            }

            #region IPeriodFieldAffix Members

            public int CalculatePrintedLength(int value)
            {
                return left.CalculatePrintedLength(value)
                    + right.CalculatePrintedLength(value);
            }

            public void PrintTo(StringBuilder stringBuilder, int value)
            {
                left.PrintTo(stringBuilder, value);
                right.PrintTo(stringBuilder, value);
            }

            public void PrintTo(TextWriter textWriter, int value)
            {
                left.PrintTo(textWriter, value);
                right.PrintTo(textWriter, value);
            }

            public int Parse(string periodString, int position)
            {
                position = left.Parse(periodString, position);
                return position >= 0 ? right.Parse(periodString, position) : position;
            }

            public int Scan(string periodString, int position)
            {
                position = left.Scan(periodString, position);
                return position >= 0 ? right.Scan(periodString, position) : position;
            }

            #endregion
        }

        /// <summary>
        /// Handles a simple literal piece of text.
        /// </summary>
        internal class Literal : IPeriodPrinter, IPeriodParser
        {
            public static readonly Literal Empty = new Literal(String.Empty);

            private readonly string text;

            public Literal(string text)
            {
                this.text = text;
            }

            #region IPeriodPrinter Members

            public int CalculatePrintedLength(IPeriod period, IFormatProvider provider)
            {
                return text.Length;
            }

            public int CountFieldsToPrint(IPeriod period, int stopAt, IFormatProvider provider)
            {
                return 0;
            }

            public void PrintTo(StringBuilder stringBuilder, IPeriod period, IFormatProvider provider)
            {
                stringBuilder.Append(text);
            }

            public void PrintTo(TextWriter textWriter, IPeriod period, IFormatProvider provider)
            {
                textWriter.Write(text);
            }
            #endregion

            #region IPeriodParser Members

            public int Parse(string periodString, int position, PeriodBuilder builder, IFormatProvider provider)
            {
                return FormatUtils.MatchSubstring(periodString, position, text);
            }

            #endregion
        }

        /// <summary>
        /// Formats the numeric value of a field, potentially with prefix/suffix.
        /// </summary>
        private class FieldFormatter : IPeriodPrinter, IPeriodParser
        {
            private readonly int minPrintedDigits;
            private readonly PrintZeroSetting printZero;
            private readonly int maxParsedDigits;
            private readonly bool rejectSignedValues;

            private readonly FormatterDurationFieldType fieldType;
            private readonly FieldFormatter[] fieldFormatters;
            private readonly IPeriodFieldAffix prefix;
            private readonly IPeriodFieldAffix suffix;

            public FieldFormatter(int minPrintedDigits, PrintZeroSetting printZero, int maxParsedDigits,
                bool rejectSignedValues, FormatterDurationFieldType fieldType, FieldFormatter[] fieldFormatters,
                IPeriodFieldAffix prefix, IPeriodFieldAffix suffix)
            {
                this.minPrintedDigits = minPrintedDigits;
                this.printZero = printZero;
                this.maxParsedDigits = maxParsedDigits;
                this.rejectSignedValues = rejectSignedValues;
                this.fieldType = fieldType;
                this.fieldFormatters = fieldFormatters;
                this.prefix = prefix;
                this.suffix = suffix;
            }

            public FieldFormatter(FieldFormatter initialFieldFormatter, IPeriodFieldAffix suffix)
            {
                this.minPrintedDigits = initialFieldFormatter.minPrintedDigits;
                this.printZero = initialFieldFormatter.printZero;
                this.maxParsedDigits = initialFieldFormatter.maxParsedDigits;
                this.rejectSignedValues = initialFieldFormatter.rejectSignedValues;
                this.fieldType = initialFieldFormatter.fieldType;
                this.fieldFormatters = initialFieldFormatter.fieldFormatters;
                this.prefix = initialFieldFormatter.prefix;
                this.suffix = initialFieldFormatter.suffix == null ? suffix
                    : new CompositeAffix(initialFieldFormatter.suffix, suffix);
            }

            public FormatterDurationFieldType FieldType { get { return fieldType; } }

            #region IPeriodPrinter Members

            public int CalculatePrintedLength(IPeriod period, IFormatProvider provider)
            {
                var fieldValue = GetFieldValue(period);
                if (fieldValue == long.MaxValue)
                    return 0;

                int digitCount = Math.Max(minPrintedDigits, FormatUtils.CalculateDigitCount(fieldValue));
                if (fieldType >= FormatterDurationFieldType.SecondsMilliseconds)
                {
                    // valueLong contains the seconds and millis fields
                    // the minimum output is 0.000, which is 4 digits
                    digitCount = Math.Max(digitCount, 4);
                    // plus one for the decimal point
                    digitCount++;
                    if (fieldType == FormatterDurationFieldType.SecondsMillisecondsOptional &&
                            (Math.Abs(fieldValue) % NodaConstants.MillisecondsPerSecond) == 0)
                    {
                        digitCount -= 4; // remove three digits and decimal point
                    }
                    // reset valueLong to refer to the seconds part for the prefic/suffix calculation
                    fieldValue = fieldValue / NodaConstants.MillisecondsPerSecond;
                }

                var intVlaue = (int)fieldValue;
                if (prefix != null)
                    digitCount += prefix.CalculatePrintedLength(intVlaue);
                if (suffix != null)
                    digitCount += suffix.CalculatePrintedLength(intVlaue);

                return digitCount;
            }

            public int CountFieldsToPrint(IPeriod period, int stopAt, IFormatProvider provider)
            {
                if (stopAt < 0)
                {
                    return 0;
                }

                if (printZero == PrintZeroSetting.Always || GetFieldValue(period) != long.MaxValue)
                {
                    return 1;
                }

                return 0;
            }

            public void PrintTo(StringBuilder stringBuilder, IPeriod period, IFormatProvider provider)
            {
                long fieldValue = GetFieldValue(period);
                if (fieldValue == long.MaxValue)
                {
                    return;
                }

                int intValue = (int)fieldValue;
                if (fieldType >= FormatterDurationFieldType.SecondsMilliseconds)
                {
                    intValue = (int)(fieldValue / NodaConstants.MillisecondsPerSecond);
                }
                if (prefix != null)
                {
                    prefix.PrintTo(stringBuilder, intValue);
                }

                int minDigits = minPrintedDigits;
                if (minDigits <= 1)
                {
                    FormatUtils.AppendUnpaddedInteger(stringBuilder, intValue);
                }
                else
                {
                    FormatUtils.AppendPaddedInteger(stringBuilder, intValue, minDigits);
                }

                if (fieldType >= FormatterDurationFieldType.SecondsMilliseconds)
                {
                    int dp = (int)(Math.Abs(fieldValue) % NodaConstants.MillisecondsPerSecond);
                    if (fieldType == FormatterDurationFieldType.SecondsMilliseconds || dp > 0)
                    {
                        stringBuilder.Append('.');
                        FormatUtils.AppendPaddedInteger(stringBuilder, dp, 3);
                    }
                }

                if (suffix != null)
                {
                    suffix.PrintTo(stringBuilder, intValue);
                }
            }

            public void PrintTo(TextWriter textWriter, IPeriod period, IFormatProvider provider)
            {
                long fieldValue = GetFieldValue(period);
                if (fieldValue == long.MaxValue)
                {
                    return;
                }

                int intValue = (int)fieldValue;
                if (fieldType >= FormatterDurationFieldType.SecondsMilliseconds)
                {
                    intValue = (int)(fieldValue / NodaConstants.MillisecondsPerSecond);
                }

                if (prefix != null)
                {
                    prefix.PrintTo(textWriter, intValue);
                }

                int minDigits = minPrintedDigits;
                if (minDigits <= 1)
                {
                    FormatUtils.WriteUnpaddedInteger(textWriter, intValue);
                }
                else
                {
                    FormatUtils.WritePaddedInteger(textWriter, intValue, minDigits);
                }

                if (fieldType >= FormatterDurationFieldType.SecondsMilliseconds)
                {
                    int dp = (int)(Math.Abs(fieldValue) % NodaConstants.MillisecondsPerSecond);
                    if (fieldType == FormatterDurationFieldType.SecondsMilliseconds || dp > 0)
                    {
                        textWriter.Write('.');
                        FormatUtils.WritePaddedInteger(textWriter, dp, 3);
                    }
                }

                if (suffix != null)
                {
                    suffix.PrintTo(textWriter, intValue);
                }
            }

            #endregion

            #region IPeriodParser Members

            public int Parse(string periodString, int position, PeriodBuilder builder, IFormatProvider provider)
            {
                bool mustParse = (printZero == PrintZeroSetting.Always);

                // Shortcut test.
                if (position >= periodString.Length)
                {
                    return mustParse ? ~position : position;
                }

                if (prefix != null)
                {
                    position = prefix.Parse(periodString, position);
                    if (position >= 0)
                    {
                        // If prefix is found, then the parse must finish.
                        mustParse = true;
                    }
                    else
                    {
                        return mustParse ? position : ~position;
                    }
                }

                int suffixPos = -1;
                if (suffix != null)
                {
                    // Pre-scan the suffix, to help determine if this field must be
                    // parsed.
                    suffixPos = suffix.Scan(periodString, position);
                    if (suffixPos >= 0)
                    {
                        // If suffix is found, then parse must finish.
                        mustParse = true;
                    }
                    else
                    {
                        return mustParse ? suffixPos : ~suffixPos;
                    }
                }

                if (!mustParse && !IsSupported(builder.PeriodType,fieldType))
                {
                    // If parsing is not required and the field is not supported,
                    // exit gracefully so that another parser can continue on.
                    return position;
                }

                int limit = suffixPos > 0 
                    ? Math.Min(maxParsedDigits, suffixPos - position) 
                    : Math.Min(maxParsedDigits, periodString.Length - position);

                // validate input number
                int length = 0;
                int fractPos = -1;
                bool hasDigits = false;
                while (length < limit)
                {
                    char c = periodString[position + length];
                    // leading sign
                    if (length == 0 && (c == '-' || c == '+') && !rejectSignedValues)
                    {
                        bool negative = c == '-';

                        // Next character must be a digit.
                        if (length + 1 >= limit || !Char.IsDigit(periodString, position + length + 1))
                        {
                            break;
                        }

                        if (negative)
                        {
                            length++;
                        }
                        else
                        {
                            // Skip the '+' for parseInt to succeed.
                            position++;
                        }
                        // Expand the limit to disregard the sign character.
                        limit = Math.Min(limit + 1, periodString.Length - position);
                        continue;
                    }
                    // main number
                    if (Char.IsDigit(c))
                    {
                        hasDigits = true;
                    }
                    else
                    {
                        if ((c == '.' || c == ',')
                             && (fieldType == FormatterDurationFieldType.SecondsMilliseconds || fieldType == FormatterDurationFieldType.SecondsMillisecondsOptional))
                        {
                            if (fractPos >= 0)
                            {
                                // can't have two decimals
                                break;
                            }
                            fractPos = position + length + 1;
                            // Expand the limit to disregard the decimal point.
                            limit = Math.Min(limit + 1, periodString.Length - position);
                        }
                        else
                        {
                            break;
                        }
                    }
                    length++;
                }

                if (!hasDigits)
                {
                    return ~position;
                }

                if (suffixPos >= 0 && position + length != suffixPos)
                {
                    // If there are additional non-digit characters before the
                    // suffix is reached, then assume that the suffix found belongs
                    // to a field not yet reached. Return original position so that
                    // another parser can continue on.
                    return position;
                }

                if (fieldType != FormatterDurationFieldType.SecondsMilliseconds && fieldType != FormatterDurationFieldType.SecondsMillisecondsOptional)
                {
                    //Handle common case.
                    AppendFieldValue(builder, fieldType, ParseInt(periodString, position, length));
                }
                else if (fractPos < 0)
                {
                    AppendFieldValue(builder, FormatterDurationFieldType.Seconds, ParseInt(periodString, position, length));
                    AppendFieldValue(builder, FormatterDurationFieldType.Milliseconds, 0);
                }
                else
                {
                    int wholeValue = ParseInt(periodString, position, fractPos - position - 1);
                    AppendFieldValue(builder, FormatterDurationFieldType.Seconds, wholeValue);

                    int fractLen = position + length - fractPos;
                    int fractValue;
                    if (fractLen <= 0)
                    {
                        fractValue = 0;
                    }
                    else
                    {
                        if (fractLen >= 3)
                        {
                            fractValue = ParseInt(periodString, fractPos, 3);
                        }
                        else
                        {
                            fractValue = ParseInt(periodString, fractPos, fractLen);
                            if (fractLen == 1)
                            {
                                fractValue *= 100;
                            }
                            else
                            {
                                fractValue *= 10;
                            }
                        }
                        if (wholeValue < 0)
                        {
                            fractValue = -fractValue;
                        }
                    }

                    AppendFieldValue(builder, FormatterDurationFieldType.Milliseconds, fractValue);
                }

                position += length;

                if (position >= 0 && suffix != null)
                {
                    position = suffix.Parse(periodString, position);
                }

                return position;
            }

            #endregion

            private long GetFieldValue(IPeriod period)
            {
                long value;

                if (printZero != PrintZeroSetting.Always && !IsSupported(period.PeriodType, fieldType))
                {
                    return long.MaxValue;
                }

                switch (fieldType)
                {
                    case FormatterDurationFieldType.Years:
                        value = period.Get(DurationFieldType.Years);
                        break;
                    case FormatterDurationFieldType.Months:
                        value = period.Get(DurationFieldType.Months);
                        break;
                    case FormatterDurationFieldType.Weeks:
                        value = period.Get(DurationFieldType.Weeks);
                        break;
                    case FormatterDurationFieldType.Days:
                        value = period.Get(DurationFieldType.Days);
                        break;
                    case FormatterDurationFieldType.Hours:
                        value = period.Get(DurationFieldType.Hours);
                        break;
                    case FormatterDurationFieldType.Minutes:
                        value = period.Get(DurationFieldType.Minutes);
                        break;
                    case FormatterDurationFieldType.Seconds:
                        value = period.Get(DurationFieldType.Seconds);
                        break;
                    case FormatterDurationFieldType.Milliseconds:
                        value = period.Get(DurationFieldType.Milliseconds);
                        break;
                    case FormatterDurationFieldType.SecondsMilliseconds: // drop through
                    case FormatterDurationFieldType.SecondsMillisecondsOptional:
                        int seconds = period.Get(DurationFieldType.Seconds);
                        int millis = period.Get(DurationFieldType.Milliseconds);
                        value = (seconds * (long)NodaConstants.MillisecondsPerSecond) + millis;
                        break;
                    default:
                        return long.MaxValue;
                }

                if (value == 0)
                {
                    switch (printZero)
                    {
                        case PrintZeroSetting.Never:
                            return long.MaxValue;
                        case PrintZeroSetting.RarelyLast:
                            if (!IsLastFieldInEmptyPeriod(period))
                            {
                                return long.MaxValue;
                            }
                            break;
                        case PrintZeroSetting.RarelyFirst:
                            if (!IsFirstFieldInEmptyPeriod(period))
                            {
                                return long.MaxValue;
                            }
                            break;
                    }
                }
                return value;
            }

            //determines if a given period is zero and this is the last formatted field
            private bool IsLastFieldInEmptyPeriod(IPeriod period)
            {
                if (!IsZero(period) || fieldFormatters[(int)fieldType] != this)
                    return false;

                for (int nextFieldType = (int)fieldType + 1; nextFieldType < MaxField; nextFieldType++)
                {
                    if (IsSupported(period.PeriodType, (FormatterDurationFieldType)nextFieldType) && fieldFormatters[nextFieldType] != null)
                    {
                        return false;
                    }                        
                }
                return true;
            }

            //determines if a given period is zero and this is the first formatted field
            private bool IsFirstFieldInEmptyPeriod(IPeriod period)
            {
                if (!IsZero(period) || fieldFormatters[(int)fieldType] != this)
                    return false;

                for (int previousFieldType = Math.Min((int)fieldType, (int)FormatterDurationFieldType.Milliseconds) - 1; previousFieldType >= 0; previousFieldType--)
                {
                    if (IsSupported(period.PeriodType, (FormatterDurationFieldType)previousFieldType) && fieldFormatters[previousFieldType] != null)
                    {
                        return false;
                    }
                }
                return true;
            }

            private static int ParseInt(String text, int position, int length)
            {
                if (length >= 10)
                {
                    // Since value may exceed max, use stock parser which checks for this.
                    return Int32.Parse(text.Substring(position, position + length));
                }
                if (length <= 0)
                {
                    return 0;
                }
                int value = text[position++];
                length--;
                bool negative;
                if (value == '-')
                {
                    if (--length < 0)
                    {
                        return 0;
                    }
                    negative = true;
                    value = text[position++];
                }
                else
                {
                    negative = false;
                }
                value -= '0';
                while (length-- > 0)
                {
                    value = ((value << 3) + (value << 1)) + text[position++] - '0';
                }
                return negative ? -value : value;
            }

            private static bool IsZero(IPeriod period) 
            {
                for (int i = 0, isize = period.Size; i < isize; i++) 
                {
                    if (period.GetValue(i) != 0)
                    {
                        return false;
                    }
                }
                return true;
            }

            private static void AppendFieldValue(PeriodBuilder builder, FormatterDurationFieldType fieldType, int value)
            {
                switch (fieldType)
                {
                    case FormatterDurationFieldType.Years:
                        builder.Append(DurationFieldType.Years, value);
                        break;
                    case FormatterDurationFieldType.Months:
                        builder.Append(DurationFieldType.Months, value);
                        break;
                    case FormatterDurationFieldType.Weeks:
                        builder.Append(DurationFieldType.Weeks, value);
                        break;
                    case FormatterDurationFieldType.Days:
                        builder.Append(DurationFieldType.Days, value);
                        break;
                    case FormatterDurationFieldType.Hours:
                        builder.Append(DurationFieldType.Hours, value);
                        break;
                    case FormatterDurationFieldType.Minutes:
                        builder.Append(DurationFieldType.Minutes, value);
                        break;
                    case FormatterDurationFieldType.Seconds:
                        builder.Append(DurationFieldType.Seconds, value);
                        break;
                    case FormatterDurationFieldType.Milliseconds:
                        builder.Append(DurationFieldType.Milliseconds, value);
                        break;
                    default:
                        break;
                }
            }

            private static bool IsSupported(PeriodType type, FormatterDurationFieldType fieldType)
            {
                switch (fieldType)
                {
                    default:
                        return false;
                    case FormatterDurationFieldType.Years:
                        return type.IsSupported(DurationFieldType.Years);
                    case FormatterDurationFieldType.Months:
                        return type.IsSupported(DurationFieldType.Months);
                    case FormatterDurationFieldType.Weeks:
                        return type.IsSupported(DurationFieldType.Weeks);
                    case FormatterDurationFieldType.Days:
                        return type.IsSupported(DurationFieldType.Days);
                    case FormatterDurationFieldType.Hours:
                        return type.IsSupported(DurationFieldType.Hours);
                    case FormatterDurationFieldType.Minutes:
                        return type.IsSupported(DurationFieldType.Minutes);
                    case FormatterDurationFieldType.Seconds:
                        return type.IsSupported(DurationFieldType.Seconds);
                    case FormatterDurationFieldType.Milliseconds:
                        return type.IsSupported(DurationFieldType.Milliseconds);
                    case FormatterDurationFieldType.SecondsMilliseconds: 
                        // drop through
                    case FormatterDurationFieldType.SecondsMillisecondsOptional:
                        return type.IsSupported(DurationFieldType.Seconds) ||
                               type.IsSupported(DurationFieldType.Milliseconds);
                }
            }
        }

        /// <summary>
        ///  Composite implementation that merges other fields to create a full pattern.
        /// </summary>
        private class Composite : IPeriodParser, IPeriodPrinter
        {
            private readonly IList<IPeriodPrinter> periodPrinters;
            private readonly IList<IPeriodParser>  periodParsers;

            public Composite(IList<IPeriodPrinter> periodPrinters, IList<IPeriodParser> periodParsers)
            {
                this.periodPrinters = periodPrinters;
                this.periodParsers = periodParsers;
            }

            public IList<IPeriodParser> Parsers { get { return periodParsers; } }
            public IList<IPeriodPrinter> Printers { get { return periodPrinters; } }

            #region IPeriodParser Members

            public int Parse(string periodString, int position, PeriodBuilder builder, IFormatProvider provider)
            {
                int len = periodParsers.Count;
                for (int i = 0; i < len && position >= 0; i++)
                {
                    position = periodParsers[i].Parse(periodString, position, builder, provider);
                }
                return position;
            }

            #endregion

            #region IPeriodPrinter Members

            public int CalculatePrintedLength(IPeriod period, IFormatProvider provider)
            {
                int sum = 0;
                for (int i = periodPrinters.Count; i > 0; --i)
                {
                    sum += periodPrinters[i - 1].CalculatePrintedLength(period, provider);
                }
                return sum;
            }

            public int CountFieldsToPrint(IPeriod period, int stopAt, IFormatProvider provider)
            {
                int sum = 0;
                for (int i = periodPrinters.Count; sum < stopAt && i > 0; --i)
                {
                    sum += periodPrinters[i - 1].CountFieldsToPrint(period, Int32.MaxValue, provider);
                }
                return sum;
            }

            public void PrintTo(StringBuilder stringBuilder, IPeriod period, IFormatProvider provider)
            {
                foreach (var printer in periodPrinters)
                {
                    printer.PrintTo(stringBuilder, period, provider);
                }
            }

            public void PrintTo(TextWriter textWriter, IPeriod period, IFormatProvider provider)
            {
                foreach (var printer in periodPrinters)
                {
                    printer.PrintTo(textWriter, period, provider);
                }
            }

            #endregion
        }

        /// <summary>
        /// Handles a separator, that splits the fields into multiple parts.
        /// For example, the 'T' in the ISO8601 standard.
        /// </summary>
        private class Separator : IPeriodPrinter, IPeriodParser
        {
            private readonly string text;
            private readonly string finalText;
            private readonly string[] parsedForms;
            private readonly bool useBefore;
            private readonly bool useAfter;

            private readonly IPeriodPrinter beforePrinter;
            private IPeriodPrinter afterPrinter;
            private readonly IPeriodParser beforeParser;
            private IPeriodParser afterParser;

            public Separator(string text, string finalText, string[] variants,
                IPeriodPrinter beforePrinter, IPeriodParser beforeParser,
                bool useBefore, bool useAfter)
            {
                this.text = text;
                this.finalText = finalText;

                if ((finalText == null || text.Equals(finalText, StringComparison.Ordinal))
                    && (variants == null || variants.Length == 0))
                {
                    parsedForms = new string[] { text };
                }
                else
                {
                    // filter unique strings
                    var uniqueStrings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    if (!uniqueStrings.ContainsKey(text))
                    {
                        uniqueStrings.Add(text, text);
                    }
                    if (!uniqueStrings.ContainsKey(finalText))
                    {
                        uniqueStrings.Add(finalText, finalText);
                    }
                    if (variants != null)
                    {
                        foreach (var variant in variants)
                        {
                            uniqueStrings.Add(variant, variant);
                        }
                    }
                    parsedForms = new string[uniqueStrings.Keys.Count];
                    uniqueStrings.Keys.CopyTo(parsedForms, 0);

                    //sort in revered order
                    Array.Sort(parsedForms, (first, second) => second.CompareTo(first));
                }

                this.beforePrinter = beforePrinter;
                this.beforeParser = beforeParser;
                this.useBefore = useBefore;
                this.useAfter = useAfter;
            }

            public Separator Finish(IPeriodPrinter afterPrinter, IPeriodParser afterParser)
            {
                this.afterPrinter = afterPrinter;
                this.afterParser = afterParser;

                return this;
            }

            #region IPeriodPrinter Members

            public int CalculatePrintedLength(IPeriod period, IFormatProvider provider)
            {
                int sum = beforePrinter.CalculatePrintedLength(period, provider)
                    + afterPrinter.CalculatePrintedLength(period, provider);

                if (useBefore)
                {
                    if (beforePrinter.CountFieldsToPrint(period, 1, provider) > 0)
                    {
                        if (useAfter)
                        {
                            int afterCount = afterPrinter.CountFieldsToPrint(period, 2, provider);
                            if (afterCount > 0)
                            {
                                sum += ((afterCount > 1) ? text : finalText).Length;
                            }
                        }
                        else
                        {
                            sum += text.Length;
                        }
                    }
                }
                else if (useAfter && afterPrinter.CountFieldsToPrint(period, 1, provider) > 0)
                {
                    sum += text.Length;
                }

                return sum;
            }

            public int CountFieldsToPrint(IPeriod period, int stopAt, IFormatProvider provider)
            {
                int sum = beforePrinter.CountFieldsToPrint(period, stopAt, provider);
                if (sum < stopAt)
                {
                    sum += afterPrinter.CountFieldsToPrint(period, stopAt, provider);
                }
                return sum;
            }

            public void PrintTo(StringBuilder stringBuilder, IPeriod period, IFormatProvider provider)
            {
                beforePrinter.PrintTo(stringBuilder, period, provider);
                if (useBefore)
                {
                    if (beforePrinter.CountFieldsToPrint(period, 1, provider) > 0)
                    {
                        if (useAfter)
                        {
                            int afterCount = afterPrinter.CountFieldsToPrint(period, 2, provider);
                            if (afterCount > 0)
                            {
                                stringBuilder.Append(afterCount > 1 ? text : finalText);
                            }
                        }
                        else
                        {
                            stringBuilder.Append(text);
                        }
                    }
                }
                else if (useAfter && afterPrinter.CountFieldsToPrint(period, 1, provider) > 0)
                {
                    stringBuilder.Append(text);
                }
                afterPrinter.PrintTo(stringBuilder, period, provider);
            }

            public void PrintTo(TextWriter textWriter, IPeriod period, IFormatProvider provider)
            {
                beforePrinter.PrintTo(textWriter, period, provider);
                if (useBefore)
                {
                    if (beforePrinter.CountFieldsToPrint(period, 1, provider) > 0)
                    {
                        if (useAfter)
                        {
                            int afterCount = afterPrinter.CountFieldsToPrint(period, 2, provider);
                            if (afterCount > 0)
                            {
                                textWriter.Write(afterCount > 1 ? text : finalText);
                            }
                        }
                        else
                        {
                           textWriter.Write(text);
                        }
                    }
                }
                else if (useAfter && afterPrinter.CountFieldsToPrint(period, 1, provider) > 0)
                {
                    textWriter.Write(text);
                }
                afterPrinter.PrintTo(textWriter, period, provider);
            }

            #endregion

            #region IPeriodParser Members

            public int Parse(string periodString, int position, PeriodBuilder builder, IFormatProvider provider)
            {
                int oldPos = position;
                position = beforeParser.Parse(periodString, position, builder, provider);

                if (position < 0)
                {
                    return position;
                }

                bool found = false;
                if (position > oldPos)
                {
                    // Consume this separator.
                    for (int i = 0; i < parsedForms.Length; i++)
                    {
                        int newPosition;
                        if((newPosition = FormatUtils.MatchSubstring(periodString, position, parsedForms[i])) > 0)
                        {
                            position = newPosition;
                            found = true;
                            break;
                        }
                    }
                }

                oldPos = position;
                position = afterParser.Parse(periodString, position, builder, provider);

                if (position < 0)
                {
                    return position;
                }

                if (found && position == oldPos)
                {
                    // Separator should not have been supplied.
                    return ~oldPos;
                }

                if (position > oldPos && !found && !useBefore)
                {
                    // Separator was required.
                    return ~oldPos;
                }

                return position;
            }

            #endregion
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="PeriodFormatterBuilder"/> class 
        /// with default values.
        /// </summary>
        public PeriodFormatterBuilder()
        {
            Clear();
        }

        /// <summary>
        /// Clears out all the appended elements, allowing this builder to be reused.
        /// </summary>
        public void Clear()
        {
            minimumPrintedDigits = 1;
            printZero = PrintZeroSetting.RarelyLast;
            maximumParsedDigits = 10;
            rejectSignedValues = false;
            prefix = null;

            printers.Clear();
            parsers.Clear();
            notParser = false;
            notPrinter = false;
            fieldFormatters = new FieldFormatter[MaxField];
        }

        #region Options

        private PrintZeroSetting printZero;
        private int maximumParsedDigits;
        private int minimumPrintedDigits;
        private bool rejectSignedValues;

        /// <summary>
        /// Always print zero values for the next and following appended fields,
        /// even if the period doesn't support it. The parser requires values for
        /// fields that always print zero.
        /// </summary>
        /// <returns>This PeriodFormatterBuilder</returns>
        public PeriodFormatterBuilder PrintZeroAlways()
        {
            printZero = PrintZeroSetting.Always;
            return this;
        }

        /// <summary>
        /// Never print zero values for the next and following appended fields,
        /// unless no fields would be printed. If no fields are printed, the printer
        /// forces the last "printZeroRarely" field to print a zero.
        /// </summary>
        /// <returns>This PeriodFormatterBuilder</returns>
        public PeriodFormatterBuilder PrintZeroNever()
        {
            printZero = PrintZeroSetting.Never;
            return this;
        }

        /// <summary>
        /// Print zero values for the next and following appened fields only if the
        /// period supports it.
        /// </summary>
        /// <returns>This PeriodFormatterBuilder</returns>
        public PeriodFormatterBuilder PrintZeroIfSupported()
        {
            printZero = PrintZeroSetting.IfSupported;
            return this;
        }

        /// <summary>
        /// Never print zero values for the next and following appended fields,
        /// unless no fields would be printed. If no fields are printed, the printer
        /// forces the first "printZeroRarely" field to print a zero.
        /// </summary>
        /// <returns>This PeriodFormatterBuilder</returns>
        public PeriodFormatterBuilder PrintZeroRarelyFirst()
        {
            printZero = PrintZeroSetting.RarelyFirst;
            return this;
        }

        /// <summary>
        /// Never print zero values for the next and following appended fields,
        /// unless no fields would be printed. If no fields are printed, the printer
        /// forces the last "printZeroRarely" field to print a zero.
        /// </summary>
        /// <returns>This PeriodFormatterBuilder</returns>
        public PeriodFormatterBuilder PrintZeroRarelyLast()
        {
            printZero = PrintZeroSetting.RarelyLast;
            return this;
        }

        /// <summary>
        /// Set the minimum digits printed for the next and following appended
        /// fields. By default, the minimum digits printed is one. If the field value
        /// is zero, it is not printed unless a printZero rule is applied.
        /// </summary>
        /// <param name="minDigits">The minimum digits value</param>
        /// <returns>This PeriodFormatterBuilder</returns>
        public PeriodFormatterBuilder MinimumPrintedDigits(int minDigits)
        {
            minimumPrintedDigits = minDigits;
            return this;
        }

        /// <summary>
        /// Set the maximum digits parsed for the next and following appended
        /// fields. By default, the maximum digits parsed is ten.
        /// </summary>
        /// <param name="maxDigits">The maximum digits value</param>
        /// <returns>This PeriodFormatterBuilder</returns>
        public PeriodFormatterBuilder MaximumParsedDigits(int maxDigits)
        {
            maximumParsedDigits = maxDigits;
            return this;
        }

        /// <summary>
        /// Reject signed values when parsing the next and following appended fields.
        /// </summary>
        /// <param name="reject">Set true to reject, false otherwise</param>
        /// <returns>This PeriodFormatterBuilder</returns>
        public PeriodFormatterBuilder RejectSignedValues(bool reject)
        {
            rejectSignedValues = reject;
            return this;
        }

        #endregion

        #region Append

        private IPeriodFieldAffix prefix;
        private readonly List<IPeriodPrinter> printers = new List<IPeriodPrinter>();
        private readonly List<IPeriodParser> parsers = new List<IPeriodParser>();

        private bool notPrinter;
        private bool notParser;
        private FieldFormatter[] fieldFormatters;

        private void VerifyPrefix()
        {
            if (prefix != null)
            {
                throw new InvalidOperationException("Prefix not followed by field");
            }

            prefix = null;
        }

        private int PairsCount { get { return printers.Count; } }

        private PeriodFormatterBuilder AppendImpl(IPeriodPrinter printer, IPeriodParser parser)
        {
            printers.Add(printer);
            parsers.Add(parser);
            notPrinter |= (printer == null);
            notParser |= (parser == null);

            return this;
        }

        /// <summary>
        /// Appends another formatter.
        /// </summary>
        /// <param name="formatter">Another formatter to append</param>
        /// <exception cref="ArgumentNullException"> If formatter is null</exception>
        /// <returns>This PeriodFormatterBuilder</returns>
        public PeriodFormatterBuilder Append(PeriodFormatter formatter)
        {
            if (formatter == null)
            {
                throw new ArgumentNullException("formatter");
            }

            VerifyPrefix();
            return AppendImpl(formatter.Printer, formatter.Parser);
        }

        /// <summary>
        /// Appends a printer parser pair.
        /// <remarks>
        /// Either the printer or the parser may be null, in which case the builder will
        /// be unable to produce a parser or printer repectively.
        /// </remarks>
        /// </summary>
        /// <exception cref="ArgumentException">If both the printer and parser are null</exception>
        /// <param name="printer">Appends a printer to the builder, null if printing is not supported</param>
        /// <param name="parser">Appends a parser to the builder, null if parsing is not supported</param>
        /// <returns>This PeriodFormatterBuilder</returns>
        public PeriodFormatterBuilder Append(IPeriodPrinter printer, IPeriodParser parser)
        {
            if (printer == null && parser == null)
            {
                throw new ArgumentException("No printer or parser supplied");
            }

            VerifyPrefix();
            return AppendImpl(printer, parser);
        }

        /// <summary>
        /// Instructs the printer to emit specific text, and the parser to expect it.
        /// The parser is case-insensitive.
        /// </summary>
        /// <exception cref="ArgumentNullException">If text is null or empty string</exception>
        /// <param name="text">The text of the literal to append</param>
        /// <returns>This PeriodFormatterBuilder</returns>
        public PeriodFormatterBuilder AppendLiteral(string text)
        {
            if (String.IsNullOrEmpty(text))
            {
                throw new ArgumentNullException("text");
            }

            VerifyPrefix();
            var newLiteral = new Literal(text);
            return AppendImpl(newLiteral, newLiteral);
        }

        #region Fields

        private PeriodFormatterBuilder AppendField(FormatterDurationFieldType fieldType)
        {
            return AppendField(fieldType, minimumPrintedDigits);
        }

        private PeriodFormatterBuilder AppendField(FormatterDurationFieldType fieldType, int minDigits)
        {
            var newFieldFormatter = new FieldFormatter(minDigits, printZero, maximumParsedDigits,
                rejectSignedValues, fieldType, fieldFormatters, prefix, null);
            fieldFormatters[(int)fieldType] = newFieldFormatter;
            prefix = null;

            return AppendImpl(newFieldFormatter, newFieldFormatter);
        }

        /// <summary>
        /// Instruct the printer to emit an integer years field, if supported.
        /// </summary>
        /// <remarks>
        /// The number of printed and parsed digits can be controlled using
        /// <see cref="MinimumPrintedDigits"/>
        /// and <see cref="MaximumParsedDigits"/>
        /// </remarks>
        /// <returns>This PeriodFormatterBuilder</returns>
        public PeriodFormatterBuilder AppendYears()
        {
            return AppendField(FormatterDurationFieldType.Years);
        }

        /// <summary>
        /// Instruct the printer to emit an integer months field, if supported.
        /// </summary>
        /// <remarks>
        /// The number of printed and parsed digits can be controlled using
        /// <see cref="MinimumPrintedDigits"/>
        /// and <see cref="MaximumParsedDigits"/>
        /// </remarks>
        /// <returns>This PeriodFormatterBuilder</returns>
        public PeriodFormatterBuilder AppendMonths()
        {
            return AppendField(FormatterDurationFieldType.Months);
        }

        /// <summary>
        /// Instruct the printer to emit an integer weeks field, if supported.
        /// </summary>
        /// <remarks>
        /// The number of printed and parsed digits can be controlled using
        /// <see cref="MinimumPrintedDigits"/>
        /// and <see cref="MaximumParsedDigits"/>
        /// </remarks>
        /// <returns>This PeriodFormatterBuilder</returns>
        public PeriodFormatterBuilder AppendWeeks()
        {
            return AppendField(FormatterDurationFieldType.Weeks);
        }

        /// <summary>
        /// Instruct the printer to emit an integer days field, if supported.
        /// </summary>
        /// <remarks>
        /// The number of printed and parsed digits can be controlled using
        /// <see cref="MinimumPrintedDigits"/>
        /// and <see cref="MaximumParsedDigits"/>
        /// </remarks>
        /// <returns>This PeriodFormatterBuilder</returns>
        public PeriodFormatterBuilder AppendDays()
        {
            return AppendField(FormatterDurationFieldType.Days);
        }

        /// <summary>
        /// Instruct the printer to emit an integer hours field, if supported.
        /// </summary>
        /// <remarks>
        /// The number of printed and parsed digits can be controlled using
        /// <see cref="MinimumPrintedDigits"/>
        /// and <see cref="MaximumParsedDigits"/>
        /// </remarks>
        /// <returns>This PeriodFormatterBuilder</returns>
        public PeriodFormatterBuilder AppendHours()
        {
            return AppendField(FormatterDurationFieldType.Hours);
        }

        /// <summary>
        /// Instruct the printer to emit an integer minutes field, if supported.
        /// </summary>
        /// <remarks>
        /// The number of printed and parsed digits can be controlled using
        /// <see cref="MinimumPrintedDigits"/>
        /// and <see cref="MaximumParsedDigits"/>
        /// </remarks>
        /// <returns>This PeriodFormatterBuilder</returns>
        public PeriodFormatterBuilder AppendMinutes()
        {
            return AppendField(FormatterDurationFieldType.Minutes);
        }

        /// <summary>
        /// Instruct the printer to emit an integer seconds field, if supported.
        /// </summary>
        /// <remarks>
        /// The number of printed and parsed digits can be controlled using
        /// <see cref="MinimumPrintedDigits"/>
        /// and <see cref="MaximumParsedDigits"/>
        /// </remarks>
        /// <returns>This PeriodFormatterBuilder</returns>
        public PeriodFormatterBuilder AppendSeconds()
        {
            return AppendField(FormatterDurationFieldType.Seconds);
        }

        /// <summary>
        /// Instruct the printer to emit a combined seconds and millis field, if supported.
        /// </summary>
        /// <remarks>
        /// The millis will overflow into the seconds if necessary.
        /// The millis are always output.
        /// The number of printed and parsed digits can be controlled using
        /// <see cref="MinimumPrintedDigits"/>
        /// and <see cref="MaximumParsedDigits"/>
        /// </remarks>
        /// <returns>This PeriodFormatterBuilder</returns>
        public PeriodFormatterBuilder AppendSecondsWithMillis()
        {
            return AppendField(FormatterDurationFieldType.SecondsMilliseconds);
        }

        /// <summary>
        /// Instruct the printer to emit a combined seconds and millis field, if supported.
        /// </summary>
        /// <remarks>
        /// The millis will overflow into the seconds if necessary.
        /// The millis are only output if non-zero.
        /// The number of printed and parsed digits can be controlled using
        /// <see cref="MinimumPrintedDigits"/>
        /// and <see cref="MaximumParsedDigits"/>
        /// </remarks>
        /// <returns>This PeriodFormatterBuilder</returns>
        public PeriodFormatterBuilder AppendSecondsWithOptionalMillis()
        {
            return AppendField(FormatterDurationFieldType.SecondsMillisecondsOptional);
        }

        /// <summary>
        /// Instruct the printer to emit an integer millis field, if supported.
        /// </summary>
        /// <remarks>
        /// The number of printed and parsed digits can be controlled using
        /// <see cref="MinimumPrintedDigits"/>
        /// and <see cref="MaximumParsedDigits"/>
        /// </remarks>
        /// <returns>This PeriodFormatterBuilder</returns>
        public PeriodFormatterBuilder AppendMillis()
        {
            return AppendField(FormatterDurationFieldType.Milliseconds);
        }

        /// <summary>
        /// Instruct the printer to emit an integer millis field, if supported.
        /// </summary>
        /// <remarks>
        /// The number of printed and parsed digits can be controlled using
        /// <see cref="MaximumParsedDigits"/>
        /// </remarks>
        /// <returns>This PeriodFormatterBuilder</returns>
        public PeriodFormatterBuilder AppendMillis3Digit()
        {
            return AppendField(FormatterDurationFieldType.Milliseconds, 3);
        }

        #endregion

        #region Prefix

        private PeriodFormatterBuilder AppendPrefix(IPeriodFieldAffix newPrefix)
        {
            this.prefix = this.prefix == null 
                            ? newPrefix 
                            : new CompositeAffix(this.prefix, newPrefix);
            return this;
        }

        /// <summary>
        /// Appends a field prefix which applies only to the next appended field. If
        /// the field is not printed, neither is the prefix.
        /// </summary>
        /// <param name="text">Text to print before field only if field is printed</param>
        /// <returns>This PeriodFormatterBuilder</returns>
        /// <exception cref="ArgumentNullException">If text is null or empty string</exception>
        public PeriodFormatterBuilder AppendPrefix(string text)
        {
            if (String.IsNullOrEmpty(text))
            {
                throw new ArgumentNullException("text");
            }

            return AppendPrefix(new SimpleAffix(text));
        }

        /// <summary>
        /// Appends a field prefix which applies only to the next appended field. If
        /// the field is not printed, neither is the prefix.
        /// </summary>
        /// <param name="singularText">Text to print before field only if field is printed, for singular values</param>
        /// <param name="pluralText">Text to print before field only if field is printed, for plural values</param>
        /// <returns>This PeriodFormatterBuilder</returns>
        public PeriodFormatterBuilder AppendPrefix(string singularText, string pluralText)
        {
            if (String.IsNullOrEmpty(singularText))
            {
                throw new ArgumentNullException("singularText");
            }
            if (String.IsNullOrEmpty(pluralText))
            {
                throw new ArgumentNullException("pluralText");
            }

            return AppendPrefix(new PluralAffix(singularText, pluralText));
        }

        #endregion

        #region Suffix

        PeriodFormatterBuilder AppendSuffix(IPeriodFieldAffix suffix)
        {
            IPeriodPrinter originalPrinter = default(IPeriodPrinter);
            IPeriodParser originalParser = default(IPeriodParser);

            if (PairsCount > 0)
            {
                originalPrinter = printers[PairsCount - 1];
                originalParser = parsers[PairsCount - 1];
            }

            var originalFormatter = originalPrinter as FieldFormatter;

            if (originalPrinter == null || originalParser == null
                || originalPrinter != originalParser
                || originalFormatter == null)
            {
                throw new InvalidOperationException("No field to apply suffix to");
            }

            VerifyPrefix();

            var newFieldFormater = new FieldFormatter(originalFormatter, suffix);
            printers[PairsCount - 1] = newFieldFormater;
            parsers[PairsCount - 1] = newFieldFormater;
            fieldFormatters[(int)newFieldFormater.FieldType] = newFieldFormater;

            return this;
        }

        /// <summary>
        /// Append a field suffix which applies only to the last appended field. If
        /// the field is not printed, neither is the suffix.
        /// </summary>
        /// <param name="text">Text to print after field only if field is printed</param>
        /// <returns>This PeriodFormatterBuilder</returns>
        /// <exception cref="InvalidOperationException">If no field exists to append to</exception>
        public PeriodFormatterBuilder AppendSuffix(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException();
            }

            return AppendSuffix(new SimpleAffix(text));
        }

        /// <summary>
        /// Append a field suffix which applies only to the last appended field. If
        /// the field is not printed, neither is the suffix.
        /// </summary>
        /// <param name="singularText">Text to print if field value is one</param>
        /// <param name="pluralText">Text to print if field value is not one</param>
        /// <returns>This PeriodFormatterBuilder</returns>
        /// <remarks>
        /// During parsing, the singular and plural versions are accepted whether or
        /// not the actual value matches plurality.
        /// </remarks>
        /// <exception cref="InvalidOperationException">If no field exists to append to</exception>
        public PeriodFormatterBuilder AppendSuffix(string singularText, string pluralText)
        {
            if (singularText == null)
            {
                throw new ArgumentNullException("singularText");
            }
            if (pluralText == null)
            {
                throw new ArgumentNullException("pluralText");
            }
            return AppendSuffix(new PluralAffix(singularText, pluralText));
        }

        #endregion

        #region Separator

        PeriodFormatterBuilder AppendSeparator(string text, string finalText, string[] variants,
            bool useBefore, bool useAfter)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }
            if (finalText == null)
            {
                throw new ArgumentNullException("finalText");
            }

            VerifyPrefix();

            //optimize zero formatter case
            if (PairsCount == 0)
            {
                if (useAfter && !useBefore)
                {
                    var newSeparator = new Separator(text, finalText, variants,
                        Literal.Empty, Literal.Empty, useBefore, useAfter);
                    AppendImpl(newSeparator, newSeparator);
                }
                return this;
            }

            //find the last separator added
            int i;
            Separator lastSeparator = default(Separator);
            for (i = PairsCount - 1; i >= 0; i--)
            {
                if ((lastSeparator = printers[i] as Separator) != null)
                {
                    break;
                }
            }

            //merge formatters
            if (lastSeparator != null && (i + 1) == PairsCount)
                throw new InvalidOperationException("Cannot have two adjacent separators");
            else
            {
                int rangeIndex = i + 1;
                int rangeCount = PairsCount - i - 1;
                IPeriodPrinter afterPrinter;
                IPeriodParser afterParser;
                Compose(printers.GetRange(rangeIndex, rangeCount), parsers.GetRange(rangeIndex, rangeCount), out afterPrinter, out afterParser);
                var separator = new Separator(
                         text, finalText, variants,
                         afterPrinter, afterParser,
                         useBefore, useAfter);

                printers.RemoveRange(rangeIndex, rangeCount);
                parsers.RemoveRange(rangeIndex, rangeCount);
                AppendImpl(separator, separator);
            }
            return this;
        }


        /// <summary>
        /// Append a separator, which is output if fields are printed both before
        /// and after the separator.
        /// </summary>
        /// <param name="text">The text to use as a separator</param>
        /// <param name="finalText">The text used if this is the final separator to be printed</param>
        /// <param name="variants">Set of text values which are also acceptable when parsed</param>
        /// <returns>This PeriodFormatterBuilder</returns>
        /// <remarks>
        /// This method changes the separator depending on whether it is the last separator
        /// to be output.
        /// <para>
        /// For example,
        /// <code>
        /// builder.AppendDays().AppendSeparator(",", "&amp;").AppendHours().AppendSeparator(",", "&amp;").AppendMinutes()
        /// </code>
        /// will output '1,2&amp;3' if all three fields are output, '1&amp;2' if two fields are output
        /// and '1' if just one field is output.
        /// </para>
        /// <para>
        /// The text will be parsed case-insensitively.
        /// </para>
        /// <para>
        /// Note: appending a separator discontinues any further work on the latest
        /// appended field.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">If this separator follows a previous one</exception>
        public PeriodFormatterBuilder AppendSeparator(string text, string finalText, string[] variants)
        {
            return AppendSeparator(text, finalText, variants, true, true);
        }

        /// <summary>
        /// Append a separator, which is output if fields are printed both before
        /// and after the separator.
        /// </summary>
        /// <param name="text">The text to use as a separator</param>
        /// <param name="finalText">The text used if this is the final separator to be printed</param>
        /// <returns>This PeriodFormatterBuilder</returns>
        /// <remarks>
        /// This method changes the separator depending on whether it is the last separator
        /// to be output.
        /// <para>
        /// For example,
        /// <code>
        /// builder.AppendDays().AppendSeparator(",", "&amp;").AppendHours().AppendSeparator(",", "&amp;").AppendMinutes()
        /// </code>
        /// will output '1,2&amp;3' if all three fields are output, '1&amp;2' if two fields are output
        /// and '1' if just one field is output.
        /// </para>
        /// <para>
        /// The text will be parsed case-insensitively.
        /// </para>
        /// <para>
        /// Note: appending a separator discontinues any further work on the latest
        /// appended field.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">If this separator follows a previous one</exception>
        public PeriodFormatterBuilder AppendSeparator(string text, string finalText)
        {
            return AppendSeparator(text, finalText, null, true, true);
        }

        /// <summary>
        /// Append a separator, which is output only if fields are printed before the separator.
        /// </summary>
        /// <param name="text">The text to use as a separator</param>
        /// <returns>This PeriodFormatterBuilder</returns>
        /// <remarks>
        /// <para>
        /// For example,
        /// <code>
        /// builder.AppendDays().AppendSeparatorIfFieldsBefore(",").appendHours()
        /// </code>
        /// will only output the comma if the days fields is output.
        /// </para>
        /// <para>
        /// The text will be parsed case-insensitively.
        /// </para>
        /// <para>
        /// Note: appending a separator discontinues any further work on the latest
        /// appended field.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">If this separator follows a previous one</exception>
        public PeriodFormatterBuilder AppendSeparatorIfFieldsBefore(string text)
        {
            return AppendSeparator(text, text, null, true, false);
        }

        /// <summary>
        /// Append a separator, which is output only if fields are printed after the separator.
        /// </summary>
        /// <param name="text">The text to use as a separator</param>
        /// <returns>This PeriodFormatterBuilder</returns>
        /// <remarks>
        /// <para>
        /// For example,
        /// <code>
        /// builder.appendDays().appendSeparatorIfFieldsAfter(",").appendHours()
        /// </code>
        /// will only output the comma if the hours fields is output.
        /// </para>
        /// <para>
        /// The text will be parsed case-insensitively.
        /// </para>
        /// <para>
        /// Note: appending a separator discontinues any further work on the latest
        /// appended field.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">If this separator follows a previous one</exception>
        public PeriodFormatterBuilder AppendSeparatorIfFieldsAfter(string text)
        {
            return AppendSeparator(text, text, null, false, true);
        }

        /// <summary>
        /// Append a separator, which is output if fields are printed both before
        /// </summary>
        /// <param name="text">The text to use as a separator</param>
        /// <returns>This PeriodFormatterBuilder</returns>
        /// <remarks>
        /// <para>
        /// For example,
        /// <code>
        /// builder.appendDays().appendSeparator(",").appendHours()
        /// </code>
        /// will only output the comma if both the days and hours fields are output.
        /// </para>
        /// <para>
        /// The text will be parsed case-insensitively.
        /// </para>
        /// <para>
        /// Note: appending a separator discontinues any further work on the latest
        /// appended field.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">If this separator follows a previous one</exception>
        public PeriodFormatterBuilder AppendSeparator(string text)
        {
            return AppendSeparator(text, text, null, true, true);
        }

        #endregion

        #region Composition

        private static void Compose(IList<IPeriodPrinter> printers, IList<IPeriodParser> parsers, out IPeriodPrinter printer, out IPeriodParser parser)
        {
            int length = printers.Count;

            switch (length)
            {
                case (0):
                    {
                        parser = Literal.Empty;
                        printer = Literal.Empty;
                        break;
                    }
                case (1):
                    {
                        printer = printers[0];
                        parser = parsers[0];
                        break;
                    }
                default:
                    {
                        var merged = new Composite(printers, parsers);
                        parser = merged;
                        printer = merged;
                        break;
                    }
            }
        }

        private PeriodFormatter ToFormatter(List<IPeriodPrinter> printers, List<IPeriodParser> parsers)
        {
            if (notPrinter && notParser)
            {
                throw new InvalidOperationException("Builder has created neither a printer nor a parser");
            }
            int size = printers.Count;
            if (size >= 1 && printers[0] is Separator)
            {
                Separator sep = (Separator)printers[0];

                PeriodFormatter f = ToFormatter(printers.GetRange(1, size - 1), parsers.GetRange(1, size - 1));
                sep = sep.Finish(f.Printer, f.Parser);
                return PeriodFormatter.FromPrinterAndParser(sep, sep);
            }
            else
            {
                IPeriodPrinter afterPrinter;
                IPeriodParser afterParser;
                Compose(printers, parsers, out afterPrinter, out afterParser);

                return notPrinter ? PeriodFormatter.FromParser(afterParser)
                        : notParser ? PeriodFormatter.FromPrinter(afterPrinter)
                        : PeriodFormatter.FromPrinterAndParser(afterPrinter, afterParser);

            }
        }

        /// <summary>
        /// Constructs a PeriodFormatter using all the appended elements.
        /// </summary>
        /// <returns>The newly created formatter</returns>
        /// <remarks>
        /// <para>
        /// This is the main method used by applications at the end of the build
        /// process to create a usable formatter.
        /// </para>
        /// <para>
        /// Subsequent changes to this builder do not affect the returned formatter.
        /// </para>
        /// <para>
        /// The returned formatter may not support both printing and parsing.
        /// The methods <see cref="PeriodFormatter.IsPrinter"/> and
        /// <see cref="PeriodFormatter.IsParser"/> will help you determine the state
        /// of the formatter.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">If the builder can produce neither a printer nor a parser</exception>
        public PeriodFormatter ToFormatter()
        {
            var newPeriodFormatter = ToFormatter(printers, parsers);
            fieldFormatters = (FieldFormatter[])fieldFormatters.Clone();

            return newPeriodFormatter;
        }

        /// <summary>
        /// Internal method to create a IPeriodPrinter instance using all the
        /// appended elements.
        /// </summary>
        /// <returns>The newly created printer, null if builder cannot create a printer</returns>
        /// <remarks>
        /// <para>
        /// Most applications will not use this method.
        /// If you want a printer in an application, call <see cref="ToFormatter()"/>
        /// and just use the printing API.
        /// </para>
        /// <para>
        /// Subsequent changes to this builder do not affect the returned printer.
        /// </para>
        /// </remarks>
        public IPeriodPrinter ToPrinter()
        {
            if (notPrinter)
            {
                return null;
            }

            return ToFormatter().Printer;
        }

        /// <summary>
        /// Internal method to create a IPeriodParser instance using all the
        /// appended elements.
        /// </summary>
        /// <returns>The newly created parser, null if builder cannot create a parser</returns>
        /// <remarks>
        /// <para>
        /// Most applications will not use this method.
        /// If you want a parser in an application, call <see cref="ToFormatter()"/>
        /// and just use the parsing API.
        /// </para>
        /// <para>
        /// Subsequent changes to this builder do not affect the returned parser.
        /// </para>
        /// </remarks>
        public IPeriodParser ToParser()
        {
            if (notParser)
            {
                return null;
            }

            return ToFormatter().Parser;
        }

        #endregion

        #endregion
    }
}
