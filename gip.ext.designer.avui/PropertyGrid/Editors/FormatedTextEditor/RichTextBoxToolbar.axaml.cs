// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using gip.ext.designer.avui.themes;

namespace gip.ext.designer.avui.PropertyGrid.Editors.FormatedTextEditor
{
	/// <summary>
	/// Interaktionslogik für RichTextBoxToolbar.xaml
	/// </summary>
	public partial class RichTextBoxToolbar
	{
		public RichTextBoxToolbar()
		{
			SpecialInitializeComponent();

			cmbFontFamily.SelectionChanged += (s, e) =>
			{
				if (cmbFontFamily.SelectedValue != null && RichTextBox != null)
				{
					TextRange tr = new TextRange(RichTextBox.Selection.Start, RichTextBox.Selection.End);
					var value = cmbFontFamily.SelectedValue;
					tr.ApplyPropertyValue(TextElement.FontFamilyProperty, value);
				}
			};

			cmbFontSize.SelectionChanged += (s, e) =>
			{
				if (cmbFontSize.SelectedValue != null && RichTextBox != null)
				{
					TextRange tr = new TextRange(RichTextBox.Selection.Start, RichTextBox.Selection.End);
					var value = ((ComboBoxItem)cmbFontSize.SelectedValue).Content.ToString();
					tr.ApplyPropertyValue(TextElement.FontSizeProperty, double.Parse(value));
				}
			};

			cmbFontSize.AddHandler(TextBoxBase.TextChangedEvent, new TextChangedEventHandler((s, e) =>
			{
				if (!string.IsNullOrEmpty(cmbFontSize.Text) && RichTextBox != null)
				{
					TextRange tr = new TextRange(RichTextBox.Selection.Start, RichTextBox.Selection.End);
					tr.ApplyPropertyValue(TextElement.FontSizeProperty, double.Parse(cmbFontSize.Text));
				}
			}));
		}

		public void SetValuesFromTextBlock(TextBlock textBlock)
		{
			cmbFontFamily.Text = textBlock.FontFamily.ToString();
			cmbFontSize.Text = textBlock.FontSize.ToString();
		}

		/// <summary>
		/// Fixes InitializeComponent with multiple Versions of same Assembly loaded
		/// </summary>
		public void SpecialInitializeComponent()
		{
			if (!this._contentLoaded)
			{
				this._contentLoaded = true;
				Uri resourceLocator = new Uri(VersionedAssemblyResourceDictionary.GetXamlNameForType(this.GetType()), UriKind.Relative);
				Application.LoadComponent(this, resourceLocator);
			}

			this.InitializeComponent();
		}

		public RichTextBox RichTextBox
		{
			get { return (RichTextBox) GetValue(RichTextBoxProperty); }
			set { SetValue(RichTextBoxProperty, value); }
		}

		public static readonly DependencyProperty RichTextBoxProperty =
			DependencyProperty.Register("RichTextBox", typeof (RichTextBox), typeof (RichTextBoxToolbar),
				new PropertyMetadata(null));
	}
}
