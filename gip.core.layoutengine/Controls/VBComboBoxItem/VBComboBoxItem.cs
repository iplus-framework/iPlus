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
using System.ComponentModel;
using System.Windows.Markup;
using gip.core.layoutengine.Helperclasses;
using gip.core.datamodel;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Implements a selectable item inside a <see cref="VBComboBox"/>
    /// </summary>
    /// <summary xml:lang="de">
    /// Implementiert ein auswählbares Element innerhalb eines <see cref="VBComboBox"/>
    /// </summary>
    public class VBComboBoxItem : ComboBoxItem
    {
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "ComboBoxItemStyleGip", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBComboBoxItem/Themes/ComboBoxItemStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "ComboBoxItemStyleAero", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBComboBoxItem/Themes/ComboBoxItemStyleAero.xaml" },
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

        static VBComboBoxItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBComboBoxItem), new FrameworkPropertyMetadata(typeof(VBComboBoxItem)));
        }

        bool _themeApplied = false;
        /// <summary>
        /// Creates a new instance of VBComboBoxItem.
        /// </summary>
        public VBComboBoxItem()
        {
        }

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
            _themeApplied = ControlManager.RegisterImplicitStyle(this, StyleInfoList, bInitializingCall);
        }
    }
}
