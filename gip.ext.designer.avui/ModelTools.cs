// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using gip.ext.design.avui;
using gip.ext.designer.avui.Controls;
using gip.ext.graphics.avui.shapes;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Layout;
using Avalonia.Controls.Shapes;
using gip.ext.designer.avui.Xaml;
using Avalonia.Media;

namespace gip.ext.designer.avui
{
	/// <summary>
	/// Static helper methods for working with the designer DOM.
	/// </summary>
	public static class ModelTools
	{
		/// <summary>
		/// Compares the positions of a and b in the model file.
		/// </summary>
		public static int ComparePositionInModelFile(DesignItem a, DesignItem b)
		{
			// first remember all parent properties of a
			HashSet<DesignItemProperty> aProps = new HashSet<DesignItemProperty>();
			DesignItem tmp = a;
			while (tmp != null) {
				aProps.Add(tmp.ParentProperty);
				tmp = tmp.Parent;
			}
			
			// now walk up b's parent tree until a matching property is found
			tmp = b;
			while (tmp != null) {
				DesignItemProperty prop = tmp.ParentProperty;
				if (aProps.Contains(prop)) {
					if (prop.IsCollection) {
						return prop.CollectionElements.IndexOf(a).CompareTo(prop.CollectionElements.IndexOf(b));
					} else {
						return 0;
					}
				}
			}
			return 0;
		}
		
		/// <summary>
		/// Gets if the specified design item is in the document it belongs to.
		/// </summary>
		/// <returns>True for live objects, false for deleted objects.</returns>
		public static bool IsInDocument(DesignItem item)
		{
			DesignItem rootItem = item.Context.RootItem;
			while (item != null) {
				if (item == rootItem) return true;
				item = item.Parent;
			}
			return false;
		}
		
		/// <summary>
		/// Gets if the specified components can be deleted.
		/// </summary>
		public static bool CanDeleteComponents(ICollection<DesignItem> items)
		{
			IPlacementBehavior b = PlacementOperation.GetPlacementBehavior(items);
			return b != null
				&& b.CanPlace(items, PlacementType.Delete, PlacementAlignment.Center);
		}

		public static bool CanSelectComponent(DesignItem item)
		{
			return item.View != null;
		}
		
		/// <summary>
		/// Deletes the specified components from their parent containers.
		/// If the deleted components are currently selected, they are deselected before they are deleted.
		/// </summary>
		public static void DeleteComponents(ICollection<DesignItem> items)
		{
			DesignItem parent = items.First().Parent;
			PlacementOperation operation = PlacementOperation.Start(items, PlacementType.Delete);
			if (operation != null)
			{
				try
				{
					ISelectionService selectionService = items.First().Services.Selection;
					selectionService.SetSelectedComponents(items, SelectionTypes.Remove);
					// if the selection is empty after deleting some components, select the parent of the deleted component
					if (selectionService.SelectionCount == 0 && !items.Contains(parent))
					{
						selectionService.SetSelectedComponents(new DesignItem[] { parent });
					}
					operation.DeleteItemsAndCommit();
				}
				catch (Exception ec)
				{
					operation.Abort();

					string msg = ec.Message;
					if (ec.InnerException != null && ec.InnerException.Message != null)
						msg += " Inner:" + ec.InnerException.Message;

					if (gip.core.datamodel.Database.Root != null && gip.core.datamodel.Database.Root.Messages != null &&
																		  gip.core.datamodel.Database.Root.InitState == gip.core.datamodel.ACInitState.Initialized)
						gip.core.datamodel.Database.Root.Messages.LogException("ModelTools", "DeleteComponents", msg);

					throw;
				}
			}
		}

        public static void CreateVisualTree(this Control element)
        {
            try
            {
                // TODO: Avalonia dosn't support Flowdocs
                //var fixedDoc = new FixedDocument();
                //var pageContent = new PageContent();
                //var fixedPage = new FixedPage();
                //fixedPage.Children.Add(element);
                //(pageContent as IAddChild).AddChild(fixedPage);
                //fixedDoc.Pages.Add(pageContent);

                //var f = new XpsSerializerFactory();
                //var w = f.CreateSerializerWriter(new MemoryStream());
                //w.Write(fixedDoc);

                //fixedPage.Children.Remove(element);
            }
            catch (Exception)
            { 
            }
        }

