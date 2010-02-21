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

namespace NodaTime.TimeZones
{
    /// <summary>
    /// Static access to time zones by ID, UTC etc. These were originally in DateTimeZone, but as
    /// that's now an interface it can't have methods etc.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The UTC time zone is defined to be present in all time zone databases and is built into the
    /// Noda Time system so it is always available. It is the only one guarenteed to be present in
    /// all systems.
    /// </para>
    /// <para>
    /// All other time zones are acquired through providers. These are abstractions around the
    /// method of reading in or building the time zone definitions from the id. There is a list of
    /// providers and each is asked in turn for the matching time zone. If it does not have one the
    /// next is asked until one does return a time zone or the list is exhusted. Providers can use
    /// any mthod they want to resolve the time zone from the id.
    /// </para>
    /// <para>
    /// The majority of time zones will be provided by the <see
    /// cref="DatetimeZoneResourceProvider"/> implementation which reads the time zones from an
    /// internal set of precompiled resources built using the <see cref="ZoneInfoCompiler"/> tool.
    /// </para>
    /// </remarks>
    public static class DateTimeZones
    {
        /// <summary>
        /// This is the ID of the UTC (Coordinated Univeral Time) time zone.
        /// </summary>
        public const string UtcId = "UTC";

        private static readonly IDateTimeZone utc = new FixedDateTimeZone(UtcId, Offset.Zero);

        private readonly static Impl implementation = new Impl();

        /// <summary>
        /// Initializes the <see cref="DateTimeZones"/> class.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static DateTimeZones()
        {
            implementation.AddStandardProvider();
        }

        /// <summary>
        /// Gets the system default time zone which can only be changed by the system.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The time zones defined in the operating system are different than the ones defines in
        /// this library so a mapping will occur. If an exact mapping can be made then that will be
        /// used otherwise UTC will be used.
        /// </para>
        /// </remarks>
        /// <value>The system default <see cref="IDateTimeZone"/>. this will never be <c>null</c>.</value>
        public static IDateTimeZone SystemDefault
        {
            get
            {
                var systemName = TimeZone.CurrentTimeZone.StandardName;
                string timeZoneId = WindowsToPosixResource.GetIdFromWindowsName(systemName);
                if (timeZoneId == null)
                {
                    timeZoneId = UtcId;
                }
                var timeZone = ForId(timeZoneId);
                if (timeZone == null)
                {
                    timeZone = Utc;
                }
                return timeZone;
            }
        }

        /// <summary>
        /// Gets the UTC (Coordinated Univeral Time) time zone.
        /// </summary>
        /// <value>The UTC <see cref="IDateTimezone"/>.</value>
        public static IDateTimeZone Utc
        {
            get { return utc; }
        }

        /// <summary>
        /// Gets or sets the current time zone.
        /// </summary>
        /// <remarks>
        /// This is the time zone that is used whenever a time zone is not given to a method. It can
        /// be set to any valid time zone. Setting it to <c>null</c> causes the <see
        /// cref="SystemDefault"/> time zone to be used.
        /// </remarks>
        /// <value>The current <see cref="IDateTimeZone"/>. This will never be <c>null</c>.</value>
        public static IDateTimeZone Current
        {
            get { return implementation.Current; }
            set { implementation.Current = value; }
        }

        /// <summary>
        /// Adds the given time zone provider to the front of the provider list.
        /// </summary>
        /// <remarks>
        /// Because this adds the new provider to the from of the list, it will be checked first for
        /// time zone definitions and therefore can override the default system definitions. This
        /// allows for adding new or replacing existing time zones without updating the system. If
        /// the provider is already on the list nothing changes.
        /// </remarks>
        /// <param name="provider">The <see cref="IDateTimeZoneProvider"/> to add.</param>
        public static void AddProvider(IDateTimeZoneProvider provider)
        {
            implementation.AddProvider(provider);
        }

        /// <summary>
        /// Removes the given time zone provider from the provider list.
        /// </summary>
        /// <remarks>
        /// If the provider is not on the list nothing changes.
        /// </remarks>
        /// <param name="provider">The <see cref="IDateTimeZoneProvider"/> to remove.</param>
        /// <returns><c>true</c> if the provider was removed.</returns>
        public static bool RemoveProvider(IDateTimeZoneProvider provider)
        {
            return implementation.RemoveProvider(provider);
        }

        /// <summary>
        /// Gets the complete list of valid time zone ids provided by all of the registered
        /// providers. This list will be sorted in lexigraphical order by the id name.
        /// </summary>
        /// <value>The <see cref="IEnumerable"/> of string ids.</value>
        public static IEnumerable<string> Ids
        {
            get { return implementation.Ids; }
        }

