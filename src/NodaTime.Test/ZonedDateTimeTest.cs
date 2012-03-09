#region Copyright and license information
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
using NUnit.Framework;
using NodaTime.Calendars;
using NodaTime.Testing.TimeZones;
using NodaTime.TimeZones;

namespace NodaTime.Test
{
    /// <summary>
    /// Tests for <see cref="ZonedDateTime"/>. Many of these are really testing
    /// calendar and time zone functionality, but the entry point to that
    /// functionality is usually through ZonedDateTime. This makes testing easier,
    /// as well as serving as more useful documentation.
    /// </summary>
    [TestFixture]
    public class ZonedDateTimeTest
    {
        /// <summary>
        /// Changes from UTC+3 to UTC+4 at 1am local time on June 13th 2011.
        /// </summary>
        private static SingleTransitionZone SampleZone = new SingleTransitionZone(Instant.FromUtc(2011, 6, 12, 22, 0), 3, 4);

        private static readonly DateTimeZone Pacific = DateTimeZone.ForId("America/Los_Angeles");

        [Test]
        public void SimpleProperties()
        {
            var value = SampleZone.AtStrictly(new LocalDateTime(2012, 2, 10, 8, 9, 10, 11, 12));
            Assert.AreEqual(Era.Common, value.Era);
            Assert.AreEqual(20, value.CenturyOfEra);
            Assert.AreEqual(12, value.YearOfCentury);
            Assert.AreEqual(2012, value.Year);
            Assert.AreEqual(2012, value.YearOfEra);
            Assert.AreEqual(2, value.Month);
            Assert.AreEqual(10, value.Day);
            Assert.AreEqual(6, value.WeekOfWeekYear);
            Assert.AreEqual(2012, value.WeekYear);
            Assert.AreEqual(IsoDayOfWeek.Friday, value.IsoDayOfWeek);
            Assert.AreEqual((int) IsoDayOfWeek.Friday, value.DayOfWeek);
            Assert.AreEqual(41, value.DayOfYear);
            Assert.AreEqual(8, value.ClockHourOfHalfDay);
            Assert.AreEqual(8, value.Hour);
            Assert.AreEqual(9, value.Minute);
            Assert.AreEqual(10, value.Second);
            Assert.AreEqual(8 * 3600 + 9 * 60 + 10, value.SecondOfDay);
            Assert.AreEqual(11, value.Millisecond);
            Assert.AreEqual(value.SecondOfDay * 1000 + 11, value.MillisecondOfDay);
            Assert.AreEqual(12, value.Tick);
            Assert.AreEqual(11 * 10000 + 12, value.TickOfSecond);
            Assert.AreEqual(value.MillisecondOfDay * 10000L + 12, value.TickOfDay);
        }

        [Test]
        public void AtStrictly_InWinter()
        {
            var when = Pacific.AtStrictly(new LocalDateTime(2009, 12, 22, 21, 39, 30));
            Instant instant = when.ToInstant();
            LocalInstant localInstant = when.LocalInstant;
            Assert.AreEqual(instant, localInstant.Minus(Offset.FromHours(-8)));

            Assert.AreEqual(2009, when.Year);
            Assert.AreEqual(12, when.Month);
            Assert.AreEqual(22, when.Day);
            Assert.AreEqual(2, when.DayOfWeek);
            Assert.AreEqual(21, when.Hour);
            Assert.AreEqual(39, when.Minute);
            Assert.AreEqual(30, when.Second);
        }

        [Test]
        public void AtStrictly_InSummer()
        {
            var when = Pacific.AtStrictly(new LocalDateTime(2009, 6, 22, 21, 39, 30));
            Instant instant = when.ToInstant();
            LocalInstant localInstant = when.LocalInstant;
            Assert.AreEqual(instant, localInstant.Minus(Offset.FromHours(-7)));

            Assert.AreEqual(2009, when.Year);
            Assert.AreEqual(6, when.Month);
            Assert.AreEqual(22, when.Day);
            Assert.AreEqual(21, when.Hour);
            Assert.AreEqual(39, when.Minute);
            Assert.AreEqual(30, when.Second);
        }

