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
    /// Interaktionslogik für FluentEmoticonGallery.xaml
    /// </summary>
    public partial class FluentEmoticonGallery : UserControl
    {
        public FluentEmoticonGallery()
        {
            InitializeComponent();
        }
        public event ClickEventHandler Click;
        public delegate void ClickEventHandler(BitmapSource img);

        #region "Small"

        private void EmoticonAngelSmall_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            BitmapSource i = EmoticonAngelSmall.Source as BitmapSource;
            if (Click != null)
            {
                Click(i);
            }
        }

        private void EmoticonAngrySmall_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            BitmapSource i = EmoticonAngrySmall.Source as BitmapSource;
            if (Click != null)
            {
                Click(i);
            }
        }

        private void EmoticonCoolSmall_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            BitmapSource i = EmoticonCoolSmall.Source as BitmapSource;
            if (Click != null)
            {
                Click(i);
            }
        }

        private void EmoticonCryingSmall_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            BitmapSource i = EmoticonCryingSmall.Source as BitmapSource;
            if (Click != null)
            {
                Click(i);
            }
        }

        private void EmoticonDevilishSmall_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            BitmapSource i = EmoticonDevilishSmall.Source as BitmapSource;
            if (Click != null)
            {
                Click(i);
            }
        }

        private void EmoticonEmbarrassedSmall_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            BitmapSource i = EmoticonEmbarrassedSmall.Source as BitmapSource;
            if (Click != null)
            {
                Click(i);
            }
        }

        private void EmoticonKissSmall_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            BitmapSource i = EmoticonKissSmall.Source as BitmapSource;
            if (Click != null)
            {
                Click(i);
            }
        }

        private void EmoticonlaughSmall_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            BitmapSource i = EmoticonlaughSmall.Source as BitmapSource;
            if (Click != null)
            {
                Click(i);
            }
        }

        private void EmoticonMonkeySmall_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            BitmapSource i = EmoticonMonkeySmall.Source as BitmapSource;
            if (Click != null)
            {
                Click(i);
            }
        }

        private void EmoticonPlainSmall_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            BitmapSource i = EmoticonMonkeySmall.Source as BitmapSource;
            if (Click != null)
            {
                Click(i);
            }
        }

        private void EmoticonRaspberrySmall_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            BitmapSource i = EmoticonRaspberrySmall.Source as BitmapSource;
            if (Click != null)
            {
                Click(i);
            }
        }

        private void EmoticonSadSmall_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            BitmapSource i = EmoticonSadSmall.Source as BitmapSource;
            if (Click != null)
            {
                Click(i);
            }
        }

        private void EmoticonSickSmall_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            BitmapSource i = EmoticonSickSmall.Source as BitmapSource;
            if (Click != null)
            {
                Click(i);
            }
        }

        private void EmoticonSmileBigSmall_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            BitmapSource i = EmoticonSmileBigSmall.Source as BitmapSource;
            if (Click != null)
            {
                Click(i);
            }
        }

        private void EmoticonSmileSmall_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            BitmapSource i = EmoticonSmileSmall.Source as BitmapSource;
            if (Click != null)
            {
                Click(i);
            }
        }

        private void EmoticonSmirkSmall_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            BitmapSource i = EmoticonSmirkSmall.Source as BitmapSource;
            if (Click != null)
            {
                Click(i);
            }
        }

        private void EmoticonSurpriseSmall_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            BitmapSource i = EmoticonSurpriseSmall.Source as BitmapSource;
            if (Click != null)
            {
                Click(i);
            }
        }

        private void EmoticonTiredSmall_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            BitmapSource i = EmoticonTiredSmall.Source as BitmapSource;
            if (Click != null)
            {
                Click(i);
            }
        }

        private void EmoticonUncertainSmall_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            BitmapSource i = EmoticonUncertainSmall.Source as BitmapSource;
            if (Click != null)
            {
                Click(i);
            }
        }

        private void EmoticonWinkSmall_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            BitmapSource i = EmoticonWinkSmall.Source as BitmapSource;
            if (Click != null)
            {
                Click(i);
            }
        }

        private void EmoticonWorriedSmall_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            BitmapSource i = EmoticonWorriedSmall.Source as BitmapSource;
            if (Click != null)
            {
                Click(i);
            }
        }

        #endregion

        #region "Normal"

        private void EmoticonAngelNormal_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            BitmapSource i = EmoticonAngelNormal.Source as BitmapSource;
            if (Click != null)
            {
                Click(i);
            }
        }

        private void EmoticonAngryNormal_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            BitmapSource i = EmoticonAngryNormal.Source as BitmapSource;
            if (Click != null)
            {
                Click(i);
            }
        }

        private void EmoticonCoolNormal_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            BitmapSource i = EmoticonCoolNormal.Source as BitmapSource;
            if (Click != null)
            {
                Click(i);
            }
        }

        private void EmoticonCryingNormal_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            BitmapSource i = EmoticonCryingNormal.Source as BitmapSource;
            if (Click != null)
            {
                Click(i);
            }
        }

        private void EmoticonDevilishNormal_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            BitmapSource i = EmoticonDevilishNormal.Source as BitmapSource;
            if (Click != null)
            {
                Click(i);
            }
        }

        private void EmoticonEmbarrassedNormal_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            BitmapSource i = EmoticonEmbarrassedNormal.Source as BitmapSource;
            if (Click != null)
            {
                Click(i);
            }
        }

        private void EmoticonKissNormal_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            BitmapSource i = EmoticonKissNormal.Source as BitmapSource;
            if (Click != null)
            {
                Click(i);
            }
        }

        private void EmoticonlaughNormal_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            BitmapSource i = EmoticonlaughNormal.Source as BitmapSource;
            if (Click != null)
            {
                Click(i);
            }
        }

        private void EmoticonMonkeyNormal_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            BitmapSource i = EmoticonMonkeyNormal.Source as BitmapSource;
            if (Click != null)
            {
                Click(i);
            }
        }

        private void EmoticonPlainNormal_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            BitmapSource i = EmoticonPlainNormal.Source as BitmapSource;
            if (Click != null)
            {
                Click(i);
            }
        }

        private void EmoticonRaspberryNormal_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            BitmapSource i = EmoticonRaspberryNormal.Source as BitmapSource;
            if (Click != null)
            {
                Click(i);
            }
        }

        private void EmoticonSadNormal_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            BitmapSource i = EmoticonSadNormal.Source as BitmapSource;
            if (Click != null)
            {
                Click(i);
            }
        }

        private void EmoticonSickNormal_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            BitmapSource i = EmoticonSickNormal.Source as BitmapSource;
            if (Click != null)
            {
                Click(i);
            }
        }

        private void EmoticonSmileBigNormal_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            BitmapSource i = EmoticonSmileBigNormal.Source as BitmapSource;
            if (Click != null)
            {
                Click(i);
            }
        }

        private void EmoticonSmileNormal_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            BitmapSource i = EmoticonSmileNormal.Source as BitmapSource;
            if (Click != null)
            {
                Click(i);
            }
        }

        private void EmoticonSmirkNormal_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            BitmapSource i = EmoticonSmirkNormal.Source as BitmapSource;
            if (Click != null)
            {
                Click(i);
            }
        }

        private void EmoticonSurpriseNormal_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            BitmapSource i = EmoticonSurpriseNormal.Source as BitmapSource;
            if (Click != null)
            {
                Click(i);
            }
        }

        private void EmoticonTiredNormal_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            BitmapSource i = EmoticonTiredNormal.Source as BitmapSource;
            if (Click != null)
            {
                Click(i);
            }
        }

        private void EmoticonUncertainNormal_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            BitmapSource i = EmoticonUncertainNormal.Source as BitmapSource;
            if (Click != null)
            {
                Click(i);
            }
        }

        private void EmoticonWinkNormal_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            BitmapSource i = EmoticonWinkNormal.Source as BitmapSource;
            if (Click != null)
            {
                Click(i);
            }
        }

        private void EmoticonWorriedNormal_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            BitmapSource i = EmoticonWorriedNormal.Source as BitmapSource;
            if (Click != null)
            {
                Click(i);
            }
        }

        #endregion

    }
}
