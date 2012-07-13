﻿#region Copyright and license information
// Copyright 2001-2009 Stephen Colebourne
// Copyright 2009-2012 Jon Skeet
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
using NodaTime.Utility;

namespace NodaTime
{
    /// <summary>
    /// A local date and time in a particular calendar system, combined with an offset from UTC. This is
    /// broadly similar to <see cref="DateTimeOffset" /> in the BCL.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A value of this type unambiguously represents both a local time and an instant on the timeline,
    /// but does not have a well-defined time zone. This means you cannot reliably know what the local
    /// time would be five minutes later, for example. While this doesn't sound terribly useful, it's very common
    /// in text representations.
    /// </para>
    /// <para>
    /// Currently there is no real text handling support for this type.
    /// </para>
    /// </remarks>
    /// <threadsafety>This type is an immutable value type. See the thread safety section of the user guide for more information.</threadsafety>
    // TODO(Post-V1): Introduce CompareByLocal and CompareByInstant properties returning IComparer<OffsetDateTime>.
    public struct OffsetDateTime : IEquatable<OffsetDateTime>
    {
        private readonly LocalDateTime localDateTime;
        private readonly Offset offset;

        /// <summary>
        /// Constructs a new offset date/time with the given local date and time, and the given offset from UTC.
        /// </summary>
        /// <param name="localDateTime">Local date and time to represent</param>
        /// <param name="offset">Offset from UTC</param>
        public OffsetDateTime(LocalDateTime localDateTime, Offset offset)
        {
            this.localDateTime = localDateTime;
            this.offset = offset;
        }

        /// <summary>Gets the calendar system associated with this local date and time.</summary>
        public CalendarSystem Calendar { get { return localDateTime.Calendar; } }

        /// <summary>Gets the year of this offset date and time.</summary>
        /// <remarks>This returns the "absolute year", so, for the ISO calendar,
        /// a value of 0 means 1 BC, for example.</remarks>
        public int Year { get { return localDateTime.Year; } }

        /// <summary>Gets the month of this offset date and time within the year.</summary>
        public int Month { get { return localDateTime.Month; } }

        /// <summary>Gets the day of this offset date and time within the month.</summary>
        public int Day { get { return localDateTime.Day; } }

        /// <summary>
        /// Gets the week day of this offset date and time expressed as an <see cref="NodaTime.IsoDayOfWeek"/> value,
        /// for calendars which use ISO days of the week.
        /// </summary>
        /// <exception cref="InvalidOperationException">The underlying calendar doesn't use ISO days of the week.</exception>
        /// <seealso cref="DayOfWeek"/>
        public IsoDayOfWeek IsoDayOfWeek { get { return localDateTime.IsoDayOfWeek; } }

        /// <summary>
        /// Gets the week day of this offset date and time as a number.
        /// </summary>
        /// <remarks>
        /// For calendars using ISO week days, this gives 1 for Monday to 7 for Sunday.
        /// </remarks>
        /// <seealso cref="IsoDayOfWeek"/>
        public int DayOfWeek { get { return localDateTime.DayOfWeek; } }

        /// <summary>
        /// Gets the "week year" of this offset date and time.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The WeekYear is the year that matches with the <see cref="WeekOfWeekYear"/> field.
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
        public int WeekYear { get { return localDateTime.WeekYear; } }

        /// <summary>Gets the week within the WeekYear. See <see cref="WeekYear"/> for more details.</summary>
        public int WeekOfWeekYear { get { return localDateTime.WeekOfWeekYear; } }

        /// <summary>Gets the year of this offset date and time within the century.</summary>
        /// <remarks>This always returns a value in the range 0 to 99 inclusive.</remarks>
        public int YearOfCentury { get { return localDateTime.YearOfCentury; } }

        /// <summary>Gets the year of this offset date and time within the era.</summary>
        public int YearOfEra { get { return localDateTime.YearOfEra; } }

        /// <summary>Gets the era of this offset date and time.</summary>
        public Era Era { get { return localDateTime.Era; } }

        /// <summary>Gets the day of this offset date and time within the year.</summary>
        public int DayOfYear { get { return localDateTime.DayOfYear; } }

        /// <summary>
        /// Gets the hour of day of this offset date and time, in the range 0 to 23 inclusive.
        /// </summary>
        public int Hour { get { return localDateTime.Hour;  } }

        /// <summary>
        /// Gets the hour of the half-day of this date and time, in the range 1 to 12 inclusive.
        /// </summary>
        public int ClockHourOfHalfDay { get { return localDateTime.ClockHourOfHalfDay; } }

        /// <summary>
        /// Gets the minute of this offset date and time, in the range 0 to 59 inclusive.
        /// </summary>
        public int Minute { get { return localDateTime.Minute; } }

        /// <summary>
        /// Gets the second of this offset date and time within the minute, in the range 0 to 59 inclusive.
        /// </summary>
        public int Second { get { return localDateTime.Second; } }

        /// <summary>
        /// Gets the second of this offset date and time within the day, in the range 0 to 86,399 inclusive.
        /// </summary>
        public int SecondOfDay { get { return localDateTime.SecondOfDay; } }

        /// <summary>
        /// Gets the millisecond of this offset date and time within the second, in the range 0 to 999 inclusive.
        /// </summary>
        public int Millisecond { get { return localDateTime.Millisecond; } }

