// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using gip.core.ControlScriptSync.file;
using gip.core.ControlScriptSync.model;
using gip.core.ControlScriptSync.sql;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gip.core.ControlScriptSync
{
    public delegate void MsgDelegate(SyncMessage msg);

    /// <summary>
    /// Head class for control sync process
    /// Sync method execute process
    /// </summary>
    public class ControlSync
    {
        /// <summary>
        /// Output for messsaging out sync process
        /// </summary>
        public event MsgDelegate OnMessage;

        public bool Sync(IRoot root, IACEntityObjectContext db)
        {
            string rootFolder = System.AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
            ConnectionSettings connectionSettings = new ConnectionSettings();
            return Sync(root, db, rootFolder, connectionSettings.DefaultConnectionString);
        }

        /// <summary>
        /// Top method for executing Control Script Sync (importing of changed scripts)
        /// </summary>
        /// <param name="Root">root object - executes script import</param>
        /// <param name="db">Database reference</param>
        /// <returns></returns>
        public bool Sync(IRoot root, IACEntityObjectContext db, string rootFolder, string connectionString)
        {
            VBSQLCommand vBSQLCommand = new VBSQLCommand(connectionString);
            FileCommand fileCommand = new FileCommand(rootFolder, vBSQLCommand);
            bool returnResult = true;
            if (OnMessage != null)
            {
                OnMessage(new SyncMessage()
                {
                    MessageLevel = MessageLevel.Info,
                    Message = "Begin of control script sync."
                });
            }

            if (root == null || db == null)
            {
                if (OnMessage != null)
                {
                    OnMessage(new SyncMessage()
                    {
                        MessageLevel = MessageLevel.Error,
                        Message = "Root and Database reference shuld be not null!"
                    });
                }
                return false;
            }

            datamodel.ControlScriptSyncInfo currentVersion = vBSQLCommand.MaxVersion();
            if (currentVersion != null)
            {
                OnMessage(new SyncMessage()
                {
                    MessageLevel = MessageLevel.Info,
                    Message = "Current version is: " + currentVersion.ToString() + "."
                });
            }
            else
            {
                OnMessage(new SyncMessage()
                {
                    MessageLevel = MessageLevel.Info,
                    Message = "No versions updated!"
                });
            }

            UpdateFiles updateFiles = fileCommand.LoadUpdateFileList();

            // Print omitted - Excluded files if exist
            if (updateFiles != null && updateFiles.ExcludedItems != null && updateFiles.ExcludedItems.Any())
            {
                OnMessage(new SyncMessage()
                {
                    MessageLevel = MessageLevel.Warning,
                    Message = string.Format("Max. version in database is: {0}! Some file are excluded:", updateFiles.MaxVersion)
                });
                foreach (var item in updateFiles.ExcludedItems)
                {
                    OnMessage(new SyncMessage()
                    {
                        MessageLevel = MessageLevel.Warning,
                        Message = string.Format("{0} - Excluded (Omitted)  while in database exist younger version script file!", item.FileName)
                    });
                }
            }

            if (updateFiles != null && updateFiles.Items != null && updateFiles.Items.Any())
            {
                string availableVersions = "";
                availableVersions = string.Join(", ", updateFiles.Items.Select(x => x.FileName).ToArray());
                availableVersions = availableVersions.TrimEnd(", ".ToCharArray());
                OnMessage(new SyncMessage()
                {
                    MessageLevel = MessageLevel.Info,
                    Message = "Next control script versions for update: " + availableVersions
                });

                ACQueryDefinition QRYACProject = gip.core.datamodel.Database.Root.Queries.CreateQuery(db as IACComponent, Const.QueryPrefix + ACProject.ClassName, null);
                foreach (var item in updateFiles.Items)
                {
                    ACFSItemContainer fsContainer = new ACFSItemContainer(db, false);

                    OnMessage(new SyncMessage()
                    {
                        MessageLevel = MessageLevel.Info,
                        Message = "Update version: " + item.FileName
                    });


                    try
                    {
                        item.DeleteFolder();
                        item.ExtractToFolder();
                        IResources resources = Activator.CreateInstance(TypeAnalyser.GetTypeInAssembly("gip.core.autocomponent.Resources")) as IResources;
                        ACFSItem rootItem = resources.Dir(db, fsContainer, item.FullDirectoryName, true);
                        MsgWithDetails importConflictMessages = new MsgWithDetails();
                        rootItem.CallAction(ACFSItemOperations.ProcessUpdateDate, importConflictMessages, item.FileName);
                        List<Msg> importConflictErrorMessages = importConflictMessages.MsgDetails.Where(c => c.MessageLevel == eMsgLevel.Error).ToList();
                        foreach (Msg msg in importConflictErrorMessages)
                        {
                            OnMessage(new SyncMessage()
                            {
                                Source = msg.Source,
                                MessageLevel = MessageLevel.Warning,
                                Message = msg.Message
                            });
                        }
                        rootItem.CallAction(ACFSItemOperations.AttachOrDeattachToContext, true);
                    }
                    catch (Exception xmlDeserializationException)
                    {
                        throw new Exception(string.Format("Deserialization problem for {0}! Exception: {1}.", item.FullDirectoryName, xmlDeserializationException.Message));
                    }

                    OnMessage(new SyncMessage()
                    {
                        MessageLevel = MessageLevel.Success,
                        Message = "Deserialized items: " + item.FileName
                    });

                    MsgWithDetails msgSaveControlScripts = db.ACSaveChanges(true, false, false);
                    if (msgSaveControlScripts != null)
                    {
                        Console.WriteLine(string.Format(@"ControlSync Error, File:{0}", item.FileName));
                        returnResult = false;
                        string message = string.Format("Error executing control  script: {0}. Error: {1}", item.FileName, msgSaveControlScripts.InnerMessage);
                        OnMessage(new SyncMessage()
                        {
                            MessageLevel = MessageLevel.Error,
                            Message = message
                        });
                        db.ACUndoChanges();
                    }
                    else
                    {
                        vBSQLCommand.Insert(item.Version, item.Author);
                        Console.WriteLine(string.Format(@"ControlSync Success, File:{0}", item.FileName));
                    }
                    item.DeleteFolder();
                }
            }
            else
            {
                OnMessage(new SyncMessage()
                {
                    MessageLevel = MessageLevel.Info,
                    Message = "No versions for update in file system!"
                });
            }

            return returnResult;
        }
    }
}
