using NodaTime.Calendars;
using NodaTime.Utility;

namespace NodaTime.Fields
{
    /// <summary>
    /// Porting status: needs text
    /// TODO(Post-V1): Rename to "GregulianEraDateTimeField" or something similar?
    /// </summary>
    internal sealed class GJEraDateTimeField : DateTimeField
    {
        private readonly BasicCalendarSystem calendarSystem;
        private const int BeforeCommonEraIndex = 0;
        private const int CommonEraIndex = 1;

        internal GJEraDateTimeField(BasicCalendarSystem calendarSystem) 
            : base(DateTimeFieldType.Era, UnsupportedPeriodField.Eras)
        {
            this.calendarSystem = calendarSystem;
        }

        internal override PeriodField RangePeriodField { get { return null; } }

        #region Values
        internal override long GetInt64Value(LocalInstant localInstant)
        {
            return calendarSystem.GetYear(localInstant) <= 0 ? BeforeCommonEraIndex : CommonEraIndex;
        }

        internal override LocalInstant SetValue(LocalInstant localInstant, long value)
        {
            Preconditions.CheckArgumentRange("value", value, BeforeCommonEraIndex, CommonEraIndex);

            int oldEra = GetValue(localInstant);
            if (oldEra == value)
            {
                return localInstant;
            }

            int yearOfEra = calendarSystem.Fields.YearOfEra.GetValue(localInstant);
            int newAbsoluteYear = value == 1 ? yearOfEra : 1 - yearOfEra;
            return calendarSystem.SetYear(localInstant, newAbsoluteYear);
        }
        #endregion

        #region Ranges
        internal override long GetMaximumValue()
        {
            return CommonEraIndex;
        }

        internal override long GetMinimumValue()
        {
            return BeforeCommonEraIndex;
        }
        #endregion

        #region Rounding
        internal override LocalInstant RoundFloor(LocalInstant localInstant)
        {
            return GetValue(localInstant) == CommonEraIndex ? calendarSystem.SetYear(LocalInstant.LocalUnixEpoch, 1) : new LocalInstant(long.MinValue);
        }

        internal override LocalInstant RoundCeiling(LocalInstant localInstant)
        {
            return GetValue(localInstant) == BeforeCommonEraIndex
                       ? calendarSystem.SetYear(LocalInstant.LocalUnixEpoch, 1)
                       : new LocalInstant(long.MaxValue);
        }

        internal override LocalInstant RoundHalfFloor(LocalInstant localInstant)
        {
            // In reality, the era is infinite, so there is no halfway point.
            return RoundFloor(localInstant);
        }

        internal override LocalInstant RoundHalfCeiling(LocalInstant localInstant)
        {
            // In reality, the era is infinite, so there is no halfway point.
            return RoundFloor(localInstant);
        }

        internal override LocalInstant RoundHalfEven(LocalInstant localInstant)
        {
            // In reality, the era is infinite, so there is no halfway point.
            return RoundFloor(localInstant);
        }
        #endregion
    }
}