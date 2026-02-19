// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.IO;
using System.Reflection;
using System.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Styling;
using gip.ext.design.avui;
using Avalonia.Platform;
using Avalonia.Media.Imaging;

namespace gip.ext.designer.avui.Controls
{
	public class ZoomControl : ZoomScrollViewer
	{
		static ZoomControl()
		{
			PanToolCursor = GetCursor("Images/PanToolCursor.cur");
			PanToolCursorMouseDown = GetCursor("Images/PanToolCursorMouseDown.cur");
        }

        public static Cursor GetCursor(string path)
		{
			try 
			{
				using (Stream s = AssetLoader.Open(new Uri("avares://gip.ext.designer.avui/" + path))) 
				{
					// Create a Bitmap from the stream
					var bitmap = new Bitmap(s);

					// Create a Cursor from the Bitmap
					return new Cursor(bitmap, PixelPoint.Origin);
				}
			}
			catch
			{
				// Fallback to standard cursor if resource loading fails
				return new Cursor(StandardCursorType.Hand);
			}
		}

        public object AdditionalControls
        {
            get { return (object)GetValue(AdditionalControlsProperty); }
            set { SetValue(AdditionalControlsProperty, value); }
        }

        public static readonly StyledProperty<object> AdditionalControlsProperty =
            AvaloniaProperty.Register<ZoomControl, object>("AdditionalControls", null);

        static Cursor PanToolCursor;
		static Cursor PanToolCursorMouseDown;
		
		double startHorizontalOffset;
		double startVericalOffset;
		Point startPoint;
		bool isMouseDown;
		bool pan;

        DesignContext _designContext;
        public DesignContext DesignContext
        {
            get
            {
                return _designContext;
            }
            set
            {
                _designContext = value;
            }
        }

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (!pan && e.Key == Key.Space) {
				pan = true;
				UpdateCursor();
			}
            if (DesignContext != null)
            {
                IKeyBindingService kbs = DesignContext.Services.GetService(typeof(IKeyBindingService)) as IKeyBindingService;
                if (kbs != null)
                {
                    KeyBinding keyBinding = kbs.GetBinding(e);
                    if (keyBinding != null)
                        return;
                }
            }

			base.OnKeyDown(e);
		}

		protected override void OnKeyUp(KeyEventArgs e)
		{
			if (e.Key == Key.Space) {
				pan = false;
				UpdateCursor();
			}
			base.OnKeyUp(e);
		}

		protected override void OnPointerPressed(PointerPressedEventArgs e)
		{
            var pointer = e.GetCurrentPoint(this);
            if (!pan && pointer.Properties.IsMiddleButtonPressed)
            {
                pan = true;
                UpdateCursor();
            }

            if (pan && !e.Handled) {
				e.Pointer.Capture(this);
				isMouseDown = true;
				e.Handled = true;
				startPoint = pointer.Position;
				PanStart();
				UpdateCursor();
			}
			base.OnPointerPressed(e);
		}

		protected override void OnPointerMoved(PointerEventArgs e)
		{
			if (isMouseDown) {
				var endPoint = e.GetPosition(this);
				PanContinue(endPoint - startPoint);
			}
			base.OnPointerMoved(e);
		}

		protected override void OnPointerReleased(PointerReleasedEventArgs e)
		{
            var pointer = e.GetCurrentPoint(this);
            if (pan && !pointer.Properties.IsMiddleButtonPressed && !e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                pan = false;
                UpdateCursor();
            }

            if (isMouseDown) {
				isMouseDown = false;
				e.Pointer.Capture(null);
				UpdateCursor();
			}
			base.OnPointerReleased(e);
		}
		
		protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
		{
			if (isMouseDown) {
				isMouseDown = false;
				e.Pointer?.Capture(null);
				UpdateCursor();
			}
			base.OnPointerCaptureLost(e);
		}
		
		// In AvaloniaUI, cursor management is handled differently
		// The cursor property can be set directly on the control
		protected override void OnPointerEntered(PointerEventArgs e)
		{
			base.OnPointerEntered(e);
			UpdateCursor();
		}

		protected override void OnPointerExited(PointerEventArgs e)
		{
			base.OnPointerExited(e);
			Cursor = Cursor.Default;
		}

		private void UpdateCursor()
		{
			if (pan || isMouseDown) {
				Cursor = isMouseDown ? PanToolCursorMouseDown : PanToolCursor;
			} else {
				Cursor = Cursor.Default;
			}
		}
		
		void PanStart()
		{
			startHorizontalOffset = this.Offset.X;
			startVericalOffset = this.Offset.Y;
		}

		void PanContinue(Vector delta)
		{
			var newOffset = new Vector(
				startHorizontalOffset - delta.X / this.CurrentZoom,
				startVericalOffset - delta.Y / this.CurrentZoom
			);
			this.Offset = newOffset;
		}
	}
}
