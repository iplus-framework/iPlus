// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Avalonia;
using Avalonia.Data;
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
            this.ClearBinding(ACStateProperty);
            base.DeInitVBControl(bso);
        }

        public static readonly StyledProperty<ACStateEnum> ACStateProperty = 
            AvaloniaProperty.Register<VBFunctionFlipItem, ACStateEnum>(nameof(ACState));
        [Category("VBControl")]
        [ACPropertyInfo(23)]
        public ACStateEnum ACState
        {
            get { return GetValue(ACStateProperty); }
            set { SetValue(ACStateProperty, value); }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == ACStateProperty)
            {
                if (ACState != ACStateEnum.SMIdle && ACState != ACStateEnum.SMBreakPoint)
                {
                    IsVisible = true;
                    VBFunctionFlipView parentFlipView = VBVisualTreeHelper.FindParentObjectInVisualTree(this, typeof(VBFunctionFlipView)) as VBFunctionFlipView;
                    if (parentFlipView != null)
                    {
                        parentFlipView.SelectedItem = ContentACObject;
                        parentFlipView.ScrollIntoView(parentFlipView.SelectedItem);
                    }
                }
                else
                {
                    IsVisible = false;
                }
            }
        }

        protected override void OnDesignLoaded()
        {
            var boundedValue = BindingOperations.GetBindingExpressionBase(this, VBFunctionFlipItem.ACStateProperty);
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
                    binding.Path = dcPath;
                    this.Bind(VBFunctionFlipItem.ACStateProperty, binding);
                    IACPropertyBase aCPropertyBase = dcSource as IACPropertyBase;
                    if (aCPropertyBase != null)
                    {
                        try
                        {
                            ACStateEnum paState = (ACStateEnum)aCPropertyBase.Value;
                            if (paState == ACStateEnum.SMIdle || paState == ACStateEnum.SMBreakPoint)
                                IsVisible = false;
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
