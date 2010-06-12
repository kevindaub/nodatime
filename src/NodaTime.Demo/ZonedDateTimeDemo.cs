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

using NodaTime.Calendars;
using NodaTime.Format;
using NodaTime.TimeZones;
using NUnit.Framework;

namespace NodaTime.Demo
{
    [TestFixture]
    internal class ZonedDateTimeDemo
    {
        private static readonly IDateTimeZone Dublin = DateTimeZones.ForId("Europe/Dublin");

        [Test]
        public void Construction()
        {
            ZonedDateTime dt = new ZonedDateTime(2010, 6, 9, 15, 15, 0, Dublin);

            Assert.AreEqual(15, dt.HourOfDay);
            Assert.AreEqual(2010, dt.Year);
            // Not 21... we're not in the Gregorian calendar!
            Assert.AreEqual(20, dt.CenturyOfEra);

            Instant instant = Instant.FromUtc(2010, 6, 9, 14, 15, 0);
            Assert.AreEqual(instant, dt.ToInstant());
        }

        [Test]
        public void Ambiguity()
        {
            ZonedDateTime late = new ZonedDateTime(2010, 10, 31, 1, 15, 0, Dublin);
            Assert.AreEqual("20101031T011500.000Z", IsoDateTimeFormats.BasicDateTime.Print(late));
            ZonedDateTime early = new ZonedDateTime(late.ToInstant() - Duration.OneHour, new Chronology(Dublin, IsoCalendarSystem.Instance));
            Assert.AreEqual("20101031T011500.000+0100", IsoDateTimeFormats.BasicDateTime.Print(early));
        }

        [Test]
        public void Impossibility()
        {
            Assert.Throws<SkippedTimeException>(() => new ZonedDateTime(2010, 3, 28, 1, 15, 0, Dublin));
        }
    }
}