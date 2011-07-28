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
using System.Collections;
using System.Collections.Generic;
using NodaTime.Fields;
using NodaTime.Utility;

namespace NodaTime
{
    /// <summary>
    /// Represents a period of time expressed in human chronological terms: hours, days,
    /// weeks, months and so on. All implementations in Noda Time are immutable, and return fields
    /// in descending size order: hours before minutes, for example.
    /// </summary>
    /// <remarks>
    /// Periods operate on calendar-related types such as
    /// <see cref="LocalDateTime" /> whereas <see cref="Duration"/> operates on instants
    /// on the time line. <see cref="ZonedDateTime" /> includes both concepts, so both
    /// durations and periods can be added to zoned date-time instances - although the
    /// results may not be the same. For example, adding a two hour period to a ZonedDateTime
    /// will always give a result has a local date-time which is two hours later, even if that
    /// means that three hours would have to actually pass in experienced time to arrive at
    /// that local date-time, due to changes in the UTC offset (e.g. for daylight savings).
    /// </remarks>
    public sealed class Period : IEnumerable<DurationFieldValue>, IEquatable<Period>
    {
        private readonly PeriodType periodType;
        private readonly long[] values;

        /// <summary>
        /// Creates a new period with the given array without copying it. The array contents must
        /// not be changed after the value has been constructed - which is why this method is private.
        /// </summary>
        /// <param name="periodType">Type of this period, describing which fields are present</param>
        /// <param name="values">Values for each field in the period type</param>
        private Period(PeriodType periodType, long[] values)
        {
            this.values = values;
            this.periodType = periodType;
        }

        public PeriodType PeriodType { get { return periodType; } }

        private static Period CreateSingleFieldPeriod(PeriodType periodType, long value)
        {
            long[] values = { value };
            return new Period(periodType, values);
        }

        public static Period FromYears(long years)
        {
            return CreateSingleFieldPeriod(PeriodType.Years, years);
        }

        public static Period FromMonths(long months)
        {
            return CreateSingleFieldPeriod(PeriodType.Months, months);
        }

        public static Period FromDays(long days)
        {
            return CreateSingleFieldPeriod(PeriodType.Days, days);
        }

        public static Period FromHours(long hours)
        {
            return CreateSingleFieldPeriod(PeriodType.Hours, hours);
        }

        public static Period FromMinutes(long minutes)
        {
            return CreateSingleFieldPeriod(PeriodType.Minutes, minutes);
        }

        public static Period FromSeconds(long seconds)
        {
            return CreateSingleFieldPeriod(PeriodType.Seconds, seconds);
        }

        public static Period FromMillseconds(long milliseconds)
        {
            return CreateSingleFieldPeriod(PeriodType.Milliseconds, milliseconds);
        }

        public static Period FromTicks(long ticks)
        {
            return CreateSingleFieldPeriod(PeriodType.Ticks, ticks);
        }

        /// <summary>
        /// Adds two periods together, by simply adding the values for each field. Currently this
        /// returns a period with a period type of "all fields".
        /// </summary>
        public static Period operator +(Period left, Period right)
        {
            if (left == null)
            {
                throw new ArgumentNullException("left");
            }
            if (right == null)
            {
                throw new ArgumentNullException("right");
            }
            PeriodType all = PeriodType.AllFields;
            long[] newValues = new long[all.Size];
            // TODO: Make this a lot faster :)
            for (int i = 0; i < all.Size; i++)
            {
                DurationFieldType fieldType = all[i];
                newValues[i] = left[fieldType] + right[fieldType];
            }
            return new Period(PeriodType.AllFields, newValues);
        }

