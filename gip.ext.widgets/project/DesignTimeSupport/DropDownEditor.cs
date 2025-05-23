﻿// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace gip.ext.widgets.DesignTimeSupport
{
	public abstract class DropDownEditor : UITypeEditor
	{
		/// <summary>
		/// Returns the drop down style.
		/// </summary>
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.DropDown;
		}
		
		public override bool IsDropDownResizable {
			get {
				return false;
			}
		}
		
		/// <summary>
		/// Shows the drop down editor control in the drop down so the user
		/// can change the value.
		/// </summary>
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			IWindowsFormsEditorService editorService = null;
			
			if (provider != null) {
				editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
			}
			
			if (editorService != null) {
				using (Control control = CreateDropDownControl(context, editorService)) {
					SetValue(control, value);
					editorService.DropDownControl(control);
					value = GetValue(control);
				}
			}
			
			return value;
		}
		
		/// <summary>
		/// Creates the drop down control.
		/// </summary>
		protected abstract Control CreateDropDownControl(ITypeDescriptorContext context, IWindowsFormsEditorService editorService);
		
		/// <summary>
		/// Sets the current value in the drop down control.
		/// </summary>
		protected virtual void SetValue(Control control, object value)
		{
			DropDownEditorListBox listBox = (DropDownEditorListBox)control;
			listBox.Value = (string)value;
		}
		
		/// <summary>
		/// Gets the current value from the drop down control.
		/// </summary>
		protected virtual object GetValue(Control control)
		{
			DropDownEditorListBox listBox = (DropDownEditorListBox)control;
			return listBox.Value;
		}
	}
}
