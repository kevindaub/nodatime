#region Copyright and license information
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
using System.Diagnostics.CodeAnalysis;

namespace NodaTime
{
    /// <summary>
    /// Original name: NodaConstants.
    /// 
    /// I'm not sure that everything in NodaConstants
    /// really belongs in one file, and it should perhaps be named to reflect its ISO background
    /// as well. We should consider an Iso8601Month enum etc. Possibly start
    /// with everything in here, then refactor it out when it's all working.
    /// </summary>
    public static class NodaConstants
    {
        // TODO: Enum for this instead? (With duplicate values where appropriate.)

        /// <summary>
        /// Constant for the era known as BCE (Before Common Era).
        /// </summary>
        // ReSharper disable InconsistentNaming
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "BCE", Justification = "BCE is the expected initialism")]
        public const int BCE = 0;
        /// <summary>
        /// Constant for the BC era; equal to BCE but provided for flexibility of expression..
        /// </summary>
        public const int BC = BCE;
        /// <summary>
        /// Constant for the BC era; equal to BCE but provided for flexibility of expression..
        /// </summary>
        public const int BeforeCommonEra = BCE;
        /// <summary>
        /// Constant for the common era.
        /// </summary>
        public const int CE = 1;
        /// <summary>
        /// Constant for the common era; equal to CE but provided for flexibility of expression.
        /// </summary>
        public const int CommonEra = CE;
        /// <summary>
        /// Constant for the "Anno Domini" era; equal to CE but provided for flexibility of expression.
        /// </summary>
        public const int AD = CE;
        // ReSharper restore InconsistentNaming

        // TODO: Enum for this instead?
        public const int January = 1;
        public const int February = 2;
        public const int March = 3;
        public const int April = 4;
        public const int May = 5;
        public const int June = 6;
        public const int July = 7;
        public const int August = 8;
        public const int September = 9;
        public const int October = 10;
        public const int November = 11;
        public const int December = 12;

        public const long TicksPerMillisecond = TimeSpan.TicksPerMillisecond;
        public const long TicksPerSecond = TicksPerMillisecond * MillisecondsPerSecond;
        public const long TicksPerMinute = TicksPerSecond * SecondsPerMinute;
        public const long TicksPerHour = TicksPerMinute * MinutesPerHour;
        public const long TicksPerStandardDay = TicksPerHour * HoursPerStandardDay;
        public const long TicksPerStandardWeek = TicksPerStandardDay * DaysPerWeek;

        public const int MillisecondsPerSecond = 1000;
        public const int MillisecondsPerMinute = MillisecondsPerSecond * SecondsPerMinute;
        public const int MillisecondsPerHour = MillisecondsPerMinute * MinutesPerHour;
        public const int MillisecondsPerDay = MillisecondsPerHour * HoursPerStandardDay;
        public const int MillisecondsPerWeek = MillisecondsPerDay * DaysPerWeek;

        public const int SecondsPerMinute = 60;
        public const int SecondsPerHour = SecondsPerMinute * MinutesPerHour;
        public const int SecondsPerDay = SecondsPerHour * HoursPerStandardDay;
        public const int SecondsPerWeek = SecondsPerDay * DaysPerWeek;

        public const int MinutesPerHour = 60;
        public const int MinutesPerDay = MinutesPerHour * HoursPerStandardDay;
        public const int MinutesPerWeek = MinutesPerDay * DaysPerWeek;

        public const int HoursPerStandardDay = 24;
        public const int HoursPerStandardWeek = HoursPerStandardDay * DaysPerWeek;

        public const int DaysPerWeek = 7;
    }
}