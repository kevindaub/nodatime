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

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NodaTime.Properties;
using NodaTime.Text;

namespace NodaTime.Test.Text
{
    public partial class LocalTimePatternTest
    {
        public static readonly CultureInfo EnUs = new CultureInfo("en-US");
        public static readonly CultureInfo ItIt = new CultureInfo("it-IT");
        // Bengali: uses a time separator of "."
        public static readonly CultureInfo BnBd = new CultureInfo("bn-BD");

        internal static readonly Data[] InvalidPatternData = {
            new Data { Pattern = "!", Message = Messages.Parse_UnknownStandardFormat, Parameters = {'!', typeof(LocalTime).FullName}},
            new Data { Pattern = "%", Message = Messages.Parse_UnknownStandardFormat, Parameters = { '%', typeof(LocalTime).FullName } },
            new Data { Pattern = "\\", Message = Messages.Parse_UnknownStandardFormat, Parameters = { '\\', typeof(LocalTime).FullName } },
            new Data { Pattern = "%%", Message = Messages.Parse_PercentDoubled },
            new Data { Pattern = "%\\", Message = Messages.Parse_EscapeAtEndOfString },
            new Data { Pattern = "ffffffff", Message = Messages.Parse_RepeatCountExceeded, Parameters = { 'f', 7 } },
            new Data { Pattern = "FFFFFFFF", Message = Messages.Parse_RepeatCountExceeded, Parameters = { 'F', 7 } },
            new Data { Pattern = "H%", Message = Messages.Parse_PercentAtEndOfString },
            new Data { Pattern = "HHH", Message = Messages.Parse_RepeatCountExceeded, Parameters = { 'H', 2 } },
            new Data { Pattern = "mmm", Message = Messages.Parse_RepeatCountExceeded, Parameters = { 'm', 2 } },
            new Data { Pattern = "mmmmmmmmmmmmmmmmmmm", Message = Messages.Parse_RepeatCountExceeded, Parameters = { 'm', 2 } },
            new Data { Pattern = "'qwe", Message = Messages.Parse_MissingEndQuote, Parameters = { '\'' } },
            new Data { Pattern = "'qwe\\", Message = Messages.Parse_EscapeAtEndOfString },
            new Data { Pattern = "'qwe\\'", Message = Messages.Parse_MissingEndQuote, Parameters = { '\'' } },
            new Data { Pattern = "sss", Message = Messages.Parse_RepeatCountExceeded, Parameters = { 's', 2 } },
        };

        internal static Data[] ParseFailureData = {
            new Data { Text = "17 6", Pattern = "HH h", Message = Messages.Parse_InconsistentValues2, Parameters = {'H', 'h', typeof(LocalTime).FullName}},
            new Data { Text = "17 AM", Pattern = "HH tt", Message = Messages.Parse_InconsistentValues2, Parameters = {'H', 't', typeof(LocalTime).FullName}},
        };

