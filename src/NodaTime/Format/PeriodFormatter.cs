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
using System.Globalization;
using NodaTime.Periods;
using System;
using System.Text;
using System.IO;

namespace NodaTime.Format
{
    /// <summary>
    /// Controls the printing and parsing of a time period to and from a string.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class is the main API for printing and parsing used by most applications.
    /// Instances of this class are created via one of three factory classes:
    /// <list type="bullet">
    /// <listheader>
    /// <term>Class</term>
    /// <description>Summary</description>
    /// </listheader>
    /// <item>
    /// <term><see cref="PeriodFormatterFactory"/></term>
    /// <description>Formats by pattern and style</description>
    /// <term><see cref="IsoPeriodFormatterFactory"/></term>
    /// <description>ISO8601 formats</description>
    /// <term><see cref="PeriodFormatterBuilder"/></term>
    /// <description>Complex formats created via method calls</description>
    /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// An instance of this class holds a reference internally to one printer and
    /// one parser. It is possible that one of these may be null, in which case the
    /// formatter cannot print/parse. This can be checked via the <see cref="PeriodFormatter.IsPrinter"/>
    /// and <see cref="PeriodFormatter.IsParser"/> methods.
    /// </para>
    /// <para>
    /// The underlying printer/parser can be altered to behave exactly as required
    /// by using a decorator modifier:
    /// <list type="bullet">
    /// <listheader>
    /// <term>Method</term>
    /// <description>Summary</description>
    /// </listheader>
    /// <item>
    /// <term><see cref="PeriodFormatter.WithProvider"/></term>
    /// <description>returns a new formatter that uses the specified provider</description>
    /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// The main methods of the class are the <code>PrintXxx</code> and <code>Parsexxx</code> methods.
    /// These are used as follows:
    /// <code>
    ///     // print using the default locale
    ///     String periodStr = formatter.Print(period);
    ///     // print using the French locale
    ///     String periodStr = formatter.WithProvider(CultureInfo.CreateSpecificCulture("fr-FR")).Print(period);
    /// 
    ///     //parse using the French locale
    ///     Period date = formatter.WithProvideer(CultureInfo.CreateSpecificCulture("fr-FR")).Parse(str);
    /// </code>
    /// </para>
    /// </remarks>
    public sealed class PeriodFormatter
    {
        private readonly IPeriodParser periodParser;
        private readonly IPeriodPrinter periodPrinter;

        private readonly IFormatProvider provider;
        private readonly PeriodType periodType;

        #region Construction

