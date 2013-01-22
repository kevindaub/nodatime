using System;
using NUnit.Framework;

namespace NodaTime.Test
{
    [TestFixture]
    public partial class OffsetTest
    {
        private static readonly Offset ThreeHours = TestObjects.CreatePositiveOffset(3, 0, 0, 0);
        private static readonly Offset NegativeThreeHours = TestObjects.CreateNegativeOffset(3, 0, 0, 0);
        private static readonly Offset NegativeTwelveHours = TestObjects.CreateNegativeOffset(12, 0, 0, 0);

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

        [Test]
        public void FromTimeSpan_OutOfRange([Values(-24, 24)] int hours)
        {
            TimeSpan ts = TimeSpan.FromHours(hours);
            Assert.Throws<ArgumentOutOfRangeException>(() => Offset.FromTimeSpan(ts));
        }

        [Test]
        public void FromTimeSpan_Truncation()
        {
            TimeSpan ts = TimeSpan.FromTicks(10000 + 200);
            Assert.AreEqual(Offset.FromMilliseconds(1), Offset.FromTimeSpan(ts));
        }

        [Test]
        public void FromTimeSpan_Simple()
        {
            TimeSpan ts = TimeSpan.FromHours(2);
            Assert.AreEqual(Offset.FromHours(2), Offset.FromTimeSpan(ts));
        }

        /// <summary>
        ///   Using the default constructor is equivalent to Offset.Zero
        /// </summary>
        [Test]
        public void DefaultConstructor()
        {
            var actual = new Offset();
            Assert.AreEqual(Offset.Zero, actual);
        }

    }
}
