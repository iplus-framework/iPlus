// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using Avalonia.Labs.Input;
using System.Windows.Input;

namespace gip.ext.designer.avui
{
	/// <summary>
	/// Description of Commands.
	/// </summary>
	public static class ComponentCommands
    {
        //
        // Summary:
        //     Gets the value that represents the Scroll Page Up command.
        //
        // Returns:
        //     The command. Default Values Key Gesture PageUp UI Text Scroll Page Up
        public readonly static RoutedCommand ScrollPageUp = new RoutedCommand(nameof(ScrollPageUp));

        //
        // Summary:
        //     Gets the value that represents the Scroll Page Down command.
        //
        // Returns:
        //     The command. Default Values Key Gesture PageDown UI Text Scroll Page Down
        public readonly static RoutedCommand ScrollPageDown = new RoutedCommand(nameof(ScrollPageDown));

        //
        // Summary:
        //     Gets the value that represents the Scroll Page Left command.
        //
        // Returns:
        //     The command. Default Values Key Gesture No gesture defined. UI Text Scroll Page
        //     Left
        public readonly static RoutedCommand ScrollPageLeft = new RoutedCommand(nameof(ScrollPageLeft));

        //
        // Summary:
        //     Gets the value that represents the Scroll Page Right command.
        //
        // Returns:
        //     The command. Default Values Key Gesture No gesture defined. UI Text Scroll Page
        //     Right
        public readonly static RoutedCommand ScrollPageRight = new RoutedCommand(nameof(ScrollPageRight));

        //
        // Summary:
        //     Gets the value that represents the Scroll By Line command.
        //
        // Returns:
        //     The command. Default Values Key Gesture No gesture defined UI Text Scroll By
        //     Line
        public readonly static RoutedCommand ScrollByLine = new RoutedCommand(nameof(ScrollByLine));

        //
        // Summary:
        //     Gets the value that represents the Move Left command.
        //
        // Returns:
        //     The command. Default Values Key Gesture Left UI Text Move Left
        public readonly static RoutedCommand MoveLeft = new RoutedCommand(nameof(MoveLeft));

        //
        // Summary:
        //     Gets the value that represents the Move Right command.
        //
        // Returns:
        //     The command. Default Values Key Gesture Right UI Text Move Right
        public readonly static RoutedCommand MoveRight = new RoutedCommand(nameof(MoveRight));

        //
        // Summary:
        //     Gets the value that represents the Move Up command.
        //
        // Returns:
        //     The command. Default Values Key Gesture Up UI Text Move Up
        public readonly static RoutedCommand MoveUp = new RoutedCommand(nameof(MoveUp));

        //
        // Summary:
        //     Gets the value that represents the Move Down command.
        //
        // Returns:
        //     The command. Default Values Key Gesture Down UI Text Move Down
        public readonly static RoutedCommand MoveDown = new RoutedCommand(nameof(MoveDown));

        //
        // Summary:
        //     Gets the value that represents the Move To Home command.
        //
        // Returns:
        //     The command. Default Values Key Gesture Home UI Text Move To Home
        public readonly static RoutedCommand MoveToHome = new RoutedCommand(nameof(MoveToHome));

        //
        // Summary:
        //     Gets the value that represents the Move To End command.
        //
        // Returns:
        //     The command. Default Values Key Gesture End UI Text Move To End
        public readonly static RoutedCommand MoveToEnd = new RoutedCommand(nameof(MoveToEnd));

        //
        // Summary:
        //     Gets the value that represents the Move To Page Up command.
        //
        // Returns:
        //     The command. Default Values Key Gesture PageUp UI Text Move To Page Up
        public readonly static RoutedCommand MoveToPageUp = new RoutedCommand(nameof(MoveToPageUp));

        //
        // Summary:
        //     Gets the value that represents the Move To Page Down command.
        //
        // Returns:
        //     The command. Default Values Key Gesture PageDown UI Text Move To Page Down
        public readonly static RoutedCommand MoveToPageDown = new RoutedCommand(nameof(MoveToPageDown));

        //
        // Summary:
        //     Gets the value that represents the Extend Selection Up command.
        //
        // Returns:
        //     The command. Default Values Key Gesture Shift+Up UI Text Extend Selection Up
        public readonly static RoutedCommand ExtendSelectionUp = new RoutedCommand(nameof(ExtendSelectionUp));

