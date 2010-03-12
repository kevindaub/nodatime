﻿#region Copyright and license information

// Copyright 2001-2010 Stephen Colebourne
// Copyright 2010 Jon Skeet
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
using NodaTime.Utility;

namespace NodaTime
{
    /// <summary>
    /// A date and time with an associated chronology - or to look at it a different way, a
    /// LocalDateTime plus a time zone.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Some seemingly valid values do not represent a valid instant due to the local time moving
    /// forward during a daylight saving transition, thus "skipping" the given value. Other values
    /// occur twice (due to local time moving backward), in which case a ZonedDateTime will always
    /// represent the later of the two possible times, when converting it to an instant.
    /// </para>
    /// <para>
    /// A value constructed with "new ZonedDateTime()" will represent the Unix epoch in the ISO
    /// calendar system in the UTC time zone. That is the only situation in which the chronology is
    /// assumed rather than specified.
    /// </para>
    /// </remarks>
    public struct ZonedDateTime
        : IEquatable<ZonedDateTime>
    {
        private readonly Chronology chronology;
        private readonly LocalInstant localInstant;
        private readonly Offset offset;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZonedDateTime"/> struct.
        /// </summary>
        /// <param name="localDateTime">The local date time.</param>
        /// <param name="zone">The zone.</param>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="zone"/> is <c>null</c>.</exception>
        public ZonedDateTime(LocalDateTime localDateTime, IDateTimeZone zone)
        {
            if (zone == null)
            {
                throw new ArgumentNullException("zone");
            }
            this.localInstant = localDateTime.LocalInstant;
            this.offset = zone.GetOffsetFromLocal(localInstant);
            this.chronology = new Chronology(zone, localDateTime.Calendar);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZonedDateTime"/> struct.
        /// </summary>
        /// <param name="instant">The instant.</param>
        /// <param name="chronology">The chronology.</param>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="chronology"/> is <c>null</c>.</exception>
        public ZonedDateTime(Instant instant, Chronology chronology)
        {
            if (chronology == null)
            {
                throw new ArgumentNullException("chronology");
            }
            this.offset = chronology.Zone.GetOffsetFromUtc(instant);
            this.localInstant = instant + offset;
            this.chronology = chronology;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZonedDateTime"/> struct.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <param name="month">The month.</param>
        /// <param name="day">The day.</param>
        /// <param name="hour">The hour.</param>
        /// <param name="minute">The minute.</param>
        /// <param name="second">The second.</param>
        /// <param name="zone">The zone.</param>
        public ZonedDateTime(int year, int month, int day, int hour, int minute, int second, IDateTimeZone zone)
            : this(year, month, day, hour, minute, second, Chronology.IsoForZone(zone))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZonedDateTime"/> struct.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <param name="month">The month.</param>
        /// <param name="day">The day.</param>
        /// <param name="hour">The hour.</param>
        /// <param name="minute">The minute.</param>
        /// <param name="second">The second.</param>
        /// <param name="chronology">The chronology.</param>
        public ZonedDateTime(int year, int month, int day, int hour, int minute, int second, Chronology chronology)
            : this(year, month, day, hour, minute, second, 0, 0, chronology)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZonedDateTime"/> struct.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <param name="month">The month.</param>
        /// <param name="day">The day.</param>
        /// <param name="hour">The hour.</param>
        /// <param name="minute">The minute.</param>
        /// <param name="second">The second.</param>
        /// <param name="millisecond">The millisecond.</param>
        /// <param name="zone">The zone.</param>
        public ZonedDateTime(int year, int month, int day, int hour, int minute, int second, int millisecond,
                             IDateTimeZone zone)
            : this(year, month, day, hour, minute, second, millisecond, Chronology.IsoForZone(zone))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZonedDateTime"/> struct.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <param name="month">The month.</param>
        /// <param name="day">The day.</param>
        /// <param name="hour">The hour.</param>
        /// <param name="minute">The minute.</param>
        /// <param name="second">The second.</param>
        /// <param name="millisecond">The millisecond.</param>
        /// <param name="chronology">The chronology.</param>
        public ZonedDateTime(int year, int month, int day, int hour, int minute, int second, int millisecond,
                             Chronology chronology)
            : this(year, month, day, hour, minute, second, millisecond, 0, chronology)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZonedDateTime"/> struct.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <param name="month">The month.</param>
        /// <param name="day">The day.</param>
        /// <param name="hour">The hour.</param>
        /// <param name="minute">The minute.</param>
        /// <param name="second">The second.</param>
        /// <param name="millisecond">The millisecond.</param>
        /// <param name="tick">The tick.</param>
        /// <param name="zone">The zone.</param>
        public ZonedDateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, int tick,
                             IDateTimeZone zone)
            : this(year, month, day, hour, minute, second, millisecond, tick, Chronology.IsoForZone(zone))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZonedDateTime"/> struct.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <param name="month">The month.</param>
        /// <param name="day">The day.</param>
        /// <param name="hour">The hour.</param>
        /// <param name="minute">The minute.</param>
        /// <param name="second">The second.</param>
        /// <param name="millisecond">The millisecond.</param>
        /// <param name="tick">The tick.</param>
        /// <param name="chronology">The chronology.</param>
        public ZonedDateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, int tick,
                             Chronology chronology)
        {
            if (chronology == null)
            {
                throw new ArgumentNullException("chronology");
            }
            localInstant = chronology.Calendar.GetLocalInstant(year, month, day, hour, minute, second, millisecond, tick);
            offset = chronology.Zone.GetOffsetFromLocal(localInstant);
            this.chronology = chronology;
        }

        /// <summary>
        /// Gets the chronology.
        /// </summary>
        /// <value>The chronology.</value>
        public Chronology Chronology
        {
            get { return chronology; }
        }

        /// <summary>
        /// Gets the offset.
        /// </summary>
        /// <value>The offset.</value>
        public Offset Offset { get { return this.offset; } }

        /// <summary>
        /// Gets the zone.
        /// </summary>
        /// <value>The zone.</value>
        public IDateTimeZone Zone
        {
            get { return Chronology.Zone; }
        }

        /// <summary>
        /// Gets the local instant.
        /// </summary>
        /// <value>The local instant.</value>
        public LocalInstant LocalInstant
        {
            get { return localInstant; }
        }

        public LocalDateTime LocalDateTime
        {
            get { return new LocalDateTime(LocalInstant, chronology.Calendar); }
        }

        public int Era
        {
            get { return LocalDateTime.Era; }
        }

        public int CenturyOfEra
        {
            get { return LocalDateTime.CenturyOfEra; }
        }

        public int Year
        {
            get { return LocalDateTime.Year; }
        }

        public int YearOfCentury
        {
            get { return LocalDateTime.YearOfCentury; }
        }

        public int YearOfEra
        {
            get { return LocalDateTime.YearOfEra; }
        }

        public int WeekYear
        {
            get { return LocalDateTime.WeekYear; }
        }

        public int MonthOfYear
        {
            get { return LocalDateTime.MonthOfYear; }
        }

        public int WeekOfWeekYear
        {
            get { return LocalDateTime.WeekOfWeekYear; }
        }

        public int DayOfYear
        {
            get { return LocalDateTime.DayOfYear; }
        }

        public int DayOfMonth
        {
            get { return LocalDateTime.DayOfMonth; }
        }

        public int DayOfWeek
        {
            get { return LocalDateTime.DayOfWeek; }
        }

        public int HourOfDay
        {
            get { return LocalDateTime.HourOfDay; }
        }

        public int MinuteOfHour
        {
            get { return LocalDateTime.MinuteOfHour; }
        }

        public int SecondOfMinute
        {
            get { return LocalDateTime.SecondOfMinute; }
        }

        public int SecondOfDay
        {
            get { return LocalDateTime.SecondOfDay; }
        }

        public int MillisecondOfSecond
        {
            get { return LocalDateTime.MillisecondOfSecond; }
        }

        public int MillisecondOfDay
        {
            get { return LocalDateTime.MillisecondOfDay; }
        }

        public int TickOfMillisecond
        {
            get { return LocalDateTime.TickOfMillisecond; }
        }

        public long TickOfDay
        {
            get { return LocalDateTime.TickOfDay; }
        }

        /// <summary>
        /// Converts this value to the instant it represents on the time line.
        /// If two instants are represented by the same set of values, the later
        /// instant is returned.
        /// </summary>
        /// <remarks>
        /// Conceptually this is a conversion (which is why it's not a property) but
        /// in reality the conversion is done at the point of construction.
        /// </remarks>
        public Instant ToInstant()
        {
            return localInstant - offset;
        }

        #region Equality

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.
        ///                 </param>
        public bool Equals(ZonedDateTime other)
        {
            return LocalInstant == other.LocalInstant &&
                   Offset == other.Offset &&
                   Chronology == other.Chronology;
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <returns>
        /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        /// <param name="obj">Another object to compare to. 
        ///                 </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (obj is ZonedDateTime)
            {
                return Equals((ZonedDateTime)obj);
            }
            return false;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            int hash = HashCodeHelper.Initialize();
            hash = HashCodeHelper.Hash(hash, LocalInstant);
            hash = HashCodeHelper.Hash(hash, Offset);
            hash = HashCodeHelper.Hash(hash, Chronology);
            return hash;
        }

        #endregion

        #region Operators

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(ZonedDateTime left, ZonedDateTime right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(ZonedDateTime left, ZonedDateTime right)
        {
            return !(left == right);
        }

        #endregion
    }
}