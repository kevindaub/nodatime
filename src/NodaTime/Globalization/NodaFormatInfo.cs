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
#region usings
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using NodaTime.Properties;
using NodaTime.Text;
using NodaTime.Text.Patterns;

#endregion

namespace NodaTime.Globalization
{
    /// <summary>
    ///   Defines how NodaTime values are formatted and displayed, depending on the culture.
    /// </summary>
    [DebuggerStepThrough]
    public class NodaFormatInfo : IFormatProvider, ICloneable
    {
        #region Patterns and pattern parsers
        private static readonly IPatternParser<Offset> GeneralOffsetPatternParser = new OffsetPatternParser();
        private static readonly IPatternParser<Instant> GeneralInstantPatternParser = new InstantPatternParser();
        private static readonly IPatternParser<LocalTime> GeneralLocalTimePatternParser = new LocalTimePatternParser(LocalTime.Midnight);
        private static readonly IPatternParser<LocalDate> GeneralLocalDatePatternParser = new LocalDatePatternParser(LocalDatePattern.DefaultTemplateValue);

        // Not read-only as they need to be changed after cloning.
        private FixedFormatInfoPatternParser<Offset> offsetPatternParser;
        private FixedFormatInfoPatternParser<Instant> instantPatternParser;
        private FixedFormatInfoPatternParser<LocalTime> localTimePatternParser;
        private FixedFormatInfoPatternParser<LocalDate> localDatePatternParser;
        #endregion

        /// <summary>
        /// A NodaFormatInfo wrapping the invariant culture.
        /// </summary>
        public static NodaFormatInfo InvariantInfo  = new NodaFormatInfo(CultureInfo.InvariantCulture);

        private static readonly IDictionary<String, NodaFormatInfo> Infos = new Dictionary<string, NodaFormatInfo>();
        internal static bool DisableCaching; // Used in testing and debugging

        private readonly string description;
        private DateTimeFormatInfo dateTimeFormat;
        private bool isReadOnly;
        private NumberFormatInfo numberFormat;
        private string offsetPatternFull;
        private string offsetPatternLong;
        private string offsetPatternMedium;
        private string offsetPatternShort;
        private readonly IList<string> longMonthNames;
        private readonly IList<string> longMonthGenitiveNames;
        private readonly IList<string> longDayNames;
        private readonly IList<string> shortMonthNames;
        private readonly IList<string> shortMonthGenitiveNames;
        private readonly IList<string> shortDayNames;

        /// <summary>
        /// Initializes a new instance of the <see cref="NodaFormatInfo" /> class.
        /// </summary>
        /// <param name="cultureInfo">The culture info to base this on.</param>
        internal NodaFormatInfo(CultureInfo cultureInfo)
        {
            if (cultureInfo == null)
            {
                cultureInfo = Thread.CurrentThread.CurrentCulture;
            }
            CultureInfo = cultureInfo;
            NumberFormat = cultureInfo.NumberFormat;
            DateTimeFormat = cultureInfo.DateTimeFormat;
            Name = cultureInfo.Name;
            description = "NodaFormatInfo[" + cultureInfo.Name + "]";
            var manager = PatternResources.ResourceManager;
            offsetPatternFull = manager.GetString("OffsetPatternFull", cultureInfo);
            OffsetPatternLong = manager.GetString("OffsetPatternLong", cultureInfo);
            offsetPatternMedium = manager.GetString("OffsetPatternMedium", cultureInfo);
            offsetPatternShort = manager.GetString("OffsetPatternShort", cultureInfo);

            offsetPatternParser = FixedFormatInfoPatternParser<Offset>.CreateCachingParser(GeneralOffsetPatternParser, this);
            instantPatternParser = FixedFormatInfoPatternParser<Instant>.CreateCachingParser(GeneralInstantPatternParser, this);
            localTimePatternParser = FixedFormatInfoPatternParser<LocalTime>.CreateCachingParser(GeneralLocalTimePatternParser, this);
            localDatePatternParser = FixedFormatInfoPatternParser<LocalDate>.CreateCachingParser(GeneralLocalDatePatternParser, this);

            // Turn month names into 1-based read-only lists
            longMonthNames = ConvertMonthArray(cultureInfo.DateTimeFormat.MonthNames);
            shortMonthNames = ConvertMonthArray(cultureInfo.DateTimeFormat.AbbreviatedMonthNames);
            longMonthGenitiveNames = ConvertGenitiveMonthArray(longMonthNames, cultureInfo.DateTimeFormat.MonthGenitiveNames);
            shortMonthGenitiveNames = ConvertGenitiveMonthArray(shortMonthNames, cultureInfo.DateTimeFormat.AbbreviatedMonthGenitiveNames);
            longDayNames = ConvertDayArray(cultureInfo.DateTimeFormat.DayNames);
            shortDayNames = ConvertDayArray(cultureInfo.DateTimeFormat.AbbreviatedDayNames);
        }

