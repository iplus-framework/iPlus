// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.ext.design.avui.PropertyGrid;
using gip.ext.design.avui;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace gip.ext.designer.avui.PropertyGrid.Editors
{
    [TypeEditor(typeof(MulticastDelegate))]
    public partial class EventEditor : TextBox
    {
        public EventEditor()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public IPropertyNode PropertyNode
        {
            get { return DataContext as IPropertyNode; }
        }

        public string ValueString
        {
            get
            {
                if (PropertyNode?.Value == null)
                    return "";
                else if (PropertyNode.Value is IConvertible)
                    return System.Convert.ToString(PropertyNode.Value);
                return PropertyNode.Value.GetType().ToString();
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Commit();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                // In Avalonia, we need to reset the binding differently
                if (PropertyNode != null)
                {
                    Text = ValueString;
                }
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                Commit();
                e.Handled = true;
            }
            base.OnPointerPressed(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            if (PropertyNode != null && Text != ValueString)
            {
                Commit();
            }
            base.OnLostFocus(e);
        }

        public void Commit()
        {
            if (Text != ValueString)
            {
                if (string.IsNullOrEmpty(Text))
                {
                    PropertyNode?.Reset();
                    return;
                }
                //if (PropertyNode.Value is MarkupExtension)
                //PropertyNode.Value = Text;
            }
            IEventHandlerService s = PropertyNode?.Services?.GetService<IEventHandlerService>();
            if (s != null)
            {
                s.CreateEventHandler(PropertyNode.FirstProperty);
            }
        }
    }
}
