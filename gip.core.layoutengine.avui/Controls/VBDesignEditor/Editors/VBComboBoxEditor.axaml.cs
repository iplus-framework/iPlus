// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using gip.ext.design.avui.PropertyGrid;
using System;

namespace gip.core.layoutengine.avui.PropertyGrid.Editors
{
    /// <summary>
    /// Represents a editor for combobox.
    /// </summary>
	[TypeEditor(typeof(Enum))]
	public partial class VBComboBoxEditor : VBComboBox
    {
        // Avalonia resolves the implicit ControlTheme only by the control's own StyleKey
        // (no base-type walk) - style as the base type so the VBComboBox theme is found.
        protected override Type StyleKeyOverride => typeof(VBComboBox);
		/// <summary>
		/// Create a new ComboBoxEditor instance.
		/// </summary>
		public VBComboBoxEditor()
		{
			InitializeComponent();
		}


        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            var popup = this.FindControl<Popup>("PART_Popup");
            if (popup != null)
                popup.SetValue(FontWeightProperty, FontWeight.Normal); 
        }
	}
}
