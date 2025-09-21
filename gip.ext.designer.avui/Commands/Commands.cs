// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using Avalonia.Labs.Input;
using System.Windows.Input;

namespace gip.ext.designer.avui
{
	/// <summary>
	/// Description of Commands.
	/// </summary>
	public static class Commands
	{
		public readonly static ICommand AlignTopCommand = new RoutedCommand(nameof(AlignTopCommand));
        public readonly static ICommand AlignMiddleCommand = new RoutedCommand(nameof(AlignMiddleCommand));
        public readonly static ICommand AlignBottomCommand = new RoutedCommand(nameof(AlignBottomCommand));
        public readonly static ICommand AlignLeftCommand = new RoutedCommand(nameof(AlignLeftCommand));
        public readonly static ICommand AlignCenterCommand = new RoutedCommand(nameof(AlignCenterCommand));
        public readonly static ICommand AlignRightCommand = new RoutedCommand(nameof(AlignRightCommand));
        public readonly static ICommand RotateLeftCommand = new RoutedCommand(nameof(RotateLeftCommand));
        public readonly static ICommand RotateRightCommand = new RoutedCommand(nameof(RotateRightCommand));
		public readonly static ICommand StretchToSameWidthCommand = new RoutedCommand(nameof(StretchToSameWidthCommand));
		public readonly static ICommand StretchToSameHeightCommand = new RoutedCommand(nameof(StretchToSameHeightCommand));
	}
}
