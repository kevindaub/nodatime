﻿// Copyright 2013 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;
using System.Diagnostics;
using NodaTime.Globalization;
using NodaTime.Properties;
using NodaTime.Text.Patterns;
using NodaTime.Utility;

namespace NodaTime.Text
{
    internal sealed class DurationPatternParser : IPatternParser<Duration>
    {
        private static readonly Dictionary<char, CharacterHandler<Duration, DurationParseBucket>> PatternCharacterHandlers =
            new Dictionary<char, CharacterHandler<Duration, DurationParseBucket>>
        {
            { '%', SteppedPatternBuilder<Duration, DurationParseBucket>.HandlePercent },
            { '\'', SteppedPatternBuilder<Duration, DurationParseBucket>.HandleQuote },
            { '\"', SteppedPatternBuilder<Duration, DurationParseBucket>.HandleQuote },
            { '\\', SteppedPatternBuilder<Duration, DurationParseBucket>.HandleBackslash },
            { '.', TimePatternHelper.CreatePeriodHandler<Duration, DurationParseBucket>(9, GetPositiveNanosecondOfSecond, (bucket, value) => bucket.AddNanoseconds(value)) },
            { ':', (pattern, builder) => builder.AddLiteral(builder.FormatInfo.TimeSeparator, ParseResult<Duration>.TimeSeparatorMismatch) },
            { 'D', CreateDayHandler() },
            { 'H', CreateTotalHandler(PatternFields.Hours24, NodaConstants.NanosecondsPerHour) },
            { 'h', CreatePartialHandler(PatternFields.Hours24, NodaConstants.NanosecondsPerHour, NodaConstants.HoursPerStandardDay) },
            { 'M', CreateTotalHandler(PatternFields.Minutes, NodaConstants.NanosecondsPerMinute) },
            { 'm', CreatePartialHandler(PatternFields.Minutes, NodaConstants.NanosecondsPerMinute, NodaConstants.MinutesPerHour) },
            { 'S', CreateTotalHandler(PatternFields.Seconds, NodaConstants.NanosecondsPerSecond) },
            { 's', CreatePartialHandler(PatternFields.Seconds, NodaConstants.NanosecondsPerSecond, NodaConstants.SecondsPerMinute) },
            { 'f', TimePatternHelper.CreateFractionHandler<Duration, DurationParseBucket>(9, GetPositiveNanosecondOfSecond, (bucket, value) => bucket.AddNanoseconds(value)) },
            { 'F', TimePatternHelper.CreateFractionHandler<Duration, DurationParseBucket>(9, GetPositiveNanosecondOfSecond, (bucket, value) => bucket.AddNanoseconds(value)) },
            { '+', HandlePlus },
            { '-', HandleMinus },
        };

        // Note: public to implement the interface. It does no harm, and it's simpler than using explicit
        // interface implementation.
        public IPattern<Duration> ParsePattern(string patternText, NodaFormatInfo formatInfo)
        {
            Preconditions.CheckNotNull(patternText, "patternText");
            if (patternText.Length == 0)
            {
                throw new InvalidPatternException(Messages.Parse_FormatStringEmpty);
            }

            // The sole standard pattern...
            if (patternText.Length == 1)
            {
                switch (patternText[0])
                {
                    case 'o':
                        return DurationPattern.Patterns.RoundtripPatternImpl;
                    default:
                        throw new InvalidPatternException(Messages.Parse_UnknownStandardFormat, patternText[0], typeof(Duration));
                }
            }

            var patternBuilder = new SteppedPatternBuilder<Duration, DurationParseBucket>(formatInfo,
                () => new DurationParseBucket());
            patternBuilder.ParseCustomPattern(patternText, PatternCharacterHandlers);
            return patternBuilder.Build();
        }

        private static int GetPositiveNanosecondOfSecond(Duration duration)
        {
            Nanoseconds positive = GetPositiveNanoseconds(duration);
            return (int) (positive.NanosecondOfDay % NodaConstants.NanosecondsPerSecond);
        }

        private static CharacterHandler<Duration, DurationParseBucket> CreateTotalHandler
            (PatternFields field, long nanosecondsPerUnit)
        {
            return (pattern, builder) =>
            {
                int count = pattern.GetRepeatCount(10);
                // AddField would throw an inappropriate exception here, so handle it specially.
                if ((builder.UsedFields & PatternFields.TotalDuration) != 0)
                {
                    throw new InvalidPatternException(Messages.Parse_MultipleCapitalDurationFields);
                }
                builder.AddField(field, pattern.Current);
                builder.AddField(PatternFields.TotalDuration, pattern.Current);
                builder.AddParseValueAction(count, 10, pattern.Current, 0, int.MaxValue, (bucket, value) => bucket.AddUnits(value, nanosecondsPerUnit));
                builder.AddFormatLeftPad(count, duration => (int) (GetPositiveNanosecondUnits(duration, nanosecondsPerUnit)) );
            };
        }

