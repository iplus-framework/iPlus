// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using Avalonia.Labs.Input;
using System.Windows.Input;

namespace gip.ext.designer.avui
{
	/// <summary>
	/// Description of Commands.
	/// </summary>
	public static class MediaCommands
    {
        // Typing Commands
        // ---------------
        /// <summary>
        /// ToggleInsert command.
        /// Changed typing mode between insertion and overtyping.
        /// </summary>
        public readonly static RoutedCommand ToggleInsert = new RoutedCommand(nameof(ToggleInsert));

        /// <summary>
        /// Delete command.
        /// When selection is empty deletes the following character or paragraph separator.
        /// When selection is not empty deletes the selected content.
        /// Formatting of deleted content is not springloaded (unlike Backspace).
        /// </summary>
        public readonly static RoutedCommand Delete = new RoutedCommand(nameof(Delete));

        /// <summary>
        /// Backspace command.
        /// When selection is empty deleted the previous character or paragraph separator.
        /// When selection is not empty deletes the selected content.
        /// Formatting of deleted content is springloaded (unlike Delete).
        /// Formatting for springloading is taken from the very first selected character.
        /// </summary>
        public readonly static RoutedCommand Backspace = new RoutedCommand(nameof(Backspace));

        /// <summary>
        /// DeleteNextWord command.
        /// </summary>
        public readonly static RoutedCommand DeleteNextWord = new RoutedCommand(nameof(DeleteNextWord));

        /// <summary>
        /// DeletePreviousWord command.
        /// </summary>
        public readonly static RoutedCommand DeletePreviousWord = new RoutedCommand(nameof(DeletePreviousWord));

        /// <summary>
        /// EnterParagraphBreak command.
        /// Acts as if the user presses Enter key. The content of current selection is deleted (if not empty)
        /// like with Backspace command(performing formatting springloading), and then
        /// the structure of text elements is changed so that paragraph break appears
        /// at caret position.
        /// </summary>
        public readonly static RoutedCommand EnterParagraphBreak = new RoutedCommand(nameof(EnterParagraphBreak));

        /// <summary>
        /// EnterLineBreak command.
        /// </summary>
        public readonly static RoutedCommand EnterLineBreak = new RoutedCommand(nameof(EnterLineBreak));

        /// <summary>
        /// TabForward command.
        /// The behavior depends from the current condition of selection.
        /// If selection is non-empty then it redirects to a IncreaseIndentation command,
        /// so that all affected paragraphs become promoted (by increasing their Margin.Left property in RichTextBox
        /// or by inserting additional Tab charater in the beginning of each non-wrapped line).
        /// If the caret is in table cell then it moves to the next cell.
        /// If the caret is in the last table cell, then in creates new row in a table and moves
        /// the caret into first cell of that row.
        /// Otherwise Tab character is inserted in current position.
        /// </summary>
        public readonly static RoutedCommand TabForward = new RoutedCommand(nameof(TabForward));

        /// <summary>
        /// TabBackward command.
        /// The behavior depends from the current condition of selection.
        /// If selection is non-empty then it redirects to a DecreaseIndentation command,
        /// so that all affected paragraphs become promoted (by decreasing their Margin.Left property in RichTextBox
        /// or by deleting a Tab charater in the beginning of each non-wrapped line).
        /// If the caret is in table cell then it moves to the previous cell.
        /// Otherwise Tab character is inserted in current position.
        /// </summary>
        public readonly static RoutedCommand TabBackward = new RoutedCommand(nameof(TabBackward));

        // Caret navigation commands
        // -------------------------
        /// <summary>
        /// MoveRightByCharacter command.
        /// </summary>
        public readonly static RoutedCommand MoveRightByCharacter = new RoutedCommand(nameof(MoveRightByCharacter));

        /// <summary>
        /// MoveLeftByCharacter command.
        /// </summary>
        public readonly static RoutedCommand MoveLeftByCharacter = new RoutedCommand(nameof(MoveLeftByCharacter));

        /// <summary>
        /// MoveRightByWord command.
        /// </summary>
        public readonly static RoutedCommand MoveRightByWord = new RoutedCommand(nameof(MoveRightByWord));

        /// <summary>
        /// MoveLeftByWord command.
        /// </summary>
        public readonly static RoutedCommand MoveLeftByWord = new RoutedCommand(nameof(MoveLeftByWord));

        /// <summary>
        /// MoveDownByLine command.
        /// </summary>
        public readonly static RoutedCommand MoveDownByLine = new RoutedCommand(nameof(MoveDownByLine));

