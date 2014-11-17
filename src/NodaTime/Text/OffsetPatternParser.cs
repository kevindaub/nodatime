// Copyright 2011 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using NodaTime.Globalization;
using NodaTime.Properties;
using NodaTime.Text.Patterns;
using NodaTime.Utility;

namespace NodaTime.Text
{
    internal sealed class OffsetPatternParser : IPatternParser<Offset>
    {
        private static readonly Dictionary<char, CharacterHandler<Offset, OffsetParseBucket>> PatternCharacterHandlers = 
            new Dictionary<char, CharacterHandler<Offset, OffsetParseBucket>>
        {
            { '%', SteppedPatternBuilder<Offset, OffsetParseBucket>.HandlePercent },
            { '\'', SteppedPatternBuilder<Offset, OffsetParseBucket>.HandleQuote },
            { '\"', SteppedPatternBuilder<Offset, OffsetParseBucket>.HandleQuote },
            { '\\', SteppedPatternBuilder<Offset, OffsetParseBucket>.HandleBackslash },
            { ':', (pattern, builder) => builder.AddLiteral(builder.FormatInfo.TimeSeparator, ParseResult<Offset>.TimeSeparatorMismatch) },
            { 'h', (pattern, builder) => { throw new InvalidPatternException(Messages.Parse_Hour12PatternNotSupported, typeof(Offset)); } },
            { 'H', SteppedPatternBuilder<Offset, OffsetParseBucket>.HandlePaddedField
                       (2, PatternFields.Hours24, 0, 23, GetPositiveHours, (bucket, value) => bucket.Hours = value) },
            { 'm', SteppedPatternBuilder<Offset, OffsetParseBucket>.HandlePaddedField
                       (2, PatternFields.Minutes, 0, 59, GetPositiveMinutes, (bucket, value) => bucket.Minutes = value) },
            { 's', SteppedPatternBuilder<Offset, OffsetParseBucket>.HandlePaddedField
                       (2, PatternFields.Seconds, 0, 59, GetPositiveSeconds, (bucket, value) => bucket.Seconds = value) },
            { '+', HandlePlus },
            { '-', HandleMinus },
            { 'Z', (ignored1, ignored2) => { throw new InvalidPatternException(Messages.Parse_ZPrefixNotAtStartOfPattern); } }
        };

        // These are used to compute the individual (always-positive) components of an offset.
        // For example, an offset of "three and a half hours behind UTC" would have a "positive hours" value
        // of 3, and a "positive minutes" value of 30. The sign is computed elsewhere.
        private static int GetPositiveHours(Offset offset) => Math.Abs(offset.Milliseconds) / NodaConstants.MillisecondsPerHour;

        private static int GetPositiveMinutes(Offset offset) => (Math.Abs(offset.Milliseconds) % NodaConstants.MillisecondsPerHour) / NodaConstants.MillisecondsPerMinute;

        private static int GetPositiveSeconds(Offset offset) => (Math.Abs(offset.Milliseconds) % NodaConstants.MillisecondsPerMinute) / NodaConstants.MillisecondsPerSecond;

        // Note: public to implement the interface. It does no harm, and it's simpler than using explicit
        // interface implementation.
        public IPattern<Offset> ParsePattern(string patternText, NodaFormatInfo formatInfo) => ParsePartialPattern(patternText, formatInfo);

        private IPartialPattern<Offset> ParsePartialPattern(string patternText, NodaFormatInfo formatInfo)
        {
            // Nullity check is performed in OffsetPattern.
            if (patternText.Length == 0)
            {
                throw new InvalidPatternException(Messages.Parse_FormatStringEmpty);
            }

            if (patternText.Length == 1)
            {
                switch (patternText)
                {
                    case "g":
                        return CreateGeneralPattern(formatInfo);
                    case "G":
                        return new ZPrefixPattern(CreateGeneralPattern(formatInfo));
                    case "l":
                        patternText = formatInfo.OffsetPatternLong;
                        break;
                    case "m":
                        patternText = formatInfo.OffsetPatternMedium;
                        break;
                    case "s":
                        patternText = formatInfo.OffsetPatternShort;
                        break;
                    default:
                        throw new InvalidPatternException(Messages.Parse_UnknownStandardFormat, patternText, typeof(Offset));
                }
            }
            // This is the only way we'd normally end up in custom parsing land for Z on its own.
            if (patternText == "%Z")
            {
                throw new InvalidPatternException(Messages.Parse_EmptyZPrefixedOffsetPattern);
            }

            // Handle Z-prefix by stripping it, parsing the rest as a normal pattern, then building a special pattern
            // which decides whether or not to delegate.
            bool zPrefix = patternText.StartsWith("Z");

            var patternBuilder = new SteppedPatternBuilder<Offset, OffsetParseBucket>(formatInfo, () => new OffsetParseBucket());
            patternBuilder.ParseCustomPattern(zPrefix ? patternText.Substring(1) : patternText, PatternCharacterHandlers);
            // No need to validate field combinations here, but we do need to do something a bit special
            // for Z-handling.
            IPartialPattern<Offset> pattern = patternBuilder.Build();
            return zPrefix ? new ZPrefixPattern(pattern) : pattern;
        }

