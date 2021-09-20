using System;
using System.Collections.Generic;
using System.Linq;
using gip.core.datamodel;
using gip.core.autocomponent;

namespace gip.core.reporthandler
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Report'}de{'Bericht'}", Global.ACKinds.TACBSOGlobal, Global.ACStorableTypes.NotStorable, false, false)]
    public class VBBSOReportDialog : ACBSO
    {
        #region c´tors
        public VBBSOReportDialog(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;

            CurrentCurrentOrList = Global.CurrentOrList.Current;

            _ACClassDesignList = new List<ACClassDesign>();
            _QueryACClassList = new List<ACClass>();

            if (ParentBSO != null)
            {
                ACBSONav navBSO = ParentBSO as ACBSONav;
                if (navBSO != null)
                {
                    foreach (IACConfig acClassConfig in navBSO.NavigationqueryList)
                    {
                        ACComposition acComposition = acClassConfig[Const.Value] as ACComposition;
                        if (acComposition == null)
                            continue;
                        ACClass acClass = acComposition.GetComposition(Database.ContextIPlus) as ACClass;
                        if (acClass == null)
                        {
                            Messages.LogError(this.GetACUrl(), "ACInit()", String.Format("Class {0} for Qry {1} does not exist in Database", acComposition.ACUrlComposition, (acClassConfig as ACClassConfig).ACClassConfigID));
                            continue;
                        }
                        _QueryACClassList.Add(acClass);
                        foreach (var acClassDesign in acClass.Designs.Where(c => c.ACKind == Global.ACKinds.DSDesignReport).OrderBy(c => c.SortIndex))
                        {
                            _ACClassDesignList.Add(acClassDesign);
                        }
                    }
                }

                ACClass parentACClass = ParentBSO.ACType as ACClass;
                if (parentACClass != null)
                {
                    if (parentACClass.Database != Database.ContextIPlus)
                        parentACClass = parentACClass.FromIPlusContext<ACClass>(Database.ContextIPlus);
                    if (parentACClass != null)
                        _ACClassDesignList.AddRange(parentACClass.Designs.Where(c => c.ACKind == Global.ACKinds.DSDesignReport));
                }            
            }


            using (ACMonitor.Lock(gip.core.datamodel.Database.GlobalDatabase.QueryLock_1X000))
            {
                ACClass acClassVS = gip.core.datamodel.Database.GlobalDatabase.ACClass.Where(c => c.ACIdentifier == "BSOiPlusStudio").First();
                _CMModifyQuery = acClassVS.GetRight(acClassVS);
            }

            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            this._ACClassDesignList = null;
            this._CurrentACClassDesign = null;
            this._CurrentACQueryDefinition = null;
            this._CurrentQueryACClass = null;
            this._CurrentSelectedACQueryDefinition = null;
            this._PrinterName = null;
            this._QueryACClassList = null;
            this._ReportACIdentifier = null;
            this._SelectedACClassDesign = null;
            this._SelectedQueryACClass = null;
            return base.ACDeInit(deleteACClassTask);
        }

        #endregion

        #region BSO->ACProperty

        #region ACQueryDefinition
        ACQueryDefinition _CurrentACQueryDefinition;
        [ACPropertyCurrent(9999, ACQueryDefinition.ClassName)]
        public ACQueryDefinition CurrentACQueryDefinition
        {
            get
            {
                return _CurrentACQueryDefinition;
            }
            set
            {
                _CurrentACQueryDefinition = value;
                OnPropertyChanged("CurrentACQueryDefinition");
            }
        }

        ACQueryDefinition _CurrentSelectedACQueryDefinition;
        [ACPropertyCurrent(9999, "SelectedACQueryDefinition")]
        public ACQueryDefinition CurrentSelectedACQueryDefinition
        {
            get
            {
                return _CurrentSelectedACQueryDefinition;
            }
            set
            {
                _CurrentSelectedACQueryDefinition = value;
                OnPropertyChanged("CurrentSelectedACQueryDefinition");
            }
        }

        #endregion

        #region ACClassDesign
        ACClassDesign _CurrentACClassDesign;
        [ACPropertyCurrent(9999, "ACClassDesign")]
        public ACClassDesign CurrentACClassDesign
        {
            get
            {
                return _CurrentACClassDesign;
            }
            set
            {
                if (_CurrentACClassDesign != value)
                {
                    _CurrentACClassDesign = value;
                    if (_CurrentACClassDesign != null && _CurrentACClassDesign.ACClass != null)
                    {
                        if (IsCurrentDesignFromParentBSO)
                        {
                            CurrentACQueryDefinition = null;
                            CurrentSelectedACQueryDefinition = null;
                        }
                        else
                        {
                            try
                            {
                                CurrentACQueryDefinition = Root.Queries.CreateQueryByClass(null, _CurrentACClassDesign.ACClass, "");
                                CurrentSelectedACQueryDefinition = CurrentACQueryDefinition;
                            }
                            catch (Exception e)
                            {
                                CurrentACQueryDefinition = null;
                                CurrentSelectedACQueryDefinition = null;

                                string msg = e.Message;
                                if (e.InnerException != null && e.InnerException.Message != null)
                                    msg += " Inner:" + e.InnerException.Message;

                                Messages.LogException("VBBSOReportDialog", "CurrentACClassDesign", msg);
                            }
                        }
                    }
                    else
                    {
                        CurrentACQueryDefinition = null;
                        CurrentSelectedACQueryDefinition = null;
                    }
                    OnPropertyChanged("CurrentACClassDesign");
                }
            }
        }

        ACClassDesign _SelectedACClassDesign;
        [ACPropertySelected(9999, "ACClassDesign")]
        public ACClassDesign SelectedACClassDesign
        {
            get
            {
                return _SelectedACClassDesign;
            }
            set
            {
                if(_SelectedACClassDesign != value)
                {
                    _SelectedACClassDesign = value;
                        CurrentACClassDesign = _SelectedACClassDesign;
                    OnPropertyChanged("SelectedACClassDesign");
                }
            }
        }

        List<ACClassDesign> _ACClassDesignList = null;
        [ACPropertyList(9999, "ACClassDesign")]
        public IEnumerable<ACClassDesign> ACClassDesignList
        {
            get
            {
                if (_ACClassDesignList == null)
                    return null;
                return _ACClassDesignList;
            }
        }

        ACClassDesign _SelectedACClassDesignPreview;
        [ACPropertySelected(9999, "ACClassDesignPreview")]
        public ACClassDesign SelectedACClassDesignPreview
        {
            get
            {
                return _SelectedACClassDesignPreview;
            }
            set
            {
                if (_SelectedACClassDesignPreview != value)
                {
                    _SelectedACClassDesignPreview = value;
                    SelectedACClassDesign = _SelectedACClassDesignPreview;
                    OnPropertyChanged("SelectedACClassDesignPreview");
                }
            }
        }

        List<ACClassDesign> _ACClassDesignListPreview = null;
        [ACPropertyList(9999, "ACClassDesignPreview")]
        public IEnumerable<ACClassDesign> ACClassDesignListPreview
        {
            get
            {
                if (_ACClassDesignListPreview == null)
                    _ACClassDesignListPreview = new List<ACClassDesign>();

                _ACClassDesignListPreview.Clear();
                var grouped = ACClassDesignList.Where(c => c.ACKind == Global.ACKinds.DSDesignReport).GroupBy(x => x.ACIdentifier);
                var singles = grouped.Where(c => c.Count() == 1).Select(x => x.FirstOrDefault());
                var duplicates = grouped.Where(c => c.Count() > 1);
                var duplicatesChecked = duplicates.Select(c => c.FirstOrDefault(x => x.ACClass.ACClassID == ((ACClass)ParentBSO.ACType).ACClassID));

                _ACClassDesignListPreview.AddRange(singles);
                _ACClassDesignListPreview.AddRange(duplicatesChecked);

                return _ACClassDesignListPreview.OrderBy(c => c.SortIndex).ThenBy(x => x.ACIdentifier).ToList();
            }
        }

        private bool IsCurrentDesignFromParentBSO
        {
            get
            {
                if (CurrentACClassDesign == null)
                    return false;
                return (ParentBSO != null && (ParentBSO.ACType == CurrentACClassDesign.ACClass || (ParentBSO.ACType as ACClass).IsDerivedClassFrom(CurrentACClassDesign.ACClass)));
            }
        }

        #endregion

        #region ACQuery-Class

        ACClass _CurrentQueryACClass;
        [ACPropertyCurrent(9999, "QueryACClass", "en{'Query of Report'}de{'Abfrage für Report'}")]
        public ACClass CurrentQueryACClass
        {
            get
            {
                return _CurrentQueryACClass;
            }
            set
            {
                if (_CurrentQueryACClass == value)
                    return;
                _CurrentQueryACClass = value;
                OnPropertyChanged("CurrentQueryACClass");
            }
        }

        ACClass _SelectedQueryACClass;
        [ACPropertySelected(9999, "QueryACClass", "en{'Query of Report'}de{'Abfrage für Report'}")]
        public ACClass SelectedQueryACClass
        {
            get
            {
                return _SelectedQueryACClass;
            }
            set
            {
                _SelectedQueryACClass = value;
                if (_CurrentQueryACClass != _SelectedQueryACClass)
                    CurrentQueryACClass = _SelectedQueryACClass;
                OnPropertyChanged("SelectedQueryACClass");
            }
        }


        List<ACClass> _QueryACClassList;
        [ACPropertyList(9999, "QueryACClass", "en{'Query of Report'}de{'Abfrage für Report'}")]
        public IEnumerable<ACClass> QueryACClassList
        {
            get
            {
                return _QueryACClassList.OrderBy(c => c.ACIdentifier);
            }
        }

        //ACClassDesign _CurrentFlowDocDesign;
        //FlowDocument _CurrentFlowDoc = null;
        //public FlowDocument FlowDoc
        //{
        //    get
        //    {
        //        if (CurrentACClassDesign == null)
        //            return null;
        //        if (CurrentACClassDesign.ACUsage == Global.ACUsages.DULayout)
        //        {
        //            if (CurrentACClassDesign != _CurrentFlowDocDesign && !String.IsNullOrEmpty(CurrentACClassDesign.XMLDesign))
        //            {
        //                _CurrentFlowDocDesign = CurrentACClassDesign;
        //                ReportDocument reportDoc = new ReportDocument(CurrentACClassDesign.XMLDesign);
        //                _CurrentFlowDoc = reportDoc.CreateFlowDocument();
        //            }
        //            else if (String.IsNullOrEmpty(CurrentACClassDesign.XMLDesign))
        //                _CurrentFlowDoc = null;
        //        }
        //        else
        //            _CurrentFlowDoc = null;
        //        return _CurrentFlowDoc;
        //    }
        //}

        //FixedDocumentSequence _CurrentXPSDocument = null;
        //public FixedDocumentSequence CurrentXPSDocument
        //{
        //    get
        //    {
        //        return _CurrentXPSDocument;
        //    }
        //    set
        //    {
        //        _CurrentXPSDocument = value;
        //        OnPropertyChanged("CurrentXPSDocument");
        //    }
        //}


        //RichTextBox _DocRichTextBox;
        //public RichTextBox DocRichTextBox
        //{
        //    get
        //    {
        //        if (FlowDoc == null)
        //            return null;
        //        if (_DocRichTextBox != null)
        //            return _DocRichTextBox;
        //        _DocRichTextBox = new RichTextBox(FlowDoc);
        //        _DocRichTextBox.Width = 800;
        //        _DocRichTextBox.Height = 600;
        //        return _DocRichTextBox;
        //    }
        //}
        #endregion

        #region CurrentOrList

        bool _IsDesignOfACQuery = false;
        [ACPropertyInfo(9999, "", "en{'Is design of query'}de{'Design von Abfrageklasse'}")]
        public bool IsDesignOfACQuery
        {
            get
            {
                return _IsDesignOfACQuery;
            }
            set
            {
                _IsDesignOfACQuery = value;
                OnPropertyChanged("IsDesignOfACQuery");
            }
        }


        Global.CurrentOrList _CurrentCurrentOrList = Global.CurrentOrList.Current;
        [ACPropertyInfo(9999, "en{'Recordcount'}de{'Datensatzanzahl'}")]
        public Global.CurrentOrList CurrentCurrentOrList
        {
            get
            {
                return _CurrentCurrentOrList;
            }
            set
            {
                _CurrentCurrentOrList = value;
                OnPropertyChanged("CurrentCurrentOrList");
            }
        }
        #endregion

        #region Options
        bool _WithDialog = true;
        [ACPropertyInfo(100, "", "en{'Printerselection'}de{'Druckerauswahl'}")]
        public bool WithDialog
        {
            get
            {
                return _WithDialog;
            }
            set
            {
                _WithDialog = value;
                OnPropertyChanged("WithDialog");
            }
        }

        string _PrinterName = "";
        [ACPropertyInfo(9999)]
        public string PrinterName
        {
            get
            {
                return _PrinterName;
            }
            set
            {
                _PrinterName = value;
                OnPropertyChanged("PrinterName");
            }
        }
       
        private bool _PreventClone = false;
        [ACPropertyInfo(9999)]
        public bool PreventClone
        {
            get
            {
                return _PreventClone;
            }
            set
            {
                _PreventClone = value;
                OnPropertyChanged("PreventClone");
            }
        }

        Global.ACUsages _ReportType = Global.ACUsages.DUReport;
        [ACPropertyInfo(9999, "", "en{'Report type'}de{'Berichtstyp'}")]
        public Global.ACUsages ReportType
        {
            get
            {
                return _ReportType;
            }
            set
            {
                _ReportType = value;
                OnPropertyChanged("ReportType");
            }
        }

        string _ReportACIdentifier = "";
        [ACPropertyInfo(9999)]
        public string ReportACIdentifier
        {
            get
            {
                return _ReportACIdentifier;
            }
            set
            {
                _ReportACIdentifier = value;
                OnPropertyChanged("ReportACIdentifier");
            }
        }

        string _ReportACCaption = "";
        [ACPropertyInfo(9999)]
        public string ReportACCaption
        {
            get
            {
                return _ReportACCaption;
            }
            set
            {
                _ReportACCaption = value;
                OnPropertyChanged("ReportACCaption");
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

        #region Intern
        public ACBSO ParentBSO
        {
            get
            {
                return ParentACComponent as ACBSO;
            }
        }

        public override bool IsPoolable
        {
            get
            {
                return false;
            }
        }
        #endregion

        #endregion

        #region BSO->ACMethod

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "ReportPrintDlg":
                    ReportPrintDlg();
                    return true;
                case Const.IsEnabledPrefix + "ReportPrintDlg":
                    result = IsEnabledReportPrintDlg();
                    return true;
                case "ReportPreviewDlg":
                    ReportPreviewDlg();
                    return true;
                case Const.IsEnabledPrefix + "ReportPreviewDlg":
                    result = IsEnabledReportPreviewDlg();
                    return true;
                case "ReportDesignDlg":
                    ReportDesignDlg();
                    return true;
                case Const.IsEnabledPrefix + "ReportDesignDlg":
                    result = IsEnabledReportDesignDlg();
                    return true;
                case "ReportNewReportDlg":
                    ReportNewReportDlg();
                    return true;
                case "ReportCancel":
                    ReportCancel();
                    return true;
                case Const.CmdNameReportNew:
                    ReportNew();
                    return true;
                case Const.IsEnabledPrefix + Const.CmdNameReportNew:
                    result = IsEnabledReportNew();
                    return true;
                case "LoadDefaultReport":
                    LoadDefaultReport();
                    return true;
                case Const.CmdNameReportDelete:
                    ReportDelete();
                    return true;
                case Const.IsEnabledPrefix + Const.CmdNameReportDelete:
                    result = IsEnabledReportDelete();
                    return true;
                case Const.CmdNameReportSave:
                    ReportSave();
                    return true;
                case Const.IsEnabledPrefix + Const.CmdNameReportSave:
                    result = IsEnabledReportSave();
                    return true;
                case Const.CmdNameReportPrint:
                    ReportPrint();
                    return true;
                case Const.IsEnabledPrefix + Const.CmdNameReportPrint:
                    result = IsEnabledReportPrint();
                    return true;
                case Const.CmdNameReportPreview:
                    ReportPreview();
                    return true;
                case Const.IsEnabledPrefix + Const.CmdNameReportPreview:
                    result = IsEnabledReportPreview();
                    return true;
                case Const.CmdNameReportDesign:
                    ReportDesign();
                    return true;
                case Const.IsEnabledPrefix + Const.CmdNameReportDesign:
                    result = IsEnabledReportDesign();
                    return true;
                case "ReportPrintSilent":
                    ReportPrintSilent(acParameter[0] as ACClassDesign, (Global.CurrentOrList)acParameter[1], acParameter[2] as string);
                    return true;
                case "ReportPreviewSilent":
                    ReportPreviewSilent(acParameter[0] as ACClassDesign, (Global.CurrentOrList)acParameter[1], acParameter[2] as string);
                    return true;
                case "ReportModifyQuery":
                    ReportModifyQuery();
                    return true;
                case Const.IsEnabledPrefix + "ReportModifyQuery":
                    result = IsEnabledReportModifyQuery();
                    return true;
                case Const.SMReadOnly:
                    SMReadOnly();
                    return true;
                case Const.SMNew:
                    SMNew();
                    return true;
                case Const.SMEdit:
                    SMEdit();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion


        #region Dialoge öffnen und schliessen (von Ribbonbar aus)

        [ACMethodCommand("Report", "en{'Print'}de{'Drucken'}", (short)MISort.QueryPrintDlg)]
        public void ReportPrintDlg()
        {
            if (!IsEnabledReportPrintDlg())
                return;
            LoadDefaultReport();
            ShowDialog(this, "ReportPrintDlg");
        }

        [ACMethodInfo("", "en{'Is enabled print'}de{'Is enabled print'}", 9999)]
        public bool IsEnabledReportPrintDlg()
        {
            if (ACClassDesignList == null)
                return false;
            return ACClassDesignList.Any();
        }


        [ACMethodCommand("Report", "en{'Preview'}de{'Vorschau'}", (short)MISort.QueryPreviewDlg)]
        public void ReportPreviewDlg()
        {
            if (!IsEnabledReportPreviewDlg())
                return;
            LoadDefaultReport();
            ShowDialog(this, "ReportPreviewDlg");
        }

        [ACMethodInfo("", "en{'Is enabled preview'}de{'Is enabled preview'}", 9999)]
        public bool IsEnabledReportPreviewDlg()
        {
            if (ACClassDesignList == null)
                return false;
            return ACClassDesignList.Any();
        }


        [ACMethodCommand("Report", "en{'Design'}de{'Entwurf'}", (short)MISort.QueryDesignDlg)]
        public void ReportDesignDlg()
        {
            LoadDefaultReport();
            if (CurrentACClassDesign == null)
            {
                ReportNewReportDlg();
                if (CurrentACClassDesign == null)
                {
                    CloseTopDialog();
                    return;
                }
            }
            ShowDialog(this, "ReportDesignDlg");
        }

        [ACMethodInfo("", "en{'Is enabled design'}de{'Is enabled design'}", 9999)]
        public bool IsEnabledReportDesignDlg()
        {
            return true;
            //return CurrentACQueryDefinition != null;
        }

        [ACMethodCommand("Report", "en{'New report'}de{'Neuer Bericht'}", (short)MISort.QueryDesignDlg)]
        public void ReportNewReportDlg()
        {
            ReportType = Global.ACUsages.DUReport;
            IsDesignOfACQuery = false;
            SelectedQueryACClass = null;
            ShowDialog(this, "ReportNewDlg");
        }


        [ACMethodCommand("Report", "en{'Close'}de{'Schließen'}", (short)MISort.Cancel)]
        public void ReportCancel()
        {
            CloseTopDialog();
        }

        #endregion

        #region Neu anlegen, Laden, Speichern und Löschen

        #region Neu anlegen
        [ACMethodCommand("Report", "en{'New'}de{'Neu'}", (short)MISort.New)]
        public void ReportNew()
        {
            ACClassDesign newDesign = null;
            ACClassDesign parentReport = _ACClassDesignList.FirstOrDefault(c => c.ACIdentifier == ReportACIdentifier && c.ACUsage == ReportType);
            if (parentReport != null && parentReport.ACClass.ACClassID == ((ACClass)this.ParentACComponent.ACType).ACClassID)
            {
                Messages.Info(this, "Info50019");
                return;
            }

            // Falls Design zur ACQueryKlasse gehört
            if (IsDesignOfACQuery && CurrentQueryACClass != null)
            {
                string secondaryKey = Root.NoManager.GetNewNo(Database, typeof(ACClassDesign), ACClassDesign.NoColumnName, ACClassDesign.FormatNewNo, this);
                newDesign = ACClassDesign.NewACObject(Database.ContextIPlus, CurrentQueryACClass, secondaryKey);
                if (!String.IsNullOrEmpty(ReportACIdentifier))
                {
                    newDesign.ACIdentifier = ReportACIdentifier;
                    ReportACIdentifier = "";
                }
                else
                    newDesign.ACIdentifier = CurrentQueryACClass.ACIdentifier;
                newDesign.ACCaptionTranslation = CurrentQueryACClass.ACCaptionTranslation;
                newDesign.ACUsage = this.ReportType;
            }
            // Sonst von Businessobject
            else
            {
                ACClass parentACClass = null;
                if (ParentACComponent.ACType is ACClass)
                    parentACClass = Database.ContextIPlus.ACClass.FirstOrDefault(c => c.ACClassID == ((ACClass)ParentACComponent.ACType).ACClassID);

                string secondaryKey = Root.NoManager.GetNewNo(Database, typeof(ACClassDesign), ACClassDesign.NoColumnName, ACClassDesign.FormatNewNo, this);
                newDesign = ACClassDesign.NewACObject(Database.ContextIPlus, parentACClass, secondaryKey);
                if (!String.IsNullOrEmpty(ReportACIdentifier))
                {
                    newDesign.ACIdentifier = ReportACIdentifier;
                    ReportACIdentifier = "";
                }
                else
                    newDesign.ACIdentifier = ParentACComponent.ACIdentifier;
                if (!string.IsNullOrEmpty(ReportACCaption))
                {
                    newDesign.ACCaption = ReportACCaption;
                    ReportACCaption = "";
                }
                else
                    newDesign.ACCaptionTranslation = ParentACComponent.ACType.ACCaptionTranslation;
                newDesign.ACUsage = this.ReportType;
            }


            //UpdateName(newDesign);
            newDesign.ACKind = Global.ACKinds.DSDesignReport;
            if (parentReport != null && parentReport.ACUsage == newDesign.ACUsage)
                newDesign.XMLDesign = parentReport.XMLDesign;
            else if (newDesign.ACUsage == Global.ACUsages.DUReport)
                newDesign.XMLDesign = "<?xml version=\"1.0\" encoding=\"utf-8\"?><FlowDocument PageWidth=\"816\" PageHeight=\"1056\" PagePadding=\"96,96,96,96\" AllowDrop=\"True\" NumberSubstitution.CultureSource=\"User\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><Paragraph LineHeight=\"1.15\"><Run xml:lang=\"de-de\" xml:space=\"preserve\" /></Paragraph></FlowDocument>";



            Database.ContextIPlus.ACClassDesign.AddObject(newDesign);
            _ACClassDesignList.Add(newDesign);
            _ACClassDesignList = _ACClassDesignList.ToList();
            OnPropertyChanged("ACClassDesignList");
            //SelectedACClassDesign = newDesign; // Don't call because of Exception "invalidoperationexception" list has changed (Datagrid)
            ACState = Const.SMNew;
            CloseTopDialog();
            ACSaveChanges();
        }

        public bool IsEnabledReportNew()
        {
            return true;
        }

        void UpdateName(ACClassDesign acClassDesign)
        {
            int maxIndex = 1;
            if (_ACClassDesignList.Count() != 0)
            {
                var query = _ACClassDesignList.Where(c => c.ACIdentifier.StartsWith(acClassDesign.ACIdentifier)).OrderBy(c => c.ACIdentifier).Select(c => c.ACIdentifier);
                if (query.Any())
                {
                    var acIdentifier = query.Last();
                    var part = acIdentifier.Substring(acIdentifier.Length - 1);
                    if (int.TryParse(part, out maxIndex))
                    {
                        maxIndex++;
                    }
                    else
                    {
                        maxIndex = 1;
                    }
                }
            }
            acClassDesign.ACIdentifier = acClassDesign.ACIdentifier + maxIndex.ToString();
            string caption = acClassDesign.ACCaption + maxIndex.ToString();
            acClassDesign.ACCaption = "en:" + caption;
            acClassDesign.ACCaption = "de:" + caption;
        }
        #endregion

        #region Laden

        [ACMethodCommand("Report", "en{'Load Defaultreport'}de{'Standardreport laden'}", (short)MISort.Load)]
        public void LoadDefaultReport()
        {
            try
            {
                if(SelectedACClassDesign == null)
                {
                    ACClassDesign defaultDesign = _ACClassDesignList.Where(c => c.IsDefault).FirstOrDefault();
                    if (defaultDesign == null)
                        defaultDesign = _ACClassDesignList.FirstOrDefault();
                    SelectedACClassDesign = defaultDesign;
                    if (defaultDesign != null)
                        ACState = Const.SMEdit;
                    else
                        ACState = Const.SMReadOnly;
                    if (ParentBSO != null)
                        ParentBSO.CurrentQueryACClassDesign = defaultDesign;
                    if (_ACClassDesignList != null)
                        SelectedACClassDesignPreview = ACClassDesignListPreview.FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                SelectedACClassDesign = null;
                CurrentACClassDesign = null;
                ACState = Const.SMReadOnly;

                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                Messages.LogException("VBBSOReportDialog", "LoadDefaultReport", msg);
            }
        }


        //[ACMethodCommand("Report", "en{'Load'}de{'Laden'}", (short)MISort.Load)]
        //public void ReportLoad()
        //{
        //    if (SelectedACClassDesign != null)
        //    {
        //        CurrentACClassDesign = SelectedACClassDesign;
        //    }
        //}

        //public bool IsEnabledReportLoad()
        //{
        //    return SelectedACClassDesign != null;
        //}

        #endregion

        #region Löschen

        [ACMethodCommand("Report", "en{'Delete'}de{'Löschen'}", (short)MISort.Delete)]
        public void ReportDelete()
        {
            if (!IsEnabledReportDelete())
                return;
            if (Messages.Question(this, "Question50026", Global.MsgResult.None, false, CurrentACClassDesign.ACIdentifier) == Global.MsgResult.Yes)
            {
                var ctx = CurrentACClassDesign.GetObjectContext();
                CurrentACClassDesign.DeleteACObject(ctx, true);
                ctx.ACSaveChanges();
                ctx = null;
                _ACClassDesignList.Remove(CurrentACClassDesign);
                _ACClassDesignList = _ACClassDesignList.ToList();
                OnPropertyChanged("ACClassDesignList");
                LoadDefaultReport();
            }
        }

        public bool IsEnabledReportDelete()
        {
            return CurrentACClassDesign != null;
        }

        #endregion

        #region Speichern

        [ACMethodCommand("Report", "en{'Save'}de{'Speichern'}", (short)MISort.Save)]
        public void ReportSave()
        {
            if (!IsEnabledReportSave())
                return;
            if (OnSave())
                Database.ContextIPlus.ACSaveChanges();
            if (ParentBSO != null)
                ParentBSO.CurrentQueryACClassDesign = CurrentACClassDesign;
        }

        public bool IsEnabledReportSave()
        {
            return Database.ContextIPlus.IsChanged;
        }

        public void ReportUndo()
        {
            if (!IsEnabledReportSave())
                return;
            if (OnUndoSave())
                Database.ContextIPlus.ACUndoChanges();
        }
        #endregion

        #endregion

        #region Drucken

        [ACMethodInfo("Report", "en{'Print direct'}de{'Direkter Druck'}", (short)MISort.QueryPrintSilent)]
        public void ReportPrintSilent(ACClassDesign ACClassDesign, Global.CurrentOrList currentOrList, string printerName)
        {
            LoadDefaultReport();
            WithDialog = false;
            CurrentACClassDesign = ACClassDesign;
            CurrentCurrentOrList = currentOrList;
            PrinterName = printerName; // Beispiel: "HP Universal Printing PCL 6";
            ReportPrint();
        }


        [ACMethodCommand("Report", "en{'Print'}de{'Drucken'}", (short)MISort.QueryPrint)]
        public void ReportPrint()
        {
            if (!IsEnabledReportPrint())
                return;
            CloseTopDialog();

            ACUrlCommand("..!Save");

            try
            {
                VBBSOReport acReportQuery = StartVBBSOReport();
                if (acReportQuery == null)
                    return;

                bool cloneInstantiated = false;
                ReportData reportData = ReportData.BuildReportData(out cloneInstantiated, CurrentCurrentOrList, ParentACComponent, CurrentACQueryDefinition, CurrentACClassDesign, PreventClone);
                if (reportData == null)
                    return;

                acReportQuery.Print(CurrentACClassDesign, WithDialog, PrinterName, reportData, CopyCount);

                if (cloneInstantiated)
                    reportData.StopACComponents();
                
                StopComponent(acReportQuery);
            }
            catch (Exception e)
            {
                Messages.LogException(this.GetACUrl(), "VBBSOReportDialog.ReportPrint()", e.Message);
            }
        }

        public bool IsEnabledReportPrint()
        {
            return CurrentACClassDesign != null;
        }

        #endregion

        #region Druckvorschau

        [ACMethodInfo("Report", "en{'Preview Direct'}de{'Direkte Vorschau'}", (short)MISort.QueryPreviewSilent)]
        public void ReportPreviewSilent(ACClassDesign ACClassDesign, Global.CurrentOrList currentOrList, string printerName)
        {
            LoadDefaultReport();
            WithDialog = false;
            CurrentACClassDesign = ACClassDesign;
            CurrentCurrentOrList = currentOrList;
            PrinterName = printerName; // Beispiel: "HP Universal Printing PCL 6";
            ReportPreview();
        }


        [ACMethodCommand("Report", "en{'Preview'}de{'Vorschau'}", (short)MISort.QueryPreview)]
        public void ReportPreview()
        {
            if (CurrentACClassDesign == null)
                return;
            CloseTopDialog();

            //ACUrlCommand("..!Save");

            try
            {
                //ReportData reportData = new ReportData();
                //if (IsCurrentDesignFromParentBSO())
                //    reportData.ReportDocumentValues.Add(ParentBSONav.GetType().Name, ParentBSONav);
                //else
                //{
                //    UpdateACQueryFilter();
                //    reportData.ReportDocumentValues.Add(CurrentACQueryDefinition.ChildACUrl, CurrentACQueryDefinition);
                //}
                //ReportDocument reportDoc = new ReportDocument(CurrentACClassDesign.XMLDesign);
                //if (reportDoc != null)
                //{
                //    XpsDocument xps = reportDoc.CreateXpsDocument(reportData);
                //    if (xps != null)
                //        CurrentXPSDocument = xps.GetFixedDocumentSequence();
                //    else
                //        CurrentXPSDocument = null;
                //    //OnPropertyChanged("CurrentXPSDocument");
                //}

                VBBSOReport acReportQuery = StartVBBSOReport();
                if (acReportQuery == null)
                    return;

                bool cloneInstantiated = false;
                ReportData reportData = ReportData.BuildReportData(out cloneInstantiated, CurrentCurrentOrList, ParentACComponent, CurrentACQueryDefinition, CurrentACClassDesign);
                if (reportData == null)
                    return;

                acReportQuery.Preview(CurrentACClassDesign, WithDialog, PrinterName, reportData);

                if (cloneInstantiated)
                    reportData.StopACComponents();
                StopComponent(acReportQuery);

            }
            catch (Exception e)
            {
                Messages.LogException(this.GetACUrl(), "VBBSOReportDialog.ReportPreview()", e.Message);
            }
        }

        public bool IsEnabledReportPreview()
        {
            return SelectedACClassDesign != null;
        }
        #endregion

        #region Entwerfen

        [ACMethodCommand("Report", "en{'Design'}de{'Entwurf'}", (short)MISort.QueryDesign)]
        public void ReportDesign()
        {
            if (CurrentACClassDesign == null)
                return;

            ReportSave();

            ACUrlCommand("..!Save");

            VBBSOReport acReportQuery = StartVBBSOReport();
            if (acReportQuery == null)
                return;

            bool cloneInstantiated = false;
            ReportData reportData = ReportData.BuildReportData(out cloneInstantiated, CurrentCurrentOrList, ParentACComponent, CurrentACQueryDefinition, CurrentACClassDesign);
            ACClassDesign currentACClassDesign = CurrentACClassDesign;
            CloseTopDialog();
            if (reportData == null || currentACClassDesign == null)
                return;

            acReportQuery.Design(currentACClassDesign, WithDialog, PrinterName, reportData);

            if (cloneInstantiated)
                reportData.StopACComponents();

            StopComponent(acReportQuery);

            ReportSave();
        }

        public bool IsEnabledReportDesign()
        {
            return SelectedACClassDesign != null;
        }

        #endregion

        #region ACQuery bearbeiten

        Global.ControlModes _CMModifyQuery = Global.ControlModes.Hidden;
        [ACMethodCommand("Report", "en{'Modify Query'}de{'Query bearbeiten'}", 9999)]
        public void ReportModifyQuery()
        {
            if (!IsEnabledReportModifyQuery())
                return;
            CloseTopDialog();

            ACMethod acMethod = gip.core.datamodel.Database.Root.ACType.ACType.ACUrlACTypeSignature(Const.BusinessobjectsACUrl + ACUrlHelper.Delimiter_Start + Const.BSOiPlusStudio);
            acMethod.ParameterValueList["AutoLoad"] = CurrentACQueryDefinition.ACType.GetACUrl();

            this.Root.RootPageWPF.StartBusinessobject(Const.BusinessobjectsACUrl + ACUrlHelper.Delimiter_Start + Const.BSOiPlusStudio, acMethod.ParameterValueList);
        }

        public bool IsEnabledReportModifyQuery()
        {
            if (!gip.core.datamodel.Database.Root.Environment.License.MayUserDevelop)
                return false;
            return CurrentACQueryDefinition != null && _CMModifyQuery == Global.ControlModes.Enabled;
        }

        #endregion

        #region ACState
        [ACMethodState("en{'Readonly'}de{'Schreibgeschützt'}", 1000)]
        public virtual void SMReadOnly()
        {
            if (!PreExecute(Const.SMReadOnly))
                return;
            PostExecute(Const.SMReadOnly);
        }

        [ACMethodState("en{'New'}de{'Neu'}", 1010)]
        public virtual void SMNew()
        {
            if (!PreExecute(Const.SMNew))
                return;
            PostExecute(Const.SMNew);
        }

        [ACMethodState("en{'Edit'}de{'Bearbeiten'}", 1020)]
        public virtual void SMEdit()
        {
            if (!PreExecute(Const.SMEdit))
                return;
            PostExecute(Const.SMEdit);
        }
        #endregion

        #region Hilfsmethoden

        protected virtual VBBSOReport StartVBBSOReport()
        {
            string acClassName = "VBBSOReport";
            ACClass acClass = GetACClassFromACClassName(ref acClassName);
            if (acClass == null)
                return null;
            return StartComponent(acClass, acClass, null, Global.ACStartTypes.Automatic) as VBBSOReport;
        }
        #endregion



        #endregion
    }
}
