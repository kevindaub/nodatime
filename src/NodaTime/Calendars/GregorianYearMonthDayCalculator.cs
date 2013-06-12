﻿// Copyright 2013 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace NodaTime.Calendars
{
    internal class GregorianYearMonthDayCalculator : GJYearMonthDayCalculator
    {
        // We precompute useful values for each month between these years, as we anticipate most
        // dates will be in this range.
        private const int FirstOptimizedYear = 1900;
        private const int LastOptimizedYear = 2100;
        private static readonly long[] MonthStartTicks = new long[(LastOptimizedYear + 1 - FirstOptimizedYear) * 12 + 1];
        private static readonly int[] MonthLengths = new int[(LastOptimizedYear + 1 - FirstOptimizedYear) * 12 + 1];
        private static readonly long[] YearStartTicks = new long[LastOptimizedYear + 1 - FirstOptimizedYear];

        private const int DaysFrom0000To1970 = 719527;
        private const long AverageTicksPerGregorianYear = (long)(365.2425m * NodaConstants.TicksPerStandardDay);

        internal static readonly GregorianYearMonthDayCalculator Instance = new GregorianYearMonthDayCalculator();

        static GregorianYearMonthDayCalculator()
        {
            for (int year = FirstOptimizedYear; year <= LastOptimizedYear; year++)
            {
                YearStartTicks[year - FirstOptimizedYear] = Instance.CalculateYearTicks(year);
                for (int month = 1; month <= 12; month++)
                {
                    int yearMonthIndex = (year - FirstOptimizedYear) * 12 + month;
                    MonthStartTicks[yearMonthIndex] = Instance.GetYearMonthTicks(year, month);
                    MonthLengths[yearMonthIndex] = Instance.GetDaysInMonth(year, month);
                }
            }
        }

        protected GregorianYearMonthDayCalculator()
            : base(-27255, 31195,  AverageTicksPerGregorianYear, 1970 * AverageTicksPerGregorianYear)
        {
        }

        internal override long GetYearTicks(int year)
        {
            if (year < FirstOptimizedYear || year > LastOptimizedYear)
            {
                return base.GetYearTicks(year);
            }
            return YearStartTicks[year - FirstOptimizedYear];
        }

        internal override LocalInstant GetLocalInstant(int year, int monthOfYear, int dayOfMonth)
        {
            int yearMonthIndex = (year - FirstOptimizedYear) * 12 + monthOfYear;
            if (year < FirstOptimizedYear || year > LastOptimizedYear - 1 || monthOfYear < 1 || monthOfYear > 12 || dayOfMonth < 1 ||
                dayOfMonth > MonthLengths[yearMonthIndex])
            {
                return base.GetLocalInstant(year, monthOfYear, dayOfMonth);
            }
            // This is guaranteed not to overflow, as we've already validated the arguments
            return new LocalInstant(unchecked(MonthStartTicks[yearMonthIndex] + (dayOfMonth - 1) * NodaConstants.TicksPerStandardDay));
        }

        protected override long CalculateYearTicks(int year)
        {
            // Initial value is just temporary.
            int leapYears = year / 100;
            if (year < 0)
            {
                // Add 3 before shifting right since /4 and >>2 behave differently
                // on negative numbers. When the expression is written as
                // (year / 4) - (year / 100) + (year / 400),
                // it works for both positive and negative values, except this optimization
                // eliminates two divisions.
                leapYears = ((year + 3) >> 2) - leapYears + ((leapYears + 3) >> 2) - 1;
            }
            else
            {
                leapYears = (year >> 2) - leapYears + (leapYears >> 2);
                if (IsLeapYear(year))
                {
                    leapYears--;
                }
            }

            return (year * 365L + (leapYears - DaysFrom0000To1970)) * NodaConstants.TicksPerStandardDay;
        }

        internal override bool IsLeapYear(int year)
        {
            return ((year & 3) == 0) && ((year % 100) != 0 || (year % 400) == 0);
        }

        internal override int GetYearOfEra(LocalInstant localInstant)
        {
            int year = GetYear(localInstant);
            return year <= 0 ? -year + 1 : year;
        }

        internal override int GetEra(LocalInstant localInstant)
        {
            // TODO: Just check the ticks against a constant.
            return GetYear(localInstant) <= 0 ? 0 : 1;
        }
    }
}
