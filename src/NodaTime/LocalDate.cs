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
using NodaTime.Calendars;
using NodaTime.Globalization;
using NodaTime.Text;
using NodaTime.Text.Patterns;

namespace NodaTime
{
    /// <summary>
    /// LocalDate is an immutable struct representing a date within the calendar,
    /// with no reference to a particular time zone or time of day.
    /// </summary>
    public struct LocalDate : IEquatable<LocalDate>
    {
        private readonly LocalDateTime localTime;

        /// <summary>
        /// Constructs an instance for the given year, month and day in the ISO calendar.
        /// </summary>
        /// <param name="year">Year of the new date/</param>
        /// <param name="month">Month of year of the new date/</param>
        /// <param name="day">Day of month of the new date/</param>
        public LocalDate(int year, int month, int day)
            : this(year, month, day, CalendarSystem.Iso)
        {
        }

        /// <summary>
        /// Constructs an instance for the given year, month and day in the specified calendar.
        /// </summary>
        /// <param name="year">Year of the new date/</param>
        /// <param name="month">Month of year of the new date/</param>
        /// <param name="day">Day of month of the new date/</param>
        /// <param name="calendar">Calendar system in which to create the date</param>
        public LocalDate(int year, int month, int day, CalendarSystem calendar)
            : this(new LocalDateTime(year, month, day, 0, 0, calendar))
        {
        }

        // Visible for extension methods.
        internal LocalDate(LocalDateTime localTime)
        {
            this.localTime = localTime;
        }

        /// <summary>Gets the calendar system associated with this local date.</summary>
        public CalendarSystem Calendar { get { return localTime.Calendar; } }

        /// <summary>Gets the year of this local date.</summary>
        public int Year { get { return localTime.Year; } }

        /// <summary>Gets the month of this local date within the year.</summary>
        public int MonthOfYear { get { return localTime.MonthOfYear; } }

        /// <summary>Gets the day of this local date within the month.</summary>
        public int DayOfMonth { get { return localTime.DayOfMonth; } }

        /// <summary>
        /// Week day of this local date expressed as an <see cref="NodaTime.IsoDayOfWeek"/> value,
        /// for calendars which use ISO days of the week.
        /// </summary>
        /// <exception cref="InvalidOperationException">The underlying calendar doesn't use ISO days of the week.</exception>
        public IsoDayOfWeek IsoDayOfWeek { get { return localTime.IsoDayOfWeek; } }

        /// <summary>
        /// Week day of this local date as a number.
        /// </summary>
        /// <remarks>
        /// For calendars using ISO week days, this gives 1 for Monday to 7 for Sunday.
        /// </remarks>
        /// <seealso cref="IsoDayOfWeek"/>
        public int DayOfWeek { get { return localTime.DayOfWeek; } }

        /// <summary>
        /// Gets the "week year" of this local date.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The WeekYear is the year that matches with the WeekOfWeekYear field.
        /// In the standard ISO8601 week algorithm, the first week of the year
        /// is that in which at least 4 days are in the year. As a result of this
        /// definition, day 1 of the first week may be in the previous year.
        /// The WeekYear allows you to query the effective year for that day
        /// </para>
        /// <para>
        /// For example, January 1st 2011 was a Saturday, so only two days of that week
        /// (Saturday and Sunday) were in 2011. Therefore January 1st is part of
        /// week 52 of WeekYear 2010. Conversely, December 31st 2012 is a Monday,
        /// so is part of week 1 of WeekYear 2013.
        /// </para>
        /// </remarks>
        public int WeekYear { get { return localTime.WeekYear; } }

        /// <summary>Gets the week within the WeekYear. See <see cref="WeekYear"/> for more details.</summary>
        public int WeekOfWeekYear { get { return localTime.WeekOfWeekYear; } }

        /// <summary>Gets the year of this local date within the century.</summary>
        public int YearOfCentury { get { return localTime.YearOfCentury; } }

        /// <summary>Gets the year of this local date within the era.</summary>
        public int YearOfEra { get { return localTime.YearOfEra; } }

        /// <summary>Gets the era of this local date.</summary>
        public Era Era { get { return localTime.Era; } }

