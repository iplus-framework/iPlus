// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using Avalonia;
using Avalonia.Input;

namespace gip.ext.design.avui
{
	public class MouseHorizontalWheelEventArgs : PointerWheelEventArgs
    {
		//public int HorizontalDelta { get; }

		public MouseHorizontalWheelEventArgs(object source, IPointer mouse, Visual rootVisual, Point rootVisualPosition, ulong timestamp, PointerPointProperties properties, KeyModifiers modifiers, Vector horizontalDelta)
			: base(source, mouse, rootVisual, rootVisualPosition, timestamp, properties, modifiers, horizontalDelta)
		{
			//HorizontalDelta = horizontalDelta;
		}
	}
}
