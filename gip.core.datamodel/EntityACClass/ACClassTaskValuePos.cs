// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 11-09-2012
// ***********************************************************************
// <copyright file="ACClassTaskValuePos.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;

namespace gip.core.datamodel
{
    /// <summary>
    /// ACClassTaskValuePos stores the values of points (Relationships between components)
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Taskvalue'}de{'Taskwert'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true)]
    [ACPropertyEntity(1, "ACClassTaskValue", "en{'Value'}de{'Wert'}", Const.ContextDatabaseIPlus + "\\ACClassTaskValue", "", true)]
    [ACPropertyEntity(2, "ACUrl", "en{'ACUrl'}de{'ACUrl'}","", "", true)]
    [ACPropertyEntity(3, "StateIndex", "en{'State'}de{'Status'}", "", "", false)]
    [ACPropertyEntity(4, "SequenceNo", "en{'SequenceNo'}de{'Sequencenr'}","", "", true)]
    [ACPropertyEntity(5, "ClientPointName", "en{'Clientpointname'}de{'Clientpunktname'}","", "", true)]
    [ACPropertyEntity(6, "AsyncCallbackDelegateName", "en{'Callback Delegate'}de{'Callback Delegate'}","", "", true)]
    [ACPropertyEntity(7, "ACRequestID", "en{'Request Id'}de{'Anfrage Id'}","", "", true)]
    [ACPropertyEntity(8, Const.ACIdentifierPrefix, "en{'Identifier'}de{'Identifizierer'}", "", "", true)]
    [ACPropertyEntity(8, "ExecutingInstance", "en{'Executing Instance'}de{'Ausführende Instanz'}","", "", true)]
    [ACPropertyEntity(9, "CallbackIsPending", "en{'Callback IsPending'}de{'Rückruf steht an'}","", "", true)]
    [ACPropertyEntity(9999, "XMLACMethod", "en{'Method XML'}de{'Methoden XML'}")]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + ACClassTaskValuePos.ClassName, "en{'Propertyvalue'}de{'Eigenschaftswert'}", typeof(ACClassTaskValuePos), ACClassTaskValuePos.ClassName, ACClassTaskValue.ClassName + "\\" + ACClassProperty.ClassName + "\\" + Const.ACIdentifierPrefix, ACClassTaskValue.ClassName + "\\" + ACClassProperty.ClassName + "\\" + Const.ACIdentifierPrefix)]
    [ACSerializeableInfo(new Type[] { typeof(ACRef<ACClassTaskValuePos>) })]
    public partial class ACClassTaskValuePos : IACObjectEntity, IACTask
    {
        public const string ClassName = "ACClassTaskValuePos";

        #region New/Delete
        /// <summary>
        /// News the AC object.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="parentACObject">The parent AC object.</param>
        /// <returns>ACClassTaskValuePos.</returns>
        public static ACClassTaskValuePos NewACObject(Database database, IACObject parentACObject)
        {
            ACClassTaskValuePos entity = new ACClassTaskValuePos();
            entity.ACClassTaskValuePosID = Guid.NewGuid();
            entity.XMLACMethod = "";
            entity.State = PointProcessingState.NewEntry;
            entity.InProcess = false;

            entity.DefaultValuesACObject();
            if (parentACObject is ACClassTaskValue)
            {
                using (ACMonitor.Lock(database.QueryLock_1X000))
                {
                    entity.ACClassTaskValue = parentACObject as ACClassTaskValue;
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
                var context = this.GetObjectContext();
                if (context != null)
                {
                    using (ACMonitor.Lock(context.QueryLock_1X000))
                    {
                        return ACClassTaskValue.ACClassProperty.ACCaption;
                    }
                }
                return ACClassTaskValue.ACClassProperty.ACCaption;
            }
        }

        /// <summary>
        /// Returns ACClassTaskValue
        /// NOT THREAD-SAFE USE QueryLock_1X000!
        /// </summary>
        /// <value>Reference to ACClassTaskValue</value>
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
                        return ACClassTaskValue;
                    }
                }
                return ACClassTaskValue;
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
            if (ACClassTaskValueID == Guid.Empty)
            {
                List<Msg> messages = new List<Msg>();
                messages.Add(new Msg
                {
                    Source = GetACUrl(),
                    ACIdentifier = "ACClassTaskValue",
                    Message = "ACClassTaskValue is null",
                    //Message = Database.Root.Environment.TranslateMessage(this, "Error50000", "ACClassTaskValue"), 
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
                return "ACClassTaskValue\\ACClassProperty\\ACIdentifier";
            }
        }

        #endregion

        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <value>The state.</value>
        public PointProcessingState State
        {
            get
            {
                return (PointProcessingState)StateIndex;
            }
            set
            {
                StateIndex = (short)value;
            }
        }

        /// <summary>
        /// Gets the sequence no.
        /// </summary>
        /// <value>The sequence no.</value>
        ulong IACPointEntry.SequenceNo
        {
            get
            {
                return Convert.ToUInt64(this.SequenceNo);
            }
        }

        /// <summary>
        /// Gets a value indicating whether [auto remove].
        /// </summary>
        /// <value><c>true</c> if [auto remove]; otherwise, <c>false</c>.</value>
        public bool AutoRemove
        {
            get 
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the AC method.
        /// </summary>
        /// <value>The AC method.</value>
        public ACMethod ACMethod
        {
            get 
            {
                return ACClassMethod.DeserializeACMethod(this.XMLACMethod);
            }
        }

        /// <summary>
        /// Gets the parameter.
        /// </summary>
        /// <value>The parameter.</value>
        public ACValueList Parameter
        {
            get 
            {
                ACMethod acMethod = ACMethod;
                if (acMethod == null)
                    return null;
                return acMethod.ParameterValueList; 
            }
        }


        /// <summary>
        /// Gets the executing instance.
        /// </summary>
        /// <value>The executing instance.</value>
        /// <exception cref="System.NotImplementedException"></exception>
        public ACRef<IACComponent> ExecutingInstance
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the workflow context.
        /// </summary>
        /// <value>The workflow context.</value>
        /// <exception cref="System.NotImplementedException"></exception>
        public IACWorkflowContext WorkflowContext
        {
            get { throw new NotImplementedException(); }
        }
    }

    /// <summary>
    /// Class ACPointTaskEqualityComparer
    /// </summary>
    public class ACPointTaskEqualityComparer : IEqualityComparer<IACTask>
    {
        /// <summary>
        /// Equalses the specified task1.
        /// </summary>
        /// <param name="task1">The task1.</param>
        /// <param name="task2">The task2.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        public bool Equals(IACTask task1, IACTask task2)
        {
            if (task1.RequestID == task2.RequestID)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public int GetHashCode(IACTask task)
        {
            return task.RequestID.GetHashCode();
        }
    }

}