        /// <summary>Gets the day of this local date within the year.</summary>
        public int DayOfYear { get { return localTime.DayOfYear; } }

        /// <summary>
        /// Gets a <see cref="LocalDateTime" /> at midnight on the date represented by this local date, in the same calendar system.
        /// </summary>
        public LocalDateTime LocalDateTime { get { return localTime; } }

        /// <summary>
        /// Adds the specified period to the date.
        /// </summary>
        /// <param name="date">The date to add the period to</param>
        /// <param name="period">The period to add</param>
        /// <returns>The sum of the given date and period</returns>
        // TODO: Assert no units smaller than a day, and more documentation.
        public static LocalDate operator +(LocalDate date, Period period)
        {
            if (period == null)
            {
                throw new ArgumentNullException("period");
            }
            return new LocalDate(date.LocalDateTime + period);
        }

        /// <summary>
        /// Combines the given <see cref="LocalDate"/> and <see cref="LocalTime"/> components
        /// into a single <see cref="LocalDateTime"/>.
        /// </summary>
        /// <param name="date">The date to add the time to</param>
        /// <param name="time">The time to add</param>
        /// <returns>The sum of the given date and time</returns>
        public static LocalDateTime operator +(LocalDate date, LocalTime time)
        {
            LocalInstant localDateInstant = date.localTime.LocalInstant;
            LocalInstant localInstant = new LocalInstant(localDateInstant.Ticks + time.TickOfDay);
            return new LocalDateTime(localInstant, date.localTime.Calendar);
        }

        /// <summary>
        /// Subtracts the specified period from the date.
        /// </summary>
        /// <param name="date">The date to subtract the period from</param>
        /// <param name="period">The period to subtract</param>
        /// <returns>The result of subtracting the given period from the date</returns>
        // TODO: Assert no units smaller than a day, and more documentation.
        public static LocalDate operator -(LocalDate date, Period period)
        {
            if (period == null)
            {
                throw new ArgumentNullException("period");
            }
            return new LocalDate(date.LocalDateTime - period);
        }

        /// <summary>
        /// Compares two <see cref="LocalDate" /> values for equality. This requires
        /// that the dates be the same, within the same calendar.
        /// </summary>
        /// <param name="lhs">The first value to compare</param>
        /// <param name="rhs">The second value to compare</param>
        /// <returns>True if the two dates are the same and in the same calendar; false otherwise</returns>
        public static bool operator ==(LocalDate lhs, LocalDate rhs)
        {
            return lhs.localTime == rhs.localTime;
        }

        /// <summary>
        /// Compares two <see cref="LocalDate" /> values for inequality.
        /// </summary>
        /// <param name="lhs">The first value to compare</param>
        /// <param name="rhs">The second value to compare</param>
        /// <returns>False if the two dates are the same and in the same calendar; true otherwise</returns>
        public static bool operator !=(LocalDate lhs, LocalDate rhs)
        {
            return lhs.localTime != rhs.localTime;
        }

        /// <summary>
        /// Returns a hash code for this local date.
        /// </summary>
        /// <returns>A hash code for this local date.</returns>
        public override int GetHashCode()
        {
            return localTime.GetHashCode();
        }

        /// <summary>
        /// Compares two <see cref="LocalDate"/> values for equality. This requires
        /// that the dates be the same, within the same calendar.
        /// </summary>
        /// <param name="obj">The object to compare this date with.</param>
        /// <returns>True if the given value is another local date equal to this one; false otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is LocalDate))
            {
                return false;
            }
            return this == (LocalDate)obj;
        }

        /// <summary>
        /// Compares two <see cref="LocalDate"/> values for equality. This requires
        /// that the dates be the same, within the same calendar.
        /// </summary>
        /// <param name="other">The value to compare this date with.</param>
        /// <returns>True if the given value is another local date equal to this one; false otherwise.</returns>
        public bool Equals(LocalDate other)
        {
            return this == other;
        }

