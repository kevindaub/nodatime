﻿// Copyright 2013 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NodaTime.Utility;

namespace NodaTime.Calendars
{
    /// <summary>
    /// Era calculator for Gregorian and Julian calendar systems, which use BC and AD.
    /// </summary>
    internal sealed class GJEraCalculator : EraCalculator
    {
        private readonly int maxYearOfBc;
        private readonly int maxYearOfAd;

        internal GJEraCalculator(YearMonthDayCalculator ymdCalculator) : base(Era.BeforeCommon, Era.Common)
        {
            maxYearOfBc = 1 - ymdCalculator.MinYear; // Convert from absolute to year-of-era
            maxYearOfAd = ymdCalculator.MaxYear;
        }

        private void ValidateEra(Era era)
        {
            if (era != Era.Common && era != Era.BeforeCommon)
            {
                Preconditions.CheckNotNull(era, "era");
                Preconditions.CheckArgument(false, "era", "Era {0} is not supported by this calendar; only BC and AD are supported", era.Name);
            }
        }

        internal override int GetAbsoluteYear(int yearOfEra, Era era)
        {
            ValidateEra(era);
            if (era == Era.Common)
            {
                Preconditions.CheckArgumentRange("yearOfEra", yearOfEra, 1, maxYearOfAd);
                return yearOfEra;
            }
            Preconditions.CheckArgumentRange("yearOfEra", yearOfEra, 1, maxYearOfBc);
            return 1 - yearOfEra;
        }

        internal override int GetYearOfEra(YearMonthDay yearMonthDay)
        {
            int absoluteYear = yearMonthDay.Year;
            return absoluteYear > 0 ? absoluteYear : 1 - absoluteYear;
        }

        internal override Era GetEra(YearMonthDay yearMonthDay)
        {
            return yearMonthDay.Year > 0 ? Era.Common : Era.BeforeCommon;
        }

        internal override int GetMinYearOfEra(Era era)
        {
            ValidateEra(era);
            return 1;
        }

        internal override int GetMaxYearOfEra(Era era)
        {
            ValidateEra(era);
            return era == Era.Common ? maxYearOfAd : maxYearOfBc;
        }
    }
}
