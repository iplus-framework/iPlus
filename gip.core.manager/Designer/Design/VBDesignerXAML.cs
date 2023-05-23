using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Collections;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using gip.core.datamodel;
using gip.core.autocomponent;
using gip.core.layoutengine;
using gip.ext.design;
using gip.ext.designer.Services;
using gip.ext.designer.Controls;

namespace gip.core.manager
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'User interface designer'}de{'Oberflächendesigner'}", Global.ACKinds.TACBSOGlobal, Global.ACStorableTypes.NotStorable, false, false)]
    public class VBDesignerXAML : VBDesigner
    {
        #region c´tors
        public VBDesignerXAML(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            ShowXMLEditor = true;
            PropertyWindowVisible = true;
            LogicalTreeWindowVisible = true;
            _SaveDesignFromOtherDBContext = false;
            _AutoLoadProperties = false;
            _AutoLoadMethods = false;
            _AutoLoadTexts = false;
            _PropertySearchDepth = 2;
            _CurrentAvailableElementClicked = 0;
            _CurrentAvailablePropertyClicked = 0;
            _CurrentAvailableMethodClicked = 0;
            _CurrentAvailableTextClicked = 0;
            if (ParentACComponent != null)
            {
                (ParentACComponent as ACComponent).ACSaveChangesExecuted += ParentComponent_ACSaveChangesExecuted;
                if (ParentACComponent.Database != null)
                    ParentACComponent.Database.ACChangesExecuted += ParentACComponentDatabase_ACChangesExecuted;
            }
           
            return base.ACInit(startChildMode);
        }



        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            if (ParentACComponent != null)
            {
                (ParentACComponent as ACComponent).ACSaveChangesExecuted -= ParentComponent_ACSaveChangesExecuted;
                if (ParentACComponent.Database != null)
                    ParentACComponent.Database.ACChangesExecuted -= ParentACComponentDatabase_ACChangesExecuted;
            }
            _SaveDesignFromOtherDBContext = false;
            _CurrentAvailableProperty = null;
            _SelectedAvailableProperty = null;
            _CurrentAvailableProperty = null;
            _CurrentAvailableMethod = null;
            _SelectedAvailableMethod = null;
            _CurrentAvailableText = null;
            _SelectedAvailableText = null;
            _VBDesignControl = null;
            return base.ACDeInit(deleteACClassTask);
        }

        #endregion

        #region Manager->ObjectLayoutTree
        /// <summary>
        /// Liefert eine Liste von ObjectLayoutEntrys zurück, die
        /// alle in der Visualisierung verwendeten grafischen Elemente enthält
        ///  return _WorkflowWorkflow.GetAvailableElements(CurrentACClassMethod == null ? null : CurrentACClassMethod.ACClass, CurrentACClassMethod);
        /// </summary>
        /// <returns>
        ///   <br />
        /// </returns>
        public override void UpdateAvailableElements()
        {
            // Ist mir abends eingefallen, dass Tree-View auf PropertyChanged reagiert => Instanz-Objekt muss sich ändern
            // Tree-View müsen wir sowieso nochmal genau untersuchen
            _AvailableElementList = new ObservableCollection<IACObject>();
            //_AvailableElementList.Clear();
            if (CurrentDesignContext != null)
            {
                if (!(CurrentDesignContext is IRoot))
                {
                    GetAvailableElements(CurrentDesignContext, _AvailableElementList);
                    return;
                }
            }

            if (ParentACComponent == null)
                return;


            // Aktuelles BSO, das designed werden soll
            if (ParentACComponent.ACType is ACClass)
                InsertACModelClass(_AvailableElementList, ParentACComponent.ACType as ACClass);

            // Global Functions
            var acClassRoot = this.Root.ACType as ACClass;
            var queryGlobalFunc = Database.ContextIPlus.ACClass.Where(c => c.ACKindIndex == (short)Global.ACKinds.TACEnvironment
                                                              && c.ACProject.ACProjectID == acClassRoot.ACProjectID);
            if (queryGlobalFunc.Any())
            {
                foreach (ACClass globalFunctionClass in queryGlobalFunc)
                {
                    InsertACModelClass(_AvailableElementList, globalFunctionClass);
                }
            }

            // Visualisierbare projekte
            foreach (var acProject in Database.ContextIPlus.ACProject.Where(c => c.IsVisualisationEnabled).OrderBy(c => c.ACProjectName))
            {
                InsertACModelClass(_AvailableElementList, acProject.RootClass);
            }

            InsertImages(_AvailableElementList);
            InsertVBControls(_AvailableElementList);
        }

        /// <summary>Wird aufgerufen von Komponenten, die den Designer-Context ändern
        /// z.B. iplus-Studio durch auswahl der Klasse</summary>
        /// <param name="currentContextOfDesignEditor"></param>
        /// <param name="objectLayoutEntrys"></param>
        /// <returns>
        ///   <br />
        /// </returns>
        public void GetAvailableElements(IACObject currentContextOfDesignEditor, ObservableCollection<IACObject> objectLayoutEntrys)
        {
            objectLayoutEntrys.Clear();
            // Falls DataContext "Tote-Welt" (ACClass)
            if (currentContextOfDesignEditor is IACType)
                InsertACModelClass(objectLayoutEntrys, (currentContextOfDesignEditor as IACType).ValueTypeACClass);
            // Sonst lebendes Objekt 
            else if (currentContextOfDesignEditor is IACComponent)
                InsertACModelClass(objectLayoutEntrys, (currentContextOfDesignEditor as IACComponent).ACType as ACClass);
            _CurrentDesignContext = currentContextOfDesignEditor;
            InsertVBControls(objectLayoutEntrys);
        }

        public override IEnumerable<IACObject> GetAvailableTools()
        {
            ACObjectItemList objectLayoutEntrys = new ACObjectItemList();
            objectLayoutEntrys.Add(new DesignManagerToolItem(gip.core.datamodel.Database.Root.Environment.TranslateText(this, "Pointer"), PointerTool.Instance, "DesignPointer"));
            objectLayoutEntrys.Add(new DesignManagerToolItem(gip.core.datamodel.Database.Root.Environment.TranslateText(this, "Connector"), new ConnectTool(this), "DesignConnector"));
            objectLayoutEntrys.Add(new DesignManagerToolItem(gip.core.datamodel.Database.Root.Environment.TranslateText(this, "Line"), new DrawingToolForLine(), "DesignLine"));
            objectLayoutEntrys.Add(new DesignManagerToolItem(gip.core.datamodel.Database.Root.Environment.TranslateText(this, "Rectangle"), new DrawingToolForRectangle(), "DesignRect"));
            objectLayoutEntrys.Add(new DesignManagerToolItem(gip.core.datamodel.Database.Root.Environment.TranslateText(this, "Ellipse"), new DrawingToolForEllipse(), "DesignEllipse"));
            objectLayoutEntrys.Add(new DesignManagerToolItem(gip.core.datamodel.Database.Root.Environment.TranslateText(this, "Polyline"), new DrawingToolForPolyline(), "DesignPolyline"));
            objectLayoutEntrys.Add(new DesignManagerToolItem(gip.core.datamodel.Database.Root.Environment.TranslateText(this, "Polygon"), new DrawingToolForPolygon(), "DesignPolygon"));
            objectLayoutEntrys.Add(new DesignManagerToolItem(gip.core.datamodel.Database.Root.Environment.TranslateText(this, "EditPoints"), new DrawingToolEditPoints(), "DesignEditPoints"));
            return objectLayoutEntrys;
        }

        private void InsertObjectLayouts(ObservableCollection<IACObject> objectLayoutEntrys, string layoutGroup, IEnumerable<ACClassDesign> acClassDesigns)
        {
            if (!acClassDesigns.Any())
                return;
            ACObjectItem objectLayoutGroup = new ACObjectItem(layoutGroup);
            objectLayoutEntrys.Add(objectLayoutGroup);

            foreach (var entry in acClassDesigns)
            {
                ACObjectItem item = new ACObjectItem(entry, entry.ACCaption); 
                objectLayoutGroup.Add(item);
            }
        }

        private void InsertVBControls(ObservableCollection<IACObject> objectLayoutEntrys)
        {
            ACObjectItem objectLayoutGroup = new ACObjectItem("Controls", "Controls");
            objectLayoutEntrys.Add(objectLayoutGroup);
            objectLayoutGroup.Add(new ACObjectItem(""));

            //var vbControlList = DesignManagerControlTool.GetVBControlList(this.Database.ContextIPlus).OrderBy(c => c.ACIdentifier);
            //foreach (ACClass acControlClass in vbControlList)
            //{
            //    ACObjectItem toolItem = new ACObjectItem(acControlClass, acControlClass.ACCaption);
            //    objectLayoutGroup.Add(toolItem);
            //}
        }

        private void InsertImages(ObservableCollection<IACObject> objectLayoutEntrys)
        {
            ACObjectItem objectLayoutGroup = new ACObjectItem("Images", "Images");
            objectLayoutEntrys.Add(objectLayoutGroup);
            objectLayoutGroup.Add(new ACObjectItem(""));
        }

        private void InsertACModelClass(ObservableCollection<IACObject> objectLayoutEntrys, ACClass acClass)
        {
            ACObjectItem objectLayoutGroup = new ACObjectItem(acClass, acClass.ACCaption, acClass.GetACUrlComponent(this.CurrentDesignContext));
            objectLayoutEntrys.Add(objectLayoutGroup);
            //InsertSubACProjectClass(objectLayoutGroup, acClass);
            if (acClass.ACClass_ParentACClass.
                        Where(c => c.ACKindIndex == (short)Global.ACKinds.TPAModule
                            || c.ACKindIndex == (short)Global.ACKinds.TPAProcessModule
                            || c.ACKindIndex == (short)Global.ACKinds.TPAProcessFunction
                            || c.ACKindIndex == (short)Global.ACKinds.TPABGModule
                            || c.ACKindIndex == (short)Global.ACKinds.TPAProcessModuleGroup
                            || c.ACKindIndex == (short)Global.ACKinds.TACDAClass).Any())
            {
                objectLayoutGroup.Add(new ACObjectItem(""));
            }
        }

        private void InsertSubACProjectClass(ACObjectItem treeEntryRoot, ACClass acProjectClassRoot)
        {
            treeEntryRoot.Remove(treeEntryRoot.Items.FirstOrDefault());
            var query = acProjectClassRoot.ACClass_ParentACClass.
                Where(c => c.ACKindIndex == (short) Global.ACKinds.TPAModule
                    || c.ACKindIndex == (short) Global.ACKinds.TPAProcessModule
                    || c.ACKindIndex == (short)Global.ACKinds.TPAProcessFunction
                    || c.ACKindIndex == (short)Global.ACKinds.TPABGModule
                    || c.ACKindIndex == (short)Global.ACKinds.TPAProcessModuleGroup
                    || c.ACKindIndex == (short) Global.ACKinds.TACDAClass).
                OrderBy(c => c.ACIdentifier);
            
            foreach (ACClass acClass in query)
            {
                ACObjectItem objectLayoutGroup = new ACObjectItem(acClass, acClass.ACCaption, acClass.GetACUrlComponent(this.CurrentDesignContext)); 
                treeEntryRoot.Add(objectLayoutGroup);
                if (acClass.ACClass_ParentACClass.Any())
                {
                    //InsertSubACProjectClass(objectLayoutGroup, acClass);
                    objectLayoutGroup.Add(new ACObjectItem(""));
                }
            }
        }

        public IEnumerable<IACObject> GetAvailableProperties()
        {
            if (CurrentAvailableElement == null)
                return new List<ACObjectItem>();
            if (AutoLoadProperties && CurrentAvailableElement.ACObject is ACClass)
            {
                ACObjectItemList objectLayoutEntrys = new ACObjectItemList();
                ACClass acClass = CurrentAvailableElement.ACObject as ACClass;
                foreach (ACClassProperty acClassProperty in acClass.Properties)
                {
                    if ((acClassProperty.ACClass.ACIdentifier == "ACComponent")
                        && (acClassProperty.ACIdentifier != Const.ACState)
                        && (acClassProperty.ACIdentifier != "ACProcessPhase"))
                        continue;
                    if (acClassProperty.ACPropUsage == Global.ACPropUsages.Current ||
                        acClassProperty.ACPropUsage == Global.ACPropUsages.Selected ||
                        acClassProperty.ACPropUsage == Global.ACPropUsages.Property ||
                        acClassProperty.ACPropUsage == Global.ACPropUsages.ConnectionPoint ||
                        acClassProperty.ACPropUsage == Global.ACPropUsages.EventPoint ||
                        acClassProperty.ACPropUsage == Global.ACPropUsages.EventPointSubscr ||
                        acClassProperty.ACPropUsage == Global.ACPropUsages.AsyncMethodPoint)
                    {
                        string acUrlRelative = acClassProperty.ACIdentifier; //.GetACUrlComponent(CurrentAvailableElement.ACObject);
                        ACObjectItem objectLayoutGroup = new ACObjectItem(acClassProperty, acClassProperty.ACCaption, acUrlRelative);
                        InsertACClassPropertySub(objectLayoutGroup, acClassProperty.ValueTypeACClass, acUrlRelative, 0);
                        objectLayoutEntrys.Add(objectLayoutGroup);
                    }
                }
                return objectLayoutEntrys;
            }
            return new List<ACObjectItem>();
        }

        private void InsertACClassPropertySub(ACObjectItem treeEntryRoot, ACClass acClass, string acUrlParent, int recursionDepth)
        {
            if (acClass == null)
                return;
            recursionDepth++;
            if (recursionDepth >= PropertySearchDepth)
                return;
            foreach (ACClassProperty acClassProperty in acClass.Properties)
            {
                ACObjectItem objectLayoutGroup = new ACObjectItem(acClassProperty, acClassProperty.ACCaption, acUrlParent + "\\" + acClassProperty.ACIdentifier);
                InsertACClassPropertySub(objectLayoutGroup, acClassProperty.ValueTypeACClass, acUrlParent + "\\" + acClassProperty.ACIdentifier, recursionDepth);
                treeEntryRoot.Add(objectLayoutGroup);
            }
        }

        public IEnumerable<IACObject> GetAvailableMethods()
        {
            if (CurrentAvailableElement == null)
                return new List<ACObjectItem>();
            if (AutoLoadMethods && CurrentAvailableElement.ACObject is ACClass)
            {
                ACObjectItemList objectLayoutEntrys = new ACObjectItemList();
                ACClass acClass = CurrentAvailableElement.ACObject as ACClass;
                foreach (ACClassMethod acClassMethod in acClass.Methods)
                {
                    // Alle Methoden, da auch Events per VBDelegateExtension verbunden werden kann
                    //if (acClassMethod.IsCommand)
                    {
                        string acUrlRelative = acClassMethod.ACIdentifier; //.GetACUrlComponent(CurrentAvailableElement.ACObject);

                        ACObjectItem objectLayoutGroup = new ACObjectItem(acClassMethod, acClassMethod.ACCaption, "!" + acUrlRelative); 
                        objectLayoutEntrys.Add(objectLayoutGroup);
                    }
                }
                return objectLayoutEntrys;
            }
            return new List<ACObjectItem>();
        }

        public IEnumerable<IACObject> GetAvailableTexts()
        {
            if (CurrentAvailableElement == null)
                return new List<ACObjectItem>();
            if (AutoLoadTexts && CurrentAvailableElement.ACObject is ACClass)
            {
                ACObjectItemList objectLayoutEntrys = new ACObjectItemList();
                ACClass acClass = CurrentAvailableElement.ACObject as ACClass;
                foreach (ACClassMessage acClassText in acClass.Messages)
                {
                    string acUrlRelative = acClassText.ACIdentifier; //.GetACUrlComponent(CurrentAvailableElement.ACObject);
                    ACObjectItem objectLayoutGroup = new ACObjectItem(acClassText, acClassText.ACCaption, "§" + acUrlRelative);
                    objectLayoutEntrys.Add(objectLayoutGroup);
                }
                return objectLayoutEntrys;
            }
            return new List<ACObjectItem>();
        }

        [ACMethodInfo("","en{'Expand'}de{'Expand'}",999)]
        public void OnItemExpand(ACObjectItem item)
        {
            if (item.ACObject is ACClass)
                InsertSubACProjectClass(item, item.ACObject as ACClass);
            else if(item.ACIdentifier == "Controls")
            {
                item.Remove(item.Items.FirstOrDefault());
                var vbControlList = DesignManagerControlTool.GetVBControlList(this.Database.ContextIPlus).OrderBy(c => c.ACIdentifier);
                foreach (ACClass acControlClass in vbControlList)
                {
                    ACObjectItem toolItem = new ACObjectItem(acControlClass, acControlClass.ACCaption);
                    item.Add(toolItem);
                }
            }
            else if (item.ACIdentifier == "Images")
            {
                item.Remove(item.Items.FirstOrDefault());
                ACProject acProjectLib = Database.ContextIPlus.ACProject.Where(c => c.ACProjectTypeIndex == (int)Global.ACProjectTypes.ClassLibrary).First();
                var query = Database.ContextIPlus.ACClassDesign.Where(c =>
                           ((c.ACKindIndex == (short)Global.ACKinds.DSDesignLayout && c.ACUsageIndex == (int)Global.ACUsages.DULayout && c.IsResourceStyle == true)
                            || (c.ACKindIndex == (short)Global.ACKinds.DSBitmapResource)
                            || (c.ACUsageIndex == (int)Global.ACUsages.DUBitmap))
                        && ((c.ACClass.ACProject.ACProjectID == acProjectLib.ACProjectID)
                            || (c.ACClass.ACKindIndex == (int)Global.ACKinds.TACEnvironment)))
                            .OrderBy(c => c.ACIdentifier);
                foreach (var entry in query)
                {
                    ACObjectItem subItem = new ACObjectItem(entry, entry.ACCaption);
                    item.Add(subItem);
                }
            }
        }

        #endregion

        #region IACComponentDesignManager
        private bool _SaveDesignFromOtherDBContext = false;
        public override string DesignXAML
        {
            get
            {
                if (CurrentDesign == null)
                    return null;
                return CurrentDesign.XMLDesign;
// ???                return CurrentDesign.GetDesignXAML(this.ElementACComponent, VBContentDesign);
            }
            set
            {
                if (CurrentDesign == null || !(CurrentDesign is ACClassDesign))
                    return;
                bool valueChanged = false;
                if (CurrentDesign.XMLDesign != value)
                    valueChanged = true;
                (CurrentDesign as ACClassDesign).XMLDesign = value;
                ACClassDesign acClassDesign = (CurrentDesign as ACClassDesign);
                if (valueChanged && acClassDesign != null)
                {
                    // Falls anderer Datenbank-Context
                    if (acClassDesign.GetObjectContext<Database>() != this.Database.ContextIPlus)
                    {
                        _SaveDesignFromOtherDBContext = true;
                    }
                }
                OnPropertyChanged("DesignXAML");
            }
        }

        void ParentComponent_ACSaveChangesExecuted(object sender, EventArgs e)
        {
            if (VBDesignEditor != null)
            {
                ((VBDesignEditor)VBDesignEditor).SaveToXAML();
            }
            if (_SaveDesignFromOtherDBContext)
            {
                ACClassDesign acClassDesign = (CurrentDesign as ACClassDesign);
                if (acClassDesign != null)
                {
                    acClassDesign.GetObjectContext().ACSaveChanges();
                }
            }
            _SaveDesignFromOtherDBContext = false;
        }

        private void ParentACComponentDatabase_ACChangesExecuted(object sender, ACChangesEventArgs e)
        {
            if (e.ChangeType == ACChangesEventArgs.ACChangesType.ACUndoChanges && e.Succeeded)
            {
                if (VBDesignEditor != null)
                {
                    ((VBDesignEditor)VBDesignEditor).ObjectsInDesignViewChanged = false;
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

        /// <summary>
        /// Called from a WPF-Control inside it's ACAction-Method when a relevant interaction-event as occured (e.g. Drag-And-Drop).
        /// </summary>
        /// <param name="targetVBDataObject">The target object that was involved in the interaction event.</param>
        /// <param name="actionArgs">Information about the type of interaction and the source.</param>
        public override void ACActionToTarget(IACInteractiveObject targetVBDataObject, ACActionArgs actionArgs)
        {
            if (targetVBDataObject is IBindingDropHandler)
            {
                IBindingDropHandler dropHandler = targetVBDataObject as IBindingDropHandler;
                var query = actionArgs.DropObject.ACContentList.Where(c => c is ACObjectItem);
                if (query.Any())
                {
                    ACObjectItem currentTool = query.First() as ACObjectItem;
                    string vbContent = this.BuildVBContentFromACUrl(currentTool.ACUrlRelative, currentTool.ACObject);
                    if (!String.IsNullOrEmpty(vbContent))
                    {
                        if (currentTool.ACObject is ACClassMethod)
                        {
                            int methodSign = vbContent.IndexOf('!');
                            bool isGlobalFunc = false;
                            if (methodSign > 0)
                            {
                                string left = vbContent.Substring(0, methodSign);
                                string urlOfEnvManager = this.Root.Environment.GetACUrl();
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
            base.ACActionToTarget(targetVBDataObject, actionArgs);
        }

        /// <summary>
        /// Called from a WPF-Control when a relevant interaction-event as occured (e.g. Drag-And-Drop) and the related component should check if this interaction-event should be handled.
        /// </summary>
        /// <param name="targetVBDataObject">The target object that was involved in the interaction event.</param>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        /// <returns><c>true</c> if ACAction can be invoked otherwise, <c>false</c>.</returns>
        public override bool IsEnabledACActionToTarget(IACInteractiveObject targetVBDataObject, ACActionArgs actionArgs)
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

        
        /// <summary>
        /// Creates a Edge between two points
        /// </summary>
        /// <param name="sourceVBConnector">The source VB connector.</param>
        /// <param name="targetVBConnector">The target VB connector.</param>
        public override void CreateEdge(IVBConnector sourceVBConnector, IVBConnector targetVBConnector)
        {
            VisualChangeList.Clear();
            string sourceACConnector = sourceVBConnector.ParentVBControl.ACIdentifier + "\\" + sourceVBConnector.VBContent;
            string targetACConnector = targetVBConnector.ParentVBControl.ACIdentifier + "\\" + targetVBConnector.VBContent;

            //var x = sourceVBConnector.ParentACObject.GetACUrl(); // \Produktionslinie1\S002\SN
            //IACWorkflowNode sourceVBVisual = sourceVBConnector.ParentACObject as IACWorkflowNode;
            //if (sourceVBVisual == null)
            //{
            //    sourceVBVisual = sourceVBConnector.ParentACObject.ACContentList.First() as IACWorkflowNode;
            //}
            //ACClassProperty sourceACClassProperty = null;
            //IACWorkflowNode targetVBVisual = targetVBConnector.ParentACObject as IACWorkflowNode;
            //if (targetVBVisual == null)
            //{
            //    targetVBVisual = targetVBConnector.ParentACObject.ACContentList.First() as IACWorkflowNode;
            //}
            //ACClassProperty targetACClassProperty = null;

            //sourceACClassProperty = sourceVBVisual.MyPoint(sourceVBConnector.VBContent);

            //targetACClassProperty = targetVBVisual.MyPoint(targetVBConnector.VBContent);

            //WFCreateWFEdge(sourceVBVisual, sourceACClassProperty, targetVBVisual, targetACClassProperty);
            AddToVisualChangeList(null, VBDesigner.LayoutActionType.InsertEdge, sourceACConnector, targetACConnector);


            UpdateVisual();
        }

        /// <summary>
        /// Checks if a Edge can be created between two points
        /// </summary>
        /// <param name="sourceVBConnector">The source VB connector.</param>
        /// <param name="targetVBConnector">The target VB connector.</param>
        /// <returns><c>true</c> if is enabled; otherwise, <c>false</c>.</returns>
        public override bool IsEnabledCreateEdge(IVBConnector sourceVBConnector, IVBConnector targetVBConnector)
        {
            if (sourceVBConnector == null)
                return true;
            //ACClassProperty sourceACClassProperty = sourceACObject.MyPoint(sourcePropertyName);
            var sourceACClass = sourceVBConnector.ParentACObject?.ACContentList?.FirstOrDefault() as ACClass;
            var targetACClass = targetVBConnector.ParentACObject?.ACContentList?.FirstOrDefault() as ACClass;

            ACClassProperty sourceACClassProperty = FindMyPoint(sourceACClass, sourceVBConnector.VBContent);
            ACClassProperty targetACClassProperty = FindMyPoint(targetACClass, targetVBConnector.VBContent);


            if (sourceACClassProperty == null || sourceACClassProperty.ACPropUsage != Global.ACPropUsages.ConnectionPoint)
                return false;

            if (targetACClassProperty == null || targetACClassProperty.ACPropUsage != Global.ACPropUsages.ConnectionPoint)
                return false;

            if ((sourceVBConnector.ParentVBControl == null) || (targetVBConnector.ParentVBControl == null))
                return false;
            if (String.IsNullOrEmpty(sourceVBConnector.ParentVBControl.ACIdentifier)
                || String.IsNullOrEmpty(targetVBConnector.ParentVBControl.ACIdentifier))
                return false;
            return true;
        }


        /// <summary>Asks this design manager if he can create edges</summary>
        /// <returns><c>true</c> if this instance can create edges; otherwise, <c>false</c>.</returns>
        public override bool CanManagerCreateEdges()
        {
            return false;
        }


        private ACClassProperty FindMyPoint(ACClass acClass, string acUrlPoint)
        {
            if (String.IsNullOrEmpty(acUrlPoint) || acClass == null)
                return null;
            ACUrlHelper helper = new ACUrlHelper(acUrlPoint);
            if (String.IsNullOrEmpty(helper.NextACUrl))
                return acClass.GetPoint(acUrlPoint);
            else if (!String.IsNullOrEmpty(helper.ACUrlPart))
            {
                var pACClass = acClass.ACClass_ParentACClass.Where(c => c.ACIdentifier == helper.ACUrlPart).FirstOrDefault();
                if (pACClass == null)
                    return null;
                return FindMyPoint(pACClass, helper.NextACUrl);
            }
            return null;
        }
        #endregion

        #region DropDesignItem
        public void DropDesignItem(Global.ElementActionType action, IACInteractiveObject dropObject, IACInteractiveObject targetVBDataObject, double x, double y)
        {
            var query = dropObject.ACContentList.Where(c => c is ACClass);
            if (!query.Any())
                return;
            ACClass dropACClass = dropObject.GetACValue(typeof(ACClass)) as ACClass;
            if (dropACClass == null)
                return;

            switch (action)
            {
                case Global.ElementActionType.Drop:
                    ACClassDesign dropACClassDesign = dropObject.GetACValue(typeof(ACClassDesign)) as ACClassDesign;

                    if (dropACClass == null && dropACClassDesign == null)
                        return;

                    ACClassDesign acClassDesign = CurrentDesign as ACClassDesign;
                    XElement x1 = Layoutgenerator.LoadLayoutAsXElement(acClassDesign.XMLDesign);
                    //x1.Elements();

                    //CurrentACVisualItem = _ACProjectManager.NewACVisualItem(CurrentVisualisation);

                    //CurrentACVisualItem.VisualWidth = 80;
                    //CurrentACVisualItem.VisualHeight = 30;

                    //CurrentACVisualItem.VisualTop = y - CurrentACVisualItem.VisualHeight / 2;
                    //CurrentACVisualItem.VisualLeft = x - CurrentACVisualItem.VisualWidth / 2;

                    //if (dropACClass != null)
                    //{
                    //    CurrentACVisualItem.ACClass = dropACClass;
                    //    if (CurrentACVisualItem.ACClass != null)
                    //    {
                    //        ACClassDesign acLayout = CurrentACVisualItem.ACClass.ACType.GetDesign(CurrentACVisualItem.ACClass, Global.ACKinds.DSControlLayout);
                    //        if (acLayout != null)
                    //        {
                    //            CurrentACVisualItem.VisualHeight = acLayout.VisualHeight;
                    //            CurrentACVisualItem.VisualWidth = acLayout.VisualWidth;
                    //        }
                    //    }
                    //} 
                    //else
                    //    if (dropACClassDesign != null)
                    //{
                    //    ACClassDesign acClassDesign = dropACClassDesign as ACClassDesign;
                    //    CurrentACVisualItem.ACClassDesign = acClassDesign;
                    //    CurrentACVisualItem.ACCaption = acClassDesign.ACIdentifier;
                    //    //CurrentACVisualItem.VisualConfigurationXML = acClassDesign.VisualConfigurationXML;
                    //    CurrentACVisualItem.VisualHeight = acClassDesign.VisualHeight;
                    //    CurrentACVisualItem.VisualWidth = acClassDesign.VisualWidth; 
                    //}
                    OnPropertyChanged("LoadedDesignItemList");
                    break;
            }
            PostExecute("DropDesignItem");
        }
        public bool IsEnabledDropDesignItem(Global.ElementActionType action, IACInteractiveObject dropObject, IACInteractiveObject targetVBDataObject, double x, double y)
        {
            var query = dropObject.ACContentList.Where(c => c is ACClass);
            if (query.Any())
                return true;

            return false; // dropObject.GetACValue(typeof(IACObjectWithBinding)) != null;
        }
        #endregion

        #region BSO->ACProperty
        #region Controller Properties
        private bool _AutoLoadProperties = false;
        [ACPropertyInfo(9999)]
        public bool AutoLoadProperties
        {
            get
            {
                return _AutoLoadProperties;
            }

            set
            {
                _AutoLoadProperties = value;
                OnPropertyChanged("AutoLoadProperties");
                OnPropertyChanged("AvailablePropertyList");
                OnPropertyChanged("AvailableTextList");
            }
        }

        private bool _AutoLoadMethods = false;
        [ACPropertyInfo(9999)]
        public bool AutoLoadMethods
        {
            get
            {
                return _AutoLoadMethods;
            }

            set
            {
                _AutoLoadMethods = value;
                OnPropertyChanged("AutoLoadMethods");
                OnPropertyChanged("AvailableMethodList");
            }
        }

        private bool _AutoLoadTexts = false;
        [ACPropertyInfo(9999)]
        public bool AutoLoadTexts
        {
            get
            {
                return _AutoLoadTexts;
            }

            set
            {
                _AutoLoadTexts = value;
                OnPropertyChanged("AutoLoadTexts");
                OnPropertyChanged("AvailableTextList");
            }
        }

        private int _PropertySearchDepth = 2;
        [ACPropertyInfo(9999)]
        public int PropertySearchDepth
        {
            get
            {
                return _PropertySearchDepth;
            }
            set
            {
                _PropertySearchDepth = value;
                OnPropertyChanged("PropertySearchDepth");
                OnPropertyChanged("AvailablePropertyList");
                OnPropertyChanged("AvailableTextList");
            }
        }
        #endregion

        #region Available Elements
        public override void OnCurrentAvailableElementChanged()
        {
            bool currentChanged = false;
            if (_CurrentAvailableMethod != null)
                currentChanged = true;
            _CurrentAvailableMethod = null;
            if (currentChanged)
                OnPropertyChanged("CurrentAvailableMethod");

            currentChanged = false;
            if (_CurrentAvailableProperty != null)
                currentChanged = true;
            _CurrentAvailableProperty = null;
            if (currentChanged)
                OnPropertyChanged("CurrentAvailableProperty");

            currentChanged = false;
            if (_CurrentAvailableText != null)
                currentChanged = true;
            _CurrentAvailableText = null;
            if (currentChanged)
                OnPropertyChanged("CurrentAvailableText");

            OnPropertyChanged("AvailablePropertyList");
            OnPropertyChanged("AvailableMethodList");
            OnPropertyChanged("AvailableTextList");
        }

        private int _CurrentAvailableElementClicked;
        [ACPropertyInfo(9999)]
        public int CurrentAvailableElementClicked
        {
            get
            {
                return _CurrentAvailableElementClicked;
            }
            set
            {
                _CurrentAvailableElementClicked = value;
                if ((ToolService != null) && (CurrentAvailableElement != null))
                {
                    if (CurrentAvailableElement is DesignManagerToolItem)
                    {
                        ITool newTool = null;
                        newTool = (CurrentAvailableElement as DesignManagerToolItem).CreateControlTool;
                        if ((newTool != null) && (ToolService as IToolService).CurrentTool != newTool)
                            (ToolService as IToolService).CurrentTool = newTool;
                    }
                }
            }
        }

        #endregion

        #region Available Properties
        [ACPropertyList(9999, "AvailableProperty")]
        public IEnumerable<IACObject> AvailablePropertyList
        {
            get
            {
                return GetAvailableProperties();
            }
        }

        ACObjectItem _CurrentAvailableProperty = null;
        [ACPropertyCurrent(9999, "AvailableProperty")]
        public ACObjectItem CurrentAvailableProperty
        {
            get
            {
                return _CurrentAvailableProperty;
            }
            set
            {
                bool currentChanged = false;
                if (_CurrentAvailableMethod != null)
                    currentChanged = true;
                _CurrentAvailableMethod = null;
                if (currentChanged)
                    OnPropertyChanged("_CurrentAvailableMethod");

                if (_CurrentAvailableText != null)
                    currentChanged = true;
                _CurrentAvailableText = null;
                if (currentChanged)
                    OnPropertyChanged("CurrentAvailableText");

                if (_CurrentAvailableProperty != value)
                {
                    _CurrentAvailableProperty = value;
                    OnPropertyChanged("CurrentAvailableProperty");
                }
                if (ToolService != null)
                {
                    ITool newTool = null;
                    if (_CurrentAvailableProperty is DesignManagerToolItem)
                    {
                        newTool = (_CurrentAvailableProperty as DesignManagerToolItem).CreateControlTool;
                    }
                    else if (value != null && value.ACObject != null)
                    {
                        newTool = DesignManagerControlTool.CreateNewInstance(this, _CurrentAvailableProperty.ACObject, _CurrentAvailableProperty.ACUrlRelative);
                        //DesignManagerToolItem designManagerToolItem = new DesignManagerToolItem(_CurrentAvailableProperty.ACObject, _CurrentAvailableProperty.ACCaption, , this);
                        //newTool = designManagerToolItem.CreateControlTool;
                    }
                    (ToolService as IToolService).CurrentTool = newTool ?? (ToolService as IToolService).PointerTool;
                }
            }
        }

        private int _CurrentAvailablePropertyClicked;
        [ACPropertyInfo(9999)]
        public int CurrentAvailablePropertyClicked
        {
            get
            {
                return _CurrentAvailablePropertyClicked;
            }
            set
            {
                _CurrentAvailablePropertyClicked = value;
                if ((ToolService != null) && (_CurrentAvailableProperty != null))
                {
                    if (_CurrentAvailableProperty is DesignManagerToolItem)
                    {
                        ITool newTool = null;
                        newTool = (_CurrentAvailableProperty as DesignManagerToolItem).CreateControlTool;
                        if ((newTool != null) && (ToolService as IToolService).CurrentTool != newTool)
                            (ToolService as IToolService).CurrentTool = newTool;
                    }
                }
            }
        }


        ACObjectItem _SelectedAvailableProperty;
        [ACPropertySelected(9999, "AvailableProperty")]
        public ACObjectItem SelectedAvailableProperty
        {
            get
            {
                return _SelectedAvailableProperty;
            }
            set
            {
                _SelectedAvailableProperty = value;
                OnPropertyChanged("SelectedAvailableProperty");
            }
        }
        #endregion

        #region Available Methods
        [ACPropertyList(9999, "AvailableMethod")]
        public IEnumerable<IACObject> AvailableMethodList
        {
            get
            {
                return GetAvailableMethods();
            }
        }

        ACObjectItem _CurrentAvailableMethod = null;
        [ACPropertyCurrent(9999, "AvailableMethod")]
        public ACObjectItem CurrentAvailableMethod
        {
            get
            {
                return _CurrentAvailableMethod;
            }
            set
            {
                bool currentChanged = false;
                if (_CurrentAvailableProperty != null)
                    currentChanged = true;
                _CurrentAvailableProperty = null;
                if (currentChanged)
                    OnPropertyChanged("CurrentAvailableProperty");

                if (_CurrentAvailableText != null)
                    currentChanged = true;
                _CurrentAvailableText = null;
                if (currentChanged)
                    OnPropertyChanged("CurrentAvailableText");

                if (_CurrentAvailableMethod != value)
                {
                    _CurrentAvailableMethod = value;
                    OnPropertyChanged("CurrentAvailableMethod");
                }
                if (ToolService != null)
                {
                    ITool newTool = null;
                    if (_CurrentAvailableMethod is DesignManagerToolItem)
                    {
                        newTool = (_CurrentAvailableMethod as DesignManagerToolItem).CreateControlTool;
                    }
                    else if (value != null && value.ACObject != null)
                    {
                        newTool = DesignManagerControlTool.CreateNewInstance(this, _CurrentAvailableMethod.ACObject, _CurrentAvailableMethod.ACUrlRelative);
//                        DesignManagerToolItem designManagerToolItem = new DesignManagerToolItem(_CurrentAvailableMethod.ACObject, _CurrentAvailableMethod.ACCaption, _CurrentAvailableMethod.ACUrlRelative, this);
//                        newTool = designManagerToolItem.CreateControlTool;
                    }
                    (ToolService as IToolService).CurrentTool = newTool ?? (ToolService as IToolService).PointerTool;
                }
            }
        }

        private int _CurrentAvailableMethodClicked;
        [ACPropertyInfo(9999)]
        public int CurrentAvailableMethodClicked
        {
            get
            {
                return _CurrentAvailableMethodClicked;
            }
            set
            {
                _CurrentAvailableMethodClicked = value;
                if ((ToolService != null) && (_CurrentAvailableMethod != null))
                {
                    if (_CurrentAvailableMethod is DesignManagerToolItem)
                    {
                        ITool newTool = null;
                        newTool = (_CurrentAvailableMethod as DesignManagerToolItem).CreateControlTool;
                        if ((newTool != null) && (ToolService as IToolService).CurrentTool != newTool)
                            (ToolService as IToolService).CurrentTool = newTool;
                    }
                }
            }
        }



        ACObjectItem _SelectedAvailableMethod;
        [ACPropertySelected(9999, "AvailableMethod")]
        public ACObjectItem SelectedAvailableMethod
        {
            get
            {
                return _SelectedAvailableMethod;
            }
            set
            {
                _SelectedAvailableMethod = value;
                OnPropertyChanged("SelectedAvailableMethod");
            }
        }
        #endregion

        #region Available Translationtexts
        [ACPropertyList(9999, "AvailableText")]
        public IEnumerable<IACObject> AvailableTextList
        {
            get
            {
                return GetAvailableTexts();
            }
        }

        ACObjectItem _CurrentAvailableText = null;
        [ACPropertyCurrent(9999, "AvailableText")]
        public ACObjectItem CurrentAvailableText
        {
            get
            {
                return _CurrentAvailableText;
            }
            set
            {
                bool currentChanged = false;
                if (_CurrentAvailableMethod != null)
                    currentChanged = true;
                _CurrentAvailableMethod = null;
                if (currentChanged)
                    OnPropertyChanged("_CurrentAvailableMethod");
                if (_CurrentAvailableProperty != null)
                    currentChanged = true;
                _CurrentAvailableProperty = null;
                if (currentChanged)
                    OnPropertyChanged("CurrentAvailableProperty");

                if (_CurrentAvailableText != value)
                {
                    _CurrentAvailableText = value;
                    OnPropertyChanged("CurrentAvailableText");
                }

                if (ToolService != null)
                {
                    ITool newTool = null;
                    if (_CurrentAvailableText is DesignManagerToolItem)
                    {
                        newTool = (_CurrentAvailableText as DesignManagerToolItem).CreateControlTool;
                    }
                    else if (value != null && value.ACObject != null)
                    {
                        newTool = DesignManagerControlTool.CreateNewInstance(this, _CurrentAvailableText.ACObject, _CurrentAvailableText.ACUrlRelative);
                        //DesignManagerToolItem designManagerToolItem = new DesignManagerToolItem(_CurrentAvailableText.ACObject, _CurrentAvailableText.ACCaption, , this);
                        //newTool = designManagerToolItem.CreateControlTool;
                    }
                    (ToolService as IToolService).CurrentTool = newTool ?? (ToolService as IToolService).PointerTool;
                }
            }
        }

        private int _CurrentAvailableTextClicked;
        [ACPropertyInfo(9999)]
        public int CurrentAvailableTextClicked
        {
            get
            {
                return _CurrentAvailableTextClicked;
            }
            set
            {
                _CurrentAvailableTextClicked = value;
                if ((ToolService != null) && (_CurrentAvailableText != null))
                {
                    if (_CurrentAvailableText is DesignManagerToolItem)
                    {
                        ITool newTool = null;
                        newTool = (_CurrentAvailableText as DesignManagerToolItem).CreateControlTool;
                        if ((newTool != null) && (ToolService as IToolService).CurrentTool != newTool)
                            (ToolService as IToolService).CurrentTool = newTool;
                    }
                }
            }
        }


        ACObjectItem _SelectedAvailableText;
        [ACPropertySelected(9999, "AvailableText")]
        public ACObjectItem SelectedAvailableText
        {
            get
            {
                return _SelectedAvailableText;
            }
            set
            {
                _SelectedAvailableText = value;
                OnPropertyChanged("SelectedAvailableText");
            }
        }
        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Builds the VBContent-String for the passed acUrl relatively to the CurrentDesignContext
        /// </summary>
        /// <param name="acUrl">The ac URL.</param>
        /// <param name="acObject">The ac object.</param>
        /// <returns>VBContent System.String</returns>
        public override string BuildVBContentFromACUrl(string acUrl, IACObject acObject)
        {
            if ((CurrentAvailableProperty != null) && (CurrentAvailableProperty.ACObject == acObject))
            {
                string urlPart1 = CurrentAvailableElement.ACUrlRelative;
                string urlPart2 = acUrl;
                if (!String.IsNullOrEmpty(urlPart1))
                {
                    if (!urlPart1.EndsWith("\\") && !urlPart2.StartsWith("\\"))
                        urlPart1 += "\\";
                }
                return urlPart1 + urlPart2;
            }
            else if (CurrentAvailableMethod != null && (CurrentAvailableMethod.ACObject != null) && (CurrentAvailableMethod.ACObject == acObject))
            {
                string urlPart1 = CurrentAvailableElement.ACUrlRelative;
                string urlPart2 = acUrl;
                if (!String.IsNullOrEmpty(urlPart1))
                {
                    if (urlPart1.EndsWith("\\"))
                        urlPart1 = urlPart1.Substring(0, urlPart1.Length - 1);
                }
                return urlPart1 + urlPart2;
            }
            else if (CurrentAvailableText != null && (CurrentAvailableText.ACObject != null) && (CurrentAvailableText.ACObject == acObject))
            {
                string urlPart1 = CurrentAvailableElement.ACUrlRelative;
                string urlPart2 = acUrl;
                if (!String.IsNullOrEmpty(urlPart1))
                {
                    if (urlPart1.EndsWith("\\"))
                        urlPart1 = urlPart1.Substring(0, urlPart1.Length - 1);
                }
                return urlPart1 + urlPart2;
            }
            else
            {
                return acUrl;
            }
        }

        #region Designer Methods
        [ACMethodInfo("", "en{'Rotate right 90°'}de{'Drehen rchts 90°'}", 9999)]
        public void DesignerRotateR90()
        {
            if (!IsEnabledDesignerRotateR90())
                return;
            DesignSurface.DesignerRotateR90();
        }

        public bool IsEnabledDesignerRotateR90()
        {
            if (VBDesignEditor == null || DesignSurface == null)
                return false;
            return DesignSurface.IsEnabledDesignerRotateR90();
        }

        [ACMethodInfo("", "en{'Flip horizontal'}de{'Horizontal spiegeln'}", 9999)]
        public void DesignerFlipHorz()
        {
            if (!IsEnabledDesignerFlipHorz())
                return;
            DesignSurface.DesignerFlipHorz();
        }

        public bool IsEnabledDesignerFlipHorz()
        {
            if (VBDesignEditor == null || DesignSurface == null)
                return false;
            return DesignSurface.IsEnabledDesignerFlipHorz();
        }

        [ACMethodInfo("", "en{'Flip vertical'}de{'Vertikal spiegeln'}", 9999)]
        public void DesignerFlipVert()
        {
            if (!IsEnabledDesignerFlipVert())
                return;
            DesignSurface.DesignerFlipVert();
        }

        public bool IsEnabledDesignerFlipVert()
        {
            if (VBDesignEditor == null || DesignSurface == null)
                return false;
            return DesignSurface.IsEnabledDesignerFlipVert();
        }

        [ACMethodInfo("", "en{'Reset transformation'}de{'Reset Transformation'}", 9999)]
        public void DesignerResetTransform()
        {
            if (!IsEnabledDesignerResetTransform())
                return;
            DesignSurface.DesignerResetTransform();
        }

        public bool IsEnabledDesignerResetTransform()
        {
            if (VBDesignEditor == null || DesignSurface == null)
                return false;
            return DesignSurface.IsEnabledDesignerResetTransform();
        }
        #endregion

        #endregion

        #region VBDesignerXAML Dialog

        IVBContent _VBDesignControl;
        /// <summary>
        /// Reference to the WPF-Control, that presents the XAML-Code of the CurrentDesign-Property
        /// </summary>
        /// <value>Reference to a WPF-Control that implements IVBContent (System.Windows.Controls.ContentControl)</value>
        public override IVBContent VBDesignControl
        {
            get
            {
                return _VBDesignControl;
            }

            set
            {
                _VBDesignControl = value;
                ReloadToolService();
            }
        }


        /// <summary>
        /// Returns a reference to the tool-window (WPF-Control)
        /// </summary>
        /// <value>Reference to the tool-window</value>
        public override IACObject ToolWindow
        {
            get
            {
                return GetWindow("ToolWindow");
            }
        }

        /// <summary>
        /// Returns a reference to a window (WPF-Control) that shows the Dependency-Properties of the selected WFP-Control in the designer (SelectedVBControl).
        /// These dependency-properties can also be manipulated.
        /// </summary>
        /// <value>Reference to the tool-window</value>
        public override IACObject PropertyWindow
        {
            get
            {
                return GetWindow("PropertyWindow");
            }
        }


        /// <summary>
        /// Returns a reference to a window (WPF-Control) that shows the logical tree of the current design.
        /// </summary>
        /// <value>Reference to the logical tree window</value>
        public override IACObject LogicalTreeWindow
        {
            get
            {
                return GetWindow("LogicalTreeWindow");
            }
        }

        /// <summary>
        /// Switches the this designer to design mode and the Designer-Tool (WPF-Control) appears on the gui.
        /// </summary>
        /// <param name="dockingManagerName">Name of the parent docking manager.</param>
        public override void ShowDesignManager(string dockingManagerName = "")
        {
            ReloadToolService();
            ShowWindow(this, "ToolWindow", false, Global.VBDesignContainer.DockableWindow, Global.VBDesignDockState.AutoHideButton, Global.VBDesignDockPosition.Left,Global.ControlModes.Hidden, dockingManagerName,Global.ControlModes.Hidden);
            ShowPropertyWindow(dockingManagerName);
            ShowLogicalTreeWindow(dockingManagerName);
            base.ShowDesignManager(dockingManagerName);
        }

        /// <summary>
        /// Switches the designer off and the Designer-Tool (WPF-Control) disappears on the gui.
        /// </summary>
        public override void HideDesignManager()
        {
            IACObject window = GetWindow("ToolWindow");
            if(window != null)
                CloseDockableWindow(window);

            ClosePropertyWindow();
            CloseLogicalTreeWindow();

            base.HideDesignManager();
        }


        /// <summary>
        /// Opens the property windows if it's closed.
        /// </summary>
        /// <param name="dockingManagerName">Name of the parent docking manager.</param>
        public override void ShowPropertyWindow(string dockingManagerName = "")
        {
            if (PropertyWindow == null)
                ShowWindow(this, "PropertyWindow", false, Global.VBDesignContainer.DockableWindow, Global.VBDesignDockState.AutoHideButton, Global.VBDesignDockPosition.Left, Global.ControlModes.Hidden, dockingManagerName, Global.ControlModes.Hidden);
        }


        /// <summary>
        /// Closes the property window.
        /// </summary>
        public override void ClosePropertyWindow()
        {
            IACObject window = GetWindow("PropertyWindow");
            if (window != null)
                CloseDockableWindow(window);
        }


        /// <summary>
        /// Opens the logical tree window if it's closed.
        /// </summary>
        /// <param name="dockingManagerName">Name of the parent docking manager.</param>
        public override void ShowLogicalTreeWindow(string dockingManagerName = "")
        {
            if (LogicalTreeWindow == null)
                ShowWindow(this, "LogicalTreeWindow", false, Global.VBDesignContainer.DockableWindow, Global.VBDesignDockState.AutoHideButton, Global.VBDesignDockPosition.Left, Global.ControlModes.Hidden, dockingManagerName, Global.ControlModes.Hidden);
        }


        /// <summary>
        /// Closes the logical tree window.
        /// </summary>
        public override void CloseLogicalTreeWindow()
        {
            IACObject window = GetWindow("LogicalTreeWindow");
            if (window != null)
                CloseDockableWindow(window);
        }
        #endregion

        //[ACMethodInfo("ACComponent", "en{'OnRemoveReference'}de{'OnRemoveReference'}", 9999)]
        //public override void OnRemoveReference(IACObject acObject)
        //{
        //    if (acObject != null && ACUrlHelper.IsSearchedGUIInstance(acObject.ACIdentifier, "","","*ToolWindow"))
        //    {
        //        if (VBDesignControl != null)
        //            VBDesignControl.ACUrlCommand("DesignMode", false);
        //    }
        //}

        #region Layout
        /// <summary>
        /// Aktualisieren XMLDesign nach dem einfügen von 1..n WF- und/oder WFEdge-Datensätzen
        /// </summary>
        protected override void UpdateVisual()
        {
            List<DesignItem> changedItems = new List<DesignItem>();

            ChangeGroup changeGroup = null;

            // Einfügen von WFEdge (Vor WF, da diese für die Layoutberechnung benötigt werden
            foreach (var change in VisualChangeList)            //foreach (var change in VisualChangeList.Where(c => !c.IsDelete))
            {
                if (change.LayoutAction != VBDesigner.LayoutActionType.InsertEdge) continue;
                if (changeGroup == null)
                {
                    changeGroup = this.DesignSurface.DesignContext.OpenGroup("Cut " + changedItems.Count + " elements", changedItems);
                }

                DesignItem designItem = CreateVBEdgeDesignItem(change, this.DesignSurface.DesignContext);
            }

            if (changeGroup != null)
                changeGroup.Commit();
        }

        public DesignItem CreateVBEdgeDesignItem(VisualInfo visualInfo, DesignContext designContext)
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

            return item;
        }

        #endregion

        /// <summary>
        /// Removes a WPF-Element from the design
        /// </summary>
        /// <param name="item">Item for delete.</param>
        /// <param name="isFromDesigner">If true, then call is invoked from this manager, else from gui</param>
        /// <returns><c>true</c> if is removed; otherwise, <c>false</c>.</returns>
        public override bool DeleteItem(object item, bool isFromDesigner = true)
        {
            return false;
        }

        #region Execute-Helper-Handlers

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case"OnItemExpand":
                    OnItemExpand((ACObjectItem)acParameter[0]);
                    return true;
                case"IsEnabledACActionToTarget":
                    result = IsEnabledACActionToTarget((IACInteractiveObject)acParameter[0], (ACActionArgs)acParameter[1]);
                    return true;
                case"IsEnabledCreateEdge":
                    result = IsEnabledCreateEdge((IVBConnector)acParameter[0], (IVBConnector)acParameter[1]);
                    return true;
                case"IsEnabledDropDesignItem":
                    result = IsEnabledDropDesignItem((Global.ElementActionType)acParameter[0], (IACInteractiveObject)acParameter[1], (IACInteractiveObject)acParameter[2], (Double)acParameter[3], (Double)acParameter[4]);
                    return true;
                case"DesignerRotateR90":
                    DesignerRotateR90();
                    return true;
                case"IsEnabledDesignerRotateR90":
                    result = IsEnabledDesignerRotateR90();
                    return true;
                case"DesignerFlipHorz":
                    DesignerFlipHorz();
                    return true;
                case"IsEnabledDesignerFlipHorz":
                    result = IsEnabledDesignerFlipHorz();
                    return true;
                case"DesignerFlipVert":
                    DesignerFlipVert();
                    return true;
                case"IsEnabledDesignerFlipVert":
                    result = IsEnabledDesignerFlipVert();
                    return true;
                case"DesignerResetTransform":
                    DesignerResetTransform();
                    return true;
                case"IsEnabledDesignerResetTransform":
                    result = IsEnabledDesignerResetTransform();
                    return true;
            }
                return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion


    }
}
