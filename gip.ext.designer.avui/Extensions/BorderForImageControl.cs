// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using gip.ext.design.avui;
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
			var element = ExtendedItem.Component as Control;
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
					border.PointerPressed += border_PointerPressed;
					border.MinWidth = 1;
					border.MinHeight = 1;

					AdornerPanel.SetPlacement(border, AdornerPlacement.FillContent);
					cachedAdornerPanel.Children.Add(border);
				}
				
				adornerPanel = cachedAdornerPanel;
				Adorners.Add(adornerPanel);
			}
		}

		void border_PointerPressed(object sender, PointerPressedEventArgs e)
		{
			if (!e.KeyModifiers.HasFlag(KeyModifiers.Alt) && ((Image) this.ExtendedItem.View).Source == null)
			{
				e.Handled = true;
				this.ExtendedItem.Services.Selection.SetSelectedComponents(new gip.ext.design.avui.DesignItem[] {this.ExtendedItem},
				                                                           gip.ext.design.avui.SelectionTypes.Auto);
				((DesignPanel) this.ExtendedItem.Services.DesignPanel).Focus();
			}
		}
	}
}
