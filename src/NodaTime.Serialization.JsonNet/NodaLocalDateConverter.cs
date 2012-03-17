﻿#region Copyright and license information
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
using System.Globalization;
using Newtonsoft.Json;

namespace NodaTime.Serialization.JsonNet
{
    /// <summary>
    /// Converts an <see cref="LocalDate"/> to and from the ISO 8601 date format (e.g. 2008-04-12).
    /// </summary>
    public class NodaLocalDateConverter : JsonConverter
    {
        private const string DefaultDateTimeFormat = "yyyy'-'MM'-'dd";

        public NodaLocalDateConverter()
        {
            // default values
            DateFormat = DefaultDateTimeFormat;
            Culture = CultureInfo.CurrentCulture;
        }

        /// <summary>
        /// Gets or sets the date format used when converting to and from JSON.
        /// </summary>
        /// <value>The date format used when converting to and from JSON.</value>
        public string DateFormat { get; set; }

        /// <summary>
        /// Gets or sets the culture used when converting to and from JSON.
        /// </summary>
        /// <value>The culture used when converting to and from JSON.</value>
        public CultureInfo Culture { get; set; }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(LocalDate) || objectType == typeof(LocalDate?);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (!(value is LocalDate))
                throw new Exception(string.Format("Unexpected value when converting. Expected NodaTime.LocalDate, got {0}.", value.GetType().FullName));
            
            var localDate = (LocalDate)value;

            if (localDate.Calendar.Name != "ISO")
                throw new NotSupportedException("Sorry, only the ISO calendar is currently supported for serialization.");

            var text = localDate.ToString(DateFormat, Culture);
            writer.WriteValue(text);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                if (objectType != typeof(LocalDate?))
                    throw new Exception(string.Format("Cannot convert null value to {0}.", objectType));

                return null;
            }

            if (reader.TokenType != JsonToken.String)
                throw new Exception(string.Format("Unexpected token parsing instant. Expected String, got {0}.", reader.TokenType));

            var localDateText = reader.Value.ToString();

            if (string.IsNullOrEmpty(localDateText) && objectType == typeof(LocalDate?))
                return null;

            return string.IsNullOrEmpty(DateFormat)
                       ? LocalDate.Parse(localDateText, Culture)
                       : LocalDate.ParseExact(localDateText, DateFormat, Culture);
        }
    }
}
