// Copyright 2009 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using NodaTime.TimeZones.IO;
using NodaTime.Utility;

namespace NodaTime.TimeZones
{
    /// <summary>
    /// Provides an implementation of a <see cref="IDateTimeZoneSource" /> that looks
    /// for its time zone definitions from a named resource in an assembly.
    /// </summary>
    /// <remarks>
    /// All calls to <see cref="ForId"/> for fixed-offset IDs advertised by the source (i.e. "UTC" and "UTC+/-Offset")
    /// will return zones equal to those returned by <see cref="DateTimeZone.ForOffset"/>.
    /// </remarks>
    /// <threadsafety>This type is immutable reference type. See the thread safety section of the user guide for more information.</threadsafety>
    public sealed class TzdbDateTimeZoneSource : IDateTimeZoneSource
    {
        /// <summary>
        /// The <see cref="TzdbDateTimeZoneSource"/> initialised from resources within the NodaTime assembly.
        /// </summary>
        public static TzdbDateTimeZoneSource Default { get { return DefaultHolder.builtin; } }

        // Class to enable lazy initialization of the default instance.
        private static class DefaultHolder
        {
            internal static readonly TzdbDateTimeZoneSource builtin = new TzdbDateTimeZoneSource(LoadDefaultDataSource());

            private static ITzdbDataSource LoadDefaultDataSource()
            {
                var assembly = typeof(DefaultHolder).Assembly;
                using (Stream stream = assembly.GetManifestResourceStream("NodaTime.TimeZones.Tzdb.nzd"))
                {
                    return TzdbStreamData.FromStream(stream);
                }
            }
        }

        /// <summary>
        /// Original source data - we delegate to this to create actual DateTimeZone instances,
        /// and for windows mappings.
        /// </summary>
        private readonly ITzdbDataSource source;
        /// <summary>
        /// Map from ID (possibly an alias) to canonical ID. This is a read-only wrapper,
        /// and can be returned directly to clients.
        /// </summary>
        private readonly IDictionary<string, string> timeZoneIdMap;
        /// <summary>
        /// Lookup from canonical ID to aliases.
        /// </summary>
        private readonly ILookup<string, string> aliases;
        /// <summary>
        /// Composite version ID including TZDB and Windows mapping version strings.
        /// </summary>
        private readonly string version;

#if !PCL
        /// <summary>
        /// Initializes a new instance of the <see cref="TzdbDateTimeZoneSource" /> class from a resource within
        /// the NodaTime assembly.
        /// </summary>
        /// <remarks>For backwards compatibility, this will use the blob time zone data when given the same
        /// base name which would previously have loaded the now-obsolete resource data.</remarks>
        /// <param name="baseName">The root name of the resource file.</param>
        /// <exception cref="InvalidNodaDataException">The data within the resource is invalid.</exception>
        /// <exception cref="EndOfStreamException">Part of the data within the resource is truncated.</exception>
        /// <exception cref="MissingManifestResourceException">The resource set cannot be found.</exception>
        [Obsolete("Use TzdbDateTimeZoneSource.Default to access the only TZDB resources within the NodaTime assembly")]
        public TzdbDateTimeZoneSource(string baseName)
            : this(baseName, Assembly.GetExecutingAssembly())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TzdbDateTimeZoneSource" /> class.
        /// </summary>
        /// <remarks>For backwards compatibility, this will use the blob time zone data when given the same
        /// base name which would previously have loaded the now-obsolete resource data from the Noda Time assembly
        /// itself.</remarks>
        /// <param name="baseName">The root name of the resource file.</param>
        /// <param name="assembly">The assembly to search for the time zone resources.</param>
        /// <exception cref="InvalidNodaDataException">The data within the resource is invalid.</exception>
        /// <exception cref="EndOfStreamException">Part of the data within the resource is truncated.</exception>
        /// <exception cref="MissingManifestResourceException">The resource set cannot be found.</exception>
        [Obsolete("The resource format for time zone data is deprecated; future versions will only support blob-based data")]
        public TzdbDateTimeZoneSource(string baseName, Assembly assembly)
            : this(TzdbResourceData.FromResource(baseName, assembly))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TzdbDateTimeZoneSource" /> class.
        /// </summary>
        /// <param name="source">The <see cref="ResourceSet"/> to search for the time zone resources.</param>
        /// <exception cref="InvalidNodaDataException">The data within the resource set is invalid.</exception>
        /// <exception cref="EndOfStreamException">Part of the data within the resource set is truncated.</exception>
        [Obsolete("The resource format for time zone data is deprecated; future versions will only support blob-based data")]
        public TzdbDateTimeZoneSource(ResourceSet source)
            : this(TzdbResourceData.FromResourceSet(source))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TzdbDateTimeZoneSource" /> class.
        /// </summary>
        /// <param name="manager">The <see cref="ResourceManager"/> to search for the time zone resources.</param>
        /// <exception cref="InvalidNodaDataException">The data within the resource manager is invalid.</exception>
        /// <exception cref="EndOfStreamException">Part of the data within the resource manager is truncated.</exception>
        [Obsolete("The resource format for time zone data is deprecated; future versions will only support blob-based data")]
        public TzdbDateTimeZoneSource(ResourceManager manager)
            : this(TzdbResourceData.FromResourceManager(manager))
        {
        }
#endif

