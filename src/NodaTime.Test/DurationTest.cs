﻿#region Copyright and license information
// Copyright 2001-2009 Stephen Colebourne
// Copyright 2009 Jon Skeet
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
using NUnit.Framework;

namespace NodaTime.Test
{
    [TestFixture]
    public partial class DurationTest
    {
        Duration zero = new Duration(0L);
        Duration one = new Duration(1L);
        Duration onePrime = new Duration(1L);
        Duration negativeOne = new Duration(-1L);
        Duration threeMillion = new Duration(3000000L);
        Duration negativeFiftyMillion = new Duration(-50000000L);
        Duration minimum = new Duration(Int64.MinValue);
        Duration maximum = new Duration(Int64.MaxValue);
    }
}