        internal static Data[] ParseOnlyData = {
            new Data(0, 0, 0, 400) { Culture = EnUs, Text = "4", Pattern = "%f", },
            new Data(0, 0, 0, 400) { Culture = EnUs, Text = "4", Pattern = "%F", },
            new Data(0, 0, 0, 400) { Culture = EnUs, Text = "4", Pattern = "FF", },
            new Data(0, 0, 0, 400) { Culture = EnUs, Text = "4", Pattern = "FFF", },
            new Data(0, 0, 0, 400) { Culture = EnUs, Text = "40", Pattern = "ff", },
            new Data(0, 0, 0, 400) { Culture = EnUs, Text = "400", Pattern = "fff", },
            new Data(0, 0, 0, 400) { Culture = EnUs, Text = "4000", Pattern = "ffff", },
            new Data(0, 0, 0, 400) { Culture = EnUs, Text = "40000", Pattern = "fffff", },
            new Data(0, 0, 0, 400) { Culture = EnUs, Text = "400000", Pattern = "ffffff", },
            new Data(0, 0, 0, 400) { Culture = EnUs, Text = "4000000", Pattern = "fffffff", },
            new Data(0, 0, 0, 400) { Culture = EnUs, Text = "4", Pattern = "%f", },
            new Data(0, 0, 0, 400) { Culture = EnUs, Text = "4", Pattern = "%F", },
            new Data(0, 0, 0, 450) { Culture = EnUs, Text = "45", Pattern = "ff" },
            new Data(0, 0, 0, 450) { Culture = EnUs, Text = "45", Pattern = "FF" },
            new Data(0, 0, 0, 450) { Culture = EnUs, Text = "45", Pattern = "FFF" },
            new Data(0, 0, 0, 450) { Culture = EnUs, Text = "450", Pattern = "fff" },
            new Data(0, 0, 0, 400) { Culture = EnUs, Text = "4", Pattern = "%f" },
            new Data(0, 0, 0, 400) { Culture = EnUs, Text = "4", Pattern = "%F" },
            new Data(0, 0, 0, 450) { Culture = EnUs, Text = "45", Pattern = "ff" },
            new Data(0, 0, 0, 450) { Culture = EnUs, Text = "45", Pattern = "FF" },
            new Data(0, 0, 0, 456) { Culture = EnUs, Text = "456", Pattern = "fff" },
            new Data(0, 0, 0, 456) { Culture = EnUs, Text = "456", Pattern = "FFF" },

            new Data(0, 0, 0, 0) { Culture = EnUs, Text = "0", Pattern = "%f" },
            new Data(0, 0, 0, 0) { Culture = EnUs, Text = "00", Pattern = "ff" },
            new Data(0, 0, 0, 8) { Culture = EnUs, Text = "008", Pattern = "fff" },
            new Data(0, 0, 0, 8) { Culture = EnUs, Text = "008", Pattern = "FFF" },
            new Data(5, 0, 0, 0) { Culture = EnUs, Text = "05", Pattern = "HH" },
            new Data(0, 6, 0, 0) { Culture = EnUs, Text = "06", Pattern = "mm" },
            new Data(0, 0, 7, 0) { Culture = EnUs, Text = "07", Pattern = "ss" },
            new Data(5, 0, 0, 0) { Culture = EnUs, Text = "5", Pattern = "%H" },
            new Data(0, 6, 0, 0) { Culture = EnUs, Text = "6", Pattern = "%m" },
            new Data(0, 0, 7, 0) { Culture = EnUs, Text = "7", Pattern = "%s" },
        };

        internal static Data[] FormatOnlyData = {
            new Data(5, 6, 7, 8) { Text = "", Pattern = "%F" },
            new Data(5, 6, 7, 8) { Text = "", Pattern = "FF" },
            new Data(1, 1, 1, 400) { Text = "4", Pattern = "%f" },
            new Data(1, 1, 1, 400) { Text = "4", Pattern = "%F" },
            new Data(1, 1, 1, 400) { Text = "4", Pattern = "FF" },
            new Data(1, 1, 1, 400) { Text = "4", Pattern = "FFF" },
            new Data(1, 1, 1, 400) { Text = "40", Pattern = "ff" },
            new Data(1, 1, 1, 400) { Text = "400", Pattern = "fff" },
            new Data(1, 1, 1, 400) { Text = "4000", Pattern = "ffff" },
            new Data(1, 1, 1, 400) { Text = "40000", Pattern = "fffff" },
            new Data(1, 1, 1, 400) { Text = "400000", Pattern = "ffffff" },
            new Data(1, 1, 1, 400) { Text = "4000000", Pattern = "fffffff" },
            new Data(1, 1, 1, 450) { Text = "4", Pattern = "%f" },
            new Data(1, 1, 1, 450) { Text = "4", Pattern = "%F" },
            new Data(1, 1, 1, 450) { Text = "45", Pattern = "ff" },
            new Data(1, 1, 1, 450) { Text = "45", Pattern = "FF" },
            new Data(1, 1, 1, 450) { Text = "45", Pattern = "FFF" },
            new Data(1, 1, 1, 450) { Text = "450", Pattern = "fff" },
            new Data(1, 1, 1, 456) { Text = "4", Pattern = "%f" },
            new Data(1, 1, 1, 456) { Text = "4", Pattern = "%F" },
            new Data(1, 1, 1, 456) { Text = "45", Pattern = "ff" },
            new Data(1, 1, 1, 456) { Text = "45", Pattern = "FF" },
            new Data(1, 1, 1, 456) { Text = "456", Pattern = "fff" },
            new Data(1, 1, 1, 456) { Text = "456", Pattern = "FFF" },


            new Data(5, 6, 7, 8) { Culture = EnUs, Text = "0", Pattern = "%f" },
            new Data(5, 6, 7, 8) { Culture = EnUs, Text = "00", Pattern = "ff" },
            new Data(5, 6, 7, 8) { Culture = EnUs, Text = "008", Pattern = "fff" },
            new Data(5, 6, 7, 8) { Culture = EnUs, Text = "008", Pattern = "FFF" },
            new Data(5, 6, 7, 8) { Culture = EnUs, Text = "05", Pattern = "HH" },
            new Data(5, 6, 7, 8) { Culture = EnUs, Text = "06", Pattern = "mm" },
            new Data(5, 6, 7, 8) { Culture = EnUs, Text = "07", Pattern = "ss" },
            new Data(5, 6, 7, 8) { Culture = EnUs, Text = "5", Pattern = "%H" },
            new Data(5, 6, 7, 8) { Culture = EnUs, Text = "6", Pattern = "%m" },
            new Data(5, 6, 7, 8) { Culture = EnUs, Text = "7", Pattern = "%s" },
        };
        
