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

namespace NodaTime.Test
{
    partial class DurationTest
    {
        private const long StartTicks = 123456789L;
        private const long EndTicks = 987654321L;

        [Test]
        public void Zero()
        {
            Duration test = Duration.Zero;
            Assert.AreEqual(0, test.Ticks);
        }

        [Test]
        public void Factory_StandardDays()
        {
            Duration test = Duration.FromStandardDays(1);
            Assert.AreEqual(1 * NodaConstants.TicksPerStandardDay, test.Ticks);

            test = Duration.FromStandardDays(2);
            Assert.AreEqual(2 * NodaConstants.TicksPerStandardDay, test.Ticks);

            test = Duration.FromStandardDays(0);
            Assert.AreEqual(Duration.Zero, test);
        }

        [Test]
        public void Factory_StandardHours()
        {
            Duration test = Duration.FromHours(1);
            Assert.AreEqual(1 * NodaConstants.TicksPerHour, test.Ticks);

            test = Duration.FromHours(2);
            Assert.AreEqual(2 * NodaConstants.TicksPerHour, test.Ticks);

            test = Duration.FromHours(0);
            Assert.AreEqual(Duration.Zero, test);
        }

        [Test]
        public void FromMinutes()
        {
            Duration test = Duration.FromMinutes(1);
            Assert.AreEqual(1 * NodaConstants.TicksPerMinute, test.Ticks);

            test = Duration.FromMinutes(2);
            Assert.AreEqual(2 * NodaConstants.TicksPerMinute, test.Ticks);

            test = Duration.FromMinutes(0);
            Assert.AreEqual(Duration.Zero, test);
        }

        [Test]
        public void FromSeconds()
        {
            Duration test = Duration.FromSeconds(1);
            Assert.AreEqual(1 * NodaConstants.TicksPerSecond, test.Ticks);

            test = Duration.FromSeconds(2);
            Assert.AreEqual(2 * NodaConstants.TicksPerSecond, test.Ticks);

            test = Duration.FromSeconds(0);
            Assert.AreEqual(Duration.Zero, test);
        }

        [Test]
        public void FromMilliseconds()
        {
            Duration test = Duration.FromMilliseconds(1);
            Assert.AreEqual(1 * NodaConstants.TicksPerMillisecond, test.Ticks);

            test = Duration.FromMilliseconds(2);
            Assert.AreEqual(2 * NodaConstants.TicksPerMillisecond, test.Ticks);

            test = Duration.FromMilliseconds(0);
            Assert.AreEqual(Duration.Zero, test);
        }

        [Test]
        public void FromTicks()
        {
            Duration test = Duration.FromTicks(1);
            Assert.AreEqual(1, test.Ticks);

            test = Duration.FromTicks(2);
            Assert.AreEqual(2, test.Ticks);

            test = Duration.FromTicks(0);
            Assert.AreEqual(Duration.Zero, test);
        }

        [Test]
        public void FromAndToTimeSpan()
        {
            TimeSpan timeSpan = TimeSpan.FromHours(3) + TimeSpan.FromSeconds(2) + TimeSpan.FromTicks(1);
            Duration duration = Duration.FromHours(3) + Duration.FromSeconds(2) + Duration.FromTicks(1);
            Assert.AreEqual(duration, Duration.FromTimeSpan(timeSpan));
            Assert.AreEqual(timeSpan, duration.ToTimeSpan());

            Duration maxDuration = Duration.FromTicks(Int64.MaxValue);
            Assert.AreEqual(maxDuration, Duration.FromTimeSpan(TimeSpan.MaxValue));
            Assert.AreEqual(TimeSpan.MaxValue, maxDuration.ToTimeSpan());
            Duration minDuration = Duration.FromTicks(Int64.MinValue);

            Assert.AreEqual(minDuration, Duration.FromTimeSpan(TimeSpan.MinValue));
            Assert.AreEqual(TimeSpan.MinValue, minDuration.ToTimeSpan());
        }

        [Test]
        public void ConstructFrom_Int64()
        {
            long length = 4 * NodaConstants.TicksPerStandardDay + 5 * NodaConstants.TicksPerHour + 6 * NodaConstants.TicksPerMinute + 7 * NodaConstants.TicksPerSecond +
                                8 * NodaConstants.TicksPerMillisecond + 9;
            var test = new Duration(length);
            Assert.AreEqual(length, test.Ticks);
        }
    }
}
