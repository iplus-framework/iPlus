// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
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
	public class DrawPathExtension : BehaviorExtension, IDrawItemExtension
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
			return createItemType == typeof(Path);
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

			var figure = new PathFigure();
			var geometry = new PathGeometry();
			var geometryDesignItem = createdItem.Services.Component.RegisterComponentForDesigner(geometry);
			var figureDesignItem = createdItem.Services.Component.RegisterComponentForDesigner(figure);
			createdItem.Properties[Path.DataProperty].SetValue(geometry);
			//geometryDesignItem.Properties[PathGeometry.FiguresProperty].CollectionElements.Add(figureDesignItem);
			figureDesignItem.Properties[PathFigure.StartPointProperty].SetValue(new Point(0,0));
			
			new DrawPathMouseGesture(figure, createdItem, clickedOn.View, changeGroup, this.ExtendedItem.GetCompleteAppliedTransformationToView()).Start(panel, (MouseButtonEventArgs) e);
		}

		#endregion
		
		sealed class DrawPathMouseGesture : ClickOrDragMouseGesture
		{
			private ChangeGroup changeGroup;
			private DesignItem newLine;
			private Point sP;
			private PathFigure figure;
			private DesignItem geometry;
			private Matrix matrix;

			public DrawPathMouseGesture(PathFigure figure, DesignItem newLine, IInputElement relativeTo, ChangeGroup changeGroup, Transform transform)
			{
				this.newLine = newLine;
				this.positionRelativeTo = relativeTo;
				this.changeGroup = changeGroup;
				this.figure = figure;
				this.matrix = transform.Value;
				matrix.Invert();
				
				sP = Mouse.GetPosition(null);
				
				geometry = newLine.Properties[Path.DataProperty].Value;
			}
			
			protected override void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
			{
				e.Handled = true;
				base.OnPreviewMouseLeftButtonDown(sender, e);
			}
			
			protected override void OnMouseMove(object sender, MouseEventArgs e)
			{
				var delta = matrix.Transform(e.GetPosition(null) - sP);
				var point = new Point(Math.Round(delta.X, 0), Math.Round(delta.Y, 0));

				var segment = figure.Segments.LastOrDefault() as LineSegment;
				if (Mouse.LeftButton == MouseButtonState.Pressed)
				{
					if (segment == null || segment.Point != point)
					{
						figure.Segments.Add(new LineSegment(point, false));
						segment = figure.Segments.Last() as LineSegment;}
				}
					
				segment.Point = point;
				var prop = geometry.Properties[PathGeometry.FiguresProperty];
				prop.SetValue(prop.TypeConverter.ConvertToInvariantString(figure));
			}
			
			protected override void OnMouseUp(object sender, MouseButtonEventArgs e)
			{
				var delta = matrix.Transform(e.GetPosition(null) - sP);
				var point = new Point(Math.Round(delta.X, 0), Math.Round(delta.Y,0));
				
				figure.Segments.Add(new LineSegment(point, false));
				var prop = geometry.Properties[PathGeometry.FiguresProperty];
				prop.SetValue(prop.TypeConverter.ConvertToInvariantString(figure));
			}
			
			protected override void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
			{
				base.OnMouseDoubleClick(sender, e);
				
				figure.Segments.RemoveAt(figure.Segments.Count - 1);
				var prop = geometry.Properties[PathGeometry.FiguresProperty];
				prop.SetValue(prop.TypeConverter.ConvertToInvariantString(figure));
				
				if (changeGroup != null) {
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
