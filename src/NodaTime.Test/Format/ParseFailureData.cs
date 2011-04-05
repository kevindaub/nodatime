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
using NodaTime.Format;
using NUnit.Framework;
#endregion

namespace NodaTime.Test.Format
{
    public class ParseFailureNull : AbstractParseFailureData
    {
        public ParseFailureNull(string value, string format, string name)
            : base(value, format, DateTimeParseStyles.None, MakeWhat(value, format), new ParseFailureInfo(name))
        {
        }

        private static string MakeWhat(string value, string format)
        {
            if (value == null)
            {
                return "value is null";
            }
            if (format == null)
            {
                return "format is null";
            }
            return "Unknown problem in test code";
        }
    }

    public class ParseFailureData : AbstractParseFailureData
    {
        public ParseFailureData(string value, string format, ParseFailureKind kind, string what, params object[] parameters)
            : this(value, format, DateTimeParseStyles.None, kind, what, parameters)
        {
        }

        public ParseFailureData(string value, string format, DateTimeParseStyles styles, ParseFailureKind kind, string what, params object[] parameters)
            : base(value, format, styles, what, MakeFailureInfo(kind, parameters))
        {
        }

        private static ParseFailureInfo MakeFailureInfo(ParseFailureKind kind, object[] parameters)
        {
            return new ParseFailureInfo(kind, parameters);
        }
    }

    public abstract class AbstractParseFailureData : TestCaseData
    {
        protected AbstractParseFailureData(string value, string format, DateTimeParseStyles styles, string what, ParseFailureInfo failureInfo)
            : base(value, format, styles, failureInfo)
        {
            string name = String.Format("value: [{0}], format: [{1}], {2}", value ?? "null", format ?? "null", what);
            SetName(name);
        }
    }
}