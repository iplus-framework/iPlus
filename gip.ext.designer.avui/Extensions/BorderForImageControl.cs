// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using gip.ext.design.avui.Adorners;
using gip.ext.design.avui.Extensions;

namespace gip.ext.designer.avui.Extensions
{
	[ExtensionFor(typeof(Image))]
	public class BorderForImageControl : PermanentAdornerProvider
	{
		AdornerPanel adornerPanel;
		AdornerPanel cachedAdornerPanel;
		Border border;
		
		protected override void OnInitialized()
		{
			base.OnInitialized();

			this.ExtendedItem.PropertyChanged += OnPropertyChanged;

			UpdateAdorner();
		}

		protected override void OnRemove()
		{
			this.ExtendedItem.PropertyChanged -= OnPropertyChanged;
			base.OnRemove();
		}

		void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (sender == null || e.PropertyName == "Width" || e.PropertyName == "Height")
			{
				((DesignPanel) this.ExtendedItem.Services.DesignPanel).AdornerLayer.UpdateAdornersForElement(this.ExtendedItem.View, true);
			}
		}

		void UpdateAdorner()
		{
			var element = ExtendedItem.Component as UIElement;
			if (element != null) {
				CreateAdorner();
			}
		}

		private void CreateAdorner()
		{
			if (adornerPanel == null) {
				
				if (cachedAdornerPanel == null) {
					cachedAdornerPanel = new AdornerPanel();
					cachedAdornerPanel.Order = AdornerOrder.Background;
					border = new Border();
					border.BorderThickness = new Thickness(1);
					border.BorderBrush = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC));
					border.Background = Brushes.Transparent;
					border.IsHitTestVisible = true;
					border.MouseDown += border_MouseDown;
					border.MinWidth = 1;
					border.MinHeight = 1;

					AdornerPanel.SetPlacement(border, AdornerPlacement.FillContent);
					cachedAdornerPanel.Children.Add(border);
				}
				
				adornerPanel = cachedAdornerPanel;
				Adorners.Add(adornerPanel);
			}
		}

		void border_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (!Keyboard.IsKeyDown(Key.LeftAlt) && ((Image) this.ExtendedItem.View).Source == null)
			{
				e.Handled = true;
				this.ExtendedItem.Services.Selection.SetSelectedComponents(new DesignItem[] {this.ExtendedItem},
				                                                           SelectionTypes.Auto);
				((DesignPanel) this.ExtendedItem.Services.DesignPanel).Focus();
			}
		}
	}
}
