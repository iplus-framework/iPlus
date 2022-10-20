using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.ComponentModel;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Represents a iPlus button chrome.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt eine iPlus-Taste in Chrom dar.
    /// </summary>
    public class VBButtonChrome : Control
    {
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "ButtonChromeStyleGip", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBButtonChrome/Themes/ButtonChromeStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "ButtonChromeStyleAero", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBButtonChrome/Themes/ButtonChromeStyleAero.xaml" },
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

        #region c'tors

        static VBButtonChrome()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBButtonChrome), new FrameworkPropertyMetadata(typeof(VBButtonChrome)));
        }

        bool _themeApplied = false;
        /// <summary>
        /// Creates a new instance of VBButtonChrome.
        /// </summary>
        public VBButtonChrome()
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

        #endregion //Contsructors


        #region CornerRadius

        /// <summary>
        /// Represents the dependency property for CornerRadius.
        /// </summary>
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(VBButtonChrome), new UIPropertyMetadata(default(CornerRadius), new PropertyChangedCallback(OnCornerRadiusChanged)));

        /// <summary>
        /// Gets or sets the corner radius for button.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt den Eckradius der Schaltfläche.
        /// </summary>
        [Category("VBControl")]
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        private static void OnCornerRadiusChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            VBButtonChrome buttonChrome = o as VBButtonChrome;
            if (buttonChrome != null)
                buttonChrome.OnCornerRadiusChanged((CornerRadius)e.OldValue, (CornerRadius)e.NewValue);
        }

        /// <summary>
        /// Invokes on corner radius changed.
        /// </summary>
        /// <param name="oldValue">The old corner radius value.</param>
        /// <param name="newValue">The new corner raduis value.</param>
        protected virtual void OnCornerRadiusChanged(CornerRadius oldValue, CornerRadius newValue)
        {
            //we always want the InnerBorderRadius to be one less than the CornerRadius
            CornerRadius newInnerCornerRadius = new CornerRadius(Math.Max(0, newValue.TopLeft - 1),
                                                                 Math.Max(0, newValue.TopRight - 1),
                                                                 Math.Max(0, newValue.BottomRight - 1),
                                                                 Math.Max(0, newValue.BottomLeft - 1));

            InnerCornerRadius = newInnerCornerRadius;
        }

        #endregion //CornerRadius

        #region InnerCornerRadius

        /// <summary>
        /// Represents the dependency property for InnerCornerRadius.
        /// </summary>
        public static readonly DependencyProperty InnerCornerRadiusProperty = DependencyProperty.Register("InnerCornerRadius", typeof(CornerRadius), typeof(VBButtonChrome), new UIPropertyMetadata(default(CornerRadius), new PropertyChangedCallback(OnInnerCornerRadiusChanged)));

        /// <summary>
        /// Gets or sets the inner corner radius for button.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt den Inneneckradius der Schaltfläche.
        /// </summary>
        [Category("VBControl")]
        public CornerRadius InnerCornerRadius
        {
            get { return (CornerRadius)GetValue(InnerCornerRadiusProperty); }
            set { SetValue(InnerCornerRadiusProperty, value); }
        }

        private static void OnInnerCornerRadiusChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            VBButtonChrome buttonChrome = o as VBButtonChrome;
            if (buttonChrome != null)
                buttonChrome.OnInnerCornerRadiusChanged((CornerRadius)e.OldValue, (CornerRadius)e.NewValue);
        }

        /// <summary>
        /// Invokes on inner corner radius changed.
        /// </summary>
        /// <param name="oldValue">The old inner corner radius value.</param>
        /// <param name="newValue">The new inner corner radius value.</param>
        protected virtual void OnInnerCornerRadiusChanged(CornerRadius oldValue, CornerRadius newValue)
        {
            // TODO: Add your property changed side-effects. Descendants can override as well.
        }

        #endregion //InnerCornerRadius

        #region RenderChecked

        /// <summary>
        /// Represents the dependency property for RenderChecked.
        /// </summary>
        public static readonly DependencyProperty RenderCheckedProperty = DependencyProperty.Register("RenderChecked", typeof(bool), typeof(VBButtonChrome), new UIPropertyMetadata(false, OnRenderCheckedChanged));

        /// <summary>
        /// Determines is render checked or not.
        /// </summary>
        /// <summary>
        /// Ermittelt wird oder nicht.
        /// </summary>
        [Category("VBControl")]
        public bool RenderChecked
        {
            get { return (bool)GetValue(RenderCheckedProperty); }
            set { SetValue(RenderCheckedProperty, value); }
        }

        private static void OnRenderCheckedChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            VBButtonChrome buttonChrome = o as VBButtonChrome;
            if (buttonChrome != null)
                buttonChrome.OnRenderCheckedChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        /// <summary>
        /// Invokes on render checked changed.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnRenderCheckedChanged(bool oldValue, bool newValue)
        {
            // TODO: Add your property changed side-effects. Descendants can override as well.
        }

        #endregion //RenderChecked

        #region RenderEnabled

        /// <summary>
        /// Represents the dependency property for RenderEnabled.
        /// </summary>
        public static readonly DependencyProperty RenderEnabledProperty = DependencyProperty.Register("RenderEnabled", typeof(bool), typeof(VBButtonChrome), new UIPropertyMetadata(true, OnRenderEnabledChanged));

        /// <summary>
        /// Determines is render enabled or disabled.
        /// </summary>
        /// <sumamry xml:lang="de">
        /// Legt fest, ob das Rendern aktiviert oder deaktiviert ist.
        /// </sumamry>
        [Category("VBControl")]
        public bool RenderEnabled
        {
            get { return (bool)GetValue(RenderEnabledProperty); }
            set { SetValue(RenderEnabledProperty, value); }
        }

        private static void OnRenderEnabledChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            VBButtonChrome buttonChrome = o as VBButtonChrome;
            if (buttonChrome != null)
                buttonChrome.OnRenderEnabledChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        /// <summary>
        /// Invokes on render enabled changed.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnRenderEnabledChanged(bool oldValue, bool newValue)
        {
            // TODO: Add your property changed side-effects. Descendants can override as well.
        }

        #endregion //RenderEnabled

        #region RenderFocused

        /// <summary>
        /// Represents the dependency property for RenderFocused.
        /// </summary>
        public static readonly DependencyProperty RenderFocusedProperty = DependencyProperty.Register("RenderFocused", typeof(bool), typeof(VBButtonChrome), new UIPropertyMetadata(false, OnRenderFocusedChanged));

        /// <summary>
        /// Determines is render focused or not.
        /// </summary>
        /// <summary xml:lang="de">
        /// Bestimmt, ob das Rendering fokussiert ist oder nicht.
        /// </summary>
        [Category("VBControl")]
        public bool RenderFocused
        {
            get { return (bool)GetValue(RenderFocusedProperty); }
            set { SetValue(RenderFocusedProperty, value); }
        }

        private static void OnRenderFocusedChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            VBButtonChrome buttonChrome = o as VBButtonChrome;
            if (buttonChrome != null)
                buttonChrome.OnRenderFocusedChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        /// <summary>
        /// Invokes on render focused changed.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnRenderFocusedChanged(bool oldValue, bool newValue)
        {
            // TODO: Add your property changed side-effects. Descendants can override as well.
        }

        #endregion //RenderFocused

        #region RenderMouseOver

        /// <summary>
        /// Represents the dependency property for RenderMouseOver.
        /// </summary>
        public static readonly DependencyProperty RenderMouseOverProperty = DependencyProperty.Register("RenderMouseOver", typeof(bool), typeof(VBButtonChrome), new UIPropertyMetadata(false, OnRenderMouseOverChanged));

        /// <summary>
        /// Determines is render mouse over or not.
        /// </summary>
        /// <summary xml:lang="de">
        /// Legt fest, ob die Maus über das Bild bewegt wird oder nicht.
        /// </summary>
        [Category("VBControl")]
        public bool RenderMouseOver
        {
            get { return (bool)GetValue(RenderMouseOverProperty); }
            set { SetValue(RenderMouseOverProperty, value); }
        }

        private static void OnRenderMouseOverChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            VBButtonChrome buttonChrome = o as VBButtonChrome;
            if (buttonChrome != null)
                buttonChrome.OnRenderMouseOverChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        /// <summary>
        /// Invokes on render mouse over changed.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnRenderMouseOverChanged(bool oldValue, bool newValue)
        {
            // TODO: Add your property changed side-effects. Descendants can override as well.
        }

        #endregion //RenderMouseOver

        #region RenderNormal

        /// <summary>
        /// Represents the dependency property for RenderNormal.
        /// </summary>
        public static readonly DependencyProperty RenderNormalProperty = DependencyProperty.Register("RenderNormal", typeof(bool), typeof(VBButtonChrome), new UIPropertyMetadata(true, OnRenderNormalChanged));

        /// <summary>
        /// Determines is render normal or not.
        /// </summary>
        /// <summary xml:lang="de">
        /// Legt fest, ob die Darstellung normal ist oder nicht.
        /// </summary>
        [Category("VBControl")]
        public bool RenderNormal
        {
            get { return (bool)GetValue(RenderNormalProperty); }
            set { SetValue(RenderNormalProperty, value); }
        }

        private static void OnRenderNormalChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            VBButtonChrome buttonChrome = o as VBButtonChrome;
            if (buttonChrome != null)
                buttonChrome.OnRenderNormalChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        /// <summary>
        /// Invokes on render normal changed.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnRenderNormalChanged(bool oldValue, bool newValue)
        {
            // TODO: Add your property changed side-effects. Descendants can override as well.
        }

        #endregion //RenderNormal

        #region RenderPressed

        /// <summary>
        /// Represents the dependency property for RenderPressed.
        /// </summary>
        public static readonly DependencyProperty RenderPressedProperty = DependencyProperty.Register("RenderPressed", typeof(bool), typeof(VBButtonChrome), new UIPropertyMetadata(false, OnRenderPressedChanged));

        /// <summary>
        /// Determines is render pressed or not.
        /// </summary>
        /// <summary xml:lang="de">
        /// Bestimmt, ob der Renderer gedrückt ist oder nicht.
        /// </summary>
        [Category("VBControl")]
        public bool RenderPressed
        {
            get { return (bool)GetValue(RenderPressedProperty); }
            set { SetValue(RenderPressedProperty, value); }
        }

        private static void OnRenderPressedChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            VBButtonChrome buttonChrome = o as VBButtonChrome;
            if (buttonChrome != null)
                buttonChrome.OnRenderPressedChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        /// <summary>
        /// Invokes on render pressed changed.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnRenderPressedChanged(bool oldValue, bool newValue)
        {
            // TODO: Add your property changed side-effects. Descendants can override as well.
        }

        #endregion //RenderPressed

    }
}
