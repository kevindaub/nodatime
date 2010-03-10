#region Copyright and license information

// Copyright 2001-2010 Stephen Colebourne
// Copyright 2010 Jon Skeet
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

namespace NodaTime
{
    /// <summary>
    /// Defines the interface of a clock object that can tell the time as an <see cref="Instant"/>.
    /// </summary>
    public interface IClock
    {
        /// <summary>
        /// Gets the current time as an <see cref="Instant"/>.
        /// </summary>
        /// <value>The current time in ticks as an <see cref="Instant"/>.</value>
        Instant Now { get; }
    }
}