// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using gip.ext.design.avui;
using gip.ext.designer.avui.themes;

namespace gip.ext.designer.avui.Extensions
{
	public partial class DefaultCommandsContextMenu : ContextMenu
	{
		private DesignItem designItem;

		public DefaultCommandsContextMenu()
		{
            this.InitializeComponent();
        }

		public DefaultCommandsContextMenu(DesignItem designItem)
		{
			this.designItem = designItem;

            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
