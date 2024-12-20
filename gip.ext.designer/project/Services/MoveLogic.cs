﻿// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using gip.ext.design;

namespace gip.ext.designer.Services
{
	public class MoveLogic
	{
		public MoveLogic(DesignItem clickedOn)
		{
			this.clickedOn = clickedOn;
			
			selectedItems = clickedOn.Services.Selection.SelectedItems;
			if (!selectedItems.Contains(clickedOn))
				selectedItems = SharedInstances.EmptyDesignItemArray;
		}
		
		DesignItem clickedOn;
		PlacementOperation operation;
        ICollection<DesignItem> selectedItems;
		Point startPoint;

		public DesignItem ClickedOn {
			get { return clickedOn;  }
		}

		public PlacementOperation Operation {
			get { return operation;  }
		}
       
        public IDesignPanel DesignPanel
        {
			get { return clickedOn.Services.DesignPanel;  }
		}

		public void Start(Point p)
		{
			startPoint = p;
			IPlacementBehavior b = PlacementOperation.GetPlacementBehavior(selectedItems);
			if (b != null && b.CanPlace(selectedItems, PlacementType.Move, PlacementAlignment.TopLeft)) {
				List<DesignItem> sortedSelectedItems = new List<DesignItem>(selectedItems);
				sortedSelectedItems.Sort(ModelTools.ComparePositionInModelFile);
				selectedItems = sortedSelectedItems;
				operation = PlacementOperation.Start(selectedItems, PlacementType.Move);
			}
        }

		public void Move(Point p)
		{
			if (operation != null) {

				// try to switch the container
				if (operation.CurrentContainerBehavior.CanLeaveContainer(operation)) {
					ChangeContainerIfPossible(p);
				}
				
				Vector v;
				UIElement designPanel = this.DesignPanel as UIElement;
				if (operation.CurrentContainer.View != null && designPanel != null) {
					v = designPanel.TranslatePoint(p, operation.CurrentContainer.View)
						- designPanel.TranslatePoint(startPoint, operation.CurrentContainer.View);
				} else {
					v = p - startPoint;
				}

                if (!operation.CurrentContainerBehavior.CanMoveVector(operation, v))
                    return;
				foreach (PlacementInformation info in operation.PlacedItems) {
					info.Bounds = new Rect(info.OriginalBounds.Left + Math.Round(v.X, PlacementInformation.BoundsPrecision),
					                       info.OriginalBounds.Top + Math.Round(v.Y, PlacementInformation.BoundsPrecision),
					                       info.OriginalBounds.Width,
					                       info.OriginalBounds.Height);
				}
				operation.CurrentContainerBehavior.BeforeSetPosition(operation);
				foreach (PlacementInformation info in operation.PlacedItems) 
                {
					operation.CurrentContainerBehavior.SetPosition(info);
                    if (operation.CurrentDependentDrawingsBehavior != null)
                        operation.CurrentDependentDrawingsBehavior.SetPosition(info);
				}
			}
		}

		public void Stop()
		{
			if (operation != null) {
				operation.Commit();
				operation = null;
			}
		}

		public void Cancel()
		{
			if (operation != null) {
				operation.Abort();
				operation = null;
			}
		}

		// Perform hit testing on the design panel and return the first model that is not selected
		DesignPanelHitTestResult HitTestUnselectedModel(Point p)
		{
			DesignPanelHitTestResult result = DesignPanelHitTestResult.NoHit;
			ISelectionService selection = clickedOn.Services.Selection;

			DesignPanel.HitTest(p, false, true,	delegate(DesignPanelHitTestResult r) {
			                    	if (r.ModelHit == null)
			                    		return true; // continue hit testing
			                    	if (selection.IsComponentSelected(r.ModelHit))
			                    		return true; // continue hit testing
			                    	result = r;
			                    	return false; // finish hit testing
			                    });

			return result;
		}
		
		bool ChangeContainerIfPossible(Point p)
		{
			DesignPanelHitTestResult result = HitTestUnselectedModel(p);
			if (result.ModelHit == null) 
				return false;
			if (operation == null || result.ModelHit == operation.CurrentContainer) 
				return false;
			
			// check that we don't move an item into itself:
			DesignItem tmp = result.ModelHit;
			while (tmp != null) {
				if (tmp == clickedOn) return false;
				tmp = tmp.Parent;
			}
			
			IPlacementBehavior b = result.ModelHit.GetBehavior<IPlacementBehavior>();
			if (b != null && b.CanEnterContainer(operation)) {
				operation.ChangeContainer(result.ModelHit);
				return true;
			}
			return false;
		}
		
		public void HandleDoubleClick()
		{
			if (selectedItems.Count == 1) {
				IEventHandlerService ehs = clickedOn.Services.GetService<IEventHandlerService>();
				if (ehs != null) {
					DesignItemProperty defaultEvent = ehs.GetDefaultEvent(clickedOn);
					if (defaultEvent != null) {
						ehs.CreateEventHandler(defaultEvent);
					}
				}
			}
		}
	}
}
