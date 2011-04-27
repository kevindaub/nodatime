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
#region usings
using System;
using System.Diagnostics;
#endregion

namespace NodaTime.Format
{
    /// <summary>
    ///   Provides a container for the interim parsed pieces of an <see cref = "Offset" /> value.
    /// </summary>
    [DebuggerStepThrough]
    internal class OffsetParseInfo : ParseInfo, ISignedValue
    {
        /// <summary>
        ///   The fractions of a seconds in milliseconds.
        /// </summary>
        internal int? FractionalSeconds;

        /// <summary>
        ///   The hours in the range [0, 23].
        /// </summary>
        internal int? Hours;

        /// <summary>
        ///   The total millisconds. This is the only value that can be negative.
        /// </summary>
        internal int? Milliseconds;

        /// <summary>
        ///   The minutes in the range [0, 59].
        /// </summary>
        internal int? Minutes;

        /// <summary>
        ///   The seconds in the range [0, 59].
        /// </summary>
        internal int? Seconds;

        /// <summary>
        ///   Initializes a new instance of the <see cref = "OffsetParseInfo" /> class.
        /// </summary>
        /// <param name = "formatInfo">The format info.</param>
        /// <param name = "throwImmediate">if set to <c>true</c> [throw immediate].</param>
        /// <param name = "parseStyles">The parse styles.</param>
        internal OffsetParseInfo(IFormatProvider formatInfo, bool throwImmediate, DateTimeParseStyles parseStyles)
            : base(formatInfo, throwImmediate, parseStyles)
        {
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "OffsetParseInfo" /> class.
        /// </summary>
        /// <param name = "value">The value.</param>
        /// <param name = "formatInfo">The format info.</param>
        internal OffsetParseInfo(Offset value, IFormatProvider formatInfo)
            : base(formatInfo, true, DateTimeParseStyles.None)
        {
            Milliseconds = value.Milliseconds;
            IsNegative = value.IsNegative;
            Hours = value.Hours;
            Minutes = value.Minutes;
            Seconds = value.Seconds;
            FractionalSeconds = value.FractionalSeconds;
        }

        /// <summary>
        ///   Gets or sets the value.
        /// </summary>
        /// <value>
        ///   The value.
        /// </value>
        internal Offset Value { get; set; }

        #region ISignedValue Members
        /// <summary>
        ///   Gets a value indicating whether this instance is negative.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is negative; otherwise, <c>false</c>.
        /// </value>
        public bool IsNegative { get; set; }

        /// <summary>
        ///   Gets the sign.
        /// </summary>
        public string Sign { get { return IsNegative ? FormatInfo.NegativeSign : FormatInfo.PositiveSign; } }
        #endregion

        /// <summary>
        ///   Calculates the value from the parsed pieces.
        /// </summary>
        internal void CalculateValue()
        {
            int hours = Hours ?? 0;
            if (IsNegative)
            {
                hours = -hours;
            }
            int minutes = Minutes ?? 0;
            int seconds = Seconds ?? 0;
            int fractionalSeconds = FractionalSeconds ?? 0;
            Value = Offset.Create(hours, minutes, seconds, fractionalSeconds);
        }
    }
}