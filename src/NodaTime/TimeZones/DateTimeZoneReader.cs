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
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace NodaTime.TimeZones
{
    /// <summary>
    ///   Provides an <see cref = "IDateTimeZone" /> reader that simply reads the values
    ///   without any compression. Can be used as a base for implementing specific 
    ///   compression readers by overriding the methods for the types to be compressed.
    /// </summary>
    internal class DateTimeZoneReader
    {
        public DateTimeZoneReader(Stream input)
        {
            Input = input;
        }

        protected Stream Input { get; private set; }

        #region IDateTimeZoneReader Members
        /// <summary>
        ///   Reads a boolean value from the stream.
        /// </summary>
        /// <remarks>
        ///   The value must have been written by <see cref = "IDateTimeZoneWriter.WriteBoolean" />.
        /// </remarks>
        /// <returns>The boolean value.</returns>
        public virtual bool ReadBoolean()
        {
            return ReadInt8() == 0 ? false : true;
        }

        /// <summary>
        ///   Reads a non-negative integer value from the stream.
        /// </summary>
        /// <remarks>
        ///   The value must have been written by <see cref = "IDateTimeZoneWriter.WriteCount" />.
        /// </remarks>
        /// <returns>The integer value from the stream.</returns>
        public virtual int ReadCount()
        {
            return ReadInt32();
        }

        /// <summary>
        ///   Reads a string to string dictionary value from the stream.
        /// </summary>
        /// <remarks>
        ///   The value must have been written by <see cref = "IDateTimeZoneWriter.WriteDictionary" />.
        /// </remarks>
        /// <returns>The <see cref = "IDictionary{TKey,TValue}" /> value from the stream.</returns>
        public virtual IDictionary<string, string> ReadDictionary()
        {
            IDictionary<string, string> results = new Dictionary<string, string>();
            int count = ReadCount();
            for (int i = 0; i < count; i++)
            {
                string key = ReadString();
                string value = ReadString();
                results.Add(key, value);
            }
            return results;
        }

        /// <summary>
        ///   Reads an enumeration integer value from the stream.
        /// </summary>
        /// <remarks>
        ///   The value must have been written by <see cref = "IDateTimeZoneWriter.WriteEnum" />.
        /// </remarks>
        /// <returns>The integer value from the stream.</returns>
        public virtual int ReadEnum()
        {
            return ReadInteger();
        }

        /// <summary>
        ///   Reads an <see cref = "Instant" /> value from the stream.
        /// </summary>
        /// <remarks>
        ///   The value must have been written by <see cref = "IDateTimeZoneWriter.WriteInstant" />.
        /// </remarks>
        /// <returns>The <see cref = "Instant" /> value from the stream.</returns>
        public virtual Instant ReadInstant()
        {
            return new Instant(ReadTicks());
        }

        /// <summary>
        ///   Reads an integer value from the stream.
        /// </summary>
        /// <remarks>
        ///   The value must have been written by <see cref = "IDateTimeZoneWriter.WriteInteger" />.
        /// </remarks>
        /// <returns>The integer value from the stream.</returns>
        public virtual int ReadInteger()
        {
            return ReadInt32();
        }

        /// <summary>
        ///   Reads an <see cref = "LocalInstant" /> value from the stream.
        /// </summary>
        /// <remarks>
        ///   The value must have been written by <see cref = "IDateTimeZoneWriter.WriteLocalInstant" />.
        /// </remarks>
        /// <returns>The <see cref = "LocalInstant" /> value from the stream.</returns>
        public virtual LocalInstant ReadLocalInstant()
        {
            return new LocalInstant(ReadTicks());
        }

        /// <summary>
        ///   Reads an integer millisecond value from the stream.
        /// </summary>
        /// <remarks>
        ///   The value must have been written by <see cref = "IDateTimeZoneWriter.WriteMilliseconds" />.
        /// </remarks>
        /// <returns>The integer millisecond value from the stream.</returns>
        public virtual int ReadMilliseconds()
        {
            return ReadInt32();
        }

        public virtual Offset ReadOffset()
        {
            return new Offset(ReadMilliseconds());
        }

        /// <summary>
        ///   Reads a string value from the stream.
        /// </summary>
        /// <remarks>
        ///   The value must have been written by <see cref = "IDateTimeZoneWriter.WriteString" />.
        /// </remarks>
        /// <returns>The string value from the stream.</returns>
        public virtual string ReadString()
        {
            int length = ReadCount();
            var data = new byte[length];
            Input.Read(data, 0, length);
            return Encoding.UTF8.GetString(data);
        }

        /// <summary>
        ///   Reads a long ticks value from the stream.
        /// </summary>
        /// <remarks>
        ///   The value must have been written by <see cref = "IDateTimeZoneWriter.WriteTicks" />.
        /// </remarks>
        /// <returns>The long ticks value from the stream.</returns>
        public virtual long ReadTicks()
        {
            return ReadInt64();
        }

        /// <summary>
        ///   Reads an <see cref = "IDateTimeZone" /> value from the stream.
        /// </summary>
        /// <remarks>
        ///   The value must have been written by <see cref = "IDateTimeZoneWriter.WriteTimeZone" />.
        /// </remarks>
        /// <returns>The <see cref = "IDateTimeZone" /> value from the stream.</returns>
        public virtual DateTimeZone ReadTimeZone(string id)
        {
            int flag = ReadInt8();
            if (flag == DateTimeZoneWriter.FlagTimeZoneFixed)
            {
                return FixedDateTimeZone.Read(this, id);
            }
            if (flag == DateTimeZoneWriter.FlagTimeZonePrecalculated)
            {
                return PrecalculatedDateTimeZone.Read(this, id);
            }
            if (flag == DateTimeZoneWriter.FlagTimeZoneCached)
            {
                return CachedDateTimeZone.Read(this, id);
            }
            if (flag == DateTimeZoneWriter.FlagTimeZoneDst)
            {
                return DaylightSavingsTimeZone.Read(this, id);
            }
            if (flag == DateTimeZoneWriter.FlagTimeZoneNull)
            {
                return NullDateTimeZone.Read(this, id);
            }
            string className = ReadString();
            Type type = Type.GetType(className);
            if (type == null)
            {
                throw new InvalidOperationException(@"Unknown DateTimeZone type: " + className);
            }
            MethodInfo method = type.GetMethod("Read", new[] { typeof(DateTimeZoneReader), typeof(string) });
            if (method != null)
            {
                return method.Invoke(null, new object[] { this, id }) as DateTimeZone;
            }
            return null;
        }
        #endregion

        /// <summary>
        ///   Reads a signed 16 bit integer value from the stream and returns it as an int.
        /// </summary>
        /// <returns>The 16 bit int value.</returns>
        protected int ReadInt16()
        {
            unchecked
            {
                int high = ReadInt8() & 0xff;
                int low = ReadInt8() & 0xff;
                return (high << 8) | low;
            }
        }

        /// <summary>
        ///   Reads a signed 32 bit integer value from the stream and returns it as an int.
        /// </summary>
        /// <returns>The 32 bit int value.</returns>
        protected int ReadInt32()
        {
            unchecked
            {
                int high = ReadInt16() & 0xffff;
                int low = ReadInt16() & 0xffff;
                return (high << 16) | low;
            }
        }

        /// <summary>
        ///   Reads a signed 64 bit integer value from the stream and returns it as an long.
        /// </summary>
        /// <returns>The 64 bit long value.</returns>
        protected long ReadInt64()
        {
            unchecked
            {
                long high = ReadInt32() & 0xffffffffL;
                long low = ReadInt32() & 0xffffffffL;
                return (high << 32) | low;
            }
        }

        /// <summary>
        ///   Reads a signed 8 bit integer value from the stream and returns it as an int.
        /// </summary>
        /// <returns>The 8 bit int value.</returns>
        protected byte ReadInt8()
        {
            return (byte)Input.ReadByte();
        }
    }
}
