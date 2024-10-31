// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.ext.design.Extensions;
using System.Windows.Controls;
using System.Windows;
using gip.ext.designer.Controls;
using System.Diagnostics;
using gip.ext.xamldom;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using gip.ext.design;

namespace gip.ext.designer.Extensions
{
	[ExtensionFor(typeof(Panel))]
	[ExtensionFor(typeof(ContentControl))]
    [CLSCompliant(false)]
    public class DefaultPlacementBehavior : BehaviorExtension, IPlacementBehavior
    {
        protected static List<Type> _contentControlsNotAllowedToAdd;

		static DefaultPlacementBehavior()
		{
			_contentControlsNotAllowedToAdd = new List<Type>();
			_contentControlsNotAllowedToAdd.Add(typeof (Frame));
			_contentControlsNotAllowedToAdd.Add(typeof (GroupItem));
			_contentControlsNotAllowedToAdd.Add(typeof (HeaderedContentControl));
			_contentControlsNotAllowedToAdd.Add(typeof (Label));
			_contentControlsNotAllowedToAdd.Add(typeof (ListBoxItem));
			_contentControlsNotAllowedToAdd.Add(typeof (ButtonBase));
			_contentControlsNotAllowedToAdd.Add(typeof (StatusBarItem));
            _contentControlsNotAllowedToAdd.Add(typeof (ToolTip));
		}

		public static bool CanContentControlAdd(ContentControl control)
		{
			Debug.Assert(control != null);
			return !_contentControlsNotAllowedToAdd.Any(type => type.IsAssignableFrom(control.GetType()));
		}
		
		protected override void OnInitialized()
		{
			base.OnInitialized();
			if (ExtendedItem.ContentProperty == null ||
			    Metadata.IsPlacementDisabled(ExtendedItem.ComponentType))
				return;
            IPlacementBehavior behavior = ExtendedItem.GetBehavior<IPlacementBehavior>();
            if (behavior == null)
			    ExtendedItem.AddBehavior(typeof(IPlacementBehavior), this);
		}

		public virtual bool CanPlace(ICollection<DesignItem> childItems, PlacementType type, PlacementAlignment position)
		{
            foreach (var di in childItems)
            {
                if (di.ComponentType != null && di.ComponentType.Name == "VBEdge")
                    return false;
            }
			return true;
		}

		public virtual void BeginPlacement(PlacementOperation operation)
		{
		}

		public virtual void EndPlacement(PlacementOperation operation)
		{
		}

		public virtual Rect GetPosition(PlacementOperation operation, DesignItem item, bool verifyAndCorrectPosition)
		{
			if (item.View == null)
				return Rect.Empty;
			var p = item.View.TranslatePoint(new Point(), operation.CurrentContainer.View);

			if (verifyAndCorrectPosition)
			{
				var left = item.SettedProperties.FirstOrDefault(c => c.IsSet && c.FullName == "System.Windows.Controls.Canvas.Left");
				var top = item.SettedProperties.FirstOrDefault(c => c.IsSet && c.FullName == "System.Windows.Controls.Canvas.Top");

				if (left != null && top != null)
				{
					double? canvasLeft = left.CurrentValue as double?;
					double? canvasTop = top.CurrentValue as double?;

					if (canvasLeft.HasValue && canvasTop.HasValue && (p.X != canvasLeft || p.Y != canvasTop))
						p = new Point(canvasLeft.Value, canvasTop.Value);
				}
			}

			return new Rect(p, item.View.RenderSize);
		}

		public virtual void BeforeSetPosition(PlacementOperation operation)
		{
		}

		public virtual void SetPosition(PlacementInformation info)
		{
			ModelTools.Resize(info.Item, info.Bounds.Width, info.Bounds.Height);
		}

		public virtual bool CanLeaveContainer(PlacementOperation operation)
		{
			return true;
		}

		public virtual void LeaveContainer(PlacementOperation operation)
		{
			if (ExtendedItem.ContentProperty.IsCollection) {
				foreach (var info in operation.PlacedItems) {
					ExtendedItem.ContentProperty.CollectionElements.Remove(info.Item);
				}
			} else {
				ExtendedItem.ContentProperty.Reset();
			}
		}

		public virtual bool CanEnterContainer(PlacementOperation operation)
		{
			if (ExtendedItem.ContentProperty.IsCollection)
				return CollectionSupport.CanCollectionAdd(ExtendedItem.ContentProperty.ReturnType,
				                                          operation.PlacedItems.Select(p => p.Item.Component));
			if (ExtendedItem.View is ContentControl) {
				if (!CanContentControlAdd((ContentControl) ExtendedItem.View)) {
					return false;
				}
			}
			
			if (!ExtendedItem.ContentProperty.IsSet)
				return true;
			
			object value = ExtendedItem.ContentProperty.ValueOnInstance;
			// don't overwrite non-primitive values like bindings
			return ExtendedItem.ContentProperty.Value == null && (value is string && string.IsNullOrEmpty(value as string));
		}

		public virtual void EnterContainer(PlacementOperation operation)
		{
			if (ExtendedItem.ContentProperty.IsCollection) {
				foreach (var info in operation.PlacedItems) {
					ExtendedItem.ContentProperty.CollectionElements.Add(info.Item);
				}
			} else {
				ExtendedItem.ContentProperty.SetValue(operation.PlacedItems[0].Item);
			}
			if (operation.Type == PlacementType.AddItem) {
				foreach (var info in operation.PlacedItems) {
					SetPosition(info);
				}
			}
		}

        public virtual bool CanMoveVector(PlacementOperation operation, Vector vector)
        {
            if (!(operation.CurrentContainer.View is System.Windows.Controls.Canvas))
                return true;
            Canvas c = operation.CurrentContainer.View as Canvas;
            double height = c.ActualHeight;
            double width = c.ActualWidth;

            foreach (PlacementInformation info in operation.PlacedItems)
            {
                if (info.OriginalBounds.Left + Math.Round(vector.X, PlacementInformation.BoundsPrecision) < 0)
                    return false;
                if (info.OriginalBounds.Top + Math.Round(vector.Y, PlacementInformation.BoundsPrecision) < 0)
                    return false;
                if (info.OriginalBounds.Left + Math.Round(vector.X, PlacementInformation.BoundsPrecision) + info.OriginalBounds.Width > width)
                    return false;
                if (info.OriginalBounds.Top + Math.Round(vector.Y, PlacementInformation.BoundsPrecision) + info.OriginalBounds.Height > height)
                    return false;
            }

            return true;
        }
	}
}
