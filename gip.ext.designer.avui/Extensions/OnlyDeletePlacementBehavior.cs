// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System.Collections.Generic;
using gip.ext.design.avui.Extensions;
using System.Windows.Controls;
using System.Windows;
using gip.ext.designer.avui.Controls;
using gip.ext.design.avui;

namespace gip.ext.designer.avui.Extensions
{
	[ExtensionFor(typeof(object))]
	public class OnlyDeletePlacementBehavior : BehaviorExtension, IPlacementBehavior
	{
		static OnlyDeletePlacementBehavior()
		{ }

		protected override void OnInitialized()
		{
			base.OnInitialized();
			if (ExtendedItem.Component is Panel || ExtendedItem.Component is Control || ExtendedItem.Component is Border || ExtendedItem.Component is Viewbox || ExtendedItem.Component is TextBlock)
				return;

			ExtendedItem.AddBehavior(typeof(IPlacementBehavior), this);
		}

		public virtual bool CanPlace(IEnumerable<DesignItem> childItems, PlacementType type, PlacementAlignment position)
		{
			return type == PlacementType.Delete;
		}

		public virtual void BeginPlacement(PlacementOperation operation)
		{
		}

		public virtual void EndPlacement(PlacementOperation operation)
		{
		}

		public virtual Rect GetPosition(PlacementOperation operation, DesignItem item)
		{
			return new Rect();
		}

		public Rect GetPositionRelativeToContainer(PlacementOperation operation, DesignItem item)
		{
			return new Rect();
		}

		public virtual void BeforeSetPosition(PlacementOperation operation)
		{
		}

		public virtual bool CanPlaceItem(PlacementInformation info)
		{
			return false;
		}

		public virtual void SetPosition(PlacementInformation info)
		{
		}

		public virtual bool CanLeaveContainer(PlacementOperation operation)
		{
			return true;
		}

		public virtual void LeaveContainer(PlacementOperation operation)
		{
			foreach (var item in operation.PlacedItems) {
				if (item.Item.ParentProperty.IsCollection) {
					item.Item.ParentProperty.CollectionElements.Remove(item.Item);
				}
				else {
					item.Item.ParentProperty.Reset();
				}
			}
		}

		private static InfoTextEnterArea infoTextEnterArea;

		public virtual bool CanEnterContainer(PlacementOperation operation, bool shouldAlwaysEnter)
		{
			return false;
		}

		public virtual void EnterContainer(PlacementOperation operation)
		{
		}

		public virtual Point PlacePoint(Point point)
		{
			return new Point();
		}
	}
}
