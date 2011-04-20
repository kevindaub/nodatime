#region Copyright and license information
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

#region usings
using NodaTime.Globalization;
using NodaTime.Test.Format;
using NUnit.Framework;
using System.Collections.Generic;
using NodaTime.Format;
#endregion

namespace NodaTime.Test
{
    [TestFixture]
    public partial class OffsetTest
    {
        [Test]
        [TestCaseSource(typeof(OffsetFormattingTestSupport), "OffsetFormattingCommonData")]
        [TestCaseSource(typeof(OffsetFormattingTestSupport), "OffsetParseData")]
        public void TestParseExact_multiple(OffsetFormattingTestSupport.OffsetData data)
        {
            FormattingTestSupport.RunParseMultipleTest(data, formats => Offset.ParseExact(data.S, formats, new NodaFormatInfo(data.C), data.Styles));
        }

        internal static IEnumerable<OffsetFormattingTestSupport.OffsetData> WithoutStyles()
        {
            foreach (var data in OffsetFormattingTestSupport.OffsetParseData)
            {
                if (data.Styles == DateTimeParseStyles.None)
                {
                    yield return data;
                }
            }
            foreach (var data in OffsetFormattingTestSupport.OffsetFormattingCommonData)
            {
                if (data.Styles == DateTimeParseStyles.None)
                {
                    yield return data;
                }
            }
        }

