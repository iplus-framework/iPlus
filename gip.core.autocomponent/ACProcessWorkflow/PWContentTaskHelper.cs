// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using Microsoft.EntityFrameworkCore;

namespace gip.core.autocomponent
{
    public static class PWContentTaskHelper
    {
        public static void InitContent(ACClass acType, PWBase wfComponent, IACObject content, ACValueList parameter)
        {
            if (content is ACClassWF)
            {
                ACClassTask parentTask = wfComponent.ParentACComponent.ContentTask;
                if (parentTask != null)
                {
                    //bool generateTask = true;
                    //if (wfComponent is PWProcessFunction)
                    //{
                    //    ACClassMethod acClassMethod = parameter["ACClassMethod"] as ACClassMethod;
                    //    AsyncMethodInvocationMode invocationMode = (AsyncMethodInvocationMode)parameter["invocationMode"];
                    //    if ((acClassMethod == null) || !acClassMethod.IsPersistable || invocationMode == AsyncMethodInvocationMode.Synchronous)
                    //        generateTask = false;
                    //}

                    ACProgram acProgram = null;
#if !DIAGNOSE
                    // *** TASKPERFOPT NEW ***
                    if (parentTask.ACProgramID.HasValue)
                    {
                        if (parentTask.ACProgram_IsLoaded)
                            acProgram = parentTask.ACProgram;
                        if (acProgram == null)// && (parentTask.EntityState == System.Data.EntityState.Added || parentTask.EntityState == System.Data.EntityState.Detached))
                            acProgram = parentTask.NewACProgramForQueue;
                        if (acProgram == null)
                            ACClassTaskQueue.TaskQueue.ProcessAction(() => acProgram = parentTask.ACProgram);
                    }

                    if (acProgram == null)
                    {
                        if (wfComponent is PWProcessFunction)
                        {
                            ACValue acValue = parameter.GetACValue(ACProgram.ClassName);
                            if (acValue != null && acValue.Value != null)
                            {
                                Guid acProgramID = (Guid)acValue.Value;
                                if (acProgramID != Guid.Empty)
                                {
                                    acProgram = ACClassTaskQueue.TaskQueue.ProgramCache.GetProgram(acProgramID);
                                    if (acProgram == null)
                                        ACClassTaskQueue.TaskQueue.ProcessAction(() => acProgram = ACClassTaskQueue.s_cQry_ACProgram(ACClassTaskQueue.TaskQueue.Context, acProgramID));
                                }
                            }
                        }
                        else
                        {
                            PWProcessFunction parentPWFunction = wfComponent.RootPW;
                            if (parentPWFunction != null)
                                acProgram = parentPWFunction.CurrentACProgram;
                        }
                    }

                    ACClassTask acClassTask = ACClassTask.NewACObject(ACClassTaskQueue.TaskQueue.Context, null);
                    acClassTask.NewParentACClassTaskForQueue = parentTask;
                    acClassTask.NewTaskTypeACClassForQueue = ACClassTaskQueue.TaskQueue.GetACClassFromTaskQueueCache(acType.ACClassID);
                    acClassTask.NewACProgramForQueue = acProgram;
                    acClassTask.NewContentACClassWFForQueue = wfComponent.ContentACClassWF.Database == ACClassTaskQueue.TaskQueue.Context ?
                                                          wfComponent.ContentACClassWF
                                                        : ACClassTaskQueue.TaskQueue.GetACClassWFFromTaskQueueCache(wfComponent.ContentACClassWF.ACClassWFID);
                    acClassTask.ACTaskType = Global.ACTaskTypes.WorkflowTask;
                    acClassTask.IsDynamic = true;
                    acClassTask.ACIdentifier = wfComponent.ACIdentifier;
                    acClassTask.IsTestmode = parentTask.IsTestmode;
                    wfComponent.Content = acClassTask;
                    ACClassTaskQueue.TaskQueue.Add(() => 
                    {
                        acClassTask.PublishToChangeTrackerInQueue();
                        ACClassTaskQueue.TaskQueue.Context.ACClassTask.Add(acClassTask); 
                    });
                    // *** TASKPERFOPT NEW END ***
#else
                    // *** TASKPERFOPT OLD ***
                    ACClassTaskQueue.TaskQueue.ProcessAction(() =>
                    {
                        acProgram = parentTask.ACProgram;
                        if (acProgram == null)
                        {
                            if (wfComponent is PWProcessFunction)
                            {
                                ACValue acValue = parameter.GetACValue(ACProgram.ClassName);
                                if (acValue != null && acValue.Value != null)
                                {
                                    Guid acProgramID = (Guid)acValue.Value;
                                    if (acProgramID != Guid.Empty)
                                    {
                                        acProgram = ACClassTaskQueue.TaskQueue.ProgramCache.GetProgram(acProgramID);
                                        if (acProgram == null)
                                            acProgram = ACClassTaskQueue.s_cQry_ACProgram(ACClassTaskQueue.TaskQueue.Context, acProgramID);
                                    }
                                }
                            }
                            else
                            {
                                PWProcessFunction parentPWFunction = wfComponent.RootPW;
                                if (parentPWFunction != null)
                                    acProgram = parentPWFunction.CurrentACProgram;
                            }
                        }

                        ACClassTask acClassTask = ACClassTask.NewACObject(ACClassTaskQueue.TaskQueue.Context, parentTask);
                        acClassTask.TaskTypeACClass = ACClassTaskQueue.TaskQueue.GetACClassFromTaskQueueCache(acType.ACClassID);
                        acClassTask.NewContentACClassWFForQueue = wfComponent.ContentACClassWF.Database == ACClassTaskQueue.TaskQueue.Context ?
                                                              wfComponent.ContentACClassWF
                                                            : ACClassTaskQueue.TaskQueue.GetACClassWFFromTaskQueueCache(wfComponent.ContentACClassWF.ACClassWFID);
                        acClassTask.ACTaskType = Global.ACTaskTypes.WorkflowTask;
                        acClassTask.IsDynamic = true;
                        acClassTask.ACIdentifier = wfComponent.ACIdentifier;
                        acClassTask.NewACProgramForQueue = acProgram;
                        ACClassTaskQueue.TaskQueue.Context.ACClassTask.AddObject(acClassTask);
                        wfComponent.Content = acClassTask;
                    });
                    // *** TASKPERFOPT OLD END ***
#endif
                }
            }
        }

