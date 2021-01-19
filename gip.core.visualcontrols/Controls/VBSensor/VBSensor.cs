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

namespace gip.core.visualcontrols
{
    [ACClassInfo(Const.PackName_VarioAutomation, "en{'VBSensor'}de{'VBSensor'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBSensor : VBPAControlBase
    {
        #region c'tors
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "SensorStyleGip", 
                                         styleUri = "/gip.core.visualcontrols;Component/Visualisierung/VBSensor/Themes/SensorStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "SensorStyleGip", 
                                         styleUri = "/gip.core.visualcontrols;Component/Visualisierung/VBSensor/Themes/SensorStyleGip.xaml" },
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

        static VBSensor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBSensor), new FrameworkPropertyMetadata(typeof(VBSensor)));
        }

        bool _themeApplied = false;
        public VBSensor()
        {
        }

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

        #region Dependency-Properties

        #region Appearance

        #region Sensor-Type
        [ACClassInfo(Const.PackName_VarioSystem, "en{'SensorTypes'}de{'SensorTypes'}", Global.ACKinds.TACEnum)]
        public enum SensorTypes : short
        {
            Rectangle = 0,
            Circle = 1,
        }

        public static readonly DependencyProperty SensorTypeProperty
            = DependencyProperty.Register("SensorType", typeof(SensorTypes), typeof(VBSensor), new PropertyMetadata(SensorTypes.Rectangle));
        /// <summary>
        /// Ventil-Typ
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public SensorTypes SensorType
        {
            get { return (SensorTypes)GetValue(SensorTypeProperty); }
            set { SetValue(SensorTypeProperty, value); }
        }
        #endregion

        #region Sensor-Role
        public static readonly DependencyProperty SensorRoleProperty
            = DependencyProperty.Register("SensorRole", typeof(PAESensorRole), typeof(VBSensor), new PropertyMetadata(PAESensorRole.None));
        /// <summary>
        /// Ventil-Typ
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public PAESensorRole SensorRole
        {
            get { return (PAESensorRole)GetValue(SensorRoleProperty); }
            set { SetValue(SensorRoleProperty, value); }
        }
        #endregion

        #endregion

        #endregion

    }
}
