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
using System.Windows.Navigation;
using System.Windows.Shapes;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using gip.ext.designer.avui.OutlineView;
using gip.ext.designer.avui;
using gip.ext.design.avui;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents the editor for triggers collection.
    /// </summary>
    /// <summary xml:lang="de">
    /// Repräsentiert den Editor für die Trigger-Sammlung.
    /// </summary>
    public class VBTriggersCollectionEditor : TriggersCollectionEditor
    {
        #region c'tors
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "TriggersCollectionEditorStyleGip", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBDesignEditor/Themes/TriggersCollectionEditorStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "TriggersCollectionEditorStyleAero", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBDesignEditor/Themes/TriggersCollectionEditorStyleAero.xaml" },
        };

        /// <summary>
        /// Gets the list of custom styles.
        /// </summary>
        public static List<CustomControlStyleInfo> StyleInfoList
        {
            get
            {
                return _styleInfoList;
            }
        }

        /// <summary>
        /// Gets the list of custom styles.
        /// </summary>
        public virtual List<CustomControlStyleInfo> MyStyleInfoList
        {
            get
            {
                return _styleInfoList;
            }
        }

        static VBTriggersCollectionEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBTriggersCollectionEditor), new FrameworkPropertyMetadata(typeof(VBTriggersCollectionEditor)));
        }

        bool _themeApplied = false;

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ActualizeTheme(true);
        }

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (!_themeApplied)
                ActualizeTheme(false);
        }

        /// <summary>
        /// Actualizes current theme.
        /// </summary>
        /// <param name="bInitializingCall">Determines is initializing call or not.</param>
        public void ActualizeTheme(bool bInitializingCall)
        {
            _themeApplied = ControlManager.RegisterImplicitStyle(this, MyStyleInfoList, bInitializingCall);
        }
        #endregion

        protected override TriggerOutlineNodeBase CreateOutlineNode(DesignItem child, DesignItem designObject)
        {
            if (typeof(MultiDataTrigger).IsAssignableFrom(child.ComponentType))
                return new VBMultiDataTriggerOutlineNode(child, designObject);
            else if (typeof(MultiTrigger).IsAssignableFrom(child.ComponentType))
                return new VBMultiTriggerOutlineNode(child, designObject);
            else if (typeof(DataTrigger).IsAssignableFrom(child.ComponentType))
                return new VBDataTriggerOutlineNode(child, designObject);
            else if (typeof(EventTrigger).IsAssignableFrom(child.ComponentType))
                return new VBEventTriggerOutlineNode(child, designObject);
            else if (typeof(Trigger).IsAssignableFrom(child.ComponentType))
                return new VBPropertyTriggerOutlineNode(child, designObject);
            return null;
        }

    }
}