        /// <summary>
        /// Creates a new LocalDate representing the same physical date, but in a different calendar.
        /// The returned LocalDate is likely to have different field values to this one.
        /// For example, January 1st 1970 in the Gregorian calendar was December 19th 1969 in the Julian calendar.
        /// </summary>
        /// <param name="calendarSystem">The calendar system to convert this local date to. Must not be null.</param>
        /// <returns>The converted LocalDate</returns>
        public LocalDate WithCalendar(CalendarSystem calendarSystem)
        {
            // TODO: This currently assumes the time will stay as midnight. Is that valid?
            return new LocalDate(LocalDateTime.WithCalendar(calendarSystem));
        }

        /// <summary>
        /// Returns a new LocalDate representing the current value with the given number of years added.
        /// </summary>
        /// <remarks>
        /// If the resulting date is invalid, lower fields (typically the day of month) are reduced to find a valid value.
        /// For example, adding one year to February 29th 2012 will return February 28th 2013; subtracting one year from
        /// February 29th 2012 will return February 28th 2011.
        /// </remarks>
        /// <param name="years">The number of years to add</param>
        /// <returns>The current value plus the given number of years.</returns>
        public LocalDate PlusYears(int years)
        {
            return new LocalDate(LocalDateTime.PlusYears(years));
        }

        /// <summary>
        /// Returns a new LocalDate representing the current value with the given number of months added.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method does not try to maintain the year of the current value, so adding four months to a value in 
        /// October will result in a value in the following February.
        /// </para>
        /// <para>
        /// If the resulting date is invalid, the day of month is reduced to find a valid value.
        /// For example, adding one month to January 30th 2011 will return February 28th 2011; subtracting one month from
        /// March 30th 2011 will return February 28th 2011.
        /// </para>
        /// </remarks>
        /// <param name="months">The number of months to add</param>
        /// <returns>The current date plus the given number of months</returns>
        public LocalDate PlusMonths(int months)
        {
            return new LocalDate(LocalDateTime.PlusMonths(months));
        }

        /// <summary>
        /// Returns a new LocalDate representing the current value with the given number of days added.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method does not try to maintain the month or year of the current value, so adding 3 days to a value of January 30th
        /// will result in a value of February 2nd.
        /// </para>
        /// </remarks>
        /// <param name="days">The number of days to add</param>
        /// <returns>The current value plus the given number of days.</returns>
        public LocalDate PlusDays(int days)
        {
            return new LocalDate(LocalDateTime.PlusDays(days));
        }

        /// <summary>
        /// Returns a new LocalDate representing the current value with the given number of weeks added.
        /// </summary>
        /// <param name="weeks">The number of weeks to add</param>
        /// <returns>The current value plus the given number of weeks.</returns>
        public LocalDate PlusWeeks(int weeks)
        {
            return new LocalDate(LocalDateTime.PlusWeeks(weeks));
        }

        /// <summary>
        /// Returns the next <see cref="LocalDate" /> falling on the specified <see cref="IsoDayOfWeek"/>.
        /// This is a strict "next" - if this date on already falls on the target
        /// day of the week, the returned value will be a week later.
        /// </summary>
        /// <param name="targetDayOfWeek">The ISO day of the week to return the next date of.</param>
        /// <returns>The next <see cref="LocalDate"/> falling on the specified day of the week.</returns>
        /// <exception cref="InvalidOperationException">The underlying calendar doesn't use ISO days of the week.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="targetDayOfWeek"/> is not a valid day of the
        /// week (Monday to Sunday).</exception>
        public LocalDate Next(IsoDayOfWeek targetDayOfWeek)
        {
            // Avoids boxing...
            if (targetDayOfWeek < IsoDayOfWeek.Monday || targetDayOfWeek > IsoDayOfWeek.Sunday)
            {
                throw new ArgumentOutOfRangeException("targetDayOfWeek");
            }
            // This will throw the desired exception for calendars with different week systems.
            IsoDayOfWeek thisDay = this.IsoDayOfWeek;
            int difference = targetDayOfWeek - thisDay;
            if (difference <= 0)
            {
                difference += 7;
            }
            return this.PlusDays(difference);
        }

