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
using System.Collections.Concurrent;

namespace gip.core.visualcontrols.avui
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBFunctionFlipView'}de{'VBFunctionFlipView'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBFunctionFlipView : VBListBox //VBFlipView
    {
        protected override void InitVBControl()
        {
            if (_Initialized)
            {
                base.InitVBControl();
                return;
            }

            if (ContextACObject != null)
            {
                LoadChildFunctions();
            }

            base.InitVBControl();
        }



        [Category("VBControl")]
        public bool ShowWFNodes
        {
            get { return (bool)GetValue(ShowWFNodesProperty); }
            set { SetValue(ShowWFNodesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowWFNodes.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowWFNodesProperty =
            DependencyProperty.Register("ShowWFNodes", typeof(bool), typeof(VBFunctionFlipView), new PropertyMetadata(true));


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
            EmptyChildFunctions();
        }

        #region Child PAFFunctions

        private ConcurrentDictionary<IACComponent, object> _WPFReferenceMap = new ConcurrentDictionary<IACComponent, object>();
        private void AddToWPFReferenceMap(IACComponent component, IACBSO bso)
        {
            if (bso == null || component == null)
                return;
            if (!_WPFReferenceMap.ContainsKey(component))
            {
                object dummyWPFObj = new object();
                _WPFReferenceMap.TryAdd(component, dummyWPFObj);
                bso.AddWPFRef(dummyWPFObj.GetHashCode(), component);
            }
        }

        private void RemoveFromWPFReferenceMap(IACComponent component, IACBSO bso)
        {
            if (bso == null || component == null)
                return;
            if (_WPFReferenceMap.ContainsKey(component))
            {
                object dummyWPFObj;
                _WPFReferenceMap.TryRemove(component, out dummyWPFObj);
                if (dummyWPFObj != null && bso != null)
                    bso.RemoveWPFRef(dummyWPFObj.GetHashCode());
            }
        }

        public static readonly DependencyProperty FunctionListProperty = DependencyProperty.Register("FunctionList", typeof(IList<IACComponent>), typeof(VBFunctionFlipView));
        [Category("VBControl")]
        [ACPropertyInfo(23)]
        public IList<IACComponent> FunctionList
        {
            get 
            { 
                return (IList<IACComponent>)GetValue(FunctionListProperty); 
            }
            set 
            {
                var bso = BSOACComponent;
                IList<IACComponent> prevList = (IList<IACComponent>)GetValue(FunctionListProperty);
                if (prevList != null && prevList.Any())
                {
                    foreach (IACComponent component in prevList)
                    {
                        if (   value != null
                            && !value.Contains(component))
                            RemoveFromWPFReferenceMap(component, bso);
                    }
                }
                SetValue(FunctionListProperty, value);
                
                if (value != null && bso != null)
                {
                    foreach (IACComponent component in value)
                    {
                        AddToWPFReferenceMap(component, bso);
                    }
                }
            }
        }


        public static readonly DependencyProperty WFNodeListProperty = DependencyProperty.Register("WFNodeList", typeof(IEnumerable<ACChildInstanceInfo>), typeof(VBFunctionFlipView), new PropertyMetadata(new PropertyChangedCallback(OnWFNodeListChanged)));
        [Category("VBControl")]
        [ACPropertyInfo(23)]
        public IEnumerable<ACChildInstanceInfo> WFNodeList
        {
            get { return (IEnumerable<ACChildInstanceInfo>)GetValue(WFNodeListProperty); }
            set { SetValue(WFNodeListProperty, value); }
        }

        #endregion

        protected void LoadChildFunctions()
        {
            if (ContextACObject == null || !(ContextACObject is ACComponent))
            {
                EmptyChildFunctions();
                return;
            }
            ACComponent processModule = ContextACObject as ACComponent;
            Type typeOfFunction = typeof(PAProcessFunction);
            FunctionList = new BindingList<IACComponent>(processModule.ACComponentChildsOnServer.Where(c => c.ACType != null && typeOfFunction.IsAssignableFrom(c.ACType.ObjectType)).ToList());

            Binding binding = new Binding();
            binding.Source = this;
            binding.Mode = BindingMode.OneWay;
            binding.Path = new PropertyPath("FunctionList");
            SetBinding(VBFlipView.ItemsSourceProperty, binding);

            IACType dcACTypeInfo = null;
            object dcSource = null;
            string dcPath = "";
            Global.ControlModes dcRightControlMode = Global.ControlModes.Hidden;
            if (ShowWFNodes && processModule.ACUrlBinding("WFNodes", ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode))
            {
                Binding bindingNodes = new Binding();
                bindingNodes.Source = dcSource;
                bindingNodes.Path = new PropertyPath(dcPath);
                bindingNodes.NotifyOnSourceUpdated = true;
                bindingNodes.NotifyOnTargetUpdated = true;
                bindingNodes.Mode = BindingMode.OneWay;
                SetBinding(VBFunctionFlipView.WFNodeListProperty, bindingNodes);
            }

        }

        protected void EmptyChildFunctions()
        {
            if (FunctionList != null)
                FunctionList = null;
            BindingOperations.ClearBinding(this, VBFlipView.ItemsSourceProperty);
        }

        private static void OnWFNodeListChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is VBFunctionFlipView)
            {
                VBFunctionFlipView control = d as VBFunctionFlipView;
                if (control != null)
                {
                    control.RefreshFunctionList();
                }
            }
        }

        protected void RefreshFunctionList()
        {
            ACComponent processModule = ContextACObject as ACComponent;
            if (processModule != null)
            {
                Type typeOfFunction = typeof(PAProcessFunction);
                var functionList = new BindingList<IACComponent>(processModule.ACComponentChildsOnServer.Where(c => c.ACType != null && typeOfFunction.IsAssignableFrom(c.ACType.ObjectType)).ToList());
                if (WFNodeList != null && WFNodeList.Any())
                {
                    foreach (var wfInstanceInfo in WFNodeList)
                    {
                        ACComponent wfNode = processModule.ACUrlCommand(wfInstanceInfo.ACUrlParent + "\\" + wfInstanceInfo.ACIdentifier) as ACComponent;
                        if (wfNode != null)
                        {
                            AddToWPFReferenceMap(wfNode, BSOACComponent);
                            functionList.Add(wfNode);
                        }
                    }
                }
                if (FunctionList == null || (FunctionList != null && (functionList.Except(FunctionList).Any() || FunctionList.Except(functionList).Any())))
                {
                    FunctionList = functionList;
                }
            }
            else
                FunctionList = null;
        }
    }
}
