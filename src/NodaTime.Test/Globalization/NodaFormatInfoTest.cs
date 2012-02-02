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
#region usings
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using NUnit.Framework;
using NodaTime.Calendars;
using NodaTime.Globalization;
#endregion

namespace NodaTime.Test.Globalization
{
    [TestFixture]
    public class NodaFormatInfoTest
    {
        private readonly CultureInfo enUs = CultureInfo.GetCultureInfo("en-US");
        private readonly CultureInfo enGb = CultureInfo.GetCultureInfo("en-GB");

        private sealed class EmptyFormatProvider : IFormatProvider
        {
            #region IFormatProvider Members
            public object GetFormat(Type formatType)
            {
                return null;
            }
            #endregion
        }

        [Test]
        public void TestClone()
        {
            var info1 = NodaFormatInfo.GetFormatInfo(enUs);
            Assert.NotNull(info1);
            Assert.IsTrue(info1.IsReadOnly);

            var info2 = info1.Clone();
            Assert.IsFalse(info2.IsReadOnly);
            Assert.AreNotSame(info1, info2);
            Assert.AreEqual(info1.Name, info2.Name);
        }

        [Test]
        public void TestClone_WithNodaCultureInfo()
        {
            NodaCultureInfo culture = NodaCultureInfo.InvariantCulture;
            NodaFormatInfo formatInfo = new NodaFormatInfo(culture);

            NodaFormatInfo clone = formatInfo.Clone();
            Assert.AreNotSame(clone, formatInfo);
            Assert.AreNotSame(clone.CultureInfo, formatInfo.CultureInfo);
            Assert.AreSame(((NodaCultureInfo)clone.CultureInfo).NodaFormatInfo, clone);
        }

        [Test]
        public void TestCloneClonesSubFormatInfo()
        {
            var clone1 = (NodaFormatInfo) NodaFormatInfo.InvariantInfo.Clone();
            var clone2 = (NodaFormatInfo) NodaFormatInfo.InvariantInfo.Clone();
            Assert.AreNotSame(clone1.CultureInfo, clone2.CultureInfo);
            Assert.AreNotSame(clone1.NumberFormat, clone2.NumberFormat);
            Assert.AreNotSame(clone1.DateTimeFormat, clone2.NumberFormat);
        }

        [Test]
        public void TestClonePatternParsersUncached()
        {
            var offset = Offset.Create(5, 10, 0, 0);
            var clone1 = NodaFormatInfo.InvariantInfo.Clone();
            clone1.OffsetPatternLong = "HH";
            var clone2 = clone1.Clone();
            clone2.OffsetPatternLong = "mm";

            var pattern1 = clone1.OffsetPatternParser.ParsePattern("l").GetResultOrThrow();
            var pattern2 = clone2.OffsetPatternParser.ParsePattern("l").GetResultOrThrow();
            Assert.AreEqual("05", pattern1.Format(offset));
            Assert.AreEqual("10", pattern2.Format(offset));
        }

        [Test]
        public void TestConstructor()
        {
            var info = new NodaFormatInfo(enUs);
            Assert.AreSame(enUs, info.CultureInfo);
            Assert.NotNull(info.NumberFormat);
            Assert.NotNull(info.DateTimeFormat);
            Assert.AreEqual(".", info.DecimalSeparator);
            Assert.AreEqual("+", info.PositiveSign);
            Assert.AreEqual("-", info.NegativeSign);
            Assert.AreEqual(":", info.TimeSeparator);
            Assert.AreEqual("/", info.DateSeparator);
            Assert.IsFalse(info.IsReadOnly);
            Assert.IsInstanceOf<string>(info.OffsetPatternFull);
            Assert.IsInstanceOf<string>(info.OffsetPatternLong);
            Assert.IsInstanceOf<string>(info.OffsetPatternMedium);
            Assert.IsInstanceOf<string>(info.OffsetPatternShort);
            Assert.AreEqual("NodaFormatInfo[en-US]", info.ToString());
        }

        [Test]
        public void TestConstructor_null()
        {
            var info = new NodaFormatInfo(null);
            Assert.AreEqual(Thread.CurrentThread.CurrentCulture.Name, info.CultureInfo.Name);
        }

        [Test]
        public void TestDateTimeFormat()
        {
            var format = DateTimeFormatInfo.InvariantInfo;
            var info = new NodaFormatInfo(enUs);
            Assert.AreNotEqual(format, info.DateTimeFormat);
            info.DateTimeFormat = format;
            Assert.AreEqual(format, info.DateTimeFormat);
            Assert.Throws<ArgumentNullException>(() => info.DateTimeFormat = null);
            info.IsReadOnly = true;
            Assert.Throws<InvalidOperationException>(() => info.DateTimeFormat = format);
        }

