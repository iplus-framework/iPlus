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

namespace gip.core.layoutengine
{
    ///// Führen Sie die Schritte 1a oder 1b und anschließend Schritt 2 aus, um dieses benutzerdefinierte Steuerelement in einer XAML-Datei zu verwenden.
    /////
    ///// Schritt 1a) Verwenden des benutzerdefinierten Steuerelements in einer XAML-Datei, die im aktuellen Projekt vorhanden ist.
    ///// Fügen Sie dieses XmlNamespace-Attribut dem Stammelement der Markupdatei 
    ///// an der Stelle hinzu, an der es verwendet werden soll:
    /////
    /////     xmlns:MyNamespace="clr-namespace:gip.core.layoutengine.Controls.VBConnectionState"
    /////
    /////
    ///// Schritt 1b) Verwenden des benutzerdefinierten Steuerelements in einer XAML-Datei, die in einem anderen Projekt vorhanden ist.
    ///// Fügen Sie dieses XmlNamespace-Attribut dem Stammelement der Markupdatei 
    ///// an der Stelle hinzu, an der es verwendet werden soll:
    /////
    /////     xmlns:MyNamespace="clr-namespace:gip.core.layoutengine.Controls.VBConnectionState;assembly=gip.core.layoutengine.Controls.VBConnectionState"
    /////
    ///// Darüber hinaus müssen Sie von dem Projekt, das die XAML-Datei enthält, einen Projektverweis
    ///// zu diesem Projekt hinzufügen und das Projekt neu erstellen, um Kompilierungsfehler zu vermeiden:
    /////
    /////     Klicken Sie im Projektmappen-Explorer mit der rechten Maustaste auf das Zielprojekt und anschließend auf
    /////     "Verweis hinzufügen"->"Projekte"->[Navigieren Sie zu diesem Projekt, und wählen Sie es aus.]
    /////
    /////
    ///// Schritt 2)
    ///// Fahren Sie fort, und verwenden Sie das Steuerelement in der XAML-Datei.
    /////
    /////     <MyNamespace:VBConnectionState/>
    /////

    ///<summary>
    /// Represents a control for displaying client/server connection.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt ein Control zur Anzeige der Client/Server-Verbindung dar.
    /// </summary>
    public class VBConnectionState : Button
    {
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "ConnectionStateStyleGip", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBConnectionState/Themes/ConnectionStateStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "ConnectionStateStyleAero", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBConnectionState/Themes/ConnectionStateStyleAero.xaml" },
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

        static VBConnectionState()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBConnectionState), new FrameworkPropertyMetadata(typeof(VBConnectionState)));
        }

        bool _themeApplied = false;
        /// <summary>
        /// Creates a new instance of VBConnectionState.
        /// </summary>
        public VBConnectionState()
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

        /// <summary>
        /// Represents the dependency property for ConnectionQuality.
        /// </summary>
        public static readonly DependencyProperty ConnectionQualityProperty
            = DependencyProperty.Register("ConnectionQuality", typeof(ConnectionQuality), typeof(VBConnectionState));

        /// <summary>
        /// Gets or sets the quality of connection.
        /// </summary>
        public ConnectionQuality ConnectionQuality
        {
            get { return (ConnectionQuality)GetValue(ConnectionQualityProperty); }
            set { SetValue(ConnectionQualityProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ConnectionInfoText.
        /// </summary>
        public static readonly DependencyProperty ConnectionInfoTextProperty
            = DependencyProperty.Register("ConnectionInfoText", typeof(string), typeof(VBConnectionState));

        /// <summary>
        /// Gets or sets connection info text.
        /// </summary>
        public string ConnectionInfoText
        {
            get { return (string)GetValue(ConnectionInfoTextProperty); }
            set { SetValue(ConnectionInfoTextProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for IsServerConnection.
        /// </summary>
        public static readonly DependencyProperty IsServerConnectionProperty
            = DependencyProperty.Register("IsServerConnection", typeof(bool), typeof(VBConnectionState));

        /// <summary>
        /// Determines is server connection or not.
        /// </summary>
        public bool IsServerConnection
        {
            get { return (bool)GetValue(IsServerConnectionProperty); }
            set { SetValue(IsServerConnectionProperty, value); }
        }

    }
}
