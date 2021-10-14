using gip.core.datamodel;
using gip.core.autocomponent;
using System;
using System.Net.Sockets;
using System.Text;
using gip.core.reporthandler.Flowdoc;
using System.Windows.Documents;

namespace gip.core.reporthandler
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACPrintServerBase'}de{'ACPrintServerBase'}", Global.ACKinds.TACApplicationManager, Global.ACStorableTypes.Required, false, "", false)]
    public class ACPrintServerBase : PAClassAlarmingBase
    {

        #region c´tors
        public const string MN_Print = "Print";

        public ACPrintServerBase(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
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

        [ACPropertyInfo(9999, DefaultValue = "localhost")]
        public string IPAddress
        {
            get;
            set;
        }

        [ACPropertyInfo(9999, DefaultValue = (Int16)502)]
        public Int16 Port
        {
            get;
            set;
        }

        [ACPropertyInfo(9999)]
        public Int32 SendTimeout
        {
            get;
            set;
        }


        [ACPropertyInfo(9999)]
        public Int32 ReceiveTimeout
        {
            get;
            set;
        }

        [ACPropertyInfo(9999)]
        public int PrintTries
        {
            get;
            set;
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
            // Use Queue
            DelegateQueue.Add(() =>
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
                    ReportDocument reportDocument = new ReportDocument(aCClassDesign.XMLDesign);
                    FlowDocument flowDoc = reportDocument.CreateFlowDocument(reportData);
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
            });
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

                WriteToStream(printContext);

                clientStream.Write(printContext.Main,0, printContext.Main.Length);

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
            RenderFlowDocment(printContext, printContext.FlowDocument);
        }

        #endregion

        #region Methods -> Render

        #region Methods -> Render -> Block

        public virtual void RenderFlowDocment(PrintContext printContext, FlowDocument flowDoc)
        {
            RenderBlocks(printContext, flowDoc.Blocks, BlockDocumentPosition.General);
        }

        public virtual void RenderBlocks(PrintContext printContext, BlockCollection blocks, BlockDocumentPosition position)
        {
            foreach (Block block in blocks)
                RenderBlock(printContext, block, position);
        }

        public virtual void RenderBlock(PrintContext printContext, Block block, BlockDocumentPosition position)
        {
            RenderBlockHeader(printContext, block, position);
            if (block is SectionReportHeader)
                RenderSectionReportHeader(printContext, (SectionReportHeader)block);
            else if (block is SectionReportFooter)
                RenderSectionReportFooter(printContext, (SectionReportFooter)block);
            else if (block is SectionDataGroup)
                RenderSectionDataGroup(printContext, (SectionDataGroup)block);
            else if (block is Table)
                RenderSectionTable(printContext, (Table)block);
            else if (block is Paragraph)
                RenderParagraph(printContext, (Paragraph)block);
            RenderBlockFooter(printContext, block, position);
        }

        public virtual void RenderBlockHeader(PrintContext printContext, Block block, BlockDocumentPosition position)
        {
        }

        public virtual void RenderBlockFooter(PrintContext printContext, Block block, BlockDocumentPosition position)
        {
        }

        public virtual void RenderSectionReportHeader(PrintContext printContext, SectionReportHeader sectionReportHeader)
        {
            RenderSectionReportHeaderHeader(printContext, sectionReportHeader);
            RenderBlocks(printContext, sectionReportHeader.Blocks, BlockDocumentPosition.General);
            RenderSectionReportHeaderFooter(printContext, sectionReportHeader);
        }


        public virtual void RenderSectionReportHeaderHeader(PrintContext printContext, SectionReportHeader sectionReportHeader)
        {
            //
        }

        public virtual void RenderSectionReportHeaderFooter(PrintContext printContext, SectionReportHeader sectionReportHeader)
        {
            //
        }

        public virtual void RenderSectionReportFooter(PrintContext printContext, SectionReportFooter sectionReportFooter)
        {
            RenderSectionReportFooterHeader(printContext, sectionReportFooter);
            RenderBlocks(printContext, sectionReportFooter.Blocks, BlockDocumentPosition.General);
            RenderSectionReportFooterFooter(printContext, sectionReportFooter);
        }

        public virtual void RenderSectionReportFooterHeader(PrintContext printContext, SectionReportFooter sectionReportFooter)
        {

        }

        public virtual void RenderSectionReportFooterFooter(PrintContext printContext, SectionReportFooter sectionReportFooter)
        {
            // 
        }

        public virtual void RenderSectionDataGroup(PrintContext printContext, SectionDataGroup sectionDataGroup)
        {
            RenderSectionDataGroupHeader(printContext, sectionDataGroup);
            RenderBlocks(printContext, sectionDataGroup.Blocks, BlockDocumentPosition.General);
            RenderSectionDataGroupFooter(printContext, sectionDataGroup);
        }

        public virtual void RenderSectionDataGroupHeader(PrintContext printContext, SectionDataGroup sectionDataGroup)
        {
            //
        }

        public virtual void RenderSectionDataGroupFooter(PrintContext printContext, SectionDataGroup sectionDataGroup)
        {
            // 
        }
        #endregion

        #region Methods -> Render -> Table

        public virtual void RenderSectionTable(PrintContext printContext, Table table)
        {
            RenderSectionTableHeader(printContext, table);
            foreach (TableColumn tableColumn in table.Columns)
                RenderTableColumn(printContext, tableColumn);

            foreach (TableRowGroup tableRowGroup in table.RowGroups)
                RenderTableRowGroup(printContext, tableRowGroup);
            RenderSectionTableFooter(printContext, table);
        }

        public virtual void RenderSectionTableHeader(PrintContext printContext, Table table)
        {
            //
        }

        public virtual void RenderSectionTableFooter(PrintContext printContext, Table table)
        {
            //
        }


        public virtual void RenderTableColumn(PrintContext printContext, TableColumn tableColumn)
        {

        }

        public virtual void RenderTableRowGroup(PrintContext printContext, TableRowGroup tableRowGroup)
        {
            RenderTableRowGroupHeader(printContext, tableRowGroup);
            foreach (TableRow tableRow in tableRowGroup.Rows)
                RenderTableRow(printContext, tableRow);
            RenderTableRowGroupFooter(printContext, tableRowGroup);
        }

        public virtual void RenderTableRowGroupHeader(PrintContext printContext, TableRowGroup tableRowGroup)
        {
            //
        }

        public virtual void RenderTableRowGroupFooter(PrintContext printContext, TableRowGroup tableRowGroup)
        {
            //
        }


        public virtual void RenderTableRow(PrintContext printContext, TableRow tableRow)
        {
            RenderTableRowHeader(printContext, tableRow);
            foreach (TableCell tableCell in tableRow.Cells)
                RenderTableCell(printContext, tableCell);
            RenderTableRowFooter(printContext, tableRow);
        }
        public virtual void RenderTableRowHeader(PrintContext printContext, TableRow tableRow)
        {
            //
        }

        public virtual void RenderTableRowFooter(PrintContext printContext, TableRow tableRow)
        {
            //
        }

        public virtual void RenderTableCell(PrintContext printContext, TableCell tableCell)
        {
            foreach (Block block in tableCell.Blocks)
                RenderBlock(printContext, block, BlockDocumentPosition.InTable);
        }

        #endregion

        #region Methods -> Render -> Inlines
        public virtual void RenderParagraph(PrintContext printContext, Paragraph paragraph)
        {
            foreach (Inline inline in paragraph.Inlines)
            {
                if (inline is InlineContextValue)
                    RenderInlineContextValue(printContext, (InlineContextValue)inline);
                else if (inline is InlineDocumentValue)
                    RenderInlineDocumentValue(printContext, (InlineDocumentValue)inline);
                else if (inline is InlineACMethodValue)
                    RenderInlineACMethodValue(printContext, (InlineACMethodValue)inline);
                else if (inline is InlineTableCellValue)
                    RenderInlineTableCellValue(printContext, (InlineTableCellValue)inline);
                else if (inline is InlineBarcode)
                    RenderInlineBarcode(printContext, (InlineBarcode)inline);
                else if (inline is InlineBoolValue)
                    RenderInlineBoolValue(printContext, (InlineBoolValue)inline);
            }
        }

        public virtual void RenderInlineContextValue(PrintContext printContext, InlineContextValue inlineContextValue)
        {
            // inline.Text
        }

        public virtual void RenderInlineDocumentValue(PrintContext printContext, InlineDocumentValue inlineDocumentValue)
        {
            // inline.Text
        }

        public virtual void RenderInlineACMethodValue(PrintContext printContext, InlineACMethodValue inlineACMethodValue)
        {
            // inline.Text
        }

        public virtual void RenderInlineTableCellValue(PrintContext printContext, InlineTableCellValue inlineTableCellValue)
        {
            // inline.Text
        }

        public virtual void RenderInlineBarcode(PrintContext printContext, InlineBarcode inlineBarcode)
        {
            //
        }

        public virtual void RenderInlineBoolValue(PrintContext printContext, InlineBoolValue inlineBoolValue)
        {
            //
        }
        #endregion

        #endregion

        #endregion
    }
}
