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
using System.Globalization;
using NodaTime.TimeZones;

namespace NodaTime
{
    /// <summary>
    /// Represents a local date and time without reference to a calendar system,
    /// as the number of ticks since the Unix epoch which would represent that time
    /// of the same date in UTC. This needs a better description, and possibly a better name
    /// at some point...
    /// </summary>
    internal struct LocalInstant : IEquatable<LocalInstant>, IComparable<LocalInstant>
    {
        public static readonly LocalInstant LocalUnixEpoch = new LocalInstant(0);
        public static readonly LocalInstant MinValue = new LocalInstant(Int64.MinValue);
        public static readonly LocalInstant MaxValue = new LocalInstant(Int64.MaxValue);

        private readonly long ticks;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalInstant"/> struct.
        /// </summary>
        /// <param name="ticks">The number of ticks from the Unix Epoch.</param>
        public LocalInstant(long ticks)
        {
            this.ticks = ticks;
        }

        /// <summary>
        /// Ticks since the Unix epoch.
        /// </summary>
        public long Ticks { get { return ticks; } }

        /// <summary>
        /// Returns a new LocalInstant for the current time adjusting for the current time zone.
        /// </summary>
        /// <value>The <see cref="LocalInstant"/> of the current time.</value>
        public static LocalInstant Now
        {
            get
            {
                var rightNow = Clock.Now;
                var offsetToLocal = DateTimeZones.Current.GetOffsetFromUtc(rightNow);
                return Instant.Add(rightNow, offsetToLocal);
            }
        }

        #region Operators
        /// <summary>
        /// Returns an instant after adding the given duration
        /// </summary>
        public static LocalInstant operator +(LocalInstant left, Duration right)
        {
            return new LocalInstant(left.Ticks + right.Ticks);
        }

        /// <summary>
        /// Adds a duration to a local instant. Friendly alternative to <c>operator-()</c>.
        /// </summary>
        /// <param name="left">The left hand side of the operator.</param>
        /// <param name="right">The right hand side of the operator.</param>
        /// <returns>A new <see cref="LocalInstant"/> representing the sum of the given values.</returns>
        public static LocalInstant Add(LocalInstant left, Duration right)
        {
            return left + right;
        }

        /// <summary>
        /// Returns the difference between two instants as a duration.
        /// </summary>
        public static Duration operator -(LocalInstant left, LocalInstant right)
        {
            return new Duration(left.Ticks - right.Ticks);
        }

        /// <summary>
        /// Implements the operator - (subtraction) for <see cref="LocalInstant"/> - <see
        /// cref="Offset"/>.
        /// </summary>
        /// <param name="left">The left hand side of the operator.</param>
        /// <param name="right">The right hand side of the operator.</param>
        /// <returns>A new <see cref="Instant"/> representing the difference of the given values.</returns>
        public static Instant operator -(LocalInstant left, Offset right)
        {
            return new Instant(left.Ticks - right.Ticks);
        }

        /// <summary>
        /// Returns an instant after subtracting the given duration
        /// </summary>
        public static LocalInstant operator -(LocalInstant left, Duration right)
        {
            return new LocalInstant(left.Ticks - right.Ticks);
        }

        /// <summary>
        /// Subtracts one local instant from another. Friendly alternative to <c>operator-()</c>.
        /// </summary>
        /// <param name="left">The left hand side of the operator.</param>
        /// <param name="right">The right hand side of the operator.</param>
        /// <returns>A new <see cref="Duration"/> representing the difference of the given values.</returns>
        public static Duration Subtract(LocalInstant left, LocalInstant right)
        {
            return left - right;
        }

        /// <summary>
        /// Subtracts an offset from a local instant. Friendly alternative to <c>operator-()</c>.
        /// </summary>
        /// <param name="left">The left hand side of the operator.</param>
        /// <param name="right">The right hand side of the operator.</param>
        /// <returns>A new <see cref="Instant"/> representing the difference of the given values.</returns>
        public static Instant Subtract(LocalInstant left, Offset right)
        {
            return left - right;
        }

        /// <summary>
        /// Subtracts a duration from a local instant. Friendly alternative to <c>operator-()</c>.
        /// </summary>
        /// <param name="left">The left hand side of the operator.</param>
        /// <param name="right">The right hand side of the operator.</param>
        /// <returns>A new <see cref="LocalInstant"/> representing the difference of the given values.</returns>
        public static LocalInstant Subtract(LocalInstant left, Duration right)
        {
            return left - right;
        }

