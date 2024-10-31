//-----------------------------------------------------------------------------
//                                                                            |
//                               Softing AG                                   |
//                        Richard-Reitzner-Allee 6                            |
//                           85540 Haar, Germany                              |
//                                                                            |
//                 This is a part of the Softing OPC Toolbox                  |
//                   Copyright (C) Softing AG 2005 - 2008                     |
//                           All Rights Reserved                              |
//                                                                            |
//-----------------------------------------------------------------------------
//-----------------------------------------------------------------------------
//                             OPC Toolbox .NET                               |
//                                                                            |
//  Filename    : AssemblyInfo.cs                                             |
//  Version     : 4.20.release                                                |
//  Date        : 29-February-2008                                            |
//                                                                            |
//  Description : Assembly information                                        |
//                                                                            |
//-----------------------------------------------------------------------------

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Resources;

//
// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
[assembly: AssemblyTitle("Softing OPC Toolbox .NET")]
[assembly: AssemblyDescription("Softing OPC Toolbox .NET")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Softing AG")]
[assembly: AssemblyProduct("Softing OPC Toolbox .NET")]
[assembly: AssemblyCopyright("Copyright � 2005-2008 Softing AG")]
[assembly: AssemblyTrademark("iplus-framework")]
[assembly: AssemblyCulture("")]		

//
// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:

[assembly: AssemblyVersion("4.22.1.1906")]
[assembly: AssemblyFileVersion("4.22.1.1906")]
//
// In order to sign your assembly you must specify a key to use. Refer to the 
// Microsoft .NET Framework documentation for more information on assembly signing.
//
// Use the attributes below to control which key is used for signing. 
//
// Notes: 
//   (*) If no key is specified, the assembly is not signed.
//   (*) KeyName refers to a key that has been installed in the Crypto Service
//       Provider (CSP) on your machine. KeyFile refers to a file which contains
//       a key.
//   (*) If the KeyFile and the KeyName values are both specified, the 
//       following processing occurs:
//       (1) If the KeyName can be found in the CSP, that key is used.
//       (2) If the KeyName does not exist and the KeyFile does exist, the key 
//           in the KeyFile is installed into the CSP and used.
//   (*) In order to create a KeyFile, you can use the sn.exe (Strong Name) utility.
//       When specifying the KeyFile, the location of the KeyFile should be
//       relative to the project output directory which is
//       %Project Directory%\obj\<configuration>. For example, if your KeyFile is
//       located in the project directory, you would specify the AssemblyKeyFile 
//       attribute as [assembly: AssemblyKeyFile("..\\..\\mykey.snk")]
//   (*) Delay Signing is an advanced option - see the Microsoft .NET Framework
//       documentation for more information on this.
//
[assembly: AssemblyDelaySign(false)]
#if (!NET20) && (!NET35)
[assembly: AssemblyKeyFile("..\\..\\SoftingKeyPair.snk")]
#endif
[assembly: AssemblyKeyName("")]
[assembly: ComVisibleAttribute(false)]
[assembly: NeutralResourcesLanguageAttribute("en")]
