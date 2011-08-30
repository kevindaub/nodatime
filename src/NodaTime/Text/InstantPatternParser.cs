﻿#region Copyright and license information
// Copyright 2001-2009 Stephen Colebourne
// Copyright 2009-2011 Jon Skeet
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
using System.Globalization;
using NodaTime.Fields;
using NodaTime.Globalization;
using NodaTime.Text.Patterns;

namespace NodaTime.Text
{
    /// <summary>
    /// Pattern parsing support for <see cref="Instant" />.
    /// </summary>
    /// <remarks>
    /// Supported patterns:
    /// <list type="bullet">
    ///   <item><description>g: general; the UTC ISO-8601 instant in the style yyyy-MM-ddTHH:mm:ssZ</description></item>
    ///   <item><description>n: numeric; the number of ticks since the epoch using thousands separators</description></item>
    ///   <item><description>d: numeric; the number of ticks since the epoch without thousands separators</description></item>
    /// </list>
    /// </remarks>
    internal sealed class InstantPatternParser : IPatternParser<Instant>
    {
        public PatternParseResult<Instant> ParsePattern(string pattern, NodaFormatInfo formatInfo)
        {
            if (pattern == null)
            {
                return PatternParseResult<Instant>.ArgumentNull("format");
            }
            if (pattern.Length == 0)
            {
                return PatternParseResult<Instant>.FormatStringEmpty;
            }
            pattern = pattern.Trim();
            if (pattern.Length > 1)
            {
                return PatternParseResult<Instant>.FormatInvalid(pattern);
            }
            char patternChar = char.ToLowerInvariant(pattern[0]);
            switch (patternChar)
            {
                case 'g':
                    return PatternParseResult<Instant>.ForValue(new GeneralParsedPattern(formatInfo));
                case 'n':
                    return PatternParseResult<Instant>.ForValue(new NumberParsedPattern(formatInfo, pattern, "N0"));
                case 'd':
                    return PatternParseResult<Instant>.ForValue(new NumberParsedPattern(formatInfo, pattern, "D"));
                default:
                    return PatternParseResult<Instant>.UnknownStandardFormat(patternChar);
            }
        }

        private sealed class GeneralParsedPattern : AbstractParsedPattern<Instant>
        {
            private static readonly DateTimeField YearField;
            private static readonly DateTimeField MonthOfYearField;
            private static readonly DateTimeField DayOfMonthField;
            private static readonly DateTimeField HourOfDayField;
            private static readonly DateTimeField MinuteOfHourField;
            private static readonly DateTimeField SecondOfMinuteField;

            static GeneralParsedPattern()
            {
                var isoFields = CalendarSystem.Iso.Fields;
                YearField = isoFields.Year;
                MonthOfYearField = isoFields.MonthOfYear;
                DayOfMonthField = isoFields.DayOfMonth;
                HourOfDayField = isoFields.HourOfDay;
                MinuteOfHourField = isoFields.MinuteOfHour;
                SecondOfMinuteField = isoFields.SecondOfMinute;
            }
 
            internal GeneralParsedPattern(NodaFormatInfo formatInfo) : base(formatInfo)
            {
            }

            protected override ParseResult<Instant> ParseImpl(string value)
            {
                if (value.Equals(Instant.BeginningOfTimeLabel, StringComparison.OrdinalIgnoreCase))
                {
                    return ParseResult<Instant>.ForValue(Instant.MinValue);
                }
                if (value.Equals(Instant.EndOfTimeLabel, StringComparison.OrdinalIgnoreCase))
                {
                    return ParseResult<Instant>.ForValue(Instant.MaxValue);
                }

                DateTime result;
                // TODO: When we've got our own parsers fully working, parse this as a LocalDateTime.
                if (!DateTime.TryParseExact(value, "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'", FormatInfo.DateTimeFormat,
                                            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out result))
                {
                    return ParseResult<Instant>.CannotParseValue(value, "g");
                }
                return ParseResult<Instant>.ForValue(Instant.FromDateTimeUtc(result));
            }

            public override string Format(Instant value)
            {
                if (value == Instant.MinValue)
                {
                    return Instant.BeginningOfTimeLabel;
                }
                if (value == Instant.MaxValue)
                {
                    return Instant.EndOfTimeLabel;
                }
                var localInstant = new LocalInstant(value.Ticks); // Effectively UTC...
                return string.Format(CultureInfo.InvariantCulture,
                        "{0}-{1:00}-{2:00}T{3:00}:{4:00}:{5:00}Z",
                        YearField.GetValue(localInstant),
                        MonthOfYearField.GetValue(localInstant),
                        DayOfMonthField.GetValue(localInstant),
                        HourOfDayField.GetValue(localInstant),
                        MinuteOfHourField.GetValue(localInstant),
                        SecondOfMinuteField.GetValue(localInstant));
            }
        }

        private sealed class NumberParsedPattern : AbstractParsedPattern<Instant>
        {
            private const NumberStyles ParsingNumberStyles = NumberStyles.AllowLeadingSign | NumberStyles.AllowThousands;
            private readonly string pattern;
            private readonly string systemFormatString;

            internal NumberParsedPattern(NodaFormatInfo formatInfo, string pattern, string systemFormatString)
                : base(formatInfo)
            {
                this.pattern = pattern;
                this.systemFormatString = systemFormatString;
            }

            protected override ParseResult<Instant> ParseImpl(string value)
            {                
                long number;
                if (Int64.TryParse(value, ParsingNumberStyles, FormatInfo.NumberFormat, out number))
                {
                    return ParseResult<Instant>.ForValue(new Instant(number));
                }
                return ParseResult<Instant>.CannotParseValue(value, pattern);
            }

            public override string Format(Instant value)
            {
                return value.Ticks.ToString(systemFormatString, FormatInfo);
            }
        }
    }
}
