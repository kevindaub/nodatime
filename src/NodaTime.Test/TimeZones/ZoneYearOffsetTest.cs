﻿#region Copyright and license information

// Copyright 2001-2009 Stephen Colebourne
// Copyright 2009 Jon Skeet
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
using NodaTime.TimeZones;
using NUnit.Framework;
using NodaTime.Calendars;

namespace NodaTime.Test.TimeZones
{
    [TestFixture]
    public partial class ZoneYearOffsetTest
    {
        private const long TicksPerStandardYear = NodaConstants.TicksPerDay * 365;
        private const long TicksPerLeapYear = NodaConstants.TicksPerDay * 366;

        private Offset oneHour = Offset.Create(1);
        private Offset twoHours = Offset.Create(2);
        private Offset minusOneHour = Offset.Create(-1);

        [Test]
        public void Construct_InvalidMonth_Exception()
        {
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => new ZoneYearOffset(TransitionMode.Standard, 0, 1, 1, true, Offset.Zero), "Month 0");
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => new ZoneYearOffset(TransitionMode.Standard, 34, 1, 1, true, Offset.Zero), "Month 34");
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => new ZoneYearOffset(TransitionMode.Standard, -3, 1, 1, true, Offset.Zero), "Month -3");
        }

        [Test]
        public void Construct_InvalidDayOfMonth_Exception()
        {
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => new ZoneYearOffset(TransitionMode.Standard, 2, 0, 1, true, Offset.Zero), "Day of Month 0");
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => new ZoneYearOffset(TransitionMode.Standard, 2, 32, 1, true, Offset.Zero), "Day of Month 32");
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => new ZoneYearOffset(TransitionMode.Standard, 2, 475, 1, true, Offset.Zero), "Day of Month 475");
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => new ZoneYearOffset(TransitionMode.Standard, 2, -32, 1, true, Offset.Zero), "Day of Month -32");
        }

        [Test]
        public void Construct_InvalidDayOfWeek_Exception()
        {
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => new ZoneYearOffset(TransitionMode.Standard, 2, 3, -1, true, Offset.Zero), "Day of Week -1");
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => new ZoneYearOffset(TransitionMode.Standard, 2, 3, 8, true, Offset.Zero), "Day of Week 8");
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => new ZoneYearOffset(TransitionMode.Standard, 2, 3, 5756, true, Offset.Zero), "Day of Week 5856");
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => new ZoneYearOffset(TransitionMode.Standard, 2, 3, -347, true, Offset.Zero), "Day of Week -347");
        }

        [Test]
        public void Construct_InvalidTickOfDay_Exception()
        {
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => new ZoneYearOffset(TransitionMode.Standard, 2, 3, -1, true, Offset.MinValue), "Tick of day MinValue");
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => new ZoneYearOffset(TransitionMode.Standard, 2, 3, 8, true, new Offset(-1)), "Tick of day MinValue -1");
        }

        [Test]
        public void Construct_ValidMonths()
        {
            for (int month = 1; month <= 12; month++)
            {
                Assert.NotNull(new ZoneYearOffset(TransitionMode.Standard, month, 1, 1, true, Offset.Zero), "Month " + month);
            }
        }

        [Test]
        public void Construct_ValidDays()
        {
            for (int day = 1; day <= 31; day++)
            {
                Assert.NotNull(new ZoneYearOffset(TransitionMode.Standard, 1, day, 1, true, Offset.Zero), "Day " + day);
            }
            for (int day = -1; day >= -31; day--)
            {
                Assert.NotNull(new ZoneYearOffset(TransitionMode.Standard, 1, day, 1, true, Offset.Zero), "Day " + day);
            }
        }

        [Test]
        public void Construct_ValidDaysOfWeek()
        {
            for (int dayOfWeek = 0; dayOfWeek <= 7; dayOfWeek++)
            {
                Assert.NotNull(new ZoneYearOffset(TransitionMode.Standard, 1, 1, dayOfWeek, true, Offset.Zero), "Day of week " + dayOfWeek);
            }
        }

        [Test]
        public void Construct_ValidTickOfDay()
        {
            int delta = (Offset.MaxValue.Milliseconds / 100);
            for (int millisecond = 0; millisecond < Offset.MaxValue.Milliseconds; millisecond += delta)
            {
                Offset tickOfDay = new Offset(millisecond);
                Assert.NotNull(new ZoneYearOffset(TransitionMode.Standard, 1, 1, 0, true, tickOfDay), "Tick of Day " + tickOfDay);
            }
        }

        [Test]
        public void MakeInstant_Defaults_Epoch()
        {
            ZoneYearOffset offset = new ZoneYearOffset(TransitionMode.Utc, 1, 1, 0, true, Offset.Zero);
            Instant actual = offset.MakeInstant(1970, Offset.Zero, Offset.Zero);
            Instant expected = Instant.UnixEpoch;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void MakeInstant_Year_1971()
        {
            ZoneYearOffset offset = new ZoneYearOffset(TransitionMode.Utc, 1, 1, 0, true, Offset.Zero);
            Instant actual = offset.MakeInstant(1971, Offset.Zero, Offset.Zero);
            Instant expected = new Instant(365L * NodaConstants.TicksPerDay);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void MakeInstant_SavingOffsetIgnored_Epoch()
        {
            ZoneYearOffset offset = new ZoneYearOffset(TransitionMode.Utc, 1, 1, 0, true, Offset.Zero);
            Instant actual = offset.MakeInstant(1970, twoHours, oneHour);
            Instant expected = Instant.UnixEpoch;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void MakeInstant_SavingIgnored()
        {
            ZoneYearOffset offset = new ZoneYearOffset(TransitionMode.Standard, 1, 1, 0, true, Offset.Zero);
            Instant actual = offset.MakeInstant(1970, twoHours, oneHour);
            Instant expected = new Instant((1L - 1) * NodaConstants.TicksPerDay - twoHours.AsTicks());
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void MakeInstant_SavingAndOffset()
        {
            ZoneYearOffset offset = new ZoneYearOffset(TransitionMode.Wall, 1, 1, 0, true, Offset.Zero);
            Instant actual = offset.MakeInstant(1970, twoHours, oneHour);
            Instant expected = new Instant((1L - 1) * NodaConstants.TicksPerDay - (twoHours.AsTicks() + oneHour.AsTicks()));
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void MakeInstant_Milliseconds()
        {
            ZoneYearOffset offset = new ZoneYearOffset(TransitionMode.Utc, 1, 1, 0, true, Offset.Create(0, 0, 0, 1));
            Instant actual = offset.MakeInstant(1970, Offset.Zero, Offset.Zero);
            Instant expected = new Instant((1L - 1) * NodaConstants.TicksPerDay + NodaConstants.TicksPerMillisecond);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void MakeInstant_WednesdayForward()
        {
            ZoneYearOffset offset = new ZoneYearOffset(TransitionMode.Utc, 1, 1, (int)DayOfWeek.Wednesday, true, Offset.Zero);
            Instant actual = offset.MakeInstant(1970, Offset.Zero, Offset.Zero);
            Instant expected = new Instant((7L - 1) * NodaConstants.TicksPerDay);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void MakeInstant_WednesdayBackward()
        {
            ZoneYearOffset offset = new ZoneYearOffset(TransitionMode.Utc, 1, 15, (int)DayOfWeek.Wednesday, false, Offset.Zero);
            Instant actual = offset.MakeInstant(1970, Offset.Zero, Offset.Zero);
            Instant expected = new Instant((14L - 1) * NodaConstants.TicksPerDay);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void MakeInstant_JanOne()
        {
            ZoneYearOffset offset = new ZoneYearOffset(TransitionMode.Utc, 1, 1, 0, true, Offset.Zero);
            Instant actual = offset.MakeInstant(1970, Offset.Zero, Offset.Zero);
            Instant expected = new Instant((1L - 1) * NodaConstants.TicksPerDay);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void MakeInstant_JanMinusTwo()
        {
            ZoneYearOffset offset = new ZoneYearOffset(TransitionMode.Utc, 1, -2, 0, true, Offset.Zero);
            Instant actual = offset.MakeInstant(1970, Offset.Zero, Offset.Zero);
            Instant expected = new Instant((30L - 1) * NodaConstants.TicksPerDay);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void MakeInstant_JanFive()
        {
            ZoneYearOffset offset = new ZoneYearOffset(TransitionMode.Utc, 1, 5, 0, true, Offset.Zero);
            Instant actual = offset.MakeInstant(1970, Offset.Zero, Offset.Zero);
            Instant expected = new Instant((5L - 1) * NodaConstants.TicksPerDay);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void MakeInstant_Feb()
        {
            ZoneYearOffset offset = new ZoneYearOffset(TransitionMode.Utc, 2, 1, 0, true, Offset.Zero);
            Instant actual = offset.MakeInstant(1970, Offset.Zero, Offset.Zero);
            Instant expected = new Instant((32L - 1) * NodaConstants.TicksPerDay);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Next_JanOne()
        {
            ZoneYearOffset offset = new ZoneYearOffset(TransitionMode.Utc, 1, 1, 0, true, Offset.Zero);
            Instant actual = offset.MakeInstant(1970, Offset.Zero, Offset.Zero);
            long baseTicks = actual.Ticks;
            actual = offset.Next(actual, Offset.Zero, Offset.Zero);
            Instant expected = new Instant(baseTicks + (1 * TicksPerStandardYear));
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void NextTwice_JanOne()
        {
            ZoneYearOffset offset = new ZoneYearOffset(TransitionMode.Utc, 1, 1, 0, true, Offset.Zero);
            Instant actual = offset.MakeInstant(1970, Offset.Zero, Offset.Zero);
            long baseTicks = actual.Ticks;
            actual = offset.Next(actual, Offset.Zero, Offset.Zero);
            actual = offset.Next(actual, Offset.Zero, Offset.Zero);
            Instant expected = new Instant(baseTicks + (2 * TicksPerStandardYear));
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Next_Feb29_FourYears()
        {
            ZoneYearOffset offset = new ZoneYearOffset(TransitionMode.Utc, 2, 29, 0, true, Offset.Zero);
            Instant actual = offset.MakeInstant(1972, Offset.Zero, Offset.Zero);
            long baseTicks = actual.Ticks;
            actual = offset.Next(actual, Offset.Zero, Offset.Zero);
            Instant expected = new Instant(baseTicks + (3 * TicksPerStandardYear) + TicksPerLeapYear);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void NextTwice_Feb29_FourYears()
        {
            ZoneYearOffset offset = new ZoneYearOffset(TransitionMode.Utc, 2, 29, 0, true, Offset.Zero);
            Instant actual = offset.MakeInstant(1972, Offset.Zero, Offset.Zero);
            long baseTicks = actual.Ticks;
            actual = offset.Next(actual, Offset.Zero, Offset.Zero);
            actual = offset.Next(actual, Offset.Zero, Offset.Zero);
            Instant expected = new Instant(baseTicks + (2 * ((3 * TicksPerStandardYear) + TicksPerLeapYear)));
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Previous_JanOne()
        {
            ZoneYearOffset offset = new ZoneYearOffset(TransitionMode.Utc, 1, 1, 0, true, Offset.Zero);
            Instant actual = offset.MakeInstant(1970, Offset.Zero, Offset.Zero);
            long baseTicks = actual.Ticks;
            actual = offset.Previous(actual, Offset.Zero, Offset.Zero);
            Instant expected = new Instant(baseTicks - (1 * TicksPerStandardYear));
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void PreviousTwice_JanOne()
        {
            ZoneYearOffset offset = new ZoneYearOffset(TransitionMode.Utc, 1, 1, 0, true, Offset.Zero);
            Instant actual = offset.MakeInstant(1970, Offset.Zero, Offset.Zero);
            long baseTicks = actual.Ticks;
            actual = offset.Previous(actual, Offset.Zero, Offset.Zero);
            actual = offset.Previous(actual, Offset.Zero, Offset.Zero);
            Instant expected = new Instant(baseTicks - (TicksPerStandardYear + TicksPerLeapYear));
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Next_WednesdayForward()
        {
            ZoneYearOffset offset = new ZoneYearOffset(TransitionMode.Utc, 10, 31, (int)DaysOfWeek.Wednesday, true, Offset.Zero);
            Instant actual = offset.MakeInstant(2006, Offset.Zero, Offset.Zero); // Nov 1 2006
            long baseTicks = actual.Ticks;
            actual = offset.Next(actual, Offset.Zero, Offset.Zero); // Oct 31 2007
            Instant expected = new Instant(baseTicks + (1 * TicksPerStandardYear) - NodaConstants.TicksPerDay);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void NextTwice_WednesdayForward()
        {
            ZoneYearOffset offset = new ZoneYearOffset(TransitionMode.Utc, 10, 31, (int)DaysOfWeek.Wednesday, true, Offset.Zero);
            Instant actual = offset.MakeInstant(2006, Offset.Zero, Offset.Zero); // Nov 1 2006
            long baseTicks = actual.Ticks;
            actual = offset.Next(actual, Offset.Zero, Offset.Zero); // Oct 31 2007
            actual = offset.Next(actual, Offset.Zero, Offset.Zero); // Nov 5 2008
            Instant expected = new Instant(baseTicks + TicksPerStandardYear + TicksPerLeapYear + (4 * NodaConstants.TicksPerDay));
            Assert.AreEqual(expected, actual);
        }
    }
}
