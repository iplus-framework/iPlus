// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.ext.design.avui.PropertyGrid;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;

namespace gip.ext.designer.avui.PropertyGrid.Editors.BrushEditor
{
    [TypeEditor(typeof(IBrush))]
    public class BrushTypeEditor : ContentControl
    {
        // Instance field (not static) so the popup can be added to this control's
        // logical children — required for style resolution to reach Application.Styles
        // and find the FluentTheme ControlThemes (TabControl, etc.) in the PopupRoot.
        private BrushEditorPopup _brushEditorPopup;

        public BrushTypeEditor()
        {
        }

        private BrushEditorPopup EnsurePopup()
        {
            if (_brushEditorPopup == null)
            {
                _brushEditorPopup = new BrushEditorPopup();
                // Adding to LogicalChildren sets this control as the popup's
                // InheritanceParent, so PopupRoot.StylingParent -> Popup ->
                // BrushTypeEditor -> ... -> Window -> Application.Styles.
                LogicalChildren.Add(_brushEditorPopup);
            }
            return _brushEditorPopup;
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            var brushEditorPopup = EnsurePopup();
            var brushEditorView = brushEditorPopup.BrushEditorView;
            if (brushEditorView?.BrushEditor != null)
            {
                brushEditorView.BrushEditor.Property = DataContext as IPropertyNode;
            }
            brushEditorPopup.PlacementTarget = this;
            brushEditorPopup.IsOpen = true;
        }
    }
}
