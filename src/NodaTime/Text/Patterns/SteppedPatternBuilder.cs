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

using System.Collections.Generic;
using System.Globalization;
using System.Text;
using NodaTime.Globalization;

namespace NodaTime.Text.Patterns
{
    /// <summary>
    /// Builder for a pattern which implements parsing and formatting as a sequence of steps applied
    /// in turn.
    /// </summary>
    internal sealed class SteppedPatternBuilder<TResult, TBucket> where TBucket : ParseBucket<TResult>
    {
        internal delegate ParseResult<TResult> ParseAction(ValueCursor cursor, TBucket bucket);

        private readonly NodaFormatInfo formatInfo;

        private List<NodaAction<TResult, StringBuilder>> formatActions;
        private readonly List<ParseAction> parseActions;
        private readonly NodaFunc<TBucket> bucketProvider;
        private PatternFields usedFields;

        internal SteppedPatternBuilder(NodaFormatInfo formatInfo, NodaFunc<TBucket> bucketProvider)
        {
            this.formatInfo = formatInfo;
            formatActions = new List<NodaAction<TResult, StringBuilder>>();
            parseActions = new List<ParseAction>();
            this.bucketProvider = bucketProvider;
        }

        public NodaFormatInfo FormatInfo { get { return formatInfo; } }

        /// <summary>
        /// Validates the combination of fields used and converts them into a suitable
        /// <see cref="PatternParseResult{T}"/>.
        /// </summary>
        internal PatternParseResult<TResult> ValidateFieldsBuildPatternParseResult()
        {
            // We assume invalid combinations are global across all parsers. The way that
            // the patterns are parsed ensures we never end up with any invalid individual fields
            // (e.g. time fields within a date pattern).

            if ((usedFields & (PatternFields.Era | PatternFields.YearOfEra)) == PatternFields.Era)
            {
                return PatternParseResult<TResult>.EraDesignatorWithoutYearOfEra;
            }
            var calendarAndEra = PatternFields.Era | PatternFields.Calendar;
            if ((usedFields & calendarAndEra) == calendarAndEra)
            {
                return PatternParseResult<TResult>.CalendarAndEra;
            }

            return PatternParseResult<TResult>.ForValue(Build());
        }

        /// <summary>
        /// Returns a built pattern. This is mostly to keep the API for the builder separate from that of the pattern,
        /// and for thread safety (publishing a new object, thus leading to a memory barrier).
        /// Note that this builder *must not* be used after the result has been built.
        /// </summary>
        internal IPattern<TResult> Build()
        {
            NodaAction<TResult, StringBuilder> formatDelegate = null;
            foreach (NodaAction<TResult, StringBuilder> formatAction in formatActions)
            {
                IPostPatternParseFormatAction postAction = formatAction.Target as IPostPatternParseFormatAction;
                formatDelegate += postAction == null ? formatAction : postAction.BuildFormatAction(usedFields);
            }
            return new SteppedPattern(formatDelegate, parseActions.ToArray(), bucketProvider, usedFields);
        }

        /// <summary>
        /// Registers that a pattern field has been used in this pattern, and returns a suitable error
        /// result if it's already been used.
        /// </summary>
        internal PatternParseResult<TResult> AddField(PatternFields field, char characterInPattern)
        {
            PatternFields newUsedFields = usedFields | field;
            if (newUsedFields == usedFields)
            {
                return PatternParseResult<TResult>.RepeatedFieldInPattern(characterInPattern);
            }
            usedFields = newUsedFields;
            return null;
        }

        internal void AddParseAction(ParseAction parseAction)
        {
            parseActions.Add(parseAction);
        }

        internal void AddFormatAction(NodaAction<TResult, StringBuilder> formatAction)
        {
            formatActions.Add(formatAction);
        }

        internal void AddParseValueAction(int minimumDigits, int maximumDigits, char patternChar,
                                          int minimumValue, int maximumValue,
                                          NodaAction<TBucket, int> valueSetter)
        {
            AddParseAction((cursor, bucket) =>
            {
                int value;
                bool negative = cursor.Match('-');
                if (negative && minimumValue >= 0)
                {
                    return ParseResult<TResult>.UnexpectedNegative;
                }
                if (!cursor.ParseDigits(minimumDigits, maximumDigits, out value))
                {
                    return ParseResult<TResult>.MismatchedNumber(new string(patternChar, minimumDigits));
                }
                if (negative)
                {
                    value = -value;
                }
                if (value < minimumValue || value > maximumValue)
                {
                    return ParseResult<TResult>.FieldValueOutOfRange(value, patternChar);
                }

                valueSetter(bucket, value);
                return null;
            });
        }

