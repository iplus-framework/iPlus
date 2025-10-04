// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using gip.ext.design.avui.PropertyGrid;

namespace gip.core.layoutengine.avui.PropertyGrid.Editors
{
    /// <summary>
    /// Represents a editor for boolean values.
    /// </summary>
	[TypeEditor(typeof(bool))]
	public partial class VBBoolEditor : VBCheckBox
    {
		public VBBoolEditor()
		{
			InitializeComponent();
		}
	}
}
