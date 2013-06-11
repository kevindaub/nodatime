// Copyright 2009 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using NUnit.Framework;
using NodaTime.Fields;

namespace NodaTime.Test.Fields
{
    [TestFixture]
    public class FixedLengthDateTimeFieldTest
    {
        [Test]
        public void Constructor_WithNullRangeField_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new FixedLengthDateTimeField(DateTimeFieldType.MinuteOfHour, new FakePeriodField(1, true), null));
        }

        [Test]
        public void Constructor_WithTooSmallRangeField_ThrowsArgumentException()
        {
            // Effectively like "seconds per second" - effective range = 1
            Assert.Throws<ArgumentException>(
                () => new FixedLengthDateTimeField(DateTimeFieldType.MinuteOfHour, new FakePeriodField(1, true), new FakePeriodField(1, true)));
        }

        [Test]
        public void Constructor_WithVariableLength_RangeField_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(
                () => new FixedLengthDateTimeField(DateTimeFieldType.MinuteOfHour, new FakePeriodField(1, true), new FakePeriodField(60, false)));
        }

        [Test]
        public void FieldType()
        {
            FixedLengthDateTimeField field = new FixedLengthDateTimeField(DateTimeFieldType.MinuteOfHour, new FakePeriodField(1, true),
                                                                  new FakePeriodField(60, true));
            Assert.AreEqual(DateTimeFieldType.MinuteOfHour, field.FieldType);
        }

        [Test]
        public void IsSupported()
        {
            FixedLengthDateTimeField field = CreateMinuteOfHourField();
            Assert.IsTrue(field.IsSupported);
        }

        [Test]
        public void GetInt64Value()
        {
            // Slightly odd LocalInstant in that it's in seconds, not ticks - due to the way the MockCountingPeriodField works.
            FixedLengthDateTimeField field = CreateMinuteOfHourField();
            Assert.AreEqual(0, field.GetInt64Value(new LocalInstant(0L)));
            Assert.AreEqual(1, field.GetInt64Value(new LocalInstant(60L)));
            Assert.AreEqual(2, field.GetInt64Value(new LocalInstant(123L)));
        }

        [Test]
        public void GetInt64Value_Negative()
        {
            // Slightly odd LocalInstant in that it's in seconds, not ticks - due to the way the MockCountingPeriodField works.
            FixedLengthDateTimeField field = CreateMinuteOfHourField();
            Assert.AreEqual(0, field.GetInt64Value(new LocalInstant(0L)));
            Assert.AreEqual(59, field.GetInt64Value(new LocalInstant(-59L)));
            Assert.AreEqual(59, field.GetInt64Value(new LocalInstant(-60L)));
            Assert.AreEqual(58, field.GetInt64Value(new LocalInstant(-61L)));
            Assert.AreEqual(58, field.GetInt64Value(new LocalInstant(-119L)));
            Assert.AreEqual(58, field.GetInt64Value(new LocalInstant(-120L)));
            Assert.AreEqual(57, field.GetInt64Value(new LocalInstant(-121L)));
        }

        [Test]
        public void SetValue()
        {
            DateTimeField field = CreateMinuteOfHourField();
            Assert.AreEqual(0, field.SetValue(new LocalInstant(120L), 0).Ticks);
            Assert.AreEqual(29 * 60, field.SetValue(new LocalInstant(120L), 29).Ticks);
        }

        private static FixedLengthDateTimeField CreateMinuteOfHourField()
        {
            return new FixedLengthDateTimeField(DateTimeFieldType.MinuteOfHour,
                new MockCountingPeriodField(PeriodFieldType.Minutes, 60),
                new MockCountingPeriodField(PeriodFieldType.Hours, 60 * 60));
        }
    }
}