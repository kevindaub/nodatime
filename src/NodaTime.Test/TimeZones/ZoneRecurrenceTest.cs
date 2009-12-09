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
using NodaTime.TimeZones;
using NUnit.Framework;
using NodaTime.Calendars;

namespace NodaTime.Test.TimeZones
{
    [TestFixture]
    public partial class ZoneRecurrenceTest
    {
        [Test]
        public void WriteRead()
        {
            var dio = new DtzIoHelper();
            var yearOffset = new ZoneYearOffset(TransitionMode.Utc, 10, 31, (int)DaysOfWeek.Wednesday, true, Offset.Zero);
            var actual = new ZoneRecurrence("bob", Offset.Zero, yearOffset);
            var expected = dio.WriteRead(actual);
            Assert.AreEqual(expected, actual);
        }
    }
}
