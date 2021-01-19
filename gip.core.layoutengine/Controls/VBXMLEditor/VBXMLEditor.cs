using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using gip.core.datamodel;
using gip.core.layoutengine.Helperclasses;
using System.Transactions;
using System.ComponentModel;
using ICSharpCode.AvalonEdit;
using gip.core.layoutengine.CodeCompletion;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Indentation;
using System.Reflection;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Control element for editing XML.
    /// </summary>
    /// <summary xml:lang="de">
    /// Steuerelemnt zur Bearbeitung von XML.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBXMLEditor'}de{'VBXMLEditor'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBXMLEditor : VBTextEditor, IVBContent, IACObject, IACMenuBuilderWPFTree
    {
        #region c'tors

        /// <summary>
        /// Creates a new instance of VBXMLEditor.
        /// </summary>
        public VBXMLEditor() : base()
        {
        }

        #endregion

        #region Init/Deinit

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            if (!string.IsNullOrEmpty(CodeCompletionSchema))
            {
                _xmlCompletionDataProvider = new XmlCompletionDataProvider();
                string path = "";

                if (CodeCompletionSchema.Contains("VBXMLEditorSchemas"))
                    path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CodeCompletionSchema);

                else
                    path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, System.IO.Path.Combine("VBXMLEditorSchemas", CodeCompletionSchema));

                if (!File.Exists(path))
                    Database.Root.Messages.Warning(this, "Schema is not exist in " + path, true);
                else
                    _xmlCompletionDataProvider.LoadSchema(path);
            }

            // XML-Highlighting
            IHighlightingDefinition customHighlighting;
            string xshdFile = "gip.core.layoutengine.Controls.VBXMLEditor.Highlighting.XML-ModeStyleGip.xshd";
            if (ControlManager.WpfTheme == eWpfTheme.Aero)
                xshdFile = "gip.core.layoutengine.Controls.VBXMLEditor.Highlighting.XML-ModeStyleAero.xshd";
            if (ControlManager.WpfTheme == eWpfTheme.Gip)
                xshdFile = "gip.core.layoutengine.Controls.VBXMLEditor.Highlighting.XML-ModeStyleGip.xshd";
            using (Stream s = this.GetType().Assembly.GetManifestResourceStream(xshdFile))
            {
                if (s == null)
                    throw new InvalidOperationException("Could not find a embedded resource");
                using (XmlReader reader = new XmlTextReader(s))
                {
                    customHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
            HighlightingManager.Instance.RegisterHighlighting("XML", new string[] { ".xml" }, customHighlighting);
            SyntaxHighlighting = customHighlighting;

            base.OnInitialized(e);
        }

        /// <summary>
        /// Initializes the VB control.
        /// </summary>
        protected override void InitVBControl()
        {
            base.InitVBControl();
        }

        /// <summary>
        /// Overides the OnApplyTemplate method and runs the VBControl initialization.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            InitVBControl();
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
            base.DeInitVBControl(bso);
        }

        #endregion

        #region Properties

        private XmlCompletionDataProvider _xmlCompletionDataProvider;

        private string _CodeCompletionSchema = "";
        /// <summary>
        /// Gets or sets the code completion schema. Here you can set which code completion schema editor will use. 
        /// Available code completion schemas is in VBXMLEditorSchemas directory that is located in binary directory. 
        /// Also in VBXMLEditorSchemas directory you can add your custom code completion schemas.
        /// </summary>
        [Category("VBControl")]
        public string CodeCompletionSchema
        {
            get
            {
                return _CodeCompletionSchema;
            }
            set
            {
                _CodeCompletionSchema = value;
            }
        }

        #endregion

        #region Methods

        #region Methods => Completion

        protected override void ucAvalonTextEditor_TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (_xmlCompletionDataProvider != null && _xmlCompletionDataProvider.IsSchemaLoaded)
            {
                int _CurrCaretIndex = TextArea.Caret.Offset;
                char ch = e.Text.FirstOrDefault();

                if (completionWindow != null)
                {
                    if (!char.IsLetterOrDigit(ch))
                    {
                        switch (ch)
                        {
                            case '>':
                                CompleteTag();
                                break;

                            case ' ':
                                ShowCompletionWindow(ch);
                                break;

                            case '=':
                                InsertStringAtCaret("\"\"");
                                TextArea.Caret.Column = TextArea.Caret.Column + 1;
                                break;

                            case '/':
                                break;

                            case ':':
                                string text = e.Text;
                                if (completionWindow.CompletionList != null && completionWindow.CompletionList.SelectedItem != null)
                                    text = completionWindow.CompletionList.SelectedItem.Text;
                                ShowCompletionWindow(text);
                                break;
                        }
                    }
                }
                else
                {
                    switch (ch)
                    {
                        case '<':
                            ShowCompletionWindow(ch);
                            break;

                        case '>':
                            CompleteTag();
                            break;

                        case '=':
                            if (!XmlParser.IsInsideAttributeValue(Text, _CurrCaretIndex))
                            {
                                int column = TextArea.Caret.Column + 1;
                                InsertStringAtCaret("\"\"");
                                TextArea.Caret.Column = column;
                                ShowCompletionWindow('\"');
                            }
                            break;

                        case ':':
                            break;

                        default:
                            if (XmlParser.IsAttributeValueChar(ch))
                            {
                                if (IsInsideQuotes(TextArea))
                                {
                                    ShowCompletionWindow(ch);
                                    break;
                                }
                                else if (Char.IsLetter(ch))
                                {
                                    ShowCompletionWindow(ch);
                                    break;
                                }
                            }
                            break;
                    }
                }
            }
        }

        internal void ShowCompletionWindow(char ch)
        {
            var result = _xmlCompletionDataProvider.GenerateCompletionData("", this.TextArea, ch);
            if (result != null && result.Any())
            {
                completionWindow = new CompletionWindow(TextArea);
                completionWindow.MinWidth = 100;
                completionWindow.SizeToContent = SizeToContent.Width;

                foreach (XmlCompletionData item in result.OrderBy(c => c.Text))
                {
                    if (item.Text.EndsWith(":"))
                        item.vBXMLEditor = this;
                    completionWindow.CompletionList.CompletionData.Add(item);
                }

                if (ch == '<')
                    completionWindow.CompletionList.CompletionData.Add(new XmlCompletionData("!--", "Comment"));
                else if (Char.IsLetter(ch))
                {
                    completionWindow.StartOffset--;
                    completionWindow.CompletionList.SelectItem(ch.ToString());
                }

                completionWindow.Show();
                completionWindow.Closed += delegate
                {
                    completionWindow = null;
                };

            }
        }

        internal void ShowCompletionWindow(string text)
        {
            if (!text.Contains(":"))
                return;

            string prefix = text.ToString().Substring(0, text.ToString().IndexOf(':'));
            ICompletionData[] result = _xmlCompletionDataProvider.GenerateCompletionData(prefix, TextArea, ':');

            if (result != null && result.Any())
            {
                completionWindow = new CompletionWindow(TextArea);
                completionWindow.MinWidth = 100;
                completionWindow.SizeToContent = SizeToContent.Width;
                foreach (var item in result)
                    completionWindow.CompletionList.CompletionData.Add(item);

                completionWindow.Show();
                completionWindow.Closed += delegate { completionWindow = null; };
            }
        }

        void CompleteTag()
        {
            int caret = TextArea.Caret.Offset - 1;
            int begin = XmlParser.GetActiveElementStartIndex(this.Text, caret);
            int end = begin + 1;

            if (Text[caret - 1] == '/' || Text.Substring(caret - 2, 3) == "-->" || Text[caret - 1] == '>')
                return;
            else
            {
                int start = XmlParser.GetActiveElementStartIndex(this.Text, caret);

                // bail if we are either in a comment or if we are completing a "closing" tag
                if (Text[start + 1] == '/' || Text[start + 1] == '!') return;

                begin++;
                while (end < Text.Length && !char.IsWhiteSpace(Text[end]) && end < caret) end++;

                int column = TextArea.Caret.Column;
                InsertStringAtCaret("</" + Text.Substring(begin, end - begin) + ">");

                TextArea.Caret.Column = column;
                TextArea.Caret.BringCaretToView();
            }
        }

        public void InsertStringAtCaret(string s)
        {
            TextArea.Document.BeginUpdate();
            TextArea.Document.Insert(TextArea.Caret.Offset, s);
            TextArea.Document.EndUpdate();
        }

        bool IsInsideQuotes(TextArea textArea)
        {
            bool inside = false;

            ICSharpCode.AvalonEdit.Document.DocumentLine line = textArea.Document.GetLineByOffset(textArea.Caret.Offset);
            if (line != null)
            {
                if ((line.Offset + line.Length > textArea.Caret.Offset) &&
                    (line.Offset < textArea.Caret.Offset))
                {

                    char charAfter = textArea.Document.GetCharAt(textArea.Caret.Offset);
                    char charBefore = textArea.Document.GetCharAt(textArea.Caret.Offset - 1);

                    if (((charBefore == '\'') && (charAfter == '\'')) ||
                        ((charBefore == '\"') && (charAfter == '\"')))
                    {
                        inside = true;
                    }
                }
            }

            return inside;
        }

        #endregion

        #region Methods => Find & Replace

        private void CanOpenFindAndReplace(object sender, CanExecuteRoutedEventArgs e)
        {
            if (_VBFindAndReplace != null && _VBFindAndReplace.InitState == ACInitState.Destructed)
                _VBFindAndReplace = null;

            if (_VBFindAndReplace == null)
                e.CanExecute = true;
            else
            {
                e.CanExecute = false;
                SetSelectedTextToCombo();
            }
            e.Handled = true;
        }

        private void OpenFindAndReplace(object sender, ExecutedRoutedEventArgs e)
        {
            InstanceVBFindAndReplace();
        }

        private void SetSelectedTextToCombo()
        {
            if (_VBFindAndReplace != null)
            {
                _VBFindAndReplace.ACUrlCommand("!UpdateFindTextFromSelection");
            }
        }

        #endregion

        #region Methods => Folding & Highlighting

        /// <summary>
        /// Changes the syntax highlighting.
        /// </summary>
        protected override void ChangeSyntaxHighlighting()
        {
            if (SyntaxHighlighting == null)
            {
                foldingStrategy = null;
            }
            else
            {
                switch (SyntaxHighlighting.Name)
                {
                    case "XML":
                        foldingStrategy = new XmlFoldingStrategy();
                        TextArea.IndentationStrategy = new DefaultIndentationStrategy();
                        break;
                    case "C#":
                        break;
                    case "C++":
                    case "PHP":
                    default:
                        TextArea.IndentationStrategy = new DefaultIndentationStrategy();
                        foldingStrategy = null;
                        break;
                }
            }
            if (foldingStrategy != null)
            {
                if (foldingManager == null)
                    foldingManager = FoldingManager.Install(TextArea);
                foldingStrategy.UpdateFoldings(foldingManager, Document);
            }
            else
            {
                if (foldingManager != null)
                {
                    FoldingManager.Uninstall(foldingManager);
                    foldingManager = null;
                }
            }
        }

        protected override void foldingUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (foldingStrategy != null)
            {
                int firstErrorOffset;
                IEnumerable<NewFolding> foldings = foldingStrategy.CreateNewFoldings(Document, out firstErrorOffset);
                foldingManager.UpdateFoldings(foldings, firstErrorOffset);
            }
        }

        #endregion

        #endregion
    }
}
