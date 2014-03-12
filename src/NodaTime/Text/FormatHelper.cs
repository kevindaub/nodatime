// Copyright 2011 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Globalization;
using System.Text;

namespace NodaTime.Text
{
    /// <summary>
    ///   Provides helper methods for formatting values using pattern strings.
    /// </summary>
    internal static class FormatHelper
    {
        /// <summary>
        /// The maximum number of characters allowed for padded values.
        /// </summary>
        private const int MaximumPaddingLength = 16;

        /// <summary>
        /// Maximum number of digits in a (positive) long.
        /// </summary>
        private const int MaximumInt64Length = 19;

        private static readonly string[] FixedNumberFormats = {
            "0", "00", "000", "0000", "00000", "000000", "0000000", "00000000", "000000000", "0000000000",
            "00000000000", "000000000000", "0000000000000", "00000000000000", "000000000000000",
            "0000000000000000"
        };

        /// <summary>
        /// Formats the given value left padded with zeros.
        /// </summary>
        /// <remarks>
        /// Left pads with zeros the value into a field of <paramref name = "length" /> characters. If the value
        /// is longer than <paramref name = "length" />, the entire value is formatted. If the value is negative,
        /// it is preceded by "-" but this does not count against the length.
        /// </remarks>
        /// <param name="value">The value to format.</param>
        /// <param name="length">The length to fill.</param>
        /// <param name="outputBuffer">The output buffer to add the digits to.</param>
        /// <exception cref="FormatException">if too many characters are requested. <see cref="MaximumPaddingLength" />.</exception>
        internal static void LeftPad(int value, int length, StringBuilder outputBuffer)
        {
            // TODO: Check whether we actually need this check. I think we're already making sure the length is short everywhere.
            if (length > MaximumPaddingLength)
            {
                throw new FormatException("Too many digits");
            }
            unchecked
            {
                if (value < 0)
                {
                    outputBuffer.Append('-');
                    // Special case, as we can't use Math.Abs.
                    if (value == int.MinValue)
                    {
                        if (length > 10)
                        {
                            outputBuffer.Append("000000".Substring(16 - length));
                        }
                        outputBuffer.Append("2147483648");
                        return;
                    }
                    LeftPad(Math.Abs(value), length, outputBuffer);
                    return;
                }
                // Special handling for common cases, because we really don't want a heap allocation
                // if we can help it...
                if (length == 1)
                {
                    if (value < 10)
                    {
                        outputBuffer.Append((char) ('0' + value));
                        return;
                    }
                    // Handle overflow by a single character manually
                    if (value < 100)
                    {
                        char digit1 = (char) ('0' + (value / 10));
                        char digit2 = (char) ('0' + (value % 10));
                        outputBuffer.Append(digit1).Append(digit2);
                        return;
                    }
                }
                if (length == 2 && value < 100)
                {
                    char digit1 = (char) ('0' + (value / 10));
                    char digit2 = (char) ('0' + (value % 10));
                    outputBuffer.Append(digit1).Append(digit2);
                    return;
                }
                if (length == 3 && value < 1000)
                {
                    char digit1 = (char) ('0' + ((value / 100) % 10));
                    char digit2 = (char) ('0' + ((value / 10) % 10));
                    char digit3 = (char) ('0' + (value % 10));
                    outputBuffer.Append(digit1).Append(digit2).Append(digit3);
                    return;
                }
                if (length == 4 && value < 10000)
                {
                    char digit1 = (char) ('0' + (value / 1000));
                    char digit2 = (char) ('0' + ((value / 100)  % 10));
                    char digit3 = (char) ('0' + ((value / 10) % 10));
                    char digit4 = (char) ('0' + (value % 10));
                    outputBuffer.Append(digit1).Append(digit2).Append(digit3).Append(digit4);
                    return;
                }
                if (length == 5 && value < 100000)
                {
                    char digit1 = (char) ('0' + (value / 10000));
                    char digit2 = (char) ('0' + ((value / 1000) % 10));
                    char digit3 = (char) ('0' + ((value / 100) % 10));
                    char digit4 = (char) ('0' + ((value / 10) % 10));
                    char digit5 = (char) ('0' + (value % 10));
                    outputBuffer.Append(digit1).Append(digit2).Append(digit3).Append(digit4).Append(digit5);
                    return;
                }

                // Unfortunate, but never mind - let's go the whole hog...
                var digits = new char[MaximumPaddingLength];
                int pos = MaximumPaddingLength;
                do
                {
                    digits[--pos] = (char) ('0' + (value % 10));
                    value /= 10;
                } while (value != 0 && pos > 0);
                while ((MaximumPaddingLength - pos) < length)
                {
                    digits[--pos] = '0';
                }
                outputBuffer.Append(digits, pos, MaximumPaddingLength - pos);
            }
        }

