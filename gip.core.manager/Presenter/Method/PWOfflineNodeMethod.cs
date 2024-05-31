using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using gip.core.autocomponent;
using System.Xml;
using Microsoft.EntityFrameworkCore;

namespace gip.core.manager
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'PWOfflineNodeMethod '}de{'PWOfflineNodeMethod '}", Global.ACKinds.TACObject, Global.ACStorableTypes.NotStorable, true, true)]
    [ACClassConstructorInfo(
        new object[]
        {
            new object[] {"WFContext", Global.ParamOption.Optional, typeof(IACWorkflowContext)}
        }
    )]
    public class PWOfflineNodeMethod : PWOfflineNode
    {
        #region c´tors
        public PWOfflineNodeMethod(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "") :
            base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _ContentACWorkflow = ParameterValue("WFContext") as IACWorkflowContext;
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (ContentACClassWF == null)
                return false;
            if (ContentACClassWF.EntityState != EntityState.Added)
            {
                if (ContentACClassWF.EntityState == EntityState.Detached)
                {
                    Database.ContextIPlus.ACClassWF.Attach(ContentACClassWF);
                }
                ContentACClassWF.ACClassWF_ParentACClassWF.AutoLoad(ContentACClassWF.ACClassWF_ParentACClassWFReference, ContentACClassWF);
                ContentACClassWF.ACClassWFEdge_SourceACClassWF.AutoLoad(ContentACClassWF.ACClassWFEdge_SourceACClassWFReference, ContentACClassWF);
                ContentACClassWF.ACClassWFEdge_TargetACClassWF.AutoLoad(ContentACClassWF.ACClassWFEdge_TargetACClassWFReference, ContentACClassWF);
            }
            foreach (var childACClassWF in ContentACClassWF.ACClassWF_ParentACClassWF)
            {
                CreateChildPWNode(childACClassWF, startChildMode);
            }

            return base.ACInit(startChildMode);
        }

        public override void CreateChildPWNode(IACWorkflowNode acWorkflowNode, Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            ACActivator.CreateInstance(ComponentClass, acWorkflowNode, this, null, startChildMode, null, acWorkflowNode.ACIdentifier);
        }
        #endregion

        #region Properties

        IACWorkflowContext _ContentACWorkflow;
        [ACPropertyInfo(2, "", "", "", false)]
        public IACWorkflowContext ContentACWorkflow
        {
            get
            {
                return _ContentACWorkflow;
            }
        }

        /// <summary>Reference to the definition of this Workflownode.</summary>
        /// <value>Reference to the definition of this Workflownode.</value>
        [ACPropertyInfo(1, "", "", "", false)]
        public override ACClassWF ContentACClassWF
        {
            get
            {
                return Content as ACClassWF;
            }
        }

        private ACStateEnum? _ACState;
        [ACPropertyInfo(3, "", "", "", false)]
        public ACStateEnum ACState
        {
            get
            {
                if (_ACState.HasValue)
                    return _ACState.Value;
                if (HasRules <= 0)
                    _ACState = ACStateEnum.SMIdle;
                else
                {
                    _ACState = ACStateEnum.SMIdle;
                    // TODO: ConfigExecutingModule - get executing module
                    Guid? vbiACClassID = null; // Rule values not support vbiACClassID
                    int priorityLevel = 0;
                    IACConfig configValue =
                        VBPresenter
                        .VarioConfigManager
                        .GetConfiguration(
                            MandatoryConfigStores,
                            PreValueACUrl,
                            ContentACClassWF.ConfigACUrl + @"\Rules\" + ACClassWFRuleTypes.Breakpoint.ToString(),
                            vbiACClassID,
                            out priorityLevel);
                    if (configValue != null)
                    {
                        RuleValueList ruleValueList = configValue[RuleValueList.ClassName] as RuleValueList;
                        if (ruleValueList != null)
                        {
                            if (ruleValueList.IsBreakPointSet())
                                _ACState = ACStateEnum.SMBreakPoint;
                        }
                    }
                }
                return _ACState.Value;
            }
            protected set
            {
                _ACState = value;
                OnPropertyChanged("ACState");
            }
        }

        private short? _HasRules = null;
        [ACPropertyInfo(4, "", "", "", false)]
        public short HasRules
        {
            get
            {

                if (_HasRules.HasValue)
                    return _HasRules.Value;
                if (this.VBPresenter == null || this.VBPresenter.VarioConfigManager == null || CurrentConfigStore == null)
                    return 0;
                // OK: ConfigExecutingModule - get executing module
                Guid? vbiACClassID = null;  // Rule values not support vbiACClassID
                int configOnLevel =
                    VBPresenter
                    .VarioConfigManager
                    .HasConfiguration(
                        CurrentConfigStore,
                        MandatoryConfigStores,
                        PreValueACUrl,
                        ContentACClassWF.ConfigACUrl,
                        vbiACClassID);
                if (configOnLevel == 0)
                    _HasRules = 0;
                else if (configOnLevel == 1)
                    _HasRules = 1;
                else
                    _HasRules = 2;
                return _HasRules.Value;
            }
        }

        private short? _HasPlanning = null;
        [ACPropertyInfo(5, "", "", "", false)]
        public short HasPlanning
        {
            get
            {

                if (_HasPlanning.HasValue)
                    return _HasPlanning.Value;
                if (this.VBPresenter == null || this.VBPresenter.VarioConfigManager == null || ContentACClassWF == null)
                    return 0;

                _HasPlanning = (short)(VBPresenter.VarioConfigManager.HasPlanning(Database, CurrentConfigStore, ContentACClassWF.ACClassWFID) ? 1 : 0);

                return _HasPlanning.Value;
            }
        }

        #endregion

        #region IACDesignProvider

        /// <summary>Returns a ACClassDesign for presenting itself on the gui</summary>
        /// <param name="acUsage">Filter for selecting designs that belongs to this ACUsages-Group</param>
        /// <param name="acKind">Filter for selecting designs that belongs to this ACKinds-Group</param>
        /// <param name="vbDesignName">Optional: The concrete acIdentifier of the design</param>
        /// <returns>ACClassDesign</returns>
        public override ACClassDesign GetDesign(Global.ACUsages acUsage, Global.ACKinds acKind, string vbDesignName = "")
        {
            return ContentACClassWF.GetDesign(acUsage, acKind, vbDesignName);
        }

        #endregion

        #region IACObject
        /// <summary>
        /// The ACUrlCommand is a universal method that can be used to query the existence of an instance via a string (ACUrl) to:
        /// 1. get references to components,
        /// 2. query property values,
        /// 3. execute method calls,
        /// 4. start and stop Components,
        /// 5. and send messages to other components.
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>Result if a property was accessed or a method was invoked. Void-Methods returns null.</returns>
        public override object ACUrlCommand(string acUrl, params object[] acParameter)
        {
            object result = base.ACUrlCommand(acUrl, acParameter);
            if (result == null)
            {
                ACClassWF childACClassWF = this.ContentACClassWF.ACUrlCommand(acUrl) as ACClassWF;
                if (childACClassWF != null)
                {
                    result = ACActivator.CreateInstance(ComponentClass, childACClassWF, this, null) as IACObjectWithInit;
                }
            }
            return result;
        }
        #endregion

        #region IACComponentWorkflow
        /// <summary>
        /// XAML-Code for Presentation
        /// </summary>
        /// <value>
        /// XAML-Code for Presentation
        /// </value>
        public override string XMLDesign
        {
            get
            {
                ContentACClassWF.ACClassMethod.AutoRefresh();
                return ContentACClassWF.ACClassMethod.XMLDesign;
            }
            set
            {
                ContentACClassWF.ACClassMethod.XMLDesign = value;
            }
        }

        /// <summary>
        /// Root-Workflownode of type PWOfflineNodeMethod
        /// </summary>
        /// <value>Root-Workflownode of type PWOfflineNodeMethod</value>
        public override IACComponentPWNode ParentRootWFNode
        {
            get
            {
                if (!(this.ParentACComponent is PWOfflineNodeMethod))
                    return null;
                PWOfflineNodeMethod parentPWNodeMethod = this.ParentACComponent as PWOfflineNodeMethod;
                if (parentPWNodeMethod.ContentACClassWF.ACClassMethod.RootWFNode == parentPWNodeMethod.ContentACClassWF)
                    return parentPWNodeMethod;
                return parentPWNodeMethod.ParentRootWFNode;
            }
        }

        /// <summary>
        /// Returns the Workflow-Context (ACClassMethod) for reading and saving the configuration-data of a workflow.
        /// </summary>
        /// <value>The Workflow-Context</value>
        public override IACWorkflowContext WFContext
        {
            get
            {
                PWOfflineNodeMethod pwNodeMethod = this;
                while (pwNodeMethod.ParentACComponent is PWOfflineNodeMethod)
                {
                    pwNodeMethod = pwNodeMethod.ParentACComponent as PWOfflineNodeMethod;
                }

                return pwNodeMethod.ContentACClassWF.ACClassMethod;
            }
        }

        public VBPresenterMethod VBPresenter
        {
            get
            {
                return FindParentComponent<VBPresenterMethod>(c => c is VBPresenterMethod);
            }
        }
        #endregion

        #region Diagnostics and Dump
        protected override void DumpPropertyList(XmlDocument doc, XmlElement xmlACPropertyList)
        {
            base.DumpPropertyList(doc, xmlACPropertyList);

            XmlElement wfInfos = xmlACPropertyList["ContentACClassWFInfo"];
            if (wfInfos == null && ContentACClassWF != null)
            {
                wfInfos = doc.CreateElement("ContentACClassWFInfo");
                if (wfInfos != null)
                    wfInfos.InnerText = String.Format("ACClassWFID:{0}, PWACClassID:{1}, RefPAACClassID:{2}, RefPAACClassMethodID:{3}", ContentACClassWF.ACClassWFID, ContentACClassWF.PWACClassID, ContentACClassWF.RefPAACClassID, ContentACClassWF.RefPAACClassMethodID);
                else
                    wfInfos.InnerText = "null";
                xmlACPropertyList.AppendChild(wfInfos);
            }

        }
        #endregion

        #region IACConfigURL

        public override string ConfigACUrl
        {
            get
            {
                if (this.ParentACObject is IACConfigURL)
                {
                    return (ParentACObject as IACConfigURL).ConfigACUrl + "\\" + ContentACClassWF.ACIdentifier;
                }
                else
                {
                    return ContentACClassWF.ACIdentifier;
                }
            }
        }

        public override string PreValueACUrl
        {
            get
            {
                IACComponentPWNode pwNodeMethod = this;
                while (pwNodeMethod != null)
                {
                    if (pwNodeMethod.ContentACClassWF?.ACClassMethod.RootWFNode == pwNodeMethod.ContentACClassWF)
                    {
                        string preACUrl = "";
                        IACComponentPWNode pwNodeMethod2 = pwNodeMethod.ParentACObject as IACComponentPWNode;
                        while (pwNodeMethod2 != null)
                        {
                            preACUrl = pwNodeMethod2.ACIdentifier + "\\" + preACUrl;
                            pwNodeMethod2 = pwNodeMethod2.ParentACObject as IACComponentPWNode;
                        }
                        return preACUrl;
                    }
                    pwNodeMethod = pwNodeMethod.ParentACObject as IACComponentPWNode;
                }
                return "";
            }
        }

        #endregion

        #region IACConfigMethodHierarchy

        public override List<ACClassMethod> ACConfigMethodHierarchy
        {
            get
            {
                List<ACClassMethod> methods = new List<ACClassMethod>();
                IACComponentPWNode pwNodeInLoop = this.ParentRootWFNode;
                while (pwNodeInLoop != null)
                {
                    if (pwNodeInLoop.ContentACClassWF != null && pwNodeInLoop.ContentACClassWF.ACClassMethod != null)
                        methods.Add(pwNodeInLoop.ContentACClassWF.ACClassMethod);
                    pwNodeInLoop = pwNodeInLoop.ParentRootWFNode;
                }
                int i = 1;
                foreach (ACClassMethod method in methods)
                {
                    method.OverridingOrder = i;
                    i++;
                }
                return methods;
            }
        }

        #endregion

        #region IACConfigItemsSelection

        public override List<IACConfigStore> MandatoryConfigStores
        {
            get
            {
                IACBSOConfigStoreSelection parentBSOConfigStore = FindParentComponent<IACBSOConfigStoreSelection>(c => c is IACBSOConfigStoreSelection);
                List<IACConfigStore> methods = ACConfigMethodHierarchy.Select(x => x as IACConfigStore).ToList();
                List<IACConfigStore> mandatoryConfigStores = new List<IACConfigStore>();
                if (parentBSOConfigStore != null)
                    mandatoryConfigStores = parentBSOConfigStore.MandatoryConfigStores.ToList();
                List<IACConfigStore> resultList = new List<IACConfigStore>();
                if (mandatoryConfigStores != null)
                    resultList.AddRange(mandatoryConfigStores);
                if (methods != null)
                    resultList.AddRange(methods);
                return resultList;
            }
        }

        public override IACConfigStore CurrentConfigStore
        {
            get
            {
                IACConfigStoreSelection parentBSOConfigStore = FindParentComponent<IACBSOConfigStoreSelection>(c => c is IACBSOConfigStoreSelection);
                if (parentBSOConfigStore != null)
                    return parentBSOConfigStore.CurrentConfigStore;
                return null;
            }
        }

        public override bool IsReadonly
        {
            get
            {
                IACConfigStoreSelection parentBSOConfigStore = FindParentComponent<IACConfigStoreSelection>();
                return (parentBSOConfigStore != null) ? parentBSOConfigStore.IsReadonly : false;
            }
        }

        #endregion

        #region Methods
        [ACMethodInteraction("Process", "en{'Set breakpoint'}de{'Haltepunkt setzen'}", (short)200, true)]
        public virtual void SetBreakPoint()
        {
            if (IsEnabledSetBreakPoint())
            {
                SetBreakPoint(true);
            }
        }

        public virtual bool IsEnabledSetBreakPoint()
        {
            return ContentACClassWF != null
                    && ContentACClassWF.PWACClass != null
                    && ContentACClassWF.PWACClass.ACKind >= Global.ACKinds.TPWNode
                    && ContentACClassWF.PWACClass.ACKind <= Global.ACKinds.TPWNodeWorkflow
                    && ACState != ACStateEnum.SMBreakPoint;
        }

        [ACMethodInteraction("Process", "en{'Remove breakpoint'}de{'Haltepunkt entfernen'}", (short)201, true)]
        public virtual void RemoveBreakPoint()
        {
            if (IsEnabledRemoveBreakPoint())
            {
                SetBreakPoint(false);
            }
        }

        public virtual bool IsEnabledRemoveBreakPoint()
        {
            return ContentACClassWF != null
                    && ContentACClassWF.PWACClass != null
                    && ContentACClassWF.PWACClass.ACKind >= Global.ACKinds.TPWNode
                    && ContentACClassWF.PWACClass.ACKind <= Global.ACKinds.TPWNodeWorkflow
                    && ACState == ACStateEnum.SMBreakPoint;
        }

        protected void SetBreakPoint(bool setBreak)
        {
            IACConfig configValue = CurrentConfigStore.ConfigurationEntries.FirstOrDefault(c => c.PreConfigACUrl == this.PreValueACUrl && c.LocalConfigACUrl == this.ContentACClassWF.ConfigACUrl + @"\Rules\" + ACClassWFRuleTypes.Breakpoint.ToString());
            if (setBreak)
            {
                if (configValue == null)
                {
                    configValue = CurrentConfigStore.NewACConfig();
                    configValue.LocalConfigACUrl = this.ContentACClassWF.ConfigACUrl + @"\Rules\" + ACClassWFRuleTypes.Breakpoint.ToString();
                    configValue.PreConfigACUrl = this.PreValueACUrl;
                    //CurrentConfigStore.ConfigurationEntries.Add(configValue);
                }

                RuleValueList newRuleValueList = new RuleValueList();
                RuleValue item = new RuleValue();
                item.RuleType = ACClassWFRuleTypes.Breakpoint;
                item.ACClassACUrl = new List<string>() { @"ValueType\System.Boolean\True" };
                item.RuleObjectValue = new List<object>();
                item.RuleObjectValue.Add(true);
                newRuleValueList.Items = new List<RuleValue>();
                newRuleValueList.Items.Add(item);

                IACEntityProperty entityProperty = configValue as IACEntityProperty;
                ACPropertyExt acPropertyExt = entityProperty.ACProperties.Properties.Where(x => x.ACIdentifier == RuleValueList.ClassName).FirstOrDefault();
                if (acPropertyExt == null)
                {
                    acPropertyExt = new ACPropertyExt();
                    acPropertyExt.ACIdentifier = RuleValueList.ClassName;
                    acPropertyExt.ObjectType = typeof(RuleValueList);
                    acPropertyExt.AttachTo(entityProperty.ACProperties);
                    configValue.SetValueTypeACClass(Database.ContextIPlus.GetACType(RuleValueList.ClassName));
                    entityProperty.ACProperties.Properties.Add(acPropertyExt);
                }
                acPropertyExt.Value = newRuleValueList;
                entityProperty.ACProperties.Serialize();
            }
            else
            {
                //CurrentConfigStore.ConfigurationEntries.Remove(configValue);
                //if (configValue is IACObjectEntity)
                //{
                //    (configValue as VBEntityObject).DeleteACObject(Database, false);
                //}
                CurrentConfigStore.RemoveACConfig(configValue);
            }

            RefreshRuleStates();
        }

        public override void RefreshRuleStates()
        {
            _HasRules = null;
            OnPropertyChanged("HasRules");
            _ACState = null;
            OnPropertyChanged("ACState");
        }
        #endregion

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "SetBreakPoint":
                    SetBreakPoint();
                    return true;
                case "RemoveBreakPoint":
                    RemoveBreakPoint();
                    return true;
                case Const.IsEnabledPrefix + "SetBreakPoint":
                    result = IsEnabledSetBreakPoint();
                    return true;
                case Const.IsEnabledPrefix + "RemoveBreakPoint":
                    result = IsEnabledRemoveBreakPoint();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
        #endregion

        #region Context-Menu

        public override ACMenuItemList GetMenu(string vbContent, string vbControl)
        {
            var menuList = base.GetMenu(vbContent, vbControl);

            if (this.ContentACClassWF == null || this.HasRules != 1)
                return menuList;

            var items = CurrentConfigStore.ConfigurationEntries.Where(c => c.ACClassWFID.HasValue && c.ACClassWFID.Value == this.ContentACClassWF.ACClassWFID).ToArray();
            if (items.Any())
            {
                bool? isEnabled = ACUrlCommand("\\Businessobjects\\BSOChangeLog!IsEnabledLogForWFConfig", items) as bool?;
                if (isEnabled.HasValue && isEnabled.Value)
                {
                    var acclassText = this.ComponentClass.GetText("ChangeLogMenuText");
                    string menuCaption;

                    if (acclassText != null)
                        menuCaption = string.Format(acclassText.ACCaption, this.ACIdentifier);
                    else
                        menuCaption = string.Format("Show change log for {0}", this.ACIdentifier);

                    ACValueList param = new ACValueList();
                    param.Add(new ACValue("EntityKeys", items));

                    menuList.Add(new ACMenuItem(null, menuCaption, "\\Businessobjects\\BSOChangeLog!ShowChangeLogForWFConfig", 200, param));
                }
            }

            return menuList;
        }

        #endregion

        #region Override

        public override string ToString()
        {
            return ACCaption;
        }

        #endregion
    }
}
