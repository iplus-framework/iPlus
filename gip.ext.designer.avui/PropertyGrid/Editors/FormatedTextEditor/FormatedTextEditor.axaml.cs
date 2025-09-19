// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia;
using Avalonia.Media;
using Avalonia.Markup.Xaml;
using AvRichTextBox;
// TODO: Determine correct namespace for AvRichTextBox document types
// using AvRichTextBox.Documents; // This may not exist
using gip.ext.design.avui;
using gip.ext.designer.avui.themes;

namespace gip.ext.designer.avui.PropertyGrid
{
	/// <summary>
	/// Interaktionslogik für FormatedTextEditor.xaml
	/// </summary>
	public partial class FormatedTextEditor : UserControl
	{
		private DesignItem designItem;

		public FormatedTextEditor(DesignItem designItem)
		{
            this.InitializeComponent();

            this.designItem = designItem;

			var tb = ((TextBlock)designItem.Component);
			SetRichTextBoxTextFromTextBlock(richTextBox, tb);

			richTextBoxToolbar.RichTextBox = richTextBox;
			richTextBoxToolbar.SetValuesFromTextBlock(tb);

			richTextBox.Foreground = tb.Foreground;
			richTextBox.Background = tb.Background;
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
			richTextBox = this.FindControl<RichTextBox>("richTextBox");
			richTextBoxToolbar = this.FindControl<Editors.FormatedTextEditor.RichTextBoxToolbar>("richTextBoxToolbar");
		}

		private RichTextBox richTextBox;
		private Editors.FormatedTextEditor.RichTextBoxToolbar richTextBoxToolbar;

		public static void SetRichTextBoxTextFromTextBlock(RichTextBox richTextBox, TextBlock textBlock)
		{
			// TODO: AvRichTextBox might not fully support all TextBlock.Inlines features
			// Need to check what inline types are supported and what Document API is available
			try
			{
				// Check if AvRichTextBox has a Document property and what type it is
				var document = richTextBox.Document;
				if (document != null)
				{
					// TODO: Determine the correct way to clear and populate document content
					// This depends on what type Document is and what methods it exposes
					
					// For now, try to set basic text content
					if (!string.IsNullOrEmpty(textBlock.Text))
					{
						// TODO: Replace with actual API calls when AvRichTextBox API is known
						// document.Blocks.Clear();
						// var paragraph = new Paragraph();
						// paragraph.Inlines.Add(new Run(textBlock.Text));
						// document.Blocks.Add(paragraph);
					}
					
					// TODO: Handle TextBlock.Inlines if AvRichTextBox supports rich formatting
					if (textBlock.Inlines?.Any() == true)
					{
						// Convert WPF inlines to AvRichTextBox format
						// This requires understanding AvRichTextBox's inline/formatting model
					}
				}
			}
			catch (Exception)
			{
				// Fallback: if rich text fails, try to set plain text
				// TODO: Determine if RichTextBox has a Text property or similar
			}
		}

		private static void GetDesignItems(DesignItem designItem, object blocks, List<DesignItem> list)
		{
			// TODO: This method needs to be rewritten based on AvRichTextBox's actual document model
			// The BlockCollection type from WPF may not exist in AvRichTextBox
			// Need to determine:
			// 1. What type blocks parameter should be
			// 2. How to iterate through document content in AvRichTextBox
			// 3. What block types are supported (Paragraph, Section, etc.)
			
			bool first = true;

			try
			{
				// Placeholder implementation - actual code depends on AvRichTextBox API
				// foreach (var block in blocks)
				// {
				//     if (block is Paragraph paragraph)
				//     {
				//         // Handle paragraph content
				//     }
				// }
			}
			catch (Exception)
			{
				// API not available in current AvRichTextBox version
			}
		}

		private static object CloneInline(object inline)
		{
			// TODO: This method needs complete rewrite for AvRichTextBox
			// WPF Inline types may not be available or may have different APIs
			// Need to determine:
			// 1. What inline types AvRichTextBox supports
			// 2. How to create and clone them
			// 3. What properties are available for formatting
			
