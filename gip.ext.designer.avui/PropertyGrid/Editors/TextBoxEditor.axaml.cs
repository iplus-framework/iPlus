// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Data;
using Avalonia.Markup.Xaml;

namespace gip.ext.designer.avui.PropertyGrid.Editors
{
	public partial class TextBoxEditor : TextBox
	{
		/// <summary>
		/// Creates a new TextBoxEditor instance.
		/// </summary>
		public TextBoxEditor()
		{
			AvaloniaXamlLoader.Load(this);
		}
		
		/// <inheritdoc/>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.Key == Key.Enter) {
                // Force binding update by clearing and setting focus
                // BindingOperations.GetBindingExpressionBase(this, TextProperty).UpdateSource();
                var currentText = Text;
				Focus();
				SelectAll();
			} else if (e.Key == Key.Escape) {
                // Reset to bound value by clearing local value
                // BindingOperations.GetBindingExpression(this, TextProperty).UpdateTarget();
                this.ClearValue(TextProperty);
			}
			
			base.OnKeyDown(e);
		}
	}
}
