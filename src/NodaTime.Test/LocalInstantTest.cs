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
using NUnit.Framework;

namespace NodaTime.Test
{
    [TestFixture]
    public partial class LocalInstantTest
    {
        // Test is commented out as little of it makes sense at the moment. We may or may not want some of it :)
        /*
        // test in 2002/03 as time zones are more well known
        // (before the late 90's they were all over the place)
        private static readonly DateTimeZone Paris = DateTimeZone.ForID("Europe/Paris");
        private static readonly DateTimeZone London = DateTimeZone.ForID("Europe/London");
         */

        private const long Y2002Days = 365 + 365 + 366 + 365 + 365 + 365 + 366 + 365 + 365 + 365 +
                                       366 + 365 + 365 + 365 + 366 + 365 + 365 + 365 + 366 + 365 +
                                       365 + 365 + 366 + 365 + 365 + 365 + 366 + 365 + 365 + 365 +
                                       366 + 365;
        private const long Y2003Days = 365 + 365 + 366 + 365 + 365 + 365 + 366 + 365 + 365 + 365 +
                                       366 + 365 + 365 + 365 + 366 + 365 + 365 + 365 + 366 + 365 +
                                       365 + 365 + 366 + 365 + 365 + 365 + 366 + 365 + 365 + 365 +
                                       366 + 365 + 365;

        // 2002-06-09
        private const long TestTimeNow =
            (Y2002Days + 31L + 28L + 31L + 30L + 31L + 9L - 1L) * NodaConstants.MillisecondsPerDay;

        // 2002-04-05
        private const long TestTime1 =
            (Y2002Days + 31L + 28L + 31L + 5L - 1L) * NodaConstants.MillisecondsPerDay
            + 12L * NodaConstants.MillisecondsPerHour
            + 24L * NodaConstants.MillisecondsPerMinute;

        // 2003-05-06
        private const long TestTime2 =
            (Y2003Days + 31L + 28L + 31L + 30L + 6L - 1L) * NodaConstants.MillisecondsPerDay
            + 14L * NodaConstants.MillisecondsPerHour
            + 28L * NodaConstants.MillisecondsPerMinute;

        private LocalInstant one = new LocalInstant(1L);
        private LocalInstant onePrime = new LocalInstant(1L);
        private LocalInstant negativeOne = new LocalInstant(-1L);
        private LocalInstant threeMillion = new LocalInstant(3000000L);
        private LocalInstant negativeFiftyMillion = new LocalInstant(-50000000L);

        private Offset offsetOneHour = Offset.ForHours(1);

        [Test]
        public void TestLocalInstantOperators()
        {
            long diff = TestTime2 - TestTime1;

            LocalInstant time1 = new LocalInstant(TestTime1);
            LocalInstant time2 = new LocalInstant(TestTime2);
            Duration duration = time2 - time1;
            
            Assert.AreEqual(diff, duration.Ticks);
            Assert.AreEqual(TestTime2, (time1 + duration).Ticks);
            Assert.AreEqual(TestTime1, (time2 - duration).Ticks);
        }
    }
}