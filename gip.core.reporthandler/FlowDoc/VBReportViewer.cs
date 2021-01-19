using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using gip.core.datamodel;
using System.Windows.Xps.Packaging;
using System.IO;
using System.Transactions;
using gip.core.layoutengine;
using gip.core.autocomponent;
using System.Windows.Controls.Primitives;
using System.Windows.Documents.Serialization;
using System.Reflection;
using System.Printing;
using System.Windows.Xps;

namespace gip.core.reporthandler.Flowdoc
{
    /// <summary>
    /// Steuerelemnt zur Anzeige von XPS-Dokumenten
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBReportViewer'}de{'VBReportViewer'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBReportViewer : VBXpsViewer
    {
        protected override void InitVBControl()
        {
            if (!_Initialized && ContextACObject != null)
            {
                if (!String.IsNullOrEmpty(VBReportData))
                {
                    IACType dcACTypeInfo2 = null;
                    object dcSource2 = null;
                    string dcPath2 = "";
                    Global.ControlModes dcRightControlMode2 = Global.ControlModes.Hidden;
                    if (!ContextACObject.ACUrlBinding(VBReportData, ref dcACTypeInfo2, ref dcSource2, ref dcPath2, ref dcRightControlMode2))
                    {
                        this.Root().Messages.Error(BSOACComponent, "Error00007", false, "VBReportEditor", VBReportData);
                        return;
                    }

                    Binding binding2 = new Binding();
                    binding2.Source = dcSource2;
                    binding2.Path = new PropertyPath(dcPath2);
                    binding2.NotifyOnSourceUpdated = true;
                    binding2.NotifyOnTargetUpdated = true;
                    binding2.Mode = BindingMode.OneWay;
                    this.SetBinding(VBReportViewer.DesignerReportDataProperty, binding2);
                }
            }
            base.InitVBControl();
        }

        /// <summary>
        /// DeInitVBControl is used to remove all References which a WPF-Control refers to.
        /// It's needed that the Garbage-Collerctor can delete the object when it's removed from the Logical-Tree.
        /// Controls that implement this interface should bind itself to the InitState of the BSOACComponent.
        /// When the BSOACComponent stops and the property changes to Destructed- or DisposedToPool-State than this method should be called.
        /// </summary>
        /// <param name="bso">The bound BSOACComponent</param>
        public override void DeInitVBControl(IACComponent bso)
        {
            if (!String.IsNullOrEmpty(VBReportData))
                BindingOperations.ClearBinding(this, VBReportEditor.DesignerReportDataProperty);

            _ReportDoc = null;
            base.DeInitVBControl(bso);
        }

        //string _LastLoadedXAMLInViewer;
        ReportDocument _ReportDoc;
        public override void LoadFile()
        {
            _ReportDoc = null;
            if (!string.IsNullOrEmpty(ContentFile) && DesignerReportData != null && ContentFile != null)
            {
                //string newXMLText = Layoutgenerator.CheckOrUpdateNamespaceInLayout(this.ContentFile);
                //if (_LastLoadedXAMLInViewer == newXMLText)
                //    return;

                try
                {
                    _ReportDoc = new ReportDocument(this.ContentFile);
                    if (_ReportDoc != null)
                    {
                        XpsDocument xps = _ReportDoc.CreateXpsDocument(DesignerReportData);
                        if (xps != null)
                            this.Document = xps.GetFixedDocumentSequence();
                        else
                            this.Document = null;
                    }
                }
                catch (Exception e)
                {
                    this.Document = null;

                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == datamodel.ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("VBReportViewer", "LoadFile", msg);
                }
            }
            else if (!string.IsNullOrEmpty(ContentFile) && ContentFile.ToUpper().EndsWith(".XPS") && File.Exists(ContentFile))
            {
                XpsDocument xpsDoc = new XpsDocument(ContentFile, FileAccess.Read);
                this.Document = xpsDoc.GetFixedDocumentSequence();
                xpsDoc.Close();
            }
        }


        public static readonly DependencyProperty VBReportDataProperty
            = DependencyProperty.Register("VBReportData", typeof(string), typeof(VBReportViewer));

        public string VBReportData
        {
            get { return (string)GetValue(VBReportDataProperty); }
            set { SetValue(VBReportDataProperty, value); }
        }

        public static readonly DependencyProperty DesignerReportDataProperty
            = DependencyProperty.Register("DesignerReportData", typeof(ReportData), typeof(VBReportViewer), new PropertyMetadata(new PropertyChangedCallback(ReportDataChanged)));

        public ReportData DesignerReportData
        {
            get
            {
                return (ReportData)GetValue(DesignerReportDataProperty);
            }
            set
            {
                SetValue(DesignerReportDataProperty, value);
            }
        }

        private static void ReportDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is VBReportViewer)
            {
                VBReportViewer vbContentControl = d as VBReportViewer;
                vbContentControl.LoadFile();
            }
        }

        System.Windows.Xps.XpsDocumentWriter _DocumentWriter = null;
        protected override void OnPrintCommand()
        {
            // get a print dialog, defaulted to default printer and default printer's preferences.
            PrintDialog printDialog = new PrintDialog();
            printDialog.PrintQueue = LocalPrintServer.GetDefaultPrintQueue();
            printDialog.PrintTicket = printDialog.PrintQueue.DefaultPrintTicket;
            if (_ReportDoc != null)
            {
                if (_ReportDoc.AutoSelectPageOrientation.HasValue)
                    printDialog.PrintTicket.PageOrientation = _ReportDoc.AutoSelectPageOrientation;
                else
                {
                    if (_ReportDoc.PageWidth > _ReportDoc.PageHeight)
                        printDialog.PrintTicket.PageOrientation = PageOrientation.Landscape;
                    else
                        printDialog.PrintTicket.PageOrientation = PageOrientation.Portrait;
                }
            }

            // get a reference to the FixedDocumentSequence for the viewer.
            if (printDialog.ShowDialog() == true)
            {
                if (this.Document is FixedDocumentSequence)
                {
                    FixedDocumentSequence docSeq = this.Document as FixedDocumentSequence;
                    // set the print ticket for the document sequence and write it to the printer.
                    docSeq.PrintTicket = printDialog.PrintTicket;

                    _DocumentWriter = PrintQueue.CreateXpsDocumentWriter(printDialog.PrintQueue);
                    _DocumentWriter.WritingCompleted += new WritingCompletedEventHandler(HandlePrintCompleted);
                    _DocumentWriter.WritingCancelled += new WritingCancelledEventHandler(HandlePrintCancelled);
                    _DocumentWriter.WriteAsync(docSeq, printDialog.PrintTicket);
                }
                else if (this.Document is FixedDocument)
                {
                    FixedDocument docSeq = this.Document as FixedDocument;
                    // set the print ticket for the document sequence and write it to the printer.
                    docSeq.PrintTicket = printDialog.PrintTicket;

                    _DocumentWriter = PrintQueue.CreateXpsDocumentWriter(printDialog.PrintQueue);
                    _DocumentWriter.WritingCompleted += new WritingCompletedEventHandler(HandlePrintCompleted);
                    _DocumentWriter.WritingCancelled += new WritingCancelledEventHandler(HandlePrintCancelled);
                    _DocumentWriter.WriteAsync(docSeq, printDialog.PrintTicket);
                }
            }
        }

        #region From Reference-Source
        //private System.Windows.Xps.XpsDocumentWriter PrivateDocumentWriter
        //{
        //    get
        //    {
        //        Type typeDocViewerBase = typeof(DocumentViewerBase);
        //        FieldInfo docFieldInfo = typeDocViewerBase.GetField("_documentWriter", BindingFlags.NonPublic | BindingFlags.Instance);
        //        System.Windows.Xps.XpsDocumentWriter _documentWriter = null;
        //        if (docFieldInfo != null)
        //        {
        //            _documentWriter = (System.Windows.Xps.XpsDocumentWriter)docFieldInfo.GetValue(this);
        //        }
        //        return _documentWriter;
        //    }
        //}

        //protected override void OnPrintCommand()
        //{
        //    ///https://referencesource.microsoft.com/#PresentationFramework/src/Framework/System/Windows/Controls/Primitives/DocumentViewerBase.cs
        //    ///
        //    //base.OnPrintCommand();
        //    System.Windows.Xps.XpsDocumentWriter docWriter;
        //    System.Printing.PrintDocumentImageableArea ia = null;
        //    System.Windows.Xps.XpsDocumentWriter _documentWriter = PrivateDocumentWriter;

        //    // Only one printing job is allowed.
        //    if (_documentWriter != null)
        //    {
        //        return;
        //    }

        //    PrintDialog printDialog = new PrintDialog();
        //    printDialog.PrintTicket.PageOrientation = PageOrientation.Landscape;
        //    printDialog.ShowDialog();

        //    if (this.Document != null)
        //    {
        //        SetDefaultPageOrientation();
        //        // Show print dialog.
        //        docWriter = System.Printing.PrintQueue.CreateXpsDocumentWriter(ref ia);
        //        if (docWriter != null && ia != null)
        //        {
        //            // Register for WritingCompleted event.
        //            _documentWriter = docWriter;
        //            _documentWriter.WritingCompleted += new WritingCompletedEventHandler(HandlePrintCompleted);
        //            _documentWriter.WritingCancelled += new WritingCancelledEventHandler(HandlePrintCancelled);

        //            // Since _documentWriter value is used to determine CanExecute state, we must invalidate that state.
        //            CommandManager.InvalidateRequerySuggested();

        //            PrintQueue pq = null;
        //            PrintTicket userPt = null;
        //            Type typeXpsDocWriter = typeof(System.Windows.Xps.XpsDocumentWriter);
        //            FieldInfo pqPropInfo = typeXpsDocWriter.GetField("destinationPrintQueue", BindingFlags.NonPublic | BindingFlags.Instance);
        //            if (pqPropInfo != null)
        //            {
        //                pq = (PrintQueue)pqPropInfo.GetValue(docWriter);
        //                if (pq != null)
        //                    userPt = pq.UserPrintTicket;
        //            }

        //            PrintTicket pt = null;
        //            if (_ReportDoc != null)
        //            {
        //                pt = userPt != null ? userPt.Clone() : new PrintTicket();
        //                if (pt.CopyCount <= 0)
        //                    pt.CopyCount = 1;
        //                if (_ReportDoc.AutoSelectPageOrientation.HasValue)
        //                    pt.PageOrientation = _ReportDoc.AutoSelectPageOrientation;
        //                else
        //                {
        //                    if (_ReportDoc.PageWidth > _ReportDoc.PageHeight)
        //                        pt.PageOrientation = PageOrientation.Landscape;
        //                    else
        //                        pt.PageOrientation = PageOrientation.Portrait;
        //                }
        //                if (userPt != null)
        //                    userPt.PageOrientation = pt.PageOrientation;

        //                //string nameSpaceURI = string.Empty;
        //                //pt = XpsPrinterUtils.ModifyPrintTicket(pt, "psk:PageOrientation", "psk:" + pt.PageOrientation.ToString(), nameSpaceURI);

        //                if (_ReportDoc.AutoPageMediaSize != null)
        //                    pt.PageMediaSize = _ReportDoc.AutoPageMediaSize;
        //                if (pq != null)
        //                    pq.UserPrintTicket = pt;

        //                //if (_ReportDoc.AutoSelectTray.HasValue)
        //                //{
        //                //    string nameSpaceURI = string.Empty;
        //                //    string selectedtray = XpsPrinterUtils.GetInputBinName(pQ.Name, _ReportDoc.AutoSelectTray.Value, out nameSpaceURI);
        //                //    pt = XpsPrinterUtils.ModifyPrintTicket(pt, "psk:JobInputBin", selectedtray, nameSpaceURI);
        //                //}
        //            }


        //            // Write to the PrintQueue
        //            if (this.Document is FixedDocumentSequence)
        //            {
        //                docWriter.WriteAsync(this.Document as FixedDocumentSequence, pt);
        //            }
        //            else if (this.Document is FixedDocument)
        //            {
        //                docWriter.WriteAsync(this.Document as FixedDocument, pt);
        //            }
        //            else
        //            {
        //                docWriter.WriteAsync(this.Document.DocumentPaginator, pt);
        //            }
        //        }
        //    }
        //}

        //private PrintTicket SetDefaultPageOrientation()
        //{
        //    if (_ReportDoc == null)
        //        return null;
        //    LocalPrintServer localPrintServer = new LocalPrintServer();
        //    PrintQueue printQueue = LocalPrintServer.GetDefaultPrintQueue();
        //    if (printQueue == null)
        //        return null;

        //    // Get default PrintTicket from printer
        //    PrintTicket printTicket = printQueue.DefaultPrintTicket;
        //    if (_ReportDoc.AutoSelectPageOrientation.HasValue)
        //        printTicket.PageOrientation = _ReportDoc.AutoSelectPageOrientation;
        //    else
        //    {
        //        if (_ReportDoc.PageWidth > _ReportDoc.PageHeight)
        //            printTicket.PageOrientation = PageOrientation.Landscape;
        //        else
        //            printTicket.PageOrientation = PageOrientation.Portrait;
        //    }

        //    return printTicket;
        //}

        private void HandlePrintCompleted(object sender, WritingCompletedEventArgs e)
        {
            CleanUpPrintOperation();
        }

        private void HandlePrintCancelled(object sender, WritingCancelledEventArgs e)
        {
            CleanUpPrintOperation();
        }

        private void CleanUpPrintOperation()
        {
            //System.Windows.Xps.XpsDocumentWriter _documentWriter = PrivateDocumentWriter;
            //if (_documentWriter != null)
            //{
            //    _documentWriter.WritingCompleted -= new WritingCompletedEventHandler(HandlePrintCompleted);
            //    _documentWriter.WritingCancelled -= new WritingCancelledEventHandler(HandlePrintCancelled);
            //    _documentWriter = null;

            //    // Since _documentWriter value is used to determine CanExecute state, we must invalidate that state.
            //    CommandManager.InvalidateRequerySuggested();
            //}

            if (_DocumentWriter != null)
            {
                _DocumentWriter.WritingCompleted -= new WritingCompletedEventHandler(HandlePrintCompleted);
                _DocumentWriter.WritingCancelled -= new WritingCancelledEventHandler(HandlePrintCancelled);
                _DocumentWriter = null;

                // Since _documentWriter value is used to determine CanExecute state, we must invalidate that state.
                CommandManager.InvalidateRequerySuggested();
            }
        }
        #endregion


    }
}
