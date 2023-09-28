using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using gip.core.datamodel;
using System.Reflection;
using System.Linq;
using gip.core.autocomponent;
using System.Xml.Linq;
using gip.core.reporthandler.Configuration;

namespace gip.core.reporthandlerwpf.Flowdoc
{
    public abstract class ReportPaginatorBase : DocumentPaginator, IDisposable
    {
        public abstract void Dispose();
    }

    /// <summary>
    /// Creates all pages of a report
    /// </summary>
    public class ReportPaginator : ReportPaginatorBase
    {
        #region c'tors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="report">report document</param>
        /// <param name="data">report data</param>
        /// <exception cref="ArgumentException">Flow document must have a specified page height</exception>
        /// <exception cref="ArgumentException">Flow document must have a specified page width</exception>
        /// <exception cref="ArgumentException">Flow document can have only one report header section</exception>
        /// <exception cref="ArgumentException">Flow document can have only one report footer section</exception>
        public ReportPaginator(ReportDocument report, ReportData data)
        {
            _report = report;
            _data = data;

            layoutengine.Layoutgenerator.CurrentDataContext = _data?.ReportDocumentValues.Values.Where(c => c is IACComponent).FirstOrDefault() as IACComponent;
            _flowDocument = report.CreateFlowDocument();
            // make height smaller to have enough space for page header and page footer
            _flowDocument.PageHeight = report.PageHeight - report.PageHeight * (report.PageHeaderHeight + report.PageFooterHeight) / 100d;
            _pageSize = new Size(_flowDocument.PageWidth, _flowDocument.PageHeight);

            if (_flowDocument.PageHeight == double.NaN) throw new ArgumentException("Flow document must have a specified page height");
            if (_flowDocument.PageWidth == double.NaN) throw new ArgumentException("Flow document must have a specified page width");

            _rootCache = new ReportPaginatorDynamicCache(_flowDocument);
            List<SectionReportHeader> listPageHeaders = _rootCache.GetObjectsOfType<SectionReportHeader>();
            if (listPageHeaders.Count > 1)
                throw new ArgumentException("Flow document can have only one report header section");
            if (listPageHeaders.Count == 1)
            {
                _blockPageHeader = (SectionReportHeader)listPageHeaders[0];
                if (report.XDoc != null)
                {
                    var queryAttr = report.XDoc.Root.Attributes().Where(c => c.IsNamespaceDeclaration);
                    XElement xElement = report.XDoc.Descendants("{http://www.iplus-framework.com/report/xaml}SectionReportHeader").FirstOrDefault();
                    if (xElement != null)
                    {
                        foreach (XAttribute nsAttribute in queryAttr)
                        {
                            XAttribute xAttrElem = xElement.Attribute(nsAttribute.Name);
                            if (xAttrElem == null)
                                xElement.SetAttributeValue(nsAttribute.Name, nsAttribute.Value);
                        }
                        _blockPageHeaderXAML = xElement.ToString();

                    }
                }
            }
            List<SectionReportFooter> listPageFooters = _rootCache.GetObjectsOfType<SectionReportFooter>();
            if (listPageFooters.Count > 1)
                throw new ArgumentException("Flow document can have only one report footer section");
            if (listPageFooters.Count == 1)
            {
                _blockPageFooter = (SectionReportFooter)listPageFooters[0];
                if (report.XDoc != null)
                {
                    var queryAttr = report.XDoc.Root.Attributes().Where(c => c.IsNamespaceDeclaration);
                    XElement xElement = report.XDoc.Descendants("{http://www.iplus-framework.com/report/xaml}SectionReportFooter").FirstOrDefault();
                    if (xElement != null)
                    {
                        foreach (XAttribute nsAttribute in queryAttr)
                        {
                            XAttribute xAttrElem = xElement.Attribute(nsAttribute.Name);
                            if (xAttrElem == null)
                                xElement.SetAttributeValue(nsAttribute.Name, nsAttribute.Value);
                        }
                        _blockPageFooterXAML = xElement.ToString();
                    }
                }
            }

            _paginator = ((IDocumentPaginatorSource)_flowDocument).DocumentPaginator;

            // remove header and footer in our working copy
            Block block = _flowDocument.Blocks.FirstBlock;
            while (block != null)
            {
                Block thisBlock = block;
                block = block.NextBlock;
                if ((thisBlock == _blockPageHeader) || (thisBlock == _blockPageFooter))
                    _flowDocument.Blocks.Remove(thisBlock);
            }

            // get report context values
            _reportContextValues = _rootCache.GetObjectsOfType<InlineContextValue>();

            ProcessReport();
        }
        #endregion

        #region Properties

        public override void Dispose()
        {
            _flowDocument = null;
            _paginator = null;
            //if (_report != null)
                //_report.Dispose();
            _report = null;
            _data = null;
            _blockPageHeader = null;
            _blockPageHeaderXAML = null;
            _blockPageFooter = null;
            _blockPageFooterXAML = null;
            _reportContextValues = null;
            _rootCache = null;
        }

    #region private members
    /// <summary>
    /// Reference to a original flowdoc paginator
    /// </summary>
    protected DocumentPaginator _paginator = null;

        protected FlowDocument _flowDocument = null;
        public FlowDocument FlowDoc
        {
            get
            {
                return _flowDocument;
            }
        }
        protected ReportDocument _report = null;
        protected ReportData _data = null;
        protected Block _blockPageHeader = null;
        protected string _blockPageHeaderXAML = "";
        protected Block _blockPageFooter = null;
        protected string _blockPageFooterXAML = "";
        protected List<InlineContextValue> _reportContextValues = null;
        protected ReportPaginatorDynamicCache _rootCache = null;
        #endregion

        public ReportData ReportData
        {
            get
            {
                return _data;
            }
        }

        /// <summary>
        /// Determines if the current page count is valid
        /// </summary>
        public override bool IsPageCountValid
        {
            get { return _paginator.IsPageCountValid; }
        }

        private int _pageCount = 0;
        /// <summary>
        /// Gets the total page count
        /// </summary>
        public override int PageCount
        {
            get { return _pageCount; }
        }

        private Size _pageSize = Size.Empty;
        /// <summary>
        /// Gets or sets the page size
        /// </summary>
        public override Size PageSize
        {
            get { return _pageSize; }
            set { _pageSize = value; }
        }

        /// <summary>
        /// Gets the paginator source
        /// </summary>
        public override IDocumentPaginatorSource Source
        {
            get { return _paginator.Source; }
        }

        #endregion

        #region methods

        #region Process methods

        /// <summary>
        /// Fills document with data
        /// </summary>
        /// <exception cref="InvalidDataException">ReportTableRow must have a TableRowGroup as parent</exception>
        protected virtual void ProcessReport()
        {
            Dictionary<string, List<object>> aggregateValues = new Dictionary<string, List<object>>();

            List<Block> blocks = new List<Block>();
            if (_blockPageHeader != null)
                blocks.Add(_blockPageHeader);
            if (_blockPageFooter != null)
                blocks.Add(_blockPageFooter);

            List<InlineDocumentValue> blockDocumentValues = _rootCache.GetObjectsOfType<InlineDocumentValue>(null, true); // walker.Walk<IInlineDocumentValue>(_flowDocument);
            DocumentWalker walker = new DocumentWalker();
            blockDocumentValues.AddRange(walker.TraverseBlockCollection<InlineDocumentValue>(blocks, 0));

            List<InlineBoolValue> blockBoolValues = _rootCache.GetObjectsOfType<InlineBoolValue>(null, true);
            blockBoolValues.AddRange(walker.TraverseBlockCollection<InlineBoolValue>(blocks, 0));

            List<InlineACMethodValue> blockACMethodValues = _rootCache.GetObjectsOfType<InlineACMethodValue>(null, true);
            blockACMethodValues.AddRange(walker.TraverseBlockCollection<InlineACMethodValue>(blocks, 0));

            List<InlineBarcode> blockBarcodeValues = _rootCache.GetObjectsOfType<InlineBarcode>(null, true);
            blockBarcodeValues.AddRange(walker.TraverseBlockCollection<InlineBarcode>(blocks, 0));

            List<InlineFlowDocContentValue> blockFlowDocContentValues = _rootCache.GetObjectsOfType<InlineFlowDocContentValue>(null, true);
            blockFlowDocContentValues.AddRange(walker.TraverseBlockCollection<InlineFlowDocContentValue>(blocks, 0));

            //ArrayList charts = _dynamicCache.GetFlowDocumentVisualListByInterface(typeof(IChart)); // walker.Walk<IChart>(_flowDocument);
            //FillCharts(charts);
            //gip.core.layoutengine.Layoutgenerator.CurrentDataContext = _data.ReportDocumentValues.Values.Where(c => c is IACComponent).FirstOrDefault() as IACComponent;

            // 1. fill report head values
            blockDocumentValues.ForEach(c => SetIPropertyValue(c, aggregateValues, null));
            blockBoolValues.ForEach(c => SetIPropertyValue(c, aggregateValues, null));
            blockACMethodValues.ForEach(c => SetIPropertyValue(c, aggregateValues, null));
            blockBarcodeValues.ForEach(c => SetIPropertyValue(c, aggregateValues, null));
            blockFlowDocContentValues.ForEach(c => SetIPropertyValue(c, aggregateValues, null));

            // 2. fill tables
            List<RPDynCacheEntry> rootEntries = _rootCache.GetEntriesOfType<TableRowDataDyn>(null, true);
            rootEntries.AddRange(_rootCache.GetEntriesOfType<TableRowDataDynHeader>(null, true));
            rootEntries.AddRange(_rootCache.GetEntriesOfType<TableRowData>(null, true));
            if (rootEntries.Any())
                rootEntries.ForEach(c => this.ProcessSubReport(_rootCache, c, null, null, null, null, null));


            // 3. fill aggregate values
            List<InlineAggregateValue> blockAggregateValues = _rootCache.GetObjectsOfType<InlineAggregateValue>(); // walker.Walk<InlineAggregateValue>(_flowDocument);
            foreach (InlineAggregateValue av in blockAggregateValues)
            {
                if (String.IsNullOrEmpty(av.AggregateGroup))
                    continue;
                if (!aggregateValues.ContainsKey(av.AggregateGroup))
                {
                    av.Text = av.EmptyValue;
                }
                else
                {
                    av.Text = av.ComputeAndFormat(aggregateValues);
                }
            }
        }

