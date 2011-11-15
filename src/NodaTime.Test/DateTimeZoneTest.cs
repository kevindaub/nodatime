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

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NodaTime.TimeZones;

namespace NodaTime.Test
{
    /// <summary>
    /// Tests for code in DateTimeZone and code which will be moving out
    /// of DateTimeZones into DateTimeZone over time.
    /// </summary>
    [TestFixture]
    public class DateTimeZoneTest
    {
        [SetUp]
        public void Setup()
        {
            DateTimeZone.SetUtcOnly(false); // Side-effect of reseting the cache
        }

        [TearDown]
        public void TearDown()
        {
            DateTimeZone.SetUtcOnly(false); // Side-effect of reseting the cache
        }

        [Test(Description = "Test for issue 7 in bug tracker")]
        public void IterateOverIds()
        {
            // According to bug, this would go bang
            int count = DateTimeZone.Ids.Count();

            Assert.IsTrue(count > 1);
            int utcCount = DateTimeZone.Ids.Count(id => id == DateTimeZone.UtcId);
            Assert.AreEqual(1, utcCount);
        }

        [Test]
        public void UtcIsNotNull()
        {
            Assert.IsNotNull(DateTimeZone.Utc);
        }

        [Test]
        public void SystemDefaultIsNotNull()
        {
            Assert.IsNotNull(DateTimeZone.SystemDefault);
        }

        [Test]
        public void TestForId_nullId()
        {
            Assert.IsNull(DateTimeZone.ForId(null));
        }

        [Test]
        public void TestForId_UtcId()
        {
            Assert.AreEqual(DateTimeZone.Utc, DateTimeZone.ForId(DateTimeZone.UtcId));
        }

        [Test]
        public void TestForId_InvalidId()
        {
            Assert.IsNull(DateTimeZone.ForId("not a known id"));
        }

        [Test]
        public void TestForId_AmericaLosAngeles()
        {
            const string americaLosAngeles = "America/Los_Angeles";
            var actual = DateTimeZone.ForId(americaLosAngeles);
            Assert.IsNotNull(actual);
            Assert.AreNotEqual(DateTimeZone.Utc, actual);
            Assert.AreEqual(americaLosAngeles, actual.Id);
        }

        [Test]
        public void TestIds_UtcOnly()
        {
            DateTimeZone.SetUtcOnly(true); // Side-effect of reseting the cache
            var actual = DateTimeZone.Ids;
            Assert.AreEqual(1, actual.Count());
            Assert.AreEqual(DateTimeZone.UtcId, actual.First());
        }

        [Test]
        public void TestIds_All()
        {
            var actual = DateTimeZone.Ids;
            var actualCount = actual.Count();
            Assert.IsTrue(actualCount > 1);
            var utc = actual.Single(id => id == DateTimeZone.UtcId);
            Assert.AreEqual(DateTimeZone.UtcId, utc);
        }

        [Test]
        public void TestAddProvider_once()
        {
            DateTimeZone.SetUtcOnly(true); // Side-effect of reseting the cache
            var provider = new TestProvider();
            DateTimeZone.AddProvider(provider);
            ExcerciseProvider(provider);
        }

        [Test]
        public void TestAddProvider_twice()
        {
            DateTimeZone.SetUtcOnly(true); // Side-effect of reseting the cache
            var provider = new TestProvider();
            DateTimeZone.AddProvider(provider);
            DateTimeZone.AddProvider(provider);
            ExcerciseProvider(provider);
        }

        [Test]
        public void TestRemoveProvider_DefaultNotPresent()
        {
            DateTimeZone.SetUtcOnly(true); // Side-effect of reseting the cache
            Assert.IsFalse(DateTimeZone.RemoveProvider(DateTimeZone.DefaultDateTimeZoneProvider));
        }

        [Test]
        public void TestRemoveProvider_Default()
        {
            Assert.IsTrue(DateTimeZone.RemoveProvider(DateTimeZone.DefaultDateTimeZoneProvider));
            Assert.IsFalse(DateTimeZone.RemoveProvider(DateTimeZone.DefaultDateTimeZoneProvider));
        }

        /// <summary>
        /// Simply tests that every ID in the built-in database can be fetched. This is also
        /// helpful for diagnostic debugging when we want to check that some potential
        /// invariant holds for all time zones...
        /// </summary>
        [Test]
        public void TestForId_AllIds()
        {
            foreach (string id in DateTimeZone.Ids)
            {
                Assert.IsNotNull(DateTimeZone.ForId(id));
            }
        }

        [Test]
        public void TestMapLocalDateTime_UnambiguousDateReturnsUnambiguousMapping()
        {
            const ZoneLocalMapping.ResultType expected = ZoneLocalMapping.ResultType.Unambiguous;

            var unambigiousTime = new LocalDateTime(2011, 11, 9, 1, 30); //2011-11-09 01:30:00 - not ambiguous in America/New York timezone
            var target = BuildAmericaNewYorkTimeZone();
            var actual = target.MapLocalDateTime(unambigiousTime).Type;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestMapLocalDateTime_AmbiguousDateReturnsAmbigousMapping()
        {
            const ZoneLocalMapping.ResultType expected = ZoneLocalMapping.ResultType.Ambiguous;

            var ambiguousTime = new LocalDateTime(2011, 11, 6, 1, 30); //2011-11-06 01:30:00 - falls during DST - EST conversion in America/New York timezone
            var target = BuildAmericaNewYorkTimeZone();
            var actual = target.MapLocalDateTime(ambiguousTime).Type;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestMapLocalDateTime_SkippedDateReturnsSkippedMapping()
        {
            const ZoneLocalMapping.ResultType expected = ZoneLocalMapping.ResultType.Skipped;

            var skippedTime = new LocalDateTime(2011, 3, 13, 2, 30); //2011-03-13 02:30:00 - falls during EST - DST conversion in America/New York timezone
            var target = BuildAmericaNewYorkTimeZone();
            var actual = target.MapLocalDateTime(skippedTime).Type;

            Assert.AreEqual(expected, actual);
        }

        private DateTimeZone BuildAmericaNewYorkTimeZone()
        {
            const string americaNewYork = "America/New York";
            return DateTimeZone.ForId(americaNewYork);
        }

        private static void ExcerciseProvider(TestProvider provider)
        {
            var ids = DateTimeZone.Ids;
            Assert.AreEqual(1, ids.Count());
            Assert.AreEqual(1, provider.Calls.Count);
            Assert.AreEqual("Ids", provider.Calls[0]);
            var unknown = DateTimeZone.ForId("an unknown id");
            Assert.IsNull(unknown);
            Assert.AreEqual(2, provider.Calls.Count);
            Assert.AreEqual("ForId(an unknown id)", provider.Calls[1]);
        }

        /// <summary>
        /// Time zone provider which doesn't know about any zones, and remembers calls made
        /// to it. (We could use a mocking framework as an alternative, if we need it elsewhere.)
        /// </summary>
        private class TestProvider : IDateTimeZoneProvider
        {
            private readonly List<string> calls = new List<string>();
            private readonly string[] list = new string[0];

            public IEnumerable<string> Ids
            {
                get
                {
                    calls.Add("Ids");
                    return list;
                }
            }

            public DateTimeZone ForId(string id)
            {
                calls.Add("ForId(" + id + ")");
                return null;
            }

            public List<String> Calls { get { return calls; } }
        }
    }
}
