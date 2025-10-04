// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace gip.core.layoutengine.avui.PropertyGrid.Editors
{
    /// <summary>
    /// Represents a popup editor for tranformations.
    /// </summary>
	public partial class VBTransformEditorPopup : Popup
    {
		public VBTransformEditorPopup()
		{
			InitializeComponent();
            KeyDown += OnKeyDown;
        }
		protected virtual void OnKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape) 
				IsOpen = false;
		}
	}
}
