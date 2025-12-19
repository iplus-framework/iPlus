// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using Avalonia.Input;
using Avalonia.Labs.Input;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace gip.ext.designer.avui
{
    public static class ApplicationCommands
    {
        public static readonly KeyModifiers PlatformCommandKey = GetPlatformCommandKey();

        // Built-In Gestures:
        public static readonly KeyGesture PasteGesture = Avalonia.Application.Current?.PlatformSettings?.HotkeyConfiguration.Paste.FirstOrDefault() ?? new KeyGesture(Key.V, PlatformCommandKey);
        public static readonly KeyGesture CopyGesture = Avalonia.Application.Current?.PlatformSettings?.HotkeyConfiguration.Copy.FirstOrDefault() ?? new KeyGesture(Key.C, PlatformCommandKey);
        public static readonly KeyGesture CutGesture = Avalonia.Application.Current?.PlatformSettings?.HotkeyConfiguration.Cut.FirstOrDefault() ?? new KeyGesture(Key.X, PlatformCommandKey);
        public static readonly KeyGesture SelectAllGesture = Avalonia.Application.Current?.PlatformSettings?.HotkeyConfiguration.SelectAll.FirstOrDefault() ?? new KeyGesture(Key.A, PlatformCommandKey);
        public static readonly KeyGesture UndoGesture = Avalonia.Application.Current?.PlatformSettings?.HotkeyConfiguration.Undo.FirstOrDefault() ?? new KeyGesture(Key.Z, PlatformCommandKey);
        public static readonly KeyGesture RedoGesture = Avalonia.Application.Current?.PlatformSettings?.HotkeyConfiguration.Redo.FirstOrDefault() ?? new KeyGesture(Key.Y, PlatformCommandKey);
        
        public static RoutedCommand Paste { get; } = new RoutedCommand(nameof(Paste), PasteGesture);
        public static RoutedCommand Copy { get; } = new RoutedCommand(nameof(Copy), CopyGesture);
        public static RoutedCommand Cut { get; } = new RoutedCommand(nameof(Cut), CutGesture);
        public static RoutedCommand SelectAll { get; } = new RoutedCommand(nameof(SelectAll), SelectAllGesture);
        public static RoutedCommand Undo { get; } = new RoutedCommand(nameof(Undo), UndoGesture);
        public static RoutedCommand Redo { get; } = new RoutedCommand(nameof(Redo), RedoGesture);


        // Custom Gestures:
        public static readonly KeyGesture DeleteGesture = new KeyGesture(Key.Delete, PlatformCommandKey);
        public static readonly KeyGesture FindGesture = new KeyGesture(Key.F, PlatformCommandKey);
        public static readonly KeyGesture ReplaceGesture = GetReplaceKeyGesture();

        public static RoutedCommand Delete { get; } = new RoutedCommand(nameof(Delete), DeleteGesture);
        public static RoutedCommand Find { get; } = new RoutedCommand(nameof(Find), FindGesture);
        public static RoutedCommand Replace { get; } = new RoutedCommand(nameof(Replace), ReplaceGesture);

        private static KeyModifiers GetPlatformCommandKey()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return KeyModifiers.Meta;
            }
            return KeyModifiers.Control;
        }

        private static KeyGesture GetReplaceKeyGesture()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return new KeyGesture(Key.F, KeyModifiers.Meta | KeyModifiers.Alt);
            }

            return new KeyGesture(Key.H, PlatformCommandKey);
        }
    }
}
