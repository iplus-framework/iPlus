﻿// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using gip.ext.design.avui;
using gip.ext.design.avui.Extensions;
using gip.ext.designer.avui.Services;

namespace gip.ext.designer.avui.Extensions
{
	[ExtensionFor(typeof(Canvas))]
	[ExtensionFor(typeof(Grid))]
	public class DrawPolyLineExtension : BehaviorExtension, IDrawItemExtension
	{
		DesignItem CreateItem(DesignContext context, Type componentType)
		{
			object newInstance = context.Services.ExtensionManager.CreateInstanceWithCustomInstanceFactory(componentType, null);
			DesignItem item = context.Services.Component.RegisterComponentForDesigner(newInstance);
			changeGroup = item.OpenGroup("Draw Line");
			context.Services.ExtensionManager.ApplyDefaultInitializers(item);
			return item;
		}

		private ChangeGroup changeGroup;

		#region IDrawItemBehavior implementation

		public bool CanItemBeDrawn(Type createItemType)
		{
			return createItemType == typeof(Polyline) || createItemType == typeof(Polygon);
		}

		public void StartDrawItem(DesignItem clickedOn, Type createItemType, IDesignPanel panel, MouseEventArgs e, Action<DesignItem> drawItemCallback)
		{
			var createdItem = CreateItem(panel.Context, createItemType);

			var startPoint = e.GetPosition(clickedOn.View);
			var operation = PlacementOperation.TryStartInsertNewComponents(clickedOn,
			                                                               new DesignItem[] { createdItem },
			                                                               new Rect[] { new Rect(startPoint.X, startPoint.Y, double.NaN, double.NaN) },
			                                                               PlacementType.AddItem);
			if (operation != null) {
				createdItem.Services.Selection.SetSelectedComponents(new DesignItem[] { createdItem });
				operation.Commit();
			}

			createdItem.Properties[Shape.StrokeProperty].SetValue(Brushes.Black);
			createdItem.Properties[Shape.StrokeThicknessProperty].SetValue(2d);
			createdItem.Properties[Shape.StretchProperty].SetValue(Stretch.None);
			if (drawItemCallback != null)
				drawItemCallback(createdItem);

			if (createItemType == typeof(Polyline))
				createdItem.Properties[Polyline.PointsProperty].CollectionElements.Add(createdItem.Services.Component.RegisterComponentForDesigner(new Point(0,0)));
			else
				createdItem.Properties[Polygon.PointsProperty].CollectionElements.Add(createdItem.Services.Component.RegisterComponentForDesigner(new Point(0,0)));
			
			new DrawPolylineMouseGesture(createdItem, clickedOn.View, changeGroup, this.ExtendedItem.GetCompleteAppliedTransformationToView()).Start(panel, (MouseButtonEventArgs) e);
		}

		#endregion
		
		sealed class DrawPolylineMouseGesture : ClickOrDragMouseGesture
		{
			private ChangeGroup changeGroup;
			private DesignItem newLine;
			private new Point startPoint;
			private Point? lastAdded;
			private Matrix matrix;

			public DrawPolylineMouseGesture(DesignItem newLine, IInputElement relativeTo, ChangeGroup changeGroup, Transform transform)
			{
				this.newLine = newLine;
				this.positionRelativeTo = relativeTo;
				this.changeGroup = changeGroup;
				this.matrix = transform.Value;
				matrix.Invert();

				startPoint = Mouse.GetPosition(null);
			}
			
			protected override void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
			{
				e.Handled = true;
				base.OnPreviewMouseLeftButtonDown(sender, e);
			}
			