        /// <summary>
        /// Subtracts one periods from another, by simply subtracting each field value. Currently this
        /// returns a period with a period type of "all fields".
        /// </summary>
        public static Period operator -(Period left, Period right)
        {
            if (left == null)
            {
                throw new ArgumentNullException("left");
            }
            if (right == null)
            {
                throw new ArgumentNullException("right");
            }
            PeriodType all = PeriodType.AllFields;
            long[] newValues = new long[all.Size];
            // TODO: Make this a lot faster :)
            for (int i = 0; i < all.Size; i++)
            {
                DurationFieldType fieldType = all[i];
                newValues[i] = left[fieldType] - right[fieldType];
            }
            return new Period(PeriodType.AllFields, newValues);
        }

        /// <summary>
        /// Returns the period between a start and an end date/time, using the set of fields in the given
        /// period type.
        /// </summary>
        /// <remarks>
        /// If <paramref name="end"/> is before <paramref name="start" />, each field in the returned period
        /// will be negative. If the given period type cannot exactly reach the end point (e.g. finding
        /// the difference between 1am and 3:15am in hours) the result will be such that adding it to <paramref name="start"/>
        /// will give a value between <paramref name="start"/> and <paramref name="end"/>. In other words,
        /// any rounding is "towards start"; this is true whether the resulting period is negative or positive.
        /// </remarks>
        /// <param name="start">Start date/time</param>
        /// <param name="end">End date/time</param>
        /// <param name="periodType">Period type to use for calculations</param>
        /// <exception cref="ArgumentException"><paramref name="start"/> and <paramref name="end"/> use different calendars</exception>
        /// <exception cref="ArgumentNullException"><paramref name="periodType"/> is null</exception>
        /// <returns>The period between the given date/times</returns>
        public static Period Between(LocalDateTime start, LocalDateTime end, PeriodType periodType)
        {
            if (periodType == null)
            {
                throw new ArgumentNullException("periodType");
            }
            if (!start.Calendar.Equals(end.Calendar))
            {
                throw new ArgumentException("start and end must use the same calendar system");
            }
            long[] values = start.Calendar.GetPeriodValues(start.LocalInstant, end.LocalInstant, periodType);
            return new Period(periodType, values);
        }

        /// <summary>
        /// Returns the difference between two date/times using the "all fields" period type.
        /// </summary>
        public static Period Between(LocalDateTime start, LocalDateTime end)
        {
            return Between(start, end, PeriodType.AllFields);
        }

        /// <summary>
        /// Returns the period between a start and an end date, using the set of fields in the given
        /// period type.
        /// </summary>
        /// <remarks>
        /// If <paramref name="end"/> is before <paramref name="start" />, each field in the returned period
        /// will be negative. If the given period type cannot exactly reach the end point (e.g. finding
        /// the difference between 12th February and 15th March in months) the result will be such that adding it to <paramref name="start"/>
        /// will give a value between <paramref name="start"/> and <paramref name="end"/>. In other words,
        /// any rounding is "towards start"; this is true whether the resulting period is negative or positive.
        /// </remarks>
        /// <param name="start">Start date/time</param>
        /// <param name="end">End date/time</param>
        /// <param name="periodType">Period type to use for calculations</param>
        /// <exception cref="ArgumentException"><paramref name="periodType"/> contains time fields</exception>
        /// <exception cref="ArgumentException"><paramref name="start"/> and <paramref name="end"/> use different calendars</exception>
        /// <exception cref="ArgumentNullException"><paramref name="periodType"/> is null</exception>
        /// <returns>The period between the given dates</returns>
        public static Period Between(LocalDate start, LocalDate end, PeriodType periodType)
        {
            return Between(start.LocalDateTime, end.LocalDateTime, periodType);
        }

        /// <summary>
        /// Returns the difference between two dates using the "year month day" period type.
        /// </summary>
        public static Period Between(LocalDate start, LocalDate end)
        {
            return Between(start.LocalDateTime, end.LocalDateTime, PeriodType.YearMonthDay);
        }

