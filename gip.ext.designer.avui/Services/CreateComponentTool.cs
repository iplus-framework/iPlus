﻿// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System.Windows;
using System;
using System.Diagnostics;
using gip.ext.design.avui.Adorners;
using gip.ext.designer.avui.Controls;
using gip.ext.design.avui;
using gip.ext.designer.avui.Xaml;
using System.Linq;
using Avalonia;
using Avalonia.Input;
using System.Collections.Generic;
using Avalonia.Input.Raw;
using Avalonia.Controls;

namespace gip.ext.designer.avui.Services
{
	/// <summary>
	/// A tool that creates a component.
	/// </summary>
	public class CreateComponentTool : ITool
	{
		readonly Type componentType;
        readonly object[] arguments = null;
        MoveLogic moveLogic;
		protected ChangeGroup ChangeGroup;
		Point createPoint;

        public event EventHandler<DesignItem> CreateComponentCompleted;

        /// <summary>
        /// Creates a new CreateComponentTool instance.
        /// </summary>
        public CreateComponentTool(Type componentType) : this(componentType, null)
        {
		}

        /// <summary>
        /// Creates a new CreateComponentTool instance.
        /// </summary>
        public CreateComponentTool(Type componentType, object[] arguments)
        {
            if (componentType == null)
                throw new ArgumentNullException(nameof(componentType));
            this.componentType = componentType;
            this.arguments = arguments;
        }


        /// <summary>
        /// Gets the type of the component to be created.
        /// </summary>
        public virtual Type ComponentType {
			get { return componentType; }
		}
		
		public Cursor Cursor {
			get { return new Cursor(StandardCursorType.Cross); }
		}
		
		public void Activate(IDesignPanel designPanel)
		{
			designPanel.PointerPressed += OnMouseDown;
			//designPanel.DragEnter += designPanel_DragOver;
			designPanel.DragOver += designPanel_DragOver;
			designPanel.Drop += designPanel_Drop;
			designPanel.DragLeave += designPanel_DragLeave;
		}
		
		public void Deactivate(IDesignPanel designPanel)
		{
			designPanel.PointerPressed -= OnMouseDown;
			//designPanel.DragEnter -= designPanel_DragOver;
			designPanel.DragOver -= designPanel_DragOver;
			designPanel.Drop -= designPanel_Drop;
			designPanel.DragLeave -= designPanel_DragLeave;
		}

		protected virtual void designPanel_DragOver(object sender, DragEventArgs e)
		{
			try {
				IDesignPanel designPanel = (IDesignPanel)sender;
				e.DragEffects = DragDropEffects.Copy;
				e.Handled = true;
				Point p = e.GetPosition(designPanel as Visual);

				if (moveLogic == null) {
                    //if (e.Data.GetData(this.GetType()) != this) return;
                    //if (e.Data.GetData(typeof(CreateComponentTool)) != this) return;
                    // TODO: dropLayer in designPanel
                    designPanel.IsAdornerLayerHitTestVisible = false;
					DesignPanelHitTestResult result = designPanel.HitTest(p, false, true, HitTestType.Default);
					
					if (result.ModelHit != null) {
						designPanel.Focus();
						DesignItem createdItem = CreateItem(designPanel.Context);
                        Size defaultSize = OnGetDefaultSize();
                        if (AddItemWithDefaultSize(result.ModelHit, createdItem, e.GetPosition(result.ModelHit.View), defaultSize))
                        {
							moveLogic = new MoveLogic(createdItem);
							createPoint = p;
							double x = createPoint.X;
							double y = createPoint.Y;
                            // Offset Mouse-Pointer, damit DesignItem nicht flackert auf Oberfläche, wegen wechsel mit Resize-Adorner
                            if (!Double.IsNaN(defaultSize.Height) && !Double.IsNaN(defaultSize.Width)
                                && (defaultSize.Height > 0) && (defaultSize.Width > 0))
                            {
                                x += (defaultSize.Width / 2);
                                y += (defaultSize.Height / 2);
                            }
                            else
                            {
                                x += 10;
                                y += 10;
                            }
							createPoint = new Point(x, y);
                            // We'll keep the ChangeGroup open as long as the moveLogic is active.
                        } else {
							// Abort the ChangeGroup created by the CreateItem() call.
							ChangeGroup.Abort();
                            ChangeGroup = null;
						}
					}
				} else if ((moveLogic.ClickedOn.View as Control).IsLoaded) {
					if (moveLogic.Operation == null) {
						moveLogic.Start(createPoint);
					} else {
						moveLogic.Move(p);
					}
				}
			} catch (Exception x) {
				DragDropExceptionHandler.RaiseUnhandledException(x);
			}
		}

