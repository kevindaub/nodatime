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

using NUnit.Framework;
using NodaTime.Periods;
using NodaTime.Format;
using System.IO;

namespace NodaTime.Test.Format
{
    public partial class PeriodFormatterBuilderTest
    {

        private class SeparatorBuilder
        {
            private string text;
            private string finalText;
            private string[] variants;

            private bool useBefore;
            private bool useAfter;

            public SeparatorBuilder()
            {
                text = "A";
                finalText = "AA";
                variants = null;
                useBefore = true;
                useAfter = true;
            }
            public PeriodFormatterBuilder.Separator Build()
            {
                PeriodFormatterBuilder.FieldFormatter[] fieldFormatters = new PeriodFormatterBuilder.FieldFormatter[2];
                var beforeFormatter = new PeriodFormatterBuilder.FieldFormatter(-1, PeriodFormatterBuilder.PrintZeroSetting.RarelyLast, 10, false, PeriodFormatterBuilder.FormatterDurationFieldType.Years, fieldFormatters, null, null);
                var afterFormatter = new PeriodFormatterBuilder.FieldFormatter(-1, PeriodFormatterBuilder.PrintZeroSetting.RarelyLast, 10, false, PeriodFormatterBuilder.FormatterDurationFieldType.Months, fieldFormatters, null, null);
                fieldFormatters[0] = beforeFormatter;
                fieldFormatters[1] = afterFormatter;

                var separator = new PeriodFormatterBuilder.Separator(text, finalText, variants, beforeFormatter, beforeFormatter, useBefore, useAfter);
                separator.Finish(afterFormatter, afterFormatter);
                return separator;
            }

        }

        [Test]
        public void Separator_PrintsItself_IfBeforeAndAfterSetAndBothPrintersPrintFieldValues()
        {
            var separator = new SeparatorBuilder().Build();
            var writer = new StringWriter();

            separator.PrintTo(writer, standardPeriodFull, null);

            Assert.That(writer.ToString(), Is.EqualTo("1AA2"));
        }

        [Test]
        public void AppendSeparatorBetweenYearsAndHours_BuildsCorrectPrinter_ForStandardPeriod()
        {
            var formatter = builder.AppendYears().AppendSeparator("T").AppendHours().ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(standardPeriodFull);

            Assert.AreEqual("1T5", printedValue);
            Assert.AreEqual(3, printer.CalculatePrintedLength(standardPeriodFull, null));
            Assert.AreEqual(2, printer.CountFieldsToPrint(standardPeriodFull, int.MaxValue, null));
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
        public void AppendSeparatorBetweenYearsAndHours_ParsesTo1yesr5MonthsStandardPeriod_FromFieldsWithSeparator()
        {
            var formatter = builder.AppendYears().AppendSeparator("T").AppendHours().ToFormatter();

            var period = formatter.Parse("1T5");

            Assert.AreEqual(Period.FromYears(1).WithHours(5), period);
        }

        #region FinalText

        [Test]
        public void AppendSeparatorFinalText_BuildsCorrectPrinter_ForStandardPeriod()
        {
            var formatter = builder
                .AppendYears().AppendSeparator(", ", " and ")
                .AppendHours().AppendSeparator(", ", " and ")
                .AppendMinutes().AppendSeparator(", ", " and ")
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(standardPeriodFull);

            Assert.AreEqual("1, 5 and 6", printedValue);
            Assert.AreEqual(10, printer.CalculatePrintedLength(standardPeriodFull, null));
            Assert.AreEqual(3, printer.CountFieldsToPrint(standardPeriodFull, int.MaxValue, null));
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

        #endregion

        #region FieldsAfter

        [Test]
        public void AppendSeparatorIfFieldsAfter_BuildsCorrectPrinter_ForStandardPeriod()
        {
            var formatter = builder
                .AppendYears().AppendSeparatorIfFieldsAfter("T")
                .AppendHours()
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(standardPeriodFull);

            Assert.AreEqual("1T5", printedValue);
            Assert.AreEqual(3, printer.CalculatePrintedLength(standardPeriodFull, null));
            Assert.AreEqual(2, printer.CountFieldsToPrint(standardPeriodFull, int.MaxValue, null));
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

        #endregion

        #region FieldsBefore

        [Test]
        public void AppendSeparatorIfFieldsBefore_BuildsCorrectPrinter_ForStandardPeriod()
        {
            var formatter = builder
                .AppendYears().AppendSeparatorIfFieldsBefore("T")
                .AppendHours()
                .ToFormatter();

            var printer = formatter.Printer;
            var printedValue = formatter.Print(standardPeriodFull);

            Assert.AreEqual("1T5", printedValue);
            Assert.AreEqual(3, printer.CalculatePrintedLength(standardPeriodFull, null));
            Assert.AreEqual(2, printer.CountFieldsToPrint(standardPeriodFull, int.MaxValue, null));
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

        #endregion
    }
}