        /// <summary>
        /// MoveUpByLine command.
        /// </summary>
        public readonly static RoutedCommand MoveUpByLine = new RoutedCommand(nameof(MoveUpByLine));

        /// <summary>
        /// MoveDownByParagraph command.
        /// </summary>
        public readonly static RoutedCommand MoveDownByParagraph = new RoutedCommand(nameof(MoveDownByParagraph));

        /// <summary>
        /// MoveUpByParagraph command.
        /// </summary>
        public readonly static RoutedCommand MoveUpByParagraph = new RoutedCommand(nameof(MoveUpByParagraph));

        /// <summary>
        /// MoveDownByPage command.
        /// Corresponds to PgDn key on the keyboard.
        /// </summary>
        public readonly static RoutedCommand MoveDownByPage = new RoutedCommand(nameof(MoveDownByPage));

        /// <summary>
        /// MoveUpByPage command.
        /// Corresponds to PgUp key on the keyboard.
        /// </summary>
        public readonly static RoutedCommand MoveUpByPage = new RoutedCommand(nameof(MoveUpByPage));

        /// <summary>
        /// MoveToLineStart command.
        /// Corresponds to Home key on the keyboard.
        /// </summary>
        public readonly static RoutedCommand MoveToLineStart = new RoutedCommand(nameof(MoveToLineStart));

        /// <summary>
        /// MoveToLineEnd command.
        /// Corresponds to End key on the keyboard.
        /// </summary>
        public readonly static RoutedCommand MoveToLineEnd = new RoutedCommand(nameof(MoveToLineEnd));

        /// <summary>
        /// MoveToDocumentStart command.
        /// </summary>
        public readonly static RoutedCommand MoveToDocumentStart = new RoutedCommand(nameof(MoveToDocumentStart));

        /// <summary>
        /// MoveToDocumentEnd command.
        /// </summary>
        public readonly static RoutedCommand MoveToDocumentEnd = new RoutedCommand(nameof(MoveToDocumentEnd));

        // Selection extension commands
        // ----------------------------

        /// <summary>
        /// SelectRightByCharacter command.
        /// </summary>
        public readonly static RoutedCommand SelectRightByCharacter = new RoutedCommand(nameof(SelectRightByCharacter));

        /// <summary>
        /// SelectLeftByCharacter command.
        /// </summary>
        public readonly static RoutedCommand SelectLeftByCharacter = new RoutedCommand(nameof(SelectLeftByCharacter));

        /// <summary>
        /// SelectRightByWord command.
        /// </summary>
        public readonly static RoutedCommand SelectRightByWord = new RoutedCommand(nameof(SelectRightByWord));

        /// <summary>
        /// SelectLeftbyWord command.
        /// </summary>
        public readonly static RoutedCommand SelectLeftByWord = new RoutedCommand(nameof(SelectLeftByWord));

        /// <summary>
        /// SelectDownByLine command.
        /// </summary>
        public readonly static RoutedCommand SelectDownByLine = new RoutedCommand(nameof(SelectDownByLine));

        /// <summary>
        /// SelectUpByLine command.
        /// </summary>
        public readonly static RoutedCommand SelectUpByLine = new RoutedCommand(nameof(SelectUpByLine));

        /// <summary>
        /// SelectDownByParagraph command.
        /// </summary>
        public readonly static RoutedCommand SelectDownByParagraph = new RoutedCommand(nameof(SelectDownByParagraph));

        /// <summary>
        /// SelectUpByParagraph command.
        /// </summary>
        public readonly static RoutedCommand SelectUpByParagraph = new RoutedCommand(nameof(SelectUpByParagraph));

        /// <summary>
        /// SelectDownByPage command.
        /// </summary>
        public readonly static RoutedCommand SelectDownByPage = new RoutedCommand(nameof(SelectDownByPage));

        /// <summary>
        /// SelectUpByPage command.
        /// </summary>
        public readonly static RoutedCommand SelectUpByPage = new RoutedCommand(nameof(SelectUpByPage));

        /// <summary>
        /// SelectToLineStart command.
        /// </summary>
        public readonly static RoutedCommand SelectToLineStart = new RoutedCommand(nameof(SelectToLineStart));

        /// <summary>
        /// SelectToLineEnd command.
        /// </summary>
        public readonly static RoutedCommand SelectToLineEnd = new RoutedCommand(nameof(SelectToLineEnd));

        /// <summary>
        /// SelectToDocumentStart command.
        /// </summary>
        public readonly static RoutedCommand SelectToDocumentStart = new RoutedCommand(nameof(SelectToDocumentStart));

