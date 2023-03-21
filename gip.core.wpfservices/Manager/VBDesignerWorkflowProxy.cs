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
using System.ComponentModel.Design;
using gip.ext.designer;
using gip.ext.designer.Services;

namespace gip.core.wpfservices
{
    public class VBDesignerWorkflowProxy : VBDesignerProxy
    {
        public VBDesignerWorkflowProxy(IACComponent component) : base(component)
        {
        }

        public override void UpdateVisual()
        {
            VBDesignerWorkflow vbDesigner = Designer<VBDesignerWorkflow>();
            if (vbDesigner == null)
                return;

            List<DesignItem> changedItems = new List<DesignItem>();

            ChangeGroup changeGroup = null;
            // Löschen von WF und WFEdge
            foreach (var change in VisualChangeList)
            {
                if (change.LayoutAction != LayoutActionType.Delete && change.LayoutAction != LayoutActionType.DeleteEdge) continue;

                if (changeGroup == null)
                {
                    changeGroup = DesignSurface.DesignContext.OpenGroup("Cut " + changedItems.Count + " elements", changedItems);
                }
                if (change.VisualObject is IACWorkflowEdge)
                {
                    DesignItem designItem = GetDesignItemWFEdge(change.ACUrl, change.ACUrl2);
                    if (designItem != null)
                    {
                        changedItems.Add(designItem);
                        ModelTools.DeleteComponents(changedItems);
                        changedItems.Clear();
                    }
                }
                else if (change.VisualObject is IACWorkflowNode)
                {
                    DesignItem designItem = GetDesignItemWF(change.ACUrl);
                    if (designItem != null)
                    {
                        VBPresenter vbPresenter = vbDesigner.ParentACComponent as VBPresenter;
                        PWOfflineNode parentPWNode = vbPresenter.ACUrlCommand(change.ACUrl) as PWOfflineNode;
                        if (parentPWNode != null)
                            parentPWNode.ParentACComponent.StopComponent(parentPWNode);

                        changedItems.Add(designItem);
                        ModelTools.DeleteComponents(changedItems);
                        changedItems.Clear();
                    }
                }
            }

            // Einfügen von WFEdge (Vor WF, da diese für die Layoutberechnung benötigt werden
            foreach (var change in VisualChangeList)            //foreach (var change in VisualChangeList.Where(c => !c.IsDelete))
            {
                if (change.LayoutAction != LayoutActionType.InsertEdge) continue;
                if (changeGroup == null)
                {
                    changeGroup = DesignSurface.DesignContext.OpenGroup("Cut " + changedItems.Count + " elements", changedItems);
                }

                if (change.VisualObject is IACWorkflowEdge)
                {
                    CreateVBEdgeDesignItem(change.VisualObject as IACWorkflowEdge, DesignSurface.DesignContext);
                }
            }

            // Einfügen von WF
            foreach (var change in VisualChangeList)            //foreach (var change in VisualChangeList.Where(c => !c.IsDelete))
            {
                if (change.LayoutAction != LayoutActionType.Insert) continue;
                if (changeGroup == null)
                {
                    changeGroup = DesignSurface.DesignContext.OpenGroup("Cut " + changedItems.Count + " elements", changedItems);
                }

                if (change.VisualObject is IACWorkflowNode)
                {
                    DesignItem designItemParent = null;
                    CreateVBVisualDesignItem(change, change.VisualObject as IACWorkflowNode, DesignSurface.DesignContext, out designItemParent);
                    DesignSurface.UpdateLayout();
                }
            }

            if (changeGroup != null)
                changeGroup.Commit();
        }

        #region DesignItem

        public void CreateVBEdgeDesignItem(IACWorkflowEdge acVisualEdge, object designContextObj)
        {
            DesignContext designContext = designContextObj as DesignContext;
            DesignItem vbCanvas = designContext.RootItem;

            String fromXName = acVisualEdge.SourceACConnector;
            String toXName = acVisualEdge.TargetACConnector;

            VBEdge newInstance = (VBEdge)vbCanvas.Context.Services.ExtensionManager.CreateInstanceWithCustomInstanceFactory(typeof(VBEdge), null);
            DesignItem item = vbCanvas.Context.Services.Component.RegisterComponentForDesigner(newInstance);
            if ((item != null) && (item.View != null))
            {
                DrawShapesAdornerBase.ApplyDefaultPropertiesToItemS(item);
                item.Properties[VBEdge.ACName1Property].SetValue(fromXName);
                item.Properties[VBEdge.ACName2Property].SetValue(toXName);
            }
            item.Properties[VBEdge.NameProperty].SetValue(acVisualEdge.ACIdentifier);
            item.Properties[VBEdge.VBContentProperty].SetValue(Const.VBPresenter_SelectedWFContext + "\\" + acVisualEdge.GetACUrl(acVisualEdge.ParentACObject));
            item.Properties[VBEdge.StrokeProperty].SetValue(new SolidColorBrush(Colors.White));
            item.Properties[VBEdge.StrokeThicknessProperty].SetValue(1.5);

            AddItemWithDefaultSize(vbCanvas, item, new Rect());
        }

