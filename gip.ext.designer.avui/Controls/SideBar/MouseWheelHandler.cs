// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

//using System;

//namespace gip.ext.designer.avui.Controls
//{
//	/// <summary>
//	/// Accumulates mouse wheel deltas and reports the actual number of lines to scroll.
//	/// </summary>
//	public sealed class MouseWheelHandler
//	{
//		// CODE DUPLICATION: See ICSharpCode.TextEditor.Util.MouseWheelHandler
		
//		const int WHEEL_DELTA = 120;
		
//		int mouseWheelDelta;
		
//		public int GetScrollAmount(MouseEventArgs e)
//		{
//			// accumulate the delta to support high-resolution mice
//			mouseWheelDelta += e.Delta;
//			Console.WriteLine("Mouse rotation: new delta=" + e.Delta + ", total delta=" + mouseWheelDelta);
			
//			int linesPerClick = Math.Max(SystemInformation.MouseWheelScrollLines, 1);
			
//			int scrollDistance = mouseWheelDelta * linesPerClick / WHEEL_DELTA;
//			mouseWheelDelta %= Math.Max(1, WHEEL_DELTA / linesPerClick);
//			return scrollDistance;
//		}
		
//		public void Scroll(ScrollBar scrollBar, MouseEventArgs e)
//		{
//			int newvalue = scrollBar.Value - GetScrollAmount(e) * scrollBar.SmallChange;
//			scrollBar.Value = Math.Max(scrollBar.Minimum, Math.Min(scrollBar.Maximum - scrollBar.LargeChange + 1, newvalue));
//		}
//	}
//}
