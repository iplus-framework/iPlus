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
        /// The _style info list3
        /// </summary>
        private static List<CustomControlStyleInfo> _styleInfoList3 = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "TextBox3StyleGip", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBTextBox/Themes/TextBoxStyleGip.xaml" },
            //new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
            //                             styleName = "TextBox2StyleAero", 
            //                             styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBTextBox/Themes/TextBoxStyleAero.xaml" },
        };

        /// <summary>
        /// Gets my style info list.
        /// </summary>
        /// <value>My style info list.</value>
        public override List<CustomControlStyleInfo> MyStyleInfoList
        {
            get
            {
                return _styleInfoList3;
            }
        }

        /// <summary>
        /// Initializes static members of the <see cref="VBTextBox3"/> class.
        /// </summary>
        static VBTextBox3()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBTextBox3), new FrameworkPropertyMetadata(typeof(VBTextBox3)));
            VBTextBox.WidthContentProperty.OverrideMetadata(typeof(VBTextBox3), new PropertyMetadata(new GridLength(16, GridUnitType.Star)));
        }
        #endregion

        #region Additional Dependency Properties
        /// <summary>
        /// The VB content2 property
        /// </summary>
        public static readonly DependencyProperty VBContent2Property
            = DependencyProperty.Register("VBContent2", typeof(string), typeof(VBTextBox3));
        /// <summary>
        /// Gets or sets the VB content2.
        /// </summary>
        /// <value>The VB content2.</value>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string VBContent2
        {
            get { return (string)GetValue(VBContent2Property); }
            set { SetValue(VBContent2Property, value); }
        }

        /// <summary>
        /// The VB source2 property
        /// </summary>
        public static readonly DependencyProperty VBSource2Property
            = DependencyProperty.Register("VBSource2", typeof(string), typeof(VBTextBox3));
        /// <summary>
        /// Gets or sets the VB source2.
        /// </summary>
        /// <value>The VB source2.</value>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string VBSource2
        {
            get { return (string)GetValue(VBSource2Property); }
            set { SetValue(VBSource2Property, value); }
        }

        #region Layout
        /// <summary>
        /// The width caption2 property
        /// </summary>
        public static readonly DependencyProperty WidthCaption2Property
            = DependencyProperty.Register("WidthCaption2", typeof(GridLength), typeof(VBTextBox3), new PropertyMetadata(new GridLength(4, GridUnitType.Star)));
        /// <summary>
        /// Gets or sets the width caption2.
        /// </summary>
        /// <value>The width caption2.</value>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public GridLength WidthCaption2
        {
            get { return (GridLength)GetValue(WidthCaption2Property); }
            set { SetValue(WidthCaption2Property, value); }
        }

        /// <summary>
        /// The width caption2 max property
        /// </summary>
        public static readonly DependencyProperty WidthCaption2MaxProperty
            = DependencyProperty.Register("WidthCaption2Max", typeof(double), typeof(VBTextBox3), new PropertyMetadata(Double.PositiveInfinity));
        /// <summary>
        /// Gets or sets the width caption2 max.
        /// </summary>
        /// <value>The width caption2 max.</value>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public double WidthCaption2Max
        {
            get { return (double)GetValue(WidthCaption2MaxProperty); }
            set { SetValue(WidthCaption2MaxProperty, value); }
        }

        /// <summary>
        /// The text alignment caption2 property
        /// </summary>
        public static readonly DependencyProperty TextAlignmentCaption2Property
            = DependencyProperty.Register("TextAlignmentCaption2", typeof(TextAlignment), typeof(VBTextBox), new PropertyMetadata(TextAlignment.Left));
        /// <summary>
        /// Gets or sets the text alignment caption2.
        /// </summary>
        /// <value>The text alignment caption2.</value>
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

            //    Binding binding2 = new Binding();
            //    binding2.Source = dcSource2;
            //    binding2.Path = new PropertyPath(dcPath2);
            //    binding2.Mode = BindingMode.OneWay;
            //    binding2.NotifyOnSourceUpdated = true;
            //    binding2.NotifyOnTargetUpdated = true;
            //    //SetBinding(VBTextBox3.ACCaption2TransProperty, binding2);
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
