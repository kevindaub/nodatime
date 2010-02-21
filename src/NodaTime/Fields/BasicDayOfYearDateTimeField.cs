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
using NodaTime.Calendars;

namespace NodaTime.Fields
{
    /// <summary>
    /// Porting status: Needs partial and max for set support.
    /// </summary>
    internal sealed class BasicDayOfYearDateTimeField : PreciseDurationDateTimeField
    {
        private readonly BasicCalendarSystem calendarSystem;

        internal BasicDayOfYearDateTimeField(BasicCalendarSystem calendarSystem, DurationField days)
            : base(DateTimeFieldType.DayOfYear, days)
        {
            this.calendarSystem = calendarSystem;
        }

        public override int GetValue(LocalInstant localInstant)
        {
            return calendarSystem.GetDayOfYear(localInstant);
        }

        public override long GetInt64Value(LocalInstant localInstant)
        {
            return calendarSystem.GetDayOfYear(localInstant);
        }

        public override DurationField RangeDurationField { get { return calendarSystem.Fields.Years; } }

        public override long GetMaximumValue()
        {
            return calendarSystem.GetDaysInYearMax();
        }

        public override long GetMaximumValue(LocalInstant localInstant)
        {
            int year = calendarSystem.GetYear(localInstant);
            return calendarSystem.GetDaysInYearMax(year);
        }

        public override long GetMinimumValue()
        {
            return 1;
        }
    }
}
