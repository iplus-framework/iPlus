// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.ext.design.avui.Extensions;
using gip.ext.designer.avui.Controls;
using System.Diagnostics;
using gip.ext.xamldom.avui;
using gip.ext.design.avui;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia;
using Avalonia.Input;
using gip.ext.design.avui.Designer;
using gip.ext.design.avui.UIExtensions;

namespace gip.ext.designer.avui.Extensions
{
	[ExtensionFor(typeof(Panel))]
	//[ExtensionFor(typeof(ContentControl))]
    [ExtensionFor(typeof(Control))]
    [ExtensionFor(typeof(Border))]
    [ExtensionFor(typeof(Viewbox))]
    public class DefaultPlacementBehavior : BehaviorExtension, IPlacementBehavior
    {
        protected static List<Type> _contentControlsNotAllowedToAdd;

		static DefaultPlacementBehavior()
		{
			_contentControlsNotAllowedToAdd = new List<Type>();
			//_contentControlsNotAllowedToAdd.Add(typeof (Frame));
			//_contentControlsNotAllowedToAdd.Add(typeof (GroupItem));
			_contentControlsNotAllowedToAdd.Add(typeof (HeaderedContentControl));
			_contentControlsNotAllowedToAdd.Add(typeof (Label));
			_contentControlsNotAllowedToAdd.Add(typeof (ListBoxItem));
			_contentControlsNotAllowedToAdd.Add(typeof (Button));
			//_contentControlsNotAllowedToAdd.Add(typeof (StatusBarItem));
            _contentControlsNotAllowedToAdd.Add(typeof (ToolTip));
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

        public static bool CanContentControlAdd(ContentControl control)
		{
			Debug.Assert(control != null);
			return !_contentControlsNotAllowedToAdd.Any(type => type.IsAssignableFrom(control.GetType()));
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
            InfoTextEnterArea.Stop(ref infoTextEnterArea);
        }

        public virtual Rect GetPosition(PlacementOperation operation, DesignItem item, bool verifyAndCorrectPosition)
		{
			if (item.View == null)
				return RectExtensions.Empty();
            var p = item.View.TranslatePoint(new Point(), operation.CurrentContainer.View);

			if (verifyAndCorrectPosition)
			{
				var left = item.SettedProperties.FirstOrDefault(c => c.IsSet && c.FullName == "Avalonia.Controls.Canvas.Left");
				var top = item.SettedProperties.FirstOrDefault(c => c.IsSet && c.FullName == "Avalonia.Controls.Canvas.Top");

				if (left != null && top != null)
				{
					double? canvasLeft = left.CurrentValue as double?;
					double? canvasTop = top.CurrentValue as double?;

					if (canvasLeft.HasValue && canvasTop.HasValue && (p?.X != canvasLeft || p?.Y != canvasTop))
						p = new Point(canvasLeft.Value, canvasTop.Value);
				}
                return new Rect(p.Value, item.View.Bounds.Size);
            }
            else
			{
                return GetPositionRelativeToContainer(operation, item);
            }
		}

        public Rect GetPositionRelativeToContainer(PlacementOperation operation, DesignItem item)
        {
            if (item.View == null)
                return RectExtensions.Empty();
            var p = item.View.TranslatePoint(new Point(), operation.CurrentContainer.View);

            return new Rect(p.Value, PlacementOperation.GetRealElementSize(item.View));
        }

        public virtual void BeforeSetPosition(PlacementOperation operation)
		{
		}

		public virtual void SetPosition(PlacementInformation info)
		{
            if (info.Operation.Type != PlacementType.Move
			&& info.Operation.Type != PlacementType.MovePoint
			&& info.Operation.Type != PlacementType.MoveAndIgnoreOtherContainers)
                ModelTools.Resize(info.Item, info.Bounds.Width, info.Bounds.Height);
		}

		public virtual bool CanLeaveContainer(PlacementOperation operation)
		{
			return true;
		}

		public virtual void LeaveContainer(PlacementOperation operation)
		{
            foreach (var info in operation.PlacedItems)
            {
                var parentProperty = info.Item.ParentProperty;
                if (parentProperty.IsCollection)
                {
                    parentProperty.CollectionElements.Remove(info.Item);
                }
                else
                {
                    parentProperty.Reset();
                }
            }
			// Old code:
            //if (ExtendedItem.ContentProperty.IsCollection) {
            //	foreach (var info in operation.PlacedItems) {
            //		ExtendedItem.ContentProperty.CollectionElements.Remove(info.Item);
            //	}
            //} else {
            //	ExtendedItem.ContentProperty.Reset();
            //}
        }

        private static InfoTextEnterArea infoTextEnterArea;

        public virtual bool CanEnterContainer(PlacementOperation operation, bool shouldAlwaysEnter)
		{
            var canEnter = internalCanEnterContainer(operation);

            if (canEnter && !shouldAlwaysEnter && !IsKeyDown(Key.LeftAlt) && !IsKeyDown(Key.RightAlt))
            {
                var b = new Rect(0, 0, ((Control)this.ExtendedItem.View).Bounds.Width, ((Control)this.ExtendedItem.View).Bounds.Height);
                InfoTextEnterArea.Start(ref infoTextEnterArea, this.Services, this.ExtendedItem.View, b, Translations.Instance.PressAltText);

                return false;
            }

            return canEnter;

            // Old code:
            //if (ExtendedItem.ContentProperty.IsCollection)
            //	return CollectionSupport.CanCollectionAdd(ExtendedItem.ContentProperty.ReturnType,
            //	                                          operation.PlacedItems.Select(p => p.Item.Component));
            //if (ExtendedItem.View is ContentControl) {
            //	if (!CanContentControlAdd((ContentControl) ExtendedItem.View)) {
            //		return false;
            //	}
            //}

            //if (!ExtendedItem.ContentProperty.IsSet)
            //	return true;

            //object value = ExtendedItem.ContentProperty.ValueOnInstance;
            //// don't overwrite non-primitive values like bindings
            //return ExtendedItem.ContentProperty.Value == null && (value is string && string.IsNullOrEmpty(value as string));
        }


        private bool internalCanEnterContainer(PlacementOperation operation)
        {
            InfoTextEnterArea.Stop(ref infoTextEnterArea);

            if (ExtendedItem.Component is Expander)
            {
                if (!((Expander)ExtendedItem.Component).IsExpanded)
                {
                    ((Expander)ExtendedItem.Component).IsExpanded = true;
                }
            }

            if (ExtendedItem.Component is UserControl && ExtendedItem.ComponentType != typeof(UserControl))
                return false;

            if (ExtendedItem.Component is Decorator)
                return ((Decorator)ExtendedItem.Component).Child == null;

            if (ExtendedItem.ContentProperty.IsCollection)
                return CollectionSupport.CanCollectionAdd(ExtendedItem.ContentProperty.ReturnType,
                                                          operation.PlacedItems.Select(p => p.Item.Component));

            if (ExtendedItem.ContentProperty.ReturnType == typeof(string))
                return false;

            if (!ExtendedItem.ContentProperty.IsSet)
                return true;

            object value = ExtendedItem.ContentProperty.GetConvertedValueOnInstance<object>();
            // don't overwrite non-primitive values like bindings
            return ExtendedItem.ContentProperty.Value == null && (value is string && string.IsNullOrEmpty(value as string));
        }

        public virtual void EnterContainer(PlacementOperation operation)
		{
            if (ExtendedItem.ContentProperty.IsCollection)
            {
                foreach (var info in operation.PlacedItems)
                {
                    ExtendedItem.ContentProperty.CollectionElements.Add(info.Item);
                }
            }
            else
            {
                ExtendedItem.ContentProperty.SetValue(operation.PlacedItems[0].Item);
            }
            if (operation.Type == PlacementType.AddItem)
            {
                foreach (var info in operation.PlacedItems)
                {
                    SetPosition(info);
                }
            }
        }

        public virtual bool CanMoveVector(PlacementOperation operation, Vector vector)
        {
            if (!(operation.CurrentContainer.View is Canvas))
                return true;
            Canvas c = operation.CurrentContainer.View as Canvas;
            double height = c.Bounds.Height;
            double width = c.Bounds.Width;

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

        public virtual Point PlacePoint(Point point)
        {
            return new Point(Math.Round(point.X), Math.Round(point.Y));
        }
    }
}
