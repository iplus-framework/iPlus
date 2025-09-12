// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System.Collections.Generic;

namespace gip.ext.design.avui
{
	public interface ISelectionFilterService
	{
		ICollection<DesignItem> FilterSelectedElements(ICollection<DesignItem> items);
	}
}
