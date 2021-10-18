﻿using gip.core.datamodel;
using gip.core.autocomponent;
using System;
using System.Net.Sockets;
using System.Text;
using gip.core.reporthandler.Flowdoc;
using System.Windows.Documents;
using System.Windows;
using System.Threading.Tasks;

namespace gip.core.reporthandler
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACPrintServerBase'}de{'ACPrintServerBase'}", Global.ACKinds.TACApplicationManager, Global.ACStorableTypes.Required, false, "", false)]
    public abstract class ACPrintServerBase : PAClassAlarmingBase
    {

        #region c´tors
        public const string MN_Print = "Print";

        public ACPrintServerBase(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _IPAddress = new ACPropertyConfigValue<string>(this, "IPAddress", null);
            _Port = new ACPropertyConfigValue<int>(this, "Port", 0);
            _SendTimeout = new ACPropertyConfigValue<int>(this, "SendTimeout", 0);
            _ReceiveTimeout = new ACPropertyConfigValue<int>(this, "ReceiveTimeout", 0);
            _PrintTries = new ACPropertyConfigValue<int>(this, "PrintTries", 0);
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;

            using (ACMonitor.Lock(_20015_LockValue))
            {
                _DelegateQueue = new ACDelegateQueue(ACIdentifier);
            }
            _DelegateQueue.StartWorkerThread();

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

        //public override bool ACPostInit()
        //{
        //    bool baseReturn = base.ACPostInit();
        //    string tempIpAddress = IPAddress;
        //    int temp = Port;
        //    temp = SendTimeout;
        //    temp = ReceiveTimeout;
        //    temp = PrintTries;
        //    return baseReturn;
        //}

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

        private ACDelegateQueue _DelegateQueue = null;
        public ACDelegateQueue DelegateQueue
        {
            get
            {

                using (ACMonitor.Lock(_20015_LockValue))
                {
                    return _DelegateQueue;
                }
            }
        }

        #endregion

        #region Methods

        #region Methods -> General (print & provide data)

        [ACMethodInfo("Print", "en{'Print on server'}de{'Auf Server drucken'}", 200, true)]
        public virtual void Print(Guid bsoClassID, string designACIdentifier, PAOrderInfo pAOrderInfo, int copies)
        {
            // suggestion: Use Queue
            DelegateQueue.Add(() =>
            {
                DoPrint(bsoClassID, designACIdentifier,pAOrderInfo,copies);
            });

            // @aagincic comment: this used while code above causes exception:
            //      -> The calling thread must be STA, because many UI components require this.
            //Application.Current.Dispatcher.Invoke((Action)delegate
            //{
            //    DoPrint(bsoClassID, designACIdentifier, pAOrderInfo, copies);
            //});
        }

        private async Task DoPrint(Guid bsoClassID, string designACIdentifier, PAOrderInfo pAOrderInfo, int copies)
        {
            ACBSO acBSO = null;
            try
            {
                // PAOrderInfo => 
                // ACPrintServer Step04 - Get server instance BSO and mandatory report design
                acBSO = GetACBSO(bsoClassID, pAOrderInfo);
                ACClassDesign aCClassDesign = acBSO.GetDesign(designACIdentifier);
                // ACPrintServer Step05 - Prepare ReportData
                ReportData reportData = GetReportData(acBSO, aCClassDesign);
                ReportDocument reportDocument = null;
                FlowDocument flowDoc = null;
                await Application.Current.Dispatcher.InvokeAsync((Action)delegate
                {
                    reportDocument = new ReportDocument(aCClassDesign.XMLDesign);
                    flowDoc = reportDocument.CreateFlowDocument(reportData);
                });
                // ACPrintServer Step06 - Write to stream
                SendDataToPrinter(flowDoc);
            }
            catch (Exception e)
            {
                // TODO: Alarm
                Messages.LogException(this.GetACUrl(), "Print(10)", e);
            }
            finally
            {
                try
                {
                    // BSO must be stopped!
                    if (acBSO != null)
                        acBSO.Stop();
                }
                catch (Exception e)
                {
                    // TODO: Alarm
                    Messages.LogException(this.GetACUrl(), "Print(20)", e);
                }
            }
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
            ACBSO acBSO = StartComponent(bsoACClass, bsoACClass, new ACValueList()) as ACBSO;
            if (acBSO == null)
                return null;
            acBSO.SetOrderInfo(pAOrderInfo);
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
            ReportData reportData = ReportData.BuildReportData(out cloneInstantiated, Global.CurrentOrList.Current, aCBSO, aCQueryDefinition, aCClassDesign);
            return reportData;
        }

        /// <summary>
        /// Convert report data to stream
        /// </summary>
        /// <param name="reportData"></param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void SendDataToPrinter(FlowDocument flowDoc)
        {
            using (TcpClient tcpClient = new TcpClient(IPAddress, Port))
            {
                NetworkStream clientStream = tcpClient.GetStream();
                ASCIIEncoding encoder = new ASCIIEncoding();

                PrintContext printContext = new PrintContext();
                printContext.TcpClient = tcpClient;
                printContext.NetworkStream = clientStream;
                printContext.FlowDocument = flowDoc;
                printContext.Encoding = encoder;
                printContext.ColumnMultiplier = 1;
                printContext.ColumnDivisor = 1;

                WriteToStream(printContext);

                clientStream.Write(printContext.Main, 0, printContext.Main.Length);

                clientStream.Flush();
            }
        }

        /// <summary>
        /// Component specific implementation writing data from ReportData to network stream 
        /// </summary>
        /// <param name="clientStream"></param>
        /// <param name="reportData"></param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void WriteToStream(PrintContext printContext)
        {
            OnRenderFlowDocment(printContext, printContext.FlowDocument);
        }

        #endregion

        #region Methods -> Render

        #region Methods -> Render -> Block

        public virtual void OnRenderFlowDocment(PrintContext printContext, FlowDocument flowDoc)
        {
            OnRenderBlocks(printContext, flowDoc.Blocks, BlockDocumentPosition.General);
        }

        public virtual void OnRenderBlocks(PrintContext printContext, BlockCollection blocks, BlockDocumentPosition position)
        {
            foreach (Block block in blocks)
                OnRenderBlock(printContext, block, position);
        }

        public virtual void OnRenderBlock(PrintContext printContext, Block block, BlockDocumentPosition position)
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

        public virtual void OnRenderBlockHeader(PrintContext printContext, Block block, BlockDocumentPosition position)
        {

        }

        public virtual void OnRenderBlockFooter(PrintContext printContext, Block block, BlockDocumentPosition position)
        {
        }

        public virtual void OnRenderSectionReportHeader(PrintContext printContext, SectionReportHeader sectionReportHeader)
        {
            OnRenderSectionReportHeaderHeader(printContext, sectionReportHeader);
            OnRenderBlocks(printContext, sectionReportHeader.Blocks, BlockDocumentPosition.General);
            OnRenderSectionReportHeaderFooter(printContext, sectionReportHeader);
        }

        public virtual void OnRenderSectionReportHeaderHeader(PrintContext printContext, SectionReportHeader sectionReportHeader)
        {
            //
        }

        public virtual void OnRenderSectionReportHeaderFooter(PrintContext printContext, SectionReportHeader sectionReportHeader)
        {
            //
        }

        public virtual void OnRenderSectionReportFooter(PrintContext printContext, SectionReportFooter sectionReportFooter)
        {
            OnRenderSectionReportFooterHeader(printContext, sectionReportFooter);
            OnRenderBlocks(printContext, sectionReportFooter.Blocks, BlockDocumentPosition.General);
            OnRenderSectionReportFooterFooter(printContext, sectionReportFooter);
        }

        public virtual void OnRenderSectionReportFooterHeader(PrintContext printContext, SectionReportFooter sectionReportFooter)
        {

        }

        public virtual void OnRenderSectionReportFooterFooter(PrintContext printContext, SectionReportFooter sectionReportFooter)
        {
            // 
        }

        public virtual void OnRenderSectionDataGroup(PrintContext printContext, SectionDataGroup sectionDataGroup)
        {
            OnRenderSectionDataGroupHeader(printContext, sectionDataGroup);
            OnRenderBlocks(printContext, sectionDataGroup.Blocks, BlockDocumentPosition.General);
            OnRenderSectionDataGroupFooter(printContext, sectionDataGroup);
        }

        public virtual void OnRenderSectionDataGroupHeader(PrintContext printContext, SectionDataGroup sectionDataGroup)
        {
            //
        }

        public virtual void OnRenderSectionDataGroupFooter(PrintContext printContext, SectionDataGroup sectionDataGroup)
        {
            // 
        }

        #endregion

        #region Methods -> Render -> Table

        public virtual void OnRenderSectionTable(PrintContext printContext, Table table)
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

        public virtual void OnRenderSectionTableHeader(PrintContext printContext, Table table)
        {
            //
        }

        public virtual void OnRenderSectionTableFooter(PrintContext printContext, Table table)
        {
            //
        }

        public virtual void OnRenderTableColumn(PrintContext printContext, TableColumn tableColumn)
        {

        }

        public virtual void OnRenderTableRowGroup(PrintContext printContext, TableRowGroup tableRowGroup)
        {
            OnRenderTableRowGroupHeader(printContext, tableRowGroup);
            foreach (TableRow tableRow in tableRowGroup.Rows)
                OnRenderTableRow(printContext, tableRow);
            OnRenderTableRowGroupFooter(printContext, tableRowGroup);
        }

        public virtual void OnRenderTableRowGroupHeader(PrintContext printContext, TableRowGroup tableRowGroup)
        {
            //
        }

        public virtual void OnRenderTableRowGroupFooter(PrintContext printContext, TableRowGroup tableRowGroup)
        {
            //
        }

        public virtual void OnRenderTableRow(PrintContext printContext, TableRow tableRow)
        {
            OnRenderTableRowHeader(printContext, tableRow);
            foreach (TableCell tableCell in tableRow.Cells)
            {
                printContext.ColumnMultiplier = tableRow.Cells.IndexOf(tableCell);
                OnRenderTableCell(printContext, tableCell);
            }
            OnRenderTableRowFooter(printContext, tableRow);
        }
        public virtual void OnRenderTableRowHeader(PrintContext printContext, TableRow tableRow)
        {
            //
        }

        public virtual void OnRenderTableRowFooter(PrintContext printContext, TableRow tableRow)
        {
            //
        }

        public virtual void OnRenderTableCell(PrintContext printContext, TableCell tableCell)
        {
            foreach (Block block in tableCell.Blocks)
                OnRenderBlock(printContext, block, BlockDocumentPosition.InTable);
        }

        #endregion

        #region Methods -> Render -> Inlines

        public virtual void OnRenderParagraph(PrintContext printContext, Paragraph paragraph)
        {
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
            }
        }

        public virtual void OnRenderInlineContextValue(PrintContext printContext, InlineContextValue inlineContextValue)
        {
            // inline.Text
        }

        public virtual void OnRenderInlineDocumentValue(PrintContext printContext, InlineDocumentValue inlineDocumentValue)
        {
            // inline.Text
        }

        public virtual void OnRenderInlineACMethodValue(PrintContext printContext, InlineACMethodValue inlineACMethodValue)
        {
            // inline.Text
        }

        public virtual void OnRenderInlineTableCellValue(PrintContext printContext, InlineTableCellValue inlineTableCellValue)
        {
            // inline.Text
        }

        public virtual void OnRenderInlineBarcode(PrintContext printContext, InlineBarcode inlineBarcode)
        {
            //
        }

        public virtual void OnRenderInlineBoolValue(PrintContext printContext, InlineBoolValue inlineBoolValue)
        {
            //
        }
        #endregion

        #endregion

        #endregion
    }
}
