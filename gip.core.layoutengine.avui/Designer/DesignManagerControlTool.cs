using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
//using gip.core.autocomponent;
using System.Xml.Linq;
using gip.core.layoutengine.avui;
using gip.ext.design.avui;
using gip.ext.designer.avui.Services;
using System.Collections;
using System.Windows;
using System.Windows.Input;
using gip.core.layoutengine.avui.Helperclasses;

namespace gip.core.layoutengine.avui
{
    public class DesignManagerControlTool : CreateComponentTool
    {
        public DesignManagerControlTool(IACType vbControlClass, IACComponentDesignManager designManager, IACObject acObject, string acUrl)
            : base(vbControlClass.ObjectType)
        {
            _VBControlClass = vbControlClass;
            VBDesigner = designManager;
            ACObject = acObject;
            ACUrl = acUrl;
        }

        public static ITool CreateNewInstance(IACComponentDesignManager designManager, IACObject acObject, string acUrl)
        {
            IACType vbControlClass = null;

            if (String.IsNullOrEmpty(acUrl))
            {
                if (acObject is IACType)
                {
                    if (typeof(System.Windows.Controls.Panel).IsAssignableFrom((acObject as IACType).ObjectType))
                        acUrl = acObject.ACIdentifier;
                }
            }
             
            // Spezialbehandlung "einfaches Steuerelement"
            if (acObject is ACClass)
            {
                var acDataType = (acObject as IACType).ValueTypeACClass;
                if (acDataType.ACKind == Global.ACKinds.TACVBControl)
                {
                    vbControlClass = acDataType;
                    return new DesignManagerControlTool(vbControlClass, designManager, acObject, acUrl);
                }
            }
            else if (acObject is ACClassDesign)
            {
                ACClassDesign acClassDesign2 = acObject as ACClassDesign;
                if (acClassDesign2.ValueTypeACClass != null)
                    vbControlClass = acClassDesign2.ValueTypeACClass;
                if ((vbControlClass != null) && ((vbControlClass.ACIdentifier != Const.VBImage_ClassName) && (vbControlClass.ACIdentifier != Const.VBBorder_ClassName)))
                {
                    if ((acClassDesign2.ACKind == Global.ACKinds.DSDesignLayout) && (acClassDesign2.ACUsage == Global.ACUsages.DULayout) && (acClassDesign2.IsResourceStyle == true))
                    {
                        var query = GetVBControlList(designManager.Database.ContextIPlus).Where(c => c.ACIdentifier == Const.VBBorder_ClassName);
                        if (query.Any())
                            vbControlClass = query.First();
                    }
                    else if ((acClassDesign2.ACKind == Global.ACKinds.DSBitmapResource) && (acClassDesign2.ACUsage == Global.ACUsages.DUBitmap))
                    {
                        var query = GetVBControlList(designManager.Database.ContextIPlus).Where(c => c.ACIdentifier == Const.VBImage_ClassName);
                        if (query.Any())
                            vbControlClass = query.First();
                    }
                }
                if (vbControlClass != null)
                {
                    return new DesignManagerControlTool(vbControlClass, designManager, acObject, acUrl);
                }
            }
            else if ((acObject is ACClassText) || (acObject is ACClassMessage))
            {
                var query = GetVBControlList(designManager.Database.ContextIPlus).Where(c => c.ACIdentifier == Const.VBTranslationText_ClassName);
                if (query.Any())
                    vbControlClass = query.First();
                if (vbControlClass != null)
                {
                    return new DesignManagerControlTool(vbControlClass, designManager, acObject, acUrl);
                }
            }

            if (acObject.ACType == null)
                return (designManager.ToolService as IToolService).PointerTool;

            ACClassDesign acClassDesign = acObject.ACType.GetDesign(acObject, Global.ACUsages.DUControl, Global.ACKinds.DSDesignLayout);
            if (acClassDesign != null && acClassDesign.ValueTypeACClass != null)
            {
                vbControlClass = acClassDesign.ValueTypeACClass;
                if (vbControlClass != null && vbControlClass.ObjectType != null)
                {
                    return new DesignManagerControlTool(vbControlClass, designManager, acObject, acUrl);
                }
                else
                {
                    return (designManager.ToolService as IToolService).PointerTool;
                }
            }
            else
            {
                return (designManager.ToolService as IToolService).PointerTool;
            }
        }

