// Copyright 2012 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using NodaTime.TimeZones;

namespace NodaTime.Test.TimeZones
{
    [TestFixture]
    public class TzdbDateTimeZoneSourceTest
    {
        [Test]
        [TestCase("UTC", "Etc/GMT")]
        [TestCase("GMT Standard Time", "Europe/London")]
        // Standard name differs from ID under Windows
        [TestCase("Israel Standard Time", "Asia/Jerusalem")]
        public void ZoneMapping(string bclId, string tzdbId)
        {
            try
            {
                var source = TzdbDateTimeZoneSource.Default;
                var bclZone = TimeZoneInfo.FindSystemTimeZoneById(bclId);
                Assert.AreEqual(tzdbId, source.MapTimeZoneId(bclZone));
            }
            catch (TimeZoneNotFoundException)
            {
                // This may occur on Mono, for example.
                Assert.Ignore("Test assumes existence of BCL zone with ID: " + bclId);
            }
        }

#if !PCL
        /// <summary>
        /// Tests that we can load (and exercise) the binary Tzdb resource file distributed with Noda Time 1.0.0.
        /// This is effectively a black-box regression test that ensures that the resource format has not changed in a
        /// way such that a custom resource file compiled with ZoneInfoCompiler from 1.0 would become unreadable.
        /// </summary>
        [Test]
        public void CanLoadNodaTimeResourceFromOnePointZeroRelease()
        {
#pragma warning disable 0618
            var source = new TzdbDateTimeZoneSource("NodaTime.Test.TestData.Tzdb2012iFromNodaTime1.0",
                Assembly.GetExecutingAssembly());
#pragma warning restore 0618
            Assert.AreEqual("TZDB: 2012i (mapping: 6356)", source.VersionId);

            var utc = Instant.FromUtc(2007, 8, 24, 9, 30, 0);

            // Test a regular zone with rules.
            var london = source.ForId("Europe/London");
            var inLondon = new ZonedDateTime(utc, london);
            var expectedLocal = new LocalDateTime(2007, 8, 24, 10, 30);
            Assert.AreEqual(expectedLocal, inLondon.LocalDateTime);

            // Test a fixed-offset zone.
            var utcFixed = source.ForId("Etc/UTC");
            var inUtcFixed = new ZonedDateTime(utc, utcFixed);
            expectedLocal = new LocalDateTime(2007, 8, 24, 9, 30);
            Assert.AreEqual(expectedLocal, inUtcFixed.LocalDateTime);

            // Test an alias.
            var jersey = source.ForId("Japan");  // Asia/Tokyo
            var inJersey = new ZonedDateTime(utc, jersey);
            expectedLocal = new LocalDateTime(2007, 8, 24, 18, 30);
            Assert.AreEqual(expectedLocal, inJersey.LocalDateTime);

            // Can't ask for GeoLocations
            Assert.Throws<InvalidOperationException>(() => source.GeoLocations.GetHashCode());
        }
#endif

        /// <summary>
        /// Simply tests that every ID in the built-in database can be fetched. This is also
        /// helpful for diagnostic debugging when we want to check that some potential
        /// invariant holds for all time zones...
        /// </summary>
        [Test]
        public void ForId_AllIds()
        {
            var source = TzdbDateTimeZoneSource.Default;
            foreach (string id in source.GetIds())
            {
                Assert.IsNotNull(source.ForId(id));
            }
        }

        [Test]
        public void UtcEqualsBuiltIn()
        {
            var zone = TzdbDateTimeZoneSource.Default.ForId("UTC");
            Assert.AreEqual(DateTimeZone.Utc, zone);
        }

        // The following tests all make assumptions about the built-in TZDB data.
        // This is simpler than constructing fake data, and validates that the creation
        // mechanism matches the reading mechanism, too.
        [Test]
        public void Aliases()
        {
            var aliases = TzdbDateTimeZoneSource.Default.Aliases;
            CollectionAssert.AreEqual(new[] { "Europe/Belfast", "Europe/Guernsey", "Europe/Isle_of_Man", "Europe/Jersey", "GB", "GB-Eire" },
                                      aliases["Europe/London"].ToArray()); // ToArray call makes diagnostics more useful
            CollectionAssert.IsOrdered(aliases["Europe/London"]);
            CollectionAssert.IsEmpty(aliases["Europe/Jersey"]);
        }

        [Test]
        public void CanonicalIdMap_Contents()
        {
            var map = TzdbDateTimeZoneSource.Default.CanonicalIdMap;
            Assert.AreEqual("Europe/London", map["Europe/Jersey"]);
            Assert.AreEqual("Europe/London", map["Europe/London"]);
        }

        [Test]
        public void CanonicalIdMap_IsReadOnly()
        {
            var map = TzdbDateTimeZoneSource.Default.CanonicalIdMap;
            Assert.Throws<NotSupportedException>(() => map.Add("Foo", "Bar"));
        }

        // Sample geolocation checks to ensure we've serialized and deserialized correctly
        // Input line: FR	+4852+00220	Europe/Paris
        [Test]
        public void GeoLocations_ContainsFrance()
        {
            var geoLocations = TzdbDateTimeZoneSource.Default.GeoLocations;
            var france = geoLocations.Single(g => g.CountryName == "France");
            // Tolerance of about 2 seconds
            Assert.AreEqual(48.86666, france.Latitude, 0.00055);
            Assert.AreEqual(2.3333, france.Longitude, 0.00055);
            Assert.AreEqual("Europe/Paris", france.ZoneId);
            Assert.AreEqual("FR", france.CountryCode);
            Assert.AreEqual("", france.Comment);
        }

        // Input line: CA	+744144-0944945	America/Resolute	Central Standard Time - Resolute, Nunavut
        [Test]
        public void GeoLocations_ContainsResolute()
        {
            var geoLocations = TzdbDateTimeZoneSource.Default.GeoLocations;
            var resolute = geoLocations.Single(g => g.ZoneId == "America/Resolute");
            // Tolerance of about 2 seconds
            Assert.AreEqual(74.69555, resolute.Latitude, 0.00055);
            Assert.AreEqual(-94.82916, resolute.Longitude, 0.00055);
            Assert.AreEqual("Canada", resolute.CountryName);
            Assert.AreEqual("CA", resolute.CountryCode);
            Assert.AreEqual("Central Standard Time - Resolute, Nunavut", resolute.Comment);
        }

        [Test]
        public void TzdbVersion()
        {
            var source = TzdbDateTimeZoneSource.Default;
            StringAssert.StartsWith("201", source.TzdbVersion);
        }

        [Test]
        public void VersionId()
        {
            var source = TzdbDateTimeZoneSource.Default;
            StringAssert.StartsWith("TZDB: " + source.TzdbVersion, source.VersionId);
        }
    }
}