        /// <summary>
        /// Formats the given value right padded with zeros.
        /// Note: current usage means this never has to cope with negative numbers.
        /// </summary>
        /// <param name="value">The value to format.</param>
        /// <param name="length">The length to fill.</param>
        /// <param name="scale">The scale of the value i.e. the number of significant digits is the range of the value.</param>
        /// <param name="outputBuffer">The output buffer to add the digits to.</param>
        internal static void RightPad(int value, int length, int scale, StringBuilder outputBuffer)
        {
            if (scale < 1)
            {
                throw new FormatException("scale < 1");
            }
            if (length > scale)
            {
                throw new FormatException("length > scale");
            }
            long relevantDigits = value;
            relevantDigits /= (long)Math.Pow(10.0, (scale - length));
            // TODO: Do we really need to call ToString here?
            outputBuffer.Append(((int) relevantDigits).ToString(FixedNumberFormats[length - 1], CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Formats the given value right padded with zeros. The rightmost zeros are truncated.  If the entire value is truncated then
        /// the preceeding decimal separater is also removed.
        /// Note: current usage means this never has to cope with negative numbers.
        /// </summary>
        /// <param name="value">The value to format.</param>
        /// <param name="length">The length to fill.</param>
        /// <param name="scale">The scale of the value i.e. the number of significant digits is the range of the value.</param>
        /// <param name="decimalSeparator">The decimal separator for this culture.</param>
        /// <param name="outputBuffer">The output buffer to add the digits to.</param>
        internal static void RightPadTruncate(int value, int length, int scale, string decimalSeparator, StringBuilder outputBuffer)
        {
            if (scale < 1)
            {
                throw new FormatException("scale < 1");
            }
            if (length > scale)
            {
                throw new FormatException("length > scale");
            }
            long relevantDigits = value;
            relevantDigits /= (long)Math.Pow(10.0, (scale - length));
            int relevantLength = length;
            while (relevantLength > 0)
            {
                if ((relevantDigits % 10L) != 0L)
                {
                    break;
                }
                relevantDigits /= 10L;
                relevantLength--;
            }
            if (relevantLength > 0)
            {
                // TODO: Do we really need to call ToString here?
                outputBuffer.Append(((int)relevantDigits).ToString(FixedNumberFormats[relevantLength - 1], CultureInfo.InvariantCulture));
            }
            else if (outputBuffer.Length > 0 && outputBuffer.ToString().EndsWith(decimalSeparator, StringComparison.CurrentCulture))
            {
                var decimalSeparatorLength = decimalSeparator.Length;
                outputBuffer.Remove(outputBuffer.Length - decimalSeparatorLength, decimalSeparatorLength);
            }
        }

        /// <summary>
        /// Formats the given value using the invariant culture, with no truncation or padding.
        /// </summary>
        /// <param name="value">The value to format.</param>
        /// <param name="outputBuffer">The output buffer to add the digits to.</param>
        internal static void FormatInvariant(long value, StringBuilder outputBuffer)
        {
            unchecked
            {
                if (value <= 0)
                {
                    if (value == 0)
                    {
                        outputBuffer.Append('0');
                        return;
                    }
                    if (value == long.MinValue)
                    {
                        outputBuffer.Append("-9223372036854775808");
                        return;
                    }
                    outputBuffer.Append('-');
                    FormatInvariant(-value, outputBuffer);
                    return;
                }

                var digits = new char[MaximumInt64Length];
                int pos = MaximumInt64Length;
                do
                {
                    digits[--pos] = (char) ('0' + (value % 10));
                    value /= 10;
                } while (value != 0);
                outputBuffer.Append(digits, pos, MaximumInt64Length - pos);
            }
        }
    }
}