        /// <summary>
        /// The BCL returns arrays of month names starting at 0; we want a read-only list starting at 1 (with 0 as null).
        /// </summary>
        private static IList<string> ConvertMonthArray(string[] monthNames)
        {
            List<string> list = new List<string>(monthNames);
            list.Insert(0, null);
            return list.AsReadOnly();
        }

        /// <summary>
        /// The BCL returns arrays of week names starting at 0 as Sunday; we want a read-only list starting at 1 (with 0 as null)
        /// and with 7 as Sunday.
        /// </summary>
        private static IList<string> ConvertDayArray(string[] dayNames)
        {
            List<string> list = new List<string>(dayNames);
            list.Add(dayNames[0]);
            list[0] = null;            
            return list.AsReadOnly();
        }

        /// <summary>
        /// Checks whether any of the genitive names differ from the non-genitive names, and returns
        /// either a reference to the non-genitive names or a converted list as per ConvertMonthArray.
        /// </summary>
        private IList<string> ConvertGenitiveMonthArray(IList<string> nonGenitiveNames, string[] bclNames)
        {
            bool hasGenitive = false;
            for (int i = 0; i < bclNames.Length && !hasGenitive; i++)
            {
                if (bclNames[i] != nonGenitiveNames[i + 1])
                {
                    hasGenitive = true;
                }
            }
            return hasGenitive ? ConvertMonthArray(bclNames) : nonGenitiveNames;
        }

        /// <summary>
        /// Gets the culture info associated with this format provider.
        /// </summary>
        public CultureInfo CultureInfo { get; private set; }

        internal FixedFormatInfoPatternParser<Offset> OffsetPatternParser { get { return offsetPatternParser; } }
        internal FixedFormatInfoPatternParser<Instant> InstantPatternParser { get { return instantPatternParser; } }
        internal FixedFormatInfoPatternParser<LocalTime> LocalTimePatternParser { get { return localTimePatternParser; } }
        internal FixedFormatInfoPatternParser<LocalDate> LocalDatePatternParser { get { return localDatePatternParser; } }

        // TODO: Make these writable?
        /// <summary>
        /// Returns a read-only list of the names of the months for the default calendar for this culture.
        /// See the usage guide for caveats around the use of these names for other calendars.
        /// Element 0 of the list is null, to allow a more natural mapping from (say) 1 to the string "January".
        /// </summary>
        public IList<string> LongMonthNames { get { return longMonthNames; } }
        /// <summary>
        /// Returns a read-only list of the abbreviated names of the months for the default calendar for this culture.
        /// See the usage guide for caveats around the use of these names for other calendars.
        /// Element 0 of the list is null, to allow a more natural mapping from (say) 1 to the string "Jan".
        /// </summary>
        public IList<string> ShortMonthNames { get { return shortMonthNames; } }
        /// <summary>
        /// Returns a read-only list of the names of the months for the default calendar for this culture.
        /// See the usage guide for caveats around the use of these names for other calendars.
        /// Element 0 of the list is null, to allow a more natural mapping from (say) 1 to the string "January".
        /// The genitive form is used for month text where the day of month also appears in the pattern.
        /// If the culture does not use genitive month names, this property will return the same reference as
        /// <see cref="LongMonthNames"/>.
        /// </summary>
        public IList<string> LongMonthGenitiveNames { get { return longMonthGenitiveNames; } }
        /// <summary>
        /// Returns a read-only list of the abbreviated names of the months for the default calendar for this culture.
        /// See the usage guide for caveats around the use of these names for other calendars.
        /// Element 0 of the list is null, to allow a more natural mapping from (say) 1 to the string "Jan".
        /// The genitive form is used for month text where the day also appears in the pattern.
        /// If the culture does not use genitive month names, this property will return the same reference as
        /// <see cref="ShortMonthNames"/>.
        /// </summary>
        public IList<string> ShortMonthGenitiveNames { get { return shortMonthGenitiveNames; } }
        /// <summary>
        /// Returns a read-only list of the names of the days of the week for the default calendar for this culture.
        /// See the usage guide for caveats around the use of these names for other calendars.
        /// Element 0 of the list is null, and the other elements correspond with the index values returned from
        /// <see cref="LocalDateTime.DayOfWeek"/> and similar properties.
        /// </summary>
        public IList<string> LongDayNames { get { return longDayNames; } }
        /// <summary>
        /// Returns a read-only list of the abbreviated names of the days of the week for the default calendar for this culture.
        /// See the usage guide for caveats around the use of these names for other calendars.
        /// Element 0 of the list is null, and the other elements correspond with the index values returned from
        /// <see cref="LocalDateTime.DayOfWeek"/> and similar properties.
        /// </summary>
        public IList<string> ShortDayNames { get { return shortDayNames; } }

