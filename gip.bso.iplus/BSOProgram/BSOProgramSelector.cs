using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gip.core.datamodel;
using gip.core.autocomponent;
using System.Runtime.Serialization;
using System.IO;
using System.IO.Compression;
using System.Xml;
using System.Windows.Threading;
using ClosedXML;
using ClosedXML.Excel;
using System.Data;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace gip.bso.iplus
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Program selector'}de{'Programmauswahl'}", Global.ACKinds.TACBSOGlobal, Global.ACStorableTypes.NotStorable, false, true)]
    public class BSOProgramSelector : ACBSO
    {
        #region c'tors
        public BSOProgramSelector(gip.core.datamodel.ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {

        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            bool init = base.ACInit(startChildMode);
            _refreshTimer = new DispatcherTimer();
            _refreshTimer.Tick += _refreshTimer_Tick;
            _refreshTimer.Interval = new TimeSpan(0, 0, 0, 1);
            return init;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            if(_refreshTimer != null)
                _refreshTimer.Tick -= _refreshTimer_Tick;
            _refreshTimer = null;
            CurrentACProgram = null;
            ArchiveRemainingDays = null;
            return base.ACDeInit(deleteACClassTask);
        }
        #endregion

        #region Private members

        private DispatcherTimer _refreshTimer;
        private int _refreshTimerCounter = 0;
        private List<core.datamodel.ACProgram> _ACProgramListCopy;
        private bool _IsEnabledRestoreArchivedProgramLog = true;

        #endregion

        #region Properties

        public IACBSOACProgramProvider ProgramProvider
        {
            get
            {
                return FindParentComponent<IACBSOACProgramProvider>(c => c is IACBSOACProgramProvider);
            }
        }

        private gip.core.datamodel.ACProgram _CurrentACProgram;
        [ACPropertyCurrent(401,"Program","en{'Program'}de{'Programm'}")]
        public gip.core.datamodel.ACProgram CurrentACProgram
        {
            get 
            {
                return _CurrentACProgram;
            }
            set
            {
                _CurrentACProgram = value;
                SetArchiveRemainingDays();
                OnPropertyChanged("CurrentACProgram");
            }
        }

        [ACPropertyList(402,"Program")]
        public IEnumerable<gip.core.datamodel.ACProgram> ACProgramList
        {
            get 
            {
                if (ProgramProvider != null)
                    return ProgramProvider.GetACProgram();
                return null;
            }  
        }   

        public bool IsVisibleInCurrentContext
        {
            get
            {
                return ProgramProvider != null /* && ProgramProvider.IsEnabledACProgram && ACProgramList.Any()*/;
            }
        }

        private string _ArchiveRemainingDays;
        [ACPropertyInfo(403, "", "")]
        public string ArchiveRemainingDays
        {
            get
            {
                return _ArchiveRemainingDays;
            }
            set
            {
                _ArchiveRemainingDays = value;
                OnPropertyChanged("ArchiveRemainingDays");
            }
        }

        #endregion

        #region Methods

        private void SetArchiveRemainingDays()
        {
            if (!IsEnabledArchiveProgramLogManual())
            {
                ArchiveRemainingDays = "";
                return;
            }

            int? archiveAfterDays = (int?)ACUrlCommand("\\Service\\ProgramLogArchive\\ProgramLogArchiveGroup\\ArchiveAfterDays");
            if (!archiveAfterDays.HasValue)
            {
                ArchiveRemainingDays = "";
                return;
            }

            DateTime? prodOrderInsertDate = ProgramProvider.GetProdOrderInsertDate();
            if (!prodOrderInsertDate.HasValue)
            {
                ArchiveRemainingDays = "";
                return;
            }

            TimeSpan diffArchiveAfter = DateTime.Now - prodOrderInsertDate.Value;
            if (diffArchiveAfter.TotalDays > archiveAfterDays)
            {
                DateTime? nextRunDate = (DateTime)ACUrlCommand("\\Service\\ProgramLogArchive\\NextRunDate");
                if(!nextRunDate.HasValue)
                {
                    ArchiveRemainingDays = "";
                    return;
                }
                TimeSpan remainTime = nextRunDate.Value - DateTime.Now;
                if (remainTime.TotalDays >= 2)
                {
                    ACValueItem valueItem = new ACValueItem("en{'Program log will be archived after -X- days.'}de{'de-Program log will be archived after -X- days.'}", "", null);
                    ArchiveRemainingDays = valueItem.ACCaption.Replace("-X-", ((int)remainTime.TotalDays).ToString());
                }
                else if (remainTime.TotalDays >= 1)
                {
                    ACValueItem valueItem = new ACValueItem("en{'Program log will be archived after -X- day.'}de{'de-Program log will be archived after -X- day.'}", "", null);
                    ArchiveRemainingDays = valueItem.ACCaption.Replace("-X-", ((int)remainTime.TotalDays).ToString());
                }
                else
                    ArchiveRemainingDays = new ACValueItem("en{'Program log will be archived today.'}de{'de-Program log will be archived today.'}", "", null).ACCaption;
            }
        }

        [ACMethodInfo("", "en{'Show program log'}de{'Programmablaufprotokoll anzeigen'}", 401, false)]
        public void ShowACProgramLog()
        {
            if (ProgramProvider is ACBSO)
            {
                PAShowDlgManagerBase service = PAShowDlgManagerBase.GetServiceInstance(ProgramProvider as ACBSO);
                ACValueList param = new ACValueList();
                param.Add(new ACValue(CurrentACProgram.ACIdentifier, typeof(ACProgram), CurrentACProgram));
                param.Add(new ACValue("WorkflowACUrl", ProgramProvider.WorkflowACUrl));
                service.ShowProgramLogViewer(ProgramProvider as IACComponent, param);
            }
        }

        public bool IsEnabledShowACProgramLog()
        {
            if (CurrentACProgram != null)
                return true;
            return false;
        }

        [ACMethodInfo("", "en{'Restore program log from archive'}de{'Programmablaufprotokoll-Wiederherstellung aus dem Archiv'}", 402)]
        public void RestoreArchivedProgramLog()
        {
            if (!IsEnabledRestoreArchivedProgramLog())
                return;

            IACComponent archiveService = GetArchiveService();
            if (archiveService == null)
                return;

            _ACProgramListCopy = ACProgramList.ToList();
            string prodOrderProgramNo = ProgramProvider.GetProdOrderProgramNo();
            if (string.IsNullOrEmpty(prodOrderProgramNo))
                return;
            DateTime prodOrderInsertDate = ProgramProvider.GetProdOrderInsertDate();

            string warning = archiveService.ExecuteMethod("RestoreArchivedProgramLogVB", prodOrderProgramNo, prodOrderInsertDate) as string;
            if (!String.IsNullOrEmpty(warning))
                Messages.Warning(this, warning);

            _refreshTimer.Start();
            _IsEnabledRestoreArchivedProgramLog = false;
        }

        public bool IsEnabledRestoreArchivedProgramLog()
        {
            if (_IsEnabledRestoreArchivedProgramLog)
                return true;
            return false;
        }

        [ACMethodInfo("","en{'Refresh list'}de{'Liste aktualisieren'}",403)]
        public void RefreshProgram()
        {
            OnPropertyChanged("ACProgramList");
        }

        [ACMethodInfo("","en{'Archive program log'}de{'Archiviere Programmablaufprotokoll'}",404)]
        public void ArchiveProgramLogManual()
        {
            if (!IsEnabledArchiveProgramLogManual())
                return;

            IACComponent archiveService = GetArchiveService();
            if (archiveService == null)
                return;

            _ACProgramListCopy = ACProgramList.ToList();
            string prodOrderProgramNo = ProgramProvider.GetProdOrderProgramNo();
            if (string.IsNullOrEmpty(prodOrderProgramNo))
                return;
            DateTime prodOrderInsertDate = ProgramProvider.GetProdOrderInsertDate();

            string warning = archiveService.ExecuteMethod("ArchiveProgramLogVBManual", prodOrderProgramNo, prodOrderInsertDate, CurrentACProgram.ProgramNo) as string;
            if (!String.IsNullOrEmpty(warning))
                Messages.Warning(this, warning);

            CurrentACProgram = null;
            _refreshTimer.Start();
            _IsEnabledRestoreArchivedProgramLog = false;
        }

        public bool IsEnabledArchiveProgramLogManual()
        {
            if (CurrentACProgram != null && !CurrentACProgram.ACClassTask_ACProgram.Any())
                return true;
            return false;
        }

        void _refreshTimer_Tick(object sender, EventArgs e)
        {
            _refreshTimerCounter++;
            if (ACProgramList != null && _ACProgramListCopy != null && (_refreshTimerCounter > 20 || _ACProgramListCopy.Count != ACProgramList.Count()))
            {
                RefreshProgram();
                _refreshTimer.Stop();
                _refreshTimerCounter = 0;
                _IsEnabledRestoreArchivedProgramLog = true;
            }
        }

        private IACComponent GetArchiveService()
        {
            IACComponent archiveGroup = ACUrlCommand("\\Service\\ProgramLogArchive\\ProgramLogArchiveGroup", null) as IACComponent;
            if (archiveGroup == null)
            {
                Messages.Warning(this, "Warning50036");
                return null;
            }

            if (archiveGroup.IsProxy && archiveGroup.ConnectionState == ACObjectConnectionState.DisConnected)
            {
                Messages.Warning(this, "Warning50037");
                return null;
            }

            return archiveGroup;
        }

        #endregion

        #region Excel export

        #region Excel export => Properties

        private bool _IsExportActive = false;

        private string _ExcelFilePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
        [ACPropertyInfo(410)]
        public string ExcelFilePath
        {
            get
            {
                return _ExcelFilePath;
            }
            set
            {
                _ExcelFilePath = value;
                OnPropertyChanged("ExcelFilePath");
            }
        }

        private string _ExcelFileName;
        [ACPropertyInfo(411, "", "en{'Export file name'}de{'Dateiname exportieren'}")]
        public string ExcelFileName
        {
            get
            {
                if (string.IsNullOrEmpty(_ExcelFileName))
                {
                    if (CurrentACProgram != null)
                        _ExcelFileName = CurrentACProgram.ProgramNo;
                    if (!_ExcelFileName.EndsWith(".xlsx"))
                        _ExcelFileName = _ExcelFileName + ".xlsx";
                }
                return _ExcelFileName;
            }
            set
            {
                _ExcelFileName = value;
                OnPropertyChanged("ExcelFileName");
            }
        }

        #region Excel export => Data types

        Type stringType = typeof(string);
        Type shortType = typeof(short);
        Type intType = typeof(int);
        Type doubleType = typeof(double);
        Type decimalType = typeof(decimal);
        Type floatType = typeof(float);

        #endregion

        #endregion

        #region Excel export => ACMethods

        [ACMethodInfo("", "en{'Export to excel'}de{'Exportieren nach Excel'}", 410,true)]
        public void ExportToExcel()
        {
            _ExcelFileName = "";
            ShowDialog(this, "ExcelExportDialog");
        }

        public bool IsEnabledExportToExcel()
        {
            if(CurrentACProgram == null)
                return false;
            return true;
        }

        [ACMethodInfo("", "en{'Export'}de{'Exportieren'}", 411, true)]
        public void DlgExport()
        {
            if (BackgroundWorker.IsBusy)
                return;
            BackgroundWorker.ProgressInfo.OnlyTotalProgress = true;
            BackgroundWorker.ProgressInfo.TotalProgress.ProgressRangeFrom = 0;
            BackgroundWorker.ProgressInfo.TotalProgress.ProgressCurrent = 0;
            _IsExportActive = true;
            BackgroundWorker.RunWorkerAsync("ExcelExport");
        }

        public bool IsEnabledDlgExport()
        {
            if (_IsExportActive)
                return false;
            return true;
        }

        [ACMethodInfo("", "en{'Cancel'}de{'Abbrechen'}", 412, true)]
        public void DlgCancel()
        {
            BackgroundWorker.CancelAsync();
            CloseTopDialog();
        }

        [ACMethodInfo("", "en{'...'}de{'...'}", 413, true)]
        public void Browse()
        {
            using (var dialog = new CommonOpenFileDialog(){IsFolderPicker = true})
            {
                dialog.InitialDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                    ExcelFilePath = dialog.FileName;
            }
        }
        
        public bool IsEnabledBrowse()
        {
            if (_IsExportActive)
                return false;
            return true;
        }

        #endregion

        #region Excel export => Private Methods

        private void ExcelExport()
        {
            int lastParamCol = -1;
            DataTable table = new DataTable();
            AddCommonColumns(table);
            ACProgramLog rootLog = CurrentACProgram.ACProgramLog_ACProgram.FirstOrDefault(c => c.ParentACProgramLogID == null);
            BackgroundWorker.ProgressInfo.TotalProgress.ProgressRangeTo = CurrentACProgram.ACProgramLog_ACProgram.Count*2;
            AddLogs(table, rootLog, ref lastParamCol);
            Export(ExcelFilePath+"\\"+ExcelFileName, table);
            _IsExportActive = false;
        }

        private void AddCommonColumns(DataTable table)
        {
            table.Columns.Add("ACUrl");
            table.Columns.Add("Start Date");
            table.Columns.Add("End Date");
            table.Columns.Add("Duration");
            table.Columns.Add("Alarms");
        }

        private void AddLogs(DataTable table, ACProgramLog programLog, ref int lastParamCol)
        {
            BackgroundWorker.ProgressInfo.ReportProgress("", BackgroundWorker.ProgressInfo.TotalProgress.ProgressCurrent +  1, "");
            AddColumnsAndValue(table, programLog, ref lastParamCol);
            foreach (ACProgramLog log in programLog.ACProgramLog_ParentACProgramLog.OrderBy(c => c.StartDate))
                AddLogs(table, log, ref lastParamCol);
        }

        private void AddColumnsAndValue(DataTable table, ACProgramLog programLog, ref int lastParamCol)
        {
            DataRow row = table.NewRow();

            row.ItemArray = new object[] { programLog.ACUrl,
                                           programLog.StartDate.Value,
                                           programLog.EndDate.Value,
                                           programLog.Duration};

            foreach (ACValue param in programLog.Value.ParameterValueList)
            {
                if (param.Value == null)
                    continue;

                if (!table.Columns.Contains(param.ACIdentifier))
                {
                    DataColumn column = new DataColumn() { ColumnName = param.ACIdentifier, Caption = param.ACCaption, DataType = param.ObjectType };
                    column.ExtendedProperties.Add("Type", "Parameter");
                    table.Columns.Add(column);
                    if (lastParamCol > -1)
                        column.SetOrdinal(lastParamCol + 1);
                    else
                        column.SetOrdinal(table.Columns.Count - 2);
                    lastParamCol = column.Ordinal;
                }
                int valueIndex = table.Columns.IndexOf(param.ACIdentifier);
                row[valueIndex] = param.Value;
            }

            foreach (ACValue result in programLog.Value.ResultValueList)
            {
                if (result.Value == null)
                    continue;

                if (!table.Columns.Contains(result.ACIdentifier))
                {
                    DataColumn column = new DataColumn() { ColumnName = result.ACIdentifier, Caption = result.ACCaption };
                    column.ExtendedProperties.Add("Type", "Result");
                    table.Columns.Add(column);
                    column.SetOrdinal(table.Columns.Count - 2);
                }

                int valueIndex = table.Columns.IndexOf(result.ACIdentifier);
                row[valueIndex] = result.Value; 
            }

            string alarms = "";
            int alarmCount = 0;

            foreach (var alarm in programLog.MsgAlarmLog_ACProgramLog)
            {
                if (alarmCount > 0)
                    alarms += ", ";
                alarms += string.Format("{0}", alarm.ACCaption);
                alarmCount++;
            }

            row[table.Columns.IndexOf("Alarms")] = alarms;
            table.Rows.Add(row);
        }
        
        private void Export(string fileName, DataTable data)
        {
            XLWorkbook workBook = new XLWorkbook();
            IXLWorksheet workSheet = workBook.Worksheets.Add("Program log");

            int colNr = 1;
            foreach(DataColumn col in data.Columns)
            {
                workSheet.Cell(1, colNr).Value = col.Caption;

                string type = col.ExtendedProperties["Type"] as string;
                if (type == "Parameter")
                    workSheet.Cell(1, colNr).Style.Fill.BackgroundColor = XLColor.Yellow;
                else if (type == "Result")
                    workSheet.Cell(1, colNr).Style.Fill.BackgroundColor = XLColor.Lime;
                else
                    workSheet.Cell(1, colNr).Style.Fill.BackgroundColor = XLColor.LightGray;

                workSheet.Cell(1, colNr).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                workSheet.Cell(1, colNr).Style.Border.RightBorderColor = XLColor.Gray;

                colNr++;
            }

            int rowNr = 2;
            foreach(DataRow row in data.Rows)
            {
                BackgroundWorker.ProgressInfo.ReportProgress("", BackgroundWorker.ProgressInfo.TotalProgress.ProgressCurrent + 1, "");
                for (int i=0; i <row.ItemArray.Count(); i++)
                {
                    string value = row[i].ToString();
                    if (value != null)
                    {
                        DataColumn col = data.Columns[i];

                        short shortValue;
                        int intValue;
                        float floatValue;
                        double doubleValue;
                        decimal decimalValue;

                        IXLCell cell = workSheet.Cell(rowNr, i + 1);
                        if (col.DataType == stringType)
                        {
                            cell.SetValue(value);
                            continue;
                        }

                        else if (col.DataType == shortType && short.TryParse(value, out shortValue))
                        {
                            cell.SetValue(shortValue);
                            continue;
                        }
                        
                        else if (col.DataType == intType && int.TryParse(value, out intValue))
                        {
                            cell.SetValue(intValue);
                            continue;
                        }

                        else if (col.DataType == floatType && float.TryParse(value, out floatValue))
                        {
                            cell.SetValue(floatValue);
                            continue;
                        }

                        else if (col.DataType == doubleType && double.TryParse(value, out doubleValue))
                        {
                            cell.SetValue(doubleValue);
                            continue;
                        }

                        else if (col.DataType == decimalType && decimal.TryParse(value, out decimalValue))
                        {
                            cell.SetValue(decimalValue);
                            continue;
                        }

                        else if (col.DataType.IsEnum && !string.IsNullOrEmpty(value))
                        {
                            try
                            {
                                cell.SetValue(Enum.Parse(col.DataType, value).ToString());
                                continue;
                            }
                            catch (Exception e)
                            {
                                Messages.LogException(this.GetACUrl(), "Export(0)", e);
                            }
                        }

                        cell.SetValue(value);
                    }
                }

                rowNr++;
            }

            workSheet.Columns().AdjustToContents();
            workSheet.SheetView.FreezeRows(1);
            workSheet.SheetView.FreezeColumns(1);
            workBook.SaveAs(fileName);
        }

        #endregion

        public override void BgWorkerDoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            if(e.Argument.ToString() == "ExcelExport")
            {
                ExcelExport();
            }
        }

        #endregion

        #region Execute-Helper-Handlers

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case"ShowACProgramLog":
                    ShowACProgramLog();
                    return true;
                case"IsEnabledShowACProgramLog":
                    result = IsEnabledShowACProgramLog();
                    return true;
                case "RestoreArchivedProgramLog":
                    RestoreArchivedProgramLog();
                    return true;
                case "IsEnabledRestoreArchivedProgramLog":
                    result = IsEnabledRestoreArchivedProgramLog();
                    return true;
                case"RefreshProgram":
                    RefreshProgram();
                    return true;
                case "ArchiveProgramLogManual":
                    ArchiveProgramLogManual();
                    return true;
                case "IsEnabledArchiveProgramLogManual":
                    result = IsEnabledArchiveProgramLogManual();
                    return true;
                case"ExportToExcel":
                    ExportToExcel();
                    return true;
                case"IsEnabledExportToExcel":
                    result = IsEnabledExportToExcel();
                    return true;
                case"DlgExport":
                    DlgExport();
                    return true;
                case"IsEnabledDlgExport":
                    result = IsEnabledDlgExport();
                    return true;
                case"DlgCancel":
                    DlgCancel();
                    return true;
                case"Browse":
                    Browse();
                    return true;
                case"IsEnabledBrowse":
                    result = IsEnabledBrowse();
                    return true;
            }
                return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }
}