        [Test]
        [TestCaseSource("WithoutStyles")]
        public void TestParseExact_noStyle(OffsetFormattingTestSupport.OffsetData data)
        {
            FormattingTestSupport.RunParseSingleTest(data, format => Offset.ParseExact(data.S, format, new NodaFormatInfo(data.C)));
        }
        /*
        [Test]
        [TestCaseSource(typeof(OffsetFormattingTestSupport), "ParseExactCommon")]
        [TestCaseSource(typeof(OffsetFormattingTestSupport), "ParseExactSingle")]
        public void TestParseExact_noStyle(OffsetFormattingTestSupport.OffsetData data)
        {
            FormattingTestSupport.RunParseTest(data, () => Offset.ParseExact(data.S, data.F, new NodaFormatInfo(data.C)));
        }

        [Test]
        [TestCaseSource(typeof(OffsetFormattingTestSupport), "ParseExactCommon")]
        [TestCaseSource(typeof(OffsetFormattingTestSupport), "ParseExactSingle")]
        [TestCaseSource(typeof(OffsetFormattingTestSupport), "ParseExactStyle")]
        public void TestParseExact_withStyle(OffsetFormattingTestSupport.OffsetData data)
        {
            FormattingTestSupport.RunParseTest(data, () => Offset.ParseExact(data.S, data.F, new NodaFormatInfo(data.C), data.Styles));
        }

        [Test]
        [TestCaseSource(typeof(OffsetFormattingTestSupport), "ParseExactCommon")]
        [TestCaseSource(typeof(OffsetFormattingTestSupport), "ParseExactMultiple")]
        [TestCaseSource(typeof(OffsetFormattingTestSupport), "ParseExactStyle")]
        public void TestTryParseExact_multiple(OffsetFormattingTestSupport.OffsetData data)
        {
            string[] formats = null;
            if (data.F != null)
            {
                formats = data.F.Split('\0');
            }
            FormattingTestSupport.RunTryParse(data, (out Offset result) => Offset.TryParseExact(data.S, formats, new NodaFormatInfo(data.C), data.Styles, out result));
        }

        [Test]
        [TestCaseSource(typeof(OffsetFormattingTestSupport), "ParseExactCommon")]
        [TestCaseSource(typeof(OffsetFormattingTestSupport), "ParseExactSingle")]
        [TestCaseSource(typeof(OffsetFormattingTestSupport), "ParseExactStyle")]
        public void TestTryParseExact_single(OffsetFormattingTestSupport.OffsetData data)
        {
            FormattingTestSupport.RunTryParse(data, (out Offset result) => Offset.TryParseExact(data.S, data.F, new NodaFormatInfo(data.C), data.Styles, out result));
        }
        */
        /*
            [Test]
            [Category("Formating")]
            [Category("Parse")]
            public void TestParse_null()
            {
                Assert.Throws<ArgumentNullException>(() => Instant.Parse(null));
            }

            [Test]
            public void TestParse_BadValue()
            {
                Assert.Throws<FormatException>(() => Instant.Parse("ads"));
            }

            [Test]
            public void TestParse_N()
            {
                Instant actual = Instant.Parse(threeMillion.Ticks.ToString("N0"));
                Assert.AreEqual(threeMillion, actual);
            }

            [Test]
            public void TestParse_N_extraSpace()
            {
                Assert.Throws<FormatException>(() => Instant.Parse(" " + threeMillion.Ticks.ToString("N0")));
            }

            [Test]
            public void TestParse_N_leadingSpace_Flaged()
            {
                Instant actual = Instant.Parse(" " + threeMillion.Ticks.ToString("N0"), null, DateTimeParseStyles.AllowLeadingWhite);
                Assert.AreEqual(threeMillion, actual);
            }

            [Test]
            public void TestParse_N_trailingSpace_Flaged()
            {
                Instant actual = Instant.Parse(threeMillion.Ticks.ToString("N0") + " ", null, DateTimeParseStyles.AllowTrailingWhite);
                Assert.AreEqual(threeMillion, actual);
            }

            [Test]
            public void TestParse_N_frFR()
            {
                var frFr = new CultureInfo("fr-FR");
                Instant actual = Instant.Parse(threeMillion.Ticks.ToString("N0", frFr), frFr);
                Assert.AreEqual(threeMillion, actual);
            }

            [Test]
            public void TestParse_D()
            {
                Instant actual = Instant.Parse(threeMillion.Ticks.ToString("D"));
                Assert.AreEqual(threeMillion, actual);
            }

            [Test]
            public void TestParse_G()
            {
                Instant actual = Instant.Parse("1970-01-01T00:00:00Z");
                Assert.AreEqual(Instant.UnixEpoch, actual);
            }

            [Test]
            public void TestTryParse_null()
            {
                Instant result;
                Assert.IsFalse(Instant.TryParse(null, null, DateTimeParseStyles.None, out result));
                Assert.AreEqual(Instant.MinValue, result);
            }

            [Test]
            public void TestTryParse_BadValue()
            {
                Instant result;
                Assert.IsFalse(Instant.TryParse("ads", null, DateTimeParseStyles.None, out result));
                Assert.AreEqual(Instant.MinValue, result);
            }

            [Test]
            public void TestTryParse_N()
            {
                Instant result;
                Assert.IsTrue(Instant.TryParse(threeMillion.Ticks.ToString("N0"), null, DateTimeParseStyles.None, out result));
                Assert.AreEqual(threeMillion, result);
            }

            [Test]
            public void TestTryParse_D()
            {
                Instant result;
                Assert.IsTrue(Instant.TryParse(threeMillion.Ticks.ToString("D"), null, DateTimeParseStyles.None, out result));
                Assert.AreEqual(threeMillion, result);
            }

            [Test]
            public void TestTryParse_G()
            {
                Instant result;
                Assert.IsTrue(Instant.TryParse("1970-01-01T00:00:00Z", null, DateTimeParseStyles.None, out result));
                Assert.AreEqual(Instant.UnixEpoch, result);
            }

            [Test]
            public void TestTryParseExact_N()
            {
                Instant result;
                Assert.IsTrue(Instant.TryParseExact(threeMillion.Ticks.ToString("N0"), "n", null, DateTimeParseStyles.None, out result));
                Assert.AreEqual(threeMillion, result);
            }

            [Test]
            public void TestTryParseExact_N_fr()
            {
                var frFr = new CultureInfo("fr-FR");
                Instant result;
                Assert.IsTrue(Instant.TryParseExact(threeMillion.Ticks.ToString("N0", frFr), "n", frFr, DateTimeParseStyles.None, out result));
                Assert.AreEqual(threeMillion, result);
            }

            [Test]
            public void TestTryParseExact_D()
            {
                Instant result;
                Assert.IsTrue(Instant.TryParseExact(Int64.MinValue.ToString("D"), "d", null, DateTimeParseStyles.None, out result));
                Assert.AreEqual(Instant.MinValue, result);
            }

            [Test]
            public void TestTryParseExact_N_null()
            {
                Instant result;
                Assert.IsFalse(Instant.TryParseExact(null, "n", null, DateTimeParseStyles.None, out result));
                Assert.AreEqual(Instant.MinValue, result);
            }

            [Test]
            public void TestTryParseExact_N_BadString()
            {
                Instant result;
                Assert.IsFalse(Instant.TryParseExact("asdf", "n", null, DateTimeParseStyles.None, out result));
                Assert.AreEqual(Instant.MinValue, result);
            }

            [Test]
            public void TestTryParseExact_NullFormat()
            {
                Instant result;
                Assert.IsFalse(Instant.TryParseExact("0", (string)null, null, DateTimeParseStyles.None, out result));
                Assert.AreEqual(Instant.MinValue, result);
            }

            [Test]
            public void TestTryParseExact_InvalidFormat()
            {
                Instant result;
                Assert.IsFalse(Instant.TryParseExact("0", "Q", null, DateTimeParseStyles.None, out result));
                Assert.AreEqual(Instant.MinValue, result);
            }

            [Test]
            public void TestTryParseExact_NullFormatList()
            {
                Instant result;
                Assert.IsFalse(Instant.TryParseExact("0", (string[])null, null, DateTimeParseStyles.None, out result));
                Assert.AreEqual(Instant.MinValue, result);
            }

            [Test]
            public void TestTryParseExact_FormatListEmpty()
            {
                Instant result;
                Assert.IsFalse(Instant.TryParseExact("0", new string[] { }, null, DateTimeParseStyles.None, out result));
                Assert.AreEqual(Instant.MinValue, result);
            }

            [Test]
            public void TestTryParseExact_G_BOT()
            {
                Instant result;
                Assert.IsTrue(Instant.TryParseExact("bot", "g", null, DateTimeParseStyles.None, out result));
                Assert.AreEqual(Instant.MinValue, result);
            }

            [Test]
            public void TestTryParseExact_G_EOT()
            {
                Instant result;
                Assert.IsTrue(Instant.TryParseExact("eot", "g", null, DateTimeParseStyles.None, out result));
                Assert.AreEqual(Instant.MaxValue, result);
            }

            [Test]
            public void TestTryParseExact_G()
            {
                Instant result;
                Assert.IsTrue(Instant.TryParseExact("1970-01-01T00:00:00Z", "g", null, DateTimeParseStyles.None, out result));
                Assert.AreEqual(Instant.UnixEpoch, result);
            }

            [Test]
            public void TestTryParseExact_NG()
            {
                Instant result;
                Assert.IsTrue(Instant.TryParseExact("1970-01-01T00:00:00Z", new string[] { "n", "g" }, null, DateTimeParseStyles.None, out result));
                Assert.AreEqual(Instant.UnixEpoch, result);
            }

            [Test]
            public void TestTryParseExact_ValidValue_WrongFormat()
            {
                Instant result;
                Assert.IsFalse(Instant.TryParseExact("1970-01-01T00:00:00Z", "n", null, DateTimeParseStyles.None, out result));
                Assert.AreEqual(Instant.MinValue, result);
            }
         */
    }
}