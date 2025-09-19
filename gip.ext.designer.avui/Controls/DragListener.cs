// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Avalonia.Media;
using Avalonia.Input;
using Avalonia;
using System.Reactive.Disposables;
using Avalonia.Input.Raw;
using Avalonia.Controls;

namespace gip.ext.designer.avui.Controls
{
	public delegate void DragHandler(DragListener drag);

	public class DragListener
	{
        private static IDisposable _inputSubscription;
        public const double MinimumDragDistance = 4.0;


        static DragListener()
		{
            // Subscribe to input post-processing using the new Avalonia observable pattern
            //_inputSubscription = InputManager.Instance?.PostProcess.Subscribe(PostProcessInput);
            //if (Application.Current != null)
            //{
            //    // Use application-level input events instead
            //    Application.Current.OnFrameworkInitializationCompleted += () =>
            //    {
            //        // Set up global input handling here
            //    };
            //}
        }

        public Transform Transform { get; set; }

        public DragListener(IInputElement target)
		{
			Target = target;
            Target.PointerPressed += Target_PointerPressed;
            Target.PointerMoved += Target_PointerMoved;
            Target.PointerReleased += Target_PointerReleased;
            SetupGlobalInputHandling();
        }

        private void Target_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            LastEventArgs = e;
            // In Avalonia, get position relative to the visual root or a specific visual
            var topLevel = TopLevel.GetTopLevel(Target as Visual);
            StartPoint = e.GetPosition(topLevel);
            CurrentPoint = StartPoint;
            DeltaDelta = new Vector();
            IsDown = true;
            IsCanceled = false;
            if (MouseDown != null)
                MouseDown(this);
        }

        private void Target_PointerReleased(object sender, PointerReleasedEventArgs e)
        {
            LastEventArgs = e;
            IsDown = false;
            if (IsActive)
            {
                Complete();
            }
        }

        private void Target_PointerMoved(object sender, PointerEventArgs e)
        {
            LastEventArgs = e;
            if (IsDown)
            {
                var topLevel = TopLevel.GetTopLevel(Target as Visual);
                var newPoint = e.GetPosition(topLevel);
                DeltaDelta = newPoint - CurrentPoint;
                CurrentPoint = newPoint;

                if (!IsActive)
                {
                    // Use reasonable default values for drag threshold since SystemParameters doesn't exist in Avalonia                    
                    if (Math.Abs(Delta.X) >= MinimumDragDistance ||
                        Math.Abs(Delta.Y) >= MinimumDragDistance)
                    {
                        IsActive = true;
                        CurrentListener = this;

                        if (Started != null)
                        {
                            Started(this);
                        }
                    }
                }

                if (IsActive && Changed != null)
                {
                    Changed(this);
                }
            }
        }

        public void ExternalStart()
        {
            Target_PointerPressed(null, null);
        }

        public void ExternalMouseMove(PointerEventArgs e)
        {
            Target_PointerMoved(null, e);
        }

        public void ExternalStop()
        {
            Target_PointerReleased(null, null);
        }

        static DragListener CurrentListener;

        // Updated to work with Avalonia's input system
        //static void PostProcessInput(RawInputEventArgs e)
        //{
        //    if (CurrentListener != null) {
        //        // Check if it's a key event and specifically the Escape key
        //        if (e is RawKeyEventArgs keyArgs && keyArgs.Key == Key.Escape) {
        //            // In Avalonia, pointer capture is handled differently
        //            if (CurrentListener.Target is IInputElement inputElement)
        //            {
        //                inputElement.ReleasePointerCapture();
        //            }
        //            CurrentListener.IsDown = false;
        //            CurrentListener.IsCanceled = true;
        //            CurrentListener.Complete();
        //        }
        //    }
        //}

        private void SetupGlobalInputHandling()
        {
            if (Target is Visual visual)
            {
                var topLevel = TopLevel.GetTopLevel(visual);
                if (topLevel != null)
                {
                    topLevel.KeyDown += OnGlobalKeyDown;
                }
            }
        }

        private void OnGlobalKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && CurrentListener != null)
            {
                // Handle escape key
                CurrentListener.IsDown = false;
                CurrentListener.IsCanceled = true;
                CurrentListener.Complete();
            }
        }


        void Complete()
		{
			IsActive = false;
			CurrentListener = null;

			if (Completed != null) {
				Completed(this);
			}
		}

        // Cleanup method to dispose of the subscription when no longer needed
        public static void Cleanup()
        {
            _inputSubscription?.Dispose();
            _inputSubscription = null;
        }

        public event DragHandler MouseDown;
        public event DragHandler Started;
		public event DragHandler Changed;
		public event DragHandler Completed;

		public IInputElement Target { get; private set; }
		public Point StartPoint { get; private set; }
		public Point CurrentPoint { get; private set; }
		public Vector DeltaDelta { get; private set; }
		public bool IsActive { get; private set; }
		public bool IsDown { get; private set; }
		public bool IsCanceled { get; private set; }
        public PointerEventArgs LastEventArgs { get; private set; }

        public Vector Delta {
            get
            {
                if (Transform != null)
                {
                    var matrix = Transform.Value;
                    matrix.Invert();
                    return matrix.Transform(CurrentPoint - StartPoint);
                }
                return CurrentPoint - StartPoint;
            }
        }
	}
}
