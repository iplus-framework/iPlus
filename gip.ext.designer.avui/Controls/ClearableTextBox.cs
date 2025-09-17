// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.Windows;

namespace gip.ext.designer.avui.Controls
{
	public class ClearableTextBox : EnterTextBox
	{
		private Button textRemoverButton;

		static ClearableTextBox()
		{
			//DefaultStyleKeyProperty.OverrideMetadata(typeof (ClearableTextBox), new FrameworkPropertyMetadata(typeof (ClearableTextBox)));
		}

		public ClearableTextBox()
		{
			this.TextChanged += this.TextBoxTextChanged;
			this.KeyUp += this.ClearableTextBox_KeyUp;
		}

		void ClearableTextBox_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
				this.TextRemoverClick(sender, null);
		}

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            this.textRemoverButton = e.NameScope.Find<Button>("TextRemover") as Button;
            if (null != this.textRemoverButton)
            {
                this.textRemoverButton.Click += this.TextRemoverClick;
            }

            base.OnApplyTemplate(e);
        }

        protected void UpdateState()
		{
            PseudoClasses.Set(":text-present", !string.IsNullOrEmpty(this.Text));
            PseudoClasses.Set(":empty", string.IsNullOrEmpty(this.Text));
		}

		private void TextBoxTextChanged(object sender, TextChangedEventArgs e)
		{
			this.UpdateState();
		}

		private void TextRemoverClick(object sender, RoutedEventArgs e)
		{
			this.Text = null;
			this.Focus();
		}

        protected override void OnGotFocus(GotFocusEventArgs e)
        {
            this.UpdateState();
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            this.UpdateState();
            base.OnLostFocus(e);
        }
	}
}
