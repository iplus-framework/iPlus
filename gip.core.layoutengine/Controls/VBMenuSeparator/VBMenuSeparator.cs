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

namespace gip.core.layoutengine
{
    /// <summary>
    /// Represents the control that is used to separate items in <see cref="VBMenu"/>.
    /// </summary>
    public class VBMenuSeparator : Separator
    {
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "MenuSeparatorStyleGip", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBMenuSeparator/Themes/MenuSeparatorStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "MenuSeparatorStyleAero", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBMenuSeparator/Themes/MenuSeparatorStyleAero.xaml" },
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

        static VBMenuSeparator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBMenuSeparator), new FrameworkPropertyMetadata(typeof(VBMenuSeparator)));
        }

        bool _themeApplied = false;
        /// <summary>
        /// Creates a new instance of VBMenuSeparator.
        /// </summary>
        public VBMenuSeparator()
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
            // Warum auch immer der Style explizit gesetzt werden muss auch wenn der Style DefaultStyleKeyProperty.OverrideMetadata gesetzt wurde
            if (_themeApplied && (this.Style == null))
                ControlManager.OverrideStyleWithTheme(this, StyleInfoList);
        }
    }
}
