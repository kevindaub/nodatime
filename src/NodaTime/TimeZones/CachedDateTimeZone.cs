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

namespace NodaTime.TimeZones
{
    /// <summary>
    ///  Provides a <see cref="DateTimeZone"/> wrapper class that implements a simple cache to
    ///  speed up the lookup of transitions.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The cache supports multiple caching strategies which are implemented in nested subclasses of
    /// this one. Until we have a better sense of what the usage behavior is, we cannot tune the
    /// cache. It is possible that we may support multiple strategies selectable at runtime so the
    /// user can tune the performance based on their knowledge of how they are using the system.
    /// </para>
    /// <para>
    /// In fact, only one cache type is currently implemented: an MRU cache existed before
    /// the GetZoneIntervals call was created in DateTimeZone, but as it wasn't being used, it
    /// was more effort than it was worth to update. The mechanism is still available for future
    /// expansion though.
    /// </para>
    /// </remarks>
    internal abstract class CachedDateTimeZone : DateTimeZone
    {
        #region CacheType enum
        internal enum CacheType
        {
            Hashtable
        }
        #endregion

        private readonly DateTimeZone timeZone;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedDateTimeZone"/> class.
        /// </summary>
        /// <param name="timeZone">The time zone to cache.</param>
        private CachedDateTimeZone(DateTimeZone timeZone) : base(timeZone.Id, false, timeZone.MinOffset, timeZone.MaxOffset)
        {
            this.timeZone = timeZone;
        }

        /// <summary>
        /// Gets the cached time zone.
        /// </summary>
        /// <value>The time zone.</value>
        internal DateTimeZone TimeZone { get { return timeZone; } }

        /// <summary>
        /// Returns a cached time zone for the given time zone.
        /// </summary>
        /// <remarks>
        /// If the time zone is already cached or it is fixed then it is returned unchanged.
        /// </remarks>
        /// <param name="timeZone">The time zone to cache.</param>
        /// <returns>The cached time zone.</returns>
        internal static DateTimeZone ForZone(DateTimeZone timeZone)
        {
            return ForZone(timeZone, CacheType.Hashtable);
        }

        /// <summary>
        /// Returns a cached time zone for the given time zone.
        /// </summary>
        /// <remarks>
        /// If the time zone is already cached or it is fixed then it is returned unchanged.
        /// </remarks>
        /// <param name="timeZone">The time zone to cache.</param>
        /// <param name="type">The type of cache to store the zone in.</param>
        /// <returns>The cached time zone.</returns>
        private static DateTimeZone ForZone(DateTimeZone timeZone, CacheType type)
        {
            if (timeZone == null)
            {
                throw new ArgumentNullException("timeZone");
            }
            if (timeZone is CachedDateTimeZone || timeZone.IsFixed)
            {
                return timeZone;
            }
            switch (type)
            {
                case CacheType.Hashtable:
                    return new HashArrayCache(timeZone);
                default:
                    throw new ArgumentException("The type parameter is invalid", "type");
            }
        }

        #region Overrides of DateTimeZone
        /// <summary>
        /// Writes the time zone to the specified writer.
        /// </summary>
        /// <param name="writer">The writer to write to.</param>
        internal override void Write(DateTimeZoneWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            writer.WriteTimeZone(timeZone);
        }

        /// <summary>
        /// Reads the specified reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        internal static DateTimeZone Read(DateTimeZoneReader reader, string id)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            var timeZone = reader.ReadTimeZone(id);
            return ForZone(timeZone);
        }
        #endregion

        #region Nested type: HashArrayCache
        /// <summary>
        /// This provides a simple cache based on two hash tables (one for local instants, another
        /// for instants).
        /// </summary>
        /// <remarks>
        /// Each hash table entry is either entry or contains a node with enough
        /// information for a particular "period" of about 40 days - so multiple calls for time
        /// zone information within the same few years are likely to hit the cache. Note that
        /// a single "period" may include a daylight saving change (or conceivably more than one);
        /// a node therefore has to contain enough intervals to completely represent that period.
        /// 
        /// If another call is made which maps to the same cache entry number but is for a different
        /// period, the existing hash entry is simply overridden.
        /// </remarks>
        private class HashArrayCache : CachedDateTimeZone
        {
            // Currently we have no need or way to create hash cache zones with
            // different cache sizes. But the cache size should always be a power of 2 to get the
            // "period to cache entry" conversion simply as a bitmask operation.
            private const int CacheSize = 512;
            // Mask to AND the period number with in order to get the cache entry index. The
            // result will always be in the range [0, CacheSize).
            private const int CachePeriodMask = CacheSize - 1;

            /// <summary>
            /// Defines the number of bits to shift an instant value to get the period. This
            /// converts a number of ticks to a number of 40.6 days periods.
            /// </summary>
            private const int PeriodShift = 45;

            // Mask to "or" a shifted period value with in order to get the end point.
            // In other words:
            // We shift a number of ticks *right* by PeriodShift to get the period number.
            // We shift the period number *left* by PeriodShift to get the start of the period.
            // We shift the period number *left* by PeriodShift and OR with PeriodEndMask to get
            // the end of the period (inclusive). An alternative would be to increment the period
            // and just shift left, to get the period end in an *exclusive* form.
            private const long PeriodEndMask = (1L << PeriodShift) - 1;

            private readonly HashCacheNode[] instantCache;
            private readonly HashCacheNode[] localInstantCache;