        public virtual DesignItem CreateVBVisualDesignItem(VisualInfo visualInfo, IACWorkflowNode acVisualWF, DesignContext designContext, out DesignItem designItemParent)
        {
            object newInstance = null;
            designItemParent = null;

            VBDesignerWorkflow vbDesigner = Designer<VBDesignerWorkflow>();
            if (vbDesigner == null)
                return null;

            if (acVisualWF.PWACClass.ACKind == Global.ACKinds.TPWGroup)
                newInstance = designContext.Services.ExtensionManager.CreateInstanceWithCustomInstanceFactory(typeof(VBVisualGroup), null);
            else
                newInstance = designContext.Services.ExtensionManager.CreateInstanceWithCustomInstanceFactory(typeof(VBVisual), null);

            string parentACUrl = null;
            if (acVisualWF.ParentACObject == null)
            {
                IACWorkflowNode wfNode = vbDesigner.CurrentDesign as IACWorkflowNode;
                if (wfNode != null)
                    parentACUrl = wfNode.VisualACUrl;
            }
            else
                parentACUrl = (acVisualWF.ParentACObject as IACWorkflowNode).VisualACUrl;
            string newACUrl = acVisualWF.ACIdentifier;
            VBPresenter vbPresenter = vbDesigner.ParentACComponent as VBPresenter;
            PWOfflineNode parentPWNode = vbPresenter.ACUrlCommand(parentACUrl) as PWOfflineNode;
            IACWorkflowNode tempACVisual = acVisualWF;

            parentPWNode.CreateChildPWNode(acVisualWF, Global.ACStartTypes.Manually);

            designItemParent = GetDesignItemWF(parentACUrl);

            ACClassDesign acClassDesign = acVisualWF.PWACClass.ACType.GetDesign(acVisualWF.PWACClass, Global.ACUsages.DUControl, Global.ACKinds.DSDesignLayout);

            DesignItem item = designItemParent.Context.Services.Component.RegisterComponentForDesigner(newInstance);
            item.Properties[VBVisual.VBContentProperty].SetValue(newACUrl);
            item.Properties[VBVisual.NameProperty].SetValue(acVisualWF.XName);
            item.Properties[VBVisual.AllowDropProperty].SetValue(true);

            Rect rect = new Rect();
            if (acClassDesign != null)
            {
                item.Properties[VBVisual.HeightProperty].SetValue(acClassDesign.VisualHeight);
                item.Properties[VBVisual.WidthProperty].SetValue(acClassDesign.VisualWidth);

                rect = new Rect(visualInfo.Position.Left, visualInfo.Position.Top, acClassDesign.VisualWidth, acClassDesign.VisualHeight);
            }

            AddItemWithDefaultSize(designItemParent.ContentProperty.Value, item, rect);

            // Falls VBVisualGroup, dann muß auch ein DesignItem für das Canvas eingefügt werden
            if (newInstance is VBVisualGroup)
            {
                object newInstanceCanvas = designContext.Services.ExtensionManager.CreateInstanceWithCustomInstanceFactory(typeof(VBCanvasInGroup), null);
                DesignItem itemCanvas = designItemParent.Context.Services.Component.RegisterComponentForDesigner(newInstanceCanvas);
                item.Properties[HeaderedContentControl.ContentProperty].SetValue(itemCanvas);
            }
            return item;
        }

        DesignItem GetDesignItemWF(string acUrl)
        {
            DesignItem designItem = DesignPanel.Context.RootItem;
            var acUrlList = acUrl.Split('\\');
            foreach (var acUrlPart in acUrlList)
            {
                designItem = FindDesignItemWF(designItem, acUrlPart);
                if (designItem == null)
                    return null;
            }
            return designItem;
        }

