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
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using gip.core.datamodel;
using gip.ext.design.avui;
using gip.ext.design.avui.PropertyGrid;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents a window for trigger style.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt ein Fenster für den Triggerstil dar.
    /// </summary>
    public partial class VBStyleTriggerWindow : VBDockingContainerToolWindow, ITypeEditorInitItem
    {
        public VBStyleTriggerWindow()
        {
        }

        public VBStyleTriggerWindow(VBDockingManager dockManager)
            : base(dockManager)
		{
			InitializeComponent();
		}

        DesignItem _DesignObject;
        public void LoadItemsCollection(DesignItem designObject)
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
    }
}
