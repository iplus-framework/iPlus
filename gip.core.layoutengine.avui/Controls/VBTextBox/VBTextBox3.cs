// ***********************************************************************
// Assembly         : gip.core.layoutengine.avui
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 04-10-2013
// ***********************************************************************
// <copyright file="VBTextBox3.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
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
    /// Steuerelement zur Darstellung von Texten und zusätzlicher Textinformation.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBTextBox3'}de{'VBTextBox3'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBTextBox3 : VBTextBox
    {
        #region c'tors
        /// <summary>
        /// Initializes static members of the <see cref="VBTextBox3"/> class.
        /// </summary>
        static VBTextBox3()
        {
            WidthContentProperty.OverrideDefaultValue<VBTextBox3>(new GridLength(16, GridUnitType.Star));
        }
        #endregion

        #region Additional Styled Properties
        /// <summary>
        /// The VB content2 property
        /// </summary>
        public static readonly StyledProperty<string> VBContent2Property =
            AvaloniaProperty.Register<VBTextBox3, string>(nameof(VBContent2));
        
        /// <summary>
        /// Gets or sets the VB content2.
        /// </summary>
        /// <value>The VB content2.</value>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string VBContent2
        {
            get { return GetValue(VBContent2Property); }
            set { SetValue(VBContent2Property, value); }
        }

        /// <summary>
        /// The VB source2 property
        /// </summary>
        public static readonly StyledProperty<string> VBSource2Property =
            AvaloniaProperty.Register<VBTextBox3, string>(nameof(VBSource2));
        
        /// <summary>
        /// Gets or sets the VB source2.
        /// </summary>
        /// <value>The VB source2.</value>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string VBSource2
        {
            get { return GetValue(VBSource2Property); }
            set { SetValue(VBSource2Property, value); }
        }

        #region Layout
        /// <summary>
        /// The width caption2 property
        /// </summary>
        public static readonly StyledProperty<GridLength> WidthCaption2Property =
            AvaloniaProperty.Register<VBTextBox3, GridLength>(nameof(WidthCaption2), new GridLength(4, GridUnitType.Star));
        
        /// <summary>
        /// Gets or sets the width caption2.
        /// </summary>
        /// <value>The width caption2.</value>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public GridLength WidthCaption2
        {
            get { return GetValue(WidthCaption2Property); }
            set { SetValue(WidthCaption2Property, value); }
        }

        /// <summary>
        /// The width caption2 max property
        /// </summary>
        public static readonly StyledProperty<double> WidthCaption2MaxProperty =
            AvaloniaProperty.Register<VBTextBox3, double>(nameof(WidthCaption2Max), Double.PositiveInfinity);
        
        /// <summary>
        /// Gets or sets the width caption2 max.
        /// </summary>
        /// <value>The width caption2 max.</value>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public double WidthCaption2Max
        {
            get { return GetValue(WidthCaption2MaxProperty); }
            set { SetValue(WidthCaption2MaxProperty, value); }
        }

        /// <summary>
        /// The text alignment caption2 property
        /// </summary>
        public static readonly StyledProperty<TextAlignment> TextAlignmentCaption2Property =
            AvaloniaProperty.Register<VBTextBox3, TextAlignment>(nameof(TextAlignmentCaption2), TextAlignment.Left);
        
        /// <summary>
        /// Gets or sets the text alignment caption2.
        /// </summary>
        /// <value>The text alignment caption2.</value>
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
        /// <summary>
        /// The _ loaded2
        /// </summary>
        protected bool _Loaded2 = false;
        
        /// <summary>
        /// Inits the VB control.
        /// </summary>
        protected override void InitVBControl()
        {
            base.InitVBControl();
            if (_Loaded2 || DataContext == null || ContextACObject == null)
                return;

            _Loaded2 = true;
            //if (!string.IsNullOrEmpty(VBContent2))
            //{
            //    IACType dcACTypeInfo2 = null;
            //    object dcSource2 = null;
            //    string dcPath2 = "";
            //    Global.ControlModes dcRightControlMode2 = Global.ControlModes.Hidden;
            //    if (!ContextACObject.ACUrlBinding(VBContent2, ref dcACTypeInfo2, ref dcSource2, ref dcPath2, ref dcRightControlMode2))
            //    {
            //        this.Root().Messages.LogDebug("Error00003", "VBTextBox3", VBContent2);
            //        return;
            //    }

            //    var binding2 = new Binding
            //    {
            //        Source = dcSource2,
            //        Path = dcPath2,
            //        Mode = BindingMode.OneWay
            //    };
            //    this.Bind(VBTextBox3.ACCaption2TransProperty, binding2);
            //}
            //else if (!string.IsNullOrEmpty(Caption2))
            //    ACCaption2Trans = this.Root().Environment.TranslateText(ContextACObject, Caption2);
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
    }
}
