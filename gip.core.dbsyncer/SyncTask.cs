using gip.core.dbsyncer.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using gip.core.dbsyncer.model;
using gip.core.dbsyncer.Command;
using gip.core.dbsyncer.helper;
using System.IO;
using Microsoft.EntityFrameworkCore;
using gip.core.datamodel;

namespace gip.core.dbsyncer
{
    public delegate void SyncMessageDelegate(BaseSyncMessage msg);
    public delegate BaseSyncMessage SyncCallExternalTool(string toolName);
    /// <summary>
    /// Head level for using database update program - DBSyncer
    /// </summary>
    public class SyncTask
    {
        /// <summary>
        /// Event enable us to recieve info about dbsyncer executing process in outer program - caller
        /// </summary>
        public event SyncMessageDelegate OnStatusChange;
        public event SyncCallExternalTool SyncCallExternalTool;

        /// <summary>
        /// main executing method - start point for calling syncer
        /// </summary>
        /// <returns></returns>
        public List<BaseSyncMessage> DoSync()
        {
            UpdateSettings settngs = new UpdateSettings();
            return DoSync(DbSyncerSettings.GetDefaultConnectionString(DbSyncerSettings.ConfigCurrentDir), settngs.RootFolder);
        }

        public List<BaseSyncMessage> DoSync(string connectionString, string rootFolder)
        {
            string dbScriptRootFolder = Path.Combine(rootFolder, DbSyncerSettings.SqlScriptLocation);
            List<BaseSyncMessage> msgList = new List<BaseSyncMessage>();
            try
            {
                using (Database db = new Database(connectionString))
                {

                    if (OnStatusChange != null)
                    {
                        OnStatusChange(new ProgressMessage() { Success = true, Message = "Begin database update process..." });
                    }

                    // 1. Check if old structure is present
                    bool isOldStructurePresent = DbSyncerInfoContextCommand.IsOldDBStructurePresent(db);
                    if (isOldStructurePresent)
                    {
                        if (OnStatusChange != null)
                        {
                            OnStatusChange(new ProgressMessage() { Success = true, Message = "Trasform old DBScript structure to @DbSyncerInfo...." });
                        }
                        DbSyncerInfoContextCommand.RunOldStructureTransformationScript(db, dbScriptRootFolder);
                    }

                    // 2. Initial sync - check and create DbInfo database structure
                    bool isInitialDbSync = DbSyncerInfoContextCommand.IsInitialDbSync(db);
                    if (isInitialDbSync)
                    {
                        if (OnStatusChange != null)
                        {
                            OnStatusChange(new ProgressMessage() { Success = true, Message = "Initial database sync .. creating db info structure..." });
                        }
                        DbSyncerInfoContextCommand.RunInitialScript(db, dbScriptRootFolder);
                        isInitialDbSync = DbSyncerInfoContextCommand.IsInitialDbSync(db);
                        if (isInitialDbSync)
                        {
                            Exception errorCreatingDbShema = new Exception("Problem with initial sync..");
                            if (OnStatusChange != null)
                            {
                                OnStatusChange(new ErrorMessage(errorCreatingDbShema));
                            }
                            throw errorCreatingDbShema;
                        }
                    }

                    // 3. Run tool for update DBSyncer
                    UpdateSettings updateSettings = new UpdateSettings(rootFolder);
                    Dictionary<string, string> missingVersions = updateSettings.GetMissingVersions(db);
                    if (missingVersions.Any())
                    {
                        foreach (var item in missingVersions)
                        {
                            OnStatusChange(new ProgressMessage() { Success = true, Message = string.Format("Updating dbsyncer to version {0}...", item.Key) });
                            if (SyncCallExternalTool != null)
                            {
                                BaseSyncMessage dbsyncerUpdateMsg = SyncCallExternalTool(item.Value);
                                if (!dbsyncerUpdateMsg.Success)
                                    throw new Exception(string.Format(@"dbsyncer update not successfully: {0}", dbsyncerUpdateMsg.Message));
                            }
                            DBSyncerVersionCommand.SetVersion(db, item.Key);
                        }
                    }
                    bool OccurencesV4InDb = DbSyncerInfoContextCommand.AreThereV4OccurencesPresent(db);
                    if (OccurencesV4InDb)
                    {
                        OnStatusChange(new ProgressMessage() { Success = true, Message = "V4 Occurences found in database, changing them to V5..." });
                        DbSyncerInfoContextCommand.ChangeV4ToV5InDb(db);
                    }

                    // 2. Checking if all context are stored in DbInfoContext structure...
                    if (OnStatusChange != null)
                    {
                        OnStatusChange(new ProgressMessage() { Success = true, Message = "Check for context registration..." });
                    }
                    List<model.DbSyncerInfoContext> notRegistredContext = DbSyncerInfoContextCommand.MissingContexts(db, dbScriptRootFolder);
                    if (notRegistredContext.Any())
                    {
                        if (OnStatusChange != null)
                        {
                            OnStatusChange(new ProgressMessage() { Success = true, Message = string.Format("Register next context: {0}", String.Join(", ", notRegistredContext.Select(x => x.DbSyncerInfoContextID).ToArray()).TrimEnd(", ".ToCharArray())) });
                        }
                        foreach (model.DbSyncerInfoContext dbInfoContext in notRegistredContext)
                        {
                            DbSyncerInfoContextCommand.InsertContext(db, dbInfoContext);
                        }
                        notRegistredContext = DbSyncerInfoContextCommand.MissingContexts(db, dbScriptRootFolder);
                        if (notRegistredContext.Any())
                        {
                            Exception errorRegisteringContexts = new Exception("Problem with registering contexts!");
                            if (OnStatusChange != null)
                            {
                                OnStatusChange(new ErrorMessage(errorRegisteringContexts));
                            }
                            throw errorRegisteringContexts;
                        }
                    }

                    // 3. Execute Sync per Context
                    List<gip.core.datamodel.DbSyncerInfoContext> dbInfoContextList = DbSyncerInfoContextCommand.DatabaseContexts(db);
                    foreach (gip.core.datamodel.DbSyncerInfoContext dbInfoContext in dbInfoContextList)
                    {
                        DBContextHelper dbInfoContextHelper = new DBContextHelper(db, dbInfoContext, dbScriptRootFolder);
                        if (dbInfoContextHelper.IsUpdateNeeded)
                        {
                            // Prepare connection string for DBContext -> context.ConnectionName -> extract app.config connection  string
                            string contextConnectionString = DbSyncerSettings.GetDefaultConnectionString(DbSyncerSettings.ConfigCurrentDir, dbInfoContext.ConnectionName);
                            using (DbContext contextDb = new DbContext(new DbContextOptionsBuilder().UseSqlServer(contextConnectionString).Options))
                            {
                                foreach (ScriptFileInfo fileInfo in dbInfoContextHelper.UpdateNeededFiles)
                                {
                                    Task mytask = Task.Run(() =>
                                    {
                                        DbSyncerInfoCommand.Update(contextDb, fileInfo);
                                    });
                                    mytask.Wait();
                                    if (OnStatusChange != null)
                                        OnStatusChange(new ProgressMessage() { Success = true, Message = string.Format("Context updated:{0} File:{1}", fileInfo.Context.Name, fileInfo.FileName) });
                                }
                            }
                        }
                        else
                            msgList.Add(new ProgressMessage() { Message = string.Format("No update needed for context {0}!", dbInfoContext.Name), Success = true });
                    }
                }
            }
            catch (Exception ec)
            {
                msgList.Add(new ErrorMessage(ec));
            }
            return msgList;
        }
    }
}
