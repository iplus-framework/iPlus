// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
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

		public void StartDrawItem(DesignItem clickedOn, Type createItemType, IDesignPanel panel, PointerEventArgs e, Action<DesignItem> drawItemCallback)
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
			
			new DrawPolylineMouseGesture(e, createdItem, clickedOn.View, changeGroup, this.ExtendedItem.GetCompleteAppliedTransformationToView()).Start(panel, e);
		}

		#endregion
		
		sealed class DrawPolylineMouseGesture : ClickOrDragMouseGesture
		{
			private ChangeGroup changeGroup;
			private DesignItem newLine;
			private new Point startPoint;
			private Point? lastAdded;
			private Matrix matrix;

			public DrawPolylineMouseGesture(PointerEventArgs e, DesignItem newLine, IInputElement relativeTo, ChangeGroup changeGroup, Transform transform)
			{
				this.newLine = newLine;
				this.positionRelativeTo = relativeTo;
				this.changeGroup = changeGroup;
				this.matrix = transform.Value;
				matrix.Invert();

                startPoint = e.GetCurrentPoint(null).Position;
                //startPoint = Mouse.GetPosition(null);
			}
			
			protected override void OnPreviewMouseLeftButtonDown(object sender, PointerPressedEventArgs e)
			{
				e.Handled = true;
				base.OnPreviewMouseLeftButtonDown(sender, e);
			}
			
			protected override void OnMouseMove(object sender, PointerEventArgs e)
			{
				if (changeGroup == null)
					return;
				var delta = matrix.Transform(e.GetPosition(null) - startPoint);
				var diff = delta;
                if (lastAdded.HasValue) 
				{
                    Vector displacement = new Vector(lastAdded.Value.X - delta.X, lastAdded.Value.Y - delta.Y);
                    diff = new Point(displacement.X, displacement.Y);
				}
				if (e.KeyModifiers.HasFlag(KeyModifiers.Alt))
				{
                    if (Math.Abs(diff.X) > Math.Abs(diff.Y))
                    {
                        var newY = 0.0;
                        if (newLine.View is Polyline && ((Polyline)newLine.View).Points.Count > 1)
                        {
                            newY = ((Polyline)newLine.View).Points.Reverse().Skip(1).First().Y;
                        }
                        else if (newLine.View is Polygon && ((Polygon)newLine.View).Points.Count > 1)
                        {
                            newY = ((Polygon)newLine.View).Points.Reverse().Skip(1).First().Y;
                        }
                        delta = new Point(delta.X, newY);
                    }
                    else
                    {
                        var newX = 0.0;
                        if (newLine.View is Polyline && ((Polyline)newLine.View).Points.Count > 1)
                        {
                            newX = ((Polyline)newLine.View).Points.Reverse().Skip(1).First().X;
                        }
                        else if (newLine.View is Polygon && ((Polygon)newLine.View).Points.Count > 1)
                        {
                            newX = ((Polygon)newLine.View).Points.Reverse().Skip(1).First().X;
                        }
                        delta = new Point(newX, delta.Y);
                    }

				}
				var point = new Point(delta.X, delta.Y);

				if (newLine.View is Polyline) 
				{
					if (((Polyline)newLine.View).Points.Count <= 1)
						((Polyline)newLine.View).Points.Add(point);
					if (!e.Properties.IsLeftButtonPressed)
						((Polyline)newLine.View).Points.RemoveAt(((Polyline)newLine.View).Points.Count - 1);
					if (((Polyline)newLine.View).Points.Last() != point)
						((Polyline)newLine.View).Points.Add(point);
				} 
				else 
				{
					if (((Polygon)newLine.View).Points.Count <= 1)
						((Polygon)newLine.View).Points.Add(point);
					if (!e.Properties.IsLeftButtonPressed)
						((Polygon)newLine.View).Points.RemoveAt(((Polygon)newLine.View).Points.Count - 1);
					if (((Polygon)newLine.View).Points.Last() != point)
						((Polygon)newLine.View).Points.Add(point);
				}
			}
			
			protected override void OnMouseUp(object sender, PointerReleasedEventArgs e)
			{
				if (changeGroup == null)
					return;

				var delta = matrix.Transform(e.GetPosition(null) - startPoint);
				var diff = delta;
				if (lastAdded.HasValue) 
				{
                    Vector displacement = new Vector(lastAdded.Value.X - delta.X, lastAdded.Value.Y - delta.Y);
                    diff = new Point(displacement.X, displacement.Y);
                }
                if (e.KeyModifiers.HasFlag(KeyModifiers.Alt)) 
				{
                    if (Math.Abs(diff.X) > Math.Abs(diff.Y))
                    {
                        var newY = 0.0;
                        if (newLine.View is Polyline && ((Polyline)newLine.View).Points.Count > 1)
                        {
                            newY = ((Polyline)newLine.View).Points.Reverse().Skip(1).First().Y;
                        }
                        else if (newLine.View is Polygon && ((Polygon)newLine.View).Points.Count > 1)
                        {
                            newY = ((Polygon)newLine.View).Points.Reverse().Skip(1).First().Y;
                        }
                        delta = new Point(delta.X, newY);
                    }
                    else
                    {
                        var newX = 0.0;
                        if (newLine.View is Polyline && ((Polyline)newLine.View).Points.Count > 1)
                        {
                            newX = ((Polyline)newLine.View).Points.Reverse().Skip(1).First().X;
                        }
                        else if (newLine.View is Polygon && ((Polygon)newLine.View).Points.Count > 1)
                        {
                            newX = ((Polygon)newLine.View).Points.Reverse().Skip(1).First().X;
                        }
                        delta = new Point(newX, delta.Y);
                    }
				}
				var point = new Point(delta.X, delta.Y);
				lastAdded = point;

				if (newLine.View is Polyline)
					((Polyline)newLine.View).Points.Add(point);
				else
					((Polygon)newLine.View).Points.Add(point);
			}
			
			protected override void OnMouseDoubleClick(object sender, PointerPressedEventArgs e)
            {
				base.OnMouseDoubleClick(sender, e);
				((Polyline)newLine.View).ToString();


                if (newLine.View is Polyline) 
				{
					((Polyline)newLine.View).Points.RemoveAt(((Polyline)newLine.View).Points.Count - 1);
                    var pointsString = string.Join(" ", ((Polyline)newLine.View).Points.Select(p => $"{p.X},{p.Y}"));
					newLine.Properties[Polyline.PointsProperty].SetValue(pointsString);
                    //newLine.Properties[Polyline.PointsProperty].SetValue(new PointCollectionConverter().ConvertToInvariantString(((Polyline)newLine.View).Points));
                } 
				else 
				{
					((Polygon)newLine.View).Points.RemoveAt(((Polygon)newLine.View).Points.Count - 1);
                    var pointsString = string.Join(" ", ((Polygon)newLine.View).Points.Select(p => $"{p.X},{p.Y}"));
                    newLine.Properties[Polygon.PointsProperty].SetValue(pointsString);
                    //newLine.Properties[Polygon.PointsProperty].SetValue(new PointCollectionConverter().ConvertToInvariantString(((Polygon)newLine.View).Points));

                }
				
				if (changeGroup != null)
				{
					changeGroup.Commit();
					changeGroup = null;
				}
				
				Stop(e);
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
