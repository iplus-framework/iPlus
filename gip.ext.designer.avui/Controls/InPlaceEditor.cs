// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.ComponentModel;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using gip.ext.design.avui;

namespace gip.ext.designer.avui.Controls
{
    /// <summary>
    /// Supports editing Text in the Designer
    /// </summary>
    public class InPlaceEditor : TemplatedControl
    {
        static InPlaceEditor()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof (InPlaceEditor), new StyledPropertyMetadata<Type>(typeof (InPlaceEditor)));
        }

        /// <summary>
        /// This property is binded to the Text Property of the editor.
        /// </summary>
        public static readonly StyledProperty<string> BindPlaceProperty =
            AvaloniaProperty.Register<InPlaceEditor, string>("BindPlace", string.Empty);

        public string BindPlace
        {
            get { return GetValue(BindPlaceProperty); }
            set { SetValue(BindPlaceProperty, value); }
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
            this.designItem = designItem;
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            editor = new TextBox();
            editor = e.NameScope.Find<TextBox>("editor"); // Gets the TextBox-editor from the Template
            Debug.Assert(editor != null);

            if (editor != null)
            {
                editor.KeyDown += (sender, keyArgs) =>
                {
                    if (keyArgs.Key == Key.Enter && !keyArgs.KeyModifiers.HasFlag(KeyModifiers.Shift))
                    {
                        keyArgs.Handled = true;
                    }
                };
            }

            ToolTip.SetTip(this, "Edit the Text. Press" + Environment.NewLine + "Enter to make changes." + Environment.NewLine + "Shift+Enter to insert a newline." + Environment.NewLine + "Esc to cancel editing.");
        }

        /// <summary>
        /// Binds the Text Property of the element extended with <see cref="Bind"/>.
        /// </summary>
        /// <param name="textBlock"></param>
        public void SetBinding(TextBlock textBlock)
        {
            Debug.Assert(textBlock != null);
            this.textBlock = textBlock;
            var binding = new Binding("Text")
            {
                Source = this.textBlock,
                Mode = BindingMode.TwoWay
            };
            this.Bind(BindPlaceProperty, binding);
            property = PropertyUpdated(textBlock);
        }

        /// <summary>
        /// Returns the property that is being edited in the element for example editing Window Title returns "Title",
        /// Button text as "Content".
        /// </summary>
        private string PropertyUpdated(TextBlock text)
        {
            // Iterate through all set properties to find one that matches the text value
            foreach (var property in designItem.AllSetProperties)
            {
                if (property.DependencyProperty != null &&
                    property.ValueOnInstance != null &&
                    property.ValueOnInstance.ToString() == textBlock.Text)
                {
                    return property.Name;
                }
            }

            // Common text properties to check if no direct match found
            var commonTextProperties = new[] { "Content", "Text", "Header", "Title" };
            foreach (var propName in commonTextProperties)
            {
                var prop = designItem.Properties.HasProperty(propName);
                if (prop != null && prop.ValueOnInstance != null &&
                    prop.ValueOnInstance.ToString() == textBlock.Text)
                {
                    return propName;
                }
            }

            return null;
        }

        protected override void OnGotFocus(GotFocusEventArgs e)
        {
            base.OnGotFocus(e);
            StartEditing();
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            if (changeGroup != null && _isChangeGroupOpen)
            {
                changeGroup.Abort();
                _isChangeGroupOpen = false;
            }
            if (textBlock != null)
                textBlock.IsVisible = true;
            Reset();
            base.OnLostFocus(e);
        }

        /// <summary>
        /// Change is committed if the user releases the Escape Key.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (!e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            {
                switch (e.Key)
                {
                    case Key.Enter:
                        // Commit the changes to DOM.
                        if (property != null)
                            designItem.Properties[property].SetValue(BindPlace);
                        if ((FontFamily)designItem.Properties[TemplatedControl.FontFamilyProperty].ValueOnInstance != editor.FontFamily)
                            designItem.Properties[TemplatedControl.FontFamilyProperty].SetValue(editor.FontFamily);
                        if ((double)designItem.Properties[TemplatedControl.FontSizeProperty].ValueOnInstance != editor.FontSize)
                            designItem.Properties[TemplatedControl.FontSizeProperty].SetValue(editor.FontSize);
                        if ((FontStretch)designItem.Properties[TemplatedControl.FontStretchProperty].ValueOnInstance != editor.FontStretch)
                            designItem.Properties[TemplatedControl.FontStretchProperty].SetValue(editor.FontStretch);
                        if ((FontStyle)designItem.Properties[TemplatedControl.FontStyleProperty].ValueOnInstance != editor.FontStyle)
                            designItem.Properties[TemplatedControl.FontStyleProperty].SetValue(editor.FontStyle);
                        if ((FontWeight)designItem.Properties[TemplatedControl.FontWeightProperty].ValueOnInstance != editor.FontWeight)
                            designItem.Properties[TemplatedControl.FontWeightProperty].SetValue(editor.FontWeight);

                        if (changeGroup != null && _isChangeGroupOpen)
                        {
                            changeGroup.Commit();
                            _isChangeGroupOpen = false;
                        }
                        changeGroup = null;
                        this.IsVisible = false;
                        if (textBlock != null)
                            textBlock.IsVisible = true;
                        break;
                    case Key.Escape:
                        AbortEditing();
                        break;
                }
            }
            else if (e.Key == Key.Enter)
            {
                if (editor != null)
                {
                    var caretIndex = editor.CaretIndex;
                    var text = editor.Text ?? string.Empty;
                    editor.Text = text.Insert(caretIndex, Environment.NewLine);
                    editor.CaretIndex = caretIndex + Environment.NewLine.Length;
                }
            }
        }

        private void Reset()
        {
            if (textBlock != null)
            {
                if (property != null)
                {
                    textBlock.Text = designItem.Properties[property].ValueOnInstance as string;
                }
                textBlock.FontFamily = (Avalonia.Media.FontFamily)designItem.Properties[TemplatedControl.FontFamilyProperty].ValueOnInstance;
                textBlock.FontSize = (double)designItem.Properties[TemplatedControl.FontSizeProperty].ValueOnInstance;
                textBlock.FontStretch = (FontStretch)designItem.Properties[TemplatedControl.FontStretchProperty].ValueOnInstance;
                textBlock.FontStyle = (FontStyle)designItem.Properties[TemplatedControl.FontStyleProperty].ValueOnInstance;
                textBlock.FontWeight = (FontWeight)designItem.Properties[TemplatedControl.FontWeightProperty].ValueOnInstance;
            }
        }

        public void AbortEditing()
        {
            if (changeGroup != null && _isChangeGroupOpen)
            {
                Reset();
                changeGroup.Abort();
                _isChangeGroupOpen = false;
            }
            this.IsVisible = false;
            if (textBlock != null)
                textBlock.IsVisible = true;
            Reset();
        }

        public void StartEditing()
        {
            if (changeGroup == null)
            {
                changeGroup = designItem.OpenGroup("Change Text");
                _isChangeGroupOpen = true;
            }
            this.IsVisible = true;
            if (textBlock != null)
                textBlock.IsVisible = false;
        }
    }
}