        /// <summary>
        /// Returns the previous <see cref="LocalDate" /> falling on the specified <see cref="IsoDayOfWeek"/>.
        /// This is a strict "previous" - if this date on already falls on the target
        /// day of the week, the returned value will be a week earlier.
        /// </summary>
        /// <param name="targetDayOfWeek">The ISO day of the week to return the previous date of.</param>
        /// <returns>The previous <see cref="LocalDate"/> falling on the specified day of the week.</returns>
        /// <exception cref="InvalidOperationException">The underlying calendar doesn't use ISO days of the week.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="targetDayOfWeek"/> is not a valid day of the
        /// week (Monday to Sunday).</exception>
        public LocalDate Previous(IsoDayOfWeek targetDayOfWeek)
        {
            // Avoids boxing...
            if (targetDayOfWeek < IsoDayOfWeek.Monday || targetDayOfWeek > IsoDayOfWeek.Sunday)
            {
                throw new ArgumentOutOfRangeException("targetDayOfWeek");
            }
            // This will throw the desired exception for calendars with different week systems.
            IsoDayOfWeek thisDay = this.IsoDayOfWeek;
            int difference = targetDayOfWeek - thisDay;
            if (difference >= 0)
            {
                difference -= 7;
            }
            return this.PlusDays(difference);
        }

        #region Formatting
        /// <summary>Formats the value of the current instance using the specified format.</summary>
        /// <returns>A <see cref="T:System.String" /> containing the value of the current instance in the specified format.</returns>
        /// <param name="patternText">The <see cref="T:System.String" /> specifying the pattern to use.
        /// -or- null to use the default pattern defined for the type of the <see cref="T:System.IFormattable" /> implementation. 
        /// </param>
        /// <param name="formatProvider">The <see cref="T:System.IFormatProvider" /> to use to format the value.
        /// -or- null to obtain the numeric format information from the current locale setting of the operating system. 
        /// </param>
        /// <filterpriority>2</filterpriority>
        public string ToString(string patternText, IFormatProvider formatProvider)
        {
            return PatternSupport.Format(this, patternText, NodaFormatInfo.GetInstance(formatProvider));
        }

