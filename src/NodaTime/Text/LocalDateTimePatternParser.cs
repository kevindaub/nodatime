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
using System.Collections.Generic;
using System.Text;
using NodaTime.Globalization;
using NodaTime.Text.Patterns;

namespace NodaTime.Text
{
    /// <summary>
    /// Parser for patterns of <see cref="LocalDateTime"/> values.
    /// </summary>
    internal sealed class LocalDateTimePatternParser : IPatternParser<LocalDateTime>
    {
        private const int FractionOfSecondLength = LocalTimePatternParser.FractionOfSecondLength;

        private readonly LocalDateTime templateValue;       

        // TODO: All the handlers are copied from LocalDatePatternParser or LocalTimePatternParser. It would be nice to reuse instead of copying...
        private static readonly Dictionary<char, CharacterHandler<LocalDateTime, LocalDateTimeParseBucket>> PatternCharacterHandlers =
            new Dictionary<char, CharacterHandler<LocalDateTime, LocalDateTimeParseBucket>>
        {
            { '%', SteppedPatternBuilder<LocalDateTime, LocalDateTimeParseBucket>.HandlePercent },
            { '\'', SteppedPatternBuilder<LocalDateTime, LocalDateTimeParseBucket>.HandleQuote },
            { '\"', SteppedPatternBuilder<LocalDateTime, LocalDateTimeParseBucket>.HandleQuote },
            { '\\', SteppedPatternBuilder<LocalDateTime, LocalDateTimeParseBucket>.HandleBackslash },
            { '/', (pattern, builder) => builder.AddLiteral(builder.FormatInfo.DateSeparator, ParseResult<LocalDateTime>.DateSeparatorMismatch) },
            { 'y', DatePatternHelper.CreateYearHandler<LocalDateTime, LocalDateTimeParseBucket>(value => value.YearOfCentury, value => value.Year, (bucket, value) => bucket.Date.Year = value) },
            { 'Y', SteppedPatternBuilder<LocalDateTime, LocalDateTimeParseBucket>.HandlePaddedField
                       (5, PatternFields.YearOfEra, 0, 99999, value => value.YearOfEra, (bucket, value) => bucket.Date.YearOfEra = value) },
            { 'M', DatePatternHelper.CreateMonthOfYearHandler<LocalDateTime, LocalDateTimeParseBucket>
                        (value => value.MonthOfYear, (bucket, value) => bucket.Date.MonthOfYearText = value, (bucket, value) => bucket.Date.MonthOfYearNumeric = value) },
            { 'd', DatePatternHelper.CreateDayHandler<LocalDateTime, LocalDateTimeParseBucket>
                        (value => value.DayOfMonth, value => value.DayOfWeek, (bucket, value) => bucket.Date.DayOfMonth = value, (bucket, value) => bucket.Date.DayOfWeek = value) },
            { '.', HandlePeriod },
            { ':', (pattern, builder) => builder.AddLiteral(builder.FormatInfo.TimeSeparator, ParseResult<LocalDateTime>.TimeSeparatorMismatch) },
            { 'h', SteppedPatternBuilder<LocalDateTime, LocalDateTimeParseBucket>.HandlePaddedField
                       (2, PatternFields.Hours12, 1, 12, value => value.ClockHourOfHalfDay, (bucket, value) => bucket.Time.Hours12 = value) },
            { 'H', SteppedPatternBuilder<LocalDateTime, LocalDateTimeParseBucket>.HandlePaddedField
                       (2, PatternFields.Hours24, 0, 23, value => value.HourOfDay, (bucket, value) => bucket.Time.Hours24 = value) },
            { 'm', SteppedPatternBuilder<LocalDateTime, LocalDateTimeParseBucket>.HandlePaddedField
                       (2, PatternFields.Minutes, 0, 59, value => value.MinuteOfHour, (bucket, value) => bucket.Time.Minutes = value) },
            { 's', SteppedPatternBuilder<LocalDateTime, LocalDateTimeParseBucket>.HandlePaddedField
                       (2, PatternFields.Seconds, 0, 59, value => value.SecondOfMinute, (bucket, value) => bucket.Time.Seconds = value) },
            { 'f', HandleFractionSpecifier },
            { 'F', HandleFractionSpecifier },
            { 't', HandleAmPmDesignator }

        };

