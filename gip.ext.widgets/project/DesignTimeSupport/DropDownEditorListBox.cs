﻿// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace gip.ext.widgets.DesignTimeSupport
{
	public class DropDownEditorListBox : ListBox
	{
		IWindowsFormsEditorService editorService;
		string dropDownValue = String.Empty;
		IEnumerable<string> dropDownItems;
		
		public DropDownEditorListBox(IWindowsFormsEditorService editorService, IEnumerable<string> dropDownItems)
		{
			if (editorService == null)
				throw new ArgumentNullException("editorService");
			if (dropDownItems == null)
				throw new ArgumentNullException("dropDownItems");
			
			this.editorService = editorService;
			this.dropDownItems = dropDownItems;
			
			BorderStyle = BorderStyle.None;
			
			AddDropDownItems();
		}
		
		public string Value {
			get {
				return dropDownValue;
			}
			set {
				dropDownValue = value;
				SelectListItem(dropDownValue);
			}
		}
		
		protected override void OnMouseClick(MouseEventArgs e)
		{
			base.OnMouseClick(e);
			int index = IndexFromPoint(e.Location);
			if (index != -1) {
				dropDownValue = (string)SelectedItem;
				editorService.CloseDropDown();
			}
		}
		
		protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
		{
			base.OnPreviewKeyDown(e);
			if (e.KeyData == Keys.Return) {
				if (SelectedIndex != -1) {
					dropDownValue = (string)SelectedItem;
				}
				editorService.CloseDropDown();
			}
		}
		
		void AddDropDownItems()
		{
			foreach (string item in dropDownItems) {
				Items.Add(item);
			}
		}
		
		void SelectListItem(string item)
		{
			int index = Items.IndexOf(item);
			SelectedIndex = index;
		}
	}
}
