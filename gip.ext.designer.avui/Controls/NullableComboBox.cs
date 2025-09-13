// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace gip.ext.designer.avui.Controls
{
	/// <summary>
	/// A ComboBox wich is Nullable
	/// </summary>
	public class NullableComboBox : ComboBox
	{
		static NullableComboBox()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(NullableComboBox), new FrameworkPropertyMetadata(typeof(NullableComboBox)));
		}
		
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			var btn = GetTemplateChild("PART_ClearButton") as Button;

			btn.Click += btn_Click;
		}

		void btn_Click(object sender, RoutedEventArgs e)
		{
			var clearButton = (Button)sender;
			var parent = VisualTreeHelper.GetParent(clearButton);

			while (!(parent is ComboBox))
			{
				parent = VisualTreeHelper.GetParent(parent);
			}

			var comboBox = (ComboBox)parent;
			comboBox.SelectedIndex = -1;
		}

		public bool IsNullable
		{
			get { return (bool)GetValue(IsNullableProperty); }
			set { SetValue(IsNullableProperty, value); }
		}

		public static readonly DependencyProperty IsNullableProperty =
			DependencyProperty.Register("IsNullable", typeof(bool), typeof(NullableComboBox), new PropertyMetadata(true));
	}
}
