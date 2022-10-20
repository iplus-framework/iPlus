using System.Linq;
using gip.core.datamodel;
using gip.core.autocomponent;
using System.Collections.Generic;
using System.Windows.Markup;
using System.Windows.Controls;
using System.Windows;
using System;

namespace gip.core.manager
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBPresenterMethod'}de{'VBPresenterMethod'}", Global.ACKinds.TACBSOGlobal, Global.ACStorableTypes.NotStorable, true, true)]
    public class VBPresenterMethod : VBPresenter 
    {
        #region c´tors
        public VBPresenterMethod(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            _VarioConfigManager = ConfigManagerIPlus.ACRefToServiceInstance(this);
            return base.ACInit(startChildMode);
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            if (_VarioConfigManager != null)
                ConfigManagerIPlus.DetachACRefFromServiceInstance(this, _VarioConfigManager);
            _VarioConfigManager = null;

            return base.ACDeInit(deleteACClassTask);
        }
        #endregion

        #region Managers

        protected ACRef<ConfigManagerIPlus> _VarioConfigManager = null;
        public ConfigManagerIPlus VarioConfigManager
        {
            get
            {
                if (_VarioConfigManager == null)
                    return null;
                return _VarioConfigManager.ValueT;
            }
        }

        #endregion

        public string SelectedACUrl
        {
            get 
            {
                if (this.SelectedWFNode == null)
                    return string.Empty;
                else
                    return this.SelectedWFNode.GetACUrl(this.PresenterACWorkflowNode.ParentACObject); 
            }
        }

        protected override void OnSelectionChanged()
        {
            base.OnSelectionChanged();
            OnPropertyChanged("HasDetail");
            OnPropertyChanged("SelectedACUrl");
        }

        #region Load
        /// <summary>
        /// Laden eines Methoden-Workflows aus der toten Welt
        /// acWorkflow:
        /// 1. ACClassMethod
        /// 2. ACProgram
        /// 3. PartslistProgram
        /// </summary>
        /// <param name="acWorkflow">Kann ein ACClassMethod oder ein ACProgram sein</param>
        public void Load(IACWorkflowContext acWorkflow)
        {
            if (this.SelectedWFNode != null) 
                SelectedWFNode = null;

            if (acWorkflow == null)
            {
                WFRootContext = null;
                PresenterACWorkflowNode = null;
                SelectedRootWFNode = null;
                return;
            }
            else if (acWorkflow is ACClassMethod)
            {
                if ((acWorkflow as ACClassMethod).ACKind != Global.ACKinds.MSWorkflow) return;
            }
            
            ACClass pwObjectNodeMethodWF = null;
            IACComponentPWNode presenterACWorkflowNode = null;
            ACValueList acValueList = null;

            using (ACMonitor.Lock(Database.ContextIPlus.QueryLock_1X000))
            {
                pwObjectNodeMethodWF = Database.ContextIPlus.ACClass.Where(c => c.ACIdentifier == typeof(PWOfflineNodeMethod).Name).FirstOrDefault();
                if (pwObjectNodeMethodWF != null)
                    acValueList = pwObjectNodeMethodWF.ACParameter;
            }
            if (pwObjectNodeMethodWF != null && acValueList != null)
            {
                acValueList["WFContext"] = acWorkflow;
                if (acWorkflow.RootWFNode != null)
                {
                    presenterACWorkflowNode = ACActivator.CreateInstance(pwObjectNodeMethodWF, acWorkflow.RootWFNode, this, acValueList, Global.ACStartTypes.Automatic, null, acWorkflow.RootWFNode.ACIdentifier) as IACComponentPWNode;
                }

                WFRootContext = acWorkflow;
                PresenterACWorkflowNode = presenterACWorkflowNode;
                SelectedRootWFNode = presenterACWorkflowNode;
            }
        }

        #endregion

        /// <summary>
        /// ACAction is called when one IACInteractiveObject (Source) wants to inform another IACInteractiveObject (Target) about an relevant interaction-event.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        public override void ACAction(ACActionArgs actionArgs)
        {
            if (actionArgs.ElementAction == Global.ElementActionType.DesignModeOn || actionArgs.ElementAction == Global.ElementActionType.DesignModeOff)
                ParentACComponent.ACAction(actionArgs);

            base.ACAction(actionArgs);
        }

        /// <summary>
        /// Returns a name of all element that's presenter shows
        /// </summary>
        /// <returns></returns>
        public Msg GetPresenterElements(out List<string> result)
        {
            result = new List<string>();

            string xaml = PresenterACWorkflowNode.XMLDesign;
            if (string.IsNullOrEmpty(xaml))
                return null;
            
            try
            {
                Canvas root = XamlReader.Parse(xaml) as Canvas;
                if (root != null)
                    GetElements(root, result);
            }
            catch (Exception e)
            {
                return new Msg(eMsgLevel.Exception, e.Message);
            }
            result = result.Where(c => !string.IsNullOrEmpty(c)).ToList();
            return null;
        }

        private void GetElements(FrameworkElement item, List<string> results)
        {
            if (item == null)
                return;

            results.Add(item.Name);

            ContentControl contentControl = item as ContentControl;
            if (contentControl != null && contentControl.Content != null)
            {
                GetElements(contentControl.Content as FrameworkElement, results);
            }
            else
            {
                Canvas canvas = item as Canvas;
                if(canvas != null && canvas.Children.Count > 0)
                {
                    foreach (var child in canvas.Children)
                    {
                        GetElements(child as FrameworkElement, results);
                    }
                }
            }
        }

        #region Sub-Methods

        public bool HasDetail
        {
            get { return IsEnabledShowDetail(); }
        }

        [ACMethodInteraction("WF", "en{'Details'}de{'Details'}", 200, true)]
        public void ShowDetail()
        {
            if (!IsEnabledShowDetail())
                return;

            PWOfflineNodeMethod pwObjectNode = SelectedWFNode as PWOfflineNodeMethod;

            var pwObjectNodeMethodWF = Database.ContextIPlus.ACClass.Where(c => c.ACIdentifier == "PWOfflineNodeMethod").First();
            var pwObjectRoot = ACActivator.CreateInstance(pwObjectNodeMethodWF, pwObjectNode.ContentACClassWF.RefPAACClassMethod.RootWFNode, pwObjectNode, null, Global.ACStartTypes.Automatic, null, pwObjectNode.ContentACClassWF.RefPAACClassMethod.RootWFNode.ACIdentifier) as IACComponentPWNode;

            SelectedRootWFNode = pwObjectRoot;
            ParentACComponent.ACAction(new ACActionArgs(this, 0, 0, Global.ElementActionType.Refresh));
        }

        public bool IsEnabledShowDetail()
        {
            PWOfflineNodeMethod pwObjectNode = SelectedWFNode as PWOfflineNodeMethod;

            if (pwObjectNode == null)
                return false;
            if (pwObjectNode.ContentACClassWF.RefPAACClassMethod == null)
                return false;
            if (pwObjectNode.ContentACClassWF.RefPAACClassMethod.ACKind != Global.ACKinds.MSWorkflow)
                return false;

            return true;
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
            if (SelectedRootWFNode == null)
                return false;
            return SelectedRootWFNode.ParentRootWFNode != null;
        }

        //[ACMethodInteraction("", "en{'Show log'}de{'Show log'}", 999, false)]
        //public void ShowACProgramFromWorkFlow()
        //{
        //    if (this.ParentACComponent is ACBSO && SelectedWFNode != null)
        //    {
        //        this.ParentACComponent.ACUrlCommand("!ShowACProgramFromWorkflow", new object[] { SelectedWFNode.GetACUrl(SelectedRootWFNode) });
        //    }
        //}

        //public bool IsEnabledShowACProgramFromWorkFlow()
        //{
        //    return true;
        //}

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