        /// <summary>
        /// Adds text which must be matched exactly when parsing, and appended directly when formatting.
        /// This overload uses the same failure result for all text values.
        /// </summary>
        internal PatternParseResult<TResult> AddLiteral(string expectedText, ParseResult<TResult> failure)
        {
            // Common case - single character literal, often a date or time separator.
            if (expectedText.Length == 1)
            {
                char expectedChar = expectedText[0];
                AddParseAction((str, bucket) => str.Match(expectedChar) ? null : failure);
                AddFormatAction((value, builder) => builder.Append(expectedChar));
                return null;
            }
            // TODO(Post-V1): These are ludicrously slow... see
            // http://msmvps.com/blogs/jon_skeet/archive/2011/08/23/optimization-and-generics-part-2-lambda-expressions-and-reference-types.aspx
            // for a description of the problem. I need to find a solution though...
            AddParseAction((str, bucket) => str.Match(expectedText) ? null : failure);
            AddFormatAction((value, builder) => builder.Append(expectedText));
            return null;
        }

        /// <summary>
        /// Handler for pattern characters which aren't understood as anything special by the pattern itself.
        /// </summary>
        internal static PatternParseResult<TResult> HandleDefaultCharacter(PatternCursor pattern, SteppedPatternBuilder<TResult, TBucket> builder)
        {
            return builder.AddLiteral(pattern.Current, ParseResult<TResult>.MismatchedCharacter);
        }

        internal static PatternParseResult<TResult> HandleQuote(PatternCursor pattern, SteppedPatternBuilder<TResult, TBucket> builder)
        {
            PatternParseResult<TResult> failure = null;
            string quoted = pattern.GetQuotedString(pattern.Current, ref failure);
            if (failure != null)
            {
                return failure;
            }
            return builder.AddLiteral(quoted, ParseResult<TResult>.QuotedStringMismatch);
        }

        internal static PatternParseResult<TResult> HandleBackslash(PatternCursor pattern, SteppedPatternBuilder<TResult, TBucket> builder)
        {
            if (!pattern.MoveNext())
            {
                return PatternParseResult<TResult>.EscapeAtEndOfString;
            }
            builder.AddLiteral(pattern.Current, ParseResult<TResult>.EscapedCharacterMismatch);
            return null;
        }

        /// <summary>
        /// Handle a leading "%" which acts as a pseudo-escape - it's mostly used to allow format strings such as "%H" to mean
        /// "use a custom format string consisting of H instead of a standard pattern H".
        /// </summary>
        internal static PatternParseResult<TResult> HandlePercent(PatternCursor pattern, SteppedPatternBuilder<TResult, TBucket> builder)
        {
            if (pattern.HasMoreCharacters)
            {
                if (pattern.PeekNext() != '%')
                {
                    // Handle the next character as normal
                    return null;
                }
                return PatternParseResult<TResult>.PercentDoubled;
            }
            return PatternParseResult<TResult>.PercentAtEndOfString;
        }

        /// <summary>
        /// Returns a handler for a zero-padded purely-numeric field specifier, such as "seconds", "minutes", "24-hour", "12-hour" etc.
        /// </summary>
        /// <param name="maxCount">Maximum permissable count (usually two)</param>
        /// <param name="field">Field to remember that we've seen</param>
        /// <param name="minValue">Minimum valid value for the field (inclusive)</param>
        /// <param name="maxValue">Maximum value value for the field (inclusive)</param>
        /// <param name="getter">Delegate to retrieve the field value when formatting</param>
        /// <param name="setter">Delegate to set the field value into a bucket when parsing</param>
        /// <returns>The pattern parsing failure, or null on success.</returns>
        internal static CharacterHandler<TResult, TBucket> HandlePaddedField(int maxCount, PatternFields field, int minValue, int maxValue, NodaFunc<TResult, int> getter,
            NodaAction<TBucket, int> setter)
        {
            return (pattern, builder) =>
            {
                PatternParseResult<TResult> failure = null;
                int count = pattern.GetRepeatCount(maxCount, ref failure);
                if (failure != null)
                {
                    return failure;
                }
                failure = builder.AddField(field, pattern.Current);
                if (failure != null)
                {
                    return failure;
                }

                builder.AddParseValueAction(count, maxCount, pattern.Current, minValue, maxValue, setter);
                builder.AddFormatLeftPad(count, getter);
                return null;
            };
        }

