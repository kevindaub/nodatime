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

namespace NodaTime.Benchmarks.Timing
{
    /// <summary>
    /// Timer used to measure performance. Implementations
    /// may use wall time, CPU timing etc. Implementations
    /// don't need to be thread-safe.
    /// </summary>
    internal interface IBenchTimer
    {
        void Reset();
        Duration ElapsedTime { get; }
    }
}