        internal static Size GetDefaultSize(DesignItem createdItem)
		{
            if (!(createdItem.Component is Control))
                return new Size(1, 1);

            Size? defS = Metadata.GetDefaultSize(createdItem.ComponentType, false);
            if (defS.HasValue && defS.Value.Width != double.NaN)
                return defS.Value;

            CreateVisualTree(createdItem.View);

            var s = createdItem.View.DesiredSize;
            double width = s.Width;
            double height = s.Height;

            Size? newS = Metadata.GetDefaultSize(createdItem.ComponentType, true);
            if (newS.HasValue && defS.Value.Width != double.NaN)
            {
                if (!(width > 5) && newS.Value.Width > 0)
                    width = newS.Value.Width;

                if (!(height > 5) && newS.Value.Height > 0)
                    height = newS.Value.Height;
            }

            if (double.IsNaN(width) && GetWidth(createdItem.View) > 0)
            {
                width = GetWidth(createdItem.View);
            }
            if (double.IsNaN(height) && GetWidth(createdItem.View) > 0)
            {
                height = GetHeight(createdItem.View);
            }

            return s;

            // Old code:
            //var s = Metadata.GetDefaultSize(createdItem.ComponentType);
            //if (double.IsNaN(s.Width)) {
            //    s.Width = GetWidth(createdItem.View);
            //}
            //if (double.IsNaN(s.Width) || s.Width < 0.1)
            //    s.Width = 40;
            //if (double.IsNaN(s.Height))
            //{
            //    s.Height = GetHeight(createdItem.View);
            //}
            //if (double.IsNaN(s.Height) || s.Height < 0.1)
            //    s.Height = 20;
            //return s;
        }
		
		internal static double GetWidth(Control element)
		{
			double v = (double)element.GetValue(Layoutable.WidthProperty);
			if (double.IsNaN(v))
				return element.Bounds.Width;
			else
				return v;
		}
		
		internal static double GetHeight(Control element)
		{
			double v = (double)element.GetValue(Layoutable.HeightProperty);
			if (double.IsNaN(v))
				return element.Bounds.Height;
			else
				return v;
		}

        internal static double GetCanvasLeft(Control element)
        {
            double v = (double)element.GetValue(Canvas.LeftProperty);
            if (double.IsNaN(v))
                return 0;
            else
                return v;
        }

        internal static double GetCanvasTop(Control element)
        {
            double v = (double)element.GetValue(Canvas.TopProperty);
            if (double.IsNaN(v))
                return 0;
            else
                return v;
        }

		public static void Resize(DesignItem item, double newWidth, double newHeight)
		{
            if (typeof(Shape).IsAssignableFrom(item.ComponentType)
                && !typeof(Rectangle).IsAssignableFrom(item.ComponentType)
                && !typeof(Ellipse).IsAssignableFrom(item.ComponentType))
            {
                DesignItemProperty property = item.Properties.HasProperty(Layoutable.WidthProperty.Name);
                if (property != null)
                    property.Reset();
                property = item.Properties.HasProperty(Layoutable.HeightProperty.Name);
                if (property != null)
                    property.Reset();
                return;
            }
            if (newWidth != GetWidth(item.View))
            {
                if (double.IsNaN(newWidth))
                    item.Properties.GetProperty(Layoutable.WidthProperty).Reset();
                else
                    item.Properties.GetProperty(Layoutable.WidthProperty).SetValue(newWidth);
            }
            if (newHeight != GetHeight(item.View))
            {
                if (double.IsNaN(newHeight))
                    item.Properties.GetProperty(Layoutable.HeightProperty).Reset();
                else
                    item.Properties.GetProperty(Layoutable.HeightProperty).SetValue(newHeight);
            }
		}


        private class ItemPos
        {
            public HorizontalAlignment HorizontalAlignment { get; set; }

            public VerticalAlignment VerticalAlignment { get; set; }

            public double Xmin { get; set; }

            public double Xmax { get; set; }

            public double Ymin { get; set; }

            public double Ymax { get; set; }

            public DesignItem DesignItem { get; set; }
        }

        private static ItemPos GetItemPos(PlacementOperation operation, DesignItem designItem)
        {
            var itemPos = new ItemPos() { DesignItem = designItem };

            var pos = operation.CurrentContainerBehavior.GetPosition(operation, designItem, false);
            itemPos.Xmin = pos.X;
            itemPos.Xmax = pos.X + pos.Width;
            itemPos.Ymin = pos.Y;
            itemPos.Ymax = pos.Y + pos.Height;

            return itemPos;
        }

