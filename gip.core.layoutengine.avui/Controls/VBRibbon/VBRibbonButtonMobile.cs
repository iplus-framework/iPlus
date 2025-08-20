using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace gip.core.layoutengine.avui
{

    /// <summary>
    /// Represents the ribbon button control for the Mobile application.
    /// </summary>
    /// <summary xml:lang="de">
    ///Repräsentiert die Ribbon-Button-Steuerung.
    /// </summary>
    public class VBRibbonButtonMobile : Button, IVBDynamicIcon
    {
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> {
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip,
                                         styleName = "RibbonButtonMobileStyleGip",
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBRibbon/Themes/RibbonButtonMobileStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero,
                                         styleName = "RibbonButtonMobileStyleAero",
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBRibbon/Themes/RibbonButtonMobileStyleAero.xaml" },
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

        static VBRibbonButtonMobile()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBRibbonButtonMobile), new FrameworkPropertyMetadata(typeof(VBRibbonButtonMobile)));
        }

        bool _themeApplied = false;
        public VBRibbonButtonMobile()
        {
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            if (!String.IsNullOrEmpty(IconName))
            {
                ResourceDictionary dict = new ResourceDictionary();
                dict.Source = new Uri("/gip.core.layoutengine.avui;Component/Controls/VBRibbon/Icons/" + IconName + ".xaml", UriKind.Relative);
                ContentControl contentControl = new ContentControl();
                contentControl.Style = (Style)dict["Icon" + IconName + "StyleGip"];
                this.Content = contentControl;
            }

            Visibility = Visibility.Visible;
            if (RightControlMode < Global.ControlModes.Disabled)
            {
                Visibility = Visibility.Collapsed;
            }
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

        [Category("VBControl")]
        public string IconName
        {
            get;
            set;
        }

        /// <summary>
        /// Represents the dependency property for ContentStroke.
        /// </summary>
        public static readonly DependencyProperty ContentStrokeProperty
            = DependencyProperty.Register("ContentStroke", typeof(Brush), typeof(VBRibbonButtonMobile));

        /// <summary>
        /// Gets or sets the stroke of content.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt den Strich des Inhalts.
        /// </summary>
        [Category("VBControl")]
        public Brush ContentStroke
        {
            get { return (Brush)GetValue(ContentStrokeProperty); }
            set { SetValue(ContentStrokeProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ContentFill.
        /// </summary>
        public static readonly DependencyProperty ContentFillProperty
            = DependencyProperty.Register("ContentFill", typeof(Brush), typeof(VBRibbonButtonMobile));

        /// <summary>
        /// Gets or sets the fill of content.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt die Füllung des Inhalts.
        /// </summary>
        [Category("VBControl")]
        public Brush ContentFill
        {
            get { return (Brush)GetValue(ContentFillProperty); }
            set { SetValue(ContentFillProperty, value); }
        }

        /// <summary>
        /// Gets or sets the right control mode.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt den richtigen Kontrollmodus.
        /// </summary>
        [Category("VBControl")]
        public Global.ControlModes RightControlMode
        {
            get;
            set;
        }

        /*
         * BorderStatic
         * BorderHover
         */

    }
}
