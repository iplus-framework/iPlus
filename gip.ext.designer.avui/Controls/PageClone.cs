// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.ComponentModel;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.LogicalTree;
using gip.ext.design.avui.Extensions;
using gip.ext.design.avui.UIExtensions;

namespace gip.ext.designer.avui.Controls
{
	/// <summary>
	/// Description of PageClone.
	/// </summary>
	public class PageClone : ContentControl, IAddChild
	{
		static PageClone()
		{
			// Set default properties for PageClone
			FocusableProperty.OverrideDefaultValue<PageClone>(false);
		}
		
		public PageClone()
		{
			// ContentControl already handles content presentation
		}

		// Navigation-related properties - these would need custom implementation in Avalonia
		// as Avalonia doesn't have built-in navigation like WPF
		public bool KeepAlive { get; set; }

		// NavigationService doesn't exist in Avalonia - would need custom implementation
		public object NavigationService { get; set; }

		public bool ShowsNavigationUI { get; set; }
		
		public string Title { get; set; }
		
		public string WindowTitle {
			get { return Title; }
			set { Title = value; }
		}
		
		public double WindowWidth { get; set; }
		
		public double WindowHeight { get; set; }
		
		void IAddChild.AddChild(object value)
		{
			if (this.Content == null || value == null)
				this.Content = value;
			else
				throw new InvalidOperationException();
		}
		
		// IAddChild in Avalonia doesn't have AddText method, 
		// but we keep this for compatibility with the original interface contract
		public void AddText(string text)
		{
			if (text == null)
				return;
			
			for (int i = 0; i < text.Length; i++) {
				if (!char.IsWhiteSpace(text[i]))
					throw new ArgumentException();
			}
		}
	}
	
	/// <summary>
	/// A <see cref="CustomInstanceFactory"/> for PageClone
	/// (and derived classes, unless they specify their own <see cref="CustomInstanceFactory"/>).
	/// </summary>
	[ExtensionFor(typeof(PageClone))]
	public class PageCloneExtension : CustomInstanceFactory
	{
		/// <summary>
		/// Used to create instances of <see cref="PageClone"/>.
		/// </summary>
		public override object CreateInstance(Type type, params object[] arguments)
		{
			Debug.Assert(arguments.Length == 0);
			return new PageClone();
		}
	}
}
