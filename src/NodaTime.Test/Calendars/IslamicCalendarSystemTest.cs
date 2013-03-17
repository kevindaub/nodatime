// Copyright 2011 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using NodaTime.Calendars;

namespace NodaTime.Test.Calendars
{
    [TestFixture]
    public class IslamicCalendarSystemTest
    {
        private static readonly CalendarSystem SampleCalendar = CalendarSystem.GetIslamicCalendar(IslamicLeapYearPattern.Base16, IslamicEpoch.Civil);

        [Test]
        public void SampleDate1()
        {
            // Note: field checks removed from the tests.
            LocalDateTime ldt = new LocalDateTime(1945, 11, 12, 0, 0, 0, 0, CalendarSystem.Iso);

            ldt = ldt.WithCalendar(SampleCalendar);
            Assert.AreEqual(Era.AnnoHegirae, ldt.Era);
            Assert.AreEqual(14, ldt.CenturyOfEra);
            Assert.AreEqual(64, ldt.YearOfCentury);
            Assert.AreEqual(1364, ldt.YearOfEra);

            Assert.AreEqual(1364, ldt.Year);
            Assert.AreEqual(12, ldt.Month);
            Assert.AreEqual(6, ldt.Day);
            Assert.AreEqual(IsoDayOfWeek.Monday, ldt.IsoDayOfWeek);
            Assert.AreEqual(6 * 30 + 5 * 29 + 6, ldt.DayOfYear);

            Assert.AreEqual(0, ldt.Hour);
            Assert.AreEqual(0, ldt.Minute);
            Assert.AreEqual(0, ldt.Second);
            Assert.AreEqual(0, ldt.TickOfSecond);
        }

        [Test]
        public void SampleDate2()
        {
            LocalDateTime ldt = new LocalDateTime(2005, 11, 26, 0, 0, 0, 0, CalendarSystem.Iso);
            ldt = ldt.WithCalendar(SampleCalendar);
            Assert.AreEqual(Era.AnnoHegirae, ldt.Era);
            Assert.AreEqual(15, ldt.CenturyOfEra);  // TODO confirm
            Assert.AreEqual(26, ldt.YearOfCentury);
            Assert.AreEqual(1426, ldt.YearOfEra);

            Assert.AreEqual(1426, ldt.Year);
            Assert.AreEqual(10, ldt.Month);
            Assert.AreEqual(24, ldt.Day);
            Assert.AreEqual(IsoDayOfWeek.Saturday, ldt.IsoDayOfWeek);
            Assert.AreEqual(5 * 30 + 4 * 29 + 24, ldt.DayOfYear);
            Assert.AreEqual(0, ldt.Hour);
            Assert.AreEqual(0, ldt.Minute);
            Assert.AreEqual(0, ldt.Second);
            Assert.AreEqual(0, ldt.TickOfSecond);
        }

        [Test]
        public void SampleDate3()
        {
            LocalDateTime ldt = new LocalDateTime(1426, 12, 24, 0, 0, 0, 0, SampleCalendar);
            Assert.AreEqual(Era.AnnoHegirae, ldt.Era);

            Assert.AreEqual(1426, ldt.Year);
            Assert.AreEqual(12, ldt.Month);
            Assert.AreEqual(24, ldt.Day);
            Assert.AreEqual(IsoDayOfWeek.Tuesday, ldt.IsoDayOfWeek);
            Assert.AreEqual(6 * 30 + 5 * 29 + 24, ldt.DayOfYear);
            Assert.AreEqual(0, ldt.Hour);
            Assert.AreEqual(0, ldt.Minute);
            Assert.AreEqual(0, ldt.Second);
            Assert.AreEqual(0, ldt.TickOfSecond);
        }