        /// <summary>
        /// Processes a subreport / Table
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="entry"></param>
        /// <param name="parentDataContext"></param>
        /// <param name="parentDataRow"></param>
        /// <param name="rootDataContext"></param>
        /// <param name="parentACQueryDef"></param>
        /// <param name="cellToFillWithData"></param>
        protected virtual void ProcessSubReport(ReportPaginatorDynamicCache cache, RPDynCacheEntry entry, object parentDataContext, object parentDataRow, object rootDataContext, ACQueryDefinition parentACQueryDef, TableCell cellToFillWithData)
        {
            Dictionary<string, List<object>> aggregateValues = new Dictionary<string, List<object>>();

            if (parentDataRow != null && cellToFillWithData != null)
            {
                // 1. Fill head values of Type IInlineDocumentValue relative to current parentDataContext
                List<IInlinePropertyValue> blockDocumentValues = cache.GetObjectsOfInterface<IInlinePropertyValue>(entry, false);
                blockDocumentValues.ForEach(c => SetIPropertyValue(c, aggregateValues, parentDataRow));

                // 2. Fill head values of Type ITableCellValue relative to current parentDataContext
                //List<ITableCellValue> newCells = cache.GetObjectsOfInterface<ITableCellValue>(entry, false);
                //newCells.ForEach(c => SetIPropertyValue(c as IPropertyValue, aggregateValues, parentDataRow));
            }


            if (parentDataRow != null && cellToFillWithData != null)
            {
                IEnumerable<RPDynCacheEntry> childTableEntries = cache.GetChildEntriesOfParent(entry, true);
                if (childTableEntries == null)
                    return;

                foreach (RPDynCacheEntry childTableEntry in childTableEntries)
                {
                    ACQueryDefinition childACQueryDef = null;
                    IACType propertyType = null;
                    object enumerableObj = GetEnumerable(childTableEntry.CurrentObj as TableRowDataBase, parentDataContext, parentDataRow, ref rootDataContext, parentACQueryDef, out childACQueryDef, out propertyType);
                    if (enumerableObj == null)
                        return;


                    if (childTableEntry.CurrentObj is TableRowDataDyn)
                    {
                        FillTableRowDataDyn(enumerableObj, childTableEntry.CurrentObj as TableRowDataDyn, parentDataContext, rootDataContext, childACQueryDef, propertyType);
                        return;
                    }
                    else if (childTableEntry.CurrentObj is TableRowDataDynHeader)
                    {
                        FillTableRowDataDynHeader(enumerableObj, childTableEntry.CurrentObj as TableRowDataDynHeader, parentDataContext, rootDataContext, childACQueryDef, propertyType);
                    }
                    else if (childTableEntry.CurrentObj is TableRowData)
                    {
                        FillTableRowData(childTableEntry, enumerableObj, parentDataContext, rootDataContext, childACQueryDef, propertyType);
                    }
                }
            }
            else
            {
                ACQueryDefinition childACQueryDef = null;
                IACType propertyType = null;
                object enumerableObj = GetEnumerable(entry.CurrentObj as TableRowDataBase, parentDataContext, parentDataRow, ref rootDataContext, parentACQueryDef, out childACQueryDef, out propertyType);
                if (enumerableObj == null)
                    return;

                if (entry.CurrentObj is TableRowDataDyn)
                {
                    FillTableRowDataDyn(enumerableObj, entry.CurrentObj as TableRowDataDyn, parentDataContext, rootDataContext, childACQueryDef, propertyType);
                    return;
                }
                else if (entry.CurrentObj is TableRowDataDynHeader)
                {
                    FillTableRowDataDynHeader(enumerableObj, entry.CurrentObj as TableRowDataDynHeader, parentDataContext, rootDataContext, childACQueryDef, propertyType);
                }
                else if (entry.CurrentObj is TableRowData)
                {
                    if (((TableRowData)entry.CurrentObj).Configuration)
                    {
                        if (ReportConfig == null)
                            enumerableObj = null;
                        else
                            enumerableObj = FilterConfigItems(enumerableObj);
                    }
                    FillTableRowData(entry, enumerableObj, parentDataContext, rootDataContext, childACQueryDef, propertyType);
                }

            }

            if (parentDataRow != null && cellToFillWithData != null)
            {
                List<InlineAggregateValue> blockAggregateValues = cache.GetObjectsOfType<InlineAggregateValue>(); // walker.Walk<InlineAggregateValue>(_flowDocument);
                foreach (InlineAggregateValue av in blockAggregateValues)
                {
                    if (String.IsNullOrEmpty(av.AggregateGroup))
                        continue;
                    if (!aggregateValues.ContainsKey(av.AggregateGroup))
                    {
                        av.Text = av.EmptyValue;
                    }
                    else
                    {
                        av.Text = av.ComputeAndFormat(aggregateValues);
                    }
                }
            }
        }

        private object FilterConfigItems(object enumerableObj)
        {
            if (ReportConfig == null)
                return null;
            IEnumerable<ReportConfigurationWrapper> configs = null;

            if (enumerableObj is IEnumerable<ReportConfigurationWrapper>)
            {
                configs = enumerableObj as IEnumerable<ReportConfigurationWrapper>;

                foreach (var config in configs)
                {
                    CheckPW(ReportConfig.Items.Cast<ConfigurationMethod>(), config);
                    CheckPAF(ReportConfig.Items.Cast<ConfigurationMethod>(), config);
                    CheckRules(ReportConfig.Items.Cast<ConfigurationMethod>(), config);
                    config.ConfigItems = config.ConfigItems.OrderBy(c => c.LocalConfigACUrl.Split('\\').Last()).ToList();
                }
                configs = configs.Where(c => (c.Rules || c.Method || c.Configuration) && c.ConfigItems.Any());
            }
            else if (enumerableObj is ACProgramLog)
            {
                ACProgramLog log = enumerableObj as ACProgramLog;
                ACMethod method = ACConvert.XMLToObject(typeof(ACMethod), log.XMLConfig, true, Database.GlobalDatabase) as ACMethod;
                if (method != null /*&& method.ParameterValueList is ISafeList*/)
                    method = method.Clone() as ACMethod;
                ACClass acClass = null;

                using (ACMonitor.Lock(Database.GlobalDatabase.QueryLock_1X000))
                    acClass = Database.GlobalDatabase.ACClass.FirstOrDefault(c => c.ACClassID == log.ACClassID);

                if (acClass != null)
                {
                    ConfigurationMethod configMethod = null;

                    var item = acClass.ACClassMethod_PWACClass.Select(c => c.ACUrl).Intersect(ReportConfig.Items.Cast<ConfigurationMethod>().Select(x => x.VBContent));
                    if (item.Count() == 1)
                        configMethod = ReportConfig.Items.Cast<ConfigurationMethod>().FirstOrDefault(c => c.VBContent == item.FirstOrDefault());

                    if (configMethod != null)
                        enumerableObj = method.ParameterValueList.Where(c => configMethod.Items.Cast<ConfigurationParameter>().Any(x => x.ParameterName == c.ACIdentifier));
                }
            }
            else if (enumerableObj is IEnumerable<ACProgramLog>)
            {
                List<ACProgramLog> tempLogs = new List<ACProgramLog>();
                IEnumerable<ACProgramLog> logs = enumerableObj as IEnumerable<ACProgramLog>;
                ACClass acClass = null;
                foreach (var log in logs)
                {

                    using (ACMonitor.Lock(Database.GlobalDatabase.QueryLock_1X000))
                        acClass = Database.GlobalDatabase.ACClass.FirstOrDefault(c => c.ACClassID == log.ACClassID);

                    if (acClass.ACKind == Global.ACKinds.TPWNodeStatic)
                    {
                        if (!(ReportConfig.Items.Cast<ConfigurationMethod>().Select(x => x.VBContent).Any(c => c == acClass.ACUrl + "\\" +nameof(ACClassMethod) + "(" + nameof(PWNodeProcessMethod.SMStarting) +")")))
                        {
                            tempLogs.Add(log);
                        }
                    }
                    else
                    {
                        if (!acClass.ACClassMethod_PWACClass.Select(c => c.ACUrl).Intersect(ReportConfig.Items.Cast<ConfigurationMethod>().Select(x => x.VBContent)).Any())
                        {
                            tempLogs.Add(log);
                        }
                    }
                }
                enumerableObj = logs.Except(tempLogs).OrderBy(c => c.StartDate);
            }

            if (configs != null)
                return configs;
            return enumerableObj;
        }

        private ReportConfiguration _ReportConfig;