        /// <summary>
        /// Creates an instance from a stream in the custom Noda Time format. The stream must be readable.
        /// </summary>
        /// <remarks>The stream is not closed by this method, but will be read from
        /// without rewinding. A successful call will read the stream to the end.</remarks>
        /// <param name="stream">The stream containing time zone data</param>
        /// <returns>A TZDB source with information from the given stream.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is null.</exception>
        /// <exception cref="EndOfStreamException">Either the stream or one of the embedded sections ends prematurely.</exception>
        /// <exception cref="InvalidNodaDataException">The stream contains invalid time zone data, or data which cannot
        /// be read by this version of Noda Time.</exception>
        /// <exception cref="IOException">Reading from the stream failed.</exception>
        /// <exception cref="InvalidOperationException">The supplied stream doesn't support reading.</exception>
        public static TzdbDateTimeZoneSource FromStream(Stream stream)
        {
            Preconditions.CheckNotNull(stream, "stream");
            return new TzdbDateTimeZoneSource(TzdbStreamData.FromStream(stream));
        }

        private TzdbDateTimeZoneSource(ITzdbDataSource source)
        {
            Preconditions.CheckNotNull(source, "source");
            this.source = source;
            timeZoneIdMap = new NodaReadOnlyDictionary<string, string>(source.TzdbIdMap);
            aliases = timeZoneIdMap
                .Where(pair => pair.Key != pair.Value)
                .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .ToLookup(pair => pair.Value, pair => pair.Key);
            version = source.TzdbVersion + " (mapping: " + source.WindowsMappingVersion + ")";
        }

        /// <summary>
        /// Returns the time zone definition associated with the given id.
        /// </summary>
        /// <param name="id">The id of the time zone to return.</param>
        /// <returns>
        /// The <see cref="DateTimeZone"/> or null if there is no time zone with the given id.
        /// </returns>
        public DateTimeZone ForId(string id)
        {
            string canonicalId;
            if (!timeZoneIdMap.TryGetValue(id, out canonicalId))
            {
                throw new ArgumentException("Time zone with ID " + id + " not found in source " + version, "id");
            }
            return source.CreateZone(id, canonicalId);
        }

        /// <summary>
        /// Returns a sequence of the available IDs from this source.
        /// </summary>
        [DebuggerStepThrough]
        public IEnumerable<string> GetIds()
        {
            return timeZoneIdMap.Keys;
        }

        /// <summary>
        /// Returns a version identifier for this source.
        /// </summary>
        public string VersionId { get { return "TZDB: " + version; } }

        /// <summary>
        /// Attempts to map the system time zone to a zoneinfo ID, and return that ID.
        /// </summary>
        public string MapTimeZoneId(TimeZoneInfo zone)
        {
#if PCL
            throw new NotSupportedException();
#else
            string result;
            source.WindowsMapping.TryGetValue(zone.Id, out result);
            return result;
#endif
        }

        /// <summary>
        /// Returns a lookup from canonical ID (e.g. "Europe/London") to a group of aliases
        /// (e.g. {"Europe/Belfast", "Europe/Guernsey", "Europe/Jersey", "Europe/Isle_of_Man", "GB", "GB-Eire"}).
        /// </summary>
        /// <remarks>
        /// The group of values for a key never contains the canonical ID, only aliases. Any time zone
        /// ID which is itself an alias or has no aliases linking to it will not be present in the lookup.
        /// The aliases within a group are returned in alphabetical (ordinal) order.
        /// </remarks>
        /// <returns>A lookup from canonical ID to the aliases of that ID.</returns>
        public ILookup<string, string> Aliases { get { return aliases; } }

        /// <summary>
        /// Returns a read-only map from time zone ID to the canonical ID. For example, the key "Europe/Jersey"
        /// would be associated with the value "Europe/London".
        /// </summary>
        /// <remarks>
        /// <para>This map contains an entry for every ID returned by <see cref="GetIds"/>, where
        /// canonical IDs map to themselves.</para>
        /// <para>The returned map is read-only; any attempts to call a mutating method will throw
        /// <see cref="NotSupportedException" />.</para>
        /// </remarks>
        /// <returns>A map from time zone ID to the canonical ID.</returns>
        public IDictionary<string, string> CanonicalIdMap { get { return timeZoneIdMap; } }
    }
}
