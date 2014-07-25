// Copyright 2009 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NodaTime.Text;
using NUnit.Framework;

namespace NodaTime.Test
{
    [TestFixture]
    public partial class OffsetTest
    {
        private static readonly Offset ThreeHours = TestObjects.CreatePositiveOffset(3, 0, 0);
        private static readonly Offset NegativeThreeHours = TestObjects.CreateNegativeOffset(3, 0, 0);
        private static readonly Offset NegativeTwelveHours = TestObjects.CreateNegativeOffset(12, 0, 0);

        [Test]
        public void Max()
        {
            Offset x = Offset.FromSeconds(100);
            Offset y = Offset.FromSeconds(200);
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
            Offset x = Offset.FromSeconds(100);
            Offset y = Offset.FromSeconds(200);
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
            TimeSpan ts = Offset.FromSeconds(1234).ToTimeSpan();
            Assert.AreEqual(ts, TimeSpan.FromSeconds(1234));
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
            TimeSpan ts = TimeSpan.FromMilliseconds(1000 + 200);
            Assert.AreEqual(Offset.FromSeconds(1), Offset.FromTimeSpan(ts));
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

        [Test]
        public void BinarySerialization()
        {
            TestHelper.AssertBinaryRoundtrip(Offset.FromSeconds(12345));
            TestHelper.AssertBinaryRoundtrip(Offset.FromSeconds(-12345));
        }

        [Test]
        public void BinarySerialization_FromNodaTime1x()
        {
            // Verify that a stream generated by Noda Time 1.x is deserialised correctly, albeit with
            // truncation-to-seconds.
#if !PCL
            var stream = new MemoryStream(new byte[] {
                0x00, 0x01, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x0C, 0x02, 0x00, 0x00, 0x00, 0x08, 0x4E, 0x6F, 0x64, 0x61, 0x54, 0x69, 0x6D, 0x65, 0x05, 0x01, 0x00,
                0x00, 0x00, 0x0F, 0x4E, 0x6F, 0x64, 0x61, 0x54, 0x69, 0x6D, 0x65, 0x2E, 0x4F, 0x66, 0x66, 0x73, 0x65,
                0x74, 0x01, 0x00, 0x00, 0x00, 0x0C, 0x6D, 0x69, 0x6C, 0x6C, 0x69, 0x73, 0x65, 0x63, 0x6F, 0x6E, 0x64,
                0x73, 0x00, 0x08, 0x02, 0x00, 0x00, 0x00,
                0x79, 0x29, 0xED, 0xFF,  // -1234567 milliseconds.
                0x0B });
            stream.Position = 0;
            var rehydrated = (Offset)new BinaryFormatter().Deserialize(stream);
            Assert.AreEqual(Offset.FromMilliseconds(-1234567), rehydrated);
            // It's actually been truncated; the underlying value in the reconstituted offset is -1234 seconds.
            Assert.AreEqual(Offset.FromSeconds(-1234), rehydrated);
#endif
        }

        [Test]
        public void XmlSerialization()
        {
            var value = Offset.FromHoursAndMinutes(5, 30);
            TestHelper.AssertXmlRoundtrip(value, "<value>+05:30</value>");
        }

        [Test]
        [TestCase("<value>+30</value>", typeof(UnparsableValueException), Description = "Offset too large")]
        public void XmlSerialization_Invalid(string xml, Type expectedExceptionType)
        {
            TestHelper.AssertXmlInvalid<Offset>(xml, expectedExceptionType);
        }
    }
}
