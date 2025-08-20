// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;

namespace gip.ext.xamldom.avui
{
	/// <summary>
	/// Static methods to help with designer operations which require access to internal Xaml elements.
	/// </summary>
	public static class XamlStaticTools
    {
        /// <summary>
        /// Gets the Xaml string of the <paramref name="xamlObject"/>
        /// </summary>
        /// <param name="xamlObject">The object whose Xaml is requested.</param>
        public static string GetXaml(XamlObject xamlObject)
        {
            if (xamlObject != null)
                return xamlObject.XmlElement.OuterXml;
            return null;
        }
    }
}
