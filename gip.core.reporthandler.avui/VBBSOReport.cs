// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using AvRichTextBox;
using gip.core.autocomponent;
using gip.core.datamodel;
using gip.core.reporthandler.avui.Flowdoc;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace gip.core.reporthandler.avui
{
    /// <summary>
    /// Klasse zum ausdrucken von Reports
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Baseclass Reports'}de{'Basisklasse Berichte'}", Global.ACKinds.TACBSOReport, Global.ACStorableTypes.NotStorable, true, false)]
    public class VBBSOReport : ACBSO, IReportHandler
    {
        #region c´tors
        public VBBSOReport(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _MaxRecursionDepthConfig = new ACPropertyConfigValue<int>(this, "MaxRecursionDepthConfig", 3);
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;

            ACInitScriptEngineContent();

            _VarioConfigManager = ConfigManagerIPlus.ACRefToServiceInstance(this);

            return true;
        }

        public override async Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            //if (_LL != null)
            //{
            //    UnSubscribeToLLEvents();
            //    _LL.Dispose();
            //    _LL = null;
            //}
            _PrintServerList = null;
            _WindowsPrinterList = null;
            //if (_SR != null)
            //{
            //    _SR.Dispose();
            //    _SR = null;
            //}
            this._CurrentACClassDesign = null;
            this._CurrentReportData = null;
            this._SelectedWindowsPrinter = null;
            this._PendingDesktopPrintPdfPath = null;

            if (_VarioConfigManager != null)
                ConfigManagerIPlus.DetachACRefFromServiceInstance(this, _VarioConfigManager);
            _VarioConfigManager = null;

            return await base.ACDeInit(deleteACClassTask);
        }

        private void ACInitScriptEngineContent()
        {
            ACInitScriptEngine(ACQueryDefinitionRoot);
        }

        private void ACInitScriptEngine(ACQueryDefinition acQueryDefinition)
        {
            if (acQueryDefinition == null)
                return;
            IEnumerable<ACClassMethod> query = null;
            ACClass classOfQuery = acQueryDefinition.TypeAsACClass as ACClass;
            if (classOfQuery == null)
                return;
            if (IsProxy)
            {
                query = classOfQuery.Methods.Where(c => c.ACKind == Global.ACKinds.MSMethodExtClient);
            }
            else
            {
                query = classOfQuery.Methods.Where(c => c.ACKind == Global.ACKinds.MSMethodExt
                                                                || c.ACKind == Global.ACKinds.MSMethodExtTrigger
                                                                || c.ACKind == Global.ACKinds.MSMethodExtClient);
            }
            if (query.Any())
            {
                foreach (ACClassMethod acClassMethod in query)
                {
                    this.ScriptEngine.RegisterScript(acClassMethod.ACIdentifier, acClassMethod.Sourcecode, acClassMethod.ContinueByError);
                }
            }

            foreach (var acQueryDefinitionChild in acQueryDefinition.ACQueryDefinitionChilds)
            {
                ACInitScriptEngine(acQueryDefinitionChild);
            }
        }

        #endregion

        #region Common

        #region Manager

        protected ACRef<ConfigManagerIPlus> _VarioConfigManager = null;
        public ConfigManagerIPlus VarioConfigManager
        {
            get
            {
                if (_VarioConfigManager == null)
                    return null;
                return _VarioConfigManager.ValueT;
            }
        }

        #endregion

        #region public Properties
        public bool WithDialog
        {
            get;
            set;
        }

        public string PrinterName
        {
            get;
            set;
        }

        bool _ReloadOnServer;
        [ACPropertyInfo(50, "", "en{'Refresh design on server'}de{'Design auf Server aktualisieren'}")]
        public bool ReloadOnServer
        {
            get
            {
                return _ReloadOnServer;
            }
            set
            {
                _ReloadOnServer = value;
                OnPropertyChanged();
            }
        }

        [ACPropertyInfo(9999)]
        public ACQueryDefinition ACQueryDefinitionRoot
        {
            get
            {
                if (_CurrentReportData == null || _CurrentReportData.ReportDocumentValues.Count <= 0)
                    return null;
                if (_CurrentReportData.ReportDocumentValues.Count == 1)
                    return _CurrentReportData.ReportDocumentValues.First().Value as ACQueryDefinition;
                return _CurrentReportData.ReportDocumentValues.Values.Where(c => c is ACQueryDefinition).Select(c => c as ACQueryDefinition).FirstOrDefault();
            }
        }

        ACClassDesign _CurrentACClassDesign;
        [ACPropertyInfo(9999)]
        public ACClassDesign CurrentACClassDesign
        {
            get
            {
                return _CurrentACClassDesign;
            }
            set
            {
                _CurrentACClassDesign = value;
                OnPropertyChanged("CurrentACClassDesign");
            }
        }

        private ReportData _CurrentReportData;
        [ACPropertyInfo(9999)]
        public ReportData CurrentReportData
        {
            get
            {
                return _CurrentReportData;
            }
            set
            {
                _CurrentReportData = value;
                OnPropertyChanged("CurrentReportData");
            }
        }
        #endregion

        #region Public Methods

        [ACMethodInfo("Report", "en{'Print'}de{'Drucken'}", 9999, false)]
        public Msg Print(ACClassDesign acClassDesign, bool withDialog, string printerName, ReportData data, int copies = 1, int maxPrintJobsInSpooler = 0)
        {
            if (acClassDesign == null || data == null)
                return null;
            CurrentACClassDesign = acClassDesign;
            CurrentReportData = data;
            WithDialog = withDialog;
            PrinterName = printerName;
            if (acClassDesign.ACUsage == Global.ACUsages.DUReport)
            {
                return FlowPrint(acClassDesign, withDialog, printerName, data, copies, maxPrintJobsInSpooler);
            }
            else if (acClassDesign.ACUsageIndex >= (short)Global.ACUsages.DULLReport && acClassDesign.ACUsageIndex <= (short)Global.ACUsages.DULLFilecard)
            {
                //RunLL(false, LlPrintMode.Export);
            }
            else if (acClassDesign.ACUsage == Global.ACUsages.DUReportPrintServer)
            {
                _= DoPrintComponent(acClassDesign, withDialog, copies, ReloadOnServer);
            }

            return null;
        }

        public async Task<Msg> PrintAsync(ACClassDesign acClassDesign, bool withDialog, string printerName, ReportData data, int copies = 1, int maxPrintJobsInSpooler = 0)
        {
            if (acClassDesign == null || data == null)
                return null;

            CurrentACClassDesign = acClassDesign;
            CurrentReportData = data;
            WithDialog = withDialog;
            PrinterName = printerName;

            if (acClassDesign.ACUsage == Global.ACUsages.DUReport)
            {
                return await FlowPrintAsync(acClassDesign, withDialog, printerName, data, copies, maxPrintJobsInSpooler);
            }
            else if (acClassDesign.ACUsageIndex >= (short)Global.ACUsages.DULLReport && acClassDesign.ACUsageIndex <= (short)Global.ACUsages.DULLFilecard)
            {
                //RunLL(false, LlPrintMode.Export);
            }
            else if (acClassDesign.ACUsage == Global.ACUsages.DUReportPrintServer)
            {
                await DoPrintComponent(acClassDesign, withDialog, copies, ReloadOnServer);
            }

            return null;
        }

        [ACMethodInfo("Report", "en{'Preview'}de{'Vorschau'}", 9999, false)]
        public async Task Preview(ACClassDesign acClassDesign, bool withDialog, string printerName, ReportData data)
        {
            CurrentACClassDesign = acClassDesign;
            CurrentReportData = data;
            WithDialog = withDialog;
            PrinterName = printerName;
            if (acClassDesign.ACUsage == Global.ACUsages.DUReport)
            {
                if (ScryberReportEngine.IsScryberTemplate(acClassDesign?.XAMLDesign))
                {
                    await PreviewScryberReportAsync(acClassDesign, data);
                    return;
                }
                await ShowDialogAsync(this, "PreviewFlowDoc");
            }
            else if (acClassDesign.ACUsageIndex >= (short)Global.ACUsages.DULLReport && acClassDesign.ACUsageIndex <= (short)Global.ACUsages.DULLFilecard)
            {
                //RunLL(false, LlPrintMode.Preview);
            }
            else if (acClassDesign.ACUsage == Global.ACUsages.DUReportPrintServer)
            {
                await ShowDialogAsync(this, "PreviewFlowDoc");
            }
        }

        [ACMethodInfo("Report", "en{'Design'}de{'Entwurf'}", 9999, false)]
        public async Task Design(ACClassDesign acClassDesign, bool withDialog, string printerName, ReportData data)
        {
            CurrentACClassDesign = acClassDesign;
            CurrentReportData = data;
            WithDialog = withDialog;
            PrinterName = printerName;
            if (acClassDesign.ACUsage == Global.ACUsages.DUReport)
            {
                await ShowDialogAsync(this, "DesignFlowDoc");
            }
            else if (acClassDesign.ACUsageIndex >= (short)Global.ACUsages.DULLReport && acClassDesign.ACUsageIndex <= (short)Global.ACUsages.DULLFilecard)
            {
                //RunLL(true);
            }
            else if (acClassDesign.ACUsage == Global.ACUsages.DUReportPrintServer)
            {
                await ShowDialogAsync(this, "DesignFlowDoc");
            }
        }

        #endregion

        #endregion

        #region Desktop printer selection

        private PrinterInfo _SelectedWindowsPrinter;
        /// <summary>
        /// Selected property for PrinterInfo
        /// </summary>
        /// <value>The selected system printer</value>
        [ACPropertySelected(9999, "WindowsPrinter", "en{'Selected system printer'}de{'Ausgewaehlter Systemdrucker'}")]
        public PrinterInfo SelectedWindowsPrinter
        {
            get
            {
                return _SelectedWindowsPrinter;
            }
            set
            {
                if (_SelectedWindowsPrinter != value)
                {
                    _SelectedWindowsPrinter = value;
                    if (value != null)
                        PrinterName = value.PrinterName;
                    OnPropertyChanged("SelectedWindowsPrinter");
                }
            }
        }

        private List<PrinterInfo> _WindowsPrinterList;
        /// <summary>
        /// List property for PrinterInfo
        /// </summary>
        /// <value>The system printer list</value>
        [ACPropertyList(9999, "WindowsPrinter", "en{'System printers'}de{'Systemdrucker'}")]
        public List<PrinterInfo> WindowsPrinterList
        {
            get
            {
                if (_WindowsPrinterList == null)
                    _WindowsPrinterList = LoadWindowsPrinterList();
                return _WindowsPrinterList;
            }
        }

        private List<PrinterInfo> LoadWindowsPrinterList()
        {
            var printers = GetDesktopPrinters();
            if (printers == null)
                return new List<PrinterInfo>();

            return ACPrintManager.GetPrinters(printers)
                                 .OrderBy(c => c.PrinterName)
                                 .ToList();
        }

        private IEnumerable<string> GetDesktopPrinters()
        {
            var windowsPrinters = Root?.WPFServices?.VBMediaControllerService?.GetWindowsPrinters();
            if (windowsPrinters != null && windowsPrinters.Any())
                return windowsPrinters;

            return GetCupsPrinters();
        }

        private IEnumerable<string> GetCupsPrinters()
        {
            var result = new List<string>();
            result.AddRange(RunLpstatAndParse("-a", ""));
            if (!result.Any())
                result.AddRange(RunLpstatAndParse("-p", "printer "));

            return result.Where(c => !String.IsNullOrWhiteSpace(c))
                         .Distinct(StringComparer.OrdinalIgnoreCase)
                         .OrderBy(c => c)
                         .ToList();
        }

        private IEnumerable<string> RunLpstatAndParse(string arguments, string requiredPrefix)
        {
            try
            {
                using (Process process = Process.Start(new ProcessStartInfo
                {
                    FileName = "lpstat",
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                }))
                {
                    if (process == null)
                        return Enumerable.Empty<string>();

                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit(2000);

                    if (String.IsNullOrWhiteSpace(output))
                        return Enumerable.Empty<string>();

                    return output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                 .Select(line => line.Trim())
                                 .Where(line => String.IsNullOrEmpty(requiredPrefix)
                                     || line.StartsWith(requiredPrefix, StringComparison.OrdinalIgnoreCase))
                                 .Select(line => String.IsNullOrEmpty(requiredPrefix)
                                     ? line
                                     : line.Substring(requiredPrefix.Length).TrimStart())
                                 .Select(line =>
                                 {
                                     int separator = line.IndexOf(' ');
                                     return separator > 0 ? line.Substring(0, separator) : line;
                                 })
                                 .Where(line => !String.IsNullOrWhiteSpace(line))
                                 .ToList();
                }
            }
            catch
            {
                return Enumerable.Empty<string>();
            }
        }

        #endregion

        #region PrintServer
        private PrinterInfo _SelectedPrintServer;
        /// <summary>
        /// Selected property for PrinterInfo
        /// </summary>
        /// <value>The selected ESCPosPrinter</value>
        [ACPropertySelected(9999, "PrintServer", "en{'Printer'}de{'Drucker'}")]
        public PrinterInfo SelectedPrintServer
        {
            get
            {
                return _SelectedPrintServer;
            }
            set
            {
                if (_SelectedPrintServer != value)
                {
                    _SelectedPrintServer = value;
                    if (value != null)
                    {
                        PrinterName = value.PrinterACUrl;
                    }
                    OnPropertyChanged("SelectedPrintServer");
                }
            }
        }


        private List<PrinterInfo> _PrintServerList;
        /// <summary>
        /// List property for PrinterInfo
        /// </summary>
        /// <value>The ESCPosPrinter list</value>
        [ACPropertyList(9999, "PrintServer")]
        public List<PrinterInfo> PrintServerList
        {
            get
            {
                if (_PrintServerList == null)
                    _PrintServerList = ACPrintManager.GetPrintServers(Database.ContextIPlus);

                if (ConfiguredPrinterList != null && ConfiguredPrinterList.Any())
                {
                    return _PrintServerList.Where(c => ConfiguredPrinterList.Select(x => x.ACCaption).Contains(c.PrinterACUrl)).OrderBy(c => c.PrinterACUrl).ToList();
                }

                return _PrintServerList;
            }
        }
        #endregion

        #region ConfiguredPrinters

        private ACValueItemList _ConfiguredPrinterList;
        public IEnumerable<ACValueItem> ConfiguredPrinterList
        {
            get
            {
                if (_ConfiguredPrinterList == null)
                    _ConfiguredPrinterList = LoadConfiguredPrinterList();
                return _ConfiguredPrinterList;
            }
        }

        private ACValueItemList LoadConfiguredPrinterList()
        {
            ACValueItemList aCValueItems = new ACValueItemList("ConfiguredPrinters");
            if (CurrentACClassDesign != null)
            {
                List<IACConfig> configs = VarioConfigManager.GetConfigurationList(new List<IACConfigStore>() { CurrentACClassDesign.ACClass }, null, new List<string>() { ACBSO.Const_PrinterPreConfigACUrl }, null);
                configs = configs.Where(c => c.KeyACUrl == CurrentACClassDesign.ACConfigKeyACUrl).ToList();
                configs.ForEach(c => aCValueItems.AddEntry(c, c.Value.ToString()));
                configs = configs.OrderBy(c => c.Value.ToString()).ToList();
            }
            return aCValueItems;
        }

        #endregion

        #region List & Label

        #region Properties


        #region public

        #endregion

        #region overrideable

        protected virtual object SourceDataForObjectProvider
        {
            get
            {
                if (_CurrentReportData == null || _CurrentReportData.ReportDocumentValues.Count <= 0)
                    return null;
                if (ACQueryDefinitionRoot != null && Database != null)
                    return Database.ACSelect(ACQueryDefinitionRoot, Database.RecommendedMergeOption);
                if (_CurrentReportData.ReportDocumentValues.Count == 1)
                    return _CurrentReportData.ReportDocumentValues.First().Value;
                return _CurrentReportData;
            }
        }

        private ACPropertyConfigValue<int> _MaxRecursionDepthConfig;
        [ACPropertyConfig("en{'Maximum recursion depth for Object Provider'}de{'Maximale rekursionstiefe für Object-Provider'}")]
        public int MaxRecursionDepthConfig
        {
            get
            {
                return _MaxRecursionDepthConfig.ValueT;
            }
            set
            {
                _MaxRecursionDepthConfig.ValueT = value;
            }
        }

        private int _MaxRecursionDepthObjectProvider = -1;
        public virtual int MaxRecursionDepthObjectProvider
        {
            get
            {
                int maxValue = 0;
                if (_MaxRecursionDepthObjectProvider >= 0)
                    maxValue = _MaxRecursionDepthObjectProvider;
                else if (CurrentACClassDesign != null && CurrentACClassDesign.DesignerMaxRecursion.HasValue)
                    maxValue = Convert.ToInt32(CurrentACClassDesign.DesignerMaxRecursion.Value);
                else if (ACQueryDefinitionRoot != null)
                    maxValue = 1;
                else
                    maxValue = MaxRecursionDepthConfig;
                return maxValue > 10 ? 10 : maxValue;
            }
            set
            {
                if (value > 10)
                    _MaxRecursionDepthObjectProvider = 10;
                else
                    _MaxRecursionDepthObjectProvider = value;
            }
        }

        #endregion

        #endregion

        #region Methods

        #region overridable

        #endregion

        #region Protected
        #endregion

        #endregion

        #endregion

        #region FlowDoc

        [ACMethodCommand("Report", "en{'Cancel'}de{'Abbrechen'}", (short)MISort.Cancel)]
        public void FlowDialogCancel()
        {
            UndoFlowDoc();
            CloseTopDialog();
        }

        [ACMethodCommand("Report", "en{'OK'}de{'OK'}", (short)MISort.Okay)]
        public void FlowDialogOk()
        {
            SaveFlowDoc();
            CloseTopDialog();
        }

        public void SaveFlowDoc()
        {
            if (ParentACComponent is VBBSOReportDialog)
                (ParentACComponent as VBBSOReportDialog).ReportSave();
            else
            {
                ACSaveChanges();
                if (Database.ContextIPlus != null)
                    Database.ContextIPlus.ACSaveChanges();
            }
        }

        public void UndoFlowDoc()
        {
            if (ParentACComponent is VBBSOReportDialog)
                (ParentACComponent as VBBSOReportDialog).ReportUndo();
            else
            {
                ACUndoChanges();
                if (Database.ContextIPlus != null)
                    Database.ContextIPlus.ACUndoChanges();
            }
        }

        private async Task PreviewScryberReportAsync(ACClassDesign acClassDesign, ReportData data)
        {
            if (acClassDesign == null || data == null)
                return;

            try
            {
                byte[] pdfBytes = ScryberReportEngine.RenderPdf(acClassDesign.XAMLDesign, data);
                if (pdfBytes == null || pdfBytes.Length == 0)
                    return;

                string tempPdfPath = Path.Combine(Path.GetTempPath(), $"iplus_{Guid.NewGuid():N}.pdf");
                File.WriteAllBytes(tempPdfPath, pdfBytes);
                OpenWithDefaultApplication(tempPdfPath);
            }
            catch (Exception e)
            {
                this.Root().Messages.LogException("VBBSOReport", "PreviewScryberReportAsync", e);
            }

            await Task.CompletedTask;
        }

        private static void OpenWithDefaultApplication(string filePath)
        {
            if (String.IsNullOrWhiteSpace(filePath))
                return;

            Process.Start(new ProcessStartInfo(filePath)
            {
                UseShellExecute = true
            });
        }

        private string _PendingDesktopPrintPdfPath;
        private int _PendingDesktopPrintCopies = 1;

        private void PrepareDesktopPrinterSelection(string pdfPath, string printerName, int copies)
        {
            _PendingDesktopPrintPdfPath = pdfPath;
            _PendingDesktopPrintCopies = copies <= 0 ? 1 : copies;

            _WindowsPrinterList = null;
            OnPropertyChanged("WindowsPrinterList");

            PrinterInfo preferredPrinter = null;
            if (!String.IsNullOrWhiteSpace(printerName))
            {
                preferredPrinter = WindowsPrinterList.FirstOrDefault(c => String.Equals(c.PrinterName, printerName, StringComparison.OrdinalIgnoreCase));
            }

            if (preferredPrinter == null)
                preferredPrinter = WindowsPrinterList.FirstOrDefault(c => c.IsDefault) ?? WindowsPrinterList.FirstOrDefault();

            SelectedWindowsPrinter = preferredPrinter;
        }

        private void ClearDesktopPrinterSelection()
        {
            _PendingDesktopPrintPdfPath = null;
            _PendingDesktopPrintCopies = 1;
        }

        private async Task ShowDesktopPrinterSelectionAsync()
        {
            try
            {
                await ShowDialogAsync(this, "PrinterSelection");
            }
            catch (Exception e)
            {
                this.Root().Messages.LogException("VBBSOReport", nameof(ShowDesktopPrinterSelectionAsync), e);
            }
        }

        private bool TryPrintPdf(string pdfPath, string printerName, int copies)
        {
            if (String.IsNullOrWhiteSpace(pdfPath) || String.IsNullOrWhiteSpace(printerName))
                return false;

            if (copies <= 0)
                copies = 1;

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process process = Process.Start(new ProcessStartInfo
                    {
                        FileName = pdfPath,
                        Verb = "printto",
                        Arguments = $"\"{printerName}\"",
                        UseShellExecute = true,
                        CreateNoWindow = true,
                    });
                    return process != null;
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process process = Process.Start(new ProcessStartInfo
                    {
                        FileName = "lp",
                        Arguments = $"-d \"{printerName}\" -n {copies} \"{pdfPath}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    });

                    if (process == null)
                        return false;

                    process.WaitForExit(3000);
                    return process.ExitCode == 0;
                }
            }
            catch
            {
            }

            return false;
        }

        public Msg FlowPrint(ACClassDesign acClassDesign, bool withDialog, string printerName, ReportData data, int copies, int maxPrintJobsInSpooler = 0)
        {
            _ = FlowPrintAsync(acClassDesign, withDialog, printerName, data, copies, maxPrintJobsInSpooler);
            return null;
        }

        public async Task<Msg> FlowPrintAsync(ACClassDesign acClassDesign, bool withDialog, string printerName, ReportData data, int copies, int maxPrintJobsInSpooler = 0)
        {
            if (acClassDesign == null || data == null)
                return null;

            if (!ScryberReportEngine.IsScryberTemplate(acClassDesign.XAMLDesign))
            {
                this.Root().Messages.LogException("VBBSOReport", "FlowPrint(5)", "Avalonia report printing currently supports only Scryber HTML templates.");
                return null;
            }

            try
            {
                byte[] pdfBytes = ScryberReportEngine.RenderPdf(acClassDesign.XAMLDesign, data);
                if (pdfBytes == null || pdfBytes.Length == 0)
                    return null;

                if (!String.IsNullOrEmpty(printerName) && printerName.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
                {
                    string fileName = printerName.Substring(7);
                    if (!String.IsNullOrWhiteSpace(Path.GetDirectoryName(fileName)))
                        Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                    File.WriteAllBytes(fileName, pdfBytes);
                    return null;
                }

                string tempPdfPath = Path.Combine(Path.GetTempPath(), $"iplus_{Guid.NewGuid():N}.pdf");
                File.WriteAllBytes(tempPdfPath, pdfBytes);

                if (withDialog)
                {
                    PrepareDesktopPrinterSelection(tempPdfPath, printerName, copies);
                    await ShowDesktopPrinterSelectionAsync();
                    return null;
                }

                bool sentToPrinter = TryPrintPdf(tempPdfPath, printerName, copies);
                if (!sentToPrinter)
                    OpenWithDefaultApplication(tempPdfPath);
            }
            catch (Exception e)
            {
                this.Root().Messages.LogException("VBBSOReport", "FlowPrint(10)", e);
            }

            return null;
        }

        #endregion

        #region ACPrintServer component

        [ACMethodCommand("Report", "en{'OK'}de{'OK'}", (short)MISort.Okay)]
        public void PrintComponentDialogOk()
        {
            ACSaveChanges();
            if (Database.ContextIPlus != null)
                Database.ContextIPlus.ACSaveChanges();
            CloseTopDialog();
        }

        [ACMethodInfo("PrintComponent", "en{'Print'}de{'Drucken'}", 9999, false)]
        public async Task PrintComponent()
        {
            CloseTopDialog();
            await ShowDialogAsync(this, "PrintXMLDoc");
        }

        [ACMethodInfo("PrintComponentOk", "en{'Print'}de{'Drucken'}", 9999, false)]
        public void PrintComponentOk()
        {
            if (!IsEnabledPrintComponent())
                return;
            PrintOnServer(CurrentACClassDesign, SelectedPrintServer.PrinterACUrl, CopyCount, ReloadOnServer);
            CloseTopDialog();
        }

        [ACMethodCommand("Report", "en{'Print'}de{'Drucken'}", (short)MISort.Okay)]
        public void PrinterSelectionOk()
        {
            if (!IsEnabledPrinterSelectionOk())
                return;

            string pendingPdfPath = _PendingDesktopPrintPdfPath;
            int pendingCopies = _PendingDesktopPrintCopies;
            string selectedPrinterName = SelectedWindowsPrinter?.PrinterName;

            ClearDesktopPrinterSelection();
            CloseTopDialog();

            PrinterName = selectedPrinterName;
            bool sentToPrinter = TryPrintPdf(pendingPdfPath, selectedPrinterName, pendingCopies);
            if (!sentToPrinter)
                OpenWithDefaultApplication(pendingPdfPath);
        }

        [ACMethodInfo("", "en{'Is enabled print'}de{'Drucken aktiviert'}", 9999)]
        public bool IsEnabledPrinterSelectionOk()
        {
            return !String.IsNullOrWhiteSpace(_PendingDesktopPrintPdfPath) && SelectedWindowsPrinter != null;
        }

        [ACMethodCommand("Report", "en{'Cancel'}de{'Abbrechen'}", (short)MISort.Cancel)]
        public void PrinterSelectionCancel()
        {
            ClearDesktopPrinterSelection();
            CloseTopDialog();
        }

        private bool IsEnabledPrintComponent()
        {
            return SelectedPrintServer != null;
        }

        private async Task DoPrintComponent(ACClassDesign acClassDesign, bool withDialog, int copies, bool reloadReport)
        {
            if (withDialog)
                await PrintComponent();
            else
            {
                string acPrintServerACUrl = PrintServerList.Where(c => c.IsDefault).Select(c => c.PrinterACUrl).FirstOrDefault();
                PrintOnServer(acClassDesign, acPrintServerACUrl, copies, reloadReport);
            }
        }

        private void PrintOnServer(ACClassDesign acClassDesign, string acPrintServerACUrl, int copies, bool reloadReport)
        {
            IACComponent printServer = Root.ACUrlCommand(acPrintServerACUrl) as IACComponent;
            if (printServer == null)
            {
                // Error50473
                Root.Messages.ErrorAsync(this, "Error50473", false, acPrintServerACUrl);
                return;
            }
            if (printServer.ConnectionState == ACObjectConnectionState.DisConnected)
            {
                // Error50474
                Root.Messages.ErrorAsync(this, "Error50474", false, acPrintServerACUrl);
                return;
            }

            VBBSOReportDialog vBBSOReportDialog = ParentACComponent as VBBSOReportDialog;
            if (vBBSOReportDialog != null && vBBSOReportDialog.ParentBSO != null)
            {
                ACBSO parentACBSO = vBBSOReportDialog.ParentBSO as ACBSO;
                PAOrderInfo pAOrderInfo = null;
                if (parentACBSO != null)
                {
                    pAOrderInfo = parentACBSO.GetOrderInfo();
                    if (pAOrderInfo != null)
                        printServer.ACUrlCommand(ACUrlHelper.Delimiter_InvokeMethod + nameof(ACPrintServerBase.Print), parentACBSO.ACType.ACTypeID, acClassDesign.ACIdentifier, pAOrderInfo, copies, reloadReport);
                }
                if (parentACBSO == null || pAOrderInfo == null)
                    Root.Messages.ErrorAsync(this, "Error50475", false, ParentACComponent.GetACUrl());
            }
        }

        int _CopyCount = 1;
        [ACPropertyInfo(101, "", "en{'Number of copies'}de{'Anzahl Kopien'}")]
        public int CopyCount
        {
            get
            {
                return _CopyCount;
            }
            set
            {
                _CopyCount = value;
                OnPropertyChanged("CopyCount");
            }
        }

        #endregion

        #region ACMethodConfiguration

        private string _ACUrl;

        private bool _LocalConfig = true;
        [ACPropertyInfo(999, "", "en{'Local configuration'}de{'Local configuration'}")]
        public bool LocalConfig
        {
            get
            {
                return _LocalConfig;
            }
            set
            {
                _LocalConfig = value;
                LoadConfig();
                OnPropertyChanged("LocalConfig");
            }
        }

        private ACClassInfoWithItems _CurrentConfigurationSource;
        [ACPropertyCurrent(999, "ConfigSource")]
        public ACClassInfoWithItems CurrentConfigurationSource
        {
            get
            {
                return _CurrentConfigurationSource;
            }
            set
            {
                _CurrentConfigurationSource = value;
                if (_CurrentConfigurationSource.Value != null && _CurrentConfigurationSource.Value is ACClass)
                {
                    var method = ((ACClass)_CurrentConfigurationSource.Value).ACClassMethod_ACClass.FirstOrDefault(c => c.ACIdentifier == ACStateConst.SMStarting);
                    if (method != null && method.ACMethod != null)
                    {
                        _ACUrl = method.GetACUrl();
                        CurrentACMethod = method.ACMethod;
                    }
                    else
                        CurrentACMethod = null;
                }
                else if (_CurrentConfigurationSource.Value == null && _CurrentConfigurationSource.ParentContainerT != null)
                {
                    ACClassMethod acClassMethod = _CurrentConfigurationSource.ParentContainerT.ValueT.Methods.FirstOrDefault(c => c.ACIdentifier == _CurrentConfigurationSource.ACCaption);

                    _ACUrl = acClassMethod.GetACUrl();
                    CurrentACMethod = ACConvert.XMLToObject<ACMethod>(acClassMethod.XMLACMethod, true, Database);
                }
                else if (_CurrentConfigurationSource.Value != null && _CurrentConfigurationSource.Value.ToString() == "Rules" && _CurrentConfigurationSource.ACCaption == "Rules")
                {
                    CurrentACMethod = null;
                    List<ACMethodReportConfigWrapper> rulesWrapperList = new List<ACMethodReportConfigWrapper>();
                    ConfigurationMethod acMethodConfigInfo = null;
                    if (CurrentReportConfiguration != null)
                        acMethodConfigInfo = CurrentReportConfiguration.Items.Cast<ConfigurationMethod>().FirstOrDefault(c => c.VBContent == "Rules");
                    foreach (var item in RulesCommand.ListOfRuleInfoPatterns)
                    {
                        if (acMethodConfigInfo != null)
                        {
                            ConfigurationParameter acMethodParamInfo = acMethodConfigInfo.Items.Cast<ConfigurationParameter>().FirstOrDefault(c => c.ParameterName == item.RuleType.ToString());
                            if (acMethodParamInfo != null)
                                rulesWrapperList.Add(new ACMethodReportConfigWrapper() { ParameterName = new ACValue() { ACIdentifier = item.RuleType.ToString(), Value = item }, IsInReport = true, ConfigSource = _CurrentConfigurationSource });
                            else
                                rulesWrapperList.Add(new ACMethodReportConfigWrapper() { ParameterName = new ACValue() { ACIdentifier = item.RuleType.ToString(), Value = item }, IsInReport = false, ConfigSource = _CurrentConfigurationSource });
                        }
                        else
                            rulesWrapperList.Add(new ACMethodReportConfigWrapper() { ParameterName = new ACValue() { ACIdentifier = item.RuleType.ToString(), Value = item }, IsInReport = false, ConfigSource = _CurrentConfigurationSource });
                    }
                    ACMethodReportConfigWrapperList = rulesWrapperList;
                }
                else
                    CurrentACMethod = null;
                OnPropertyChanged("CurrentConfigurationSource");
                //OnPropertyChanged("CurrentACMethod");
                //OnPropertyChanged("ACMethodParamList");
            }
        }

        private List<ACClassInfoWithItems> _ConfigurationSource;
        [ACPropertyList(999, "ConfigSource")]
        public List<ACClassInfoWithItems> ConfigurationSource
        {
            get
            {
                if (_ConfigurationSource == null)
                {
                    _ConfigurationSource = new List<ACClassInfoWithItems>();

                }
                return _ConfigurationSource;
            }
        }

        public void FillConfigurationSource()
        {
            ConfigurationSource.Clear();

            //Configuration
            ACClassInfoWithItems configCategory = new ACClassInfoWithItems() { ACCaption = "Configuration" };
            var mainACClassesPW = Database.ContextIPlus.ACClass.Where(c => c.ACKindIndex == (short)Global.ACKinds.TPWNodeMethod
                                                                        || c.ACKindIndex == (short)Global.ACKinds.TPWGroup
                                                                        || c.ACKindIndex == (short)Global.ACKinds.TPWNodeWorkflow
                                                                        || c.ACKindIndex == (short)Global.ACKinds.TPWNodeStatic)
                                                                        .ToList().GroupBy(x => x.ACIdentifier).Select(q => q.FirstOrDefault());
            foreach (ACClass acclass in mainACClassesPW.Where(c => c.ACClassMethod_ACClass.Any(x => x.ACIdentifier == "SMStarting")))
            {
                ACClassInfoWithItems info = new ACClassInfoWithItems() { ACCaption = acclass.ACCaption, ValueT = acclass };
                configCategory.Add(info);
            }

            _ConfigurationSource.Add(configCategory);

            //ACMethod
            ACClassInfoWithItems acMethodCategory = new ACClassInfoWithItems() { ACCaption = "Method" };
            var mainACClasses = Database.ContextIPlus.ACClass.Where(c => c.ACKindIndex == (short)Global.ACKinds.TPAProcessFunction).GroupBy(x => x.ACIdentifier).Select(q => q.FirstOrDefault());
            foreach (ACClass acClass in mainACClasses)
            {
                ACClassInfoWithItems info = new ACClassInfoWithItems() { ACCaption = acClass.ACCaption, ValueT = acClass };
                foreach (var method in acClass.Methods.Where(c => c.IsParameterACMethod && c.ACIdentifier != "Start"))
                {
                    var methodInfo = new ACClassInfoWithItems() { ACCaption = method.ACIdentifier };
                    info.Add(methodInfo);
                }
                if (info.Items.Any())
                    acMethodCategory.Add(info);
            }
            _ConfigurationSource.Add(acMethodCategory);


            //Rules
            _ConfigurationSource.Add(new ACClassInfoWithItems() { ACCaption = "Rules", Value = "Rules" });
            _ConfigurationSource = _ConfigurationSource.ToList();
            OnPropertyChanged("ConfigurationSource");
        }

        private ACMethod _CurrentACMethod;
        [ACPropertyInfo(9999)]
        public ACMethod CurrentACMethod
        {
            get
            {
                return _CurrentACMethod;
            }
            set
            {
                _CurrentACMethod = value;
                if (_CurrentACMethod != null && _CurrentACMethod.ParameterValueList.Any())
                    ACMethodParamList = _CurrentACMethod.ParameterValueList.Concat(_CurrentACMethod.ResultValueList);
                else
                    ACMethodReportConfigWrapperList = null;
                OnPropertyChanged("CurrentACMethod");
            }
        }

        private IEnumerable<ACValue> _ACMethodParamList;

        [ACPropertyList(9999, "ParamACMethod")]
        public IEnumerable<ACValue> ACMethodParamList
        {
            get
            {
                return _ACMethodParamList;
            }
            set
            {
                _ACMethodParamList = value;
                var list = new List<ACMethodReportConfigWrapper>();
                string acurl = CurrentConfigurationSource.GetACUrl();
                if (string.IsNullOrEmpty(acurl) && CurrentConfigurationSource.ValueT == null)
                {
                    var method = CurrentConfigurationSource.ParentContainerT.ValueT.Methods.FirstOrDefault(c => c.ACIdentifier == CurrentConfigurationSource.ACCaption);
                    acurl = method.GetACUrl();
                }

                ConfigurationMethod acMethodConfigInfo = null;
                if (CurrentReportConfiguration != null)
                    acMethodConfigInfo = CurrentReportConfiguration.Items.Cast<ConfigurationMethod>().FirstOrDefault(c => c.VBContent == _ACUrl);
                foreach (var item in _ACMethodParamList)
                {
                    if (acMethodConfigInfo != null)
                    {
                        ConfigurationParameter acMethodParamInfo = acMethodConfigInfo.Items.Cast<ConfigurationParameter>().FirstOrDefault(c => c.ParameterName == item.ACIdentifier);
                        if (acMethodParamInfo != null)
                            list.Add(new ACMethodReportConfigWrapper() { ParameterName = item, IsInReport = true, ConfigSource = CurrentConfigurationSource });
                        else
                            list.Add(new ACMethodReportConfigWrapper() { ParameterName = item, IsInReport = false, ConfigSource = CurrentConfigurationSource });
                    }
                    else
                        list.Add(new ACMethodReportConfigWrapper() { ParameterName = item, IsInReport = false, ConfigSource = CurrentConfigurationSource });
                }
                ACMethodReportConfigWrapperList = list;
                OnPropertyChanged("ACMethodParamList");
            }
        }

        private ACMethodReportConfigWrapper _SelectedACMetodReportConfigWrapper;
        [ACPropertySelected(999, "ACMethodReportWrapper")]
        public ACMethodReportConfigWrapper SelectedACMetodReportConfigWrapper
        {
            get
            {
                return _SelectedACMetodReportConfigWrapper;
            }
            set
            {
                CreateOrUpdateConfigs();
                _SelectedACMetodReportConfigWrapper = value;
            }
        }

        private List<ACMethodReportConfigWrapper> _ACMethodReportConfigWrapperList;
        [ACPropertyList(999, "ACMethodReportWrapper")]
        public List<ACMethodReportConfigWrapper> ACMethodReportConfigWrapperList
        {
            get
            {
                return _ACMethodReportConfigWrapperList;
            }
            set
            {
                _ACMethodReportConfigWrapperList = value;
                OnPropertyChanged("ACMethodReportConfigWrapperList");
            }
        }

        public FlowDocument FlowDocumentConfig
        {
            get;
            set;
        }

        private ReportConfiguration _CurrentReportConfiguation;
        public ReportConfiguration CurrentReportConfiguration
        {
            get
            {
                return _CurrentReportConfiguation;
            }
            set
            {
                _CurrentReportConfiguation = value;
                FillConfigurationSource();
                OnPropertyChanged("CurrentReportConfiguration");
            }
        }

        private void CreateOrUpdateConfigs()
        {
            if (_SelectedACMetodReportConfigWrapper != null)
            {
                if (CurrentReportConfiguration != null)
                {
                    ConfigurationMethod config = CurrentReportConfiguration.Items.Cast<ConfigurationMethod>().FirstOrDefault(c => c.VBContent == CheckConfigUrl());
                    if (config != null)
                    {
                        ConfigurationParameter param = config.Items.Cast<ConfigurationParameter>().FirstOrDefault(c => c.ParameterName == _SelectedACMetodReportConfigWrapper.ParameterName.ACIdentifier);
                        if (_SelectedACMetodReportConfigWrapper.IsInReport && param == null)
                            config.Items.Add(new ConfigurationParameter() { ParameterName = _SelectedACMetodReportConfigWrapper.ParameterName.ACIdentifier });

                        else if (!_SelectedACMetodReportConfigWrapper.IsInReport && param != null)
                            config.Items.Remove(param);
                    }
                    else if (config == null && _SelectedACMetodReportConfigWrapper.IsInReport)
                    {
                        config = new ConfigurationMethod() { VBContent = CheckConfigUrl() };
                        ConfigurationParameter param = new ConfigurationParameter() { ParameterName = _SelectedACMetodReportConfigWrapper.ParameterName.ACIdentifier };
                        config.Items.Add(param);
                        CurrentReportConfiguration.Items.Add(config);
                    }
                }
                else if (_SelectedACMetodReportConfigWrapper.IsInReport)
                {
                    CurrentReportConfiguration = new ReportConfiguration();
                    ConfigurationMethod config = new ConfigurationMethod() { VBContent = CheckConfigUrl() };
                    ConfigurationParameter param = new ConfigurationParameter() { ParameterName = _SelectedACMetodReportConfigWrapper.ParameterName.ACIdentifier };
                    config.Items.Add(param);
                    CurrentReportConfiguration.Items.Add(config);
                }
            }
        }

        private string CheckConfigUrl()
        {
            if (_SelectedACMetodReportConfigWrapper.ConfigSource.ACIdentifier == "Rules")
                return "Rules";

            string url = _SelectedACMetodReportConfigWrapper.ConfigSource.GetACUrl();
            if (!url.Contains(ACClassMethod.ClassName + "("))
                url += "\\" + ACClassMethod.ClassName + "(" + ACStateEnum.SMStarting + ")";
            return url;
        }

        private ACClassDesign GlobalReportConfig
        {
            get;
            set;
        }

        public void LoadConfig()
        {
            //if (LocalConfig)
            //{
            //    CurrentReportConfiguration = null;
            //    CurrentReportConfiguration = FlowDocumentConfig.Resources["Config"] as ReportConfiguration;
            //}
            //else if (!LocalConfig)
            //{
            //    GlobalReportConfig = Database.ContextIPlus.ACClassDesign.First(c => c.ACIdentifier == "ReportGlobalConfig");
            //    try
            //    {
            //        CurrentReportConfiguration = null;
            //        ResourceDictionary rd = AvaloniaRuntimeXamlLoader.Load(GlobalReportConfig.XAMLDesign) as ResourceDictionary;
            //        if (rd != null && rd.Contains("Config"))
            //            CurrentReportConfiguration = rd["Config"] as ReportConfiguration;
            //    }
            //    catch (Exception e)
            //    {
            //        MessageBox.Show(e.Message, "Global report configuration exception", MessageBoxButton.OK, MessageBoxImage.Warning);
            //    }
            //}
        }

        [ACMethodInfo("", "en{'Apply configuration'}de{Konfiguration anwenden}", 9999)]
        public void ApplyConfig()
        {
            //CreateOrUpdateConfigs();
            //CurrentReportConfiguration.Items.Cast<ConfigurationMethod>().Where(c => c.Items.Count == 0).ToList().ForEach(x => CurrentReportConfiguration.Items.Remove(x));
            //if (LocalConfig)
            //    BroadcastToVBControls(Const.CmdApplyConfig, "CurrentACClassDesign\\XMLDesign", this);
            //else if (GlobalReportConfig != null)
            //{
            //    ResourceDictionary rd = new ResourceDictionary();
            //    rd.Add("Config", CurrentReportConfiguration);
            //    string xaml = XamlWriter.Save(rd);
            //    GlobalReportConfig.XAMLDesign = xaml;
            //    SaveFlowDoc();
            //}
        }
        #endregion

        #region Execute-Helper-Handlers

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(Print):
                    Print((ACClassDesign)acParameter[0], (Boolean)acParameter[1], (String)acParameter[2], (ReportData)acParameter[3], acParameter.Count() == 5 ? (Int32)acParameter[4] : 1);
                    return true;
                case nameof(Preview):
                    result = Preview((ACClassDesign)acParameter[0], (Boolean)acParameter[1], (String)acParameter[2], (ReportData)acParameter[3]);
                    return true;
                case nameof(Design):
                    result = Design((ACClassDesign)acParameter[0], (Boolean)acParameter[1], (String)acParameter[2], (ReportData)acParameter[3]);
                    return true;
                case nameof(FlowDialogCancel):
                    FlowDialogCancel();
                    return true;
                case nameof(FlowDialogOk):
                    FlowDialogOk();
                    return true;
                case nameof(PrinterSelectionOk):
                    PrinterSelectionOk();
                    return true;
                case nameof(IsEnabledPrinterSelectionOk):
                    result = IsEnabledPrinterSelectionOk();
                    return true;
                case nameof(PrinterSelectionCancel):
                    PrinterSelectionCancel();
                    return true;
                case nameof(ApplyConfig):
                    ApplyConfig();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion

        #region GetPropsToObserveForIsEnabled

        public override IEnumerable<string> GetPropsToObserveForIsEnabled(string acMethodName)
        {
            switch (acMethodName)
            {
                #region Print/Preview/Design
                case nameof(Print):
                case nameof(Preview):
                case nameof(Design):
                    return new string[] { nameof(CurrentACClassDesign), nameof(CurrentReportData), nameof(PrinterName) };
                #endregion

                #region FlowDoc Dialog
                case nameof(FlowDialogCancel):
                case nameof(FlowDialogOk):
                    return new string[] { nameof(InitState) };
                #endregion

                #region PrinterSelection Dialog
                case nameof(PrinterSelectionOk):
                case nameof(IsEnabledPrinterSelectionOk):
                    return new string[] { nameof(SelectedWindowsPrinter) };
                case nameof(PrinterSelectionCancel):
                    return new string[] { nameof(InitState) };
                #endregion

                #region Config
                case nameof(ApplyConfig):
                    return new string[] { nameof(InitState) };
                #endregion

                #region PrintComponent
                case nameof(PrintComponentOk):
                case nameof(IsEnabledPrintComponent):
                    return new string[] { nameof(SelectedPrintServer) };
                #endregion
            }
            return base.GetPropsToObserveForIsEnabled(acMethodName);
        }

        #endregion

    }

    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACMethodReportConfigWrapper'}de{'ACMethodReportConfigWrapper'}", Global.ACKinds.TACSimpleClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class ACMethodReportConfigWrapper
    {
        [ACPropertyInfo(999)]
        public ACValue ParameterName
        {
            get;
            set;
        }

        [ACPropertyInfo(999)]
        public bool IsInReport
        {
            get;
            set;
        }

        public ACClassInfoWithItems ConfigSource
        {
            get;
            set;
        }

        public string GetACUrl()
        {
            string result = "";
            if (ConfigSource != null && ConfigSource.ParentContainerT != null && ConfigSource.ParentContainerT.ValueT != null)
            {
                var method = ConfigSource.ParentContainerT.ValueT.Methods.FirstOrDefault(c => c.ACIdentifier == ConfigSource.ACIdentifier);
                if (method != null)
                    result = method.GetACUrl();
            }
            return result;
        }
    }
}
