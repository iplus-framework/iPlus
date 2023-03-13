using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using gip.core.datamodel;
using gip.core.autocomponent;
using gip.core.layoutengine;
using gip.ext.design;
using gip.ext.designer;
using gip.core.layoutengine.Helperclasses;
using gip.ext.designer.Controls;

namespace gip.core.manager
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Workflow Designer'}de{'Workflow Desginer'}", Global.ACKinds.TACAbstractClass, Global.ACStorableTypes.NotStorable, false, false)]
    public abstract class VBDesignerWorkflow : VBDesigner, IACObjectDesign
    {
        #region c´tors
        public VBDesignerWorkflow(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }
        #endregion

        #region PWObjectRoot // Baustelle
        PWOfflineNode _PWRootNode;
        [ACPropertyCurrent(9999, "PWRoot", "en{'PWRoot'}de{'PWRoot'}")]
        public PWOfflineNode PWRootNode
        {
            get
            {
                return _PWRootNode;
            }
            set
            {
                _PWRootNode = value;
                OnPropertyChanged("PWRootNode");
            }
        }

        PWOfflineNode _ParentPWNode;
        public PWOfflineNode ParentPWNode
        {
            get
            {
                return _ParentPWNode;
            }
            protected set
            {
                _ParentPWNode = value;
                OnPropertyChanged("ParentPWNode");
            }
        }

        public VBDesignerWorkflow ParentPWRoot
        {
            get
            {
                return this.ParentACComponent as VBDesignerWorkflow;
                //var parentACObject = this.ParentACObject;
                //while (parentACObject != null)
                //{
                //    if (parentACObject is VBDesignerWorkflow)
                //        return parentACObject as VBDesignerWorkflow;
                //    parentACObject = parentACObject.ParentACObject;
                //}

                //return null;
            }
            //set
            //{
            //    _ParentPWRoot = value;
            //    OnPropertyChanged("ParentPWRoot");
            //}
        }

        [ACPropertyCurrent(9999, "PWRoot", "en{'Caller'}de{'Aufrufer'}")]
        public string PWRootACUrl
        {
            get
            {
                if (this.ParentPWNode != null)
                    return this.ParentPWNode.GetACUrl();
                return "";
            }
        }

        public IACWorkflowDesignContext CurrentDesignWF
        {
            get
            {
                return CurrentDesign as IACWorkflowDesignContext;
            }
        }

        #region IACObjectDesign
        /// <summary>
        /// XAML-Code for Presentation
        /// </summary>
        /// <value>
        /// XAML-Code for Presentation
        /// </value>
        public abstract string XMLDesign
        {
            get;
            set;
        }
        #endregion

        #endregion

        #region DesignModify
        WFLayoutCalculator _WFLayoutCalculator = null;
        public WFLayoutCalculator WFLayoutCalculator
        {
            get
            {
                if (_WFLayoutCalculator == null)
                {
                    _WFLayoutCalculator = new WFLayoutCalculator();
                }
                return _WFLayoutCalculator;
            }
        }

        public abstract bool IsEnabledDoModifyAction(IACWorkflowDesignContext acMethodMain, IACInteractiveObject dropObject, IACInteractiveObject targetVBDataObject, double x, double y);

        public abstract IACWorkflowNode DoModifyAction(IACWorkflowDesignContext acMethodMain, IACInteractiveObject dropObject, IACInteractiveObject targetVBDataObject, double x, double y);

        public abstract bool DoInsertRoot(IACWorkflowDesignContext vbWorkflow, ACClass methodACClass);

        public bool InsertRoot(IACWorkflowDesignContext vbWorkflow, ACClass methodACClass)
        {
            return DoInsertRoot(vbWorkflow, methodACClass);
        }

        public IACWorkflowNode ElementAction(IACWorkflowDesignContext acMethodMain, IACInteractiveObject dropObject, IACInteractiveObject targetVBDataObject, double x, double y)
        {
            return DoModifyAction(acMethodMain, dropObject, targetVBDataObject, x, y);
        }

        public bool IsEnabledModifyAction(IACWorkflowDesignContext acMethodMain, IACInteractiveObject dropObject, IACInteractiveObject targetVBDataObject, double x, double y)
        {
            return IsEnabledDoModifyAction(acMethodMain, dropObject, targetVBDataObject, x, y);
        }

        #endregion

        #region Drop
        public IACWorkflowNode LayoutAction(IACWorkflowDesignContext acMethodMain, IACInteractiveObject dropObject, double x, double y)
        {
            
            if (!IsEnabledLayoutAction(acMethodMain, dropObject, x, y))
                return null;

            IACWorkflowNode rControlClass = dropObject as IACWorkflowNode;
            IACWorkflowNode rControlClassGroup = rControlClass.WFGroup;
            // TODO: Layout
            //double topAbs = rControlClassGroup.VisualTopAbs;
            //double leftAbs = rControlClassGroup.VisualLeftAbs;

            //double newTopPos = y - rControlClassGroup.VisualTopAbs - (rControlClass.VisualHeight / 2);
            //double newLeftPos = x - rControlClassGroup.VisualLeftAbs - (rControlClass.VisualWidth / 2);

            //rControlClass.VisualTop = newTopPos > 0 ? newTopPos : 0;

            //rControlClass.VisualLeft = newLeftPos > 0 ? newLeftPos : 0;

#if DEBUG
            System.Diagnostics.Debug.WriteLine("Verschiebe nach " + x.ToString() + "/" + y.ToString());
#endif

            //_WFLayoutManager.WFUpdateGroupSize(rControlClassGroup);
            return rControlClass;
        }
        #endregion

        #region IsEnabledDrop
        /// <summary>
        /// Klonen eines IWorkflowClass innerhalb einnes RControl
        /// Voraussetzung ist, das folgende Elemente identisch sind:
        /// 1. r3ControlClass    Das einzufügende Class
        /// 3. visualTarget     Das Dropziel
        /// Von der Oberfläche wird dies immer über ein Popup auf dem Element ausgelöst
        /// </summary>
        //public bool IsEnabledCloneVisualClass(IVisualManager acMethodMain, IVisualWF visualClassEx, IVisual visualTarget, int clonePosition)
        //{
        //    return (visualClassEx == visualSelected && visualSelected == visualTarget);
        //}

        public bool IsEnabledLayoutAction(IACWorkflowDesignContext rControlTarget, IACInteractiveObject dropObject, double x, double y)
        {
            if (!(dropObject is IACWorkflowNode))
                return false;
            IACWorkflowNode visualClassEx = dropObject as IACWorkflowNode;

            if (visualClassEx.WFGroup == null)
                return false;

            IACWorkflowNode visualClassExGroup = visualClassEx.WFGroup;

            // TODO: Layout
            //if (visualClassExGroup.HitTest(x, y))
            //{
            //    System.Diagnostics.Debug.WriteLine("Verschieben " + x.ToString() + "/" + y.ToString());
            //}
            //else
            //{
            //    System.Diagnostics.Debug.WriteLine("Nicht Verschieben " + x.ToString() + "/" + y.ToString());
            //}

            // left und top sind absolute Positionen
            // Zur Positionierung werden jeweils 
            //return visualClassExGroup.HitTest(x, y);
            return false;
        }
        #endregion

        #region CreateEdge
        protected virtual void CreateWFEdge(VBEdge newVBEdge, VBConnector targetConnector)
        {
        }

        public void WFCreateWFEdge(IACWorkflowObject visualItemFrom, ACClassProperty sourceACClassProperty, IACWorkflowObject visualItemTo, ACClassProperty targetACClassProperty)
        {
            IACWorkflowDesignContext vbWorkflow = CurrentDesign as IACWorkflowDesignContext;

            if (visualItemFrom is IACWorkflowNode && visualItemTo is IACWorkflowNode)
            {
                IACWorkflowEdge visualEdge = CreateWFEdge(vbWorkflow, visualItemFrom as IACWorkflowNode, sourceACClassProperty, visualItemTo as IACWorkflowNode, targetACClassProperty, Global.ConnectionTypes.StartTrigger);

                IACWorkflowNode visualClassExTo = visualItemTo as IACWorkflowNode;
                CheckEdges(vbWorkflow, visualEdge);
                OnPropertyChanged("DesignXAML");
            }
        }

        protected IACWorkflowEdge CreateWFEdge(IACWorkflowDesignContext vbWorkflow, IACWorkflowNode visualClassFrom, ACClassProperty sourceACClassProperty, IACWorkflowNode visualClassTo, ACClassProperty targetACClassProperty, gip.core.datamodel.Global.ConnectionTypes connectionType)
        {
            IACWorkflowEdge newEdge = vbWorkflow.CreateNewEdge(Database.ContextIPlus);
            if (newEdge == null)
                newEdge = vbWorkflow.CreateNewEdge(Database);
            newEdge.FromWFNode = visualClassFrom;
            newEdge.SourceACClassProperty = sourceACClassProperty;
            newEdge.ToWFNode = visualClassTo;
            newEdge.TargetACClassProperty = targetACClassProperty;
            newEdge.ConnectionType = connectionType;
            vbWorkflow.AddEdge(newEdge);
            AddToVisualChangeList(newEdge, VBDesigner.LayoutActionType.InsertEdge, newEdge.SourceACConnector, newEdge.TargetACConnector);
            return newEdge;
        }
        #endregion

        #region IsEnabledCreateEdge

        public bool IsEnabledWFCreateEdge(IACWorkflowObject sourceVBVisual, ACClassProperty sourceACClassProperty, IACWorkflowObject targetVBVisual, ACClassProperty targetACClassProperty)
        {
            // From und To müssen IWorkflowClass sein
            if (!(sourceVBVisual is IACWorkflowNode) || !(targetVBVisual is IACWorkflowNode))
                return false;
            // From und To müssen unterschiedliche Classs sein
            if (sourceVBVisual == targetVBVisual)
                return false;
            IACWorkflowNode rControlClassFrom = sourceVBVisual as IACWorkflowNode;
            IACWorkflowNode rControlClassTo = targetVBVisual as IACWorkflowNode;
            IACWorkflowDesignContext vbWorkflow = CurrentDesign as IACWorkflowDesignContext;
            if (vbWorkflow == null)
                return false;

            if (rControlClassTo.GetIncomingWFEdges(vbWorkflow).Where(c => c.FromWFNode == rControlClassFrom).Any())
                return false;

            if (IsEdgeCollsion(rControlClassFrom, rControlClassTo))
                return false;
            if (sourceACClassProperty.ACPropUsage != Global.ACPropUsages.EventPoint)
                return false;

            if (targetACClassProperty.ACPropUsage != Global.ACPropUsages.EventPointSubscr)
                return false;

            return true;
        }

        /// <summary>
        /// Prüfen ob beim Einfügen einer Edge eine nicht auflösbare Abhängigkeit entsteht
        /// </summary>
        /// <param name="rControlClassFrom"></param>
        /// <param name="rControlClassTo"></param>
        /// <returns></returns>
        private bool IsEdgeCollsion(IACWorkflowNode rControlClassFrom, IACWorkflowNode rControlClassTo)
        {
            /*foreach (var edge in rControlClassFrom.ParentVisualEdges)
            {
                if (edge.FromVisualClass == rControlClassTo)
                    return true;

                if (IsEdgeCollsion(edge.FromVisualClass as IACWorkflowNode, rControlClassTo))
                {
                    return true;
                }
            }*/

            return false;
        }
        #endregion

        #region CheckEdges
        /// <summary>
        /// TODO: !!! Überlegen ob dieser Test überhaupt sinnvoll ist !!!
        /// Prüft bei einer neu eingefügten Edge, ob andere Edges überflüssig geworden sind und
        /// entfernt diese. 
        /// Betrifft nur Edges, die innerhalb einer Gruppe sind
        /// 
        /// Folgende Regeln sind zu beachten:
        /// 1. Alle Vorgänger untersuchen, die vor dem "FromVisualClass" liegen, ob 
        ///    diese einen direkten Edge zum "ToVisualClass" besitzen. Diese dann löschen.
        /// 2. Alle Nachfolger untersuchen, die nach dem "ToVisualClass" liegen, ob 
        ///    diese einen direkten Edge zum "FromVisualClass" besitzen. Diese dann löschen.
        /// </summary>
        /// <param name="acMethodMain"></param>
        /// <param name="visualEdge"></param>
        protected void CheckEdges(IACWorkflowDesignContext acMethodMain, IACWorkflowEdge visualEdge)
        {
            return;
            // Wenn in Unterschiedlichen Gruppen, dann nicht weiter untersuchen 
            //if (((IVBVisual)visualEdge.FromVisualClass).GetVisualGroup(acMethodMain.InstanceNo) != ((IVBVisual)visualEdge.ToVisualClass).GetVisualGroup(acMethodMain.InstanceNo))
            //    return;

            //IVBVisualWF fromVisualClass = visualEdge.FromVisualClass as IVBVisualNode;
            //IVBVisualWF toVisualClass = visualEdge.ToVisualClass as IVBVisualNode;

            //CheckEdgesToTop(toVisualClass, fromVisualClass, toVisualClass);
            //CheckEdgesToBottom(fromVisualClass, fromVisualClass, toVisualClass);
        }

        protected void CheckEdgesToTop(IACWorkflowNode startRControlClass, IACWorkflowNode fromRControlClass, IACWorkflowNode toRControlClass)
        {
            IACWorkflowDesignContext vbWorkflow = CurrentDesign as IACWorkflowDesignContext;
            if (vbWorkflow == null)
                return;

            // Alle Vorgänger des startRControlClass untersuchen
            foreach (var edge in startRControlClass.GetIncomingWFEdgesInGroup(vbWorkflow).ToList())
            {
                IACWorkflowNode toRControlClassNext = edge.FromWFNode;

                foreach (var edge2 in toRControlClassNext.GetOutgoingWFEdgesInGroup(vbWorkflow).ToList())
                {
                    IACWorkflowNode startRControlClassNext = edge2.FromWFNode;
                    if (edge2.ToWFNode == toRControlClass && !(edge2.FromWFNode == fromRControlClass && edge2.ToWFNode == toRControlClass))
                    {
                        IACObjectEntity edgeAsEntity = edge as IACObjectEntity;
                        if (edgeAsEntity != null)
                            edgeAsEntity.DeleteACObject(Database.ContextIPlus, false);
                    }
                    CheckEdgesToTop(startRControlClassNext, fromRControlClass, toRControlClass);
                }
            }
        }

        protected void CheckEdgesToBottom(IACWorkflowNode startRControlClass, IACWorkflowNode fromRControlClass, IACWorkflowNode toRControlClass)
        {
            IACWorkflowDesignContext vbWorkflow = CurrentDesign as IACWorkflowDesignContext;
            if (vbWorkflow == null)
                return;

            // Alle Nachfolger des startRControlClass untersuchen
            foreach (var edge in startRControlClass.GetOutgoingWFEdgesInGroup(vbWorkflow).ToList())
            {
                IACWorkflowNode fromRControlClassNext = edge.ToWFNode;

                foreach (var edge2 in fromRControlClassNext.GetIncomingWFEdgesInGroup(vbWorkflow).ToList())
                {
                    IACWorkflowNode startRControlClassNext = edge2.ToWFNode;
                    if (edge2.FromWFNode == fromRControlClass && !(edge2.FromWFNode == fromRControlClass && edge2.ToWFNode == toRControlClass))
                    {
                        IACObjectEntity edgeAsEntity = edge as IACObjectEntity;
                        if (edgeAsEntity != null)
                            edgeAsEntity.DeleteACObject(Database.ContextIPlus, false);
                    }
                    CheckEdgesToTop(startRControlClassNext, fromRControlClass, toRControlClass);
                }
            }
        }
        #endregion

        #region Hilfsfunktionen
        //protected bool ExistsEdge(IACObject fromClass, IACObject toClass)
        //{
        //    foreach (var edge in ((IACWorkflowNode)fromClass).GetOutgoingWFEdges)
        //    {
        //        if (edge.ToWFNode == toClass)
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            if (ExistsEdge(edge.ToWFNode, toClass))
        //                return true;
        //        }
        //    }
        //    return false;
        //}
        #endregion

        #region IACInteractiveObject Member
        new public string VBContent
        {
            get;
            set;
        }

        new public void ACAction(ACActionArgs actionArgs)
        {
            throw new NotImplementedException();
        }

        new public bool IsEnabledACAction(ACActionArgs actionArgs)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IACObject Member

        //public string ACCaption
        //{
        //    get { return ""; }
        //}

        //public IACType ACType
        //{
        //    get { return null; }
        //}

        //public IEnumerable<IACObject> ACContentList
        //{
        //    get { return null; }
        //}

        //public new object ACUrlCommand(string acUrl, params object[] acParameter)
        //{
        //    return null;
        //}

        //public IACObject ParentACObject
        //{
        //    get { return null; }
        //}

        #endregion

        #region IACInteractiveObjectEx Member
        /// <summary>
        /// Called from a WPF-Control inside it's ACAction-Method when a relevant interaction-event as occured (e.g. Drag-And-Drop).
        /// </summary>
        /// <param name="targetVBDataObject">The target object that was involved in the interaction event.</param>
        /// <param name="actionArgs">Information about the type of interaction and the source.</param>
        public override void ACActionToTarget(IACInteractiveObject targetVBDataObject, ACActionArgs actionArgs)
        {
            IACWorkflowNode rControlClass = null;
            switch (actionArgs.ElementAction)
            {
                case Global.ElementActionType.Move:
                    rControlClass = LayoutAction(CurrentDesignWF, actionArgs.DropObject, actionArgs.X, actionArgs.Y);
                    break;
                case Global.ElementActionType.Drop:
                    rControlClass = ElementAction(CurrentDesignWF, actionArgs.DropObject, targetVBDataObject, actionArgs.X, actionArgs.Y);
                    if (rControlClass != null)
                    {
                        DroppedElement(rControlClass);
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
            switch (actionArgs.ElementAction)
            {
                case Global.ElementActionType.Move: // Verschieben eines bestehenden Elements
                    return IsEnabledLayoutAction(CurrentDesignWF, actionArgs.DropObject, actionArgs.X, actionArgs.Y);
                case Global.ElementActionType.Drop:
                    {
                        bool isEnabled = IsEnabledModifyAction(CurrentDesignWF, actionArgs.DropObject, targetVBDataObject, actionArgs.X, actionArgs.Y);
                        actionArgs.ElementAction = isEnabled ? Global.ElementActionType.Drop : Global.ElementActionType.None;
                        actionArgs.Handled = true;
                        return isEnabled;
                    }
            }
            return base.IsEnabledACActionToTarget(targetVBDataObject, actionArgs);
        }
        #endregion

        #region Delete
        [ACMethodInteraction("WF", "en{'Delete'}de{'Löschen'}", (short)MISort.Delete, true)]
        public virtual bool DeleteVBVisual()
        {
            return false;
        }

        public virtual bool IsEnabledDeleteVBVisual()
        {
            return false;
        }
        #endregion

        #region Change PWClass-Derivation
        [ACPropertyList(9999, "BaseACClass")]
        public virtual IEnumerable<ACClass> BaseACClassList
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// The _ current new AC class
        /// </summary>
        ACClass _PWClassToSwitch;
        /// <summary>
        /// Gets or sets the current new AC class.
        /// </summary>
        /// <value>The current new AC class.</value>
        [ACPropertyInfo(200, "WF", "en{'Switch with class'}de{'Austauschen durch Klasse'}")]
        public ACClass PWClassToSwitch
        {
            get
            {
                return _PWClassToSwitch;
            }
            set
            {
                _PWClassToSwitch = value;
                OnPropertyChanged("PWClassToSwitch");
            }
        }


        [ACMethodInteraction("WF", "en{'Switch class'}de{'Klasse austauschen'}", (short)MISort.New, true)]
        public virtual bool SwitchPWClass()
        {
            if (!IsEnabledSwitchPWClass())
                return false;
            PWClassToSwitch = null;
            ShowDialog(this, "SwitchACClass");
            return false;
        }

        public virtual bool IsEnabledSwitchPWClass()
        {
            return false;
        }

        /// <summary>Switches the ac class ok.</summary>
        [ACMethodCommand("SwitchACClass", "en{'OK'}de{'OK'}", (short)MISort.Okay)]
        public virtual void SwitchACClassOK()
        {
        }

        /// <summary>
        /// Determines whether [is enabled new AC class OK].
        /// </summary>
        /// <returns><c>true</c> if [is enabled new AC class OK]; otherwise, <c>false</c>.</returns>
        public virtual bool IsEnabledSwitchACClassOK()
        {
            return false;
        }

        /// <summary>
        /// News the AC class cancel.
        /// </summary>
        [ACMethodCommand("SwitchACClass", "en{'Cancel'}de{'Abbrechen'}", (short)MISort.Cancel)]
        public void SwitchACClassCancel()
        {
            CloseTopDialog();
            PWClassToSwitch = null;
        }

        #endregion

        public delegate void DroppedElementEventHandler(IACWorkflowNode vbVisualWF);
        public event DroppedElementEventHandler OnDroppedElement;
        public void DroppedElement(IACWorkflowNode vbVisualWF)
        {
            if (OnDroppedElement != null)
            {
                OnDroppedElement(vbVisualWF);
            }
        }

        #region Layout
        /// <summary>
        /// Aktualisieren XMLDesign nach dem einfügen von 1..n WF- und/oder WFEdge-Datensätzen
        /// </summary>
        protected override void UpdateVisual()
        {
            if (WPFProxy == null)
                throw new MemberAccessException("DesignerService is null. This method could only be used from WPF-applications");
            WPFProxy.UpdateVisual();
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

        protected override void ParentPropertyChanged(string propertyName)
        {
            if (propertyName == VBContentDesign && ParentACComponent != null)
            {
                VBPresenter vbPresenter = ParentACComponent as VBPresenter;
                if (vbPresenter != null && vbPresenter.WFRootContext != null)
                    CurrentDesign = vbPresenter.WFRootContext as IACObjectDesign;
            }
            base.ParentPropertyChanged(propertyName);
        }

        #region Execute-Helper-Handlers

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "IsEnabledDoModifyAction":
                    result = IsEnabledDoModifyAction(acParameter[0] as IACWorkflowDesignContext, (IACInteractiveObject)acParameter[1], (IACInteractiveObject)acParameter[2], (Double)acParameter[3], (Double)acParameter[4]);
                    return true;
                case "IsEnabledModifyAction":
                    result = IsEnabledModifyAction(acParameter[0] as IACWorkflowDesignContext, (IACInteractiveObject)acParameter[1], (IACInteractiveObject)acParameter[2], (Double)acParameter[3], (Double)acParameter[4]);
                    return true;
                case "IsEnabledLayoutAction":
                    result = IsEnabledLayoutAction(acParameter[0] as IACWorkflowDesignContext, (IACInteractiveObject)acParameter[1], (Double)acParameter[2], (Double)acParameter[3]);
                    return true;
                case "IsEnabledWFCreateEdge":
                    result = IsEnabledWFCreateEdge((IACWorkflowObject)acParameter[0], (ACClassProperty)acParameter[1], (IACWorkflowObject)acParameter[2], (ACClassProperty)acParameter[3]);
                    return true;
                case "IsEnabledACAction":
                    result = IsEnabledACAction((ACActionArgs)acParameter[0]);
                    return true;
                case "IsEnabledACActionToTarget":
                    result = IsEnabledACActionToTarget((IACInteractiveObject)acParameter[0], (ACActionArgs)acParameter[1]);
                    return true;
                case "DeleteVBVisual":
                    result = DeleteVBVisual();
                    return true;
                case "IsEnabledDeleteVBVisual":
                    result = IsEnabledDeleteVBVisual();
                    return true;
                case "SwitchPWClass":
                    result = SwitchPWClass();
                    return true;
                case "IsEnabledSwitchPWClass":
                    result = IsEnabledSwitchPWClass();
                    return true;
                case "SwitchACClassOK":
                    SwitchACClassOK();
                    return true;
                case "IsEnabledSwitchACClassOK":
                    result = IsEnabledSwitchACClassOK();
                    return true;
                case "SwitchACClassCancel":
                    SwitchACClassCancel();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }
}
