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

        public VBStyleTriggerWindow(VBDockingManagerOldWPF dockManager)
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
            DesignItemProperty triggersProp = styleProp.Value.Properties.GetProperty("Triggers");
            TriggerEditor.InitEditor(designObject, triggersProp);
        }

        protected override VBDockPanel RootPanel { get => _DesignObject?.Component as VBDockPanel; }

    }
}
