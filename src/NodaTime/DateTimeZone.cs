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
using NodaTime.TimeZones;

namespace NodaTime
{
    /// <summary>
    /// Interface describing a time zone.
    /// </summary>
    /// <remarks>
    /// Time zones primarily encapsulate two facts: and offset from UTC and a set of rules on how
    /// the values are adjusted.
    /// </remarks>
    public abstract class DateTimeZone
    {
        private readonly string id;
        private readonly bool isFixed;

        /// <summary>
        /// This is the ID of the UTC (Coordinated Univeral Time) time zone.
        /// </summary>
        public const string UtcId = "UTC";

        private static readonly DateTimeZone UtcZone = new FixedDateTimeZone(Offset.Zero);

        /// <summary>
        /// Gets the UTC (Coordinated Univeral Time) time zone.
        /// </summary>
        /// <value>The UTC <see cref="DateTimeZone"/>.</value>
        public static DateTimeZone Utc { get { return UtcZone; } }

        /// <summary>
        /// Gets the system default time zone which can only be changed by the system.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The time zones defined in the operating system are different than the ones defines in
        /// this library so a mapping will occur. If an exact mapping can be made then that will be
        /// used otherwise UTC will be used.
        /// </para>
        /// </remarks>
        /// <value>The system default <see cref="DateTimeZone"/>. this will never be <c>null</c>.</value>
        public static DateTimeZone SystemDefault
        {
            get
            {
                var systemName = TimeZone.CurrentTimeZone.StandardName;
                var timeZoneId = WindowsToPosixResource.GetIdFromWindowsName(systemName);
                if (timeZoneId == null)
                {
                    timeZoneId = UtcId;
                }
                return DateTimeZones.ForId(timeZoneId) ?? Utc;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeZone"/> class.
        /// </summary>
        /// <param name="id">The unique id of this time zone.</param>
        /// <param name="isFixed">Set to <c>true</c> if this time zone has no transitions.</param>
        protected DateTimeZone(string id, bool isFixed)
        {
            this.id = id;
            this.isFixed = isFixed;
        }

        /// <summary>
        /// Returns a new chronology based on this time zone, in the ISO calendar system.
        /// </summary>
        internal Chronology ToIsoChronology()
        {
            return new Chronology(this, CalendarSystem.Iso);
        }

        /// <summary>
        /// The database ID for the time zone.
        /// </summary>
        public string Id { get { return id; } }

        /// <summary>
        /// Indicates whether the time zone is fixed, i.e. contains no transitions.
        /// </summary>
        public bool IsFixed { get { return isFixed; } }

        /// <summary>
        /// Returns the offset from UTC, where a positive duration indicates that local time is
        /// later than UTC. In other words, local time = UTC + offset.
        /// </summary>
        /// <param name="instant">The instant for which to calculate the offset.</param>
        /// <returns>
        /// The offset from UTC at the specified instant.
        /// </returns>
        public virtual Offset GetOffsetFromUtc(Instant instant)
        {
            var period = GetZoneInterval(instant);
            return period.Offset;
        }

        /// <summary>
        /// Gets the offset to subtract from a local time to get the UTC time.
        /// </summary>
        /// <param name="localInstant">The local instant to get the offset of.</param>
        /// <returns>The offset to subtract from the specified local time to obtain a UTC instant.</returns>
        /// <remarks>
        /// Around a DST transition, local times behave peculiarly. When the time springs forward,
        /// (e.g. 12:59 to 02:00) some times never occur; when the time falls back (e.g. 1:59 to
        /// 01:00) some times occur twice. This method always returns a smaller offset when there is
        /// ambiguity, i.e. it treats the local time as the later of the possibilities.
        /// </remarks>
        /// <exception cref="SkippedTimeException">The local instant doesn't occur in this time zone
        /// due to zone transitions.</exception>
        internal virtual Offset GetOffsetFromLocal(LocalInstant localInstant)
        {
            var period = GetZoneInterval(localInstant);
            return period.Offset;
        }

        /// <summary>
        /// Gets the zone interval for the given instant. Null is returned if no interval is
        /// defined by the time zone for the given instant.
        /// </summary>
        /// <param name="instant">The Instant to query.</param>
        /// <returns>The defined <see cref="ZoneInterval"/> or <c>null</c>.</returns>
        public abstract ZoneInterval GetZoneInterval(Instant instant);

        /// <summary>
        /// Gets the zone interval for the given local instant. Null is returned if no interval is
        /// defined by the time zone for the given local instant.
        /// </summary>
        /// <param name="localInstant">The <see cref="LocalInstant"/> to query.</param>
        /// <returns>The defined <see cref="ZoneInterval"/> or <c>null</c>.</returns>
        internal abstract ZoneInterval GetZoneInterval(LocalInstant localInstant);

        /// <summary>
        /// Returns the name associated with the given instant.
        /// </summary>
        /// <remarks>
        /// For a fixed time zone this will always return the same value but for a time zone that
        /// honors daylight savings this will return a different name depending on the time of year
        /// it represents. For example in the Pacific Standard Time (UTC-8) it will return either
        /// PST or PDT depending on the time of year.
        /// </remarks>
        /// <param name="instant">The instant to get the name for.</param>
        /// <returns>The name of this time. Never returns <c>null</c>.</returns>
        public virtual string GetName(Instant instant)
        {
            var period = GetZoneInterval(instant);
            return period.Name;
        }

        /// <summary>
        /// Writes the time zone to the specified writer.
        /// </summary>
        /// <param name="writer">The writer to write to.</param>
        internal abstract void Write(DateTimeZoneWriter writer);

        #region Object overrides
        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Id;
        }
        #endregion
    }
}