        [Test]
        public void Base15LeapYear()
        {
            CalendarSystem calendar = CalendarSystem.GetIslamicCalendar(IslamicLeapYearPattern.Base15, IslamicEpoch.Civil);

            Assert.AreEqual(false, calendar.IsLeapYear(1));
            Assert.AreEqual(true, calendar.IsLeapYear(2));
            Assert.AreEqual(false, calendar.IsLeapYear(3));
            Assert.AreEqual(false, calendar.IsLeapYear(4));
            Assert.AreEqual(true, calendar.IsLeapYear(5));
            Assert.AreEqual(false, calendar.IsLeapYear(6));
            Assert.AreEqual(true, calendar.IsLeapYear(7));
            Assert.AreEqual(false, calendar.IsLeapYear(8));
            Assert.AreEqual(false, calendar.IsLeapYear(9));
            Assert.AreEqual(true, calendar.IsLeapYear(10));
            Assert.AreEqual(false, calendar.IsLeapYear(11));
            Assert.AreEqual(false, calendar.IsLeapYear(12));
            Assert.AreEqual(true, calendar.IsLeapYear(13));
            Assert.AreEqual(false, calendar.IsLeapYear(14));
            Assert.AreEqual(true, calendar.IsLeapYear(15));
            Assert.AreEqual(false, calendar.IsLeapYear(16));
            Assert.AreEqual(false, calendar.IsLeapYear(17));
            Assert.AreEqual(true, calendar.IsLeapYear(18));
            Assert.AreEqual(false, calendar.IsLeapYear(19));
            Assert.AreEqual(false, calendar.IsLeapYear(20));
            Assert.AreEqual(true, calendar.IsLeapYear(21));
            Assert.AreEqual(false, calendar.IsLeapYear(22));
            Assert.AreEqual(false, calendar.IsLeapYear(23));
            Assert.AreEqual(true, calendar.IsLeapYear(24));
            Assert.AreEqual(false, calendar.IsLeapYear(25));
            Assert.AreEqual(true, calendar.IsLeapYear(26));
            Assert.AreEqual(false, calendar.IsLeapYear(27));
            Assert.AreEqual(false, calendar.IsLeapYear(28));
            Assert.AreEqual(true, calendar.IsLeapYear(29));
            Assert.AreEqual(false, calendar.IsLeapYear(30));
        }

        [Test]
        public void Base16LeapYear()
        {
            CalendarSystem calendar = CalendarSystem.GetIslamicCalendar(IslamicLeapYearPattern.Base16, IslamicEpoch.Civil);

            Assert.AreEqual(false, calendar.IsLeapYear(1));
            Assert.AreEqual(true, calendar.IsLeapYear(2));
            Assert.AreEqual(false, calendar.IsLeapYear(3));
            Assert.AreEqual(false, calendar.IsLeapYear(4));
            Assert.AreEqual(true, calendar.IsLeapYear(5));
            Assert.AreEqual(false, calendar.IsLeapYear(6));
            Assert.AreEqual(true, calendar.IsLeapYear(7));
            Assert.AreEqual(false, calendar.IsLeapYear(8));
            Assert.AreEqual(false, calendar.IsLeapYear(9));
            Assert.AreEqual(true, calendar.IsLeapYear(10));
            Assert.AreEqual(false, calendar.IsLeapYear(11));
            Assert.AreEqual(false, calendar.IsLeapYear(12));
            Assert.AreEqual(true, calendar.IsLeapYear(13));
            Assert.AreEqual(false, calendar.IsLeapYear(14));
            Assert.AreEqual(false, calendar.IsLeapYear(15));
            Assert.AreEqual(true, calendar.IsLeapYear(16));
            Assert.AreEqual(false, calendar.IsLeapYear(17));
            Assert.AreEqual(true, calendar.IsLeapYear(18));
            Assert.AreEqual(false, calendar.IsLeapYear(19));
            Assert.AreEqual(false, calendar.IsLeapYear(20));
            Assert.AreEqual(true, calendar.IsLeapYear(21));
            Assert.AreEqual(false, calendar.IsLeapYear(22));
            Assert.AreEqual(false, calendar.IsLeapYear(23));
            Assert.AreEqual(true, calendar.IsLeapYear(24));
            Assert.AreEqual(false, calendar.IsLeapYear(25));
            Assert.AreEqual(true, calendar.IsLeapYear(26));
            Assert.AreEqual(false, calendar.IsLeapYear(27));
            Assert.AreEqual(false, calendar.IsLeapYear(28));
            Assert.AreEqual(true, calendar.IsLeapYear(29));
            Assert.AreEqual(false, calendar.IsLeapYear(30));
        }