        public static void DeInitContent(PABase wfComponent, bool deleteACClassTask)
        {
            if (!deleteACClassTask || wfComponent.ContentTask == null)
            {
                wfComponent.Content = null;
                return;
            }
            ACClassTask task = wfComponent.ContentTask;
            wfComponent.Content = null;
            //ACClassTaskQueue.TaskQueue.AddACClassTaskForBulkDelete(task);

            ACClassTaskQueue.TaskQueue.Add(() =>
            {
                if (task.EntityState != EntityState.Deleted
                    && task.EntityState != EntityState.Detached)
                    ACClassTaskQueue.TaskQueue.Context.Remove(task);
            }
            );
        }

        public static void InitWFChilds(IACComponentPWGroup pwGroup, WFDictionary wfDictionary)
        {
            if (wfDictionary == null)
                return;
            PWBase pwGroupComponent = pwGroup as PWBase;
            if (pwGroupComponent == null || pwGroupComponent.ContentACClassWF == null)
                return;

            if (pwGroupComponent.WFInitPhase >= PWBase.WFInstantiatonPhase.NewWF_Creating)
            {
                ACClassWF[] wfChilds = null;
                bool mustRefreshACClassWF = false;
                try
                {
                    mustRefreshACClassWF = pwGroupComponent.ContentACClassWF.ACClassMethod != null && pwGroupComponent.ContentACClassWF.ACClassMethod.MustRefreshACClassWF;
                    bool childsLoaded = pwGroupComponent.ContentACClassWF.ACClassWF_ParentACClassWF_IsLoaded;

                    if (mustRefreshACClassWF || !childsLoaded)
                    {
                        using (ACMonitor.Lock(pwGroupComponent.ContextLockForACClassWF))
                        {
                            // Nachladen, falls Workflow von Client verändert worden ist
                            if (mustRefreshACClassWF || !childsLoaded)
                            {
                                //pwGroupComponent.ContentACClassWF.ACClassWF_ParentACClassWF.AutoRefresh();
                                pwGroupComponent.ContentACClassWF.ACClassWF_ParentACClassWF.AutoLoad(pwGroupComponent.ContentACClassWF.ACClassWF_ParentACClassWFReference, pwGroupComponent.ContentACClassWF);
                            }
                            wfChilds = pwGroupComponent.ContentACClassWF.ACClassWF_ParentACClassWF.ToArray();
                            if (mustRefreshACClassWF && wfChilds != null && wfChilds.Any())
                            {
                                foreach (var acClassWF in wfChilds)
                                {
                                    acClassWF.AutoRefresh();
                                }
                            }
                        }
                    }
                    else
                    {
                        wfChilds = pwGroupComponent.ContentACClassWF.ACClassWF_ParentACClassWF.ToArray();
                    }
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("PWContentTaskHelper", "InitWFChilds", msg);
                }

                if (wfChilds != null && wfChilds.Any())
                {
                    foreach (var acClassWF in wfChilds)
                    {
                        //if (mustRefreshACClassWF.HasValue && mustRefreshACClassWF.Value)
                        //{
                        //    using (ACMonitor.Lock(pwGroupComponent.ContextLockForACClassWF))
                        //    {
                        //        acClassWF.AutoRefresh();
                        //    }
                        //}

                        // Typ der Klasse ist vom Globalen Context, WF vom Task-Context
                        ACClass acClass = pwGroupComponent.Root.Database.ContextIPlus.GetACType(acClassWF.PWACClassID);
                        if (acClass != null)
                        {
                            ACComponent pwComponent = pwGroupComponent.StartComponent(acClass, acClassWF, null) as ACComponent;
                            if (pwComponent != null)
                            {
                                wfDictionary.AddPWComponent((ACClassWF)acClassWF, pwComponent);
                            }
                        }
                    }
                }

                if (pwGroup is PWProcessFunction)
                {
                    ACClassTaskQueue.TaskQueue.ProcessAction(() =>
                    {
                        try
                        {
                            ACClassTaskQueue.TaskQueue.Context.ACSaveChanges();
                        }
                        catch (Exception e)
                        {
                            string msg = e.Message;
                            if (e.InnerException != null && e.InnerException.Message != null)
                                msg += " Inner:" + e.InnerException.Message;

                            Database.Root.Messages.LogException("PWContentTaskHelper", "InitWFChilds(Save)", msg);
                        }
                    });
                }
            }
            else
            {
                foreach (var child in pwGroupComponent.ACComponentChilds)
                {
                    PWBase childPW = child as PWBase;
                    if (childPW != null)
                    {
                        if (childPW.ContentACClassWF != null)
                            wfDictionary.AddPWComponent(childPW.ContentACClassWF, child as ACComponent);
                    }
                }
            }
        }
    }
}
