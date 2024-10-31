// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.ext.xamldom
{
	/// <summary>
	/// Interface where errors during XAML loading are reported.
	/// </summary>
	public interface IXamlErrorSink
	{
		/// <summary>
		/// Reports a XAML load error.
		/// </summary>
		void ReportError(string message, int line, int column);
	}
}
