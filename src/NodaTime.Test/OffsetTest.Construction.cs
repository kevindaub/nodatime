#region Copyright and license information

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

using NUnit.Framework;

namespace NodaTime.Test
{
    partial class OffsetTest
    {
        [Test]
        public void Zero()
        {
            Offset test = Offset.Zero;
            Assert.AreEqual(0, test.Ticks);
        }

        [Test]
        public void ConstructFrom_Int64()
        {
            long length = 4 * NodaConstants.TicksPerDay +
                          5 * NodaConstants.TicksPerHour +
                          6 * NodaConstants.TicksPerMinute +
                          7 * NodaConstants.TicksPerSecond +
                          8 * NodaConstants.TicksPerMillisecond + 9;
            long expected = 5 * NodaConstants.TicksPerHour +
                            6 * NodaConstants.TicksPerMinute +
                            7 * NodaConstants.TicksPerSecond +
                            8 * NodaConstants.TicksPerMillisecond + 9;
            Offset test = new Offset(length);
            Assert.AreEqual(expected, test.Ticks);
        }
    }
}