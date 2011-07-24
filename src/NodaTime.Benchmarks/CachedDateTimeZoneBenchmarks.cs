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

using NodaTime.Benchmarks.Timing;
using NodaTime.TimeZones;

namespace NodaTime.Benchmarks
{
    internal class CachedDateTimeZoneBenchmarks
    {
        private readonly Instant[] cacheInstants = new Instant[100];
        private readonly Instant[] noCacheInstants = new Instant[500];
        private readonly DateTimeZone paris = DateTimeZone.ForId("Europe/Paris");
        private readonly Instant[] twoYearsCacheInstants = new Instant[365];
        private int cacheIndex;
        private int noCacheIndex;
        private int twoYearsCacheIndex;

        public CachedDateTimeZoneBenchmarks()
        {
            var adjustment = new Duration(NodaConstants.TicksPerStandardDay * 365);
            for (int i = 0; i < noCacheInstants.Length; i++)
            {
                noCacheInstants[i] = Instant.UnixEpoch + (adjustment * i);
            }
            for (int i = 0; i < cacheInstants.Length; i++)
            {
                cacheInstants[i] = Instant.UnixEpoch + (adjustment * i);
            }
            var twoDays = new Duration(NodaConstants.TicksPerStandardDay * 2);
            for (int i = 0; i < twoYearsCacheInstants.Length; i++)
            {
                twoYearsCacheInstants[i] = Instant.UnixEpoch + (twoDays * i);
            }
        }

        [Benchmark]
        public void GetPeriodInstant()
        {
            paris.GetZoneInterval(Instant.UnixEpoch);
        }

        [Benchmark]
        public void GetPeriodInstant_NoCache()
        {
            paris.GetZoneInterval(noCacheInstants[noCacheIndex]);
            noCacheIndex = (noCacheIndex + 1) % noCacheInstants.Length;
        }

        [Benchmark]
        public void GetPeriodInstant_Cache()
        {
            paris.GetZoneInterval(cacheInstants[cacheIndex]);
            cacheIndex = (cacheIndex + 1) % cacheInstants.Length;
        }

        [Benchmark]
        public void GetPeriodInstant_TwoYears()
        {
            paris.GetZoneInterval(twoYearsCacheInstants[twoYearsCacheIndex]);
            twoYearsCacheIndex = (twoYearsCacheIndex + 1) % twoYearsCacheInstants.Length;
        }

        [Benchmark]
        public void GetOffsetFromUtc()
        {
            paris.GetOffsetFromUtc(Instant.UnixEpoch);
        }

        [Benchmark]
        public void GetOffsetFromLocal()
        {
            paris.GetOffsetFromLocal(LocalInstant.LocalUnixEpoch);
        }

        [Benchmark]
        public void Name()
        {
            paris.GetName(Instant.UnixEpoch);
        }
    }
}