        private static CharacterHandler<Duration, DurationParseBucket> CreateDayHandler()
        {
            return (pattern, builder) =>
            {
                int count = pattern.GetRepeatCount(10);
                // AddField would throw an inappropriate exception here, so handle it specially.
                if ((builder.UsedFields & PatternFields.TotalDuration) != 0)
                {
                    throw new InvalidPatternException(Messages.Parse_MultipleCapitalDurationFields);
                }
                builder.AddField(PatternFields.DayOfMonth, pattern.Current);
                builder.AddField(PatternFields.TotalDuration, pattern.Current);
                builder.AddParseValueAction(count, 10, pattern.Current, 0, int.MaxValue, (bucket, value) => bucket.AddDays(value));
                builder.AddFormatLeftPad(count, duration => 
                {
                    var nanoseconds = duration.Nanoseconds;
                    int days = nanoseconds.Days;
                    if (days >= 0)
                    {
                        return days;
                    }
                    // Round towards 0.
                    return nanoseconds.NanosecondOfDay == 0 ? -days : -(days + 1);
                });
            };
        }

        private static CharacterHandler<Duration, DurationParseBucket> CreatePartialHandler
            (PatternFields field, long nanosecondsPerUnit, int unitsPerContainer)
        {
            return (pattern, builder) =>
            {
                int count = pattern.GetRepeatCount(2);
                builder.AddField(field, pattern.Current);
                builder.AddParseValueAction(count, 2, pattern.Current, 0, unitsPerContainer - 1,
                    (bucket, value) => bucket.AddUnits(value, nanosecondsPerUnit));
                builder.AddFormatLeftPad(count, duration => (int)(GetPositiveNanosecondUnits(duration, nanosecondsPerUnit) % unitsPerContainer));
            };
        }

        private static void HandlePlus(PatternCursor pattern, SteppedPatternBuilder<Duration, DurationParseBucket> builder)
        {
            builder.AddField(PatternFields.Sign, pattern.Current);
            builder.AddRequiredSign((bucket, positive) => bucket.IsNegative = !positive, duration => duration.Nanoseconds.Days >= 0);
        }

        private static void HandleMinus(PatternCursor pattern, SteppedPatternBuilder<Duration, DurationParseBucket> builder)
        {
            builder.AddField(PatternFields.Sign, pattern.Current);
            builder.AddNegativeOnlySign((bucket, positive) => bucket.IsNegative = !positive, duration => duration.Nanoseconds.Days >= 0);
        }

        /// <summary>
        /// Returns the absolute number of nanoseconds in a duration.
        /// </summary>
        private static Nanoseconds GetPositiveNanoseconds(Duration duration)
        {
            Nanoseconds nanos = duration.Nanoseconds;
            return nanos.Days >= 0 ? nanos : -nanos;
        }

        private static long GetPositiveNanosecondUnits(Duration duration, long nanosecondsPerUnit)
        {
            Nanoseconds nanos = GetPositiveNanoseconds(duration);
            return (long) (nanos / nanosecondsPerUnit);
        }

        /// <summary>
        /// Provides a container for the interim parsed pieces of an <see cref="Offset" /> value.
        /// </summary>
        [DebuggerStepThrough]
        private sealed class DurationParseBucket : ParseBucket<Duration>
        {
            internal bool IsNegative { get; set; }
            private Nanoseconds nanoseconds;

            internal void AddNanoseconds(long nanoseconds)
            {
                this.nanoseconds = this.nanoseconds.Plus(nanoseconds);
            }

            internal void AddDays(int days)
            {
                // TODO(2.0): Add a PlusDays method to Nanoseconds?
                nanoseconds = new Nanoseconds(nanoseconds.Days + days, nanoseconds.NanosecondOfDay);
            }

            internal void AddUnits(int units, long nanosecondsPerUnit)
            {
                // TODO(2.0): Check whether there's a quicker way to do this,
                // possibly by adding something to Nanoseconds itself.
                if (units < long.MaxValue / nanosecondsPerUnit)
                {
                    nanoseconds += (Nanoseconds) (units * nanosecondsPerUnit);
                }
                else
                {
                    nanoseconds += ((Nanoseconds) units) * nanosecondsPerUnit;
                }
            }

            /// <summary>
            /// Calculates the value from the parsed pieces.
            /// </summary>
            internal override ParseResult<Duration> CalculateValue(PatternFields usedFields, string text)
            {
                return ParseResult<Duration>.ForValue(Duration.FromNanoseconds(IsNegative ? -nanoseconds : nanoseconds));
            }
        }
    }
}
