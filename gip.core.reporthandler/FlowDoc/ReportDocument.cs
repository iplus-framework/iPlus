using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using System.Windows.Xps.Packaging;
using System.Windows.Xps.Serialization;
using System.Linq;
using gip.core.autocomponent;
using System.Xml.Linq;
using System.Xml;
using System.Globalization;
using System.Printing;

namespace gip.core.reporthandler.Flowdoc
{
    /// <summary>
    /// Contains a complete report template without data
    /// </summary>
    public class ReportDocument
    {
        #region c'tors
        public ReportDocument(string xaml)
        {
            _xamlData = xaml;
        }
        #endregion

        #region Properties
        private double _pageHeaderHeight = 0;
        /// <summary>
        /// Gets or sets the page header height
        /// </summary>
        public double PageHeaderHeight
        {
            get { return _pageHeaderHeight; }
            set { _pageHeaderHeight = value; }
        }

        private double _pageFooterHeight = 0;
        /// <summary>
        /// Gets or sets the page footer height
        /// </summary>
        public double PageFooterHeight
        {
            get { return _pageFooterHeight; }
            set { _pageFooterHeight = value; }
        }

        private double _pageHeight = double.NaN;
        /// <summary>
        /// Gets the original page height of the FlowDocument
        /// </summary>
        public double PageHeight
        {
            get { return _pageHeight; }
        }

        private double _pageWidth = double.NaN;
        /// <summary>
        /// Gets the original page width of the FlowDocument
        /// </summary>
        public double PageWidth
        {
            get { return _pageWidth; }
        }

        private string _reportName = "";
        /// <summary>
        /// Gets or sets the optional report name
        /// </summary>
        public string ReportName
        {
            get { return _reportName; }
            set { _reportName = value; }
        }

        private string _reportTitle = "";
        /// <summary>
        /// Gets or sets the optional report title
        /// </summary>
        public string ReportTitle
        {
            get { return _reportTitle; }
            set { _reportTitle = value; }
        }

        private string _xamlImagePath = "";
        /// <summary>
        /// XAML image path
        /// </summary>
        public string XamlImagePath
        {
            get { return _xamlImagePath; }
            set { _xamlImagePath = value; }
        }

        private string _xamlData = "";
        /// <summary>
        /// XAML report data
        /// </summary>
        public string XamlData
        {
            get
            {
                return _xamlData;
            }
            set
            {
                _xamlData = value;
                _XDoc = null;
            }
        }

        #region PrintOptions
        public string AutoSelectPrinterName
        {
            get;
            set;
        }

        public PageOrientation? AutoSelectPageOrientation
        {
            get;
            set;
        }

        public int? AutoSelectTray
        {
            get;
            set;
        }

        public PageMediaSize AutoPageMediaSize
        {
            get;
            set;
        }
        #endregion

        public static readonly DependencyProperty StringFormatProperty
            = DependencyProperty.Register("StringFormat", typeof(string), typeof(TextElement), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        public static readonly DependencyProperty MaxLengthProperty
            = DependencyProperty.Register("MaxLength", typeof(int), typeof(TextElement), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.Inherits));

