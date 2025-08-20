using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace gip.core.layoutengine.avui
{
    public class VBRibbonBarMobile : ToolBar
    {
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> {
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip,
                                         styleName = "RibbonBarMobileStyleGip",
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBRibbon/Themes/RibbonBarMobileStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero,
                                         styleName = "RibbonBarMobileStyleAero",
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBRibbon/Themes/RibbonBarMobileStyleAero.xaml" },
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

        static VBRibbonBarMobile()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBRibbonBarMobile), new FrameworkPropertyMetadata(typeof(VBRibbonBarMobile)));
        }

        bool _themeApplied = false;
        public VBRibbonBarMobile()
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