            /// <summary>
            /// Initializes a new instance of the <see cref="CachedDateTimeZone"/> class.
            /// </summary>
            /// <param name="timeZone">The time zone to cache.</param>
            internal HashArrayCache(DateTimeZone timeZone) : base(timeZone)
            {
                if (timeZone == null)
                {
                    throw new ArgumentNullException("timeZone");
                }
                instantCache = new HashCacheNode[CacheSize];
                localInstantCache = new HashCacheNode[CacheSize];
            }

            #region Overrides of DateTimeZone
            /// <summary>
            /// Gets the zone offset period for the given instant. Null is returned if no period is
            /// defined by the time zone for the given instant.
            /// </summary>
            /// <param name="instant">The Instant to test.</param>
            /// <returns>The defined ZoneOffsetPeriod or <c>null</c>.</returns>
            public override ZoneInterval GetZoneInterval(Instant instant)
            {
                int period = (int)(instant.Ticks >> PeriodShift);
                int index = period & CachePeriodMask;
                var node = instantCache[index];
                if (node == null || node.Period != period)
                {
                    node = CreateInstantNode(period);
                    instantCache[index] = node;
                }
                // TODO: Why doesn't this need to check for node being null,
                // as we do in the LocalInterval version?
                while (node.Interval.Start > instant)
                {
                    node = node.Previous;
                }
                return node.Interval;
            }

            /// <summary>
            /// Gets the zone offset period for the given local instant. Null is returned if no period
            /// is defined by the time zone for the given local instant.
            /// </summary>
            /// <param name="localInstant">The LocalInstant to test.</param>
            /// <returns>The defined ZoneOffsetPeriod or <c>null</c>.</returns>
            internal override ZoneInterval GetZoneInterval(LocalInstant localInstant)
            {
                int period = (int)(localInstant.Ticks >> PeriodShift);
                int index = period & CachePeriodMask;
                var node = localInstantCache[index];
                if (node == null || node.Period != period)
                {
                    node = CreateLocalInstantNode(period);
                    localInstantCache[index] = node;
                }
                while (node != null && node.Interval.LocalStart > localInstant)
                {
                    node = node.Previous;
                }
                if (node == null || node.Interval.LocalEnd <= localInstant)
                {
                    throw new SkippedTimeException(localInstant, TimeZone);
                }
                return node.Interval;
            }

            internal override ZoneIntervalPair GetZoneIntervals(LocalInstant localInstant)
            {
                throw new NotImplementedException();
            }
            #endregion

            /// <summary>
            /// Creates a hash table node with all the information for this period.
            /// We start off by finding the interval for the start of the period, and
            /// then repeatedly check whether that interval ends after the end of the
            /// period - at which point we're done. If not, find the next interval, create
            /// a new node referring to that interval and the previous interval, and keep going.
            /// 
            /// TODO: Simplify this significantly. There's no need to have anything more than
            /// a single node with a list or array of intervals. In most cases it'll only be one
            /// or two intervals anyway.
            /// </summary>
            private HashCacheNode CreateInstantNode(int period)
            {
                var periodStart = new Instant((long)period << PeriodShift);
                var interval = TimeZone.GetZoneInterval(periodStart);
                var node = new HashCacheNode(interval, period, null);
                var periodEnd = new Instant(periodStart.Ticks | PeriodEndMask);
                while (true)
                {
                    periodStart = node.Interval.End;
                    if (periodStart > periodEnd)
                    {
                        break;
                    }
                    interval = TimeZone.GetZoneInterval(periodStart);
                    node = new HashCacheNode(interval, period, node);
                }

                return node;
            }

            /// <summary>
            /// See CreateInstantNode - this is the equivalent, but for local instants.
            /// 
            /// TODO: Local instants are actually conceivably trickier due to ambiguity
            /// etc. I have a plan for fixing this, but again making this just a list
            /// of intervals which occur at any point in the period would make life easier.
            /// </summary>
            private HashCacheNode CreateLocalInstantNode(int period)
            {
                var periodStart = new LocalInstant((long)period << PeriodShift);
                var interval = TimeZone.GetZoneInterval(periodStart);
                var node = new HashCacheNode(interval, period, null);
                var periodEnd = new LocalInstant(periodStart.Ticks | PeriodEndMask);
                while (true)
                {
                    periodStart = node.Interval.LocalEnd;
                    if (periodStart > periodEnd)
                    {
                        break;
                    }
                    try
                    {
                        interval = TimeZone.GetZoneInterval(periodStart);
                    }
                    catch (SkippedTimeException)
                    {
                        interval = TimeZone.GetZoneInterval(node.Interval.End);
                    }
                    node = new HashCacheNode(interval, period, node);
                }

                return node;
            }

            #region Nested type: HashCacheNode
            /// <summary>
            /// See CreateInstantNode for an explanation.
            /// </summary>
            private class HashCacheNode
            {
                private readonly ZoneInterval interval;
                private readonly int period;
                private readonly HashCacheNode previous;

                /// <summary>
                /// Initializes a new instance of the <see cref="HashCacheNode"/> class.
                /// </summary>
                /// <param name="interval">The zone interval.</param>
                /// <param name="period"></param>
                /// <param name="previous">The previous <see cref="HashCacheNode"/> node.</param>
                internal HashCacheNode(ZoneInterval interval, int period, HashCacheNode previous)
                {
                    this.period = period;
                    this.interval = interval;
                    this.previous = previous;
                }

                internal int Period { get { return period; } }

                internal ZoneInterval Interval { get { return interval; } }

                internal HashCacheNode Previous { get { return previous; } }
            }
            #endregion
        }
        #endregion
    }
}