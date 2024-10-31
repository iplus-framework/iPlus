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
using gip.ext.design.PropertyGrid;
using gip.ext.design;
using System.Windows.Markup;

namespace gip.ext.designer.PropertyGrid.Editors
{
    [TypeEditor(typeof(MulticastDelegate))]
    public partial class EventEditor
    {
        public EventEditor()
        {
            InitializeComponent();
        }

        public IPropertyNode PropertyNode
        {
            get { return DataContext as IPropertyNode; }
        }

        public string ValueString
        {
            get
            {
                if (PropertyNode.Value == null)
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
            }
            else if (e.Key == Key.Escape)
            {
                BindingOperations.GetBindingExpression(this, TextProperty).UpdateTarget();
            }
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            Commit();
        }

        protected override void OnPreviewLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            if (PropertyNode != null && Text != ValueString)
            {
                Commit();
            }
        }

        public void Commit()
        {
            if (Text != ValueString)
            {
                if (string.IsNullOrEmpty(Text))
                {
                    PropertyNode.Reset();
                    return;
                }
                //if (PropertyNode.Value is MarkupExtension)
                //PropertyNode.Value = Text;
            }
            IEventHandlerService s = PropertyNode.Services.GetService<IEventHandlerService>();
            if (s != null)
            {
                s.CreateEventHandler(PropertyNode.FirstProperty);
            }
        }
    }
}
