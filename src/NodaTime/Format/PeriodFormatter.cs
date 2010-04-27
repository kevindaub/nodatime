#region Copyright and license information
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
using System;
using System.IO;
using System.Text;
using NodaTime.Periods;

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
    /// <term><see cref="PeriodFormats"/></term>
    /// <description>Formats by pattern and style</description>
    /// <term><see cref="IsoPeriodFormats"/></term>
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
    ///     Period date = formatter.WithProvider(CultureInfo.CreateSpecificCulture("fr-FR")).Parse(str);
    /// </code>
    /// </para>
    /// </remarks>
    public sealed class PeriodFormatter
    {
        private readonly PeriodType parsePeriodType;

        private readonly IPeriodParser periodParser;
        private readonly IPeriodPrinter periodPrinter;

        private readonly IFormatProvider provider;

        #region Construction

        /// <summary>
        /// Initializes a new instance of the <see cref="PeriodFormatter"/> class with specified arguments.
        /// </summary>
        /// <param name="periodParser">The internal parser, null if cannot parse</param>
        /// <param name="periodPrinter">The internal printer, null if cannot print</param>
        /// <param name="provider">The format provider to use</param>
        /// <param name="parsePeriodType">The period type to use for parsing</param>
        private PeriodFormatter(IPeriodParser periodParser, IPeriodPrinter periodPrinter,
            IFormatProvider provider, PeriodType parsePeriodType)
        {
            this.periodParser = periodParser;
            this.periodPrinter = periodPrinter;
            this.provider = provider;
            this.parsePeriodType = parsePeriodType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PeriodFormatter"/> class with specified arguments.
        /// </summary>
        /// <param name="periodParser">The internal parser, null if cannot parse</param>
        /// <param name="periodPrinter">The internal printer, null if cannot print</param>
        private PeriodFormatter(IPeriodParser periodParser, IPeriodPrinter periodPrinter)
            : this(periodParser, periodPrinter, null, null) { }

        /// <summary>
        /// Creates a new instance of the <see cref="PeriodFormatter"/> class with the supplied <see cref="IPeriodPrinter"/>
        /// </summary>
        /// <param name="periodPrinter">PeriodPrinter to use</param>
        /// <returns>A new formatter which can print, but not parse</returns>
        public static PeriodFormatter FromPrinter(IPeriodPrinter periodPrinter)
        {
            if (periodPrinter == null)
            {
                throw new ArgumentNullException("periodPrinter");
            }

            return new PeriodFormatter(null, periodPrinter);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="PeriodFormatter"/> class with the supplied <see cref="IPeriodParser"/> instance
        /// </summary>
        /// <param name="periodParser">PeriodParser to use</param>
        /// <returns>A new formatter which can parse, but not print</returns>
        public static PeriodFormatter FromParser(IPeriodParser periodParser)
        {
            if (periodParser == null)
            {
                throw new ArgumentNullException("periodParser");
            }

            return new PeriodFormatter(periodParser, null);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="PeriodFormatter"/> class 
        /// with the supplied <see cref="IPeriodParser"/> instance and <see cref="IPeriodPrinter"/> instance
        /// </summary>
        /// <param name="periodPrinter">PeriodPrinter to use</param>
        /// <param name="periodParser">PeriodParser to use</param>
        /// <returns>A new formatter which can parse and print</returns>
        public static PeriodFormatter FromPrinterAndParser(IPeriodPrinter periodPrinter, IPeriodParser periodParser)
        {
            if (periodPrinter == null)
            {
                throw new ArgumentNullException("periodPrinter");
            }

            if (periodParser == null)
            {
                throw new ArgumentNullException("periodParser");
            }

            return new PeriodFormatter(periodParser, periodPrinter);
        }
        #endregion

        #region Public properties
        /// <summary>
        /// Gets the PeriodType that will be used for parsing.
        /// </summary>
        public PeriodType ParsePeriodType { get { return parsePeriodType; } } 

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
        /// <param name="newProvider">The IFormatProvider to use.</param>
        /// <returns>The new formatter</returns>
        public PeriodFormatter WithProvider(IFormatProvider newProvider)
        {
            if (Object.Equals(newProvider, provider))
            {
                return this;
            }

            return new PeriodFormatter(periodParser, periodPrinter, newProvider, parsePeriodType);
        }

        /// <summary>
        /// Returns a new formatter with a different PeriodType for parsing.
        /// </summary>
        /// <param name="newParsePeriodType">The type to use in parsing</param>
        /// <returns>The new formatter</returns>
        /// <remarks>
        /// A PeriodFormatter is immutable, so a new instance is returned,
        /// and the original is unaltered and still usable.
        /// </remarks>
        public PeriodFormatter WithParseType(PeriodType newParsePeriodType)
        {
            if (newParsePeriodType == parsePeriodType)
            {
                return this;
            }

            return new PeriodFormatter(periodParser, periodPrinter, provider, newParsePeriodType);
        }
        #endregion

        #region Printing and parsing

        /// <summary>
        /// Prints an IPeriod to a <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="stringBuilder">The formatted period is appended to this builder</param>
        /// <param name="period">The period to format, not null</param>
        /// <exception cref="ArgumentNullException">if either period or stringBuilder is null</exception>
        /// <exception cref="NotSupportedException">if this formatter can't be used for printing</exception>
        public void PrintTo(StringBuilder stringBuilder, IPeriod period)
        {
            VerifyPrinter();
            VerifyPeriodArgument(period);
            if (stringBuilder == null)
            {
                throw new ArgumentNullException("stringBuilder");
            }

            Printer.PrintTo(new StringWriter(stringBuilder), period, provider);
        }

        /// <summary>
        /// Prints an IPeriod to a <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="textWriter">the formatted period is written out</param>
        /// <param name="period">The period to format, not null</param>
        /// <exception cref="ArgumentNullException">if either period or textWriter is null</exception>
        /// <exception cref="NotSupportedException">if this formatter can't be used for printing</exception>        
        public void PrintTo(TextWriter textWriter, IPeriod period)
        {
            VerifyPrinter();
            VerifyPeriodArgument(period);
            if (textWriter == null)
            {
                throw new ArgumentNullException("textWriter");
            }

            Printer.PrintTo(textWriter, period, provider);
        }

        /// <summary>
        /// Prints an IPeriod to a new <see cref="String"/>.
        /// </summary>
        /// <param name="period">The period to format, not null</param>
        /// <returns>The printed result</returns>
        /// <exception cref="ArgumentNullException">if period argument is null</exception>
        /// <exception cref="NotSupportedException">if this formatter can't be used for printing</exception>        
        public string Print(IPeriod period)
        {
            VerifyPrinter();
            VerifyPeriodArgument(period);

            var sb = new StringBuilder(periodPrinter.CalculatePrintedLength(period, provider));
            var sw = new StringWriter(sb);
            Printer.PrintTo(sw, period, provider);

            return sb.ToString();
        }

        /// <summary>
        /// Parses a period from the given text
        /// </summary>
        /// <param name="text">Text to parse</param>
        /// <returns>Parsed pariod</returns>
        /// <exception cref="ArgumentNullException">if text argument is null</exception>
        /// <exception cref="FormatException">text does not contain a valid string representation of a period.</exception>
        /// <exception cref="NotSupportedException">if this formatter can't be used for parsing</exception>        
        public Period Parse(String text)
        {
            VerifyParser();
            if (text == null)
            {
                throw new ArgumentNullException(text);
            }

            var builder = new PeriodBuilder(parsePeriodType);
            int newPosition = Parser.Parse(text, 0, builder, provider);

            if (newPosition >= text.Length)
            {
                return builder.ToPeriod();
            }

            if (newPosition < 0)
            {
                newPosition = ~newPosition;
            }

            throw new FormatException(FormatUtils.CreateErrorMessage(text, newPosition));
        }

        #endregion

        private void VerifyPrinter()
        {
            if (periodPrinter == null)
            {
                throw new NotSupportedException("Printing not supported");
            }
        }

        private void VerifyParser()
        {
            if (periodParser == null)
            {
                throw new NotSupportedException("Parsing not supported");
            }
        }

        private static void VerifyPeriodArgument(IPeriod period)
        {
            if (period == null)
            {
                throw new ArgumentNullException("period");
            }
        }
    }
}