        public static readonly DependencyProperty CultureInfoProperty
            = DependencyProperty.Register("CultureInfo", typeof(string), typeof(TextElement), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        private XDocument _XDoc = null;
        public XDocument XDoc
        {
            get
            {
                if (_XDoc != null)
                    return _XDoc;
                if (String.IsNullOrEmpty(_xamlData))
                    return null;
                try
                {
                    _XDoc = XDocument.Parse(_xamlData);
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == datamodel.ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("ReportDocument", "XDoc", msg);
                }
                return _XDoc;
            }
        }

        private CompressionOption _xpsCompressionOption = CompressionOption.NotCompressed;
        /// <summary>
        /// Gets or sets the compression option which is used to create XPS files
        /// </summary>
        /// <seealso cref="CreateXpsDocument(ReportData data, string fileName)" />
        public CompressionOption XpsCompressionOption
        {
            get { return _xpsCompressionOption; }
            set { _xpsCompressionOption = value; }
        }

        protected IEnumerable<ReportData> _ReportData = null;
        public IEnumerable<ReportData> ReportData
        {
            get
            {
                return _ReportData;
            }
        }

        #endregion

        #region Events
        /// <summary>
        /// Event occurs after loop of next Row
        /// </summary>
        public event EventHandler<PaginatorNextRowEventArgs> NextRow = null;

        /// <summary>
        /// Event occurs before populating a Row with a cell
        /// </summary>
        public event EventHandler<PaginatorNewTableCellEventArgs> NewCell = null;

        /// <summary>
        /// Event occurs when value of a Flowdocument-Object should be set
        /// </summary>
        public event EventHandler<PaginatorOnSetValueEventArgs> SetFlowDocObjValue = null;

        /// <summary>
        /// Event occurs after a page has been completed
        /// </summary>
        public event GetPageCompletedEventHandler GetPageCompleted = null;

        /// <summary>
        /// Event occurs if an exception has encountered while loading the BitmapSource
        /// </summary>
        public event EventHandler<ImageErrorEventArgs> ImageError = null;

        /// <summary>
        /// Event occurs before an image is being processed
        /// </summary>
        public event EventHandler<ImageEventArgs> ImageProcessing = null;

        /// <summary>
        /// Event occurs after an image has being processed
        /// </summary>
        public event EventHandler<ImageEventArgs> ImageProcessed = null;

        #endregion

        #region methods

        #region public
        /// <summary>
        /// Creates a flow document of the report data
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Flow document must have a specified page height</exception>
        /// <exception cref="ArgumentException">Flow document must have a specified page width</exception>
        /// <exception cref="ArgumentException">"Flow document must have only one ReportProperties section, but it has {0}"</exception>
        public FlowDocument CreateFlowDocument(bool usageInDesigner = false)
        {
            MemoryStream memoryStream = gip.core.layoutengine.Layoutgenerator.GetEncodedStream(_xamlData);
            if (memoryStream == null)
                return null;

            //MemoryStream mem = new MemoryStream();
            //byte[] buf = Encoding.UTF8.GetBytes(_xamlData);
            //mem.Write(buf, 0, buf.Length);
            //mem.Position = 0;
            FlowDocument res = XamlReader.Load(memoryStream) as FlowDocument;

            if (res.PageHeight == double.NaN)
                throw new ArgumentException("Flow document must have a specified page height");
            if (res.PageWidth == double.NaN)
                throw new ArgumentException("Flow document must have a specified page width");

            // remember original values
            _pageHeight = res.PageHeight;
            _pageWidth = res.PageWidth;
            if (!String.IsNullOrEmpty(res.Name))
            {
                try
                {
                    PageMediaSizeName sizeName = (PageMediaSizeName)Enum.Parse(typeof(PageMediaSizeName), res.Name);
                    this.AutoPageMediaSize = new PageMediaSize(sizeName);
                }
                catch (Exception e)
                {
                    if (_pageHeight > 0 && _pageWidth > 0)
                    {
                        this.AutoPageMediaSize = new PageMediaSize(_pageWidth, _pageHeight);
                    }

                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == datamodel.ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("ReportDocument", "CreateFlowDocument", msg);
                }
            }

            // search report properties
            DocumentWalker walker = new DocumentWalker();
            List<SectionReportHeader> headers = walker.Walk<SectionReportHeader>(res);
            List<SectionReportFooter> footers = walker.Walk<SectionReportFooter>(res);
            List<ReportProperties> properties = walker.Walk<ReportProperties>(res);
            if (properties.Count > 0)
            {
                if (properties.Count > 1)
                    throw new ArgumentException(String.Format("Flow document must have only one ReportProperties section, but it has {0}", properties.Count));
                ReportProperties prop = properties[0];
                if (prop.ReportName != null)
                    ReportName = prop.ReportName;
                if (prop.ReportTitle != null)
                    ReportTitle = prop.ReportTitle;
                if (headers.Count > 0)
                    PageHeaderHeight = headers[0].PageHeaderHeight;
                if (footers.Count > 0)
                    PageFooterHeight = footers[0].PageFooterHeight;
                if (!String.IsNullOrEmpty(prop.AutoSelectPrinterName))
                    AutoSelectPrinterName = prop.AutoSelectPrinterName;
                if (!String.IsNullOrEmpty(prop.AutoSelectPageOrientation))
                {
                    PageOrientation pageOrientation = PageOrientation.Unknown;
                    if (Enum.TryParse<PageOrientation>(prop.AutoSelectPageOrientation, out pageOrientation))
                        AutoSelectPageOrientation = pageOrientation;
                }
                if (prop.AutoSelectTray >= 0)
                    AutoSelectTray = prop.AutoSelectTray;
                if (!String.IsNullOrEmpty(prop.AutoPageMediaSize))
                {
                    string[] sizes = prop.AutoPageMediaSize.Split('x');
                    if (sizes != null && sizes.Count() == 2)
                    {
                        try
                        {
                            this.AutoPageMediaSize = new PageMediaSize(Double.Parse(sizes[0]), Double.Parse(sizes[1]));
                        }
                        catch(Exception exp)
                        {
                            if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == datamodel.ACInitState.Initialized)
                                datamodel.Database.Root.Messages.LogException("ReportDocument", "CreateFlowDocument(AutoPageMediaSize)", exp.Message);
                        }
                    }
                }

                // remove properties section from FlowDocument
                DependencyObject parent = prop.Parent;
                if (!usageInDesigner && prop != null)
                {
                    if (parent is FlowDocument)
                    {
                        ((FlowDocument)parent).Blocks.Remove(prop);
                        parent = null;
                    }
                    if (parent is Section)
                    {
                        ((Section)parent).Blocks.Remove(prop);
                        parent = null;
                    }
                }
            }
            else
            {
                if (headers.Count > 0)
                    PageHeaderHeight = headers[0].PageHeaderHeight;
                if (footers.Count > 0)
                    PageFooterHeight = footers[0].PageFooterHeight;
            }

            // make height smaller to have enough space for page header and page footer
            //res.PageHeight = _pageHeight - _pageHeight * (PageHeaderHeight + PageFooterHeight) / 100d;

            // search image objects
            List<Image> images = new List<Image>();
            walker.Tag = images;
            walker.VisualVisited += new DocumentVisitedEventHandler(walker_VisualVisited);
            walker.Walk(res);
            walker.VisualVisited -= walker_VisualVisited;

            // load all images
            foreach (Image image in images)
            {
                if (ImageProcessing != null)
                    ImageProcessing(this, new ImageEventArgs(this, image));
                try
                {
                    if (image.Tag is string && (image.Tag as string).IndexOf("StaticResource") < 0)
                        image.Source = new BitmapImage(new Uri("file:///" + Path.Combine(_xamlImagePath, image.Tag.ToString())));
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                    if (ex.InnerException != null && ex.InnerException.Message != null)
                        msg += " Inner:" + ex.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == datamodel.ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("ReportDocument", "CreateFlowDocument(10)", msg);

                    // fire event on exception and check for Handled = true after each invoke
                    if (ImageError != null)
                    {
                        bool handled = false;
                        lock (ImageError)
                        {
                            ImageErrorEventArgs eventArgs = new ImageErrorEventArgs(ex, this, image);
                            foreach (var ed in ImageError.GetInvocationList())
                            {
                                ed.DynamicInvoke(this, eventArgs);
                                if (eventArgs.Handled)
                                {
                                    handled = true;
                                    break;
                                }
                            }
                        }
                        if (!handled)
                            throw;
                    }
                    else
                        throw;
                }
                if (ImageProcessed != null)
                    ImageProcessed(this, new ImageEventArgs(this, image));
                // TODO: find a better way to specify file names
            }

            return res;
        }

