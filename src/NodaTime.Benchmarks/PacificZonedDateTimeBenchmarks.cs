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

using NodaTime.Benchmarks.Extensions;
using NodaTime.Benchmarks.Timing;
using NodaTime.TimeZones;

namespace NodaTime.Benchmarks
{
    internal class PacificZonedDateTimeBenchmarks
    {
        private static readonly DateTimeZone Pacific = DateTimeZone.ForId("America/Los_Angeles");
        private static readonly LocalDateTime SampleLocal = new LocalDateTime(2009, 12, 26, 10, 8, 30);
        private static readonly ZonedDateTime SampleZoned = Pacific.AtExactly(SampleLocal);

        [Benchmark]
        public void Construction()
        {
            Pacific.AtExactly(SampleLocal);
        }

        [Benchmark]
        public void Year()
        {
            SampleZoned.Year.Consume();
        }

        [Benchmark]
        public void Month()
        {
            SampleZoned.MonthOfYear.Consume();
        }

        [Benchmark]
        public void DayOfMonth()
        {
            SampleZoned.DayOfMonth.Consume();
        }

        [Benchmark]
        public void IsoDayOfWeek()
        {
            SampleZoned.IsoDayOfWeek.Consume();
        }

        [Benchmark]
        public void DayOfYear()
        {
            SampleZoned.DayOfYear.Consume();
        }

        [Benchmark]
        public void Hour()
        {
            SampleZoned.HourOfDay.Consume();
        }

        [Benchmark]
        public void Minute()
        {
            SampleZoned.MinuteOfHour.Consume();
        }

        [Benchmark]
        public void Second()
        {
            SampleZoned.SecondOfMinute.Consume();
        }

        [Benchmark]
        public void Millisecond()
        {
            SampleZoned.MillisecondOfSecond.Consume();
        }

        [Benchmark]
        public void ToInstant()
        {
            SampleZoned.ToInstant();
        }
    }
}