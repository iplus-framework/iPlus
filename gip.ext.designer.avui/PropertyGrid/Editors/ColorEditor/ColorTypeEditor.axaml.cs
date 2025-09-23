// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Reactive.Linq;
using gip.ext.designer.avui.PropertyGrid;
using gip.ext.designer.avui.themes;
using System.ComponentModel;
using gip.ext.designer.avui.PropertyGrid.Editors.BrushEditor;
using System.Linq;
using gip.ext.design.avui.PropertyGrid;
using gip.ext.design.avui;
using Avalonia.Media;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using System.Threading.Tasks;
using Avalonia;

namespace gip.ext.designer.avui.PropertyGrid.Editors.ColorEditor
{
	[TypeEditor(typeof(Color))]
	public partial class ColorTypeEditor : UserControl
	{
		private ChangeGroup _changeGroup = null;

		public ColorTypeEditor()
		{
            this.InitializeComponent();
        }

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		protected override void OnPointerReleased(PointerReleasedEventArgs e)
		{
			ShowColorEditor();
			base.OnPointerReleased(e);
		}

		private async void ShowColorEditor()
		{
			var pnode = this.DataContext as PropertyNode;
			if (pnode == null) return;
			
			var colorEditorPopup = new ColorEditorPopup();
			
			// Set initial color value
			colorEditorPopup.solidBrushEditor.Color = (Color)pnode.DesignerValue;
			
			// Subscribe to color changes using Avalonia property change subscription
			var colorObservable = colorEditorPopup.solidBrushEditor.GetObservable(SolidBrushEditor.ColorProperty);
			var subscription = colorObservable.Subscribe(newColor => {
				if (_changeGroup == null) {
					_changeGroup = pnode.Context.OpenGroup("change color",
										   pnode.Properties.Select(p => p.DesignItem).ToArray());
				}
				pnode.DesignerValue = newColor;
			});
			
			// Subscribe to closed event
			colorEditorPopup.Closed += (s, e) => {
				ColorEditorPopup_Closed(s, e);
				subscription?.Dispose();
			};
			
			// Find parent window and show dialog
			var parentWindow = this.FindAncestorOfType<Window>();
			if (parentWindow != null)
			{
				await colorEditorPopup.ShowDialog(parentWindow);
			}
			else
			{
				colorEditorPopup.Show();
			}
		}

		private void ColorEditorPopup_Closed(object sender, EventArgs e)
		{
			if (_changeGroup != null) {
				_changeGroup.Commit();
				_changeGroup = null;
			}
		}
	}
}