			protected override void OnMouseMove(object sender, MouseEventArgs e)
			{
				if (changeGroup == null)
					return;
				var delta = matrix.Transform(e.GetPosition(null) - startPoint);
				var diff = delta;
				if (lastAdded.HasValue) {
					diff = new Vector(lastAdded.Value.X - delta.X, lastAdded.Value.Y - delta.Y);
				}
				if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
				{
					if (Math.Abs(diff.X) > Math.Abs(diff.Y)) {
						delta.Y = 0;
						if (newLine.View is Polyline && ((Polyline)newLine.View).Points.Count > 1) {
							delta.Y = ((Polyline) newLine.View).Points.Reverse().Skip(1).First().Y;
						} else if (newLine.View is Polygon && ((Polygon)newLine.View).Points.Count > 1) {
							delta.Y = ((Polygon)newLine.View).Points.Reverse().Skip(1).First().Y;
						}
					} else {
						delta.X = 0;
						if (newLine.View is Polyline && ((Polyline)newLine.View).Points.Count > 1) {
							delta.X = ((Polyline)newLine.View).Points.Reverse().Skip(1).First().X;
						} else if (newLine.View is Polygon && ((Polygon)newLine.View).Points.Count > 1) {
							delta.X = ((Polygon)newLine.View).Points.Reverse().Skip(1).First().X;
						}
					}
				}
				var point = new Point(delta.X, delta.Y);

				if (newLine.View is Polyline) {
					if (((Polyline)newLine.View).Points.Count <= 1)
						((Polyline)newLine.View).Points.Add(point);
					if (Mouse.LeftButton != MouseButtonState.Pressed)
						((Polyline)newLine.View).Points.RemoveAt(((Polyline)newLine.View).Points.Count - 1);
					if (((Polyline)newLine.View).Points.Last() != point)
						((Polyline)newLine.View).Points.Add(point);
				} else {
					if (((Polygon)newLine.View).Points.Count <= 1)
						((Polygon)newLine.View).Points.Add(point);
					if (Mouse.LeftButton != MouseButtonState.Pressed)
						((Polygon)newLine.View).Points.RemoveAt(((Polygon)newLine.View).Points.Count - 1);
					if (((Polygon)newLine.View).Points.Last() != point)
						((Polygon)newLine.View).Points.Add(point);
				}
			}
			
			protected override void OnMouseUp(object sender, MouseButtonEventArgs e)
			{
				if (changeGroup == null)
					return;

				var delta = matrix.Transform(e.GetPosition(null) - startPoint);
				var diff = delta;
				if (lastAdded.HasValue) {
					diff = new Vector(lastAdded.Value.X - delta.X, lastAdded.Value.Y - delta.Y);
				}
				if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)) {
					if (Math.Abs(diff.X) > Math.Abs(diff.Y)) {
						delta.Y = 0;
						if (newLine.View is Polyline && ((Polyline)newLine.View).Points.Count > 1) {
							delta.Y = ((Polyline)newLine.View).Points.Reverse().Skip(1).First().Y;
						} else if (newLine.View is Polygon && ((Polygon)newLine.View).Points.Count > 1) {
							delta.Y = ((Polygon)newLine.View).Points.Reverse().Skip(1).First().Y;
						}
					} else {
						delta.X = 0;
						if (newLine.View is Polyline && ((Polyline)newLine.View).Points.Count > 1) {
							delta.X = ((Polyline)newLine.View).Points.Reverse().Skip(1).First().X;
						} else if (newLine.View is Polygon && ((Polygon)newLine.View).Points.Count > 1) {
							delta.X = ((Polygon)newLine.View).Points.Reverse().Skip(1).First().X;
						}
					}
				}
				var point = new Point(delta.X, delta.Y);
				lastAdded = point;

				if (newLine.View is Polyline)
					((Polyline)newLine.View).Points.Add(point);
				else
					((Polygon)newLine.View).Points.Add(point);
			}
			
			protected override void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
			{
				base.OnMouseDoubleClick(sender, e);
				
				if (newLine.View is Polyline) {
					((Polyline)newLine.View).Points.RemoveAt(((Polyline)newLine.View).Points.Count - 1);
					newLine.Properties[Polyline.PointsProperty].SetValue(new PointCollectionConverter().ConvertToInvariantString(((Polyline)newLine.View).Points));
				} else {
					((Polygon)newLine.View).Points.RemoveAt(((Polygon)newLine.View).Points.Count - 1);
					newLine.Properties[Polygon.PointsProperty].SetValue(new PointCollectionConverter().ConvertToInvariantString(((Polygon)newLine.View).Points));
					
				}
				
				if (changeGroup != null)
				{
					changeGroup.Commit();
					changeGroup = null;
				}
				
				Stop();
			}

			protected override void OnStopped()
			{
				if (changeGroup != null) {
					changeGroup.Abort();
					changeGroup = null;
				}
				if (services.Tool.CurrentTool is CreateComponentTool) {
					services.Tool.CurrentTool = services.Tool.PointerTool;
				}

				newLine.ReapplyAllExtensions();

				base.OnStopped();
			}
			
		}
	}
}
