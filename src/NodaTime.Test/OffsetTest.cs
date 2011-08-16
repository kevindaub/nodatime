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
using System.Globalization;
using NUnit.Framework;

namespace NodaTime.Test
{
    [TestFixture]
    public partial class OffsetTest
    {
        private static readonly Offset ThreeHours = MakeOffset(3, 0, 0, 0);
        private static readonly Offset NegativeThreeHours = MakeOffset(-3, 0, 0, 0);
        private static readonly Offset NegativeTwelveHours = MakeOffset(-12, 0, 0, 0);
        private static readonly Offset HmsfOffset = MakeOffset(5, 12, 34, 567);
        private static readonly Offset HmsOffset = MakeOffset(5, 12, 34, 0);
        private static readonly Offset HmOffset = MakeOffset(5, 12, 0, 0);
        private static readonly Offset HOffset = MakeOffset(5, 0, 0, 0);
        private static readonly Offset Full = MakeOffset(5, 6, 7, 8);
        private static readonly Offset OneFractional = MakeOffset(1, 1, 1, 400);
        private static readonly Offset TwoFractional = MakeOffset(1, 1, 1, 450);
        private static readonly Offset ThreeFractional = MakeOffset(1, 1, 1, 456);
        private static readonly CultureInfo EnUs = new CultureInfo("en-US");
        private static readonly CultureInfo FrFr = new CultureInfo("fr-FR");
        private static readonly CultureInfo ItIt = new CultureInfo("it-IT");

        private static Offset MakeOffset(int hours, int minutes, int seconds, int milliseconds)
        {
            return Offset.Create(hours, minutes, seconds, milliseconds);
        }

        [Test]
        public void Max()
        {
            Offset x = Offset.FromMilliseconds(100);
            Offset y = Offset.FromMilliseconds(200);
            Assert.AreEqual(y, Offset.Max(x, y));
            Assert.AreEqual(y, Offset.Max(y, x));
            Assert.AreEqual(x, Offset.Max(x, Offset.MinValue));
            Assert.AreEqual(x, Offset.Max(Offset.MinValue, x));
            Assert.AreEqual(Offset.MaxValue, Offset.Max(Offset.MaxValue, x));
            Assert.AreEqual(Offset.MaxValue, Offset.Max(x, Offset.MaxValue));
        }

        [Test]
        public void Min()
        {
            Offset x = Offset.FromMilliseconds(100);
            Offset y = Offset.FromMilliseconds(200);
            Assert.AreEqual(x, Offset.Min(x, y));
            Assert.AreEqual(x, Offset.Min(y, x));
            Assert.AreEqual(Offset.MinValue, Offset.Min(x, Offset.MinValue));
            Assert.AreEqual(Offset.MinValue, Offset.Min(Offset.MinValue, x));
            Assert.AreEqual(x, Offset.Min(Offset.MaxValue, x));
            Assert.AreEqual(x, Offset.Min(x, Offset.MaxValue));
        }

        [Test]
        public void ToTimeSpan()
        {
            TimeSpan ts = Offset.FromMilliseconds(1234).ToTimeSpan();
            Assert.AreEqual(ts, TimeSpan.FromMilliseconds(1234));
        }
    }
}