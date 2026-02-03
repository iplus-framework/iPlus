// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;
using gip.core.datamodel;
using gip.core.autocomponent;
using System.IO;
using System.Diagnostics;

namespace gip.core.communication
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Export mode of property logs'}de{'Exportmodus des Eigenschaftslogs'}", Global.ACKinds.TACEnum)]
    public enum PAPropertyLogExportMode : short
    {
        /// <summary>
        /// Each variable will be exported into a seperate file. 
        /// All values which occured since the last export will be exported in chronological order.
        /// The Fileformat is sequentially: ACUrl;Time;Value;
        /// </summary>
        EachVariableInSeperateFile_Chronological = 0,

        /// <summary>
        /// Each variable will be exported into a seperate file. 
        /// Only one (latest) value will be exported.
        /// The Fileformat is sequentially: ACUrl;Time;Value;
        /// </summary>
        EachVariableInSeperateFile_OnlyLatestValue = 10,

        /// <summary>
        /// All variables will be exported into a single file. 
        /// All values which occured since the last export will be exported in chronological order.
        /// The Fileformat is sequentially: ACUrl;Time;Value;
        /// </summary>
        OneFile_Chronological = 100,

        /// <summary>
        /// All variables will be exported into a single file. 
        /// The export is grouped by each variable.
        /// All values which occured since the last export will be exported in chronological order.
        /// The Fileformat is sequentially: ACUrl;Time;Value;
        /// </summary>
        OneFile_Grouped = 110,

        /// <summary>
        /// All variables will be exported into a single file. 
        /// Only one (latest) value will be exported per variable.
        /// The Fileformat is sequentially: ACUrl;Time;Value;
        /// </summary>
        OneFile_OnlyLastValue = 120,


        /// <summary>
        /// All variables will be exported into a single file. 
        /// Only one (latest) value will be exported per variable.
        /// The Fileformat is in table-form: Time, Value1; Value2; Value3....
        /// </summary>
        OneFile_AsTableView = 130
    }

    /// <summary>
    /// Group for PAFilePropertyLogExport
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Propertylog Export'}de{'Propertylog Export'}", Global.ACKinds.TACDAClass, Global.ACStorableTypes.Required, false, false)]
    public class PAFilePropertyLogGroup : PAFileCyclicGroupBase
    {

        private class PAFilePropertyLogItem
        {
            public PropertyLogItem LogItem { get; set; }
            public IACPropertyNetBase ACProperty { get; set; }
            public IACConfig ACConfig { get; set; }
        }

        #region c´tors
        public PAFilePropertyLogGroup(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            bool result = base.ACInit(startChildMode);
            return result;
        }

        public override bool ACPostInit()
        {
            return base.ACPostInit();
        }

        public override async Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            bool result = await base.ACDeInit(deleteACClassTask);
            return result;
        }

        #endregion

        #region Properties

        #region Configuration
        private PAPropertyLogExportMode _ExportMode;
        [ACPropertyInfo(true, 300, "Configuration", "en{'Export mode'}de{'Exportmodus'}", "", true)]
        public PAPropertyLogExportMode ExportMode
        {
            get
            {
                return _ExportMode;
            }
            set
            {
                _ExportMode = value;
                OnPropertyChanged("ExportMode");
            }
        }

        public Boolean ExpAllVariablesInOneFile
        {
            get
            {
                return ExportMode >= PAPropertyLogExportMode.OneFile_Chronological;
            }
        }

        public Boolean ExpGroupByVariable
        {
            get
            {
                return ExportMode == PAPropertyLogExportMode.OneFile_Grouped;
            }
        }

        public Boolean ExpOnlyLatestValue
        {
            get
            {
                return    ExportMode == PAPropertyLogExportMode.EachVariableInSeperateFile_OnlyLatestValue
                       || ExportMode == PAPropertyLogExportMode.OneFile_OnlyLastValue
                       || ExportMode == PAPropertyLogExportMode.OneFile_AsTableView;
            }
        }

        public Boolean ExpAsTableView
        {
            get
            {
                return ExportMode == PAPropertyLogExportMode.OneFile_AsTableView;
            }
        }

        private IACConfig _CurrentACConfig;
        [ACPropertyInfo(9999)]
        public IACConfig CurrentACConfig
        {
            get
            {
                return _CurrentACConfig;
            }
        }

        private PAFilePropertyLogConfig _CurrentConfig;
        [ACPropertyInfo(9999)]
        public PAFilePropertyLogConfig CurrentConfig
        {
            get
            {
                return _CurrentConfig;
            }
        }

        private IACPropertyNetBase _CurrentACProperty;
        [ACPropertyInfo(9999)]
        public IACPropertyNetBase CurrentACProperty
        {
            get
            {
                return _CurrentACProperty;
            }
        }

        private int _CurrentExportLoop;
        [ACPropertyInfo(9999)]
        public int CurrentExportLoop
        {
            get
            {
                return _CurrentExportLoop;
            }
        }
        #endregion

        #region virtual and overridden
        protected override string DefaultFileName
        {
            get
            {
                string propertyName = "";
                if (!String.IsNullOrEmpty(CurrentConfig.PartOfFileName))
                    propertyName = CurrentConfig.PartOfFileName;
                else
                    propertyName = String.Format("{0}_{1}", CurrentACProperty.ACIdentifier, CurrentExportLoop);

                string fileNameFormat = FilenameFormat;
                if (String.IsNullOrEmpty(fileNameFormat))
                    fileNameFormat = "{0}_{1:yyyyMMdd_HHmmss}.txt";
                string fileName = "";
                try
                {
                    if (ExpAllVariablesInOneFile)
                        fileName = String.Format(fileNameFormat, this.ACIdentifier, DateTime.Now);
                    else
                        fileName = String.Format(fileNameFormat, propertyName, DateTime.Now);
                }
                catch (Exception e)
                {
                    IsExportingAlarm.ValueT = PANotifyState.AlarmOrFault;
                    Msg msg = new Msg(e.Message, this, eMsgLevel.Exception, "PAFilePropertyLogGroup", "DefaultFileName", 1000);
                    ErrorText.ValueT = msg.Message;

                    Messages.LogException(this.GetACUrl(), "PAFilePropertyLogGroup.DefaultFileName(Format)", e.Message);
                    OnNewAlarmOccurred(IsExportingAlarm, msg, true);
                }
                if (String.IsNullOrEmpty(fileName))
                    fileName = base.DefaultFileName;
                return fileName;
            }
        }


        protected override string DefaultSubFolderName
        {
            get
            {
                string folderName = "";
                if (!_HasScriptDefaultSubFolderNameMethod.HasValue)
                    _HasScriptDefaultSubFolderNameMethod = ACClassMethods.Where(c => c.ACIdentifier == "ScriptDefaultSubFolderName").Any();
                if (_HasScriptDefaultSubFolderNameMethod.Value)
                    folderName = ACUrlCommand("!ScriptDefaultSubFolderName") as string;

                if (!String.IsNullOrEmpty(folderName))
                    return folderName;
                folderName = this.ACIdentifier;
                if (!ExpAllVariablesInOneFile)
                {
                    string propertyName = "";
                    if (!String.IsNullOrEmpty(CurrentConfig.PartOfFileName))
                        propertyName = CurrentConfig.PartOfFileName;
                    else
                        propertyName = String.Format("{0}_{1}", CurrentACProperty.ACIdentifier, CurrentExportLoop);
                    folderName = propertyName;
                }
                return this.ACIdentifier;
            }
        }
        #endregion

        #endregion

        #region Points
        [ACPropertyPointConfig(9999, "", typeof(PAFilePropertyLogConfig), "en{'Variables to export'}de{'Zu exportierende Variablen'}")]
        public IEnumerable<IACConfig> PropertiesToExport
        {
            get
            {
                string keyACUrl = ".\\ACClassProperty(PropertiesToExport)";
                List<ACClassConfig> result = null;
                ACClassTaskQueue.TaskQueue.ProcessAction(() => 
                {
                    try
                    {
                        ACTypeFromLiveContext.ACClassConfig_ACClass.AutoLoad(ACTypeFromLiveContext.ACClassConfig_ACClassReference, ACTypeFromLiveContext);
                        var query = ACTypeFromLiveContext.ACClassConfig_ACClass.Where(c => c.KeyACUrl == keyACUrl);
                        if (query.Any())
                            result = query.ToList();
                        else
                            result = new List<ACClassConfig>();
                    }
                    catch (Exception e)
                    {
                        Messages.LogException(this.GetACUrl(), "PropertiesToExport", e.Message);
                    }
                });
                return result;
            }
        }
        #endregion

        #region Methods

        #region public methods
        public override Msg DoExport(string exportPath, DateTime fromDate, DateTime toDate)
        {
            if (PropertiesToExport == null || !PropertiesToExport.Any() || !this.Root.Initialized)
                return null;
            SortedList<DateTime, PAFilePropertyLogItem> sortedExportList = new SortedList<DateTime, PAFilePropertyLogItem>();
            List<PAFilePropertyLogItem> tableExportList = new List<PAFilePropertyLogItem>();
            // TODO: Open Files, FileFormat

            var propertyQuery = PropertiesToExport;
            if (propertyQuery == null || !propertyQuery.Any())
                return null;
            IEnumerable<IACConfig> exportList = propertyQuery.Where(c => c.Value != null
                                                && (c.Value is PAFilePropertyLogConfig)
                                                && (c.Value as PAFilePropertyLogConfig).ExportOff == false
                                                && !String.IsNullOrEmpty(c.LocalConfigACUrl))
                                    .OrderBy(c => (c.Value as PAFilePropertyLogConfig).ExportSortIndex);
                                    //.Select(c => c.Value as PAFilePropertyLogConfig);
            if (exportList == null || !exportList.Any())
                return null;

            _CurrentExportLoop = 0;
            _CurrentConfig = null;
            _CurrentACConfig = null;
            _CurrentACProperty = null;

            foreach (IACConfig acConfig in exportList)
            {
                _CurrentACConfig = acConfig;
                _CurrentConfig = acConfig.Value as PAFilePropertyLogConfig;
                _CurrentExportLoop++;
                int nPos = acConfig.LocalConfigACUrl.LastIndexOf('\\');
                if (nPos < 2 || (nPos >= acConfig.LocalConfigACUrl.Length - 1))
                {
                    IsExportingAlarm.ValueT = PANotifyState.AlarmOrFault;
                    //Error50195: acConfig.LocalConfigACUrl is empty.
                    Msg msg = new Msg(this, eMsgLevel.Error, "PAFilePropertyLogGroup", "DoExport()", 1010, "Error50195");
                    ErrorText.ValueT = msg.Message;
                    Messages.LogError(this.GetACUrl(), "PAFilePropertyLogGroup.DoExport()", ErrorText.ValueT);
                    OnNewAlarmOccurred(IsExportingAlarm, msg, true);
                    continue;
                }
                string acUrlComponent = acConfig.LocalConfigACUrl.Substring(0, nPos);
                string propertyName = acConfig.LocalConfigACUrl.Substring(nPos + 1);
                ACComponent acComponent = ACUrlCommand("?" + acUrlComponent) as ACComponent;
                if (acComponent == null)
                {
                    IsExportingAlarm.ValueT = PANotifyState.AlarmOrFault;
                    //Error50196: Component {0} not found or no access rights.
                    Msg msg = new Msg(this, eMsgLevel.Error, "PAFilePropertyLogGroup", "DoExport(10)", 1020, "Error50196", acUrlComponent);
                    ErrorText.ValueT = msg.Message;
                    Messages.LogError(this.GetACUrl(), "PAFilePropertyLogGroup.DoExport()", ErrorText.ValueT);
                    OnNewAlarmOccurred(IsExportingAlarm, msg, true);
                    continue;
                }
                _CurrentACProperty = acComponent.GetPropertyNet(propertyName);
                if (_CurrentACProperty == null)
                {
                    IsExportingAlarm.ValueT = PANotifyState.AlarmOrFault;
                    //Error50197: Property {0} not found or no access rights.
                    Msg msg = new Msg(this, eMsgLevel.Error, "PAFilePropertyLogGroup", "DoExport(20)", 1030, "Error50197", acConfig.LocalConfigACUrl);
                    ErrorText.ValueT = msg.Message;
                    Messages.LogError(this.GetACUrl(), "PAFilePropertyLogGroup.DoExport()", ErrorText.ValueT);
                    OnNewAlarmOccurred(IsExportingAlarm, msg, true);
                    continue;
                }

                var logRefreshRate = (_CurrentACProperty.ACType as ACClassProperty).LogRefreshRate;

                DateTime corrFromDate = fromDate;
                if (ExpOnlyLatestValue && _CurrentConfig.Interpolation != Global.InterpolationMethod.None)
                {
                    double? estimatedDurationForOneValue = null;
                    switch (logRefreshRate)
                    {
                        case Global.MaxRefreshRates.Off:
                            break;
                        case Global.MaxRefreshRates.EventDriven:
                            estimatedDurationForOneValue = 1.0 / 20;
                            break;
                        case Global.MaxRefreshRates.R100ms:
                            estimatedDurationForOneValue = 1.0 / 10;
                            break;
                        case Global.MaxRefreshRates.R200ms:
                            estimatedDurationForOneValue = 1.0 / 5;
                            break;
                        case Global.MaxRefreshRates.R500ms:
                            estimatedDurationForOneValue = 1.0 / 2;
                            break;
                        case Global.MaxRefreshRates.R1sec:
                            estimatedDurationForOneValue = 1;
                            break;
                        case Global.MaxRefreshRates.R2sec:
                            estimatedDurationForOneValue = 2.0;
                            break;
                        case Global.MaxRefreshRates.R5sec:
                            estimatedDurationForOneValue = 5.0;
                            break;
                        case Global.MaxRefreshRates.R10sec:
                            estimatedDurationForOneValue = 10.0;
                            break;
                        case Global.MaxRefreshRates.R20sec:
                            estimatedDurationForOneValue = 20.0;
                            break;
                        case Global.MaxRefreshRates.R1min:
                            estimatedDurationForOneValue = 60.0;
                            break;
                        case Global.MaxRefreshRates.R2min:
                            estimatedDurationForOneValue = 60 * 2;
                            break;
                        case Global.MaxRefreshRates.R5min:
                            estimatedDurationForOneValue = 60 * 5;
                            break;
                        case Global.MaxRefreshRates.R10min:
                            estimatedDurationForOneValue = 60 * 10;
                            break;
                        case Global.MaxRefreshRates.R20min:
                            estimatedDurationForOneValue = 60 * 20;
                            break;
                        case Global.MaxRefreshRates.Hourly:
                            estimatedDurationForOneValue = 60 * 60;
                            break;
                        case Global.MaxRefreshRates.Daily:
                            estimatedDurationForOneValue = 60 * 60 * 24;
                            break;
                        case Global.MaxRefreshRates.Weekly:
                            estimatedDurationForOneValue = 60 * 60 * 24 * 7;
                            break;
                        case Global.MaxRefreshRates.Monthly:
                            estimatedDurationForOneValue = 60 * 60 * 24 * 30;
                            break;
                        case Global.MaxRefreshRates.Yearly:
                            estimatedDurationForOneValue = 60 * 60 * 24 * 356;
                            break;
                    }

                    if (estimatedDurationForOneValue.HasValue)
                    {
                        double timeRangeSec = (toDate - fromDate).TotalSeconds;
                        double minTimeRangeSec = 20 * estimatedDurationForOneValue.Value; // 20 Values minimum needed for interpolation
                        if (_CurrentConfig.EnlargeTimeRange.HasValue)
                            minTimeRangeSec *= _CurrentConfig.EnlargeTimeRange.Value;
                        if (minTimeRangeSec > timeRangeSec)
                            corrFromDate = fromDate.AddSeconds(minTimeRangeSec * -1);
                    }
                }

                PropertyLogListInfo logListInfo = logRefreshRate != Global.MaxRefreshRates.Off ? _CurrentACProperty.GetArchiveLog(corrFromDate, toDate) : null;
                if (logListInfo == null && logRefreshRate != Global.MaxRefreshRates.Off)
                {
                    IsExportingAlarm.ValueT = PANotifyState.AlarmOrFault;
                    //Error50198: Logging for Property {0} not activated.
                    Msg msg = new Msg(this, eMsgLevel.Error, "PAFilePropertyLogGroup", "DoExport(30)", 1040, "Error50198", acConfig.LocalConfigACUrl);
                    ErrorText.ValueT = msg.Message;
                    Messages.LogError(this.GetACUrl(), "PAFilePropertyLogGroup.DoExport()", ErrorText.ValueT);
                    OnNewAlarmOccurred(IsExportingAlarm, msg, true);
                    continue;
                }
                else if (logListInfo == null || logListInfo.PropertyLogList == null || !logListInfo.PropertyLogList.Any())
                {
                    if (ExpOnlyLatestValue)
                    {
                        logListInfo = new PropertyLogListInfo(Global.MaxRefreshRates.Off, 
                                                                new PropertyLogItem[] { 
                                                                       new PropertyLogItem() { 
                                                                           Time = DateTime.Now, 
                                                                           Value = _CurrentACProperty.Value 
                                                                       } 
                                                                });
                    }
                    else
                        continue;
                }

                if (ExpOnlyLatestValue 
                    && _CurrentConfig.Interpolation != Global.InterpolationMethod.None 
                    && _CurrentConfig.InterpolationRange > 0
                    && logListInfo.PropertyLogList != null)
                {
                    if (!logListInfo.IsInterpolationPossible(_CurrentConfig.Interpolation, _CurrentConfig.InterpolationRange))
                    {
                        if (!_CurrentConfig.EnlargeTimeRange.HasValue || _CurrentConfig.EnlargeTimeRange <= 8)
                        {
                            _CurrentConfig.EnlargeTimeRange = _CurrentConfig.EnlargeTimeRange.HasValue ? _CurrentConfig.EnlargeTimeRange *= 2 : 2;
                            string infoText = String.Format("Time-Range-Mupltiplier was increased to {3}, because the count of values were not enough for the interpolation method. Current Time-Range was ({1} - {2}) for Property {0}.", acConfig.LocalConfigACUrl, corrFromDate, toDate, _CurrentConfig.EnlargeTimeRange);
                            Messages.LogInfo(this.GetACUrl(), "PAFilePropertyLogGroup.DoExport(31)", infoText);
                        }
                    }
                    else
                    {
                        logListInfo.SetInterpolationParams(_CurrentConfig.Interpolation, _CurrentConfig.InterpolationRange, _CurrentConfig.InterpolationDecay);
                        logListInfo.Interpolate();
                    }
                }

                if (ExpAllVariablesInOneFile && !ExpGroupByVariable)
                {
                    if (ExpOnlyLatestValue)
                    {
                        PropertyLogItem logItem = logListInfo.PropertyLogList.LastOrDefault();
                        PAFilePropertyLogItem newItem = new PAFilePropertyLogItem { LogItem = logItem, ACProperty = _CurrentACProperty, ACConfig = acConfig };
                        if (this.ExpAsTableView)
                            tableExportList.Add(newItem);
                        else
                        {
                            try { sortedExportList.Add(logItem.Time, newItem); }
                            catch(Exception e)
                            {
                                string msg = e.Message;
                                if (e.InnerException != null && e.InnerException.Message != null)
                                    msg += " Inner:" + e.InnerException.Message;

                                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                                    datamodel.Database.Root.Messages.LogException("PAFileProprtyLogGroup", "DoExport(40)", msg);
                            }
                        }
                    }
                    else
                    {
                        foreach (PropertyLogItem logItem in logListInfo.PropertyLogList)
                        {
                            PAFilePropertyLogItem newItem = new PAFilePropertyLogItem { LogItem = logItem, ACProperty = _CurrentACProperty, ACConfig = acConfig };
                            try { sortedExportList.Add(logItem.Time, newItem); }
                            catch (Exception e)
                            {
                                string msg = e.Message;
                                if (e.InnerException != null && e.InnerException.Message != null)
                                    msg += " Inner:" + e.InnerException.Message;

                                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                                    datamodel.Database.Root.Messages.LogException("PAFileProprtyLogGroup", "DoExport(50)", msg);
                            }
                        }
                    }
                }
                // Sonst direkt wegschreiben
                else
                {
                    if (!ExpAllVariablesInOneFile)
                    {
                        if (!OpenFile(exportPath, DefaultFileName))
                            continue;
                        OnFileOpened(_CurrentExportLoop, acConfig, _CurrentConfig, _CurrentACProperty);

                        int j = 0;
                        if (ExpOnlyLatestValue)
                        {
                            PropertyLogItem logItem = logListInfo.PropertyLogList.LastOrDefault();
                            OnExportLogItem(logItem, _CurrentExportLoop, j, _CurrentACProperty, acConfig, _CurrentConfig, ItemExportRowType.Data);
                        }
                        else
                        {
                            foreach (PropertyLogItem logItem in logListInfo.PropertyLogList)
                            {
                                OnExportLogItem(logItem, _CurrentExportLoop, j, _CurrentACProperty, acConfig, _CurrentConfig, ItemExportRowType.Data);
                            }
                        }

                        OnClosingFile(_CurrentExportLoop, acConfig, _CurrentConfig, _CurrentACProperty);
                        CloseFile();
                    }
                    // ExpGroupByVariable
                    else 
                    {
                        if (!OpenFile(exportPath, DefaultFileName))
                            continue;

                        OnFileOpened(_CurrentExportLoop, acConfig, _CurrentConfig, _CurrentACProperty);

                        int j = 0;
                        if (ExpOnlyLatestValue)
                        {
                            PropertyLogItem logItem = logListInfo.PropertyLogList.LastOrDefault();
                            OnExportLogItem(logItem, _CurrentExportLoop, j, _CurrentACProperty, acConfig, _CurrentConfig, ItemExportRowType.Data);
                        }
                        else
                        {
                            foreach (PropertyLogItem logItem in logListInfo.PropertyLogList)
                            {
                                OnExportLogItem(logItem, _CurrentExportLoop, j, _CurrentACProperty, acConfig, _CurrentConfig, ItemExportRowType.Data);
                            }
                        }
                    }
                }
            }

            if (ExpAllVariablesInOneFile && !ExpGroupByVariable)
            {
                if ((sortedExportList.Any() || tableExportList.Any()) && OpenFile(exportPath, DefaultFileName))
                {
                    int j = 0;
                    _CurrentACConfig = null;
                    _CurrentConfig = null;
                    _CurrentACProperty = null;
                    IList<PAFilePropertyLogItem> loopList = tableExportList;
                    if (sortedExportList.Any())
                        loopList = sortedExportList.Values;
                    // If Export Table, then write Headers first
                    if (!IsAppendingToStream && ExpAsTableView)
                    {
                        foreach (PAFilePropertyLogItem itemToExport in loopList)
                        {
                            _CurrentACConfig = itemToExport.ACConfig;
                            _CurrentConfig = _CurrentACConfig.Value as PAFilePropertyLogConfig;
                            _CurrentACProperty = itemToExport.ACProperty;
                            OnExportLogItem(itemToExport.LogItem, _CurrentExportLoop, j, _CurrentACProperty, _CurrentACConfig, _CurrentConfig, ItemExportRowType.Header);
                            j++;
                        }
                    }
                    j = 0;
                    _CurrentACConfig = null;
                    _CurrentConfig = null;
                    _CurrentACProperty = null;
                    foreach (PAFilePropertyLogItem itemToExport in loopList)
                    {
                        _CurrentACConfig = itemToExport.ACConfig;
                        _CurrentConfig = _CurrentACConfig.Value as PAFilePropertyLogConfig;
                        _CurrentACProperty = itemToExport.ACProperty;
                        OnExportLogItem(itemToExport.LogItem, _CurrentExportLoop, j, _CurrentACProperty, _CurrentACConfig, _CurrentConfig, ItemExportRowType.Data);
                        j++;
                    }
                    if  (ExpAsTableView)
                        OnExportLogItem(null, _CurrentExportLoop, j, null, null, null, ItemExportRowType.EndOfTableRow);

                    OnClosingFile(0, _CurrentACConfig, _CurrentConfig, _CurrentACProperty);
                    CloseFile();
                }
            }
            else if (ExpAllVariablesInOneFile)
            {
                OnClosingFile(_CurrentExportLoop, _CurrentACConfig, _CurrentConfig, _CurrentACProperty);
                CloseFile();
            }

            return null;
        }
        #endregion

        #region virtual and overridden
        // Property to avoid unnecessary calls to script methods if they doesn't exist
        private bool? _HasScriptOnFileOpenedMethod = null;
        protected virtual void OnFileOpened(int indexProperty, IACConfig acConfig, PAFilePropertyLogConfig config, IACPropertyNetBase acProperty)
        {
            if (!_HasScriptOnFileOpenedMethod.HasValue)
                _HasScriptOnFileOpenedMethod = ACClassMethods.Where(c => c.ACIdentifier == "ScriptOnFileOpened").Any();
            if (!_HasScriptOnFileOpenedMethod.Value)
                return;
            ACUrlCommand("!ScriptOnFileOpened", _StreamWriter, indexProperty, acConfig, config, acProperty);
        }

        // Property to avoid unnecessary calls to script methods if they doesn't exist
        private bool? _HasScriptOnClosingFileMethod = null;
        protected virtual void OnClosingFile(int indexProperty, IACConfig acConfig, PAFilePropertyLogConfig config, IACPropertyNetBase acProperty)
        {
            if (!_HasScriptOnClosingFileMethod.HasValue)
                _HasScriptOnClosingFileMethod = ACClassMethods.Where(c => c.ACIdentifier == "ScriptOnClosingFile").Any();
            if (!_HasScriptOnClosingFileMethod.Value)
                return;
            ACUrlCommand("!ScriptOnClosingFile", _StreamWriter, indexProperty, acConfig, config, acProperty);
        }

        private ACClassMethod _ScriptOnExportLogItemMethod = null;
        private bool _CheckedScriptOnExportLogItemMethod = false;
        private StringBuilder _StringBuilder = null;
        protected enum ItemExportRowType
        {
            Data,
            Header,
            EndOfTableRow
        }

        protected virtual void OnExportLogItem(PropertyLogItem logItem, int indexProperty, int indexLogItem, IACPropertyNetBase acProperty, IACConfig acConfig, PAFilePropertyLogConfig config, ItemExportRowType rowType)
        {
            if (!_CheckedScriptOnExportLogItemMethod)
            {
                _ScriptOnExportLogItemMethod = ComponentClass.GetMethod("ScriptOnExportLogItem");
                _CheckedScriptOnExportLogItemMethod = true;
            }

            if (_ScriptOnExportLogItemMethod == null)
            {
                if (ExpAsTableView)
                {
                    // If Header shuld be written:
                    if (rowType == ItemExportRowType.Header)
                    {
                        if (_StringBuilder == null)
                        {
                            _StringBuilder = new StringBuilder();
                            _StringBuilder.Append("DateTime;");
                        }
                        if (!String.IsNullOrEmpty(config.ACIdentifier))
                            _StringBuilder.AppendFormat("{0};", config.ACIdentifier);
                        else
                            _StringBuilder.AppendFormat("{0};", acConfig.LocalConfigACUrl);
                    }
                    // If End of Row
                    else if (rowType == ItemExportRowType.EndOfTableRow)
                    {
                        if (_StringBuilder != null)
                        {
                            _StreamWriter.WriteLine(_StringBuilder.ToString());
                        }
                        _StringBuilder = null;
                    }
                    else
                    {
                        // If Header created, then write to file:
                        if (_StringBuilder != null && indexLogItem == 0)
                        {
                            _StreamWriter.WriteLine(_StringBuilder.ToString());
                            _StringBuilder = null;
                        }
                        if (_StringBuilder == null)
                        {
                            _StringBuilder = new StringBuilder();
                            _StringBuilder.AppendFormat("{0};", DateTime.Now);
                        }
                        _StringBuilder.AppendFormat("{0};", FormatExportValue(logItem));
                    }
                }
                else
                {
                    _StreamWriter.WriteLine(String.Format("{0};{1};{2};", acConfig.LocalConfigACUrl, logItem.Time, FormatExportValue(logItem)));
                }
            }
            else
                InvokeScriptMethod(AsyncMethodInvocationMode.Synchronous, _ScriptOnExportLogItemMethod, new object[] { _StreamWriter, logItem, indexProperty, indexLogItem, acProperty, acConfig, config, rowType });
        }

        protected virtual object FormatExportValue(PropertyLogItem logItem)
        {
            if (logItem.Value == null)
                return "NULL";
            else if (logItem.Value is String)
            {
                string strValue = logItem.Value as string;
                if (String.IsNullOrEmpty(strValue))
                    return strValue;
                strValue = strValue.Replace("\r", String.Empty).Replace("\n", String.Empty).Replace(";", " ");
                return strValue;
            }
            return logItem.Value;
        }

        #endregion

        #endregion
    }
}
