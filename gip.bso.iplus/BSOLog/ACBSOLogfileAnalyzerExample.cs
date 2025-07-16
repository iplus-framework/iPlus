// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using gip.core.autocomponent;
using gip.core.datamodel;
using gip.core.media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ClosedXML.Excel;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;

namespace gip.bso.iplus
{
    /// <summary>
    /// Example Implementation of a log file analyzer
    /// </summary>
    //[ACClassInfo(Const.PackName_VarioSystem, "en{'Example File Analyzer'}de{'Example Datei Analysierer'}", Global.ACKinds.TACBSO, Global.ACStorableTypes.NotStorable, true, true)]
    public class ACBSOLogfileAnalyzerExample : ACBSOLogfileAnalyzerBase
    {
        #region c'tors

        /// <summary>
        /// Creates a new instance of BSOLogfileAnalyzerRoaster.
        /// </summary>
        public ACBSOLogfileAnalyzerExample(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "") :
            base(acType, content, parentACObject, parameter, acIdentifier)
        {
            InitializeVariableNames();
        }

        #endregion

        #region Properties

        private readonly string[] _TrackedVariablesA = {
            "LogItem01",
            "LogItem02",
            "LogItem03",
            "LogItem04",
            "LogItem05"
        };

        private readonly string[] _TrackedVariablesB = {
            "LogItem10",
            "LogItem11",
            "LogItem12",
            "LogItem13",
        };

        private List<ExampleLogEntry> _AEntries;
        private List<ExampleLogEntry> _BEntries;

        #endregion

        #region Override Methods

        protected override void PerformAnalysis(Action<int, int> progressCallback)
        {
            _AEntries = new List<ExampleLogEntry>();
            _BEntries = new List<ExampleLogEntry>();

            LogAnalysisMessage($"Starting analysis of {LogLines.Count} log lines...");

            bool[] previousValuesA = new bool[5];
            bool[] previousValuesB = new bool[4];

            int count = LogLines.Count;
            int processedLines = 0;
            foreach (var logLine in LogLines.OrderBy(l => l.DateTime))
            {
                var entry = ParseExampleLogLine(logLine, _TrackedVariablesA);
                bool anyChanged = false;
                if (entry != null)
                {
                    // First entry, all variables are considered "changed"
                    for (int i = 0; i < 5; i++)
                    {
                        if (entry.Variables.ContainsKey(_TrackedVariablesA[i]))
                        {
                            bool newValue = entry.Variables[_TrackedVariablesA[i]];
                            if (newValue != previousValuesA[i])
                            {
                                entry.ChangedVariables.Add(_TrackedVariablesA[i]);
                                anyChanged = true;
                            }
                            previousValuesA[i] = newValue;
                        }
                        else
                        {
                            entry.Variables.Add(_TrackedVariablesA[i], previousValuesA[i]);
                        }
                    }

                    if (anyChanged)
                        _AEntries.Add(entry);
                }
                entry = ParseExampleLogLine(logLine, _TrackedVariablesB);
                anyChanged = false;
                if (entry != null)
                {
                    // First entry, all variables are considered "changed"
                    for (int i = 0; i < 4; i++)
                    {
                        if (entry.Variables.ContainsKey(_TrackedVariablesB[i]))
                        {
                            bool newValue = entry.Variables[_TrackedVariablesB[i]];
                            if (newValue != previousValuesB[i])
                            {
                                entry.ChangedVariables.Add(_TrackedVariablesB[i]);
                                anyChanged = true;
                            }
                            previousValuesB[i] = newValue;
                        }
                        else
                        {
                            entry.Variables.Add(_TrackedVariablesB[i], previousValuesB[i]);
                        }
                    }

                    if (anyChanged)
                        _BEntries.Add(entry);
                }

                processedLines++;
                progressCallback(processedLines, count);
                if (processedLines % 100 == 0)
                {
                    progressCallback((int)((double)processedLines / LogLines.Count * 50), 100);
                }
            }

            LogAnalysisMessage($"Parsed {_AEntries.Count} example entries.");

            // Generate outputs
            if (!string.IsNullOrEmpty(OutputFilePath))
            {
                GenerateExcelOutput();
                progressCallback(75, 100);

                GenerateTextOutput();
                progressCallback(90, 100);
            }

            AnalysisResult = $"Analysis completed. Processed {_AEntries.Count} example entries with tracked variable changes.";
            progressCallback(100, 100);
        }

