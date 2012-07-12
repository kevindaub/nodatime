﻿#region Copyright and license information
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

using NUnit.Framework;
using NodaTime.Testing;

namespace NodaTime.Test.Testing
{
    /// <summary>
    /// Tests for the FakeClock class in the Testing assembly.
    /// </summary>
    [TestFixture]
    public class FakeClockTest
    {
        [Test]
        public void DirectConstruction()
        {
            Instant instant = new Instant(100L);
            FakeClock clock = new FakeClock(instant);
            Assert.AreEqual(instant, clock.Now);
        }

        [Test]
        public void ConstructionWithAdvance()
        {
            Instant instant = new Instant(100L);
            Duration advance = Duration.FromTicks(5);
            FakeClock clock = new FakeClock(instant, advance);
            Assert.AreEqual(advance, clock.AutoAdvance);
            Assert.AreEqual(instant, clock.Now);
            Assert.AreEqual(instant + advance, clock.Now);
            Assert.AreEqual(instant + advance + advance, clock.Now);
        }

        [Test]
        public void ConstructionThenAdvance()
        {
            Instant instant = new Instant(100L);
            FakeClock clock = new FakeClock(instant);
            Assert.AreEqual(instant, clock.Now);
            Assert.AreEqual(instant, clock.Now);
            Duration advance = Duration.FromTicks(5);
            clock.AutoAdvance = advance;
            // Setting auto-advance doesn't actually change the clock...
            // but this call will.
            Assert.AreEqual(instant, clock.Now);
            Assert.AreEqual(instant + advance, clock.Now);
            Assert.AreEqual(instant + advance + advance, clock.Now);
        }

        [Test]
        public void UtcDateConstruction()
        {
            Instant instant = Instant.FromUtc(2010, 1, 1, 0, 0);
            FakeClock clock = FakeClock.FromUtc(2010, 1, 1);
            Assert.AreEqual(instant, clock.Now);
        }

        [Test]
        public void UtcDateTimeConstruction()
        {
            Instant instant = Instant.FromUtc(2010, 1, 1, 10, 30, 25);
            FakeClock clock = FakeClock.FromUtc(2010, 1, 1, 10, 30, 25);
            Assert.AreEqual(instant, clock.Now);
        }

        [Test]
        public void Advance()
        {
            FakeClock clock = new FakeClock(new Instant(100L));
            Duration d = new Duration(25);
            clock.Advance(d);
            Assert.AreEqual(125, clock.Now.Ticks);
        }

        [Test]
        public void AdvanceTicks()
        {
            FakeClock clock = new FakeClock(new Instant(100L));
            clock.AdvanceTicks(3);
            Assert.AreEqual(103, clock.Now.Ticks);
        }

        [Test]
        public void AdvanceMilliseconds()
        {
            FakeClock clock = new FakeClock(new Instant(100L));
            clock.AdvanceMilliseconds(3);
            Assert.AreEqual(100 + 3 * NodaConstants.TicksPerMillisecond, clock.Now.Ticks);
        }

        [Test]
        public void AdvanceSeconds()
        {
            FakeClock clock = new FakeClock(new Instant(100L));
            clock.AdvanceSeconds(3);
            Assert.AreEqual(100 + 3 * NodaConstants.TicksPerSecond, clock.Now.Ticks);
        }

        [Test]
        public void AdvanceMinutes()
        {
            FakeClock clock = new FakeClock(new Instant(100L));
            clock.AdvanceMinutes(3);
            Assert.AreEqual(100 + 3 * NodaConstants.TicksPerMinute, clock.Now.Ticks);
        }

        [Test]
        public void AdvanceHours()
        {
            FakeClock clock = new FakeClock(new Instant(100L));
            clock.AdvanceHours(3);
            Assert.AreEqual(100 + 3 * NodaConstants.TicksPerHour, clock.Now.Ticks);
        }

        [Test]
        public void AdvanceDays()
        {
            FakeClock clock = new FakeClock(new Instant(100L));
            clock.AdvanceDays(3);
            Assert.AreEqual(100 + 3 * NodaConstants.TicksPerStandardDay, clock.Now.Ticks);
        }

        [Test]
        public void Reset()
        {
            Instant instant1 = new Instant(100L);
            Instant instant2 = new Instant(500L);
            FakeClock clock = new FakeClock(instant1);
            Assert.AreEqual(instant1, clock.Now);
            clock.Reset(instant2);
            Assert.AreEqual(instant2, clock.Now);
        }
    }
}
