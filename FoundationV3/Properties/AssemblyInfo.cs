#region Usings

using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

#endregion

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

[assembly: AssemblyTitle("51degrees - Foundation")]
[assembly: AssemblyDescription("51degrees - Foundation Open Source")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("51 Degrees Mobile Experts Limited")]
[assembly: AssemblyProduct("51degrees - Foundation")]
[assembly: AssemblyCopyright("Copyright 51 Degrees Mobile Experts Limited 2009 - 2015")]
[assembly: AssemblyTrademark("51Degrees")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.

[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM

[assembly: Guid("71439680-d7e5-4497-96ff-3acfb9f68a72")]

// Enable the test assemblies to access internal classes when
// the project is being built normally. If the project not is being
// built for NuGet and SQL then the unit tests will also need to be 
// signed in order to support compilation.
#if !NUGET_BUILD && !SQL_BUILD
[assembly: InternalsVisibleToAttribute("FiftyOne.Tests.Unit")]
[assembly: InternalsVisibleToAttribute("FiftyOne.Tests.Integration")]
#endif

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision

[assembly: AssemblyVersion("3.2.12.3")]
[assembly: AssemblyFileVersion("3.2.12.3")]
[assembly: NeutralResourcesLanguage("en-GB")]
[assembly: AllowPartiallyTrustedCallers]
