using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using gip.core.autocomponent;

namespace gip.core.manager
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBPresenterTask'}de{'VBPresenterTask'}", Global.ACKinds.TACBSOGlobal, Global.ACStorableTypes.NotStorable, true, true)]
    public class VBPresenterTask : VBPresenter 
    {
        #region c´tors

        public VBPresenterTask(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }
        
        #endregion


        #region Property

        public string CurrentLayout
        {
            get
            {
                var workflowDesign = this.GetDesign(Global.ACKinds.DSDesignLayout, Global.ACUsages.DUMain);
                return workflowDesign != null ? workflowDesign.XAMLDesign : null;
            }
        }

        private bool _LoadedByTask = false;

        #endregion


        #region Methods -> Load
        // TODO: IACComponentTaskExec auch bei Proxy
        /// <summary>
        ///   <br />
        /// </summary>
        /// <param name="acChildInstanceInfo"></param>
        public void Load(IACTask acChildInstanceInfo)
        {
            if (!acChildInstanceInfo.ExecutingInstance.IsObjLoaded)
                return;
            IACComponentPWNode wfInstance = acChildInstanceInfo.ExecutingInstance.ValueT as IACComponentPWNode;
            if (wfInstance == null)
                return;
            LoadWFInstance(wfInstance);
        }

        public void Load(ACClassTask acClassTask)
        {
            if (acClassTask == null)
                return;
            if (acClassTask.ACClassTask1_ParentACClassTask == null)
                return;
            string acUrl = acClassTask.ACClassTask1_ParentACClassTask.TaskTypeACClass.GetACUrlComponent();
            if (String.IsNullOrEmpty(acUrl))
                return;
            ACComponent appManager = ACUrlCommand(acUrl) as ACComponent;
            if (appManager == null)
                return;
            var childsInfo = appManager.GetChildInstanceInfo(1, true);
            if (childsInfo == null || !childsInfo.Any())
                return;
            var childInfo = childsInfo.Where(c => c.ACIdentifier == acClassTask.ACIdentifier).FirstOrDefault();
            if (childInfo == null)
                return;
            Load(appManager, childInfo);
            _LoadedByTask = true;
        }

        public void Load(IACComponent applicationManager, ACChildInstanceInfo acChildInstanceInfo)
        {
            _LoadedByTask = false;
            IACComponentPWNode wfInstance = null;
            if (applicationManager.IsProxy)
            {
                wfInstance = applicationManager.StartComponent(acChildInstanceInfo, Global.ACStartTypes.Automatic, true) as IACComponentPWNode;
                if (wfInstance != null && wfInstance is ACComponentProxy)
                {
                    (wfInstance as ACComponentProxy).ReloadChildsOverServerInstanceInfo(null);
                }
            }
            else
            {
                var queryChild = applicationManager.ACComponentChilds.Where(c => c.ACIdentifier == acChildInstanceInfo.ACIdentifier);
                if (queryChild != null && queryChild.Any())
                {
                    wfInstance = queryChild.First() as IACComponentPWNode;
                }
            }
            if (wfInstance == null)
                return;
            LoadWFInstance(wfInstance);
        }

        public void Load(IACComponentPWNode wfInstance)
        {
            _LoadedByTask = false;
            if (wfInstance == null)
                return;
            if (wfInstance != null && wfInstance is ACComponentProxy)
            {
                (wfInstance as ACComponentProxy).ReloadChildsOverServerInstanceInfo(null);
            }
            LoadWFInstance(wfInstance);
        }

        protected void LoadWFInstance(IACComponentPWNode wfInstance)
        {
            if (wfInstance == null)
                return;
            WFRootContext = wfInstance.WFContext;      //   (ACProgram oder ACClassMethod, wenn kein ACProgram vorhanden
            PresenterACWorkflowNode = wfInstance;
            SelectedRootWFNode = wfInstance;
            ACMethod acMethod = wfInstance.ACUrlCommand("CurrentACMethod") as ACMethod;
            ReferencePoint.CheckWPFRefsAndDetachUnusedProxies();
        }

        public void Unload()
        {
            WFRootContext = null;
            PresenterACWorkflowNode = null;
            SelectedRootWFNode = null;
            ReferencePoint?.CheckWPFRefsAndDetachUnusedProxies();
        }

        protected override void OnSelectedRootWFNodeContextChanged()
        {
            // If Loaded by Task, then don't automatically reload workflow on gui because use must selct another task first.
            if (_LoadedByTask)
                return;
            //if (SelectedRootWFNode != null)
            //{
            //    Load(SelectedRootWFNode);
            //}
            base.OnSelectedRootWFNodeContextChanged();
        }

        #endregion

        #region Methods -> Browsing


        [ACMethodInteraction("WF", "en{'Details'}de{'Details'}", 200, true)]
        public void ShowDetail()
        {
            if (!IsEnabledShowDetail())
                return;

            PWNodeProcessWorkflow pwNodeProcessWorkflow = SelectedWFNode as PWNodeProcessWorkflow;

            if(pwNodeProcessWorkflow != null)
            {
                string test = pwNodeProcessWorkflow.ACIdentifier;
                ACClass childApplicationManager = null;
                ACClassTask childTask = null;
                if(pwNodeProcessWorkflow.InvokableTaskExecutors != null && pwNodeProcessWorkflow.InvokableTaskExecutors.Any())
                {
                   ACComponent component =  pwNodeProcessWorkflow.InvokableTaskExecutors.FirstOrDefault();
                   childApplicationManager = component.ACType as ACClass;
                   childTask = Database.ContextIPlus.ACClassTask.Where(c => c.TaskTypeACClassID == childApplicationManager.ACClassID && !c.IsTestmode).FirstOrDefault().ACClassTask1_ParentACClassTask;
                }
                
                if(childApplicationManager != null && childTask != null)
                {
                    ITaskPreviewCall bsoHandler = ParentACObject as ITaskPreviewCall;
                    bsoHandler.PreviewTask(childApplicationManager, childTask);
                }
            }
            

            //var pwObjectNodeMethodWF = Database.ContextIPlus.ACClass.Where(c => c.ACIdentifier == "PWOfflineNodeMethod").First();
            //var pwObjectRoot = ACActivator.CreateInstance(pwObjectNodeMethodWF, pwObjectNode.ContentACClassWF.RefPAACClassMethod.RootACVisual, pwObjectNode, null, Global.ACStartTypes.Automatic, null, pwObjectNode.ContentACClassWF.RefPAACClassMethod.RootACVisual.ACIdentifier) as IACComponentPWNode;

            //SelectedRootWFNode = pwObjectRoot;
            //ParentACComponent.ACAction(new ACActionArgs(this, 0, 0, Global.ElementActionType.Refresh));
        }

        public bool IsEnabledShowDetail()
        {
            return true;
            //PWOfflineNodeMethod pwObjectNode = SelectedWFNode as PWOfflineNodeMethod;

            //if (pwObjectNode == null)
            //    return false;
            //if (pwObjectNode.ContentACClassWF.RefPAACClassMethod == null)
            //    return false;
            //if (pwObjectNode.ContentACClassWF.RefPAACClassMethod.ACKind != Global.ACKinds.MSWorkflow)
            //    return false;

            //return true;
        }

        [ACMethodInteraction("Workflow", "en{'Back'}de{'Zurück'}", 9999, false)]
        public void HideDetail()
        {
            var parentRootWorkflowNode = SelectedRootWFNode.ParentRootWFNode;
            if (parentRootWorkflowNode != null)
            {
                parentRootWorkflowNode.StopComponent(SelectedRootWFNode);
                SelectedRootWFNode = parentRootWorkflowNode;
            }
            ParentACComponent.ACAction(new ACActionArgs(this, 0, 0, Global.ElementActionType.Refresh));
        }

        public bool IsEnabledHideDetail()
        {
            return false;
            //if (SelectedRootWFNode == null)
            //    return false;
            //return SelectedRootWFNode.ParentRootWFNode != null;
        }

        #endregion

        #region Alarm-Selection

        private Msg _MsgForSwitchingView = null;
        public Msg MsgForSwitchingView
        {
            get
            {
                return _MsgForSwitchingView;
            }
            set
            {
                _MsgForSwitchingView = value;
            }
        }

        protected override void OnWPFRefAdded(int hashOfDepObj, IACObject boundedObject)
        {
            base.OnWPFRefAdded(hashOfDepObj, boundedObject);
            if (_MsgForSwitchingView != null
                && _MsgForSwitchingView.SourceComponent != null
                && boundedObject is IACComponent)
            {
                string boundedObjUrl = boundedObject.GetACUrl();
                if ((_MsgForSwitchingView.SourceComponent.ValueT != null && _MsgForSwitchingView.SourceComponent.ValueT == boundedObject)
                    || _MsgForSwitchingView.Source == boundedObjUrl)
                {
                    var selManager = VBBSOSelectionManager as VBBSOSelectionManager;
                    if (selManager != null)
                        selManager.HighlightContentACObject(boundedObject, false);
                    _MsgForSwitchingView = null;
                }
            }
        }
        #endregion


        #region Execute-Helper-Handlers

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case"ShowDetail":
                    ShowDetail();
                    return true;
                case"IsEnabledShowDetail":
                    result = IsEnabledShowDetail();
                    return true;
                case"HideDetail":
                    HideDetail();
                    return true;
                case"IsEnabledHideDetail":
                    result = IsEnabledHideDetail();
                    return true;
            }
                return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion


    }
}
