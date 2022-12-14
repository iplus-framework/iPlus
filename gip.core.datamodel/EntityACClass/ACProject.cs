// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 12-14-2012
// ***********************************************************************
// <copyright file="ACProject.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace gip.core.datamodel
{
    /// <summary>
    /// ACProject represents the root of a iPlus-Application
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Project'}de{'Projekt'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true)]
    [ACPropertyEntity(1, "ACProjectName", "en{'Projectname'}de{'Projektname'}","", "", true)]
    [ACPropertyEntity(2, "ACProjectTypeIndex", "en{'Projecttype'}de{'Projecttype'}", typeof(Global.ACProjectTypes), "", "", true)]
    [ACPropertyEntity(3, "ACProject1_BasedOnACProject", "en{'Baseproject'}de{'Basisprojekt'}", Const.ContextDatabaseIPlus + "\\" + ACProject.ClassName, "", true)]
    [ACPropertyEntity(4, "IsEnabled", "en{'Enabled'}de{'Aktiv'}","", "", true)]
    [ACPropertyEntity(5, "IsGlobal", "en{'Global'}de{'Global'}","", "", true)]
    [ACPropertyEntity(6, "IsWorkflowEnabled", "en{'Workflow'}de{'Workflow'}","", "", true)]
    [ACPropertyEntity(7, "IsControlCenterEnabled", "en{'Controlcenter'}de{'Steuerungszentrale'}","", "", true)]
    [ACPropertyEntity(8, "IsVisualisationEnabled", "en{'Visualisation'}de{'Visualisierung'}","", "", true)]
    [ACPropertyEntity(9, "IsProduction", "en{'Production'}de{'Produktion'}","", "", true)]
    [ACPropertyEntity(10, "IsDataAccess", "en{'Dataaccess'}de{'Dataaccess'}","", "", true)]
    [ACPropertyEntity(11, "Comment", "en{'Comment'}de{'Bemerkung'}","", "", true)]
    [ACPropertyEntity(12, "ACProjectNo", "en{'Projectnumber'}de{'Projektnummer'}","", "", true)]
    [ACPropertyEntity(9999, "BasedOnACProject", "en{'Based on Project'}de{'Basiert auf Projekt'}", Const.ContextDatabaseIPlus + "\\" + ACProject.ClassName, "", true)]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + ACProject.ClassName, "en{'Project'}de{'Projekt'}", typeof(ACProject), ACProject.ClassName, "ACProjectName", "ACProjectTypeIndex,ACProjectName", new object[]
        {
            new object[] {Const.QueryPrefix + ACClass.ClassName, "en{'Class'}de{'Klasse'}", typeof(ACClass), ACClass.ClassName + "_" + ACProject.ClassName, Const.ACCaptionPrefix + "," + Const.ACIdentifierPrefix, Const.ACIdentifierPrefix, new object[]
                {
                    new object[] {Const.QueryPrefix + ACClassProperty.ClassName, "en{'Property'}de{'Eigenschaft'}", typeof(ACClassProperty), ACClassProperty.ClassName + "_" + ACClass.ClassName, Const.ACIdentifierPrefix, Const.ACIdentifierPrefix},
                    new object[] {Const.QueryPrefix + ACClassMethod.ClassName, "en{'Method'}de{'Methode'}", typeof(ACClassMethod), ACClassMethod.ClassName + "_" + ACClass.ClassName, Const.ACIdentifierPrefix, Const.ACIdentifierPrefix, new object[]
                        {
                            new object[] {Const.QueryPrefix + ACClassWF.ClassName, "en{'Workflow'}de{'Workflow'}", typeof(ACClassWF), ACClassWF.ClassName + "_" + ACClassMethod.ClassName, Const.ACCaptionPrefix, Const.ACIdentifierPrefix},
                            new object[] {Const.QueryPrefix + ACClassWFEdge.ClassName, "en{'Workflowedge'}de{'Workflowbeziehung'}", typeof(ACClassWFEdge), ACClassWFEdge.ClassName + "_" + ACClassMethod.ClassName, "", Const.ACIdentifierPrefix},
                        },
                    },
                    new object[] {Const.QueryPrefix + ACClassDesign.ClassName, "en{'Design'}de{'Design'}", typeof(ACClassDesign), ACClassDesign.ClassName + "_" + ACClass.ClassName, Const.ACIdentifierPrefix, Const.ACIdentifierPrefix},
                    new object[] {Const.QueryPrefix + ACClassConfig.ClassName, "en{'Classconfig'}de{'Klassenkonfiguration'}", typeof(ACClassConfig), ACClassConfig.ClassName + "_" + ACClass.ClassName, Const.PN_LocalConfigACUrl, Const.PN_LocalConfigACUrl},
                    new object[] {Const.QueryPrefix + ACClassPropertyRelation.ClassName, "en{'Propertyrelation'}de{'Eigenschaftsbeziehung'}", typeof(ACClassPropertyRelation), ACClassPropertyRelation.ClassName + "_SourceACClass", "", ACClassPropertyRelation.ClassName + "ID"},
                    new object[] {Const.QueryPrefix + ACClassMessage.ClassName, "en{'Message'}de{'Meldung'}", typeof(ACClassMessage), ACClassMessage.ClassName + "_" + ACClass.ClassName, Const.ACIdentifierPrefix, Const.ACIdentifierPrefix},
                    new object[] {Const.QueryPrefix + ACClassText.ClassName, "en{'Text'}de{'Text'}", typeof(ACClassText), ACClassText.ClassName + "_" + ACClass.ClassName, Const.ACIdentifierPrefix, Const.ACIdentifierPrefix}
                }
            }
        }
    )]
    [ACSerializeableInfo(new Type[] { typeof(ACRef<ACProject>) })]
    public partial class ACProject : IACClassEntity
    {
        public const string ClassName = "ACProject";
        public const string NoColumnName = "ProjectNo";
        public const string FormatNewNo = null;

        /// <summary>
        /// Delegate ACProjectStateChangedEventHandler
        /// </summary>
        public delegate void ACProjectStateChangedEventHandler();

        #region New/Delete
        /// <summary>
        /// News the AC object.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="parentACObject">The parent AC object.</param>
        /// <param name="secondaryKey"></param>
        /// <returns>ACProject</returns>
        public static ACProject NewACObject(Database database, IACObject parentACObject, string secondaryKey)
        {
            ACProject entity = new ACProject();
            entity.Database = database;
            entity.ACProjectID = Guid.NewGuid();
            entity.DefaultValuesACObject();
            entity.ACProjectType = Global.ACProjectTypes.Application;
            entity.ACProjectNo = secondaryKey;
            entity.ACProjectName = "<TODO>";
            entity.Comment = "";
            entity.IsEnabled = false;
            entity.IsGlobal = false;
            entity.IsVisualisationEnabled = false;
            entity.IsWorkflowEnabled = false;
            entity.IsControlCenterEnabled = false;
            entity.IsDataAccess = false;
            entity.SetInsertAndUpdateInfo(database.UserName, database);
            return entity;
        }

        /// <summary>
        /// Deletes this entity-object from the database
        /// UNSAFE, use (QueryLock_1X000)
        /// </summary>
        /// <param name="database">Entity-Framework databasecontext</param>
        /// <param name="withCheck">If set to true, a validation happens before deleting this EF-object. If Validation fails message ís returned.</param>
        /// <param name="softDelete">If set to true a delete-Flag is set in the dabase-table instead of a physical deletion. If  a delete-Flag doesn't exit in the table the record will be deleted.</param>
        /// <returns>If a validation or deletion failed a message is returned. NULL if sucessful.</returns>
        public override MsgWithDetails DeleteACObject(IACEntityObjectContext database, bool withCheck, bool softDelete = false)
        {
            if (withCheck)
            {
                MsgWithDetails msg = IsEnabledDeleteACObject(database);
                if (msg != null)
                    return msg;
            }

            foreach (var acClass in this.ACClass_ACProject.ToList())
            {
                acClass.DeleteACObject(database, withCheck);
            }

            database.Remove(this);
            return null;
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
                return ACProjectNo + " " + ACProjectName;
            }
        }

        /// <summary>
        /// Returns a related EF-Object which is in a Child-Relationship to this.
        /// </summary>
        /// <param name="className">Classname of the Table/EF-Object</param>
        /// <param name="filterValues">Search-Parameters</param>
        /// <returns>A Entity-Object as IACObject</returns>
        public override IACObject GetChildEntityObject(string className, params string[] filterValues)
        {
            if (filterValues.Any() && className == ACClass.ClassName)
            {

                using (ACMonitor.Lock(Database.QueryLock_1X000))
                {
                    return ACClass_ACProject.Where(c => c.ACClass1_ParentACClass == null && c.ACIdentifier == filterValues[0]).FirstOrDefault();
                }
            }
            return null;
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
            if (string.IsNullOrEmpty(ACProjectName))
            {
                List<Msg> messages = new List<Msg>();
                messages.Add(new Msg
                {
                    Source = GetACUrl(),
                    ACIdentifier = "ACProjectName",
                    Message = "ACProjectName is null",
                    //Message = Database.Root.Environment.TranslateMessage(this, "Error50000", "ACPackageName"), 
                    MessageLevel = eMsgLevel.Error
                });
                return messages;
            }
            base.EntityCheckAdded(user, context);
            return null;
        }

        /// <summary>
        /// Gets the key AC identifier.
        /// </summary>
        /// <value>The key AC identifier.</value>
        [NotMapped]
        static public string KeyACIdentifier
        {
            get
            {
                return "ACProjectName";
            }
        }
#endregion

        /// <summary>
        /// Gets the root class.
        /// </summary>
        /// <value>The root class.</value>
        [NotMapped]
        public ACClass RootClass
        {
            get
            {

                using (ACMonitor.Lock(Database.QueryLock_1X000))
                {
                    return ACClass_ACProject.FirstOrDefault(c => c.ACClass1_ParentACClass == null);
                }
            }
        }

        /// <summary>
        /// Gets or sets the type of the AC project.
        /// </summary>
        /// <value>The type of the AC project.</value>
        [ACPropertyInfo(9999)]
        [NotMapped]
        public Global.ACProjectTypes ACProjectType
        {
            get
            {
                return (Global.ACProjectTypes)ACProjectTypeIndex;
            }
            set
            {
                ACProjectTypeIndex = (short)value;
            }
        }

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


        public Database Database { get; private set; } = null;

        void IACClassEntity.OnObjectMaterialized(Database db)
        {
            if (Database == null)
                Database = db;
        }

    }
}