        // These have to come *after* the above field initializers...
        private static readonly IPattern<LocalDateTime> RoundTripPattern =
            new LocalDateTimePatternParser(LocalDateTimePattern.DefaultTemplateValue).ParsePattern("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffff", NodaFormatInfo.InvariantInfo).GetResultOrThrow();
        private static readonly IPattern<LocalDateTime> SortablePattern =
            new LocalDateTimePatternParser(LocalDateTimePattern.DefaultTemplateValue).ParsePattern("yyyy'-'MM'-'dd'T'HH':'mm':'ss", NodaFormatInfo.InvariantInfo).GetResultOrThrow();

        internal LocalDateTimePatternParser(LocalDateTime templateValue)
        {
            this.templateValue = templateValue;
        }

        // Note: public to implement the interface. It does no harm, and it's simpler than using explicit
        // interface implementation.
        public PatternParseResult<LocalDateTime> ParsePattern(string patternText, NodaFormatInfo formatInfo)
        {
            if (patternText == null)
            {
                return PatternParseResult<LocalDateTime>.ArgumentNull("patternText");
            }
            if (patternText.Length == 0)
            {
                return PatternParseResult<LocalDateTime>.FormatStringEmpty;
            }

            if (patternText.Length == 1)
            {
                char patternCharacter = patternText[0];
                if (patternCharacter == 'o' || patternCharacter == 'O')
                {
                    return PatternParseResult<LocalDateTime>.ForValue(RoundTripPattern);
                }
                if (patternCharacter == 's')
                {
                    return PatternParseResult<LocalDateTime>.ForValue(SortablePattern);
                }
                patternText = ExpandStandardFormatPattern(patternCharacter, formatInfo);
                if (patternText == null)
                {
                    return PatternParseResult<LocalDateTime>.UnknownStandardFormat(patternCharacter);
                }
            }

            var patternBuilder = new SteppedPatternBuilder<LocalDateTime, LocalDateTimeParseBucket>(formatInfo, () => new LocalDateTimeParseBucket(templateValue));
            var patternCursor = new PatternCursor(patternText);

            // Prime the pump...
            // TODO: Add this to the builder?
            patternBuilder.AddParseAction((str, bucket) =>
            {
                str.MoveNext();
                return null;
            });

            while (patternCursor.MoveNext())
            {
                CharacterHandler<LocalDateTime, LocalDateTimeParseBucket> handler;
                // The era parser needs access to the calendar, so we need a new handler each time.
                if (patternCursor.Current == 'g')
                {
                    handler = DatePatternHelper.CreateEraHandler<LocalDateTime, LocalDateTimeParseBucket>
                        (templateValue.Calendar, value => value.Era, (bucket, value) => bucket.Date.EraIndex = value);
                }
                else
                {
                    if (!PatternCharacterHandlers.TryGetValue(patternCursor.Current, out handler))
                    {
                        handler = HandleDefaultCharacter;
                    }                    
                }
                PatternParseResult<LocalDateTime> possiblePatternParseFailure = handler(patternCursor, patternBuilder);
                if (possiblePatternParseFailure != null)
                {
                    return possiblePatternParseFailure;
                }
            }
            if ((patternBuilder.UsedFields & (PatternFields.Era | PatternFields.YearOfEra)) == PatternFields.Era)
            {
                return PatternParseResult<LocalDateTime>.EraDesignatorWithoutYearOfEra;
            }
            return PatternParseResult<LocalDateTime>.ForValue(patternBuilder.Build());
        }