        protected virtual void designPanel_Drop(object sender, DragEventArgs e)
		{
			try {
				if (moveLogic != null) {
					moveLogic.Stop();
					if (moveLogic.ClickedOn.Services.Tool.CurrentTool is CreateComponentTool) {
						moveLogic.ClickedOn.Services.Tool.CurrentTool = moveLogic.ClickedOn.Services.Tool.PointerTool;
					}
					moveLogic.DesignPanel.IsAdornerLayerHitTestVisible = true;
					moveLogic = null;
                    ChangeGroup.Commit();
                    ChangeGroup = null;
				}
			} catch (Exception x) {
				DragDropExceptionHandler.RaiseUnhandledException(x);
			}
		}

        protected virtual void designPanel_DragLeave(object sender, DragEventArgs e)
		{
			try {
				if (moveLogic != null) {
					moveLogic.Cancel();
					moveLogic.ClickedOn.Services.Selection.SetSelectedComponents(null);
					moveLogic.DesignPanel.IsAdornerLayerHitTestVisible = true;
					moveLogic = null;
					ChangeGroup.Abort();
                    ChangeGroup = null;

				}
			} catch (Exception x) {
				DragDropExceptionHandler.RaiseUnhandledException(x);
			}
		}

        /// <summary>
        /// Is called to create the item used by the CreateComponentTool.
        /// </summary>
        protected virtual DesignItem CreateItemWithPosition(DesignContext context, Point position)
        {
            var item = CreateItem(context);
            item.Position = position;
            return item;
        }

        /// <summary>
        /// Is called to create the item used by the CreateComponentTool.
        /// </summary>
        protected virtual DesignItem CreateItem(DesignContext context)
        {
            if (ChangeGroup == null)
                ChangeGroup = context.RootItem.OpenGroup("Add Control");

            var item = CreateItem(context, componentType, arguments);

            return item;
        }

        protected virtual DesignItem[] CreateItemsWithPosition(DesignContext context, Point position)
        {
            var items = CreateItems(context);
            if (items != null)
            {
                foreach (var designItem in items)
                {
                    designItem.Position = position;
                }
            }

            return items;
        }

        protected virtual DesignItem[] CreateItems(DesignContext context)
        {
            return null;
        }

        /// <summary>
        /// Is called to create the item used by the CreateComponentTool.
        /// </summary>
        public static DesignItem CreateItem(DesignContext context, Type type)
        {
            return CreateItem(context, type, null);
        }

        /// <summary>
        /// Is called to create the item used by the CreateComponentTool.
        /// </summary>
        public static DesignItem CreateItem(DesignContext context, Type type, object[] arguments)
		{
			object newInstance = context.Services.ExtensionManager.CreateInstanceWithCustomInstanceFactory(type, null);
			DesignItem item = context.Services.Component.RegisterComponentForDesigner(newInstance);
            context.Services.Component.SetDefaultPropertyValues(item);
            context.Services.ExtensionManager.ApplyDefaultInitializers(item);
			return item;
		}
		
		internal static bool AddItemWithDefaultSize(DesignItem container, DesignItem createdItem, Point position, Size suggestedSize)
		{
            var size = suggestedSize;
            if (double.IsNaN(size.Width) || double.IsNaN(size.Height))
                size = ModelTools.GetDefaultSize(createdItem);
			PlacementOperation operation = PlacementOperation.TryStartInsertNewComponents(
				container,
				new DesignItem[] { createdItem },
				new Rect[] { new Rect(position, size).Round() },
				PlacementType.AddItem
			);
			if (operation != null) {
				container.Services.Selection.SetSelectedComponents(new DesignItem[] { createdItem });
				operation.Commit();
				return true;
			} else {
				return false;
			}
		}