        /// <summary>
        /// Gets the millisecond of this offset date and time within the day, in the range 0 to 86,399,999 inclusive.
        /// </summary>
        public int MillisecondOfDay { get { return localDateTime.MillisecondOfDay; } }

        /// <summary>
        /// Gets the tick of this offset date and time within the millisecond, in the range 0 to 9,999 inclusive.
        /// </summary>
        public int Tick { get { return localDateTime.Tick; } }

        /// <summary>
        /// Gets the tick of this offset date and time within the second, in the range 0 to 9,999,999 inclusive.
        /// </summary>
        public int TickOfSecond { get { return localDateTime.TickOfSecond; } }

        /// <summary>
        /// Gets the tick of this offset date and time within the day, in the range 0 to 863,999,999,999 inclusive.
        /// </summary>
        public long TickOfDay { get { return localDateTime.TickOfDay; } }

        /// <summary>
        /// Returns the local date and time represented within this offset date and time.
        /// </summary>
        public LocalDateTime LocalDateTime { get { return localDateTime; } }

        /// <summary>
        /// Returns the offset from UTC.
        /// </summary>
        public object Offset { get { return offset; } }

        /// <summary>
        /// Converts this offset date and time to an instant in time by subtracting the offset from the local date and time.
        /// </summary>
        /// <returns>The instant represented by this offset date and time</returns>
        public Instant ToInstant()
        {
            return localDateTime.LocalInstant.Minus(offset);
        }

        /// <summary>
        /// Creates a <see cref="ZonedDateTime"/> in a time zone with a fixed offset (which is the same as the offset
        /// for this value), and with the same local date and time.
        /// </summary>
        /// <returns>A zoned date/time with the same local time and a fixed time zone using the offset from this value.</returns>
        public ZonedDateTime InFixedZone()
        {
            return new ZonedDateTime(localDateTime, offset, DateTimeZone.ForOffset(offset));
        }

        /// <summary>
        /// Returns the BCL <see cref="DateTimeOffset"/> corresponding to this offset date and time.
        /// </summary>
        /// <returns>A DateTimeOffset with the same local date/time and offset as this. The <see cref="DateTime"/> part of
        /// the result always has a "kind" of Unspecified.</returns>
        public DateTimeOffset ToDateTimeOffset()
        {
            return new DateTimeOffset(localDateTime.ToDateTimeUnspecified(), offset.ToTimeSpan());
        }

        /// <summary>
        /// Builds an <see cref="OffsetDateTime"/> from a BCL <see cref="DateTimeOffset"/> by converting
        /// the <see cref="DateTime"/> part to a <see cref="LocalDateTime"/>, and the offset part to an <see cref="Offset"/>.
        /// </summary>
        /// <param name="dateTimeOffset">DateTimeOffset to convert</param>
        /// <returns>The converted offset date and time</returns>
        public static OffsetDateTime FromDateTimeOffset(DateTimeOffset dateTimeOffset)
        {
            return new OffsetDateTime(LocalDateTime.FromDateTime(dateTimeOffset.DateTime),
                NodaTime.Offset.FromTimeSpan(dateTimeOffset.Offset));
        }

        /// <summary>
        /// Returns a hash code for this local date.
        /// </summary>
        /// <returns>A hash code for this local date.</returns>
        public override int GetHashCode()
        {
            int hash = HashCodeHelper.Initialize();
            hash = HashCodeHelper.Hash(hash, localDateTime);
            hash = HashCodeHelper.Hash(hash, offset);
            return hash;
        }

        /// <summary>
        /// Compares two <see cref="OffsetDateTime"/> values for equality. This requires
        /// that the local date/time values be the same (in the same calendar) and the offsets.
        /// </summary>
        /// <param name="obj">The object to compare this date with.</param>
        /// <returns>True if the given value is another offset date/time equal to this one; false otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is OffsetDateTime))
            {
                return false;
            }
            return this == (OffsetDateTime)obj;
        }

        /// <summary>
        /// Compares two <see cref="OffsetDateTime"/> values for equality. This requires
        /// that the local date/time values be the same (in the same calendar) and the offsets.
        /// </summary>
        /// <param name="other">The value to compare this offset date/time with.</param>
        /// <returns>True if the given value is another offset date/time equal to this one; false otherwise.</returns>
        public bool Equals(OffsetDateTime other)
        {
            return this.localDateTime == other.localDateTime && this.offset == other.offset;
        }

        #region Operators
        /// <summary>
        /// Implements the operator == (equality).
        /// </summary>
        /// <param name="left">The left hand side of the operator.</param>
        /// <param name="right">The right hand side of the operator.</param>
        /// <returns><c>true</c> if values are equal to each other, otherwise <c>false</c>.</returns>
        public static bool operator ==(OffsetDateTime left, OffsetDateTime right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Implements the operator != (inequality).
        /// </summary>
        /// <param name="left">The left hand side of the operator.</param>
        /// <param name="right">The right hand side of the operator.</param>
        /// <returns><c>true</c> if values are not equal to each other, otherwise <c>false</c>.</returns>
        public static bool operator !=(OffsetDateTime left, OffsetDateTime right)
        {
            return !(left == right);
        }
        #endregion
    }
}