        [Test]
        public void IndianBasedLeapYear()
        {
            CalendarSystem calendar = CalendarSystem.GetIslamicCalendar(IslamicLeapYearPattern.Indian, IslamicEpoch.Civil);

            Assert.AreEqual(false, calendar.IsLeapYear(1));
            Assert.AreEqual(true, calendar.IsLeapYear(2));
            Assert.AreEqual(false, calendar.IsLeapYear(3));
            Assert.AreEqual(false, calendar.IsLeapYear(4));
            Assert.AreEqual(true, calendar.IsLeapYear(5));
            Assert.AreEqual(false, calendar.IsLeapYear(6));
            Assert.AreEqual(false, calendar.IsLeapYear(7));
            Assert.AreEqual(true, calendar.IsLeapYear(8));
            Assert.AreEqual(false, calendar.IsLeapYear(9));
            Assert.AreEqual(true, calendar.IsLeapYear(10));
            Assert.AreEqual(false, calendar.IsLeapYear(11));
            Assert.AreEqual(false, calendar.IsLeapYear(12));
            Assert.AreEqual(true, calendar.IsLeapYear(13));
            Assert.AreEqual(false, calendar.IsLeapYear(14));
            Assert.AreEqual(false, calendar.IsLeapYear(15));
            Assert.AreEqual(true, calendar.IsLeapYear(16));
            Assert.AreEqual(false, calendar.IsLeapYear(17));
            Assert.AreEqual(false, calendar.IsLeapYear(18));
            Assert.AreEqual(true, calendar.IsLeapYear(19));
            Assert.AreEqual(false, calendar.IsLeapYear(20));
            Assert.AreEqual(true, calendar.IsLeapYear(21));
            Assert.AreEqual(false, calendar.IsLeapYear(22));
            Assert.AreEqual(false, calendar.IsLeapYear(23));
            Assert.AreEqual(true, calendar.IsLeapYear(24));
            Assert.AreEqual(false, calendar.IsLeapYear(25));
            Assert.AreEqual(false, calendar.IsLeapYear(26));
            Assert.AreEqual(true, calendar.IsLeapYear(27));
            Assert.AreEqual(false, calendar.IsLeapYear(28));
            Assert.AreEqual(true, calendar.IsLeapYear(29));
            Assert.AreEqual(false, calendar.IsLeapYear(30));
        }

        [Test]
        public void HabashAlHasibBasedLeapYear()
        {
            CalendarSystem calendar = CalendarSystem.GetIslamicCalendar(IslamicLeapYearPattern.HabashAlHasib, IslamicEpoch.Civil);

            Assert.AreEqual(false, calendar.IsLeapYear(1));
            Assert.AreEqual(true, calendar.IsLeapYear(2));
            Assert.AreEqual(false, calendar.IsLeapYear(3));
            Assert.AreEqual(false, calendar.IsLeapYear(4));
            Assert.AreEqual(true, calendar.IsLeapYear(5));
            Assert.AreEqual(false, calendar.IsLeapYear(6));
            Assert.AreEqual(false, calendar.IsLeapYear(7));
            Assert.AreEqual(true, calendar.IsLeapYear(8));
            Assert.AreEqual(false, calendar.IsLeapYear(9));
            Assert.AreEqual(false, calendar.IsLeapYear(10));
            Assert.AreEqual(true, calendar.IsLeapYear(11));
            Assert.AreEqual(false, calendar.IsLeapYear(12));
            Assert.AreEqual(true, calendar.IsLeapYear(13));
            Assert.AreEqual(false, calendar.IsLeapYear(14));
            Assert.AreEqual(false, calendar.IsLeapYear(15));
            Assert.AreEqual(true, calendar.IsLeapYear(16));
            Assert.AreEqual(false, calendar.IsLeapYear(17));
            Assert.AreEqual(false, calendar.IsLeapYear(18));
            Assert.AreEqual(true, calendar.IsLeapYear(19));
            Assert.AreEqual(false, calendar.IsLeapYear(20));
            Assert.AreEqual(true, calendar.IsLeapYear(21));
            Assert.AreEqual(false, calendar.IsLeapYear(22));
            Assert.AreEqual(false, calendar.IsLeapYear(23));
            Assert.AreEqual(true, calendar.IsLeapYear(24));
            Assert.AreEqual(false, calendar.IsLeapYear(25));
            Assert.AreEqual(false, calendar.IsLeapYear(26));
            Assert.AreEqual(true, calendar.IsLeapYear(27));
            Assert.AreEqual(false, calendar.IsLeapYear(28));
            Assert.AreEqual(false, calendar.IsLeapYear(29));
            Assert.AreEqual(true, calendar.IsLeapYear(30));
        }