        public override Size OnGetDefaultSize()
        {
            if ((ACObject != null) && (ACObject is ACClass))
            {
                ACClassDesign acClassDesign = (ACObject as ACClass).GetDesign(ACObject, Global.ACUsages.DUControl, Global.ACKinds.DSDesignLayout);
                if (acClassDesign != null)
                {
                    if ((acClassDesign.VisualWidth > 0.1) && (acClassDesign.VisualHeight > 0.1))
                        return new Size(acClassDesign.VisualWidth, acClassDesign.VisualHeight);
                }
            }
            if (_VBControlClass != null)
            {
                ACClassDesign acClassDesign = _VBControlClass.GetDesign(_VBControlClass, Global.ACUsages.DUControl, Global.ACKinds.DSDesignLayout);
                if (acClassDesign != null)
                {
                    if ((acClassDesign.VisualWidth > 0.1) && (acClassDesign.VisualHeight > 0.1))
                        return new Size(acClassDesign.VisualWidth, acClassDesign.VisualHeight);
                }
            }
            return new Size(double.NaN, double.NaN);
        }


        private static List<ACClass> VBControlList;

        public static List<ACClass> GetVBControlList(Database database)
        {
            if (VBControlList == null)
            {
                short nTypeIndex = (short)Global.ACKinds.TACVBControl;
                VBControlList = database.ACClass.Where(c => c.ACKindIndex == nTypeIndex).ToList();
            }
            return VBControlList;
        }

        public IACComponentDesignManager VBDesigner
        {
            get;
            set;
        }

        private IACType _VBControlClass;
        public IACType VBControlClass
        {
            get
            {
                return _VBControlClass;
            }
        }

        // TODO: Auswertelogik welches Control erzeugt werden soll (z.B. Spinner oder Textbox, oder Textblock..)
        public override Type ComponentType
        {
            get { return base.ComponentType; }
        }

        protected override DesignItem CreateItem(DesignContext context)
        {
            object newInstance = null;
            if (context.Services.Selection.PrimarySelection != null)
            {
                var query = context.Services.Selection.PrimarySelection.Extensions.OfType<IPlacementChildGenerator>();
                if (query.Any())
                    newInstance = (query.First() as IPlacementChildGenerator).CreateDefaultNewChildInstance();
            }
            if (newInstance == null)
                newInstance = context.Services.ExtensionManager.CreateInstanceWithCustomInstanceFactory(ComponentType, null);

            DesignItem item = context.Services.Component.RegisterComponentForDesigner(newInstance);
            changeGroup = item.OpenGroup("Drop Control");

            if (item.View != null)
            {
                if (item.View is System.Windows.Controls.ContentControl)
                {
                    if ((item.View as System.Windows.Controls.ContentControl).Content != null)
                        (item.View as System.Windows.Controls.ContentControl).Content = null;
                }

                if ((item.View is VBImage) && (ACObject is ACClassDesign))
                {
                    VBStaticResourceExtension staticResource = new VBStaticResourceExtension();
                    string acUrl = ACObject.GetACUrl();
                    staticResource.VBContent = acUrl;
                    DesignItem newItemResourceExt = context.Services.Component.RegisterComponentForDesigner(staticResource);
                    item.Properties["Source"].SetValue(newItemResourceExt);
                    newItemResourceExt.Properties["VBContent"].SetValue(acUrl);
                }
                else if ((item.View is VBBorder) && (ACObject is ACClassDesign))
                {
                    VBStaticResourceExtension staticResource = new VBStaticResourceExtension();
                    string acUrl = ACObject.GetACUrl();
                    staticResource.VBContent = acUrl;
                    DesignItem newItemResourceExt = context.Services.Component.RegisterComponentForDesigner(staticResource);
                    item.Properties["Background"].SetValue(newItemResourceExt);
                    newItemResourceExt.Properties["ResourceKey"].SetValue(acUrl);
                }
                else if (item.View is IVBContent)
                {
                    DesignItemProperty itemProperty = item.Properties["VBContent"];
                    if (itemProperty != null)
                    {
                        itemProperty.SetValue(VBContent);
                    }

                    String vbXName = (item.View as IVBContent).GetVBContentAsXName();
                    if (!String.IsNullOrEmpty(vbXName) && (context.Services.Component.DesignItems != null))
                    {
                        try
                        {
                            vbXName = String.Format("{0}_{1}", vbXName, context.Services.Component.DesignItems.Where(c => (c.Component != null)
                                && (c.Component is FrameworkElement)
                                && !String.IsNullOrEmpty(c.Name)
                                && c.Name.StartsWith(vbXName)).Count());
                            item.Name = vbXName;
                        }
                        catch (Exception e)
                        {
                            string msg = e.Message;
                            if (e.InnerException != null && e.InnerException.Message != null)
                                msg += " Inner:" + e.InnerException.Message;

                            if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                                datamodel.Database.Root.Messages.LogException("DesignManagerControlTool", "CreateItem", msg);
                        }
                    }
                }
            }
            else if (item.Component != null)
            {
                if (item.Component is IVBContent)
                {
                    DesignItemProperty itemProperty = item.Properties["VBContent"];
                    if (itemProperty != null)
                    {
                        itemProperty.SetValue(VBContent);
                    }
                }
            }

            return item;
        }


