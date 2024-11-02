// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace gip.tool.installerAndUpdater
{
    /// <summary>
    /// Interaction logic for DBConnectingDialog.xaml
    /// </summary>
    public partial class OperationDialog : Window
    {
        public OperationDialog(string header)
        {
            InitializeComponent();
            _CancelTokenSource = new CancellationTokenSource();
            tbHeader.Text = header;
            Task.Run(() => Animate(_CancelTokenSource.Token));
        }

        CancellationTokenSource _CancelTokenSource;

        public void Animate(CancellationToken cancelToken)
        {
            int i = 0;
            while (true)
            {
                switch (i)
                {
                    case 1:
                        Dispatcher.Invoke(() => elipse1.Fill = Brushes.Red);
                        break;
                    case 2:
                        Dispatcher.Invoke(() => elipse2.Fill = Brushes.Red);
                        break;
                    case 3:
                        Dispatcher.Invoke(() => elipse3.Fill = Brushes.Red);
                        break;
                    case 4:
                        Dispatcher.Invoke(() => elipse1.Fill = Brushes.Transparent);
                        break;
                    case 5:
                        Dispatcher.Invoke(() => elipse2.Fill = Brushes.Transparent);
                        break;
                    case 6:
                        Dispatcher.Invoke(() => elipse3.Fill = Brushes.Transparent);
                        i = 0;
                        break;
                }
                i++;
                Thread.Sleep(300);

                if (cancelToken.IsCancellationRequested)
                    break;
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _CancelTokenSource.Cancel(false);
            base.OnClosing(e);
        }
    }
}
