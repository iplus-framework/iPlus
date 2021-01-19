using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.ComponentModel;
using System.IO;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Text;
using gip.core.datamodel;
using gip.core.layoutengine;
using gip.core.reporthandler;

namespace Document.Editor
{
    public class DocumentEditor : RichTextBox
    {
        public bool FileChanged = false;
        public string DocumentName = null;
        public double ZoomLevel = 1;
        public Thickness docpadding = new Thickness(96, 96, 96, 96);
        public TableCell SelectedTableCell = null;
        public Image SelectedImage = null;
        public MediaElement SelectedVideo = null;
        public Shape SelectedShape = null;

        public UIElement SelectedObject = null;
        public int SelectedLineNumber = 0;
        public int LineCount = 0;
        public int SelectedColumnNumber = 0;
        public int ColumnCount = 0;
        public int WordCount = 0;
        private bool _EventsSubscr = false;

        public DocumentEditor()
        {
            SubscribeEvents();
            this.IsDocumentEnabled = true;
            Document.PageWidth = 816;
            Document.PageHeight = 1056;
            CaretPosition.Paragraph.LineHeight = 1.15;
            FontFamily = gip.core.reporthandler.Properties.Settings.Default.Options_DefaultFont;
            FontSize = gip.core.reporthandler.Properties.Settings.Default.Options_DefaultFontSize;
            AcceptsTab = true;
            Margin = new Thickness(-2, -3, 0, 0);
            SetPageMargins(docpadding);
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\spellcheck_ignorelist.lex"))
            {
                IList dictionaries = SpellCheck.GetCustomDictionaries(this);
                dictionaries.Add(new Uri(AppDomain.CurrentDomain.BaseDirectory + "\\spellcheck_ignorelist.lex"));
            }
        }

        public DocumentEditor(FlowDocument flowDoc) : this()
        {
            Document = flowDoc;
            if (flowDoc != null)
            {
                docpadding = flowDoc.PagePadding;
            }
        }

        public void SetupMarginsManager(FlowDocument document, Thickness margin)
        {
            docpadding = margin;
            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(FlowDocument.PagePaddingProperty, typeof(FlowDocument));
            dpd.AddValueChanged(document, new EventHandler(SetPadding));
        }

        public void SetPageMargins(Thickness thickness)
        {
            docpadding = thickness;
            Document.PagePadding = docpadding;
        }

        private void SetPadding(object sender, EventArgs e)
        {
            FlowDocument fd = (FlowDocument)sender;
            if (fd.PagePadding != new Thickness(docpadding.Left, docpadding.Top, docpadding.Right, docpadding.Bottom))
            {
                fd.PagePadding = docpadding;
            }
        }

        #region "LoadDocument"

