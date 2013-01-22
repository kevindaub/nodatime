using NodaTime.Fields;

namespace NodaTime.Test.Fields
{
    /// <summary>
    /// Class allowing simple construction of fields for testing constructors of other fields.
    /// </summary>
    internal class FakePeriodField : PeriodField
    {
        internal FakePeriodField(long unitTicks, bool fixedLength) : base(PeriodFieldType.Seconds, unitTicks, fixedLength, true)
        {
        }

        internal override long GetInt64Value(Duration duration, LocalInstant localInstant)
        {
            return 0;
        }

        internal override Duration GetDuration(long value, LocalInstant localInstant)
        {
            return new Duration(0);
        }

        internal override LocalInstant Add(LocalInstant localInstant, int value)
        {
            return new LocalInstant();
        }

        internal override LocalInstant Add(LocalInstant localInstant, long value)
        {
            return new LocalInstant();
        }

        internal override long GetInt64Difference(LocalInstant minuendInstant, LocalInstant subtrahendInstant)
        {
            return 0;
        }
    }
}