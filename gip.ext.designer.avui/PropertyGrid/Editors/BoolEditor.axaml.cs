// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia.Markup.Xaml;
using gip.ext.design.avui.PropertyGrid;

namespace gip.ext.designer.avui.PropertyGrid.Editors
{
	[TypeEditor(typeof(bool))]
	public partial class BoolEditor
	{
		public BoolEditor()
		{
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