        public static Tuple<DesignItem, Rect> WrapItemsNewContainer(IEnumerable<DesignItem> items, Type containerType, bool doInsert = true)
        {
            var collection = items;

            var _context = collection.First().Context as XamlDesignContext;

            var container = collection.First().Parent;

            if (collection.Any(x => x.Parent != container))
                return null;

            //Change Code to use the Placment Operation!
            var placement = container.Extensions.OfType<IPlacementBehavior>().FirstOrDefault();
            if (placement == null)
                return null;

            var operation = PlacementOperation.Start(items.ToList(), PlacementType.Move);

            var newInstance = _context.Services.ExtensionManager.CreateInstanceWithCustomInstanceFactory(containerType, null);
            DesignItem newPanel = _context.Services.Component.RegisterComponentForDesigner(newInstance);

            List<ItemPos> itemList = new List<ItemPos>();

            int? firstIndex = null;

            foreach (var item in collection)
            {
                itemList.Add(GetItemPos(operation, item));
                //var pos = placement.GetPosition(null, item);
                if (container.Component is Canvas)
                {
                    item.Properties.GetAttachedProperty(Canvas.RightProperty).Reset();
                    item.Properties.GetAttachedProperty(Canvas.LeftProperty).Reset();
                    item.Properties.GetAttachedProperty(Canvas.TopProperty).Reset();
                    item.Properties.GetAttachedProperty(Canvas.BottomProperty).Reset();
                }
                else if (container.Component is Grid)
                {
                    item.Properties.GetProperty(Layoutable.HorizontalAlignmentProperty).Reset();
                    item.Properties.GetProperty(Layoutable.VerticalAlignmentProperty).Reset();
                    item.Properties.GetProperty(Layoutable.MarginProperty).Reset();
                }

                if (item.ParentProperty.IsCollection)
                {
                    var parCol = item.ParentProperty.CollectionElements;
                    if (!firstIndex.HasValue)
                        firstIndex = parCol.IndexOf(item);
                    parCol.Remove(item);
                }
                else
                {
                    item.ParentProperty.Reset();
                }
            }

            var xmin = itemList.Min(x => x.Xmin);
            var xmax = itemList.Max(x => x.Xmax);
            var ymin = itemList.Min(x => x.Ymin);
            var ymax = itemList.Max(x => x.Ymax);

            foreach (var item in itemList)
            {
                if (newPanel.Component is Canvas)
                {
                    if (item.HorizontalAlignment == HorizontalAlignment.Right)
                    {
                        item.DesignItem.Properties.GetAttachedProperty(Canvas.RightProperty).SetValue(xmax - item.Xmax);
                    }
                    else
                    {
                        item.DesignItem.Properties.GetAttachedProperty(Canvas.LeftProperty).SetValue(item.Xmin - xmin);
                    }

                    if (item.VerticalAlignment == VerticalAlignment.Bottom)
                    {
                        item.DesignItem.Properties.GetAttachedProperty(Canvas.BottomProperty).SetValue(ymax - item.Ymax);
                    }
                    else
                    {
                        item.DesignItem.Properties.GetAttachedProperty(Canvas.TopProperty).SetValue(item.Ymin - ymin);
                    }

                    newPanel.ContentProperty.CollectionElements.Add(item.DesignItem);

                }
                else if (newPanel.Component is Grid)
                {
                    double right = 0;
                    double left = 0;
                    double bottom = 0;
                    double top = 0;
                    if (item.HorizontalAlignment == HorizontalAlignment.Right)
                    {
                        item.DesignItem.Properties.GetProperty(Layoutable.HorizontalAlignmentProperty).SetValue(HorizontalAlignment.Right);
                        right = xmax - item.Xmax;
                    }
                    else
                    {
                        item.DesignItem.Properties.GetProperty(Layoutable.HorizontalAlignmentProperty).SetValue(HorizontalAlignment.Left);
                        left = item.Xmin - xmin;
                    }

                    if (item.VerticalAlignment == VerticalAlignment.Bottom)
                    {
                        item.DesignItem.Properties.GetProperty(Layoutable.VerticalAlignmentProperty).SetValue(VerticalAlignment.Bottom);
                        bottom = ymax - item.Ymax;
                    }
                    else
                    {
                        item.DesignItem.Properties.GetProperty(Layoutable.VerticalAlignmentProperty).SetValue(VerticalAlignment.Top);
                        top = item.Ymin - ymin;
                    }
                    Thickness thickness = new Thickness(left, top, right, bottom);

                    item.DesignItem.Properties.GetProperty(Layoutable.MarginProperty).SetValue(thickness);

                    newPanel.ContentProperty.CollectionElements.Add(item.DesignItem);

                }
                else if (newPanel.Component is Viewbox)
                {
                    newPanel.ContentProperty.SetValue(item.DesignItem);
                }
                else if (newPanel.Component is ContentControl)
                {
                    newPanel.ContentProperty.SetValue(item.DesignItem);
                }
            }

            if (doInsert)
            {
                PlacementOperation operation2 = PlacementOperation.TryStartInsertNewComponents(
                    container,
                    new[] { newPanel },
                    new[] { new Rect(xmin, ymin, xmax - xmin, ymax - ymin).Round() },
                    PlacementType.AddItem
                );

                if (items.Count() == 1 && container.ContentProperty != null && container.ContentProperty.IsCollection)
                {
                    container.ContentProperty.CollectionElements.Remove(newPanel);
                    container.ContentProperty.CollectionElements.Insert(firstIndex.Value, newPanel);
                }

                operation2.Commit();

                _context.Services.Selection.SetSelectedComponents(new[] { newPanel });
            }

            operation.Commit();

            return new Tuple<DesignItem, Rect>(newPanel, new Rect(xmin, ymin, xmax - xmin, ymax - ymin).Round());
        }

