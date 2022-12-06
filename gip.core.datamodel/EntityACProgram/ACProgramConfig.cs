using System;
using System.Collections.Generic;
using System.Linq;

namespace gip.core.datamodel
{
    /// <summary>
    /// Config-Table of ACPrograms.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Program.config'}de{'Programm.config'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true)]
    [ACPropertyEntity(1, Const.PN_PreConfigACUrl, "en{'Parent WF URL'}de{'WF Eltern-URL'}","", "", true)]
    [ACPropertyEntity(2, Const.PN_LocalConfigACUrl, "en{'Property URL'}de{'Eigenschafts-URL'}","", "", true)]
    [ACPropertyEntity(3, "XMLValue", "en{'Value'}de{'Wert'}")]
    [ACPropertyEntity(4, "ValueTypeACClass", "en{'Datatype'}de{'Datentyp'}", Const.ContextDatabase + "\\" + ACClass.ClassName, "", true)]
    [ACPropertyEntity(5, "Expression", "en{'Expression'}de{'Ausdruck'}","", "", true)]
    [ACPropertyEntity(6, "Comment", "en{'Comment'}de{'Bemerkung'}","", "", true)]
    [ACPropertyEntity(100, Const.PN_KeyACUrl, "en{'Key'}de{'Schlüssel'}","", "", true)]
    [ACDeleteAction("ACProgramConfig_ParentACProgramConfig", Global.DeleteAction.CascadeManual)]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + ACProgramConfig.ClassName, "en{'Program.config'}de{'Programm.config'}", typeof(ACProgramConfig), ACProgramConfig.ClassName, Const.PN_LocalConfigACUrl, Const.PN_LocalConfigACUrl)]
    [ACSerializeableInfo(new Type[] { typeof(ACRef<ACProgramConfig>) })]
    public partial class ACProgramConfig : IACConfig
    {
        public const string ClassName = "ACProgramConfig";

        #region New/Delete
        public static ACProgramConfig NewACObject(Database database, IACObject parentACObject)
        {
            ACProgramConfig entity = new ACProgramConfig();
            entity.ACProgramConfigID = Guid.NewGuid();
            entity.DefaultValuesACObject();
            entity.XMLConfig = "";
            if (parentACObject is ACProgram)
            {
                using (ACMonitor.Lock(database.QueryLock_1X000))
                {
                    entity.ACProgram = parentACObject as ACProgram;
                }
            }
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
        public override string ACCaption
        {
            get
            {
                if (this.Value is IACObject)
                {
                    return (this.Value as IACObject).ACCaption;
                }
                return this.LocalConfigACUrl;
            }
        }

        /// <summary>
        /// Returns ACProgram
        /// NOT THREAD-SAFE USE QueryLock_1X000!
        /// </summary>
        /// <value>Reference to ACProgram</value>
        [ACPropertyInfo(9999)]
        public override IACObject ParentACObject
        {
            get
            {
                var context = this.GetObjectContext();
                if (context != null)
                {
                    using (ACMonitor.Lock(context.QueryLock_1X000))
                    {
                        return ACProgram;
                    }
                }
                return ACProgram;
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
                    ACIdentifier = "ValueTypeACClass",
                    Message = "ValueTypeACClass is null",
                    //Message = Database.Root.Environment.TranslateMessage(this, "Error50000", "ValueTypeACClass"), 
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
        public IACConfigStore ConfigStore
        {
            get
            {
                return (IACConfigStore)ACProgram;
            }
        }

        /// <summary>Sets the Metadata (iPlus-Type) of the Value-Property.</summary>
        /// <param name="typeOfValue">Metadata (iPlus-Type) of the Value-Property.</param>
        public void SetValueTypeACClass(ACClass typeOfValue)
        {
            this.ValueTypeACClass = typeOfValue;
        }


        /// <summary>ACProgramConfig-Childs</summary>
        /// <value>ACProgramConfig-Childs</value>
        public IEnumerable<IACContainerWithItems> Items
        {
            get
            {
                return ACProgramConfig_ParentACProgramConfig;
            }
        }

        /// <summary>Gets the parent container.</summary>
        /// <value>The parent container.</value>
        public IACContainerWithItems ParentContainer
        {
            get
            {
                return ACProgramConfig1_ParentACProgramConfig;
            }
        }

        /// <summary>Gets the root container.</summary>
        /// <value>The root container.</value>
        public IACContainerWithItems RootContainer
        {
            get
            {
                if (ACProgramConfig1_ParentACProgramConfig == null)
                    return this;
                return ACProgramConfig1_ParentACProgramConfig.RootContainer;
            }
        }

        /// <summary>Adds the specified child-container</summary>
        /// <param name="child">The child-container</param>
        public void Add(IACContainerWithItems child)
        {
            if (child is ACProgramConfig)
            {
                ACProgramConfig acProgramConfig = child as ACProgramConfig;
                acProgramConfig.ACProgramConfig1_ParentACProgramConfig = this;
                ACProgramConfig_ParentACProgramConfig.Add(acProgramConfig);
            }
        }

        /// <summary>Removes the specified child-container</summary>
        /// <param name="child">The child-container</param>
        /// <returns>true if removed</returns>
        public bool Remove(IACContainerWithItems child)
        {
            if (child is ACProgramConfig)
            {
                return ACProgramConfig_ParentACProgramConfig.Remove(child as ACProgramConfig);
            }
            return false;
        }

        public ACClass VBACClass
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
