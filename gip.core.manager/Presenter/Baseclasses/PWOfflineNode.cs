using gip.core.datamodel;
using gip.core.autocomponent;
using System.Collections.Generic;

namespace gip.core.manager
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'PWOfflineNode'}de{'PWOfflineNode'}", Global.ACKinds.TACObject, Global.ACStorableTypes.NotStorable, true, true)]
    public abstract class PWOfflineNode : ACComponent, IACClassDesignProvider, IACComponentPWNode, IACObjectDesign
    {
        #region c´tors
        public PWOfflineNode(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="") :
            base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public abstract void CreateChildPWNode(IACWorkflowNode acWorkflowNode, Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic);

        #endregion

        #region IACObject
        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [ACPropertyInfo(9999)]
        public override string ACCaption
        {
            get { return Content.ACCaption; }
        }
        #endregion

        #region IACComponentPWNode
        /// <summary>
        /// XAML-Code for Presentation
        /// </summary>
        /// <value>
        /// XAML-Code for Presentation
        /// </value>
        public abstract string XMLDesign { get; set; }

        /// <summary>
        /// Root-Workflownode
        /// </summary>
        /// <value>Root-Workflownode</value>
        public abstract IACComponentPWNode ParentRootWFNode { get; }

        /// <summary>
        /// Returns the Workflow-Context (ACClassMethod, ACProgram, Partslist or MaterialWF) for reading and saving the configuration-data of a workflow.
        /// </summary>
        /// <value>The Workflow-Context</value>
        public abstract IACWorkflowContext WFContext { get; }


        /// <summary>Reference to the definition of this Workflownode.</summary>
        /// <value>Reference to the definition of this Workflownode.</value>
        public abstract ACClassWF ContentACClassWF { get; }

        #region IACConfigURL

        public virtual string ConfigACUrl
        {
            get
            {
                return "";
            }
        }

        public virtual string PreValueACUrl
        {
            get
            {
                return "";
            }
        }

        public string LocalConfigACUrl
        {
            get
            {
                return ACUrlHelper.BuildLocalConfigACUrl(this);
            }
        }

        public virtual void RefreshRuleStates()
        {

        }

        #endregion

        #region IACConfigMethodHierarchy

        public virtual List<ACClassMethod> ACConfigMethodHierarchy
        {
            get
            {
                return null;
            }
        }

        #endregion

        #region IACConfigStoreSelection

        public virtual List<IACConfigStore> MandatoryConfigStores
        {
            get
            {
                return null;
            }
        }

        public virtual IACConfigStore CurrentConfigStore
        {
            get
            {
                return null;
            }
        }

        public virtual bool IsReadonly
        {
            get
            {
                return false;
            }
        }

        #endregion
        #endregion
    }
}