        DesignItem FindDesignItemWF(DesignItem designItem, string acUrl)
        {
            switch (designItem.View.GetType().Name)
            {
                case Const.VBVisual_ClassName:
                    {
                        VBVisual vbVisual = designItem.View as VBVisual;
                        if (vbVisual.VBContent == acUrl)
                            return designItem;
                        return null;
                    }
                case Const.VBVisualGroup_ClassName:
                    {
                        VBVisualGroup vbVisualGroup = designItem.View as VBVisualGroup;
                        if (vbVisualGroup.VBContent == acUrl)
                            return designItem;
                    }
                    break;
            }
            if (designItem.ContentProperty != null)
            {
                if (designItem.ContentProperty.IsCollection)
                {
                    var collection = designItem.ContentProperty.CollectionElements;
                    foreach (var content in collection)
                    {
                        var designItemResult = FindDesignItemWF(content, acUrl);
                        if (designItemResult != null)
                            return designItemResult;
                    }
                }
                else
                {
                    if (designItem.ContentProperty.Value != null)
                    {
                        var content = designItem.ContentProperty.Value;
                        return FindDesignItemWF(content, acUrl);
                    }
                }
            }

            return null;
        }

        DesignItem GetDesignItemWFEdge(string vbConnectorSource, string vbConnectorTarget)
        {
            DesignItem designItem = DesignPanel.Context.RootItem;
            return FindDesignItemWFEdge(DesignPanel.Context.RootItem, vbConnectorSource, vbConnectorTarget);
        }

        DesignItem FindDesignItemWFEdge(DesignItem designItem, string vbConnectorSource, string vbConnectorTarget)
        {
            if (designItem.View.GetType().Name == "VBEdge")
            {
                VBEdge vbEdge = designItem.View as VBEdge;
                if (vbEdge.VBConnectorSource == vbConnectorSource && vbEdge.VBConnectorTarget == vbConnectorTarget)
                    return designItem;
            }
            if (designItem.ContentProperty != null)
            {
                if (designItem.ContentProperty.IsCollection)
                {
                    var collection = designItem.ContentProperty.CollectionElements;
                    foreach (var content in collection)
                    {
                        var designItemResult = FindDesignItemWFEdge(content, vbConnectorSource, vbConnectorTarget);
                        if (designItemResult != null)
                            return designItemResult;
                    }
                }
                else
                {
                    if (designItem.ContentProperty.Value != null)
                    {
                        var content = designItem.ContentProperty.Value;
                        return FindDesignItemWFEdge(content, vbConnectorSource, vbConnectorTarget);
                    }
                }
            }

            return null;
        }

        #endregion

        public override IEnumerable<IACObject> GetAvailableTools()
        {
            throw new NotImplementedException();
        }

        protected virtual void CreateWFEdge(VBEdge newVBEdge, VBConnector targetConnector)
        {
        }

        public override void ACActionToTargetLogic(IACInteractiveObject oldTargetVBDataObject, ACActionArgs oldActionArgs, out IACInteractiveObject targetVBDataObject, out ACActionArgs actionArgs)
        {
            actionArgs = oldActionArgs;
            targetVBDataObject = oldTargetVBDataObject;

            VBDesignerWorkflow vbDesigner = Designer<VBDesignerWorkflow>();
            if (vbDesigner == null)
                return;

            IACWorkflowNode rControlClass = null;
            switch (actionArgs.ElementAction)
            {
                case Global.ElementActionType.Move:
                    rControlClass = vbDesigner.LayoutAction(vbDesigner.CurrentDesignWF, actionArgs.DropObject, actionArgs.X, actionArgs.Y);
                    break;
                case Global.ElementActionType.Drop:
                    rControlClass = vbDesigner.ElementAction(vbDesigner.CurrentDesignWF, actionArgs.DropObject, targetVBDataObject, actionArgs.X, actionArgs.Y);
                    if (rControlClass != null)
                    {
                        vbDesigner.DroppedElement(rControlClass);
                        actionArgs.Handled = true;
                    }
                    break;
                case Global.ElementActionType.Line:
                    VBEdge newVBEdge = actionArgs.DropObject as VBEdge;
                    if (newVBEdge != null)
                    {
                        CreateWFEdge(newVBEdge, targetVBDataObject as VBConnector);
                        actionArgs.Handled = true;
                    }
                    break;
            }
        }
    }
}