        /// <summary>
        /// Pacific time changed from -7 to -8 at 2am wall time on November 2nd 2009,
        /// so 2am became 1am. Using the constructor of ZonedDateTime, we should get
        /// the *later* version, i.e. when the offset is 8 hours. The instant should
        /// therefore represent 09:30 UTC.
        /// FIXME: This test makes no sense. It passes because the time went back on November 1st, not November 2nd.
        /// </summary>
        [Test]
        public void AtStrictly_ThrowsWhenAmbiguous()
        {
            Assert.Throws<AmbiguousTimeException>(() => Pacific.AtStrictly(new LocalDateTime(2009, 11, 1, 1, 30, 0)));
        }

        /// <summary>
        /// Pacific time changed from -8 to -7 at 2am wall time on March 8th 2009,
        /// so 2am became 3am. This means that 2.30am doesn't exist on that day.
        /// </summary>
        [Test]
        public void AtStrictly_ThrowsWhenSkipped()
        {
            Assert.Throws<SkippedTimeException>(() => Pacific.AtStrictly(new LocalDateTime(2009, 3, 8, 2, 30, 0)));
        }

        [Test]
        public void Add_AroundTimeZoneTransition()
        {
            // Before the transition at 3pm...
            ZonedDateTime before = SampleZone.AtStrictly(new LocalDateTime(2011, 6, 12, 15, 0));
            // 24 hours elapsed, and it's 4pm
            ZonedDateTime afterExpected = SampleZone.AtStrictly(new LocalDateTime(2011, 6, 13, 16, 0));
            ZonedDateTime afterAdd = ZonedDateTime.Add(before, Duration.OneStandardDay);
            ZonedDateTime afterOperator = before + Duration.OneStandardDay;

            Assert.AreEqual(afterExpected, afterAdd);
            Assert.AreEqual(afterExpected, afterOperator);
        }

        [Test]
        public void Add_MethodEquivalents()
        {
            ZonedDateTime before = SampleZone.AtStrictly(new LocalDateTime(2011, 6, 12, 15, 0));
            Assert.AreEqual(before + Duration.OneStandardDay, ZonedDateTime.Add(before, Duration.OneStandardDay));
            Assert.AreEqual(before + Duration.OneStandardDay, before.Plus(Duration.OneStandardDay));
        }

        [Test]
        public void Subtract_AroundTimeZoneTransition()
        {
            // After the transition at 4pm...
            ZonedDateTime after = SampleZone.AtStrictly(new LocalDateTime(2011, 6, 13, 16, 0));
            // 24 hours earlier, and it's 3pm
            ZonedDateTime beforeExpected = SampleZone.AtStrictly(new LocalDateTime(2011, 6, 12, 15, 0));
            ZonedDateTime beforeSubtract = ZonedDateTime.Subtract(after, Duration.OneStandardDay);
            ZonedDateTime beforeOperator = after - Duration.OneStandardDay;

            Assert.AreEqual(beforeExpected, beforeSubtract);
            Assert.AreEqual(beforeExpected, beforeOperator);
        }

        [Test]
        public void Subtract_MethodEquivalents()
        {
            ZonedDateTime after = SampleZone.AtStrictly(new LocalDateTime(2011, 6, 13, 16, 0));
            Assert.AreEqual(after - Duration.OneStandardDay, ZonedDateTime.Subtract(after, Duration.OneStandardDay));
            Assert.AreEqual(after - Duration.OneStandardDay, after.Minus(Duration.OneStandardDay));
        }


        [Test]
        public void WithZone()
        {
            Instant instant = Instant.FromUtc(2012, 2, 4, 12, 35);
            ZonedDateTime zoned = new ZonedDateTime(instant, SampleZone);
            Assert.AreEqual(new LocalDateTime(2012, 2, 4, 16, 35, 0), zoned.LocalDateTime);

            // Will be UTC-8 for our instant.
            DateTimeZone newZone = new SingleTransitionZone(Instant.FromUtc(2000, 1, 1, 0, 0), -7, -8);
            ZonedDateTime converted = zoned.WithZone(newZone);
            Assert.AreEqual(new LocalDateTime(2012, 2, 4, 4, 35, 0), converted.LocalDateTime);
            Assert.AreEqual(converted.ToInstant(), instant);
        }

