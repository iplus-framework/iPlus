// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using gip.ext.design.avui;

namespace gip.ext.designer.avui.OutlineView
{ 
	public class PropertyOutlineNode : OutlineNode
	{
		private DesignItemProperty _property;

		protected PropertyOutlineNode(DesignItemProperty property) : base(property.Name)
		{
			_property = property;
		}

		public override ServiceContainer Services
		{
			get { return this._property.DesignItem.Services; }
		}

		static PropertyOutlineNode()
		{
			DummyPlacementType = PlacementType.Register("DummyPlacement");
		}

		public static IOutlineNode Create(DesignItemProperty property)
		{
			return new PropertyOutlineNode(property);
		}
	}
}
