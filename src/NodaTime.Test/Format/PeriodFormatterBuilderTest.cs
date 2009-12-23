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
using NodaTime.Format;
using System.Globalization;
using NodaTime.Periods;

namespace NodaTime.Test.Format
{
    [TestFixture]
    public partial class PeriodFormatterBuilderTest
    {
        PeriodFormatterBuilder builder;
        Period zeroPeriod;
        Period standartPeriod;
        Period timePeriod;
        Period datePeriod;
        Period emptyYearDayTimePeriod;
        Period fullYearDayTimePeriod;

        [SetUp]
        public void Init()
        {
            builder = new PeriodFormatterBuilder();
            zeroPeriod = new Period(0, 0, 0, 0, 0, 0, 0, 0);
            standartPeriod = new Period(1, 2, 3, 4, 5, 6, 7, 8);
            timePeriod = new Period(0, 0, 0, 0, 5, 6, 7, 8);
            datePeriod = new Period(1, 2, 3, 4, 0, 0, 0, 0);
            emptyYearDayTimePeriod = new Period(0, 0, 0, 0, 0, 0, 0, 0, PeriodType.YearDayTime);
            fullYearDayTimePeriod = new Period(1, 0, 0, 4, 5, 6, 7, 8, PeriodType.YearDayTime);
        }