        /// <summary>
        /// Adds a character which must be matched exactly when parsing, and appended directly when formatting.
        /// </summary>
        internal PatternParseResult<TResult> AddLiteral(char expectedChar, NodaFunc<char, ParseResult<TResult>> failureSelector)
        {
            AddParseAction((str, bucket) => str.Match(expectedChar) ? null : failureSelector(expectedChar));
            AddFormatAction((value, builder) => builder.Append(expectedChar));
            return null;
        }

        /// <summary>
        /// Adds parse actions for a list of strings, such as days of the week or month names.
        /// The parsing is performed case-insensitively. All candidates are tested, and only the longest
        /// match is used.
        /// TODO(Post-V1): Make this much more efficient in terms of capture...
        /// </summary>
        internal void AddParseLongestTextAction(char field, NodaAction<TBucket, int> setter, CompareInfo compareInfo, IList<string> textValues)
        {
            AddParseAction((str, bucket) => {
                int bestIndex = -1;
                int longestMatch = 0;
                FindLongestMatch(compareInfo, str, textValues, ref bestIndex, ref longestMatch);
                if (bestIndex != -1)
                {
                    setter(bucket, bestIndex);
                    str.Move(str.Index + longestMatch);
                    return null;
                }
                return ParseResult<TResult>.MismatchedText(field);
            });
        }

        /// <summary>
        /// Adds parse actions for two list of strings, such as non-genitive and genitive month names.
        /// The parsing is performed case-insensitively. All candidates are tested, and only the longest
        /// match is used.
        /// TODO(Post-V1): Make this much more efficient in terms of capture...
        /// </summary>
        internal void AddParseLongestTextAction(char field, NodaAction<TBucket, int> setter, CompareInfo compareInfo, IList<string> textValues1, IList<string> textValues2)
        {
            AddParseAction((str, bucket) =>
            {
                int bestIndex = -1;
                int longestMatch = 0;
                FindLongestMatch(compareInfo, str, textValues1, ref bestIndex, ref longestMatch);
                FindLongestMatch(compareInfo, str, textValues2, ref bestIndex, ref longestMatch);
                if (bestIndex != -1)
                {
                    setter(bucket, bestIndex);
                    str.Move(str.Index + longestMatch);
                    return null;
                }
                return ParseResult<TResult>.MismatchedText(field);
            });
        }

        /// <summary>
        /// Find the longest match from a given set of candidate strings, updating the index/length of the best value
        /// accordingly.
        /// </summary>
        private static void FindLongestMatch(CompareInfo compareInfo, ValueCursor cursor, IList<string> values, ref int bestIndex, ref int longestMatch)
        {
            for (int i = 0; i < values.Count; i++)
            {
                string candidate = values[i];
                if (candidate == null || candidate.Length <= longestMatch)
                {
                    continue;
                }
                if (cursor.MatchCaseInsensitive(candidate, compareInfo, false))
                {
                    bestIndex = i;
                    longestMatch = candidate.Length;
                }
            }
        }

        /// <summary>
        /// Adds parse and format actions for a mandatory positive/negative sign.
        /// </summary>
        /// <param name="signSetter">Action to take when to set the given sign within the bucket</param>
        /// <param name="nonNegativePredicate">Predicate to detect whether the value being formatted is non-negative</param>
        public PatternParseResult<TResult> AddRequiredSign(NodaAction<TBucket, bool> signSetter, NodaFunc<TResult, bool> nonNegativePredicate)
        {
            string negativeSign = formatInfo.NegativeSign;
            string positiveSign = formatInfo.PositiveSign;
            AddParseAction((str, bucket) =>
            {
                if (str.Match(negativeSign))
                {
                    signSetter(bucket, false);
                    return null;
                }
                if (str.Match(positiveSign))
                {
                    signSetter(bucket, true);
                    return null;
                }
                return ParseResult<TResult>.MissingSign;
            });
            AddFormatAction((value, sb) => sb.Append(nonNegativePredicate(value) ? positiveSign : negativeSign));
            return null;
        }

