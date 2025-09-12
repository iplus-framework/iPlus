// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using gip.ext.design.avui.Adorners;

namespace gip.ext.design.avui.Extensions
{
	/// <summary>
	/// Applies an extension to the hovered components.
	/// </summary>
	public class MouseOverExtensionServer : DefaultExtensionServer
	{
		private DesignItem _lastItem = null;

		/// <summary>
		/// Is called after the extension server is initialized and the Context property has been set.
		/// </summary>
		protected override void OnInitialized()
		{
			base.OnInitialized();
			var panel = this.Services.GetService<IDesignPanel>() as Control;
			if (panel != null)
			{
                ((Control)this.Services.DesignPanel).PointerMoved += MouseOverExtensionServer_PointerMoved;
                ((Control)this.Services.DesignPanel).PointerExited += MouseOverExtensionServer_PointerExited;
				Services.Selection.SelectionChanged += OnSelectionChanged;
			}
		}

        void OnSelectionChanged(object sender, DesignItemCollectionEventArgs e)
		{
			ReapplyExtensions(e.Items);
		}

		private void MouseOverExtensionServer_PointerExited(object sender, PointerEventArgs e)
        {
			if (_lastItem != null)
			{
				var oldLastItem = _lastItem;
				_lastItem = null;
				ReapplyExtensions(new[] { oldLastItem });
			}
		}

		private void MouseOverExtensionServer_PointerMoved(object sender, PointerEventArgs e)
		{
			DesignItem element = null;
			var designPanel = this.Services.DesignPanel as Control;
			if (designPanel == null) return;

			var position = e.GetPosition(designPanel);
			var hitTest = designPanel.InputHitTest(position);
			
			if (hitTest is Visual visual)
			{
				// Walk up the visual tree to find the appropriate design item
				var current = visual;
				while (current != null)
				{
					// Skip adorner layers
					if (current is IAdornerLayer)
					{
						current = current.GetVisualParent();
						continue;
					}

					if (Extension.GetDisableMouseOverExtensions(current))
					{
                        current = current.GetVisualParent();
                        continue;
                    }

                    var item = this.Services.Component.GetDesignItem(current);
					if (item != null)
					{
						if (element == null || item.Parent == element)
						{
							element = item;
							break;
						}

						var par = item.Parent;
						while (par != null)
						{
							if (par.Parent == element)
							{
								element = item;
								break;
							}
							par = par.Parent;
						}
						
						if (element == item)
							break;
					}

					current = current.GetVisualParent();
				}
			}

			var oldLastItem = _lastItem;
			_lastItem = element;
			if (oldLastItem != null && oldLastItem != element)
				ReapplyExtensions(new[] { oldLastItem, element });
			else
			{
				ReapplyExtensions(new[] { element });
			}
		}

		/// <summary>
		/// Gets if the item is selected.
		/// </summary>
		public override bool ShouldApplyExtensions(DesignItem extendedItem)
		{
			return extendedItem == _lastItem && !Services.Selection.IsComponentSelected(extendedItem);
		}
	}
}
