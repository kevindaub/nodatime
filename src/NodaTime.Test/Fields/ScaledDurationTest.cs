﻿using System;
using NodaTime.Fields;
using NUnit.Framework;

namespace NodaTime.Test.Fields
{
    [TestFixture]
    public class ScaledDurationTest
    {
        private readonly ScaledDurationField sample =
            new ScaledDurationField(TicksDurationField.Instance,
                DurationFieldType.Minutes, 90);
        private readonly LocalInstant localInstant = new LocalInstant(567L);

        [Test]
        public void Constructor_WithNullField_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ScaledDurationField(null, DurationFieldType.Minutes, 10));
        }

        [Test]
        public void Constructor_WithUnsupportedField_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentException>(() => new ScaledDurationField(UnsupportedDurationField.Milliseconds, DurationFieldType.Minutes, 10));
        }

        [Test]
        public void Constructor_WithInvalidFieldType_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ScaledDurationField(TicksDurationField.Instance, (DurationFieldType)(-1), 10));
        }

        [Test]
        public void Constructor_WithScaleOfZero_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ScaledDurationField(TicksDurationField.Instance, DurationFieldType.Minutes, 0));
        }

        [Test]
        public void Constructor_WithScaleOfOne_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ScaledDurationField(TicksDurationField.Instance, DurationFieldType.Minutes, 1));
        }

        [Test]
        public void SimpleProperties()
        {
            Assert.AreEqual(DurationFieldType.Minutes, sample.FieldType);
            Assert.IsTrue(sample.IsSupported);
            Assert.IsTrue(sample.IsPrecise);
        }

        [Test]
        public void UnitTicks()
        {
            Assert.AreEqual(90, sample.UnitTicks);
        }

        [Test]
        public void GetValue()
        {
            Assert.AreEqual(0, sample.GetValue(new Duration(0L)));
            Assert.AreEqual(12345678 / 90, sample.GetValue(new Duration(12345678L)));
            Assert.AreEqual(-1234 / 90, sample.GetValue(new Duration(-1234L)));
            Assert.AreEqual(int.MaxValue / 90, sample.GetValue(new Duration(int.MaxValue)));
        }

        public void GetValue_WithNonInt32Value_Overflows()
        {
            Assert.Throws<OverflowException>(() => sample.GetValue(new Duration(int.MaxValue + 1L)));
        }

        [Test]
        public void GetInt64Value()
        {
            Assert.AreEqual(0L, sample.GetInt64Value(new Duration(0L)));
            Assert.AreEqual(12345678L / 90, sample.GetInt64Value(new Duration(12345678L)));
            Assert.AreEqual(-1234L / 90, sample.GetInt64Value(new Duration(-1234L)));
            Assert.AreEqual(int.MaxValue / 90L, sample.GetInt64Value(new Duration(int.MaxValue)));
            Assert.AreEqual(int.MaxValue + 1L, sample.GetInt64Value(new Duration(int.MaxValue * 90L + 90L)));
        }

        public void GetValue_WithLocalInstant()
        {
            Assert.AreEqual(0, sample.GetValue(new Duration(0L), localInstant));
            Assert.AreEqual(12345678 / 90, sample.GetValue(new Duration(12345678L), localInstant));
            Assert.AreEqual(-1234 / 90, sample.GetValue(new Duration(-1234L), localInstant));
            Assert.AreEqual(int.MaxValue / 90, sample.GetValue(new Duration(int.MaxValue), localInstant));
        }

        public void GetValue_WithLocalInstantButNonInt32Value_Overflows()
        {
            Assert.Throws<OverflowException>(() => sample.GetValue(new Duration(int.MaxValue + 1L), localInstant));
        }

        [Test]
        public void GetInt64Value_WithLocalInstant()
        {
            Assert.AreEqual(0L, sample.GetInt64Value(new Duration(0L), localInstant));
            Assert.AreEqual(12345678L / 90, sample.GetInt64Value(new Duration(12345678L), localInstant));
            Assert.AreEqual(-1234L / 90, sample.GetInt64Value(new Duration(-1234L), localInstant));
            Assert.AreEqual(int.MaxValue / 90L, sample.GetInt64Value(new Duration(int.MaxValue), localInstant));
            Assert.AreEqual(int.MaxValue + 1L, sample.GetInt64Value(new Duration(int.MaxValue * 90L + 90L), localInstant));
        }

        [Test]
        public void GetDuration()
        {
            Assert.AreEqual(0L, sample.GetDuration(0).Ticks);
            Assert.AreEqual(1234 * 90L, sample.GetDuration(1234).Ticks);
            Assert.AreEqual(-1234 * 90L, sample.GetDuration(-1234).Ticks);
            Assert.AreEqual(int.MaxValue * 90L, sample.GetDuration(int.MaxValue).Ticks);
        }

        [Test]
        public void GetDuration_WithOverflow()
        {
            Assert.Throws<OverflowException>(() => sample.GetDuration(long.MaxValue));
        }

        [Test]
        public void GetDuration_WithLocalInstant()
        {
            Assert.AreEqual(0L, sample.GetDuration(0, localInstant).Ticks);
            Assert.AreEqual(1234 * 90L, sample.GetDuration(1234, localInstant).Ticks);
            Assert.AreEqual(-1234 * 90L, sample.GetDuration(-1234, localInstant).Ticks);
            Assert.AreEqual(int.MaxValue * 90L, sample.GetDuration(int.MaxValue, localInstant).Ticks);
        }

        [Test]
        public void GetDuration_WithLocalInstantCausingOverflow()
        {
            Assert.Throws<OverflowException>(() => sample.GetDuration(long.MaxValue, localInstant));
        }

        [Test]
        public void Add_WithInt32()
        {
            Assert.AreEqual(567L, sample.Add(localInstant, 0).Ticks);
            Assert.AreEqual(567L + 1234L * 90L, sample.Add(localInstant, 1234).Ticks);
            Assert.AreEqual(567L - 1234L * 90L, sample.Add(localInstant, -1234).Ticks);
        }

        [Test]
        public void Add_WithInt32CausingOverflow()
        {
            Assert.Throws<OverflowException>(() => sample.Add(new LocalInstant(long.MaxValue), 1));
        }

        [Test]
        public void Add_WithInt64()
        {
            Assert.AreEqual(567L, sample.Add(localInstant, 0L).Ticks);
            Assert.AreEqual(567L + 1234L * 90L, sample.Add(localInstant, 1234L).Ticks);
            Assert.AreEqual(567L - 1234L * 90L, sample.Add(localInstant, -1234L).Ticks);
        }

        [Test]
        public void Add_WithInt64CausingOverflow()
        {
            Assert.Throws<OverflowException>(() => sample.Add(new LocalInstant(long.MaxValue), 1L));
        }

        [Test]
        public void GetDifference()
        {
            Assert.AreEqual(0, sample.GetDifference(new LocalInstant(1L), new LocalInstant(0L)));
            Assert.AreEqual(567, sample.GetDifference(new LocalInstant(567 * 90L), new LocalInstant(0L)));
            Assert.AreEqual(567 - 1234, sample.GetDifference(new LocalInstant(567 * 90L), new LocalInstant(1234 * 90L)));
            Assert.AreEqual(567 + 1234, sample.GetDifference(new LocalInstant(567 * 90L), new LocalInstant(-1234 * 90L)));
        }

        [Test]
        public void GetDifference_WithOverflow()
        {
            Assert.Throws<OverflowException>(() => sample.GetDifference(new LocalInstant(long.MaxValue), new LocalInstant(-1L)));
        }

        [Test]
        public void GetInt64Difference()
        {
            Assert.AreEqual(0L, sample.GetInt64Difference(new LocalInstant(1L), new LocalInstant(0L)));
            Assert.AreEqual(567L, sample.GetInt64Difference(new LocalInstant(567 * 90L), new LocalInstant(0L)));
            Assert.AreEqual(567L - 1234L, sample.GetInt64Difference(new LocalInstant(567 * 90L), new LocalInstant(1234 * 90L)));
            Assert.AreEqual(567L + 1234L, sample.GetInt64Difference(new LocalInstant(567 * 90L), new LocalInstant(-1234 * 90L)));
        }

        [Test]
        public void GetInt64Difference_WithOverflow()
        {
            Assert.Throws<OverflowException>(() => sample.GetInt64Difference(new LocalInstant(long.MaxValue), new LocalInstant(-1L)));
        }
    }
}
