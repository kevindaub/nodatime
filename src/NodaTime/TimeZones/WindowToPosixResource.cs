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

using System.Collections.Generic;
using System.Reflection;
using System.Resources;
using NodaTime.Utility;

namespace NodaTime.TimeZones
{
    /// <summary>
    /// Provides a mapping between the time zone ids used by Noda Time (which are the POSIX names)
    /// and the names used by Microsoft Windows.
    /// </summary>
    public static class WindowsToPosixResource
    {
        public const string WindowToPosixMapBase = "winmap";
        public const string WindowToPosixMapBaseFull = "NodaTime.TimeZones." + WindowToPosixMapBase;
        public const string WindowToPosixMapKey = "WindowsToPosix";

        private static readonly IDictionary<string, string> windowsToPosixMap = new Dictionary<string, string>();
        private static readonly IDictionary<string, string> posixToWindowsMap = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeZoneResourceProvider"/> class.
        /// </summary>
        /// <param name="baseName">Name of the base.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static WindowsToPosixResource()
        {
            var manager = new ResourceManager(WindowToPosixMapBaseFull, Assembly.GetExecutingAssembly());
            var map = ResourceHelper.LoadDictionary(manager, WindowToPosixMapKey);
            foreach (var item in map)
            {
                windowsToPosixMap.Add(item.Key, item.Value);
                posixToWindowsMap[item.Value] = item.Key;
            }
        }

        /// <summary>
        /// Gets the time zone id from the given Windows time zone name.
        /// </summary>
        /// <param name="windowsName">The Windows time zone name.</param>
        /// <returns>The time zone id or <c>null</c>.</returns>
        public static string GetIdFromWindowsName(string windowsName)
        {
            string result;
            windowsToPosixMap.TryGetValue(windowsName, out result);
            return result;
        }

        /// <summary>
        /// Gets the Windows time zone name from the given time zone id.
        /// </summary>
        /// <param name="id">The time zone id.</param>
        /// <returns>The Windows time zone name or <c>null</c>.</returns>
        public static string GetWindowsNameFromId(string id)
        {
            string result;
            posixToWindowsMap.TryGetValue(id, out result);
            return result;
        }
    }
}
