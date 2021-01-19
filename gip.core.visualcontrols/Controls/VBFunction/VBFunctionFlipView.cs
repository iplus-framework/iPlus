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
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using gip.core.datamodel;
using gip.core.autocomponent;
using gip.core.layoutengine;

namespace gip.core.visualcontrols
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

        #region Child PAFFunctions

        public static readonly DependencyProperty FunctionListProperty = DependencyProperty.Register("FunctionList", typeof(IList<IACComponent>), typeof(VBFunctionFlipView));
        [Category("VBControl")]
        [ACPropertyInfo(23)]
        public IList<IACComponent> FunctionList
        {
            get { return (IList<IACComponent>)GetValue(FunctionListProperty); }
            set { SetValue(FunctionListProperty, value); }
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
            if (processModule.ACUrlBinding("WFNodes", ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode))
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
                            functionList.Add(wfNode);
                        }
                    }
                }
                if (FunctionList == null || (FunctionList != null && functionList.Except(FunctionList).Any()))
                {
                    FunctionList = functionList;
                    var bindingEx = GetBindingExpression(VBFlipView.ItemsSourceProperty);
                    if (bindingEx != null)
                        bindingEx.UpdateTarget();
                }
            }
            else
                FunctionList = null;
        }
    }
}