        [Test]
        public void AppendYears_BuildsCorrectPrinter_For1YearStandartPeriod()
        {
            var formatter = builder.AppendYears().ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(standartPeriod);

            Assert.AreEqual("1", printedValue);
            Assert.AreEqual(1, printer.CalculatePrintedLength(standartPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(standartPeriod, int.MaxValue, null));            
        }

        [Test]
        public void AppendYears_BuildsCorrectPrinter_ForZeroYearStandartPeriod()
        {
            var formatter = builder.AppendYears().ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(zeroPeriod);

            Assert.AreEqual("0", printedValue);
            Assert.AreEqual(1, printer.CalculatePrintedLength(zeroPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(zeroPeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendMonths_BuildsCorrectPrinter_For2MonthStandartPeriod()
        {
            var formatter = builder.AppendMonths().ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(standartPeriod);

            Assert.AreEqual("2", printedValue);
            Assert.AreEqual(1, printer.CalculatePrintedLength(standartPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(standartPeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendMonths_BuildsCorrectPrinter_ForzeroMonthStandartPeriod()
        {
            var formatter = builder.AppendMonths().ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(zeroPeriod);

            Assert.AreEqual("0", printedValue);
            Assert.AreEqual(1, printer.CalculatePrintedLength(zeroPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(zeroPeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendWeeks_BuildsCorrectPrinter_For3WeeksStandartPeriod()
        {
            var formatter = builder.AppendWeeks().ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(standartPeriod);

            Assert.AreEqual("3", printedValue);
            Assert.AreEqual(1, printer.CalculatePrintedLength(standartPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(standartPeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendWeeks_BuildsCorrectPrinter_ForZeroWeeksStandartPeriod()
        {
            var formatter = builder.AppendWeeks().ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(zeroPeriod);

            Assert.AreEqual("0", printedValue);
            Assert.AreEqual(1, printer.CalculatePrintedLength(zeroPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(zeroPeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendDays_BuildsCorrectPrinter_For4DaysStandartPeriod()
        {
            var formatter = builder.AppendDays().ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(standartPeriod);

            Assert.AreEqual("4", printedValue);
            Assert.AreEqual(1, printer.CalculatePrintedLength(standartPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(standartPeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendDays_BuildsCorrectPrinter_ForZeroDaysStandartPeriod()
        {
            var formatter = builder.AppendDays().ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(zeroPeriod);

            Assert.AreEqual("0", printedValue);
            Assert.AreEqual(1, printer.CalculatePrintedLength(zeroPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(zeroPeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendHours_BuildsCorrectPrinter_For5HoursStandartPeriod()
        {
            var formatter = builder.AppendHours().ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(standartPeriod);

            Assert.AreEqual("5", printedValue);
            Assert.AreEqual(1, printer.CalculatePrintedLength(standartPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(standartPeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendHours_BuildsCorrectPrinter_ForZeroHoursStandartPeriod()
        {
            var formatter = builder.AppendHours().ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(zeroPeriod);

            Assert.AreEqual("0", printedValue);
            Assert.AreEqual(1, printer.CalculatePrintedLength(zeroPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(zeroPeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendMinutes_BuildsCorrectPrinter_For6MinutesStandartPeriod()
        {
            var formatter = builder.AppendMinutes().ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(standartPeriod);

            Assert.AreEqual("6", printedValue);
            Assert.AreEqual(1, printer.CalculatePrintedLength(standartPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(standartPeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendMinutes_BuildsCorrectPrinter_ForZeroMinutesStandartPeriod()
        {
            var formatter = builder.AppendMinutes().ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(zeroPeriod);

            Assert.AreEqual("0", printedValue);
            Assert.AreEqual(1, printer.CalculatePrintedLength(zeroPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(zeroPeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendSeconds_BuildsCorrectPrinter_For7SecondsStandartPeriod()
        {
            var formatter = builder.AppendSeconds().ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(standartPeriod);

            Assert.AreEqual("7", printedValue);
            Assert.AreEqual(1, printer.CalculatePrintedLength(standartPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(standartPeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendSeconds_BuildsCorrectPrinter_ForZeroSecondsStandartPeriod()
        {
            var formatter = builder.AppendSeconds().ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(zeroPeriod);

            Assert.AreEqual("0", printedValue);
            Assert.AreEqual(1, printer.CalculatePrintedLength(zeroPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(zeroPeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendMilliseconds_BuildsCorrectPrinter_For8MillisecondsStandartPeriod()
        {
            var formatter = builder.AppendMillis().ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(standartPeriod);

            Assert.AreEqual("8", printedValue);
            Assert.AreEqual(1, printer.CalculatePrintedLength(standartPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(standartPeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendMilliseconds_BuildsCorrectPrinter_ForZeroMillisecondsStandartPeriod()
        {
            var formatter = builder.AppendMillis().ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(zeroPeriod);

            Assert.AreEqual("0", printedValue);
            Assert.AreEqual(1, printer.CalculatePrintedLength(zeroPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(zeroPeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendMilliseconds3Digit_BuildsCorrectPrinter_For8MillisecondsStandartPeriod()
        {
            var formatter = builder.AppendMillis3Digit().ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(standartPeriod);

            Assert.AreEqual("008", printedValue);
            Assert.AreEqual(3, printer.CalculatePrintedLength(standartPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(standartPeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendMilliseconds3Digit_BuildsCorrectPrinter_ForZeroMillisecondsStandartPeriod()
        {
            var formatter = builder.AppendMillis3Digit().ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(zeroPeriod);

            Assert.AreEqual("000", printedValue);
            Assert.AreEqual(3, printer.CalculatePrintedLength(zeroPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(zeroPeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendPrefixBeforeYears_BuildsCorrectPrinter_For1YearsStandartPeriod()
        {
            var formatter = builder.AppendPrefix("Years:").AppendYears().ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(standartPeriod);

            Assert.AreEqual("Years:1", printedValue);
            Assert.AreEqual(7, printer.CalculatePrintedLength(standartPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(standartPeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendPrefixBeforeYears_BuildsCorrectPrinter_ForZeroYearsStandartPeriod()
        {
            var formatter = builder.AppendPrefix("Years:").AppendYears().ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(zeroPeriod);

            Assert.AreEqual("Years:0", printedValue);
            Assert.AreEqual(7, printer.CalculatePrintedLength(zeroPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(zeroPeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendPrefixBeforeMonths_BuildsCorrectPrinter_For2MOnthsStandartPeriod()
        {
            var formatter = builder.AppendPrefix("Months:").AppendMonths().ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(standartPeriod);

            Assert.AreEqual("Months:2", printedValue);
            Assert.AreEqual(8, printer.CalculatePrintedLength(standartPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(standartPeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendPrefixBeforeMonths_BuildsCorrectPrinter_ForZeroMonthsStandartPeriod()
        {
            var formatter = builder.AppendPrefix("Months:").AppendMonths().ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(zeroPeriod);

            Assert.AreEqual("Months:0", printedValue);
            Assert.AreEqual(8, printer.CalculatePrintedLength(zeroPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(zeroPeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendPrefixNull_ThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() => builder.AppendPrefix(null));
        }

        [Test]
        public void AppendPrefixPluralBeforeYears_BuildsCorrectPrinter_ForSingularYearsStandartPeriod()
        {
            var formatter = builder.AppendPrefix("Year:", "Years:").AppendYears().ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(standartPeriod);

            Assert.AreEqual("Year:1", printedValue);
            Assert.AreEqual(6, printer.CalculatePrintedLength(standartPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(standartPeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendPrefixPluralBeforeYears_BuildsCorrectPrinter_ForPluralYearsStandartPeriod()
        {
            var formatter = builder.AppendPrefix("Year:", "Years:").AppendYears().ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(zeroPeriod);

            Assert.AreEqual("Years:0", printedValue);
            Assert.AreEqual(7, printer.CalculatePrintedLength(zeroPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(zeroPeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendPrefixPluralBeforeHours_BuildsCorrectPrinter_For5HoursStandartPeriod()
        {
            var formatter = builder.AppendPrefix("Hour:", "Hours:").AppendMinutes().ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(standartPeriod);

            Assert.AreEqual("Hours:6", printedValue);
            Assert.AreEqual(7, printer.CalculatePrintedLength(standartPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(standartPeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendPrefixPluralBeforeHours_BuildsCorrectPrinter_ForZeroHoursStandartPeriod()
        {
            var formatter = builder.AppendPrefix("Hour:", "Hours:").AppendMinutes().ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(zeroPeriod);

            Assert.AreEqual("Hours:0", printedValue);
            Assert.AreEqual(7, printer.CalculatePrintedLength(zeroPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(zeroPeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendPrefixPluralNull_ThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() => builder.AppendPrefix("prefix", null));
            Assert.Throws<ArgumentNullException>(() => builder.AppendPrefix(null, "prefix"));
            Assert.Throws<ArgumentNullException>(() => builder.AppendPrefix(null, null));

        }

        [Test]
        public void AppendSuffixAfterYears_BuildsCorrectPrinter_For1YearsStandartPeriod()
        {
            var formatter = builder.AppendYears().AppendSuffix(" years").ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(standartPeriod);

            Assert.AreEqual("1 years", printedValue);
            Assert.AreEqual(7, printer.CalculatePrintedLength(standartPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(standartPeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendSuffixAfterYears_BuildsCorrectPrinter_ForZeroYearsStandartPeriod()
        {
            var formatter = builder.AppendYears().AppendSuffix(" years").ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(zeroPeriod);

            Assert.AreEqual("0 years", printedValue);
            Assert.AreEqual(7, printer.CalculatePrintedLength(zeroPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(zeroPeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendSuffixAfterHours_BuildsCorrectPrinter_For5HoursStandartPeriod()
        {
            var formatter = builder.AppendHours().AppendSuffix(" hours").ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(standartPeriod);

            Assert.AreEqual("5 hours", printedValue);
            Assert.AreEqual(7, printer.CalculatePrintedLength(standartPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(standartPeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendSuffixAfterHours_BuildsCorrectPrinter_ForZeroHoursStandartPeriod()
        {
            var formatter = builder.AppendHours().AppendSuffix(" hours").ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(zeroPeriod);

            Assert.AreEqual("0 hours", printedValue);
            Assert.AreEqual(7, printer.CalculatePrintedLength(zeroPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(zeroPeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendSuffixNull_ThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() => builder.AppendSuffix(null));
        }

        [Test]
        public void AppendSuffixWithoutFieldBefore_ThrowsInvalidOperation()
        {
            Assert.Throws<InvalidOperationException>(() => builder.AppendSuffix(" years"));
        }

        [Test]
        public void AppendSuffixPluralAfterYears_BuildsCorrectPrinter_For1YearsStandartPeriod()
        {
            var formatter = builder.AppendYears().AppendSuffix(" year", " years").ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(standartPeriod);

            Assert.AreEqual("1 year", printedValue);
            Assert.AreEqual(6, printer.CalculatePrintedLength(standartPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(standartPeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendSuffixPluralAfterYears_BuildsCorrectPrinter_ForZeroYearsStandartPeriod()
        {
            var formatter = builder.AppendYears().AppendSuffix(" year", " years").ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(zeroPeriod);

            Assert.AreEqual("0 years", printedValue);
            Assert.AreEqual(7, printer.CalculatePrintedLength(zeroPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(zeroPeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendSuffixPluralAfterHours_BuildsCorrectPrinter_For5HoursStandartPeriod()
        {
            var formatter = builder.AppendHours().AppendSuffix(" hour", " hours").ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(standartPeriod);

            Assert.AreEqual("5 hours", printedValue);
            Assert.AreEqual(7, printer.CalculatePrintedLength(standartPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(standartPeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendSuffixPluralAfterHours_BuildsCorrectPrinter_ForZeroHoursStandartPeriod()
        {
            var formatter = builder.AppendHours().AppendSuffix(" hour", " hours").ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(zeroPeriod);

            Assert.AreEqual("0 hours", printedValue);
            Assert.AreEqual(7, printer.CalculatePrintedLength(zeroPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(zeroPeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendSuffixPluralNull_ThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() => builder.AppendSuffix("_", null));
            Assert.Throws<ArgumentNullException>(() => builder.AppendSuffix(null, "_"));
            Assert.Throws<ArgumentNullException>(() => builder.AppendSuffix(null, null));
        }

        [Test]
        public void AppendSuffixPluralWithoutFieldBefore_ThrowsInvalidOperation()
        {
            Assert.Throws<InvalidOperationException>(() => builder.AppendSuffix("_", "_"));
        }

        [Test]
        public void AppendPrefixAndSuffixOnYears_BuildsCorrectPrinter_For1YearsStandartPeriod()
        {
            var formatter = builder.AppendPrefix("P").AppendYears().AppendSuffix("Y").ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(standartPeriod);

            Assert.AreEqual("P1Y", printedValue);
            Assert.AreEqual(3, printer.CalculatePrintedLength(standartPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(standartPeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendPrefixAndSuffixOnYears_BuildsCorrectPrinter_ForZeroYearsStandartPeriod()
        {
            var formatter = builder.AppendPrefix("P").AppendYears().AppendSuffix("Y").ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(zeroPeriod);

            Assert.AreEqual("P0Y", printedValue);
            Assert.AreEqual(3, printer.CalculatePrintedLength(zeroPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(zeroPeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendSeparatorBetweenYearsAndHours_BuildsCorrectPrinter_ForStandartPeriod()
        {
            var formatter = builder.AppendYears().AppendSeparator("T").AppendHours().ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(standartPeriod);

            Assert.AreEqual("1T5", printedValue);
            Assert.AreEqual(3, printer.CalculatePrintedLength(standartPeriod, null));
            Assert.AreEqual(2, printer.CountFieldsToPrint(standartPeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendSeparatorBetweenYearsAndHours_BuildsCorrectPrinter_ForTimePeriod()
        {
            var formatter = builder.AppendYears().AppendSeparator("T").AppendHours().ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(timePeriod);

            Assert.AreEqual("5", printedValue);
            Assert.AreEqual(1, printer.CalculatePrintedLength(timePeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(timePeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendSeparatorBetweenYearsAndHours_BuildsCorrectPrinter_ForDatePeriod()
        {
            var formatter = builder.AppendYears().AppendSeparator("T").AppendHours().ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(datePeriod);

            Assert.AreEqual("1", printedValue);
            Assert.AreEqual(1, printer.CalculatePrintedLength(datePeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(datePeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendSeparatorFinalText_BuildsCorrectPrinter_ForStandartPeriod()
        {
            var formatter = builder
                .AppendYears().AppendSeparator(", ", " and ")
                .AppendHours().AppendSeparator(", ", " and ")
                .AppendMinutes().AppendSeparator(", ", " and ")
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(standartPeriod);

            Assert.AreEqual("1, 5 and 6", printedValue);
            Assert.AreEqual(10, printer.CalculatePrintedLength(standartPeriod, null));
            Assert.AreEqual(3, printer.CountFieldsToPrint(standartPeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendSeparatorFinalText_BuildsCorrectPrinter_ForTimePeriod()
        {
            var formatter = builder
                .AppendYears().AppendSeparator(", ", " and ")
                .AppendHours().AppendSeparator(", ", " and ")
                .AppendMinutes().AppendSeparator(", ", " and ")
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(timePeriod);

            Assert.AreEqual("5 and 6", printedValue);
            Assert.AreEqual(7, printer.CalculatePrintedLength(timePeriod, null));
            Assert.AreEqual(2, printer.CountFieldsToPrint(timePeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendSeparatorFinalText_BuildsCorrectPrinter_ForDatePeriod()
        {
            var formatter = builder
                .AppendYears().AppendSeparator(", ", " and ")
                .AppendHours().AppendSeparator(", ", " and ")
                .AppendMinutes().AppendSeparator(", ", " and ")
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(datePeriod);

            Assert.AreEqual("1", printedValue);
            Assert.AreEqual(1, printer.CalculatePrintedLength(datePeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(datePeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendSeparatorIfFieldsAfter_BuildsCorrectPrinter_ForStandartPeriod()
        {
            var formatter = builder
                .AppendYears().AppendSeparatorIfFieldsAfter("T")
                .AppendHours()
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(standartPeriod);

            Assert.AreEqual("1T5", printedValue);
            Assert.AreEqual(3, printer.CalculatePrintedLength(standartPeriod, null));
            Assert.AreEqual(2, printer.CountFieldsToPrint(standartPeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendSeparatorIfFieldsAfter_BuildsCorrectPrinter_ForTimePeriod()
        {
            var formatter = builder
                .AppendYears().AppendSeparatorIfFieldsAfter("T")
                .AppendHours()
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(timePeriod);

            Assert.AreEqual("T5", printedValue);
            Assert.AreEqual(2, printer.CalculatePrintedLength(timePeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(timePeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendSeparatorIfFieldsAfter_BuildsCorrectPrinter_ForDatePeriod()
        {
            var formatter = builder
                .AppendYears().AppendSeparatorIfFieldsAfter("T")
                .AppendHours()
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(datePeriod);

            Assert.AreEqual("1", printedValue);
            Assert.AreEqual(1, printer.CalculatePrintedLength(datePeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(datePeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendSeparatorIfFieldsBefore_BuildsCorrectPrinter_ForStandartPeriod()
        {
            var formatter = builder
                .AppendYears().AppendSeparatorIfFieldsBefore("T")
                .AppendHours()
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(standartPeriod);

            Assert.AreEqual("1T5", printedValue);
            Assert.AreEqual(3, printer.CalculatePrintedLength(standartPeriod, null));
            Assert.AreEqual(2, printer.CountFieldsToPrint(standartPeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendSeparatorIfFieldsBefore_BuildsCorrectPrinter_ForTimePeriod()
        {
            var formatter = builder
                .AppendYears().AppendSeparatorIfFieldsBefore("T")
                .AppendHours()
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(timePeriod);

            Assert.AreEqual("5", printedValue);
            Assert.AreEqual(1, printer.CalculatePrintedLength(timePeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(timePeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendSeparatorIfFieldsBefore_BuildsCorrectPrinter_ForDatePeriod()
        {
            var formatter = builder
                .AppendYears().AppendSeparatorIfFieldsBefore("T")
                .AppendHours()
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(datePeriod);

            Assert.AreEqual("1T", printedValue);
            Assert.AreEqual(2, printer.CalculatePrintedLength(datePeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(datePeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendLiteral_BuildsCorrectPrinter_ForStandartPeriod()
        {
            var formatter = builder
                .AppendLiteral("HELLO")
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(standartPeriod);

            Assert.AreEqual("HELLO", printedValue);
            Assert.AreEqual(5, printer.CalculatePrintedLength(standartPeriod, null));
            Assert.AreEqual(0, printer.CountFieldsToPrint(standartPeriod, int.MaxValue, null));
        }

        [Test]
        public void AppendFormatter_BuildsCorrectPrinter_ForStandartPeriod()
        {
            var baseFormatter = builder
                .AppendYears()
                .AppendLiteral("-")
                .ToFormatter();

            var formatter = new PeriodFormatterBuilder()
                .Append(baseFormatter)
                .AppendYears()
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(standartPeriod);

            Assert.AreEqual("1-1", printedValue);
            Assert.AreEqual(3, printer.CalculatePrintedLength(standartPeriod, null));
            Assert.AreEqual(2, printer.CountFieldsToPrint(standartPeriod, int.MaxValue, null));
        }

        #region ZeroDefault

        [Test]
        public void ZeroDefault_BuildsCorrectPrinter_ForStandartPeriod()
        {
            var formatter = builder
                .AppendYears().AppendLiteral("-")
                .AppendMonths().AppendLiteral("-")
                .AppendWeeks().AppendLiteral("-")
                .AppendDays()
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(standartPeriod);

            Assert.AreEqual("1-2-3-4", printedValue);
            Assert.AreEqual(7, printer.CalculatePrintedLength(standartPeriod, null));
            Assert.AreEqual(4, printer.CountFieldsToPrint(standartPeriod, int.MaxValue, null));
        }

        [Test]
        public void ZeroDefault_BuildsCorrectPrinter_ForEmptyYearDayTimePeriod()
        {
            var formatter = builder
                .AppendYears().AppendLiteral("-")
                .AppendMonths().AppendLiteral("-")
                .AppendWeeks().AppendLiteral("-")
                .AppendDays()
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(emptyYearDayTimePeriod);

            Assert.AreEqual("---0", printedValue);
            Assert.AreEqual(4, printer.CalculatePrintedLength(emptyYearDayTimePeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(emptyYearDayTimePeriod, int.MaxValue, null));
        }

        [Test]
        public void ZeroDefault_BuildsCorrectPrinter_ForFullYearDayTimePeriod()
        {
            var formatter = builder
                .AppendYears().AppendLiteral("-")
                .AppendMonths().AppendLiteral("-")
                .AppendWeeks().AppendLiteral("-")
                .AppendDays()
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(fullYearDayTimePeriod);

            Assert.AreEqual("1---4", printedValue);
            Assert.AreEqual(5, printer.CalculatePrintedLength(fullYearDayTimePeriod, null));
            Assert.AreEqual(2, printer.CountFieldsToPrint(fullYearDayTimePeriod, int.MaxValue, null));
        }

        [Test]
        public void ZeroDefault_BuildsCorrectPrinter_ForEmptyStandartPeriod()
        {
            var formatter = builder
                .AppendYears().AppendLiteral("-")
                .AppendMonths().AppendLiteral("-")
                .AppendWeeks().AppendLiteral("-")
                .AppendDays()
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(zeroPeriod);

            Assert.AreEqual("---0", printedValue);
            Assert.AreEqual(4, printer.CalculatePrintedLength(zeroPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(zeroPeriod, int.MaxValue, null));
        }

        [Test]
        public void ZeroDefault_BuildsCorrectPrinter_ForEmptyStandartPeriod2YearsField()
        {
            var formatter = builder
                .AppendYears().AppendLiteral("-")
                .AppendYears()
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(zeroPeriod);

            Assert.AreEqual("-0", printedValue);
            Assert.AreEqual(2, printer.CalculatePrintedLength(zeroPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(zeroPeriod, int.MaxValue, null));
        }

        #endregion

        #region ZeroRarelyLast

        [Test]
        public void ZeroRarelyLast_PrintsYearsMonthsWeeksDays_ForFullStandartPeriod()
        {
            var formatter = builder
                .PrintZeroRarelyLast()
                .AppendYears().AppendLiteral("-")
                .AppendMonths().AppendLiteral("-")
                .AppendWeeks().AppendLiteral("-")
                .AppendDays()
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(standartPeriod);

            Assert.AreEqual("1-2-3-4", printedValue);
            Assert.AreEqual(7, printer.CalculatePrintedLength(standartPeriod, null));
            Assert.AreEqual(4, printer.CountFieldsToPrint(standartPeriod, int.MaxValue, null));
        }

        [Test]
        public void ZeroRarelyLast_PrintsYearsMonthsWeeksDays_ForEmptyYearDayTimePeriod()
        {
            var formatter = builder
                .PrintZeroRarelyLast()
                .AppendYears().AppendLiteral("-")
                .AppendMonths().AppendLiteral("-")
                .AppendWeeks().AppendLiteral("-")
                .AppendDays()
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(emptyYearDayTimePeriod);

            Assert.AreEqual("---0", printedValue);
            Assert.AreEqual(4, printer.CalculatePrintedLength(emptyYearDayTimePeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(emptyYearDayTimePeriod, int.MaxValue, null));
        }

        [Test]
        public void ZeroRarelyLast_PrintsYearsMonthsWeeksDays_ForFullYearDayTimePeriod()
        {
            var formatter = builder
                .PrintZeroRarelyLast()
                .AppendYears().AppendLiteral("-")
                .AppendMonths().AppendLiteral("-")
                .AppendWeeks().AppendLiteral("-")
                .AppendDays()
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(fullYearDayTimePeriod);

            Assert.AreEqual("1---4", printedValue);
            Assert.AreEqual(5, printer.CalculatePrintedLength(fullYearDayTimePeriod, null));
            Assert.AreEqual(2, printer.CountFieldsToPrint(fullYearDayTimePeriod, int.MaxValue, null));
        }

        [Test]
        public void ZeroRarelyLast_PrintsYearsMonthsWeeksDays_ForEmptyStandartPeriod()
        {
            var formatter = builder
                .PrintZeroRarelyLast()
                .AppendYears().AppendLiteral("-")
                .AppendMonths().AppendLiteral("-")
                .AppendWeeks().AppendLiteral("-")
                .AppendDays()
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(zeroPeriod);

            Assert.AreEqual("---0", printedValue);
            Assert.AreEqual(4, printer.CalculatePrintedLength(zeroPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(zeroPeriod, int.MaxValue, null));
        }

        #endregion

        #region ZeroRarelyFirst

        [Test]
        public void ZeroRarelyFirst_PrintsYearsMonthsWeeksDays_ForFullStandartPeriod()
        {
            var formatter = builder
                .PrintZeroRarelyFirst()
                .AppendYears().AppendLiteral("-")
                .AppendMonths().AppendLiteral("-")
                .AppendWeeks().AppendLiteral("-")
                .AppendDays()
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(standartPeriod);

            Assert.AreEqual("1-2-3-4", printedValue);
            Assert.AreEqual(7, printer.CalculatePrintedLength(standartPeriod, null));
            Assert.AreEqual(4, printer.CountFieldsToPrint(standartPeriod, int.MaxValue, null));
        }

        [Test]
        public void ZeroRarelyFirst_PrintsYearsMonthsWeeksDays_ForEmptyYearDayTimePeriod()
        {
            var formatter = builder
                .PrintZeroRarelyFirst()
                .AppendYears().AppendLiteral("-")
                .AppendMonths().AppendLiteral("-")
                .AppendWeeks().AppendLiteral("-")
                .AppendDays()
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(emptyYearDayTimePeriod);

            Assert.AreEqual("0---", printedValue);
            Assert.AreEqual(4, printer.CalculatePrintedLength(emptyYearDayTimePeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(emptyYearDayTimePeriod, int.MaxValue, null));
        }

        [Test]
        public void ZeroRarelyFirst_PrintsYearsMonthsWeeksDays_ForFullYearDayTimePeriod()
        {
            var formatter = builder
                .PrintZeroRarelyFirst()
                .AppendYears().AppendLiteral("-")
                .AppendMonths().AppendLiteral("-")
                .AppendWeeks().AppendLiteral("-")
                .AppendDays()
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(fullYearDayTimePeriod);

            Assert.AreEqual("1---4", printedValue);
            Assert.AreEqual(5, printer.CalculatePrintedLength(fullYearDayTimePeriod, null));
            Assert.AreEqual(2, printer.CountFieldsToPrint(fullYearDayTimePeriod, int.MaxValue, null));
        }

        [Test]
        public void ZeroRarelyFirst_PrintsYearsMonthsWeeksDays_ForEmptyStandartPeriod()
        {
            var formatter = builder
                .PrintZeroRarelyFirst()
                .AppendYears().AppendLiteral("-")
                .AppendMonths().AppendLiteral("-")
                .AppendWeeks().AppendLiteral("-")
                .AppendDays()
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(zeroPeriod);

            Assert.AreEqual("0---", printedValue);
            Assert.AreEqual(4, printer.CalculatePrintedLength(zeroPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(zeroPeriod, int.MaxValue, null));
        }

        [Test]
        public void ZeroRarelyFirst_PrintsYears_ForEmptyStandartPeriod()
        {
            var formatter = builder
                .PrintZeroRarelyFirst()
                .AppendYears()
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(zeroPeriod);

            Assert.AreEqual("0", printedValue);
            Assert.AreEqual(1, printer.CalculatePrintedLength(zeroPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(zeroPeriod, int.MaxValue, null));
        }

        [Test]
        public void ZeroRarelyFirst_PrintsMonths_ForEmptyStandartPeriod()
        {
            var formatter = builder
                .PrintZeroRarelyFirst()
                .AppendMonths()
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(zeroPeriod);

            Assert.AreEqual("0", printedValue);
            Assert.AreEqual(1, printer.CalculatePrintedLength(zeroPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(zeroPeriod, int.MaxValue, null));
        }

        [Test]
        public void ZeroRarelyFirst_PrintsWeeks_ForEmptyStandartPeriod()
        {
            var formatter = builder
                .PrintZeroRarelyFirst()
                .AppendWeeks()
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(zeroPeriod);

            Assert.AreEqual("0", printedValue);
            Assert.AreEqual(1, printer.CalculatePrintedLength(zeroPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(zeroPeriod, int.MaxValue, null));
        }

        [Test]
        public void ZeroRarelyFirst_PrintsHours_ForEmptyStandartPeriod()
        {
            var formatter = builder
                .PrintZeroRarelyFirst()
                .AppendHours()
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(zeroPeriod);

            Assert.AreEqual("0", printedValue);
            Assert.AreEqual(1, printer.CalculatePrintedLength(zeroPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(zeroPeriod, int.MaxValue, null));
        }

        [Test]
        public void ZeroRarelyFirst_PrintsMinutes_ForEmptyStandartPeriod()
        {
            var formatter = builder
                .PrintZeroRarelyFirst()
                .AppendMinutes()
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(zeroPeriod);

            Assert.AreEqual("0", printedValue);
            Assert.AreEqual(1, printer.CalculatePrintedLength(zeroPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(zeroPeriod, int.MaxValue, null));
        }

        [Test]
        public void ZeroRarelyFirst_PrintsSeconds_ForEmptyStandartPeriod()
        {
            var formatter = builder
                .PrintZeroRarelyFirst()
                .AppendSeconds()
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(zeroPeriod);

            Assert.AreEqual("0", printedValue);
            Assert.AreEqual(1, printer.CalculatePrintedLength(zeroPeriod, null));
            Assert.AreEqual(1, printer.CountFieldsToPrint(zeroPeriod, int.MaxValue, null));
        }

        #endregion

        #region ZeroIfSupported

        [Test]
        public void ZeroIfSupported_PrintsYearsMonthsWeeksDays_ForFullStandartPeriod()
        {
            var formatter = builder
                .PrintZeroIfSupported()
                .AppendYears().AppendLiteral("-")
                .AppendMonths().AppendLiteral("-")
                .AppendWeeks().AppendLiteral("-")
                .AppendDays()
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(standartPeriod);

            Assert.AreEqual("1-2-3-4", printedValue);
            Assert.AreEqual(7, printer.CalculatePrintedLength(standartPeriod, null));
            Assert.AreEqual(4, printer.CountFieldsToPrint(standartPeriod, int.MaxValue, null));
        }

        [Test]
        public void ZeroIfSupported_PrintsYearsMonthsWeeksDays_ForEmptyYearDayTimePeriod()
        {
            var formatter = builder
                .PrintZeroIfSupported()
                .AppendYears().AppendLiteral("-")
                .AppendMonths().AppendLiteral("-")
                .AppendWeeks().AppendLiteral("-")
                .AppendDays()
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(emptyYearDayTimePeriod);

            Assert.AreEqual("0---0", printedValue);
            Assert.AreEqual(5, printer.CalculatePrintedLength(emptyYearDayTimePeriod, null));
            Assert.AreEqual(2, printer.CountFieldsToPrint(emptyYearDayTimePeriod, int.MaxValue, null));
        }

        [Test]
        public void ZeroIfSupported_PrintsYearsMonthsWeeksDays_ForFullYearDayTimePeriod()
        {
            var formatter = builder
                .PrintZeroIfSupported()
                .AppendYears().AppendLiteral("-")
                .AppendMonths().AppendLiteral("-")
                .AppendWeeks().AppendLiteral("-")
                .AppendDays()
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(fullYearDayTimePeriod);

            Assert.AreEqual("1---4", printedValue);
            Assert.AreEqual(5, printer.CalculatePrintedLength(fullYearDayTimePeriod, null));
            Assert.AreEqual(2, printer.CountFieldsToPrint(fullYearDayTimePeriod, int.MaxValue, null));
        }

        [Test]
        public void ZeroIfSupported_PrintsYearsMonthsWeeksDays_ForEmptyStandartPeriod()
        {
            var formatter = builder
                .PrintZeroIfSupported()
                .AppendYears().AppendLiteral("-")
                .AppendMonths().AppendLiteral("-")
                .AppendWeeks().AppendLiteral("-")
                .AppendDays()
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(zeroPeriod);

            Assert.AreEqual("0-0-0-0", printedValue);
            Assert.AreEqual(7, printer.CalculatePrintedLength(zeroPeriod, null));
            Assert.AreEqual(4, printer.CountFieldsToPrint(zeroPeriod, int.MaxValue, null));
        }

        #endregion

        #region ZeroAlways

        [Test]
        public void ZeroAlways_PrintsYearsMonthsWeeksDays_ForFullStandartPeriod()
        {
            var formatter = builder
                .PrintZeroAlways()
                .AppendYears().AppendLiteral("-")
                .AppendMonths().AppendLiteral("-")
                .AppendWeeks().AppendLiteral("-")
                .AppendDays()
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(standartPeriod);

            Assert.AreEqual("1-2-3-4", printedValue);
            Assert.AreEqual(7, printer.CalculatePrintedLength(standartPeriod, null));
            Assert.AreEqual(4, printer.CountFieldsToPrint(standartPeriod, int.MaxValue, null));
        }

        [Test]
        public void ZeroAlways_PrintsYearsMonthsWeeksDays_ForEmptyYearDayTimePeriod()
        {
            var formatter = builder
                .PrintZeroAlways()
                .AppendYears().AppendLiteral("-")
                .AppendMonths().AppendLiteral("-")
                .AppendWeeks().AppendLiteral("-")
                .AppendDays()
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(emptyYearDayTimePeriod);

            Assert.AreEqual("0-0-0-0", printedValue);
            Assert.AreEqual(7, printer.CalculatePrintedLength(emptyYearDayTimePeriod, null));
            Assert.AreEqual(4, printer.CountFieldsToPrint(emptyYearDayTimePeriod, int.MaxValue, null));
        }

        [Test]
        public void ZeroAlways_PrintsYearsMonthsWeeksDays_ForFullYearDayTimePeriod()
        {
            var formatter = builder
                .PrintZeroAlways()
                .AppendYears().AppendLiteral("-")
                .AppendMonths().AppendLiteral("-")
                .AppendWeeks().AppendLiteral("-")
                .AppendDays()
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(fullYearDayTimePeriod);

            Assert.AreEqual("1-0-0-4", printedValue);
            Assert.AreEqual(7, printer.CalculatePrintedLength(fullYearDayTimePeriod, null));
            Assert.AreEqual(4, printer.CountFieldsToPrint(fullYearDayTimePeriod, int.MaxValue, null));
        }

        [Test]
        public void ZeroAlways_PrintsYearsMonthsWeeksDays_ForEmptyStandartPeriod()
        {
            var formatter = builder
                .PrintZeroAlways()
                .AppendYears().AppendLiteral("-")
                .AppendMonths().AppendLiteral("-")
                .AppendWeeks().AppendLiteral("-")
                .AppendDays()
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(zeroPeriod);

            Assert.AreEqual("0-0-0-0", printedValue);
            Assert.AreEqual(7, printer.CalculatePrintedLength(zeroPeriod, null));
            Assert.AreEqual(4, printer.CountFieldsToPrint(zeroPeriod, int.MaxValue, null));
        }

        #endregion

        #region ZeroNever

        [Test]
        public void ZeroNever_PrintsYearsMonthsWeeksDays_ForFullStandartPeriod()
        {
            var formatter = builder
                .PrintZeroNever()
                .AppendYears().AppendLiteral("-")
                .AppendMonths().AppendLiteral("-")
                .AppendWeeks().AppendLiteral("-")
                .AppendDays()
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(standartPeriod);

            Assert.AreEqual("1-2-3-4", printedValue);
            Assert.AreEqual(7, printer.CalculatePrintedLength(standartPeriod, null));
            Assert.AreEqual(4, printer.CountFieldsToPrint(standartPeriod, int.MaxValue, null));
        }

        [Test]
        public void ZeroNever_PrintsYearsMonthsWeeksDays_ForEmptyYearDayTimePeriod()
        {
            var formatter = builder
                .PrintZeroNever()
                .AppendYears().AppendLiteral("-")
                .AppendMonths().AppendLiteral("-")
                .AppendWeeks().AppendLiteral("-")
                .AppendDays()
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(emptyYearDayTimePeriod);

            Assert.AreEqual("---", printedValue);
            Assert.AreEqual(3, printer.CalculatePrintedLength(emptyYearDayTimePeriod, null));
            Assert.AreEqual(0, printer.CountFieldsToPrint(emptyYearDayTimePeriod, int.MaxValue, null));
        }

        [Test]
        public void ZeroNever_PrintsYearsMonthsWeeksDays_ForFullYearDayTimePeriod()
        {
            var formatter = builder
                .PrintZeroNever()
                .AppendYears().AppendLiteral("-")
                .AppendMonths().AppendLiteral("-")
                .AppendWeeks().AppendLiteral("-")
                .AppendDays()
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(fullYearDayTimePeriod);

            Assert.AreEqual("1---4", printedValue);
            Assert.AreEqual(5, printer.CalculatePrintedLength(fullYearDayTimePeriod, null));
            Assert.AreEqual(2, printer.CountFieldsToPrint(fullYearDayTimePeriod, int.MaxValue, null));
        }

        [Test]
        public void ZeroNever_PrintsYearsMonthsWeeksDays_ForEmptyStandartPeriod()
        {
            var formatter = builder
                .PrintZeroNever()
                .AppendYears().AppendLiteral("-")
                .AppendMonths().AppendLiteral("-")
                .AppendWeeks().AppendLiteral("-")
                .AppendDays()
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(zeroPeriod);

            Assert.AreEqual("---", printedValue);
            Assert.AreEqual(3, printer.CalculatePrintedLength(zeroPeriod, null));
            Assert.AreEqual(0, printer.CountFieldsToPrint(zeroPeriod, int.MaxValue, null));
        }

        #endregion

        #region AppendPrinterParser

        [Test]
        public void AppendPrinterParser_Throws_IfBothNull()
        {
            Assert.Throws<ArgumentException>(()=>builder.Append(null, null));
        }

        [Test]
        public void AppendPrinterParser_ReturnsOnlyPrinter_IfParserNull()
        {
            var printer = builder
                .AppendYears()
                .AppendLiteral("-")
                .ToPrinter();
            
            var builder2 = new PeriodFormatterBuilder()
                .Append(printer, null)
                .AppendMonths();

            Assert.NotNull(builder2.ToPrinter());
            Assert.IsNull(builder2.ToParser());

            var formatter = builder2.ToFormatter();

            Assert.AreEqual("1-2", formatter.Print(standartPeriod));
            Assert.Throws<NotSupportedException>(()=>formatter.Parse("1-3"));
        }

        #endregion
    }
}
