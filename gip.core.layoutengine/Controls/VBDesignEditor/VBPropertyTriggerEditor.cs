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
using gip.core.layoutengine.Helperclasses;
using gip.ext.designer.OutlineView;
using gip.ext.designer;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Represents a editor for property triggers.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt einen Editor für Property-Trigger dar.
    /// </summary>
    public class VBPropertyTriggerEditor : PropertyTriggerEditor
    {
        #region c'tors
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "PropertyTriggerEditorStyleGip", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBDesignEditor/Themes/PropertyTriggerEditorStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "PropertyTriggerEditorStyleAero", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBDesignEditor/Themes/PropertyTriggerEditorStyleAero.xaml" },
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

        static VBPropertyTriggerEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBPropertyTriggerEditor), new FrameworkPropertyMetadata(typeof(VBPropertyTriggerEditor)));
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
    }
}
