using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using gip.core.datamodel;
using System.Runtime.InteropServices;
using ICSharpCode.AvalonEdit.Editing;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Find and Replace-Handler for Text-Editor
    /// </summary>
    public class FindAndReplaceHandler : IVBFindAndReplace
    {
        private ICSharpCode.AvalonEdit.Editing.TextArea _TextArea;
        private Regex regex;
        private Match match;
        private bool newStartOfFind = true;

        /// <summary>
        /// Default-Contructor
        /// </summary>
        /// <param name="area"></param>
        public FindAndReplaceHandler(ICSharpCode.AvalonEdit.Editing.TextArea area)
        {
            _TextArea = area;
            _TextArea.Caret.PositionChanged += textArea_Caret_PositionChanged;
        }

        /// <summary>
        /// To reset the Postion aof the current Search
        /// </summary>
        public void NewStartOfFind()
        {
            newStartOfFind = true;
        }

        void textArea_Caret_PositionChanged(object sender, EventArgs e)
        {
            NewStartOfFind();
        }

        public void UngegisterEvents()
        {
            if (_TextArea != null)
                _TextArea.Caret.PositionChanged -= textArea_Caret_PositionChanged;
        }

        public ICSharpCode.AvalonEdit.Editing.TextArea TextArea
        {
            get
            {
                return _TextArea;
            }
            set
            {
                if ((_TextArea != null) && (_TextArea != value))
                    _TextArea.Caret.PositionChanged -= textArea_Caret_PositionChanged;
                if ((value != null) && (_TextArea != value))
                    value.Caret.PositionChanged += textArea_Caret_PositionChanged;
                _TextArea = value;
            }
        }

        /// <summary>
        /// Called when User Clicks Next-Button
        /// </summary>
        /// <returns>FindAndReplaceResult</returns>
        public FindAndReplaceResult FindNext()
        {
            if (_TextArea == null)
                return FindAndReplaceResult.WordNotFound;
            FindAndReplaceResult Found = FindAndReplaceResult.WordFound;
            // Falls neue Suche
            if (newStartOfFind)
            {
                regex = GetRegExpression();
                match = regex.Match(_TextArea.Document.Text, _TextArea.Caret.Offset);
                newStartOfFind = false;
            }
            else
            {
                match = regex.Match(_TextArea.Document.Text, match.Index + 1);
            }

            // Falls Suchergebnis gefunden
            if (match.Success)
            {
                //SimpleSegment selection = new SimpleSegment(match.Index, match.Length);
                ICSharpCode.AvalonEdit.Document.TextLocation location = _TextArea.Document.GetLocation(match.Index + match.Length);
                _TextArea.Caret.Position = new ICSharpCode.AvalonEdit.TextViewPosition(location, -1);
                _TextArea.Caret.DesiredXPos = double.NaN;

                _TextArea.Selection = ICSharpCode.AvalonEdit.Editing.Selection.Create(TextArea, match.Index, match.Index + match.Length);
                //_TextArea.Selection = new ICSharpCode.AvalonEdit.Editing.SimpleSelection(_TextArea, match.Index, match.Index + match.Length);

                _TextArea.Caret.BringCaretToView();
            }
            else
            {
                _TextArea.ClearSelection();
                //_TextArea.Selection = new ICSharpCode.AvalonEdit.Editing.EmptySelection(_TextArea);
                _TextArea.Caret.Offset = 0;
                Found = FindAndReplaceResult.WordNotFound;
                newStartOfFind = true;
            }
            return Found;
        }

        /// <summary>
        /// Called when User Clicks Replace-Button
        /// </summary>
        /// <returns>FindAndReplaceResult</returns>
        public FindAndReplaceResult Replace()
        {
            if (_TextArea == null)
                return FindAndReplaceResult.WordNotFound;
            if ((_TextArea.Selection == null) || (_TextArea.Selection.IsEmpty) || newStartOfFind)
                return FindNext();

            FindAndReplaceResult Found = FindAndReplaceResult.Replaced_NextWordFound;

            _TextArea.Selection.ReplaceSelectionWithText(wordReplaceWith);
            //_TextArea.ReplaceSelectionWithText(wordReplaceWith);
            TextArea tx = new TextArea();
            newStartOfFind = true;

            _TextArea.AllowCaretOutsideSelection();
            if (FindNext() == FindAndReplaceResult.WordNotFound)
                Found = FindAndReplaceResult.Replaced_NextWordNotFound;
            return Found;
        }

        /// <summary>
        /// Called when User Clicks Replace-All-Button
        /// </summary>
        /// <returns>FindAndReplaceResult</returns>
        public FindAndReplaceResult ReplaceAll()
        {
            if (_TextArea == null)
                return FindAndReplaceResult.WordNotFound;
            string oldText = _TextArea.Document.Text;
            if (oldText.Length <= 0)
                return FindAndReplaceResult.WordNotFound;
            if ((_TextArea.Selection == null) || (_TextArea.Selection.IsEmpty))
                return FindAndReplaceResult.SelectionRequired;
            //if (!(_TextArea.Selection is ICSharpCode.AvalonEdit.Editing.SimpleSelection))
            //    return FindAndReplaceResult.SelectionRequired;
            if (_TextArea.Selection.Segments == null || !_TextArea.Selection.Segments.Any())
                return FindAndReplaceResult.SelectionRequired;
            var segment = _TextArea.Selection.Segments.FirstOrDefault();

            Regex replaceRegex = GetRegExpression();
            String replacedString;

            int selectedPos = _TextArea.Caret.Offset;
            int startOffset = segment.StartOffset;
            int endOffset = segment.EndOffset;

            String before = "";
            if (startOffset > 0)
                before = oldText.Substring(0, startOffset);
            String after = "";
            if ((endOffset + 1) < oldText.Length)
                after = oldText.Substring((endOffset + 1), (oldText.Length - endOffset - 1));
            String selection = oldText.Substring(startOffset, _TextArea.Selection.Length);
            replacedString = replaceRegex.Replace(selection, wordReplaceWith);

            if (selection != replacedString)
            {
                _TextArea.Document.Text = before + replacedString + after;
                _TextArea.Caret.Offset = selectedPos;
                return FindAndReplaceResult.ReplacedAll;
            }
            else
            {
                return FindAndReplaceResult.WordNotFound;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string GetTextInSelection()
        {
            if (_TextArea == null)
                return "";
            if ((_TextArea.Selection == null) || (_TextArea.Selection.IsEmpty))
                return "";
            //if (!(_TextArea.Selection is ICSharpCode.AvalonEdit.Editing.SimpleSelection))
            //    return "";
            if (_TextArea.Selection.Length <= 0)
                return "";
            if (_TextArea.Selection.Segments == null || !_TextArea.Selection.Segments.Any())
                return "";
            var segment = _TextArea.Selection.Segments.FirstOrDefault();
            int startOffset = segment.StartOffset;
            String selection = _TextArea.Document.Text.Substring(startOffset, _TextArea.Selection.Length);
            return selection;
        }


        #region Options
        private string _wordToFind;
        /// <summary>
        /// 
        /// </summary>
        public string wordToFind
        {
            get
            {
                return _wordToFind;
            }
            set
            {
                if (_wordToFind != value)
                    newStartOfFind = true;
                _wordToFind = value;
            }
        }

        private string _wordReplaceWith;
        /// <summary>
        /// 
        /// </summary>
        public string wordReplaceWith
        {
            get
            {
                return _wordReplaceWith;
            }
            set
            {
                if (_wordReplaceWith != value)
                    newStartOfFind = true;
                _wordReplaceWith = value;
            }
        }

        private bool _OptionCaseSensitive = false;
        /// <summary>
        /// 
        /// </summary>
        public bool OptionCaseSensitive
        {
            get
            {
                return _OptionCaseSensitive;
            }
            set
            {
                if (_OptionCaseSensitive != value)
                    newStartOfFind = true;
                _OptionCaseSensitive = value;
            }
        }

        private bool _OptionFindCompleteWord = false;
        /// <summary>
        /// 
        /// </summary>
        public bool OptionFindCompleteWord
        {
            get
            {
                return _OptionFindCompleteWord;
            }
            set
            {
                if (_OptionFindCompleteWord != value)
                    newStartOfFind = true;
                _OptionFindCompleteWord = value;
            }
        }

        private bool _OptionIsRegularExpr = false;
        /// <summary>
        /// 
        /// </summary>
        public bool OptionIsRegularExpr
        {
            get
            {
                return _OptionIsRegularExpr;
            }
            set
            {
                if (_OptionIsRegularExpr != value)
                    newStartOfFind = true;
                _OptionIsRegularExpr = value;
            }
        }

        private bool _OptionIsWildcard = false;
        /// <summary>
        /// 
        /// </summary>
        public bool OptionIsWildcard
        {
            get
            {
                return _OptionIsWildcard;
            }
            set
            {
                if (_OptionIsWildcard != value)
                    newStartOfFind = true;
                _OptionIsWildcard = value;
            }
        }
        #endregion

        #region private
        private Regex GetRegExpression()
        {
            Regex result;
            String regExString;

            regExString = wordToFind;
            if (regExString.Length <= 0)
                regExString = " ";

            // Falls Reglulärer Ausdruck, dann stimmt der übergebene String
            if (OptionIsRegularExpr)
            {
                regExString = wordToFind;
            }
            // Falls Platzhaltersuche
            else if (OptionIsWildcard)
            {
                // Falls * angegeben: Suche an belibiger Stelle
                regExString = regExString.Replace("*", @"\w*");

                // Falls ? angegeben: Nur an betimmter Position
                regExString = regExString.Replace("?", @"\w");

                // Falls Platzhalter angegeben, dann suche nur nach ganzen Wörtern
                regExString = String.Format("{0}{1}{0}", @"\b", regExString);
            }
            else
            {
                // Sonst ersetze Escape-Zeichen
                regExString = Regex.Escape(regExString);
            }

            // Falls nur ganzes Wort
            if (OptionFindCompleteWord)
            {
                regExString = String.Format("{0}{1}{0}", @"\b", regExString);
            }

            // Falls Gross-undKleinschreibung
            if (OptionCaseSensitive)
            {
                result = new Regex(regExString);
            }
            else
            {
                result = new Regex(regExString, RegexOptions.IgnoreCase);
            }

            return result;
        }
        #endregion
    }
}
