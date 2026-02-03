using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gip.core.autocomponent;
using gip.core.datamodel;
using System.Data;
using System.ComponentModel;

namespace gip.bso.iplus
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Change log'}de{'Änderungsprotokoll'}", Global.ACKinds.TACBSO, Global.ACStorableTypes.NotStorable, true, true)]
    public class BSOChangeLog : ACBSO
    {
        #region c'tors

        public BSOChangeLog(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "") :
               base(acType, content, parentACObject, parameter, acIdentifier)
        {

        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            return base.ACInit(startChildMode);
        }

        public async override Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            _SelectedACClass = null;
            _SelectedACClassProperty = null;
            _SelectedEntityKey = null;
            _SelectedChangeLog = null;
            bool done = await base.ACDeInit(deleteACClassTask);
            if (done && _BSODatabase != null)
            {
                ACObjectContextManager.DisposeAndRemove(_BSODatabase);
                _BSODatabase = null;
            }
            return done;
        }

        public const string ClassName = "BSOChangeLog";

        #endregion

        #region DB

        private Database _BSODatabase = null;
        /// <summary>
        /// Overriden: Returns a separate database context.
        /// </summary>
        /// <value>The context as IACEntityObjectContext.</value>
        public override IACEntityObjectContext Database
        {
            get
            {
                if (_BSODatabase == null)
                    _BSODatabase = ACObjectContextManager.GetOrCreateContext<Database>(this.GetACUrl());
                return _BSODatabase;
            }
        }

        public Database Db
        {
            get
            {
                return Database as Database;
            }
        }

        #endregion

        #region BSO -> ACProperty

        private Dictionary<string, List<Guid>> _EntityKeysMap;

        private bool _IsShowConfigsInDialog = false;

        private ACClass _SelectedACClass;
        [ACPropertySelected(999,"ChangeLogACClass")]
        public ACClass SelectedACClass
        {
            get
            {
                return _SelectedACClass;
            }
            set
            {
                _SelectedACClass = value;
                LoadEntities();

                OnPropertyChanged("SelectedACClass");
                OnPropertyChanged("SelectedACClassProperty");
                ChangeLogList = null;
            }
        }

        [ACPropertyList(401, "ChangeLogACClass")]
        public IEnumerable<ACClass> ACClassList
        {
            get
            {
                return Db.ACChangeLog.GroupBy(c => c.ACClassID).Select(t => t.FirstOrDefault().ACClass).ToList().OrderBy(x => x.ACCaption);
            }
        }

        private ACValueItem _SelectedEntityKey;
        [ACPropertySelected(402,"ChangeLogEntityKey")]
        public ACValueItem SelectedEntityKey
        {
            get
            {
                return _SelectedEntityKey;
            }
            set
            {
                _SelectedEntityKey = value;
                OnPropertyChanged("SelectedEntityKey");
                OnPropertyChanged("ACClassPropertyList");
                if (!_IsShowConfigsInDialog && _SelectedEntityKey != null)
                    SelectedACClassProperty = ACClassPropertyList.FirstOrDefault();
                else
                    UpdateChangeLogList();
            }
        }

        private ACValueItemList _EntityKeyList;
        [ACPropertyList(403, "ChangeLogEntityKey")]
        public ACValueItemList EntityKeyList
        {
            get
            {
                return _EntityKeyList;
            }
            set
            {
                _EntityKeyList = value;
                OnPropertyChanged("EntityKeyList");
            }
        }

        private ACClassProperty _SelectedACClassProperty;
        [ACPropertySelected(404, "ChangeLogACClassProperty")]
        public ACClassProperty SelectedACClassProperty
        {
            get
            {
                return _SelectedACClassProperty;
            }
            set
            {
                _SelectedACClassProperty = value;
                OnPropertyChanged("SelectedACClassProperty");
                UpdateChangeLogList();
            }
        }

        [ACPropertyList(405, "ChangeLogACClassProperty")]
        public IEnumerable<ACClassProperty> ACClassPropertyList
        {
            get
            {
                if (SelectedACClass == null || SelectedEntityKey == null)
                    return null;

                List<Guid> entityKeys;
                if (!_EntityKeysMap.TryGetValue(SelectedEntityKey.ACCaption, out entityKeys))
                    return null;

                return SelectedACClass.ACChangeLog_ACClass.Where(c => c.ACClassID == SelectedACClass.ACClassID && entityKeys.Any(k => k == c.EntityKey))
                                                          .GroupBy(x => x.ACClassPropertyID).Select(t => t.FirstOrDefault().ACClassProperty).OrderBy(k => k.ACCaption);
            }
        }

        private ACChangeLog _SelectedChangeLog;
        [ACPropertySelected(406,"ChangeLog")]
        public ACChangeLog SelectedChangeLog
        {
            get
            {
                return _SelectedChangeLog;
            }
            set
            {
                _SelectedChangeLog = value;
                OnPropertyChanged("SelectedChangeLog");
            }
        }

        private IEnumerable<ACChangeLog> _ChangeLogList;
        [ACPropertyList(407, "ChangeLog")]
        public IEnumerable<ACChangeLog> ChangeLogList
        {
            get
            {
                return _ChangeLogList;

            }
            set
            {
                _ChangeLogList = value;
                OnPropertyChanged("ChangeLogList");
            }
        }

        private bool _ShowDeletedEntities = false;
        [ACPropertyInfo(408, "", "en{'Show deleted entities'}de{'Gelöschte Objekte anzeigen'}")]
        public bool ShowDeletedEntities
        {
            get
            {
                return _ShowDeletedEntities;
            }
            set
            {
                _ShowDeletedEntities = value;
                OnPropertyChanged("ShowDeletedEntities");
                LoadEntities();
            }
        }

        #endregion

        #region BSO -> ACMethod

        private void LoadEntityKeys()
        {
            if (SelectedACClass == null)
                return;

            IACEntityObjectContext tmpDatabase = GetContext(SelectedACClass);
            if (tmpDatabase == null)
                tmpDatabase = Db;

            var deletedEntities = Db.ACChangeLog.GroupBy(c => c.EntityKey).Where(x => x.Any(t => t.Deleted)).Select(k => k.FirstOrDefault().EntityKey).ToArray();

            var entityKeys = SelectedACClass.ACChangeLog_ACClass.GroupBy(x => x.EntityKey).Select(t => t.Key).ToArray();

            _EntityKeysMap = new Dictionary<string, List<Guid>>();
            ACValueItemList captions = new ACValueItemList("ACCaption");
            
            foreach(Guid entityKey in entityKeys)
            {
                if (deletedEntities.Any(c => c == entityKey))
                    continue;

                string keyName = GetType().Name + "ID";

                KeyValuePair<string, object> kvp = new KeyValuePair<string, object>(SelectedACClass.ACIdentifier+"ID", entityKey);
                KeyValuePair<string, object>[] kvpList = new KeyValuePair<string, object>[] { kvp };
                EntityKey eKey = new EntityKey(tmpDatabase.GetQualifiedEntitySetNameForEntityKey(_SelectedACClass.ACIdentifier), kvpList);
                object result;
                try
                {
                    if (tmpDatabase.TryGetObjectByKey(eKey, out result))
                    {
                        string caption = "";
                        IACConfig resultConfig = result as IACConfig;
                        if(resultConfig != null)
                            caption = resultConfig.LocalConfigACUrl;

                        else
                        {
                            IACObject resultACObject = result as IACObject;
                            if (resultACObject != null)
                                caption = resultACObject.ACIdentifier;
                        }

                        if(!string.IsNullOrEmpty(caption))
                        {
                            if (!_EntityKeysMap.ContainsKey(caption))
                            {
                                _EntityKeysMap.Add(caption, new List<Guid>() { entityKey });
                                captions.Add(new ACValueItem(caption, true, null));
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Messages.ExceptionAsync(this, e.Message, true);
                    EntityKeyList = null;
                    return;
                }
            }

            EntityKeyList = captions;
        }

        private void LoadEntityKeysDeleted()
        {
            ACValueItemList captions = new ACValueItemList("ACCaption");
            captions.AddRange(EntityKeyList);

            var deletedEntities = SelectedACClass.ACChangeLog_ACClass.GroupBy(c => c.EntityKey).Where(x => x.Any(t => t.Deleted));
            if (!deletedEntities.Any())
                return;

            foreach(var deletedEntity in deletedEntities)
            {
                var deletedInfoLog = deletedEntity.FirstOrDefault(c => c.Deleted);
                if (deletedInfoLog == null)
                    continue;

                ACChangeLogInfo info = new ACChangeLogInfo() { XMLValue = deletedInfoLog.XMLValue };
                if (string.IsNullOrEmpty(info.Info))
                    continue;

                string delInfo = string.Format("{0}", info.Info);

                List<Guid> keys;

                if(_EntityKeysMap.TryGetValue(delInfo, out keys))
                {
                    keys.Add(deletedInfoLog.EntityKey);
                }
                else
                {
                    _EntityKeysMap.Add(delInfo, new List<Guid>() { deletedInfoLog.EntityKey });
                    captions.Add(new ACValueItem(delInfo, false, null));
                }
            }

            EntityKeyList = captions;
        }

        private void LoadEntities()
        {
            if(!_IsShowConfigsInDialog)
                LoadEntityKeys();

            if (ShowDeletedEntities)
                LoadEntityKeysDeleted();
        }

        private void MapConfigs(IACConfig[] configs, Database db)
        {
            if (!configs.Any())
                return;

            SelectedACClass = null;

            Guid configStoreID = (configs[0].ACType as ACClass).ACClassID;
            SelectedACClass = db.ACClass.FirstOrDefault(c => c.ACClassID == configStoreID);

            if (SelectedACClass == null)
                return;

            _EntityKeysMap = new Dictionary<string, List<Guid>>();
            ACValueItemList captions = new ACValueItemList("ACCaption");

            foreach (IACConfig config in configs)
            {
                VBEntityObject entityObject = config as VBEntityObject;
                if (entityObject == null || entityObject.EntityKey == null || entityObject.EntityKey.EntityKeyValues == null)
                    continue;
                try
                {
                    Guid entityKey = (Guid)entityObject.EntityKey.EntityKeyValues[0].Value;
                    if (SelectedACClass.ACChangeLog_ACClass.Any(c => c.EntityKey == entityKey))
                    {
                        _EntityKeysMap.Add(config.LocalConfigACUrl, new List<Guid>() { entityKey });
                        captions.Add(new ACValueItem(config.LocalConfigACUrl, config.LocalConfigACUrl, null));
                    }
                }
                catch (Exception e)
                {
                    Messages.LogException("BSOChangeLog", "MapConfigs", e);
                    continue;
                }
            }

            EntityKeyList = captions;
        }

        private IACEntityObjectContext GetContext(ACClass acClass)
        {
            IACEntityObjectContext ctx = null;
            if (acClass == null)
                return ctx;

            ACClass parent = acClass.ACClass1_ParentACClass;
            if(typeof(IACEntityObjectContext).IsAssignableFrom(parent.ObjectFullType))
                 ctx = ACObjectContextManager.GetOrCreateContext(parent.ObjectType, ClassName + parent.ACIdentifier);
            return ctx;
        }

        private void UpdateChangeLogList()
        {
            if (SelectedACClass == null || SelectedEntityKey == null || SelectedACClassProperty == null)
            {
                ChangeLogList = null;
                return;
            }

            List<Guid> entityKeys;
            if (!_EntityKeysMap.TryGetValue(SelectedEntityKey.ACCaption, out entityKeys))
                return;

            var changelogs = SelectedACClassProperty.ACChangeLog_ACClassProperty.Where(c => entityKeys.Any(k => k == c.EntityKey))
                                           .OrderByDescending(x => x.ChangeDate).ToList();

            IACEntityObjectContext dbCtx = GetContext(SelectedACClass);
            if (dbCtx == null)
                return;

            try
            {
                changelogs.ForEach(x => x.ChangeLogValue = new ACValue(SelectedACClassProperty.ACIdentifier, 
                                                                       ACConvert.XMLToObject(SelectedACClassProperty.ObjectType, x.Deleted ? "" : x.XMLValue, true, dbCtx)));
                ChangeLogList = changelogs;
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                Messages.LogException("BSOChangeLog", "UpdateChangeLogList", msg);

                ChangeLogList = null;
            }
        }

        [ACMethodInfo("", "", 401)]
        public void ShowChangeLogForClass(Guid acClassID, Guid entityKey)
        {
            ShowDeletedEntities = false;

            using (Database db = new core.datamodel.Database())
            {
                ACClass selectedACClass = db.ACClass.FirstOrDefault(c => c.ACClassID == acClassID);
                if (selectedACClass == null || !selectedACClass.ACChangeLog_ACClass.Any())
                    return;

                SelectedACClass = selectedACClass;
                var caption = _EntityKeysMap.FirstOrDefault(c => c.Value.Any(x => x == entityKey)).Key;
                if (caption != null)
                {
                    SelectedEntityKey = new ACValueItem(caption, caption, null);
                    SelectedACClassProperty = ACClassPropertyList.FirstOrDefault();
                    ShowDialog(this, "ClassDialog");
                }
            }
        }

        [ACMethodInfo("", "", 402)]
        public bool IsEnabledLogForClass(Guid acClassID, Guid entityKey)
        {
            using (Database db = new core.datamodel.Database())
            {
                return db.ACChangeLog.Any(c => c.ACClassID == acClassID && c.EntityKey == entityKey);
            }
        }

        [ACMethodInfo("", "", 403)]
        public void ShowChangeLogForProperty(Guid entityKey, Guid acClassPropertyID)
        {
            _ShowDeletedEntities = false;

            using (Database db = new core.datamodel.Database())
            {
                ACClassProperty acClassProperty = db.ACClassProperty.FirstOrDefault(c => c.ACClassPropertyID == acClassPropertyID);
                if (acClassProperty == null)
                    return;

                var changeLogs = acClassProperty.ACChangeLog_ACClassProperty.Where(c =>  c.EntityKey == entityKey).OrderByDescending(x => x.ChangeDate).ToList();
                changeLogs.ForEach(x => x.ChangeLogValue = new ACValue(acClassProperty.ACIdentifier, ACConvert.XMLToObject(acClassProperty.ObjectType, x.XMLValue, true, db)));
                ChangeLogList = changeLogs;
                ShowDialog(this, "PropertyDialog");
            }
        }

        [ACMethodInfo("", "", 404)]
        public bool IsEnabledLogForProperty(Guid entityKey, Guid acClassPropertyID)
        {
            using (Database db = new core.datamodel.Database())
            {
                return db.ACChangeLog.Any(c => c.ACClassPropertyID == acClassPropertyID && c.EntityKey == entityKey);
            }
        }

        [ACMethodInfo("", "", 405)]
        public void ShowChangeLogForWFConfig(IACConfig[] configs)
        {
            _ShowDeletedEntities = false;
            _IsShowConfigsInDialog = true;

            using (Database db = new core.datamodel.Database())
            {
                MapConfigs(configs, db);

                SelectedEntityKey = EntityKeyList.FirstOrDefault();
                SelectedACClassProperty = ACClassPropertyList.FirstOrDefault();

                ShowDialog(this, "ConfigDialog");
            }
            _IsShowConfigsInDialog = false;
        }

        [ACMethodInfo("", "", 406)]
        public bool IsEnabledLogForWFConfig(IACConfig[] configs)
        {
            foreach (IACConfig config in configs)
            {
                VBEntityObject entityObject = config as VBEntityObject;
                if (entityObject == null || entityObject.EntityKey == null || entityObject.EntityKey.EntityKeyValues == null)
                    return false;
                try
                {
                    Guid entityKey = (Guid)entityObject.EntityKey.EntityKeyValues[0].Value;
                    if (Db.ACChangeLog.Any(c => c.EntityKey == entityKey))
                        return true;
                }
                catch (Exception e)
                {
                    Messages.LogException("BSOChangeLog", "IsEnabledLogForWFConfig", e);
                }
            }
            return false;
        }

        [ACMethodInfo("", "", 407)]
        public void ShowMainLayout()
        {
            SelectedEntityKey = null;
            SelectedACClass = ACClassList.FirstOrDefault();
            ShowWindow(this, "Mainlayout", true, Global.VBDesignContainer.DockableWindow, Global.VBDesignDockState.Tabbed, Global.VBDesignDockPosition.Bottom, Global.ControlModes.Enabled);
        }

        public bool IsEnabledShowMainLayout()
        {
            return true;
        }

        [ACMethodInfo("", "en{'Refresh change logs'}de{'Änderungsprotokolle aktualisieren'}", 408, true)]
        public void RefreshChangeLogs()
        {
            if (BackgroundWorker.IsBusy)
                return;

            BackgroundWorker.RunWorkerAsync("RefreshChangeLogs");

            CurrentProgressInfo.ProgressInfoIsIndeterminate = true;
            ShowDialog(this, DesignNameProgressBar);
            CurrentProgressInfo.ProgressInfoIsIndeterminate = false;
        }

        public override void BgWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            base.BgWorkerDoWork(BackgroundWorker, e);

            if(e.Argument.ToString() == "RefreshChangeLogs")
            {
                CurrentProgressInfo.TotalProgress.ProgressText = Root.Environment.TranslateText(this, "RefreshProgress");
                // TODO: DatabaseRefresh not implemented
                //Database.Refresh(RefreshMode.StoreWins, Db.ACChangeLog);
                OnPropertyChanged("ACClassList");
                CurrentProgressInfo.Complete();
            }
        }

        #endregion

        #region Execute-Helper-Handlers

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "ShowChangeLogForClass":
                    ShowChangeLogForClass((Guid)acParameter[0], (Guid)acParameter[1]);
                    return true;
                case "IsEnabledLogForClass":
                    result = IsEnabledLogForClass((Guid)acParameter[0], (Guid)acParameter[1]);
                    return true;
                case "ShowChangeLogForProperty":
                    ShowChangeLogForProperty((Guid)acParameter[0], (Guid)acParameter[1]);
                    return true;
                case "IsEnabledLogForProperty":
                    result = IsEnabledLogForProperty((Guid)acParameter[0], (Guid)acParameter[1]);
                    return true;
                case "ShowMainLayout":
                    ShowMainLayout();
                    return true;
                case "IsEnabledLogForWFConfig":
                    result = IsEnabledLogForWFConfig(acParameter as IACConfig[]);
                    return true;
                case "ShowChangeLogForWFConfig":
                    ShowChangeLogForWFConfig(acParameter[0] as IACConfig[]);
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }
}
