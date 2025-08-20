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
using System.Collections;
using gip.core.layoutengine.avui.Helperclasses;
using gip.core.datamodel;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Control for displaying a value with list selection and additional text information.
    /// </summary>
    /// <summary xml:lang="de">
    /// Steuerelement zur Darstellung von einem Wert mit Listenauswahl und zusätzlicher Textinformation.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBComboBox2'}de{'VBComboBox2'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBComboBox2 : VBComboBox
    {
        #region c'tors
        private static List<CustomControlStyleInfo> _styleInfoList3 = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "ComboBox2StyleGip", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBComboBox/Themes/ComboBoxStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "ComboBox2StyleAero", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBComboBox/Themes/ComboBoxStyleAero.xaml" },
        };

        /// <summary>
        /// Gets the list of custom styles.
        /// </summary>
        public override List<CustomControlStyleInfo> MyStyleInfoList
        {
            get
            {
                return _styleInfoList3;
            }
        }

        static VBComboBox2()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBComboBox2), new FrameworkPropertyMetadata(typeof(VBComboBox2)));
            VBComboBox.WidthContentProperty.OverrideMetadata(typeof(VBComboBox2), new PropertyMetadata(new GridLength(10, GridUnitType.Star)));
        }
        #endregion

        #region Additional Dependency Properties
        /// <summary>
        /// Represents the dependency property for VBContent2
        /// </summary>
        public static readonly DependencyProperty VBContent2Property
            = DependencyProperty.Register("VBContent2", typeof(string), typeof(VBComboBox2));

        /// <summary>
        /// Gets or sets the VBContent2.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string VBContent2
        {
            get { return (string)GetValue(VBContent2Property); }
            set { SetValue(VBContent2Property, value); }
        }

        /// <summary>
        /// Represents the dependency property for ShowCaption2.
        /// </summary>
        public static readonly DependencyProperty ShowCaption2Property
            = DependencyProperty.Register("ShowCaption2", typeof(bool), typeof(VBComboBox2), new PropertyMetadata(true));
        /// <summary>
        /// Determines is control caption shown or not.
        /// </summary>
        /// <summary xml:lang="de">
        /// Legt fest, ob die Steuertitel angezeigt werden oder nicht.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public bool ShowCaption2
        {
            get { return (bool)GetValue(ShowCaption2Property); }
            set { SetValue(ShowCaption2Property, value); }
        }

        /// <summary>
        /// Represents the dependency property for Caption2.
        /// </summary>
        public static readonly DependencyProperty Caption2Property
            = DependencyProperty.Register("Caption2", typeof(string), typeof(VBComboBox2), new PropertyMetadata(new PropertyChangedCallback(OnACCaption2Changed)));

        /// <summary>
        /// Gets or sets the Caption 2.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string Caption2
        {
            get { return (string)GetValue(Caption2Property); }
            set { SetValue(Caption2Property, value); }
        }

        private static void OnACCaption2Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is VBComboBox2)
            {
                VBComboBox2 control = d as VBComboBox2;
                if (control.ContextACObject != null)
                {
                    if (!control._Initialized)
                        return;

                    (control as VBComboBox2).ACCaption2Trans = control.Root().Environment.TranslateText(control.ContextACObject, control.Caption2);
                }
            }
        }

        /// <summary>
        /// Represents the dependency property for ACCaption2Trans.
        /// </summary>
        public static readonly DependencyProperty ACCaption2TransProperty
            = DependencyProperty.Register("ACCaption2Trans", typeof(string), typeof(VBComboBox2));
        /// <summary>
        /// Gets or sets the ACCaption2Trans.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt die ACCaption2Trans.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string ACCaption2Trans
        {
            get { return (string)GetValue(ACCaption2TransProperty); }
            set { SetValue(ACCaption2TransProperty, value); }
        }

        #region Layout

        /// <summary>
        /// Represents the dependency property for WidthCaption2.
        /// </summary>
        public static readonly DependencyProperty WidthCaption2Property
            = DependencyProperty.Register("WidthCaption2", typeof(GridLength), typeof(VBComboBox2), new PropertyMetadata(new GridLength(10, GridUnitType.Star)));
        /// <summary>
        /// Gets or sets the width of caption2.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public GridLength WidthCaption2
        {
            get { return (GridLength)GetValue(WidthCaption2Property); }
            set { SetValue(WidthCaption2Property, value); }
        }

        /// <summary>
        /// Represents the dependency property for WidthCaption2Max.
        /// </summary>
        public static readonly DependencyProperty WidthCaption2MaxProperty
            = DependencyProperty.Register("WidthCaption2Max", typeof(double), typeof(VBComboBox2), new PropertyMetadata(Double.PositiveInfinity));

        /// <summary>
        /// Gets or sets the WidthCaption2Max.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public double WidthCaption2Max
        {
            get { return (double)GetValue(WidthCaption2MaxProperty); }
            set { SetValue(WidthCaption2MaxProperty, value); }
        }
        #endregion
        #endregion

        #region Loaded-Event
        /// <summary>
        /// Determines is control loaded or not.
        /// </summary>
        protected bool _Loaded2 = false;
        /// <summary>
        /// Intializes VB control.
        /// </summary>
        protected override void InitVBControl()
        {
            base.InitVBControl();
            if (_Loaded2 || DataContext == null || ContextACObject == null)
                return;

            _Loaded2 = true;
            if (!string.IsNullOrEmpty(VBContent2))
            {
                IACType dcACTypeInfo2 = null;
                object dcSource2 = null;
                string dcPath2 = "";
                Global.ControlModes dcRightControlMode2 = Global.ControlModes.Hidden;
                if (!ContextACObject.ACUrlBinding(VBContent2, ref dcACTypeInfo2, ref dcSource2, ref dcPath2, ref dcRightControlMode2))
                {
                    this.Root().Messages.LogDebug("Error00003", "VBComboBox2", VBContent2);
                    return;
                }

                Binding binding2 = new Binding();
                binding2.Source = dcSource2;
                binding2.Path = new PropertyPath(dcPath2);
                binding2.Mode = BindingMode.OneWay;
                binding2.NotifyOnSourceUpdated = true;
                binding2.NotifyOnTargetUpdated = true;
                SetBinding(VBComboBox2.ACCaption2TransProperty, binding2);
            }
            else if (!string.IsNullOrEmpty(Caption2))
                ACCaptionTrans = this.Root().Environment.TranslateText(ContextACObject, ACCaption);
        }

        /// <summary>
        /// DeInitVBControl is used to remove all References which a WPF-Control refers to.
        /// It's needed that the Garbage-Collerctor can delete the object when it's removed from the Logical-Tree.
        /// Controls that implement this interface should bind itself to the InitState of the BSOACComponent.
        /// When the BSOACComponent stops and the property changes to Destructed- or DisposedToPool-State than this method should be called.
        /// </summary>
        /// <param name="bso">The bound BSOACComponent</param>
        public override void DeInitVBControl(IACComponent bso)
        {
            if (_Initialized)
                BindingOperations.ClearBinding(this, VBComboBox2.ACCaption2TransProperty);
            base.DeInitVBControl(bso);
        }

        #endregion
    }
}
