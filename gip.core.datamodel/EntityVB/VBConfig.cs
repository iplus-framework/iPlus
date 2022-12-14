using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using gipCoreData = gip.core.datamodel;

namespace gip.core.datamodel
{
    /// <summary>
    /// VBConfig
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Config'}de{'Konfiguration'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true)]
    [ACPropertyEntity(1, Const.PN_PreConfigACUrl, "en{'Parent WF URL'}de{'WF Eltern-URL'}","", "", true)]
    [ACPropertyEntity(2, Const.PN_LocalConfigACUrl, "en{'Property URL'}de{'Eigenschafts-URL'}","", "", true)]
    [ACPropertyEntity(3, "XMLValue", "en{'Value'}de{'Wert'}")]
    [ACPropertyEntity(4, "ValueTypeACClass", "en{'Datatype'}de{'Datentyp'}", Const.ContextDatabase + "\\" + ACClass.ClassName, "", true)]
    [ACPropertyEntity(5, "Expression", "en{'Expression'}de{'Ausdruck'}","", "", true)]
    [ACPropertyEntity(6, "Comment", "en{'Comment'}de{'Bemerkung'}","", "", true)]
    [ACPropertyEntity(100, Const.PN_KeyACUrl, "en{'Key'}de{'Schlüssel'}","", "", true)]
    [ACDeleteAction("VBConfig_ParentVBConfig", Global.DeleteAction.CascadeManual)]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + VBConfig.ClassName, "en{'Config'}de{'Konfiguration'}", typeof(VBConfig), VBConfig.ClassName, Const.PN_LocalConfigACUrl, Const.PN_LocalConfigACUrl)]
    [ACSerializeableInfo(new Type[] { typeof(ACRef<VBConfig>) })]
    public partial class VBConfig : IACConfig
    {
        public const string ClassName = "VBConfig";

        #region New/Delete
        public static VBConfig NewACObject(Database database, IACObject parentACObject)
        {
            VBConfig entity = new VBConfig();
            entity.VBConfigID = Guid.NewGuid();
            entity.DefaultValuesACObject();
            entity.XMLConfig = "";
            entity.SetInsertAndUpdateInfo(database.UserName, database);
            return entity;
        }

        #endregion

        #region IACUrl Member

        public override string ToString()
        {
            return ACCaption;
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [ACPropertyInfo(9999, "", "en{'Description'}de{'Bezeichnung'}")]
        [NotMapped]
        public override string ACCaption
        {
            get
            {
                if (this.Value is IACObject)
                {
                    return (this.Value as IACObject).ACCaption;
                }
                return (PreConfigACUrl != null? PreConfigACUrl + @"\" : "") + LocalConfigACUrl;
            }
        }

        #endregion

        #region IACObjectEntity Members
        /// <summary>
        /// Method for validating values and references in this EF-Object.
        /// Is called from Change-Tracking before changes will be saved for new unsaved entity-objects.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="context">Entity-Framework databasecontext</param>
        /// <returns>NULL if sucessful otherwise a Message-List</returns>
        public override IList<Msg> EntityCheckAdded(string user, IACEntityObjectContext context)
        {
            if (ValueTypeACClass == null)
            {
                List<Msg> messages = new List<Msg>();
                messages.Add(new Msg
                {
                    Source = GetACUrl(),
                    ACIdentifier = "Key",
                    Message = "Key",
                    //Message = Database.Root.Environment.TranslateMessage(this, "Error50000", "Key"), 
                    MessageLevel = eMsgLevel.Error
                });
                return messages;
            }
            base.EntityCheckAdded(user, context);
            return null;
        }

        static public string KeyACIdentifier
        {
            get
            {
                return Const.ACUrlPrefix;
            }
        }
        #endregion

        #region IEntityProperty Members

        bool bRefreshConfig = false;
        protected override void OnPropertyChanging<T>(T newValue, string propertyName, bool afterChange)
        {
            if (propertyName == nameof(XMLConfig))
            {
                string xmlConfig = newValue as string;
                if (afterChange)
                {
                    if (bRefreshConfig)
                        ACProperties.Refresh();
                }
                else
                {
                    bRefreshConfig = false;
                    if (!(String.IsNullOrEmpty(xmlConfig) && String.IsNullOrEmpty(XMLConfig)) && xmlConfig != XMLConfig)
                        bRefreshConfig = true;
                }
            }
            base.OnPropertyChanging(newValue, propertyName, afterChange);
        }

        #endregion

        #region Properties
        #endregion

        #region IACConfig

        [ACPropertyInfo(101, Const.PN_ConfigACUrl, "en{'WF Property URL'}de{'WF Eigenschaft URL'}")]
        [NotMapped]
        public string ConfigACUrl
        {
            get
            {
                return (PreConfigACUrl != null ? PreConfigACUrl + @"\" : "") + LocalConfigACUrl;
            }
        }

        /// <summary>Gets or sets the encapsulated value as a boxed type</summary>
        /// <value>The boxed value.</value>
        [ACPropertyInfo(9999, "", "en{'Value'}de{'Wert'}")]
        [NotMapped]
        public object Value
        {
            get
            {
                ACPropertyExt acPropertyExt = ACProperties.GetOrCreateACPropertyExtByName(Const.Value, false, true);
                if (acPropertyExt == null)
                    return null;
                return acPropertyExt.Value;
            }
            set
            {
                ACPropertyExt acPropertyExt = ACProperties.GetOrCreateACPropertyExtByName(Const.Value, false, true);
                if ((acPropertyExt != null && acPropertyExt.Value != value)
                    || (acPropertyExt == null && value != null))
                    ACProperties.SetACPropertyExtValue(ACProperties.GetOrCreateACPropertyExtByName(Const.Value), value);
            }
        }

        [ACPropertyInfo(6, "", "en{'Source']de{'Quelle'}")]
        [NotMapped]
        public IACConfigStore ConfigStore
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>Sets the Metadata (iPlus-Type) of the Value-Property.</summary>
        /// <param name="typeOfValue">Metadata (iPlus-Type) of the Value-Property.</param>
        public void SetValueTypeACClass(ACClass typeOfValue)
        {
            this.ValueTypeACClass = typeOfValue;
        }

        /// <summary>VBConfig-Childs</summary>
        /// <value>VBConfig-Childs</value>
        public IEnumerable<IACContainerWithItems> Items
        {
            get 
            {
                return VBConfig_ParentVBConfig;
            }
        }

        /// <summary>Gets the parent container.</summary>
        /// <value>The parent container.</value>
        public IACContainerWithItems ParentContainer
        {
            get
            {
                return VBConfig1_ParentVBConfig;
            }
        }

        /// <summary>Gets the root container.</summary>
        /// <value>The root container.</value>
        public IACContainerWithItems RootContainer
        {
            get
            {
                if (VBConfig1_ParentVBConfig == null)
                    return this;
                return VBConfig1_ParentVBConfig.RootContainer;
            }
        }


        /// <summary>Adds the specified child-container</summary>
        /// <param name="child">The child-container</param>
        public void Add(IACContainerWithItems child)
        {
            if (child is VBConfig)
            {
                VBConfig vbConfig = child as VBConfig;
                vbConfig.VBConfig1_ParentVBConfig = this;
                VBConfig_ParentVBConfig.Add(vbConfig);
            }
        }

        /// <summary>Removes the specified child-container</summary>
        /// <param name="child">The child-container</param>
        /// <returns>true if removed</returns>
        public bool Remove(IACContainerWithItems child)
        {
            if (child is VBConfig)
            {
                return VBConfig_ParentVBConfig.Remove(child as VBConfig);
            }
            return false;
        }

        public gipCoreData.ACClass VBACClass
        {
            get
            {
                return null;
            }
        }

        public Guid? VBiACClassID { get; set; }

        public Guid? ACClassWFID
        {
            get
            {
                return null;
            }
        }

        #endregion
    }
}
