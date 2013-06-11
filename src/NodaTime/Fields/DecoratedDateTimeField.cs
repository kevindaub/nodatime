// Copyright 2009 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using NodaTime.Utility;

namespace NodaTime.Fields
{
    /// <summary>
    /// Derives from <see cref="DateTimeField" />, implementing
    /// only the minimum required set of methods. These implemented methods
    /// delegate to a wrapped field.
    /// Porting status: Done.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This design allows new DateTimeField types to be defined that piggyback
    /// on top of another, inheriting all the safe method implementations from
    /// DateTimeField. Should any method require pure delegation to the wrapped
    /// field, simply override and use the provided WrappedField property.
    /// </para>
    /// <para>
    /// Note that currently following the Joda Time model, this type does not delegate
    /// leap-related methods and properties - those would need to be overridden directly.
    /// However, presumably that's not required as Joda doesn't use it...
    /// </para>
    /// </remarks>
    internal abstract class DecoratedDateTimeField : DateTimeField
    {
        private readonly DateTimeField wrappedField;

        protected DecoratedDateTimeField(DateTimeField wrappedField, DateTimeFieldType fieldType, PeriodField periodField)
            : base(fieldType, periodField, true)
        {
            this.wrappedField = Preconditions.CheckNotNull(wrappedField, "wrappedField");
            Preconditions.CheckArgument(wrappedField.IsSupported, "wrappedField", "The wrapped field must be supported");
        }

        protected DecoratedDateTimeField(DateTimeField wrappedField, DateTimeFieldType fieldType)
            : this(Preconditions.CheckNotNull(wrappedField, "wrappedField"), fieldType, wrappedField.PeriodField)
        {
        }

        /// <summary>
        /// Gets the wrapped date time field.
        /// </summary>
        public DateTimeField WrappedField { get { return wrappedField; } }

        internal override long GetInt64Value(LocalInstant localInstant)
        {
            return wrappedField.GetInt64Value(localInstant);
        }

        internal override LocalInstant SetValue(LocalInstant localInstant, long value)
        {
            return wrappedField.SetValue(localInstant, value);
        }

        internal override long GetMaximumValue()
        {
            return wrappedField.GetMaximumValue();
        }

        internal override long GetMinimumValue()
        {
            return wrappedField.GetMinimumValue();
        }
    }
}