// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Markup.Xaml;
using gip.ext.design.avui.PropertyGrid;


namespace gip.ext.designer.avui.PropertyGrid.Editors
{
	[TypeEditor(typeof(Enum))]
	public partial class ComboBoxEditor : UserControl
	{
		private ComboBox _comboBox;

		/// <summary>
		/// Create a new ComboBoxEditor instance.
		/// </summary>
		public ComboBoxEditor()
		{
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
			_comboBox = this.FindControl<ComboBox>("PART_ComboBox");
		}

		/// <summary>
		/// Gets the inner ComboBox control.
		/// </summary>
		public ComboBox ComboBox => _comboBox;

		/// <inheritdoc/>
		protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
		{
			base.OnApplyTemplate(e);
			
			// The ComboBox will handle its own template application
			// We don't need to set FontWeight on popup here as it's typically handled by styles
		}
	}
}
