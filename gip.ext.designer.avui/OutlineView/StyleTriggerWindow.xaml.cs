// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using gip.ext.designer.avui.OutlineView;
using gip.ext.design.avui;
using System.Windows.Markup;
using gip.ext.design.avui.PropertyGrid;

namespace gip.ext.designer.avui.OutlineView
{
    public partial class StyleTriggerWindow : Window, ITypeEditorInitItem
	{
        public StyleTriggerWindow()
		{
			InitializeComponent();
		}

        DesignItem _DesignObject;
        public void InitEditor(DesignItem designObject)
		{
            if (designObject == null)
                return;
            _DesignObject = designObject;
            if ((designObject.View == null) || (designObject.Style == null))
                return;
            DesignItemProperty styleProp = designObject.Properties.GetProperty(FrameworkElement.StyleProperty);
            if (styleProp == null)
                return;
            DesignItemProperty triggersProp = styleProp.Value.Properties.GetProperty("Triggers");
            TriggerEditor.InitEditor(designObject, triggersProp);
        }
		
		protected override void OnClosing(CancelEventArgs e)
		{
            //_DesignObject.Services.Selection.SetSelectedComponents(new[] { _DesignObject });
		}
	}
}
