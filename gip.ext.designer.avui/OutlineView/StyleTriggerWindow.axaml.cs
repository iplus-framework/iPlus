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
using Avalonia.Xaml.Interactivity;

namespace gip.ext.designer.avui.OutlineView
{
    public partial class StyleTriggerWindow : Window, ITypeEditorInitItem
	{
        public StyleTriggerWindow()
		{
            this.InitializeComponent();
        }

        DesignItem _DesignObject;
        public void LoadItemsCollection(DesignItem designObject)
		{
            if (designObject == null)
                return;
            _DesignObject = designObject;
            if (designObject.View == null)
                return;

            DesignItemProperty triggersProp = designObject.Properties.GetAttachedProperty(Interaction.BehaviorsProperty);

            if (triggersProp == null)
            {
                DesignItemProperty styleProp = designObject.Properties.GetProperty(Control.ThemeProperty);
                if (styleProp != null && styleProp.Value != null)
                {
                    triggersProp = styleProp.Value.Properties.GetProperty("Triggers");
                }
            }

            if (triggersProp == null)
                return;

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
