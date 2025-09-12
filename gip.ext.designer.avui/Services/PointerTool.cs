// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using Avalonia.Input;
using gip.ext.design.avui;
using System.Linq;

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
			designPanel.MouseDown += OnMouseDown;
		}
		
		public void Deactivate(IDesignPanel designPanel)
		{
			designPanel.MouseDown -= OnMouseDown;
		}
		
		void OnMouseDown(object sender, PointerEventArgs e)
		{
			IDesignPanel designPanel = (IDesignPanel)sender;
			DesignPanelHitTestResult result = designPanel.HitTest(e.GetPosition(designPanel), false, true);
			if (result.ModelHit != null) {
				IHandlePointerToolMouseDown b = result.ModelHit.GetBehavior<IHandlePointerToolMouseDown>();
				if (b != null) {
					b.HandleSelectionMouseDown(designPanel, e, result);
				}
                if (!e.Handled)
                {
                    if (e.ChangedButton == MouseButton.Left && MouseGestureBase.IsOnlyButtonPressed(e, MouseButton.Left))
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
                        if (itemToDrag == null)
                        {
                            itemToDrag = result.ModelHit;
                            selectionService.SetSelectedComponents(new DesignItem[] { itemToDrag }, SelectionTypes.Auto);
                        }

                        //ISelectionService dependentDrawingService = designPanel.Context.Services.DependentDrawingsService;
                        //dependentDrawingService.SetSelectedComponents(new DesignItem[] { result.ModelHit }, SelectionTypes.Auto);
                        if (selectionService.IsComponentSelected(itemToDrag))
                        {
                            new DragMoveMouseGesture(itemToDrag, e.ClickCount == 2).Start(designPanel, e);
                        }
                    }
                }
			}
		}
	}
}
