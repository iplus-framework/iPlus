using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Markup;

// Allgemeine Informationen über eine Assembly werden über die folgenden 
// Attribute gesteuert. Ändern Sie diese Attributwerte, um die Informationen zu ändern,
// die mit einer Assembly verknüpft sind.
[assembly: AssemblyTitle("gip.ext.widgets")]
[assembly: AssemblyDescription("WPF-Controls for XAML-Editor based on SharpDevelop 2.0")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("gipSoft d.o.o.")]
[assembly: AssemblyProduct("iPlus gip.ext.widgets")]
[assembly: AssemblyCopyright("LGPL SharpDevelop 2.0")]
[assembly: AssemblyTrademark("IPlus")]
[assembly: AssemblyCulture("")]

// Durch Festlegen von ComVisible auf "false" werden die Typen in dieser Assembly unsichtbar 
// für COM-Komponenten.  Wenn Sie auf einen Typ in dieser Assembly von 
// COM zugreifen müssen, legen Sie das ComVisible-Attribut für diesen Typ auf "true" fest.
[assembly: ComVisible(false)]

// Die folgende GUID bestimmt die ID der Typbibliothek, wenn dieses Projekt für COM verfügbar gemacht wird
//[assembly: Guid("757d50b1-0b95-43a9-9c55-42fee8db140d")]

// Versionsinformationen für eine Assembly bestehen aus den folgenden vier Werten:
//
//      Hauptversion
//      Nebenversion 
//      Buildnummer
//      Revision
//
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

[assembly: CLSCompliant(false)]


[assembly: ThemeInfo(
    ResourceDictionaryLocation.SourceAssembly, //where theme specific resource dictionaries are located
    ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
)]

[assembly: XmlnsPrefix("http://icsharpcode.net/sharpdevelop/widgets", "widgets")]
[assembly: XmlnsDefinition("http://icsharpcode.net/sharpdevelop/widgets", "gip.ext.widgets")]