        /// <summary>
        /// Is called to set Properties of the Drawn Item
        /// </summary>
        protected virtual void SetPropertiesForDrawnItem(DesignItem designItem)
        { }

        public static bool AddItemWithCustomSizePosition(DesignItem container, Type createdItem, Size size, Point position)
        {
            return AddItemWithCustomSizePosition(container, createdItem, null, size, position);
        }

        public static bool AddItemWithCustomSizePosition(DesignItem container, Type createdItem, object[] arguments, Size size, Point position)
        {
            CreateComponentTool cct = new CreateComponentTool(createdItem, arguments);
            return AddItemsWithCustomSize(container, new[] { cct.CreateItem(container.Context) }, new List<Rect> { new Rect(position, size) });
        }

        public static bool AddItemWithDefaultSize(DesignItem container, Type createdItem, Size size)
        {
            return AddItemWithDefaultSize(container, createdItem, null, size);
        }

        public static bool AddItemWithDefaultSize(DesignItem container, Type createdItem, object[] arguments, Size size)
        {
            CreateComponentTool cct = new CreateComponentTool(createdItem, arguments);
            return AddItemsWithCustomSize(container, new[] { cct.CreateItem(container.Context) }, new List<Rect> { new Rect(new Point(0, 0), size) });
        }

        internal static bool AddItemsWithDefaultSize(DesignItem container, DesignItem[] createdItems)
        {
            return AddItemsWithCustomSize(container, createdItems, createdItems.Select(x => new Rect(x.Position, ModelTools.GetDefaultSize(x))).ToList());
        }

        internal static bool AddItemsWithCustomSize(DesignItem container, DesignItem[] createdItems, IList<Rect> positions)
        {
            PlacementOperation operation = null;

            while (operation == null && container != null)
            {
                operation = PlacementOperation.TryStartInsertNewComponents(
                    container,
                    createdItems,
                    positions,
                    PlacementType.AddItem
                );

                if (operation != null)
                    break;

                try
                {
                    if (container.Parent != null)
                    {
                        var rel = container.View.TranslatePoint(new Point(0, 0), container.Parent.View).Value;
                        for (var index = 0; index < positions.Count; index++)
                        {
                            positions[index] = new Rect(new Point(positions[index].X + rel.X, positions[index].Y + rel.Y), positions[index].Size);
                        }
                    }
                }
                catch (Exception)
                { }

                container = container.Parent;
            }

            if (operation != null)
            {
                container.Services.Selection.SetSelectedComponents(createdItems);
                operation.Commit();
                return true;
            }
            else
            {
                return false;
            }
        }


        public virtual Size OnGetDefaultSize()
        {
            return new Size(double.NaN, double.NaN);
        }
		
		void OnMouseDown(object sender, PointerPressedEventArgs e)
		{
            MouseDown(sender, e);
		}

        protected virtual void MouseDown(object sender, PointerPressedEventArgs e)
        {
            if (e.Properties.IsLeftButtonPressed && MouseGestureBase.IsOnlyButtonPressed(e, RawPointerEventType.LeftButtonDown))
            {
                e.Handled = true;
                IDesignPanel designPanel = (IDesignPanel)sender;
                DesignPanelHitTestResult result = designPanel.HitTest(e.GetPosition(designPanel as Visual), false, true);
                if (result.ModelHit != null)
                {
                    var darwItemBehaviors = result.ModelHit.Extensions.OfType<IDrawItemExtension>();
                    var drawItembehavior = darwItemBehaviors.FirstOrDefault(x => x.CanItemBeDrawn(componentType));
					if (drawItembehavior != null && drawItembehavior.CanItemBeDrawn(componentType))
					{
						drawItembehavior.StartDrawItem(result.ModelHit, componentType, designPanel, e, SetPropertiesForDrawnItem);
					}
					else
					{
						IPlacementBehavior behavior = result.ModelHit.GetBehavior<IPlacementBehavior>();
						if (behavior != null)
						{
							DesignItem createdItem = CreateItem(designPanel.Context);

                            CreateComponentCompleted?.Invoke(this, createdItem);

                            new CreateComponentMouseGesture(result.ModelHit, createdItem, ChangeGroup).Start(designPanel, e);
							// CreateComponentMouseGesture now is responsible for the changeGroup created by CreateItem()
							ChangeGroup = null;
						}
					}
                }
            }
        }
	}

