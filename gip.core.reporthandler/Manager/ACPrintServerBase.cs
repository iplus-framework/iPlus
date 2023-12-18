using gip.core.datamodel;
using gip.core.autocomponent;
using System;
using System.Net.Sockets;
using System.Text;
using gip.core.reporthandler.Flowdoc;
using System.Windows.Documents;
using gip.core.layoutengine;

namespace gip.core.reporthandler
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACPrintServerBase'}de{'ACPrintServerBase'}", Global.ACKinds.TPABGModule, Global.ACStorableTypes.Required, false, "", false)]
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

            _ = IPAddress;
            _ = Port;
            _ = SendTimeout;
            _ = ReceiveTimeout;
            _ = PrintTries;
            _ = CodePage;

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
                Messages.LogException(this.GetACUrl(), "DoPrint(30)", e, true);
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
            PrintJob printJob = null;

            try
            {
                // FlowDocument generate (separate thread)
                using (ReportDocument reportDocument = new ReportDocument(aCClassDesign.XMLDesign))
                {
                    FlowDocument flowDoc = reportDocument.CreateFlowDocument(reportData);
                    printJob = GetPrintJob(flowDoc);
                }
            }
            catch (Exception e)
            {
                this.Messages.LogException(this.GetACUrl(), "InvokeAsync", e);
            }

            if (printJob != null)
                SendDataToPrinter(printJob);
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

        public static bool IsLocalConnection(string ipAddress)
        {
            return String.IsNullOrEmpty(ipAddress)
                    || ipAddress == "localhost"
                    || ipAddress == "127.0.0.1";
        }

        /// <summary>
        /// Convert report data to stream
        /// </summary>
        /// <param name="reportData"></param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual bool SendDataToPrinter(PrintJob printJob)
        {
            if (printJob == null || printJob.Main == null)
            {
                return false;
            }

            if (IsLocalConnection(IPAddress))
            {
                //IsConnected.ValueT = true;
                return true;
            }
            try
            {
                using (TcpClient tcpClient = new TcpClient(IPAddress, Port))
                {
                    NetworkStream clientStream = tcpClient.GetStream();
                    clientStream.Write(printJob.Main, 0, printJob.Main.Length);
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
        public virtual PrintJob GetPrintJob(FlowDocument flowDocument)
        {
            Encoding encoder = Encoding.ASCII;
            VBFlowDocument vBFlowDocument = flowDocument as VBFlowDocument;

            int? codePage = null;

            if (vBFlowDocument != null && vBFlowDocument.CodePage > 0)
            {
                codePage = vBFlowDocument.CodePage;
            }
            else if (CodePage > 0)
            {
                codePage = CodePage;
            }


            if (codePage != null)
            {
                try
                {
                    encoder = Encoding.GetEncoding(codePage.Value);
                }
                catch (Exception ex)
                {
                    Messages.LogException(GetACUrl(), nameof(GetPrintJob), ex);
                }
            }

            PrintJob printJob = new PrintJob();
            printJob.FlowDocument = flowDocument;
            printJob.Encoding = encoder;
            printJob.ColumnMultiplier = 1;
            printJob.ColumnDivisor = 1;
            OnRenderFlowDocument(printJob, printJob.FlowDocument);
            return printJob;
        }

        #endregion

        #region Methods -> Render

        #region Methods -> Render -> Block

        public virtual void OnRenderFlowDocument(PrintJob printJob, FlowDocument flowDoc)
        {
            OnRenderBlocks(printJob, flowDoc.Blocks, BlockDocumentPosition.General);
        }

        protected void OnRenderBlocks(PrintJob printJob, BlockCollection blocks, BlockDocumentPosition position)
        {
            foreach (Block block in blocks)
                OnRenderBlock(printJob, block, position);
        }

        protected void OnRenderBlock(PrintJob printJob, Block block, BlockDocumentPosition position)
        {
            OnRenderBlockHeader(printJob, block, position);
            if (block is SectionReportHeader)
                OnRenderSectionReportHeader(printJob, (SectionReportHeader)block);
            else if (block is SectionReportFooter)
                OnRenderSectionReportFooter(printJob, (SectionReportFooter)block);
            else if (block is SectionDataGroup)
                OnRenderSectionDataGroup(printJob, (SectionDataGroup)block);
            else if (block is Table)
                OnRenderSectionTable(printJob, (Table)block);
            else if (block is Paragraph)
                OnRenderParagraph(printJob, (Paragraph)block);
            OnRenderBlockFooter(printJob, block, position);
        }

        public abstract void OnRenderBlockHeader(PrintJob printJob, Block block, BlockDocumentPosition position);

        public abstract void OnRenderBlockFooter(PrintJob printJob, Block block, BlockDocumentPosition position);

        protected void OnRenderSectionReportHeader(PrintJob printJob, SectionReportHeader sectionReportHeader)
        {
            OnRenderSectionReportHeaderHeader(printJob, sectionReportHeader);
            OnRenderBlocks(printJob, sectionReportHeader.Blocks, BlockDocumentPosition.General);
            OnRenderSectionReportHeaderFooter(printJob, sectionReportHeader);
        }

        public abstract void OnRenderSectionReportHeaderHeader(PrintJob printJob, SectionReportHeader sectionReportHeader);

        public abstract void OnRenderSectionReportHeaderFooter(PrintJob printJob, SectionReportHeader sectionReportHeader);

        protected void OnRenderSectionReportFooter(PrintJob printJob, SectionReportFooter sectionReportFooter)
        {
            OnRenderSectionReportFooterHeader(printJob, sectionReportFooter);
            OnRenderBlocks(printJob, sectionReportFooter.Blocks, BlockDocumentPosition.General);
            OnRenderSectionReportFooterFooter(printJob, sectionReportFooter);
        }

        public abstract void OnRenderSectionReportFooterHeader(PrintJob printJob, SectionReportFooter sectionReportFooter);

        public abstract void OnRenderSectionReportFooterFooter(PrintJob printJob, SectionReportFooter sectionReportFooter);

        protected virtual void OnRenderSectionDataGroup(PrintJob printJob, SectionDataGroup sectionDataGroup)
        {
            OnRenderSectionDataGroupHeader(printJob, sectionDataGroup);
            OnRenderBlocks(printJob, sectionDataGroup.Blocks, BlockDocumentPosition.General);
            OnRenderSectionDataGroupFooter(printJob, sectionDataGroup);
        }

        public abstract void OnRenderSectionDataGroupHeader(PrintJob printJob, SectionDataGroup sectionDataGroup);

        public abstract void OnRenderSectionDataGroupFooter(PrintJob printJob, SectionDataGroup sectionDataGroup);

        #endregion

        #region Methods -> Render -> Table

        protected void OnRenderSectionTable(PrintJob printJob, Table table)
        {
            OnRenderSectionTableHeader(printJob, table);

            printJob.ColumnMultiplier = 1;
            printJob.ColumnDivisor = table.Columns.Count;

            foreach (TableColumn tableColumn in table.Columns)
                OnRenderTableColumn(printJob, tableColumn);

            foreach (TableRowGroup tableRowGroup in table.RowGroups)
                OnRenderTableRowGroup(printJob, tableRowGroup);
            OnRenderSectionTableFooter(printJob, table);

            printJob.ColumnMultiplier = 1;
            printJob.ColumnDivisor = 1;
        }

        public abstract void OnRenderSectionTableHeader(PrintJob printJob, Table table);

        public abstract void OnRenderSectionTableFooter(PrintJob printJob, Table table);

        public abstract void OnRenderTableColumn(PrintJob printJob, TableColumn tableColumn);

        protected void OnRenderTableRowGroup(PrintJob printJob, TableRowGroup tableRowGroup)
        {
            OnRenderTableRowGroupHeader(printJob, tableRowGroup);
            foreach (TableRow tableRow in tableRowGroup.Rows)
                OnRenderTableRow(printJob, tableRow);
            OnRenderTableRowGroupFooter(printJob, tableRowGroup);
        }

        public abstract void OnRenderTableRowGroupHeader(PrintJob printJob, TableRowGroup tableRowGroup);

        public abstract void OnRenderTableRowGroupFooter(PrintJob printJob, TableRowGroup tableRowGroup);

        protected void OnRenderTableRow(PrintJob printJob, TableRow tableRow)
        {
            OnRenderTableRowHeader(printJob, tableRow);
            foreach (TableCell tableCell in tableRow.Cells)
            {
                printJob.ColumnMultiplier = tableRow.Cells.IndexOf(tableCell);
                OnRenderTableCell(printJob, tableCell);
            }
            OnRenderTableRowFooter(printJob, tableRow);
        }
        public abstract void OnRenderTableRowHeader(PrintJob printJob, TableRow tableRow);

        public abstract void OnRenderTableRowFooter(PrintJob printJob, TableRow tableRow);

        protected void OnRenderTableCell(PrintJob printJob, TableCell tableCell)
        {
            foreach (Block block in tableCell.Blocks)
                OnRenderBlock(printJob, block, BlockDocumentPosition.InTable);
        }

        #endregion

        #region Methods -> Render -> Inlines

        protected void OnRenderParagraph(PrintJob printJob, Paragraph paragraph)
        {
            OnRenderParagraphHeader(printJob, paragraph);
            foreach (Inline inline in paragraph.Inlines)
            {
                if (inline is InlineContextValue)
                    OnRenderInlineContextValue(printJob, (InlineContextValue)inline);
                else if (inline is InlineDocumentValue)
                    OnRenderInlineDocumentValue(printJob, (InlineDocumentValue)inline);
                else if (inline is InlineACMethodValue)
                    OnRenderInlineACMethodValue(printJob, (InlineACMethodValue)inline);
                else if (inline is InlineTableCellValue)
                    OnRenderInlineTableCellValue(printJob, (InlineTableCellValue)inline);
                else if (inline is InlineBarcode)
                    OnRenderInlineBarcode(printJob, (InlineBarcode)inline);
                else if (inline is InlineBoolValue)
                    OnRenderInlineBoolValue(printJob, (InlineBoolValue)inline);
                else if (inline is Run)
                    OnRenderRun(printJob, (Run)inline);
                else if (inline is LineBreak)
                    OnRenderLineBreak(printJob, (LineBreak)inline);

            }
            OnRenderParagraphFooter(printJob, paragraph);
        }

        public abstract void OnRenderParagraphHeader(PrintJob printJob, Paragraph paragraph);

        public abstract void OnRenderParagraphFooter(PrintJob printJob, Paragraph paragraph);

        public abstract void OnRenderInlineContextValue(PrintJob printJob, InlineContextValue inlineContextValue);

        public abstract void OnRenderInlineDocumentValue(PrintJob printJob, InlineDocumentValue inlineDocumentValue);

        public abstract void OnRenderInlineACMethodValue(PrintJob printJob, InlineACMethodValue inlineACMethodValue);

        public abstract void OnRenderInlineTableCellValue(PrintJob printJob, InlineTableCellValue inlineTableCellValue);

        public abstract void OnRenderInlineBarcode(PrintJob printJob, InlineBarcode inlineBarcode);

        public abstract void OnRenderInlineBoolValue(PrintJob printJob, InlineBoolValue inlineBoolValue);

        public abstract void OnRenderRun(PrintJob printJob, Run run);

        public abstract void OnRenderLineBreak(PrintJob printJob, LineBreak lineBreak);
        #endregion

        #endregion

        #region Execute-Helper
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(Print):
                    Print((Guid)acParameter[0],
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
