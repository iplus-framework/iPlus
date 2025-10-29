using Avalonia;
using gip.core.datamodel;
using gip.core.layoutengine.avui;
using gip.core.manager;
using gip.ext.design.avui;
using gip.ext.designer.avui.Controls;
using gip.ext.designer.avui.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using static gip.core.manager.VBDesigner;

namespace gip.core.wpfservices.avui
{
    public class VBDesignerXAMLProxy : VBDesignerProxy, IVBComponentDesignManagerXAML
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
            foreach (var change in VisualChangeList)
            {
                if (change.LayoutAction != LayoutActionType.InsertEdge) continue;
                if (changeGroup == null)
                {
                    changeGroup = DesignSurface.DesignContext.OpenGroup("Cut " + changedItems.Count + " elements", changedItems);
                }

                CreateVBEdgeDesignItem(change, DesignSurface.DesignContext);
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
                item.Properties[VBEdge.VBConnectorSourceProperty].SetValue(fromXName);
                item.Properties[VBEdge.VBConnectorTargetProperty].SetValue(toXName);
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

        #region Available Elements

        public void CurrentAvailableElementClicked(object ToolService, ACObjectItem _CurrentAvailableProperty)
        {
            if (_CurrentAvailableProperty is DesignManagerToolItem)
            {
                ITool newTool = null;
                newTool = (_CurrentAvailableProperty as DesignManagerToolItem).CreateControlTool;
                if ((newTool != null) && (ToolService as IToolService).CurrentTool != newTool)
                    (ToolService as IToolService).CurrentTool = newTool;
            }
        }

        public override void CurrentAvailableElement(object ToolService, ACObjectItem _CurrentAvailableProperty, ACObjectItem value)
        {
            VBDesignerXAML vbDesigner = Designer<VBDesignerXAML>();
            if (vbDesigner == null)
                return;

            ITool newTool = null;
            if (_CurrentAvailableProperty is DesignManagerToolItem)
            {
                newTool = (_CurrentAvailableProperty as DesignManagerToolItem).CreateControlTool;
            }
            else if (value != null && value.ACObject != null)
            {
                newTool = DesignManagerControlTool.CreateNewInstance(vbDesigner, _CurrentAvailableProperty.ACObject, _CurrentAvailableProperty.ACUrlRelative);
                //DesignManagerToolItem designManagerToolItem = new DesignManagerToolItem(_CurrentAvailableProperty.ACObject, _CurrentAvailableProperty.ACCaption, , this);
                //newTool = designManagerToolItem.CreateControlTool;
            }
            (ToolService as IToolService).CurrentTool = newTool ?? (ToolService as IToolService).PointerTool;
        }

        #endregion

        #region DesignMethods

        public void DesignerRotateR90()
        {
            DesignSurface.DesignerRotateR90();
        }

        public bool IsEnabledDesignerRotateR90()
        {
            VBDesignerXAML vbDesigner = Designer<VBDesignerXAML>();
            if (vbDesigner == null)
                return false;

            if (vbDesigner.VBDesignEditor == null || DesignSurface == null)
                return false;
            return DesignSurface.IsEnabledDesignerRotateR90();
        }

        public void DesignerFlipHorz()
        {
            VBDesignerXAML vbDesigner = Designer<VBDesignerXAML>();
            if (vbDesigner == null)
                return;

            if (vbDesigner.VBDesignEditor == null || DesignSurface == null)
                return;

            DesignSurface.DesignerFlipHorz();
        }

        public void DesignerFlipVert()
        {
            DesignSurface.DesignerFlipVert();
        }

        public bool IsEnabledDesignerFlipHorz()
        {
            VBDesignerXAML vbDesigner = Designer<VBDesignerXAML>();
            if (vbDesigner == null)
                return false;

            if (vbDesigner.VBDesignEditor == null || DesignSurface == null)
                return false;

            return DesignSurface.IsEnabledDesignerFlipHorz();
        }

        public bool IsEnabledDesignerFlipVert()
        {
            VBDesignerXAML vbDesigner = Designer<VBDesignerXAML>();
            if (vbDesigner == null)
                return false;

            if (vbDesigner.VBDesignEditor == null || DesignSurface == null)
                return false;

            return DesignSurface.IsEnabledDesignerFlipVert();
        }

        public void DesignerResetTransform()
        {
            DesignSurface.DesignerResetTransform();
        }

        public bool IsEnabledDesignerResetTransform()
        {
            VBDesignerXAML vbDesigner = Designer<VBDesignerXAML>();
            if (vbDesigner == null)
                return false;

            if (vbDesigner.VBDesignEditor == null || DesignSurface == null)
                return false;

            return DesignSurface.IsEnabledDesignerResetTransform();
        }

        #endregion

        #region IACComponentDesignManager

        public override void ACActionToTargetLogic(IACInteractiveObject oldTargetVBDataObject, ACActionArgs oldActionArgs, out IACInteractiveObject targetVBDataObject, out ACActionArgs actionArgs)
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

        public bool IsEnabledACActionToTarget(IACInteractiveObject targetVBDataObject, ACActionArgs actionArgs)
        {
            if (targetVBDataObject is IBindingDropHandler)
            {
                var query = actionArgs.DropObject.ACContentList.Where(c => c is ACObjectItem);
                if (query.Any())
                {
                    ACObjectItem currentTool = query.First() as ACObjectItem;
                    if (currentTool.ACObject is ACClassProperty || currentTool.ACObject is ACClassMethod)
                        return true;
                }
                return false;
            }

            return true;
        }

        public void ParentComponent_ACSaveChangesExecuted(object sender, EventArgs e)
        {
            VBDesignerXAML vbDesigner = Designer<VBDesignerXAML>();
            if (vbDesigner == null)
                return;

            if (vbDesigner.VBDesignEditor != null)
            {
                ((VBDesignEditor)vbDesigner.VBDesignEditor).SaveToXAML();
            }
            if (vbDesigner.SaveDesignFromOtherDBContext)
            {
                ACClassDesign acClassDesign = (vbDesigner.CurrentDesign as ACClassDesign);
                if (acClassDesign != null)
                {
                    acClassDesign.GetObjectContext().ACSaveChanges();
                }
            }
            vbDesigner.SaveDesignFromOtherDBContext = false;
        }

        public void ParentACComponentDatabase_ACChangesExecuted(object sender, ACChangesEventArgs e)
        {
            VBDesignerXAML vbDesigner = Designer<VBDesignerXAML>();
            if (vbDesigner == null)
                return;

            if (e.ChangeType == ACChangesEventArgs.ACChangesType.ACUndoChanges && e.Succeeded)
            {
                if (vbDesigner.VBDesignEditor != null)
                {
                    ((VBDesignEditor)vbDesigner.VBDesignEditor).ObjectsInDesignViewChanged = false;
                }
            }
        }

        DesignManagerToolItem GetToolItemOfDropObject(IACInteractiveObject dropObject)
        {
            if (dropObject == null)
                return null;
            var query = dropObject.ACContentList.Where(c => typeof(DesignManagerToolItem).IsAssignableFrom(c.GetType()));
            if (query.Any())
                return query.First() as DesignManagerToolItem;
            return null;
        }

        IACEntityProperty GetContentOfACObjectItem(DesignManagerToolItem objectItem)
        {
            if (objectItem == null)
                return null;
            if (objectItem.ACObject == null)
                return null;
            return objectItem.ACObject as IACEntityProperty;
        }

        #endregion

        public IEnumerable<ACClass> DesignManagerToolGetVBControlList(Database context)
        {
            return DesignManagerControlTool.GetVBControlList(context).OrderBy(c => c.ACIdentifier);
        }

        public XElement LayoutGeneratorLoadLayoutAsXElement(string xmlLayout)
        {
            return Layoutgenerator.LoadLayoutAsXElement(xmlLayout);
        }
    }
}
