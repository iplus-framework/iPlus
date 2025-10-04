// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace gip.core.layoutengine.avui.PropertyGrid.Editors
{
    /// <summary>
    /// Represents a popup editor for converters.
    /// </summary>
	public partial class VBConverterEditorPopup : Popup
    {
		public VBConverterEditorPopup()
		{
			InitializeComponent();
            KeyDown += VBBindingEditorPopup_KeyDown;
        }

        private void VBBindingEditorPopup_KeyDown(object sender, Avalonia.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                IsOpen = false;
        }
    }
}