        public void LoadDocument(string filename)
        {
            FileInfo f = new FileInfo(filename);
            TextRange tr = new TextRange(Document.ContentStart, Document.ContentEnd);
            ScrollViewer s = Parent as ScrollViewer;
            Grid g = s.Parent as Grid;
            DocumentTab p = g.Parent as DocumentTab;
            FileStream fs = null;
            bool isreadonlyfile = false;
            if (f.IsReadOnly)
            {
                isreadonlyfile = true;
            }
            if (f.Extension.ToLower() == ".xamlpackage")
            {
                TextRange t = new TextRange(Document.ContentStart, Document.ContentEnd);
                FileStream file = new FileStream(filename, FileMode.Open);
                t.Load(file, System.Windows.DataFormats.XamlPackage);
                file.Close();
            }
            else if (f.Extension.ToLower() == ".xaml")
            {
                fs = File.Open(f.FullName, FileMode.Open, FileAccess.Read);
                FlowDocument content = XamlReader.Load(fs) as FlowDocument;
                Thickness thi = content.PagePadding;
                try
                {
                    double leftmargin = thi.Left;
                    double topmargin = thi.Top;
                    double rightmargin = thi.Right;
                    double bottommargin = thi.Bottom;
                    SetPageMargins(new Thickness(leftmargin, topmargin, rightmargin, bottommargin));
                }
                catch (Exception e)
                {
                    SetPageMargins(new Thickness(0, 0, 0, 0));
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;
                    
                    this.Root().Messages.LogException("Document.Editor.DocumentEditor", "LoadDocument", msg);
                }
                Document = content;
            }
            else if (f.Extension.ToLower() == ".docx")
            {
                OpenXMLtoFlowDocument converter = new OpenXMLtoFlowDocument(f.FullName);
                FlowDocument content = converter.Convert() as FlowDocument;
                Document = content;
                converter.Close();
                //ElseIf f.Extension.ToLower = ".odt" Then
                //    fs.Close()
                //    fs = Nothing
                //    Dim converter As New OpenDocumenttoFlowDocument
                //    Document = converter.Convert(f.FullName)
            }
            else if (f.Extension.ToLower() == ".html" || f.Extension.ToLower() == ".htm")
            {
                try
                {
                    FlowDocument content = XamlReader.Parse(HTMLConverter.HtmlToXamlConverter.ConvertHtmlToXaml(File.ReadAllText(filename), true)) as FlowDocument;
                    Document = content;
                }
                catch (Exception e)
                {
                    VBWindowDialogMsg vbMessagebox = new VBWindowDialogMsg(new Msg() { Message = "Error loading html document", MessageLevel = eMsgLevel.Error }, eMsgButton.OK, null);
                    vbMessagebox.ShowMessageBox();
                    //MessageBoxDialog m = new MessageBoxDialog("Error loading html document", "Error", null, 0);
                    //m.MessageImage.Source = new BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Common/error32.png", UriKind.Relative));
                    //m.Owner = App._GlobalApp.MainWindow;
                    //m.ShowDialog();

                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    this.Root().Messages.LogException("Document.Editor.DocumentEditor", "LoadDocument(10)", msg);
                }
            }
            else if (f.Extension.ToLower() == ".rtf")
            {
                if (isreadonlyfile)
                {
                    fs = File.Open(f.FullName, FileMode.Open, FileAccess.Read);
                }
                else
                {
                    fs = File.Open(f.FullName, FileMode.Open);
                }
                tr.Load(fs, DataFormats.Rtf);
            }
            else
            {
                if (isreadonlyfile)
                {
                    fs = File.Open(f.FullName, FileMode.Open, FileAccess.Read);
                }
                else
                {
                    fs = File.Open(f.FullName, FileMode.Open);
                }
                tr.Load(fs, DataFormats.Text);
            }
            if (fs != null)
            {
                fs.Close();
                fs = null;
            }
            if (f.IsReadOnly)
            {
                IsReadOnly = true;
            }
            DocumentName = filename;
            Semagsoft.HyperlinkHelper helper = new Semagsoft.HyperlinkHelper();
            helper.SubscribeToAllHyperlinks(Document);
            //Dim helper2 As New Semagsoft.ImageHelper
            //helper2.AddImageResizers(Me)
            p.SetDocumentTitle(f.Name);
            FileChanged = false;
        }

        #endregion

        #region "SaveDocument"

        public void SaveDocument(string filename)
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
            FileInfo file = new FileInfo(filename);
            FileStream fs = File.Open(filename, FileMode.OpenOrCreate);
            TextRange tr = new TextRange(Document.ContentStart, Document.ContentEnd);
            ScrollViewer sc = Parent as ScrollViewer;
            Grid tabgrid = sc.Parent as Grid;
            DocumentTab doctab = tabgrid.Parent as DocumentTab;
            if (file.Extension.ToLower() == ".xamlpackage")
            {
                fs.Close();
                fs = null;
                TextRange range = default(TextRange);
                FileStream fStream = null;
                range = new TextRange(Document.ContentStart, Document.ContentEnd);
                fStream = new FileStream(filename, FileMode.Create);
                range.Save(fStream, DataFormats.XamlPackage, true);
                fStream.Close();
            }
            else if (file.Extension.ToLower() == ".xaml")
            {
                XamlWriter.Save(Document, fs);
                fs.Close();
                fs = null;
                XmlDocument xd = new XmlDocument();
                xd.LoadXml(File.ReadAllText(filename));
                StringBuilder sb = new StringBuilder();
                using (StringWriter sw = new StringWriter(sb))
                using (XmlTextWriter xtw = new XmlTextWriter(sw))
                try
                {
                    xtw.Formatting = Formatting.Indented;
                    xd.WriteTo(xtw);
                }
                finally
                {
                    if (xtw != null)
                    {
                        xtw.Close();
                    }
                }
                string tex = sb.ToString();
                //Dim final As String
                //Try
                //    final = tex.Remove(tex.IndexOf("</FlowDocument>"), tex.Length)
                //Catch ex As Exception
                //End Try
                File.WriteAllText(filename, tex);
            }
            else if (file.Extension.ToLower() == ".docx")
            {
                fs.Close();
                fs = null;
                FlowDocumenttoOpenXML converter = new FlowDocumenttoOpenXML();
                converter.Convert(Document, filename);
                converter.Close();
                //ElseIf file.Extension.ToLower = ".odt" Then
                //    fs.Close()
                //    fs = Nothing
                //    Dim converter As New FlowDocumenttoOpenDocument
                //    Dim opendoc As AODL.Document.TextDocuments.TextDocument = converter.Convert(Document)
                //    opendoc.SaveTo(filename)
            }
            else if (file.Extension.ToLower() == ".html" || file.Extension.ToLower() == ".htm")
            {
                fs.Close();
                fs = null;
                string s = System.Windows.Markup.XamlWriter.Save(Document);
                try
                {
                    File.WriteAllText(filename, HTMLConverter.HtmlFromXamlConverter.ConvertXamlToHtml(s));
                }
                catch (Exception e)
                {
                    VBWindowDialogMsg vbMessagebox = new VBWindowDialogMsg(new Msg() { Message = "Error saving document", MessageLevel = eMsgLevel.Error }, eMsgButton.OK, null);
                    vbMessagebox.ShowMessageBox();
                    //MessageBoxDialog m = new MessageBoxDialog("Error saving document", "Error", null, 0);
                    //m.MessageImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/gip.core.reporthandler;Component/FlowDocEditor/Editor/Images/Common/error32.png", UriKind.Relative));
                    //m.Owner = App._GlobalApp.MainWindow;
                    //m.ShowDialog();
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    this.Root().Messages.LogException("Document.Editor.DocumentEditor", "SaveDocument", msg);
                }
            }
            else if (file.Extension.ToLower() == ".rtf")
            {
                tr.Save(fs, DataFormats.Rtf);
            }
            else
            {
                tr.Save(fs, DataFormats.Text);
            }
            if (fs != null)
            {
                fs.Close();
                fs = null;
            }
            doctab.SetDocumentTitle(file.Name);
            DocumentName = filename;
            FileChanged = false;
        }