        /// <summary>
        /// Gets or sets the number format. This is usually initialized from the <see cref="CultureInfo"/>, but may be
        /// changed indepedently.
        /// </summary>
        /// <value>
        /// The <see cref="NumberFormatInfo" />. May not be <c>null</c>.
        /// </value>
        public NumberFormatInfo NumberFormat
        {            
            get { return numberFormat; }
            set { SetValue(value, ref numberFormat); }
        }

        /// <summary>
        /// Gets or sets the date time format.
        /// </summary>
        /// <value>
        /// The <see cref="DateTimeFormatInfo" />. May not be <c>null</c>.
        /// </value>
        public DateTimeFormatInfo DateTimeFormat
        {
            get { return dateTimeFormat; }            
            set { SetValue(value, ref dateTimeFormat); }
        }

        /// <summary>
        /// Gets the decimal separator from the number format associated with this provider.
        /// </summary>
        public string DecimalSeparator { get { return NumberFormat.NumberDecimalSeparator; } }

        /// <summary>
        /// Name of the culture providing this formatting information.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        ///   Gets the positive sign.
        /// </summary>
        public string PositiveSign { get { return NumberFormat.PositiveSign; } }

        /// <summary>
        /// Gets the negative sign.
        /// </summary>
        public string NegativeSign { get { return NumberFormat.NegativeSign; } }

        /// <summary>
        /// Gets the time separator.
        /// </summary>
        public string TimeSeparator { get { return DateTimeFormat.TimeSeparator; } }

        /// <summary>
        /// Gets the date separator.
        /// </summary>
        public string DateSeparator { get { return DateTimeFormat.DateSeparator; } }

        /// <summary>
        /// Gets the AM designator.
        /// </summary>
        public string AMDesignator { get { return DateTimeFormat.AMDesignator; } }

        /// <summary>
        /// Gets the PM designator.
        /// </summary>
        public string PMDesignator { get { return DateTimeFormat.PMDesignator; } }

