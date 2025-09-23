// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Reflection;

namespace gip.ext.designer.avui.Services
{
	public partial class ChooseClassDialog : Window
	{
		public ChooseClassDialog() : base()
		{
		}

        public ChooseClassDialog(ChooseClass core)
		{
			DataContext = core;
			InitializeComponent();
			
			var uxFilter = this.FindControl<TextBox>("uxFilter");
			var uxList = this.FindControl<ClassListBox>("uxList");
			var uxOk = this.FindControl<Button>("uxOk");
			
			uxFilter?.Focus();
			if (uxList != null)
				uxList.DoubleTapped += uxList_DoubleTapped;
			if (uxOk != null)
				uxOk.Click += delegate { Ok(); };
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
		
		protected override void OnKeyDown(KeyEventArgs e)
		{
			var uxList = this.FindControl<ClassListBox>("uxList");
			if (uxList == null) return;

			if (e.Key == Key.Enter) {
				Ok();
				e.Handled = true;
			} else if (e.Key == Key.Up) {
				if (uxList.SelectedIndex > 0)
					uxList.SelectedIndex = uxList.SelectedIndex - 1;
				e.Handled = true;
			} else if (e.Key == Key.Down) {
				if (uxList.SelectedIndex < uxList.ItemCount - 1)
					uxList.SelectedIndex++;
				e.Handled = true;
			}
			base.OnKeyDown(e);
		}
		
		void uxList_DoubleTapped(object sender, TappedEventArgs e)
		{
			if (e.Source is Control f && f.DataContext is Type) {
				Ok();
			}
		}
		
		void Ok()
		{
			Close(true);
		}
	}
	
	class ClassListBox : ListBox
	{
		public ClassListBox()
		{
			SelectionChanged += OnSelectionChangedInternal;
		}

		protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
		{
			base.OnPropertyChanged(change);
			
			if (change.Property == ItemsSourceProperty && change.NewValue != null)
			{
				SelectedIndex = 0;
				ScrollIntoView(SelectedIndex);
			}
		}
		
		private void OnSelectionChangedInternal(object sender, SelectionChangedEventArgs e)
		{
			if (SelectedIndex >= 0)
				ScrollIntoView(SelectedIndex);
		}
	}
	
	public class ClassNameConverter : IValueConverter
	{
		public static ClassNameConverter Instance = new ClassNameConverter();
		
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var c = value as Type;
			if (c == null) return value;
			return c.Name + " (" + c.Namespace + ")";
		}
		
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
	
	public class NullToBoolConverter : IValueConverter
	{
		public static NullToBoolConverter Instance = new NullToBoolConverter();
		
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value != null;
		}
		
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
