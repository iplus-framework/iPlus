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

namespace Document.Editor
{
    /// <summary>
    /// Interaktionslogik für FluentSymbolGallery.xaml
    /// </summary>
    public partial class SymbolPanel : UserControl
    {
        public SymbolPanel()
        {
            InitializeComponent();
        }
        public event ClickEventHandler Click;
        public delegate void ClickEventHandler(string symbol);

        #region "Currency"

        private void DollarButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Click != null)
            {
                Click("$");
            }
        }

        private void CentButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Click != null)
            {
                Click("¢");
            }
        }

        private void PoundButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Click != null)
            {
                Click("£");
            }
        }

        private void YenButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Click != null)
            {
                Click("¥");
            }
        }

        private void EuroButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Click != null)
            {
                Click("€");
            }
        }

        #endregion

        #region "Misc"

        private void CopyrightButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Click != null)
            {
                Click("©");
            }
        }

        private void RegisteredTrademarkButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Click != null)
            {
                Click("®");
            }
        }

        private void TrademarkButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Click != null)
            {
                Click("™");
            }
        }

        #endregion

        private void SuperscriptOneButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Click != null)
            {
                Click("¹");
            }
        }

        private void SuperscriptTwoButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Click != null)
            {
                Click("²");
            }
        }

        private void SuperscriptThreeButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Click != null)
            {
                Click("³");
            }
        }

    }
}
