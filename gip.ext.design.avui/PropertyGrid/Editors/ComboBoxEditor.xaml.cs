// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

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
using gip.ext.design.avui.PropertyGrid;
using System.Windows.Controls.Primitives;

namespace gip.ext.design.avui.PropertyGrid.Editors
{
	[TypeEditor(typeof(Enum))]
	public partial class ComboBoxEditor
	{
		/// <summary>
		/// Create a new ComboBoxEditor instance.
		/// </summary>
		public ComboBoxEditor()
		{
			InitializeComponent();
		}

		/// <inheritdoc/>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			var popup = (Popup)Template.FindName("PART_Popup", this);
			popup.SetValue(FontWeightProperty, FontWeights.Normal);
		}
	}
}