    [CLSCompliant(false)]
    sealed public class CreateComponentMouseGesture : ClickOrDragMouseGesture
	{
		DesignItem createdItem;
		PlacementOperation operation;
		DesignItem container;
		ChangeGroup changeGroup;
		
		public CreateComponentMouseGesture(DesignItem clickedOn, DesignItem createdItem, ChangeGroup changeGroup)
		{
			this.container = clickedOn;
			this.createdItem = createdItem;
			this.positionRelativeTo = clickedOn.View;
			this.changeGroup = changeGroup;
		}
		
//		GrayOutDesignerExceptActiveArea grayOut;
//		SelectionFrame frame;
//		AdornerPanel adornerPanel;
		
		Rect GetStartToEndRect(PointerEventArgs e)
		{
			Point endPoint = e.GetPosition(positionRelativeTo as Visual);
			return new Rect(
				Math.Min(startPoint.X, endPoint.X),
				Math.Min(startPoint.Y, endPoint.Y),
				Math.Abs(startPoint.X - endPoint.X),
				Math.Abs(startPoint.Y - endPoint.Y)
			);
		}
		
		protected override void OnDragStarted(PointerEventArgs e)
		{
			operation = PlacementOperation.TryStartInsertNewComponents(container,
			                                                           new DesignItem[] { createdItem },
			                                                           new Rect[] { GetStartToEndRect(e).Round() },
			                                                           PlacementType.Resize);
			if (operation != null) {
				services.Selection.SetSelectedComponents(new DesignItem[] { createdItem });
			}
		}
		
		protected override void OnMouseMove(object sender, PointerEventArgs e)
		{
			base.OnMouseMove(sender, e);
			if (operation != null) {
				foreach (PlacementInformation info in operation.PlacedItems) {
					info.Bounds = GetStartToEndRect(e).Round();
					operation.CurrentContainerBehavior.SetPosition(info);
				}
			}
		}
		
		protected override void OnMouseUp(object sender, PointerReleasedEventArgs e)
		{
			if (_HasDragStarted) 
            {
				if (operation != null) {
					operation.Commit();
					operation = null;
				}
			} 
            else 
            {
                Size suggestedSize = new Size(double.NaN, double.NaN);
                if ((container.Services != null) 
                    && (container.Services.Tool != null) 
                    && (container.Services.Tool.CurrentTool != null)
                    && (container.Services.Tool.CurrentTool is CreateComponentTool))
                    suggestedSize = (container.Services.Tool.CurrentTool as CreateComponentTool).OnGetDefaultSize();
                CreateComponentTool.AddItemWithDefaultSize(container, createdItem, e.GetPosition(positionRelativeTo as Visual), suggestedSize);
			}
			if (changeGroup != null) {
				changeGroup.Commit();
				changeGroup = null;
			}

            if (designPanel.Context.Services.Component is XamlComponentService)
            {
                ((XamlComponentService)designPanel.Context.Services.Component).RaiseComponentRegisteredAndAddedToContainer(createdItem);
            }

            base.OnMouseUp(sender, e);
		}
		
		protected override void OnStopped()
		{
			if (operation != null) {
				operation.Abort();
				operation = null;
			}
			if (changeGroup != null) {
				changeGroup.Abort();
				changeGroup = null;
			}
			if (services.Tool.CurrentTool is CreateComponentTool) {
				services.Tool.CurrentTool = services.Tool.PointerTool;
			}
			base.OnStopped();
		}
	}
}
