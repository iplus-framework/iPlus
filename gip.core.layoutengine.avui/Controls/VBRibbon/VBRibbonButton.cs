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
    // Führen Sie die Schritte 1a oder 1b und anschließend Schritt 2 aus, um dieses benutzerdefinierte Steuerelement in einer XAML-Datei zu verwenden.
    //
    // Schritt 1a) Verwenden des benutzerdefinierten Steuerelements in einer XAML-Datei, die im aktuellen Projekt vorhanden ist.
    // Fügen Sie dieses XmlNamespace-Attribut dem Stammelement der Markupdatei 
    // an der Stelle hinzu, an der es verwendet werden soll:
    //
    //     xmlns:MyNamespace="clr-namespace:gip.core.layoutengine.avui.Controls.VBRibbonButton"
    //
    //
    // Schritt 1b) Verwenden des benutzerdefinierten Steuerelements in einer XAML-Datei, die in einem anderen Projekt vorhanden ist.
    // Fügen Sie dieses XmlNamespace-Attribut dem Stammelement der Markupdatei 
    // an der Stelle hinzu, an der es verwendet werden soll:
    //
    //     xmlns:MyNamespace="clr-namespace:gip.core.layoutengine.avui.Controls.VBRibbonButton;assembly=gip.core.layoutengine.avui.Controls.VBRibbonButton"
    //
    // Darüber hinaus müssen Sie von dem Projekt, das die XAML-Datei enthält, einen Projektverweis
    // zu diesem Projekt hinzufügen und das Projekt neu erstellen, um Kompilierungsfehler zu vermeiden:
    //
    //     Klicken Sie im Projektmappen-Explorer mit der rechten Maustaste auf das Zielprojekt und anschließend auf
    //     "Verweis hinzufügen"->"Projekte"->[Navigieren Sie zu diesem Projekt, und wählen Sie es aus.]
    //
    //
    // Schritt 2)
    // Fahren Sie fort, und verwenden Sie das Steuerelement in der XAML-Datei.
    //
    //     <MyNamespace:VBRibbonButton/>
    //

    /// <summary>
    /// Represents the ribbon button control.
    /// </summary>
    /// <summary xml:lang="de">
    ///Repräsentiert die Ribbon-Button-Steuerung.
    /// </summary>
    public class VBRibbonButton : Button, IVBDynamicIcon
    {
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "RibbonButtonStyleGip", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBRibbon/Themes/RibbonButtonStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "RibbonButtonStyleAero", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBRibbon/Themes/RibbonButtonStyleAero.xaml" },
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

        static VBRibbonButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBRibbonButton), new FrameworkPropertyMetadata(typeof(VBRibbonButton)));
        }

        bool _themeApplied = false;
        public VBRibbonButton()
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
            = DependencyProperty.Register("ContentStroke", typeof(Brush), typeof(VBRibbonButton));

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
            = DependencyProperty.Register("ContentFill", typeof(Brush), typeof(VBRibbonButton));

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
