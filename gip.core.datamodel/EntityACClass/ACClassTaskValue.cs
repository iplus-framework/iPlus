// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ACClassTaskValue.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;

namespace gip.core.datamodel
{
    /// <summary>
    /// ACClassTaskValue stores the values of persistable properties.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Taskvalue'}de{'Taskwert'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true)]
    [ACPropertyEntity(1, ACClass.ClassName, "en{'Class'}de{'Klasse'}", Const.ContextDatabaseIPlus + "\\" + ACClass.ClassName, "", true)]
    [ACPropertyEntity(2, ACClassProperty.ClassName, "en{'Property'}de{'Eigenschaft'}", Const.ContextDatabaseIPlus + "\\" + ACClassProperty.ClassName, "", true)]
    [ACPropertyEntity(9999, "User", "en{'User'}de{'Benutzer'}", Const.ContextDatabaseIPlus + "\\VBUser", "", true)]
    [ACPropertyEntity(9999, "XMLValue2", "en{'Value XML2'}de{'Wert XML2'}")]
    [ACPropertyEntity(9999, "XMLValue", "en{'Value XML'}de{'Wert XML'}")]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + ACClassTaskValue.ClassName, "en{'Propertyvalue'}de{'Eigenschaftswert'}", typeof(ACClassTaskValue), ACClassTaskValue.ClassName, ACClassProperty.ClassName + "\\" + Const.ACIdentifierPrefix, ACClassProperty.ClassName + "\\" + Const.ACIdentifierPrefix)]
    [ACSerializeableInfo(new Type[] { typeof(ACRef<ACClassTaskValue>) })]
    public partial class ACClassTaskValue : IACObjectEntity
    {
        public const string ClassName = "ACClassTaskValue";

        #region New/Delete
        /// <summary>
        /// News the AC object.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="parentACObject">The parent AC object.</param>
        /// <returns>ACClassTaskValue.</returns>
        public static ACClassTaskValue NewACObject(Database database, IACObject parentACObject)
        {
            ACClassTaskValue entity = new ACClassTaskValue();
            entity.ACClassTaskValueID = Guid.NewGuid();
            entity.XMLValue = "";
            entity.XMLValue2 = "";
            entity.DefaultValuesACObject();
            if (parentACObject is ACClassTask)
            {
                entity.ACClassTask = parentACObject as ACClassTask;
            }
            entity.SetInsertAndUpdateInfo(Database.Initials, database);
            return entity;
        }

        /// <summary>
        /// News the AC class task value.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="acClass">The ac class.</param>
        /// <param name="acClassProperty">The ac class property.</param>
        /// <returns>ACClassTaskValue.</returns>
        public static ACClassTaskValue NewACClassTaskValue(Database database, IACObject parentACObject, IACType acClassProperty)
        {
            ACClassTaskValue entity = ACClassTaskValue.NewACObject(database, parentACObject);
            entity.ACClassProperty = acClassProperty as ACClassProperty;
            entity.EntityCheckAdded(Database.Initials, database);
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
                return ACClassProperty.ACCaption;
            }
        }

        /// <summary>
        /// Returns ACClassProperty
        /// NOT THREAD-SAFE USE QueryLock_1X000!
        /// </summary>
        /// <value>Reference to ACClassProperty</value>
        [ACPropertyInfo(9999)]
        public override IACObject ParentACObject
        {
            get
            {
                return ACClassProperty;
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
            if ((ACClassTask == null) || (ACClassProperty == null))
            {
                List<Msg> messages = new List<Msg>();
                messages.Add(new Msg
                {
                    Source = GetACUrl(),
                    ACIdentifier = "ACClassTask",
                    Message = "ACClassTask, ACClassProperty is null",
                    //Message = Database.Root.Environment.TranslateMessage(this, "Error50000", "ValueTypeACClass"), 
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
        static public string KeyACIdentifier
        {
            get
            {
                return "ACClassProperty\\ACIdentifier";
            }
        }

        #endregion

        #region RWLock
        private object _RWLock = new object();
        public string XMLValueRW
        {
            get
            {
                lock (_RWLock)
                    return XMLValue;
            }
            set
            {
                lock (_RWLock)
                    XMLValue = value;
            }
        }
        #endregion
    }
}
