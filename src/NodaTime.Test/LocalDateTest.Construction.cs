﻿#region Copyright and license information
// Copyright 2001-2009 Stephen Colebourne
// Copyright 2009-2010 Jon Skeet
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
using NUnit.Framework;

namespace NodaTime.Test
{
    public partial class LocalDateTest
    {
        [Test]
        public void Constructor_CalendarDefaultsToIso()
        {
            LocalDate date = new LocalDate(2000, 1, 1);
            Assert.AreEqual(CalendarSystem.Iso, date.Calendar);
        }

        [Test]
        public void Constructor_PropertiesRoundTrip()
        {
            LocalDate date = new LocalDate(2023, 7, 27);
            Assert.AreEqual(2023, date.Year);
            Assert.AreEqual(7, date.MonthOfYear);
            Assert.AreEqual(27, date.DayOfMonth);
        }

        [Test]
        public void Constructor_PropertiesRoundTrip_CustomCalendar()
        {
            LocalDate date = new LocalDate(2023, 7, 27, CalendarSystem.GetJulianCalendar(4));
            Assert.AreEqual(2023, date.Year);
            Assert.AreEqual(7, date.MonthOfYear);
            Assert.AreEqual(27, date.DayOfMonth);
        }

        [Test]
        public void Constructor_InvalidMonth()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new LocalDate(2010, 13, 1));
        }

        [Test]
        public void Constructor_InvalidDay()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new LocalDate(2010, 1, 100));
        }

        [Test]
        public void Constructor_InvalidDayWithinMonth()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new LocalDate(2010, 2, 30));
        }

        [Test]
        public void Constructor_InvalidYear()
        {
            // TODO: We don't actually *expose* the maximum year of calendars. Maybe we should.
            Assert.Throws<ArgumentOutOfRangeException>(() => new LocalDate(40000, 1, 1));
        }
    }
}
