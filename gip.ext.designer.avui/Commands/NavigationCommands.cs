// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using Avalonia.Labs.Input;
using System.Windows.Input;

namespace gip.ext.designer.avui
{
	/// <summary>
	/// Description of Commands.
	/// </summary>
	public static class NavigationCommands
    {
        //
        // Summary:
        //     Gets the value that represents the Browse Back command.
        //
        // Returns:
        //     The routed command. Default Values Key Gesture ALT+LEFT UI Text Back
        public readonly static RoutedCommand BrowseBack = new RoutedCommand(nameof(BrowseBack));

        //
        // Summary:
        //     Gets the value that represents the Browse Forward command.
        //
        // Returns:
        //     The routed command. Default Values Key Gesture ALT+RIGHT UI Text Forward
        public readonly static RoutedCommand BrowseForward = new RoutedCommand(nameof(BrowseForward));

        //
        // Summary:
        //     Gets the value that represents the Browse Home command.
        //
        // Returns:
        //     The routed command. Default Values Key Gesture ALT+HOME UI Text Home
        public readonly static RoutedCommand BrowseHome = new RoutedCommand(nameof(BrowseHome));

        //
        // Summary:
        //     Gets the value that represents the Browse Stop command.
        //
        // Returns:
        //     The routed command. Default Values Key Gesture ALT+ESC UI Text Stop
        public readonly static RoutedCommand BrowseStop = new RoutedCommand(nameof(BrowseStop));

        //
        // Summary:
        //     Gets the value that represents the Refresh command.
        //
        // Returns:
        //     The routed command. Default Values Key Gesture F5 UI Text Refresh
        public readonly static RoutedCommand Refresh = new RoutedCommand(nameof(Refresh));

        //
        // Summary:
        //     Gets the value that represents the Favorites command.
        //
        // Returns:
        //     The routed command. Default Values Key Gesture CTRL+I UI Text Favorites
        public readonly static RoutedCommand Favorites = new RoutedCommand(nameof(Favorites));

        //
        // Summary:
        //     Gets the value that represents the Search command.
        //
        // Returns:
        //     The routed command. Default Values Key Gesture F3 UI Text Search
        public readonly static RoutedCommand Search = new RoutedCommand(nameof(Search));

        //
        // Summary:
        //     Gets the value that represents the Increase Zoom command.
        //
        // Returns:
        //     The routed command. Default Values Key Gesture N/A UI Text Increase Zoom
        public readonly static RoutedCommand IncreaseZoom = new RoutedCommand(nameof(IncreaseZoom));

        //
        // Summary:
        //     Gets the value that represents the Decrease Zoom command.
        //
        // Returns:
        //     The routed command. Default Values Key Gesture N/A UI Text Decrease Zoom
        public readonly static RoutedCommand DecreaseZoom = new RoutedCommand(nameof(DecreaseZoom));

        //
        // Summary:
        //     Gets the value that represents the Zoom command.
        //
        // Returns:
        //     The routed command. Default Values Key Gesture N/A UI Text Zoom
        public readonly static RoutedCommand Zoom = new RoutedCommand(nameof(Zoom));

        //
        // Summary:
        //     Gets the value that represents the Next Page command.
        //
        // Returns:
        //     The routed command. Default Values Key Gesture N/A UI Text Next Page
        public readonly static RoutedCommand NextPage = new RoutedCommand(nameof(NextPage));

        //
        // Summary:
        //     Gets the value that represents the Previous Page command.
        //
        // Returns:
        //     The routed command. Default Values Key Gesture N/A UI Text Previous Page
        public readonly static RoutedCommand PreviousPage = new RoutedCommand(nameof(PreviousPage));

        //
        // Summary:
        //     Gets the value that represents the First Page command.
        //
        // Returns:
        //     The routed command. Default Values Key Gesture N/A UI Text First Page
        public readonly static RoutedCommand FirstPage = new RoutedCommand(nameof(FirstPage));

        //
        // Summary:
        //     Gets the value that represents the Last Page command.
        //
        // Returns:
        //     The routed command. Default Values Key Gesture N/A UI Text Last Page
        public readonly static RoutedCommand LastPage = new RoutedCommand(nameof(LastPage));

        //
        // Summary:
        //     Gets the value that represents the Go To Page command.
        //
        // Returns:
        //     The routed command. Default Values Key Gesture N/A UI Text Go To Page
        public readonly static RoutedCommand GoToPage = new RoutedCommand(nameof(GoToPage));

        //
        // Summary:
        //     Gets the value that represents the Navigate Journal command.
        //
        // Returns:
        //     The routed command. Default Values Key Gesture N/A UI Text Navigation Journal
        public readonly static RoutedCommand NavigateJournal = new RoutedCommand(nameof(NavigateJournal));
    }
}
