using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.Data.Objects;
using System.Xml;
using System.IO;
using System.Runtime.Serialization;


namespace gip.core.autocomponent
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'PWNodeProxy'}de{'PWNodeProxy'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class PWNodeProxy : ACComponentProxy, IACComponentPWNode
    {
        #region private Members
        #endregion

        #region c´tors
        public PWNodeProxy(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        protected override void Construct(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
        {
            base.Construct(acType, content, parentACObject, parameter, acIdentifier);
            if (content is ACClassWF)
            {
                _ContentACClassWF = content as ACClassWF;
            }
            else if (content is ACClassTask)
            {
                ACClassTask acClassTask = content as ACClassTask;
                _ContentACClassWF = acClassTask.ContentACClassWF;
            }
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            return true;
        }

        /// <summary>
        /// This method is called inside the Construct-Method. Derivations can have influence to the naming of the instance by changing the acIdentifier-Parameter.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <param name="acIdentifier">The ac identifier.</param>
        protected override void InitACIdentifier(ACValueList parameter, ref string acIdentifier)
        {
            base.InitACIdentifier(parameter, ref acIdentifier);
            if (String.IsNullOrEmpty(acIdentifier))
            {
                if (ContentACClassWF != null)
                    acIdentifier = ContentACClassWF.ACIdentifier;
            }
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            bool result = base.ACDeInit(deleteACClassTask);
            _ContentACClassWF = null;

            using (ACMonitor.Lock(_20015_LockValue))
            {
                _MandatoryConfigStores = null;
            }
            return result;
        }
        #endregion

        #region Methods
        internal override void FinalizeComponent()
        {
            base.FinalizeComponent();
        }
        #endregion

        #region IACComponentWorkflow
        /// <summary>The primary IACObject that IACComponent encapsulates.</summary>
        /// <value>Content of type IACObject</value>
        public override IACObject Content
        {
            get
            {
                return _Content;
            }
            set
            {
                if (_Content != value)
                {
                    if (value == null)
                        _ContentACClassWF = null;
                    else if (value is ACClassWF)
                    {
                        _ContentACClassWF = value as ACClassWF;
                    }
                    else if (value is ACClassTask)
                    {
                        ACClassTask acClassTask = value as ACClassTask;
                        _ContentACClassWF = acClassTask.ContentACClassWF;
                    }
                }
                _Content = value;
                OnPropertyChanged("Content");
            }
        }

        ACClassWF _ContentACClassWF;
        /// <summary>Reference to the definition of this Workflownode.</summary>
        /// <value>Reference to the definition of this Workflownode.</value>
        public ACClassWF ContentACClassWF
        {
            get
            {
                if (_ContentACClassWF == null && Content != null)
                {
                    if (Content is ACClassWF)
                        _ContentACClassWF = Content as ACClassWF;
                    else if (Content is ACClassTask)
                    {
                        ACClassTask acClassTask = Content as ACClassTask;
                        _ContentACClassWF = acClassTask.ContentACClassWF;
                    }
                }
                return _ContentACClassWF;
            }
        }

        public ACMonitorObject ContextLockForACClassWF
        {
            get
            {
                return (Content == null || Content is ACClassWF) ?
                      gip.core.datamodel.Database.GlobalDatabase.QueryLock_1X000
                    : ACClassTaskQueue.TaskQueue.Context.QueryLock_1X000;
            }
        }

        #endregion

        #region IACObject
        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        public override string ACCaption
        {
            get
            {
                if (ContentACClassWF == null)
                    return ACType.ACCaption;
                return ContentACClassWF.ACCaption;
            }
        }

        #endregion

        #region Diagnostics and Dump
        protected override void DumpPropertyList(XmlDocument doc, XmlElement xmlACPropertyList)
        {
            base.DumpPropertyList(doc, xmlACPropertyList);

            XmlElement xmlContentACClassWF = xmlACPropertyList["ContentACClassWF"];
            if (xmlContentACClassWF == null)
            {
                xmlContentACClassWF = doc.CreateElement("ContentACClassWF");
                if (ContentACClassWF != null)
                    xmlContentACClassWF.InnerText = ACConvert.ObjectToXML(ContentACClassWF, true, true);
                xmlACPropertyList.AppendChild(xmlContentACClassWF);
            }
        }
        #endregion

        #region IACComponentWorkflow
        /// <summary>
        /// XAML-Code for Presentation
        /// </summary>
        /// <value>
        /// XAML-Code for Presentation
        /// </value>
        public string XMLDesign
        {
            get
            {
                if (_ContentACClassWF == null)
                    return "";
                _ContentACClassWF.ACClassMethod.AutoRefresh();
                return _ContentACClassWF.ACClassMethod.XMLDesign;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Root-Workflownode
        /// </summary>
        /// <value>Root-Workflownode</value>
        public IACComponentPWNode ParentRootWFNode
        {
            get
            {
                Type typeMethodBase = typeof(PWProcessFunction);
                ACClass acTypeMethodBase = ComponentClass.Database.GetACType(typeMethodBase);
                if (acTypeMethodBase == null)
                    return null;
                return FindParentComponent(acTypeMethodBase) as IACComponentPWNode;
            }
        }

        public IACComponentPWNode RootPW
        {
            get
            {
                Type typeMethodBase = typeof(PWProcessFunction);
                ACClass acTypeMethodBase = ComponentClass.Database.GetACType(typeMethodBase);
                if (acTypeMethodBase == null)
                    return null;
                if (ComponentClass.IsDerivedClassFrom(acTypeMethodBase))
                    return this;
                return ParentRootWFNode;
            }
        }

        public IACWorkflowContext WFContext
        {
            get
            {
                if (this.ContentTask == null)
                    return null;

                ACProgram acProgram = null;
                ACClassTaskQueue.TaskQueue.ProcessAction(() => { acProgram = ContentTask.ACProgram; });
                if (acProgram != null)
                    return acProgram;

                if (ContentACClassWF == null)
                    return null;

                using (ACMonitor.Lock(this.ContextLockForACClassWF))
                {
                    return ContentACClassWF.ACClassMethod;
                }
            }
        }

#endregion

#region IACConfigURL

        public string ConfigACUrl
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

        public string PreValueACUrl
        {
            get
            {
                PWNodeProxy pWNodeProxy = this;
                while (pWNodeProxy != null)
                {
                    if (pWNodeProxy.ContentACClassWF.ACClassMethod.RootWFNode == pWNodeProxy.ContentACClassWF)
                    {
                        string preACUrl = "";
                        PWNodeProxy pwNodeMethod2 = pWNodeProxy.ParentACObject as PWNodeProxy;
                        while (pwNodeMethod2 != null)
                        {
                            preACUrl = pwNodeMethod2.ACIdentifier + "\\" + preACUrl;
                            pwNodeMethod2 = pwNodeMethod2.ParentACObject as PWNodeProxy;
                        }
                        return preACUrl;
                    }
                    pWNodeProxy = pWNodeProxy.ParentACObject as PWNodeProxy;
                }
                return "";
            }
        }

#endregion

#region IACConfigMethodHierarchy



        /// <summary>
        ///  This should be not implemented
        /// </summary>
        public List<ACClassMethod> ACConfigMethodHierarchy
        {
            get { return new List<ACClassMethod>(); }
        }

#endregion

#region IACConfigStoreSelection

        List<IACConfigStore> _MandatoryConfigStores = null;
        public List<IACConfigStore> MandatoryConfigStores
        {
            get
            {

                using (ACMonitor.Lock(_20015_LockValue))
                {
                    if (_MandatoryConfigStores != null)
                        return _MandatoryConfigStores;
                }

                PWNodeProxy rootPWProxy = RootPW as PWNodeProxy;
                List<IACConfigStore> mandatoryConfigStores;
                if (rootPWProxy == null)
                {
                    mandatoryConfigStores = new List<IACConfigStore>();

                using (ACMonitor.Lock(_20015_LockValue))
                    {
                        if (_MandatoryConfigStores == null && mandatoryConfigStores != null)
                            _MandatoryConfigStores = mandatoryConfigStores;
                        return _MandatoryConfigStores;
                    }
                }
                if (rootPWProxy == this)
                {
                    List<ACConfigStoreInfo> rmiResult = RMInvoker.ExecuteMethod("GetSerializedMandatoryConfigStores", new Object[] { }) as List<ACConfigStoreInfo>;
                    ConfigManagerIPlus serviceInstance = ConfigManagerIPlus.GetServiceInstance(this);


                    using (ACMonitor.Lock(this.Database.QueryLock_1X000))
                    {
                        mandatoryConfigStores = serviceInstance.DeserializeMandatoryConfigStores(this.Database, rmiResult);
                    }


                    using (ACMonitor.Lock(_20015_LockValue))
                    {
                        if (_MandatoryConfigStores == null && mandatoryConfigStores != null)
                            _MandatoryConfigStores = mandatoryConfigStores;
                        return _MandatoryConfigStores;
                    }
                }
                else
                    return rootPWProxy.MandatoryConfigStores;
            }
        }

        public IACConfigStore CurrentConfigStore
        {
            get 
            {
                return MandatoryConfigStores.LastOrDefault();
                //return _MandatoryConfigStores.FirstOrDefault(); 
            }
        }

        public bool IsReadonly
        {
            get { return true; }
        }

#endregion


        public void RefreshRuleStates()
        {
        }
    }
}
