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
using System.Collections.Generic;
using NodaTime.Calendars;
using NodaTime.Fields;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace NodaTime.TimeZones
{
    public class DateTimeZoneWriter
    {
        internal const long MaxHalfHours = 0x3f;
        private const long MinHalfHours = -MaxHalfHours;
        private const long MaskHalfHour = (MaxHalfHours << 1) + 1;

        internal const long MaxMinutes = 0x1fffffL;
        private const long MinMinutes = -MaxMinutes;
        private const long MaskMinutes = (MaxMinutes << 1) + 1;

        internal const long MaxSeconds = 0x1fffffffffL;
        private const long MinSeconds = -MaxSeconds;
        private const long MaskSeconds = (MaxSeconds << 1) + 1;

        internal const byte FlagHalfHour = 0x00;
        internal const byte FlagMinutes = 0x40;
        internal const byte FlagSeconds = 0x80;
        internal const byte FlagTicks = 0xc0;
        internal const byte FlagOffsetMaxValue = 0xfc;
        internal const byte FlagOffsetMinValue = 0xfd;
        internal const byte FlagMaxValue = 0xfe;
        internal const byte FlagMinValue = 0xff;

        internal const byte FlagTimeZoneUser = 0;
        internal const byte FlagTimeZoneFixed = 1;
        internal const byte FlagTimeZoneCached = 2;
        internal const byte FlagTimeZonePrecalculated = 3;
        internal const byte FlagTimeZoneDst = 4;

        private readonly Stream stream;

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeZoneWriter"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public DateTimeZoneWriter(Stream stream)
        {
            this.stream = stream;
        }

        /// <summary>
        /// Writes the given <see cref="IDateTimeZone"/> object to the given stream.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Currently this method only supports writing of 
        /// </para>
        /// <para>
        /// This method uses a <see cref="BinaryWriter"/> to write the time zone to the stream and
        /// <see cref="BinaryWriter"/> does not support leaving the underlying stream open when it
        /// is closed. Because of this there is no good way to guarentee that the input stream will
        /// still be open because the finalizer for <see cref="BinaryWriter"/> will close the
        /// stream. Therefore, we make sure that the stream is always closed.
        /// </para>
        /// </remarks>
        /// <param name="stream">The stream to write to. Closed on return.</param>
        /// <param name="timeZone">The <see cref="IDateTimeZone"/> to write.</param>
        /// <returns><c>true</c> if the time zone was successfully written.</returns>
        public void WriteTimeZone(IDateTimeZone timeZone)
        {
            if (timeZone is FixedDateTimeZone)
            {
                WriteInt8(FlagTimeZoneFixed);
            }
            else if (timeZone is PrecalculatedDateTimeZone)
            {
                WriteInt8(FlagTimeZonePrecalculated);
            }
            else if (timeZone is CachedDateTimeZone)
            {
                WriteInt8(FlagTimeZoneCached);
            }
            else if (timeZone is DSTZone)
            {
                WriteInt8(FlagTimeZoneDst);
            }
            else
            {
                WriteInt8(FlagTimeZoneUser);
                WriteString(timeZone.GetType().AssemblyQualifiedName);
            }
            timeZone.Write(this);
        }

        /**
         * Ticks encoding formats:
         *
         * upper two bits  units       field length  approximate range
         * ---------------------------------------------------------------
         * 00              30 minutes  1 byte        +/- 16 hours
         * 01              minutes     4 bytes       +/- 1020 years
         * 10              seconds     5 bytes       +/- 4355 years
         * 11000000        ticks       9 bytes       +/- 292,000 years
         * 11111100  0xfc              1 byte         Offset.MaxValue
         * 11111101  0xfd              1 byte         Offset.MinValue
         * 11111110  0xfe              1 byte         Instant, LocalInstant, Duration MaxValue
         * 11111111  0xff              1 byte         Instant, LocalInstant, Duration MinValue
         *
         * Remaining bits in field form signed offset from 1970-01-01T00:00:00Z.
         */
        public void WriteTicks(long ticks)
        {
            unchecked
            {
                if (ticks == Int64.MinValue)
                {
                    WriteInt8(FlagMinValue);
                    return;
                }
                else if (ticks == Int64.MaxValue)
                {
                    WriteInt8(FlagMaxValue);
                    return;
                }
                else if (ticks == Offset.MinValue.Ticks)
                {
                    WriteInt8(FlagOffsetMinValue);
                    return;
                }
                else if (ticks == Offset.MaxValue.Ticks)
                {
                    WriteInt8(FlagOffsetMaxValue);
                    return;
                }
                if (ticks % (30 * NodaConstants.TicksPerMinute) == 0)
                {
                    // Try to write in 30 minute units.
                    long units = ticks / (30 * NodaConstants.TicksPerMinute);
                    if (MinHalfHours <= units && units <= MaxHalfHours)
                    {
                        units = units + MaskHalfHour;
                        WriteInt8((byte)(units & 0x3f));
                        return;
                    }
                }

                if (ticks % NodaConstants.TicksPerMinute == 0)
                {
                    // Try to write minutes.
                    long minutes = ticks / NodaConstants.TicksPerMinute;
                    if (MinMinutes <= minutes && minutes <= MaxMinutes)
                    {
                        minutes = minutes + MaxMinutes;
                        WriteInt32((int)((FlagMinutes << 24) | (int)(minutes & 0x3fffffff)));
                        return;
                    }
                }

                if (ticks % NodaConstants.TicksPerSecond == 0)
                {
                    // Try to write seconds.
                    long seconds = ticks / NodaConstants.TicksPerSecond;
                    if (MinSeconds <= seconds && seconds <= MaxSeconds)
                    {
                        seconds = seconds + MaxSeconds;
                        WriteInt8((byte)(FlagSeconds | (byte)((seconds >> 32) & 0x3f)));
                        WriteInt32((int)(seconds & 0xffffffff));
                        return;
                    }
                }

                // Write milliseconds either because the additional precision is
                // required or the minutes didn't fit in the field.

                // Form 11 (64 bits effective precision, but write as if 70 bits)
                WriteInt8(FlagTicks);
                WriteInt64(ticks);
            }
        }

        /// <summary>
        /// Writes the given dictionary of string to string to the stream.
        /// </summary>
        /// <param name="dictionary">The <see cref="IDictionary"/> to write.</param>
        public void WriteDictionary(IDictionary<string, string> dictionary)
        {
            WriteNumber(dictionary.Count);
            foreach (var entry in dictionary)
            {
                WriteString(entry.Key);
                WriteString(entry.Value);
            }
        }

        /// <summary>
        /// Writes the boolean.
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        public void WriteBoolean(bool value)
        {
            WriteInt8((byte)(value ? 1 : 0));
        }

        /// <summary>
        /// Writes the given number to the stream. The number is compressed in the output into the
        /// fewest necessary bytes.
        /// </summary>
        /// <remarks>
        /// This method is optimized for positive numbers. Negative number always take 5 bytes so if
        /// negative numbers are likely, then <see cref="WriteIn32"/> should be used.
        /// </remarks>
        /// <param name="value">The value to write.</param>
        public void WriteNumber(int value)
        {
            unchecked
            {
                if (value < 0)
                {
                    WriteInt8((byte)0xff);
                    WriteInt32(value);
                    return;
                }
                if (value <= 0x0e)
                {
                    WriteInt8((byte)(0xf0 + value));
                    return;
                }
                value -= 0x0f;
                if (value <= 0x7f)
                {
                    WriteInt8((byte)value);
                    return;
                }
                value -= 0x80;
                if (value <= 0x3fff)
                {
                    WriteInt8((byte)(0x80 + (value >> 8)));
                    WriteInt8((byte)(value & 0xff));
                    return;
                }
                value -= 0x4000;

                if (value <= 0x1fffff)
                {
                    WriteInt8((byte)(0xc0 + (value >> 16)));
                    WriteInt16((short)(value & 0xffff));
                    return;
                }
                value -= 0x200000;
                if (value <= 0x0fffffff)
                {
                    WriteInt8((byte)(0xe0 + (value >> 24)));
                    WriteInt8((byte)((value >> 16) & 0xff));
                    WriteInt16((short)(value & 0xffff));
                    return;
                }
                WriteInt8((byte)0xff);
                WriteInt32(value + 0x200000 + 0x4000 + 0x80 + 0x0f);
            }
        }

        /// <summary>
        /// Writes the given value to the stream.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteInt64(long value)
        {
            unchecked
            {
                WriteInt32((int)(value >> 32));
                WriteInt32((int)value);
            }
        }

        /// <summary>
        /// Writes the given value to the stream.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteInt32(int value)
        {
            unchecked
            {
                WriteInt16((short)(value >> 16));
                WriteInt16((short)value);
            }
        }

        /// <summary>
        /// Writes the given value to the stream.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteInt16(short value)
        {
            unchecked
            {
                WriteInt8((byte)((value >> 8) & 0xff));
                WriteInt8((byte)(value & 0xff));
            }
        }

        /// <summary>
        /// Writes the given value to the stream.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteInt8(byte value)
        {
            unchecked
            {
                this.stream.WriteByte(value);
            }
        }

        /// <summary>
        /// Writes the given string to the stream.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteString(string value)
        {
            byte[] data = Encoding.UTF8.GetBytes(value);
            int length = data.Length;
            WriteNumber(length);
            this.stream.Write(data, 0, data.Length);
        }
    }
}
