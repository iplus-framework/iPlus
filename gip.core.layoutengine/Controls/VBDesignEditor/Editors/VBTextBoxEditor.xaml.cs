// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace gip.core.layoutengine.PropertyGrid.Editors
{
    /// <summary>
    /// Represents a editor for TextBox
    /// </summary>
	public partial class VBTextBoxEditor
	{
		/// <summary>
		/// Creates a new TextBoxEditor instance.
		/// </summary>
		public VBTextBoxEditor()
		{
			InitializeComponent();
		}

		/// <inheritdoc/>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.Key == Key.Enter) {
				BindingOperations.GetBindingExpressionBase(this, TextProperty).UpdateSource();
				SelectAll();
			} else if (e.Key == Key.Escape) {
				BindingOperations.GetBindingExpression(this, TextProperty).UpdateTarget();
			}
		}
	}
}
