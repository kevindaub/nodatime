// Copyright 2009 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using NodaTime.Fields;

namespace NodaTime.Test.Fields
{
    internal class MockCountingPeriodField : PeriodField
    {
        // TODO(V1.2): Use a proper mock? (This will probably die in the great refactoring.)
        private readonly long unitTicks;

        internal MockCountingPeriodField(PeriodFieldType fieldType) : this(fieldType, 60)
        {
        }

        internal MockCountingPeriodField(PeriodFieldType fieldType, long unitTicks) : base(fieldType, unitTicks, true, true)
        {
            this.unitTicks = unitTicks;
        }

        internal static int int64Additions;
        internal static LocalInstant Add64InstantArg;
        internal static long Add64ValueArg;

        internal override LocalInstant Add(LocalInstant localInstant, long value)
        {
            int64Additions++;
            Add64InstantArg = localInstant;
            Add64ValueArg = value;

            return new LocalInstant(localInstant.Ticks + value * unitTicks);
        }

        internal static int differences64;
        internal static LocalInstant Diff64FirstArg;
        internal static LocalInstant Diff64SecondArg;

        internal override long GetInt64Difference(LocalInstant minuendInstant, LocalInstant subtrahendInstant)
        {
            differences64++;
            Diff64FirstArg = minuendInstant;
            Diff64SecondArg = subtrahendInstant;
            return 30;
        }
    }
}