        [Test]
        public void FromDateTimeOffset()
        {
            DateTimeOffset dateTimeOffset = new DateTimeOffset(2011, 3, 5, 1, 0, 0, TimeSpan.FromHours(3));
            DateTimeZone fixedZone = new FixedDateTimeZone(Offset.FromHours(3));
            ZonedDateTime expected = fixedZone.AtStrictly(new LocalDateTime(2011, 3, 5, 1, 0, 0));
            ZonedDateTime actual = ZonedDateTime.FromDateTimeOffset(dateTimeOffset);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ToDateTimeOffset()
        {
            ZonedDateTime zoned = SampleZone.AtStrictly(new LocalDateTime(2011, 3, 5, 1, 0, 0));
            DateTimeOffset expected = new DateTimeOffset(2011, 3, 5, 1, 0, 0, TimeSpan.FromHours(3));
            DateTimeOffset actual = zoned.ToDateTimeOffset();
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ToDateTimeUtc()
        {
            ZonedDateTime zoned = SampleZone.AtStrictly(new LocalDateTime(2011, 3, 5, 1, 0, 0));
            // Note that this is 10pm the previous day, UTC - so 1am local time
            DateTime expected = new DateTime(2011, 3, 4, 22, 0, 0, DateTimeKind.Utc);
            DateTime actual = zoned.ToDateTimeUtc();
            Assert.AreEqual(expected, actual);
            // Kind isn't checked by Equals...
            Assert.AreEqual(DateTimeKind.Utc, actual.Kind);

        }

        [Test]
        public void ToDateTimeUnspecified()
        {
            ZonedDateTime zoned = SampleZone.AtStrictly(new LocalDateTime(2011, 3, 5, 1, 0, 0));
            DateTime expected = new DateTime(2011, 3, 5, 1, 0, 0, DateTimeKind.Unspecified);
            DateTime actual = zoned.ToDateTimeUnspecified();
            Assert.AreEqual(expected, actual);
            // Kind isn't checked by Equals...
            Assert.AreEqual(DateTimeKind.Unspecified, actual.Kind);
        }

        [Test]
        public void Equality()
        {
            // Goes back from 2am to 1am on June 13th
            SingleTransitionZone zone = new SingleTransitionZone(Instant.FromUtc(2011, 6, 12, 22, 0), 4, 3);
            var sample = zone.MapLocal(new LocalDateTime(2011, 6, 13, 1, 30)).First();
            var fromUtc = Instant.FromUtc(2011, 6, 12, 21, 30).InZone(zone);

            // Checks all the overloads etc: first check is that the zone matters
            TestHelper.TestEqualsStruct(sample, fromUtc, Instant.FromUtc(2011, 6, 12, 21, 30).InIsoUtc());
            TestHelper.TestOperatorEquality(sample, fromUtc, Instant.FromUtc(2011, 6, 12, 21, 30).InIsoUtc());

            // Now just use a simple inequality check for other aspects...

            // Different offset
            var later = zone.MapLocal(new LocalDateTime(2011, 6, 13, 1, 30)).Last();
            Assert.AreEqual(sample.LocalDateTime, later.LocalDateTime);
            Assert.AreNotEqual(sample.Offset, later.Offset);
            Assert.AreNotEqual(sample, later);

            // Different local time
            Assert.AreNotEqual(sample, zone.MapLocal(new LocalDateTime(2011, 6, 13, 1, 29)).First());

            // Different calendar
            var withOtherCalendar = zone.MapLocal(new LocalDateTime(2011, 6, 13, 1, 30, CalendarSystem.GetGregorianCalendar(4))).First();
            Assert.AreNotEqual(sample, withOtherCalendar);
        }

        [Test]
        public void ComparisonOperators_SameCalendarAndZone()
        {
            ZonedDateTime value1 = SampleZone.AtStrictly(new LocalDateTime(2011, 1, 2, 10, 30, 0));
            ZonedDateTime value2 = SampleZone.AtStrictly(new LocalDateTime(2011, 1, 2, 10, 30, 0));
            ZonedDateTime value3 = SampleZone.AtStrictly(new LocalDateTime(2011, 1, 2, 10, 45, 0));

            Assert.IsFalse(value1 < value2);
            Assert.IsTrue(value1 < value3);
            Assert.IsFalse(value2 < value1);
            Assert.IsFalse(value3 < value1);

            Assert.IsTrue(value1 <= value2);
            Assert.IsTrue(value1 <= value3);
            Assert.IsTrue(value2 <= value1);
            Assert.IsFalse(value3 <= value1);

            Assert.IsFalse(value1 > value2);
            Assert.IsFalse(value1 > value3);
            Assert.IsFalse(value2 > value1);
            Assert.IsTrue(value3 > value1);

            Assert.IsTrue(value1 >= value2);
            Assert.IsFalse(value1 >= value3);
            Assert.IsTrue(value2 >= value1);
            Assert.IsTrue(value3 >= value1);
        }

        [Test]
        public void ComparisonOperators_DifferentCalendars_AlwaysReturnsFalse()
        {
            LocalDateTime value1 = new LocalDateTime(2011, 1, 2, 10, 30);
            LocalDateTime value2 = new LocalDateTime(2011, 1, 3, 10, 30, CalendarSystem.GetJulianCalendar(4));

            // All inequality comparisons return false
            Assert.IsFalse(value1 < value2);
            Assert.IsFalse(value1 <= value2);
            Assert.IsFalse(value1 > value2);
            Assert.IsFalse(value1 >= value2);
        }

        [Test]
        public void ComparisonOperators_DifferentZones_AlwaysReturnsFalse()
        {
            // Note that the offsets will be the same as for SampleZone in the values we're using
            var otherZone = new FixedDateTimeZone(SampleZone.EarlyInterval.WallOffset);

            ZonedDateTime value1 = SampleZone.AtStrictly(new LocalDateTime(2011, 1, 2, 10, 30));
            ZonedDateTime value2 = otherZone.AtStrictly(new LocalDateTime(2011, 1, 3, 10, 30));

            // All inequality comparisons return false
            Assert.IsFalse(value1 < value2);
            Assert.IsFalse(value1 <= value2);
            Assert.IsFalse(value1 > value2);
            Assert.IsFalse(value1 >= value2);
        }

        [Test]
        public void CompareTo_SameCalendarAndZone()
        {
            ZonedDateTime value1 = SampleZone.AtStrictly(new LocalDateTime(2011, 1, 2, 10, 30, 0));
            ZonedDateTime value2 = SampleZone.AtStrictly(new LocalDateTime(2011, 1, 2, 10, 30, 0));
            ZonedDateTime value3 = SampleZone.AtStrictly(new LocalDateTime(2011, 1, 2, 10, 45, 0));

            Assert.That(value1.CompareTo(value2), Is.EqualTo(0));
            Assert.That(value1.CompareTo(value3), Is.LessThan(0));
            Assert.That(value3.CompareTo(value2), Is.GreaterThan(0));
        }

        [Test]
        public void CompareTo_DifferentCalendars_OnlyInstantMatters()
        {
            CalendarSystem islamic = CalendarSystem.GetIslamicCalendar(IslamicLeapYearPattern.Base15, IslamicEpoch.Astronomical);
            ZonedDateTime value1 = SampleZone.AtStrictly(new LocalDateTime(2011, 1, 2, 10, 30));
            ZonedDateTime value2 = SampleZone.AtStrictly(new LocalDateTime(1500, 1, 1, 10, 30, islamic));
            ZonedDateTime value3 = SampleZone.AtStrictly(value1.LocalDateTime.WithCalendar(islamic));

            Assert.That(value1.CompareTo(value2), Is.LessThan(0));
            Assert.That(value2.CompareTo(value1), Is.GreaterThan(0));
            Assert.That(value1.CompareTo(value3), Is.EqualTo(0));
        }

        [Test]
        public void CompareTo_DifferentZones_OnlyInstantMatters()
        {
            var otherZone = new FixedDateTimeZone(Offset.FromHours(-20));

            ZonedDateTime value1 = SampleZone.AtStrictly(new LocalDateTime(2011, 1, 2, 10, 30));
            // Earlier local time, but later instant
            ZonedDateTime value2 = otherZone.AtStrictly(new LocalDateTime(2011, 1, 2, 5, 30));
            ZonedDateTime value3 = value1.WithZone(otherZone);

            Assert.That(value1.CompareTo(value2), Is.LessThan(0));
            Assert.That(value2.CompareTo(value1), Is.GreaterThan(0));
            Assert.That(value1.CompareTo(value3), Is.EqualTo(0));
        }
        [Test]
        public void Constructor_ArgumentValidation()
        {
            Assert.Throws<ArgumentNullException>(() => new ZonedDateTime(new Instant(1000), null));
            Assert.Throws<ArgumentNullException>(() => new ZonedDateTime(new Instant(1000), null, CalendarSystem.Iso));
            Assert.Throws<ArgumentNullException>(() => new ZonedDateTime(new Instant(1000), SampleZone, null));
        }
    }
}
