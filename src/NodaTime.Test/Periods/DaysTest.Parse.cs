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
using NodaTime.Periods;
using NUnit.Framework;

namespace NodaTime.Test.Periods
{
    public partial class DaysTest
    {
        private object[] ParseCorrectTestData = {
            new TestCaseData(null, 0), new TestCaseData(String.Empty, 0), new TestCaseData("P0D", 0),
            new TestCaseData("P1D", 1), new TestCaseData("P-3D", -3), new TestCaseData("P0Y0M0W2D", 2), new TestCaseData("P2DT0H0M", 2),
        };

        [Test]
        [TestCaseSource("ParseCorrectTestData")]
        public void Parse_CorrectString(string weeksString, int expectedWeeksValue)
        {
            var sut = Days.Parse(weeksString);
            Assert.AreEqual(expectedWeeksValue, sut.Value);
        }

        private object[] ParseWrongTestData = { new TestCaseData("P1DT1H"), new TestCaseData("P1Y1D"), };

        [Test]
        [TestCaseSource("ParseWrongTestData")]
        public void Parse_ThrowsNotSupported_ForWrongString(string yearsString)
        {
            Assert.Throws<NotSupportedException>(() => Days.Parse(yearsString));
        }
    }
}