        /// <summary>
        /// Adds parse and format actions for an "negative only" sign.
        /// </summary>
        /// <param name="signSetter">Action to take when to set the given sign within the bucket</param>
        /// <param name="nonNegativePredicate">Predicate to detect whether the value being formatted is non-negative</param>
        public PatternParseResult<TResult> AddNegativeOnlySign(NodaAction<TBucket, bool> signSetter, NodaFunc<TResult, bool> nonNegativePredicate)
        {
            string negativeSign = formatInfo.NegativeSign;
            string positiveSign = formatInfo.PositiveSign;
            AddParseAction((str, bucket) =>
            {
                if (str.Match(negativeSign))
                {
                    signSetter(bucket, false);
                    return null;
                }
                if (str.Match(positiveSign))
                {
                    return ParseResult<TResult>.PositiveSignInvalid;
                }
                signSetter(bucket, true);
                return null;
            });
            AddFormatAction((value, builder) =>
            {
                if (!nonNegativePredicate(value))
                {
                    builder.Append(negativeSign);
                }
            });
            return null;
        }

        internal PatternParseResult<TResult> AddFormatLeftPad(int count, NodaFunc<TResult, int> selector)
        {
            AddFormatAction((value, sb) => FormatHelper.LeftPad(selector(value), count, sb));
            return null;
        }

        internal PatternParseResult<TResult> AddFormatRightPad(int width, int scale, NodaFunc<TResult, int> selector)
        {
            AddFormatAction((value, sb) => FormatHelper.RightPad(selector(value), width, scale, sb));
            return null;
        }

        internal PatternParseResult<TResult> AddFormatRightPadTruncate(int width, int scale, NodaFunc<TResult, int> selector)
        {
            AddFormatAction((value, sb) => FormatHelper.RightPadTruncate(selector(value), width, scale, formatInfo.DecimalSeparator, sb));
            return null;
        }

        /// <summary>
        /// Hack to handle genitive month names - we only know what we need to do *after* we've parsed the whole pattern.
        /// </summary>
        internal interface IPostPatternParseFormatAction
        {
            NodaAction<TResult, StringBuilder> BuildFormatAction(PatternFields finalFields);
        }

        private sealed class SteppedPattern : IPattern<TResult>
        {
            private readonly NodaAction<TResult, StringBuilder> formatActions;
            private readonly ParseAction[] parseActions;
            private readonly NodaFunc<TBucket> bucketProvider;
            private readonly PatternFields usedFields;

            public SteppedPattern(NodaAction<TResult, StringBuilder> formatActions,
                ParseAction[] parseActions,
                NodaFunc<TBucket> bucketProvider, PatternFields usedFields)
            {
                this.formatActions = formatActions;
                this.parseActions = parseActions;
                this.bucketProvider = bucketProvider;
                this.usedFields = usedFields;
            }

            public ParseResult<TResult> Parse(string text)
            {
                if (text == null)
                {
                    return ParseResult<TResult>.ArgumentNull("text");
                }
                if (text.Length == 0)
                {
                    return ParseResult<TResult>.ValueStringEmpty;
                }

                ValueCursor valueCursor = new ValueCursor(text);
                TBucket bucket = bucketProvider();

                foreach (var action in parseActions)
                {
                    ParseResult<TResult> failure = action(valueCursor, bucket);
                    if (failure != null)
                    {
                        return failure;
                    }
                }
                if (valueCursor.Current != TextCursor.Nul)
                {
                    return ParseResult<TResult>.ExtraValueCharacters(valueCursor.Remainder);
                }

                return bucket.CalculateValue(usedFields);
            }

            public string Format(TResult value)
            {
                StringBuilder builder = new StringBuilder();
                // This will call all the actions in the multicast delegate.
                formatActions(value, builder);
                return builder.ToString();
            }
        }
    }
}
