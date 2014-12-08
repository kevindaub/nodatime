// Copyright 2011 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using NodaTime.Calendars;
using NUnit.Framework;

namespace NodaTime.Test.Calendars
{
    /// <summary>
    /// Tests for the Coptic calendar system.
    /// </summary>
    [TestFixture]
    public class CopticCalendarSystemTest
    {
        [Test]
        public void CopticEpoch()
        {
            CalendarSystem coptic = CalendarSystem.Coptic;
            LocalDateTime copticEpoch = new LocalDateTime(1, 1, 1, 0, 0, coptic);

            CalendarSystem julian = CalendarSystem.Julian;
            LocalDateTime converted = copticEpoch.WithCalendar(julian);

            LocalDateTime expected = new LocalDateTime(284, 8, 29, 0, 0, julian);
            Assert.AreEqual(expected, converted);
        }

        [Test]
        public void UnixEpoch()
        {
            CalendarSystem coptic = CalendarSystem.Coptic;
            LocalDateTime unixEpochInCopticCalendar = NodaConstants.UnixEpoch.InZone(DateTimeZone.Utc, coptic).LocalDateTime;
            LocalDateTime expected = new LocalDateTime(1686, 4, 23, 0, 0, coptic);
            Assert.AreEqual(expected, unixEpochInCopticCalendar);
        }

        [Test]
        public void SampleDate()
        {
            CalendarSystem copticCalendar = CalendarSystem.Coptic;
            LocalDateTime iso = new LocalDateTime(2004, 6, 9, 0, 0, 0, 0);
            LocalDateTime coptic = iso.WithCalendar(copticCalendar);

            Assert.AreEqual(Era.AnnoMartyrum, coptic.Era);
            Assert.AreEqual(1720, coptic.YearOfEra);

            Assert.AreEqual(1720, coptic.Year);
            Assert.IsFalse(copticCalendar.IsLeapYear(1720));
        
            Assert.AreEqual(10, coptic.Month);
            Assert.AreEqual(2, coptic.Day);
            
            // TODO: Determine whether we should consider the Coptic calendar to use ISO days of the week.
            Assert.AreEqual(IsoDayOfWeek.Wednesday, coptic.IsoDayOfWeek);

            Assert.AreEqual(9 * 30 + 2, coptic.DayOfYear);

            Assert.AreEqual(0, coptic.Hour);
            Assert.AreEqual(0, coptic.Minute);
            Assert.AreEqual(0, coptic.Second);
            Assert.AreEqual(0, coptic.Millisecond);
        }
    }
}
