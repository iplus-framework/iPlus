// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace gip.ext.design.avui
{
	public interface IOutlineNode
	{
		ISelectionService SelectionService { get; }
		bool IsExpanded { get; set; }
		DesignItem DesignItem { get; set; }
		ServiceContainer Services { get; }
		bool IsSelected { get; set; }
		bool IsDesignTimeVisible { get; set; }
		bool IsDesignTimeLocked { get; }
		string Name { get; }
		bool CanInsert(IEnumerable<IOutlineNode> nodes, IOutlineNode after, bool copy);
		void Insert(IEnumerable<IOutlineNode> nodes, IOutlineNode after, bool copy);
		ObservableCollection<IOutlineNode> Children { get; }
	}
}
