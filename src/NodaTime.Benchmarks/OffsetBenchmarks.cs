﻿using System.Globalization;
using NodaTime.Benchmarks.Timing;
using NodaTime.Globalization;
using NodaTime.Text;

namespace NodaTime.Benchmarks
{
    internal class OffsetBenchmarks
    {
        private static readonly NodaFormatInfo InvariantFormatInfo = NodaFormatInfo.GetFormatInfo(CultureInfo.InvariantCulture);
        private static readonly Offset SampleOffset = Offset.FromHoursAndMinutes(12, 34);

        private readonly IPattern<Offset> offsetPattern;
        private readonly OffsetPatternParser offsetPatternParser;

        public OffsetBenchmarks()
        {
            offsetPatternParser = new OffsetPatternParser();
            offsetPattern = offsetPatternParser.ParsePattern("HH:mm", InvariantFormatInfo);
        }

        [Benchmark]
        public void ParseExactIncludingPreparse_Valid()
        {
            var pattern = offsetPatternParser.ParsePattern("HH:mm", InvariantFormatInfo);
            Offset result;
            ParseResult<Offset> parseResult = pattern.Parse("12:34");
            parseResult.TryGetValue(default(Offset), out result);
        }

        [Benchmark]
        public void PreparsedParseExact_Valid()
        {
            Offset result;
            ParseResult<Offset> parseResult = offsetPattern.Parse("12:34");
            parseResult.TryGetValue(default(Offset), out result);
        }

        [Benchmark]
        public void ParsePattern_Invalid()
        {
            offsetPatternParser.ParsePattern("hh:mm", InvariantFormatInfo);
        }

        [Benchmark]
        public void ParsePattern_Valid()
        {
            offsetPatternParser.ParsePattern("HH:mm", InvariantFormatInfo);
        }

        [Benchmark]
        public void PreparedParseExact_InvalidValue()
        {
            Offset result;
            ParseResult<Offset> parseResult = offsetPattern.Parse("123:45");
            parseResult.TryGetValue(default(Offset), out result);
        }

        [Benchmark]
        public void ToString_ExplicitFormat()
        {
            SampleOffset.ToString("HH:mm", InvariantFormatInfo);
        }

        [Benchmark]
        public void ParsedPattern_Format()
        {
            offsetPattern.Format(SampleOffset);
        }
    }
}