        //
        // Summary:
        //     Gets the value that represents the Extend Selection Down command.
        //
        // Returns:
        //     The command. Default Values Key Gesture Shift+Down UI Text Extend Selection Down
        public readonly static RoutedCommand ExtendSelectionDown = new RoutedCommand(nameof(ExtendSelectionDown));

        //
        // Summary:
        //     Gets the value that represents the Extend Selection Left command.
        //
        // Returns:
        //     The command. Default Values Key Gesture Shift+Left UI Text Extend Selection Left
        public readonly static RoutedCommand ExtendSelectionLeft = new RoutedCommand(nameof(ExtendSelectionLeft));

        //
        // Summary:
        //     Gets the value that represents the Extend Selection Right command.
        //
        // Returns:
        //     The command. Default Values Key Gesture Shift+Right UI Text Extend Selection
        //     Right
        public readonly static RoutedCommand ExtendSelectionRight = new RoutedCommand(nameof(ExtendSelectionRight));

        //
        // Summary:
        //     Gets the value that represents the Select To Home command.
        //
        // Returns:
        //     The command. Default Values Key Gesture Shift+Home UI Text Select To Home
        public readonly static RoutedCommand SelectToHome = new RoutedCommand(nameof(SelectToHome));

        //
        // Summary:
        //     Gets the value that represents the Select To End command.
        //
        // Returns:
        //     The command. Default Values Key Gesture Shift+End UI Text Select To End
        public readonly static RoutedCommand SelectToEnd = new RoutedCommand(nameof(SelectToEnd));

        //
        // Summary:
        //     Gets the value that represents the Select To Page Up command.
        //
        // Returns:
        //     The command. Default Values Key Gesture Shift+PageUp UI Text Select To Page Up
        public readonly static RoutedCommand SelectToPageUp = new RoutedCommand(nameof(SelectToPageUp));

        //
        // Summary:
        //     Gets the value that represents the Select To Page Down command.
        //
        // Returns:
        //     The command. Default Values Key Gesture Shift+PageDown UI Text Select To Page
        //     Down
        public readonly static RoutedCommand SelectToPageDown = new RoutedCommand(nameof(SelectToPageDown));

        //
        // Summary:
        //     Gets the value that represents the Move Focus Up command.
        //
        // Returns:
        //     The command. Default Values Key Gesture Ctrl+Up UI Text Move Focus Up
        public readonly static RoutedCommand MoveFocusUp = new RoutedCommand(nameof(MoveFocusUp));

        //
        // Summary:
        //     Gets the value that represents the Move Focus Down command.
        //
        // Returns:
        //     The command. Default Values Key Gesture Ctrl+Down UI Text Move Focus Down
        public readonly static RoutedCommand MoveFocusDown = new RoutedCommand(nameof(MoveFocusDown));

        //
        // Summary:
        //     Gets the value that represents the Move Focus Forward command.
        //
        // Returns:
        //     The command. Default Values Key Gesture Ctrl+Right UI Text Move Focus Forward
        public readonly static RoutedCommand MoveFocusForward = new RoutedCommand(nameof(MoveFocusForward));

        //
        // Summary:
        //     Gets the value that represents the Move Focus Back command.
        //
        // Returns:
        //     The command. Default Values Key Gesture Ctrl+Left UI Text Move Focus Back
        public readonly static RoutedCommand MoveFocusBack = new RoutedCommand(nameof(MoveFocusBack));

        //
        // Summary:
        //     Gets the value that represents the Move Focus Page Up command.
        //
        // Returns:
        //     The command. Default Values Key Gesture Ctrl+PageUp UI Text Move Focus Page Up
        public readonly static RoutedCommand MoveFocusPageUp = new RoutedCommand(nameof(MoveFocusPageUp));

        //
        // Summary:
        //     Gets the value that represents the Move Focus Page Down command.
        //
        // Returns:
        //     The command. Default Values Key Gesture Ctrl+PageDown UI Text Move Focus Page
        //     Down
        public readonly static RoutedCommand MoveFocusPageDown = new RoutedCommand(nameof(MoveFocusPageDown));
    }
}
