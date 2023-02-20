using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using gip.core.datamodel;
using System.IO;
using System.Xml;
using System.Threading;
using Microsoft.EntityFrameworkCore;

namespace gip.core.autocomponent
{
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACPointTask'}de{'ACPointTask'}", Global.ACKinds.TACClass)]
    public sealed class ACPointTask : ACPointAsyncRMI
    {
        #region c'tors
        /// <summary>
        /// Constructor for Deserializer
        /// </summary>
        public ACPointTask()
            : this(null, (ACClassProperty)null, 0)
        {
        }

        /// <summary>Constructor for Reflection-Instantiation</summary>
        /// <param name="parent"></param>
        /// <param name="acClassProperty"></param>
        /// <param name="maxCapacity"></param>
        public ACPointTask(IACComponent parent, IACType acClassProperty, uint maxCapacity)
            : base(parent,acClassProperty, maxCapacity)
        {
        }

        public ACPointTask(IACComponent parent, string propertyName, uint maxCapacity)
            : base(parent, parent.ComponentClass.GetMember(propertyName), maxCapacity)
        {
        }

        /// <summary>Is called when the parent ACComponent is stopping/unloading.</summary>
        /// <param name="deleteACClassTask">if set to <c>true if the parent ACComponent should be removed from the persistable Application-Tree.</c></param>
        public override void ACDeInit(bool deleteACClassTask = false)
        {
            if (deleteACClassTask)
            {
                ACClassTaskValue taskValue = this.ACClassTaskValue;
                if (taskValue != null)
                {
                    if (   !taskValue.ACClassTaskValuePos_ACClassTaskValue_IsLoaded
                        || taskValue.ACClassTaskValuePos_ACClassTaskValue.Any())
                    {
                        ACClassTaskQueue.TaskQueue.Add(() => 
                        {
                            try
                            {
                                foreach (var pos in taskValue.ACClassTaskValuePos_ACClassTaskValue.ToArray())
                                {
                                    if (pos.EntityState != EntityState.Deleted
                                        && pos.EntityState != EntityState.Detached)
                                        ACClassTaskQueue.TaskQueue.Context.Detach(pos);
                                }
                            }
                            catch (Exception e)
                            {
                                string msg = e.Message;
                                if (e.InnerException != null && e.InnerException.Message != null)
                                    msg += " Inner:" + e.InnerException.Message;

                                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                                    datamodel.Database.Root.Messages.LogException("ACPointTask", "ACDeInit", msg);
                            }
                        }
                        );
                    }
                }
            }
            base.ACDeInit(deleteACClassTask);
        }

#endregion

#region Method's

        public override bool Persist(bool withLock)
        {
            // Persistierung durch Seralisierung der LocalStorage-Liste
            // bool result =  base.Persist(withLock);
            // Persistierung per Tabelle:
            bool result = ACPointTask.PersistToACClassTask(this, withLock);
            if (!PropertyInfo.IsPersistable)
                return result;
            PersistUnsent(withLock);
            return result;
        }

        internal override bool ReStoreFromDB()
        {
            // Restore durch De-Seralisierung der LocalStorage-Liste
            // bool result = base.ReStoreFromDB();
            // Restore per Tabelle:
            bool result = ACPointTask.ReStoreFromACClassTask(this);
            if (!PropertyInfo.IsPersistable)
                return result;
            RestoreUnsetAsyncRMI();
            return result;
        }