        protected override void designPanel_DragOver(object sender, DragEventArgs e)
        {
            IACInteractiveObject dropObject = VBDragDrop.GetDropObject(e);
            if ((dropObject != null) && (sender is IDesignPanel))
            {
                IDesignPanel designPanel = (IDesignPanel)sender;
                Point pModel; // Umgebendes UIElement, wo das zu draggende Objekt gezogen wird
                DesignPanelHitTestResult hitTestResult;
                if (HitTestACElement(designPanel, e, out pModel, out hitTestResult))
                {
                    ACActionArgs acActionArgs = new ACActionArgs(dropObject, pModel.X, pModel.Y, Global.ElementActionType.Drop);

#if DEBUG
                    System.Diagnostics.Debug.WriteLine(hitTestResult.ModelHit.View.ToString());
#endif
                    bool isEnabled = VBDesigner.IsEnabledACActionToTarget(
                                hitTestResult.ModelHit.View is IACInteractiveObject ?
                                    hitTestResult.ModelHit.View as IACInteractiveObject :
                                    this.VBDesigner.VBDesignEditor, acActionArgs);

                    if (!isEnabled)
                    {
                        e.Effects = DragDropEffects.None;
                        e.Handled = true;
                        return;
                    }
                    if (acActionArgs.Handled)
                    {
                        e.Effects = acActionArgs.ElementAction == Global.ElementActionType.Drop ? DragDropEffects.Copy : DragDropEffects.None;
                        e.Handled = true;
                        return;
                    }
                }
            }
            base.designPanel_DragOver(sender, e);
        }

        protected override void designPanel_Drop(object sender, DragEventArgs e)
        {
            IACInteractiveObject dropObject = VBDragDrop.GetDropObject(e);
            if ((dropObject != null) && (sender is IDesignPanel))
            {
                IDesignPanel designPanel = (IDesignPanel)sender;
                Point pModel; // Umgebendes UIElement, wo das zu draggende Objekt gezogen wird
                DesignPanelHitTestResult hitTestResult;
                if (HitTestACElement(designPanel, e, out pModel, out hitTestResult))
                {
                    ACActionArgs acActionArgs = new ACActionArgs(dropObject, pModel.X, pModel.Y, Global.ElementActionType.Drop);
                    VBDesigner.ACActionToTarget(hitTestResult.ModelHit.View is IACInteractiveObject ? 
                                    hitTestResult.ModelHit.View as IACInteractiveObject : 
                                    this.VBDesigner.VBDesignEditor, acActionArgs);

                    if (acActionArgs.Handled)
                    {
                        e.Effects = DragDropEffects.None;
                        e.Handled = true;
                        base.designPanel_DragLeave(sender, e);
                        return;
                    }
                }
            }

            base.designPanel_Drop(sender, e);
        }

        protected override void designPanel_DragLeave(object sender, DragEventArgs e)
        {
            base.designPanel_DragLeave(sender, e);
        }

        protected override void MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (VBDesigner.CurrentDesign is ACClassDesign)
            {
                base.MouseDown(sender, e);
            }
            else
            {
                e.Handled = true;
            }
        }

        protected bool HitTestACElement(IDesignPanel designPanel, DragEventArgs e, out Point pModel, out DesignPanelHitTestResult result)
        {
            Point p = e.GetPosition(designPanel);
            designPanel.IsAdornerLayerHitTestVisible = false;
            result = designPanel.HitTest(p, false, true);
            designPanel.IsAdornerLayerHitTestVisible = true;
            if (result.ModelHit != null && result.ModelHit.View != null)
            {
                pModel = e.GetPosition(result.ModelHit.View);
                return true;
            }
            pModel = new Point();
            return false;
        }

        IACObject _ACObject;
        public IACObject ACObject
        {
            get { return _ACObject; }
            set { _ACObject = value; }
        }

        public string ACUrl
        {
            get;
            set;
        }

        public string VBContent
        {
            get
            {
                return VBDesigner.BuildVBContentFromACUrl(this.ACUrl, this.ACObject);
            }
        }
    }
}