        internal static Data[] DefaultPatternData = {                              
            // Invariant culture uses HH:mm:ss for the "long" pattern
            new Data(5, 0, 0, 0) { Text = "05:00:00" },
            new Data(5, 12, 0, 0) { Text = "05:12:00" },
            new Data(5, 12, 34, 0) { Text = "05:12:34" },

            // US uses hh:mm:ss tt for the "long" pattern
            new Data(17, 0, 0, 0) { Culture = EnUs, Text = "5:00:00 PM" },
            new Data(5, 0, 0, 0) { Culture = EnUs, Text = "5:00:00 AM" },
            new Data(5, 12, 0, 0) { Culture = EnUs, Text = "5:12:00 AM" },
            new Data(5, 12, 34, 0) { Culture = EnUs, Text = "5:12:34 AM" },
        };

        internal static readonly Data[] TemplateValueData = {
            // Pattern specifies nothing - template value is passed through
            new Data(new LocalTime(1, 2, 3, 4, 5)) { Culture = EnUs, Text = "X", Pattern = "%X", Template = new LocalTime(1, 2, 3, 4, 5) },
            // Tests for each individual field being propagated
            new Data(new LocalTime(1, 6, 7, 8, 9)) { Culture = EnUs, Text = "06:07.0080009", Pattern = "mm:ss.FFFFFFF", Template = new LocalTime(1, 2, 3, 4, 5) },
            new Data(new LocalTime(6, 2, 7, 8, 9)) { Culture = EnUs, Text = "06:07.0080009", Pattern = "HH:ss.FFFFFFF", Template = new LocalTime(1, 2, 3, 4, 5) },
            new Data(new LocalTime(6, 7, 3, 8, 9)) { Culture = EnUs, Text = "06:07.0080009", Pattern = "HH:mm.FFFFFFF", Template = new LocalTime(1, 2, 3, 4, 5) },
            new Data(new LocalTime(6, 7, 8, 4, 5)) { Culture = EnUs, Text = "06:07:08", Pattern = "HH:mm:ss", Template = new LocalTime(1, 2, 3, 4, 5) },

            // Hours are tricky because of the ways they can be specified
            new Data(new LocalTime(6, 2, 3)) { Culture = EnUs, Text = "6", Pattern = "%h", Template = new LocalTime(1, 2, 3) },
            new Data(new LocalTime(18, 2, 3)) { Culture = EnUs, Text = "6", Pattern = "%h", Template = new LocalTime(14, 2, 3) },
            new Data(new LocalTime(2, 2, 3)) { Culture = EnUs, Text = "AM", Pattern = "tt", Template = new LocalTime(14, 2, 3) },
            new Data(new LocalTime(14, 2, 3)) { Culture = EnUs, Text = "PM", Pattern = "tt", Template = new LocalTime(14, 2, 3) },
            new Data(new LocalTime(2, 2, 3)) { Culture = EnUs, Text = "AM", Pattern = "tt", Template = new LocalTime(2, 2, 3) },
            new Data(new LocalTime(14, 2, 3)) { Culture = EnUs, Text = "PM", Pattern = "tt", Template = new LocalTime(2, 2, 3) },
            new Data(new LocalTime(17, 2, 3)) { Culture = EnUs, Text = "5 PM", Pattern = "h tt", Template = new LocalTime(1, 2, 3) },
        };

