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

namespace NodaTime.Calendars
{
    /// <summary>
    /// The Islamic or Hijri calendar is defined in either observational or tabular terms;
    /// Noda Time implements a tabular calendar where a pattern of leap years repeats every 30
    /// years, according to one of the patterns within this enum. See <see cref="CalendarSystem.GetIslamicCalendar"/>
    /// for more detail.
    /// </summary>
    /// <remarks>
    /// While the patterns themselves are reasonably commonly documented (see e.g.
    /// <a href="http://en.wikipedia.org/wiki/Tabular_Islamic_calendar">Wikipedia</a>)
    /// there is little standardization in terms of naming the patterns. I hope the current names do not
    /// cause offence to anyone; suggestions for better names would be welcome.
    /// </remarks>
    public enum IslamicLeapYearPattern
    {
        /// <summary>
        /// A pattern of leap years in 2, 5, 7, 10, 13, 15, 18, 21, 24, 26 and 29.
        /// This pattern and <see cref="Base16"/> are the most commonly used ones,
        /// and only differ in whether the 15th or 16th year is deemed leap.
        /// </summary>
        Base15 = 1,
        /// <summary>
        /// A pattern of leap years in 2, 5, 7, 10, 13, 16, 18, 21, 24, 26 and 29.
        /// This pattern and <see cref="Base15"/> are the most commonly used ones,
        /// and only differ in whether the 15th or 16th year is deemed leap.
        /// </summary>
        Base16 = 2,
        /// <summary>
        /// A pattern of leap years in 2, 5, 8, 10, 13, 16, 19, 21, 24, 27 and 29.
        /// </summary>
        Indian = 3,
        /// <summary>
        /// A pattern of leap years in 2, 5, 8, 11, 13, 16, 19, 21, 24, 27 and 30.
        /// </summary>
        HabashAlHasib = 4,
    }
}
