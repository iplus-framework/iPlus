// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
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

namespace Document.Editor
{
    /// <summary>
    /// Interaktionslogik für TabHeader.xaml
    /// </summary>
    public partial class TabHeader : UserControl
    {
        public TabHeader()
        {
            InitializeComponent();
        }
        private bool showmenu;
        public static RoutedEvent CloseTabEvent = EventManager.RegisterRoutedEvent("CloseTab", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(TabHeader));
        public static RoutedEvent CloseAllButTabEvent = EventManager.RegisterRoutedEvent("CloseAllButTab", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(TabHeader));
        public static RoutedEvent CloseAllTabEvent = EventManager.RegisterRoutedEvent("CloseAllTab", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(TabHeader));
        public static RoutedEvent SaveTabEvent = EventManager.RegisterRoutedEvent("SaveTab", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(TabHeader));
        public static RoutedEvent SaveAsTabEvent = EventManager.RegisterRoutedEvent("SaveAsTab", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(TabHeader));

        public static RoutedEvent SaveAllTabEvent = EventManager.RegisterRoutedEvent("SaveAllTab", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(TabHeader));
        private void CloseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(CloseTabEvent));
        }

        private void SaveMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(SaveTabEvent));
        }

        private void SaveAsMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(SaveAsTabEvent));
        }

        public void SetMenu(bool show)
        {
            if (show == true)
            {
                this.ContextMenu = HeaderMenu;
            }
            else
            {
                this.ContextMenu = null;
            }
            showmenu = show;
        }

        private void CloseAllMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(CloseAllTabEvent));
        }

        private void SaveAllMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(SaveAllTabEvent));
        }

    }
}
