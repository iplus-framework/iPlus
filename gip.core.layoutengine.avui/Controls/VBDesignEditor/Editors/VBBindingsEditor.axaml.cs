// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using Avalonia.Data;
using gip.ext.design.avui.PropertyGrid;

namespace gip.core.layoutengine.avui.PropertyGrid.Editors
{
    /// <summary>
    /// Represents a editor for binding.
    /// </summary>
    [PropertyEditorAttribute(typeof(MultiBinding),"Bindings")]
	public partial class VBBindingsEditor : VBListBox
    {
		/// <summary>
		/// Creates a new TextBoxEditor instance.
		/// </summary>
		public VBBindingsEditor()
		{
			InitializeComponent();
		}

	}
}
