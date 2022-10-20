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
using gip.core.layoutengine.Helperclasses;
using gip.core.datamodel;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Control element for displaying texts and additional text information.
    /// </summary>
    /// <summary xml:lang="de">
    /// Steuerelement zur Darstellung von Texten und zusätzlicher Textinformation
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBTextBox2'}de{'VBTextBox2'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBTextBox2 : VBTextBox
    {
        #region c'tors
        private static List<CustomControlStyleInfo> _styleInfoList3 = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "TextBox2StyleGip", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBTextBox/Themes/TextBoxStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "TextBox2StyleAero", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBTextBox/Themes/TextBoxStyleAero.xaml" },
        };

        public override List<CustomControlStyleInfo> MyStyleInfoList
        {
            get
            {
                return _styleInfoList3;
            }
        }

        static VBTextBox2()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBTextBox2), new FrameworkPropertyMetadata(typeof(VBTextBox2)));
            VBTextBox.WidthContentProperty.OverrideMetadata(typeof(VBTextBox2), new PropertyMetadata(new GridLength(16, GridUnitType.Star)));
        }
        #endregion

        #region Additional Dependency Properties
        public static readonly DependencyProperty VBContent2Property
            = DependencyProperty.Register("VBContent2", typeof(string), typeof(VBTextBox2));
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string VBContent2
        {
            get { return (string)GetValue(VBContent2Property); }
            set { SetValue(VBContent2Property, value); }
        }

        public static readonly DependencyProperty ShowCaption2Property
            = DependencyProperty.Register("ShowCaption2", typeof(bool), typeof(VBTextBox2), new PropertyMetadata(true));
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

        public static readonly DependencyProperty Caption2Property
            = DependencyProperty.Register("Caption2", typeof(string), typeof(VBTextBox2), new PropertyMetadata(new PropertyChangedCallback(OnACCaption2Changed)));
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
            if (d is VBTextBox2)
            {
                VBTextBox2 control = d as VBTextBox2;
                if (control.ContextACObject != null)
                {
                    if (!control._Initialized)
                        return;
                    (control as VBTextBox2).ACCaption2Trans = control.Root().Environment.TranslateText(control.ContextACObject, control.Caption2);
                }
            }
        }

        public static readonly DependencyProperty ACCaption2TransProperty
            = DependencyProperty.Register("ACCaption2Trans", typeof(string), typeof(VBTextBox2));
        /// <summary>
        /// Gets or sets the ACCaption.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt die ACCaption.
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
        public static readonly DependencyProperty WidthCaption2Property
            = DependencyProperty.Register("WidthCaption2", typeof(GridLength), typeof(VBTextBox2), new PropertyMetadata(new GridLength(4, GridUnitType.Star)));
        /// <summary>
        /// Gets or sets the width of caption.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt die Breite der Beschriftung.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public GridLength WidthCaption2
        {
            get { return (GridLength)GetValue(WidthCaption2Property); }
            set { SetValue(WidthCaption2Property, value); }
        }

        public static readonly DependencyProperty WidthCaption2MaxProperty
            = DependencyProperty.Register("WidthCaption2Max", typeof(double), typeof(VBTextBox2), new PropertyMetadata(Double.PositiveInfinity));
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public double WidthCaption2Max
        {
            get { return (double)GetValue(WidthCaption2MaxProperty); }
            set { SetValue(WidthCaption2MaxProperty, value); }
        }

        public static readonly DependencyProperty TextAlignmentCaption2Property
            = DependencyProperty.Register("TextAlignmentCaption2", typeof(TextAlignment), typeof(VBTextBox2), new PropertyMetadata(TextAlignment.Left));
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public TextAlignment TextAlignmentCaption2
        {
            get { return (TextAlignment)GetValue(TextAlignmentCaption2Property); }
            set { SetValue(TextAlignmentCaption2Property, value); }
        }
        #endregion
        #endregion

        #region Loaded-Event
        protected bool _Loaded2 = false;
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
                    this.Root().Messages.LogDebug("Error00003", "VBTextBox2", VBContent2);
                    return;
                }

                Binding binding2 = new Binding();
                binding2.Source = dcSource2;
                binding2.Path = new PropertyPath(dcPath2);
                binding2.Mode = BindingMode.OneWay;
                binding2.NotifyOnSourceUpdated = true;
                binding2.NotifyOnTargetUpdated = true;
                SetBinding(VBTextBox2.ACCaption2TransProperty, binding2);
            }
            else if (!string.IsNullOrEmpty(Caption2))
                ACCaption2Trans = this.Root().Environment.TranslateText(ContextACObject, Caption2);
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
            {
                BindingOperations.ClearBinding(this, VBTextBox2.ACCaption2TransProperty);
            }
            base.DeInitVBControl(bso);
        }


        #endregion
    }
}