        public static void UnwrapItemsFromContainer(DesignItem container)
        {
            var collection = container.ContentProperty.CollectionElements.ToList();

            var newPanel = container.Parent;

            if (collection.Any(x => x.Parent != container))
                return;

            //Change Code to use the Placment Operation!
            var placement = container.Extensions.OfType<IPlacementBehavior>().FirstOrDefault();
            if (placement == null)
                return;

            var operation = PlacementOperation.Start(collection.ToList(), PlacementType.Move);

            List<ItemPos> itemList = new List<ItemPos>();

            int? firstIndex = null;

            var containerPos = GetItemPos(operation, container);

            foreach (var item in collection)
            {
                itemList.Add(GetItemPos(operation, item));
                if (container.Component is Canvas)
                {
                    item.Properties.GetAttachedProperty(Canvas.RightProperty).Reset();
                    item.Properties.GetAttachedProperty(Canvas.LeftProperty).Reset();
                    item.Properties.GetAttachedProperty(Canvas.TopProperty).Reset();
                    item.Properties.GetAttachedProperty(Canvas.BottomProperty).Reset();
                }
                else if (container.Component is Grid)
                {
                    item.Properties.GetProperty(Layoutable.HorizontalAlignmentProperty).Reset();
                    item.Properties.GetProperty(Layoutable.VerticalAlignmentProperty).Reset();
                    item.Properties.GetProperty(Layoutable.MarginProperty).Reset();
                }

                if (item.ParentProperty.IsCollection)
                {
                    var parCol = item.ParentProperty.CollectionElements;
                    if (!firstIndex.HasValue)
                        firstIndex = parCol.IndexOf(item);
                    parCol.Remove(item);
                }
                else
                {
                    item.ParentProperty.Reset();
                }
            }

            newPanel.ContentProperty.CollectionElements.Remove(container);

            foreach (var item in itemList)
            {
                if (newPanel.Component is Canvas)
                {
                    if (item.HorizontalAlignment == HorizontalAlignment.Right)
                    {
                        item.DesignItem.Properties.GetAttachedProperty(Canvas.RightProperty).SetValue(containerPos.Xmax - item.Xmax);
                    }
                    else
                    {
                        item.DesignItem.Properties.GetAttachedProperty(Canvas.LeftProperty).SetValue(item.Xmin + containerPos.Xmin);
                    }

                    if (item.VerticalAlignment == VerticalAlignment.Bottom)
                    {
                        item.DesignItem.Properties.GetAttachedProperty(Canvas.BottomProperty).SetValue(containerPos.Ymax - item.Ymax);
                    }
                    else
                    {
                        item.DesignItem.Properties.GetAttachedProperty(Canvas.TopProperty).SetValue(item.Ymin + containerPos.Ymin);
                    }

                    newPanel.ContentProperty.CollectionElements.Add(item.DesignItem);

                }
                else if (newPanel.Component is Grid)
                {
                    double right = 0;
                    double left = 0;
                    double bottom = 0;
                    double top = 0;
                    if (item.HorizontalAlignment == HorizontalAlignment.Right)
                    {
                        item.DesignItem.Properties.GetProperty(Layoutable.HorizontalAlignmentProperty).SetValue(HorizontalAlignment.Right);
                        right = containerPos.Xmax - item.Xmax;
                    }
                    else
                    {
                        item.DesignItem.Properties.GetProperty(Layoutable.HorizontalAlignmentProperty).SetValue(HorizontalAlignment.Left);
                        left = item.Xmin;
                    }

                    if (item.VerticalAlignment == VerticalAlignment.Bottom)
                    {
                        item.DesignItem.Properties.GetProperty(Layoutable.VerticalAlignmentProperty).SetValue(VerticalAlignment.Bottom);
                        bottom = containerPos.Ymax - item.Ymax;
                    }
                    else
                    {
                        item.DesignItem.Properties.GetProperty(Layoutable.VerticalAlignmentProperty).SetValue(VerticalAlignment.Top);
                        top = item.Ymin;
                    }
                    Thickness thickness = new Thickness(left, top, right, bottom);

                    item.DesignItem.Properties.GetProperty(Layoutable.MarginProperty).SetValue(thickness);

                    newPanel.ContentProperty.CollectionElements.Add(item.DesignItem);

                }
                else if (newPanel.Component is Viewbox)
                {
                    newPanel.ContentProperty.SetValue(item.DesignItem);
                }
                else if (newPanel.Component is ContentControl)
                {
                    newPanel.ContentProperty.SetValue(item.DesignItem);
                }
            }

            operation.Commit();
        }

