// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using gip.ext.design.avui.PropertyGrid;

namespace gip.ext.designer.avui.PropertyGrid
{
    public partial class PropertyContextMenu : ContextMenu
    {
        public PropertyContextMenu()
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

        void Click_Reset(object sender, RoutedEventArgs e)
        {
            PropertyNode.Reset();
        }

        void Click_Binding(object sender, RoutedEventArgs e)
        {
            PropertyNode.CreateBindings();
        }

        void Click_MultiBinding(object sender, RoutedEventArgs e)
        {
            PropertyNode.CreateMultiBindings();
        }

        void Click_CustomExpression(object sender, RoutedEventArgs e)
        {
        }

        void Click_ConvertToLocalValue(object sender, RoutedEventArgs e)
        {
        }

        void Click_SaveAsResource(object sender, RoutedEventArgs e)
        {
        }
    }
}