        [Test]
        public void ThursdayEpoch()
        {
            CalendarSystem thursdayEpochCalendar = CalendarSystem.GetIslamicCalendar(IslamicLeapYearPattern.Base16, IslamicEpoch.Astronomical);
            CalendarSystem julianCalendar = CalendarSystem.GetJulianCalendar(4);

            LocalDate thursdayEpoch = new LocalDate(1, 1, 1, thursdayEpochCalendar);
            LocalDate thursdayEpochJulian = new LocalDate(622, 7, 15, julianCalendar);
            Assert.AreEqual(thursdayEpochJulian, thursdayEpoch.WithCalendar(julianCalendar));
        }

        [Test]
        public void FridayEpoch()
        {
            CalendarSystem fridayEpochCalendar = CalendarSystem.GetIslamicCalendar(IslamicLeapYearPattern.Base16, IslamicEpoch.Civil);
            CalendarSystem julianCalendar = CalendarSystem.GetJulianCalendar(4);

            LocalDate fridayEpoch = new LocalDate(1, 1, 1, fridayEpochCalendar);
            LocalDate fridayEpochJulian = new LocalDate(622, 7, 16, julianCalendar);
            Assert.AreEqual(fridayEpochJulian, fridayEpoch.WithCalendar(julianCalendar));
        }

        [Test]
        public void BclUsesAstronomicalEpoch()
        {
            Calendar hijri = new HijriCalendar();
            DateTime bclDirect = new DateTime(1, 1, 1, 0, 0, 0, 0, hijri, DateTimeKind.Unspecified);

            CalendarSystem julianCalendar = CalendarSystem.GetJulianCalendar(4);
            LocalDate julianIslamicEpoch = new LocalDate(622, 7, 15, julianCalendar);
            LocalDate isoIslamicEpoch = julianIslamicEpoch.WithCalendar(CalendarSystem.Iso);
            DateTime bclFromNoda = isoIslamicEpoch.AtMidnight().ToDateTimeUnspecified();
            Assert.AreEqual(bclDirect, bclFromNoda);
        }

        [Test]
        public void SampleDateBclCompatibility()
        {
            Calendar hijri = new HijriCalendar();
            DateTime bclDirect = new DateTime(1302, 10, 15, 0, 0, 0, 0, hijri, DateTimeKind.Unspecified);

            CalendarSystem islamicCalendar = CalendarSystem.GetIslamicCalendar(IslamicLeapYearPattern.Base16, IslamicEpoch.Astronomical);
            LocalDate iso = new LocalDate(1302, 10, 15, islamicCalendar);
            DateTime bclFromNoda = iso.AtMidnight().ToDateTimeUnspecified();
            Assert.AreEqual(bclDirect, bclFromNoda);
        }

        /// <summary>
        /// This tests every day for 9000 (ISO) years, to check that it always matches the year, month and day.
        /// </summary>
        [Test, Ignore("Takes a long time")]
        public void BclThroughHistory()
        {
            Calendar hijri = new HijriCalendar();
            DateTime bclDirect = new DateTime(1, 1, 1, 0, 0, 0, 0, hijri, DateTimeKind.Unspecified);

            CalendarSystem islamicCalendar = CalendarSystem.GetIslamicCalendar(IslamicLeapYearPattern.Base16, IslamicEpoch.Astronomical);
            CalendarSystem julianCalendar = CalendarSystem.GetJulianCalendar(4);
            LocalDate julianIslamicEpoch = new LocalDate(622, 7, 15, julianCalendar);
            LocalDate islamicDate = julianIslamicEpoch.WithCalendar(islamicCalendar);

            for (int i = 0; i < 9000 * 365; i++)
            {
                Assert.AreEqual(bclDirect, islamicDate.AtMidnight().ToDateTimeUnspecified());
                Assert.AreEqual(hijri.GetYear(bclDirect), islamicDate.Year, i.ToString());
                Assert.AreEqual(hijri.GetMonth(bclDirect), islamicDate.Month);
                Assert.AreEqual(hijri.GetDayOfMonth(bclDirect), islamicDate.Day);
                bclDirect = hijri.AddDays(bclDirect, 1);
                islamicDate = islamicDate.PlusDays(1);
            }
        }

