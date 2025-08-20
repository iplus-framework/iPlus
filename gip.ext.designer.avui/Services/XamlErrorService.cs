// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.ext.xamldom.avui;
using System.Collections.ObjectModel;

namespace gip.ext.designer.avui.Services
{
	public class XamlErrorService : IXamlErrorSink
	{
		public XamlErrorService()
		{
			Errors = new ObservableCollection<XamlError>();
		}

		public ObservableCollection<XamlError> Errors { get; private set; }

		public void ReportError(string message, int line, int column)
		{
			Errors.Add(new XamlError() { Message = message, Line = line, Column = column });
		}
	}

	public class XamlError
	{
		public string Message { get; set; }
		public int Line { get; set; }
		public int Column { get; set; }
	}
}