        /// <summary>
        /// SelectToDocumentEnd command.
        /// </summary>
        public readonly static RoutedCommand SelectToDocumentEnd = new RoutedCommand(nameof(SelectToDocumentEnd));

        // Character editing commands
        // --------------------------

        /// <summary>
        /// ToggleBold command.
        /// When command argument is present applies provided value to a selected range.
        /// When command argument is null applies an value of FontWeight opposite to the one taken from the first
        /// character of selected range.
        /// When selection is empty and within a word, the same operation is applied to this word.
        /// When empty selection is between words or in the process of typing
        /// the property is springloaded.
        /// </summary>
        public readonly static RoutedCommand ToggleBold = new RoutedCommand(nameof(ToggleBold));

        /// <summary>
        /// ToggleItalic command.
        /// </summary>
        public readonly static RoutedCommand ToggleItalic = new RoutedCommand(nameof(ToggleItalic));

        /// <summary>
        /// ToggleUnderline command.
        /// </summary>
        public readonly static RoutedCommand ToggleUnderline = new RoutedCommand(nameof(ToggleUnderline));

        /// <summary>
        /// ToggleSubscript command.
        /// </summary>
        public readonly static RoutedCommand ToggleSubscript = new RoutedCommand(nameof(ToggleSubscript));

        /// <summary>
        /// ToggleSuperscript command.
        /// </summary>
        public readonly static RoutedCommand ToggleSuperscript = new RoutedCommand(nameof(ToggleSuperscript));

        /// <summary>
        /// IncreaseFontSize command.
        /// </summary>
        public readonly static RoutedCommand IncreaseFontSize = new RoutedCommand(nameof(IncreaseFontSize));

        /// <summary>
        /// DecreaseFontSize command.
        /// </summary>
        public readonly static RoutedCommand DecreaseFontSize = new RoutedCommand(nameof(DecreaseFontSize));

        // Paragraph editing commands
        // --------------------------

        /// <summary>
        /// AlignLeft command.
        /// </summary>
        public readonly static RoutedCommand AlignLeft = new RoutedCommand(nameof(AlignLeft));

        /// <summary>
        /// AlightCenter command.
        /// </summary>
        public readonly static RoutedCommand AlignCenter = new RoutedCommand(nameof(AlignCenter));

        /// <summary>
        /// AlignRight command.
        /// </summary>
        public readonly static RoutedCommand AlignRight = new RoutedCommand(nameof(AlignRight));

        /// <summary>
        /// AlignJustify command.
        /// </summary>
        public readonly static RoutedCommand AlignJustify = new RoutedCommand(nameof(AlignJustify));

        // List editing commands
        // ---------------------

        /// <summary>
        /// ToggelBullets command.
        /// When command argument is present it must be of ListMarkerStyle value;
        /// this value is set as a marker style to all selected list items.
        /// When command argument is null the command toggles a marker style
        /// by circle over all predefined non-numeric list marker styles.
        /// The circle includes no-list state as well.
        /// </summary>
        public readonly static RoutedCommand ToggleBullets = new RoutedCommand(nameof(ToggleBullets));

        /// <summary>
        /// ToggelNumbers command.
        /// When command argument is present it must be of ListMarkerStyle value;
        /// this value is set as a marker style to all selected list items.
        /// When command argument is null the command toggles a marker style
        /// by circle over all predefined numeric list marker styles
        /// The circle includes no-list state as well.
        /// </summary>
        public readonly static RoutedCommand ToggleNumbering = new RoutedCommand(nameof(ToggleNumbering));

        /// <summary>
        /// IncreaseIndentation command.
        /// </summary>
        public readonly static RoutedCommand IncreaseIndentation = new RoutedCommand(nameof(IncreaseIndentation));

        /// <summary>
        /// DecreaseIndentation command.
        /// </summary>
        public readonly static RoutedCommand DecreaseIndentation = new RoutedCommand(nameof(DecreaseIndentation));

        // Spelling commands
        // ---------------------

        /// <summary>
        /// Corrects a misspelled word at the insertion position.
        /// </summary>
        public readonly static RoutedCommand CorrectSpellingError = new RoutedCommand(nameof(CorrectSpellingError));

        /// <summary>
        /// Ignores all instances of the misspelled word at the insertion position.
        /// </summary>
        public readonly static RoutedCommand IgnoreSpellingError = new RoutedCommand(nameof(IgnoreSpellingError));
    }
}
