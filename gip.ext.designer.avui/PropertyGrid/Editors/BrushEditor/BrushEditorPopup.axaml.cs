// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Avalonia.Markup.Xaml;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Controls;

namespace gip.ext.designer.avui.PropertyGrid.Editors.BrushEditor
{
	public partial class BrushEditorPopup : Popup
    {
        private BrushEditorView _brushEditorView;

		public BrushEditorPopup()
		{
            this.InitializeComponent();
            this.Closed += OnClosed;
            this.KeyDown += OnKeyDown;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            _brushEditorView = this.FindControl<BrushEditorView>("BrushEditorView");
        }

        protected void OnClosed(object sender, EventArgs e)
        {
		    BrushEditorView?.BrushEditor?.Commit();
		}

		protected void OnKeyDown(object sender, Avalonia.Input.KeyEventArgs e)
        {
			if (e.Key == Key.Escape) 
                IsOpen = false;
		}
	}
}
