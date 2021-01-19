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
using System.Windows.Media.Animation;
using gip.core.datamodel;
using System.ComponentModel;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Führen Sie die Schritte 1a oder 1b und anschließend Schritt 2 aus, um dieses benutzerdefinierte Steuerelement in einer XAML-Datei zu verwenden.
    ///
    /// Schritt 1a) Verwenden des benutzerdefinierten Steuerelements in einer XAML-Datei, die im aktuellen Projekt vorhanden ist.
    /// Fügen Sie dieses XmlNamespace-Attribut dem Stammelement der Markupdatei 
    /// an der Stelle hinzu, an der es verwendet werden soll:
    ///
    ///     xmlns:MyNamespace="clr-namespace:gip.core.layoutengine.Controls.VBControlSelectionIcon"
    ///
    ///
    /// Schritt 1b) Verwenden des benutzerdefinierten Steuerelements in einer XAML-Datei, die in einem anderen Projekt vorhanden ist.
    /// Fügen Sie dieses XmlNamespace-Attribut dem Stammelement der Markupdatei 
    /// an der Stelle hinzu, an der es verwendet werden soll:
    ///
    ///     xmlns:MyNamespace="clr-namespace:gip.core.layoutengine.Controls.VBControlSelectionIcon;assembly=gip.core.layoutengine.Controls.VBControlSelectionIcon"
    ///
    /// Darüber hinaus müssen Sie von dem Projekt, das die XAML-Datei enthält, einen Projektverweis
    /// zu diesem Projekt hinzufügen und das Projekt neu erstellen, um Kompilierungsfehler zu vermeiden:
    ///
    ///     Klicken Sie im Projektmappen-Explorer mit der rechten Maustaste auf das Zielprojekt und anschließend auf
    ///     "Verweis hinzufügen"->"Projekte"->[Navigieren Sie zu diesem Projekt, und wählen Sie es aus.]
    ///
    ///
    /// Schritt 2)
    /// Fahren Sie fort, und verwenden Sie das Steuerelement in der XAML-Datei.
    ///
    ///     MyNamespace:VBControlSelectionIcon
    ///
    /// </summary>
    [TemplatePart(Name = "PART_Lense_Fill", Type = typeof(FrameworkElement))]
    public class VBControlSelectionIcon : Button
    {
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "ControlSelectionStyleGip", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBControlSelectionIcon/Themes/VBControlSelectionIconStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "ControlSelectionStyleAero", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBControlSelectionIcon/Themes/VBControlSelectionIconStyleAero.xaml" },
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

        static VBControlSelectionIcon()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBControlSelectionIcon), new FrameworkPropertyMetadata(typeof(VBControlSelectionIcon)));
        }

        bool _themeApplied = false;
        public VBControlSelectionIcon()
        {
            _AnimationFreeze = new DoubleAnimation();
            _AnimationFreeze.From = 0;
            _AnimationFreeze.To = 1;
            _AnimationFreeze.RepeatBehavior = RepeatBehavior.Forever;
            _AnimationFreeze.AutoReverse = true;
            _AnimationFreeze.Duration = new Duration(new TimeSpan(0, 0, 2));
            //Click += new RoutedEventHandler(VBControlSelectionIcon_Click);
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
            object partObj = (object)GetTemplateChild("PART_Lense_Fill");
            if ((partObj != null) && (partObj is FrameworkElement))
            {
                _PART_Lense_Fill = ((FrameworkElement)partObj);
            }
        }

        /// <summary>
        /// Actualizes current theme.
        /// </summary>
        /// <param name="bInitializingCall">Determines is initializing call or not.</param>
        public void ActualizeTheme(bool bInitializingCall)
        {
            _themeApplied = ControlManager.RegisterImplicitStyle(this, StyleInfoList, bInitializingCall);
        }

        public static readonly DependencyProperty ControlSelectionActiveProperty = DependencyProperty.Register("ControlSelectionActive", typeof(bool), typeof(VBControlSelectionIcon));
        public bool ControlSelectionActive
        {
            get { return (bool)GetValue(ControlSelectionActiveProperty); }
            set { SetValue(ControlSelectionActiveProperty, value); }
        }


        public static readonly DependencyProperty IconTypeProperty = DependencyProperty.Register("IconType", typeof(short), typeof(VBControlSelectionIcon));
        [Category("VBControl")]
        public short IconType
        {
            get { return (short)GetValue(IconTypeProperty); }
            set { SetValue(IconTypeProperty, value); }
        }

        private FrameworkElement _PART_Lense_Fill;
        public FrameworkElement PART_Lense_Fill
        {
            get
            {
                return _PART_Lense_Fill;
            }
        }

        private DoubleAnimation _AnimationFreeze;

        void VBControlSelectionIcon_Click(object sender, RoutedEventArgs e)
        {
            SwitchControlSelectionState();
        }

        public void SwitchControlSelectionState()
        {
            ControlSelectionActive = !ControlSelectionActive;
            if (ControlSelectionActive)
            {
                _AnimationFreeze.RepeatBehavior = RepeatBehavior.Forever;
                PART_Lense_Fill.BeginAnimation(UIElement.OpacityProperty, _AnimationFreeze);
            }
            else
            {
                _AnimationFreeze.RepeatBehavior = new RepeatBehavior(0);
                PART_Lense_Fill.BeginAnimation(UIElement.OpacityProperty, _AnimationFreeze);
            }
        }
    }
}
