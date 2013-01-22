namespace NodaTime.TimeZones.IO
{
    /// <summary>
    /// Enumeration of the fields which can occur in a TZDB stream file.
    /// This enables the file to be self-describing to a reasonable extent.
    /// </summary>
    internal enum TzdbStreamFieldId : byte
    {
        /// <summary>
        /// String pool. Format is: number of strings (WriteCount) followed by that many string values.
        /// The indexes into the resultant list are used for other strings in the file, in some fields.
        /// </summary>
        StringPool = 0,
        /// <summary>
        /// Repeated field of time zones. Format is: zone ID, then zone as written by DateTimeZoneWriter.
        /// </summary>
        TimeZone = 1,
        /// <summary>
        /// Single field giving the version of the TZDB source data. A string value which does *not* use the string pool.
        /// </summary>
        TzdbVersion = 2,
        /// <summary>
        /// Single field giving the mapping of ID to canonical ID, as written by DateTimeZoneWriter.WriteDictionary.
        /// </summary>
        TzdbIdMap = 3,
        /// <summary>
        /// Single field giving the version of Windows Mapping source data from CLDR. A string value which does *not* use
        /// the string pool.
        /// </summary>
        WindowsMappingVersion = 4,
        /// <summary>
        /// Single field giving the mapping of Windows system ID to TZDB canonical ID,
        /// as written by DateTimeZoneWriter.WriteDictionary.
        /// </summary>
        WindowsMapping = 5
    }
}