        [Test]
        public void GetDaysInMonth()
        {
            // Just check that we've got the long/short the right way round...
            CalendarSystem calendar = CalendarSystem.GetIslamicCalendar(IslamicLeapYearPattern.HabashAlHasib, IslamicEpoch.Civil);
            Assert.AreEqual(30, calendar.GetDaysInMonth(7, 1));
            Assert.AreEqual(29, calendar.GetDaysInMonth(7, 2));
            Assert.AreEqual(30, calendar.GetDaysInMonth(7, 3));
            Assert.AreEqual(29, calendar.GetDaysInMonth(7, 4));
            Assert.AreEqual(30, calendar.GetDaysInMonth(7, 5));
            Assert.AreEqual(29, calendar.GetDaysInMonth(7, 6));
            Assert.AreEqual(30, calendar.GetDaysInMonth(7, 7));
            Assert.AreEqual(29, calendar.GetDaysInMonth(7, 8));
            Assert.AreEqual(30, calendar.GetDaysInMonth(7, 9));
            Assert.AreEqual(29, calendar.GetDaysInMonth(7, 10));
            Assert.AreEqual(30, calendar.GetDaysInMonth(7, 11));
            // As noted before, 7 isn't a leap year in this calendar
            Assert.AreEqual(29, calendar.GetDaysInMonth(7, 12));
            // As noted before, 8 is a leap year in this calendar
            Assert.AreEqual(30, calendar.GetDaysInMonth(8, 12));
        }

        [Test]
        public void GetInstance_Caching()
        {
            var queue = new Queue<CalendarSystem>();
            var set = new HashSet<CalendarSystem>();
            var ids = new HashSet<string>();

            foreach (IslamicLeapYearPattern leapYearPattern in Enum.GetValues(typeof(IslamicLeapYearPattern)))
            {
                foreach (IslamicEpoch epoch in Enum.GetValues(typeof(IslamicEpoch)))
                {
                    var calendar = CalendarSystem.GetIslamicCalendar(leapYearPattern, epoch);
                    queue.Enqueue(calendar);
                    Assert.IsTrue(set.Add(calendar)); // Check we haven't already seen it...
                    Assert.IsTrue(ids.Add(calendar.Id));
                }
            }

            // Now check we get the same references again...
            foreach (IslamicLeapYearPattern leapYearPattern in Enum.GetValues(typeof(IslamicLeapYearPattern)))
            {
                foreach (IslamicEpoch epoch in Enum.GetValues(typeof(IslamicEpoch)))
                {
                    var oldCalendar = queue.Dequeue();
                    var newCalendar = CalendarSystem.GetIslamicCalendar(leapYearPattern, epoch);
                    Assert.AreSame(oldCalendar, newCalendar);
                }
            }
        }

        [Test]
        public void GetInstance_ArgumentValidation()
        {
            var epochs = Enum.GetValues(typeof(IslamicEpoch)).Cast<IslamicEpoch>();
            var leapYearPatterns = Enum.GetValues(typeof(IslamicLeapYearPattern)).Cast<IslamicLeapYearPattern>();
            Assert.Throws<ArgumentOutOfRangeException>(() => CalendarSystem.GetIslamicCalendar(leapYearPatterns.Min() - 1, epochs.Min()));
            Assert.Throws<ArgumentOutOfRangeException>(() => CalendarSystem.GetIslamicCalendar(leapYearPatterns.Min(), epochs.Min() - 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => CalendarSystem.GetIslamicCalendar(leapYearPatterns.Max() + 1, epochs.Min()));
            Assert.Throws<ArgumentOutOfRangeException>(() => CalendarSystem.GetIslamicCalendar(leapYearPatterns.Min(), epochs.Max() + 1));
        }

        [Test]
        public void PropertiesInMaxYear()
        {
            // Construct the largest LocalDate we can, and validate that all the properties can be fetched without
            // issues.
            int year = SampleCalendar.MaxYear;
            int month = SampleCalendar.GetMaxMonth(year);
            int day = SampleCalendar.GetDaysInMonth(year, month);
            var localDate = new LocalDate(year, month, day, SampleCalendar);
            Assert.AreEqual(year, localDate.Year);
            Assert.AreEqual(month, localDate.Month);
            Assert.AreEqual(day, localDate.Day);

            foreach (var property in typeof(LocalDate).GetProperties()
                .Where(p => p.Name != "WeekYear" && p.Name != "WeekOfWeekYear"))
            {
                property.GetValue(localDate, null);
            }
        }
    }
}