        private string ExpandStandardFormatPattern(char patternCharacter, NodaFormatInfo formatInfo)
        {
            switch (patternCharacter)
            {
                case 'f':
                    return formatInfo.DateTimeFormat.LongDatePattern + " " + formatInfo.DateTimeFormat.ShortTimePattern;
                case 'F':
                    return formatInfo.DateTimeFormat.FullDateTimePattern;
                case 'g':
                    return formatInfo.DateTimeFormat.ShortDatePattern + " " + formatInfo.DateTimeFormat.ShortTimePattern;
                case 'G':
                    return formatInfo.DateTimeFormat.ShortDatePattern + " " + formatInfo.DateTimeFormat.LongTimePattern;
                default:
                    // Will be turned into an exception.
                    return null;
            }
        }

        #region Character handlers
        // Handlers copied from LocalTimePatternParser
        private static PatternParseResult<LocalDateTime> HandlePeriod(PatternCursor pattern, SteppedPatternBuilder<LocalDateTime, LocalDateTimeParseBucket> builder)
        {
            // Note: Deliberately *not* using the decimal separator of the culture - see issue 21.

            // If the next part of the pattern is an F, then this decimal separator is effectively optional.
            // At parse time, we need to check whether we've matched the decimal separator. If we have, match the fractional
            // seconds part as normal. Otherwise, we continue on to the next parsing token.
            // At format time, we should always append the decimal separator, and then append using PadRightTruncate.
            if (pattern.PeekNext() == 'F')
            {
                PatternParseResult<LocalDateTime> failure = null;
                pattern.MoveNext();
                int count = pattern.GetRepeatCount(FractionOfSecondLength, ref failure);
                if (failure != null)
                {
                    return failure;
                }
                failure = builder.AddField(PatternFields.FractionalSeconds, pattern.Current);
                if (failure != null)
                {
                    return failure;
                }
                builder.AddParseAction((valueCursor, bucket) =>
                {
                    // If the next token isn't the decimal separator, we assume it's part of the next token in the pattern
                    if (!valueCursor.Match('.'))
                    {
                        return null;
                    }

                    // If there *was* a decimal separator, we should definitely have a number.
                    int fractionalSeconds;
                    // Last argument is false because we don't need *all* the digits to be present
                    if (!valueCursor.ParseFraction(count, FractionOfSecondLength, out fractionalSeconds, false))
                    {
                        return ParseResult<LocalDateTime>.MismatchedNumber(new string('F', count));
                    }
                    // No need to validate the value - we've got one to three digits, so the range 0-999 is guaranteed.
                    bucket.Time.FractionalSeconds = fractionalSeconds;
                    return null;
                });
                builder.AddFormatAction((value, sb) => sb.Append('.'));
                builder.AddFormatRightPadTruncate(count, FractionOfSecondLength, value => value.TickOfSecond);
                return null;
            }
            else
            {
                return builder.AddLiteral('.', ParseResult<LocalDateTime>.MismatchedCharacter);
            }
        }

        private static PatternParseResult<LocalDateTime> HandleFractionSpecifier(PatternCursor pattern, SteppedPatternBuilder<LocalDateTime, LocalDateTimeParseBucket> builder)
        {
            PatternParseResult<LocalDateTime> failure = null;
            char patternCharacter = pattern.Current;
            int count = pattern.GetRepeatCount(FractionOfSecondLength, ref failure);
            if (failure != null)
            {
                return failure;
            }
            failure = builder.AddField(PatternFields.FractionalSeconds, pattern.Current);
            if (failure != null)
            {
                return failure;
            }

            builder.AddParseAction((str, bucket) =>
            {
                int fractionalSeconds;
                // If the pattern is 'f', we need exactly "count" digits. Otherwise ('F') we need
                // "up to count" digits.
                if (!str.ParseFraction(count, FractionOfSecondLength, out fractionalSeconds, patternCharacter == 'f'))
                {
                    return ParseResult<LocalDateTime>.MismatchedNumber(new string(patternCharacter, count));
                }
                // No need to validate the value - we've got one to three digits, so the range 0-999 is guaranteed.
                bucket.Time.FractionalSeconds = fractionalSeconds;
                return null;
            });
            return patternCharacter == 'f' ? builder.AddFormatRightPad(count, FractionOfSecondLength, value => value.TickOfSecond)
                                           : builder.AddFormatRightPadTruncate(count, FractionOfSecondLength, value => value.TickOfSecond);
        }

