// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

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
using gip.ext.design.PropertyGrid;
using System.Windows.Controls.Primitives;

namespace gip.ext.designer.PropertyGrid.Editors.BrushEditor
{
    [TypeEditor(typeof(Brush))]
    public partial class BrushTypeEditor
    {
        public BrushTypeEditor()
        {
            InitializeComponent();
        }

        static BrushEditorPopup brushEditorPopup = new BrushEditorPopup();

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            brushEditorPopup.BrushEditorView.BrushEditor.Property = DataContext as IPropertyNode;
            brushEditorPopup.PlacementTarget = this;
            brushEditorPopup.IsOpen = true;
        }
    }
}
