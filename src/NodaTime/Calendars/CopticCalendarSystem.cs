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
using System.Globalization;
using NodaTime.Fields;

namespace NodaTime.Calendars
{
    /// <summary>
    /// See <see cref="CalendarSystem.GetCopticCalendar"/> for details.
    /// </summary>
    internal sealed class CopticCalendarSystem : BasicFixedMonthCalendarSystem
    {
        private const string CopticName = "Coptic";
        private static readonly CopticCalendarSystem[] instances;
        private static readonly DateTimeField EraField = new BasicSingleEraDateTimeField(Era.AnnoMartyrm);

        static CopticCalendarSystem()
        {
            instances = new CopticCalendarSystem[7];
            for (int i = 0; i < 7; i++)
            {
                instances[i] = new CopticCalendarSystem(i + 1);
            }
        }

        /// <summary>
        /// Returns the instance of the Coptic calendar system with the given number of days in the week.
        /// </summary>
        /// <param name="minDaysInFirstWeek">The minimum number of days at the start of the year to consider it
        /// a week in that year as opposed to at the end of the previous year.</param>
        internal static CopticCalendarSystem GetInstance(int minDaysInFirstWeek)
        {
            if (minDaysInFirstWeek < 1 || minDaysInFirstWeek > 7)
            {
                throw new ArgumentOutOfRangeException("minDaysInFirstWeek", "Minimum days in first week must be between 1 and 7 inclusive");
            }
            return instances[minDaysInFirstWeek - 1];
        }

        private CopticCalendarSystem(int minDaysInFirstWeek)
            : base(CreateIdFromNameAndMinDaysInFirstWeek(CopticName, minDaysInFirstWeek), CopticName, minDaysInFirstWeek, 1, 29227, AssembleCopticFields, new[] { Era.AnnoMartyrm })
        {
        }

        private static void AssembleCopticFields(FieldSet.Builder builder, CalendarSystem @this)
        {
            builder.Era = EraField;
            builder.MonthOfYear = new BasicMonthOfYearDateTimeField((BasicCalendarSystem) @this, 13);
            builder.Months = builder.MonthOfYear.PeriodField;
        }

        protected override LocalInstant CalculateStartOfYear(int year)
        {
            // Unix epoch is 1970-01-01 Gregorian which is 1686-04-23 Coptic.
            // Calculate relative to the nearest leap year and account for the
            // difference later.

            int relativeYear = year - 1687;
            int leapYears;
            if (relativeYear <= 0)
            {
                // Add 3 before shifting right since /4 and >>2 behave differently
                // on negative numbers.
                leapYears = (relativeYear + 3) >> 2;
            }
            else
            {
                leapYears = relativeYear >> 2;
                // For post 1687 an adjustment is needed as jan1st is before leap day
                if (!IsLeapYear(year))
                {
                    leapYears++;
                }
            }

            long ticks = (relativeYear * 365L + leapYears) * NodaConstants.TicksPerStandardDay;

            // Adjust to account for difference between 1687-01-01 and 1686-04-23.
            return new LocalInstant(ticks + (365L - 112) * NodaConstants.TicksPerStandardDay);
        }

        internal override long ApproxTicksAtEpochDividedByTwo
        {
            get { return (1686L * AverageTicksPerYear + 112L * NodaConstants.TicksPerStandardDay) / 2;  }
        }
    }
}