        protected override string GetOutputFileFilter()
        {
            return "Excel files (*.xlsx)|*.xlsx";
        }

        protected override string GenerateDefaultFileName()
        {
            return $"ExampleAnalysis_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        }

        #endregion

        #region Private Methods

        private void InitializeVariableNames()
        {
            // Could be extended to load variable names from configuration
        }

        private ExampleLogEntry ParseExampleLogLine(LogfileReaderLine logLine, string[] trackedVariables)
        {
            try
            {
                var entry = new ExampleLogEntry
                {
                    DateTime = logLine.DateTime,
                    OriginalMessage = logLine.Message,
                    Variables = new Dictionary<string, bool>(),
                    ChangedVariables = new HashSet<string>()
                };

                // Parse boolean variables from the message
                // Expected format: "...LogItem01:False, LogItem02:True..."
                var regex = new Regex(@"([A-Za-z_0-9]+):(True|False)", RegexOptions.IgnoreCase);
                var matches = regex.Matches(logLine.Message);

                foreach (Match match in matches)
                {
                    string variableName = match.Groups[1].Value;
                    bool variableValue = bool.Parse(match.Groups[2].Value);

                    if (trackedVariables.Contains(variableName))
                    {
                        entry.Variables[variableName] = variableValue;
                    }
                }

                // Only return entries that contain at least one tracked variable
                return entry.Variables.Count > 0 ? entry : null;
            }
            catch (Exception ex)
            {
                Messages.LogException(this.GetACUrl(), "ParseExampleLogLine",
                    $"Error parsing line: {logLine.OriginalLine}. Exception: {ex.Message}");
                return null;
            }
        }

        private void GenerateExcelOutput()
        {
            try
            {
                EnsureOutputDirectoryExists(OutputFilePath);

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("ExampleA");

                    // Create headers
                    worksheet.Cell(1, 1).Value = "DateTime";
                    worksheet.Cell(1, 1).Style.Font.Bold = true;
                    worksheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.LightGray;

                    for (int i = 0; i < _TrackedVariablesA.Length; i++)
                    {
                        worksheet.Cell(1, i + 2).Value = _TrackedVariablesA[i];
                        worksheet.Cell(1, i + 2).Style.Font.Bold = true;
                        worksheet.Cell(1, i + 2).Style.Fill.BackgroundColor = XLColor.LightGray;
                    }
                    worksheet.SheetView.FreezeRows(1);

                    // Fill data
                    int row = 2;
                    foreach (var entry in _AEntries)
                    {
                        worksheet.Cell(row, 1).Value = entry.DateTime.ToString("yyyy-MM-dd-HH:mm:ss.ffff");

                        for (int i = 0; i < _TrackedVariablesA.Length; i++)
                        {
                            string variable = _TrackedVariablesA[i];
                            var cell = worksheet.Cell(row, i + 2);

                            if (entry.Variables.ContainsKey(variable))
                            {
                                bool value = entry.Variables[variable];

                                cell.Value = value.ToString();
                                if (entry.ChangedVariables.Contains(variable))
                                {
                                    cell.Style.Fill.BackgroundColor = value ? XLColor.Yellow : XLColor.LightGray;
                                    cell.Style.Font.Bold = true;
                                }
                            }
                        }

                        row++;
                    }

                    // Auto-fit columns
                    worksheet.ColumnsUsed().AdjustToContents();


                    worksheet = workbook.Worksheets.Add("ExampleB");

                    // Create headers
                    worksheet.Cell(1, 1).Value = "DateTime";
                    worksheet.Cell(1, 1).Style.Font.Bold = true;
                    worksheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.LightGray;

                    for (int i = 0; i < _TrackedVariablesB.Length; i++)
                    {
                        worksheet.Cell(1, i + 2).Value = _TrackedVariablesB[i];
                        worksheet.Cell(1, i + 2).Style.Font.Bold = true;
                        worksheet.Cell(1, i + 2).Style.Fill.BackgroundColor = XLColor.LightGray;
                    }
                    worksheet.SheetView.FreezeRows(1);

                    // Fill data
                    row = 2;
                    foreach (var entry in _BEntries)
                    {
                        worksheet.Cell(row, 1).Value = entry.DateTime.ToString("yyyy-MM-dd-HH:mm:ss.ffff");

                        for (int i = 0; i < _TrackedVariablesB.Length; i++)
                        {
                            string variable = _TrackedVariablesB[i];
                            var cell = worksheet.Cell(row, i + 2);

                            if (entry.Variables.ContainsKey(variable))
                            {
                                bool value = entry.Variables[variable];

                                cell.Value = value.ToString();
                                if (entry.ChangedVariables.Contains(variable))
                                {
                                    cell.Style.Fill.BackgroundColor = value ? XLColor.Yellow : XLColor.LightGray;
                                    cell.Style.Font.Bold = true;
                                }
                            }
                        }

                        row++;
                    }

                    // Auto-fit columns
                    worksheet.ColumnsUsed().AdjustToContents();

                    workbook.SaveAs(OutputFilePath);
                }

                LogAnalysisMessage($"Excel output saved to: {OutputFilePath}");
            }
            catch (Exception ex)
            {
                Messages.LogException(this.GetACUrl(), "GenerateExcelOutput", ex);
                throw;
            }
        }

