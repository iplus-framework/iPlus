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
    /// Represents a control that lets the user select from a range of values by moving a <see cref="Thumb"/> control along a <see cref="Track"/>.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt ein Steuerelement dar, mit dem der Benutzer aus einem Wertebereich auswählen kann, indem er ein <see cref="Thumb"/> Steuerelement entlang eines <see cref="Track"/> bewegt.
    /// </summary>
    public class VBSlider : Slider
    {
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "SliderStyleGip", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBSlider/Themes/SliderStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "SliderStyleAero", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBSlider/Themes/SliderStyleAero.xaml" },
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

        static VBSlider()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBSlider), new FrameworkPropertyMetadata(typeof(VBSlider)));
        }

        bool _themeApplied = false;
        public VBSlider()
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