        private static PatternParseResult<LocalDateTime> HandleAmPmDesignator(PatternCursor pattern, SteppedPatternBuilder<LocalDateTime, LocalDateTimeParseBucket> builder)
        {
            PatternParseResult<LocalDateTime> failure = null;
            int count = pattern.GetRepeatCount(2, ref failure);
            if (failure != null)
            {
                return failure;
            }
            failure = builder.AddField(PatternFields.AmPm, pattern.Current);
            if (failure != null)
            {
                return failure;
            }

            string amDesignator = builder.FormatInfo.AMDesignator;
            string pmDesignator = builder.FormatInfo.PMDesignator;
            // TODO: Work out if the single character designator should also consume the full designator if it's present.
            // Single character designator
            if (count == 1)
            {
                builder.AddParseAction((str, bucket) =>
                {
                    if (str.Match(amDesignator[0]))
                    {
                        bucket.Time.AmPm = 0;
                        return null;
                    }
                    if (str.Match(pmDesignator[0]))
                    {
                        bucket.Time.AmPm = 1;
                        return null;
                    }
                    return ParseResult<LocalDateTime>.MissingAmPmDesignator;
                });
                builder.AddFormatAction((value, sb) => sb.Append(value.HourOfDay > 11 ? pmDesignator[0] : amDesignator[0]));
                return null;
            }
            // Full designator
            builder.AddParseAction((str, bucket) =>
            {
                if (str.Match(amDesignator))
                {
                    bucket.Time.AmPm = 0;
                    return null;
                }
                if (str.Match(pmDesignator))
                {
                    bucket.Time.AmPm = 1;
                    return null;
                }
                return ParseResult<LocalDateTime>.MissingAmPmDesignator;
            });
            builder.AddFormatAction((value, sb) => sb.Append(value.HourOfDay > 11 ? pmDesignator : amDesignator));
            return null;
        }

        private static PatternParseResult<LocalDateTime> HandleDefaultCharacter(PatternCursor pattern, SteppedPatternBuilder<LocalDateTime, LocalDateTimeParseBucket> builder)
        {
            return builder.AddLiteral(pattern.Current, ParseResult<LocalDateTime>.MismatchedCharacter);
        }
        #endregion
        
        private sealed class LocalDateTimeParseBucket : ParseBucket<LocalDateTime>
        {
            internal readonly LocalDatePatternParser.LocalDateParseBucket Date;
            internal readonly LocalTimePatternParser.LocalTimeParseBucket Time;

            internal LocalDateTimeParseBucket(LocalDateTime templateValue)
            {
                Date = new LocalDatePatternParser.LocalDateParseBucket(templateValue.Date);
                Time = new LocalTimePatternParser.LocalTimeParseBucket(templateValue.TimeOfDay);
            }

            internal override ParseResult<LocalDateTime> CalculateValue(PatternFields usedFields)
            {
                ParseResult<LocalDate> dateResult = Date.CalculateValue(usedFields & PatternFields.AllDateFields);
                if (!dateResult.Success)
                {
                    return dateResult.WithResultType<LocalDateTime>();
                }
                ParseResult<LocalTime> timeResult = Time.CalculateValue(usedFields & PatternFields.AllTimeFields);
                if (!timeResult.Success)
                {
                    return timeResult.WithResultType<LocalDateTime>();
                }
                return ParseResult<LocalDateTime>.ForValue(dateResult.Value + timeResult.Value);
            }
        }
    }
}