			try
			{
				// Placeholder - replace with actual AvRichTextBox inline creation
				// if (inline is Run run)
				// {
				//     return new Run(run.Text);
				// }
				// else if (inline is LineBreak)
				// {
				//     return new LineBreak();
				// }
				
				return null;
			}
			catch (Exception)
			{
				return null;
			}
		}

		private static DesignItem InlineToDesignItem(DesignItem designItem, object inline)
		{
			// TODO: Rewrite for AvRichTextBox inline types
			try
			{
				var cloned = CloneInline(inline);
				if (cloned != null)
				{
					var d = designItem.Services.Component.RegisterComponentForDesigner(cloned);
					// TODO: Set properties based on AvRichTextBox inline type
					return d;
				}
			}
			catch (Exception)
			{
				// Registration failed or inline type not supported
			}

			return null;
		}

		private static void SetDesignItemTextpropertiesFromInline(DesignItem targetDesignItem, object inline)
		{
			// TODO: Rewrite for AvRichTextBox inline property model
			try
			{
				// Only set properties that are supported by AvRichTextBox
				// This requires knowing what properties are available on AvRichTextBox inlines
			}
			catch (Exception)
			{
				// Ignore property setting errors
			}
		}

		public static void SetTextBlockTextFromRichTextBlox(DesignItem designItem, RichTextBox richTextBox)
		{
			// Reset TextBlock properties
			designItem.Properties.GetProperty(TextBlock.TextProperty).Reset();
			designItem.Properties.GetProperty(TextBlock.FontSizeProperty).Reset();
			designItem.Properties.GetProperty(TextBlock.FontFamilyProperty).Reset();
			// TODO: Check if these properties exist in Avalonia TextBlock
			//designItem.Properties.GetProperty(TextBlock.FontStretchProperty).Reset();
			designItem.Properties.GetProperty(TextBlock.FontWeightProperty).Reset();
			designItem.Properties.GetProperty(TextBlock.BackgroundProperty).Reset();
			designItem.Properties.GetProperty(TextBlock.ForegroundProperty).Reset();
			designItem.Properties.GetProperty(TextBlock.FontStyleProperty).Reset();
			// TODO: TextEffects and TextDecorations are WPF-specific
			//designItem.Properties.GetProperty(TextBlock.TextEffectsProperty).Reset();
			//designItem.Properties.GetProperty(TextBlock.TextDecorationsProperty).Reset();

			var inlinesProperty = designItem.Properties.GetProperty("Inlines");
			inlinesProperty.CollectionElements.Clear();

			try
			{
				var doc = richTextBox.Document;
				if (doc != null)
				{
					// TODO: Extract content from AvRichTextBox document
					// This requires understanding the AvRichTextBox document model
					
					var inlines = new List<DesignItem>();
					// GetDesignItems(designItem, doc.Blocks, inlines);

					if (inlines.Count == 1 && inlines.First().Component is object run) 
					{
						// TODO: Handle single run case
						// SetDesignItemTextpropertiesFromInline(designItem, run);
						// designItem.Properties.GetProperty(TextBlock.TextProperty).SetValue(run.Text);
					}
					else 
					{
						foreach (var inline in inlines) 
						{
							inlinesProperty.CollectionElements.Add(inline);
						}
					}
				}
			}
			catch (Exception)
			{
				// Fallback: try to extract plain text if rich text extraction fails
				// TODO: Determine if AvRichTextBox has a Text property or GetPlainText() method
			}
		}

		private void Ok_Click(object sender, RoutedEventArgs e)
		{
			var changeGroup = designItem.OpenGroup("Formated Text");

			SetTextBlockTextFromRichTextBlox(designItem, richTextBox);

			changeGroup.Commit();

			this.FindAncestorOfType<Window>()?.Close();
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			this.FindAncestorOfType<Window>()?.Close();
		}

		private void StrikeThroughButton_Click(object sender, RoutedEventArgs e)
		{
			// TODO: Strikethrough functionality depends on AvRichTextBox selection and formatting APIs
			// WPF TextRange and Selection APIs are not available
			// Need to research:
			// 1. How to get current selection in AvRichTextBox
			// 2. How to apply text decorations (if supported)
			// 3. Alternative approaches if direct formatting is not available
			
			// Placeholder implementation - functionality not available until AvRichTextBox API is clarified
		}
	}
}
