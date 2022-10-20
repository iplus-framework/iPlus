using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using gip.core.datamodel;
using System.Windows.Controls.Primitives;
using System.ComponentModel;

namespace gip.core.layoutengine
{
    /// <summary>
    /// The converter for scroll bar size.
    /// </summary>
    [ValueConversion(typeof(double), typeof(double))]
    public class VBScrollBarSizeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((values == null) /*|| (parameter == null)*/)
                return (double)2000;
            if ((values[0] == null) || (values[1] == null) || (values[2] == null) || (values[3] == null))
                return (double)2000;

            double viewportSize = (double)values[0];
            double maximum = (double)values[1];
            double minimum = (double)values[2];
            double trackLength = (double)values[3];
            double thumbSize = (viewportSize / (maximum - minimum + viewportSize)) * trackLength;
            return thumbSize;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    /// <summary>
    /// Control for scrollbars.
    /// </summary>
    /// <summary xml:lang="de">
    /// Steuerelement für Scrollbalken
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBScrollViewer'}de{'VBScrollViewer'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBScrollViewer : ScrollViewer
    {
        #region c'tors
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "VBScrollViewerStyleGip", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBScrollViewer/Themes/ScrollViewerStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "VBScrollViewerStyleAero", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBScrollViewer/Themes/ScrollViewerStyleAero.xaml" },
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

        /// <summary>
        /// Gets the list of custom styles.
        /// </summary>
        public virtual List<CustomControlStyleInfo> MyStyleInfoList
        {
            get
            {
                return _styleInfoList;
            }
        }

        static VBScrollViewer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBScrollViewer), new FrameworkPropertyMetadata(typeof(VBScrollViewer)));
        }

        bool _themeApplied = false;

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
            _themeApplied = ControlManager.RegisterImplicitStyle(this, MyStyleInfoList, bInitializingCall);
        }
        #endregion

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            return base.ArrangeOverride(arrangeSize);
        }

        private bool _IsEnabledScrollOnLeftCtrlKeyDown = true;
        [Category("VBControl")]
        public bool IsEnabledScrollOnLeftCtrlKeyDown
        {
            get
            {
                return _IsEnabledScrollOnLeftCtrlKeyDown;
            }
            set
            {
                _IsEnabledScrollOnLeftCtrlKeyDown = value;
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
        }

        /// <summary>
        /// Handles the OnMouseLeftButtonDown event.
        /// </summary>
        /// <param name="e">The MouseButtonEvent arguments.</param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            bool handled = e.Handled;
            base.OnMouseLeftButtonDown(e);
            // Bug in WPF!!!!
            e.Handled = handled;
        }

        /// <summary>
        /// Handles the OnMouseLeftButtonUp event.
        /// </summary>
        /// <param name="e">The MouseButtonEvent arguments.</param>
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
        }

        /// <summary>
        /// Handles the OnMouseRightButtonDown event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);
        }

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonUp(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (!IsEnabledScrollOnLeftCtrlKeyDown && Keyboard.IsKeyDown(Key.LeftCtrl))
                return;
            base.OnMouseWheel(e);
        }
    }


    /// <summary>
    /// Steuerelement für Scrollbalken
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBScrollBar'}de{'VBScrollBar'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBScrollBar : ScrollBar
    {
        #region c'tors
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "VBScrollBarStyleGip", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBScrollViewer/Themes/ScrollViewerStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "VBScrollBarStyleAero", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBScrollViewer/Themes/ScrollViewerStyleAero.xaml" },
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

        public virtual List<CustomControlStyleInfo> MyStyleInfoList
        {
            get
            {
                return _styleInfoList;
            }
        }

        static VBScrollBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBScrollBar), new FrameworkPropertyMetadata(typeof(VBScrollBar)));
        }

        bool _themeApplied = false;

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ActualizeTheme(true);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (!_themeApplied)
                ActualizeTheme(false);
        }

        public void ActualizeTheme(bool bInitializingCall)
        {
            _themeApplied = ControlManager.RegisterImplicitStyle(this, MyStyleInfoList, bInitializingCall);
        }
        #endregion

    }

}
