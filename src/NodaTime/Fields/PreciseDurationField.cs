﻿#region Copyright and license information
// Copyright 2001-2009 Stephen Colebourne
// Copyright 2009 Jon Skeet
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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NodaTime.Fields
{
    /// <summary>
    /// Duration field class representing a field with a fixed unit length.
    /// </summary>
    internal sealed class PreciseDurationField : DurationFieldBase
    {
        /// <summary>
        /// The size of the unit, in ticks.
        /// </summary>
        private readonly long unitTicks;

        internal PreciseDurationField(DurationFieldType type, long unitTicks) : base(type)
        {
            this.unitTicks = unitTicks;
        }

        /// <summary>
        /// Always returns true.
        /// </summary>
        public override bool IsPrecise { get { return true; } }

        public override long UnitTicks { get { return unitTicks; } }

        public override long GetInt64Value(Duration duration, LocalInstant localInstant)
        {
            return duration.Ticks / unitTicks;
        }

        public override Duration GetDuration(long value, LocalInstant localInstant)
        {
            return new Duration(checked(value * UnitTicks));
        }

        public override LocalInstant Add(LocalInstant localInstant, int value)
        {
            return new LocalInstant(checked(localInstant.Ticks + value * UnitTicks));
        }

        public override LocalInstant Add(LocalInstant localInstant, long value)
        {
            return new LocalInstant(checked(localInstant.Ticks + value * UnitTicks));
        }

        public override long GetInt64Difference(LocalInstant minuendInstant, LocalInstant subtrahendInstant)
        {
            return checked((minuendInstant.Ticks - subtrahendInstant.Ticks) / UnitTicks);
        }
    }
}