        private PeriodFormatter(IPeriodParser periodParser, IPeriodPrinter periodPrinter
            , IFormatProvider provider, PeriodType periodType)
        {
            this.periodParser = periodParser;
            this.periodPrinter = periodPrinter;
            this.provider = provider;
            this.periodType = periodType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PeriodFormatter"/> class with specified arguments.
        /// </summary>
        /// <param name="periodParser">The internal parser, null if cannot parse</param>
        /// <param name="periodPrinter">The internal printer, null if cannot print</param>
        /// <param name="periodType">The period type</param>
        public PeriodFormatter(IPeriodParser periodParser, IPeriodPrinter periodPrinter, PeriodType periodType)
            : this(periodParser, periodPrinter, null, periodType) { }

        #endregion

        #region Public properties

        /// <summary>
        /// Indicates whether this formatter is capable of printing.
        /// </summary>        
        /// <value>True if this formatter is capable of printing, false otherwise</value>
        public bool IsPrinter { get { return periodPrinter != null; } }

        /// <summary>
        /// Gets the internal printer object that performs the real printing work.
        /// </summary>
        public IPeriodPrinter Printer { get { return periodPrinter; } }

        /// <summary>
        /// Indicates whether this formatter is capable of parsing.
        /// </summary>
        /// <value>True if this formatter is capable of parsing, false otherwise</value>
        public bool IsParser { get { return periodParser != null; } }

        /// <summary>
        /// Gets the internal parser object that performs the real parsing work.
        /// </summary>
        public IPeriodParser Parser { get { return periodParser; } }

        /// <summary>
        /// Gets the IFormatProvider that will be used for printing and parsing.
        /// </summary>
        public IFormatProvider Provider { get { return provider; } }

        /// <summary>
        /// Gets the PeriodType that will be used for parsing.
        /// </summary>
        public PeriodType PeriodType { get { return periodType; } }

        #endregion

        #region Creation methods

        /// <summary>
        /// Returns a new formatter with a different provider that will be used
        /// for printing and parsing.
        /// <para>
        /// A PeriodFormatter is immutable, so a new instance is returned,
        /// and the original is unaltered and still usable.
        /// </para>
        /// </summary>
        /// <param name="provider">The IFormatProvider to use.</param>
        /// <returns>The new formatter</returns>
        public PeriodFormatter WithProvider(IFormatProvider provider)
        {
            if (Object.Equals(provider, this.provider))
                return this;

            return new PeriodFormatter(this.periodParser, this.periodPrinter, provider, this.periodType);
        }

        /// <summary>
        /// Returns a new formatter with a different PeriodType for parsing
        /// <para>
        /// A PeriodFormatter is immutable, so a new instance is returned,
        /// and the original is unaltered and still usable.
        /// </para>
        /// </summary>
        /// <param name="periodType">The PeriodType to use.</param>
        /// <returns>The new formatter</returns>
        public PeriodFormatter WithPeriodType(PeriodType periodType)
        {
            if (periodType == this.periodType)
                return this;

            return new PeriodFormatter(this.periodParser, this.periodPrinter, this.provider, periodType);
        }

        #endregion

        #region Printing and parsing

        /// <summary>
        /// Prints an IPeriod to a StringBuilder.
        /// </summary>
        /// <param name="stringBuilder">The formatted period is appended to this buffer</param>
        /// <param name="period">The period to format, not null</param>
        public void PrintTo(StringBuilder stringBuilder, IPeriod period)
        {
            VerifyPrinter();
            VerifyPeriodArgument(period);

            Printer.PrintTo(stringBuilder, period, provider);
        }

        /// <summary>
        /// Prints an IPeriod to a TextWriter.
        /// </summary>
        /// <param name="textWriter">the formatted period is written out</param>
        /// <param name="period">The period to format, not null</param>
        public void PrintTo(TextWriter textWriter, IPeriod period)
        {
            VerifyPrinter();
            VerifyPeriodArgument(period);

            Printer.PrintTo(textWriter, period, provider);
        }

        /// <summary>
        /// Prints an IPeriod to a new String.
        /// </summary>
        /// <param name="period">The period to format, not null</param>
        /// <returns>The printed result</returns>
        public string Print(IPeriod period)
        {
            VerifyPrinter();
            VerifyPeriodArgument(period);

            var sb = new StringBuilder(periodPrinter.CalculatePrintedLength(period, provider));
            Printer.PrintTo(sb, period, provider);
            return sb.ToString();
        }

        /// <summary>
        /// Parses a period from the given text, at the given position, passing the
        /// result into the given IPeriod.
        /// </summary>
        /// <remarks>
        /// If the parse succeeds, the return value is the new text position.
        /// Note that the parse may succeed without fully reading the text.
        /// <para>
        /// The parse type of the formatter is not used by this method.
        /// If it fails, the return value is negative, 
        /// but the period may still be passed into the given variable.
        /// To determine the position where the parse failed, apply the
        /// one's complement operator (~) on the return value.
        /// </para>
        /// </remarks>
        /// <param name="text">Text to parse</param>
        /// <param name="position">Position to start parsing from</param>
        /// <param name="period">A period that will be created as a result of parsing</param>
        /// <returns>New position, if negative, parse failed.</returns>
        public int ParseInto(String text, int position, out IPeriod period)
        {
            VerifyParser();
            return Parser.ParseInto(text, position, provider,  out period);

        }

        /// <summary>
        /// Parses a period from the given text
        /// </summary>
        /// <param name="text">Text to parse</param>
        /// <returns>Parsed pariod</returns>
        public IPeriod Parse(String text)
        {
            VerifyParser();
            IPeriod period;
            int newPosition = Parser.ParseInto(text, 0, provider, out period);
            if (newPosition >= 0)
            {
                if (newPosition > text.Length)
                    return period;
            }
            else
                newPosition  = ~newPosition;

            throw new ArgumentException(FormatUtils.CreateErrorMessage(text, newPosition));
        }

        #endregion

        private void VerifyPrinter()
        {
            if (periodPrinter == null)
                throw new NotSupportedException("Printing not supported");
        }

        private void VerifyParser()
        {
            if (periodParser == null)
                throw new NotSupportedException("Parsing not supported");
        }

        private static void VerifyPeriodArgument(IPeriod period)
        {
            if (period == null)
                throw new ArgumentNullException("Period must not be null");
        }
    }
}
