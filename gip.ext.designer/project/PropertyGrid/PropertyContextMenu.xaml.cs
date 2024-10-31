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

namespace gip.ext.designer.PropertyGrid
{
    public partial class PropertyContextMenu
    {
        public PropertyContextMenu()
        {
            InitializeComponent();
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