        public static void ApplyTransform(DesignItem designItem, Transform transform, bool relative = true, AvaloniaProperty transformProperty = null)
        {
            var changeGroup = designItem.OpenGroup("Apply Transform");

            transformProperty = transformProperty ?? Visual.RenderTransformProperty;
            Transform oldTransform = null;
            if (designItem.Properties.GetProperty(transformProperty).IsSet)
            {
                oldTransform = designItem.Properties.GetProperty(transformProperty).GetConvertedValueOnInstance<Transform>();
            }

            if (oldTransform is MatrixTransform)
            {
                var mt = oldTransform as MatrixTransform;
                var tg = new TransformGroup();
                if (mt.Matrix.M31 != 0 && mt.Matrix.M32 != 0)
                    tg.Children.Add(new TranslateTransform() { X = mt.Matrix.M31, Y = mt.Matrix.M32 });
                if (mt.Matrix.M11 != 0 && mt.Matrix.M22 != 0)
                    tg.Children.Add(new ScaleTransform() { ScaleX = mt.Matrix.M11, ScaleY = mt.Matrix.M22 });

                var angle = Math.Atan2(mt.Matrix.M21, mt.Matrix.M11) * 180 / Math.PI;
                if (angle != 0)
                    tg.Children.Add(new RotateTransform() { Angle = angle });
                //if (mt.Matrix.M11 != 0 && mt.Matrix.M22 != 0)
                //	tg.Children.Add(new SkewTransform(){ ScaleX = mt.Matrix.M11, ScaleY = mt.Matrix.M22 });
            }
            else if (oldTransform != null && oldTransform.GetType() != transform.GetType())
            {
                var tg = new TransformGroup();
                var tgDes = designItem.Services.Component.RegisterComponentForDesigner(tg);
                tgDes.ContentProperty.CollectionElements.Add(designItem.Services.Component.GetDesignItem(oldTransform));
                designItem.Properties.GetProperty(Visual.RenderTransformProperty).SetValue(tg);
                oldTransform = tg;
            }



            if (transform is RotateTransform)
            {
                var rotateTransform = transform as RotateTransform;

                if (oldTransform is RotateTransform || oldTransform == null)
                {
                    if (rotateTransform.Angle != 0)
                    {
                        designItem.Properties.GetProperty(transformProperty).SetValue(transform);
                        var angle = rotateTransform.Angle;
                        if (relative && oldTransform != null)
                        {
                            angle = rotateTransform.Angle + ((RotateTransform)oldTransform).Angle;
                        }
                        designItem.Properties.GetProperty(transformProperty).Value.Properties.GetProperty(RotateTransform.AngleProperty).SetValue(angle);
                        if (rotateTransform.CenterX != 0.0)
                            designItem.Properties.GetProperty(transformProperty).Value.Properties.GetProperty(RotateTransform.CenterXProperty).SetValue(rotateTransform.CenterX);
                        if (rotateTransform.CenterY != 0.0)
                            designItem.Properties.GetProperty(transformProperty).Value.Properties.GetProperty(RotateTransform.CenterYProperty).SetValue(rotateTransform.CenterY);

                        if (oldTransform == null)
                            designItem.Properties.GetProperty(Visual.RenderTransformOriginProperty).SetValue(new Point(0.5, 0.5));
                    }
                    else
                    {
                        designItem.Properties.GetProperty(transformProperty).Reset();
                        designItem.Properties.GetProperty(Visual.RenderTransformOriginProperty).Reset();
                    }
                }
                else if (oldTransform is TransformGroup)
                {
                    var tg = oldTransform as TransformGroup;
                    var rot = tg.Children.FirstOrDefault(x => x is RotateTransform);
                    if (rot != null)
                    {
                        designItem.Services.Component.GetDesignItem(tg).ContentProperty.CollectionElements.Remove(designItem.Services.Component.GetDesignItem(rot));
                    }
                    if (rotateTransform.Angle != 0)
                    {
                        var des = designItem.Services.Component.GetDesignItem(transform);
                        if (des == null)
                            des = designItem.Services.Component.RegisterComponentForDesigner(transform);
                        designItem.Services.Component.GetDesignItem(tg).ContentProperty.CollectionElements.Add(des);
                        if (oldTransform == null)
                            designItem.Properties.GetProperty(Visual.RenderTransformOriginProperty).SetValue(new Point(0.5, 0.5));
                    }
                }
                else
                {
                    if (rotateTransform.Angle != 0)
                    {
                        designItem.Properties.GetProperty(transformProperty).SetValue(transform);
                        if (oldTransform == null)
                            designItem.Properties.GetProperty(Visual.RenderTransformOriginProperty).SetValue(new Point(0.5, 0.5));
                    }
                }
            }

            ((DesignPanel)designItem.Services.DesignPanel).AdornerLayer.UpdateAdornersForElement(designItem.View, true);

            changeGroup.Commit();
        }

