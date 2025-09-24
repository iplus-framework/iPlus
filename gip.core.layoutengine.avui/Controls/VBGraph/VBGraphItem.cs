using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using gip.core.layoutengine.avui.Helperclasses;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Control which represents a node in graph.
    /// </summary>
    [TemplatePart(Name = "InL", Type = typeof(VBConnector))]
    [TemplatePart(Name = "InT", Type = typeof(VBConnector))]
    [TemplatePart(Name = "OutR", Type = typeof(VBConnector))]
    [TemplatePart(Name = "OutB", Type = typeof(VBConnector))]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBGraphItem'}de{'VBGraphItem'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBGraphItem : ContentControl, IVBContent, IACObject
    {
        #region c'tors

        static VBGraphItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBGraphItem), new FrameworkPropertyMetadata(typeof(VBGraphItem)));
        }

        public VBGraphItem(IACObject contextACObject, VBGraphSurface parent)
        {
            Binding binding = new Binding();
            binding.Source = contextACObject;
            SetBinding(ContextACObjectProperty, binding);

            Binding binding2 = new Binding();
            binding2.Source = parent;
            SetBinding(ParentSurfaceProperty, binding2);

            if(ParentSurface != null)
                ParentSurface.OnGraphItemsChanged += ParentSurface_OnGraphItemsChanged;

            VBDesignBase.IsSelectableEnum isSelectable = VBDesignBase.GetIsSelectable(this);
            if (isSelectable == VBDesignBase.IsSelectableEnum.Unset)
                VBDesignBase.SetIsSelectable(this, VBDesignBase.IsSelectableEnum.True);

            Name = ACUrlHelper.GetTrimmedName(ContextACObject.GetACUrl());
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            object partObj = (object)GetTemplateChild("InL");
            if (partObj != null && partObj is VBConnector)
            {
                connectors[0] = partObj as VBConnector;
                connectors[0].OnConnectorArranged += OnConnectorArranged;
            }

            partObj = (object)GetTemplateChild("InT");
            if (partObj != null && partObj is VBConnector)
            {
                connectors[1] = partObj as VBConnector;
                connectors[1].OnConnectorArranged += OnConnectorArranged;
            }

            partObj = (object)GetTemplateChild("OutR");
            if (partObj != null && partObj is VBConnector)
            {
                connectors[2] = partObj as VBConnector;
                connectors[2].OnConnectorArranged += OnConnectorArranged;
            }

            partObj = (object)GetTemplateChild("OutB");
            if (partObj != null && partObj is VBConnector)
            {
                connectors[3] = partObj as VBConnector;
                connectors[3].OnConnectorArranged += OnConnectorArranged;
            }
        }

        /// <summary>
        /// DeInitVBControl is used to remove all References which a WPF-Control refers to.
        /// It's needed that the Garbage-Collerctor can delete the object when it's removed from the Logical-Tree.
        /// Controls that implement this interface should bind itself to the InitState of the BSOACComponent.
        /// When the BSOACComponent stops and the property changes to Destructed- or DisposedToPool-State than this method should be called.
        /// </summary>
        /// <param name="bso">The bound BSOACComponent</param>
        public void DeInitVBControl(IACComponent bso)
        {
            if (ParentSurface != null)
                ParentSurface.OnGraphItemsChanged -= ParentSurface_OnGraphItemsChanged;

            foreach (var conn in connectors)
                if (conn != null)
                    conn.OnConnectorArranged -= OnConnectorArranged;

            this.ClearAllBindings();

            Name = null;
            ParentSurface = null;
        }

        #endregion

        #region Properties

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(VBGraphItem));

        public bool IsStartOrEndItem
        {
            get { return (bool)GetValue(IsStartOrEndItemProperty); }
            set { SetValue(IsStartOrEndItemProperty, value); }
        }

        public static readonly DependencyProperty IsStartOrEndItemProperty =
            DependencyProperty.Register("IsStartOrEndItem", typeof(bool), typeof(VBGraphItem));

        public VBGraphSurface ParentSurface
        {
            get { return (VBGraphSurface)GetValue(ParentSurfaceProperty); }
            set { SetValue(ParentSurfaceProperty, value); }
        }

        public static readonly DependencyProperty ParentSurfaceProperty =
            DependencyProperty.Register("ParentSurface", typeof(VBGraphSurface), typeof(VBGraphItem));

        private VBConnector[] connectors = new VBConnector[4];

        byte _ConnectorArrangeCounter = 0;

        #endregion

        #region Methods

        private void OnConnectorArranged(object sender, EventArgs e)
        {
            _ConnectorArrangeCounter++;
            if (_ConnectorArrangeCounter >= 4)
            {
                ParentSurface.OnGraphItemArranged();
                _ConnectorArrangeCounter = 0;
            }
        }

        private void ParentSurface_OnGraphItemsChanged(object sender, EventArgs e)
        {
            if (ParentSurface.ActiveObjects.Any(c => c == ContextACObject))
                IsSelected = true;
            else
                IsSelected = false;
        }

        public void ArrangeConnectors()
        {
            foreach (var item in connectors)
            {
                if (item == null)
                    continue;
                item.InvalidateArrange();
                item.InvalidateVisual();
            }
        }

        protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
        {
            Point point = e.GetPosition(this);
            ACActionMenuArgs actionArgs = new ACActionMenuArgs(this, point.X, point.Y, Global.ElementActionType.ContextMenu);
            ParentSurface.BSOACComponent.ACAction(actionArgs);
            IACInteractiveObject interactiveObject = ContextACObject as IACInteractiveObject;
            if (interactiveObject != null)
                interactiveObject.ACAction(actionArgs);

            VBContextMenu vbContextMenu = new VBContextMenu(this, actionArgs.ACMenuItemList);
            this.ContextMenu = vbContextMenu;
            //@ihrastinski NOTE: Remote desktop context menu problem - added placement target
            if (vbContextMenu.PlacementTarget == null)
                vbContextMenu.PlacementTarget = this;
            ContextMenu.IsOpen = true;

            base.OnPreviewMouseRightButtonDown(e);
        }
        #endregion

        #region IVBContent members

        /// <summary>
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        public IEnumerable<IACObject> ACContentList
        {
            get
            {
                return this.ReflectGetACContentList();
            }
        }

        IACType _VBContentValueType = null;
        /// <summary>
        /// Gets the ACClassProperty which describes a bounded property by VBContent.
        /// </summary>
        public ACClassProperty VBContentPropertyInfo
        {
            get
            {
                return _VBContentValueType as ACClassProperty;
            }
        }

        public Global.ControlModes RightControlMode
        {
            get
            {
                return Global.ControlModes.Enabled;
            }
        }

        public string DisabledModes
        {
            get
            {
                return null;
            }
        }

        /// <summary>By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
        /// The ACUrlBinding()-Method returns a Source and a Path which the Control use to create a WPF-Binding to bind the right value and set the WPF-DataContext.
        /// ACUrl's can be either absolute or relative to the DataContext of the parent WPFControl (or the ContextACObject of the parent IACInteractiveObject)</summary>
        /// <value>Relative or absolute ACUrl</value>
        [Category("VBControl")]
        public string VBContent
        {
            get;
            set;
        }

        /// <summary>
        /// ContextACObject is used by WPF-Controls and mostly it equals to the FrameworkElement.DataContext-Property.
        /// IACInteractiveObject-Childs in the logical WPF-tree resolves relative ACUrl's to this ContextACObject-Property.
        /// </summary>
        /// <value>The Data-Context as IACObject</value>
        [ACPropertyInfo(999)]
        public IACObject ContextACObject
        {
            get
            {
                return (IACObject)GetValue(ContextACObjectProperty);
            }
            set
            {
                SetValue(ContextACObjectProperty, value);
            }
        }

        public static readonly DependencyProperty ContextACObjectProperty =
            DependencyProperty.Register("ContextACObject", typeof(IACObject), typeof(VBGraphItem));


        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public string ACIdentifier
        {
            get;
            set;
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
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
            get => this.ReflectACType();
        }

        /// <summary>
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        public IACObject ParentACObject
        {
            get { return Parent as IACObject; }
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
            return true;
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
        /// This method is called before ACUrlCommand if a method-command was encoded in the ACUrl
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>true if ACUrlCommand can be invoked</returns>
        public bool IsEnabledACUrlCommand(string acUrl, params object[] acParameter)
        {
            return this.ReflectIsEnabledACUrlCommand(acUrl, acParameter);
        }

        /// <summary>
        /// Returns a ACUrl relatively to the passed object.
        /// If the passed object is null then the absolute path is returned
        /// </summary>
        /// <param name="rootACObject">Object for creating a realtive path to it</param>
        /// <returns>ACUrl as string</returns>
        public string GetACUrl(IACObject rootACObject = null)
        {
            return this.ReflectGetACUrl(rootACObject);
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
            return this.ReflectACUrlBinding(acUrl, ref acTypeInfo, ref source, ref path, ref rightControlMode);
        }

        public bool ACUrlTypeInfo(string acUrl, ref ACUrlTypeInfo acUrlTypeInfo)
        {
            return this.ReflectACUrlTypeInfo(acUrl, ref acUrlTypeInfo);
        }

        #endregion
    }
}
