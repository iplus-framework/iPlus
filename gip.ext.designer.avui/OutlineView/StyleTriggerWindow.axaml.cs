// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using gip.ext.designer.avui.OutlineView;
using gip.ext.design.avui;
using gip.ext.design.avui.PropertyGrid;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace gip.ext.designer.avui.OutlineView
{
    public partial class StyleTriggerWindow : Window, ITypeEditorInitItem
	{
        public StyleTriggerWindow()
		{
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        DesignItem _DesignObject;
        public void LoadItemsCollection(DesignItem designObject)
		{
            if (designObject == null)
                return;
            _DesignObject = designObject;
            if ((designObject.View == null) || (designObject.Style == null))
                return;
            DesignItemProperty styleProp = designObject.Properties.GetProperty(Control.ThemeProperty);
            if (styleProp == null)
                return;
            DesignItemProperty triggersProp = styleProp.Value.Properties.GetProperty("Triggers");
            var triggerEditor = this.FindControl<TriggersCollectionEditor>("TriggerEditor");
            triggerEditor.InitEditor(designObject, triggersProp);
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            //_DesignObject.Services.Selection.SetSelectedComponents(new[] { _DesignObject });
            base.OnClosing(e);
        }
    }
}