        /// <summary>
        /// Gets the <see cref="NodaFormatInfo" /> object for the current thread.
        /// </summary>
        public static NodaFormatInfo CurrentInfo
        {
            get { return GetInstance(Thread.CurrentThread.CurrentCulture); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is read only; otherwise, <c>false</c>.
        /// </value>
        public bool IsReadOnly
        {
            get { return isReadOnly; }
            set
            {
                if (isReadOnly && value != true)
                {
                    throw new InvalidOperationException("Cannot make a read only object writable.");
                }
                isReadOnly = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Offset" /> "F" pattern.
        /// </summary>
        /// <value>
        /// The full standard offset pattern.
        /// </value>
        public string OffsetPatternFull
        {
            get { return offsetPatternFull; }
            set { SetValue(value, ref offsetPatternFull); }
        }

        /// <summary>
        /// Gets or sets the <see cref="Offset" /> "L" pattern.
        /// </summary>
        /// <value>
        /// The long standard offset pattern.
        /// </value>
        public string OffsetPatternLong
        {            
            get { return offsetPatternLong; }
            set { SetValue(value, ref offsetPatternLong); }
        }

        /// <summary>
        /// Gets or sets the <see cref="Offset" /> "M" pattern.
        /// </summary>
        /// <value>
        /// The medium standard offset pattern.
        /// </value>
        public string OffsetPatternMedium
        {
            get { return offsetPatternMedium; }
            set { SetValue(value, ref offsetPatternMedium); }
        }

        /// <summary>
        ///   Gets or sets the <see cref="Offset" /> "S" pattern.
        /// </summary>
        /// <value>
        ///   The offset pattern short.
        /// </value>
        public string OffsetPatternShort
        {
            
            get { return offsetPatternShort; }
            
            set { SetValue(value, ref offsetPatternShort); }
        }

        #region ICloneable Members
        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <remarks>
        /// This method is present to implement ICloneable, which has a return value of <see cref="System.Object"/>.
        /// The implementation simply calls the public <see cref="Clone()"/> method.
        /// </remarks>
        object ICloneable.Clone()
        {
            return Clone();
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <remarks>
        /// The returned value is always writable and independent of the original object. The culture info,
        /// number format and date/time format all cloned too.
        /// </remarks>
        /// <returns>A writable clone of this value.</returns>
        public virtual NodaFormatInfo Clone()
        {
            var info = (NodaFormatInfo)MemberwiseClone();
            info.isReadOnly = false;

            // Take care when cloning culture information - we want two end up with two objects referring to each other
            var cultureInfo = CultureInfo;
            var nodaCultureInfo = cultureInfo as NodaCultureInfo;
            info.CultureInfo = nodaCultureInfo == null ? (CultureInfo) cultureInfo.Clone() : nodaCultureInfo.Clone(info);
            info.NumberFormat = (NumberFormatInfo) NumberFormat.Clone();
            info.DateTimeFormat = (DateTimeFormatInfo) DateTimeFormat.Clone();
            info.offsetPatternParser = FixedFormatInfoPatternParser<Offset>.CreateNonCachingParser(GeneralOffsetPatternParser, info);
            info.instantPatternParser = FixedFormatInfoPatternParser<Instant>.CreateNonCachingParser(GeneralInstantPatternParser, info);
            info.localTimePatternParser = FixedFormatInfoPatternParser<LocalTime>.CreateNonCachingParser(GeneralLocalTimePatternParser, info);
            info.localDatePatternParser = FixedFormatInfoPatternParser<LocalDate>.CreateNonCachingParser(GeneralLocalDatePatternParser, info);
            return info;
        }

        /// <summary>
        /// Returns a clone with the specified NodaCultureInfo as the CultureInfo.
        /// This is to avoid stack overflows when cloning a NodaFormatInfo with a NodaCultureInfo. This is slightly tricky,
        /// as when you clone either a NodaCultureInfo or a NodaFormatInfo with a NodaCultureInfo as its culture, we need
        /// to create two objects which refer to each other.
        /// </summary>
        internal NodaFormatInfo Clone(NodaCultureInfo clonedCultureInfo)
        {
            // TODO: Potentially extract common code with the parameterless Clone method above.
            var info = (NodaFormatInfo)MemberwiseClone();
            info.isReadOnly = false;
            info.CultureInfo = clonedCultureInfo;
            info.NumberFormat = (NumberFormatInfo)NumberFormat.Clone();
            info.DateTimeFormat = (DateTimeFormatInfo)DateTimeFormat.Clone();
            info.offsetPatternParser = FixedFormatInfoPatternParser<Offset>.CreateNonCachingParser(GeneralOffsetPatternParser, info);
            info.instantPatternParser = FixedFormatInfoPatternParser<Instant>.CreateNonCachingParser(GeneralInstantPatternParser, info);
            info.localTimePatternParser = FixedFormatInfoPatternParser<LocalTime>.CreateNonCachingParser(GeneralLocalTimePatternParser, info);
            info.localDatePatternParser = FixedFormatInfoPatternParser<LocalDate>.CreateNonCachingParser(GeneralLocalDatePatternParser, info);
            return info;
        }
        #endregion

        #region IFormatProvider Members
        /// <summary>
        ///   Returns an object that provides formatting services for the specified type.
        /// </summary>
        /// <param name="formatType">An object that specifies the type of format object to return.</param>
        /// <returns>
        ///   An instance of the object specified by <paramref name = "formatType" />, if the <see cref="T:System.IFormatProvider" />
        ///   implementation can supply that type of object; otherwise, null.
        /// </returns>
        
        public object GetFormat(Type formatType)
        {
            if (formatType == typeof(NodaFormatInfo))
            {
                return this;
            }
            if (formatType == typeof(NumberFormatInfo))
            {
                return NumberFormat;
            }
            if (formatType == typeof(DateTimeFormatInfo))
            {
                return DateTimeFormat;
            }
            return null;
        }
        #endregion

        /// <summary>
        ///   Clears the cache.
        /// </summary>
        internal static void ClearCache()
        {
            lock (Infos) Infos.Clear();
        }

        private void SetValue<T>(T value, ref T property) where T : class
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException(Messages.Noda_CannotChangeReadOnly);
            }
            if (value == null)
            {
                throw new ArgumentNullException("value", Messages.Noda_ArgumentNull);
            }
            property = value;
        }

        /// <summary>
        ///   Gets the <see cref="NodaFormatInfo" /> for the given <see cref="CultureInfo" />.
        /// </summary>
        /// <param name="cultureInfo">The culture info.</param>
        /// <returns>The <see cref="NodaFormatInfo" />. Will next be <c>null</c>.</returns>
        internal static NodaFormatInfo GetFormatInfo(CultureInfo cultureInfo)
        {
            if (cultureInfo == null)
            {
                throw new ArgumentNullException("cultureInfo");
            }
            string name = cultureInfo.Name;
            NodaFormatInfo result;
            if (cultureInfo == CultureInfo.InvariantCulture)
            {
                return InvariantInfo;
            }
            // Never cache (or consult the cache) for non-read-only cultures.
            // TODO
            if (!cultureInfo.IsReadOnly)
            {
                return new NodaFormatInfo(cultureInfo);
            }
            lock (Infos)
            {
                // TODO: Consider fetching by the cultureInfo instead, as otherwise two culture instances
                // with the same name will give the wrong result.
                if (DisableCaching || !Infos.TryGetValue(name, out result))
                {
                    result = new NodaFormatInfo(cultureInfo) { IsReadOnly = true };
                    if (!DisableCaching)
                    {
                        Infos.Add(name, result);
                    }
                }
            }
            return result;
        }

        /// <summary>
        ///   Gets the <see cref="NodaFormatInfo" /> for the given <see cref="IFormatProvider" />. If the
        ///   format provider is <c>null</c> or if it does not provide a <see cref="NodaFormatInfo" />
        ///   object then the format object for the current thread is returned.
        /// </summary>
        /// <param name="provider">The <see cref="IFormatProvider" />.</param>
        /// <returns>The <see cref="NodaFormatInfo" />. Will next be <c>null.</c></returns>
        public static NodaFormatInfo GetInstance(IFormatProvider provider)
        {
            if (provider != null)
            {
                var format = provider as NodaFormatInfo;
                if (format != null)
                {
                    return format;
                }
                format = provider.GetFormat(typeof(NodaFormatInfo)) as NodaFormatInfo;
                if (format != null)
                {
                    return format;
                }
                var cultureInfo = provider as CultureInfo;
                if (cultureInfo != null)
                {
                    return GetFormatInfo(cultureInfo);
                }
            }
            return GetInstance(CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Sets the <see cref="NodaFormatInfo" /> to use for the given culture.
        /// </summary>
        /// <param name="cultureInfo">The culture info.</param>
        /// <param name="formatInfo">The format info.</param>
        internal static void SetFormatInfo(CultureInfo cultureInfo, NodaFormatInfo formatInfo)
        {
            if (cultureInfo == null)
            {
                throw new ArgumentNullException("cultureInfo");
            }
            string name = cultureInfo.Name;
            if (formatInfo == null)
            {
                lock (Infos) Infos.Remove(name);
            }
            else
            {
                formatInfo.IsReadOnly = true;
                lock (Infos) Infos[name] = formatInfo;
            }
        }

        /// <summary>
        ///   Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        ///   A <see cref="System.String" /> that represents this instance.
        /// </returns>
        
        public override string ToString()
        {
            return description;
        }
    }
}