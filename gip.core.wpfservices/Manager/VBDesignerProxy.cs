using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using gip.core.datamodel;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Markup;
using gip.ext.design;
using gip.core.layoutengine;
using gip.ext.designer.Controls;
using gip.core.manager;
using static gip.core.manager.VBDesigner;
using System.Windows.Media;
using gip.ext.designer;
using System.ComponentModel;

namespace gip.core.wpfservices
{
    public abstract class VBDesignerProxy : IVBComponentDesignManagerProxy
    {
        public VBDesignerProxy(IACComponent component)
        {
            _DesignerComp = component;
        }

        #region properties

        private IACComponent _DesignerComp;
        public T Designer<T>() where T : VBDesigner
        {
            if (_DesignerComp == null)
                return null;
            return _DesignerComp as T;
        }

        /*
        public enum LayoutActionType : short
        {
            Insert = 0,
            Delete = 1,
            Move = 2,
            InsertEdge = 3,
            DeleteEdge = 4,
        }

        public class VisualInfo
        {
            public LayoutActionType LayoutAction { get; set; }
            public IACObject VisualObject { get; set; }
            public string ACUrl { get; set; }
            public string ACUrl2 { get; set; }
            public Rect Position { get; set; }
        }

        List<VisualInfo> _VisualChangeList = new List<VisualInfo>();
        public List<VisualInfo> VisualChangeList
        {
            get
            {
                return _VisualChangeList;
            }
        }

        public void AddToVisualChangeList(IACObject visualObject, LayoutActionType layoutAction = LayoutActionType.Insert, string acUrl = "", string acUrl2 = "", Rect position = new Rect())
        {
            _VisualChangeList.Add(new VisualInfo { VisualObject = visualObject, LayoutAction = layoutAction, ACUrl = acUrl, ACUrl2 = acUrl2, Position = position });
        }
        */
        #endregion

        public abstract void UpdateVisual();

        public IACInteractiveObject GetVBDesignEditor(IACComponent component)
        {
            VBDesigner vbDesigner = component as VBDesigner;
            if (vbDesigner == null)
                return null;
            if (vbDesigner.VBDesign != null)
            {
                if ((vbDesigner.VBDesign.Content != null) && (vbDesigner.VBDesign.Content is VBDesignEditor))
                    return (vbDesigner.VBDesign.Content as VBDesignEditor);
            }
            if (vbDesigner.ParentACComponent != null && vbDesigner.ParentACComponent.ReferencePoint != null)
                return vbDesigner.ParentACComponent.ReferencePoint.ConnectionList.Where(c => c is VBDesignEditor).FirstOrDefault() as IACInteractiveObject;
            return null;
        }

        internal static bool AddItemWithDefaultSize(DesignItem container, DesignItem createdItem, Rect position)
        {
            //Rect position = new Rect();
            PlacementOperation operation = PlacementOperation.TryStartInsertNewComponents(
                container,
                new DesignItem[] { createdItem },
                new Rect[] { position },
                PlacementType.AddItem
            );
            if (operation != null)
            {
                container.Services.Selection.SetSelectedComponents(new DesignItem[] { createdItem });
                operation.Commit();
                return true;
            }
            else
            {
                return false;
            }
        }

        public void CloseDockableWindow(IACObject window)
        {
            if (window is VBDockingContainerToolWindowVB)
            {
                VBDockingContainerToolWindow vbDockingContainerToolWindow = window as VBDockingContainerToolWindow;
                if (vbDockingContainerToolWindow != null)
                {
                    if (vbDockingContainerToolWindow.VBDockingPanel != null)
                    {
                        if (vbDockingContainerToolWindow.VBDockingPanel is VBDockingPanelToolWindow)
                        {
                            VBDockingPanelToolWindow VBDockingPanel = vbDockingContainerToolWindow.VBDockingPanel as VBDockingPanelToolWindow;
                            if (VBDockingPanel != null && VBDockingPanel.State != VBDockingPanelState.AutoHide)
                                VBDockingPanel.ChangeState(VBDockingPanelState.Hidden);
                            window.ACUrlCommand(Const.CmdClose);
                        }
                        else if (vbDockingContainerToolWindow.VBDockingPanel is VBDockingPanelTabbedDoc)
                        {
                            VBDockingPanelTabbedDoc VBDockingPanel = ((VBDockingContainerToolWindow)window).VBDockingPanel as VBDockingPanelTabbedDoc;
                            if (VBDockingPanel != null)
                                VBDockingPanel.RemoveDockingContainerToolWindow((VBDockingContainerBase)window);
                        }
                    }
                }
            }
            else if (window != null)
                window.ACUrlCommand(Const.CmdClose);
        }

        public virtual void OnDesignerLoaded(IVBContent designEditor, bool reverseToXaml)
        {
            VBDesigner vbDesigner = Designer<VBDesigner>();
            if (vbDesigner == null)
                return;

            if (!reverseToXaml)
            {
                VBPresenter vbPresenter = vbDesigner.ParentACComponent as VBPresenter;
                if (vbPresenter != null && vbPresenter.RoutingLogic != null)
                {
                    vbPresenter.RoutingLogic.ClearVB();
                    VBDesignEditor vbDesignEditor = designEditor as VBDesignEditor;
                    if (vbDesignEditor != null)
                    {
                        DesignContext designContext = vbDesignEditor.DesignSurface.DesignContext;
                        if (designContext != null)
                        {
                            var designItemEdges = designContext.Services.Component.DesignItems.Where(c => c.View is VBEdge);
                            vbPresenter.RoutingLogic.InitWithDesignItems(designContext.RootItem,
                                designContext.Services.Component.DesignItems.Where(c => c.View is VBVisual || c.View is VBVisualGroup).Select(x => x.View).OfType<FrameworkElement>().ToList(),
                                designItemEdges.Select(c => c.View).OfType<FrameworkElement>(), designItemEdges);
                        }
                    }
                }
            }
        }

        public abstract IEnumerable<IACObject> GetAvailableTools();

        public virtual void RecalcEdgeRouting()
        {
            VBDesigner vbDesigner = Designer<VBDesigner>();
            if (vbDesigner == null)
                return;

            OnDesignerLoaded(vbDesigner.VBDesignEditor as VBDesignEditor, false);
            VBPresenter vbPresenter = vbDesigner.ParentACComponent as VBPresenter;
            if (vbPresenter != null && vbPresenter.RoutingLogic != null)
            {
                UIElement rootCanvas = null;
                if (vbPresenter.RoutingLogic.RootDesignItem.View is VBCanvas)
                    rootCanvas = vbPresenter.RoutingLogic.RootDesignItem.View;
                else if (vbPresenter.RoutingLogic.RootDesignItem.View is VBScrollViewer)
                    rootCanvas = ((VBScrollViewer)vbPresenter.RoutingLogic.RootDesignItem.View).Content as UIElement;
                if (rootCanvas != null)
                    vbPresenter.RoutingLogic.CalculateEdgeRoute(rootCanvas);
            }
            vbDesigner.DesignPanel.Focus();
        }

        public void ACActionToTargetLogic(IACInteractiveObject oldTargetVBDataObject, ACActionArgs oldActionArgs, out IACInteractiveObject targetVBDataObject, out ACActionArgs actionArgs)
        {
            throw new NotImplementedException();
        }
    }
}