        /// <summary>
        /// Common test data for both formatting and parsing. A test should be placed here unless is truly
        /// cannot be run both ways. This ensures that as many round-trip type tests are performed as possible.
        /// </summary>
        internal static readonly Data[] FormatAndParseData = {
            new Data(LocalTime.Midnight) { Culture = EnUs, Text = ".", Pattern = "%." },
            new Data(LocalTime.Midnight) { Culture = EnUs, Text = ":", Pattern = "%:" },
            // TODO: Work out what this should actually do...
            new Data(LocalTime.Midnight) { Culture = ItIt, Text = ".", Pattern = "%." },
            new Data(LocalTime.Midnight) { Culture = ItIt, Text = ".", Pattern = "%:" },
            new Data(LocalTime.Midnight) { Culture = EnUs, Text = "H", Pattern = "\\H" },
            new Data(LocalTime.Midnight) { Culture = EnUs, Text = "HHss", Pattern = "'HHss'" },
            new Data(0, 0, 0, 100) { Culture = EnUs, Text = "1", Pattern = "%f" },
            new Data(0, 0, 0, 100) { Culture = EnUs, Text = "1", Pattern = "%F" },
            new Data(0, 0, 0, 100) { Culture = EnUs, Text = "1", Pattern = "FF" },
            new Data(0, 0, 0, 100) { Culture = EnUs, Text = "1", Pattern = "FFF" },
            new Data(0, 0, 0, 120) { Culture = EnUs, Text = "12", Pattern = "ff" },
            new Data(0, 0, 0, 120) { Culture = EnUs, Text = "12", Pattern = "FF" },
            new Data(0, 0, 0, 120) { Culture = EnUs, Text = "12", Pattern = "FFF" },
            new Data(0, 0, 0, 123) { Culture = EnUs, Text = "123", Pattern = "fff" },
            new Data(0, 0, 0, 123) { Culture = EnUs, Text = "123", Pattern = "FFF" },
            new Data(0, 0, 0, 123, 4000) { Culture = EnUs, Text = "1234", Pattern = "ffff" },
            new Data(0, 0, 0, 123, 4000) { Culture = EnUs, Text = "1234", Pattern = "FFFF" },
            new Data(0, 0, 0, 123, 4500) { Culture = EnUs, Text = "12345", Pattern = "fffff" },
            new Data(0, 0, 0, 123, 4500) { Culture = EnUs, Text = "12345", Pattern = "FFFFF" },
            new Data(0, 0, 0, 123, 4560) { Culture = EnUs, Text = "123456", Pattern = "ffffff" },
            new Data(0, 0, 0, 123, 4560) { Culture = EnUs, Text = "123456", Pattern = "FFFFFF" },
            new Data(0, 0, 0, 123, 4567) { Culture = EnUs, Text = "1234567", Pattern = "fffffff" },
            new Data(0, 0, 0, 123, 4567) { Culture = EnUs, Text = "1234567", Pattern = "FFFFFFF" },
            new Data(0, 0, 0, 600) { Culture = EnUs, Text = ".6", Pattern = ".f" },
            new Data(0, 0, 0, 600) { Culture = EnUs, Text = ".6", Pattern = ".F" },
            new Data(0, 0, 0, 600) { Culture = EnUs, Text = ".6", Pattern = ".FFF" }, // Elided fraction
            new Data(0, 0, 0, 678) { Culture = EnUs, Text = ".678", Pattern = ".fff" },
            new Data(0, 0, 0, 678) { Culture = EnUs, Text = ".678", Pattern = ".FFF" },
            new Data(0, 0, 12, 0) { Culture = EnUs, Text = "12", Pattern = "%s" },
            new Data(0, 0, 12, 0) { Culture = EnUs, Text = "12", Pattern = "ss" },
            new Data(0, 0, 2, 0) { Culture = EnUs, Text = "2", Pattern = "%s" },
            new Data(0, 12, 0, 0) { Culture = EnUs, Text = "12", Pattern = "%m" },
            new Data(0, 12, 0, 0) { Culture = EnUs, Text = "12", Pattern = "mm" },
            new Data(0, 2, 0, 0) { Culture = EnUs, Text = "2", Pattern = "%m" },
            new Data(1, 0, 0, 0) { Culture = EnUs, Text = "1", Pattern = "H.FFF" }, // Missing fraction
            new Data(12, 0, 0, 0) { Culture = EnUs, Text = "12", Pattern = "%H" },
            new Data(12, 0, 0, 0) { Culture = EnUs, Text = "12", Pattern = "HH" },
            new Data(2, 0, 0, 0) { Culture = EnUs, Text = "2", Pattern = "%H" },
            new Data(2, 0, 0, 0) { Culture = EnUs, Text = "2", Pattern = "%H" },
            new Data(0, 0, 12, 340) { Culture = EnUs, Text = "12.34", Pattern = "ss.FFF"  },

            new Data(14, 15, 16) { Culture = EnUs, Text = "14:15:16", Pattern = "r" },
            new Data(14, 15, 16, 700) { Culture = EnUs, Text = "14:15:16.7", Pattern = "r" },
            new Data(14, 15, 16, 780) { Culture = EnUs, Text = "14:15:16.78", Pattern = "r" },
            new Data(14, 15, 16, 789) { Culture = EnUs, Text = "14:15:16.789", Pattern = "r" },
            new Data(14, 15, 16, 789, 1000) { Culture = EnUs, Text = "14:15:16.7891", Pattern = "r" },
            new Data(14, 15, 16, 789, 1200) { Culture = EnUs, Text = "14:15:16.78912", Pattern = "r" },
            new Data(14, 15, 16, 789, 1230) { Culture = EnUs, Text = "14:15:16.789123", Pattern = "r" },
            new Data(14, 15, 16, 789, 1234) { Culture = EnUs, Text = "14:15:16.7891234", Pattern = "r" },
            new Data(14, 15, 16, 700) { Culture = BnBd, Text = "14.15.16.7", Pattern = "r" },
            new Data(14, 15, 16, 780) { Culture = BnBd, Text = "14.15.16.78", Pattern = "r" },
            new Data(14, 15, 16, 789) { Culture = BnBd, Text = "14.15.16.789", Pattern = "r" },
            new Data(14, 15, 16, 789, 1000) { Culture = BnBd, Text = "14.15.16.7891", Pattern = "r" },
            new Data(14, 15, 16, 789, 1200) { Culture = BnBd, Text = "14.15.16.78912", Pattern = "r" },
            new Data(14, 15, 16, 789, 1230) { Culture = BnBd, Text = "14.15.16.789123", Pattern = "r" },
            new Data(14, 15, 16, 789, 1234) { Culture = BnBd, Text = "14.15.16.7891234", Pattern = "r" },

            // ------------ Template value tests ----------
            // Mixtures of 12 and 24 hour times
            new Data(18, 0, 0) { Culture = EnUs, Text = "18 6 PM", Pattern = "HH h tt" },
            new Data(18, 0, 0) { Culture = EnUs, Text = "18 6", Pattern = "HH h" },
            new Data(18, 0, 0) { Culture = EnUs, Text = "18 PM", Pattern = "HH tt" },
            new Data(18, 0, 0) { Culture = EnUs, Text = "6 PM", Pattern = "h tt" },
            new Data(6, 0, 0) { Culture = EnUs, Text = "6", Pattern = "%h" },
            new Data(0, 0, 0) { Culture = EnUs, Text = "AM", Pattern = "tt" },
            new Data(12, 0, 0) { Culture = EnUs, Text = "PM", Pattern = "tt" },

            // Pattern specifies nothing - template value is passed through
            new Data(new LocalTime(1, 2, 3, 4, 5)) { Culture = EnUs, Text = "X", Pattern = "%X", Template = new LocalTime(1, 2, 3, 4, 5) },
            // Tests for each individual field being propagated
            new Data(new LocalTime(1, 6, 7, 8, 9)) { Culture = EnUs, Text = "06:07.0080009", Pattern = "mm:ss.FFFFFFF", Template = new LocalTime(1, 2, 3, 4, 5) },
            new Data(new LocalTime(6, 2, 7, 8, 9)) { Culture = EnUs, Text = "06:07.0080009", Pattern = "HH:ss.FFFFFFF", Template = new LocalTime(1, 2, 3, 4, 5) },
            new Data(new LocalTime(6, 7, 3, 8, 9)) { Culture = EnUs, Text = "06:07.0080009", Pattern = "HH:mm.FFFFFFF", Template = new LocalTime(1, 2, 3, 4, 5) },
            new Data(new LocalTime(6, 7, 8, 4, 5)) { Culture = EnUs, Text = "06:07:08", Pattern = "HH:mm:ss", Template = new LocalTime(1, 2, 3, 4, 5) },

            // Hours are tricky because of the ways they can be specified
            new Data(new LocalTime(6, 2, 3)) { Culture = EnUs, Text = "6", Pattern = "%h", Template = new LocalTime(1, 2, 3) },
            new Data(new LocalTime(18, 2, 3)) { Culture = EnUs, Text = "6", Pattern = "%h", Template = new LocalTime(14, 2, 3) },
            new Data(new LocalTime(2, 2, 3)) { Culture = EnUs, Text = "AM", Pattern = "tt", Template = new LocalTime(14, 2, 3) },
            new Data(new LocalTime(14, 2, 3)) { Culture = EnUs, Text = "PM", Pattern = "tt", Template = new LocalTime(14, 2, 3) },
            new Data(new LocalTime(2, 2, 3)) { Culture = EnUs, Text = "AM", Pattern = "tt", Template = new LocalTime(2, 2, 3) },
            new Data(new LocalTime(14, 2, 3)) { Culture = EnUs, Text = "PM", Pattern = "tt", Template = new LocalTime(2, 2, 3) },
            new Data(new LocalTime(17, 2, 3)) { Culture = EnUs, Text = "5 PM", Pattern = "h tt", Template = new LocalTime(1, 2, 3) },
            // --------------- end of template value tests ----------------------
        };

