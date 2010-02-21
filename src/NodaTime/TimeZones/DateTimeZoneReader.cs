#region Copyright and license information
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
using System.Reflection;

namespace NodaTime.TimeZones
{
    public class DateTimeZoneReader
    {
        private readonly Stream stream;

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeZoneWriter"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public DateTimeZoneReader(Stream stream)
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
        public IDateTimeZone ReadTimeZone(string id)
        {
            int flag = ReadByte();
            if (flag == DateTimeZoneWriter.FlagTimeZoneFixed)
            {
                return FixedDateTimeZone.Read(this, id);
            }
            else if (flag == DateTimeZoneWriter.FlagTimeZonePrecalculated)
            {
                return PrecalculatedDateTimeZone.Read(this, id);
            }
            else if (flag == DateTimeZoneWriter.FlagTimeZoneCached)
            {
                return CachedDateTimeZone.Read(this, id);
            }
            else if (flag == DateTimeZoneWriter.FlagTimeZoneDst)
            {
                return DaylightSavingsTimeZone.Read(this, id);
            }
            else
            {
                string className = ReadString();
                Type type = Type.GetType(className);
                if (type == null)
                {
                    throw new InvalidOperationException("Unknown DateTimeZone type: " + className);
                }
                MethodInfo method = type.GetMethod("Read", new Type[] { typeof(DateTimeZoneReader), typeof(string) });
                if (method != null)
                {
                    return method.Invoke(null, new object[] { this, id }) as IDateTimeZone;
                }
            }
            return null;
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
        public long ReadTicks()
        {
            unchecked
            {
                byte flag = (byte)ReadByte();
                if (flag == DateTimeZoneWriter.FlagMinValue)
                {
                    return Int64.MinValue;
                }
                else if (flag == DateTimeZoneWriter.FlagMaxValue)
                {
                    return Int64.MaxValue;
                }

                if ((flag & 0xc0) == DateTimeZoneWriter.FlagHalfHour)
                {
                    long units = (long)flag - DateTimeZoneWriter.MaxHalfHours;
                    return units * (30 * NodaConstants.TicksPerMinute);
                }
                if ((flag & 0xc0) == DateTimeZoneWriter.FlagMinutes)
                {
                    int first = flag & ~0xc0;
                    int second = ReadByte();
                    int third = ReadByte();
                    int fourth = ReadByte();

                    long units = (((((first << 8) + second) << 8) + third) << 8) + fourth;
                    units = units - DateTimeZoneWriter.MaxHalfHours;
                    return units * NodaConstants.TicksPerMinute;
                }
                if ((flag & 0xc0) == DateTimeZoneWriter.FlagSeconds)
                {
                    long first = flag & ~0xc0;
                    long second = (long)ReadInt32() & 0xffffffffL;

                    long units = (first << 32) + second;
                    units = units - DateTimeZoneWriter.MaxSeconds;
                    return units * NodaConstants.TicksPerSecond;
                }

                return ReadInt64();
            }
        }

        /// <summary>
        /// Reads a dictionary of string to string from the stream.
        /// </summary>
        /// <returns>The <see cref="IDictionary"/> to read.</returns>
        public IDictionary<string, string> ReadDictionary()
        {
            IDictionary<string, string> results = new Dictionary<string, string>();
            int count = ReadNumber();
            for (int i = 0; i < count; i++)
            {
                string key = ReadString();
                string value = ReadString();
                results.Add(key, value);
            }
            return results;
        }

        /// <summary>
        /// Writes the offset value to the stream
        /// </summary>
        /// <param name="value">The offset to write.</param>
        public Offset ReadOffset()
        {
            int milliseconds = ReadNumber();
            return new Offset(milliseconds);
        }

        /// <summary>
        /// Reads the boolean.
        /// </summary>
        /// <returns></returns>
        public bool ReadBoolean()
        {
            return ReadByte() == 0 ? false : true;
        }

        /// <summary>
        /// Reads a number from the stream. The number must have been written by <see
        /// cref="DateTimeZoneWriter.WriteNumber"/> as the value is assuemd to be compressed.
        /// </summary>
        /// <returns>The integer value from the stream.</returns>
        public int ReadNumber()
        {
            unchecked
            {
                byte flag = (byte)ReadByte();
                if (flag == 0xff)
                {
                    return ReadInt32();
                }
                if (0xf0 <= flag && flag <= 0xfe)
                {
                    return flag & 0x0f;
                }
                int adjustment = 0x0f;
                if ((flag & 0x80) == 0)
                {
                    return flag + adjustment;
                }
                adjustment += 0x80;
                if ((flag & 0xc0) == 0x80)
                {
                    int first = flag & 0x3f;
                    int second = ReadByte();
                    return ((first << 8) + second) + adjustment;
                }
                adjustment += 0x4000;
                if ((flag & 0xe0) == 0xc0)
                {
                    int first = flag & 0x1f;
                    int second = ReadInt16();
                    return ((first << 16) + second) + adjustment;
                }
                else
                {
                    adjustment += 0x200000;
                    int first = flag & 0x0f;
                    int second = ReadByte();
                    int third = ReadInt16();
                    return (((first << 8) + second) << 16) + third + adjustment;
                }
            }
        }

        public long ReadInt64()
        {
            unchecked
            {
                long high = ReadInt32();
                long low = ReadInt32();
                return (high << 32) + low;
            }
        }

        public int ReadInt32()
        {
            unchecked
            {
                int high = ReadInt16();
                int low = ReadInt16();
                return (high << 16) + low;
            }
        }

        public int ReadInt16()
        {
            unchecked
            {
                int high = ReadByte();
                int low = ReadByte();
                return (high << 8) + low;
            }
        }

        public int ReadByte()
        {
            return this.stream.ReadByte();
        }

        /// <summary>
        /// Writes the given string to the stream.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public string ReadString()
        {
            int length = ReadNumber();
            byte[] data = new byte[length];
            stream.Read(data, 0, length);
            return Encoding.UTF8.GetString(data);
        }
    }
}
