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

using System.Globalization;
using NUnit.Framework;
using System;

namespace NodaTime.Test
{
    [TestFixture]
    public partial class OffsetTest
    {
        private readonly Offset threeHours = MakeOffset(3, 0, 0, 0);
        // private Offset threeHoursPrime = MakeOffset(3, 0, 0, 0);
        private readonly Offset negativeThreeHours = MakeOffset(-3, 0, 0, 0);
        private readonly Offset negativeTwelveHours = MakeOffset(-12, 0, 0, 0);

        private static Offset MakeOffset(int hours, int minutes, int seconds, int milliseconds)
        {
            int millis = (hours * NodaConstants.MillisecondsPerHour);
            millis += (minutes * NodaConstants.MillisecondsPerMinute);
            millis += (seconds * NodaConstants.MillisecondsPerSecond);
            millis += milliseconds;
            return Offset.FromMilliseconds(millis);
        }

        [Test]
        public void TestToString_InvalidFormat()
        {
            Assert.Throws<FormatException>(() => Offset.Zero.ToString("A"));
        }

        [Test]
        public void TestToString_MinValue()
        {
            TestToStringBase(Offset.MinValue, "-PT23H59M59.999S", "-PT23H59M59.999S", "-PT23H59M");
        }

        [Test]
        public void TestToString_MaxValue()
        {
            TestToStringBase(Offset.MaxValue, "+PT23H59M59.999S", "+PT23H59M59.999S", "+PT23H59M");
        }

        [Test]
        public void TestToString_Zero()
        {
            TestToStringBase(Offset.Zero, "+PT0H", "+PT0H00M00.000S", "+PT0H00M");
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

        private static void TestToStringBase(Offset value, string gvalue, string lvalue, string svalue)
        {
            var actual = value.ToString();
            Assert.AreEqual(gvalue, actual);
            actual = value.ToString("G");
            Assert.AreEqual(gvalue, actual);
            actual = value.ToString("L");
            Assert.AreEqual(lvalue, actual);
            actual = value.ToString("S");
            Assert.AreEqual(svalue, actual);
            actual = value.ToString("S", CultureInfo.InvariantCulture);
            Assert.AreEqual(svalue, actual);
            actual = value.ToString(CultureInfo.InvariantCulture);
            Assert.AreEqual(gvalue, actual);

            actual = string.Format("{0}", value);
            Assert.AreEqual(gvalue, actual);
            actual = string.Format("{0:G}", value);
            Assert.AreEqual(gvalue, actual);
            actual = string.Format("{0:L}", value);
            Assert.AreEqual(lvalue, actual);
            actual = string.Format("{0:S}", value);
            Assert.AreEqual(svalue, actual);
        }
    }
}