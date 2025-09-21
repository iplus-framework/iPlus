// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.ext.design.avui.PropertyGrid;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Markup.Xaml;

namespace gip.ext.designer.avui.PropertyGrid.Editors.BrushEditor
{
    [TypeEditor(typeof(Brush))]
    public partial class BrushTypeEditor : UserControl
    {
        static BrushEditorPopup brushEditorPopup = new BrushEditorPopup();

        public BrushTypeEditor()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
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
