using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Objects.DataClasses;
using gip.core.datamodel;
using gip.core.autocomponent;
using combit.ListLabel17;
using System.IO;
using gip.core.reporthandler.Flowdoc;
using System.Windows.Xps.Packaging;
using System.Printing;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Documents;
using System.Windows.Xps;

namespace gip.core.reporthandler
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

            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            if (_LL != null)
            {
                UnSubscribeToLLEvents();
                _LL.Dispose();
                _LL = null;
            }
            _PrintServerList = null;
            //if (_SR != null)
            //{
            //    _SR.Dispose();
            //    _SR = null;
            //}
            this._CurrentACClassDesign = null;
            this._CurrentReportData = null;
            return base.ACDeInit(deleteACClassTask);
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
        //private int _Copies;

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
                RunLL(false, LlPrintMode.Export);
            }
            else if (acClassDesign.ACUsage == Global.ACUsages.DUReportPrintServer)
            {
                DoPrintComponent(acClassDesign, withDialog, copies, ReloadOnServer);
            }

            return null;
        }

        [ACMethodInfo("Report", "en{'Preview'}de{'Vorschau'}", 9999, false)]
        public void Preview(ACClassDesign acClassDesign, bool withDialog, string printerName, ReportData data)
        {
            CurrentACClassDesign = acClassDesign;
            CurrentReportData = data;
            WithDialog = withDialog;
            PrinterName = printerName;
            if (acClassDesign.ACUsage == Global.ACUsages.DUReport)
            {
                ShowDialog(this, "PreviewFlowDoc");
            }
            else if (acClassDesign.ACUsageIndex >= (short)Global.ACUsages.DULLReport && acClassDesign.ACUsageIndex <= (short)Global.ACUsages.DULLFilecard)
            {
                RunLL(false, LlPrintMode.Preview);
            }
            else if (acClassDesign.ACUsage == Global.ACUsages.DUReportPrintServer)
            {
                ShowDialog(this, "PreviewFlowDoc");
            }
        }

        [ACMethodInfo("Report", "en{'Design'}de{'Entwurf'}", 9999, false)]
        public void Design(ACClassDesign acClassDesign, bool withDialog, string printerName, ReportData data)
        {
            CurrentACClassDesign = acClassDesign;
            CurrentReportData = data;
            WithDialog = withDialog;
            PrinterName = printerName;
            if (acClassDesign.ACUsage == Global.ACUsages.DUReport)
            {
                ShowDialog(this, "DesignFlowDoc");
            }
            else if (acClassDesign.ACUsageIndex >= (short)Global.ACUsages.DULLReport && acClassDesign.ACUsageIndex <= (short)Global.ACUsages.DULLFilecard)
            {
                RunLL(true);
            }
            else if (acClassDesign.ACUsage == Global.ACUsages.DUReportPrintServer)
            {
                ShowDialog(this, "DesignFlowDoc");
            }
        }

        #endregion

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
                return _PrintServerList;
            }
        }

        #endregion

        #region List & Label

        #region Properties

        #region private
        private bool _LLEventsSubscribed;
        #endregion

        #region public
        private VBListLabel _LL;
        public VBListLabel LL
        {
            get
            {
                return _LL;
            }
        }

        public LlPrintMode PrintMode
        {
            get;
            set;
        }
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

        /// <summary>
        /// In List & Label werden zwei Arten von Datenfeldern grundlegend unterschieden: 
        /// Es gibt Datenfelder, die pro gedruckter Seite (bzw. pro Etikett oder Karteikarte) 
        /// nur einmal mit Daten gefüllt werden (sprich: von Ihrer Anwendung mit Echtdateninhalt angemeldet werden), 
        /// dies sind in der List & Label Terminologie "Variablen". 
        /// Dem gegenüber stehen die Datenfelder, welche mehrfach auf einer Seite mit unterschiedlichen Daten gefüllt werden, 
        /// zum Beispiel die einzelnen Datenfelder einer Postenliste einer Rechnung. 
        /// Diese Datenfelder werden in der List & Label Terminologie "Felder" genannt. 
        /// Diese Felder stehen nur in Tabellenobjekten, Kreuztabellenobjekten und im Berichtscontainer zur Verfügung.
        /// Demzufolge gibt es in Etiketten- und Karteikartenprojekten lediglich Variablen, 
        /// während in Listenprojekten sowohl Variablen als auch Felder vorkommen können. 
        /// Für den Druck einer Rechnung würde eine Anwendung typischerweise die Rechnungskopfdaten wie Empfänger-Name und -Adresse und die Belegnummer als Variablen anmelden, 
        /// hingegen die Postendaten wie Stückzahl, Artikelnummer, Stückpreis etc. als Felder.
        /// </summary>
        protected virtual LlProject ProjectType
        {
            get
            {
                if (CurrentACClassDesign == null)
                    return LlProject.List;
                else if (CurrentACClassDesign.ACUsage == Global.ACUsages.DULLLabel)
                    return LlProject.Label;

                else if (CurrentACClassDesign.ACUsage == Global.ACUsages.DULLFilecard)
                    return LlProject.Card;
                else
                    //if (CurrentACClassDesign.ACUsage == Global.ACUsages.DUList
                    //|| CurrentACClassDesign.ACUsage == Global.ACUsages.DUReport
                    //|| CurrentACClassDesign.ACUsage == Global.ACUsages.DUOverview)
                    return LlProject.List;
            }
        }

        protected virtual bool IsReportWithVariables
        {
            get
            {
                if (ProjectType == LlProject.List && CurrentACClassDesign != null)
                {
                    if (CurrentACClassDesign.ACUsage == Global.ACUsages.DULLReport
                        || CurrentACClassDesign.ACUsage == Global.ACUsages.DULLList)
                        return true;
                }
                return false;
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
        protected virtual VBListLabel OnCreateLLInstance()
        {
            LlLanguage language = LlLanguage.English;
            switch (this.Root.Environment.VBLanguageCode.ToUpper())
            {
                case "DE":
                    language = LlLanguage.German;
                    break;
                case "EN":
                    language = LlLanguage.English;
                    break;
                case "FR":
                    language = LlLanguage.French;
                    break;
                case "IT":
                    language = LlLanguage.Italian;
                    break;
                case "ES":
                    language = LlLanguage.Spanish;
                    break;
                case "RU":
                    language = LlLanguage.Russian;
                    break;
                default:
                    language = LlLanguage.English;
                    break;
            }

            VBListLabel instance = new VBListLabel(language, true);
            instance.LicensingInfo = "/BfwD";
            return instance;
        }
        #endregion

        #region Protected
        protected void SubscribeToLLEvents()
        {
            if (_LL == null || _LLEventsSubscribed)
                return;
            _LLEventsSubscribed = true;
            _LL.AutoDefineField += new AutoDefineElementHandler(OnLL_AutoDefineField);
            _LL.AutoDefineNewLine += new AutoDefineNewLineHandler(OnLL_AutoDefineNewLine);
            _LL.AutoDefineNewPage += new AutoDefineNewPageHandler(OnLL_AutoDefineNewPage);
            _LL.AutoDefineTable += new AutoDefineDataItemHandler(OnLL_AutoDefineTable);
            _LL.AutoDefineTableRelation += new AutoDefineDataItemHandler(OnLL_AutoDefineTableRelation);
            _LL.AutoDefineTableSortOrder += new AutoDefineDataItemHandler(OnLL_AutoDefineTableSortOrder);
            _LL.AutoDefineVariable += new AutoDefineElementHandler(OnLL_AutoDefineVariable);
            _LL.DefinePrintOptions += new DefinePrintOptionsHandler(OnLL_DefinePrintOptions);
            _LL.DesignerPrintJob += new DesignerPrintJobHandler(OnLL_DesignerPrintJob);
            _LL.DrawObject += new DrawObjectHandler(OnLL_DrawObject);
            _LL.DrawPage += new DrawPageHandler(OnLL_DrawPage);
            _LL.DrawProject += new DrawProjectHandler(OnLL_DrawProject);
            _LL.DrawTableField += new DrawTableFieldHandler(OnLL_DrawTableField);
            _LL.DrawTableLine += new DrawTableLineHandler(OnLL_DrawTableLine);
            _LL.Evaluate += new EvaluateHandler(OnLL_Evaluate);
        }

        protected void UnSubscribeToLLEvents()
        {
            if (_LL == null || !_LLEventsSubscribed)
                return;
            _LLEventsSubscribed = false;
            _LL.AutoDefineField -= OnLL_AutoDefineField;
            _LL.AutoDefineNewLine -= OnLL_AutoDefineNewLine;
            _LL.AutoDefineNewPage -= OnLL_AutoDefineNewPage;
            _LL.AutoDefineTable -= OnLL_AutoDefineTable;
            _LL.AutoDefineTableRelation -= OnLL_AutoDefineTableRelation;
            _LL.AutoDefineTableSortOrder -= OnLL_AutoDefineTableSortOrder;
            _LL.AutoDefineVariable -= OnLL_AutoDefineVariable;
            _LL.DefinePrintOptions -= OnLL_DefinePrintOptions;
            _LL.DesignerPrintJob -= OnLL_DesignerPrintJob;
            _LL.DrawObject -= OnLL_DrawObject;
            _LL.DrawPage -= OnLL_DrawPage;
            _LL.DrawProject -= OnLL_DrawProject;
            _LL.DrawTableField -= OnLL_DrawTableField;
            _LL.DrawTableLine -= OnLL_DrawTableLine;
            _LL.Evaluate -= OnLL_Evaluate;
        }

        protected void RunLL(bool designer, LlPrintMode printMode = LlPrintMode.Normal)
        {
            object data = SourceDataForObjectProvider;
            if (data == null)
                return;

            ObjectDataProvider objectDataProvider = null;
            try
            {
                if (this.ACQueryDefinitionRoot != null)
                    objectDataProvider = new ObjectDataProvider(data, this.ACQueryDefinitionRoot, MaxRecursionDepthObjectProvider);
                else
                    objectDataProvider = new ObjectDataProvider(data, MaxRecursionDepthObjectProvider);
                objectDataProvider.HandleEnumerableProperty += new EventHandler<LLHandleEnumerablePropertyEventArgs>(OnHandleObjectProviderEnumerableProperty);

                if (_LL == null)
                    _LL = OnCreateLLInstance();
                SubscribeToLLEvents();
                LL.DataSource = objectDataProvider;

                if (this.IsReportWithVariables)
                {
                    if (ACQueryDefinitionRoot != null)
                        LL.DataMember = ACQueryDefinitionRoot.ChildACUrl;
                    else
                        LL.DataMember = objectDataProvider.RootTableName;
                    LL.AutoMasterMode = LlAutoMasterMode.AsVariables;
                }

                using (MemoryStream memStream = new MemoryStream())
                {
                    if (CurrentACClassDesign.DesignBinary != null && CurrentACClassDesign.DesignBinary.Any())
                    {
                        using (MemoryStream memStream2 = new MemoryStream(CurrentACClassDesign.DesignBinary, true))
                        {
                            memStream.Write(CurrentACClassDesign.DesignBinary, 0, (int)memStream2.Length);
                            memStream.Seek(0, SeekOrigin.Begin);
                        }
                    }

                    if (designer)
                    {
                        LL.Design(this.ProjectType, memStream);
                        CurrentACClassDesign.DesignBinary = memStream.GetBuffer();
                    }
                    else
                    {
                        LL.AutoDestination = printMode;
                        if (WithDialog)
                            LL.AutoBoxType = LlBoxType.StandardAbort;
                        else
                        {
                            LL.AutoShowSelectFile = false;
                            LL.AutoShowPrintOptions = false;
                        }
                        if (!string.IsNullOrEmpty(PrinterName))
                            LL.Print(memStream, ProjectType, PrinterName);
                        else
                            LL.Print(ProjectType, memStream);
                    }
                }
            }

            catch (ListLabelException e /*LlException*/)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                this.Root().Messages.LogException("VBBSOReport", "RunLL", msg);
                // Catch Exceptions
                //MessageBox.Show("Information: " + LlException.Message + "\n\nThis information was generated by a List & Label custom exception.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception e /*e*/)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                this.Root().Messages.LogException("VBBSOReport", "RunLL(10)", msg);
            }
            finally
            {
                if (objectDataProvider != null)
                    objectDataProvider.HandleEnumerableProperty -= OnHandleObjectProviderEnumerableProperty;
                if (_LL != null)
                {
                    UnSubscribeToLLEvents();
                    _LL.Dispose();
                    _LL = null;
                }
            }
        }

        //private void GetStiBusinessObjectData(ACQueryDefinition qry, IACObject parent, List<StiBusinessObjectData> dataList)
        //{
        //    foreach (ACQueryDefinition child in qry.ACQueryDefinitionChilds)
        //    {
        //        if (!child.IsUsed)
        //            continue;
        //        IQueryable queryPos = parent.ACSelect(child);
        //        foreach (IACObject pos in queryPos)
        //        {
        //            dataList.Add(new StiBusinessObjectData("Test", child.ChildACUrl, queryPos));
        //            GetStiBusinessObjectData(child, pos, dataList);
        //            break;
        //        }
        //    }
        //}


        //protected void RunSR(bool designer, LlPrintMode printMode = LlPrintMode.Normal)
        //{
        //    object data = SourceDataForObjectProvider;
        //    if (data == null)
        //        return;

        //    //SR.RegData("RootData", data);
        //    List<String> assemblies = SR.ReferencedAssemblies.ToList();
        //    foreach (Assembly classAssembly in AppDomain.CurrentDomain.GetAssemblies())
        //    {
        //        try
        //        {
        //            if (classAssembly.GlobalAssemblyCache
        //                || classAssembly.IsDynamic
        //                || classAssembly.EntryPoint != null
        //                || String.IsNullOrEmpty(classAssembly.Location))
        //                continue;
        //            if (!assemblies.Contains(classAssembly.Location))
        //                assemblies.Add(classAssembly.Location);
        //        }
        //        catch (Exception e)
        //        {
        //        }
        //    }

        //    //assemblies.Add(System.Reflection.Assembly.GetAssembly(typeof(ACClass)).Location);
        //    SR.ReferencedAssemblies = assemblies.ToArray();
        //    List<StiBusinessObjectData> dataList = new List<StiBusinessObjectData>();
        //    dataList.Add(new StiBusinessObjectData("Test", ACQueryDefinitionRoot.ChildACUrl, data));

        //    foreach (IACObject objValue in data as IEnumerable)
        //    {
        //        GetStiBusinessObjectData(ACQueryDefinitionRoot, objValue, dataList);
        //        break;
        //    }
        //    SR.RegBusinessObject(dataList);

        //    //if (this.IsReportWithVariables)
        //    //{
        //    //    if (ACQueryDefinitionRoot != null)
        //    //        LL.DataMember = ACQueryDefinitionRoot.ChildACUrl;
        //    //    else
        //    //        LL.DataMember = objectDataProvider.RootTableName;
        //    //    LL.AutoMasterMode = LlAutoMasterMode.AsVariables;
        //    //}

        //    try
        //    {
        //        using (MemoryStream memStream = new MemoryStream())
        //        {
        //            if (CurrentACClassDesign.DesignBinary != null && CurrentACClassDesign.DesignBinary.Any())
        //            {
        //                using (MemoryStream memStream2 = new MemoryStream(CurrentACClassDesign.DesignBinary, true))
        //                {
        //                    memStream.Write(CurrentACClassDesign.DesignBinary, 0, (int)memStream2.Length);
        //                    memStream.Seek(0, SeekOrigin.Begin);
        //                }
        //            }

        //            if (designer)
        //            {
        //                SR.Load(memStream);
        //                SR.Design();
        //                CurrentACClassDesign.DesignBinary = SR.SaveDocumentToByteArray();
        //            }
        //            else
        //            {
        //                //LL.AutoDestination = printMode;
        //                //if (WithDialog)
        //                //    LL.AutoBoxType = LlBoxType.StandardAbort;
        //                //else
        //                //{
        //                //    LL.AutoShowSelectFile = false;
        //                //    LL.AutoShowPrintOptions = false;
        //                //}
        //                //if (!string.IsNullOrEmpty(PrinterName))
        //                //    LL.Print(memStream, ProjectType, PrinterName);
        //                //else
        //                //    LL.Print(ProjectType, memStream);
        //            }
        //        }
        //    }

        //    catch (ListLabelException /*LlException*/)
        //    {
        //        // Catch Exceptions
        //        //MessageBox.Show("Information: " + LlException.Message + "\n\nThis information was generated by a List & Label custom exception.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //    }
        //    catch (Exception /*e*/)
        //    {
        //    }
        //    finally
        //    {
        //    }
        //}

        #endregion

        #region Event callbacks

        protected virtual void OnHandleObjectProviderEnumerableProperty(object sender, LLHandleEnumerablePropertyEventArgs e)
        {
            if (!typeof(EntityObject).IsAssignableFrom(e.ObjectType)
                && e.ObjectType.Namespace == "gip.core.datamodel")
                e.CancelRecursion = true;
            else if (e.PropertyPath.IndexOf("ACType") >= 0
                || e.PropertyPath.IndexOf(ACClass.ClassName) >= 0
                || e.PropertyPath.IndexOf("ObjectType") >= 0
                || e.PropertyPath.IndexOf("ObjectFullType") >= 0
                || e.PropertyPath.IndexOf("ParentACObject") >= 0
                || e.PropertyPath.IndexOf("ACContent") >= 0
                || e.PropertyPath.IndexOf("ACProperty") >= 0
                || e.PropertyPath.IndexOf("DocumentationList") >= 0
                || e.PropertyPath.IndexOf("Businessobject") >= 0)
                e.CancelRecursion = true;
        }

        protected virtual void OnLL_Evaluate(object sender, EvaluateEventArgs e)
        {
        }

        protected virtual void OnLL_DrawTableLine(object sender, DrawTableLineEventArgs e)
        {
        }

        protected virtual void OnLL_DrawTableField(object sender, DrawTableFieldEventArgs e)
        {
        }

        protected virtual void OnLL_DrawProject(object sender, DrawProjectEventArgs e)
        {
        }

        protected virtual void OnLL_DrawPage(object sender, DrawPageEventArgs e)
        {
        }

        protected virtual void OnLL_DrawObject(object sender, DrawObjectEventArgs e)
        {
        }

        protected virtual void OnLL_DesignerPrintJob(object sender, DesignerPrintJobEventArgs e)
        {
        }

        protected virtual void OnLL_DefinePrintOptions(object sender, EventArgs e)
        {
        }

        protected virtual void OnLL_AutoDefineVariable(object sender, AutoDefineElementEventArgs e)
        {
        }

        protected virtual void OnLL_AutoDefineTableSortOrder(object sender, AutoDefineDataItemEventArgs e)
        {
        }

        protected virtual void OnLL_AutoDefineTableRelation(object sender, AutoDefineDataItemEventArgs e)
        {
        }

        protected virtual void OnLL_AutoDefineTable(object sender, AutoDefineDataItemEventArgs e)
        {
        }

        protected virtual void OnLL_AutoDefineNewPage(object sender, AutoDefineNewPageEventArgs e)
        {
        }

        protected virtual void OnLL_AutoDefineNewLine(object sender, AutoDefineNewLineEventArgs e)
        {
        }

        protected virtual void OnLL_AutoDefineField(object sender, AutoDefineElementEventArgs e)
        {
        }
        #endregion

        #endregion

        #endregion

        #region FlowDoc

        [ACMethodCommand("Report", Const.Cancel, (short)MISort.Cancel)]
        public void FlowDialogCancel()
        {
            UndoFlowDoc();
            CloseTopDialog();
        }

        [ACMethodCommand("Report", Const.Ok, (short)MISort.Okay)]
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

        public Msg FlowPrint(ACClassDesign acClassDesign, bool withDialog, string printerName, ReportData data, int copies, int maxPrintJobsInSpooler = 0)
        {
            if (acClassDesign == null || data == null)
                return null;

            using (ReportDocument reportDoc = new ReportDocument(CurrentACClassDesign.XMLDesign))
            { 
                if (reportDoc == null)
                    return null;
                XpsDocument xps = null;
                if (!String.IsNullOrEmpty(printerName) && printerName.StartsWith("file://"))
                {
                    string fileName = printerName.Substring(7);
                    xps = reportDoc.CreateXpsDocument(data, fileName);
                    return null;
                }
                else
                    xps = reportDoc.CreateXpsDocument(data);
                if (xps == null)
                    return null;

                using (xps)
                {
                    FixedDocumentSequence fDocSeq = null;
                    try
                    {
                        fDocSeq = xps.GetFixedDocumentSequence();
                        if (fDocSeq == null)
                            return null;
                        // Fix for leaking memory
                        // https://social.msdn.microsoft.com/Forums/vstudio/en-US/c6511918-17f6-42be-ac4c-459eeac676fd/memory-leak-when-launching-new-sta-thread-to-convert-xps-to-images?forum=wpf
                        //var docpage = fDocSeq.DocumentPaginator.GetPage(0);
                        //if (docpage != null && docpage.Visual != null)
                        //{
                        //    FixedPage fixedPage = docpage.Visual as FixedPage;
                        //    if (fixedPage != null)
                        //        fixedPage.UpdateLayout();
                        //}

                        if (copies <= 0)
                            copies = 1;

                        if (withDialog)
                        {
                            try
                            {
                                var printDialog = new System.Windows.Controls.PrintDialog();
                                if (printDialog.ShowDialog() == true)
                                {
                                    PrintQueue pQ = printDialog.PrintQueue;
                                    XpsDocumentWriter writer = PrintQueue.CreateXpsDocumentWriter(pQ);
                                    if (writer != null)
                                    {
                                        PrintTicket pt = new PrintTicket();
                                        pt.CopyCount = printDialog.PrintTicket != null && printDialog.PrintTicket.CopyCount.HasValue ? printDialog.PrintTicket.CopyCount : copies;
                                        if (reportDoc.AutoSelectPageOrientation.HasValue)
                                        {
                                            pt.PageOrientation = reportDoc.AutoSelectPageOrientation;
                                        }
                                        else
                                        {
                                            if (reportDoc.PageWidth > reportDoc.PageHeight)
                                                pt.PageOrientation = PageOrientation.Landscape;
                                            else
                                                pt.PageOrientation = PageOrientation.Portrait;
                                        }

                                        if (reportDoc.AutoPageMediaSize != null)
                                            pt.PageMediaSize = reportDoc.AutoPageMediaSize;

                                        // example of calling above code
                                        if (reportDoc.AutoSelectTray.HasValue)
                                        {
                                            string nameSpaceURI = string.Empty;
                                            string selectedtray = XpsPrinterUtils.GetInputBinName(pQ.Name, reportDoc.AutoSelectTray.Value, out nameSpaceURI);
                                            pt = XpsPrinterUtils.ModifyPrintTicket(pt, "psk:JobInputBin", selectedtray, nameSpaceURI);
                                        }

                                        writer.Write(fDocSeq, pt);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                this.Root().Messages.LogException("VBBSOReport", "FlowPrint(10)", e.Message);
                                PrintDocumentImageableArea area = null;
                                XpsDocumentWriter writer = PrintQueue.CreateXpsDocumentWriter(ref area);
                                if (writer != null)
                                    writer.Write(fDocSeq);
                            }
                        }
                        else
                        {
                            PrintQueue pQ = null;
                            if (!String.IsNullOrEmpty(reportDoc.AutoSelectPrinterName))
                                printerName = reportDoc.AutoSelectPrinterName;
                            if (String.IsNullOrEmpty(printerName))
                            {
                                pQ = LocalPrintServer.GetDefaultPrintQueue();
                            }
                            else
                            {
                                if (printerName.StartsWith("\\\\"))
                                {
                                    int index = printerName.LastIndexOf("\\");
                                    if (index > 0)
                                    {
                                        string server = printerName.Substring(0, index);
                                        string printerName2 = printerName.Substring(index + 1);
                                        PrintServer pServer = new PrintServer(server);
                                        if (pServer != null)
                                        {
                                            pQ = pServer.GetPrintQueues().Where(c => c.Name == printerName2).FirstOrDefault();
                                        }
                                    }
                                }
                                else
                                {
                                    PrintServer pServer = new LocalPrintServer();
                                    if (pServer != null)
                                    {
                                        pQ = pServer.GetPrintQueues().Where(c => c.Name == printerName).FirstOrDefault();
                                    }
                                    if (pQ == null)
                                    {
                                        pServer = new PrintServer();
                                        PrintQueueCollection printQueues = pServer.GetPrintQueues(new[] { EnumeratedPrintQueueTypes.Local, EnumeratedPrintQueueTypes.Connections, EnumeratedPrintQueueTypes.Shared });
                                        pQ = printQueues.Where(c => c.Name == printerName).FirstOrDefault();
                                    }
                                }
                            }
                            if (pQ == null)
                                return null;

                            if (maxPrintJobsInSpooler > 0
                                && (pQ.IsInError || pQ.NumberOfJobs >= maxPrintJobsInSpooler))
                            {
                                return new Msg(this, eMsgLevel.Question, "VBBSOReport", "FlowPrint", 947, "Question50078", eMsgButton.YesNo);
                            }

                            XpsDocumentWriter writer = PrintQueue.CreateXpsDocumentWriter(pQ);
                            if (writer != null)
                            {
                                PrintTicket pt = new PrintTicket();
                                pt.CopyCount = copies;
                                if (reportDoc.AutoSelectPageOrientation.HasValue)
                                {
                                    pt.PageOrientation = reportDoc.AutoSelectPageOrientation;
                                }
                                else
                                {
                                    if (reportDoc.PageWidth > reportDoc.PageHeight)
                                        pt.PageOrientation = PageOrientation.Landscape;
                                    else
                                        pt.PageOrientation = PageOrientation.Portrait;
                                }

                                if (reportDoc.AutoPageMediaSize != null)
                                    pt.PageMediaSize = reportDoc.AutoPageMediaSize;

                                // example of calling above code
                                if (reportDoc.AutoSelectTray.HasValue)
                                {
                                    string nameSpaceURI = string.Empty;
                                    string selectedtray = XpsPrinterUtils.GetInputBinName(pQ.Name, reportDoc.AutoSelectTray.Value, out nameSpaceURI);
                                    pt = XpsPrinterUtils.ModifyPrintTicket(pt, "psk:JobInputBin", selectedtray, nameSpaceURI);
                                }

                                //for (int i = 0; i < copies; i++)
                                //{
                                try
                                {
                                    writer.Write(fDocSeq, pt);
                                }
                                catch (Exception ex2)
                                {
                                    this.Root().Messages.LogException("VBBSOReport", "FlowPrint(50)", ex2.Message);
                                }
                                //}
                            }
                        }
                    }
                    finally
                    {
                        if (xps != null)
                            xps.Close();
                        try
                        {
                            // https://stackoverflow.com/questions/8742454/saving-a-fixeddocument-to-an-xps-file-causes-memory-leak
                            if (fDocSeq != null)
                            {
                                for (int i = 0; i < fDocSeq.DocumentPaginator.PageCount; i++)
                                {
                                    var docpage = fDocSeq.DocumentPaginator.GetPage(i);
                                    if (docpage != null && docpage.Visual != null)
                                    {
                                        FixedPage fixedPage = docpage.Visual as FixedPage;
                                        if (fixedPage != null)
                                        {
                                            fixedPage.Children.Clear();
                                            fixedPage.UpdateLayout();
                                            //var dispatcher = fixedPage.Dispatcher;
                                            //if (dispatcher != null
                                            //    && dispatcher.Thread != null
                                            //    && !String.IsNullOrEmpty(dispatcher.Thread.Name)
                                            //    && (dispatcher.Thread.Name.Contains(RootDbOpQueue.ClassName)
                                            //        || dispatcher.Thread.Name.Contains(ACUrlHelper.Delimiter_DirSeperator)))
                                            //{
                                            //    fixedPage.DetachFromDispatcherExt();
                                            //}
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex2)
                        {
                            this.Root().Messages.LogException("VBBSOReport", "FlowPrint(99)", ex2.Message);
                        }
                    }
                }
            }

            return null;
        }

        #endregion

        #region ACPrintServer component

        [ACMethodCommand("Report", Const.Ok, (short)MISort.Okay)]
        public void PrintComponentDialogOk()
        {
            ACSaveChanges();
            if (Database.ContextIPlus != null)
                Database.ContextIPlus.ACSaveChanges();
            CloseTopDialog();
        }

        [ACMethodInfo("PrintComponent", "en{'Print'}de{'Drucken'}", 9999, false)]
        public void PrintComponent()
        {
            CloseTopDialog();
            ShowDialog(this, "PrintXMLDoc");
        }

        [ACMethodInfo("PrintComponentOk", "en{'Print'}de{'Drucken'}", 9999, false)]
        public void PrintComponentOk()
        {
            if (!IsEnabledPrintComponent())
                return;
            PrintOnServer(CurrentACClassDesign, SelectedPrintServer.PrinterACUrl, CopyCount, ReloadOnServer);
            CloseTopDialog();
        }

        private bool IsEnabledPrintComponent()
        {
            return SelectedPrintServer != null;
        }

        private void DoPrintComponent(ACClassDesign acClassDesign, bool withDialog, int copies, bool reloadReport)
        {
            if (withDialog)
                PrintComponent();
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
                Root.Messages.Error(this, "Error50473", false, acPrintServerACUrl);
                return;
            }
            if (printServer.ConnectionState == ACObjectConnectionState.DisConnected)
            {
                // Error50474
                Root.Messages.Error(this, "Error50474", false, acPrintServerACUrl);
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
                    Root.Messages.Error(this, "Error50475", false, ParentACComponent.GetACUrl());
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
                                                               .GroupBy(x => x.ACIdentifier).Select(q => q.FirstOrDefault());
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
            if (LocalConfig)
            {
                CurrentReportConfiguration = null;
                CurrentReportConfiguration = FlowDocumentConfig.Resources["Config"] as ReportConfiguration;
            }
            else if (!LocalConfig)
            {
                GlobalReportConfig = Database.ContextIPlus.ACClassDesign.First(c => c.ACIdentifier == "ReportGlobalConfig");
                try
                {
                    CurrentReportConfiguration = null;
                    ResourceDictionary rd = XamlReader.Parse(GlobalReportConfig.XMLDesign) as ResourceDictionary;
                    if (rd != null && rd.Contains("Config"))
                        CurrentReportConfiguration = rd["Config"] as ReportConfiguration;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Global report configuration exception", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        [ACMethodInfo("", "en{'Apply configuration'}de{Konfiguration anwenden}", 9999)]
        public void ApplyConfig()
        {
            CreateOrUpdateConfigs();
            CurrentReportConfiguration.Items.Cast<ConfigurationMethod>().Where(c => c.Items.Count == 0).ToList().ForEach(x => CurrentReportConfiguration.Items.Remove(x));
            if (LocalConfig)
                BroadcastToVBControls(Const.CmdApplyConfig, "CurrentACClassDesign\\XMLDesign", this);
            else if (GlobalReportConfig != null)
            {
                ResourceDictionary rd = new ResourceDictionary();
                rd.Add("Config", CurrentReportConfiguration);
                string xaml = XamlWriter.Save(rd);
                GlobalReportConfig.XMLDesign = xaml;
                SaveFlowDoc();
            }
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
                    Preview((ACClassDesign)acParameter[0], (Boolean)acParameter[1], (String)acParameter[2], (ReportData)acParameter[3]);
                    return true;
                case nameof(Design):
                    Design((ACClassDesign)acParameter[0], (Boolean)acParameter[1], (String)acParameter[2], (ReportData)acParameter[3]);
                    return true;
                case nameof(FlowDialogCancel):
                    FlowDialogCancel();
                    return true;
                case nameof(FlowDialogOk):
                    FlowDialogOk();
                    return true;
                case nameof(ApplyConfig):
                    ApplyConfig();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
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
