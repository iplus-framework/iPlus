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
        /// <summary>
        /// Optional factory for creating the BrushEditorView. Override in gip.core.layoutengine.avui
        /// (or any consumer) to inject a custom subclass, e.g. one that uses VBTabControl:
        ///   BrushEditorPopup.ViewFactory = () => new VBBrushEditorView();
        /// </summary>
        public static Func<BrushEditorView> ViewFactory { get; set; }

        public BrushEditorView BrushEditorView { get; private set; }

		public BrushEditorPopup()
		{
            this.InitializeComponent();
            BrushEditorView = ViewFactory?.Invoke() ?? new BrushEditorView();
            this.Child = BrushEditorView;
            this.Closed += OnClosed;
            this.KeyDown += OnKeyDown;
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