        #endregion

        #region Events
        public void SubscribeEvents()
        {
            if (_EventsSubscr)
                return;
            TextChanged += DocumentEditor_TextChanged;
            SelectionChanged += DocumentEditor_SelectionChanged;
            LayoutUpdated += DocumentEditor_LayoutUpdated;
            _EventsSubscr = true;
        }

        public void UnSubscribeEvents()
        {
            if (!_EventsSubscr)
                return;
            TextChanged -= DocumentEditor_TextChanged;
            SelectionChanged -= DocumentEditor_SelectionChanged;
            LayoutUpdated -= DocumentEditor_LayoutUpdated;
            _EventsSubscr = false;
        }

        private void DocumentEditor_LayoutUpdated(object sender, EventArgs e)
        {
            SetPageMargins(docpadding);
        }

        private void DocumentEditor_SelectionChanged(object sender, RoutedEventArgs e)
        {
            TextPointer ls = CaretPosition.GetLineStartPosition(0);
            TextPointer p = Document.ContentStart.GetLineStartPosition(0);
            int @int = 1;
            int int2 = 1;
            while (true)
            {
                if (ls.CompareTo(p) < 1)
                {
                    break; // TODO: might not be correct. Was : Exit While
                }
                int r = 0;
                p = p.GetLineStartPosition(1, out r);
                if (r == 0)
                {
                    break; // TODO: might not be correct. Was : Exit While
                }
                @int += 1;
            }
            TextPointer ls2 = Document.ContentStart.DocumentEnd.GetLineStartPosition(0);
            TextPointer p2 = Document.ContentEnd.DocumentStart.GetLineStartPosition(0);
            while (true)
            {
                if (ls2.CompareTo(p2) < 1)
                {
                    break; // TODO: might not be correct. Was : Exit While
                }
                int r = 0;
                p2 = p2.GetLineStartPosition(1, out r);
                if (r == 0)
                {
                    break; // TODO: might not be correct. Was : Exit While
                }
                int2 += 1;
            }
            SelectedLineNumber = @int;
            LineCount = int2;
            TextRange t = new TextRange(Document.ContentStart, Document.ContentEnd);
            TextPointer caretPos = CaretPosition;
            TextPointer poi = CaretPosition.GetLineStartPosition(0);
            int currentColumnNumber = Math.Max(p.GetOffsetToPosition(caretPos) - 1, 0) + 1;
            int currentColumnCount = currentColumnNumber;
            currentColumnCount += CaretPosition.GetTextRunLength(LogicalDirection.Forward);
            SelectedColumnNumber = currentColumnNumber;
            ColumnCount = currentColumnCount;
        }

