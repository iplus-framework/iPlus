// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using Avalonia.Labs.Input;
using System.Windows.Input;

namespace gip.ext.designer.avui
{
    public static class ApplicationCommands
    {
        public readonly static RoutedCommand Undo = new RoutedCommand(nameof(Undo));
        public readonly static RoutedCommand Redo = new RoutedCommand(nameof(Redo));
        public readonly static RoutedCommand Copy = new RoutedCommand(nameof(Copy));
        public readonly static RoutedCommand Cut = new RoutedCommand(nameof(Cut));
        public readonly static RoutedCommand Delete = new RoutedCommand(nameof(Delete));
        public readonly static RoutedCommand Paste = new RoutedCommand(nameof(Paste));
        public readonly static RoutedCommand SelectAll = new RoutedCommand(nameof(SelectAll));
    }
}
