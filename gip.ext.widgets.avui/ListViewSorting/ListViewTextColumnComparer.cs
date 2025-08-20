// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace gip.ext.widgets.avui.ListViewSorting
{
	/// <summary>
	/// Compares ListViewItems by the text of a specific column.
	/// </summary>
	public class ListViewTextColumnComparer : AbstractListViewSubItemComparer
	{
		readonly IComparer<string> stringComparer;
		
		/// <summary>
		/// Creates a new instance of the <see cref="ListViewTextColumnComparer"/> class
		/// with a culture-independent and case-sensitive string comparer.
		/// </summary>
		public ListViewTextColumnComparer()
			: this(StringComparer.InvariantCulture)
		{
		}
		
		/// <summary>
		/// Creates a new instance of the <see cref="ListViewTextColumnComparer"/> class
		/// with the specified string comparer.
		/// </summary>
		/// <param name="stringComparer">The string comparer used to compare the item texts.</param>
		public ListViewTextColumnComparer(IComparer<string> stringComparer)
		{
			if (stringComparer == null) {
				throw new ArgumentNullException("stringComparer");
			}
			this.stringComparer = stringComparer;
		}
		
		protected override int Compare(ListViewItem.ListViewSubItem lhs, ListViewItem.ListViewSubItem rhs)
		{
			return this.stringComparer.Compare(lhs.Text, rhs.Text);
		}
	}
}