        /// <summary>
        /// Returns the period between a start and an end time, using the set of fields in the given
        /// period type.
        /// </summary>
        /// <remarks>
        /// If <paramref name="end"/> is before <paramref name="start" />, each field in the returned period
        /// will be negative. If the given period type cannot exactly reach the end point (e.g. finding
        /// the difference between 3am and 4.30am in hours) the result will be such that adding it to <paramref name="start"/>
        /// will give a value between <paramref name="start"/> and <paramref name="end"/>. In other words,
        /// any rounding is "towards start"; this is true whether the resulting period is negative or positive.
        /// </remarks>
        /// <param name="start">Start date/time</param>
        /// <param name="end">End date/time</param>
        /// <param name="periodType">Period type to use for calculations</param>
        /// <exception cref="ArgumentException"><paramref name="periodType"/> contains time fields</exception>
        /// <exception cref="ArgumentException"><paramref name="start"/> and <paramref name="end"/> use different calendars</exception>
        /// <exception cref="ArgumentNullException"><paramref name="periodType"/> is null</exception>
        /// <returns>The period between the given times</returns>
        public static Period Between(LocalTime start, LocalTime end, PeriodType periodType)
        {
            return Between(start.LocalDateTime, end.LocalDateTime, periodType);
        }

        /// <summary>
        /// Returns the difference between two dates using the "time" period type.
        /// </summary>
        public static Period Between(LocalTime start, LocalTime end)
        {
            return Between(start.LocalDateTime, end.LocalDateTime, PeriodType.Time);
        }

        /// <summary>
        /// Returns the fields and values within this period.
        /// </summary>
        public IEnumerator<DurationFieldValue> GetEnumerator()
        {
            for (int i = 0; i < values.Length; i++)
            {
                yield return new DurationFieldValue(periodType[i], values[i]);
            }
        }

        /// <summary>
        /// Returns the fields and values within this period.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns the value of the given field within this period. If the period does not contain
        /// the given field, 0 is returned.
        /// </summary>
        public long this[DurationFieldType fieldType]
        {
            get
            {
                int index = periodType.IndexOf(fieldType);
                return index == -1 ? 0 : values[index];
            }
        }

        #region Helper properties
        public long Years { get { return this[DurationFieldType.Years]; } }
        public long Months { get { return this[DurationFieldType.Months]; } }
        public long Weeks { get { return this[DurationFieldType.Weeks]; } }
        public long Days { get { return this[DurationFieldType.Days]; } }
        public long Hours { get { return this[DurationFieldType.Hours]; } }
        public long Minutes { get { return this[DurationFieldType.Minutes]; } }
        public long Seconds { get { return this[DurationFieldType.Seconds]; } }
        public long Millseconds { get { return this[DurationFieldType.Milliseconds]; } }
        public long Ticks { get { return this[DurationFieldType.Ticks]; } }
        #endregion

        #region Object overrides
        /// <summary>
        /// Compares the given object for equality with this one, as per <see cref="Equals(Period)"/>.
        /// </summary>
        public override bool Equals(object other)
        {
            return Equals(other as Period);
        }

        /// <summary>
        /// Returns the hash code for this period, consistent with <see cref="Equals(Period)"/>.
        /// </summary>
        public override int GetHashCode()
        {
            int hash = HashCodeHelper.Initialize();
            // TODO: Make this a lot faster :)
            foreach (DurationFieldType fieldType in PeriodType.AllFields)
            {
                hash = HashCodeHelper.Hash(hash, this[fieldType]);
            }
            return hash;
        }

        /// <summary>
        /// Compares the given period for equality with this one.
        /// </summary>
        /// <remarks>
        /// Periods are equal if they contain the same values for the same fields, regardless of period type
        /// - so a period of "one hour" is the same whether or not it's potentially got other fields with
        /// a zero value. However, no normalization takes place, so "one hour" is not equal to "sixty minutes".
        /// </remarks>
        public bool Equals(Period other)
        {
            if (other == null)
            {
                return false;
            }

            // TODO: Make this a lot faster :)
            foreach (DurationFieldType fieldType in PeriodType.AllFields)
            {
                if (this[fieldType] != other[fieldType])
                {
                    return false;
                }
            }
            return true;
        }
        #endregion
    }
}