        /// <summary>
        /// Returns the time zone with the given id.
        /// </summary>
        /// <param name="id">The time zone id to find.</param>
        /// <returns>The <see cref="IDateTimeZone"/> with the given id or <c>null</c> if there isn't one defined.</returns>
        public static IDateTimeZone ForId(string id)
        {
            return implementation.ForId(id);
        }

        /// <summary>
        /// Provides a standard object that implements the functionality of the static class <see
        /// cref="DateTimeZones"/>. This makes testing simpler because all of the logic is here only
        /// this class needs to be tested and it can use the standard testing methods.
        /// </summary>
        internal class Impl
        {
            private IDateTimeZone current = null;
            private readonly LinkedList<IDateTimeZoneProvider> providers = new LinkedList<IDateTimeZoneProvider>();
            private readonly SortedDictionary<string, string> idList = new SortedDictionary<string, string>();
            private readonly IDictionary<string, IDateTimeZone> timeZoneMap = new Dictionary<string, IDateTimeZone>();

            /// <summary>
            /// Adds the standard provider to the list of providers.
            /// </summary>
            internal void AddStandardProvider()
            {
                AddProvider(new DateTimeZoneResourceProvider("NodaTime.TimeZones.Tzdb"));
            }

            /// <summary>
            /// Gets or sets the current time zone.
            /// </summary>
            /// <remarks>
            /// This is the time zone that is used whenever a time zone is not given to a method. It can
            /// be set to any valid time zone. Setting it to <c>null</c> causes the <see
            /// cref="SystemDefault"/> time zone to be used.
            /// </remarks>
            /// <value>The current <see cref="IDateTimeZone"/>. This will never be <c>null</c>.</value>
            internal IDateTimeZone Current
            {
                get
                {
                    if (this.current == null)
                    {
                        return SystemDefault;
                    }
                    return this.current;
                }
                set { this.current = value; }
            }

            /// <summary>
            /// Adds the given time zone provider to the front of the provider list.
            /// </summary>
            /// <remarks>
            /// Because this adds the new provider to the from of the list, it will be checked first for
            /// time zone definitions and therefore can override the default system definitions. This
            /// allows for adding new or replacing existing time zones without updating the system. If
            /// the provider is already on the list nothing changes.
            /// </remarks>
            /// <param name="provider">The <see cref="IDateTimeZoneProvider"/> to add.</param>
            internal void AddProvider(IDateTimeZoneProvider provider)
            {
                if (!this.providers.Contains(provider))
                {
                    this.providers.AddFirst(provider);
                    this.timeZoneMap.Clear();
                    this.idList.Clear();
                }
            }

            /// <summary>
            /// Removes the given time zone provider from the provider list.
            /// </summary>
            /// <remarks>
            /// If the provider is not on the list nothing changes.
            /// </remarks>
            /// <param name="provider">The <see cref="IDateTimeZoneProvider"/> to remove.</param>
            /// <returns><c>true</c> if the provider was removed.</returns>
            internal bool RemoveProvider(IDateTimeZoneProvider provider)
            {
                if (this.providers.Contains(provider))
                {
                    this.providers.Remove(provider);
                    this.timeZoneMap.Clear();
                    this.idList.Clear();
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Gets the complete list of valid time zone ids provided by all of the registered
            /// providers. This list will be sorted in lexigraphical order by the id name.
            /// </summary>
            /// <value>The <see cref="IEnumerable"/> of string ids.</value>
            internal IEnumerable<string> Ids
            {
                get
                {
                    if (this.idList.Count == 0)
                    {
                        this.idList.Add(UtcId, null);
                        foreach (var provider in this.providers)
                        {
                            foreach (var id in provider.Ids)
                            {
                                this.idList.Add(id, null);
                            }
                        }
                    }
                    return this.idList.Keys;
                }
            }

            /// <summary>
            /// Returns the time zone with the given id.
            /// </summary>
            /// <param name="id">The time zone id to find.</param>
            /// <returns>The <see cref="IDateTimeZone"/> with the given id or <c>null</c> if there isn't one defined.</returns>
            internal IDateTimeZone ForId(string id)
            {
                IDateTimeZone result = Utc;
                if (id != UtcId)
                {
                    if (!this.timeZoneMap.TryGetValue(id, out result))
                    {
                        foreach (var provider in this.providers)
                        {
                            result = provider.ForId(id);
                            if (result != null)
                            {
                                break;
                            }
                        }
                        this.timeZoneMap.Add(id, result);
                    }
                }
                return result;
            }
        }
    }
}