        [Test]
        public void TestGetFormat()
        {
            using (CultureSaver.SetCultures(enGb, FailingCultureInfo.Instance))
            {
                var info = new NodaFormatInfo(enUs);
                Assert.AreSame(info, info.GetFormat(typeof(NodaFormatInfo)));

                var actualNfi = info.GetFormat(typeof(NumberFormatInfo));
                Assert.AreSame(enUs.NumberFormat, actualNfi);
                Assert.AreNotSame(Thread.CurrentThread.CurrentCulture.NumberFormat, actualNfi);

                var actualDtfi = info.GetFormat(typeof(DateTimeFormatInfo));
                Assert.AreSame(enUs.DateTimeFormat, actualDtfi);
                Assert.AreNotSame(Thread.CurrentThread.CurrentCulture.DateTimeFormat, actualDtfi);
            }
        }

        [Test]
        public void TestGetFormatInfo()
        {
            NodaFormatInfo.ClearCache();
            var info1 = NodaFormatInfo.GetFormatInfo(enUs);
            Assert.NotNull(info1);
            Assert.IsTrue(info1.IsReadOnly);

            var info2 = NodaFormatInfo.GetFormatInfo(enUs);
            Assert.AreSame(info1, info2);

            var info3 = NodaFormatInfo.GetFormatInfo(enGb);
            Assert.AreNotSame(info1, info3);
        }

        [Test]
        public void TestGetFormatInfo_null()
        {
            NodaFormatInfo.ClearCache();
            Assert.Throws<ArgumentNullException>(() => NodaFormatInfo.GetFormatInfo(null));
        }

        [Test]
        public void TestGetInstance_CultureInfo()
        {
            NodaFormatInfo.ClearCache();
            using (CultureSaver.SetCultures(enUs, FailingCultureInfo.Instance))
            {
                var actual = NodaFormatInfo.GetInstance(enGb);
                Assert.AreSame(enGb.Name, actual.Name);
            }
        }

        [Test]
        public void TestGetInstance_IFormatProvider()
        {
            NodaFormatInfo.ClearCache();
            using (CultureSaver.SetCultures(enUs, FailingCultureInfo.Instance))
            {
                var provider = new EmptyFormatProvider();
                var actual = NodaFormatInfo.GetInstance(provider);
                Assert.AreSame(enUs.Name, actual.Name);
            }
        }

        [Test]
        public void TestGetInstance_NodaCultureInfo()
        {
            NodaFormatInfo.ClearCache();
            using (CultureSaver.SetCultures(enUs, FailingCultureInfo.Instance))
            {
                var info = new NodaCultureInfo("en-GB");
                var actual = NodaFormatInfo.GetInstance(info);
                Assert.AreSame(info.Name, actual.Name);
            }
        }

        [Test]
        public void TestGetInstance_NodaFormatInfo()
        {
            NodaFormatInfo.ClearCache();
            using (CultureSaver.SetCultures(enUs, FailingCultureInfo.Instance))
            {
                var info = new NodaFormatInfo(enGb);
                var actual = NodaFormatInfo.GetInstance(info);
                Assert.AreSame(info, actual);
            }
        }

        [Test]
        public void TestGetInstance_null()
        {
            NodaFormatInfo.ClearCache();
            using (CultureSaver.SetCultures(enUs, FailingCultureInfo.Instance))
            {
                var info = NodaFormatInfo.GetInstance(null);
                Assert.AreEqual(Thread.CurrentThread.CurrentCulture, info.CultureInfo);
            }
            using (CultureSaver.SetCultures(enGb, FailingCultureInfo.Instance))
            {
                var info = NodaFormatInfo.GetInstance(null);
                Assert.AreEqual(Thread.CurrentThread.CurrentCulture, info.CultureInfo);
            }
        }

        [Test]
        public void TestIsReadOnly()
        {
            var info = new NodaFormatInfo(enUs);
            Assert.IsFalse(info.IsReadOnly);
            info.IsReadOnly = false;
            Assert.IsFalse(info.IsReadOnly);
            info.IsReadOnly = false;
            Assert.IsFalse(info.IsReadOnly);
            info.IsReadOnly = true;
            Assert.IsTrue(info.IsReadOnly);
            info.IsReadOnly = true;
            Assert.IsTrue(info.IsReadOnly);
            Assert.Throws<InvalidOperationException>(() => info.IsReadOnly = false);
        }

        [Test]
        public void TestNumberFormat()
        {
            var format = NumberFormatInfo.InvariantInfo;
            var info = new NodaFormatInfo(enUs);
            Assert.AreNotEqual(format, info.NumberFormat);
            info.NumberFormat = format;
            Assert.AreEqual(format, info.NumberFormat);
            Assert.Throws<ArgumentNullException>(() => info.NumberFormat = null);
            info.IsReadOnly = true;
            Assert.Throws<InvalidOperationException>(() => info.NumberFormat = format);
        }

        [Test]
        public void TestOffsetPatternFull()
        {
            const string pattern = "This is a test";
            var info = new NodaFormatInfo(enUs);
            Assert.AreNotEqual(pattern, info.OffsetPatternFull);
            info.OffsetPatternFull = pattern;
            Assert.AreEqual(pattern, info.OffsetPatternFull);
            Assert.Throws<ArgumentNullException>(() => info.OffsetPatternFull = null);
            info.IsReadOnly = true;
            Assert.Throws<InvalidOperationException>(() => info.OffsetPatternFull = "abc");
        }