        private void DocumentEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (FileChanged == false)
            {
                FileChanged = true;
            }
            Semagsoft.HyperlinkHelper helper = new Semagsoft.HyperlinkHelper();
            helper.SubscribeToAllHyperlinks(Document);
            WordCount = GetWordCount();
        }
        #endregion

        #region "Find"

        public TextRange FindWordFromPosition(TextPointer position, string word)
        {
            while (position != null)
            {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string textRun = position.GetTextInRun(LogicalDirection.Forward);
                    // Find the starting index of any substring that matches "word".
                    int indexInRun = textRun.IndexOf(word);
                    if (indexInRun >= 0)
                    {
                        TextPointer start = position.GetPositionAtOffset(indexInRun);
                        TextPointer end = start.GetPositionAtOffset(word.Length);
                        return new TextRange(start, end);
                    }
                }
                position = position.GetNextContextPosition(LogicalDirection.Forward);
            }
            // position will be null if "word" is not found.
            return null;
        }

        #endregion

        #region "Subscript/Superscript"

        public void ToggleSubscript()
        {
            dynamic currentAlignment = this.Selection.GetPropertyValue(Inline.BaselineAlignmentProperty);
            BaselineAlignment newAlignment = ((BaselineAlignment)currentAlignment == BaselineAlignment.Subscript) ? BaselineAlignment.Baseline : BaselineAlignment.Subscript;
            this.Selection.ApplyPropertyValue(Inline.BaselineAlignmentProperty, newAlignment);
        }

        public void ToggleSuperscript()
        {
            dynamic currentAlignment = this.Selection.GetPropertyValue(Inline.BaselineAlignmentProperty);
            BaselineAlignment newAlignment = ((BaselineAlignment)currentAlignment == BaselineAlignment.Superscript) ? BaselineAlignment.Baseline : BaselineAlignment.Superscript;
            this.Selection.ApplyPropertyValue(Inline.BaselineAlignmentProperty, newAlignment);
        }

        #endregion

        #region "Strikethrough"

        public void ToggleStrikethrough()
        {
            TextRange range = new TextRange(Selection.Start, Selection.End);
            TextDecorationCollection t = (TextDecorationCollection)Selection.GetPropertyValue(Inline.TextDecorationsProperty);
            if (t == null || !t.Equals(TextDecorations.Strikethrough))
            {
                t = TextDecorations.Strikethrough;
            }
            else
            {
                t = new TextDecorationCollection();
            }
            range.ApplyPropertyValue(Inline.TextDecorationsProperty, t);
        }

        #endregion

        #region "GetWordCount"

        public int GetWordCount()
        {
            int SpacePos = 0;
            int X = 1;
            int WordCount = 0;
            bool NoMore = false;
            int CharValue = 0;
            TextRange tr = new TextRange(Document.ContentStart, Document.ContentEnd);
            string content = tr.Text;
            content = content.Replace("\r", " ");
            content = content.Replace("\n", " ");
            if (content.Trim().Length > 0)
            {
                while (NoMore == false)
                {
                    string trimmed = content.Trim();
                    if (trimmed.Length <= (X + 1))
                        X = trimmed.Length - 1;
                    SpacePos = content.Trim().IndexOf(" ", X);
                    if (SpacePos > 0)
                    {
                        CharValue = System.Convert.ToChar(content.Substring(X - 1, 1));
                        if (CharValue > 64 && CharValue < 91 || CharValue > 96 && CharValue < 123 || CharValue > 47 && CharValue < 58)
                        {
                            WordCount += 1;
                        }
                        X = SpacePos + 1;
                        while (X < content.Length && content.Substring(X - 1, 1) == " ")
                        {
                            X += 1;
                        }
                    }
                    else
                    {
                        if (X <= content.Length)
                        {
                            CharValue = System.Convert.ToChar(content.Substring(X - 1, 1));
                            if (CharValue > 64 && CharValue < 91 || CharValue > 96 && CharValue < 123 || CharValue > 47 && CharValue < 58)
                            {
                                WordCount += 1;
                            }
                        }
                        NoMore = true;
                    }
                }
            }
            return WordCount;
        }

        #endregion

        #region "GoToLine"

        public void GoToLine(int linenumber)
        {
            if (linenumber == 1)
            {
                this.CaretPosition = Document.ContentStart.DocumentStart.GetLineStartPosition(0);
            }
            else
            {
                TextPointer ls = Document.ContentStart.DocumentStart.GetLineStartPosition(0);
                TextPointer p = Document.ContentStart.GetLineStartPosition(0);
                int @int = 2;
                while (true)
                {
                    int r = 0;
                    p = p.GetLineStartPosition(1, out r);
                    if (r == 0)
                    {
                        this.CaretPosition = p;
                        break; // TODO: might not be correct. Was : Exit While
                    }
                    if (linenumber == @int)
                    {
                        this.CaretPosition = p;
                        break; // TODO: might not be correct. Was : Exit While
                    }
                    @int += 1;
                }
            }
        }

        #endregion

    }
}