        private void GenerateTextOutput()
        {
            try
            {
                string textFilePath = Path.ChangeExtension(OutputFilePath, ".txt");
                EnsureOutputDirectoryExists(textFilePath);

                using (var writer = new StreamWriter(textFilePath, false, Encoding.UTF8))
                {
                    writer.WriteLine("ExampleA - Changed Variables Only");
                    writer.WriteLine("========================================");
                    writer.WriteLine();

                    foreach (var entry in _AEntries)
                    {
                        if (entry.ChangedVariables.Count > 0)
                        {
                            writer.WriteLine($"{entry.DateTime:yyyy-MM-dd HH:mm:ss.ffff}:");

                            foreach (string variable in entry.ChangedVariables)
                            {
                                if (entry.Variables.ContainsKey(variable))
                                {
                                    writer.WriteLine($"  {variable}: {entry.Variables[variable]}");
                                }
                            }
                            writer.WriteLine();
                        }
                    }

                    writer.WriteLine("ExampleB - Changed Variables Only");
                    writer.WriteLine("========================================");
                    writer.WriteLine();

                    foreach (var entry in _BEntries)
                    {
                        if (entry.ChangedVariables.Count > 0)
                        {
                            writer.WriteLine($"{entry.DateTime:yyyy-MM-dd HH:mm:ss.ffff}:");

                            foreach (string variable in entry.ChangedVariables)
                            {
                                if (entry.Variables.ContainsKey(variable))
                                {
                                    writer.WriteLine($"  {variable}: {entry.Variables[variable]}");
                                }
                            }
                            writer.WriteLine();
                        }
                    }
                }

                LogAnalysisMessage($"Text output saved to: {textFilePath}");
            }
            catch (Exception ex)
            {
                Messages.LogException(this.GetACUrl(), "GenerateTextOutput", ex);
                throw;
            }
        }

        #endregion

        #region Helper Classes

        private class ExampleLogEntry
        {
            public DateTime DateTime { get; set; }
            public string OriginalMessage { get; set; }
            public Dictionary<string, bool> Variables { get; set; }
            public HashSet<string> ChangedVariables { get; set; }
        }

        #endregion
    }
}