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

using NodaTime.Fields;

namespace NodaTime.Test.Fields
{
    /// <summary>
    /// Class allowing simple construction of fields for testing constructors of other fields.
    /// </summary>
    internal class FakeDurationField : DurationField
    {
        private readonly long unitTicks;
        private readonly bool precise;

        internal FakeDurationField(long unitTicks, bool precise) : base(DurationFieldType.Seconds)
        {
            this.unitTicks = unitTicks;
            this.precise = precise;
        }

        internal override bool IsSupported { get { return true; } }

        internal override bool IsPrecise { get { return precise; } }

        internal override long UnitTicks { get { return unitTicks; } }

        internal override long GetInt64Value(Duration duration, LocalInstant localInstant)
        {
            return 0;
        }

        internal override Duration GetDuration(long value, LocalInstant localInstant)
        {
            return new Duration(0);
        }

        internal override LocalInstant Add(LocalInstant localInstant, int value)
        {
            return new LocalInstant();
        }

        internal override LocalInstant Add(LocalInstant localInstant, long value)
        {
            return new LocalInstant();
        }

        internal override long GetInt64Difference(LocalInstant minuendInstant, LocalInstant subtrahendInstant)
        {
            return 0;
        }
    }
}