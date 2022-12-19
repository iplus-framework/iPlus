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
using System.ComponentModel.DataAnnotations.Schema;

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
    [NotMapped]
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
            if (parentACObject != null && parentACObject is ACClassTask)
            {
                using (ACMonitor.Lock(database.QueryLock_1X000))
                {
                    entity.ACClassTask = parentACObject as ACClassTask;
                }
            }
            entity.SetInsertAndUpdateInfo(database.UserName, database);
            return entity;
        }

        /// <summary>
        /// Creates a new ACClassTask-Entry
        /// </summary>
        /// <param name="database"></param>
        /// <param name="parentACObject"></param>
        /// <param name="acClassProperty"></param>
        /// <returns></returns>
        public static ACClassTaskValue NewACClassTaskValue(Database database, IACObject parentACObject, IACType acClassProperty)
        {
            ACClassTaskValue entity = ACClassTaskValue.NewACObject(database, parentACObject);
            if (acClassProperty != null)
            {
                using (ACMonitor.Lock(database.QueryLock_1X000))
                {
                    entity.ACClassProperty = acClassProperty as ACClassProperty;
                }
            }
            entity.EntityCheckAdded(database.UserName, database);
            return entity;
        }

        [NotMapped]
        ACClassProperty _NewACClassPropertyForQueue;
        [NotMapped]
        public ACClassProperty NewACClassPropertyForQueue
        {
            get
            {
                return _NewACClassPropertyForQueue;
            }
            set
            {
                _NewACClassPropertyForQueue = value;
            }
        }

        [NotMapped]
        ACClassTask _NewACClassTaskForQueue;
        [NotMapped]
        public ACClassTask NewACClassTaskForQueue
        {
            get
            {
                return _NewACClassTaskForQueue;
            }
            set
            {
                _NewACClassTaskForQueue = value;
            }
        }

        public void PublishToChangeTrackerInQueue()
        {
            this.ACClassProperty = NewACClassPropertyForQueue;
            this.ACClassTask = NewACClassTaskForQueue;
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
                var context = this.GetObjectContext();
                if (context != null)
                {
                    using (ACMonitor.Lock(context.QueryLock_1X000))
                    {
                        return ACClassProperty.ACCaption;
                    }
                }
                return ACClassProperty.ACCaption;
            }
        }

        /// <summary>
        /// Returns ACClassProperty
        /// NOT THREAD-SAFE USE QueryLock_1X000!
        /// </summary>
        /// <value>Reference to ACClassProperty</value>
        [ACPropertyInfo(9999)]
        [NotMapped]
        public override IACObject ParentACObject
        {
            get
            {
                var context = this.GetObjectContext();
                if (context != null)
                {
                    using (ACMonitor.Lock(context.QueryLock_1X000))
                    {
                        return ACClassProperty;
                    }
                }
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
            if ((this.ACClassTaskID == Guid.Empty) || (ACClassPropertyID == Guid.Empty))
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
        [NotMapped]
        static public string KeyACIdentifier
        {
            get
            {
                return "ACClassProperty\\ACIdentifier";
            }
        }

        #endregion

        #region RWLock
        [NotMapped]
        private object _RWLock = new object();
        [NotMapped]
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
