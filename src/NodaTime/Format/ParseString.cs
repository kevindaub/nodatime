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
#endregion

namespace NodaTime.Format
{
    /// <summary>
    ///   Provides a simple parser for value strings.
    /// </summary>
    internal class ParseString : Parsable
    {
        /// <summary>
        ///   Initializes a new instance of the <see cref = "ParseString" /> class.
        /// </summary>
        /// <param name = "value">The string to parse.</param>
        internal ParseString(string value)
            : base(value)
        {
        }

        /// <summary>
        ///   Attempts to match the specified character with the current character of the string. If the
        ///   character matches then the index is moved passed the character.
        /// </summary>
        /// <param name = "character">The character to match.</param>
        /// <returns><c>true</c> if the character matches.</returns>
        internal bool Match(char character)
        {
            if (Current == character)
            {
                MoveNext();
                return true;
            }
            return false;
        }

        /// <summary>
        ///   Attempts to match the specified string with the current point in the string. If the
        ///   character matches then the index is moved passed the string.
        /// </summary>
        /// <param name = "match">The string to match.</param>
        /// <returns><c>true</c> if the string matches.</returns>
        internal bool Match(string match)
        {
            if (string.CompareOrdinal(Value, Index, match, 0, match.Length) == 0)
            {
                Move(Index + match.Length);
                return true;
            }
            return false;
        }

        /// <summary>
        ///   Parses digits at the current point in the string. If the minimum required
        ///   digits are not present then the index is unchanged. If there are more digits than
        ///   the maximum allowed they are ignored.
        /// </summary>
        /// <param name = "minimumDigits">The minimum allowed digits.</param>
        /// <param name = "maximumDigits">The maximum allowed digits.</param>
        /// <param name = "result">The result integer value.</param>
        /// <returns><c>true</c> if the digits were parsed.</returns>
        internal bool ParseDigits(int minimumDigits, int maximumDigits, out int result)
        {
            result = 0;
            int startIndex = Index;
            int count = 0;
            while (count < maximumDigits)
            {
                if (!IsDigit())
                {
                    break;
                }
                result = (result * 10) + GetDigit();
                count++;
                if (!MoveNext())
                {
                    break;
                }
            }
            if (count < minimumDigits)
            {
                Move(startIndex);
                return false;
            }
            return true;
        }

        /// <summary>
        ///   Parses digits at the current point in the string as a fractional value.
        /// </summary>
        /// <param name = "maximumDigits">The maximum allowed digits.</param>
        /// <param name = "scale">The scale of the fractional value.</param>
        /// <param name = "result">The result value scaled by scale.</param>
        /// <returns><c>true</c> if the digits were parsed.</returns>
        internal bool ParseFractionExact(int maximumDigits, int scale, out int result)
        {
            if (scale < maximumDigits)
            {
                scale = maximumDigits;
            }
            result = Int32.MinValue;
            if (!IsDigit())
            {
                return false;
            }
            result = GetDigit();
            int count = 1;
            while (MoveNext() && count < maximumDigits && IsDigit())
            {
                result = (result * 10) + GetDigit();
                count++;
            }
            result = (int)(result * Math.Pow(10.0, scale - count));
            return (count == maximumDigits);
        }

        /// <summary>
        ///   Gets the integer value of the current digit character. Allows for non-roman digits.
        /// </summary>
        /// <returns></returns>
        private int GetDigit()
        {
            return (int)char.GetNumericValue(Current);
        }

        /// <summary>
        ///   Determines whether the current character is a digit character.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if the current character is a digit; otherwise, <c>false</c>.
        /// </returns>
        private bool IsDigit()
        {
            return char.IsNumber(Current);
        }
    }
}