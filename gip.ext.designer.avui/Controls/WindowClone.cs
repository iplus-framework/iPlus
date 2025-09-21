// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.ComponentModel;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Input;
using Avalonia.Metadata;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using gip.ext.design.avui.Extensions;
using gip.ext.design.avui.UIExtensions;

namespace gip.ext.designer.avui.Controls
{
	/// <summary>
	/// A custom control that imitates the properties of <see cref="Window"/>, but is not a top-level control.
	/// </summary>
	public class WindowClone : StyledElement //ContentControl, IStyleable
    {
		//Type IStyleable.StyleKey => typeof(WindowClone);

		static WindowClone()
		{
			//This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
			//This style is defined in themes\generic.xaml
			//IsTabStopProperty.OverrideDefaultValue<WindowClone>(false);
		}
		
		/// <summary>
		/// Defines the AllowsTransparency property.
		/// </summary>
		public static readonly StyledProperty<bool> AllowsTransparencyProperty =
			AvaloniaProperty.Register<WindowClone, bool>(nameof(AllowsTransparency), false);

		/// <summary>
		/// This property has no effect. (for compatibility with <see cref="Window"/> only).
		/// </summary>
		public bool AllowsTransparency {
			get { return GetValue(AllowsTransparencyProperty); }
			set { SetValue(AllowsTransparencyProperty, value); }
		}
		
		/// <summary>
		/// This property has no effect. (for compatibility with <see cref="Window"/> only).
		/// </summary>
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
		public bool? DialogResult {
			get {
				return null;
			}
			set { }
		}
		
		/// <summary>
		/// Defines the Icon property.
		/// </summary>
		public static readonly StyledProperty<WindowIcon> IconProperty =
			AvaloniaProperty.Register<WindowClone, WindowIcon>(nameof(Icon));

		/// <summary>
		/// Specifies the icon to use.
		/// </summary>
		public WindowIcon Icon {
			get { return GetValue(IconProperty); }
			set { SetValue(IconProperty, value); }
		}
		
		/// <summary>
		/// Defines the Left property.
		/// </summary>
		public static readonly StyledProperty<double> LeftProperty =
			AvaloniaProperty.Register<WindowClone, double>(nameof(Left), 0.0);

		/// <summary>
		/// This property has no effect. (for compatibility with <see cref="Window"/> only).
		/// </summary>
		public double Left {
			get { return GetValue(LeftProperty); }
			set { SetValue(LeftProperty, value); }
		}
		
		Window owner;
		
		/// <summary>
		/// This property has no effect. (for compatibility with <see cref="Window"/> only).
		/// </summary>
		public Window Owner {
			get { return owner; }
			set { owner = value; }
		}
		
		/// <summary>
		/// Defines the ResizeMode property.
		/// </summary>
		public static readonly StyledProperty<SizeToContent> ResizeModeProperty =
			AvaloniaProperty.Register<WindowClone, SizeToContent>(nameof(ResizeMode), SizeToContent.Manual);

		/// <summary>
		/// Gets or sets the resize mode.
		/// </summary>
		public SizeToContent ResizeMode {
			get { return GetValue(ResizeModeProperty); }
			set { SetValue(ResizeModeProperty, value); }
		}
		
		/// <summary>
		/// Defines the ShowActivated property.
		/// </summary>
		public static readonly StyledProperty<bool> ShowActivatedProperty =
			AvaloniaProperty.Register<WindowClone, bool>(nameof(ShowActivated), true);

		/// <summary>
		/// This property has no effect. (for compatibility with <see cref="Window"/> only).
		/// </summary>
		public bool ShowActivated {
			get { return GetValue(ShowActivatedProperty); }
			set { SetValue(ShowActivatedProperty, value); }
		}
		
		/// <summary>
		/// Defines the ShowInTaskbar property.
		/// </summary>
		public static readonly StyledProperty<bool> ShowInTaskbarProperty =
			AvaloniaProperty.Register<WindowClone, bool>(nameof(ShowInTaskbar), true);

		/// <summary>
		/// This property has no effect. (for compatibility with <see cref="Window"/> only).
		/// </summary>
		public bool ShowInTaskbar {
			get { return GetValue(ShowInTaskbarProperty); }
			set { SetValue(ShowInTaskbarProperty, value); }
		}
		
		/// <summary>
		/// Defines the SizeToContent property.
		/// </summary>
		public static readonly StyledProperty<SizeToContent> SizeToContentProperty =
			AvaloniaProperty.Register<WindowClone, SizeToContent>(nameof(SizeToContent), SizeToContent.Manual);

		/// <summary>
		/// Gets or sets a value that specifies whether a window will automatically size itself to fit the size of its content.
		/// </summary>
		public SizeToContent SizeToContent {
			get { return GetValue(SizeToContentProperty); }
			set { SetValue(SizeToContentProperty, value); }
		}
		
