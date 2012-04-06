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
namespace NodaTime.Fields
{
    /// <summary>
    /// Period field class representing a field with a fixed unit length.
    /// </summary>
    internal sealed class FixedLengthPeriodField : PeriodField
    {
        private static readonly int TypeInitializationChecking = NodaTime.Utility.TypeInitializationChecker.RecordInitializationStart();

        internal static readonly FixedLengthPeriodField Milliseconds = new FixedLengthPeriodField(PeriodFieldType.Milliseconds, NodaConstants.TicksPerMillisecond);
        internal static readonly FixedLengthPeriodField Seconds = new FixedLengthPeriodField(PeriodFieldType.Seconds, NodaConstants.TicksPerSecond);
        internal static readonly FixedLengthPeriodField Minutes = new FixedLengthPeriodField(PeriodFieldType.Minutes, NodaConstants.TicksPerMinute);
        internal static readonly FixedLengthPeriodField Hours = new FixedLengthPeriodField(PeriodFieldType.Hours, NodaConstants.TicksPerHour);
        internal static readonly FixedLengthPeriodField HalfDays = new FixedLengthPeriodField(PeriodFieldType.HalfDays, NodaConstants.TicksPerStandardDay / 2);
        internal static readonly FixedLengthPeriodField Days = new FixedLengthPeriodField(PeriodFieldType.Days, NodaConstants.TicksPerStandardDay);
        internal static readonly FixedLengthPeriodField Weeks = new FixedLengthPeriodField(PeriodFieldType.Weeks, NodaConstants.TicksPerStandardWeek);

        internal FixedLengthPeriodField(PeriodFieldType type, long unitTicks) : base(type, unitTicks, true, true)
        {
        }

        internal override long GetInt64Value(Duration duration, LocalInstant localInstant)
        {
            return duration.Ticks / UnitTicks;
        }

        internal override Duration GetDuration(long value, LocalInstant localInstant)
        {
            return new Duration(value * UnitTicks);
        }

        internal override LocalInstant Add(LocalInstant localInstant, int value)
        {
            return new LocalInstant(localInstant.Ticks + value * UnitTicks);
        }

        internal override LocalInstant Add(LocalInstant localInstant, long value)
        {
            return new LocalInstant(localInstant.Ticks + value * UnitTicks);
        }

        internal override long GetInt64Difference(LocalInstant minuendInstant, LocalInstant subtrahendInstant)
        {
            return (minuendInstant.Ticks - subtrahendInstant.Ticks) / UnitTicks;
        }
    }
}