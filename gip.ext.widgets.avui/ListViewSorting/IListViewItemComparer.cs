// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Windows.Forms;

namespace gip.ext.widgets.avui.ListViewSorting
{
	/// <summary>
	/// Provides the ability to compare two ListViewItems with regard to
	/// a specific column.
	/// </summary>
	public interface IListViewItemComparer
	{
		/// <summary>
		/// Compares two ListViewItems with regard to the specified column.
		/// </summary>
		/// <returns>
		/// Less than zero if <paramref name="lhs"/> is less than <paramref name="rhs"/>.
		/// Zero if <paramref name="lhs"/> is equal to <paramref name="rhs"/>.
		/// Greater than zero if <paramref name="lhs"/> is greater than <paramref name="rhs"/>.
		/// </returns>
		/// <remarks>
		/// The implementation must always compare the specified items in ascending order.
		/// </remarks>
		int Compare(ListViewItem lhs, ListViewItem rhs, int column);
	}
}