        #region Standard patterns

        private IPartialPattern<Offset> CreateGeneralPattern(NodaFormatInfo formatInfo)
        {
            var patterns = new List<IPartialPattern<Offset>>();
            foreach (char c in "lms")
            {
                patterns.Add(ParsePartialPattern(c.ToString(), formatInfo));
            }
            Func<Offset, IPartialPattern<Offset>> formatter = value => PickGeneralFormatter(value, patterns);
            return new CompositePattern<Offset>(patterns, formatter);
        }

        private static IPartialPattern<Offset> PickGeneralFormatter(Offset value, List<IPartialPattern<Offset>> patterns)
        {
            // Note: this relies on the order in ExpandStandardFormatPattern
            int index;
            // Work out the least significant non-zero part.
            int absoluteSeconds = Math.Abs(value.Seconds);
            if (absoluteSeconds % NodaConstants.SecondsPerMinute != 0)
            {
                index = 0;
            }
            else if ((absoluteSeconds % NodaConstants.SecondsPerHour) / NodaConstants.SecondsPerMinute != 0)
            {
                index = 1;
            }
            else
            {
                index = 2;
            }
            return patterns[index];
        }
        #endregion

        /// <summary>
        /// Pattern which optionally delegates to another, but both parses and formats Offset.Zero as "Z".
        /// </summary>
        private sealed class ZPrefixPattern : IPartialPattern<Offset>
        {
            private readonly IPartialPattern<Offset> fullPattern;

            internal ZPrefixPattern(IPartialPattern<Offset> fullPattern)
            {
                this.fullPattern = fullPattern;
            }

            public ParseResult<Offset> Parse(string text) => text == "Z" ? ParseResult<Offset>.ForValue(Offset.Zero) : fullPattern.Parse(text);

            public string Format(Offset value) => value == Offset.Zero ? "Z" : fullPattern.Format(value);

            public ParseResult<Offset> ParsePartial(ValueCursor cursor)
            {
                if (cursor.Current == 'Z')
                {
                    cursor.MoveNext();
                    return ParseResult<Offset>.ForValue(Offset.Zero);
                }
                return fullPattern.ParsePartial(cursor);
            }

            public StringBuilder AppendFormat(Offset value, StringBuilder builder)
            {
                Preconditions.CheckNotNull(builder, "builder");
                return value == Offset.Zero ? builder.Append("Z") : fullPattern.AppendFormat(value, builder);
            }
        }

        #region Character handlers
        private static void HandlePlus(PatternCursor pattern, SteppedPatternBuilder<Offset, OffsetParseBucket> builder)
        {
            builder.AddField(PatternFields.Sign, pattern.Current);
            builder.AddRequiredSign((bucket, positive) => bucket.IsNegative = !positive, offset => offset.Milliseconds >= 0);
        }

        private static void HandleMinus(PatternCursor pattern, SteppedPatternBuilder<Offset, OffsetParseBucket> builder)
        {
            builder.AddField(PatternFields.Sign, pattern.Current);
            builder.AddNegativeOnlySign((bucket, positive) => bucket.IsNegative = !positive, offset => offset.Milliseconds >= 0);
        }
        #endregion

        /// <summary>
        /// Provides a container for the interim parsed pieces of an <see cref="Offset" /> value.
        /// </summary>
        [DebuggerStepThrough]
        private sealed class OffsetParseBucket : ParseBucket<Offset>
        {
            /// <summary>
            /// The hours in the range [0, 23].
            /// </summary>
            internal int Hours;

            /// <summary>
            /// The minutes in the range [0, 59].
            /// </summary>
            internal int Minutes;

            /// <summary>
            /// The seconds in the range [0, 59].
            /// </summary>
            internal int Seconds;

            /// <summary>
            /// Gets a value indicating whether this instance is negative.
            /// </summary>
            /// <value>
            /// <c>true</c> if this instance is negative; otherwise, <c>false</c>.
            /// </value>
            public bool IsNegative;

            /// <summary>
            /// Calculates the value from the parsed pieces.
            /// </summary>
            internal override ParseResult<Offset> CalculateValue(PatternFields usedFields, string text)
            {
                int seconds = Hours * NodaConstants.SecondsPerHour +
                    Minutes * NodaConstants.SecondsPerMinute +
                    Seconds;
                if (IsNegative)
                {
                    seconds = -seconds;
                }
                return ParseResult<Offset>.ForValue(Offset.FromSeconds(seconds));
            }
        }
    }
}
