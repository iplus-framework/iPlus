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
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using gip.core.datamodel;
using gip.core.layoutengine;
using gip.core.processapplication;
using gip.core.autocomponent;
using gip.core.layoutengine.Helperclasses;

namespace gip.core.visualcontrols
{
    [ACClassInfo(Const.PackName_VarioAutomation, "en{'VBMotor'}de{'VBMotor'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBMotor : VBPAControlBase
    {
        #region c'tors
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "MotorStyleGip", 
                                         styleUri = "/gip.core.visualcontrols;Component/Visualisierung/VBMotor/Themes/MotorStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "MotorStyleGip", 
                                         styleUri = "/gip.core.visualcontrols;Component/Visualisierung/VBMotor/Themes/MotorStyleGip.xaml" },
        };

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

        static VBMotor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBMotor), new FrameworkPropertyMetadata(typeof(VBMotor)));
        }

        bool _themeApplied = false;
        public VBMotor()
        {
        }

        protected override void OnInitialized(EventArgs e)
        {
            TileViewport = new Rect(0, 0, 0.2, 1);
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

        #region Dependency-Properties

        #region Appearance

        #region Motor-Type
        [ACClassInfo(Const.PackName_VarioSystem, "en{'MotorTypes'}de{'MotorTypes'}", Global.ACKinds.TACEnum)]
        public enum MotorTypes : short
        {
            Motor = 0,
            StarFeeder = 1,
            Screw = 2,
            Chain = 3,
            Conveyor = 4,
            ElevatorBody = 5,
            ElevTopLeft = 6,
            ElevTopRight = 7,
            ElevBottomLeft = 8,
            ElevBottomRight = 9,
            Pump = 10,
            PumpMembrane = 11,
            PumpGear = 12,
            Ventilator = 13,
            Sieve = 14,
            ScrewVert = 15,
            Filter = 16,
            Mixer = 17,
        }

        public static readonly DependencyProperty MotorTypeProperty
            = DependencyProperty.Register("MotorType", typeof(MotorTypes), typeof(VBMotor), new PropertyMetadata(MotorTypes.Motor));
        /// <summary>
        /// Ventil-Typ
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public MotorTypes MotorType
        {
            get { return (MotorTypes)GetValue(MotorTypeProperty); }
            set { SetValue(MotorTypeProperty, value); }
        }
        #endregion

        #region TileView
        public static readonly DependencyProperty TileViewportProperty
            = DependencyProperty.Register("TileViewport", typeof(Rect), typeof(VBMotor), new PropertyMetadata(new Rect(0, 0, 0.2, 1)));
        public Rect TileViewport
        {
            get { return (Rect)GetValue(TileViewportProperty); }
            set { SetValue(TileViewportProperty, value); }
        }

        public static readonly DependencyProperty TileViewportVertProperty
            = DependencyProperty.Register("TileViewportVert", typeof(Rect), typeof(VBMotor), new PropertyMetadata(new Rect(0, 0, 0.2, 1)));
        public Rect TileViewportVert
        {
            get { return (Rect)GetValue(TileViewportVertProperty); }
            set { SetValue(TileViewportVertProperty, value); }
        }
        #endregion

        #region Effects
        public static readonly DependencyProperty RotorProperty
            = DependencyProperty.Register("Rotor", typeof(Visibility), typeof(VBMotor), new PropertyMetadata(Visibility.Hidden));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public Visibility Rotor
        {
            get { return (Visibility)GetValue(RotorProperty); }
            set { SetValue(RotorProperty, value); }
        }
        #endregion // Effects

        #endregion

        #region Binding-Properties

        #region RunState
        public static readonly DependencyProperty RunStateProperty
            = DependencyProperty.Register("RunState", typeof(Boolean), typeof(VBMotor), new PropertyMetadata(false));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public Boolean RunState
        {
            get { return (Boolean)GetValue(RunStateProperty); }
            set { SetValue(RunStateProperty, value); }
        }

        #endregion

        #region ReqRunState
        public static readonly DependencyProperty ReqRunStateProperty
            = DependencyProperty.Register("ReqRunState", typeof(Boolean), typeof(VBMotor), new PropertyMetadata(false));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public Boolean ReqRunState
        {
            get { return (Boolean)GetValue(ReqRunStateProperty); }
            set { SetValue(ReqRunStateProperty, value); }
        }
        #endregion

        #region DirectionLeft
        public static readonly DependencyProperty DirectionLeftProperty
            = DependencyProperty.Register("DirectionLeft", typeof(Boolean), typeof(VBMotor));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public Boolean DirectionLeft
        {
            get { return (Boolean)GetValue(DirectionLeftProperty); }
            set { SetValue(DirectionLeftProperty, value); }
        }
        #endregion

        #region ReqDirectionLeft
        public static readonly DependencyProperty ReqDirectionLeftProperty
            = DependencyProperty.Register("ReqDirectionLeft", typeof(Boolean), typeof(VBMotor));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public Boolean ReqDirectionLeft
        {
            get { return (Boolean)GetValue(ReqDirectionLeftProperty); }
            set { SetValue(ReqDirectionLeftProperty, value); }
        }
        #endregion

        #region SpeedFast
        public static readonly DependencyProperty SpeedFastProperty
            = DependencyProperty.Register("SpeedFast", typeof(Boolean), typeof(VBMotor));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public Boolean SpeedFast
        {
            get { return (Boolean)GetValue(SpeedFastProperty); }
            set { SetValue(SpeedFastProperty, value); }
        }
        #endregion

        #endregion

        #endregion

        #region Methods

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            Size result = base.ArrangeOverride(arrangeBounds);

            double tileX = 0.2;
            if ((result.Width > 0) && (result.Height > 0))
            {
                double factor = result.Height / result.Width;
                if (factor < 1)
                    tileX = factor;
                else
                    tileX = 1;
            }

            if (tileX > 1)
                tileX = 1;
            else if (tileX < 0.01)
                tileX = 0.01;
            //tileX = 1 - tileX;
            TileViewport = new Rect(0, 0, tileX, 1);

            double tileY = 0.2;
            if ((result.Width > 0) && (result.Height > 0))
            {
                double factor = result.Width / result.Height;
                if (factor < 1)
                    tileY = factor;
                else
                    tileY = 1;
            }

            if (tileY > 1)
                tileY = 1;
            else if (tileY < 0.01)
                tileY = 0.01;

            TileViewportVert = new Rect(0, 0, tileY, 1);
            return result;
        }

        #endregion

    }
}
