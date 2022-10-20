using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Represents a control that displays items and information in a horizontal bar in an application window.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt ein Steuerelement dar, das Elemente und Informationen in einer horizontalen Leiste in einem Anwendungsfenster anzeigt.
    /// </summary>
    public class VBStatusBar : StatusBar
    {
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "StatusBarStyleGip", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBStatusBar/Themes/StatusBarStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "StatusBarStyleAero", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBStatusBar/Themes/StatusBarStyleAero.xaml" },
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

        static VBStatusBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBStatusBar), new FrameworkPropertyMetadata(typeof(VBStatusBar)));
        }

        bool _themeApplied = false;
        public VBStatusBar()
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