        public static void StretchItems(IEnumerable<DesignItem> items, StretchDirection stretchDirection)
        {
            var collection = items;

            var container = collection.First().Parent;

            if (collection.Any(x => x.Parent != container))
                return;

            var placement = container.Extensions.OfType<IPlacementBehavior>().FirstOrDefault();
            if (placement == null)
                return;

            var changeGroup = container.OpenGroup("StretchItems");

            var w = GetWidth(collection.First().View);
            var h = GetHeight(collection.First().View);

            foreach (var item in collection.Skip(1))
            {
                switch (stretchDirection)
                {
                    case StretchDirection.Width:
                        {
                            if (!double.IsNaN(w))
                                item.Properties.GetProperty(Layoutable.WidthProperty).SetValue(w);
                        }
                        break;
                    case StretchDirection.Height:
                        {
                            if (!double.IsNaN(h))
                                item.Properties.GetProperty(Layoutable.HeightProperty).SetValue(h);
                        }
                        break;
                }
            }

            changeGroup.Commit();
        }

        public static void ArrangeItems(IEnumerable<DesignItem> items, ArrangeDirection arrangeDirection)
        {
            var collection = items;

            var _context = collection.First().Context as XamlDesignContext;

            var container = collection.First().Parent;

            if (collection.Any(x => x.Parent != container))
                return;

            var placement = container.Extensions.OfType<IPlacementBehavior>().FirstOrDefault();
            if (placement == null)
                return;

            var operation = PlacementOperation.Start(items.ToList(), PlacementType.Move);

            List<ItemPos> itemList = new List<ItemPos>();
            foreach (var item in collection)
            {
                itemList.Add(GetItemPos(operation, item));
            }

            var xmin = itemList.Min(x => x.Xmin);
            var xmax = itemList.Max(x => x.Xmax);
            var mpos = (xmax - xmin) / 2 + xmin;
            var ymin = itemList.Min(x => x.Ymin);
            var ymax = itemList.Max(x => x.Ymax);
            var ympos = (ymax - ymin) / 2 + ymin;

            foreach (var item in collection)
            {
                switch (arrangeDirection)
                {
                    case ArrangeDirection.Left:
                        {
                            if (container.Component is Canvas)
                            {
                                if (!item.Properties.GetAttachedProperty(Canvas.RightProperty).IsSet)
                                {
                                    item.Properties.GetAttachedProperty(Canvas.LeftProperty).SetValue(xmin);
                                }
                                else
                                {
                                    var pos = (double)((Panel)item.Parent.Component).Bounds.Width - (xmin + (double)((Control)item.Component).Bounds.Width);
                                    item.Properties.GetAttachedProperty(Canvas.RightProperty).SetValue(pos);
                                }
                            }
                            else if (container.Component is Grid)
                            {
                                if (item.Properties.GetProperty(Layoutable.HorizontalAlignmentProperty).GetConvertedValueOnInstance<HorizontalAlignment>() != HorizontalAlignment.Right)
                                {
                                    var margin = item.Properties.GetProperty(Control.MarginProperty).GetConvertedValueOnInstance<Thickness>();
                                    margin = new Thickness(xmin, margin.Top, margin.Right, margin.Bottom);
                                    item.Properties.GetProperty(Layoutable.MarginProperty).SetValue(margin);
                                }
                                else
                                {
                                    var pos = (double)((Panel)item.Parent.Component).Bounds.Width - (xmin + (double)((Layoutable)item.Component).Bounds.Width);
                                    var margin = item.Properties.GetProperty(Layoutable.MarginProperty).GetConvertedValueOnInstance<Thickness>();
                                    margin = new Thickness(margin.Left, margin.Top, pos, margin.Bottom);
                                    item.Properties.GetProperty(Layoutable.MarginProperty).SetValue(margin);
                                }
                            }
                        }
                        break;
                    case ArrangeDirection.HorizontalMiddle:
                        {
                            if (container.Component is Canvas)
                            {
                                if (!item.Properties.GetAttachedProperty(Canvas.RightProperty).IsSet)
                                {
                                    if (!item.Properties.GetAttachedProperty(Canvas.RightProperty).IsSet)
                                    {
                                        item.Properties.GetAttachedProperty(Canvas.LeftProperty).SetValue(mpos - (((Control)item.Component).Bounds.Width) / 2);
                                    }
                                    else
                                    {
                                        var pp = mpos - (((Control)item.Component).Bounds.Width) / 2;
                                        var pos = (double)((Panel)item.Parent.Component).Bounds.Width - pp - (((Control)item.Component).Bounds.Width);
                                        item.Properties.GetAttachedProperty(Canvas.RightProperty).SetValue(pos);
                                    }
                                }
                            }
                            else if (container.Component is Grid)
                            {
                                if (item.Properties.GetProperty(Control.HorizontalAlignmentProperty).GetConvertedValueOnInstance<HorizontalAlignment>() != HorizontalAlignment.Right)
                                {
                                    var margin = item.Properties.GetProperty(Layoutable.MarginProperty).GetConvertedValueOnInstance<Thickness>();
                                    double left = mpos - (((Control)item.Component).Bounds.Width) / 2;
                                    margin = new Thickness(left, margin.Top, margin.Right, margin.Bottom);
                                    item.Properties.GetProperty(Layoutable.MarginProperty).SetValue(margin);
                                }
                                else
                                {
                                    var pp = mpos - (((Control)item.Component).Bounds.Width) / 2;
                                    var pos = (double)((Panel)item.Parent.Component).Bounds.Width - pp - (((Control)item.Component).Bounds.Width);
                                    var margin = item.Properties.GetProperty(Layoutable.MarginProperty).GetConvertedValueOnInstance<Thickness>();
                                    margin = new Thickness(margin.Left, margin.Top, pos, margin.Bottom);
                                    item.Properties.GetProperty(Layoutable.MarginProperty).SetValue(margin);
                                }
                            }
                        }
                        break;
                    case ArrangeDirection.Right:
                        {
                            if (container.Component is Canvas)
                            {
                                if (!item.Properties.GetAttachedProperty(Canvas.RightProperty).IsSet)
                                {
                                    var pos = xmax - (double)((Control)item.Component).Bounds.Width;
                                    item.Properties.GetAttachedProperty(Canvas.LeftProperty).SetValue(pos);
                                }
                                else
                                {
                                    var pos = (double)((Panel)item.Parent.Component).Bounds.Width - xmax;
                                    item.Properties.GetAttachedProperty(Canvas.RightProperty).SetValue(pos);
                                }
                            }
                            else if (container.Component is Grid)
                            {
                                if (item.Properties.GetProperty(Layoutable.HorizontalAlignmentProperty).GetConvertedValueOnInstance<HorizontalAlignment>() != HorizontalAlignment.Right)
                                {
                                    var pos = xmax - (double)((Control)item.Component).Bounds.Width;
                                    var margin = item.Properties.GetProperty(Layoutable.MarginProperty).GetConvertedValueOnInstance<Thickness>();
                                    margin = new Thickness(pos, margin.Top, margin.Right, margin.Bottom);
                                    item.Properties.GetProperty(Layoutable.MarginProperty).SetValue(margin);
                                }
                                else
                                {
                                    var pos = (double)((Panel)item.Parent.Component).Bounds.Width - xmax;
                                    var margin = item.Properties.GetProperty(Layoutable.MarginProperty).GetConvertedValueOnInstance<Thickness>();
                                    margin = new Thickness(margin.Left, margin.Top, pos, margin.Bottom);
                                    item.Properties.GetProperty(Layoutable.MarginProperty).SetValue(margin);
                                }
                            }
                        }
                        break;
                    case ArrangeDirection.Top:
                        {
                            if (container.Component is Canvas)
                            {
                                if (!item.Properties.GetAttachedProperty(Canvas.BottomProperty).IsSet)
                                {
                                    item.Properties.GetAttachedProperty(Canvas.TopProperty).SetValue(ymin);
                                }
                                else
                                {
                                    var pos = (double)((Panel)item.Parent.Component).Bounds.Height - (ymin + (double)((Control)item.Component).Bounds.Height);
                                    item.Properties.GetAttachedProperty(Canvas.BottomProperty).SetValue(pos);
                                }
                            }
                            else if (container.Component is Grid)
                            {
                                if (item.Properties.GetProperty(Layoutable.VerticalAlignmentProperty).GetConvertedValueOnInstance<VerticalAlignment>() != VerticalAlignment.Bottom)
                                {
                                    item.Properties.GetAttachedProperty(Canvas.TopProperty).SetValue(ymin);
                                    var margin = item.Properties.GetProperty(Layoutable.MarginProperty).GetConvertedValueOnInstance<Thickness>();
                                    margin = new Thickness(margin.Left, ymin, margin.Right, margin.Bottom);
                                    item.Properties.GetProperty(Layoutable.MarginProperty).SetValue(margin);
                                }
                                else
                                {
                                    var pos = (double)((Panel)item.Parent.Component).Bounds.Height - (ymin + (double)((Control)item.Component).Bounds.Height);
                                    var margin = item.Properties.GetProperty(Layoutable.MarginProperty).GetConvertedValueOnInstance<Thickness>();
                                    margin = new Thickness(margin.Left, margin.Top, margin.Right, pos);
                                    item.Properties.GetProperty(Layoutable.MarginProperty).SetValue(margin);
                                }
                            }
                        }
                        break;
                    case ArrangeDirection.VerticalMiddle:
                        {
                            if (container.Component is Canvas)
                            {
                                if (!item.Properties.GetAttachedProperty(Canvas.BottomProperty).IsSet)
                                {
                                    item.Properties.GetAttachedProperty(Canvas.TopProperty).SetValue(ympos - (((Control)item.Component).Bounds.Height) / 2);
                                }
                                else
                                {
                                    var pp = mpos - (((Control)item.Component).Bounds.Height) / 2;
                                    var pos = (double)((Panel)item.Parent.Component).Bounds.Height - pp - (((Control)item.Component).Bounds.Height);
                                    item.Properties.GetAttachedProperty(Canvas.BottomProperty).SetValue(pos);
                                }
                            }
                            else if (container.Component is Grid)
                            {
                                if (item.Properties.GetProperty(Layoutable.VerticalAlignmentProperty).GetConvertedValueOnInstance<VerticalAlignment>() != VerticalAlignment.Bottom)
                                {
                                    var margin = item.Properties.GetProperty(Layoutable.MarginProperty).GetConvertedValueOnInstance<Thickness>();
                                    double top = ympos - (((Control)item.Component).Bounds.Height) / 2;
                                    margin = new Thickness(margin.Left, top, margin.Right, margin.Bottom);
                                    item.Properties.GetProperty(Layoutable.MarginProperty).SetValue(margin);
                                }
                                else
                                {
                                    var pp = mpos - (((Control)item.Component).Bounds.Height) / 2;
                                    var pos = (double)((Panel)item.Parent.Component).Bounds.Height - pp - (((Control)item.Component).Bounds.Height);
                                    var margin = item.Properties.GetProperty(Layoutable.MarginProperty).GetConvertedValueOnInstance<Thickness>();
                                    margin = new Thickness(margin.Left, margin.Top, margin.Right, pos);
                                    item.Properties.GetProperty(Layoutable.MarginProperty).SetValue(margin);
                                }
                            }
                        }
                        break;
                    case ArrangeDirection.Bottom:
                        {
                            if (container.Component is Canvas)
                            {
                                if (!item.Properties.GetAttachedProperty(Canvas.BottomProperty).IsSet)
                                {
                                    var pos = ymax - (double)((Control)item.Component).Bounds.Height;
                                    item.Properties.GetAttachedProperty(Canvas.TopProperty).SetValue(pos);
                                }
                                else
                                {
                                    var pos = (double)((Panel)item.Parent.Component).Bounds.Height - ymax;
                                    item.Properties.GetAttachedProperty(Canvas.BottomProperty).SetValue(pos);
                                }
                            }
                            else if (container.Component is Grid)
                            {
                                if (item.Properties.GetProperty(Layoutable.VerticalAlignmentProperty).GetConvertedValueOnInstance<VerticalAlignment>() != VerticalAlignment.Bottom)
                                {
                                    var pos = ymax - (double)((Control)item.Component).Bounds.Height;
                                    var margin = item.Properties.GetProperty(Layoutable.MarginProperty).GetConvertedValueOnInstance<Thickness>();
                                    margin = new Thickness(margin.Left, pos, margin.Right, margin.Bottom);
                                    item.Properties.GetProperty(Layoutable.MarginProperty).SetValue(margin);
                                }
                                else
                                {
                                    var pos = (double)((Panel)item.Parent.Component).Bounds.Height - ymax;
                                    var margin = item.Properties.GetProperty(Layoutable.MarginProperty).GetConvertedValueOnInstance<Thickness>();
                                    margin = new Thickness(margin.Left, margin.Top, margin.Right, pos); 
                                    item.Properties.GetProperty(Layoutable.MarginProperty).SetValue(margin);
                                }
                            }
                        }
                        break;
                }
            }

            operation.Commit();
        }

    }
}
