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

namespace NodaTime
{
    /// <summary>
    /// LocalDate is an immutable struct representing a date within the calendar,
    /// with no reference to a particular time zone or time of day.
    /// </summary>
    public struct LocalDate
    {
        private readonly LocalDateTime localTime;

        public LocalDate(int year, int month, int day) : this(year, month, day, CalendarSystem.Iso)
        {
        }

        public LocalDate(int year, int month, int day, CalendarSystem calendar) : this(new LocalDateTime(year, month, day, 0, 0, calendar))
        {
        }

        private LocalDate(LocalDateTime localTime)
        {
            this.localTime = localTime;
        }

        public CalendarSystem Calendar { get { return localTime.Calendar; } }
        public int Year { get { return localTime.Year; } }
        public int MonthOfYear { get { return localTime.MonthOfYear; } }
        public int DayOfMonth { get { return localTime.DayOfMonth; } }
        public IsoDayOfWeek IsoDayOfWeek { get { return localTime.IsoDayOfWeek; } }
        public int DayOfWeek { get { return localTime.DayOfWeek; } }
        public int WeekYear { get { return localTime.WeekYear; } }
        public int WeekOfWeekYear { get { return localTime.WeekOfWeekYear; } }
        public int YearOfCentury { get { return localTime.YearOfCentury; } }
        public int YearOfEra { get { return localTime.YearOfEra; } }
        public int DayOfYear { get { return localTime.DayOfYear; } }
        public LocalDateTime LocalDateTime { get { return localTime; } }

        public LocalDate WithYear(int year)
        {
            return new LocalDate(LocalDateTime.WithYear(year));
        }

        public LocalDate WithMonthOfYear(int month)
        {
            return new LocalDate(LocalDateTime.WithMonthOfYear(month));
        }

        public LocalDate WithDayOfMonth(int day)
        {
            return new LocalDate(LocalDateTime.WithDayOfMonth(day));
        }

        /// <summary>
        /// TODO: Assert no units smaller than a day
        /// </summary>
        public static LocalDate operator +(LocalDate date, Period period)
        {
            if (period == null)
            {
                throw new ArgumentNullException("period");
            }
            return new LocalDate(date.LocalDateTime + period);
        }

        public static LocalDateTime operator +(LocalDate date, LocalTime time)
        {
            LocalInstant localDateInstant = date.localTime.LocalInstant;
            LocalInstant localInstant = new LocalInstant(localDateInstant.Ticks + time.TickOfDay);
            return new LocalDateTime(localInstant, date.localTime.Calendar);
        }

        /// <summary>
        /// TODO: Assert no units smaller than a day
        /// </summary>
        public static LocalDate operator -(LocalDate date, Period period)
        {
            if (period == null)
            {
                throw new ArgumentNullException("period");
            }
            return new LocalDate(date.LocalDateTime - period);
        }

        public static bool operator ==(LocalDate lhs, LocalDate rhs)
        {
            return lhs.localTime == rhs.localTime;
        }

        public static bool operator !=(LocalDate lhs, LocalDate rhs)
        {
            return lhs.localTime != rhs.localTime;
        }

        // TODO: Implement IEquatable etc

        public override string ToString()
        {
            // TODO: Implement as part of general formatting work
            return string.Format("{0:00}-{1:00}-{2:00}", Year, MonthOfYear, DayOfMonth);
        }

        public override int GetHashCode()
        {
            return localTime.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is LocalDate))
            {
                return false;
            }
            return this == (LocalDate)obj;
        }
    }
}