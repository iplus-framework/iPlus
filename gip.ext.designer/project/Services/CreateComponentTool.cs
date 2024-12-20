﻿// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System.Windows;
using System;
using System.Diagnostics;
using System.Windows.Input;
using gip.ext.design.Adorners;
using gip.ext.designer.Controls;
using gip.ext.design;

namespace gip.ext.designer.Services
{
	/// <summary>
	/// A tool that creates a component.
	/// </summary>
	public class CreateComponentTool : ITool
	{
		readonly Type componentType;
		MoveLogic moveLogic;
		protected ChangeGroup changeGroup;
		Point createPoint;
		
		/// <summary>
		/// Creates a new CreateComponentTool instance.
		/// </summary>
		public CreateComponentTool(Type componentType)
		{
			if (componentType == null)
				throw new ArgumentNullException("componentType");
			this.componentType = componentType;
		}
		
		/// <summary>
		/// Gets the type of the component to be created.
		/// </summary>
		public virtual Type ComponentType {
			get { return componentType; }
		}
		
		public Cursor Cursor {
			get { return Cursors.Cross; }
		}
		
		public void Activate(IDesignPanel designPanel)
		{
			designPanel.MouseDown += OnMouseDown;
			//designPanel.DragEnter += designPanel_DragOver;
			designPanel.DragOver += designPanel_DragOver;
			designPanel.Drop += designPanel_Drop;
			designPanel.DragLeave += designPanel_DragLeave;
		}
		
		public void Deactivate(IDesignPanel designPanel)
		{
			designPanel.MouseDown -= OnMouseDown;
			//designPanel.DragEnter -= designPanel_DragOver;
			designPanel.DragOver -= designPanel_DragOver;
			designPanel.Drop -= designPanel_Drop;
			designPanel.DragLeave -= designPanel_DragLeave;
		}

		protected virtual void designPanel_DragOver(object sender, DragEventArgs e)
		{
			try {
				IDesignPanel designPanel = (IDesignPanel)sender;
				e.Effects = DragDropEffects.Copy;
				e.Handled = true;
				Point p = e.GetPosition(designPanel);

				if (moveLogic == null) {
					//if (e.Data.GetData(typeof(CreateComponentTool)) != this) return;
					// TODO: dropLayer in designPanel
					designPanel.IsAdornerLayerHitTestVisible = false;
					DesignPanelHitTestResult result = designPanel.HitTest(p, false, true);
					
					if (result.ModelHit != null) {
						designPanel.Focus();
						DesignItem createdItem = CreateItem(designPanel.Context);
                        Size defaultSize = OnGetDefaultSize();
                        if (AddItemWithDefaultSize(result.ModelHit, createdItem, e.GetPosition(result.ModelHit.View), defaultSize))
                        {
							moveLogic = new MoveLogic(createdItem);
							createPoint = p;
                            // Offset Mouse-Pointer, damit DesignItem nicht flackert auf Oberfläche, wegen wechsel mit Resize-Adorner
                            if (!Double.IsNaN(defaultSize.Height) && !Double.IsNaN(defaultSize.Width)
                                && (defaultSize.Height > 0) && (defaultSize.Width > 0))
                            {
                                createPoint.X += (defaultSize.Width / 2);
                                createPoint.Y += (defaultSize.Height / 2);
                            }
                            else
                            {
                                createPoint.X += 10;
                                createPoint.Y += 10;
                            }
                            // We'll keep the ChangeGroup open as long as the moveLogic is active.
						} else {
							// Abort the ChangeGroup created by the CreateItem() call.
							changeGroup.Abort();
                            changeGroup = null;
						}
					}
				} else if ((moveLogic.ClickedOn.View as FrameworkElement).IsLoaded) {
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
					changeGroup.Commit();
                    changeGroup = null;
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
					changeGroup.Abort();
                    changeGroup = null;

				}
			} catch (Exception x) {
				DragDropExceptionHandler.RaiseUnhandledException(x);
			}
		}
		
		/// <summary>
		/// Is called to create the item used by the CreateComponentTool.
		/// </summary>
		protected virtual DesignItem CreateItem(DesignContext context)
		{
			object newInstance = context.Services.ExtensionManager.CreateInstanceWithCustomInstanceFactory(componentType, null);
			DesignItem item = context.Services.Component.RegisterComponentForDesigner(newInstance);
			changeGroup = item.OpenGroup("Drop Control");
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

        public virtual Size OnGetDefaultSize()
        {
            return new Size(double.NaN, double.NaN);
        }
		
		void OnMouseDown(object sender, MouseButtonEventArgs e)
		{
            MouseDown(sender, e);
		}

        protected virtual void MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && MouseGestureBase.IsOnlyButtonPressed(e, MouseButton.Left))
            {
                e.Handled = true;
                IDesignPanel designPanel = (IDesignPanel)sender;
                DesignPanelHitTestResult result = designPanel.HitTest(e.GetPosition(designPanel), false, true);
                if (result.ModelHit != null)
                {
                    IPlacementBehavior behavior = result.ModelHit.GetBehavior<IPlacementBehavior>();
                    if (behavior != null)
                    {
                        DesignItem createdItem = CreateItem(designPanel.Context);

                        new CreateComponentMouseGesture(result.ModelHit, createdItem, changeGroup).Start(designPanel, e);
                        // CreateComponentMouseGesture now is responsible for the changeGroup created by CreateItem()
                        changeGroup = null;
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
		
		Rect GetStartToEndRect(MouseEventArgs e)
		{
			Point endPoint = e.GetPosition(positionRelativeTo);
			return new Rect(
				Math.Min(startPoint.X, endPoint.X),
				Math.Min(startPoint.Y, endPoint.Y),
				Math.Abs(startPoint.X - endPoint.X),
				Math.Abs(startPoint.Y - endPoint.Y)
			);
		}
		
		protected override void OnDragStarted(MouseEventArgs e)
		{
			operation = PlacementOperation.TryStartInsertNewComponents(container,
			                                                           new DesignItem[] { createdItem },
			                                                           new Rect[] { GetStartToEndRect(e).Round() },
			                                                           PlacementType.Resize);
			if (operation != null) {
				services.Selection.SetSelectedComponents(new DesignItem[] { createdItem });
			}
		}
		
		protected override void OnMouseMove(object sender, MouseEventArgs e)
		{
			base.OnMouseMove(sender, e);
			if (operation != null) {
				foreach (PlacementInformation info in operation.PlacedItems) {
					info.Bounds = GetStartToEndRect(e).Round();
					operation.CurrentContainerBehavior.SetPosition(info);
				}
			}
		}
		
		protected override void OnMouseUp(object sender, MouseButtonEventArgs e)
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
                CreateComponentTool.AddItemWithDefaultSize(container, createdItem, e.GetPosition(positionRelativeTo), suggestedSize);
			}
			if (changeGroup != null) {
				changeGroup.Commit();
				changeGroup = null;
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
