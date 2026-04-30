// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.
using System;
using gip.ext.design.avui.PropertyGrid;
using Avalonia.Interactivity;
using gip.ext.designer.avui.PropertyGrid.Editors;

namespace gip.core.layoutengine.avui.PropertyGrid.Editors
{
    /// <summary>
    /// Represents a editor for boolean values.
    /// </summary>
	[TypeEditor(typeof(TimeSpan))]
	public partial class VBTimeSpanEditor : TimeSpanEditor
    {
		public VBTimeSpanEditor()
		{
			InitializeComponent();
		}

		protected override void OnLoaded(RoutedEventArgs e)
		{
			base.OnLoaded(e);
		}
	}
}
