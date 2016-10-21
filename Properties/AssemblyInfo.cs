//   AssemblyInfo.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2016 Allis Tauri

using System;
using System.Reflection;

// Information about this assembly is defined by the following attributes.
// Change them to the values specific to your project.

[assembly: AssemblyTitle("AT_Utils")]
[assembly: AssemblyDescription("A utility library for Kerbal Space Program plugin development")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("")]
[assembly: AssemblyCopyright("Allis Tauri")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// The assembly version has the format "{Major}.{Minor}.{Build}.{Revision}".
// The form "{Major}.{Minor}.*" will automatically update the build and revision,
// and "{Major}.{Minor}.{Build}.*" will update just the revision.

[assembly: AssemblyVersion("1.2.1")]
[assembly: KSPAssembly("AT_Utils", 1, 2)]

// The following attributes are used to specify the signing key for the assembly,
// if desired. See the Mono documentation for more information about signing.

//[assembly: AssemblyDelaySign(false)]
//[assembly: AssemblyKeyFile("")]

namespace AT_Utils
{
	public class ModInfo : KSP_AVC_Info
	{
		public ModInfo()
		{
			MinKSPVersion = new Version(1,2);
			MaxKSPVersion = new Version(1,2);

			VersionURL   = "https://raw.githubusercontent.com/allista/AT_Utils/master/GameData/000_AT_Utils/000_AT_Utils.version";
			UpgradeURL   = "https://github.com/allista/AT_Utils/releases";
		}
	}
}