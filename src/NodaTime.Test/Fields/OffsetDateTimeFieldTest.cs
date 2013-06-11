// Copyright 2009 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using NUnit.Framework;
using NodaTime.Fields;

namespace NodaTime.Test.Fields
{
    [TestFixture]
    public class OffsetDateTimeFieldTest
    {
        [Test]
        public void Constructor_WithoutFieldType_HardCodedProperties()
        {
            OffsetDateTimeField field = new OffsetDateTimeField(CalendarSystem.Iso.Fields.SecondOfMinute, 3);
            Assert.AreEqual(DateTimeFieldType.SecondOfMinute, field.FieldType);
            Assert.IsTrue(field.IsSupported);
        }

        [Test]
        public void Constructor_WithNullField_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new OffsetDateTimeField(null, 3));
        }

        [Test]
        public void Constructor_WithZeroOffset_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new OffsetDateTimeField(CalendarSystem.Iso.Fields.SecondOfMinute, 0));
        }

        [Test]
        public void Constructor_WithUnsupportedField_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(
                () => new OffsetDateTimeField(UnsupportedDateTimeField.SecondOfMinute, 3));
        }

        [Test]
        public void Constructor_WithSpecificFieldType()
        {
            OffsetDateTimeField field = new OffsetDateTimeField(CalendarSystem.Iso.Fields.SecondOfMinute, DateTimeFieldType.SecondOfDay, 3);
            Assert.AreEqual(DateTimeFieldType.SecondOfDay, field.FieldType);
        }

        [Test]
        public void Constructor_WithSpecificFieldTypeButNullField_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new OffsetDateTimeField(null, DateTimeFieldType.SecondOfDay, 3));
        }

        [Test]
        public void Constructor_WithSpecificNullFieldType_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new OffsetDateTimeField(CalendarSystem.Iso.Fields.SecondOfMinute, null, 3));
        }

        [Test]
        public void Constructor_WithSpecificFieldTypeButZeroOffset_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new OffsetDateTimeField(CalendarSystem.Iso.Fields.SecondOfMinute, DateTimeFieldType.SecondOfDay, 0));
        }

        [Test]
        public void GetValue_AddsOffset()
        {
            OffsetDateTimeField field = GetSampleField();
            Assert.AreEqual(0 + 3, field.GetValue(new LocalInstant(0)));
            Assert.AreEqual(6 + 3, field.GetValue(new LocalInstant(6 * NodaConstants.TicksPerSecond)));
        }

        [Test]
        public void GetInt64Value_AddsOffset()
        {
            OffsetDateTimeField field = GetSampleField();
            Assert.AreEqual(0 + 3, field.GetInt64Value(new LocalInstant(0)));
            Assert.AreEqual(6 + 3, field.GetInt64Value(new LocalInstant(6 * NodaConstants.TicksPerSecond)));
        }

        [Test]
        public void SetValue_AdjustsByOffset()
        {
            OffsetDateTimeField field = GetSampleField();
            Assert.AreEqual(31200000L, field.SetValue(new LocalInstant(21200000L), 6).Ticks);
            Assert.AreEqual(261200000L, field.SetValue(new LocalInstant(21200000L), 29).Ticks);
            // Note the wrapping here
            Assert.AreEqual(571200000L, field.SetValue(new LocalInstant(21200000L), 60).Ticks);
        }

        [Test]
        public void GetMinimumValue_UsesOffset()
        {
            OffsetDateTimeField field = GetSampleField();
            Assert.AreEqual(3, field.GetMinimumValue());
        }

        [Test]
        public void GetMinimumValue_WithLocalInstant_UsesOffset()
        {
            OffsetDateTimeField field = GetSampleField();
            Assert.AreEqual(3, field.GetMinimumValue(new LocalInstant(0)));
        }

        [Test]
        public void GetMaximumValue_UsesOffset()
        {
            OffsetDateTimeField field = GetSampleField();
            Assert.AreEqual(62, field.GetMaximumValue());
        }

        [Test]
        public void GetMaximumValue_WithLocalInstant_UsesOffset()
        {
            OffsetDateTimeField field = GetSampleField();
            Assert.AreEqual(62, field.GetMaximumValue(new LocalInstant(0)));
        }

        /// <summary>
        /// Helper method to avoid having to repeat all of this every time
        /// </summary>
        private static OffsetDateTimeField GetSampleField()
        {
            return new OffsetDateTimeField(CalendarSystem.Iso.Fields.SecondOfMinute, 3);
        }
    }
}