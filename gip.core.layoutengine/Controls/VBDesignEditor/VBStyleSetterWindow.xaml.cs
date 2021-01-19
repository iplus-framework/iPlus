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
using gip.ext.design;
using gip.ext.design.PropertyGrid;

namespace gip.core.layoutengine
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
            DesignItemProperty settersProp = styleProp.Value.Properties.GetProperty("Setters");
            SetterEditor.InitEditor(designObject, settersProp);
        }
    }
}