        private ReportConfiguration ReportConfig
        {
            get
            {
                if (_ReportConfig == null)
                {
                    if (_flowDocument.Resources.Contains("Config"))
                        _ReportConfig = _flowDocument.Resources["Config"] as ReportConfiguration;
                    if (_ReportConfig == null || _ReportConfig.Items.Count == 0)
                    {
                        ACClassDesign globalReportConfig = null;

                        using (ACMonitor.Lock(Database.GlobalDatabase.QueryLock_1X000))
                            globalReportConfig = Database.GlobalDatabase.ACClassDesign.First(c => c.ACIdentifier == "ReportGlobalConfig");
                        if (globalReportConfig != null)
                        {
                            try
                            {
                                ResourceDictionary rd = XamlReader.Parse(globalReportConfig.XMLDesign) as ResourceDictionary;
                                if (rd.Contains("Config"))
                                    _ReportConfig = rd["Config"] as ReportConfiguration;
                            }
                            catch (Exception e)
                            {
                                _ReportConfig = null;

                                string msg = e.Message;
                                if (e.InnerException != null && e.InnerException.Message != null)
                                    msg += " Inner:" + e.InnerException.Message;

                                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == datamodel.ACInitState.Initialized)
                                    datamodel.Database.Root.Messages.LogException(nameof(ReportPaginator), nameof(ReportConfig), msg);
                            }
                        }
                    }
                }
                return _ReportConfig;
            }
        }

        private void CheckPAF(IEnumerable<ConfigurationMethod> defConfig, ReportConfigurationWrapper configuration)
        {
            ConfigurationMethod configInfo = null;

            var urls = configuration.ConfigACClassWF.PWACClass.ACClassMethod_PWACClass.Select(c => c.ACUrl).Intersect(defConfig.Select(x => x.VBContent));
            if (urls.Count() == 1)
                configInfo = defConfig.FirstOrDefault(c => c.VBContent == urls.FirstOrDefault());

            if (configInfo != null)
            {
                configuration.Method = true;
                List<IACConfig> tempList = new List<IACConfig>();
                foreach (var item in configuration.ConfigItems)
                {
                    if (!item.LocalConfigACUrl.Contains("Rules") && !item.LocalConfigACUrl.Contains(nameof(PWBaseExecutable.SMStarting)) && !configInfo.Items.Cast<ConfigurationParameter>().Any(c => c.ParameterName == item.LocalConfigACUrl.Split('\\').Last()))
                    {
                        tempList.Add(item);
                    }
                }
                tempList.ForEach(c => configuration.ConfigItems.Remove(c));
                tempList = null;
            }
            else
            {
                configuration.Method = false;
                configuration.ConfigItems.RemoveAll(c => !c.LocalConfigACUrl.Contains(nameof(PWBaseExecutable.SMStarting)) && !c.LocalConfigACUrl.Contains("Rules"));
            }
        }

        private void CheckPW(IEnumerable<ConfigurationMethod> defConfig, ReportConfigurationWrapper configuration)
        {
            ACClassMethod method = null;
            if (configuration.ConfigACClassWF.PWACClass.ACClassMethod_ACClass.Any())
            {
                method = configuration.ConfigACClassWF.PWACClass.ACClassMethod_ACClass.FirstOrDefault(c => c.ACIdentifier == nameof(PWBaseExecutable.SMStarting));
                if (method == null)
                    method = configuration.ConfigACClassWF.PWACClass.ACClass1_BasedOnACClass.ACClassMethod_ACClass.FirstOrDefault(c => c.ACIdentifier == nameof(PWBaseExecutable.SMStarting));
            }
            else
            {
                method = configuration.ConfigACClassWF.PWACClass.ACClass1_BasedOnACClass.ACClassMethod_ACClass.FirstOrDefault(c => c.ACIdentifier == nameof(PWBaseExecutable.SMStarting));
            }
            if (method != null)
            {
                ConfigurationMethod configInfo = defConfig.FirstOrDefault(c => c.VBContent == method.GetACUrl());
                if (configInfo != null)
                {
                    configuration.Configuration = true;
                    List<IACConfig> tempList = new List<IACConfig>();
                    foreach (var item in configuration.ConfigItems)
                    {
                        if (item.LocalConfigACUrl.Contains(nameof(PWBaseExecutable.SMStarting)) && !configInfo.Items.Cast<ConfigurationParameter>().Any(c => c.ParameterName == item.LocalConfigACUrl.Split('\\').Last()))
                        {
                            tempList.Add(item);
                        }
                    }
                    tempList.ForEach(c => configuration.ConfigItems.Remove(c));
                    tempList = null;
                }
                else
                {
                    configuration.Configuration = false;
                    configuration.ConfigItems.RemoveAll(c => c.LocalConfigACUrl.Contains(nameof(PWBaseExecutable.SMStarting)));
                }
            }
        }

        private void CheckRules(IEnumerable<ConfigurationMethod> defConfig, ReportConfigurationWrapper configuration)
        {
            ConfigurationMethod configInfo = defConfig.FirstOrDefault(c => c.VBContent == "Rules");
            if (configInfo != null)
            {
                configuration.Rules = true;
                List<IACConfig> tempList = new List<IACConfig>();
                foreach (var item in configuration.ConfigItems)
                {
                    if (item.LocalConfigACUrl.Contains("Rules") && !configInfo.Items.Cast<ConfigurationParameter>().Any(c => c.ParameterName == item.LocalConfigACUrl.Split('\\').Last()))
                    {
                        tempList.Add(item);
                    }
                }
                tempList.ForEach(c => configuration.ConfigItems.Remove(c));
                tempList = null;
            }
            else
            {
                configuration.Rules = false;
                configuration.ConfigItems.RemoveAll(c => c.LocalConfigACUrl.Contains("Rules"));
            }
        }

        #endregion

        #region Filling methods

