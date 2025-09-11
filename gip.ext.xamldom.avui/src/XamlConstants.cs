// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;

namespace gip.ext.xamldom.avui
{
	/// <summary>
	/// Contains constants used by the Xaml parser.
	/// </summary>
	public static class XamlConstants
	{
        #region Namespaces

        /// <summary>
        /// The namespace used to identify "xmlns".
        /// Value: "http://www.w3.org/2000/xmlns/"
        /// </summary>
        public const string XmlnsNamespace = "http://www.w3.org/2000/xmlns/";

        /// <summary>
        /// The namespace used for the XAML schema.
        /// Value: "http://schemas.microsoft.com/winfx/2006/xaml"
        /// </summary>
        public const string XamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

        /// <summary>
        /// The namespace used for the 2009 XAML schema.
        /// Value: "http://schemas.microsoft.com/winfx/2009/xaml"
        /// </summary>
        public const string Xaml2009Namespace = "http://schemas.microsoft.com/winfx/2009/xaml";

        /// <summary>
        /// The namespace used for the WPF schema.
        /// Value: "https://github.com/avaloniaui"
        /// </summary>
        public const string PresentationNamespace = "https://github.com/avaloniaui";

        /// <summary>
        /// The namespace used for the DesignTime schema.
        /// Value: "http://schemas.microsoft.com/expression/blend/2008"
        /// </summary>
        public const string DesignTimeNamespace = "http://schemas.microsoft.com/expression/blend/2008";

        /// <summary>
        /// The namespace used for the MarkupCompatibility schema.
        /// Value: "http://schemas.openxmlformats.org/markup-compatibility/2006"
        /// </summary>
        public const string MarkupCompatibilityNamespace = "http://schemas.openxmlformats.org/markup-compatibility/2006";

        #endregion

        #region Common property names

        /// <summary>
        /// The name of the Resources property.
        /// Value: "Resources"
        /// </summary>
        public const string ResourcesPropertyName = "Resources";

        /// <summary>
        /// The name of xmlns.
        /// Value: "xmlns"
        /// </summary>
        public const string Xmlns = "xmlns";

        #endregion
	}
}
