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

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents an item of a <see cref="VBStatusBar"/> control.
    /// </summary>
    /// <summary>
    /// Stellt ein Element eines <see cref="VBStatusBar"/> Controls dar.
    /// </summary>
    public class VBStatusBarItem : StatusBarItem
    {
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "StatusBarItemStyleGip", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBStatusBarItem/Themes/StatusBarItemStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "StatusBarItemStyleAero", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBStatusBarItem/Themes/StatusBarItemStyleAero.xaml" },
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

        static VBStatusBarItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBStatusBarItem), new FrameworkPropertyMetadata(typeof(VBStatusBarItem)));
        }

        bool _themeApplied = false;
        public VBStatusBarItem()
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
