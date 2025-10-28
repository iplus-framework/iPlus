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
using System.Collections.Concurrent;
using gip.core.layoutengine.avui.Helperclasses;

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
            get { return GetValue(ShowWFNodesProperty); }
            set { SetValue(ShowWFNodesProperty, value); }
        }

        // Using a StyledProperty as the backing store for ShowWFNodes.  This enables animation, styling, binding, etc...
        public static readonly StyledProperty<bool> ShowWFNodesProperty =
            AvaloniaProperty.Register<VBFunctionFlipView, bool>(nameof(ShowWFNodes), true);


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

        public static readonly StyledProperty<IList<IACComponent>> FunctionListProperty = 
            AvaloniaProperty.Register<VBFunctionFlipView, IList<IACComponent>>(nameof(FunctionList));
        [Category("VBControl")]
        [ACPropertyInfo(23)]
        public IList<IACComponent> FunctionList
        {
            get 
            { 
                return GetValue(FunctionListProperty); 
            }
            set 
            {
                var bso = BSOACComponent;
                IList<IACComponent> prevList = GetValue(FunctionListProperty);
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


        public static readonly StyledProperty<IEnumerable<ACChildInstanceInfo>> WFNodeListProperty = 
            AvaloniaProperty.Register<VBFunctionFlipView, IEnumerable<ACChildInstanceInfo>>(nameof(WFNodeList));
        [Category("VBControl")]
        [ACPropertyInfo(23)]
        public IEnumerable<ACChildInstanceInfo> WFNodeList
        {
            get { return GetValue(WFNodeListProperty); }
            set { SetValue(WFNodeListProperty, value); }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == WFNodeListProperty)
            {
                RefreshFunctionList();
            }
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
            binding.Path = nameof(FunctionList);
            this.Bind(VBFlipView.ItemsSourceProperty, binding);

            IACType dcACTypeInfo = null;
            object dcSource = null;
            string dcPath = "";
            Global.ControlModes dcRightControlMode = Global.ControlModes.Hidden;
            if (ShowWFNodes && processModule.ACUrlBinding("WFNodes", ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode))
            {
                Binding bindingNodes = new Binding();
                bindingNodes.Source = dcSource;
                bindingNodes.Path = dcPath;
                bindingNodes.Mode = BindingMode.OneWay;
                this.Bind(VBFunctionFlipView.WFNodeListProperty, bindingNodes);
            }

        }

        protected void EmptyChildFunctions()
        {
            if (FunctionList != null)
                FunctionList = null;
            this.ClearBinding(VBFlipView.ItemsSourceProperty);
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
