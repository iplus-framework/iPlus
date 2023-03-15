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
using gip.ext.designer.Services;

namespace gip.core.wpfservices
{
    public class VBDesignerXAMLProxy : VBDesignerProxy
    {
        public VBDesignerXAMLProxy(IACComponent component) : base(component)
        {
        }

        public override void UpdateVisual()
        {
            VBDesignerXAML vbDesigner = Designer<VBDesignerXAML>();
            if (vbDesigner == null)
                return;

            List<DesignItem> changedItems = new List<DesignItem>();

            ChangeGroup changeGroup = null;

            // Einfügen von WFEdge (Vor WF, da diese für die Layoutberechnung benötigt werden
            foreach (var change in vbDesigner.VisualChangeList)
            {
                if (change.LayoutAction != VBDesigner.LayoutActionType.InsertEdge) continue;
                if (changeGroup == null)
                {
                    changeGroup = vbDesigner.DesignSurface.DesignContext.OpenGroup("Cut " + changedItems.Count + " elements", changedItems);
                }

                CreateVBEdgeDesignItem(change, vbDesigner.DesignSurface.DesignContext);
            }

            if (changeGroup != null)
                changeGroup.Commit();
        }

        public void CreateVBEdgeDesignItem(VisualInfo visualInfo, DesignContext designContext)
        {
            DesignItem vbCanvas = designContext.RootItem.ContentProperty.Value;

            String fromXName = visualInfo.ACUrl;
            String toXName = visualInfo.ACUrl2;

            VBEdge newInstance = (VBEdge)vbCanvas.Context.Services.ExtensionManager.CreateInstanceWithCustomInstanceFactory(typeof(VBEdge), null);
            DesignItem item = vbCanvas.Context.Services.Component.RegisterComponentForDesigner(newInstance);
            if ((item != null) && (item.View != null))
            {
                DrawShapesAdornerBase.ApplyDefaultPropertiesToItemS(item);
                item.Properties[VBEdge.ACName1Property].SetValue(fromXName);
                item.Properties[VBEdge.ACName2Property].SetValue(toXName);
            }
            //item.Properties[VBEdge.NameProperty].SetValue(acVisualEdge.ACIdentifier);
            //            item.Properties[VBEdge.VBContentProperty].SetValue(RootACUrl + "\\" + acVisualEdge.GetACUrl(acVisualEdge.ParentACObject));

            AddItemWithDefaultSize(vbCanvas, item, new Rect());
        }

        public override IEnumerable<IACObject> GetAvailableTools()
        {
            VBDesignerXAML vbDesigner = Designer<VBDesignerXAML>();
            if (vbDesigner == null)
                return null;

            ACObjectItemList objectLayoutEntrys = new ACObjectItemList();
            objectLayoutEntrys.Add(new DesignManagerToolItem(gip.core.datamodel.Database.Root.Environment.TranslateText(vbDesigner, "Pointer"), PointerTool.Instance, "DesignPointer"));
            objectLayoutEntrys.Add(new DesignManagerToolItem(gip.core.datamodel.Database.Root.Environment.TranslateText(vbDesigner, "Connector"), new ConnectTool(vbDesigner), "DesignConnector"));
            objectLayoutEntrys.Add(new DesignManagerToolItem(gip.core.datamodel.Database.Root.Environment.TranslateText(vbDesigner, "Line"), new DrawingToolForLine(), "DesignLine"));
            objectLayoutEntrys.Add(new DesignManagerToolItem(gip.core.datamodel.Database.Root.Environment.TranslateText(vbDesigner, "Rectangle"), new DrawingToolForRectangle(), "DesignRect"));
            objectLayoutEntrys.Add(new DesignManagerToolItem(gip.core.datamodel.Database.Root.Environment.TranslateText(vbDesigner, "Ellipse"), new DrawingToolForEllipse(), "DesignEllipse"));
            objectLayoutEntrys.Add(new DesignManagerToolItem(gip.core.datamodel.Database.Root.Environment.TranslateText(vbDesigner, "Polyline"), new DrawingToolForPolyline(), "DesignPolyline"));
            objectLayoutEntrys.Add(new DesignManagerToolItem(gip.core.datamodel.Database.Root.Environment.TranslateText(vbDesigner, "Polygon"), new DrawingToolForPolygon(), "DesignPolygon"));
            objectLayoutEntrys.Add(new DesignManagerToolItem(gip.core.datamodel.Database.Root.Environment.TranslateText(vbDesigner, "EditPoints"), new DrawingToolEditPoints(), "DesignEditPoints"));
            return objectLayoutEntrys;
        }

        public new void ACActionToTargetLogic(IACInteractiveObject oldTargetVBDataObject, ACActionArgs oldActionArgs, out IACInteractiveObject targetVBDataObject, out ACActionArgs actionArgs)
        {
            actionArgs = oldActionArgs;
            targetVBDataObject = oldTargetVBDataObject;

            VBDesignerXAML vbDesigner = Designer<VBDesignerXAML>();
            if (vbDesigner == null)
                return;

            if (targetVBDataObject is IBindingDropHandler)
            {
                IBindingDropHandler dropHandler = targetVBDataObject as IBindingDropHandler;
                var query = actionArgs.DropObject.ACContentList.Where(c => c is ACObjectItem);
                if (query.Any())
                {
                    ACObjectItem currentTool = query.First() as ACObjectItem;
                    string vbContent = vbDesigner.BuildVBContentFromACUrl(currentTool.ACUrlRelative, currentTool.ACObject);
                    if (!String.IsNullOrEmpty(vbContent))
                    {
                        if (currentTool.ACObject is ACClassMethod)
                        {
                            int methodSign = vbContent.IndexOf('!');
                            bool isGlobalFunc = false;
                            if (methodSign > 0)
                            {
                                string left = vbContent.Substring(0, methodSign);
                                string urlOfEnvManager = vbDesigner.Root.Environment.GetACUrl();
                                urlOfEnvManager = urlOfEnvManager.Replace("\\", "");
                                isGlobalFunc = left.Contains(urlOfEnvManager);
                            }
                            dropHandler.AddOrUpdateBindingWithMethod(vbContent, isGlobalFunc, currentTool.ACObject);
                        }
                        else
                        {
                            dropHandler.AddOrUpdateBindingWithProperty(vbContent, currentTool.ACObject);
                        }
                        actionArgs.Handled = true;
                    }
                }
            }
        }
    }
}
