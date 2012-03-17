﻿using Newtonsoft.Json;

namespace NodaTime.Serialization.JsonNet
{
    public static class Extensions
    {
        /// <summary>
        /// Configures json.net with everything required to properly serialize and deserialize NodaTime data types.
        /// </summary>
        public static JsonSerializerSettings ConfigureForNodaTime(this JsonSerializerSettings settings)
        {
            // add our converters
            settings.Converters.Add(new NodaInstantConverter());
            settings.Converters.Add(new NodaIntervalConverter());
            settings.Converters.Add(new NodaLocalDateConverter());
            settings.Converters.Add(new NodaLocalDateTimeConverter());
            settings.Converters.Add(new NodaLocalTimeConverter());

            // return to allow fluent chaining if desired
            return settings;
        }

        /// <summary>
        /// Configures json.net with everything required to properly serialize and deserialize NodaTime data types.
        /// </summary>
        public static JsonSerializer ConfigureForNodaTime(this JsonSerializer serializer)
        {
            // add our converters
            serializer.Converters.Add(new NodaInstantConverter());
            serializer.Converters.Add(new NodaIntervalConverter());
            serializer.Converters.Add(new NodaLocalDateConverter());
            serializer.Converters.Add(new NodaLocalDateTimeConverter());
            serializer.Converters.Add(new NodaLocalTimeConverter());

            // return to allow fluent chaining if desired
            return serializer;
        }
    }
}
