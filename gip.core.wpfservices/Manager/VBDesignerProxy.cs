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
using static gip.core.datamodel.IVBComponentDesignManagerProxy;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Xml.Linq;

namespace gip.core.wpfservices
{
    public abstract class VBDesignerProxy : IVBComponentDesignManagerProxy
    {
        public VBDesignerProxy(IACComponent component)
        {
            _DesignerComp = component;
        }

        #region Properties

        private IACComponent _DesignerComp;
        public T Designer<T>() where T : VBDesigner
        {
            if (_DesignerComp == null)
                return null;
            return _DesignerComp as T;
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

        public DesignSurface DesignSurface
        {
            get
            {
                VBDesigner vbDesigner = Designer<VBDesigner>();
                if (vbDesigner == null)
                    return null;

                return (vbDesigner.VBDesignEditor as VBDesignEditor).DesignSurface;
            }
        }

        public DesignPanel DesignPanel
        {
            get
            {
                VBDesigner vbDesigner = Designer<VBDesigner>();
                if (vbDesigner == null)
                    return null;

                return vbDesigner.VBDesignEditor != null ? (vbDesigner.VBDesignEditor as VBDesignEditor).DesignSurface.DesignPanel : null;
            }
        }

        public VBDesign VBDesign
        {
            get
            {
                VBDesigner vbDesigner = Designer<VBDesigner>();
                if (vbDesigner == null)
                    return null;

                if ((vbDesigner.VBDesignControl != null) && (vbDesigner.VBDesignControl is VBDesign))
                    return (vbDesigner.VBDesignControl as VBDesign);
                return null;
            }
        }

        #endregion

        #region VisualChange

        public abstract void UpdateVisual();

        public void AddToVisualChangeListRect(IACObject visualObject, LayoutActionType layoutAction = LayoutActionType.Insert, string acUrl = "", string acUrl2 = "", Rect position = new Rect())
        {
            VBDesigner vbDesigner = Designer<VBDesigner>();
            if (vbDesigner == null)
                return;

            _VisualChangeList.Add(new VisualInfo { VisualObject = visualObject, LayoutAction = layoutAction, ACUrl = acUrl, ACUrl2 = acUrl2, Position = position });
        }

        public void AddToVisualChangeList(IACObject visualObject, short layoutAction, string acUrl, string acUrl2)
        {
            LayoutActionType layoutActionType;
            switch (layoutAction)
            {
                case 0:
                    layoutActionType = LayoutActionType.Insert;
                    break;
                case 1:
                    layoutActionType = LayoutActionType.Delete;
                    break;
                case 2:
                    layoutActionType = LayoutActionType.Move;
                    break;
                case 3:
                    layoutActionType = LayoutActionType.InsertEdge;
                    break;
                case 4:
                    layoutActionType = LayoutActionType.DeleteEdge;
                    break;
                default:
                    layoutActionType = LayoutActionType.Insert;
                    break;
            }

            AddToVisualChangeListRect(visualObject, layoutActionType, acUrl, acUrl2);
        }

        public LayoutActionType TransformShortToLayoutAction(short layoutAction)
        {
            LayoutActionType layoutActionType;
            switch (layoutAction)
            {
                case 0:
                    layoutActionType = LayoutActionType.Insert;
                    break;
                case 1:
                    layoutActionType = LayoutActionType.Delete;
                    break;
                case 2:
                    layoutActionType = LayoutActionType.Move;
                    break;
                case 3:
                    layoutActionType = LayoutActionType.InsertEdge;
                    break;
                case 4:
                    layoutActionType = LayoutActionType.DeleteEdge;
                    break;
                default:
                    layoutActionType = LayoutActionType.Insert;
                    break;
            }
            return layoutActionType;
        }

        public void ClearVisualChangeList()
        {
            VBDesigner vbDesigner = Designer<VBDesigner>();
            if (vbDesigner == null)
                return;

            VisualChangeList.Clear();
        }

        #endregion

        #region DesignManager

        public virtual void ShowDesignManager(string dockingManagerName = "")
        {
            VBDesigner vbDesigner = Designer<VBDesigner>();
            if (vbDesigner == null)
                return;

            if (vbDesigner.VBDesignControl == null || !(vbDesigner.VBDesignControl is VBDesign))
                return;

            vbDesigner._VBDesignEditor = null;
            ((VBDesign)vbDesigner.VBDesignControl).IsDesignerActive = true;
            vbDesigner.IsDesignMode = true;
        }

        public virtual void HideDesignManager()
        {
            VBDesigner vbDesigner = Designer<VBDesigner>();
            if (vbDesigner == null)
                return;

            if (vbDesigner.VBDesignControl == null || !(vbDesigner.VBDesignControl is VBDesign))
                return;

            ((VBDesign)vbDesigner.VBDesignControl).IsDesignerActive = false;
            vbDesigner.IsDesignMode = false;
        }

        #endregion

        #region ToolService

        public abstract IEnumerable<IACObject> GetAvailableTools();

        public virtual void OnCurrentAvailableToolChanged()
        {
            if (DesignPanel != null)
                DesignPanel.Focus();
        }

        public virtual void CurrentAvailableElement(object ToolService, ACObjectItem _CurrentAvailableTool, ACObjectItem value)
        {
            VBDesigner vbDesigner = Designer<VBDesigner>();
            if (vbDesigner == null)
                return;

            ITool newTool = null;
            if (_CurrentAvailableTool is DesignManagerToolItem)
            {
                newTool = (_CurrentAvailableTool as DesignManagerToolItem).CreateControlTool;
            }
            else if (value != null && value.ACObject != null)
            {
                newTool = DesignManagerControlTool.CreateNewInstance(vbDesigner, value.ACObject, value.ACUrlRelative);
            }
            (ToolService as IToolService).CurrentTool = newTool ?? (ToolService as IToolService).PointerTool;
        }

        public void CurrentAvailableElementIsToolSelection(object ToolService, ACObjectItem _CurrentAvailableProperty, ACObjectItem value)
        {
            VBDesigner vbDesigner = Designer<VBDesigner>();
            if (vbDesigner == null)
                return;

            ITool newTool = null;
            if (_CurrentAvailableProperty is DesignManagerToolItem)
            {
                newTool = (_CurrentAvailableProperty as DesignManagerToolItem).CreateControlTool;
            }
            else if (value != null && value.ACObject != null)
            {
                bool isTool = true;
                if (value.ACObject is NodeInfo)
                {
                    NodeInfo nodeInfo = value.ACObject as NodeInfo;
                    if (nodeInfo.PAACClass != null && nodeInfo.PWACClass == null && nodeInfo.PAACClass.ACKind == Global.ACKinds.TACApplicationManager)
                        isTool = false;
                }
                if (isTool)
                {
                    newTool = DesignManagerControlTool.CreateNewInstance(vbDesigner, value.ACObject, value.ACUrlRelative);
                }
            }
            (ToolService as IToolService).CurrentTool = newTool ?? (ToolService as IToolService).PointerTool;
        }

        IToolService _ToolService = null;

        public void ReloadToolService()
        {
            VBDesigner vbDesigner = Designer<VBDesigner>();
            if (vbDesigner == null)
                return;

            IToolService newService = null;
            if ((vbDesigner.VBDesignEditor != null)
                && (DesignSurface != null)
                && (DesignSurface.DesignContext != null))
            {
                newService = this.DesignSurface.DesignContext.Services.Tool;
            }
            bool changed = false;
            if (_ToolService != newService)
            {
                if (_ToolService != null)
                    _ToolService.CurrentToolChanged -= OnCurrentToolChanged;
                changed = true;
                _ToolService = newService;
            }
            if (changed && (_ToolService != null))
            {
                _ToolService.CurrentToolChanged += OnCurrentToolChanged;
                OnCurrentToolChanged(null, null);
            }
        }

        public void OnCurrentToolChanged(object sender, EventArgs e)
        {
            object tagToFind;
            if (_ToolService.CurrentTool == _ToolService.PointerTool)
            {
                tagToFind = null;
            }
            else
            {
                tagToFind = _ToolService.CurrentTool;
            }
        }

        public object GetToolService()
        {
            ReloadToolService();
            return _ToolService;
        }

        public void DeInitToolService()
        {
            _ToolService = null;
        }

        #endregion

        public IACInteractiveObject GetVBDesignEditor(IACComponent component)
        {
            VBDesigner vbDesigner = component as VBDesigner;
            if (vbDesigner == null)
                return null;

            if (VBDesign != null)
            {
                if ((VBDesign.Content != null) && (VBDesign.Content is VBDesignEditor))
                    return (VBDesign.Content as VBDesignEditor);
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
                VBRoutingLogic routingLogic = null;
                VBPresenter vbPresenter = vbDesigner.ParentACComponent as VBPresenter;
                if (vbPresenter != null)
                {
                    routingLogic = vbPresenter.GetRoutingLogic() as VBRoutingLogic;
                }
                if (routingLogic != null)
                {
                    routingLogic.ClearVB();
                    VBDesignEditor vbDesignEditor = designEditor as VBDesignEditor;
                    if (vbDesignEditor != null)
                    {
                        DesignContext designContext = vbDesignEditor.DesignSurface.DesignContext;
                        if (designContext != null)
                        {
                            var designItemEdges = designContext.Services.Component.DesignItems.Where(c => c.View is VBEdge);
                            routingLogic.InitWithDesignItems(designContext.RootItem,
                                designContext.Services.Component.DesignItems.Where(c => c.View is VBVisual || c.View is VBVisualGroup).Select(x => x.View).OfType<FrameworkElement>().ToList(),
                                designItemEdges.Select(c => c.View).OfType<FrameworkElement>(), designItemEdges);
                        }
                    }
                }
            }
        }

        public virtual void RecalcEdgeRouting()
        {
            VBDesigner vbDesigner = Designer<VBDesigner>();
            if (vbDesigner == null)
                return;

            OnDesignerLoaded(vbDesigner.VBDesignEditor as VBDesignEditor, false);
            VBRoutingLogic routingLogic = null;
            VBPresenter vbPresenter = vbDesigner.ParentACComponent as VBPresenter;
            if (vbPresenter != null)
            {
                routingLogic = vbPresenter.GetRoutingLogic() as VBRoutingLogic;
            }
            if (routingLogic != null)
            {
                UIElement rootCanvas = null;
                if (routingLogic.RootDesignItem.View is VBCanvas)
                    rootCanvas = routingLogic.RootDesignItem.View;
                else if (routingLogic.RootDesignItem.View is VBScrollViewer)
                    rootCanvas = ((VBScrollViewer)routingLogic.RootDesignItem.View).Content as UIElement;
                if (rootCanvas != null)
                    routingLogic.CalculateEdgeRoute(rootCanvas);
            }
            DesignPanel.Focus();
        }

        #region Abstract/Virtual Methods

        public abstract void ACActionToTargetLogic(IACInteractiveObject oldTargetVBDataObject, ACActionArgs oldActionArgs, out IACInteractiveObject targetVBDataObject, out ACActionArgs actionArgs);

        #endregion
    }
}
