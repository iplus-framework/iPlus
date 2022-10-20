using System;
using System.Resources;
using System.Windows.Markup;
using System.Runtime.CompilerServices;
using gip.ext.chart;
using System.Diagnostics.CodeAnalysis;

[module: SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]

[assembly: XmlnsDefinition(D3AssemblyConstants.DefaultXmlNamespace, "gip.ext.chart")]
[assembly: XmlnsDefinition(D3AssemblyConstants.DefaultXmlNamespace, "gip.ext.chart.Navigation")]
[assembly: XmlnsDefinition(D3AssemblyConstants.DefaultXmlNamespace, "gip.ext.chart.Charts.Navigation")]
[assembly: XmlnsDefinition(D3AssemblyConstants.DefaultXmlNamespace, "gip.ext.chart.Charts")]
[assembly: XmlnsDefinition(D3AssemblyConstants.DefaultXmlNamespace, "gip.ext.chart.DataSources")]
[assembly: XmlnsDefinition(D3AssemblyConstants.DefaultXmlNamespace, "gip.ext.chart.Common.Palettes")]
[assembly: XmlnsDefinition(D3AssemblyConstants.DefaultXmlNamespace, "gip.ext.chart.Charts.Axes")]
[assembly: XmlnsDefinition(D3AssemblyConstants.DefaultXmlNamespace, "gip.ext.chart.PointMarkers")]
[assembly: XmlnsDefinition(D3AssemblyConstants.DefaultXmlNamespace, "gip.ext.chart.Charts.Shapes")]

[assembly: XmlnsPrefix(D3AssemblyConstants.DefaultXmlNamespace, "d3")]

[assembly: CLSCompliant(true)]
[assembly: InternalsVisibleTo("DynamicDataDisplay.Test, PublicKey=002400000480000094000000060200000024000052534131000400000100010039f88065585acdedaac491218a8836c4c54070b4b0f85bc909bd002856b509349f95fc845d1d4c664ea6b93045f2ada3b4fe70c6cd9b3fb615f94b8b5f67e4ea8ea5decb233e2e3c0ce84b78dc3ca0cd9fd2260792ece12224fca5813f03c7ad57b1faa07e3ca8fafb278fa23976fc7a35b8b4ae4efedacd1e193d89738ac2aa")]
[assembly: InternalsVisibleTo("DynamicDataDisplay.Maps, PublicKey=002400000480000094000000060200000024000052534131000400000100010039f88065585acdedaac491218a8836c4c54070b4b0f85bc909bd002856b509349f95fc845d1d4c664ea6b93045f2ada3b4fe70c6cd9b3fb615f94b8b5f67e4ea8ea5decb233e2e3c0ce84b78dc3ca0cd9fd2260792ece12224fca5813f03c7ad57b1faa07e3ca8fafb278fa23976fc7a35b8b4ae4efedacd1e193d89738ac2aa")]

namespace gip.ext.chart
{
	public static class D3AssemblyConstants
	{
		public const string DefaultXmlNamespace = "http://research.microsoft.com/DynamicDataDisplay/1.0";
	}
}
