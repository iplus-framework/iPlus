// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Globalization;

namespace gip.ext.widgets.avui.ListViewSorting
{
	/// <summary>
	/// Compares ListViewItems by the signed integer content of a specific column.
	/// </summary>
	public sealed class ListViewIntegerParseColumnComparer
		: AbstractListViewParseableColumnComparer<Int64>
	{
		protected override bool TryParse(string textValue, out Int64 parsedValue)
		{
			return Int64.TryParse(textValue, NumberStyles.Any, CultureInfo.InvariantCulture, out parsedValue);
		}
	}
}