        internal static IEnumerable<Data> ParseData = ParseOnlyData.Concat(FormatAndParseData);
        internal static IEnumerable<Data> FormatData = FormatOnlyData.Concat(FormatAndParseData);
        
        /// <summary>
        /// A container for test data for formatting and parsing <see cref="LocalTime" /> objects.
        /// </summary>
        public sealed class Data : PatternTestData<LocalTime>
        {
            public Data(LocalTime value)
                : base(value)
            {
                // Default to midnight
                Template = LocalTime.Midnight;
            }

            public Data(int hours, int minutes, int seconds)
                : this(new LocalTime(hours, minutes, seconds))
            {
            }

            public Data(int hours, int minutes, int seconds, int milliseconds)
                : this(new LocalTime(hours, minutes, seconds, milliseconds))
            {
            }

            public Data(int hours, int minutes, int seconds, int milliseconds, int ticksWithinMillisecond)
                : this(new LocalTime(hours, minutes, seconds, milliseconds, ticksWithinMillisecond))
            {
            }

            public Data() : this(LocalTime.Midnight)
            {
            }

            internal override IPattern<LocalTime> CreatePattern()
            {
                return LocalTimePattern.CreateWithInvariantInfo(Pattern)
                    .WithTemplateValue(Template)
                    .WithCulture(Culture);
            }
        }
    }
}
