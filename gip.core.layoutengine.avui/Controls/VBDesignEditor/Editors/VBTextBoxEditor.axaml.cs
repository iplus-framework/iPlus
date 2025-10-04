// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using Avalonia.Input;

namespace gip.core.layoutengine.avui.PropertyGrid.Editors
{
    /// <summary>
    /// Represents a editor for TextBox
    /// </summary>
	public partial class VBTextBoxEditor : VBTextBox
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
			base.OnKeyDown(e);
   //         if (e.Key == Key.Enter) {
			//	BindingOperations.GetBindingExpressionBase(this, TextProperty).UpdateSource();
			//	SelectAll();
			//} else if (e.Key == Key.Escape) {
			//	BindingOperations.GetBindingExpression(this, TextProperty).UpdateTarget();
			//}
		}
	}
}