        /// <summary>
        /// Implements the operator == (equality).
        /// </summary>
        /// <param name="left">The left hand side of the operator.</param>
        /// <param name="right">The right hand side of the operator.</param>
        /// <returns><c>true</c> if values are equal to each other, otherwise <c>false</c>.</returns>
        public static bool operator ==(LocalInstant left, LocalInstant right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Implements the operator != (inequality).
        /// </summary>
        /// <param name="left">The left hand side of the operator.</param>
        /// <param name="right">The right hand side of the operator.</param>
        /// <returns><c>true</c> if values are not equal to each other, otherwise <c>false</c>.</returns>
        public static bool operator !=(LocalInstant left, LocalInstant right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Implements the operator &lt; (less than).
        /// </summary>
        /// <param name="left">The left hand side of the operator.</param>
        /// <param name="right">The right hand side of the operator.</param>
        /// <returns><c>true</c> if the left value is less than the right value, otherwise <c>false</c>.</returns>
        public static bool operator <(LocalInstant left, LocalInstant right)
        {
            return left.CompareTo(right) < 0;
        }

        /// <summary>
        /// Implements the operator &lt;= (less than or equal).
        /// </summary>
        /// <param name="left">The left hand side of the operator.</param>
        /// <param name="right">The right hand side of the operator.</param>
        /// <returns><c>true</c> if the left value is less than or equal to the right value, otherwise <c>false</c>.</returns>
        public static bool operator <=(LocalInstant left, LocalInstant right)
        {
            return left.CompareTo(right) <= 0;
        }

        /// <summary>
        /// Implements the operator &gt; (greater than).
        /// </summary>
        /// <param name="left">The left hand side of the operator.</param>
        /// <param name="right">The right hand side of the operator.</param>
        /// <returns><c>true</c> if the left value is greater than the right value, otherwise <c>false</c>.</returns>
        public static bool operator >(LocalInstant left, LocalInstant right)
        {
            return left.CompareTo(right) > 0;
        }

        /// <summary>
        /// Implements the operator &gt;= (greater than or equal).
        /// </summary>
        /// <param name="left">The left hand side of the operator.</param>
        /// <param name="right">The right hand side of the operator.</param>
        /// <returns><c>true</c> if the left value is greater than or equal to the right value, otherwise <c>false</c>.</returns>
        public static bool operator >=(LocalInstant left, LocalInstant right)
        {
            return left.CompareTo(right) >= 0;
        }
        #endregion // Operators

        #region IComparable<LocalInstant> Members
        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared.
        /// The return value has the following meanings:
        /// <list type="table">
        /// <listheader>
        /// <term>Value</term>
        /// <description>Meaning</description>
        /// </listheader>
        /// <item>
        /// <term>&lt; 0</term>
        /// <description>This object is less than the <paramref name="other"/> parameter.</description>
        /// </item>
        /// <item>
        /// <term>0</term>
        /// <description>This object is equal to <paramref name="other"/>.</description>
        /// </item>
        /// <item>
        /// <term>&gt; 0</term>
        /// <description>This object is greater than <paramref name="other"/>.</description>
        /// </item>
        /// </list>
        /// </returns>
        public int CompareTo(LocalInstant other)
        {
            return Ticks.CompareTo(other.Ticks);
        }
        #endregion

        #region Object overrides
        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance;
        /// otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is LocalInstant)
            {
                return Equals((LocalInstant)obj);
            }
            return false;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data
        /// structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return Ticks.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            // TODO: Use proper formatting!
            var utc = new LocalDateTime(new LocalInstant(Ticks));
            return string.Format(CultureInfo.InvariantCulture, "{0}-{1:00}-{2:00}T{3:00}:{4:00}:{5:00} LOC", utc.Year, utc.MonthOfYear, utc.DayOfMonth,
                                 utc.HourOfDay, utc.MinuteOfHour, utc.SecondOfMinute);
        }
        #endregion  // Object overrides

        #region IEquatable<LocalInstant> Members
        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter;
        /// otherwise, false.
        /// </returns>
        public bool Equals(LocalInstant other)
        {
            return Ticks == other.Ticks;
        }
        #endregion
    }
}