        /// <summary>
        /// FillTableRowDataDyn
        /// </summary>
        /// <param name="enumerableObj"></param>
        /// <param name="tableRowTemplate"></param>
        /// <param name="acQueryDef"></param>
        protected virtual void FillTableRowDataDyn(object enumerableObj, TableRowDataDyn tableRowTemplate, object parentDataContext, object rootDataContext, ACQueryDefinition acQueryDef, IACType propertyType)
        {
            TableRowGroup tableGroup = tableRowTemplate.Parent as TableRowGroup;
            if (tableGroup == null)
                return;

            var stackComponent = gip.core.layoutengine.Layoutgenerator.CurrentACComponent;

            try
            {
                IACComponent acComponent = parentDataContext as IACComponent;
                if (acComponent == null)
                    acComponent = rootDataContext as IACComponent;
                if (acComponent != null)
                    gip.core.layoutengine.Layoutgenerator.CurrentDataContext = acComponent;

                if (enumerableObj is DataTable)
                {
                    DataTable table = enumerableObj as DataTable;
                    if (table == null)
                        return;
                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        TableRow currentRow = new TableRow();

                        DataRow dataRow = table.Rows[i];
                        for (int j = 0; j < table.Columns.Count; j++)
                        {
                            object value = dataRow[j];
                            TableCell cell = OnSetNewTableCell(acComponent, propertyType, dataRow, tableRowTemplate, value, dataRow[0].ToString(), tableRowTemplate);
                            currentRow.Cells.Add(cell);
                        }
                        tableGroup.Rows.Add(currentRow);

                        //foreach (RPDynCacheEntry childEntry in childTableEntries)
                        //{
                        //    FillData(childEntry, table, dataRow, rootDataContext, null);
                        //}
                    }
                    return;
                }
                else if (enumerableObj is IEnumerable)
                {
                    FieldInfo[] fields = null;
                    Type rowType = null;
                    IEnumerable<ACClassProperty> acPropertyList = null;
                    int i = 0;
                    foreach (object dataRow in enumerableObj as IEnumerable)
                    {
                        TableRow currentRow = new TableRow();
                        if (acQueryDef != null)
                        {
                            foreach (ACColumnItem acColumnItem in acQueryDef.ACColumns)
                            {
                                object fieldValue = dataRow.GetValue(acColumnItem.ACIdentifier);
                                TableCell cell = OnSetNewTableCell(acComponent, propertyType, dataRow, currentRow, fieldValue, acColumnItem.ACIdentifier, tableRowTemplate);
                                currentRow.Cells.Add(cell);
                            }
                        }
                        else if (dataRow is IACObject)
                        {
                            LoadCurrentProperty(acComponent, propertyType, dataRow, currentRow);
                            if (acPropertyList == null)
                            {
                                ACClass typeAsACClass = (dataRow as IACObject).ACType as ACClass;
                                if (typeAsACClass != null)
                                    acPropertyList = typeAsACClass.Properties;
                            }
                            if (acPropertyList != null)
                            {
                                foreach (ACClassProperty acProperty in acPropertyList)
                                {
                                    object fieldValue = dataRow.GetValue(acProperty.ACIdentifier);
                                    TableCell cell = OnSetNewTableCell(acComponent, propertyType, dataRow, currentRow, fieldValue, acProperty.ACIdentifier, tableRowTemplate);
                                    currentRow.Cells.Add(cell);
                                }
                            }
                        }
                        else
                        {
                            LoadCurrentProperty(acComponent, propertyType, dataRow, currentRow);
                            if (rowType == null)
                            {
                                rowType = dataRow.GetType();
                                fields = rowType.GetFields();
                            }
                            foreach (FieldInfo field in fields)
                            {
                                object fieldValue = dataRow.GetValue(field.Name);
                                TableCell cell = OnSetNewTableCell(acComponent, propertyType, dataRow, currentRow, fieldValue, field.Name, tableRowTemplate);
                                currentRow.Cells.Add(cell);
                            }
                        }
                        tableGroup.Rows.Add(currentRow);
                        i++;
                        //foreach (RPDynCacheEntry childEntry in childTableEntries)
                        //{
                        //    FillData(childEntry, enumerableObj, dataRow, rootDataContext, acQueryDef);
                        //}
                    }
                }
                else
                    return;
            }
            finally
            {
                gip.core.layoutengine.Layoutgenerator.CurrentDataContext = stackComponent;
            }
        }

        /// <summary>
        /// FillTableRowDataDynHeader
        /// </summary>
        /// <param name="enumerableObj"></param>
        /// <param name="tableRowTemplate"></param>
        /// <param name="acQueryDef"></param>
        protected virtual void FillTableRowDataDynHeader(object enumerableObj, TableRowDataDynHeader tableRowTemplate, object parentDataContext, object rootDataContext, ACQueryDefinition acQueryDef, IACType propertyType)
        {
            TableRowGroup tableGroup = tableRowTemplate.Parent as TableRowGroup;
            if (tableGroup == null)
                return;

            Table parentTable = tableGroup.Parent as Table;
            if (parentTable == null)
                return;
            bool noColDefined = false;
            if (parentTable.Columns == null || parentTable.Columns.Count <= 0)
                noColDefined = true;

            var stackComponent = gip.core.layoutengine.Layoutgenerator.CurrentACComponent;

            try
            {
                IACComponent acComponent = parentDataContext as IACComponent;
                if (acComponent == null)
                    acComponent = rootDataContext as IACComponent;
                if (acComponent != null)
                    gip.core.layoutengine.Layoutgenerator.CurrentDataContext = acComponent;

                if (enumerableObj is DataTable)
                {
                    DataTable table = enumerableObj as DataTable;
                    if (table == null)
                        return;
                    if (table.Columns.Count > 0)
                    {
                        int index = 0;
                        foreach (DataColumn col in table.Columns)
                        {
                            if (noColDefined && tableRowTemplate.GetColumnWidth(0) > 0)
                            {
                                if (parentTable.Columns != null)
                                {
                                    TableColumn tableCol = new TableColumn();
                                    parentTable.Columns.Add(tableCol);
                                    double width = tableRowTemplate.GetColumnWidth(index);
                                    if (width > 0)
                                        tableCol.Width = new GridLength(width);
                                }
                            }
                            string value = col.ColumnName;
                            TableCell cell = OnSetNewTableCell(acComponent, propertyType, null, tableRowTemplate, value, "ColumnHeader", tableRowTemplate);
                            tableRowTemplate.Cells.Add(cell);
                            index++;
                        }
                    }
                    else
                    {
                        foreach (DataRow row in table.Rows)
                        {
                            object value = row[0];
                            TableCell cell = OnSetNewTableCell(acComponent, propertyType, row, tableRowTemplate, value, "ColumnHeader", tableRowTemplate);
                            tableRowTemplate.Cells.Add(cell);
                        }
                    }
                    return;
                }
                else if (enumerableObj is IEnumerable)
                {
                    FieldInfo[] fields = null;
                    Type rowType = null;
                    IEnumerable<ACClassProperty> acPropertyList = null;
                    int i = 0;
                    foreach (object dataRow in enumerableObj as IEnumerable)
                    {
                        LoadCurrentProperty(acComponent, propertyType, dataRow, tableRowTemplate);

                        TableRow currentRow = new TableRow();
                        if (acQueryDef != null)
                        {
                            foreach (ACColumnItem acColumnItem in acQueryDef.ACColumns)
                            {
                                TableCell cell = OnSetNewTableCell(acComponent, propertyType, dataRow, tableRowTemplate, acColumnItem.ACIdentifier, "ColumnHeader", tableRowTemplate);
                                tableRowTemplate.Cells.Add(cell);
                                break;
                            }
                        }
                        else if (dataRow is IACObject)
                        {
                            LoadCurrentProperty(acComponent, propertyType, dataRow, tableRowTemplate);
                            if (acPropertyList == null)
                            {
                                ACClass typeAsACClass = (dataRow as IACObject).ACType as ACClass;
                                if (typeAsACClass != null)
                                    acPropertyList = typeAsACClass.Properties;
                            }
                            if (acPropertyList != null)
                            {
                                foreach (ACClassProperty acProperty in acPropertyList)
                                {
                                    TableCell cell = OnSetNewTableCell(acComponent, propertyType, dataRow, tableRowTemplate, acProperty.ACIdentifier, "ColumnHeader", tableRowTemplate);
                                    tableRowTemplate.Cells.Add(cell);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            LoadCurrentProperty(acComponent, propertyType, dataRow, tableRowTemplate);
                            if (rowType == null)
                            {
                                rowType = dataRow.GetType();
                                fields = rowType.GetFields();
                            }
                            foreach (FieldInfo field in fields)
                            {
                                TableCell cell = OnSetNewTableCell(acComponent, propertyType, dataRow, tableRowTemplate, field.Name, "ColumnHeader", tableRowTemplate);
                                tableRowTemplate.Cells.Add(cell);
                                break;
                            }
                        }
                        tableGroup.Rows.Add(currentRow);
                        i++;

                        //foreach (RPDynCacheEntry childEntry in childTableEntries)
                        //{
                        //    FillData(childEntry, enumerableObj, dataRow, rootDataContext, acQueryDef);
                        //}
                    }
                }
                else
                    return;
            }
            finally
            {
                gip.core.layoutengine.Layoutgenerator.CurrentDataContext = stackComponent;
            }
        }

        /// <summary>
        /// FillTableRowData
        /// </summary>
        /// <param name="childTableEntry"></param>
        /// <param name="enumerableObj"></param>
        /// <param name="rootDataContext"></param>
        /// <param name="acQueryDef"></param>
        protected virtual void FillTableRowData(RPDynCacheEntry childTableEntry, object enumerableObj, object parentDataContext, object rootDataContext, ACQueryDefinition acQueryDef, IACType propertyType, bool configuration = false)
        {
            TableRowDataBase iTableRow = childTableEntry.CurrentObj as TableRowDataBase;
            TableRow tableRow = iTableRow as TableRow;
            //if (tableRow == null
            //    || (parentDataContext == null && String.IsNullOrEmpty(iTableRow.DictKey))
            //    || (parentDataContext != null && String.IsNullOrEmpty(iTableRow.DictKey) && String.IsNullOrEmpty(iTableRow.VBSource)))
            //    return;

            if (tableRow == null)
                return;
            TableRowGroup rowGroup = tableRow.Parent as TableRowGroup;
            if (rowGroup == null)
                throw new InvalidDataException("ReportTableRow must have a TableRowGroup as parent");

            var stackComponent = gip.core.layoutengine.Layoutgenerator.CurrentACComponent;

            try
            {
                IACComponent acComponent = parentDataContext as IACComponent;
                if (acComponent == null)
                    acComponent = rootDataContext as IACComponent;
                if (acComponent != null)
                    gip.core.layoutengine.Layoutgenerator.CurrentDataContext = acComponent;

                string tableAsXaml = null;
                Table parentTable = null;
                Section parentSection = null;
                if (iTableRow.BreakPageBeforeNextRow)
                {
                    parentTable = rowGroup.Parent as Table;
                    if (parentTable == null)
                        throw new InvalidDataException("TableRowGroup must have a Table as parent");
                    tableAsXaml = XamlWriter.Save(parentTable);
                    parentSection = parentTable.Parent as Section;
                    if (parentSection == null)
                        throw new InvalidDataException("Table must have a Section as parent");
                }

                // If BreakPageBeforeNextRow ist set an collection contains more than one item, then clone table
                bool tableIsCloned = false;

                List<Table> clonedTables = new List<Table>();
                List<TableRow> clonedTableRows = new List<TableRow>();
                foreach (TableRow row in rowGroup.Rows)
                {
                    TableRowData reportTableRow = row as TableRowData;
                    if (reportTableRow == null)
                    {
                        // clone regular row
                        clonedTableRows.Add(XamlHelper.CloneTableRow(row));
                    }
                    else
                    {
                        string reportTableRowXaml = XamlWriter.Save(reportTableRow);

                        // clone ReportTableRows
                        DataTable dataTable = enumerableObj as DataTable;
                        IEnumerable iEnumerable = enumerableObj as IEnumerable;
                        int countRows = 0;
                        if (dataTable != null)
                        {
                            if (iTableRow.BreakPageBeforeNextRow && dataTable.Rows.Count > 1)
                            {
                                tableIsCloned = true;
                                foreach (DataRow dataRow in dataTable.Rows)
                                {
                                    clonedTables.Add((Table)XamlHelper.LoadXamlFromString(tableAsXaml));
                                    countRows++;
                                }
                            }
                            else
                            {
                                foreach (DataRow dataRow in dataTable.Rows)
                                {
                                    clonedTableRows.Add((TableRow)XamlHelper.LoadXamlFromString(reportTableRowXaml));
                                    countRows++;
                                }
                            }

                            int i = 0;
                            if (tableIsCloned)
                            {
                                foreach (DataRow dataRow in dataTable.Rows)
                                {
                                    Table clonedTable = clonedTables[i];
                                    if (i > 0)
                                        clonedTable.BreakPageBefore = true;
                                    TableRowGroup clonedGroup = clonedTable.RowGroups.FirstOrDefault();
                                    TableRow clonedTableRow = clonedGroup.Rows.FirstOrDefault();

                                    LoadCurrentProperty(acComponent, propertyType, dataRow, clonedTableRow);

                                    foreach (TableCell cellToFillWithData in clonedTableRow.Cells)
                                    {
                                        ReportPaginatorDynamicCache cache = new ReportPaginatorDynamicCache(clonedTableRow, cellToFillWithData);
                                        ProcessSubReport(cache, cache.RootTableRowCacheEntry, enumerableObj, dataRow, rootDataContext, acQueryDef, cellToFillWithData);
                                    }
                                    i++;
                                }
                            }
                            else
                            {
                                foreach (DataRow dataRow in dataTable.Rows)
                                {
                                    LoadCurrentProperty(acComponent, propertyType, dataRow, tableRow);

                                    TableRow clonedTableRow = clonedTableRows[i];
                                    foreach (TableCell cellToFillWithData in clonedTableRow.Cells)
                                    {
                                        ReportPaginatorDynamicCache cache = new ReportPaginatorDynamicCache(clonedTableRow, cellToFillWithData);
                                        ProcessSubReport(cache, cache.RootTableRowCacheEntry, enumerableObj, dataRow, rootDataContext, acQueryDef, cellToFillWithData);
                                    }
                                    i++;
                                }
                            }
                        }
                        else if (iEnumerable != null)
                        {
                            foreach (object dataRow in iEnumerable)
                            {
                                countRows++;
                            }

                            if (iTableRow.BreakPageBeforeNextRow && countRows > 1)
                            {
                                tableIsCloned = true;
                                foreach (object dataRow in iEnumerable)
                                {
                                    clonedTables.Add((Table)XamlHelper.LoadXamlFromString(tableAsXaml));
                                }
                            }
                            else
                            {
                                foreach (object dataRow in iEnumerable)
                                {
                                    clonedTableRows.Add((TableRow)XamlHelper.LoadXamlFromString(reportTableRowXaml));
                                }
                            }

                            int i = 0;
                            if (tableIsCloned)
                            {
                                foreach (object dataRow in iEnumerable)
                                {
                                    Table clonedTable = clonedTables[i];
                                    if (i > 0)
                                        clonedTable.BreakPageBefore = true;
                                    TableRowGroup clonedGroup = clonedTable.RowGroups.FirstOrDefault();
                                    TableRow clonedTableRow = clonedGroup.Rows.FirstOrDefault();

                                    LoadCurrentProperty(acComponent, propertyType, dataRow, clonedTableRow);

                                    foreach (TableCell cellToFillWithData in clonedTableRow.Cells)
                                    {
                                        ReportPaginatorDynamicCache cache = new ReportPaginatorDynamicCache(clonedTableRow, cellToFillWithData);
                                        ProcessSubReport(cache, cache.RootTableRowCacheEntry, enumerableObj, dataRow, rootDataContext, acQueryDef, cellToFillWithData);
                                    }
                                    i++;
                                }
                            }
                            else
                            {
                                foreach (object dataRow in iEnumerable)
                                {
                                    LoadCurrentProperty(acComponent, propertyType, dataRow, tableRow);

                                    // get cloned ReportTableRow
                                    TableRow clonedTableRow = clonedTableRows[i];
                                    foreach (TableCell cellToFillWithData in clonedTableRow.Cells)
                                    {
                                        ReportPaginatorDynamicCache cache = new ReportPaginatorDynamicCache(clonedTableRow, cellToFillWithData);
                                        ProcessSubReport(cache, cache.RootTableRowCacheEntry, enumerableObj, dataRow, rootDataContext, acQueryDef, cellToFillWithData);
                                    }
                                    i++;
                                }
                            }
                        }
                        else
                            continue;
                    }
                }

                if (tableIsCloned)
                {
                    parentSection.Blocks.Remove(parentTable);
                    foreach (Table clonedTable in clonedTables)
                    {
                        if (clonedTable.BreakPageBefore)
                        {
                            // Paragraph needed beacuse of bug in DocumentPaginator, who ignores the BreakPageBefore-Property of a Table
                            Paragraph pageBreakP = new Paragraph();
                            pageBreakP.BreakPageBefore = true;
                            parentSection.Blocks.Add(pageBreakP);
                        }
                        parentSection.Blocks.Add(clonedTable);
                    }
                }
                else
                {
                    rowGroup.Rows.Clear();
                    foreach (TableRow row in clonedTableRows)
                    {
                        rowGroup.Rows.Add(row);
                    }
                }
            }
            finally
            {
                gip.core.layoutengine.Layoutgenerator.CurrentDataContext = stackComponent;
            }
        }

        /// <summary>
        /// Fill charts with data
        /// </summary>
        /// <param name="charts">list of charts</param>
        /// <exception cref="TimeoutException">Thread for drawing charts timed out</exception>
        protected virtual void FillCharts(ArrayList charts)
        {
            Window window = null;

            // fill charts
            foreach (IChart chart in charts)
            {
                if (chart == null) continue;
                Canvas chartCanvas = chart as Canvas;
                if (String.IsNullOrEmpty(chart.TableName)) continue;
                if (String.IsNullOrEmpty(chart.TableColumns)) continue;

                DataTable table = _data.GetValue<DataTable>(chart.TableName);
                if (table == null) continue;

                if (chartCanvas != null)
                {
                    // HACK: this here is REALLY dirty!!!
                    IChart newChart = (IChart)chart.Clone();
                    if (window == null)
                    {
                        window = new Window();
                        window.WindowStyle = WindowStyle.None;
                        window.BorderThickness = new Thickness(0);
                        window.ShowInTaskbar = false;
                        window.Left = 30000;
                        window.Top = 30000;
                        window.Show();
                    }
                    window.Width = chartCanvas.Width + 2 * SystemParameters.BorderWidth;
                    window.Height = chartCanvas.Height + 2 * SystemParameters.BorderWidth;
                    window.Content = newChart;

                    newChart.DataColumns = null;

                    newChart.DataView = table.DefaultView;
                    newChart.DataColumns = chart.TableColumns.Split(',', ';');
                    newChart.UpdateChart();

                    RenderTargetBitmap bitmap = new RenderTargetBitmap((int)((window.Content as FrameworkElement).RenderSize.Width * 600d / 96d), (int)((window.Content as FrameworkElement).RenderSize.Height * 600d / 96d), 600d, 600d, PixelFormats.Pbgra32);
                    bitmap.Render(window);
                    chartCanvas.Children.Add(new Image() { Source = bitmap });
                }
                else
                {
                    chart.DataColumns = null;

                    chart.DataView = table.DefaultView;
                    chart.DataColumns = chart.TableColumns.Split(',', ';');
                    chart.UpdateChart();
                }
            }

            if (window != null) window.Close();
        }

        #endregion

        #region protected methods

        /// <summary>
        /// SetIPropertyValue
        /// </summary>
        /// <param name="dv"></param>
        /// <param name="aggregateValues"></param>
        /// <param name="parentDataRow"></param>
        protected void SetIPropertyValue(IInlinePropertyValue dv, Dictionary<string, List<object>> aggregateValues, object parentDataRow)
        {
            if (dv == null)
                return;
            IAggregateValue av = dv as IAggregateValue;

            if (parentDataRow != null)
            {
                if (String.IsNullOrEmpty(dv.VBContent))
                {
                    if (dv is InlineTableCellConfigurationValue)
                        if (SetConfigurationValue(dv, aggregateValues, parentDataRow))
                            return;

                    if (_data.ShowUnknownValues)
                        OnSetFlowDocObjValue(dv, aggregateValues, parentDataRow, "[" + dv.VBContent + "]");
                    return;
                }
                try
                {
                    if (parentDataRow is DataRow)
                    {
                        DataRow dataRow = parentDataRow as DataRow;
                        object obj = dataRow[dv.VBContent];
                        if (obj == DBNull.Value)
                            obj = null;

                        if (dv is InlineFlowDocContentValue)
                        {
                            SetInlineFlowDocContentValue(dv as InlineFlowDocContentValue, null, parentDataRow, obj);
                        }

                        OnSetFlowDocObjValue(dv, aggregateValues, parentDataRow, obj);
                    }
                    else
                    {
                        if (dv is InlineFlowDocContentValue)
                        {
                            SetInlineFlowDocContentValue(dv as InlineFlowDocContentValue, null, parentDataRow, parentDataRow.GetValue(dv.VBContent));
                        }

                        OnSetFlowDocObjValue(dv, aggregateValues, parentDataRow, parentDataRow.GetValue(dv.VBContent));
                    }
                    if (av != null)
                        RememberAggregateValue(aggregateValues, av.AggregateGroup, dv.Value);
                }
                catch (Exception e)
                {
                    if (_data.ShowUnknownValues)
                        OnSetFlowDocObjValue(dv, aggregateValues, parentDataRow, "[" + dv.DictKey + "]");
                    else
                        OnSetFlowDocObjValue(dv, aggregateValues, parentDataRow, "");
                    if (av != null)
                        RememberAggregateValue(aggregateValues, av.AggregateGroup, null);

                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == datamodel.ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("ReportPaginator", "SetIPropertyValue", msg);
                }
            }
            else
            {
                object obj = null;
                if (!String.IsNullOrEmpty(dv.DictKey))
                    obj = _data.GetValue<object>(dv.DictKey);
                else if (_data.ReportDocumentValues.Any())
                    obj = _data.ReportDocumentValues.FirstOrDefault().Value;

                if (obj != null)
                {
                    InlineFlowDocContentValue docValue = dv as InlineFlowDocContentValue;

                    if (dv is InlineACMethodValue)
                    {
                        if (!SetACMethodConfigValue(dv, aggregateValues, parentDataRow, obj))
                            return;
                    }
                    else if (docValue != null)
                    {
                        if (!SetInlineFlowDocContentValue(docValue, aggregateValues, parentDataRow, obj.GetValue(dv.VBContent)))
                            return;
                    }
                    else
                        OnSetFlowDocObjValue(dv, aggregateValues, parentDataRow, obj.GetValue(dv.VBContent));
                    if (av != null)
                        RememberAggregateValue(aggregateValues, av.AggregateGroup, dv.Value);
                    return;
                }
                else if ((_data.ShowUnknownValues) && (dv.Value == null))
                    OnSetFlowDocObjValue(dv, aggregateValues, parentDataRow, "[" + ((dv.DictKey != null) ? dv.DictKey : "NULL") + "]");
                if (av != null)
                    RememberAggregateValue(aggregateValues, av.AggregateGroup, null);
            }
        }

        protected bool SetConfigurationValue(IInlinePropertyValue dv, Dictionary<string, List<object>> aggregateValues, object parentDataRow)
        {
            InlineTableCellConfigurationValue tableConfig = dv as InlineTableCellConfigurationValue;
            if (tableConfig.ParameterNameIndex >= 0)
            {
                if (ReportConfig == null)
                    return false;

                ConfigurationMethod configInfo = null;
                ACMethod method = null;
                if (parentDataRow is ACProgramLog)
                {
                    method = ((ACProgramLog)parentDataRow).Value;
                    ACClass acClass = null;

                    using (ACMonitor.Lock(Database.GlobalDatabase.QueryLock_1X000))
                        acClass = Database.GlobalDatabase.ACClass.FirstOrDefault(c => c.ACClassID == ((ACProgramLog)parentDataRow).ACClassID);

                    if (acClass != null)
                    {

                        if (acClass.ACKind == Global.ACKinds.TPWNodeStatic)
                        {
                            var items = ReportConfig.Items.Cast<ConfigurationMethod>().Where(c => c.VBContent == acClass.ACUrl + "\\" + nameof(ACClassMethod) + "(" + nameof(PWNodeProcessMethod.SMStarting) + ")");
                            if (items.Count() == 1)
                                configInfo = items.FirstOrDefault();

                        }
                        else
                        {

                            var item = acClass.ACClassMethod_PWACClass.Select(c => c.ACUrl).Intersect(ReportConfig.Items.Cast<ConfigurationMethod>().Select(x => x.VBContent));
                            if (item.Count() == 1)
                                configInfo = ReportConfig.Items.Cast<ConfigurationMethod>().FirstOrDefault(c => c.VBContent == item.FirstOrDefault());
                        }
                    }
                }

                if (method != null && configInfo != null && configInfo.Items.Count > tableConfig.ParameterNameIndex)
                {
                    ConfigurationParameter paramInfo = configInfo.Items[tableConfig.ParameterNameIndex] as ConfigurationParameter;
                    ACValue value = method.ParameterValueList.FirstOrDefault(c => c.ACIdentifier == paramInfo.ParameterName);
                    if (value == null)
                        value = method.ResultValueList.FirstOrDefault(c => c.ACIdentifier == paramInfo.ParameterName);
                    if (value == null)
                        return false;
                    string sValue = null;
                    if (value.ObjectType == typeof(double))
                        sValue = string.Format("{0}{1:N}", value.ACCaption + ": " + System.Environment.NewLine, value.Value);
                    else
                        sValue = value.ACCaption + ": " + System.Environment.NewLine + value.Value;
                    OnSetFlowDocObjValue(dv, aggregateValues, parentDataRow, sValue);
                    return true;
                }
            }
            return false;
        }

        protected bool SetACMethodConfigValue(IInlinePropertyValue dv, Dictionary<string, List<object>> aggregateValues, object parentDataRow, object objValue)
        {
            InlineACMethodValue inlineACMethodValue = dv as InlineACMethodValue;
            if (ReportConfig == null)
                return false;

            var configObj = objValue.GetValue(dv.VBContent);
            if (configObj == null)
                return false;

            ConfigurationMethod configInfo = null;
            ACMethod method = null;
            if (configObj is ACProgramLog)
            {
                method = ((ACProgramLog)configObj).Value;
                ACClass acClass = null;

                using (ACMonitor.Lock(Database.GlobalDatabase.QueryLock_1X000))
                    acClass = Database.GlobalDatabase.ACClass.FirstOrDefault(c => c.ACClassID == ((ACProgramLog)configObj).ACClassID);

                if (acClass != null)
                {
                    var item = acClass.ACClassMethod_PWACClass.Select(c => c.ACUrl).Intersect(ReportConfig.Items.Cast<ConfigurationMethod>().Select(x => x.VBContent));
                    if (item.Count() == 1)
                        configInfo = ReportConfig.Items.Cast<ConfigurationMethod>().FirstOrDefault(c => c.VBContent == item.FirstOrDefault());
                }
            }

            if (method != null && configInfo != null && configInfo.Items.Count > inlineACMethodValue.ParameterNameIndex - 1)
            {
                ConfigurationParameter paramInfo = configInfo.Items[inlineACMethodValue.ParameterNameIndex] as ConfigurationParameter;
                ACValue value = method.ParameterValueList.FirstOrDefault(c => c.ACIdentifier == paramInfo.ParameterName);
                OnSetFlowDocObjValue(dv, aggregateValues, parentDataRow, value.ACCaption + ": " + value.Value);
                return true;
            }
            return false;
        }

        protected bool SetInlineFlowDocContentValue(InlineFlowDocContentValue dv, Dictionary<string, List<object>> aggregateValues, object parentDataRow, object objValue)
        {
            if (dv == null || objValue == null)
                return false;

            SectionDataGroup sdg = (dv.Parent as Block)?.Parent as SectionDataGroup;
            TableCell tc = null;
            if (sdg == null)
            {
                if (parentDataRow == null)
                    return false;

                tc = (dv.Parent as Block)?.Parent as TableCell;

                if (tc == null)
                {
                    string error = "The parent object of the InlineFlowDocValue must be Paragraph in a SectionDataGroup or TableCell!";
                    MessageBox.Show(error);
                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == datamodel.ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogError("ReportPaginator", "SetInlineFlowDocContentValue(10)", error);
                    return false;
                }
            }

            try
            {
                List<Block> blocks = null;
                try
                {

                    FlowDocument fd = XamlHelper.LoadXamlFromString(objValue.ToString()) as FlowDocument;
                    blocks = fd.Blocks.ToList();
                }
                catch (Exception ex)
                {
                    string error = "Set InlineFlowDocContenValue error: " + ex.Message;
                    MessageBox.Show(error);
                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == datamodel.ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogError("ReportPaginator", "SetInlineFlowDocContentValue(15s)", error);
                }

                if (sdg != null)
                {
                    sdg.Blocks.Clear();
                    if (blocks != null && blocks.Any())
                        sdg.Blocks.AddRange(blocks);
                }
                else if (tc != null)
                {
                    tc.Blocks.Clear();

                    bool addBlocks = true;
                    if (!blocks.Any())
                    {
                        addBlocks = false;
                    }
                    else if (blocks.Count == 1)
                    {
                        Paragraph paragraph = blocks.FirstOrDefault() as Paragraph;
                        if (paragraph != null)
                        {
                            if (!paragraph.Inlines.Any())
                                addBlocks = false;
                        }
                    }


                    if (blocks != null && addBlocks)
                        tc.Blocks.AddRange(blocks);
                    else
                    {
                        TableRow tr = tc.Parent as TableRow;
                        if (tr != null)
                        {
                            tr.Cells.Clear();
                            TableRowGroup trg = tr.Parent as TableRowGroup;
                            if (trg != null)
                            {
                                trg.Rows.Remove(tr);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                string error = "Set InlineFlowDocContenValue error: " + e.Message;
                MessageBox.Show(error);
                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == datamodel.ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogError(nameof(ReportPaginator), "SetInlineFlowDocContentValue(20)", error);
                return false;
            }

            return true;
        }

        /// <summary>
        /// GetEnumerable
        /// </summary>
        /// <param name="iTableRow"></param>
        /// <param name="parentDataContext"></param>
        /// <param name="rootDataContext"></param>
        /// <param name="parentACQueryDef"></param>
        /// <param name="childACQueryDef"></param>
        /// <returns></returns>
        protected object GetEnumerable(TableRowDataBase iTableRow, object parentDataContext, object parentDataRow, ref object rootDataContext, ACQueryDefinition parentACQueryDef, out ACQueryDefinition childACQueryDef, out IACType propertyType)
        {
            childACQueryDef = null;
            propertyType = null;
            object enumerableObj = null;

            if (iTableRow == null
                || (parentDataContext == null && String.IsNullOrEmpty(iTableRow.DictKey) && String.IsNullOrEmpty(iTableRow.VBSource))
                || (parentDataContext != null && String.IsNullOrEmpty(iTableRow.DictKey) && String.IsNullOrEmpty(iTableRow.VBSource)))
                return null;

            if (parentDataContext != null)
            {
                if (parentACQueryDef != null)
                {
                    IACObject parentIACObject = parentDataContext as IACObject;
                    if (parentIACObject == null || parentACQueryDef.ACObjectChilds == null)
                        return null;
                    if (!String.IsNullOrEmpty(iTableRow.VBSource))
                    {
                        childACQueryDef = parentACQueryDef.ACQueryDefinitionChilds.Where(c => c.ACIdentifier == iTableRow.VBSource).FirstOrDefault();
                        if (childACQueryDef == null)
                            return null;
                        enumerableObj = parentIACObject.ACSelect(childACQueryDef);
                    }
                    else if (!String.IsNullOrEmpty(iTableRow.DictKey))
                    {
                        object dataContext = _data.GetValue<object>(iTableRow.DictKey);
                        if (!(dataContext is ACQueryDefinition))
                            return null;
                        childACQueryDef = dataContext as ACQueryDefinition;
                        enumerableObj = _data.Database.ACSelect(childACQueryDef, _data.Database.RecommendedMergeOption);
                    }
                    else
                    {
                        childACQueryDef = parentACQueryDef.ACQueryDefinitionChilds.FirstOrDefault();
                        if (childACQueryDef == null)
                            return null;
                        enumerableObj = parentIACObject.ACSelect(childACQueryDef);
                    }
                }
                else
                {
                    object parentDataContext2 = parentDataContext;
                    if (parentDataContext is IEnumerable && parentDataRow != null && !(parentDataRow is IEnumerable))
                        parentDataContext2 = parentDataRow;

                    if (parentDataContext2 is IACObject)
                    {
                        IACObject acObject = parentDataContext2 as IACObject;
                        if (!String.IsNullOrEmpty(iTableRow.VBSource))
                        {
                            //enumerableObj = acObject.ACUrlCommand(iTableRow.VBSource) as IEnumerable;
                            string path = "";
                            Global.ControlModes controlMode = Global.ControlModes.Enabled;
                            if (acObject.ACUrlBinding(iTableRow.VBSource, ref propertyType, ref enumerableObj, ref path, ref controlMode))
                            {
                                enumerableObj = acObject.ACUrlCommand(iTableRow.VBSource);
                                return enumerableObj;
                            }
                            else
                            {
                                enumerableObj = null;
                            }
                        }
                        else if (!String.IsNullOrEmpty(iTableRow.DictKey))
                            enumerableObj = acObject.ACUrlCommand(iTableRow.DictKey) as IEnumerable;
                    }
                    if (enumerableObj == null)
                    {
                        if (parentDataContext is DataTable)
                        {
                            if (String.IsNullOrEmpty(iTableRow.DictKey))
                                return null;
                            enumerableObj = _data.GetValue<object>(iTableRow.DictKey);
                        }
                        else if (!String.IsNullOrEmpty(iTableRow.DictKey) && !String.IsNullOrEmpty(iTableRow.VBSource))
                        {
                            enumerableObj = _data.GetValue<object>(iTableRow.DictKey);
                            if (enumerableObj != null)
                            {
                                IACObject acObject = enumerableObj as IACObject;
                                if (acObject != null)
                                {
                                    enumerableObj = acObject.ACUrlCommand(iTableRow.VBSource);
                                    return enumerableObj;
                                }
                            }
                            else
                                return null;
                        }
                        else if (!String.IsNullOrEmpty(iTableRow.VBSource))
                        {
                            enumerableObj = parentDataContext2.GetValue(iTableRow.VBSource);
                            if (enumerableObj != null && enumerableObj is IEnumerable)
                                return enumerableObj;
                            return null;
                        }
                        // Fuktioniert nicht, da keine Relationsbechreibung vorhanden
                        else // if (parentDataContext is IEnumerable)
                            return null;
                    }
                }
            }
            // Kein Parent-DataContext
            else
            {
                object dataContext = null;
                if (!String.IsNullOrEmpty(iTableRow.DictKey))
                    dataContext = _data.GetValue<object>(iTableRow.DictKey);
                else if (_data.ReportDocumentValues.Any())
                    dataContext = _data.ReportDocumentValues.FirstOrDefault().Value;
                rootDataContext = dataContext;
                if (dataContext is ACQueryDefinition)
                {
                    childACQueryDef = dataContext as ACQueryDefinition;
                    enumerableObj = _data.Database.ACSelect(childACQueryDef, _data.Database.RecommendedMergeOption);
                }
                else if (dataContext is IACObject) // (dataContext is IACComponent)
                {
                    IACObject acObject = dataContext as IACObject;
                    if (!String.IsNullOrEmpty(iTableRow.VBSource))
                    {
                        //enumerableObj = acObject.ACUrlCommand(iTableRow.VBSource) as IEnumerable;
                        string path = "";
                        Global.ControlModes controlMode = Global.ControlModes.Enabled;
                        if (acObject.ACUrlBinding(iTableRow.VBSource, ref propertyType, ref enumerableObj, ref path, ref controlMode))
                        {
                            enumerableObj = acObject.ACUrlCommand(iTableRow.VBSource);
                            return enumerableObj;
                        }
                        else
                            return null;
                    }
                }
                else if (dataContext is DataTable)
                {
                    enumerableObj = dataContext;
                }
                else if (dataContext is IEnumerable) // (dataContext is IACComponent)
                {
                    enumerableObj = dataContext as IEnumerable;
                }
            }
            // Ende else if parentDatacontext

            return enumerableObj;
        }

        /// <summary>
        /// Raise PaginatorOnSetValueEventArgs 
        /// </summary>
        /// <param name="dv"></param>
        /// <param name="aggregateValues"></param>
        /// <param name="parentDataRow"></param>
        /// <param name="value"></param>
        protected virtual void OnSetFlowDocObjValue(IInlinePropertyValue dv, Dictionary<string, List<object>> aggregateValues, object parentDataRow, object value)
        {
            PaginatorOnSetValueEventArgs eventArgs = new PaginatorOnSetValueEventArgs(dv, parentDataRow, value, this);
            _report.FireEventSetFlowDocObjValue(eventArgs);
            if (!eventArgs.Handled)
                dv.Value = value;
        }

        /// <summary>
        /// Raise PaginatorNextRowEventArgs
        /// </summary>
        /// <param name="acComponent"></param>
        /// <param name="propertyType"></param>
        /// <param name="data"></param>
        /// <param name="tableRow"></param>
        /// <returns></returns>
        protected virtual bool LoadCurrentProperty(IACComponent acComponent, IACType propertyType, object data, TableRow tableRow)
        {
            PaginatorNextRowEventArgs eventArgs = new PaginatorNextRowEventArgs(acComponent, propertyType, data, this, tableRow);
            _report.FireEventNextRowEventArgs(eventArgs);
            if (!eventArgs.Handled)
            {
                if (acComponent != null && propertyType != null)
                    ACAccessNav<IACComponent>.LoadCurrentProperty(acComponent, propertyType, data);
            }
            return eventArgs.Handled;
        }

        protected virtual TableCell OnSetNewTableCell(IACComponent acComponent, IACType propertyType, object data, TableRow tableRow, object fieldValue, string fieldName, TableRowDataBase tableRowDataTemplate)
        {
            string valueAsString = "";
            if (fieldValue != null)
            {
                try
                {
                    if (tableRowDataTemplate != null && !String.IsNullOrEmpty(tableRowDataTemplate.StringFormat))
                        valueAsString = InlineValueBase.FormatValue(fieldValue, tableRowDataTemplate.StringFormat, tableRowDataTemplate.CultureInfo, tableRowDataTemplate.MaxLength, tableRowDataTemplate.Truncate);
                    else
                        valueAsString = fieldValue.ToString();
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == datamodel.ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException(nameof(ReportPaginator), nameof(OnSetNewTableCell), msg);
                }
            }
            TableCell newCell = new TableCell(new Paragraph(new Run(valueAsString)));
            PaginatorNewTableCellEventArgs eventArgs = new PaginatorNewTableCellEventArgs(acComponent, propertyType, data, this, tableRow, newCell, fieldValue, fieldName);
            _report.FireEventNewCellEventArgs(eventArgs);
            if (eventArgs.Handled && eventArgs.NewCell != null)
            {
                newCell = eventArgs.NewCell;
            }
            return newCell;
        }


        /// <summary>
        /// RememberAggregateValue
        /// </summary>
        /// <param name="aggregateValues"></param>
        /// <param name="aggregateGroups"></param>
        /// <param name="value"></param>
        protected void RememberAggregateValue(Dictionary<string, List<object>> aggregateValues, string aggregateGroups, object value)
        {
            if (String.IsNullOrEmpty(aggregateGroups))
                return;

            string[] aggregateGroupParts = aggregateGroups.Split(',', ';');

            // remember value for aggregate functions
            List<object> aggregateValueList = null;
            foreach (string aggregateGroup in aggregateGroupParts)
            {
                if (String.IsNullOrEmpty(aggregateGroup)) continue;
                string trimmedGroup = aggregateGroup.Trim();
                if (String.IsNullOrEmpty(trimmedGroup)) continue;
                if (!aggregateValues.TryGetValue(trimmedGroup, out aggregateValueList))
                {
                    aggregateValueList = new List<object>();
                    aggregateValues[trimmedGroup] = aggregateValueList;
                }
                aggregateValueList.Add(value);
            }
        }

        /// <summary>
        /// CloneVisualBlock
        /// </summary>
        /// <param name="block"></param>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        private ContainerVisual CloneVisualBlock(Block block, int pageNumber, string xaml)
        {
            FlowDocument tmpDoc = new FlowDocument();
            tmpDoc.ColumnWidth = double.PositiveInfinity;
            tmpDoc.PageHeight = _report.PageHeight;
            tmpDoc.PageWidth = _report.PageWidth;
            if (_flowDocument != null)
            {
                //tmpDoc.PagePadding.Left = _flowDocument.PagePadding.Left;
                //tmpDoc.PagePadding.Right = _flowDocument.PagePadding.Right;
                tmpDoc.PagePadding = new Thickness(_flowDocument.PagePadding.Left, 0, _flowDocument.PagePadding.Right, 0);
            }
            else
                tmpDoc.PagePadding = new Thickness(0);

            if (String.IsNullOrEmpty(xaml))
                xaml = XamlWriter.Save(block);
            Block newBlock = XamlReader.Parse(xaml) as Block;
            tmpDoc.Blocks.Add(newBlock);

            DocumentWalker walkerBlock = new DocumentWalker();
            List<InlineContextValue> blockValues = new List<InlineContextValue>();
            blockValues.AddRange(walkerBlock.Walk<InlineContextValue>(tmpDoc));

            // fill context values
            FillContextValues(blockValues, pageNumber);

            DocumentPage dp = ((IDocumentPaginatorSource)tmpDoc).DocumentPaginator.GetPage(0);
            return (ContainerVisual)dp.Visual;
        }

        /// <summary>
        /// FillContextValues
        /// </summary>
        /// <param name="list"></param>
        /// <param name="pageNumber"></param>
        protected virtual void FillContextValues(List<InlineContextValue> list, int pageNumber)
        {
            // fill context values
            foreach (InlineContextValue cv in list)
            {
                if (cv == null)
                    continue;

                ReportContextValueType? reportContextValueType = ReportPaginatorStaticCache.GetReportContextValueTypeByName(cv.DictKey);
                if (reportContextValueType == null)
                {
                    if (_data.ShowUnknownValues)
                        cv.Value = "<" + ((cv.DictKey != null) ? cv.DictKey : "NULL") + ">";
                    else
                        cv.Value = "";
                    SetIPropertyValue(cv, new Dictionary<string, List<object>>(), null);
                }
                else
                {
                    switch (reportContextValueType.Value)
                    {
                        case ReportContextValueType.PageNumber:
                            cv.Value = pageNumber;
                            break;
                        case ReportContextValueType.PageCount:
                            cv.Value = _pageCount;
                            break;
                        case ReportContextValueType.ReportName:
                            cv.Value = _report.ReportName;
                            break;
                        case ReportContextValueType.ReportTitle:
                            cv.Value = _report.ReportTitle;
                            break;
                        case ReportContextValueType.PrintingDate:
                            cv.Value = DateTime.Now;
                            break;
                    }
                }
            }
        }

        Tuple<ContainerVisual, TableRowDataHeader> currentHeader = null;

        /// <summary>
        /// This is most important method, modifies the original 
        /// </summary>
        /// <param name="pageNumber">page number</param>
        /// <returns></returns>
        public override DocumentPage GetPage(int pageNumber)
        {
            for (int i = 0; i < 2; i++) // do it twice because filling context values could change the page count
            {
                // compute page count
                if (pageNumber == 0)
                {
                    _paginator.ComputePageCount();
                    _pageCount = _paginator.PageCount;
                }

                // fill context values
                FillContextValues(_reportContextValues, pageNumber + 1);
            }

            DocumentPage page = _paginator.GetPage(pageNumber);
            if (page == DocumentPage.Missing)
                return DocumentPage.Missing; // page missing

            _pageSize = page.Size;

            // add header block
            ContainerVisual newPage = new ContainerVisual();


            double marginTop = 0;
            double marginBottom = 0;
            if (_flowDocument != null)
            {
                marginTop = _flowDocument.PagePadding.Top;
                marginBottom = _flowDocument.PagePadding.Bottom;
            }

            if (_blockPageHeader != null)
            {
                SectionReportHeader header = _blockPageHeader as SectionReportHeader;
                if (header == null || ((header != null && header.ShowHeaderOnFirstPage && pageNumber == 0) || pageNumber != 0))
                {
                    ContainerVisual v = CloneVisualBlock(_blockPageHeader, pageNumber + 1, _blockPageHeaderXAML);
                    v.Offset = new Vector(0, marginTop);
                    newPage.Children.Add(v);
                }
            }

            // TODO: process ReportContextValues

            // add content page
            ContainerVisual smallerPage = new ContainerVisual();
            smallerPage.Offset = new Vector(0, (_report.PageHeaderHeight / 100d * _report.PageHeight) + marginTop);
            smallerPage.Children.Add(page.Visual);
            newPage.Children.Add(smallerPage);

            // add footer block
            if (_blockPageFooter != null)
            {
                ContainerVisual v = CloneVisualBlock(_blockPageFooter, pageNumber + 1, _blockPageFooterXAML);
                v.Offset = new Vector(0, _report.PageHeight - (_report.PageFooterHeight / 100d * _report.PageHeight) - marginBottom);
                newPage.Children.Add(v);
            }

            // create modified BleedBox
            Rect bleedBox = new Rect(page.BleedBox.Left, page.BleedBox.Top, page.BleedBox.Width,
                _report.PageHeight - (page.Size.Height - page.BleedBox.Size.Height));

            // create modified ContentBox
            Rect contentBox = new Rect(page.ContentBox.Left, page.ContentBox.Top, page.ContentBox.Width,
                _report.PageHeight - (page.Size.Height - page.ContentBox.Size.Height));


            ContainerVisual table;
            if (PageStartsWithTable(smallerPage, out table) && currentHeader != null && currentHeader.Item2.RepeatTableHeader)
            {
                // The page starts with a table and a table header was
                // found on the previous page. Presumably this table 
                // was started on the previous page, so we'll repeat the
                // table header.
                Rect headerBounds = VisualTreeHelper.GetDescendantBounds(currentHeader.Item1);
                Vector offset = VisualTreeHelper.GetOffset(currentHeader.Item1);
                DrawingVisual tableHeaderVisual = new DrawingVisual();

                // Translate the header to be at the top of the page
                // instead of its previous position
                tableHeaderVisual.Transform = new TranslateTransform(
                    bleedBox.X,
                    bleedBox.Y - headerBounds.Top
                );

                // Since we've placed the repeated table header on top of the
                // content area, we'll need to scale down the rest of the content
                // to accomodate this. Since the table header is relatively small,
                // this probably is barely noticeable.
                double yScale = (contentBox.Height - headerBounds.Height) / contentBox.Height;
                TransformGroup group = new TransformGroup();
                group.Children.Add(new ScaleTransform(1.0, yScale));
                group.Children.Add(new TranslateTransform(
                    bleedBox.X,
                    bleedBox.Y + headerBounds.Height
                ));
                smallerPage.Transform = group;

                ContainerVisual cp = VisualTreeHelper.GetParent(currentHeader.Item1) as ContainerVisual;
                if (cp != null)
                {
                    cp.Children.Remove(currentHeader.Item1);


                }
                tableHeaderVisual.Children.Add(currentHeader.Item1);

                //Drawing table header background
                if (currentHeader.Item2 != null)
                {
                    Brush backgroundBrush = (Brush)currentHeader.Item2.GetValue(TextElement.BackgroundProperty);
                    using (DrawingContext drawingContext = tableHeaderVisual.RenderOpen())
                    {
                        if (backgroundBrush != null)
                        {
                            Rect bounds = headerBounds;
                            if (cp != null)
                            {
                                Rect parentBounds = VisualTreeHelper.GetDescendantBounds(cp);
                                if (parentBounds != null)
                                {
                                    bounds.Width = parentBounds.Width;
                                }
                            }

                            drawingContext.DrawRectangle(backgroundBrush, null, bounds);
                        }
                    }
                }

                smallerPage.Children.Add(tableHeaderVisual);
            }

            // Check if there is a table on the bottom of the page.
            // If it's there, its header should be repeated
            ContainerVisual newTable, newHeader;
            if (PageEndsWithTable(smallerPage, out newTable, out newHeader))
            {
                if (newTable == table && currentHeader != null)
                {
                    // Still the same table so don't change the repeating header
                }
                else
                {
                    // We've found a new table. Repeat the header on the next page
                    TableRowDataHeader dataHeader = GetTableRow(newHeader);
                    if (dataHeader != null)
                        currentHeader = new Tuple<ContainerVisual, TableRowDataHeader>(newHeader, dataHeader);
                }
            }
            else
            {
                // There was no table at the end of the page
                currentHeader = null;
            }


            DocumentPage dp = new DocumentPage(newPage, new Size(_report.PageWidth, _report.PageHeight), bleedBox, contentBox);
            _report.FireEventGetPageCompleted(new GetPageCompletedEventArgs(page, pageNumber, null, false, null));
            return dp;
        }

        public override void ComputePageCount()
        {
            base.ComputePageCount();
        }

        protected override void OnPagesChanged(PagesChangedEventArgs e)
        {
            base.OnPagesChanged(e);
        }

        /// <summary>
		/// Checks if the page ends with a table.
		/// </summary>
		/// <remarks>
		/// There is no such thing as a 'TableVisual'. There is a RowVisual, which
		/// is contained in a ParagraphVisual if it's part of a table. For our
		/// purposes, we'll consider this the table Visual
		/// 
		/// You'd think that if the last element on the page was a table row, 
		/// this would also be the last element in the visual tree, but this is not true
		/// The page ends with a ContainerVisual which is aparrently  empty.
		/// Therefore, this method will only check the last child of an element
		/// unless this is a ContainerVisual
		/// </remarks>
		/// <param name="originalPage"></param>
		/// <returns></returns>
		private bool PageEndsWithTable(DependencyObject element, out ContainerVisual tableVisual, out ContainerVisual headerVisual)
        {
            tableVisual = null;
            headerVisual = null;
            if (element.GetType().Name == "RowVisual")
            {
                tableVisual = (ContainerVisual)VisualTreeHelper.GetParent(element);
                headerVisual = (ContainerVisual)VisualTreeHelper.GetChild(tableVisual, 0);
                return true;
            }
            int children = VisualTreeHelper.GetChildrenCount(element);
            if (element.GetType() == typeof(ContainerVisual))
            {
                for (int c = children - 1; c >= 0; c--)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(element, c);
                    if (PageEndsWithTable(child, out tableVisual, out headerVisual))
                    {
                        return true;
                    }
                }
            }
            else if (children > 0)
            {
                DependencyObject child = VisualTreeHelper.GetChild(element, children - 1);
                if (PageEndsWithTable(child, out tableVisual, out headerVisual))
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Checks if the page starts with a table which presumably has wrapped
        /// from the previous page.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="tableVisual"></param>
        /// <param name="headerVisual"></param>
        /// <returns></returns>
        private bool PageStartsWithTable(DependencyObject element, out ContainerVisual tableVisual)
        {
            tableVisual = null;
            if (element.GetType().Name == "RowVisual")
            {
                tableVisual = (ContainerVisual)VisualTreeHelper.GetParent(element);
                return true;
            }
            if (VisualTreeHelper.GetChildrenCount(element) > 0)
            {
                DependencyObject child = VisualTreeHelper.GetChild(element, 0);
                if (PageStartsWithTable(child, out tableVisual))
                {
                    return true;
                }
            }
            return false;
        }

        private TableRowDataHeader GetTableRow(ContainerVisual rowVisual)
        {
            if (rowVisual == null)
                return null;

            PropertyInfo propInfo = rowVisual.GetType().GetProperty("Row", BindingFlags.NonPublic | BindingFlags.Instance);
            return propInfo?.GetValue(rowVisual) as TableRowDataHeader;
        }

        #endregion

        #endregion
    }
}
