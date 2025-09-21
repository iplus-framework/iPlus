// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using Avalonia.Input;
using gip.ext.design.avui;
using System.Linq;
using Avalonia;
using Avalonia.Input.Raw;

namespace gip.ext.designer.avui.Services
{
	public sealed class PointerTool : ITool
	{
		public static readonly PointerTool Instance = new PointerTool();
		
		public Cursor Cursor {
			get { return null; }
		}
		
		public void Activate(IDesignPanel designPanel)
		{
			designPanel.PointerPressed += OnMouseDown;
            if (designPanel is InputElement ie)
                ie.Cursor = Cursor;
        }
		
		public void Deactivate(IDesignPanel designPanel)
		{
			designPanel.PointerPressed -= OnMouseDown;
            if (designPanel is InputElement ie)
                ie.Cursor = null;
        }
		
		void OnMouseDown(object sender, PointerPressedEventArgs e)
		{
			IDesignPanel designPanel = (IDesignPanel)sender;
			DesignPanelHitTestResult result = designPanel.HitTest(e.GetPosition(designPanel as Visual), false, true, HitTestType.ElementSelection);
			if (result.ModelHit != null) {
				IHandlePointerToolMouseDown b = result.ModelHit.GetBehavior<IHandlePointerToolMouseDown>();
				if (b != null) {
					b.HandleSelectionMouseDown(designPanel, e, result);
				}
                if (!e.Handled)
                {
                    if (e.Properties.IsLeftButtonPressed && MouseGestureBase.IsOnlyButtonPressed(e, RawPointerEventType.LeftButtonDown))
                    {
                        e.Handled = true;
                        DesignItem itemToDrag = null;
                        ISelectionService selectionService = designPanel.Context.Services.Selection;
                        // If DesignItem selected over DesignItemTreeView, test if HitTest matches the tempoerary Selection
                        if (selectionService.TemporarySelectionFromTreeView != null)
                        {
                            var ancestors = result.ModelHit.View.GetVisualAncestors();
                            if (ancestors.Where(c => c == selectionService.TemporarySelectionFromTreeView.View).Any())
                                itemToDrag = selectionService.TemporarySelectionFromTreeView;
                            //else
                                selectionService.TemporarySelectionFromTreeView = null;
                        }
                        bool setSelectionIfNotMoving = false;
                        if (itemToDrag == null)
                        {
                            if (selectionService.IsComponentSelected(result.ModelHit))
                            {
                                setSelectionIfNotMoving = true;
                                // There might be multiple components selected. We might have
                                // to set the selection to only the item clicked on
                                // (or deselect the item clicked on if Ctrl is pressed),
                                // but we should do so only if the user isn't performing a drag operation.
                            }
                            else
                            {
                                itemToDrag = result.ModelHit;
                                selectionService.SetSelectedComponents(new DesignItem[] { itemToDrag }, SelectionTypes.Auto);
                            }
                        }

                        //ISelectionService dependentDrawingService = designPanel.Context.Services.DependentDrawingsService;
                        //dependentDrawingService.SetSelectedComponents(new DesignItem[] { result.ModelHit }, SelectionTypes.Auto);
                        if (selectionService.IsComponentSelected(itemToDrag))
                        {
                            new DragMoveMouseGesture(itemToDrag, e.ClickCount == 2, setSelectionIfNotMoving).Start(designPanel, e);
                        }
                    }
                }
			}
		}
	}
}
