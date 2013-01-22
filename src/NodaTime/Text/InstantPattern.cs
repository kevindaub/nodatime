﻿using System;
using System.Globalization;
using NodaTime.Globalization;
using NodaTime.Text.Patterns;
using NodaTime.Utility;

namespace NodaTime.Text
{
    /// <summary>
    /// Represents a pattern for parsing and formatting <see cref="Instant"/> values.
    /// </summary>
    /// <threadsafety>
    /// When used with a read-only <see cref="CultureInfo" />, this type is immutable and instances
    /// may be shared freely between threads. We recommend only using read-only cultures for patterns, although this is
    /// not currently enforced.
    /// </threadsafety>
    public sealed class InstantPattern : IPattern<Instant>
    {
        /// <summary>
        /// Returns the general pattern, which always uses an invariant culture. The general pattern represents
        /// an instant as a UTC date/time in ISO-8601 style "yyyy-MM-ddTHH:mm:ssZ".
        /// </summary>
        public static InstantPattern GeneralPattern { get { return Patterns.GeneralPatternImpl; } }

        /// <summary>
        /// Returns an invariant instant pattern which is ISO-8601 compatible, providing up to 7 decimal places
        /// of sub-second accuracy. (These digits are omitted when unnecessary.)
        /// This corresponds to the text pattern "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFF;Z'".
        /// </summary>
        public static InstantPattern ExtendedIsoPattern { get { return Patterns.ExtendedIsoPatternImpl; } }

        // TODO: Possibly support an "f" standard pattern which is like "g" but down to ticks.
        private const string DefaultFormatPattern = "g";

        internal static readonly PatternBclSupport<Instant> BclSupport = new PatternBclSupport<Instant>(DefaultFormatPattern, fi => fi.InstantPatternParser);

        /// <summary>
        /// Class whose existence is solely to avoid type initialization order issues, most of which stem
        /// from needing NodaFormatInfo.InvariantInfo...
        /// </summary>
        private static class Patterns
        {
            internal static readonly InstantPattern ExtendedIsoPatternImpl = CreateWithInvariantCulture("yyyy'-'MM'-'dd'T'HH':'mm':'ss;FFFFFFF'Z'");
            internal static readonly InstantPattern GeneralPatternImpl = CreateWithInvariantCulture("g");
        }

        private readonly string patternText;
        private readonly NodaFormatInfo formatInfo;
        private readonly IPattern<Instant> pattern;

        /// <summary>
        /// Returns the pattern text for this pattern, as supplied on creation.
        /// </summary>
        public string PatternText { get { return patternText; } }

        /// <summary>
        /// Returns the localization information used in this pattern.
        /// </summary>
        internal NodaFormatInfo FormatInfo { get { return formatInfo; } }

        private InstantPattern(string patternText, NodaFormatInfo formatInfo, IPattern<Instant> pattern)
        {
            this.patternText = patternText;
            this.formatInfo = formatInfo;
            this.pattern = pattern;
        }

        /// <summary>
        /// Parses the given text value according to the rules of this pattern.
        /// </summary>
        /// <remarks>
        /// This method never throws an exception (barring a bug in Noda Time itself). Even errors such as
        /// the argument being null are wrapped in a parse result.
        /// </remarks>
        /// <param name="text">The text value to parse.</param>
        /// <returns>The result of parsing, which may be successful or unsuccessful.</returns>
        public ParseResult<Instant> Parse(string text)
        {
            return pattern.Parse(text);
        }

        /// <summary>
        /// Formats the given instant as text according to the rules of this pattern.
        /// </summary>
        /// <param name="value">The instant to format.</param>
        /// <returns>The instant formatted according to this pattern.</returns>
        public string Format(Instant value)
        {
            return pattern.Format(value);
        }

