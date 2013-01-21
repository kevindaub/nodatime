#region Copyright and license information
// Copyright 2001-2009 Stephen Colebourne
// Copyright 2009-2012 Jon Skeet
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
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

[assembly: AssemblyTitle("NodaTime")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("NodaTime")]
[assembly: AssemblyCopyright("Copyright � 2009-2012 by Jon Skeet. All rights reserved.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

#if !PCL
// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.

[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM

[assembly: Guid("c91ba14f-e2a9-4ed0-a501-a31b97c035fb")]
#endif

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]

[assembly: AssemblyVersion("1.1")]
[assembly: AssemblyFileVersion("1.1.0")]
[assembly: AssemblyInformationalVersion("1.1.0-dev")]
[assembly: CLSCompliant(true)]
[assembly: NeutralResourcesLanguage("en")]

[assembly: InternalsVisibleTo("NodaTime.Test" + NodaTime.Properties.AssemblyInfo.PublicKeySuffix)]
[assembly: InternalsVisibleTo("ZoneInfoCompiler" + NodaTime.Properties.AssemblyInfo.PublicKeySuffix)]
[assembly: InternalsVisibleTo("ZoneInfoCompiler.Test" + NodaTime.Properties.AssemblyInfo.PublicKeySuffix)]
[assembly: InternalsVisibleTo("NodaTime.Benchmarks" + NodaTime.Properties.AssemblyInfo.PublicKeySuffix)]

namespace NodaTime.Properties
{
    /// <summary>
    /// Just a static class to house the public key, which allows us to avoid repeating it all over the place.
    /// </summary>
    internal static class AssemblyInfo
    {
#if SIGNED
        internal const string PublicKeySuffix =
            ",PublicKey=0024000004800000940000000602000000240000525341310004000001000100d335797ef2bff7"
            + "4db7c046f874523c553f88d3f8e0c2ba769820c54f0e64a11b47198b544c74abb487f8d3b64669"
            + "08ae2ac6fced4738e46a75e5661d5ac03fb29c7e26b13a220400cb9df95134e85716203f83b96f"
            + "ab661135c39b10f33e1c467a6750d8af331c602351b09a7bf5dd3a8943712d676481c5054c8031"
            + "84f77ed5";
#else
        // If we're building an unsigned release, all the InternalsVisibleToAttribute arguments
        // should have an empty suffix.
        internal const string PublicKeySuffix = "";
#endif
    }
}
