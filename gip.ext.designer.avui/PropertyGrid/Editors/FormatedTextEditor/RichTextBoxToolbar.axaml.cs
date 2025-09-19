// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using AvRichTextBox;
// TODO: Check what document types are available in AvRichTextBox
// using AvRichTextBox.Documents; // This namespace doesn't exist

namespace gip.ext.designer.avui.PropertyGrid.Editors.FormatedTextEditor
{
	/// <summary>
	/// Interaktionslogik für RichTextBoxToolbar.axaml
	/// </summary>
	public partial class RichTextBoxToolbar : UserControl
	{
		public RichTextBoxToolbar()
		{
            InitializeComponent();
			LoadFontFamilies();
			InitializeCommands();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		private void LoadFontFamilies()
		{
			try
			{
				// In Avalonia, we access system fonts differently
				var systemFonts = FontManager.Current.SystemFonts;
				FontFamilies = systemFonts.ToList();
			}
			catch
			{
				// Fallback font families
				FontFamilies = new List<FontFamily>
				{
					new FontFamily("Arial"),
					new FontFamily("Times New Roman"), 
					new FontFamily("Courier New"),
					new FontFamily("Helvetica"),
					new FontFamily("Verdana")
				};
			}
		}

		private void InitializeCommands()
		{
			ToggleBoldCommand = new RelayCommand(ExecuteToggleBold);
			ToggleItalicCommand = new RelayCommand(ExecuteToggleItalic);
			ToggleUnderlineCommand = new RelayCommand(ExecuteToggleUnderline);
			FontFamilyChangedCommand = new RelayCommand(ExecuteFontFamilyChanged);
			FontSizeChangedCommand = new RelayCommand(ExecuteFontSizeChanged);
		}

		public void SetValuesFromTextBlock(TextBlock textBlock)
		{
			var fontFamilyCombo = this.FindControl<ComboBox>("cmbFontFamily");
			var fontSizeCombo = this.FindControl<ComboBox>("cmbFontSize");

			if (textBlock != null && fontFamilyCombo != null && fontSizeCombo != null)
			{
				// Find matching font family
				var matchingFont = FontFamilies.FirstOrDefault(f => f.Name == textBlock.FontFamily?.Name);
				if (matchingFont != null)
				{
					fontFamilyCombo.SelectedItem = matchingFont;
				}
				
				// Set font size
				var fontSizeText = textBlock.FontSize.ToString();
				var existingItem = fontSizeCombo.Items.Cast<ComboBoxItem>()
					.FirstOrDefault(item => item.Content?.ToString() == fontSizeText);
				
				if (existingItem != null)
				{
					fontSizeCombo.SelectedItem = existingItem;
				}
				else
				{
					// Add custom font size if not in the predefined list
					var newItem = new ComboBoxItem { Content = fontSizeText };
					fontSizeCombo.Items.Add(newItem);
					fontSizeCombo.SelectedItem = newItem;
				}
			}
		}

		// Event handlers for XAML
		private void OnFontFamilyChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems?.Count > 0 && e.AddedItems[0] is FontFamily fontFamily)
			{
				FontFamilyChangedCommand?.Execute(fontFamily);
			}
		}

		private void OnFontSizeChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems?.Count > 0 && e.AddedItems[0] is ComboBoxItem item)
			{
				FontSizeChangedCommand?.Execute(item.Content?.ToString());
			}
		}

		// Font Families property for binding
		public List<FontFamily> FontFamilies { get; private set; }

		// Commands for formatting
		public ICommand ToggleBoldCommand { get; private set; }
		public ICommand ToggleItalicCommand { get; private set; }
		public ICommand ToggleUnderlineCommand { get; private set; }
		public ICommand FontFamilyChangedCommand { get; private set; }
		public ICommand FontSizeChangedCommand { get; private set; }

		private void ExecuteToggleBold(object parameter)
		{
			if (RichTextBox != null)
			{
				// TODO: AvRichTextBox does not appear to have a Selection property
				// Need to research the actual API available in AvRichTextBox for text formatting
				// Possible alternatives:
				// 1. Check if there are formatting commands available
				// 2. Use document manipulation methods if available
				// 3. Work directly with the Document property and modify inlines
				try
				{
					// Placeholder implementation - actual API needs to be determined
					// var document = RichTextBox.Document;
					// Apply formatting to document or selected text
				}
				catch (Exception)
				{
					// AvRichTextBox may not support this operation yet
				}
			}
		}

		private void ExecuteToggleItalic(object parameter)
		{
			if (RichTextBox != null)
			{
				// TODO: AvRichTextBox does not appear to have a Selection property
				// Similar implementation challenge as with bold formatting
				try
				{
					// Placeholder implementation
				}
				catch (Exception)
				{
					// AvRichTextBox may not support this operation yet
				}
			}
		}

		private void ExecuteToggleUnderline(object parameter)
		{
			if (RichTextBox != null)
			{
				// TODO: AvRichTextBox does not appear to have a Selection property
				// Underline formatting might be even more limited
				try
				{
					// Placeholder implementation
				}
				catch (Exception)
				{
					// AvRichTextBox may not support this operation yet
				}
			}
		}

		private void ExecuteFontFamilyChanged(object parameter)
		{
			if (RichTextBox != null && parameter is FontFamily fontFamily)
			{
				// TODO: Apply font family to current selection or entire document
				// Need to determine the correct API for AvRichTextBox
				try
				{
					// Placeholder implementation
					// This might require working with Document.Blocks and Inlines
				}
				catch (Exception)
				{
					// AvRichTextBox may not support this operation yet
				}
			}
		}

		private void ExecuteFontSizeChanged(object parameter)
		{
			if (RichTextBox != null && parameter is string sizeText && double.TryParse(sizeText, out double size))
			{
				// TODO: Apply font size to current selection or entire document
				try
				{
					// Placeholder implementation
				}
				catch (Exception)
				{
					// AvRichTextBox may not support this operation yet
				}
			}
		}

		public RichTextBox RichTextBox
		{
			get { return GetValue(RichTextBoxProperty); }
			set { SetValue(RichTextBoxProperty, value); }
		}

		public static readonly StyledProperty<RichTextBox> RichTextBoxProperty =
			AvaloniaProperty.Register<RichTextBoxToolbar, RichTextBox>(nameof(RichTextBox));
	}

	// Simple RelayCommand implementation for AvaloniaUI
	public class RelayCommand : ICommand
	{
		private readonly Action<object> _execute;
		private readonly Func<object, bool> _canExecute;

		public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
		{
			_execute = execute ?? throw new ArgumentNullException(nameof(execute));
			_canExecute = canExecute;
		}

		public event EventHandler CanExecuteChanged;

		public bool CanExecute(object parameter)
		{
			return _canExecute?.Invoke(parameter) ?? true;
		}

		public void Execute(object parameter)
		{
			_execute(parameter);
		}

		public void RaiseCanExecuteChanged()
		{
			CanExecuteChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}