        /// <summary>
        /// Creates a pattern for the given pattern text and format info.
        /// </summary>
        /// <param name="patternText">Pattern text to create the pattern for</param>
        /// <param name="formatInfo">The format info to use in the pattern</param>
        /// <returns>A pattern for parsing and formatting instants.</returns>
        /// <exception cref="InvalidPatternException">The pattern text was invalid.</exception>
        internal static InstantPattern Create(string patternText, NodaFormatInfo formatInfo)
        {
            Preconditions.CheckNotNull(patternText, "patternText");
            Preconditions.CheckNotNull(formatInfo, "formatInfo");
            var pattern = formatInfo.InstantPatternParser.ParsePattern(patternText);
            return new InstantPattern(patternText, formatInfo, pattern);
        }

        /// <summary>
        /// Creates a pattern for the given pattern text and culture.
        /// </summary>
        /// <remarks>
        /// See the user guide for the available pattern text options.
        /// </remarks>
        /// <param name="patternText">Pattern text to create the pattern for</param>
        /// <param name="cultureInfo">The culture to use in the pattern</param>
        /// <returns>A pattern for parsing and formatting instants.</returns>
        /// <exception cref="InvalidPatternException">The pattern text was invalid.</exception>
        public static InstantPattern Create(string patternText, CultureInfo cultureInfo)
        {
            return Create(patternText, NodaFormatInfo.GetFormatInfo(cultureInfo));
        }

        /// <summary>
        /// Creates a pattern for the given pattern text in the current thread's current culture.
        /// </summary>
        /// <remarks>
        /// See the user guide for the available pattern text options. Note that the current culture
        /// is captured at the time this method is called - it is not captured at the point of parsing
        /// or formatting values.
        /// </remarks>
        /// <param name="patternText">Pattern text to create the pattern for</param>
        /// <returns>A pattern for parsing and formatting instants.</returns>
        /// <exception cref="InvalidPatternException">The pattern text was invalid.</exception>
        public static InstantPattern CreateWithCurrentCulture(string patternText)
        {
            return Create(patternText, NodaFormatInfo.CurrentInfo);
        }

        /// <summary>
        /// Creates a pattern for the given pattern text in the invariant culture.
        /// </summary>
        /// <remarks>
        /// See the user guide for the available pattern text options. Note that the current culture
        /// is captured at the time this method is called - it is not captured at the point of parsing
        /// or formatting values.
        /// </remarks>
        /// <param name="patternText">Pattern text to create the pattern for</param>
        /// <returns>A pattern for parsing and formatting instants.</returns>
        /// <exception cref="InvalidPatternException">The pattern text was invalid.</exception>
        public static InstantPattern CreateWithInvariantCulture(string patternText)
        {
            return Create(patternText, NodaFormatInfo.InvariantInfo);
        }

        /// <summary>
        /// Creates a "numeric" pattern for the given culture. The numeric format gives the
        /// number of ticks in decimal format, with or without thousands separators.
        /// </summary>
        /// <param name="cultureInfo">The culture to use in the pattern</param>
        /// <param name="includeThousandsSeparators">True to include thousands separators when parsing or formatting; false to omit them.</param>
        /// <returns>A numeric pattern for the configuration</returns>
        public static InstantPattern CreateNumericPattern(CultureInfo cultureInfo, bool includeThousandsSeparators)
        {
            return Create(includeThousandsSeparators ? "n" : "d", cultureInfo);
        }

        /// <summary>
        /// Creates a pattern for the same original pattern text as this pattern, but with the specified
        /// localization information.
        /// </summary>
        /// <param name="formatInfo">The localization information to use in the new pattern.</param>
        /// <returns>A new pattern with the given localization information.</returns>
        private InstantPattern WithFormatInfo(NodaFormatInfo formatInfo)
        {
            return Create(patternText, formatInfo);
        }

        /// <summary>
        /// Creates a pattern for the same original pattern text as this pattern, but with the specified
        /// culture.
        /// </summary>
        /// <param name="cultureInfo">The culture to use in the new pattern.</param>
        /// <returns>A new pattern with the given culture.</returns>
        public InstantPattern WithCulture(CultureInfo cultureInfo)
        {
            return WithFormatInfo(NodaFormatInfo.GetFormatInfo(cultureInfo));
        }
    }
}
