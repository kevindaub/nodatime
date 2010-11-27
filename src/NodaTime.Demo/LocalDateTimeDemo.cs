﻿#region Copyright and license information
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

using NodaTime.Periods;
using NUnit.Framework;

namespace NodaTime.Demo
{
    [TestFixture]
    public class LocalDateTimeDemo
    {
        [Test]
        public void SimpleConstruction()
        {
            CalendarSystem calendar = CalendarSystem.Iso;
            LocalDateTime dt = new LocalDateTime(2010, 6, 16, 16, 20, calendar);
            Assert.AreEqual(20, dt.MinuteOfHour);
        }

        [Test]
        public void ImplicitIsoCalendar()
        {
            LocalDateTime dt = new LocalDateTime(2010, 6, 16, 16, 20);
            Assert.AreEqual(20, dt.MinuteOfHour);
        }

        [Test]
        public void TestToString()
        {
            LocalDateTime dt = new LocalDateTime(2010, 6, 16, 16, 20);
            Assert.AreEqual("ISO: 2010-06-16T16:20:00 LOC", dt.ToString());
        }

        [Test]
        public void SimpleArithmetic()
        {
            LocalDateTime start = new LocalDateTime(2010, 1, 5, 0, 0);
            LocalDateTime end = start + Days.From(10);
            Assert.AreEqual(new LocalDateTime(2010, 1, 15, 0, 0), end);
        }

        [Test]
        public void ArithmeticIsNotReversible()
        {
            LocalDateTime start = new LocalDateTime(2010, 1, 30, 0, 0);
            LocalDateTime end = start + Months.One;
            Assert.AreEqual(new LocalDateTime(2010, 2, 28, 0, 0), end);
            LocalDateTime backAgain = end - Months.One;
            Assert.AreEqual(new LocalDateTime(2010, 1, 28, 0, 0), backAgain);
        }

        [Test]
        public void ArithmeticIsNotComposable()
        {
            LocalDateTime start = new LocalDateTime(2010, 1, 30, 0, 0);
            LocalDateTime addOneMonthTwice = (start + Months.One) + Months.One;
            LocalDateTime addTwoMonthsOnce = start + (Months.One + Months.One);
            Assert.AreEqual(new LocalDateTime(2010, 3, 28, 0, 0), addOneMonthTwice);
            Assert.AreEqual(new LocalDateTime(2010, 3, 30, 0, 0), addTwoMonthsOnce);
        }
    }
}