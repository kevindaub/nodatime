#region Copyright and license information
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
using NodaTime.Format;
using System.Globalization;

namespace NodaTime.Test
{
    [TestFixture]
    public partial class InstantTest
    {
        // Test is commented out as little of it makes sense at the moment. We may or may not want some of it :)
        /*
        // test in 2002/03 as time zones are more well known
        // (before the late 90's they were all over the place)
        private static readonly DateTimeZone Paris = DateTimeZone.ForID("Europe/Paris");
        private static readonly DateTimeZone London = DateTimeZone.ForID("Europe/London");
         */

        private const long Y2002Days =
            365 + 365 + 366 + 365 + 365 + 365 + 366 + 365 + 365 + 365 + 366 + 365 + 365 + 365 + 366 + 365 + 365 + 365 + 366 + 365 + 365 + 365 + 366 + 365 + 365 +
            365 + 366 + 365 + 365 + 365 + 366 + 365;

        private const long Y2003Days =
            365 + 365 + 366 + 365 + 365 + 365 + 366 + 365 + 365 + 365 + 366 + 365 + 365 + 365 + 366 + 365 + 365 + 365 + 366 + 365 + 365 + 365 + 366 + 365 + 365 +
            365 + 366 + 365 + 365 + 365 + 366 + 365 + 365;

        // 2002-06-09
        // private const long TestTimeNow = (Y2002Days + 31L + 28L + 31L + 30L + 31L + 9L - 1L) * NodaConstants.MillisecondsPerDay;

        // 2002-04-05
        private const long TestTime1 =
            (Y2002Days + 31L + 28L + 31L + 5L - 1L) * NodaConstants.MillisecondsPerDay + 12L * NodaConstants.MillisecondsPerHour +
            24L * NodaConstants.MillisecondsPerMinute;

        // 2003-05-06
        private const long TestTime2 =
            (Y2003Days + 31L + 28L + 31L + 30L + 6L - 1L) * NodaConstants.MillisecondsPerDay + 14L * NodaConstants.MillisecondsPerHour +
            28L * NodaConstants.MillisecondsPerMinute;

        private Instant one = new Instant(1L);
        private readonly Instant onePrime = new Instant(1L);
        private Instant negativeOne = new Instant(-1L);
        private Instant threeMillion = new Instant(3000000L);
        private Instant negativeFiftyMillion = new Instant(-50000000L);

        private readonly Offset offsetOneHour = Offset.ForHours(1);

        [Test]
        public void TestInstantOperators()
        {
            const long diff = TestTime2 - TestTime1;

            var time1 = new Instant(TestTime1);
            var time2 = new Instant(TestTime2);
            Duration duration = time2 - time1;

            Assert.AreEqual(diff, duration.Ticks);
            Assert.AreEqual(TestTime2, (time1 + duration).Ticks);
            Assert.AreEqual(TestTime1, (time2 - duration).Ticks);
        }

        [Test]
        public void FromUtcNoSeconds()
        {
            Instant viaUtc = DateTimeZone.Utc.At(new LocalDateTime(2008, 4, 3, 10, 35, 0)).ToInstant();
            Assert.AreEqual(viaUtc, Instant.FromUtc(2008, 4, 3, 10, 35));
        }

        [Test]
        public void FromUtcWithSeconds()
        {
            Instant viaUtc = DateTimeZone.Utc.At(new LocalDateTime(2008, 4, 3, 10, 35, 23)).ToInstant();
            Assert.AreEqual(viaUtc, Instant.FromUtc(2008, 4, 3, 10, 35, 23));
        }

        [Test]
        public void TestToString_InvalidFormat()
        {
            Assert.Throws<FormatException>(() => Instant.UnixEpoch.ToString("A"));
        }

        [Test]
        public void TestToString_MinValue()
        {
            TestToStringBase(Instant.MinValue, Instant.BeginningOfTimeLabel);
        }

        [Test]
        public void TestToString_MaxValue()
        {
            TestToStringBase(Instant.MaxValue, Instant.EndOfTimeLabel);
        }

        [Test]
        public void TestToString_UnixEpoch()
        {
            TestToStringBase(Instant.UnixEpoch, "1970-01-01T00:00:00Z");
        }

        [Test]
        public void Max()
        {
            Instant x = new Instant(100);
            Instant y = new Instant(200);
            Assert.AreEqual(y, Instant.Max(x, y));
            Assert.AreEqual(y, Instant.Max(y, x));
            Assert.AreEqual(x, Instant.Max(x, Instant.MinValue));
            Assert.AreEqual(x, Instant.Max(Instant.MinValue, x));
            Assert.AreEqual(Instant.MaxValue, Instant.Max(Instant.MaxValue, x));
            Assert.AreEqual(Instant.MaxValue, Instant.Max(x, Instant.MaxValue));
        }

        [Test]
        public void Min()
        {
            Instant x = new Instant(100);
            Instant y = new Instant(200);
            Assert.AreEqual(x, Instant.Min(x, y));
            Assert.AreEqual(x, Instant.Min(y, x));
            Assert.AreEqual(Instant.MinValue, Instant.Min(x, Instant.MinValue));
            Assert.AreEqual(Instant.MinValue, Instant.Min(Instant.MinValue, x));
            Assert.AreEqual(x, Instant.Min(Instant.MaxValue, x));
            Assert.AreEqual(x, Instant.Min(x, Instant.MaxValue));
        }

        private static void TestToStringBase(Instant value, string gvalue)
        {
            var actual = value.ToString();
            Assert.AreEqual(gvalue, actual);
            actual = value.ToString("G");
            Assert.AreEqual(gvalue, actual);
            actual = value.ToString("N");
            Assert.AreEqual(value.Ticks.ToString("N0"), actual);
            actual = value.ToString("N", CultureInfo.InvariantCulture);
            Assert.AreEqual(value.Ticks.ToString("N0", CultureInfo.InvariantCulture), actual);
            actual = value.ToString(CultureInfo.InvariantCulture);
            Assert.AreEqual(gvalue, actual);
            actual = value.ToString("D");
            Assert.AreEqual(value.Ticks.ToString("D"), actual);

            actual = string.Format("{0}", value);
            Assert.AreEqual(gvalue, actual);
            actual = string.Format("{0:G}", value);
            Assert.AreEqual(gvalue, actual);
            actual = string.Format("{0:N}", value);
            Assert.AreEqual(value.Ticks.ToString("N0"), actual);
            actual = string.Format("{0:D}", value);
            Assert.AreEqual(value.Ticks.ToString("D"), actual);
        }
    }
}