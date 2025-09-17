// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Reflection;
using gip.ext.designer.avui.Services;
using gip.ext.xamldom.avui;
using gip.ext.graphics.avui.shapes;

namespace gip.ext.designer.avui.Xaml
{
	/// <summary>
	/// Settings used to load a XAML document.
	/// </summary>
	public sealed class XamlLoadSettings
	{
		public readonly ICollection<Assembly> DesignerAssemblies = new List<Assembly>();
		public readonly List<Action<XamlDesignContext>> CustomServiceRegisterFunctions = new List<Action<XamlDesignContext>>();
		public Action<XamlErrorService> ReportErrors;
		XamlTypeFinder typeFinder = XamlTypeFinder.CreateWpfTypeFinder();
		
		public XamlTypeFinder TypeFinder {
			get { return typeFinder; }
			set {
				if (value == null)
					throw new ArgumentNullException("value");
				typeFinder = value;
			}
		}

        public string CurrentProjectAssemblyName { get; set; }

        public XamlLoadSettings()
		{
			DesignerAssemblies.Add(typeof(XamlDesignContext).Assembly);
        }
	}
}
