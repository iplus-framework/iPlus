using gip.core.autocomponent;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Reflection;
using ClosedXML.Excel;
using System.IO;

namespace gip.core.reporthandler
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Performance Log Dump to Excel'}de{'Performance Log Dump to Excel'}", Global.ACKinds.TPARole, Global.ACStorableTypes.NotStorable, false, false)]
    public class ACPerfLog2ExcelWriter : PARole, IPerfLogWriter
    {

        #region c'tors
        public ACPerfLog2ExcelWriter(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
           : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            return true;
        }

        #endregion

        #region Properties
        private string _FilenameFormat;
        [ACPropertyInfo(true, 204, "Configuration", "en{'Format of filename'}de{'Format des Dateinamens'}", "", true, DefaultValue = "{0}{1:yyyyMMdd}_{2}.xlsx")]
        public string FilenameFormat
        {
            get
            {
                return _FilenameFormat;
            }
            set
            {
                _FilenameFormat = value;
                OnPropertyChanged("FilenameFormat");
            }
        }
        #endregion

        #region Methods
        public void WritePerformanceData(DateTime stamp, PerformanceLoggerData perfData, PerformanceLogger perfLogger)
        {
            string excelFilePath = string.Format(FilenameFormat, Messages.LogFilePath, stamp, perfLogger.LogName);
            WritePerformanceDataToExcel(excelFilePath, perfData, stamp);
        }

        /// <summary>
        /// Writes performance data to Excel file using ClosedXML, appending to existing file if it exists
        /// </summary>
        /// <param name="filePath">Path where Excel file should be saved</param>
        /// <param name="perfData">The performance data from GetPerformanceData()</param>
        /// <param name="dumpTimestamp">Timestamp of this dump</param>
        private void WritePerformanceDataToExcel(string filePath, PerformanceLoggerData perfData, DateTime dumpTimestamp)
        {
            try
            {
                XLWorkbook workbook;
                bool isNewFile = !File.Exists(filePath);

                if (isNewFile)
                {
                    workbook = new XLWorkbook();
                }
                else
                {
                    workbook = new XLWorkbook(filePath);
                }

                using (workbook)
                {
                    // Update or create summary worksheet
                    UpdateSummaryWorksheet(workbook, perfData, dumpTimestamp, isNewFile);

                    // Update or create detail worksheets for each instance
                    foreach (var instanceData in perfData.Instances)
                    {
                        string safeName = GetSafeWorksheetName(instanceData.InstanceName);
                        UpdateInstanceWorksheet(workbook, instanceData, dumpTimestamp, safeName, isNewFile);
                    }

                    workbook.SaveAs(filePath);
                    Messages.LogInfo(this.GetACUrl(), "WritePerformanceDataToExcel", $"Excel file updated: {filePath}");
                }
            }
            catch (Exception ex)
            {
                Messages.LogException(this.GetACUrl(), "WritePerformanceDataToExcel", ex);
            }
        }

        /// <summary>
        /// Updates or creates the summary worksheet
        /// </summary>
        private void UpdateSummaryWorksheet(XLWorkbook workbook, PerformanceLoggerData perfData, DateTime dumpTimestamp, bool isNewFile)
        {
            IXLWorksheet summaryWs;

            if (workbook.Worksheets.Contains("Summary"))
            {
                summaryWs = workbook.Worksheet("Summary");
            }
            else
            {
                summaryWs = workbook.Worksheets.Add("Summary");
                CreateSummaryHeaders(summaryWs);
            }

            // Find the next available row
            int nextRow = summaryWs.LastRowUsed()?.RowNumber() + 1 ?? 2;

            // Add summary data for each instance
            foreach (var instanceData in perfData.Instances.OrderByDescending(i => i.TotalExecutionTime))
            {
                double usagePercent = perfData.TotalExecutionTime.TotalMilliseconds > 0
                    ? (instanceData.TotalExecutionTime.TotalMilliseconds / perfData.TotalExecutionTime.TotalMilliseconds) * 100
                    : 0;

                summaryWs.Cell(nextRow, 1).Value = instanceData.InstanceName;
                summaryWs.Cell(nextRow, 2).Value = instanceData.TotalExecutionTime.ToString(@"hh\:mm\:ss\.fffffff");
                summaryWs.Cell(nextRow, 3).Value = usagePercent;
                summaryWs.Cell(nextRow, 4).Value = dumpTimestamp;
                nextRow++;
            }

            // Ensure AutoFilter is enabled
            if (summaryWs.RangeUsed() != null)
            {
                summaryWs.RangeUsed().SetAutoFilter();
                summaryWs.Columns().AdjustToContents();
            }
        }

        /// <summary>
        /// Updates or creates an instance detail worksheet
        /// </summary>
        private void UpdateInstanceWorksheet(XLWorkbook workbook, PerformanceLoggerInstanceData instanceData, DateTime dumpTimestamp, string worksheetName, bool isNewFile)
        {
            IXLWorksheet detailWs;

            if (workbook.Worksheets.Contains(worksheetName))
            {
                detailWs = workbook.Worksheet(worksheetName);
            }
            else
            {
                detailWs = workbook.Worksheets.Add(worksheetName);
                CreateInstanceHeaders(detailWs);
            }

            // Find the next available row
            int nextRow = detailWs.LastRowUsed()?.RowNumber() + 1 ?? 2;

            // Add detail data for each statistic
            foreach (var statData in instanceData.Statistics.OrderByDescending(s => s.TotalExecutionTime))
            {
                foreach (var eventData in statData.Events.OrderByDescending(e => e.Elapsed))
                {
                    detailWs.Cell(nextRow, 1).Value = statData.Id;
                    detailWs.Cell(nextRow, 2).Value = eventData.StartTime;
                    detailWs.Cell(nextRow, 3).Value = eventData.Elapsed.ToString(@"hh\:mm\:ss\.fffffff");
                    detailWs.Cell(nextRow, 4).Value = eventData.UsagePercent;
                    detailWs.Cell(nextRow, 5).Value = instanceData.InstanceName;
                    detailWs.Cell(nextRow, 6).Value = dumpTimestamp;
                    nextRow++;
                }
            }

            // Ensure AutoFilter is enabled
            if (detailWs.RangeUsed() != null)
            {
                detailWs.RangeUsed().SetAutoFilter();
                detailWs.Columns().AdjustToContents();
            }
        }

        /// <summary>
        /// Creates headers for the summary worksheet
        /// </summary>
        private void CreateSummaryHeaders(IXLWorksheet worksheet)
        {
            worksheet.Cell(1, 1).Value = "Instance";
            worksheet.Cell(1, 2).Value = "Execution Time";
            worksheet.Cell(1, 3).Value = "Usage %";
            worksheet.Cell(1, 4).Value = "Dump Timestamp";

            // Apply header formatting
            var headerRange = worksheet.Range(1, 1, 1, 4);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
        }

        /// <summary>
        /// Creates headers for instance detail worksheets
        /// </summary>
        private void CreateInstanceHeaders(IXLWorksheet worksheet)
        {
            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Timestamp";
            worksheet.Cell(1, 3).Value = "Execution Time";
            worksheet.Cell(1, 4).Value = "Usage %";
            worksheet.Cell(1, 5).Value = "Instance Name";
            worksheet.Cell(1, 6).Value = "Dump Timestamp";

            // Apply header formatting
            var headerRange = worksheet.Range(1, 1, 1, 6);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGreen;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
        }

        /// <summary>
        /// Creates a safe worksheet name by removing invalid characters
        /// </summary>
        private string GetSafeWorksheetName(string instanceName)
        {
            if (string.IsNullOrEmpty(instanceName))
                return "Instance";

            // Remove or replace invalid characters for worksheet names
            var invalidChars = new char[] { '\\', '/', '?', '*', '[', ']', ':' };
            string safeName = instanceName;

            foreach (char c in invalidChars)
            {
                safeName = safeName.Replace(c, '_');
            }

            // Ensure worksheet name is not too long (Excel limit is 31 characters)
            if (safeName.Length > 31)
            {
                safeName = safeName.Substring(0, 28) + "...";
            }

            return safeName;
        }

        #endregion
    }
}
