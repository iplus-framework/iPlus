// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System.Windows.Input;

namespace gip.ext.designer.avui
{
	/// <summary>
	/// Description of Commands.
	/// </summary>
	public static class Commands
	{
		public static ICommand AlignTopCommand = new RoutedCommand();
        public static ICommand AlignMiddleCommand = new RoutedCommand();
        public static ICommand AlignBottomCommand = new RoutedCommand();
        public static ICommand AlignLeftCommand = new RoutedCommand();
        public static ICommand AlignCenterCommand = new RoutedCommand();
        public static ICommand AlignRightCommand = new RoutedCommand();
        public static ICommand RotateLeftCommand = new RoutedCommand();
        public static ICommand RotateRightCommand = new RoutedCommand();
		public static ICommand StretchToSameWidthCommand = new RoutedCommand();
		public static ICommand StretchToSameHeightCommand = new RoutedCommand();

		static Commands()
        {
        }
	}
}
