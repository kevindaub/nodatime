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
using System;
using System.Linq;
using NodaTime.Fields;
using NUnit.Framework;

namespace NodaTime.Test.Fields
{
    /// <summary>
    /// Tests for FieldSet; most just work on one or two fields as samples - the
    /// logic is the same for all fields of a given type.
    /// </summary>
    [TestFixture]
    public class FieldSetTest
    {
        private readonly IDateTimeField sampleField = new PreciseDateTimeField(DateTimeFieldType.SecondOfMinute,
            PreciseDurationField.Seconds, PreciseDurationField.Minutes);

        [Test]
        public void FieldsAreCopiedFromBuilderToSet()
        {
            FieldSet fieldSet = new FieldSet.Builder { SecondOfMinute = sampleField, Seconds = sampleField.DurationField }.Build();
            Assert.AreSame(sampleField, fieldSet.SecondOfMinute);
            Assert.AreSame(sampleField.DurationField, fieldSet.Seconds);
        }

        [Test]
        public void UnsupportedDateTimeFields_AreBuiltFromDurationFields()
        {
            FieldSet fieldSet = new FieldSet.Builder { 
                Seconds = PreciseDurationField.Seconds 
            }.Build();
            IDateTimeField field = fieldSet.SecondOfMinute;
            Assert.IsFalse(field.IsSupported);
            Assert.AreSame(fieldSet.Seconds, field.DurationField);
        }

        [Test]
        public void UnspecifiedDurationFields_DefaultToUnsupported()
        {
            FieldSet fieldSet = new FieldSet.Builder().Build();
            foreach (var prop in typeof(FieldSet).GetProperties().Where(p => p.PropertyType == typeof(DurationField)))
            {
                DurationField field = (DurationField)prop.GetValue(fieldSet, null);
                Assert.IsNotNull(field);
                Assert.IsFalse(field.IsSupported);
            }
        }

        [Test]
        public void UnspecifiedDateTimeFields_DefaultToUnsupported()
        {
            FieldSet fieldSet = new FieldSet.Builder().Build();
            foreach (var prop in typeof(FieldSet).GetProperties().Where(p => p.PropertyType == typeof(IDateTimeField)))
            {
                IDateTimeField field = (IDateTimeField)prop.GetValue(fieldSet, null);
                Assert.IsNotNull(field);
                Assert.IsFalse(field.IsSupported);
            }
        }

        [Test]
        public void Builder_WithFieldSet_CopiesFields()
        {
            FieldSet originalFieldSet = new FieldSet.Builder { SecondOfMinute = sampleField, Seconds = sampleField.DurationField }.Build();

            FieldSet newFieldSet = new FieldSet.Builder(originalFieldSet).Build();
            Assert.AreSame(sampleField, newFieldSet.SecondOfMinute);
            Assert.AreSame(sampleField.DurationField, newFieldSet.Seconds);
        }

        [Test]
        public void Builder_WithNullFieldSet_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new FieldSet.Builder(null));
        }

        [Test]
        public void WithSupportedFieldsFrom_WithNullSet_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new FieldSet.Builder().WithSupportedFieldsFrom(null));
        }

        [Test]
        public void WithSupportedFieldsFrom_CopiedSupportedFields()
        {
            FieldSet originalFieldSet = new FieldSet.Builder { SecondOfMinute = sampleField }.Build();
            IDateTimeField newField = new PreciseDateTimeField(DateTimeFieldType.SecondOfMinute,
                PreciseDurationField.Seconds, PreciseDurationField.Minutes);

            FieldSet newFieldSet = new FieldSet.Builder { SecondOfMinute = newField }
                .WithSupportedFieldsFrom(originalFieldSet).Build();
            // SecondOfMinute is supported in originalFieldSet, so the field is copied over
            Assert.AreSame(originalFieldSet.SecondOfMinute, newFieldSet.SecondOfMinute);
        }

        [Test]
        public void WithSupportedFieldsFrom_DoesNotCopyUnsupportedFields()
        {
            FieldSet originalFieldSet = new FieldSet.Builder { SecondOfMinute = sampleField }.Build();
            Assert.IsFalse(originalFieldSet.SecondOfDay.IsSupported);

            IDateTimeField newField = new PreciseDateTimeField(DateTimeFieldType.SecondOfMinute,
                PreciseDurationField.Seconds, PreciseDurationField.Minutes);

            FieldSet newFieldSet = new FieldSet.Builder { SecondOfDay = newField }
                .WithSupportedFieldsFrom(originalFieldSet).Build();
            // SecondOfDay isn't supported in originalFieldSet, so the property we set is kept
            Assert.AreSame(newField, newFieldSet.SecondOfDay);
        }

        [Test]
        public void WithSupportedFieldsFrom_ReturnsSameBuilderReference()
        {
            FieldSet originalSet = new FieldSet.Builder().Build();
            FieldSet.Builder builder = new FieldSet.Builder();
            Assert.AreSame(builder, builder.WithSupportedFieldsFrom(originalSet));
        }
    }
}
