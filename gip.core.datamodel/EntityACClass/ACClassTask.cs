// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 11-07-2012
// ***********************************************************************
// <copyright file="ACClassTask.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Linq;

namespace gip.core.datamodel
{
    /// <summary>
    /// ACClassTask table entries ensure that the state of all persistable instances in the application trees is persisted and that after the restart of the iPlus service, 
    /// the last state of the process (all object states before the last shutdown) can be restored again.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACComp.-Instance DB'}de{'ACComp.-Instanz DB'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true)]
    [ACPropertyEntity(1, "TaskTypeACClass", "en{'Class'}de{'Klasse'}", Const.ContextDatabaseIPlus + "\\" + ACClass.ClassName, "", true)]
    [ACPropertyEntity(2, Const.ACIdentifierPrefix, "en{'Task-ID'}de{'Task-ID'}", "", "", true)]
    [ACPropertyEntity(3, Const.ACState, "en{'State'}de{'Status'}", "", "", false)]
    [ACPropertyEntity(4, "IsTestmode", "en{'Testmode'}de{'Testmodus'}", "", "", false)]
    [ACPropertyEntity(5, "IsDynamic", "en{'Dynamic'}de{'Dynamisch'}", "", "", false)]
    [ACPropertyEntity(9999, "ACTaskTypeIndex", "en{'Index'}de{'Index'}", typeof(Global.ACTaskTypes), "", "", true)]
    [ACPropertyEntity(9999, "ParentACClassTask", "en{'Parent Task'}de{'Elterntask'}", Const.ContextDatabaseIPlus + "\\" + ACClassTask.ClassName, "", true)]
    [ACPropertyEntity(9999, "ContentACClassWF", "en{'Workflow Content'}de{'Workflowinhalt'}", Const.ContextDatabaseIPlus + "\\" + ACClassWF.ClassName, "", true)]
    [ACPropertyEntity(9999, ACProgram.ClassName, "en{'Program'}de{'Programm'}", Const.ContextDatabaseIPlus + "\\" + ACProgram.ClassName, "", true)]
    [ACDeleteAction("ACClassTask_ParentACClassTask", Global.DeleteAction.CascadeManual)]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + ACClassTask.ClassName, "en{'Task'}de{'Task'}", typeof(ACClassTask), ACClassTask.ClassName, Const.ACIdentifierPrefix, Const.ACIdentifierPrefix)]
    [ACSerializeableInfo(new Type[] { typeof(ACRef<ACClassTask>) })]
    public partial class ACClassTask
    {
        public const string ClassName = "ACClassTask";

        #region New/Delete
        /// <summary>
        /// News the AC object.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="parentACObject">The parent AC object.</param>
        /// <returns>ACClassTask.</returns>
        public static ACClassTask NewACObject(Database database, IACObject parentACObject)
        {
            ACClassTask entity = new ACClassTask();
            entity.ACClassTaskID = Guid.NewGuid();
            //entity.ACState = Global.ACStates.Offline;
            //entity.ACProcessPhase = Global.ACProcessPhases.Idle;
            entity.ACTaskType = Global.ACTaskTypes.MethodTask;
            entity.IsDynamic = true;
            entity.ACIdentifier = "";
            // Bei Systembelegung gibt es keine Vorbelegung, da hier kein Customizing erwünscht ist
            if (parentACObject != null && parentACObject is ACClassTask)
            {
                //using (ACMonitor.Lock(database.QueryLock_1X000))
                //{
                entity.ACClassTask1_ParentACClassTask = parentACObject as ACClassTask;
                entity.IsTestmode = (parentACObject as ACClassTask).IsTestmode;
                //}
            }
            //else
            //{
            //    entity.IsTestmode = false;
            //}

            entity.SetInsertAndUpdateInfo(database.UserName, database);
            return entity;
        }

        #region Fix referenced for avoiding Task-Queue-Collisions
        ACClassTask _NewParentACClassTaskForQueue;
        public ACClassTask NewParentACClassTaskForQueue
        {
            get
            {
                return _NewParentACClassTaskForQueue;
            }
            set
            {
                _NewParentACClassTaskForQueue = value;
                //ACClassTask1_ParentACClassTask = value;
            }
        }

        ACProgram _NewACProgramForQueue;
        public ACProgram NewACProgramForQueue
        {
            get
            {
                return _NewACProgramForQueue;
            }
            set
            {
                _NewACProgramForQueue = value;
                //ACProgram = value;
            }
        }

        ACClassWF _NewContentACClassWFForQueue;
        public ACClassWF NewContentACClassWFForQueue
        {
            get
            {
                return _NewContentACClassWFForQueue;
            }
            set
            {
                _NewContentACClassWFForQueue = value;
                //ContentACClassWF = value;
            }
        }

        ACClass _NewTaskTypeACClassForQueue;
        public ACClass NewTaskTypeACClassForQueue
        {
            get
            {
                return _NewTaskTypeACClassForQueue;
            }
            set
            {
                _NewTaskTypeACClassForQueue = value;
            }
        }

        public void PublishToChangeTrackerInQueue()
        {
            this.ACClassTask1_ParentACClassTask = _NewParentACClassTaskForQueue;
            this.ACProgram = _NewACProgramForQueue;
            this.ContentACClassWF = _NewContentACClassWFForQueue;
            this.TaskTypeACClass = _NewTaskTypeACClassForQueue;
        }
        #endregion

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
                return ACType.ACCaption;
            }
        }

        /// <summary>
        /// Returns ParentACClassTask
        /// NOT THREAD-SAFE USE QueryLock_1X000!
        /// </summary>
        /// <value>Reference to ParentACClassTask</value>
        [ACPropertyInfo(9999)]
        public override IACObject ParentACObject
        {
            get
            {
                ACClassTask parentTask = null;
                if (ACClassTask1_ParentACClassTaskReference.IsLoaded)
                    parentTask = (ACClassTask) ACClassTask1_ParentACClassTaskReference.CurrentValue;
                if (parentTask == null)
                    parentTask = NewParentACClassTaskForQueue;
                if (parentTask == null)
                {
                    var context = this.GetObjectContext();
                    if (context != null)
                    {
                        using (ACMonitor.Lock(context.QueryLock_1X000))
                        {
                            return ACClassTask1_ParentACClassTask;
                        }
                    }
                    else
                        parentTask = ACClassTask1_ParentACClassTask;
                }
                return parentTask;
            }
        }

        #endregion

        #region IACObjectEntity Members
        /// <summary>
        /// Gets the key AC identifier.
        /// </summary>
        /// <value>The key AC identifier.</value>
        static public string KeyACIdentifier
        {
            get
            {
                return Const.ACIdentifierPrefix;
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

        #region enum-Properties
        //public Global.ACStates ACState
        //{
        //    get
        //    {
        //        return (Global.ACStates)ACStateIndex;
        //    }
        //    set
        //    {
        //        ACStateIndex = (short)value;
        //    }
        //}

        //public Global.ACProcessPhases ACProcessPhase
        //{
        //    get
        //    {
        //        return (Global.ACProcessPhases)ACProcessPhaseIndex;
        //    }
        //    set
        //    {
        //        ACProcessPhaseIndex = (short)value;
        //    }
        //}

        /// <summary>
        /// Gets or sets the type of the AC task.
        /// </summary>
        /// <value>The type of the AC task.</value>
        public Global.ACTaskTypes ACTaskType
        {
            get
            {
                return (Global.ACTaskTypes)ACTaskTypeIndex;
            }
            set
            {
                ACTaskTypeIndex = (short)value;
            }
        }        
        #endregion
        
        #region Extended
        /// <summary>
        /// Gets or sets a value indicating whether this instance is AC component initialized.
        /// </summary>
        /// <value><c>true</c> if this instance is AC component initialized; otherwise, <c>false</c>.</value>
        public bool IsACComponentInitialized
        {
            get;
            set;
        }
        #endregion
    }
}
