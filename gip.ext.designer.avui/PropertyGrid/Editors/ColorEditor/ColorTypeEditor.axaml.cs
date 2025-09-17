// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Windows;
using System.Windows.Media;
using gip.ext.designer.avui.PropertyGrid;
using gip.ext.designer.avui.themes;
using System.Windows.Input;
using System.ComponentModel;
using gip.ext.designer.avui.PropertyGrid.Editors.BrushEditor;
using System.Linq;
using gip.ext.design.avui.PropertyGrid;

namespace gip.ext.designer.avui.PropertyGrid.Editors.ColorEditor
{
	[TypeEditor(typeof(Color))]
	public partial class ColorTypeEditor
	{
		public ColorTypeEditor()
		{
			SpecialInitializeComponent();
		}

		/// <summary>
		/// Fixes InitializeComponent with multiple Versions of same Assembly loaded
		/// </summary>
		public void SpecialInitializeComponent()
		{
			if (!this._contentLoaded)
			{
				this._contentLoaded = true;
				Uri resourceLocator = new Uri(VersionedAssemblyResourceDictionary.GetXamlNameForType(this.GetType()), UriKind.Relative);
				Application.LoadComponent(this, resourceLocator);
			}

			this.InitializeComponent();
		}

		private ChangeGroup _changeGroup = null;

		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			var pnode = this.DataContext as PropertyNode;
			var colorEditorPopup = new ColorEditorPopup();
			colorEditorPopup.PlacementTarget = this;
			colorEditorPopup.IsOpen = true;
			colorEditorPopup.solidBrushEditor.Color = (Color)pnode.DesignerValue;
			colorEditorPopup.Closed += ColorEditorPopup_Closed;
			DependencyPropertyDescriptor.FromProperty(SolidBrushEditor.ColorProperty, typeof(SolidBrushEditor))
				.AddValueChanged(colorEditorPopup.solidBrushEditor, 
				(s, ee) => {
					if (_changeGroup == null) {
						_changeGroup = pnode.Context.OpenGroup("change color",
											   pnode.Properties.Select(p => p.DesignItem).ToArray());

					}
					pnode.DesignerValue = colorEditorPopup.solidBrushEditor.Color;
				});
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