        /// <summary>
        /// Helper method to create page header or footer from flow document template
        /// </summary>
        /// <param name="data">report data</param>
        /// <returns></returns>
        public XpsDocument CreateXpsDocument(ReportData data)
        {
            _ReportData = new List<ReportData>() { data };
            MemoryStream ms = new MemoryStream();
            Package pkg = Package.Open(ms, FileMode.Create, FileAccess.ReadWrite);
            string pack = "pack://report.xps";
            PackageStore.RemovePackage(new Uri(pack));
            PackageStore.AddPackage(new Uri(pack), pkg);
            XpsDocument doc = new XpsDocument(pkg, CompressionOption.NotCompressed, pack);
            XpsSerializationManager rsm = new XpsSerializationManager(new XpsPackagingPolicy(doc), false);
            //DocumentPaginator paginator = ((IDocumentPaginatorSource)CreateFlowDocument()).DocumentPaginator;

            data.InformComponents(this, datamodel.ACPrintingPhase.Started);
            ReportPaginator rp = new ReportPaginator(this, data);
            try
            {
                rsm.SaveAsXaml(rp);
            }
            catch (Exception e)
            {
                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == datamodel.ACInitState.Initialized)
                {
                    datamodel.Database.Root.Messages.LogException("ReportDocument", "CreateXpsDocument(10)", e.Message);
                    if (e.InnerException != null)
                        datamodel.Database.Root.Messages.LogException("ReportDocument", "CreateXpsDocument(20)", e.InnerException.Message);
                    datamodel.Database.Root.Messages.LogException("ReportDocument", "CreateXpsDocument(30)", e.StackTrace);
                }
            }
            data.InformComponents(this, datamodel.ACPrintingPhase.Completed);
            _ReportData = null;
            return doc;
        }

