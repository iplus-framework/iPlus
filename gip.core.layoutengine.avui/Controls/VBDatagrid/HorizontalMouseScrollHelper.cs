using Avalonia;
using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// The horizontal mouse scroll helper.
    /// </summary>
	public class HorizontalMouseScrollHelper
	{
		ScrollViewer scrollViewer;
		//HwndSource source;

		public HorizontalMouseScrollHelper(ScrollViewer scrollviewer, AvaloniaObject d)
		{
			scrollViewer = scrollviewer;
			//source = (HwndSource)PresentationSource.FromDependencyObject(d);
			//if (source != null)
			//	source.AddHook(WindowProc);
		}

		IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			switch (msg)
			{
				case WM_MOUSEHWEEL:

					ConvertParams(wParam, lParam);
					handled = true;//to indicate to IntelliType we've handled message.
					break;

			}
			return IntPtr.Zero;
		}


		void ConvertParams(IntPtr wParam, IntPtr lParam)
		{
			Int32 tilt = (Int16)HIWORD(wParam);
			Int32 keys = LOWORD(wParam);
			Int32 x = LOWORD(lParam);
			Int32 y = HIWORD(lParam);
			Scroll(0, x, y, tilt);
		}
		void Scroll(int clicks, int x, int y, int delta)
		{
			//string output = string.Format("x: {0}, y: {1}, tilt: {2}", x, y, delta);
			//Debug.WriteLine(output);
			if (delta > 0)
			{
				for (int i = 0; i < TILT_HORIZ_FACTOR; i++)
				{
					scrollViewer.LineRight();
				}
			}
			else if (delta < 0)
			{
				for (int i = 0; i < TILT_HORIZ_FACTOR; i++)
				{
					scrollViewer.LineLeft();
				}
			}	
		}


		/// <summary>
		/// multiplier of how far to scroll horizontally
		/// </summary>
		const Int32 TILT_HORIZ_FACTOR = 4;
		const Int32 WM_MOUSEHWEEL = 0x20e;
		static int LOWORD(int n)
		{
			return (n & 0xffff);
		}
		static Int32 HIWORD(IntPtr ptr)
		{
			Int32 val32 = ptr.ToInt32();
			return ((val32 >> 16) & 0xFFFF);
		}

		static Int32 LOWORD(IntPtr ptr)
		{
			Int32 val32 = ptr.ToInt32();
			return (val32 & 0xFFFF);
		}
	}
}
