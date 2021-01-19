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
using System.ComponentModel;
using System.Windows.Markup;
using gip.core.layoutengine.Helperclasses;
using gip.core.datamodel;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Represents a button that can be selected with click on it, but deselected only by clicked on a another <see cref="VBRadioButton"/>.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt eine Schaltfläche dar, die durch Anklicken ausgewählt werden kann, aber nur durch Anklicken eines anderen <see cref="VBRadioButton"/> deaktiviert wird.
    /// </summary>
    public class VBRadioButton : RadioButton, IVBDynamicIcon
    {
        #region c'tors

        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> {
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip,
                                         styleName = "RadioButtonStyleGip",
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBRadioButton/Themes/RadioButtonStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero,
                                         styleName = "RadioButtonStyleAero",
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBRadioButton/Themes/RadioButtonStyleAero.xaml" },
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

        static VBRadioButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBRadioButton), new FrameworkPropertyMetadata(typeof(VBRadioButton)));
        }

        bool _themeApplied = false;
        public VBRadioButton()
        {
        }

        #endregion

        #region Init

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
            InitVBControl();
        }

        bool _isInitialized = false;

        private void InitVBControl()
        {
            if (_isInitialized)
                return;

            if (!string.IsNullOrEmpty(ACCaption))
                this.Content = Translator.GetTranslation(null, ACCaption, this.Root().Environment.VBLanguageCode);

            _isInitialized = true;
        }

        /// <summary>
        /// Actualizes current theme.
        /// </summary>
        /// <param name="bInitializingCall">Determines is initializing call or not.</param>
        public void ActualizeTheme(bool bInitializingCall)
        {
            _themeApplied = ControlManager.RegisterImplicitStyle(this, StyleInfoList, bInitializingCall);
        }

        #endregion

        #region Dependency properties

        /// <summary>
        /// Represents the dependency property for ContentStroke.
        /// </summary>
        public static readonly DependencyProperty ContentStrokeProperty
            = DependencyProperty.Register("ContentStroke", typeof(Brush), typeof(VBRadioButton));

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
            = DependencyProperty.Register("ContentFill", typeof(Brush), typeof(VBRadioButton));

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

        public static readonly DependencyProperty PushButtonStyleProperty
            = DependencyProperty.Register("PushButtonStyle", typeof(Boolean), typeof(VBRadioButton));
        [Category("VBControl")]
        public Boolean PushButtonStyle
        {
            get { return (Boolean)GetValue(PushButtonStyleProperty); }
            set { SetValue(PushButtonStyleProperty, value); }
        }

        public static readonly DependencyProperty IsMouseOverParentProperty
            = DependencyProperty.Register("IsMouseOverParent", typeof(Boolean), typeof(VBRadioButton));
        public Boolean IsMouseOverParent
        {
            get { return (Boolean)GetValue(IsMouseOverParentProperty); }
            set { SetValue(IsMouseOverParentProperty, value); }
        }

        #endregion

        private string _Caption;
        /// <summary>
        /// Gets or sets the ACCaption.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt die ACCaption.
        /// </summary>
        [Category("VBControl")]
        public string ACCaption
        {
            get
            {
                return _Caption;
            }
            set
            {
                _Caption = value;
            }
        }
    }
}
