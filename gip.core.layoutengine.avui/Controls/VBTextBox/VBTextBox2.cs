using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections;
using gip.core.layoutengine.avui.Helperclasses;
using gip.core.datamodel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;

namespace gip.core.layoutengine.avui
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
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBTextBox/Themes/TextBoxStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "TextBox2StyleAero", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBTextBox/Themes/TextBoxStyleAero.xaml" },
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
            WidthContentProperty.OverrideDefaultValue<VBTextBox2>(new GridLength(16, GridUnitType.Star));
        }
        #endregion

        #region Additional Dependency Properties
        public static readonly StyledProperty<string> VBContent2Property =
            AvaloniaProperty.Register<VBTextBox2, string>(nameof(VBContent2));
        
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string VBContent2
        {
            get { return GetValue(VBContent2Property); }
            set { SetValue(VBContent2Property, value); }
        }

        public static readonly StyledProperty<bool> ShowCaption2Property =
            AvaloniaProperty.Register<VBTextBox2, bool>(nameof(ShowCaption2), true);
        
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
            get { return GetValue(ShowCaption2Property); }
            set { SetValue(ShowCaption2Property, value); }
        }

        public static readonly StyledProperty<string> Caption2Property =
            AvaloniaProperty.Register<VBTextBox2, string>(nameof(Caption2));
        
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string Caption2
        {
            get { return GetValue(Caption2Property); }
            set { SetValue(Caption2Property, value); }
        }

        public static readonly StyledProperty<string> ACCaption2TransProperty =
            AvaloniaProperty.Register<VBTextBox2, string>(nameof(ACCaption2Trans));
        
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
            get { return GetValue(ACCaption2TransProperty); }
            set { SetValue(ACCaption2TransProperty, value); }
        }

        #region Layout
        public static readonly StyledProperty<GridLength> WidthCaption2Property =
            AvaloniaProperty.Register<VBTextBox2, GridLength>(nameof(WidthCaption2), new GridLength(4, GridUnitType.Star));
        
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
            get { return GetValue(WidthCaption2Property); }
            set { SetValue(WidthCaption2Property, value); }
        }

        public static readonly StyledProperty<double> WidthCaption2MaxProperty =
            AvaloniaProperty.Register<VBTextBox2, double>(nameof(WidthCaption2Max), Double.PositiveInfinity);
        
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public double WidthCaption2Max
        {
            get { return GetValue(WidthCaption2MaxProperty); }
            set { SetValue(WidthCaption2MaxProperty, value); }
        }

        public static readonly StyledProperty<TextAlignment> TextAlignmentCaption2Property =
            AvaloniaProperty.Register<VBTextBox2, TextAlignment>(nameof(TextAlignmentCaption2), TextAlignment.Left);
        
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public TextAlignment TextAlignmentCaption2
        {
            get { return GetValue(TextAlignmentCaption2Property); }
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

                var binding2 = new Binding
                {
                    Source = dcSource2,
                    Path = dcPath2,
                    Mode = BindingMode.OneWay
                };
                this.Bind(VBTextBox2.ACCaption2TransProperty, binding2);
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
            base.DeInitVBControl(bso);
        }

        #endregion

        #region Property Changed Handling
        
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            
            if (change.Property == Caption2Property)
            {
                OnACCaption2Changed();
            }
        }

        private void OnACCaption2Changed()
        {
            if (ContextACObject != null)
            {
                if (!_Initialized)
                    return;
                ACCaption2Trans = this.Root().Environment.TranslateText(ContextACObject, Caption2);
            }
        }
        
        #endregion
    }
}
