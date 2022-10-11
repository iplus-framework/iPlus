using gip.core.datamodel;
using gip.core.autocomponent;
using System;
using System.Net.Sockets;
using System.Text;
using gip.core.reporthandler.Flowdoc;
using System.Windows.Documents;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Threading;
using System.IO;
using gip.core.layoutengine;
using System.Xaml;

namespace gip.core.reporthandler
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACPrintServerBase'}de{'ACPrintServerBase'}", Global.ACKinds.TACApplicationManager, Global.ACStorableTypes.Required, false, "", false)]
    public abstract class ACPrintServerBase : PAClassAlarmingBase
    {

        #region c´tors
        public const string MN_Print = nameof(Print);
        public const string MN_PrintByACUrl = nameof(PrintByACUrl);

        public ACPrintServerBase(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _IPAddress = new ACPropertyConfigValue<string>(this, "IPAddress", "");
            _Port = new ACPropertyConfigValue<int>(this, "Port", 0);
            _SendTimeout = new ACPropertyConfigValue<int>(this, "SendTimeout", 0);
            _ReceiveTimeout = new ACPropertyConfigValue<int>(this, "ReceiveTimeout", 0);
            _PrintTries = new ACPropertyConfigValue<int>(this, "PrintTries", 1);
            _CodePage = new ACPropertyConfigValue<int>(this, "CodePage", 0);
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;

            using (ACMonitor.Lock(_20015_LockValue))
            {
                _DelegateQueue = new ACDispatchedDelegateQueue(GetACUrl());
            }
            _DelegateQueue.StartWorkerThreadSTA();
            //_DelegateQueue.StartWorkerThread();
            
            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            _DelegateQueue.StopWorkerThread();
            using (ACMonitor.Lock(_20015_LockValue))
            {
                _DelegateQueue = null;
            }

            return base.ACDeInit(deleteACClassTask);
        }

        public override bool ACPostInit()
        {
            bool baseReturn = base.ACPostInit();
           
            _ = _IPAddress;
            _ = _Port;
            _ = _SendTimeout;
            _ = _ReceiveTimeout;
            _ = _PrintTries;
            _ = _CodePage;

            return baseReturn;
        }

        protected static IACEntityObjectContext _CommonManagerContext;
        /// <summary>
        /// Returns a seperate and shared Database-Context "StaticACComponentManager".
        /// Because Businessobjects also inherit from this class all BSO's get this shared database context.
        /// If some custom BSO's needs its own context, then they have to override this property.
        /// Application-Managers that also inherit this class should override this property an use their own context.
        /// </summary>
        /// <value>The context as IACEntityObjectContext.</value>
        public override IACEntityObjectContext Database
        {
            get
            {
                if (_CommonManagerContext == null)
                    _CommonManagerContext = ACObjectContextManager.GetOrCreateContext<Database>("StaticACComponentManager");
                return _CommonManagerContext;
            }
        }
        #endregion

        #region Properties

        private ACPropertyConfigValue<string> _IPAddress;
        [ACPropertyConfig("en{'IP Address'}de{'IP Addresse'}")]
        public string IPAddress
        {
            get => _IPAddress.ValueT;
            set => _IPAddress.ValueT = value;
        }

        private ACPropertyConfigValue<int> _Port;
        [ACPropertyConfig("en{'Port'}de{'Port'}")]
        public int Port
        {
            get => _Port.ValueT;
            set => _Port.ValueT = value;
        }


        private ACPropertyConfigValue<int> _SendTimeout;
        [ACPropertyConfig("en{'SendTimeout'}de{'SendTimeout'}")]
        public int SendTimeout
        {
            get => _SendTimeout.ValueT;
            set => _SendTimeout.ValueT = value;
        }


        private ACPropertyConfigValue<int> _ReceiveTimeout;
        [ACPropertyConfig("en{'ReceiveTimeout'}de{'ReceiveTimeout'}")]
        public int ReceiveTimeout
        {
            get => _ReceiveTimeout.ValueT;
            set => _ReceiveTimeout.ValueT = value;
        }


        private ACPropertyConfigValue<int> _PrintTries;
        [ACPropertyConfig("en{'Print Tries'}de{'Print Tries'}")]
        public int PrintTries
        {
            get => _PrintTries.ValueT;
            set => _PrintTries.ValueT = value;
        }

        private ACPropertyConfigValue<int> _CodePage;
        [ACPropertyConfig("en{'Code Page'}de{'Code Page'}")]
        public int CodePage
        {
            get => _CodePage.ValueT;
            set => _CodePage.ValueT = value;
        }

        private ACDispatchedDelegateQueue _DelegateQueue = null;
        public ACDispatchedDelegateQueue DelegateQueue
        {
            get
            {

                using (ACMonitor.Lock(_20015_LockValue))
                {
                    return _DelegateQueue;
                }
            }
        }

        [ACPropertyBindingSource]
        public IACContainerTNet<bool> IsConnected { get; set; }

        #endregion

        #region Methods

        #region Methods -> General (print & provide data)

        [ACMethodInfo("Print", "en{'Print on server'}de{'Auf Server drucken'}", 200, true)]
        public virtual void Print(Guid bsoClassID, string designACIdentifier, PAOrderInfo pAOrderInfo, int copies)
        {
            // suggestion: Use Queue
            DelegateQueue.Add(() =>
            {
                DoPrint(bsoClassID, designACIdentifier, pAOrderInfo, copies);
            });
        }

        [ACMethodInfo("Print", "en{'Print on server'}de{'Auf Server drucken'}", 200, true)]
        public virtual void PrintByACUrl(string acUrl, string designACIdentifier, PAOrderInfo pAOrderInfo, int copies)
        {
            // suggestion: Use Queue
            DelegateQueue.Add(() =>
            {
                DoPrint(acUrl, designACIdentifier, pAOrderInfo, copies);
            });
        }

        public void DoPrint(Guid bsoClassID, string designACIdentifier, PAOrderInfo pAOrderInfo, int copies)
        {
            ACBSO acBSO = null;
            try
            {
                acBSO = GetACBSO(bsoClassID, pAOrderInfo);
                if (acBSO == null)
                    return;
                DoPrint(acBSO, designACIdentifier, pAOrderInfo, copies);
            }
            catch (Exception e)
            {
                Messages.LogException(this.GetACUrl(), "DoPrint(10)", e);
            }
            finally
            {
                try
                {
                    // @aagincic: is this reqiered by IsPoolable = true?
                    // with this database context is disposed
                    // by many concurent request is exception thrown:
                    // ObjectContext instance has been disposed and can no longer be used for operations that require a connection.
                    if (acBSO != null)
                        acBSO.Stop();
                }
                catch (Exception e)
                {
                    Messages.LogException(this.GetACUrl(), "DoPrint(20)", e);
                }
            }
        }

        public void DoPrint(string acUrl, string designACIdentifier, PAOrderInfo pAOrderInfo, int copies)
        {
            ACBSO acBSO = null;
            try
            {
                acBSO = GetACBSO(acUrl, pAOrderInfo);
                if (acBSO == null)
                    return;
                DoPrint(acBSO, designACIdentifier, pAOrderInfo, copies);
            }
            catch (Exception e)
            {
                Messages.LogException(this.GetACUrl(), "DoPrint(30)", e);
            }
            finally
            {
                try
                {
                    // @aagincic: is this reqiered by IsPoolable = true?
                    // with this database context is disposed
                    // by many concurent request is exception thrown:
                    // ObjectContext instance has been disposed and can no longer be used for operations that require a connection.
                    if (acBSO != null)
                        acBSO.Stop();
                }
                catch (Exception e)
                {
                    Messages.LogException(this.GetACUrl(), "DoPrint(40)", e);
                }
            }
        }

        public void DoPrint(ACBSO acBSO, string designACIdentifier, PAOrderInfo pAOrderInfo, int copies)
        {
            ACClassDesign aCClassDesign = acBSO.GetDesignForPrinting(GetACUrl(), designACIdentifier, pAOrderInfo);
            if (aCClassDesign == null)
                return;
            ReportData reportData = GetReportData(acBSO, aCClassDesign);
            byte[] bytes = null;


            try
            {
                // FlowDocument generate (separate thread)
                using (ReportDocument reportDocument = new ReportDocument(aCClassDesign.XMLDesign))
                {
                    FlowDocument flowDoc = reportDocument.CreateFlowDocument(reportData);
                    PrintContext printContext = GetPrintContext(flowDoc);
                    bytes = printContext.Main;
                }
            }
            catch (Exception e)
            {
                this.Messages.LogException(this.GetACUrl(), "InvokeAsync", e);
            }

            if (bytes != null)
                SendDataToPrinter(bytes);
        }

        /// <summary>
        /// Factiry BSI abd setzo data frin PAPrderInfo
        /// </summary>
        /// <param name="componetACUrl"></param>
        /// <param name="pAOrderInfo"></param>
        /// <returns></returns>
        public virtual ACBSO GetACBSO(Guid bsoClassID, PAOrderInfo pAOrderInfo)
        {
            ACClass bsoACClass = Root.Database.ContextIPlus.GetACType(bsoClassID);
            ACBSO acBSO = StartComponent(bsoACClass, bsoACClass, 
                new ACValueList() 
                { 
                    new ACValue(Const.ParamSeperateContext, typeof(bool), true), 
                    new ACValue(Const.SkipSearchOnStart, typeof(bool), true) 
                }) as ACBSO;
            if (acBSO == null)
                return null;
            acBSO.FilterByOrderInfo(pAOrderInfo);
            return acBSO;
        }

        public virtual ACBSO GetACBSO(string acUrl, PAOrderInfo pAOrderInfo)
        {
            ACBSO acBSO = this.Root.ACUrlCommand(acUrl,
                new ACValueList()
                {
                    new ACValue(Const.ParamSeperateContext, typeof(bool), true),
                    new ACValue(Const.SkipSearchOnStart, typeof(bool), true)
                }) as ACBSO;
            if (acBSO == null)
                return null;
            acBSO.FilterByOrderInfo(pAOrderInfo);
            return acBSO;
        }


        /// <summary>
        /// From prepared ACBSO produce ReportData
        /// </summary>
        /// <param name="aCBSO"></param>
        /// <param name="aCClassDesign"></param>
        /// <returns></returns>
        public virtual ReportData GetReportData(ACBSO aCBSO, ACClassDesign aCClassDesign)
        {
            bool cloneInstantiated = false;
            ACQueryDefinition aCQueryDefinition = null;
            ReportData reportData = ReportData.BuildReportData(out cloneInstantiated, Global.CurrentOrList.Current, aCBSO, aCQueryDefinition, aCClassDesign, true);
            return reportData;
        }

        /// <summary>
        /// Convert report data to stream
        /// </summary>
        /// <param name="reportData"></param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual bool SendDataToPrinter(byte[] bytes)
        {
            try
            {
                using (TcpClient tcpClient = new TcpClient(IPAddress, Port))
                {
                    NetworkStream clientStream = tcpClient.GetStream();
                    clientStream.Write(bytes, 0, bytes.Length);
                    clientStream.Flush();
                }
            }
            catch (Exception e)
            {
                string message = String.Format("Connection failed to {0}. See log for further details.", IPAddress);
                if (IsAlarmActive(IsConnected, message) == null)
                    Messages.LogException(GetACUrl(), "SendDataToPrinter(10)", e);
                OnNewAlarmOccurred(IsConnected, message);
                IsConnected.ValueT = false;
                return false;
            }
            if (IsAlarmActive(IsConnected) != null)
                AcknowledgeAlarms();
            //OnAlarmDisappeared(IsConnected);
            IsConnected.ValueT = true;
            return true;
        }

        /// <summary>
        /// Component specific implementation writing data from ReportData to network stream 
        /// </summary>
        /// <param name="clientStream"></param>
        /// <param name="reportData"></param>
        /// <exception cref="NotImplementedException"></exception>
        protected PrintContext GetPrintContext(FlowDocument flowDocument)
        {
            Encoding encoder = Encoding.ASCII;
            VBFlowDocument vBFlowDocument = flowDocument as VBFlowDocument;

            int? codePage = null;

            if(CodePage > 0)
            {
                codePage = CodePage;
            }
            else if (vBFlowDocument != null && vBFlowDocument.CodePage > 0)
            {
                codePage = vBFlowDocument.CodePage;
            }

            if(codePage != null)
            {
                try
                {
                    encoder = Encoding.GetEncoding(codePage.Value);
                }
                catch (Exception ex)
                {
                    Messages.LogException(GetACUrl(), nameof(GetPrintContext), ex);
                }
            }

            PrintContext printContext = new PrintContext();
            printContext.FlowDocument = flowDocument;
            printContext.Encoding = encoder;
            printContext.ColumnMultiplier = 1;
            printContext.ColumnDivisor = 1;
            OnRenderFlowDocment(printContext, printContext.FlowDocument);
            return printContext;
        }

        #endregion

        #region Methods -> Render

        #region Methods -> Render -> Block

        public virtual void OnRenderFlowDocment(PrintContext printContext, FlowDocument flowDoc)
        {
            OnRenderBlocks(printContext, flowDoc.Blocks, BlockDocumentPosition.General);
        }

        protected void OnRenderBlocks(PrintContext printContext, BlockCollection blocks, BlockDocumentPosition position)
        {
            foreach (Block block in blocks)
                OnRenderBlock(printContext, block, position);
        }

        protected void OnRenderBlock(PrintContext printContext, Block block, BlockDocumentPosition position)
        {
            OnRenderBlockHeader(printContext, block, position);
            if (block is SectionReportHeader)
                OnRenderSectionReportHeader(printContext, (SectionReportHeader)block);
            else if (block is SectionReportFooter)
                OnRenderSectionReportFooter(printContext, (SectionReportFooter)block);
            else if (block is SectionDataGroup)
                OnRenderSectionDataGroup(printContext, (SectionDataGroup)block);
            else if (block is Table)
                OnRenderSectionTable(printContext, (Table)block);
            else if (block is Paragraph)
                OnRenderParagraph(printContext, (Paragraph)block);
            OnRenderBlockFooter(printContext, block, position);
        }

        public abstract void OnRenderBlockHeader(PrintContext printContext, Block block, BlockDocumentPosition position);

        public abstract void OnRenderBlockFooter(PrintContext printContext, Block block, BlockDocumentPosition position);

        protected void OnRenderSectionReportHeader(PrintContext printContext, SectionReportHeader sectionReportHeader)
        {
            OnRenderSectionReportHeaderHeader(printContext, sectionReportHeader);
            OnRenderBlocks(printContext, sectionReportHeader.Blocks, BlockDocumentPosition.General);
            OnRenderSectionReportHeaderFooter(printContext, sectionReportHeader);
        }

        public abstract void OnRenderSectionReportHeaderHeader(PrintContext printContext, SectionReportHeader sectionReportHeader);

        public abstract void OnRenderSectionReportHeaderFooter(PrintContext printContext, SectionReportHeader sectionReportHeader);

        protected void OnRenderSectionReportFooter(PrintContext printContext, SectionReportFooter sectionReportFooter)
        {
            OnRenderSectionReportFooterHeader(printContext, sectionReportFooter);
            OnRenderBlocks(printContext, sectionReportFooter.Blocks, BlockDocumentPosition.General);
            OnRenderSectionReportFooterFooter(printContext, sectionReportFooter);
        }

        public abstract void OnRenderSectionReportFooterHeader(PrintContext printContext, SectionReportFooter sectionReportFooter);

        public abstract void OnRenderSectionReportFooterFooter(PrintContext printContext, SectionReportFooter sectionReportFooter);

        protected virtual void OnRenderSectionDataGroup(PrintContext printContext, SectionDataGroup sectionDataGroup)
        {
            OnRenderSectionDataGroupHeader(printContext, sectionDataGroup);
            OnRenderBlocks(printContext, sectionDataGroup.Blocks, BlockDocumentPosition.General);
            OnRenderSectionDataGroupFooter(printContext, sectionDataGroup);
        }

        public abstract void OnRenderSectionDataGroupHeader(PrintContext printContext, SectionDataGroup sectionDataGroup);

        public abstract void OnRenderSectionDataGroupFooter(PrintContext printContext, SectionDataGroup sectionDataGroup);

        #endregion

        #region Methods -> Render -> Table

        protected void OnRenderSectionTable(PrintContext printContext, Table table)
        {
            OnRenderSectionTableHeader(printContext, table);

            printContext.ColumnMultiplier = 1;
            printContext.ColumnDivisor = table.Columns.Count;

            foreach (TableColumn tableColumn in table.Columns)
                OnRenderTableColumn(printContext, tableColumn);

            foreach (TableRowGroup tableRowGroup in table.RowGroups)
                OnRenderTableRowGroup(printContext, tableRowGroup);
            OnRenderSectionTableFooter(printContext, table);

            printContext.ColumnMultiplier = 1;
            printContext.ColumnDivisor = 1;
        }

        public abstract void OnRenderSectionTableHeader(PrintContext printContext, Table table);

        public abstract void OnRenderSectionTableFooter(PrintContext printContext, Table table);

        public abstract void OnRenderTableColumn(PrintContext printContext, TableColumn tableColumn);

        protected void OnRenderTableRowGroup(PrintContext printContext, TableRowGroup tableRowGroup)
        {
            OnRenderTableRowGroupHeader(printContext, tableRowGroup);
            foreach (TableRow tableRow in tableRowGroup.Rows)
                OnRenderTableRow(printContext, tableRow);
            OnRenderTableRowGroupFooter(printContext, tableRowGroup);
        }

        public abstract void OnRenderTableRowGroupHeader(PrintContext printContext, TableRowGroup tableRowGroup);

        public abstract void OnRenderTableRowGroupFooter(PrintContext printContext, TableRowGroup tableRowGroup);

        protected void OnRenderTableRow(PrintContext printContext, TableRow tableRow)
        {
            OnRenderTableRowHeader(printContext, tableRow);
            foreach (TableCell tableCell in tableRow.Cells)
            {
                printContext.ColumnMultiplier = tableRow.Cells.IndexOf(tableCell);
                OnRenderTableCell(printContext, tableCell);
            }
            OnRenderTableRowFooter(printContext, tableRow);
        }
        public abstract void OnRenderTableRowHeader(PrintContext printContext, TableRow tableRow);

        public abstract void OnRenderTableRowFooter(PrintContext printContext, TableRow tableRow);

        protected void OnRenderTableCell(PrintContext printContext, TableCell tableCell)
        {
            foreach (Block block in tableCell.Blocks)
                OnRenderBlock(printContext, block, BlockDocumentPosition.InTable);
        }

        #endregion

        #region Methods -> Render -> Inlines

        protected void OnRenderParagraph(PrintContext printContext, Paragraph paragraph)
        {
            OnRenderParagraphHeader(printContext, paragraph);
            foreach (Inline inline in paragraph.Inlines)
            {
                if (inline is InlineContextValue)
                    OnRenderInlineContextValue(printContext, (InlineContextValue)inline);
                else if (inline is InlineDocumentValue)
                    OnRenderInlineDocumentValue(printContext, (InlineDocumentValue)inline);
                else if (inline is InlineACMethodValue)
                    OnRenderInlineACMethodValue(printContext, (InlineACMethodValue)inline);
                else if (inline is InlineTableCellValue)
                    OnRenderInlineTableCellValue(printContext, (InlineTableCellValue)inline);
                else if (inline is InlineBarcode)
                    OnRenderInlineBarcode(printContext, (InlineBarcode)inline);
                else if (inline is InlineBoolValue)
                    OnRenderInlineBoolValue(printContext, (InlineBoolValue)inline);
                else if (inline is Run)
                    OnRednerRun(printContext, (Run)inline);
            }
            OnRenderParagraphFooter(printContext, paragraph);
        }

        public abstract void OnRenderParagraphHeader(PrintContext printContext, Paragraph paragraph);

        public abstract void OnRenderParagraphFooter(PrintContext printContext, Paragraph paragraph);

        public abstract void OnRenderInlineContextValue(PrintContext printContext, InlineContextValue inlineContextValue);

        public abstract void OnRenderInlineDocumentValue(PrintContext printContext, InlineDocumentValue inlineDocumentValue);

        public abstract void OnRenderInlineACMethodValue(PrintContext printContext, InlineACMethodValue inlineACMethodValue);

        public abstract void OnRenderInlineTableCellValue(PrintContext printContext, InlineTableCellValue inlineTableCellValue);

        public abstract void OnRenderInlineBarcode(PrintContext printContext, InlineBarcode inlineBarcode);

        public abstract void OnRenderInlineBoolValue(PrintContext printContext, InlineBoolValue inlineBoolValue);

        public abstract void OnRednerRun(PrintContext printContext, Run run);

        #endregion

        #endregion

        #region Execute-Helper
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(Print):
                    Print((Guid) acParameter[0],
                          acParameter[1] as string,
                          acParameter[2] as PAOrderInfo,
                          (int)acParameter[3]);
                    return true;
                case nameof(PrintByACUrl):
                    PrintByACUrl(acParameter[0] as string,
                          acParameter[1] as string,
                          acParameter[2] as PAOrderInfo,
                          (int)acParameter[3]);
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
        #endregion


        #endregion
    }
}
