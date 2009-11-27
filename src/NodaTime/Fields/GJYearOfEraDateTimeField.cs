﻿using NodaTime.Calendars;

namespace NodaTime.Fields
{
    /// <summary>
    /// Porting status: Need AddWrapField
    /// </summary>
    internal sealed class GJYearOfEraDateTimeField : DecoratedDateTimeField
    {
        private readonly BasicCalendarSystem calendarSystem;

        internal GJYearOfEraDateTimeField(IDateTimeField yearField, BasicCalendarSystem calendarSystem)
            : base (yearField, DateTimeFieldType.YearOfEra)
        {
            this.calendarSystem = calendarSystem;
        }

        public override long GetInt64Value(LocalInstant localInstant)
        {
            int year = WrappedField.GetValue(localInstant);
            return year <= 0 ? 1 - year : year;
        }

        public override LocalInstant Add(LocalInstant localInstant, int value)
        {
            return WrappedField.Add(localInstant, value);
        }

        public override LocalInstant Add(LocalInstant localInstant, long value)
        {
            return WrappedField.Add(localInstant, value);
        }

        public override int GetDifference(LocalInstant minuendInstant, LocalInstant subtrahendInstant)
        {
            return WrappedField.GetDifference(minuendInstant, subtrahendInstant);
        }

        public override long GetInt64Difference(LocalInstant minuendInstant, LocalInstant subtrahendInstant)
        {
            return WrappedField.GetInt64Difference(minuendInstant, subtrahendInstant);
        }

        public override LocalInstant SetValue(LocalInstant localInstant, long value)
        {
            FieldUtils.VerifyValueBounds(this, value, 1, GetMaximumValue());
            if (calendarSystem.GetYear(localInstant) <= 0)
            {
                value = 1 - value;
            }
            return base.SetValue(localInstant, value);
        }

        public override long GetMinimumValue()
        {
            return 1;
        }

        public override long GetMaximumValue()
        {
            return WrappedField.GetMaximumValue();
        }

        public override LocalInstant RoundFloor(LocalInstant localInstant)
        {
            return WrappedField.RoundFloor(localInstant);
        }

        public override LocalInstant RoundCeiling(LocalInstant localInstant)
        {
            return WrappedField.RoundCeiling(localInstant);
        }

        public override Duration Remainder(LocalInstant localInstant)
        {
            return WrappedField.Remainder(localInstant);
        }
    }
}
