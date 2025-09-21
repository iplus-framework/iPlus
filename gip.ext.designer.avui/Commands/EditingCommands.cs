// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using Avalonia.Labs.Input;
using System.Windows.Input;

namespace gip.ext.designer.avui
{
	/// <summary>
	/// Description of Commands.
	/// </summary>
	public static class EditingCommands
    {
        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.ToggleInsert command,
        //     which toggles the typing mode between Insert and Overtype.
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Insert.
        public readonly static RoutedCommand ToggleInsert = new RoutedCommand(nameof(ToggleInsert));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.Delete command, which
        //     requests that the current selection be deleted.
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Delete.
        public readonly static RoutedCommand Delete = new RoutedCommand(nameof(Delete));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.Backspace command, which
        //     requests that a backspace be entered at the current position or over the current
        //     selection.
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Backspace.
        public readonly static RoutedCommand Backspace = new RoutedCommand(nameof(Backspace));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.DeleteNextWord command,
        //     which requests that the next word (relative to a current position) be deleted.
        //
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Ctrl+Delete.
        public readonly static RoutedCommand DeleteNextWord = new RoutedCommand(nameof(DeleteNextWord));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.DeletePreviousWord command,
        //     which requests that the previous word (relative to a current position) be deleted.
        //
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Ctrl+Backspace.
        public readonly static RoutedCommand DeletePreviousWord = new RoutedCommand(nameof(DeletePreviousWord));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.EnterParagraphBreak command,
        //     which requests that a paragraph break be inserted at the current position or
        //     over the current selection.
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Enter.
        public readonly static RoutedCommand EnterParagraphBreak = new RoutedCommand(nameof(EnterParagraphBreak));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.EnterLineBreak command,
        //     which requests that a line break be inserted at the current position or over
        //     the current selection.
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Shift+Enter.
        public readonly static RoutedCommand EnterLineBreak = new RoutedCommand(nameof(EnterLineBreak));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.TabForward command.
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Tab.
        public readonly static RoutedCommand TabForward = new RoutedCommand(nameof(TabForward));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.TabBackward command.
        //
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Shift+Tab.
        public readonly static RoutedCommand TabBackward = new RoutedCommand(nameof(TabBackward));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.MoveRightByCharacter
        //     command, which requests that the caret move one character right.
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Right.
        public readonly static RoutedCommand MoveRightByCharacter = new RoutedCommand(nameof(MoveRightByCharacter));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.MoveLeftByCharacter command,
        //     which requests that the caret move one character left.
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Left.
        public readonly static RoutedCommand MoveLeftByCharacter = new RoutedCommand(nameof(MoveLeftByCharacter));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.MoveRightByWord command,
        //     which requests that the caret move right by one word.
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Ctrl+Right.
        public readonly static RoutedCommand MoveRightByWord = new RoutedCommand(nameof(MoveRightByWord));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.MoveLeftByWord command,
        //     which requests that the caret move one word left.
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Ctrl+Left.
        public readonly static RoutedCommand MoveLeftByWord = new RoutedCommand(nameof(MoveLeftByWord));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.MoveDownByLine command,
        //     which requests that the caret move down by one line.
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Down.
        public readonly static RoutedCommand MoveDownByLine = new RoutedCommand(nameof(MoveDownByLine));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.MoveUpByLine command,
        //     which requests that the caret move up by one line.
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Up.
        public readonly static RoutedCommand MoveUpByLine = new RoutedCommand(nameof(MoveUpByLine));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.MoveDownByParagraph command,
        //     which requests that the caret move down by one paragraph.
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Ctrl+Down.
        public readonly static RoutedCommand MoveDownByParagraph = new RoutedCommand(nameof(MoveDownByParagraph));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.MoveUpByParagraph command,
        //     which requests that the caret move up by one paragraph.
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Ctrl+Up.
        public readonly static RoutedCommand MoveUpByParagraph = new RoutedCommand(nameof(MoveUpByParagraph));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.MoveDownByPage command,
        //     which requests that the caret move down by one page.
        //
        // Returns:
        //     The requested command. The default key gesture for this command is PageDown.
        public readonly static RoutedCommand MoveDownByPage = new RoutedCommand(nameof(MoveDownByPage));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.MoveUpByPage command,
        //     which requests that the caret move up by one page.
        //
        // Returns:
        //     The requested command. The default key gesture for this command is PageUp.
        public readonly static RoutedCommand MoveUpByPage = new RoutedCommand(nameof(MoveUpByPage));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.MoveToLineStart command,
        //     which requests that the caret move to the beginning of the current line.
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Home.
        public readonly static RoutedCommand MoveToLineStart = new RoutedCommand(nameof(MoveToLineStart));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.MoveToLineEnd command,
        //     which requests that the caret move to the end of the current line.
        //
        // Returns:
        //     The requested command. The default key gesture for this command is End.
        public readonly static RoutedCommand MoveToLineEnd = new RoutedCommand(nameof(MoveToLineEnd));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.MoveToDocumentStart command,
        //     which requests that the caret move to the very beginning of content.
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Ctrl+Home.
        public readonly static RoutedCommand MoveToDocumentStart = new RoutedCommand(nameof(MoveToDocumentStart));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.MoveToDocumentEnd command,
        //     which requests that the caret move to the very end of content.
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Ctrl+End.
        public readonly static RoutedCommand MoveToDocumentEnd = new RoutedCommand(nameof(MoveToDocumentEnd));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.SelectRightByCharacter
        //     command, which requests that the current selection be expanded right by one character.
        //
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Shift+Right.
        public readonly static RoutedCommand SelectRightByCharacter = new RoutedCommand(nameof(SelectRightByCharacter));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.SelectLeftByCharacter
        //     command, which requests that the current selection be expanded left by one character.
        //
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Shift+Left.
        public readonly static RoutedCommand SelectLeftByCharacter = new RoutedCommand(nameof(SelectLeftByCharacter));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.SelectRightByWord command,
        //     which requests that the current selection be expanded right by one word.
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Ctrl+Shift+Right.
        public readonly static RoutedCommand SelectRightByWord = new RoutedCommand(nameof(SelectRightByWord));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.SelectLeftByWord command,
        //     which requests that the current selection be expanded left by one word.
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Ctrl+Shift+Left.
        public readonly static RoutedCommand SelectLeftByWord = new RoutedCommand(nameof(SelectLeftByWord));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.SelectDownByLine command,
        //     which requests that the current selection be expanded down by one line.
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Shift+Down.
        public readonly static RoutedCommand SelectDownByLine = new RoutedCommand(nameof(SelectDownByLine));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.SelectUpByLine command,
        //     which requests that the current selection be expanded up by one line.
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Shift+Up.
        public readonly static RoutedCommand SelectUpByLine = new RoutedCommand(nameof(SelectUpByLine));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.SelectDownByParagraph
        //     command, which requests that the current selection be expanded down by one paragraph.
        //
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Ctrl+Shift+Down.
        public readonly static RoutedCommand SelectDownByParagraph = new RoutedCommand(nameof(SelectDownByParagraph));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.SelectUpByParagraph command,
        //     which requests that the current selection be expanded up by one paragraph.
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Ctrl+Shift+Up.
        public readonly static RoutedCommand SelectUpByParagraph = new RoutedCommand(nameof(SelectUpByParagraph));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.SelectDownByPage command,
        //     which requests that the current selection be expanded down by one page.
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Shift+PageDown.
        public readonly static RoutedCommand SelectDownByPage = new RoutedCommand(nameof(SelectDownByPage));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.SelectUpByPage command,
        //     which requests that the current selection be expanded up by one page.
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Shift+PageUp.
        public readonly static RoutedCommand SelectUpByPage = new RoutedCommand(nameof(SelectUpByPage));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.SelectToLineStart command,
        //     which requests that the current selection be expanded to the beginning of the
        //     current line.
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Shift+Home.
        public readonly static RoutedCommand SelectToLineStart = new RoutedCommand(nameof(SelectToLineStart));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.SelectToLineEnd command,
        //     which requests that the current selection be expanded to the end of the current
        //     line.
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Shift+End.
        public readonly static RoutedCommand SelectToLineEnd = new RoutedCommand(nameof(SelectToLineEnd));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.SelectToDocumentStart
        //     command, which requests that the current selection be expanded to the very beginning
        //     of content.
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Ctrl+Shift+Home.
        public readonly static RoutedCommand SelectToDocumentStart = new RoutedCommand(nameof(SelectToDocumentStart));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.SelectToDocumentEnd command,
        //     which requests that the current selection be expanded to the very end of content.
        //
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Ctrl+Shift+End.
        public readonly static RoutedCommand SelectToDocumentEnd = new RoutedCommand(nameof(SelectToDocumentEnd));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.ToggleBold command, which
        //     requests that System.Windows.Documents.Bold formatting be toggled on the current
        //     selection.
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Ctrl+B.
        public readonly static RoutedCommand ToggleBold = new RoutedCommand(nameof(ToggleBold));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.ToggleItalic command,
        //     which requests that System.Windows.Documents.Italic formatting be toggled on
        //     the current selection.
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Ctrl+I.
        public readonly static RoutedCommand ToggleItalic = new RoutedCommand(nameof(ToggleItalic));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.ToggleUnderline command,
        //     which requests that System.Windows.Documents.Underline formatting be toggled
        //     on the current selection.
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Ctrl+U.
        public readonly static RoutedCommand ToggleUnderline = new RoutedCommand(nameof(ToggleUnderline));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.ToggleSubscript command,
        //     which requests that subscript formatting be toggled on the current selection.
        //
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Ctrl+OemPlus.
        public readonly static RoutedCommand ToggleSubscript = new RoutedCommand(nameof(ToggleSubscript));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.ToggleSuperscript command,
        //     which requests that superscript formatting be toggled on the current selection.
        //
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Ctrl+Shift+OemPlus.
        public readonly static RoutedCommand ToggleSuperscript = new RoutedCommand(nameof(ToggleSuperscript));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.IncreaseFontSize command,
        //     which requests that the font size for the current selection be increased by 1
        //     point.
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Ctrl+OemCloseBrackets.
        public readonly static RoutedCommand IncreaseFontSize = new RoutedCommand(nameof(IncreaseFontSize));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.DecreaseFontSize command,
        //     which requests that the font size for the current selection be decreased by 1
        //     point.
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Ctrl+OemOpenBrackets.
        public readonly static RoutedCommand DecreaseFontSize = new RoutedCommand(nameof(DecreaseFontSize));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.AlignLeft command, which
        //     requests that a selection of content be aligned left.
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Ctrl+L.
        public readonly static RoutedCommand AlignLeft = new RoutedCommand(nameof(AlignLeft));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.AlignCenter command,
        //     which requests that the current paragraph or a selection of paragraphs be centered.
        //
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Ctrl+E.
        public readonly static RoutedCommand AlignCenter = new RoutedCommand(nameof(AlignCenter));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.AlignRight command, which
        //     requests that a selection of content be aligned right.
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Ctrl+R.
        public readonly static RoutedCommand AlignRight = new RoutedCommand(nameof(AlignRight));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.AlignJustify command,
        //     which requests that the current paragraph or a selection of paragraphs be justified.
        //
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Ctrl+J.
        public readonly static RoutedCommand AlignJustify = new RoutedCommand(nameof(AlignJustify));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.ToggleBullets command,
        //     which requests that unordered list (also referred to as bulleted list) formatting
        //     be toggled on the current selection.
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Ctrl+Shift+L.
        public readonly static RoutedCommand ToggleBullets = new RoutedCommand(nameof(ToggleBullets));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.ToggleNumbering command,
        //     which requests that ordered list (also referred to as numbered list) formatting
        //     be toggled on the current selection.
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Ctrl+Shift+N.
        public readonly static RoutedCommand ToggleNumbering = new RoutedCommand(nameof(ToggleNumbering));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.IncreaseIndentation command,
        //     which requests that indentation for the current paragraph be increased by one
        //     tab stop.
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Ctrl+T.
        public readonly static RoutedCommand IncreaseIndentation = new RoutedCommand(nameof(IncreaseIndentation));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.DecreaseIndentation command,
        //     which requests that indentation for the current paragraph be decreased by one
        //     tab stop.
        //
        // Returns:
        //     The requested command. The default key gesture for this command is Ctrl+Shift+T.
        public readonly static RoutedCommand DecreaseIndentation = new RoutedCommand(nameof(DecreaseIndentation));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.CorrectSpellingError
        //     command, which requests that any misspelled word at the current position be corrected.
        //
        //
        // Returns:
        //     The requested command. This command has no default key gesture.
        public readonly static RoutedCommand CorrectSpellingError = new RoutedCommand(nameof(CorrectSpellingError));

        //
        // Summary:
        //     Represents the System.Windows.Documents.EditingCommands.IgnoreSpellingError command,
        //     which requests that any instances of misspelled words at the current position
        //     or in the current selection be ignored.
        //
        // Returns:
        //     The requested command. This command has no default key gesture.
        public readonly static RoutedCommand IgnoreSpellingError = new RoutedCommand(nameof(IgnoreSpellingError));
    }
}