		object taskbarItemInfo;
		
		/// <summary>
		/// This property has no effect. (for compatibility with <see cref="Window"/> only).
		/// </summary>
		public object TaskbarItemInfo {
			get { return taskbarItemInfo; }
			set { taskbarItemInfo = value; }
		}
		
		/// <summary>
		/// Defines the Title property.
		/// </summary>
		public static readonly StyledProperty<string> TitleProperty =
			AvaloniaProperty.Register<WindowClone, string>(nameof(Title));

		/// <summary>
		/// The title to display in the Window's title bar.
		/// </summary>
		public string Title {
			get { return GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}
		
		/// <summary>
		/// Defines the Top property.
		/// </summary>
		public static readonly StyledProperty<double> TopProperty =
			AvaloniaProperty.Register<WindowClone, double>(nameof(Top), 0.0);

		/// <summary>
		/// This property has no effect. (for compatibility with <see cref="Window"/> only).
		/// </summary>
		public double Top {
			get { return GetValue(TopProperty); }
			set { SetValue(TopProperty, value); }
		}
		
		/// <summary>
		/// Defines the Topmost property.
		/// </summary>
		public static readonly StyledProperty<bool> TopmostProperty =
			AvaloniaProperty.Register<WindowClone, bool>(nameof(Topmost), false);

		/// <summary>
		/// This property has no effect. (for compatibility with <see cref="Window"/> only).
		/// </summary>
		public bool Topmost {
			get { return GetValue(TopmostProperty); }
			set { SetValue(TopmostProperty, value); }
		}
		
		WindowStartupLocation windowStartupLocation;
		
		/// <summary>
		/// This property has no effect. (for compatibility with <see cref="Window"/> only).
		/// </summary>
		public WindowStartupLocation WindowStartupLocation {
			get { return windowStartupLocation; }
			set { windowStartupLocation = value; }
		}
		
		/// <summary>
		/// Defines the WindowState property.
		/// </summary>
		public static readonly StyledProperty<WindowState> WindowStateProperty =
			AvaloniaProperty.Register<WindowClone, WindowState>(nameof(WindowState), WindowState.Normal);

		/// <summary>
		/// This property has no effect. (for compatibility with <see cref="Window"/> only).
		/// </summary>
		public WindowState WindowState {
			get { return GetValue(WindowStateProperty); }
			set { SetValue(WindowStateProperty, value); }
		}
		
		SystemDecorations windowStyle;
		
		/// <summary>
		/// This property has no effect. (for compatibility with <see cref="Window"/> only).
		/// </summary>
		public SystemDecorations WindowStyle {
			get { return windowStyle; }
			set { windowStyle = value; }
		}
		
		/// <summary>
		/// This event is never raised. (for compatibility with <see cref="Window"/> only).
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
		public event EventHandler Activated { add {} remove {} }
		
		/// <summary>
		/// This event is never raised. (for compatibility with <see cref="Window"/> only).
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
		public event EventHandler Closed { add {} remove {} }
		
		/// <summary>
		/// This event is never raised. (for compatibility with <see cref="Window"/> only).
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
		public event EventHandler Closing { add {} remove {} }
		
		/// <summary>
		/// This event is never raised. (for compatibility with <see cref="Window"/> only).
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
		public event EventHandler ContentRendered { add {} remove {} }
		
		/// <summary>
		/// This event is never raised. (for compatibility with <see cref="Window"/> only).
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
		public event EventHandler Deactivated { add {} remove {} }
		
		/// <summary>
		/// This event is never raised. (for compatibility with <see cref="Window"/> only).
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
		public event EventHandler LocationChanged { add {} remove {} }
		
		/// <summary>
		/// This event is never raised. (for compatibility with <see cref="Window"/> only).
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
		public event EventHandler SourceInitialized { add {} remove {} }
		
		/// <summary>
		/// This event is never raised. (for compatibility with <see cref="Window"/> only).
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
		public event EventHandler StateChanged { add {} remove {} }
	}
	
	/// <summary>
	/// A <see cref="CustomInstanceFactory"/> for <see cref="Window"/>
	/// (and derived classes, unless they specify their own <see cref="CustomInstanceFactory"/>).
	/// </summary>
	[ExtensionFor(typeof(Window))]
	public class WindowCloneExtension : CustomInstanceFactory
	{
		/// <summary>
		/// Used to create instances of <see cref="WindowClone"/>.
		/// </summary>
		public override object CreateInstance(Type type, params object[] arguments)
		{
			Debug.Assert(arguments.Length == 0);
			return new WindowClone();
		}
	}
}
