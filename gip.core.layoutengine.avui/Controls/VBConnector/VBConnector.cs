using System;
using System.Collections.Generic;
using System.ComponentModel;
using gip.core.layoutengine.avui.VisualControlAnalyser;
using gip.core.layoutengine.avui.Helperclasses;
using gip.core.datamodel;
using System.Linq;
using Avalonia.Controls.Primitives;
using Avalonia;
using gip.ext.design.avui;
using Avalonia.Controls;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Control for the placement of a connection line. 
    /// </summary>
    /// <summary xml:lang="de">
    /// Steuerelement für Platzierung von Verbindungslinien
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBConnector'}de{'VBConnector'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBConnector : TemplatedControl, IVBConnector
    {
        #region c'tors
        /// <summary>
        /// Called to arrange and size the content. 
        /// </summary>
        /// <param name="arrangeBounds">The arrange bounds.</param>
        /// <returns>Return a new size after arranged.</returns>
        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            if (OnConnectorArranged != null)
                OnConnectorArranged.Invoke(this, new EventArgs());
            return base.ArrangeOverride(arrangeBounds);
        }

        #endregion

        #region Dependency-Properties

        #region Orientation
        /// <summary>
        /// Represents the dependency property for Orientation.
        /// </summary>
        public static readonly StyledProperty<Global.ConnectorOrientation> OrientationProperty = AvaloniaProperty.Register<VBConnector, Global.ConnectorOrientation>(nameof(Orientation));
        /// <summary>
        /// Gets or sets orientation.
        /// </summary>
        [Category("VBControl")]
        public Global.ConnectorOrientation Orientation
        {
            get
            {
                return (Global.ConnectorOrientation)GetValue(OrientationProperty);
            }
            set
            {
                SetValue(OrientationProperty, value);
            }
        }
        #endregion

        #endregion

        #region public Properties

        private List<VBEdge> _ConnectedEdges;
        /// <summary>
        /// Gets the list of connected edges to this connector.
        /// </summary>
        public List<VBEdge> ConnectedEdges
        {
            get
            {
                if (_ConnectedEdges == null)
                    _ConnectedEdges = new List<VBEdge>();
                return _ConnectedEdges;
            }
        }

        /// <summary>
        /// Gets the ACPropUsage.
        /// </summary>
        public Global.ACPropUsages ACPropUsage
        {
            get
            {
                var acObject = this.ParentACObject;
                if (acObject != null && acObject.ACContentList != null && acObject.ACContentList.Any())
                {
                    ACClass acClass = null;
                    IACWorkflowNode contentWFNode = null;
                    IACComponent contentObject = acObject as IACComponent;
                    if (contentObject != null)
                        contentWFNode = contentObject.Content as IACWorkflowNode;
                    if (contentWFNode != null)
                        acClass = contentWFNode.PWACClass;
                    if (acClass == null)
                    {
                        var acObjectForCheck = acObject.ACContentList.FirstOrDefault();
                        if (acObjectForCheck is IACWorkflowNode)
                        {
                            acClass = (acObjectForCheck as IACWorkflowNode).PWACClass;
                        }
                        else
                        {
                            acClass = acObjectForCheck as ACClass;
                        }
                    }

                    var point = acClass.GetPoint(this.VBContent);
                    if (point == null)
                        return Global.ACPropUsages.Property;
                    return point.ACPropUsage;
                }
                return Global.ACPropUsages.Property;
            }
        }

        #endregion

        #region public Methods

        /// <summary>
        /// Gets the point which is relativate to given container
        /// </summary>
        /// <param name="container">The container.</param>
        /// <returns>Returns a new point which is relative to given container.</returns>
        public Point GetPointRelativeToContainer(Visual container)
        {
            return GetPointRelativeToContainer(container, this);
        }

        /// <summary>
        /// Gets the point relative to given container and connector.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="connector"></param>
        /// <returns></returns>
        public static Point GetPointRelativeToContainer(Visual container, VBConnector connector)
        {
            if ((container == null) || (connector == null))
                return new Point();
            double actualWidth = 2;
            double actualHeight = 2;
            if ((connector.Bounds.Width > 0) || (connector.Bounds.Height > 0))
            {
                actualWidth = connector.Bounds.Width;
                actualHeight = connector.Bounds.Height;
            }
            else if ((connector.Width > 0) || (connector.Height > 0))
            {
                actualWidth = connector.Width;
                actualHeight = connector.Height;
            }
            try
            {
                if (!connector.IsDescendantOf(container as Control))
                    return new Point();
                Matrix? transform = connector.TransformToVisual(container);
                if (transform != null)
                    return transform.Value.Transform(new Point(actualWidth / 2, actualHeight / 2));
                return new Point();
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("VBConnector", "GetPointRelativeToContainer", msg);

                return new Point();
            }
        }

        #endregion

        #region internal Methods
        internal VCConnectorInfoIntern GetInfo(Control containerOfPath)
        {
            Control containerOfConnector = ParentVBControl as Control;
            VCConnectorInfoIntern info = new VCConnectorInfoIntern();
            Point relativePoint = containerOfConnector.TranslatePoint(new Point(0, 0), containerOfPath).Value;
            info.DesignerItemLeft = relativePoint.X;
            info.DesignerItemTop = relativePoint.Y;
            if (containerOfConnector is Control)
                info.DesignerItemSize = new Size((containerOfConnector as Control).Bounds.Width, (containerOfConnector as Control).Bounds.Height);
            info.Orientation = this.Orientation;
            if (info.Orientation == Global.ConnectorOrientation.Hidden)
                info.Orientation = Global.ConnectorOrientation.Top;
            info.Position = this.TranslatePoint(new Point(0, 0), containerOfPath).Value;
            return info;
        }
        #endregion

        #region IVBContent Members
        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public string ACIdentifier
        {
            get
            {
                return Name;
            }
            set
            {
                Name = value;
            }
        }

        /// <summary>
        /// Gets the ACClassProperty which describes a bounded property by VBContent.
        /// </summary>
        public ACClassProperty VBContentPropertyInfo
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the right control mode.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt den richtigen Kontrollmodus.
        /// </summary>
        public Global.ControlModes RightControlMode
        {
            get;
            set;
        }

        /// <summary>
        /// Disables the control. XAML sample: DisabledModes="Disabled"
        /// </summary>
        /// <summary xml:lang="de">
        /// Deaktiviert die Steuerung. XAML-Probe: DisabledModes="Disabled"
        /// </summary>
        [Category("VBControl")]
        public string DisabledModes
        {
            get;
            set;
        }


        /// <summary>
        /// Checks and corrects the control modes.
        /// </summary>
        public void ControlModeChanged()
        {
        }

        /// <summary>
        /// Determines is selected or not.
        /// </summary>
        public bool IsSelected
        {
            get;
            set;
        }

        /// <summary>
        /// Represents the dependency property for VBContent.
        /// </summary>
        public static readonly StyledProperty<string> VBContentProperty = AvaloniaProperty.Register<VBConnector, string>(nameof(VBContent));
        /// <summary>By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
        /// The ACUrlBinding()-Method returns a Source and a Path which the Control use to create a WPF-Binding to bind the right value and set the WPF-DataContext.
        /// ACUrl's can be either absolute or relative to the DataContext of the parent WPFControl (or the ContextACObject of the parent IACInteractiveObject)</summary>
        /// <value>Relative or absolute ACUrl</value>
        [Category("VBControl")]
        public string VBContent
        {
            get { return (string)GetValue(VBContentProperty); }
            set { SetValue(VBContentProperty, value); }
        }

        private IACObject _ContextACObject;
        /// <summary>
        /// ContextACObject is used by WPF-Controls and mostly it equals to the Control.DataContext-Property.
        /// IACInteractiveObject-Childs in the logical WPF-tree resolves relative ACUrl's to this ContextACObject-Property.
        /// </summary>
        /// <value>The Data-Context as IACObject</value>
        public IACObject ContextACObject
        {
            get
            {
                if (_ContextACObject != null)
                    return _ContextACObject;
                return _ParentACObject as IACObject;
            }
            set
            {
                _ContextACObject = value;
            }
        }

        /// <summary>
        /// Gets the parent VBControl.
        /// </summary>
        public IVBContent ParentVBControl
        {
            get
            {
                IVBContent vbContent = VBVisualTreeHelper.FindParentObjectInVisualTree(this, new Type[] { typeof(VBVisual), typeof(VBVisualGroup) }) as IVBContent;
                return vbContent;
            }
        }

        private IACObject _ParentACObject;
        /// <summary>
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        public IACObject ParentACObject
        {
            get
            {
                if (_ParentACObject != null)
                    return _ParentACObject;
                IVBContent vbContent = VBVisualTreeHelper.FindParentObjectInVisualTree(this, new Type[] { typeof(VBVisual), typeof(VBVisualGroup) }) as IVBContent;

                if (vbContent == null)
                    return null;
                if (vbContent.ContextACObject != null)
                    _ParentACObject = vbContent.ContextACObject;
                else if (vbContent.ACContentList != null && vbContent.ACContentList.Any())
                    _ParentACObject = vbContent.ACContentList.First();
                else
                    _ParentACObject = vbContent.ParentACObject;
                return _ParentACObject;
            }
        }

        /// <summary>
        /// Gets the ACClass of component.
        /// </summary>
        public ACClass ACClassOfComponent
        {
            get
            {
                ACClass acClass = null;
                if (ParentACObject is ACClass)
                    acClass = ParentACObject as ACClass;
                else if (ContextACObject != null)
                    acClass = ContextACObject.ACType as ACClass;
                return acClass;
            }
        }

        /// <summary>
        /// Gets the point property.
        /// </summary>
        public ACClassProperty PointProperty
        {
            get
            {
                if ((ParentACObject == null) || String.IsNullOrEmpty(this.VBContent))
                    return null;
                if (ParentACObject != null)
                {
                    if (ACClassOfComponent == null)
                        return null;
                    return FindMyPoint(ACClassOfComponent, VBContent);
                    //return ACClassOfComponent.MyACClassPropertyPointList.Where(c => c.ACIdentifier == this.VBContent).FirstOrDefault();
                }
                return null;
            }
        }

        private ACClassProperty FindMyPoint(ACClass acClass, string acUrlPoint)
        {
            if (String.IsNullOrEmpty(acUrlPoint))
                return null;
            ACUrlHelper helper = new ACUrlHelper(acUrlPoint);
            if (String.IsNullOrEmpty(helper.NextACUrl))
                return acClass.GetPoint(acUrlPoint);
            else if (!String.IsNullOrEmpty(helper.ACUrlPart))
            {
                var property = acClass.ACClass_ParentACClass.Where(c => c.ACIdentifier == helper.ACUrlPart).FirstOrDefault();
                if (property == null)
                    return null;
                return FindMyPoint(property, helper.NextACUrl);
            }
            return null;
        }


        List<ACClassPropertyRelation> _PointStateInfoList = null;
        /// <summary>
        /// Gets the point states info.
        /// </summary>
        public IEnumerable<ACClassPropertyRelation> PointStatesInfo
        {
            get
            {
                ACClassProperty pointProperty = PointProperty;
                if (pointProperty == null)
                    return null;
                _PointStateInfoList = new List<ACClassPropertyRelation>();
                var queryPointStates = pointProperty.TopBaseACClassProperty.ACClassPropertyRelation_SourceACClassProperty.Where(c => c.ConnectionTypeIndex == (short)Global.ConnectionTypes.PointState);
                foreach (ACClassPropertyRelation rel in queryPointStates)
                {
                    ACClass superClass = pointProperty.ACClass;

                    using (ACMonitor.Lock(superClass.Database.QueryLock_1X000))
                    {
                        while (superClass != null)
                        {
                            if (superClass == rel.SourceACClass)
                            {
                                _PointStateInfoList.Add(rel);
                                break;
                            }
                            superClass = superClass.ACClass1_BasedOnACClass;
                        }
                    }
                }
                return _PointStateInfoList;

                //return pointProperty.TopBaseACClassProperty.ACClassPropertyRelation_SourceACClassProperty.Where(c => c.ConnectionType == Global.ConnectionTypes.PointState);
            }
        }

        /// <summary>
        /// Gets the point states.
        /// </summary>
        public IEnumerable<IACPropertyBase> PointStates
        {
            get
            {
                if (PointStatesInfo == null || ContextACObject == null)
                    return null;
                if (!PointStatesInfo.Any())
                    return null;
                if (!(ContextACObject is IACComponent))
                    return null;
                List<IACPropertyBase> pointStates = new List<IACPropertyBase>();
                foreach (ACClassPropertyRelation relation in PointStatesInfo)
                {
                    if (relation.TargetACClassProperty == null)
                        continue;

                    string acUrlOfMember = relation.TargetACClassProperty.GetACUrlComponent(ACClassOfComponent);
                    IACMember member = (ContextACObject as IACComponent).GetMember(acUrlOfMember);
                    if (member != null && member is IACPropertyBase)
                        if (member is IACPropertyBase)
                        {
                            pointStates.Add(member as IACPropertyBase);
                        }
                }
                return pointStates;
            }
        }

        /// <summary>
        /// ACAction is called when one IACInteractiveObject (Source) wants to inform another IACInteractiveObject (Target) about an relevant interaction-event.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        public void ACAction(ACActionArgs actionArgs)
        {
        }

        /// <summary>
        /// It's called at the Target-IACInteractiveObject to inform the Source-IACInteractiveObject that ACAction ist allowed to be invoked.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        /// <returns><c>true</c> if ACAction can be invoked otherwise, <c>false</c>.</returns>
        public bool IsEnabledACAction(ACActionArgs actionArgs)
        {
            throw new NotImplementedException();
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [Category("VBControl")]
        public string ACCaption
        {
            get;
            set;
        }

        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        public IACType ACType
        {
            get;
        }

        /// <summary>
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        public IEnumerable<IACObject> ACContentList
        {
            get { return new List<IACObject>(); }
        }

        /// <summary>
        /// The ACUrlCommand is a universal method that can be used to query the existence of an instance via a string (ACUrl) to:
        /// 1. get references to components,
        /// 2. query property values,
        /// 3. execute method calls,
        /// 4. start and stop Components,
        /// 5. and send messages to other components.
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>Result if a property was accessed or a method was invoked. Void-Methods returns null.</returns>
        public object ACUrlCommand(string acUrl, params object[] acParameter)
        {
            return this.ReflectACUrlCommand(acUrl, acParameter);
        }

        /// <summary>
        /// Determines is ACUrlCommand is enabled or disabled.
        /// </summary>
        /// <param name="acUrl">The acUrl of command.</param>
        /// <param name="acParameter">The command parameters.</param>
        ///<returns>Returns true if is ACUrlCommand is enabled, otherwise false.</returns>
        public bool IsEnabledACUrlCommand(string acUrl, params Object[] acParameter)
        {
            return false;
        }

        #endregion

        #region IACObject Member

        /// <summary>
        /// Returns a ACUrl relatively to the passed object.
        /// If the passed object is null then the absolute path is returned
        /// </summary>
        /// <param name="rootACObject">Object for creating a realtive path to it</param>
        /// <returns>ACUrl as string</returns>
        public string GetACUrl(IACObject rootACObject = null)
        {
            return ACIdentifier;
        }

        /// <summary>
        /// Method that returns a source and path for WPF-Bindings by passing a ACUrl.
        /// </summary>
        /// <param name="acUrl">ACUrl of the Component, Property or Method</param>
        /// <param name="acTypeInfo">Reference to the iPlus-Type (ACClass)</param>
        /// <param name="source">The Source for WPF-Databinding</param>
        /// <param name="path">Relative path from the returned source for WPF-Databinding</param>
        /// <param name="rightControlMode">Information about access rights for the requested object</param>
        /// <returns><c>true</c> if binding could resolved for the passed ACUrl<c>false</c> otherwise</returns>
        public bool ACUrlBinding(string acUrl, ref IACType acTypeInfo, ref object source, ref string path, ref Global.ControlModes rightControlMode)
        {
            return false;
        }

        public bool ACUrlTypeInfo(string acUrl, ref ACUrlTypeInfo acUrlTypeInfo)
        {
            return this.ReflectACUrlTypeInfo(acUrl, ref acUrlTypeInfo);
        }
        #endregion

        #region Events

        /// <summary>
        /// The event handler for OnConnctorArranged.
        /// </summary>
        public event EventHandler OnConnectorArranged;

        #endregion  

        /// <summary>
        /// DeInitVBControl is used to remove all References which a WPF-Control refers to.
        /// It's needed that the Garbage-Collerctor can delete the object when it's removed from the Logical-Tree.
        /// Controls that implement this interface should bind itself to the InitState of the BSOACComponent.
        /// When the BSOACComponent stops and the property changes to Destructed- or DisposedToPool-State than this method should be called.
        /// </summary>
        /// <param name="bso">The bound BSOACComponent</param>
        public void DeInitVBControl(IACComponent bso)
        {
        }
    }

    // provides compact info about a connector; used for the 
    // routing algorithm, instead of hand over a full fledged VBEdge
    internal struct VCConnectorInfoIntern
    {
        public double DesignerItemLeft { get; set; }
        public double DesignerItemTop { get; set; }
        public Size DesignerItemSize { get; set; }
        public Point Position { get; set; }
        public Global.ConnectorOrientation Orientation { get; set; }
    }

}
