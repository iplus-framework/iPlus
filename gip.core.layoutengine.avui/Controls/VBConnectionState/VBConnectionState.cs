using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia.Controls;
using Avalonia;
using gip.core.datamodel;

namespace gip.core.layoutengine.avui
{
    ///// Führen Sie die Schritte 1a oder 1b und anschließend Schritt 2 aus, um dieses benutzerdefinierte Steuerelement in einer XAML-Datei zu verwenden.
    /////
    ///// Schritt 1a) Verwenden des benutzerdefinierten Steuerelements in einer XAML-Datei, die im aktuellen Projekt vorhanden ist.
    ///// Fügen Sie dieses XmlNamespace-Attribut dem Stammelement der Markupdatei 
    ///// an der Stelle hinzu, an der es verwendet werden soll:
    /////
    /////     xmlns:MyNamespace="clr-namespace:gip.core.layoutengine.avui.Controls.VBConnectionState"
    /////
    /////
    ///// Schritt 1b) Verwenden des benutzerdefinierten Steuerelements in einer XAML-Datei, die in einem anderen Projekt vorhanden ist.
    ///// Fügen Sie dieses XmlNamespace-Attribut dem Stammelement der Markupdatei 
    ///// an der Stelle hinzu, an der es verwendet werden soll:
    /////
    /////     xmlns:MyNamespace="clr-namespace:gip.core.layoutengine.avui.Controls.VBConnectionState;assembly=gip.core.layoutengine.avui.Controls.VBConnectionState"
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
        static VBConnectionState()
        {
        }

        /// <summary>
        /// Creates a new instance of VBConnectionState.
        /// </summary>
        public VBConnectionState()
        {
        }

        /// <summary>
        /// Represents the dependency property for ConnectionQuality.
        /// </summary>
        public static readonly StyledProperty<ConnectionQuality> ConnectionQualityProperty
            = AvaloniaProperty.Register<VBConnectionState, ConnectionQuality>(nameof(ConnectionQuality));

        /// <summary>
        /// Gets or sets the quality of connection.
        /// </summary>
        public ConnectionQuality ConnectionQuality
        {
            get { return GetValue(ConnectionQualityProperty); }
            set { SetValue(ConnectionQualityProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ConnectionInfoText.
        /// </summary>
        public static readonly StyledProperty<string> ConnectionInfoTextProperty
            = AvaloniaProperty.Register<VBConnectionState, string>(nameof(ConnectionInfoText));

        /// <summary>
        /// Gets or sets connection info text.
        /// </summary>
        public string ConnectionInfoText
        {
            get { return GetValue(ConnectionInfoTextProperty); }
            set { SetValue(ConnectionInfoTextProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for IsServerConnection.
        /// </summary>
        public static readonly StyledProperty<bool> IsServerConnectionProperty
            = AvaloniaProperty.Register<VBConnectionState, bool>(nameof(IsServerConnection));

        /// <summary>
        /// Determines is server connection or not.
        /// </summary>
        public bool IsServerConnection
        {
            get { return GetValue(IsServerConnectionProperty); }
            set { SetValue(IsServerConnectionProperty, value); }
        }

    }
}
