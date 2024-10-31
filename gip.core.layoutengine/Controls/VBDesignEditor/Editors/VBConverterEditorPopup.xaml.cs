// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace gip.core.layoutengine.PropertyGrid.Editors
{
    /// <summary>
    /// Represents a popup editor for converters.
    /// </summary>
	public partial class VBConverterEditorPopup
	{
		public VBConverterEditorPopup()
		{
			InitializeComponent();
		}

		protected override void OnClosed(EventArgs e)
		{
		    base.OnClosed(e);
		    //VBConverterEditorView.VBConverterEditor.Commit();
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.Key == Key.Escape) IsOpen = false;
		}
	}
}
