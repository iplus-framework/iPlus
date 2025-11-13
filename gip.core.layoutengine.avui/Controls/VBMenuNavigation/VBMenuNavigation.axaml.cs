using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.layoutengine.avui
{
    public partial class VBMenuNavigation : UserControl
    {
        public VBMenuNavigation()
        {
            InitializeComponent();
        }

        public bool IsMenuOpen
        {
            get;
            set;
        }


        private void OnHamburgerClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
        }
    }
}
