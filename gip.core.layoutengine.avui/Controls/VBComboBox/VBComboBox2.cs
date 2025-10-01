using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections;
using gip.core.layoutengine.avui.Helperclasses;
using gip.core.datamodel;
using Avalonia;
using Avalonia.Data;
using Avalonia.Controls;

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
        static VBComboBox2()
        {
            VBComboBox.WidthContentProperty.OverrideMetadata(typeof(VBComboBox2), new StyledPropertyMetadata<GridLength>(new GridLength(10, GridUnitType.Star)));
            // Set the default WidthContent to 10 Star instead of the base class default of 20 Star
            //WidthContent = new GridLength(10, GridUnitType.Star);
        }
        #endregion

        #region Additional Styled Properties
        /// <summary>
        /// Represents the styled property for VBContent2
        /// </summary>
        public static readonly StyledProperty<string> VBContent2Property
            = AvaloniaProperty.Register<VBComboBox2, string>(nameof(VBContent2));

        /// <summary>
        /// Gets or sets the VBContent2.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string VBContent2
        {
            get { return GetValue(VBContent2Property); }
            set { SetValue(VBContent2Property, value); }
        }

        /// <summary>
        /// Represents the styled property for ShowCaption2.
        /// </summary>
        public static readonly StyledProperty<bool> ShowCaption2Property
            = AvaloniaProperty.Register<VBComboBox2, bool>(nameof(ShowCaption2), true);
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

        /// <summary>
        /// Represents the styled property for Caption2.
        /// </summary>
        public static readonly StyledProperty<string> Caption2Property
            = AvaloniaProperty.Register<VBComboBox2, string>(nameof(Caption2));

        /// <summary>
        /// Gets or sets the Caption 2.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string Caption2
        {
            get { return GetValue(Caption2Property); }
            set { SetValue(Caption2Property, value); }
        }

        /// <summary>
        /// Represents the styled property for ACCaption2Trans.
        /// </summary>
        public static readonly StyledProperty<string> ACCaption2TransProperty
            = AvaloniaProperty.Register<VBComboBox2, string>(nameof(ACCaption2Trans));
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
            get { return GetValue(ACCaption2TransProperty); }
            set { SetValue(ACCaption2TransProperty, value); }
        }

        #region Layout

        /// <summary>
        /// Represents the styled property for WidthCaption2.
        /// </summary>
        public static readonly StyledProperty<GridLength> WidthCaption2Property
            = AvaloniaProperty.Register<VBComboBox2, GridLength>(nameof(WidthCaption2), new GridLength(10, GridUnitType.Star));
        /// <summary>
        /// Gets or sets the width of caption2.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public GridLength WidthCaption2
        {
            get { return GetValue(WidthCaption2Property); }
            set { SetValue(WidthCaption2Property, value); }
        }

        /// <summary>
        /// Represents the styled property for WidthCaption2Max.
        /// </summary>
        public static readonly StyledProperty<double> WidthCaption2MaxProperty
            = AvaloniaProperty.Register<VBComboBox2, double>(nameof(WidthCaption2Max), Double.PositiveInfinity);

        /// <summary>
        /// Gets or sets the WidthCaption2Max.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public double WidthCaption2Max
        {
            get { return GetValue(WidthCaption2MaxProperty); }
            set { SetValue(WidthCaption2MaxProperty, value); }
        }
        #endregion
        #endregion

        #region Property Change Handling
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            
            if (change.Property == Caption2Property)
            {
                OnACCaption2Changed(change.OldValue as string, change.NewValue as string);
            }
        }

        private void OnACCaption2Changed(string oldValue, string newValue)
        {
            if (ContextACObject != null)
            {
                if (!_Initialized)
                    return;

                ACCaption2Trans = this.Root().Environment.TranslateText(ContextACObject, Caption2);
            }
        }
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

                var binding2 = new Binding
                {
                    Source = dcSource2,
                    Path = dcPath2,
                    Mode = BindingMode.OneWay
                };
                // Note: In Avalonia, NotifyOnSourceUpdated and NotifyOnTargetUpdated are not available
                // These were WPF-specific properties for debugging bindings
                this.Bind(VBComboBox2.ACCaption2TransProperty, binding2);
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
                // In Avalonia, we use ClearValue instead of BindingOperations.ClearBinding
                this.ClearValue(VBComboBox2.ACCaption2TransProperty);
            }
            base.DeInitVBControl(bso);
        }

        #endregion
    }
}
