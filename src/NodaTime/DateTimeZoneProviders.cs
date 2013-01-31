// Copyright 2012 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using NodaTime.TimeZones;

namespace NodaTime
{
    /// <summary>
    /// Static access to date/time zone providers built into Noda Time. These are all thread-safe and caching.
    /// </summary>
    public static class DateTimeZoneProviders
    {
        private static readonly DateTimeZoneCache tzdbFactory = new DateTimeZoneCache(TzdbDateTimeZoneSource.Default);
        /// <summary>
        /// Gets a time zone provider which uses a <see cref="TzdbDateTimeZoneSource"/>.
        /// The underlying source is <see cref="TzdbDateTimeZoneSource.Default"/>, which is initialized from
        /// resources within the NodaTime assembly.
        /// </summary>
        public static IDateTimeZoneProvider Tzdb { get { return tzdbFactory; } }

#if !PCL
        /// <summary>
        /// Gets the TZDB time zone provider.
        /// This always returns the same value as the <see cref="Tzdb"/> property.
        /// </summary>
        /// <remarks>This method is not available in the PCL version, as it was made obsolete in Noda Time 1.1.</remarks>
        /// <seealso cref="Tzdb"/>
        [Obsolete("Use DateTimeZoneProviders.Tzdb instead")]
        public static IDateTimeZoneProvider Default { get { return Tzdb; } }

        private static readonly DateTimeZoneCache bclFactory = new DateTimeZoneCache(new BclDateTimeZoneSource());

        /// <summary>
        /// Gets a time zone provider which uses a <see cref="BclDateTimeZoneSource"/>.
        /// This property is not available on the PCL build of Noda Time.
        /// </summary>
        public static IDateTimeZoneProvider Bcl { get { return bclFactory; } }
#endif
    }
}
