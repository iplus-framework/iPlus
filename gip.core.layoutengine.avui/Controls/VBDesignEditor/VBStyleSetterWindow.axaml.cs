using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using gip.core.datamodel;
using gip.ext.design.avui;
using gip.ext.design.avui.PropertyGrid;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents a window for style setters.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt ein Fenster für Style Setter dar.
    /// </summary>
    public partial class VBStyleSetterWindow : VBDockingContainerToolWindow, ITypeEditorInitItem
    {
        public VBStyleSetterWindow()
        {
        }

        public VBStyleSetterWindow(VBDockingManager dockManager)
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
            DesignItemProperty styleProp = designObject.Properties.GetProperty(ThemeProperty);
            if (styleProp == null)
                return;
            DesignItemProperty settersProp = styleProp.Value.Properties.GetProperty("Setters");
            SetterEditor.InitEditor(designObject, settersProp);
        }
    }
}