        /// <summary>
        /// Helper method to create page header or footer from flow document template
        /// </summary>
        /// <param name="data">enumerable report data</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">data</exception>
        public XpsDocument CreateXpsDocument(IEnumerable<ReportData> data)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            _ReportData = data;

            int count = 0;
            ReportData firstData = null;
            foreach (ReportData rd in data)
            {
                if (firstData == null)
                    firstData = rd;
                count++;
            }
            if (count == 1)
            {
                XpsDocument xpsDoc = CreateXpsDocument(firstData); // we have only one ReportData object -> use the normal ReportPaginator instead
                _ReportData = null;
                return xpsDoc;
            }

            MemoryStream ms = new MemoryStream();
            Package pkg = Package.Open(ms, FileMode.Create, FileAccess.ReadWrite);
            string pack = "pack://report.xps";
            PackageStore.RemovePackage(new Uri(pack));
            PackageStore.AddPackage(new Uri(pack), pkg);
            XpsDocument doc = new XpsDocument(pkg, CompressionOption.NotCompressed, pack);
            XpsSerializationManager rsm = new XpsSerializationManager(new XpsPackagingPolicy(doc), false);
            //DocumentPaginator paginator = ((IDocumentPaginatorSource)CreateFlowDocument()).DocumentPaginator;