        [Test]
        public void TestOffsetPatternLong()
        {
            const string pattern = "This is a test";
            var info = new NodaFormatInfo(enUs);
            Assert.AreNotEqual(pattern, info.OffsetPatternLong);
            info.OffsetPatternLong = pattern;
            Assert.AreEqual(pattern, info.OffsetPatternLong);
            Assert.Throws<ArgumentNullException>(() => info.OffsetPatternLong = null);
            info.IsReadOnly = true;
            Assert.Throws<InvalidOperationException>(() => info.OffsetPatternLong = "abc");
        }

        [Test]
        public void TestOffsetPatternMedium()
        {
            const string pattern = "This is a test";
            var info = new NodaFormatInfo(enUs);
            Assert.AreNotEqual(pattern, info.OffsetPatternMedium);
            info.OffsetPatternMedium = pattern;
            Assert.AreEqual(pattern, info.OffsetPatternMedium);
            Assert.Throws<ArgumentNullException>(() => info.OffsetPatternMedium = null);
            info.IsReadOnly = true;
            Assert.Throws<InvalidOperationException>(() => info.OffsetPatternMedium = "abc");
        }

        [Test]
        public void TestOffsetPatternShort()
        {
            const string pattern = "This is a test";
            var info = new NodaFormatInfo(enUs);
            Assert.AreNotEqual(pattern, info.OffsetPatternShort);
            info.OffsetPatternShort = pattern;
            Assert.AreEqual(pattern, info.OffsetPatternShort);
            Assert.Throws<ArgumentNullException>(() => info.OffsetPatternShort = null);
            info.IsReadOnly = true;
            Assert.Throws<InvalidOperationException>(() => info.OffsetPatternShort = "abc");
        }

        [Test]
        public void TestSetFormatInfo()
        {
            NodaFormatInfo.ClearCache();

            var info1 = NodaFormatInfo.GetFormatInfo(enUs);
            Assert.NotNull(info1);
            Assert.IsTrue(info1.IsReadOnly);

            var info2 = new NodaFormatInfo(enGb);
            Assert.IsFalse(info2.IsReadOnly);
            NodaFormatInfo.SetFormatInfo(enUs, info2);
            Assert.IsTrue(info2.IsReadOnly);

            var info3 = NodaFormatInfo.GetFormatInfo(enUs);
            Assert.NotNull(info3);
            Assert.IsTrue(info3.IsReadOnly);
            Assert.AreSame(info2, info3);
            Assert.AreNotSame(info1, info3);

            NodaFormatInfo.SetFormatInfo(enUs, null);
            var info4 = NodaFormatInfo.GetFormatInfo(enUs);
            Assert.NotNull(info4);
            Assert.IsTrue(info4.IsReadOnly);
            Assert.AreNotSame(info1, info4);
            Assert.AreSame(info1.CultureInfo, info4.CultureInfo);
        }

        [Test]
        public void TestSetFormatInfo_failure()
        {
            NodaFormatInfo.ClearCache();
            Assert.Throws<ArgumentNullException>(() => NodaFormatInfo.SetFormatInfo(null, null));
        }

        [Test]
        public void TestGetEraNames()
        {
            var info = NodaFormatInfo.GetFormatInfo(enUs);
            IList<string> names = info.GetEraNames(Era.BeforeCommon);
            CollectionAssert.AreEqual(new[] { "B.C.E.", "B.C.", "BCE", "BC" }, names);
        }

        [Test]
        public void TestGetEraNames_NoSuchEra()
        {
            var info = NodaFormatInfo.GetFormatInfo(enUs);
            Assert.AreEqual(0, info.GetEraNames(new Era("Ignored", "NonExistantResource")).Count);
        }

        [Test]
        public void TestEraGetNames_Null()
        {
            var info = NodaFormatInfo.GetFormatInfo(enUs);
            Assert.Throws<ArgumentNullException>(() => info.GetEraNames(null));
        }

        [Test]
        public void TestGetEraPrimaryName()
        {
            var info = NodaFormatInfo.GetFormatInfo(enUs);
            Assert.AreEqual("B.C.", info.GetEraPrimaryName(Era.BeforeCommon));
        }

        [Test]
        public void TestGetEraPrimaryName_NoSuchEra()
        {
            var info = NodaFormatInfo.GetFormatInfo(enUs);
            Assert.AreEqual("", info.GetEraPrimaryName(new Era("Ignored", "NonExistantResource")));
        }

        [Test]
        public void TestEraGetEraPrimaryName_Null()
        {
            var info = NodaFormatInfo.GetFormatInfo(enUs);
            Assert.Throws<ArgumentNullException>(() => info.GetEraPrimaryName(null));
        }
    }
}
