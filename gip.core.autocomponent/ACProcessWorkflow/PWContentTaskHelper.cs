using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.Data.Objects;

namespace gip.core.autocomponent
{
    public static class PWContentTaskHelper
    {
        public static void InitContent(ACClass acType, PWBase wfComponent, IACObject content, ACValueList parameter)
        {
            if (content is ACClassWF)
            {
                if (wfComponent.ParentACComponent.ContentTask != null)
                {
                    //bool generateTask = true;
                    //if (wfComponent is PWProcessFunction)
                    //{
                    //    ACClassMethod acClassMethod = parameter["ACClassMethod"] as ACClassMethod;
                    //    AsyncMethodInvocationMode invocationMode = (AsyncMethodInvocationMode)parameter["invocationMode"];
                    //    if ((acClassMethod == null) || !acClassMethod.IsPersistable || invocationMode == AsyncMethodInvocationMode.Synchronous)
                    //        generateTask = false;
                    //}

                    ACClassTaskQueue.TaskQueue.ProcessAction(() =>
                    {
                        ACProgram acProgram = wfComponent.ParentACComponent.ContentTask.ACProgram;
                        if (acProgram == null)
                        {
                            if (wfComponent is PWProcessFunction)
                            {
                                ACValue acValue = parameter.GetACValue(ACProgram.ClassName);
                                if (acValue != null && acValue.Value != null)
                                {
                                    Guid acProgramID = (Guid)acValue.Value;
                                    if (acProgramID != Guid.Empty)
                                        acProgram = ACClassTaskQueue.s_cQry_ACProgram(ACClassTaskQueue.TaskQueue.Context, acProgramID);
                                }
                            }
                            else
                            {
                                PWProcessFunction parentPWFunction = wfComponent.RootPW;
                                if (parentPWFunction != null)
                                    acProgram = parentPWFunction.CurrentACProgram;
                            }
                        }

                        ACClassTask acClassTask = ACClassTask.NewACObject(ACClassTaskQueue.TaskQueue.Context, wfComponent.ParentACComponent.ContentTask);
                        acClassTask.TaskTypeACClass = ACClassTaskQueue.TaskQueue.GetACClassFromTaskQueueCache(acType.ACClassID);
                        acClassTask.ContentACClassWF = wfComponent.ContentACClassWF.Database == ACClassTaskQueue.TaskQueue.Context ? 
                                                              wfComponent.ContentACClassWF 
                                                            : ACClassTaskQueue.TaskQueue.GetACClassWFFromTaskQueueCache(wfComponent.ContentACClassWF.ACClassWFID);
                        acClassTask.ACTaskType = Global.ACTaskTypes.WorkflowTask;
                        acClassTask.IsDynamic = true;
                        acClassTask.ACIdentifier = wfComponent.ACIdentifier;
                        acClassTask.ACProgram = acProgram;
                        ACClassTaskQueue.TaskQueue.Context.ACClassTask.AddObject(acClassTask);
                        wfComponent.Content = acClassTask;
                    });
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
            ACClassTaskQueue.TaskQueue.Add(() =>
            {
                if (task.EntityState != System.Data.EntityState.Deleted
                    && task.EntityState != System.Data.EntityState.Detached)
                    ACClassTaskQueue.TaskQueue.Context.DeleteObject(task);
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
                bool? mustRefreshACClassWF = null;
                try
                {
                    using (ACMonitor.Lock(pwGroupComponent.ContextLockForACClassWF))
                    {
                        mustRefreshACClassWF = pwGroupComponent.ContentACClassWF.ACClassMethod != null && pwGroupComponent.ContentACClassWF.ACClassMethod.MustRefreshACClassWF;
                        // Nachladen, falls Workflow von Client verändert worden ist
                        if (mustRefreshACClassWF.Value)
                        {
                            if (pwGroupComponent.ContentACClassWF.ACClassWF_ParentACClassWF.IsLoaded)
                            {
                                pwGroupComponent.ContentACClassWF.ACClassWF_ParentACClassWF.AutoRefresh();
                                pwGroupComponent.ContentACClassWF.ACClassWF_ParentACClassWF.AutoLoad();
                            }
                        }
                        wfChilds = pwGroupComponent.ContentACClassWF.ACClassWF_ParentACClassWF.ToArray();
                        if (mustRefreshACClassWF.Value && wfChilds != null && wfChilds.Any())
                        {
                            foreach (var acClassWF in wfChilds)
                            {
                                acClassWF.AutoRefresh();
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
