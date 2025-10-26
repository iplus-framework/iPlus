using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using gip.core.layoutengine.avui.Helperclasses;
using gip.core.datamodel;
using System.Transactions;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Input;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents a item in <see cref="VBTreeView"/> control.
    /// </summary>
    /// <summary>
    /// Stellt ein Element in <see cref="VBTreeView"/> control dar.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBTreeViewItem'}de{'VBTreeViewItem'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBTreeViewItem : TreeViewItem, IACInteractiveObjectParent, IVBContent, IACObject
    {
        #region c´tors
        /// <summary>
        /// Creates a new instance of VBTreeViewItem.
        /// </summary>
        public VBTreeViewItem() : base()
        {
        }

        /// <summary>
        /// Creates a new instance of VBTreeViewItem.
        /// </summary>
        /// <param name="parentACElement">The parent ACElement.</param>
        /// <param name="acComponent">The acComponent parameter.</param>
        public VBTreeViewItem(IACInteractiveObject parentACElement, IACObject acComponent)
        {
            _ParentACElement = parentACElement;
            ContextACObject = acComponent;
        }
        #endregion

        #region IACInteractiveObject Member
        IACObject _ACComponent;
        /// <summary>
        /// ContextACObject is used by WPF-Controls and mostly it equals to the Control.DataContext-Property.
        /// IACInteractiveObject-Childs in the logical WPF-tree resolves relative ACUrl's to this ContextACObject-Property.
        /// </summary>
        /// <value>The Data-Context as IACObject</value>
        public IACObject ContextACObject
        {
            get
            {
                return _ACComponent;
            }
            set
            {
                _ACComponent = value;
            }
        }

        /// <summary>
        /// Represents the dependency property for BSOACComponent.
        /// </summary>
        public static readonly AttachedProperty<IACBSO> BSOACComponentProperty = AvaloniaProperty.RegisterAttached<VBTreeViewItem, Control, IACBSO>(nameof(BSOACComponent), null, true);
        /// <summary>
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get { return (IACBSO)GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }

        IACObject _ACObject;
        /// <summary>
        /// Gets or sets the ContentACObject.
        /// </summary>
        public IACObject ContentACObject
        {
            get
            {
                return _ACObject;
            }
            set
            {
                _ACObject = value;
            }
        }

        /// <summary>
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        public IEnumerable<IACObject> ACContentList
        {
            get
            {
                List<IACObject> acContentList = new List<IACObject>();
                acContentList.Add(_ACObject);

                return acContentList;
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
            return false;
        }
        #endregion

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>

        protected override void OnInitialized()
        {
            base.OnInitialized();

            VBTreeView treeView = FindParentTreeView(this) as VBTreeView;

            if (treeView != null)
            {
                if (treeView.TreeItemTemplate != null)
                {
                    VBStaticResourceExtension.DataTemplateContext = this.DataContext;
                    try
                    {
                        // TODO Avalonia:
                        //Header = treeView.TreeItemTemplate.LoadContent() as Control;
                        //if (Header != null)
                        //{
                        //    (Header as Control).DataContext = this.DataContext;
                        //}
                    }
                    catch (Exception ex)
                    {
                        string msg = ex.Message;
                        if (ex.InnerException != null && ex.InnerException.Message != null)
                            msg += " Inner:" + ex.InnerException.Message;

                        if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                            datamodel.Database.Root.Messages.LogException("VBTreeViewItem", "OnInitialized", msg);

                        System.Diagnostics.Debug.Print(ex.Message);
                    }
                    VBStaticResourceExtension.DataTemplateContext = null;
                }
            }

            if (ContentACObject != null)
                ToolTip.SetTip(this, ContentACObject.ACUrlCommand(Const.CmdTooltip));
        }

        private VBTreeView FindParentTreeView(VBTreeViewItem item)
        {
            if (item.Parent == null)
                return null;
            if (item.Parent is VBTreeView)
                return item.Parent as VBTreeView;
            else if (item.Parent is VBTreeViewItem)
                return (item.Parent as VBTreeViewItem).FindParentTreeView(item.Parent as VBTreeViewItem);
            return null;
        }

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            if (!_IsInitialized)
                InitVBControl();
            
        }

        bool _IsInitialized = false;

        private void InitVBControl()
        {
            VBTreeView vbTreeView = ((VBTreeView)ParentACElement);
            if (vbTreeView == null)
                return;
            if (ContentACObject != null && vbTreeView.TreeItemTemplate is DataTemplate && ContentACObject is IVBDataCheckbox && ((IVBDataCheckbox)ContentACObject).DataContentCheckBox == "IsChecked")
            {
                VBCheckBox vbCheckBox = VBVisualTreeHelper.FindObjectInLogicalAndVisualTree(this, "VBCheckBox") as VBCheckBox;
                if (vbCheckBox != null)
                {
                    vbCheckBox.ClearAllBindings();
                    Binding bindingCheckbox = new Binding();
                    bindingCheckbox.Mode = BindingMode.TwoWay;
                    bindingCheckbox.Source = this.ContentACObject;
                    bindingCheckbox.Path = ((IVBDataCheckbox)ContentACObject).DataContentCheckBox;
                    vbCheckBox.Bind(VBCheckBox.IsCheckedProperty, bindingCheckbox);
                    vbCheckBox.IsEnabled = ((IVBDataCheckbox)ContentACObject).IsEnabled;
                    vbCheckBox.Click -= vbTreeView.cb_Click;
                    vbCheckBox.Click += vbTreeView.cb_Click;
                }
            }
            _IsInitialized = true;
        }


        /// <summary>
        /// Handles the OnMouseRightButtonDown event.
        /// </summary>
        /// <param name="e">The event arguments.</param>

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            if (e.InitialPressMouseButton == MouseButton.Right)
            {
                VBTreeView treeView = FindParentTreeView(this) as VBTreeView;
                if (treeView != null)
                {
                    treeView.treeViewItem_MouseRightButtonDown(this, e);
                }
            }
            base.OnPointerReleased(e);
        }

        /// <summary>
        /// Gets the container for item override.
        /// </summary>
        /// <returns>The new instance of VBTreeViewIem.</returns>
        protected override Control CreateContainerForItemOverride(object item, int index, object recycleKey)
        {
            return new VBTreeViewItem();
        }

        protected override bool NeedsContainerOverride(object item, int index, out object recycleKey)
        {
            return NeedsContainer<VBTreeViewItem>(item, out recycleKey);
        }

        //protected bool _Loaded = false;
        //protected virtual void VBTreeViewItem_Loaded(object sender, RoutedEventArgs e)
        //{
        //    if (_Loaded)
        //        return;
        //    _Loaded = true;
        //    if ((HeaderTemplate != null) && (Header != null))
        //        (Header as Control).DataContext = this.DataContext;
        //}
        #region IACInteractiveObjectParent Member

        IACInteractiveObject _ParentACElement;
        /// <summary>
        /// Gets the parent ACElement.
        /// </summary>
        public IACInteractiveObject ParentACElement
        {
            get
            {
                return _ParentACElement;
            }
        }

        #endregion

        #region IVBContent Member

        /// <summary>By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
        /// The ACUrlBinding()-Method returns a Source and a Path which the Control use to create a WPF-Binding to bind the right value and set the WPF-DataContext.
        /// ACUrl's can be either absolute or relative to the DataContext of the parent WPFControl (or the ContextACObject of the parent IACInteractiveObject)</summary>
        /// <value>Relative or absolute ACUrl</value>
        [Category("VBControl")]
        public string VBContent
        {
            get
            {
                VBTreeView vbTreeView = ParentACElement as VBTreeView;
                return vbTreeView.VBContent;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

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

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [Category("VBControl")]
        public string ACCaption
        {
            get
            {
                return "";
                //throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Enables or disables auto focus.
        /// </summary>
        /// <summary xml:lang="de">Aktiviert oder deaktiviert den Autofokus.</summary>
        [Category("VBControl")]
        public bool AutoFocus
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the ACClassProperty which describes a bounded property by VBContent.
        /// </summary>
        public ACClassProperty VBContentPropertyInfo
        {
            get
            {
                ACClass typeAsACClass = ACType as ACClass;
                return typeAsACClass != null ? typeAsACClass.Properties.FirstOrDefault() : null;
            }
        }

        /// <summary>
        /// Gets or sets the right control mode.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt den richtigen Kontrollmodus.
        /// </summary>
        public Global.ControlModes RightControlMode
        {
            get { return Global.ControlModes.Enabled; }
        }

        /// <summary>
        /// Checks and corrects the control modes.
        /// </summary>
        public void ControlModeChanged()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IACObject
        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        public IACType ACType
        {
            get
            {
                return this.ReflectACType();
            }
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
            return this.ReflectIsEnabledACUrlCommand(acUrl, acParameter);
        }

        /// <summary>
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        public IACObject ParentACObject
        {
            get
            {
                return Parent as IACObject;
            }
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
        /// DeInitVBControl is used to remove all References which a WPF-Control refers to.
        /// It's needed that the Garbage-Collerctor can delete the object when it's removed from the Logical-Tree.
        /// Controls that implement this interface should bind itself to the InitState of the BSOACComponent.
        /// When the BSOACComponent stops and the property changes to Destructed- or DisposedToPool-State than this method should be called.
        /// </summary>
        /// <param name="bso">The bound BSOACComponent</param>
        public virtual void DeInitVBControl(IACComponent bso)
        {
            VBCheckBox vbCheckBox = VBVisualTreeHelper.FindObjectInLogicalAndVisualTree(this, "VBCheckBox") as VBCheckBox;
            if (vbCheckBox != null)
            {
                vbCheckBox.ClearAllBindings();
                vbCheckBox.Click -= ((VBTreeView)ParentACElement).cb_Click;
            }
        }

        /// <summary>
        /// Determines is tree filled or not.
        /// </summary>
        public bool IsTreeFilled = false;

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == IsExpandedProperty 
                && BSOACComponent != null 
                && !IsTreeFilled
                && (bool)change.NewValue == true)
            {
                VBTreeView vbtv = ParentACElement as VBTreeView;
                if (vbtv != null && !string.IsNullOrEmpty(vbtv.VBTreeViewExpandMethod))
                {
                    Items.Clear();
                    BSOACComponent.ACUrlCommand(vbtv.VBTreeViewExpandMethod, new object[] { ContentACObject });
                    vbtv.FillChildsOnItemExpand(this);
                    this.IsSelected = true;
                }
            }
            IsTreeFilled = true;
            base.OnPropertyChanged(change);

        }
    }
}
