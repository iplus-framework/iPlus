// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.Threading;
using gip.core.datamodel.Licensing;
using gip.core.dbsyncer;
using gip.core.dbsyncer.Messages;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace gip.core.autocomponent
{
    public class ACStartUpRoot
    {
        public ACStartUpRoot(IWPFServices wpfServices) 
        {
            _IWPFServices = wpfServices;
        }


        public short LoginUser(string userName, string password, bool registerACObjects, bool propPersistenceOff, ref String errorMsg, bool wcfOff = false, bool simulation = false, bool fullscreen = false)
        {
            try
            {
                try
                {
                    CoreConfiguration coreConfig = (CoreConfiguration)CommandLineHelper.ConfigCurrentDir.GetSection("Process/CoreConfiguration");
                    if (coreConfig != null)
                    {
                        ACMonitor.UseSimpleMonitor = coreConfig.UseSimpleMonitor;
                        if (coreConfig.ValidateLockHierarchies)
                            ACMonitor.ValidateLockHierarchy = true;
                        PWBase.PoolWFComponents = coreConfig.PoolWFNodes;
                        PWProcessFunction.ConsistencyCheckWF = coreConfig.ConsistencyCheckWF;
                        PoolReference.UseWeekRef = coreConfig.UseWeekRefForPooling;
                    }
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                    {
                        msg += " Inner:" + e.InnerException.Message;
                        if (e.InnerException.InnerException != null && e.InnerException.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.InnerException.Message;
                    }

                    if (   datamodel.Database.Root != null 
                        && datamodel.Database.Root.Messages != null 
                        && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("ACStartUpRoot", "LoginUser", msg);
                }

                Messages.ConsoleMsg("System", "Connecting to database...");

                // DB-Sync
                if (registerACObjects)
                {
                    SyncTask syncTask = new SyncTask();
                    //syncTask.SyncCallExternalTool += SyncTask_SyncCallExternalTool;
                    syncTask.OnStatusChange += syncTask_OnStatusChange;
                    List<BaseSyncMessage> syncMsgList = syncTask.DoSync();
                    if (syncMsgList != null && syncMsgList.Where(x => !x.Success).Any())
                    {
                        string errorDBSyncMessage = "There is some errors by executing DBSync!";
                        errorDBSyncMessage += System.Environment.NewLine;
                        errorDBSyncMessage += System.Environment.NewLine;
                        foreach (var a in syncMsgList)
                        {
                            errorDBSyncMessage += a.Message;
                            errorDBSyncMessage += System.Environment.NewLine;
                        }
                        errorDBSyncMessage += System.Environment.NewLine;
                        Messages.ConsoleMsg("System", errorDBSyncMessage);
                        string logFileName = string.Format(@"gip.core.dbsyncer.exe_log_{0}.log", DateTime.Now.ToString("yyyy-MM-dd_HH_mm_ss"));
                        File.WriteAllText(Path.Combine(Path.GetTempPath(), logFileName), errorDBSyncMessage);
                        throw new Exception(errorDBSyncMessage);
                    }
                }

                VBUser user = CheckLogin(userName, password, ref errorMsg);
                if (user == null)
                    return 0;

                try
                {
                    Environment._Licence = new License(user, Database);
                }
                catch (Exception e)
                {
                    Messages.ConsoleMsg("System", "Login failed!" + e.Message);
                    if (e.InnerException != null && e.InnerException.Message != null)
                    {
                        Messages.ConsoleMsg("System", e.InnerException.Message);
                        if (e.InnerException.InnerException != null && e.InnerException.InnerException.Message != null)
                            Messages.ConsoleMsg("System", e.InnerException.InnerException.Message);
                    }
                    return -1;
                }

                Messages.ConsoleMsg("System", "Connected!");

                ACClassTask acClassTask = null;
                ACClass taskTypeACClass = null;
                ACClassTaskQueue.TaskQueue.ProcessAction(() =>
                {
                    var acProject = ACClassTaskQueue.TaskQueue.Context.ACProject.Where(c => c.ACProjectTypeIndex == (short)Global.ACProjectTypes.Root)
                                                                                .FirstOrDefault();
                    if (acProject != null)
                    {
                        acClassTask = acProject.RootClass.ACClassTask_TaskTypeACClass.Where(c => !c.IsTestmode).FirstOrDefault();
                        if (acClassTask != null)
                            taskTypeACClass = acClassTask.TaskTypeACClass;
                    }
                });
                if (taskTypeACClass == null)
                    return 0;
                // ACType immer vom globalen Datenbankkontext!
                ACClass typeACClass = Database.ACClass.Where(c => c.ACClassID == acClassTask.TaskTypeACClass.ACClassID).First();

                ACValueList acValueList = new ACValueList();
                acValueList.Add(new ACValue(Const.StartupParamUser, typeof(VBUser), user));
                acValueList.Add(new ACValue(Const.StartupParamRegisterACObjects, typeof(Boolean), registerACObjects));
                acValueList.Add(new ACValue(Const.StartupParamPropPersistenceOff, typeof(Boolean), propPersistenceOff));
                acValueList.Add(new ACValue(Const.StartupParamWCFOff, typeof(Boolean), wcfOff));
                acValueList.Add(new ACValue(Const.StartupParamSimulation, typeof(Boolean), simulation));
                acValueList.Add(new ACValue(Const.StartupParamFullscreen, typeof(Boolean), fullscreen));
                acValueList.Add(new ACValue(Const.StartupParamWPFServices, typeof(IWPFServices), _IWPFServices));

                ACConvert.MyDataContractResolver = new WCFDataContractResolver();
                ACRoot root = ACActivator.CreateInstance(typeACClass, acClassTask, null, acValueList, Global.ACStartTypes.Automatic, null, "", true) as ACRoot;
                root.ACPostInit();
                root.OnStartupSucceeded();

                return (root != null) ? (short)1 : (short)0;
            }
            catch (Exception e)
            {
                errorMsg = e.Message;
                Exception innerException = e.InnerException;
                while (innerException != null)
                {
                    errorMsg += " " + innerException.Message;
                    innerException = innerException.InnerException;
                }
                return (short)0;
            }
        }

        //private BaseSyncMessage SyncTask_SyncCallExternalTool(string toolName)
        //{
        //    BaseSyncMessage message = null;
        //    switch (toolName)
        //    {
        //        case "DBSyncerUpdate.unit.test":
        //            try
        //            {
        //                TransferCommand cmd = new TransferCommand();
        //                bool success = cmd.DoTransfer();
        //                message = new ProgressMessage();
        //                message.Success = success;
        //            }
        //            catch(Exception ec)
        //            {
        //                message = new ErrorMessage(ec);
        //            }
        //            break;
        //    }
        //    return message;
        //}

        private void syncTask_OnStatusChange(core.dbsyncer.Messages.BaseSyncMessage msg)
        {
            Messages.ConsoleMsg("System", msg.Message);
        }
        /// <summary>Prüft auf gültiges Login</summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <param name="errorMsg"></param>
        /// <returns>
        ///   <br />
        /// </returns>
        public VBUser CheckLogin(string user, string password, ref String errorMsg)
        {
            Messages.ConsoleMsg("System", "Login " + user + "...");

            try
            {
                if (Database == null)
                {
                    errorMsg = "DB-Connection failed!";
                    Messages.ConsoleMsg("System", "DB-Connection failed!");
                    return null;
                }
            }
            catch (Exception e)
            {
                Messages.ConsoleMsg("System", "DB-Connection failed!");
                Messages.ConsoleMsg("System", e.Message);
                errorMsg = "DB-Connection failed: " + e.Message;
                if (e.InnerException != null && !String.IsNullOrEmpty(e.InnerException.Message))
                {
                    errorMsg += " " + e.InnerException.Message;
                    if (e.InnerException.InnerException != null && !String.IsNullOrEmpty(e.InnerException.InnerException.Message))
                    {
                        errorMsg += " " + e.InnerException.InnerException.Message;
                    }
                }
                return null;
            }


            try
            {
                // TODO: Passwortverschlüsselung von C++ übernehmen
                if (password == "autologin")
                {
                    VBUser vbUser = null;
                    using (ACMonitor.Lock(Database.QueryLock_1X000))
                    {
                        vbUser = this.Database.VBUser.Include(c => c.VBUserGroup_VBUser)
                                                    .Where(c => c.VBUserName.ToLower() == user.ToLower())
                                                    .FirstOrDefault();
                    }
                    if (vbUser != null)
                    {
                        Messages.ConsoleMsg("System", "Login succeed!");
                        return vbUser;
                    }
                }
                else
                {
                    VBUser vbUser = null;
                    using (ACMonitor.Lock(Database.QueryLock_1X000))
                    {
                        vbUser = this.Database.VBUser.Include(c => c.VBUserGroup_VBUser)
                                                    .Where(c => c.VBUserName.ToLower() == user.ToLower())
                                                    .FirstOrDefault();
                    }
                    if (vbUser == null)
                    {
                        Messages.ConsoleMsg("System", "User doesn't exist!");
                        errorMsg = "User doesn't exist!";
                        return null;
                    }
                    if (password != vbUser.Password)
                    {
                        try
                        {
                            vbUser.AutoRefresh();
                        }
                        catch (Exception e)
                        {
                            Messages.ConsoleMsg("System", e.Message);
                        }

                        if (password != vbUser.Password)
                        {
                            if (!ACCrypt.VerifyMd5Hash(password, vbUser.Password))
                            {
                                Messages.ConsoleMsg("System", "Wrong password!");
                                errorMsg = "Wrong password!";
                                return null;
                            }
                        }
                    }

                    Messages.ConsoleMsg("System", "Login succeed!");
                    return vbUser;
                }
                Messages.ConsoleMsg("System", "Login failed!");
            }
            catch (Exception e)
            {
                errorMsg = e.Message;
                Messages.ConsoleMsg("System", e.Message);
                return null;
            }
            return null;
        }

        public Database Database
        {
            get
            {
                return Database.GlobalDatabase;
            }
        }

        private IWPFServices _IWPFServices;
    }
}
