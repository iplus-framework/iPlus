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
using System.Windows.Shapes;
using System.Diagnostics;
using System.Reflection;
using gip.ext.chart.Common.Auxiliary;

namespace gip.ext.chart.Charts.Navigation
{
	/// <summary>
	/// Interaction logic for AboutWindow.xaml
	/// </summary>
	internal partial class AboutWindow : Window
	{
		public AboutWindow()
		{
			InitializeComponent();

            Title = gip.ext.chart.Properties.Resources.AboutTitle;

            r1.Text = gip.ext.chart.Properties.Resources.About_1;
            r2.Text = gip.ext.chart.Properties.Resources.About_2;
            r3.Text = gip.ext.chart.Properties.Resources.About_3;
            r4.Text = gip.ext.chart.Properties.Resources.About_4;
            r5.Text = gip.ext.chart.Properties.Resources.About_5;
            r6.Text = gip.ext.chart.Properties.Resources.About_6;
            r7.Text = gip.ext.chart.Properties.Resources.About_7;
            r8.Text = gip.ext.chart.Properties.Resources.About_8;
            r9.Text = gip.ext.chart.Properties.Resources.About_2;
            r10.Text = gip.ext.chart.Properties.Resources.About_9;
            r11.Text = gip.ext.chart.Properties.Resources.About_10;
            r12.Text = gip.ext.chart.Properties.Resources.About_10;
            r13.Text = gip.ext.chart.Properties.Resources.About_11;
            r14.Text = gip.ext.chart.Properties.Resources.About_12;
            r15.Text = gip.ext.chart.Properties.Resources.About_13;
            r16.Text = gip.ext.chart.Properties.Resources.About_14;
            r17.Text = gip.ext.chart.Properties.Resources.About_15;
		}

		private void Hyperlink_Click(object sender, RoutedEventArgs e)
		{
			Hyperlink source = (Hyperlink)sender;
			Process.Start(source.NavigateUri.ToString());
		}

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			// close on Esc or Enter pressed
			if (e.Key == Key.Escape || e.Key == Key.Enter)
			{
				Close();
			}
		}

		private void Hyperlink_Click_1(object sender, RoutedEventArgs e)
		{
			Hyperlink source = (Hyperlink)sender;
			Process.Start(source.NavigateUri.ToString());
		}
	}
}
