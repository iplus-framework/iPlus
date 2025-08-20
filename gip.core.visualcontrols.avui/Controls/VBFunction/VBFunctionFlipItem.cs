// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
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
using gip.core.autocomponent;
using gip.core.layoutengine.avui;
using gip.core.layoutengine.avui.Helperclasses;

namespace gip.core.visualcontrols.avui
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBFunctionFlipItem'}de{'VBFunctionFlipItem'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBFunctionFlipItem : VBVisual
    {

        /// <summary>
        /// DeInitVBControl is used to remove all References which a WPF-Control refers to.
        /// It's needed that the Garbage-Collerctor can delete the object when it's removed from the Logical-Tree.
        /// Controls that implement this interface should bind itself to the InitState of the BSOACComponent.
        /// When the BSOACComponent stops and the property changes to Destructed- or DisposedToPool-State than this method should be called.
        /// </summary>
        /// <param name="bso">The bound BSOACComponent</param>
        public override void DeInitVBControl(IACComponent bso)
        {
            BindingOperations.ClearBinding(this, ACStateProperty);
            base.DeInitVBControl(bso);
        }

        public static readonly DependencyProperty ACStateProperty = DependencyProperty.Register("ACState", typeof(ACStateEnum), typeof(VBFunctionFlipItem), new PropertyMetadata(new PropertyChangedCallback(OnDepPropChanged)));
        [Category("VBControl")]
        [ACPropertyInfo(23)]
        public ACStateEnum ACState
        {
            get { return (ACStateEnum)GetValue(ACStateProperty); }
            set { SetValue(ACStateProperty, value); }
        }

        private static void OnDepPropChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            VBFunctionFlipItem thisControl = dependencyObject as VBFunctionFlipItem;
            if (thisControl == null)
                return;
            if (args.Property == ACStateProperty)
            {
                if (thisControl.ACState != ACStateEnum.SMIdle && thisControl.ACState != ACStateEnum.SMBreakPoint)
                {
                    thisControl.Visibility = Visibility.Visible;
                    VBFunctionFlipView parentFlipView = VBVisualTreeHelper.FindParentObjectInVisualTree(thisControl, typeof(VBFunctionFlipView)) as VBFunctionFlipView;
                    if (parentFlipView != null)
                    {
                        parentFlipView.SelectedItem = thisControl.ContentACObject;
                        parentFlipView.ScrollIntoView(parentFlipView.SelectedItem);
                    }
                }
                else
                {
                    thisControl.Visibility = Visibility.Collapsed;
                }
            }
        }

        protected override void OnDesignLoaded()
        {
            Binding boundedValue = BindingOperations.GetBinding(this, VBFunctionFlipItem.ACStateProperty);
            if (boundedValue == null && ContentACObject is ACComponent)
            {
                ACComponent paFunction = ContentACObject as ACComponent;
                IACType dcACTypeInfo = null;
                object dcSource = null;
                string dcPath = "";
                Global.ControlModes dcRightControlMode = Global.ControlModes.Hidden;
                if (paFunction.ACUrlBinding(Const.ACState, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode))
                {
                    Binding binding = new Binding();
                    binding.Source = dcSource;
                    binding.Path = new PropertyPath(dcPath);
                    SetBinding(VBFunctionFlipItem.ACStateProperty, binding);
                    IACPropertyBase aCPropertyBase = dcSource as IACPropertyBase;
                    if (aCPropertyBase != null)
                    {
                        try
                        {
                            ACStateEnum paState = (ACStateEnum)aCPropertyBase.Value;
                            if (paState == ACStateEnum.SMIdle || paState == ACStateEnum.SMBreakPoint)
                                Visibility = Visibility.Collapsed;
                        }
                        catch (Exception e)
                        {
                            paFunction.Messages.LogException("VBFunctionFlipItem", "OnDesignLoaded()", e.Message);
                        }
                    }
                }
            }
        }
    }
}