        /// <summary>Returns a <see cref="System.String" /> that represents this instance.</summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return PatternSupport.Format(this, null, NodaFormatInfo.CurrentInfo);
        }

        /// <summary>Formats the value of the current instance using the specified format.</summary>
        /// <returns>A <see cref="T:System.String" /> containing the value of the current instance in the specified format.</returns>
        /// <param name="patternText">The <see cref="T:System.String" /> specifying the pattern to use.
        /// -or- null to use the default pattern defined for the type of the <see cref="T:System.IFormattable" /> implementation. 
        /// </param>
        /// <filterpriority>2</filterpriority>
        public string ToString(string patternText)
        {
            return PatternSupport.Format(this, patternText, NodaFormatInfo.CurrentInfo);
        }

        /// <summary>Formats the value of the current instance using the specified format.</summary>
        /// <returns>A <see cref="T:System.String" /> containing the value of the current instance in the specified format.</returns>
        /// <param name="formatProvider">The <see cref="T:System.IFormatProvider" /> to use to format the value.
        /// -or- null to obtain the format information from the current locale setting of the operating system. 
        /// </param>
        /// <filterpriority>2</filterpriority>
        public string ToString(IFormatProvider formatProvider)
        {
            return PatternSupport.Format(this, null, NodaFormatInfo.GetInstance(formatProvider));
        }
        #endregion Formatting

        #region Parsing
        private static readonly string[] AllPatterns = { "D", "d" }; // Long, short
        private const string DefaultFormatPattern = "D"; // Long

        private static readonly PatternBclSupport<LocalDate> PatternSupport =
            new PatternBclSupport<LocalDate>(AllPatterns, DefaultFormatPattern, LocalDatePattern.DefaultTemplateValue, fi => fi.LocalDatePatternParser);

        /// <summary>Parses the given string using the current culture's default format provider.</summary>
        /// <param name="value">The value to parse.</param>
        /// <returns>The parsed local date.</returns>
        public static LocalDate Parse(string value)
        {
            return PatternSupport.Parse(value, NodaFormatInfo.CurrentInfo);
        }

        /// <summary>Parses the given string using the specified format provider.</summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="formatProvider">The format provider to use for culture-specific settings.</param>
        /// <returns>The parsed local date.</returns>
        public static LocalDate Parse(string value, IFormatProvider formatProvider)
        {
            return PatternSupport.Parse(value, NodaFormatInfo.GetInstance(formatProvider));
        }

        /// <summary>Parses the given string using the specified format pattern and format provider.</summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="patternText">The text of the pattern to use for parsing.</param>
        /// <param name="formatProvider">The format provider to use for culture-specific settings.</param>
        /// <returns>The parsed local date.</returns>
        public static LocalDate ParseExact(string value, string patternText, IFormatProvider formatProvider)
        {
            return PatternSupport.ParseExact(value, patternText, NodaFormatInfo.GetInstance(formatProvider));
        }

        /// <summary>Parses the given string using the specified patterns and format provider.</summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="patterns">The patterns to use for parsing.</param>
        /// <param name="formatProvider">The format provider to use for culture-specific settings.</param>
        /// <returns>The parsed local date.</returns>
        public static LocalDate ParseExact(string value, string[] patterns, IFormatProvider formatProvider)
        {
            return PatternSupport.ParseExact(value, patterns, NodaFormatInfo.GetInstance(formatProvider));
        }

        /// <summary>
        /// Attempts to parse the given string using the current culture's default format provider. If the parse is successful,
        /// the result is stored in the <paramref name="result"/> parameter and the return value is true;
        /// otherwise <see cref="LocalDatePattern.DefaultTemplateValue"/> is stored in the parameter and the return value is false.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="result">The parsed local date, when successful.</param>
        /// <returns>true if the value was parsed successfully; false otherwise.</returns>
        public static bool TryParse(string value, out LocalDate result)
        {
            return PatternSupport.TryParse(value, NodaFormatInfo.CurrentInfo, out result);
        }

        /// <summary>
        /// Attempts to parse the given string using the specified format provider.
        /// If the parse is successful, the result is stored in the <paramref name="result"/> parameter and the return value is true;
        /// otherwise <see cref="LocalDatePattern.DefaultTemplateValue"/> is stored in the parameter and the return value is false.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="formatProvider">The format provider to use for culture-specific settings.</param>
        /// <param name="result">The parsed local date, when successful.</param>
        /// <returns>true if the value was parsed successfully; false otherwise.</returns>
        public static bool TryParse(string value, IFormatProvider formatProvider, out LocalDate result)
        {
            return PatternSupport.TryParse(value, NodaFormatInfo.GetInstance(formatProvider), out result);
        }

        /// <summary>
        /// Attempts to parse the given string using the specified pattern, format provider and style.
        /// If the parse is successful, the result is stored in the <paramref name="result"/> parameter and the return value is true;
        /// otherwise <see cref="LocalDatePattern.DefaultTemplateValue"/> is stored in the parameter and the return value is false.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="patternText">The text of the pattern to use for parsing.</param>
        /// <param name="formatProvider">The format provider to use for culture-specific settings.</param>
        /// <param name="result">The parsed local date, when successful.</param>
        /// <returns>true if the value was parsed successfully; false otherwise.</returns>
        public static bool TryParseExact(string value, string patternText, IFormatProvider formatProvider, out LocalDate result)
        {
            return PatternSupport.TryParseExact(value, patternText, NodaFormatInfo.GetInstance(formatProvider), out result);
        }

        /// <summary>
        /// Attempts to parse the given string using the specified patterns and format provider.
        /// If the parse is successful, the result is stored in the <paramref name="result"/> parameter and the return value is true;
        /// otherwise <see cref="LocalDatePattern.DefaultTemplateValue"/> is stored in the parameter and the return value is false.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="patterns">The patterns to use for parsing.</param>
        /// <param name="formatProvider">The format provider to use for culture-specific settings.</param>
        /// <param name="result">The parsed local date, when successful.</param>
        /// <returns>true if the value was parsed successfully; false otherwise.</returns>
        public static bool TryParseExact(string value, string[] patterns, IFormatProvider formatProvider, out LocalDate result)
        {
            return PatternSupport.TryParseExact(value, patterns, NodaFormatInfo.GetInstance(formatProvider), out result);
        }
        #endregion Parsing
    }
}
