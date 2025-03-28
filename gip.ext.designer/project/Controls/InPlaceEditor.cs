﻿// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup.Primitives;
using System.Windows.Controls;
using System.Windows.Input;
using gip.ext.design;

namespace gip.ext.designer.Controls
{
	/// <summary>
	/// Supports editing Text in the Designer
	/// </summary>
	public class InPlaceEditor : TextBox
	{
		static InPlaceEditor()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof (InPlaceEditor), new FrameworkPropertyMetadata(typeof (InPlaceEditor)));
		}
		
		/// <summary>
		/// This property is binded to the Text Property of the editor.
		/// </summary>
		public static readonly DependencyProperty BindProperty =
			DependencyProperty.Register("Bind", typeof (string), typeof (InPlaceEditor), new FrameworkPropertyMetadata());
		
		public string Bind{
			get { return (string) GetValue(BindProperty); }
			set { SetValue(BindProperty, value); }
		}
		
		readonly DesignItem designItem;
		ChangeGroup changeGroup;
		TextBlock textBlock;
		TextBox editor;
		
		bool _isChangeGroupOpen;
		
		/// <summary>
		/// This is the name of the property that is being edited for example Window.Title, Button.Content .
		/// </summary>
		string property;
		
		public InPlaceEditor(DesignItem designItem)
		{
			this.designItem=designItem;
		}
		
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			editor = new TextBox();
			editor = Template.FindName("editor", this) as TextBox; // Gets the TextBox-editor from the Template
			Debug.Assert(editor != null);
			editor.PreviewKeyDown+= delegate(object sender, KeyEventArgs e) {
				if (e.Key == Key.Enter && (e.KeyboardDevice.Modifiers & ModifierKeys.Shift) != ModifierKeys.Shift)
				{
					e.Handled = true;
				} };
			ToolTip = "Edit the Text. Press"+Environment.NewLine+"Enter to make changes."+Environment.NewLine+"Shift+Enter to insert a newline."+Environment.NewLine+"Esc to cancel editing.";
		}
		
		/// <summary>
		/// Binds the Text Property of the element extended with <see cref="Bind"/>.
		/// </summary>
		/// <param name="textBlock"></param>
		public void SetBinding(TextBlock textBlock)
		{
			Debug.Assert(textBlock!=null);
			this.textBlock = textBlock;
			Binding binding = new Binding("Text");
			binding.Source = this.textBlock;
			binding.Mode = BindingMode.TwoWay;
			SetBinding(BindProperty, binding);
			property=PropertyUpdated(textBlock);
		}
		
		/// <summary>
		/// Returns the property that is being edited in the element for example editing Window Title returns "Title",
		/// Button text as "Content".
		/// </summary>
		private string PropertyUpdated(TextBlock text)
		{
			MarkupObject obj = MarkupWriter.GetMarkupObjectFor(designItem.Component);
			foreach (MarkupProperty property in obj.Properties) {
				if (property.DependencyProperty != null && property.StringValue == textBlock.Text)
					return property.Name;
			}
			return null;
		}
		
		protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
		{
			base.OnGotKeyboardFocus(e);
			StartEditing();
		}
		
		protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
		{
			if (changeGroup != null && _isChangeGroupOpen){
				changeGroup.Abort();
				_isChangeGroupOpen=false;
			}			
			if (textBlock != null)
				textBlock.Visibility = Visibility.Visible;
			Reset();
			base.OnLostKeyboardFocus(e);
		}
		
		/// <summary>
		/// Change is committed if the user releases the Escape Key.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnKeyUp(KeyEventArgs e)
		{
			base.OnKeyUp(e);
			if(e.KeyboardDevice.Modifiers != ModifierKeys.Shift) {
				switch(e.Key) {						
					case Key.Enter:
						// Commit the changes to DOM.
						if(property!=null)
							designItem.Properties[property].SetValue(Bind);
						if(designItem.Properties[Control.FontFamilyProperty].ValueOnInstance!=editor.FontFamily)
							designItem.Properties[Control.FontFamilyProperty].SetValue(editor.FontFamily);
						if((double)designItem.Properties[Control.FontSizeProperty].ValueOnInstance!=editor.FontSize)
							designItem.Properties[Control.FontSizeProperty].SetValue(editor.FontSize);
						if((FontStretch)designItem.Properties[Control.FontStretchProperty].ValueOnInstance!=editor.FontStretch)
							designItem.Properties[Control.FontStretchProperty].SetValue(editor.FontStretch);
						if((FontStyle)designItem.Properties[Control.FontStyleProperty].ValueOnInstance!=editor.FontStyle)
							designItem.Properties[Control.FontStyleProperty].SetValue(editor.FontStyle);
						if((FontWeight)designItem.Properties[Control.FontWeightProperty].ValueOnInstance!=editor.FontWeight)
							designItem.Properties[Control.FontWeightProperty].SetValue(editor.FontWeight);
						
						if (changeGroup != null && _isChangeGroupOpen){
							changeGroup.Commit();
							_isChangeGroupOpen=false;
						}
						changeGroup = null;
						this.Visibility = Visibility.Hidden;
						textBlock.Visibility = Visibility.Visible;
						break;
					case Key.Escape:
						AbortEditing();
						break;
				}
			}else if(e.Key == Key.Enter){
				editor.Text.Insert(editor.CaretIndex, Environment.NewLine);
			}
		}
		
		private void Reset()
		{
			if (textBlock != null) {
				if (property != null)
				{
					textBlock.Text = designItem.Properties[property].ValueOnInstance as string;
				}
				textBlock.FontFamily = (System.Windows.Media.FontFamily)designItem.Properties[Control.FontFamilyProperty].ValueOnInstance;
				textBlock.FontSize = (double) designItem.Properties[Control.FontSizeProperty].ValueOnInstance;
				textBlock.FontStretch = (FontStretch) designItem.Properties[Control.FontStretchProperty].ValueOnInstance;
				textBlock.FontStretch = (FontStretch) designItem.Properties[Control.FontStretchProperty].ValueOnInstance;
				textBlock.FontWeight = (FontWeight) designItem.Properties[Control.FontWeightProperty].ValueOnInstance;
			}
		}
		
		public void AbortEditing()
		{
			if(changeGroup!=null && _isChangeGroupOpen){
				Reset();
				changeGroup.Abort();
				_isChangeGroupOpen=false;
			}			
			this.Visibility= Visibility.Hidden;
			if(textBlock!=null)
				textBlock.Visibility=Visibility.Visible;
			Reset();
		}
		
		public void StartEditing()
		{
			if(changeGroup==null){
				changeGroup = designItem.OpenGroup("Change Text");
				_isChangeGroupOpen=true;
			}
			this.Visibility=Visibility.Visible;
			if(textBlock!=null)
				textBlock.Visibility=Visibility.Hidden;
		}
	}
}