        internal static bool PersistToACClassTask(ACPointNetStorableAsyncRMIBase<ACComponent, ACPointAsyncRMIWrap<ACComponent>> point, bool withLock)
        {
            if (!point.PropertyInfo.IsPersistable 
                || !point.ACRef.IsObjLoaded
                || (point.ACRef.ValueT.ContentTask == null)
                || (point.ACRef.ValueT.ACOperationMode == ACOperationModes.Test))
                return false;

            ACComponent parentACComponent = point.ParentACComponent as ACComponent;
            if (parentACComponent == null)
                return false;

            try
            {
                string valueACMethodXML = null;
                IEnumerable<ACPointAsyncRMIWrap<ACComponent>> copyLocalStorage = null;

                using (ACMonitor.Lock(point.LockLocalStorage_20033))
                {
                    copyLocalStorage = point.LocalStorage.ToArray();
                }

                ACClassTaskValue taskValue = point.ACClassTaskValueWithInit;
                if (copyLocalStorage != null && copyLocalStorage.Any())
                {
                    if (taskValue != null)
                    {
                            // Sofort ausführen, damit keine Multithreading-Probleme auftauchen
                            ACClassTaskQueue.TaskQueue.Add(() => 
                            {
                                StringBuilder sb = new StringBuilder();
                                using (StringWriter sw = new StringWriter(sb))
                                using (XmlTextWriter xmlWriter = new XmlTextWriter(sw))
                                {
                                    DataContractSerializer serializer = new DataContractSerializer(typeof(ACMethod), new DataContractSerializerSettings() { KnownTypes = ACKnownTypes.GetKnownType(), MaxItemsInObjectGraph = 99999999, IgnoreExtensionDataObject = true, PreserveObjectReferences = true, DataContractResolver = ACConvert.MyDataContractResolver });
                                    IEnumerable<IACTask> diffTasks = taskValue.ACClassTaskValuePos_ACClassTaskValue
                                                                            .Select(c => c as IACTask)
                                                                            .Except(copyLocalStorage, new ACPointTaskEqualityComparer())
                                                                            .ToList();
                                    if (diffTasks.Any())
                                    {
                                        foreach (ACClassTaskValuePos posToDelete in diffTasks)
                                        {
                                            posToDelete.DeleteACObject(ACClassTaskQueue.TaskQueue.Context, false);
                                        }
                                    }

                                    foreach (ACPointAsyncRMIWrap<ACComponent> entry in copyLocalStorage)
                                    {
                                        ACMethod acMethodClone = entry.ACMethod;
                                        acMethodClone = acMethodClone.Clone() as ACMethod;
                                        serializer.WriteObject(xmlWriter, acMethodClone);

                                        valueACMethodXML = sw.ToString();
                                        sb.Clear();
                                        sw.Flush();
                                        xmlWriter.Flush();

                                        bool add = false;
                                        ACClassTaskValuePos taskValuePos = taskValue.ACClassTaskValuePos_ACClassTaskValue.Where(c => c.RequestID == entry.RequestID).FirstOrDefault();
                                        if (taskValuePos == null)
                                        {
                                            taskValuePos = ACClassTaskValuePos.NewACObject(ACClassTaskQueue.TaskQueue.Context, taskValue);
                                            add = true;
                                        }
                                        taskValuePos.XMLACMethod = valueACMethodXML;
                                        taskValuePos.ACUrl = entry.ACUrl;
                                        taskValuePos.State = entry.State;
                                        taskValuePos.InProcess = entry.InProcess;
                                        taskValuePos.SequenceNo = Convert.ToInt64(entry.SequenceNo);
                                        taskValuePos.ClientPointName = entry.ClientPointName;
                                        taskValuePos.AsyncCallbackDelegateName = entry.AsyncCallbackDelegateName;
                                        taskValuePos.RequestID = entry.RequestID;
                                        taskValuePos.ACIdentifier = entry.MethodACIdentifier;
                                        if (entry.ExecutingInstance != null)
                                            taskValuePos.ExecutingInstanceURL = entry.ExecutingInstance.ACUrl;
                                        taskValuePos.CallbackIsPending = entry.CallbackIsPending;
                                        if (add)
                                            taskValue.ACClassTaskValuePos_ACClassTaskValue.Add(taskValuePos);
                                        taskValue.UpdateDate = DateTime.Now;
                                    }
                                }
                            }
                        );
                        //// Schliesse Transaktion, damit ProcessQueue am Ende Save-Changes auslöst.
                        //ACClassTaskQueue.TaskQueue.Add(() =>
                        //    {
                        //        taskValue.UpdateDate = DateTime.Now;
                        //    }
                        //);
                    }
                }
                else if (taskValue != null)
                {
                    // Sofort ausführen, damit keine Multithreading-Probleme auftauchen
                    ACClassTaskQueue.TaskQueue.Add(() =>
                        {
                            if (taskValue.ACClassTaskValuePos_ACClassTaskValue.Any())
                            {
                                foreach (ACClassTaskValuePos posToDelete in taskValue.ACClassTaskValuePos_ACClassTaskValue.ToList())
                                {
                                    posToDelete.DeleteACObject(ACClassTaskQueue.TaskQueue.Context, false);
                                }
                                taskValue.UpdateDate = DateTime.Now;
                            }
                        }
                    );
                    //if (saveNeeded)
                    //{
                    //    // Schliesse Transaktion, damit ProcessQueue am Ende Save-Changes auslöst.
                    //    ACClassTaskQueue.TaskQueue.Add(() =>
                    //        {
                    //            taskValue.UpdateDate = DateTime.Now;
                    //        }
                    //    );
                    //}
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ACPointTask", "PersistToACClassTask", msg);
            }
            return true;
        }

        internal class ACClassTaskValuePosSafeWrapper
        {
            public ACClassTaskValuePosSafeWrapper(ACClassTaskValuePos taskValuePos)
            {
                _Entry = new ACPointAsyncRMIWrap<ACComponent>();
                _Entry.ACUrl = taskValuePos.ACUrl;
                _XMLACMethod = taskValuePos.XMLACMethod;
                _ExecutingInstanceURL = taskValuePos.ExecutingInstanceURL;
                _Entry.State = taskValuePos.State;
                _Entry.InProcess = taskValuePos.InProcess;
                _Entry.SequenceNo = Convert.ToUInt64(taskValuePos.SequenceNo);
                _Entry.ClientPointName = taskValuePos.ClientPointName;
                _Entry.AsyncCallbackDelegateName = taskValuePos.AsyncCallbackDelegateName;
                _Entry.CallbackIsPending = taskValuePos.CallbackIsPending;
            }

            ACPointAsyncRMIWrap<ACComponent> _Entry;
            public ACPointAsyncRMIWrap<ACComponent> Entry
            {
                get
                {
                    return _Entry;
                }
            }
            private string _XMLACMethod;
            public string XMLACMethod
            {
                get
                {
                    return _XMLACMethod;
                }
            }

            public string _ExecutingInstanceURL;
            public string ExecutingInstanceURL
            {
                get
                {
                    return _ExecutingInstanceURL;
                }
            }
        }

        internal static bool ReStoreFromACClassTask(ACPointNetStorableAsyncRMIBase<ACComponent, ACPointAsyncRMIWrap<ACComponent>> point)
        {
            // Gilt für Proxy- und Real-Points
            if (!point.PropertyInfo.IsPersistable
                || !point.ACRef.IsObjLoaded
                || (point.ACRef.ValueT.ContentTask == null)
                || (point.ACRef.ValueT.ACOperationMode == ACOperationModes.Test))
                return false;

            ACComponent parentACComponent = point.ParentACComponent as ACComponent;
            if (parentACComponent == null)
                return false;
            ACClassTask contentTask = parentACComponent.ContentTask;
            if (contentTask == null)
                return false;

            bool restored = false;
            try
            {
                ACClassTaskValue acClassTaskValue = point.ACClassTaskValue;
                if (acClassTaskValue == null)
                {
                    Guid thisACTypeID = point.ACType.ACTypeID;
                    Guid vbUserID = point.ACRef.ValueT.Root.Environment.User.VBUserID;

                    if (ACClassTaskQueue.TaskQueue.MassLoadPropertyValuesOff)
                    {
                        ACClassTaskQueue.TaskQueue.ProcessAction(() =>
                            {
                                acClassTaskValue = contentTask.ACClassTaskValue_ACClassTask.
                                            Where(c => (c.ACClassPropertyID == thisACTypeID)
                                                    && (c.VBUser != null)
                                                    && (c.VBUserID == vbUserID))
                                                    .FirstOrDefault();
                            }
                        );
                    }
                    else
                    {
                        acClassTaskValue =  ACClassTaskQueue.TaskQueue.GetFromAllPropValues(contentTask.ACClassTaskID,
                                                                                        thisACTypeID,
                                                                                        vbUserID);
                    }
                    point.ACClassTaskValue = acClassTaskValue;

                    if (acClassTaskValue != null)
                    {
                        ACClassTaskValuePosSafeWrapper[] taskValuePositions = null;
                        if (acClassTaskValue.ACClassTaskValuePos_ACClassTaskValue_IsLoaded)
                            taskValuePositions = acClassTaskValue.ACClassTaskValuePos_ACClassTaskValue.Select(c => new ACClassTaskValuePosSafeWrapper(c)).ToArray();
                        else
                        {
                            ACClassTaskQueue.TaskQueue.ProcessAction(() =>
                            {
                                taskValuePositions = acClassTaskValue.ACClassTaskValuePos_ACClassTaskValue.Select(c => new ACClassTaskValuePosSafeWrapper(c)).ToArray();
                            });
                        }

                        if (taskValuePositions != null && taskValuePositions.Any())
                        {
                            DataContractSerializer serializer = new DataContractSerializer(typeof(ACMethod), new DataContractSerializerSettings() { KnownTypes = ACKnownTypes.GetKnownType(), MaxItemsInObjectGraph = 99999999, IgnoreExtensionDataObject = true, PreserveObjectReferences = true, DataContractResolver = ACConvert.MyDataContractResolver });
                            foreach (var taskValuePos in taskValuePositions)
                            {
                                if (!String.IsNullOrEmpty(taskValuePos.XMLACMethod))
                                {
                                    using (StringReader ms = new StringReader(taskValuePos.XMLACMethod))
                                    using (XmlTextReader xmlReader = new XmlTextReader(ms))
                                    {
                                        ACMethod acMethod = (ACMethod)serializer.ReadObject(xmlReader);
                                        acMethod = acMethod.Clone() as ACMethod;
                                        taskValuePos.Entry.ACMethod = acMethod;
                                    }
                                    taskValuePos.Entry.ExecutingInstance = new ACRef<IACComponent>(taskValuePos.ExecutingInstanceURL, ACRef<IACComponent>.RefInitMode.AutoStart, point, false, true);
                                    restored = true;
                                    point.AddToList(taskValuePos.Entry);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ACPointTask", "ReStoreFromACClassTask", msg);
            }
            return restored;
        }


        public bool ActivateTask(ACMethod acMethod, bool executeMethod, IACComponent executingInstance = null)
        {
            IACTask task = GetTaskOfACMethod(acMethod);
            if (task == null)
                return false;
            return ActivateTask(task, executeMethod, executingInstance);
        }

        public bool ActivateTask(IACTask task, bool executeMethod, IACComponent executingInstance = null)
        {
            if (task == null)
                return false;
            ACPointAsyncRMIWrap<ACComponent> wrapObject = task as ACPointAsyncRMIWrap<ACComponent>;
            if (wrapObject == null)
                return false;
            bool result = base.ActivateAsyncRMI(wrapObject, executeMethod, executingInstance);
            if (!result)
            {
            }
            return result;
        }

        public bool InvokeCallbackDelegate(ACMethod acMethod, ACMethodEventArgs result, PointProcessingState state = PointProcessingState.Deleted)
        {
            IACTask task = GetTaskOfACMethod(acMethod);
            if (task == null)
                return false;
            return InvokeCallbackDelegate(task, result, state);
        }

        public bool InvokeCallbackDelegate(IACTask task, ACMethodEventArgs result, PointProcessingState state = PointProcessingState.Deleted)
        {
            if (task == null)
                return false;
            ACPointAsyncRMIWrap<ACComponent> wrapObject = task as ACPointAsyncRMIWrap<ACComponent>;
            if (wrapObject == null)
                return false;
            return base.InvokeCallbackDelegate(wrapObject, result, state);
        }

        public IACTask GetTaskOfACMethod(ACMethod acMethod)
        {
            if (acMethod == null)
                return null;
            return GetTaskOfRequestID(acMethod.ACRequestID);
        }

        public IACTask GetTaskOfRequestID(Guid requestID)
        {

            using (ACMonitor.Lock(LockConnectionList_20040))
            {
                var query = ConnectionList.Where(c => c != null && (c as ACPointAsyncRMIWrap<ACComponent>).RequestID == requestID);
                if (query.Any())
                    return query.First();
            }
            return null;
        }

        public ACMethod GetACMethodOfRequestID(Guid requestID)
        {
            IACTask task = GetTaskOfRequestID(requestID);
            if (task == null)
                return null;
            return task.ACMethod;
        }

#endregion
    }
}

