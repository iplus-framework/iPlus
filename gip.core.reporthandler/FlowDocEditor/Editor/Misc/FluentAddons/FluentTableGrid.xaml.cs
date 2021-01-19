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
    /// Interaktionslogik für FluentTableGrid.xaml
    /// </summary>
    public partial class FluentTableGrid : UserControl
    {
        public FluentTableGrid()
        {
            InitializeComponent();

            TableGrid1X1.MouseDown += new MouseButtonEventHandler(TableGrid1X1_MouseDown);
            TableGrid1X2.MouseDown += new MouseButtonEventHandler(TableGrid1X2_MouseDown);
            TableGrid1X3.MouseDown += new MouseButtonEventHandler(TableGrid1X3_MouseDown);
            TableGrid1X4.MouseDown += new MouseButtonEventHandler(TableGrid1X4_MouseDown);
            TableGrid1X5.MouseDown += new MouseButtonEventHandler(TableGrid1X5_MouseDown);
            TableGrid1X6.MouseDown += new MouseButtonEventHandler(TableGrid1X6_MouseDown);
            TableGrid1X7.MouseDown += new MouseButtonEventHandler(TableGrid1X7_MouseDown);
            TableGrid1X8.MouseDown += new MouseButtonEventHandler(TableGrid1X8_MouseDown);
            TableGrid1X9.MouseDown += new MouseButtonEventHandler(TableGrid1X9_MouseDown);
            TableGrid1X10.MouseDown += new MouseButtonEventHandler(TableGrid1X10_MouseDown);

            TableGrid2X1.MouseDown += new MouseButtonEventHandler(TableGrid2X1_MouseDown);
            TableGrid2X2.MouseDown += new MouseButtonEventHandler(TableGrid2X2_MouseDown);
            TableGrid2X3.MouseDown += new MouseButtonEventHandler(TableGrid2X3_MouseDown);
            TableGrid2X4.MouseDown += new MouseButtonEventHandler(TableGrid2X4_MouseDown);
            TableGrid2X5.MouseDown += new MouseButtonEventHandler(TableGrid2X5_MouseDown);
            TableGrid2X6.MouseDown += new MouseButtonEventHandler(TableGrid2X6_MouseDown);
            TableGrid2X7.MouseDown += new MouseButtonEventHandler(TableGrid2X7_MouseDown);
            TableGrid2X8.MouseDown += new MouseButtonEventHandler(TableGrid2X8_MouseDown);
            TableGrid2X9.MouseDown += new MouseButtonEventHandler(TableGrid2X9_MouseDown);
            TableGrid2X10.MouseDown += new MouseButtonEventHandler(TableGrid2X10_MouseDown);

            TableGrid3X1.MouseDown += new MouseButtonEventHandler(TableGrid3X1_MouseDown);
            TableGrid3X2.MouseDown += new MouseButtonEventHandler(TableGrid3X2_MouseDown);
            TableGrid3X3.MouseDown += new MouseButtonEventHandler(TableGrid3X3_MouseDown);
            TableGrid3X4.MouseDown += new MouseButtonEventHandler(TableGrid3X4_MouseDown);
            TableGrid3X5.MouseDown += new MouseButtonEventHandler(TableGrid3X5_MouseDown);
            TableGrid3X6.MouseDown += new MouseButtonEventHandler(TableGrid3X6_MouseDown);
            TableGrid3X7.MouseDown += new MouseButtonEventHandler(TableGrid3X7_MouseDown);
            TableGrid3X8.MouseDown += new MouseButtonEventHandler(TableGrid3X8_MouseDown);
            TableGrid3X9.MouseDown += new MouseButtonEventHandler(TableGrid3X9_MouseDown);
            TableGrid3X10.MouseDown += new MouseButtonEventHandler(TableGrid3X10_MouseDown);

            TableGrid4X1.MouseDown += new MouseButtonEventHandler(TableGrid4X1_MouseDown);
            TableGrid4X2.MouseDown += new MouseButtonEventHandler(TableGrid4X2_MouseDown);
            TableGrid4X3.MouseDown += new MouseButtonEventHandler(TableGrid4X3_MouseDown);
            TableGrid4X4.MouseDown += new MouseButtonEventHandler(TableGrid4X4_MouseDown);
            TableGrid4X5.MouseDown += new MouseButtonEventHandler(TableGrid4X5_MouseDown);
            TableGrid4X6.MouseDown += new MouseButtonEventHandler(TableGrid4X6_MouseDown);
            TableGrid4X7.MouseDown += new MouseButtonEventHandler(TableGrid4X7_MouseDown);
            TableGrid4X8.MouseDown += new MouseButtonEventHandler(TableGrid4X8_MouseDown);
            TableGrid4X9.MouseDown += new MouseButtonEventHandler(TableGrid4X9_MouseDown);
            TableGrid4X10.MouseDown += new MouseButtonEventHandler(TableGrid4X10_MouseDown);

            TableGrid5X1.MouseDown += new MouseButtonEventHandler(TableGrid5X1_MouseDown);
            TableGrid5X2.MouseDown += new MouseButtonEventHandler(TableGrid5X2_MouseDown);
            TableGrid5X3.MouseDown += new MouseButtonEventHandler(TableGrid5X3_MouseDown);
            TableGrid5X4.MouseDown += new MouseButtonEventHandler(TableGrid5X4_MouseDown);
            TableGrid5X5.MouseDown += new MouseButtonEventHandler(TableGrid5X5_MouseDown);
            TableGrid5X6.MouseDown += new MouseButtonEventHandler(TableGrid5X6_MouseDown);
            TableGrid5X7.MouseDown += new MouseButtonEventHandler(TableGrid5X7_MouseDown);
            TableGrid5X8.MouseDown += new MouseButtonEventHandler(TableGrid5X8_MouseDown);
            TableGrid5X9.MouseDown += new MouseButtonEventHandler(TableGrid5X9_MouseDown);
            TableGrid5X10.MouseDown += new MouseButtonEventHandler(TableGrid5X10_MouseDown);

            TableGrid6X1.MouseDown += new MouseButtonEventHandler(TableGrid6X1_MouseDown);
            TableGrid6X2.MouseDown += new MouseButtonEventHandler(TableGrid6X2_MouseDown);
            TableGrid6X3.MouseDown += new MouseButtonEventHandler(TableGrid6X3_MouseDown);
            TableGrid6X4.MouseDown += new MouseButtonEventHandler(TableGrid6X4_MouseDown);
            TableGrid6X5.MouseDown += new MouseButtonEventHandler(TableGrid6X5_MouseDown);
            TableGrid6X6.MouseDown += new MouseButtonEventHandler(TableGrid6X6_MouseDown);
            TableGrid6X7.MouseDown += new MouseButtonEventHandler(TableGrid6X7_MouseDown);
            TableGrid6X8.MouseDown += new MouseButtonEventHandler(TableGrid6X8_MouseDown);
            TableGrid6X9.MouseDown += new MouseButtonEventHandler(TableGrid6X9_MouseDown);
            TableGrid6X10.MouseDown += new MouseButtonEventHandler(TableGrid6X10_MouseDown);

            TableGrid7X1.MouseDown += new MouseButtonEventHandler(TableGrid7X1_MouseDown);
            TableGrid7X2.MouseDown += new MouseButtonEventHandler(TableGrid7X2_MouseDown);
            TableGrid7X3.MouseDown += new MouseButtonEventHandler(TableGrid7X3_MouseDown);
            TableGrid7X4.MouseDown += new MouseButtonEventHandler(TableGrid7X4_MouseDown);
            TableGrid7X5.MouseDown += new MouseButtonEventHandler(TableGrid7X5_MouseDown);
            TableGrid7X6.MouseDown += new MouseButtonEventHandler(TableGrid7X6_MouseDown);
            TableGrid7X7.MouseDown += new MouseButtonEventHandler(TableGrid7X7_MouseDown);
            TableGrid7X8.MouseDown += new MouseButtonEventHandler(TableGrid7X8_MouseDown);
            TableGrid7X9.MouseDown += new MouseButtonEventHandler(TableGrid7X9_MouseDown);
            TableGrid7X10.MouseDown += new MouseButtonEventHandler(TableGrid7X10_MouseDown);

            TableGrid8X1.MouseDown += new MouseButtonEventHandler(TableGrid8X1_MouseDown);
            TableGrid8X2.MouseDown += new MouseButtonEventHandler(TableGrid8X2_MouseDown);
            TableGrid8X3.MouseDown += new MouseButtonEventHandler(TableGrid8X3_MouseDown);
            TableGrid8X4.MouseDown += new MouseButtonEventHandler(TableGrid8X4_MouseDown);
            TableGrid8X5.MouseDown += new MouseButtonEventHandler(TableGrid8X5_MouseDown);
            TableGrid8X6.MouseDown += new MouseButtonEventHandler(TableGrid8X6_MouseDown);
            TableGrid8X7.MouseDown += new MouseButtonEventHandler(TableGrid8X7_MouseDown);
            TableGrid8X8.MouseDown += new MouseButtonEventHandler(TableGrid8X8_MouseDown);
            TableGrid8X9.MouseDown += new MouseButtonEventHandler(TableGrid8X9_MouseDown);
            TableGrid8X10.MouseDown += new MouseButtonEventHandler(TableGrid8X10_MouseDown);

            TableGrid1X1.MouseEnter += new MouseEventHandler(TableGrid1X1_MouseEnter);
            TableGrid1X1.MouseLeave +=new MouseEventHandler(TableGrid1X1_MouseLeave);
            TableGrid1X2.MouseEnter += new MouseEventHandler(TableGrid1X2_MouseEnter);
            TableGrid1X2.MouseLeave += new MouseEventHandler(TableGrid1X2_MouseLeave);
            TableGrid1X3.MouseEnter += new MouseEventHandler(TableGrid1X3_MouseEnter);
            TableGrid1X3.MouseLeave += new MouseEventHandler(TableGrid1X3_MouseLeave);
            TableGrid1X4.MouseEnter += new MouseEventHandler(TableGrid1X4_MouseEnter);
            TableGrid1X4.MouseLeave += new MouseEventHandler(TableGrid1X4_MouseLeave);
            TableGrid1X5.MouseEnter += new MouseEventHandler(TableGrid1X5_MouseEnter);
            TableGrid1X5.MouseLeave += new MouseEventHandler(TableGrid1X5_MouseLeave);
            TableGrid1X6.MouseEnter += new MouseEventHandler(TableGrid1X6_MouseEnter);
            TableGrid1X6.MouseLeave += new MouseEventHandler(TableGrid1X6_MouseLeave);
            TableGrid1X7.MouseEnter += new MouseEventHandler(TableGrid1X7_MouseEnter);
            TableGrid1X7.MouseLeave += new MouseEventHandler(TableGrid1X7_MouseLeave);
            TableGrid1X8.MouseEnter += new MouseEventHandler(TableGrid1X8_MouseEnter);
            TableGrid1X8.MouseLeave += new MouseEventHandler(TableGrid1X8_MouseLeave);
            TableGrid1X9.MouseEnter += new MouseEventHandler(TableGrid1X9_MouseEnter);
            TableGrid1X9.MouseLeave += new MouseEventHandler(TableGrid1X9_MouseLeave);
            TableGrid1X10.MouseEnter += new MouseEventHandler(TableGrid1X10_MouseEnter);
            TableGrid1X10.MouseLeave += new MouseEventHandler(TableGrid1X10_MouseLeave);

            TableGrid2X1.MouseEnter += new MouseEventHandler(TableGrid2X1_MouseEnter);
            TableGrid2X1.MouseLeave += new MouseEventHandler(TableGrid2X1_MouseLeave);
            TableGrid2X2.MouseEnter += new MouseEventHandler(TableGrid2X2_MouseEnter);
            TableGrid2X2.MouseLeave += new MouseEventHandler(TableGrid2X2_MouseLeave);
            TableGrid2X3.MouseEnter += new MouseEventHandler(TableGrid2X3_MouseEnter);
            TableGrid2X3.MouseLeave += new MouseEventHandler(TableGrid2X3_MouseLeave);
            TableGrid2X4.MouseEnter += new MouseEventHandler(TableGrid2X4_MouseEnter);
            TableGrid2X4.MouseLeave += new MouseEventHandler(TableGrid2X4_MouseLeave);
            TableGrid2X5.MouseEnter += new MouseEventHandler(TableGrid2X5_MouseEnter);
            TableGrid2X5.MouseLeave += new MouseEventHandler(TableGrid2X5_MouseLeave);
            TableGrid2X6.MouseEnter += new MouseEventHandler(TableGrid2X6_MouseEnter);
            TableGrid2X6.MouseLeave += new MouseEventHandler(TableGrid2X6_MouseLeave);
            TableGrid2X7.MouseEnter += new MouseEventHandler(TableGrid2X7_MouseEnter);
            TableGrid2X7.MouseLeave += new MouseEventHandler(TableGrid2X7_MouseLeave);
            TableGrid2X8.MouseEnter += new MouseEventHandler(TableGrid2X8_MouseEnter);
            TableGrid2X8.MouseLeave += new MouseEventHandler(TableGrid2X8_MouseLeave);
            TableGrid2X9.MouseEnter += new MouseEventHandler(TableGrid2X9_MouseEnter);
            TableGrid2X9.MouseLeave += new MouseEventHandler(TableGrid2X9_MouseLeave);
            TableGrid2X10.MouseEnter += new MouseEventHandler(TableGrid2X10_MouseEnter);
            TableGrid2X10.MouseLeave += new MouseEventHandler(TableGrid2X10_MouseLeave);

            TableGrid3X1.MouseEnter += new MouseEventHandler(TableGrid3X1_MouseEnter);
            TableGrid3X1.MouseLeave += new MouseEventHandler(TableGrid3X1_MouseLeave);
            TableGrid3X2.MouseEnter += new MouseEventHandler(TableGrid3X2_MouseEnter);
            TableGrid3X2.MouseLeave += new MouseEventHandler(TableGrid3X2_MouseLeave);
            TableGrid3X3.MouseEnter += new MouseEventHandler(TableGrid3X3_MouseEnter);
            TableGrid3X3.MouseLeave += new MouseEventHandler(TableGrid3X3_MouseLeave);
            TableGrid3X4.MouseEnter += new MouseEventHandler(TableGrid3X4_MouseEnter);
            TableGrid3X4.MouseLeave += new MouseEventHandler(TableGrid3X4_MouseLeave);
            TableGrid3X5.MouseEnter += new MouseEventHandler(TableGrid3X5_MouseEnter);
            TableGrid3X5.MouseLeave += new MouseEventHandler(TableGrid3X5_MouseLeave);
            TableGrid3X6.MouseEnter += new MouseEventHandler(TableGrid3X6_MouseEnter);
            TableGrid3X6.MouseLeave += new MouseEventHandler(TableGrid3X6_MouseLeave);
            TableGrid3X7.MouseEnter += new MouseEventHandler(TableGrid3X7_MouseEnter);
            TableGrid3X7.MouseLeave += new MouseEventHandler(TableGrid3X7_MouseLeave);
            TableGrid3X8.MouseEnter += new MouseEventHandler(TableGrid3X8_MouseEnter);
            TableGrid3X8.MouseLeave += new MouseEventHandler(TableGrid3X8_MouseLeave);
            TableGrid3X9.MouseEnter += new MouseEventHandler(TableGrid3X9_MouseEnter);
            TableGrid3X9.MouseLeave += new MouseEventHandler(TableGrid3X9_MouseLeave);
            TableGrid3X10.MouseEnter += new MouseEventHandler(TableGrid3X10_MouseEnter);
            TableGrid3X10.MouseLeave += new MouseEventHandler(TableGrid3X10_MouseLeave);

            TableGrid4X1.MouseEnter += new MouseEventHandler(TableGrid4X1_MouseEnter);
            TableGrid4X1.MouseLeave += new MouseEventHandler(TableGrid4X1_MouseLeave);
            TableGrid4X2.MouseEnter += new MouseEventHandler(TableGrid4X2_MouseEnter);
            TableGrid4X2.MouseLeave += new MouseEventHandler(TableGrid4X2_MouseLeave);
            TableGrid4X3.MouseEnter += new MouseEventHandler(TableGrid4X3_MouseEnter);
            TableGrid4X3.MouseLeave += new MouseEventHandler(TableGrid4X3_MouseLeave);
            TableGrid4X4.MouseEnter += new MouseEventHandler(TableGrid4X4_MouseEnter);
            TableGrid4X4.MouseLeave += new MouseEventHandler(TableGrid4X4_MouseLeave);
            TableGrid4X5.MouseEnter += new MouseEventHandler(TableGrid4X5_MouseEnter);
            TableGrid4X5.MouseLeave += new MouseEventHandler(TableGrid4X5_MouseLeave);
            TableGrid4X6.MouseEnter += new MouseEventHandler(TableGrid4X6_MouseEnter);
            TableGrid4X6.MouseLeave += new MouseEventHandler(TableGrid4X6_MouseLeave);
            TableGrid4X7.MouseEnter += new MouseEventHandler(TableGrid4X7_MouseEnter);
            TableGrid4X7.MouseLeave += new MouseEventHandler(TableGrid4X7_MouseLeave);
            TableGrid4X8.MouseEnter += new MouseEventHandler(TableGrid4X8_MouseEnter);
            TableGrid4X8.MouseLeave += new MouseEventHandler(TableGrid4X8_MouseLeave);
            TableGrid4X9.MouseEnter += new MouseEventHandler(TableGrid4X9_MouseEnter);
            TableGrid4X9.MouseLeave += new MouseEventHandler(TableGrid4X9_MouseLeave);
            TableGrid4X10.MouseEnter += new MouseEventHandler(TableGrid4X10_MouseEnter);
            TableGrid4X10.MouseLeave += new MouseEventHandler(TableGrid4X10_MouseLeave);

            TableGrid5X1.MouseEnter += new MouseEventHandler(TableGrid5X1_MouseEnter);
            TableGrid5X1.MouseLeave += new MouseEventHandler(TableGrid5X1_MouseLeave);
            TableGrid5X2.MouseEnter += new MouseEventHandler(TableGrid5X2_MouseEnter);
            TableGrid5X2.MouseLeave += new MouseEventHandler(TableGrid5X2_MouseLeave);
            TableGrid5X3.MouseEnter += new MouseEventHandler(TableGrid5X3_MouseEnter);
            TableGrid5X3.MouseLeave += new MouseEventHandler(TableGrid5X3_MouseLeave);
            TableGrid5X4.MouseEnter += new MouseEventHandler(TableGrid5X4_MouseEnter);
            TableGrid5X4.MouseLeave += new MouseEventHandler(TableGrid5X4_MouseLeave);
            TableGrid5X5.MouseEnter += new MouseEventHandler(TableGrid5X5_MouseEnter);
            TableGrid5X5.MouseLeave += new MouseEventHandler(TableGrid5X5_MouseLeave);
            TableGrid5X6.MouseEnter += new MouseEventHandler(TableGrid5X6_MouseEnter);
            TableGrid5X6.MouseLeave += new MouseEventHandler(TableGrid5X6_MouseLeave);
            TableGrid5X7.MouseEnter += new MouseEventHandler(TableGrid5X7_MouseEnter);
            TableGrid5X7.MouseLeave += new MouseEventHandler(TableGrid5X7_MouseLeave);
            TableGrid5X8.MouseEnter += new MouseEventHandler(TableGrid5X8_MouseEnter);
            TableGrid5X8.MouseLeave += new MouseEventHandler(TableGrid5X8_MouseLeave);
            TableGrid5X9.MouseEnter += new MouseEventHandler(TableGrid5X9_MouseEnter);
            TableGrid5X9.MouseLeave += new MouseEventHandler(TableGrid5X9_MouseLeave);
            TableGrid5X10.MouseEnter += new MouseEventHandler(TableGrid5X10_MouseEnter);
            TableGrid5X10.MouseLeave += new MouseEventHandler(TableGrid5X10_MouseLeave);

            TableGrid6X1.MouseEnter += new MouseEventHandler(TableGrid6X1_MouseEnter);
            TableGrid6X1.MouseLeave += new MouseEventHandler(TableGrid6X1_MouseLeave);
            TableGrid6X2.MouseEnter += new MouseEventHandler(TableGrid6X2_MouseEnter);
            TableGrid6X2.MouseLeave += new MouseEventHandler(TableGrid6X2_MouseLeave);
            TableGrid6X3.MouseEnter += new MouseEventHandler(TableGrid6X3_MouseEnter);
            TableGrid6X3.MouseLeave += new MouseEventHandler(TableGrid6X3_MouseLeave);
            TableGrid6X4.MouseEnter += new MouseEventHandler(TableGrid6X4_MouseEnter);
            TableGrid6X4.MouseLeave += new MouseEventHandler(TableGrid6X4_MouseLeave);
            TableGrid6X5.MouseEnter += new MouseEventHandler(TableGrid6X5_MouseEnter);
            TableGrid6X5.MouseLeave += new MouseEventHandler(TableGrid6X5_MouseLeave);
            TableGrid6X6.MouseEnter += new MouseEventHandler(TableGrid6X6_MouseEnter);
            TableGrid6X6.MouseLeave += new MouseEventHandler(TableGrid6X6_MouseLeave);
            TableGrid6X7.MouseEnter += new MouseEventHandler(TableGrid6X7_MouseEnter);
            TableGrid6X7.MouseLeave += new MouseEventHandler(TableGrid6X7_MouseLeave);
            TableGrid6X8.MouseEnter += new MouseEventHandler(TableGrid6X8_MouseEnter);
            TableGrid6X8.MouseLeave += new MouseEventHandler(TableGrid6X8_MouseLeave);
            TableGrid6X9.MouseEnter += new MouseEventHandler(TableGrid6X9_MouseEnter);
            TableGrid6X9.MouseLeave += new MouseEventHandler(TableGrid6X9_MouseLeave);
            TableGrid6X10.MouseEnter += new MouseEventHandler(TableGrid6X10_MouseEnter);
            TableGrid6X10.MouseLeave += new MouseEventHandler(TableGrid6X10_MouseLeave);

            TableGrid7X1.MouseEnter += new MouseEventHandler(TableGrid7X1_MouseEnter);
            TableGrid7X1.MouseLeave += new MouseEventHandler(TableGrid7X1_MouseLeave);
            TableGrid7X2.MouseEnter += new MouseEventHandler(TableGrid7X2_MouseEnter);
            TableGrid7X2.MouseLeave += new MouseEventHandler(TableGrid7X2_MouseLeave);
            TableGrid7X3.MouseEnter += new MouseEventHandler(TableGrid7X3_MouseEnter);
            TableGrid7X3.MouseLeave += new MouseEventHandler(TableGrid7X3_MouseLeave);
            TableGrid7X4.MouseEnter += new MouseEventHandler(TableGrid7X4_MouseEnter);
            TableGrid7X4.MouseLeave += new MouseEventHandler(TableGrid7X4_MouseLeave);
            TableGrid7X5.MouseEnter += new MouseEventHandler(TableGrid7X5_MouseEnter);
            TableGrid7X5.MouseLeave += new MouseEventHandler(TableGrid7X5_MouseLeave);
            TableGrid7X6.MouseEnter += new MouseEventHandler(TableGrid7X6_MouseEnter);
            TableGrid7X6.MouseLeave += new MouseEventHandler(TableGrid7X6_MouseLeave);
            TableGrid7X7.MouseEnter += new MouseEventHandler(TableGrid7X7_MouseEnter);
            TableGrid7X7.MouseLeave += new MouseEventHandler(TableGrid7X7_MouseLeave);
            TableGrid7X8.MouseEnter += new MouseEventHandler(TableGrid7X8_MouseEnter);
            TableGrid7X8.MouseLeave += new MouseEventHandler(TableGrid7X8_MouseLeave);
            TableGrid7X9.MouseEnter += new MouseEventHandler(TableGrid7X9_MouseEnter);
            TableGrid7X9.MouseLeave += new MouseEventHandler(TableGrid7X9_MouseLeave);
            TableGrid7X10.MouseEnter += new MouseEventHandler(TableGrid7X10_MouseEnter);
            TableGrid7X10.MouseLeave += new MouseEventHandler(TableGrid7X10_MouseLeave);

            TableGrid8X1.MouseEnter += new MouseEventHandler(TableGrid8X1_MouseEnter);
            TableGrid8X1.MouseLeave += new MouseEventHandler(TableGrid8X1_MouseLeave);
            TableGrid8X2.MouseEnter += new MouseEventHandler(TableGrid8X2_MouseEnter);
            TableGrid8X2.MouseLeave += new MouseEventHandler(TableGrid8X2_MouseLeave);
            TableGrid8X3.MouseEnter += new MouseEventHandler(TableGrid8X3_MouseEnter);
            TableGrid8X3.MouseLeave += new MouseEventHandler(TableGrid8X3_MouseLeave);
            TableGrid8X4.MouseEnter += new MouseEventHandler(TableGrid8X4_MouseEnter);
            TableGrid8X4.MouseLeave += new MouseEventHandler(TableGrid8X4_MouseLeave);
            TableGrid8X5.MouseEnter += new MouseEventHandler(TableGrid8X5_MouseEnter);
            TableGrid8X5.MouseLeave += new MouseEventHandler(TableGrid8X5_MouseLeave);
            TableGrid8X6.MouseEnter += new MouseEventHandler(TableGrid8X6_MouseEnter);
            TableGrid8X6.MouseLeave += new MouseEventHandler(TableGrid8X6_MouseLeave);
            TableGrid8X7.MouseEnter += new MouseEventHandler(TableGrid8X7_MouseEnter);
            TableGrid8X7.MouseLeave += new MouseEventHandler(TableGrid8X7_MouseLeave);
            TableGrid8X8.MouseEnter += new MouseEventHandler(TableGrid8X8_MouseEnter);
            TableGrid8X8.MouseLeave += new MouseEventHandler(TableGrid8X8_MouseLeave);
            TableGrid8X9.MouseEnter += new MouseEventHandler(TableGrid8X9_MouseEnter);
            TableGrid8X9.MouseLeave += new MouseEventHandler(TableGrid8X9_MouseLeave);
            TableGrid8X10.MouseEnter += new MouseEventHandler(TableGrid8X10_MouseEnter);
            TableGrid8X10.MouseLeave += new MouseEventHandler(TableGrid8X10_MouseLeave);

            //TableGrid1X1.MouseEnter += new MouseEventHandler(TableGrid1X1_MouseEnter);
            //TableGrid1X1.MouseLeave += new MouseEventHandler(TableGrid1X1_MouseLeave);
            //TableGrid1X2.MouseEnter += new MouseEventHandler(TableGrid1X2_MouseEnter);
            //TableGrid1X2.MouseLeave += new MouseEventHandler(TableGrid1X2_MouseLeave);
            //TableGrid1X3.MouseEnter += new MouseEventHandler(TableGrid1X3_MouseEnter);
            //TableGrid1X3.MouseLeave += new MouseEventHandler(TableGrid1X3_MouseLeave);
            //TableGrid1X4.MouseEnter += new MouseEventHandler(TableGrid1X4_MouseEnter);
            //TableGrid1X4.MouseLeave += new MouseEventHandler(TableGrid1X4_MouseLeave);
            //TableGrid1X5.MouseEnter += new MouseEventHandler(TableGrid1X5_MouseEnter);
            //TableGrid1X5.MouseLeave += new MouseEventHandler(TableGrid1X5_MouseLeave);
            //TableGrid1X6.MouseEnter += new MouseEventHandler(TableGrid1X6_MouseEnter);
            //TableGrid1X6.MouseLeave += new MouseEventHandler(TableGrid1X6_MouseLeave);
            //TableGrid1X7.MouseEnter += new MouseEventHandler(TableGrid1X7_MouseEnter);
            //TableGrid1X7.MouseLeave += new MouseEventHandler(TableGrid1X7_MouseLeave);
            //TableGrid1X8.MouseEnter += new MouseEventHandler(TableGrid1X8_MouseEnter);
            //TableGrid1X8.MouseLeave += new MouseEventHandler(TableGrid1X8_MouseLeave);
            //TableGrid1X9.MouseEnter += new MouseEventHandler(TableGrid1X9_MouseEnter);
            //TableGrid1X9.MouseLeave += new MouseEventHandler(TableGrid1X9_MouseLeave);
            //TableGrid1X10.MouseEnter += new MouseEventHandler(TableGrid1X10_MouseEnter);
            //TableGrid1X10.MouseLeave += new MouseEventHandler(TableGrid1X10_MouseLeave);

            //TableGrid1X1.MouseEnter += new MouseEventHandler(TableGrid1X1_MouseEnter);
            //TableGrid1X1.MouseLeave += new MouseEventHandler(TableGrid1X1_MouseLeave);
            //TableGrid1X2.MouseEnter += new MouseEventHandler(TableGrid1X2_MouseEnter);
            //TableGrid1X2.MouseLeave += new MouseEventHandler(TableGrid1X2_MouseLeave);
            //TableGrid1X3.MouseEnter += new MouseEventHandler(TableGrid1X3_MouseEnter);
            //TableGrid1X3.MouseLeave += new MouseEventHandler(TableGrid1X3_MouseLeave);
            //TableGrid1X4.MouseEnter += new MouseEventHandler(TableGrid1X4_MouseEnter);
            //TableGrid1X4.MouseLeave += new MouseEventHandler(TableGrid1X4_MouseLeave);
            //TableGrid1X5.MouseEnter += new MouseEventHandler(TableGrid1X5_MouseEnter);
            //TableGrid1X5.MouseLeave += new MouseEventHandler(TableGrid1X5_MouseLeave);
            //TableGrid1X6.MouseEnter += new MouseEventHandler(TableGrid1X6_MouseEnter);
            //TableGrid1X6.MouseLeave += new MouseEventHandler(TableGrid1X6_MouseLeave);
            //TableGrid1X7.MouseEnter += new MouseEventHandler(TableGrid1X7_MouseEnter);
            //TableGrid1X7.MouseLeave += new MouseEventHandler(TableGrid1X7_MouseLeave);
            //TableGrid1X8.MouseEnter += new MouseEventHandler(TableGrid1X8_MouseEnter);
            //TableGrid1X8.MouseLeave += new MouseEventHandler(TableGrid1X8_MouseLeave);
            //TableGrid1X9.MouseEnter += new MouseEventHandler(TableGrid1X9_MouseEnter);
            //TableGrid1X9.MouseLeave += new MouseEventHandler(TableGrid1X9_MouseLeave);
            //TableGrid1X10.MouseEnter += new MouseEventHandler(TableGrid1X10_MouseEnter);
            //TableGrid1X10.MouseLeave += new MouseEventHandler(TableGrid1X10_MouseLeave);
        }
        public event ClickEventHandler Click;
        public delegate void ClickEventHandler(int y, int x);

        #region "Highlight"

        #region "Row 1"

        private void TableGrid1X1_MouseEnter(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.Gray);
        }

        private void TableGrid1X1_MouseLeave(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.LightGray);
        }

        private void TableGrid1X2_MouseEnter(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid1X2.Background = new SolidColorBrush(Colors.Gray);
        }

        private void TableGrid1X2_MouseLeave(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid1X2.Background = new SolidColorBrush(Colors.LightGray);
        }

        private void TableGrid1X3_MouseEnter(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid1X2.Background = new SolidColorBrush(Colors.Gray);
            TableGrid1X3.Background = new SolidColorBrush(Colors.Gray);
        }

        private void TableGrid1X3_MouseLeave(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid1X2.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid1X3.Background = new SolidColorBrush(Colors.LightGray);
        }

        private void TableGrid1X4_MouseEnter(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid1X2.Background = new SolidColorBrush(Colors.Gray);
            TableGrid1X3.Background = new SolidColorBrush(Colors.Gray);
            TableGrid1X4.Background = new SolidColorBrush(Colors.Gray);
        }

        private void TableGrid1X4_MouseLeave(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid1X2.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid1X3.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid1X4.Background = new SolidColorBrush(Colors.LightGray);
        }

        private void TableGrid1X5_MouseEnter(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid1X2.Background = new SolidColorBrush(Colors.Gray);
            TableGrid1X3.Background = new SolidColorBrush(Colors.Gray);
            TableGrid1X4.Background = new SolidColorBrush(Colors.Gray);
            TableGrid1X5.Background = new SolidColorBrush(Colors.Gray);
        }

        private void TableGrid1X5_MouseLeave(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid1X2.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid1X3.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid1X4.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid1X5.Background = new SolidColorBrush(Colors.LightGray);
        }

        private void TableGrid1X6_MouseEnter(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid1X2.Background = new SolidColorBrush(Colors.Gray);
            TableGrid1X3.Background = new SolidColorBrush(Colors.Gray);
            TableGrid1X4.Background = new SolidColorBrush(Colors.Gray);
            TableGrid1X5.Background = new SolidColorBrush(Colors.Gray);
            TableGrid1X6.Background = new SolidColorBrush(Colors.Gray);
        }

        private void TableGrid1X6_MouseLeave(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid1X2.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid1X3.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid1X4.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid1X5.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid1X6.Background = new SolidColorBrush(Colors.LightGray);
        }

        private void TableGrid1X7_MouseEnter(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid1X2.Background = new SolidColorBrush(Colors.Gray);
            TableGrid1X3.Background = new SolidColorBrush(Colors.Gray);
            TableGrid1X4.Background = new SolidColorBrush(Colors.Gray);
            TableGrid1X5.Background = new SolidColorBrush(Colors.Gray);
            TableGrid1X6.Background = new SolidColorBrush(Colors.Gray);
            TableGrid1X7.Background = new SolidColorBrush(Colors.Gray);
        }

        private void TableGrid1X7_MouseLeave(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid1X2.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid1X3.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid1X4.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid1X5.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid1X6.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid1X7.Background = new SolidColorBrush(Colors.LightGray);
        }

        private void TableGrid1X8_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                g.Background = new SolidColorBrush(Colors.Gray);
                count += 1;
                if (count == 8)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
            }
        }

        private void TableGrid1X8_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                g.Background = new SolidColorBrush(Colors.LightGray);
                count += 1;
                if (count == 8)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
            }
        }

        private void TableGrid1X9_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                g.Background = new SolidColorBrush(Colors.Gray);
                count += 1;
                if (count == 9)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
            }
        }

        private void TableGrid1X9_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                g.Background = new SolidColorBrush(Colors.LightGray);
                count += 1;
                if (count == 9)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
            }
        }

        private void TableGrid1X10_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                g.Background = new SolidColorBrush(Colors.Gray);
                count += 1;
                if (count == 10)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
            }
        }

        private void TableGrid1X10_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                g.Background = new SolidColorBrush(Colors.LightGray);
                count += 1;
                if (count == 10)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
            }
        }

        #endregion

        #region "Row 2"

        private void TableGrid2X1_MouseEnter(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid2X1.Background = new SolidColorBrush(Colors.Gray);
        }

        private void TableGrid2X1_MouseLeave(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid2X1.Background = new SolidColorBrush(Colors.LightGray);
        }

        private void TableGrid2X2_MouseEnter(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid1X2.Background = new SolidColorBrush(Colors.Gray);
            TableGrid2X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid2X2.Background = new SolidColorBrush(Colors.Gray);
        }

        private void TableGrid2X2_MouseLeave(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid1X2.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid2X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid2X2.Background = new SolidColorBrush(Colors.LightGray);
        }

        private void TableGrid2X3_MouseEnter(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid1X2.Background = new SolidColorBrush(Colors.Gray);
            TableGrid1X3.Background = new SolidColorBrush(Colors.Gray);
            TableGrid2X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid2X2.Background = new SolidColorBrush(Colors.Gray);
            TableGrid2X3.Background = new SolidColorBrush(Colors.Gray);
        }

        private void TableGrid2X3_MouseLeave(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid1X2.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid1X3.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid2X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid2X2.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid2X3.Background = new SolidColorBrush(Colors.LightGray);
        }

        private void TableGrid2X4_MouseEnter(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid1X2.Background = new SolidColorBrush(Colors.Gray);
            TableGrid1X3.Background = new SolidColorBrush(Colors.Gray);
            TableGrid1X4.Background = new SolidColorBrush(Colors.Gray);
            TableGrid2X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid2X2.Background = new SolidColorBrush(Colors.Gray);
            TableGrid2X3.Background = new SolidColorBrush(Colors.Gray);
            TableGrid2X4.Background = new SolidColorBrush(Colors.Gray);
        }

        private void TableGrid2X4_MouseLeave(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid1X2.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid1X3.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid1X4.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid2X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid2X2.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid2X3.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid2X4.Background = new SolidColorBrush(Colors.LightGray);
        }

        private void TableGrid2X5_MouseEnter(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid1X2.Background = new SolidColorBrush(Colors.Gray);
            TableGrid1X3.Background = new SolidColorBrush(Colors.Gray);
            TableGrid1X4.Background = new SolidColorBrush(Colors.Gray);
            TableGrid1X5.Background = new SolidColorBrush(Colors.Gray);
            TableGrid2X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid2X2.Background = new SolidColorBrush(Colors.Gray);
            TableGrid2X3.Background = new SolidColorBrush(Colors.Gray);
            TableGrid2X4.Background = new SolidColorBrush(Colors.Gray);
            TableGrid2X5.Background = new SolidColorBrush(Colors.Gray);
        }

        private void TableGrid2X5_MouseLeave(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid1X2.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid1X3.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid1X4.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid1X5.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid2X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid2X2.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid2X3.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid2X4.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid2X5.Background = new SolidColorBrush(Colors.LightGray);
        }

        private void TableGrid2X6_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 7 || count == 9 || count == 11 || count == 13)
                {
                    count += 1;
                }
                else if (count == 21)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid2X6_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 7 || count == 9 || count == 11 || count == 13)
                {
                    count += 1;
                }
                else if (count == 21)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid2X7_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 8 || count == 10 || count == 12)
                {
                    count += 1;
                }
                else if (count == 21)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid2X7_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 8 || count == 10 || count == 12)
                {
                    count += 1;
                }
                else if (count == 21)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid2X8_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 9 || count == 11)
                {
                    count += 1;
                }
                else if (count == 21)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid2X8_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 9 || count == 11)
                {
                    count += 1;
                }
                else if (count == 21)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid2X9_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 10)
                {
                    count += 1;
                }
                else if (count == 21)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid2X9_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 10)
                {
                    count += 1;
                }
                else if (count == 21)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid2X10_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                g.Background = new SolidColorBrush(Colors.Gray);
                count += 1;
                if (count == 20)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
            }
        }

        private void TableGrid2X10_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                g.Background = new SolidColorBrush(Colors.LightGray);
                count += 1;
                if (count == 20)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
            }
        }

        #endregion

        #region "Row 3"

        private void TableGrid3X1_MouseEnter(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid2X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid3X1.Background = new SolidColorBrush(Colors.Gray);
        }

        private void TableGrid3X1_MouseLeave(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid2X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid3X1.Background = new SolidColorBrush(Colors.LightGray);
        }

        private void TableGrid3X2_MouseEnter(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid1X2.Background = new SolidColorBrush(Colors.Gray);
            TableGrid2X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid2X2.Background = new SolidColorBrush(Colors.Gray);
            TableGrid3X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid3X2.Background = new SolidColorBrush(Colors.Gray);
        }

        private void TableGrid3X2_MouseLeave(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid1X2.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid2X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid2X2.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid3X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid3X2.Background = new SolidColorBrush(Colors.LightGray);
        }

        private void TableGrid3X3_MouseEnter(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid1X2.Background = new SolidColorBrush(Colors.Gray);
            TableGrid1X3.Background = new SolidColorBrush(Colors.Gray);
            TableGrid2X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid2X2.Background = new SolidColorBrush(Colors.Gray);
            TableGrid2X3.Background = new SolidColorBrush(Colors.Gray);
            TableGrid3X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid3X2.Background = new SolidColorBrush(Colors.Gray);
            TableGrid3X3.Background = new SolidColorBrush(Colors.Gray);
        }

        private void TableGrid3X3_MouseLeave(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid1X2.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid1X3.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid2X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid2X2.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid2X3.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid3X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid3X2.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid3X3.Background = new SolidColorBrush(Colors.LightGray);
        }

        private void TableGrid3X4_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 5 || count == 7 || count == 9 || count == 11 || count == 13 || count == 15 || count == 21 || count == 23 || count == 25 || count == 27 || count == 29 || count == 31)
                {
                    count += 1;
                }
                else if (count == 37)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid3X4_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 5 || count == 7 || count == 9 || count == 11 || count == 13 || count == 15 || count == 21 || count == 23 || count == 25 || count == 27 || count == 29 || count == 31)
                {
                    count += 1;
                }
                else if (count == 37)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid3X5_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 6 || count == 8 || count == 10 || count == 12 || count == 14 || count == 21 || count == 23 || count == 25 || count == 27 || count == 29)
                {
                    count += 1;
                }
                else if (count == 36)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid3X5_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 6 || count == 8 || count == 10 || count == 12 || count == 14 || count == 21 || count == 23 || count == 25 || count == 27 || count == 29)
                {
                    count += 1;
                }
                else if (count == 36)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid3X6_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 7 || count == 9 || count == 11 || count == 13 || count == 21 || count == 23 || count == 25 || count == 27)
                {
                    count += 1;
                }
                else if (count == 35)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid3X6_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 7 || count == 9 || count == 11 || count == 13 || count == 21 || count == 23 || count == 25 || count == 27)
                {
                    count += 1;
                }
                else if (count == 35)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid3X7_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 8 || count == 10 || count == 12 || count == 21 || count == 23 || count == 25)
                {
                    count += 1;
                }
                else if (count == 34)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid3X7_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 8 || count == 10 || count == 12 || count == 21 || count == 23 || count == 25)
                {
                    count += 1;
                }
                else if (count == 34)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid3X8_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 9 || count == 11 || count == 21 || count == 23)
                {
                    count += 1;
                }
                else if (count == 33)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid3X8_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 9 || count == 11 || count == 21 || count == 23)
                {
                    count += 1;
                }
                else if (count == 33)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid3X9_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 10 || count == 21)
                {
                    count += 1;
                }
                else if (count == 32)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid3X9_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 10 || count == 21)
                {
                    count += 1;
                }
                else if (count == 32)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid3X10_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                g.Background = new SolidColorBrush(Colors.Gray);
                count += 1;
                if (count == 30)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
            }
        }

        private void TableGrid3X10_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                g.Background = new SolidColorBrush(Colors.LightGray);
                count += 1;
                if (count == 30)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
            }
        }

        #endregion

        #region "Row 4"

        private void TableGrid4X1_MouseEnter(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid2X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid3X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid4X1.Background = new SolidColorBrush(Colors.Gray);
        }

        private void TableGrid4X1_MouseLeave(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid2X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid3X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid4X1.Background = new SolidColorBrush(Colors.LightGray);
        }

        private void TableGrid4X2_MouseEnter(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid1X2.Background = new SolidColorBrush(Colors.Gray);
            TableGrid2X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid2X2.Background = new SolidColorBrush(Colors.Gray);
            TableGrid3X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid3X2.Background = new SolidColorBrush(Colors.Gray);
            TableGrid4X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid4X2.Background = new SolidColorBrush(Colors.Gray);
        }

        private void TableGrid4X2_MouseLeave(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid1X2.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid2X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid2X2.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid3X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid3X2.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid4X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid4X2.Background = new SolidColorBrush(Colors.LightGray);
        }

        private void TableGrid4X3_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 4 || count == 6 || count == 8 || count == 10 || count == 12 || count == 14 || count == 16 || count == 21 || count == 23 || count == 25 || count == 27 || count == 29 || count == 31 || count == 33 || count == 38 || count == 40 || count == 42 || count == 44 || count == 46 || count == 48 || count == 50)
                {
                    count += 1;
                }
                else if (count == 55)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid4X3_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 4 || count == 6 || count == 8 || count == 10 || count == 12 || count == 14 || count == 16 || count == 21 || count == 23 || count == 25 || count == 27 || count == 29 || count == 31 || count == 33 || count == 38 || count == 40 || count == 42 || count == 44 || count == 46 || count == 48 || count == 50)
                {
                    count += 1;
                }
                else if (count == 55)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid4X4_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 5 || count == 7 || count == 9 || count == 11 || count == 13 || count == 15 || count == 21 || count == 23 || count == 25 || count == 27 || count == 29 || count == 31 || count == 37 || count == 39 || count == 41 || count == 43 || count == 45 || count == 47)
                {
                    count += 1;
                }
                else if (count == 53)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid4X4_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 5 || count == 7 || count == 9 || count == 11 || count == 13 || count == 15 || count == 21 || count == 23 || count == 25 || count == 27 || count == 29 || count == 31 || count == 37 || count == 39 || count == 41 || count == 43 || count == 45 || count == 47)
                {
                    count += 1;
                }
                else if (count == 53)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid4X5_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 6 || count == 8 || count == 10 || count == 12 || count == 14 || count == 21 || count == 23 || count == 25 || count == 27 || count == 29 || count == 36 || count == 38 || count == 40 || count == 42 || count == 44)
                {
                    count += 1;
                }
                else if (count == 51)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid4X5_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 6 || count == 8 || count == 10 || count == 12 || count == 14 || count == 21 || count == 23 || count == 25 || count == 27 || count == 29 || count == 36 || count == 38 || count == 40 || count == 42 || count == 44)
                {
                    count += 1;
                }
                else if (count == 51)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid4X6_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 7 || count == 9 || count == 11 || count == 13 || count == 21 || count == 23 || count == 25 || count == 27 || count == 35 || count == 37 || count == 39 || count == 41)
                {
                    count += 1;
                }
                else if (count == 49)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid4X6_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 7 || count == 9 || count == 11 || count == 13 || count == 21 || count == 23 || count == 25 || count == 27 || count == 35 || count == 37 || count == 39 || count == 41)
                {
                    count += 1;
                }
                else if (count == 49)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid4X7_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 8 || count == 10 || count == 12 || count == 21 || count == 23 || count == 25 || count == 34 || count == 36 || count == 38)
                {
                    count += 1;
                }
                else if (count == 47)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid4X7_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 8 || count == 10 || count == 12 || count == 21 || count == 23 || count == 25 || count == 34 || count == 36 || count == 38)
                {
                    count += 1;
                }
                else if (count == 47)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid4X8_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 9 || count == 11 || count == 21 || count == 23 || count == 33 || count == 35)
                {
                    count += 1;
                }
                else if (count == 45)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid4X8_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 9 || count == 11 || count == 21 || count == 23 || count == 33 || count == 35)
                {
                    count += 1;
                }
                else if (count == 45)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid4X9_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 10 || count == 21 || count == 32)
                {
                    count += 1;
                }
                else if (count == 43)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid4X9_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 10 || count == 21 || count == 32)
                {
                    count += 1;
                }
                else if (count == 43)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid4X10_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                g.Background = new SolidColorBrush(Colors.Gray);
                count += 1;
                if (count == 40)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
            }
        }

        private void TableGrid4X10_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                g.Background = new SolidColorBrush(Colors.LightGray);
                count += 1;
                if (count == 40)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
            }
        }

        #endregion

        #region "Row 5"

        private void TableGrid5X1_MouseEnter(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid2X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid3X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid4X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid5X1.Background = new SolidColorBrush(Colors.Gray);
        }

        private void TableGrid5X1_MouseLeave(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid2X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid3X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid4X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid5X1.Background = new SolidColorBrush(Colors.LightGray);
        }

        private void TableGrid5X2_MouseEnter(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid1X2.Background = new SolidColorBrush(Colors.Gray);
            TableGrid2X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid2X2.Background = new SolidColorBrush(Colors.Gray);
            TableGrid3X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid3X2.Background = new SolidColorBrush(Colors.Gray);
            TableGrid4X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid4X2.Background = new SolidColorBrush(Colors.Gray);
            TableGrid5X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid5X2.Background = new SolidColorBrush(Colors.Gray);
        }

        private void TableGrid5X2_MouseLeave(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid1X2.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid2X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid2X2.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid3X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid3X2.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid4X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid4X2.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid5X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid5X2.Background = new SolidColorBrush(Colors.LightGray);
        }

        private void TableGrid5X3_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 4 || count == 6 || count == 8 || count == 10 || count == 12 || count == 14 || count == 16 || count == 21 || count == 23 || count == 25 || count == 27 || count == 29 || count == 31 || count == 33 || count == 38 || count == 40 || count == 42 || count == 44 || count == 46 || count == 48 || count == 50 || count == 55 || count == 57 || count == 59 || count == 61 || count == 63 || count == 65 || count == 67)
                {
                    count += 1;
                }
                else if (count == 72)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid5X3_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 4 || count == 6 || count == 8 || count == 10 || count == 12 || count == 14 || count == 16 || count == 21 || count == 23 || count == 25 || count == 27 || count == 29 || count == 31 || count == 33 || count == 38 || count == 40 || count == 42 || count == 44 || count == 46 || count == 48 || count == 50 || count == 55 || count == 57 || count == 59 || count == 61 || count == 63 || count == 65 || count == 67)
                {
                    count += 1;
                }
                else if (count == 72)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid5X4_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 5 || count == 7 || count == 9 || count == 11 || count == 13 || count == 15 || count == 21 || count == 23 || count == 25 || count == 27 || count == 29 || count == 31 || count == 37 || count == 39 || count == 41 || count == 43 || count == 45 || count == 47 || count == 53 || count == 55 || count == 57 || count == 59 || count == 61 || count == 63)
                {
                    count += 1;
                }
                else if (count == 69)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid5X4_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 5 || count == 7 || count == 9 || count == 11 || count == 13 || count == 15 || count == 21 || count == 23 || count == 25 || count == 27 || count == 29 || count == 31 || count == 37 || count == 39 || count == 41 || count == 43 || count == 45 || count == 47 || count == 53 || count == 55 || count == 57 || count == 59 || count == 61 || count == 63)
                {
                    count += 1;
                }
                else if (count == 69)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid5X5_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 6 || count == 8 || count == 10 || count == 12 || count == 14 || count == 21 || count == 23 || count == 25 || count == 27 || count == 29 || count == 36 || count == 38 || count == 40 || count == 42 || count == 44 || count == 51 || count == 53 || count == 55 || count == 57 || count == 59)
                {
                    count += 1;
                }
                else if (count == 66)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid5X5_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 6 || count == 8 || count == 10 || count == 12 || count == 14 || count == 21 || count == 23 || count == 25 || count == 27 || count == 29 || count == 36 || count == 38 || count == 40 || count == 42 || count == 44 || count == 51 || count == 53 || count == 55 || count == 57 || count == 59)
                {
                    count += 1;
                }
                else if (count == 66)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid5X6_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 7 || count == 9 || count == 11 || count == 13 || count == 21 || count == 23 || count == 25 || count == 27 || count == 35 || count == 37 || count == 39 || count == 41 || count == 49 || count == 51 || count == 53 || count == 55)
                {
                    count += 1;
                }
                else if (count == 63)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid5X6_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 7 || count == 9 || count == 11 || count == 13 || count == 21 || count == 23 || count == 25 || count == 27 || count == 35 || count == 37 || count == 39 || count == 41 || count == 49 || count == 51 || count == 53 || count == 55)
                {
                    count += 1;
                }
                else if (count == 63)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid5X7_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 8 || count == 10 || count == 12 || count == 21 || count == 23 || count == 25 || count == 34 || count == 36 || count == 38 || count == 47 || count == 49 || count == 51)
                {
                    count += 1;
                }
                else if (count == 60)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid5X7_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 8 || count == 10 || count == 12 || count == 21 || count == 23 || count == 25 || count == 34 || count == 36 || count == 38 || count == 47 || count == 49 || count == 51)
                {
                    count += 1;
                }
                else if (count == 60)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid5X8_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 9 || count == 11 || count == 21 || count == 23 || count == 33 || count == 35 || count == 45 || count == 47)
                {
                    count += 1;
                }
                else if (count == 57)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid5X8_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 9 || count == 11 || count == 21 || count == 23 || count == 33 || count == 35 || count == 45 || count == 47)
                {
                    count += 1;
                }
                else if (count == 57)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid5X9_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 10 || count == 21 || count == 32 || count == 43)
                {
                    count += 1;
                }
                else if (count == 54)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid5X9_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 10 || count == 21 || count == 32 || count == 43)
                {
                    count += 1;
                }
                else if (count == 54)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid5X10_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                g.Background = new SolidColorBrush(Colors.Gray);
                count += 1;
                if (count == 50)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
            }
        }

        private void TableGrid5X10_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                g.Background = new SolidColorBrush(Colors.LightGray);
                count += 1;
                if (count == 50)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
            }
        }

        #endregion

        #region "Row 6"

        private void TableGrid6X1_MouseEnter(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid2X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid3X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid4X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid5X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid6X1.Background = new SolidColorBrush(Colors.Gray);
        }

        private void TableGrid6X1_MouseLeave(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid2X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid3X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid4X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid5X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid6X1.Background = new SolidColorBrush(Colors.LightGray);
        }

        private void TableGrid6X2_MouseEnter(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid1X2.Background = new SolidColorBrush(Colors.Gray);
            TableGrid2X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid2X2.Background = new SolidColorBrush(Colors.Gray);
            TableGrid3X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid3X2.Background = new SolidColorBrush(Colors.Gray);
            TableGrid4X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid4X2.Background = new SolidColorBrush(Colors.Gray);
            TableGrid5X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid5X2.Background = new SolidColorBrush(Colors.Gray);
            TableGrid6X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid6X2.Background = new SolidColorBrush(Colors.Gray);
        }

        private void TableGrid6X2_MouseLeave(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid1X2.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid2X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid2X2.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid3X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid3X2.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid4X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid4X2.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid5X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid5X2.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid6X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid6X2.Background = new SolidColorBrush(Colors.LightGray);
        }

        private void TableGrid6X3_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 4 || count == 6 || count == 8 || count == 10 || count == 12 || count == 14 || count == 16 || count == 21 || count == 23 || count == 25 || count == 27 || count == 29 || count == 31 || count == 33 || count == 38 || count == 40 || count == 42 || count == 44 || count == 46 || count == 48 || count == 50 || count == 55 || count == 57 || count == 59 || count == 61 || count == 63 || count == 65 || count == 67 || count == 72 || count == 74 || count == 76 || count == 78 || count == 80 || count == 82 || count == 84)
                {
                    count += 1;
                }
                else if (count == 89)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid6X3_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 4 || count == 6 || count == 8 || count == 10 || count == 12 || count == 14 || count == 16 || count == 21 || count == 23 || count == 25 || count == 27 || count == 29 || count == 31 || count == 33 || count == 38 || count == 40 || count == 42 || count == 44 || count == 46 || count == 48 || count == 50 || count == 55 || count == 57 || count == 59 || count == 61 || count == 63 || count == 65 || count == 67 || count == 72 || count == 74 || count == 76 || count == 78 || count == 80 || count == 82 || count == 84)
                {
                    count += 1;
                }
                else if (count == 89)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid6X4_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 5 || count == 7 || count == 9 || count == 11 || count == 13 || count == 15 || count == 21 || count == 23 || count == 25 || count == 27 || count == 29 || count == 31 || count == 37 || count == 39 || count == 41 || count == 43 || count == 45 || count == 47 || count == 53 || count == 55 || count == 57 || count == 59 || count == 61 || count == 63 || count == 69 || count == 71 || count == 73 || count == 75 || count == 77 || count == 79)
                {
                    count += 1;
                }
                else if (count == 85)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid6X4_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 5 || count == 7 || count == 9 || count == 11 || count == 13 || count == 15 || count == 21 || count == 23 || count == 25 || count == 27 || count == 29 || count == 31 || count == 37 || count == 39 || count == 41 || count == 43 || count == 45 || count == 47 || count == 53 || count == 55 || count == 57 || count == 59 || count == 61 || count == 63 || count == 69 || count == 71 || count == 73 || count == 75 || count == 77 || count == 79)
                {
                    count += 1;
                }
                else if (count == 85)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid6X5_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 6 || count == 8 || count == 10 || count == 12 || count == 14 || count == 21 || count == 23 || count == 25 || count == 27 || count == 29 || count == 36 || count == 38 || count == 40 || count == 42 || count == 44 || count == 51 || count == 53 || count == 55 || count == 57 || count == 59 || count == 66 || count == 68 || count == 70 || count == 72 || count == 74)
                {
                    count += 1;
                }
                else if (count == 81)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid6X5_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 6 || count == 8 || count == 10 || count == 12 || count == 14 || count == 21 || count == 23 || count == 25 || count == 27 || count == 29 || count == 36 || count == 38 || count == 40 || count == 42 || count == 44 || count == 51 || count == 53 || count == 55 || count == 57 || count == 59 || count == 66 || count == 68 || count == 70 || count == 72 || count == 74)
                {
                    count += 1;
                }
                else if (count == 81)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid6X6_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 7 || count == 9 || count == 11 || count == 13 || count == 21 || count == 23 || count == 25 || count == 27 || count == 35 || count == 37 || count == 39 || count == 41 || count == 49 || count == 51 || count == 53 || count == 55 || count == 63 || count == 65 || count == 67 || count == 69)
                {
                    count += 1;
                }
                else if (count == 77)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid6X6_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 7 || count == 9 || count == 11 || count == 13 || count == 21 || count == 23 || count == 25 || count == 27 || count == 35 || count == 37 || count == 39 || count == 41 || count == 49 || count == 51 || count == 53 || count == 55 || count == 63 || count == 65 || count == 67 || count == 69)
                {
                    count += 1;
                }
                else if (count == 77)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid6X7_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 8 || count == 10 || count == 12 || count == 21 || count == 23 || count == 25 || count == 34 || count == 36 || count == 38 || count == 47 || count == 49 || count == 51 || count == 60 || count == 62 || count == 64)
                {
                    count += 1;
                }
                else if (count == 73)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid6X7_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 8 || count == 10 || count == 12 || count == 21 || count == 23 || count == 25 || count == 34 || count == 36 || count == 38 || count == 47 || count == 49 || count == 51 || count == 60 || count == 62 || count == 64)
                {
                    count += 1;
                }
                else if (count == 73)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid6X8_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 9 || count == 11 || count == 21 || count == 23 || count == 33 || count == 35 || count == 45 || count == 47 || count == 57 || count == 59)
                {
                    count += 1;
                }
                else if (count == 69)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid6X8_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 9 || count == 11 || count == 21 || count == 23 || count == 33 || count == 35 || count == 45 || count == 47 || count == 57 || count == 59)
                {
                    count += 1;
                }
                else if (count == 69)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid6X9_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 10 || count == 21 || count == 32 || count == 43 || count == 54)
                {
                    count += 1;
                }
                else if (count == 65)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid6X9_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 10 || count == 21 || count == 32 || count == 43 || count == 54)
                {
                    count += 1;
                }
                else if (count == 65)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid6X10_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                g.Background = new SolidColorBrush(Colors.Gray);
                count += 1;
                if (count == 60)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
            }
        }

        private void TableGrid6X10_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                g.Background = new SolidColorBrush(Colors.LightGray);
                count += 1;
                if (count == 60)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
            }
        }

        #endregion

        #region "Row 7"

        private void TableGrid7X1_MouseEnter(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid2X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid3X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid4X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid5X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid6X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid7X1.Background = new SolidColorBrush(Colors.Gray);
        }

        private void TableGrid7X1_MouseLeave(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid2X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid3X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid4X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid5X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid6X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid7X1.Background = new SolidColorBrush(Colors.LightGray);
        }

        private void TableGrid7X2_MouseEnter(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid1X2.Background = new SolidColorBrush(Colors.Gray);
            TableGrid2X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid2X2.Background = new SolidColorBrush(Colors.Gray);
            TableGrid3X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid3X2.Background = new SolidColorBrush(Colors.Gray);
            TableGrid4X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid4X2.Background = new SolidColorBrush(Colors.Gray);
            TableGrid5X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid5X2.Background = new SolidColorBrush(Colors.Gray);
            TableGrid6X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid6X2.Background = new SolidColorBrush(Colors.Gray);
            TableGrid7X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid7X2.Background = new SolidColorBrush(Colors.Gray);
        }

        private void TableGrid7X2_MouseLeave(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid1X2.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid2X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid2X2.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid3X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid3X2.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid4X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid4X2.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid5X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid5X2.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid6X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid6X2.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid7X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid7X2.Background = new SolidColorBrush(Colors.LightGray);
        }

        private void TableGrid7X3_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 4 || count == 6 || count == 8 || count == 10 || count == 12 || count == 14 || count == 16 || count == 21 || count == 23 || count == 25 || count == 27 || count == 29 || count == 31 || count == 33 || count == 38 || count == 40 || count == 42 || count == 44 || count == 46 || count == 48 || count == 50 || count == 55 || count == 57 || count == 59 || count == 61 || count == 63 || count == 65 || count == 67 || count == 72 || count == 74 || count == 76 || count == 78 || count == 80 || count == 82 || count == 84 || count == 89 || count == 91 || count == 93 || count == 95 || count == 97 || count == 99 || count == 101)
                {
                    count += 1;
                }
                else if (count == 106)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid7X3_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 4 || count == 6 || count == 8 || count == 10 || count == 12 || count == 14 || count == 16 || count == 21 || count == 23 || count == 25 || count == 27 || count == 29 || count == 31 || count == 33 || count == 38 || count == 40 || count == 42 || count == 44 || count == 46 || count == 48 || count == 50 || count == 55 || count == 57 || count == 59 || count == 61 || count == 63 || count == 65 || count == 67 || count == 72 || count == 74 || count == 76 || count == 78 || count == 80 || count == 82 || count == 84 || count == 89 || count == 91 || count == 93 || count == 95 || count == 97 || count == 99 || count == 101)
                {
                    count += 1;
                }
                else if (count == 106)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid7X4_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 5 || count == 7 || count == 9 || count == 11 || count == 13 || count == 15 || count == 21 || count == 23 || count == 25 || count == 27 || count == 29 || count == 31 || count == 37 || count == 39 || count == 41 || count == 43 || count == 45 || count == 47 || count == 53 || count == 55 || count == 57 || count == 59 || count == 61 || count == 63 || count == 69 || count == 71 || count == 73 || count == 75 || count == 77 || count == 79 || count == 85 || count == 87 || count == 89 || count == 91 || count == 93 || count == 95)
                {
                    count += 1;
                }
                else if (count == 101)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid7X4_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 5 || count == 7 || count == 9 || count == 11 || count == 13 || count == 15 || count == 21 || count == 23 || count == 25 || count == 27 || count == 29 || count == 31 || count == 37 || count == 39 || count == 41 || count == 43 || count == 45 || count == 47 || count == 53 || count == 55 || count == 57 || count == 59 || count == 61 || count == 63 || count == 69 || count == 71 || count == 73 || count == 75 || count == 77 || count == 79 || count == 85 || count == 87 || count == 89 || count == 91 || count == 93 || count == 95)
                {
                    count += 1;
                }
                else if (count == 101)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid7X5_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 6 || count == 8 || count == 10 || count == 12 || count == 14 || count == 21 || count == 23 || count == 25 || count == 27 || count == 29 || count == 36 || count == 38 || count == 40 || count == 42 || count == 44 || count == 51 || count == 53 || count == 55 || count == 57 || count == 59 || count == 66 || count == 68 || count == 70 || count == 72 || count == 74 || count == 81 || count == 83 || count == 85 || count == 87 || count == 89)
                {
                    count += 1;
                }
                else if (count == 96)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid7X5_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 6 || count == 8 || count == 10 || count == 12 || count == 14 || count == 21 || count == 23 || count == 25 || count == 27 || count == 29 || count == 36 || count == 38 || count == 40 || count == 42 || count == 44 || count == 51 || count == 53 || count == 55 || count == 57 || count == 59 || count == 66 || count == 68 || count == 70 || count == 72 || count == 74 || count == 81 || count == 83 || count == 85 || count == 87 || count == 89)
                {
                    count += 1;
                }
                else if (count == 96)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid7X6_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 7 || count == 9 || count == 11 || count == 13 || count == 21 || count == 23 || count == 25 || count == 27 || count == 35 || count == 37 || count == 39 || count == 41 || count == 49 || count == 51 || count == 53 || count == 55 || count == 63 || count == 65 || count == 67 || count == 69 || count == 77 || count == 79 || count == 81 || count == 83)
                {
                    count += 1;
                }
                else if (count == 91)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid7X6_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 7 || count == 9 || count == 11 || count == 13 || count == 21 || count == 23 || count == 25 || count == 27 || count == 35 || count == 37 || count == 39 || count == 41 || count == 49 || count == 51 || count == 53 || count == 55 || count == 63 || count == 65 || count == 67 || count == 69 || count == 77 || count == 79 || count == 81 || count == 83)
                {
                    count += 1;
                }
                else if (count == 91)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid7X7_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 8 || count == 10 || count == 12 || count == 21 || count == 23 || count == 25 || count == 34 || count == 36 || count == 38 || count == 47 || count == 49 || count == 51 || count == 60 || count == 62 || count == 64 || count == 73 || count == 75 || count == 77)
                {
                    count += 1;
                }
                else if (count == 86)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid7X7_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 8 || count == 10 || count == 12 || count == 21 || count == 23 || count == 25 || count == 34 || count == 36 || count == 38 || count == 47 || count == 49 || count == 51 || count == 60 || count == 62 || count == 64 || count == 73 || count == 75 || count == 77)
                {
                    count += 1;
                }
                else if (count == 86)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid7X8_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 9 || count == 11 || count == 21 || count == 23 || count == 33 || count == 35 || count == 45 || count == 47 || count == 57 || count == 59 || count == 69 || count == 71)
                {
                    count += 1;
                }
                else if (count == 81)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid7X8_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 9 || count == 11 || count == 21 || count == 23 || count == 33 || count == 35 || count == 45 || count == 47 || count == 57 || count == 59 || count == 69 || count == 71)
                {
                    count += 1;
                }
                else if (count == 81)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid7X9_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 10 || count == 21 || count == 32 || count == 43 || count == 54 || count == 65)
                {
                    count += 1;
                }
                else if (count == 76)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid7X9_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 10 || count == 21 || count == 32 || count == 43 || count == 54 || count == 65)
                {
                    count += 1;
                }
                else if (count == 76)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid7X10_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                g.Background = new SolidColorBrush(Colors.Gray);
                count += 1;
                if (count == 70)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
            }
        }

        private void TableGrid7X10_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                g.Background = new SolidColorBrush(Colors.LightGray);
                count += 1;
                if (count == 70)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
            }
        }

        #endregion

        #region "Row 8"

        private void TableGrid8X1_MouseEnter(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid2X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid3X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid4X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid5X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid6X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid7X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid8X1.Background = new SolidColorBrush(Colors.Gray);
        }

        private void TableGrid8X1_MouseLeave(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid2X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid3X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid4X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid5X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid6X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid7X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid8X1.Background = new SolidColorBrush(Colors.LightGray);
        }

        private void TableGrid8X2_MouseEnter(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid1X2.Background = new SolidColorBrush(Colors.Gray);
            TableGrid2X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid2X2.Background = new SolidColorBrush(Colors.Gray);
            TableGrid3X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid3X2.Background = new SolidColorBrush(Colors.Gray);
            TableGrid4X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid4X2.Background = new SolidColorBrush(Colors.Gray);
            TableGrid5X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid5X2.Background = new SolidColorBrush(Colors.Gray);
            TableGrid6X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid6X2.Background = new SolidColorBrush(Colors.Gray);
            TableGrid7X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid7X2.Background = new SolidColorBrush(Colors.Gray);
            TableGrid8X1.Background = new SolidColorBrush(Colors.Gray);
            TableGrid8X2.Background = new SolidColorBrush(Colors.Gray);
        }

        private void TableGrid8X2_MouseLeave(object sender, MouseEventArgs e)
        {
            TableGrid1X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid1X2.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid2X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid2X2.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid3X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid3X2.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid4X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid4X2.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid5X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid5X2.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid6X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid6X2.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid7X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid7X2.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid8X1.Background = new SolidColorBrush(Colors.LightGray);
            TableGrid8X2.Background = new SolidColorBrush(Colors.LightGray);
        }

        private void TableGrid8X3_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 4 || count == 6 || count == 8 || count == 10 || count == 12 || count == 14 || count == 16 || count == 21 || count == 23 || count == 25 || count == 27 || count == 29 || count == 31 || count == 33 || count == 38 || count == 40 || count == 42 || count == 44 || count == 46 || count == 48 || count == 50 || count == 55 || count == 57 || count == 59 || count == 61 || count == 63 || count == 65 || count == 67 || count == 72 || count == 74 || count == 76 || count == 78 || count == 80 || count == 82 || count == 84 || count == 89 || count == 91 || count == 93 || count == 95 || count == 97 || count == 99 || count == 101 || count == 106 || count == 108 || count == 110 || count == 112 || count == 114 || count == 116 || count == 118)
                {
                    count += 1;
                }
                else if (count == 123)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid8X3_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 4 || count == 6 || count == 8 || count == 10 || count == 12 || count == 14 || count == 16 || count == 21 || count == 23 || count == 25 || count == 27 || count == 29 || count == 31 || count == 33 || count == 38 || count == 40 || count == 42 || count == 44 || count == 46 || count == 48 || count == 50 || count == 55 || count == 57 || count == 59 || count == 61 || count == 63 || count == 65 || count == 67 || count == 72 || count == 74 || count == 76 || count == 78 || count == 80 || count == 82 || count == 84 || count == 89 || count == 91 || count == 93 || count == 95 || count == 97 || count == 99 || count == 101 || count == 106 || count == 108 || count == 110 || count == 112 || count == 114 || count == 116 || count == 118)
                {
                    count += 1;
                }
                else if (count == 123)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid8X4_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 5 || count == 7 || count == 9 || count == 11 || count == 13 || count == 15 || count == 21 || count == 23 || count == 25 || count == 27 || count == 29 || count == 31 || count == 37 || count == 39 || count == 41 || count == 43 || count == 45 || count == 47 || count == 53 || count == 55 || count == 57 || count == 59 || count == 61 || count == 63 || count == 69 || count == 71 || count == 73 || count == 75 || count == 77 || count == 79 || count == 85 || count == 87 || count == 89 || count == 91 || count == 93 || count == 95 || count == 101 || count == 103 || count == 105 || count == 107 || count == 109 || count == 111)
                {
                    count += 1;
                }
                else if (count == 117)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid8X4_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 5 || count == 7 || count == 9 || count == 11 || count == 13 || count == 15 || count == 21 || count == 23 || count == 25 || count == 27 || count == 29 || count == 31 || count == 37 || count == 39 || count == 41 || count == 43 || count == 45 || count == 47 || count == 53 || count == 55 || count == 57 || count == 59 || count == 61 || count == 63 || count == 69 || count == 71 || count == 73 || count == 75 || count == 77 || count == 79 || count == 85 || count == 87 || count == 89 || count == 91 || count == 93 || count == 95 || count == 101 || count == 103 || count == 105 || count == 107 || count == 109 || count == 111)
                {
                    count += 1;
                }
                else if (count == 117)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid8X5_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 6 || count == 8 || count == 10 || count == 12 || count == 14 || count == 21 || count == 23 || count == 25 || count == 27 || count == 29 || count == 36 || count == 38 || count == 40 || count == 42 || count == 44 || count == 51 || count == 53 || count == 55 || count == 57 || count == 59 || count == 66 || count == 68 || count == 70 || count == 72 || count == 74 || count == 81 || count == 83 || count == 85 || count == 87 || count == 89 || count == 96 || count == 98 || count == 100 || count == 102 || count == 104)
                {
                    count += 1;
                }
                else if (count == 111)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid8X5_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 6 || count == 8 || count == 10 || count == 12 || count == 14 || count == 21 || count == 23 || count == 25 || count == 27 || count == 29 || count == 36 || count == 38 || count == 40 || count == 42 || count == 44 || count == 51 || count == 53 || count == 55 || count == 57 || count == 59 || count == 66 || count == 68 || count == 70 || count == 72 || count == 74 || count == 81 || count == 83 || count == 85 || count == 87 || count == 89 || count == 96 || count == 98 || count == 100 || count == 102 || count == 104)
                {
                    count += 1;
                }
                else if (count == 111)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid8X6_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 7 || count == 9 || count == 11 || count == 13 || count == 21 || count == 23 || count == 25 || count == 27 || count == 35 || count == 37 || count == 39 || count == 41 || count == 49 || count == 51 || count == 53 || count == 55 || count == 63 || count == 65 || count == 67 || count == 69 || count == 77 || count == 79 || count == 81 || count == 83 || count == 91 || count == 93 || count == 95 || count == 97)
                {
                    count += 1;
                }
                else if (count == 105)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid8X6_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 7 || count == 9 || count == 11 || count == 13 || count == 21 || count == 23 || count == 25 || count == 27 || count == 35 || count == 37 || count == 39 || count == 41 || count == 49 || count == 51 || count == 53 || count == 55 || count == 63 || count == 65 || count == 67 || count == 69 || count == 77 || count == 79 || count == 81 || count == 83 || count == 91 || count == 93 || count == 95 || count == 97)
                {
                    count += 1;
                }
                else if (count == 105)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid8X7_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 8 || count == 10 || count == 12 || count == 21 || count == 23 || count == 25 || count == 34 || count == 36 || count == 38 || count == 47 || count == 49 || count == 51 || count == 60 || count == 62 || count == 64 || count == 73 || count == 75 || count == 77 || count == 86 || count == 88 || count == 90)
                {
                    count += 1;
                }
                else if (count == 99)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid8X7_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 8 || count == 10 || count == 12 || count == 21 || count == 23 || count == 25 || count == 34 || count == 36 || count == 38 || count == 47 || count == 49 || count == 51 || count == 60 || count == 62 || count == 64 || count == 73 || count == 75 || count == 77 || count == 86 || count == 88 || count == 90)
                {
                    count += 1;
                }
                else if (count == 99)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid8X8_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 9 || count == 11 || count == 21 || count == 23 || count == 33 || count == 35 || count == 45 || count == 47 || count == 57 || count == 59 || count == 69 || count == 71 || count == 81 || count == 83)
                {
                    count += 1;
                }
                else if (count == 93)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid8X8_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 9 || count == 11 || count == 21 || count == 23 || count == 33 || count == 35 || count == 45 || count == 47 || count == 57 || count == 59 || count == 69 || count == 71 || count == 81 || count == 83)
                {
                    count += 1;
                }
                else if (count == 93)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid8X9_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 10 || count == 21 || count == 32 || count == 43 || count == 54 || count == 65 || count == 76)
                {
                    count += 1;
                }
                else if (count == 87)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private void TableGrid8X9_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                count += 1;
                if (count == 10 || count == 21 || count == 32 || count == 43 || count == 54 || count == 65 || count == 76)
                {
                    count += 1;
                }
                else if (count == 87)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
                else
                {
                    g.Background = new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        private void TableGrid8X10_MouseEnter(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                g.Background = new SolidColorBrush(Colors.Gray);
                count += 1;
                if (count == 80)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
            }
        }

        private void TableGrid8X10_MouseLeave(object sender, MouseEventArgs e)
        {
            int count = 0;
            foreach (Grid g in TableGrid.Items)
            {
                g.Background = new SolidColorBrush(Colors.LightGray);
                count += 1;
                if (count == 80)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
            }
        }

        #endregion

        #endregion

        #region "Click"

        #region "Row 1"

        private void TableGrid1X1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(1, 1);
            }
        }

        private void TableGrid1X2_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(1, 2);
            }
        }

        private void TableGrid1X3_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(1, 3);
            }
        }

        private void TableGrid1X4_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(1, 4);
            }
        }

        private void TableGrid1X5_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(1, 5);
            }
        }

        private void TableGrid1X6_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(1, 6);
            }
        }

        private void TableGrid1X7_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(1, 7);
            }
        }

        private void TableGrid1X8_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(1, 8);
            }
        }

        private void TableGrid1X9_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(1, 9);
            }
        }

        private void TableGrid1X10_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(1, 10);
            }
        }

        #endregion

        #region "Row 2"

        private void TableGrid2X1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(2, 1);
            }
        }

        private void TableGrid2X2_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(2, 2);
            }
        }

        private void TableGrid2X3_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(2, 3);
            }
        }

        private void TableGrid2X4_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(2, 4);
            }
        }

        private void TableGrid2X5_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(2, 5);
            }
        }

        private void TableGrid2X6_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(2, 6);
            }
        }

        private void TableGrid2X7_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(2, 7);
            }
        }

        private void TableGrid2X8_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(2, 8);
            }
        }

        private void TableGrid2X9_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(2, 9);
            }
        }

        private void TableGrid2X10_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(2, 10);
            }
        }

        #endregion

        #region "Row 3"

        private void TableGrid3X1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(3, 1);
            }
        }

        private void TableGrid3X2_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(3, 2);
            }
        }

        private void TableGrid3X3_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(3, 3);
            }
        }

        private void TableGrid3X4_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(3, 4);
            }
        }

        private void TableGrid3X5_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(3, 5);
            }
        }

        private void TableGrid3X6_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(3, 6);
            }
        }

        private void TableGrid3X7_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(3, 7);
            }
        }

        private void TableGrid3X8_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(3, 8);
            }
        }

        private void TableGrid3X9_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(3, 9);
            }
        }

        private void TableGrid3X10_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(3, 10);
            }
        }

        #endregion

        #region "Row 4"

        private void TableGrid4X1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(4, 1);
            }
        }

        private void TableGrid4X2_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(4, 2);
            }
        }

        private void TableGrid4X3_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(4, 3);
            }
        }

        private void TableGrid4X4_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(4, 4);
            }
        }

        private void TableGrid4X5_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(4, 5);
            }
        }

        private void TableGrid4X6_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(4, 6);
            }
        }

        private void TableGrid4X7_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(4, 7);
            }
        }

        private void TableGrid4X8_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(4, 8);
            }
        }

        private void TableGrid4X9_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(4, 9);
            }
        }

        private void TableGrid4X10_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(4, 10);
            }
        }

        #endregion

        #region "Row 5"

        private void TableGrid5X1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(5, 1);
            }
        }

        private void TableGrid5X2_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(5, 2);
            }
        }

        private void TableGrid5X3_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(5, 3);
            }
        }

        private void TableGrid5X4_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(5, 4);
            }
        }

        private void TableGrid5X5_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(5, 5);
            }
        }

        private void TableGrid5X6_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(5, 6);
            }
        }

        private void TableGrid5X7_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(5, 7);
            }
        }

        private void TableGrid5X8_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(5, 8);
            }
        }

        private void TableGrid5X9_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(5, 9);
            }
        }

        private void TableGrid5X10_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(5, 10);
            }
        }

        #endregion

        #region "Row 6"

        private void TableGrid6X1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(6, 1);
            }
        }

        private void TableGrid6X2_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(6, 2);
            }
        }

        private void TableGrid6X3_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(6, 3);
            }
        }

        private void TableGrid6X4_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(6, 4);
            }
        }

        private void TableGrid6X5_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(6, 5);
            }
        }

        private void TableGrid6X6_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(6, 6);
            }
        }

        private void TableGrid6X7_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(6, 7);
            }
        }

        private void TableGrid6X8_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(6, 8);
            }
        }

        private void TableGrid6X9_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(6, 9);
            }
        }

        private void TableGrid6X10_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(6, 10);
            }
        }

        #endregion

        #region "Row 7"

        private void TableGrid7X1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(7, 1);
            }
        }

        private void TableGrid7X2_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(7, 2);
            }
        }

        private void TableGrid7X3_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(7, 3);
            }
        }

        private void TableGrid7X4_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(7, 4);
            }
        }

        private void TableGrid7X5_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(7, 5);
            }
        }

        private void TableGrid7X6_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(7, 6);
            }
        }

        private void TableGrid7X7_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(7, 7);
            }
        }

        private void TableGrid7X8_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(7, 8);
            }
        }

        private void TableGrid7X9_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(7, 9);
            }
        }

        private void TableGrid7X10_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(7, 10);
            }
        }

        #endregion

        #region "Row 8"

        private void TableGrid8X1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(8, 1);
            }
        }

        private void TableGrid8X2_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(8, 2);
            }
        }

        private void TableGrid8X3_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(8, 3);
            }
        }

        private void TableGrid8X4_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(8, 4);
            }
        }

        private void TableGrid8X5_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(8, 5);
            }
        }

        private void TableGrid8X6_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(8, 6);
            }
        }

        private void TableGrid8X7_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(8, 7);
            }
        }

        private void TableGrid8X8_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(8, 8);
            }
        }

        private void TableGrid8X9_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(8, 9);
            }
        }

        private void TableGrid8X10_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Click != null)
            {
                Click(8, 10);
            }
        }

        #endregion

        #endregion

    }
}