            if (data != null && data.Any())
                data.FirstOrDefault().InformComponents(this, datamodel.ACPrintingPhase.Started);
            MultipleReportPaginator rp = new MultipleReportPaginator(this, data);
            try
            { 
                rsm.SaveAsXaml(rp);
            }
            catch (Exception e)
            {
                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == datamodel.ACInitState.Initialized)
                {
                    datamodel.Database.Root.Messages.LogException("ReportDocument", "CreateXpsDocument(10)", e.Message);
                    if (e.InnerException != null)
                        datamodel.Database.Root.Messages.LogException("ReportDocument", "CreateXpsDocument(20)", e.InnerException.Message);
                    datamodel.Database.Root.Messages.LogException("ReportDocument", "CreateXpsDocument(30)", e.StackTrace);
                }
            }
            if (data != null && data.Any())
                data.FirstOrDefault().InformComponents(this, datamodel.ACPrintingPhase.Completed);
            _ReportData = null;
            return doc;
        }

        /// <summary>
        /// Helper method to create page header or footer from flow document template
        /// </summary>
        /// <param name="data">report data</param>
        /// <param name="fileName">file to save XPS to</param>
        /// <returns></returns>
        public XpsDocument CreateXpsDocument(ReportData data, string fileName)
        {
            _ReportData = new List<ReportData>() { data };
            Package pkg = Package.Open(fileName, FileMode.Create, FileAccess.ReadWrite);
            string pack = "pack://report.xps";
            PackageStore.RemovePackage(new Uri(pack));
            PackageStore.AddPackage(new Uri(pack), pkg);
            XpsDocument doc = new XpsDocument(pkg, _xpsCompressionOption, pack);
            XpsSerializationManager rsm = new XpsSerializationManager(new XpsPackagingPolicy(doc), false);
            //DocumentPaginator paginator = ((IDocumentPaginatorSource)CreateFlowDocument()).DocumentPaginator;

            data.InformComponents(this, datamodel.ACPrintingPhase.Started);
            ReportPaginator rp = new ReportPaginator(this, data);
            try
            { 
                rsm.SaveAsXaml(rp);
            }
            catch (Exception e)
            {
                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == datamodel.ACInitState.Initialized)
                {
                    datamodel.Database.Root.Messages.LogException("ReportDocument", "CreateXpsDocument(10)", e.Message);
                    if (e.InnerException != null)
                        datamodel.Database.Root.Messages.LogException("ReportDocument", "CreateXpsDocument(20)", e.InnerException.Message);
                    datamodel.Database.Root.Messages.LogException("ReportDocument", "CreateXpsDocument(30)", e.StackTrace);
                }
            }
            rsm.Commit();
            pkg.Close();
            data.InformComponents(this, datamodel.ACPrintingPhase.Completed);
            _ReportData = null;
            return new XpsDocument(fileName, FileAccess.Read);
        }

        /// <summary>
        /// Helper method to create page header or footer from flow document template
        /// </summary>
        /// <param name="data">enumerable report data</param>
        /// <param name="fileName">file to save XPS to</param>
        /// <returns></returns>
        public XpsDocument CreateXpsDocument(IEnumerable<ReportData> data, string fileName)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            _ReportData = data;
            int count = 0;
            ReportData firstData = null;
            foreach (ReportData rd in data)
            {
                if (firstData == null) firstData = rd;
                count++;
            }
            if (count == 1)
            {
                _ReportData = null;
                return CreateXpsDocument(firstData); // we have only one ReportData object -> use the normal ReportPaginator instead
            }

            Package pkg = Package.Open(fileName, FileMode.Create, FileAccess.ReadWrite);
            string pack = "pack://report.xps";
            PackageStore.RemovePackage(new Uri(pack));
            PackageStore.AddPackage(new Uri(pack), pkg);
            XpsDocument doc = new XpsDocument(pkg, _xpsCompressionOption, pack);
            XpsSerializationManager rsm = new XpsSerializationManager(new XpsPackagingPolicy(doc), false);
            //DocumentPaginator paginator = ((IDocumentPaginatorSource)CreateFlowDocument()).DocumentPaginator;

            if (data != null && data.Any())
                data.FirstOrDefault().InformComponents(this, datamodel.ACPrintingPhase.Started);
            MultipleReportPaginator rp = new MultipleReportPaginator(this, data);
            try
            { 
                rsm.SaveAsXaml(rp);
            }
            catch (Exception e)
            {
                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == datamodel.ACInitState.Initialized)
                {
                    datamodel.Database.Root.Messages.LogException("ReportDocument", "CreateXpsDocument(10)", e.Message);
                    if (e.InnerException != null)
                        datamodel.Database.Root.Messages.LogException("ReportDocument", "CreateXpsDocument(20)", e.InnerException.Message);
                    datamodel.Database.Root.Messages.LogException("ReportDocument", "CreateXpsDocument(30)", e.StackTrace);
                }
            }
            rsm.Commit();
            pkg.Close();
            if (data != null && data.Any())
                data.FirstOrDefault().InformComponents(this, datamodel.ACPrintingPhase.Completed);
            _ReportData = null;
            return new XpsDocument(fileName, FileAccess.Read);
        }

        public string UpdateXAMLDataFromChangedFlowDoc(FlowDocument changedFlowDoc)
        {
            if (changedFlowDoc == null)
                return null;
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.ConformanceLevel = ConformanceLevel.Document;
            //settings.OmitXmlDeclaration = true;
            XamlDesignerSerializationManager dsm = new XamlDesignerSerializationManager(XmlWriter.Create(sb, settings));
            dsm.XamlWriterMode = XamlWriterMode.Expression;

            string newXAML = "";

            try
            {

                XamlWriter.Save(changedFlowDoc, dsm);
                newXAML = sb.ToString();


                XDocument newXDoc = XDocument.Parse(newXAML);
                var queryImages = newXDoc.Descendants("{http://schemas.microsoft.com/winfx/2006/xaml/presentation}Image");
                foreach (XElement xElImage in queryImages)
                {
                    XAttribute xAttrTag = xElImage.Attribute("Tag");
                    XElement xBitmapImage = xElImage.Elements("{http://schemas.microsoft.com/winfx/2006/xaml/presentation}Image.Source").FirstOrDefault();
                    if (xAttrTag != null && xBitmapImage != null)
                    {
                        xBitmapImage.Remove();
                        if (xAttrTag.Value.IndexOf("StaticResource") >= 0)
                        {
                            xElImage.SetAttributeValue("Source", "{" + xAttrTag.Value + "}");
                        }
                    }
                }
                sb = new StringBuilder(newXAML.Length, newXAML.Length * 2);
                XmlWriter writer = XmlWriter.Create(sb, settings);
                newXDoc.WriteTo(writer);
                writer.Flush();
                newXAML = sb.ToString();
                this.XamlData = newXAML;
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == datamodel.ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ReportDocument", "UpdateXAMLDataFromChangedFlowDoc", msg);
            }

            return newXAML;
        }
        #endregion

        #region internal
        /// <summary>
        /// Fire event after a page has been completed
        /// </summary>
        /// <param name="ea">GetPageCompletedEventArgs</param>
        internal void FireEventGetPageCompleted(GetPageCompletedEventArgs ea)
        {
            if (GetPageCompleted != null)
                GetPageCompleted(this, ea);
        }

        /// <summary>
        /// Fire event after a data row has been bound
        /// </summary>
        /// <param name="ea">DataRowBoundEventArgs</param>
        internal void FireEventNextRowEventArgs(PaginatorNextRowEventArgs ea)
        {
            if (NextRow != null)
                NextRow(this, ea);
        }

        internal void FireEventNewCellEventArgs(PaginatorNewTableCellEventArgs ea)
        {
            if (NewCell != null)
                NewCell(this, ea);
        }

        internal void FireEventSetFlowDocObjValue(PaginatorOnSetValueEventArgs ea)
        {
            if (SetFlowDocObjValue != null)
                SetFlowDocObjValue(this, ea);
        }

        private void walker_VisualVisited(object sender, DocumentVisitedEventArgs e)
        {
            if (!(e.VisitedObject is Image))
                return;

            DocumentWalker walker = sender as DocumentWalker;
            if (walker == null)
                return;

            List<Image> list = walker.Tag as List<Image>;
            if (list == null)
                return;

            list.Add((Image)e.VisitedObject);
        }
        #endregion

        #endregion

    }
}
