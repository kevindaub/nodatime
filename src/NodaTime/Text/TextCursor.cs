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

using System;
using System.Diagnostics;
using System.Text;

namespace NodaTime.Text
{
    /// <summary>
    /// Provides a cursor over text being parsed. None of the methods in this class throw exceptions (unless
    /// there is a bug in Noda Time, in which case an exception is appropriate) and none of the methods
    /// have ref parameters indicating failures, unlike subclasses. This class is used as the basis for both
    /// value and pattern parsing, so can make no judgement about what's wrong (i.e. it wouldn't know what
    /// type of failure to indicate). Instead, methods return Boolean values to indicate success or failure.
    /// </summary>
    [DebuggerStepThrough]
    internal abstract class TextCursor
    {
        /// <summary>
        /// A nul character. This character is not allowed in any parsable string and is used to
        /// indicate that the current character is not set.
        /// </summary>
        internal const char Nul = '\0';

        /// <summary>
        /// Initializes a new instance to parse the given value.
        /// </summary>
        protected TextCursor(string value)
        {
            // Validated by caller.
            Value = value;
            Length = value.Length;
            Move(-1);
        }

        /// <summary>
        /// Gets the current character.
        /// </summary>
        internal char Current { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance has more characters.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has more characters; otherwise, <c>false</c>.
        /// </value>
        internal bool HasMoreCharacters { get { return Index + 1 < Length; } }

        /// <summary>
        /// Gets the current index into the string being parsed.
        /// </summary>
        internal int Index { get; private set; }

        /// <summary>
        /// Gets the length of the string being parsed.
        /// </summary>
        internal int Length { get; private set; }

        /// <summary>
        /// Gets the string being parsed.
        /// </summary>
        internal string Value { get; private set; }

        /// <summary>
        /// Gets the remainder the string that has not been parsed yet.
        /// </summary>
        internal string Remainder { get { return Value.Substring(Index); } }

        /// <summary>
        ///   Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        ///   A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            if (Index < 0)
            {
                builder.Append("<<>>");
                builder.Append(Value);
            }
            else if (Index >= Length)
            {
                builder.Append(Value);
                builder.Append("<<>>");
            }
            else
            {
                builder.Append(Value.Substring(0, Index));
                builder.Append("<<");
                builder.Append(Current);
                builder.Append(">>");
                if (Index < Length - 1)
                {
                    builder.Append(Value.Substring(Index + 1, Length - Index - 1));
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// Returns the next character if there is one or <see cref="Nul" /> if there isn't.
        /// </summary>
        /// <returns></returns>
        internal char PeekNext()
        {
            return HasMoreCharacters ? Value[Index + 1] : Nul;
        }

        /// <summary>
        /// Moves the specified target index. If the new index is out of range of the valid indicies
        /// for this string then the index is set to the beginning or the end of the string whichever
        /// is nearest the requested index.
        /// </summary>
        /// <param name="targetIndex">Index of the target.</param>
        /// <returns><c>true</c> if the requested index is in range.</returns>
        internal bool Move(int targetIndex)
        {
            var inRange = 0 <= targetIndex && targetIndex < Length;
            Index = inRange ? targetIndex : Math.Max(-1, Math.Min(Length, targetIndex));
            Current = inRange ? Value[Index] : Nul;
            return inRange;
        }

        /// <summary>
        /// Moves to the current index. This resets various values and is used when the index
        /// is moved manually.
        /// </summary>
        /// <returns><c>true</c> if the requested index is in range.</returns>
        internal bool MoveCurrent()
        {
            return Move(Index);
        }

        /// <summary>
        /// Moves to the next character.
        /// </summary>
        /// <returns><c>true</c> if the requested index is in range.</returns>
        internal bool MoveNext()
        {
            return Move(Index + 1);
        }

        /// <summary>
        /// Moves to the previous character.
        /// </summary>
        /// <returns><c>true</c> if the requested index is in range.</returns>
        internal bool MovePrevious()
        {
            return Move(Index - 1);
        }

        /// <summary>
        /// Moves the current index forward as long as the current character is a white space character.
        /// </summary>
        /// <returns><c>true</c> if the requested index is in range.</returns>
        internal bool SkipWhiteSpaces()
        {
            while (Current != Nul && char.IsWhiteSpace(Current))
            {
                MoveNext();
            }
            return Current != Nul;
        }

        /// <summary>
        /// If the string starts with a quoted string then any leading white space characters in
        /// that string are removed. This modifies the value string.
        /// </summary>
        internal void TrimLeadingInQuoteSpaces()
        {
            if (Length > 2)
            {
                Move(0);
                if (Current == '\'' || Current == '"')
                {
                    while (MoveNext() && char.IsWhiteSpace(Current))
                    {
                    }
                    if (Index > 1)
                    {
                        Value = Value.Remove(1, Index - 1);
                        Length = Value.Length;
                    }
                }
            }
            Move(-1);
        }

        /// <summary>
        /// Any leading white space characters are removed. This modifies the value string.
        /// </summary>
        internal void TrimLeadingWhiteSpaces()
        {
            Move(0);
            while (Current != Nul && char.IsWhiteSpace(Current))
            {
                MoveNext();
            }
            if (Index > 0)
            {
                Value = Value.Substring(Index);
                Length = Value.Length;
            }
            Move(-1);
        }

        /// <summary>
        /// If the string end with a quoted string then any trailing white space characters in
        /// that string are removed. This modifies the value string.
        /// </summary>
        internal void TrimTrailingInQuoteSpaces()
        {
            // FIXME: Should check for quotes!
            if (Length > 2)
            {
                Move(Length - 1);
                while (MovePrevious() && char.IsWhiteSpace(Current))
                {
                }
                Value = Value.Remove(Index + 1, (Length - 2) - Index);
                Length = Value.Length;
            }
            Move(-1);
        }

        /// <summary>
        /// Any trailing white space characters are removed. This modifies the value string.
        /// </summary>
        internal void TrimTrailingWhiteSpaces()
        {
            while (Length > 0 && char.IsWhiteSpace(Value[Length - 1]))
            {
                Length--;
            }
            Value = Value.Substring(0, Length);
            Length = Value.Length;
            Move(-1);
        }
    }
}
