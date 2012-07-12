﻿#region Copyright and license information
// Copyright 2001-2009 Stephen Colebourne
// Copyright 2009-2012 Jon Skeet
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
using NodaTime.Testing.TimeZones;
using NodaTime.TimeZones;

namespace NodaTime.Test.TimeZones
{
    [TestFixture]
    public class ResolversTest
    {
        /// <summary>
        /// Zone where the clocks go back at 1am at the start of the year 2000, back to midnight.
        /// </summary>
        private static readonly SingleTransitionZone AmbiguousZone = new SingleTransitionZone(Instant.FromUtc(2000, 1, 1, 0, 0), 1, 0);

        /// <summary>
        /// Zone where the clocks go forward at midnight at the start of the year 2000, to 1am.
        /// </summary>
        private static readonly SingleTransitionZone GapZone = new SingleTransitionZone(Instant.FromUtc(2000, 1, 1, 0, 0), 0, 1);

        /// <summary>
        /// Local time which is either skipped or ambiguous, depending on the zones above.
        /// </summary>
        private static readonly LocalDateTime TimeInTransition = new LocalDateTime(2000, 1, 1, 0, 20);

        [Test]
        public void ReturnEarlier()
        {
            var mapping = AmbiguousZone.MapLocal(TimeInTransition);
            Assert.AreEqual(2, mapping.Count);
            var resolved = Resolvers.ReturnEarlier(mapping.First(), mapping.Last());
            Assert.AreEqual(mapping.First(), resolved);
        }

        [Test]
        public void ReturnLater()
        {
            var mapping = AmbiguousZone.MapLocal(TimeInTransition);
            Assert.AreEqual(2, mapping.Count);
            var resolved = Resolvers.ReturnLater(mapping.First(), mapping.Last());
            Assert.AreEqual(mapping.Last(), resolved);
        }

        [Test]
        public void ThrowWhenAmbiguous()
        {
            var mapping = AmbiguousZone.MapLocal(TimeInTransition);
            Assert.AreEqual(2, mapping.Count);
            Assert.Throws<AmbiguousTimeException>(() => Resolvers.ThrowWhenAmbiguous(mapping.First(), mapping.Last()));
        }

        [Test]
        public void ReturnEndOfIntervalBefore()
        {
            var mapping = GapZone.MapLocal(TimeInTransition);
            Assert.AreEqual(0, mapping.Count);
            var resolved = Resolvers.ReturnEndOfIntervalBefore(TimeInTransition, GapZone, mapping.EarlyInterval, mapping.LateInterval);
            Assert.AreEqual(GapZone.EarlyInterval.End - Duration.Epsilon, resolved.ToInstant());
            Assert.AreEqual(GapZone, resolved.Zone);
        }

        [Test]
        public void ReturnStartOfIntervalAfter()
        {
            var mapping = GapZone.MapLocal(TimeInTransition);
            Assert.AreEqual(0, mapping.Count);
            var resolved = Resolvers.ReturnStartOfIntervalAfter(TimeInTransition, GapZone, mapping.EarlyInterval, mapping.LateInterval);
            Assert.AreEqual(GapZone.LateInterval.Start, resolved.ToInstant());
            Assert.AreEqual(GapZone, resolved.Zone);
        }

        [Test]
        public void ThrowWhenSkipped()
        {
            var mapping = GapZone.MapLocal(TimeInTransition);
            Assert.AreEqual(0, mapping.Count);
            Assert.Throws<SkippedTimeException>(() => Resolvers.ThrowWhenSkipped(TimeInTransition, GapZone, mapping.EarlyInterval, mapping.LateInterval));
        }

        [Test]
        public void CreateResolver_Unambiguous()
        {
            AmbiguousTimeResolver ambiguityResolver = (earlier, later) => { Assert.Fail("Shouldn't be called"); return default(ZonedDateTime); };
            SkippedTimeResolver skippedTimeResolver = (local, zone, before, after) => { Assert.Fail("Shouldn't be called"); return default(ZonedDateTime); };
            var resolver = Resolvers.CreateMappingResolver(ambiguityResolver, skippedTimeResolver);

            LocalDateTime localTime = new LocalDateTime(1900, 1, 1, 0, 0);
            var resolved = resolver(GapZone.MapLocal(localTime));
            Assert.AreEqual(new ZonedDateTime(localTime, GapZone.EarlyInterval.WallOffset, GapZone), resolved);
        }

        [Test]
        public void CreateResolver_Ambiguous()
        {
            ZonedDateTime zoned = new ZonedDateTime(TimeInTransition.PlusDays(1), GapZone.EarlyInterval.WallOffset, GapZone);
            AmbiguousTimeResolver ambiguityResolver = (earlier, later) => zoned;
            SkippedTimeResolver skippedTimeResolver = (local, zone, before, after) => { Assert.Fail("Shouldn't be called"); return default(ZonedDateTime); };
            var resolver = Resolvers.CreateMappingResolver(ambiguityResolver, skippedTimeResolver);

            var resolved = resolver(AmbiguousZone.MapLocal(TimeInTransition));
            Assert.AreEqual(zoned, resolved);
        }

        [Test]
        public void CreateResolver_Skipped()
        {
            ZonedDateTime zoned = new ZonedDateTime(TimeInTransition.PlusDays(1), GapZone.EarlyInterval.WallOffset, GapZone);
            AmbiguousTimeResolver ambiguityResolver = (earlier, later) => { Assert.Fail("Shouldn't be called"); return default(ZonedDateTime); };
            SkippedTimeResolver skippedTimeResolver = (local, zone, before, after) => zoned;
            var resolver = Resolvers.CreateMappingResolver(ambiguityResolver, skippedTimeResolver);

            var resolved = resolver(GapZone.MapLocal(TimeInTransition));
            Assert.AreEqual(zoned, resolved);
        }
    }
}
