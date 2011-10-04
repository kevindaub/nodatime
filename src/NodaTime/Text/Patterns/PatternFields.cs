﻿#region Copyright and license information
// Copyright 2001-2009 Stephen Colebourne
// Copyright 2009-2011 Jon Skeet
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
using System.Collections.Generic;
using System.Text;

namespace NodaTime.Text.Patterns
{
    /// <summary>
    /// Enum representing the fields available within patterns. This single enum is shared
    /// by all parser types for simplicity, although most fields aren't used by most parsers.
    /// Pattern fields don't necessarily have corresponding duration or date/time fields,
    /// due to concepts such as "sign".
    /// </summary>
    [Flags]
    internal enum PatternFields
    {
        None = 0,
        Sign = 1 << 0,
        Hours12 = 1 << 1,
        Hours24 = 1 << 2,
        Minutes = 1 << 3,
        Seconds = 1 << 4,
        FractionalSeconds = 1 << 5,
        AmPm = 1 << 6,
        Year = 1 << 7,
        YearOfEra = 1 << 8,
        MonthOfYearNumeric = 1 << 9,
        MonthOfYearText = 1 << 10,
        DayOfMonth = 1 << 11,
        DayOfWeek = 1 << 12,
        Era = 1 << 13
    }
}
