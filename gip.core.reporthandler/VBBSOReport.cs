using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Data.Objects.DataClasses;
using gip.core.datamodel;
using System.Transactions;
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
using System.Xml;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Threading;

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

        #region public Methods
        [ACMethodInfo("Report", "en{'Print'}de{'Drucken'}", 9999, false)]
        public void Print(ACClassDesign acClassDesign, bool withDialog, string printerName, ReportData data, int copies = 1)
        {
            if (acClassDesign == null || data == null)
                return;
            CurrentACClassDesign = acClassDesign;
            CurrentReportData = data;
            WithDialog = withDialog;
            PrinterName = printerName;
            if (acClassDesign.ACUsage == Global.ACUsages.DUReport)
            {
                if (Thread.CurrentThread.ApartmentState == ApartmentState.STA)
                    ReportDocument.FlowPrint(acClassDesign.XMLDesign, withDialog, printerName, data, copies);
                else
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ReportDocument.FlowPrintAsync(acClassDesign.XMLDesign, false, printerName, data, copies);
                    });

            }
            else if (acClassDesign.ACUsageIndex >= (short)Global.ACUsages.DULLReport && acClassDesign.ACUsageIndex <= (short)Global.ACUsages.DULLFilecard)
            {
                RunLL(false, LlPrintMode.Export);
            }
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
        }

        #endregion

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

            string url = _SelectedACMetodReportConfigWrapper.GetACUrl();
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

        [ACMethodInfo("", "en", 999)]
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
                case "Print":
                    Print((ACClassDesign)acParameter[0], (Boolean)acParameter[1], (String)acParameter[2], (ReportData)acParameter[3], acParameter.Count() == 5 ? (Int32)acParameter[4] : 1);
                    return true;
                case "Preview":
                    Preview((ACClassDesign)acParameter[0], (Boolean)acParameter[1], (String)acParameter[2], (ReportData)acParameter[3]);
                    return true;
                case "Design":
                    Design((ACClassDesign)acParameter[0], (Boolean)acParameter[1], (String)acParameter[2], (ReportData)acParameter[3]);
                    return true;
                case "FlowDialogCancel":
                    FlowDialogCancel();
                    return true;
                case "FlowDialogOk":
                    FlowDialogOk();
                    return true;
                case "ApplyConfig":
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
