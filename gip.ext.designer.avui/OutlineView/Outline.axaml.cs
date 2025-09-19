// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using gip.ext.design.avui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Labs.Input;
using Avalonia.Markup.Xaml;

namespace gip.ext.designer.avui.OutlineView
{
	public partial class Outline : UserControl
	{
        public Outline()
		{
			InitializeComponent();

            this.AddCommandHandler(ApplicationCommands.Undo,
                () => ((DesignPanel)Root.DesignItem.Services.DesignPanel).DesignSurface.Undo(),
                () => Root == null ? false : ((DesignPanel)Root.DesignItem.Services.DesignPanel).DesignSurface.CanUndo());
            this.AddCommandHandler(ApplicationCommands.Redo,
                () => ((DesignPanel)Root.DesignItem.Services.DesignPanel).DesignSurface.Redo(),
                () => Root == null ? false : ((DesignPanel)Root.DesignItem.Services.DesignPanel).DesignSurface.CanRedo());
            this.AddCommandHandler(ApplicationCommands.Copy,
                () => ((DesignPanel)Root.DesignItem.Services.DesignPanel).DesignSurface.Copy(),
                () => Root == null ? false : ((DesignPanel)Root.DesignItem.Services.DesignPanel).DesignSurface.CanCopy());
            this.AddCommandHandler(ApplicationCommands.Cut,
                () => ((DesignPanel)Root.DesignItem.Services.DesignPanel).DesignSurface.Cut(),
                () => Root == null ? false : ((DesignPanel)Root.DesignItem.Services.DesignPanel).DesignSurface.CanCut());
            this.AddCommandHandler(ApplicationCommands.Delete,
                () => ((DesignPanel)Root.DesignItem.Services.DesignPanel).DesignSurface.Delete(),
                () => Root == null ? false : ((DesignPanel)Root.DesignItem.Services.DesignPanel).DesignSurface.CanDelete());
            this.AddCommandHandler(ApplicationCommands.Paste,
                () => ((DesignPanel)Root.DesignItem.Services.DesignPanel).DesignSurface.Paste(),
                () => Root == null ? false : ((DesignPanel)Root.DesignItem.Services.DesignPanel).DesignSurface.CanPaste());
            this.AddCommandHandler(ApplicationCommands.SelectAll,
                () => ((DesignPanel)Root.DesignItem.Services.DesignPanel).DesignSurface.SelectAll(),
                () => Root == null ? false : ((DesignPanel)Root.DesignItem.Services.DesignPanel).DesignSurface.CanSelectAll());

        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public static readonly StyledProperty<IOutlineNode> RootProperty =
			AvaloniaProperty.Register<Outline, IOutlineNode>(nameof(Root));

		public IOutlineNode Root {
			get { return GetValue(RootProperty); }
			set { SetValue(RootProperty, value); }
		}
		
		public object OutlineContent {
			get { return this; }
		}
	}
}
