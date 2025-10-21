using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using gip.ext.design.avui;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// The control element for displaying hierarchies. Example for item data template with checkbox => BSOTC3Sync.cs, design Explorer
    /// </summary>
    /// <summary xml:lang="de">
    /// Steuerelement zur Darstellung von Hierarchien. Beispiel für Positionsdatenvorlage mit Checkbox => BSOTC3Sync.cs, Design Explorer
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBTreeView'}de{'VBTreeView'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public partial class VBTreeView : TreeView, IVBContent, IVBSource, IACMenuBuilderWPFTree, IACObject
    {
        #region c'tors
        /// <summary>
        /// Creates a new instance of VBTreeView.
        /// </summary>
        public VBTreeView()
        {
            CheckBoxLevel = 0;
            ExpandLevel = 1;
            AutoLoad = true;
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.Loaded += VBTreeView_Loaded;
            this.Unloaded += VBTreeView_Unloaded;
            AddHandler(DragDrop.DragEnterEvent, OnDragEnter);
            AddHandler(DragDrop.DragOverEvent, OnDragOver);
            AddHandler(DragDrop.DropEvent, OnDrop);
            AddHandler(DragDrop.DragLeaveEvent, OnDragLeave);
        }

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            InitVBControl();
        }

        protected override Control CreateContainerForItemOverride(object item, int index, object recycleKey)
        {
            return new VBTreeViewItem();
        }

        protected override bool NeedsContainerOverride(object item, int index, out object recycleKey)
        {
            return NeedsContainer<VBTreeViewItem>(item, out recycleKey);
        }
        #endregion


        #region Loaded-Event

        /// <summary>
        /// Initializes the VB control.
        /// </summary>
        protected virtual void InitVBControl()
        {
            if (_Initialized || (ContextACObject == null))
                return;
            if (String.IsNullOrEmpty(VBContent) || String.IsNullOrEmpty(VBSource))
                return;

            if (DisableContextMenu)
                ContextFlyout = null;

            _Initialized = true;
            IACType dcACTypeInfo = null;
            object dcSource = null;
            string dcPath = "";
            Global.ControlModes dcRightControlMode = Global.ControlModes.Hidden;

            if (!ContextACObject.ACUrlBinding(VBContent, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode))
            {
                this.Root().Messages.LogDebug("Error00003", "VBTreeView", VBContent);
                return;
            }
            IACType dsACTypeInfo = null;
            object dsSource = null;
            string dsPath = "";
            Global.ControlModes dsRightControlMode = Global.ControlModes.Hidden;
            if (!ContextACObject.ACUrlBinding(VBSource, ref dsACTypeInfo, ref dsSource, ref dsPath, ref dsRightControlMode))
            {
                this.Root().Messages.LogDebug("Error00004", "VBTreeView", VBSource + " " + VBContent);
                return;
            }

            _VBContentPropertyInfo = dcACTypeInfo;
            RightControlMode = dcRightControlMode;

            if (VBContent != null && VBContent != "")
            {
                if (IsVisible)
                {
                    if (RightControlMode < Global.ControlModes.Disabled)
                    {
                        IsVisible = false;
                    }
                    else
                    {
                        // Beschriftung
                        if (string.IsNullOrEmpty(ACCaption))
                            ACCaptionTrans = VBContentPropertyInfo.ACCaption;
                        else
                            ACCaptionTrans = this.Root().Environment.TranslateText(ContextACObject, ACCaption);
                    }
                }
                if (IsEnabled)
                {
                    if (RightControlMode < Global.ControlModes.Enabled)
                    {
                        IsEnabled = false;
                    }
                    else
                    {
                        if (BSOACComponent != null)
                        {
                            Global.ControlModesInfo controlModeInfo = BSOACComponent.GetControlModes(this);
                            Global.ControlModes controlMode = controlModeInfo.Mode;
                            Enabled = controlMode >= Global.ControlModes.Enabled;
                        }
                    }
                }

            }

            if (AutoFocus)
            {
                Focus();
                //MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }

            Binding binding;
            if (dsSource == null)
            {
                binding = new Binding(VBSource);
            }
            else
            {
                binding = new Binding();
                binding.Source = dsSource;
                binding.Path = dsPath;
            }
            binding.Mode = BindingMode.OneWay;
            this.Bind(VBTreeView.TreeDataSourceProperty, binding);

            IACType ciACTypeInfo = null;
            object ciSource = null;
            string ciPath = "";
            Global.ControlModes ciRightControlMode = Global.ControlModes.Hidden;
            if (ContextACObject.ACUrlBinding(VBSource + "ChangeInfo", ref ciACTypeInfo, ref ciSource, ref ciPath, ref ciRightControlMode))
            {
                Binding bindingChangeInfo = new Binding();
                bindingChangeInfo.Source = ciSource;
                bindingChangeInfo.Path = ciPath;
                bindingChangeInfo.Mode = BindingMode.OneWay;
                this.Bind(VBTreeView.ChangeInfoProperty, bindingChangeInfo);
            }

            if (BSOACComponent != null)
            {
                binding = new Binding();
                binding.Source = BSOACComponent;
                binding.Path = Const.InitState;
                binding.Mode = BindingMode.OneWay;
                Bind(VBTreeView.ACCompInitStateProperty, binding);
            }

        }

        void VBTreeView_Loaded(object sender, RoutedEventArgs e)
        {
            InitVBControl();
            if (_Loaded)
                return;

            if (BSOACComponent != null)
            {
                BindingExpressionBase boundedValue = BindingOperations.GetBindingExpressionBase(this, VBTreeView.TreeDataSourceProperty);
                if (boundedValue != null)
                {
                    IACObject boundToObject = boundedValue.GetSource() as IACObject;
                    try
                    {
                        if (boundToObject != null)
                            BSOACComponent.AddWPFRef(this.GetHashCode(), boundToObject);
                    }
                    catch (Exception exw)
                    {
                        this.Root().Messages.LogDebug("VBTreeView", "AddWPFRef", exw.Message);
                    }

                    if (this.Items.Count <= 0)
                        FillTree();
                }
            }
            _Loaded = true;
        }

        void VBTreeView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (!_Loaded)
                return;

            if (BSOACComponent != null)
            {
                if (BSOACComponent != null)
                    BSOACComponent.RemoveWPFRef(this.GetHashCode());
            }

            _Loaded = false;
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
            if (!_Initialized)
                return;
            _Initialized = false;
            if (bso != null && bso is IACBSO)
                (bso as IACBSO).RemoveWPFRef(this.GetHashCode());
            RemoveHandler(DragDrop.DragEnterEvent, OnDragEnter);
            RemoveHandler(DragDrop.DragOverEvent, OnDragOver);
            RemoveHandler(DragDrop.DropEvent, OnDrop);
            RemoveHandler(DragDrop.DragLeaveEvent, OnDragLeave);
            this.Loaded -= VBTreeView_Loaded;
            this.Unloaded -= VBTreeView_Unloaded;
            foreach (TreeViewItem item in Items)
            {
                DeInitVBTreeViewItem(item);
            }

            this.ClearAllBindings();
            _VBContentPropertyInfo = null;

            ACQueryDefinition = null;
            CurrentTargetVBDataObject = null;
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == ACCompInitStateProperty)
                InitStateChanged();
            else if (change.Property == BSOACComponentProperty)
            {
                if (change.NewValue == null && change.OldValue != null && !String.IsNullOrEmpty(VBContent))
                {
                    IACBSO bso = change.OldValue as IACBSO;
                    if (bso != null)
                        DeInitVBControl(bso);
                }
            }
            else if (change.Property == ACCaptionProperty)
            {
                if (!_Initialized)
                    return;
                ACCaptionTrans = this.Root().Environment.TranslateText(ContextACObject, ACCaption);
            }
            else if (change.Property == ControlModeProperty)
                UpdateControlMode();
            else if (change.Property == TreeDataSourceProperty && change.NewValue != null && change.NewValue != change.OldValue)
            {
                FillTree(change.NewValue);
            }
            else if (change.Property == SelectedItemProperty && change.NewValue != change.OldValue)
            {
                if (SelectedItem != null)
                {
                    VBTreeViewItem vbTreeViewItem = SelectedItem as VBTreeViewItem;
                    if (vbTreeViewItem == null)
                        return;
                    if (TreeValue != vbTreeViewItem)
                    {
                        TreeValue = vbTreeViewItem;
                    }
                }
            }
            else if (change.Property == TreeValueProperty)
            {
                if (String.IsNullOrEmpty(VBContent))
                    return;
                UpdateValue();
            }
            else if (change.Property == ChangeInfoProperty)
            {
                switch (ChangeInfo.ChangeCmd)
                {
                    case Const.CmdDeleteData:
                        {
                            VBTreeViewItem treeItem = (VBTreeViewItem)SelectedItem;
                            if (treeItem != null)
                            {
                                if (treeItem.Parent is TreeView) // Root-Item
                                {
                                    TreeView treeView = (TreeView)treeItem.Parent;
                                    treeView.Items.Remove(treeItem);
                                }
                                else
                                {
                                    VBTreeViewItem parentTreeItem = (VBTreeViewItem)treeItem.Parent;
                                    if (parentTreeItem != null)
                                    {
                                        if (treeItem.Parent is VBTreeViewItem)
                                        {
                                            VBTreeViewItem parentVBTreeViewItem = treeItem.Parent as VBTreeViewItem;
                                            if (parentVBTreeViewItem.ContentACObject is IACContainerWithItems)
                                                ((IACContainerWithItems)parentVBTreeViewItem.ContentACObject).Remove(treeItem.ContentACObject as IACContainerWithItems);
                                        }
                                        parentTreeItem.Items.Remove(treeItem);
                                    }
                                }
                            }
                        }
                        break;
                    case Const.CmdNewData:
                        {
                            VBTreeViewItem treeItem = (VBTreeViewItem)SelectedItem;
                            if (treeItem != null)
                            {
                                if (treeItem.Parent is TreeView) // Root-Item
                                {
                                    TreeView treeView = (TreeView)treeItem.Parent;
                                    if (ChangeInfo.ChangedObject != null)
                                    {
                                        VBTreeViewItem newTreeItem = CreateNewTreeItem(ChangeInfo.ChangedObject);
                                        treeView.Items.Add(newTreeItem);
                                        newTreeItem.IsSelected = true;
                                        newTreeItem.BringIntoView();
                                    }
                                }
                                else
                                {
                                    VBTreeViewItem parentTreeItem = (VBTreeViewItem)treeItem.Parent;
                                    if (parentTreeItem != null)
                                    {
                                        if (ChangeInfo.ChangedObject != null)
                                        {
                                            VBTreeViewItem newTreeItem = CreateNewTreeItem(ChangeInfo.ChangedObject);
                                            parentTreeItem.Items.Add(newTreeItem);
                                            newTreeItem.IsSelected = true;
                                            newTreeItem.BringIntoView();
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case Const.CmdInsertData:
                        {
                            VBTreeViewItem treeItem = (VBTreeViewItem)SelectedItem;
                            if (treeItem != null && treeItem.Parent is VBTreeViewItem)
                            {
                                VBTreeViewItem parentTreeItem = (VBTreeViewItem)treeItem.Parent;
                                if (parentTreeItem != null)
                                {
                                    // Suche der Position zum einfügen
                                    for (int i = 0; i < parentTreeItem.Items.Count; i++)
                                    {
                                        if (parentTreeItem.Items[i] == treeItem)
                                        {
                                            if (ChangeInfo.ChangedObject != null)
                                            {
                                                VBTreeViewItem newTreeItem = CreateNewTreeItem(ChangeInfo.ChangedObject);
                                                parentTreeItem.Items.Insert(i, newTreeItem);
                                                newTreeItem.IsSelected = true;
                                                newTreeItem.BringIntoView();
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case Const.CmdAddChildData:
                        {
                            VBTreeViewItem treeItem = null;
                            if (ChangeInfo.ParentObject != null)
                            {
                                treeItem = (VBTreeViewItem)FindItem(Items, ChangeInfo.ParentObject);
                            }
                            else
                            {
                                treeItem = (VBTreeViewItem)SelectedItem;
                            }
                            if (treeItem != null)
                            {
                                if (ChangeInfo.ChangedObject != null)
                                {
                                    VBTreeViewItem newTreeItem = FillChilds(treeItem.Items, ChangeInfo.ChangedObject, 1);
                                    if (newTreeItem != null)
                                    {
                                        newTreeItem.IsSelected = true;
                                        newTreeItem.BringIntoView();
                                    }
                                }
                            }
                        }
                        break;
                    case Const.CmdAddChildNoExpand:
                        {
                            VBTreeViewItem treeItem = null;
                            if (ChangeInfo.ParentObject != null)
                                treeItem = (VBTreeViewItem)FindItem(Items, ChangeInfo.ParentObject);
                            else
                                treeItem = (VBTreeViewItem)SelectedItem;

                            if (treeItem != null)
                                if (ChangeInfo.ChangedObject != null)
                                {
                                    VBTreeViewItem newTreeItem = FillChilds(treeItem.Items, ChangeInfo.ChangedObject, 1);
                                }
                        }
                        break;
                    case Const.CmdUpdateAllData:
                        {
                            VBTreeViewItem treeItem = null;
                            if (ChangeInfo.ChangedObject != null)
                            {
                                treeItem = (VBTreeViewItem)FindItem(Items, ChangeInfo.ChangedObject);
                            }
                            else
                            {
                                treeItem = (VBTreeViewItem)SelectedItem;
                            }
                            if (treeItem != null)
                            {
                                treeItem.IsSelected = true;
                                treeItem.BringIntoView();
                            }
                        }
                        break;
                    default:
                        FillTree();
                        break;
                }
            }
        }

        /// <summary>
        /// Deinitializes a tree view item.
        /// </summary>
        /// <param name="item">The tree view item to deinitialize.</param>
        protected virtual void DeInitVBTreeViewItem(TreeViewItem item)
        {
            // Damir TODO: Delete when done with Datatemplate
            if (item == null)
                return;
            if (item.Header != null && item.Header is VBTextBlock)
            {
                VBTextBlock tb = item.Header as VBTextBlock;
                tb.ClearAllBindings();
            }
            else if (item.Header != null && item.Header is Grid)
            {
                Grid g = item.Header as Grid;
                foreach (Control child in g.Children)
                {
                    if (child is VBCheckBox)
                    {
                        VBCheckBox cb = child as VBCheckBox;
                        cb.ClearAllBindings();
                        cb.Click -= cb_Click;
                    }
                    else if (child is VBTextBlock)
                    {
                        VBTextBlock tb = child as VBTextBlock;
                        tb.ClearAllBindings();
                    }
                }
            }
            foreach (TreeViewItem item2 in item.Items)
            {
                DeInitVBTreeViewItem(item2);
            }

        }

        /// <summary>
        /// Calls on when initialization state is changed.
        /// </summary>
        protected void InitStateChanged()
        {
            if (BSOACComponent != null &&
                (ACCompInitState == ACInitState.Destructed || ACCompInitState == ACInitState.DisposedToPool))
                DeInitVBControl(BSOACComponent);
        }
        #endregion

        #region Event-Handling
        #endregion


        #region IVBSource Members

        private VBTreeViewItem CreateNewTreeItem(IACObject acObject)
        {
            VBTreeViewItem newTreeItem = new VBTreeViewItem(this, ContextACObject);
            newTreeItem.ContentACObject = acObject;
            if (this.TreeItemTemplate != null)
            {
                newTreeItem.DataContext = acObject;
                return newTreeItem;
            }

            bool showCheckbox = false;
            IVBDataCheckbox treeEntry = acObject as IVBDataCheckbox;
            if (treeEntry != null)
            {
                showCheckbox = !string.IsNullOrEmpty(treeEntry.DataContentCheckBox);
            }
            object bindingDataObject = acObject;

            string[] dataPath = VBShowColumns.Split('.');

            // Wenn es Properties in untergeordneten Objekten sind, dann sind diese erst zu ermitteln
            for (int i = 0; i < dataPath.Length - 1; i++)
            {
                string path = dataPath[i];
                bindingDataObject = bindingDataObject.GetType().GetProperty(path).GetValue(bindingDataObject, null);
            }
            Binding binding = new Binding(dataPath.Last());
            binding.Source = bindingDataObject;

            if (!showCheckbox)
            {
                VBTextBlock tb = new VBTextBlock();
                tb.Bind(VBTextBlock.TextProperty, binding);
                newTreeItem.Header = tb;
            }
            if (showCheckbox)
            {
                if (!_isRoot)
                {
                    Grid g = new Grid();
                    g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30), MinWidth = 30 });
                    g.ColumnDefinitions.Add(new ColumnDefinition());

                    VBCheckBox cb = new VBCheckBox();
                    cb.Width = 30;
                    cb.MinWidth = 30;
                    Binding bindingCheckbox = new Binding();
                    bindingCheckbox.Mode = BindingMode.TwoWay;
                    bindingCheckbox.Source = bindingDataObject;
                    bindingCheckbox.Path = treeEntry.DataContentCheckBox;
                    cb.Bind(VBCheckBox.IsCheckedProperty, bindingCheckbox);

                    Binding bindingEnabled = new Binding();
                    bindingEnabled.Mode = BindingMode.OneWay;
                    bindingEnabled.Source = bindingDataObject;
                    bindingEnabled.Path = nameof(VBCheckBox.IsEnabled);
                    cb.Bind(VBCheckBox.IsEnabledProperty, bindingEnabled);
                    
                    cb.SetValue(Grid.RowProperty, 0);
                    cb.SetValue(Grid.ColumnProperty, 0);
                    cb.Click += cb_Click;

                    VBTextBlock tb = new VBTextBlock();
                    tb.SetValue(Grid.RowProperty, 0);
                    tb.SetValue(Grid.ColumnProperty, 1);
                    tb.Bind(VBTextBlock.TextProperty, binding);

                    g.Children.Add(tb);
                    g.Children.Add(cb);

                    newTreeItem.Header = g;
                }
                _isRoot = false;
            }
            return newTreeItem;
        }

        public bool IsKeyDown(Key key)
        {
            return _pressedKeys.TryGetValue(key, out bool isPressed) && isPressed;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            _pressedKeys[e.Key] = true;
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            _pressedKeys[e.Key] = false;
            base.OnKeyUp(e);
        }

        internal void cb_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            AvaloniaObject o = VisualTreeHelper.GetParent(cb);
            while (true)
            {
                if (o is TreeViewItem)
                {
                    break;
                }
                o = VisualTreeHelper.GetParent(o);
            }
            VBTreeViewItem vbtvi = (VBTreeViewItem)o;
            vbtvi.IsSelected = true;
            if (vbtvi.ContentACObject is IVBDataCheckbox && !IsKeyDown(Key.LeftCtrl))
            {
                checkChilds(vbtvi, (vbtvi.ContentACObject as IVBDataCheckbox).IsChecked);
                if (CheckToRoot)
                    checkParents(vbtvi);
            }
            vbtvi.BringIntoView();
        }

        private void checkParents(VBTreeViewItem vbtvi)
        {
            object o = vbtvi.Parent;
            if (o.GetType() == typeof(VBTreeViewItem))
            {
                checkParents((VBTreeViewItem)o);
                VBTreeViewItem parent = (VBTreeViewItem)o;

                if (parent.ContentACObject is IVBDataCheckbox)
                {
                    string dataContentCheckBox = ((IVBDataCheckbox)parent.ContentACObject).DataContentCheckBox;
                    if (!string.IsNullOrEmpty(dataContentCheckBox))
                    {
                        Type t = parent.ContentACObject.GetType();
                        PropertyInfo pi = t.GetProperty(dataContentCheckBox);
                        pi.SetValue(parent.ContentACObject, true, null);
                    }
                }
            }
            else
            {
                return;
            }
        }

        private void checkChilds(VBTreeViewItem vbtvi, bool isChecked)
        {
            foreach (VBTreeViewItem tvi in vbtvi.Items)
            {
                checkChilds(tvi, isChecked);

                if (tvi.ContentACObject is IVBDataCheckbox)
                {
                    string dataContentCheckBox = ((IVBDataCheckbox)tvi.ContentACObject).DataContentCheckBox;
                    if (!string.IsNullOrEmpty(dataContentCheckBox))
                    {
                        Type t = tvi.ContentACObject.GetType();
                        if (t != null)
                        {
                            PropertyInfo pi = t.GetProperty(dataContentCheckBox);
                            if (pi != null)
                            {
                                pi.SetValue(tvi.ContentACObject, isChecked, null);
                            }
                        }
                    }
                }
            }
        }

        private object GetBSOData(Guid guid)
        {
            if (ContextACObject == null)
                return null;
            Type t = ContextACObject.GetType();
            Object dataObjectRoot = null;
            try
            {
                // Rootelement ermitteln
                dataObjectRoot = t.InvokeMember(VBSource, Global.bfGetProp, null, ContextACObject, null);

                return GetDataFromChilds(dataObjectRoot, guid);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("VBTreeView", "GetBSOData", msg);
                return null;
            }
        }

        private object GetDataFromChilds(object dataObject, Guid guid)
        {
            if (dataObject is IEnumerable)
            {
                IEnumerable list = dataObject as IEnumerable;
                foreach (var item in list)
                {
                    object result = GetDataFromChilds(item, guid);
                    if (result != null)
                        return result;
                }
                return null;
            }

            Type t2 = dataObject.GetType();

            object actItem = dataObject;
            string[] dataPath = VBChilds.Split('.');

            // Wenn es Properties in untergeordneten Objekten sind, dann sind diese erst zu ermitteln
            for (int i = 0; i < dataPath.Length - 1; i++)
            {
                string path = dataPath[i];
                actItem = actItem.GetType().GetProperty(path).GetValue(actItem, null);
            }

            t2 = actItem.GetType();
            Object dataChilds = t2.InvokeMember(dataPath.Last(), Global.bfGetProp, null, actItem, null);
            if (dataChilds != null && dataChilds is IEnumerable)
            {
                // Es gibt keinen Rootknoten
                var listValues = dataChilds as IEnumerable;
                foreach (var dataObjectChild in listValues)
                {
                    object result = GetDataFromChilds(dataObjectChild, guid);
                    if (result != null)
                        return result;
                }
            }

            return null;
        }
        #endregion

        #region IDataContent Members
        /// <summary>
        /// ACAction is called when one IACInteractiveObject (Source) wants to inform another IACInteractiveObject (Target) about an relevant interaction-event.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        public void ACAction(ACActionArgs actionArgs)
        {
            if (!(ContextACObject is IACComponent))
                return;
            (ContextACObject as IACComponent).ACActionToTarget(CurrentTargetVBDataObject, actionArgs);
        }

        /// <summary>
        /// It's called at the Target-IACInteractiveObject to inform the Source-IACInteractiveObject that ACAction ist allowed to be invoked.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        /// <returns><c>true</c> if ACAction can be invoked otherwise, <c>false</c>.</returns>
        public bool IsEnabledACAction(ACActionArgs actionArgs)
        {
            if (!(ContextACObject is IACComponent))
                return false;
            return (ContextACObject as IACComponent).IsEnabledACActionToTarget(CurrentTargetVBDataObject, actionArgs);
        }

        /// <summary>
        /// Updates a control mode.
        /// </summary>
        public void UpdateControlMode()
        {
            IACComponent elementACComponent = ContextACObject as IACComponent;
            if (elementACComponent == null)
                return;
            Global.ControlModesInfo controlModeInfo = elementACComponent.GetControlModes(this);
            Global.ControlModes controlMode = controlModeInfo.Mode;
            Enabled = controlMode >= Global.ControlModes.Enabled;
            IsVisible = controlMode >= Global.ControlModes.Disabled;
            RemoteCommandAdornerManager.Instance.VisualizeIfRemoteControlled(this, elementACComponent, false);
        }

        /// <summary>
        /// Checks and corrects the control modes.
        /// </summary>
        public void ControlModeChanged()
        {
            if (Enabled)
            {
                if (VBContentPropertyInfo != null)
                {
                    if (VBContentPropertyInfo.IsNullable)
                    {
                        ControlMode = Global.ControlModes.Enabled;
                    }
                    else
                    {
                        ControlMode = Global.ControlModes.EnabledRequired;
                    }
                }
                else
                    ControlMode = Global.ControlModes.Disabled;
            }
            else
            {
                ControlMode = Global.ControlModes.Disabled;
            }
        }
        #endregion


        #region private methods


        internal void treeViewItem_MouseRightButtonDown(object sender, PointerReleasedEventArgs e)
        {
            if (DisableContextMenu)
            {
                e.Handled = true;
                return;
            }
            if (((TreeViewItem)sender).IsSelected != true)
                ((TreeViewItem)sender).IsSelected = true;
            if (sender is TreeViewItem)
            {
                if (ContextACObject == null)
                    return;
                Point point = e.GetPosition(sender as TreeViewItem);

                ACActionMenuArgs actionArgs = new ACActionMenuArgs(sender as IACInteractiveObject, point.X, point.Y, Global.ElementActionType.ContextMenu);
                BSOACComponent.ACAction(actionArgs);
                if (actionArgs.ACMenuItemList != null && actionArgs.ACMenuItemList.Any())
                {
                    VBContextMenu vbContextMenu = new VBContextMenu(sender as IACInteractiveObject, actionArgs.ACMenuItemList);
                    this.ContextMenu = vbContextMenu;
                    //@ihrastinski NOTE: Remote desktop context menu problem - added placement target
                    if (((TreeViewItem)sender).Parent is TreeView)
                        vbContextMenu.PlacementTarget = this;
                    ContextMenu.Open();
                }
                else
                {
                    this.ContextMenu = null;
                }
                e.Handled = true;
            }
            else
                e.Handled = true;
        }

        private void FillTree()
        {
            Items.Clear();

            if (ContextACObject == null)
                return;
            Type t = ContextACObject.GetType();
            Object root = ContextACObject.ACUrlCommand(VBSource, null);
            if (root == null)
            {
                root = ContextACObject;
                string[] dataPath = VBSource.Split('\\');

                // Wenn es Properties in untergeordneten Objekten sind, dann sind diese erst zu ermitteln
                for (int i = 0; i < dataPath.Length - 1; i++)
                {
                    string path = dataPath[i];
                    root = root.GetType().GetProperty(path).GetValue(root, null);
                }
                try
                {
                    t = root.GetType();
                    PropertyInfo pi = t.GetProperty(dataPath.Last());
                    root = pi.GetValue(root, null);
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException(nameof(VBTreeView), nameof(FillTree), msg);
                    return;
                }
            }
            FillTree(root);
        }

        private void FillTree(object root)
        {
            Items.Clear();
            if (root == null)
                return;
            IACObject saveACObject = null;
            if (ContextACObject != null)
                saveACObject = ContextACObject.ACUrlCommand(VBContent, null) as IACObject;
            if (root is IEnumerable)
            {
                // Es gibt keinen Rootknoten
                var listValues = root as IEnumerable;

                if (!string.IsNullOrEmpty(SortOrder))
                {
                    List<SortItem> sortItems = new List<SortItem>();

                    foreach (var dataClass in listValues)
                    {
                        sortItems.Add(new SortItem(dataClass as IACObject, SortOrder));
                    }

                    var sorted = sortItems.OrderBy(c => c.Property).Select(c => c.Item);
                    foreach (var item in sorted)
                    {

                        _isRoot = CheckBoxLevel > 0;
                        FillChilds(Items, item, 0);
                    }
                }
                else
                {
                    List<SortItem> sortItems = new List<SortItem>();

                    foreach (var item in listValues)
                    {
                        FillChilds(Items, item as IACObject, 1);
                    }
                }
            }
            else
            {
                // Es gibt einen Rootknoten
                _isRoot = CheckBoxLevel > 0;
                FillChilds(Items, root as IACObject, 0);
            }
            if (Items.Count > 0)
            {
                VBTreeViewItem treeItem = null;
                if (saveACObject != null)
                {
                    treeItem = FindVBTreeViewItemByContent(Items, saveACObject);
                }
                if (treeItem == null)
                    treeItem = (VBTreeViewItem)Items[0];
                treeItem.IsSelected = true;
                treeItem.BringIntoView();
            }
        }

        private VBTreeViewItem FillChilds(ItemCollection itemColletion, IACObject dataObject, int level)
        {
            IVBIsVisible isVisibleItem = dataObject as IVBIsVisible;
            if (isVisibleItem != null && !isVisibleItem.IsVisible)
                return null;
            VBTreeViewItem newTreeItem = CreateNewTreeItem(dataObject);
            if (level < ExpandLevel)
            {
                newTreeItem.IsExpanded = true;
            }
            itemColletion.Add(newTreeItem);

            object actItem = dataObject;
            string[] dataPath = VBChilds.Split('.');

            // Wenn es Properties in untergeordneten Objekten sind, dann sind diese erst zu ermitteln
            for (int i = 0; i < dataPath.Length - 1; i++)
            {
                string path = dataPath[i];
                actItem = actItem.GetType().GetProperty(path).GetValue(actItem, null);
            }

            Type t1 = actItem.GetType();
            PropertyInfo pi = t1.GetProperty(dataPath.Last());
            Object o2 = pi.GetValue(actItem, null);
            if (o2 is IEnumerable)
            {
                // TODO: Sortierung der Elemente
                // Es gibt keinen Rootknoten
                var listValues = o2 as IEnumerable;
                if (!string.IsNullOrEmpty(SortOrder))
                {
                    List<SortItem> sortItems = new List<SortItem>();

                    foreach (var dataClass in listValues)
                    {
                        sortItems.Add(new SortItem(dataClass as IACObject, SortOrder));
                    }

                    var sorted = sortItems.OrderBy(c => c.Property).Select(c => c.Item);
                    foreach (var item in sorted)
                    {
                        FillChilds(newTreeItem.Items, item, level + 1);
                    }
                }
                else
                {
                    List<SortItem> sortItems = new List<SortItem>();

                    foreach (var item in listValues)
                    {
                        FillChilds(newTreeItem.Items, item as IACObject, level + 1);
                    }
                }
            }
            return newTreeItem;
        }

        internal VBTreeViewItem FillChildsOnItemExpand(VBTreeViewItem expandedItem)
        {
            expandedItem.IsExpanded = true;
            object actItem = expandedItem.ContentACObject;
            string[] dataPath = VBChilds.Split('.');

            // Wenn es Properties in untergeordneten Objekten sind, dann sind diese erst zu ermitteln
            for (int i = 0; i < dataPath.Length - 1; i++)
            {
                string path = dataPath[i];
                actItem = actItem.GetType().GetProperty(path).GetValue(actItem, null);
            }

            Type t1 = actItem.GetType();
            PropertyInfo pi = t1.GetProperty(dataPath.Last());
            Object o2 = pi.GetValue(actItem, null);
            if (o2 is IEnumerable)
            {
                // TODO: Sortierung der Elemente
                // Es gibt keinen Rootknoten
                var listValues = o2 as IEnumerable;
                if (!string.IsNullOrEmpty(SortOrder))
                {
                    List<SortItem> sortItems = new List<SortItem>();

                    foreach (var dataClass in listValues)
                    {
                        sortItems.Add(new SortItem(dataClass as IACObject, SortOrder));
                    }

                    var sorted = sortItems.OrderBy(c => c.Property).Select(c => c.Item);
                    foreach (var item in sorted)
                    {
                        FillChilds(expandedItem.Items, item, 1);
                    }
                }
                else
                {
                    List<SortItem> sortItems = new List<SortItem>();

                    foreach (var item in listValues)
                    {
                        FillChilds(expandedItem.Items, item as IACObject, 1);
                    }
                }
            }
            return expandedItem;
        }

        /// <summary>
        /// Sucht das VBTreeViewItem in dem das acObject gefunden wird
        /// </summary>
        /// <param name="itemColletion"></param>
        /// <param name="acObject"></param>
        /// <returns></returns>
        private VBTreeViewItem FindVBTreeViewItemByContent(ItemCollection itemColletion, IACObject acObject)
        {
            if (acObject is IACContainerWithItems)
            {
                foreach (var item in itemColletion)
                {
                    VBTreeViewItem vbTreeViewItem = item as VBTreeViewItem;
                    if (vbTreeViewItem.ContentACObject is IACContainerWithItems)
                    {
                        if ((acObject as IACContainerWithItems).Value == (vbTreeViewItem.ContentACObject as IACContainerWithItems).Value)
                        {
                            return vbTreeViewItem;
                        }
                    }
                    var result = FindVBTreeViewItemByContent(vbTreeViewItem.Items, acObject);
                    if (result != null)
                        return result;
                }
                return null;
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region DragAndDrop

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (e.Properties.IsLeftButtonPressed 
                && (DragEnabled == DragMode.Enabled || (e.KeyModifiers == KeyModifiers.Control && DragEnabled == DragMode.EnabledMove)))
            {
                Thumb thumb = e.Source as Thumb;
                if (thumb != null && thumb.TemplatedParent != null && thumb.TemplatedParent is ScrollBar)
                    return;

                VBTreeViewItem vbTreeViewItem = SelectedItem as VBTreeViewItem;
                if (vbTreeViewItem != null)
                {
                    UpdateValue(true);
                    TreeItemClicked++;

                    try
                    {
                        VBDragDrop.VBDoDragDrop(e, vbTreeViewItem);
                    }
                    catch (Exception) 
                    {
                    }
                }
            }
            base.OnPointerPressed(e);

        }
        #endregion

        private void UpdateValue(bool always = false)
        {
            if (ContextACObject == null)
                return;
            if (TreeValue != null)
            {
                if (always || ContextACObject.ACUrlCommand(VBContent, null) != TreeValue.ContentACObject)
                {
                    ContextACObject.ACUrlCommand(VBContent, TreeValue.ContentACObject);
                }
            }
            else
            {
                VBTreeViewItem value = (VBTreeViewItem)SelectedItem;
                if (always || ContextACObject.ACUrlCommand(VBContent, null) != value.ContentACObject)
                {
                    ContextACObject.ACUrlCommand(VBContent, value.ContentACObject);
                }
            }
        }

        /// <summary>
        /// Finds the tree view item in the collection.
        /// </summary>
        /// <param name="itemCollection">The items collection.</param>
        /// <param name="acObject">The acObject.</param>
        /// <returns>TreeViewItem if it was found, otherwise false.</returns>
        public TreeViewItem FindItem(ItemCollection itemCollection, IACObject acObject)
        {
            foreach (var item in itemCollection)
            {
                VBTreeViewItem vbTreeViewItem = item as VBTreeViewItem;

                if (vbTreeViewItem.ContentACObject == acObject)
                    return vbTreeViewItem;
                TreeViewItem treeViewItem = FindItem(vbTreeViewItem.Items, acObject);
                if (treeViewItem != null)
                    return treeViewItem;
            }
            return null;
        }


        #region DragAndDrop
        /// <summary>
        /// Handles the OnDragEnter event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected void OnDragEnter(DragEventArgs e)
        {
            if (this.GetVBDesign().IsDesignerActive)
                return;
            HandleDragOver(this, e);
        }

        /// <summary>
        /// Handles the OnDragLeave event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected void OnDragLeave(DragEventArgs e)
        {
            if (this.GetVBDesign().IsDesignerActive)
                return;
            HandleDragOver(this, e);
        }

        /// <summary>
        /// Handles the OnDragOver event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected void OnDragOver(DragEventArgs e)
        {
            if (this.GetVBDesign().IsDesignerActive)
                return;
            HandleDragOver(this, e);
        }

        /// <summary>
        /// Handles the OnDrop event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected void OnDrop(DragEventArgs e)
        {
            if (this.GetVBDesign().IsDesignerActive)
                return;
            HandleDrop(this, e);
        }

        /// <summary>
        /// Handles the DragEnter event.
        /// </summary>
        /// <param name="sender">The sender parameter.</param>
        /// <param name="e">The event arguments.</param>
        public void HandleDragEnter(object sender, DragEventArgs e)
        {
        }

        /// <summary>
        /// Handles the DragLeave event.
        /// </summary>
        /// <param name="sender">The sender parameter.</param>
        /// <param name="e">The event arguments.</param>
        public void HandleDragLeave(object sender, DragEventArgs e)
        {
        }

        /// <summary>
        /// Handles the DragOver event.
        /// </summary>
        /// <param name="sender">The sender parameter.</param>
        /// <param name="e">The event arguments.</param>
        public void HandleDragOver(object sender, DragEventArgs e)
        {
            Control uiElement = e.Source as Control;

            if (uiElement == null)
            {
                e.DragEffects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            switch (e.DragEffects)
            {
                case DragDropEffects.Move: // Vorhandene Elemente verschieben
                    HandleDragOver_Move(sender, 0, 0, e);
                    return;
                case DragDropEffects.Copy: // Neue Elemente einfügen
                case DragDropEffects.Copy | DragDropEffects.Move:
                case DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link:
                    HandleDragOver_Copy(sender, 0, 0, e);
                    return;
                default:
                    e.DragEffects = DragDropEffects.None;
                    e.Handled = true;
                    return;
            }
        }

        private void HandleDragOver_Move(object sender, double x, double y, DragEventArgs e)
        {
            Control uiElement = e.Source as Control;

            if (uiElement == null)
            {
                e.DragEffects = DragDropEffects.None;
                e.Handled = true;
                return;
            }
            CurrentTargetVBDataObject = GetNearestContainer(uiElement);

            IACInteractiveObject dropObject = VBDragDrop.GetDropObject(e);
            if (dropObject == null || CurrentTargetVBDataObject == null)
            {
                e.DragEffects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            ACActionArgs actionArgs = new ACActionArgs(dropObject, x, y, Global.ElementActionType.Move);
            if (IsEnabledACAction(actionArgs))
            {
                e.DragEffects = DragDropEffects.Move;
            }
            else
            {
                e.DragEffects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void HandleDragOver_Copy(object sender, double x, double y, DragEventArgs e)
        {
            Control uiElement = e.Source as Control;

            if (uiElement == null)
            {
                e.DragEffects = DragDropEffects.None;
                e.Handled = true;
                return;
            }
            CurrentTargetVBDataObject = GetNearestContainer(uiElement);
            IACInteractiveObject dropObject = VBDragDrop.GetDropObject(e);
            if (dropObject == null || CurrentTargetVBDataObject == null)
            {
                e.DragEffects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            // Wenn alle generellen Vorbedingungen erfüllt sind, dann wird
            // noch das BSO gefragt, ob das kopieren (einfügen) erlaubt ist

            Global.ElementActionType elementActionType = Global.ElementActionType.Drop;
            if (IsKeyDown(Key.LeftShift) && DragEnabled == DragMode.EnabledMove)
            {
                elementActionType = Global.ElementActionType.Move;
            }

            ACActionArgs actionArgs = new ACActionArgs(dropObject, x, y, elementActionType);
            if (IsEnabledACAction(actionArgs))
            {
                e.DragEffects = DragDropEffects.Copy;
            }
            else
            {
                e.DragEffects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        /// <summary>
        /// Handles the Drop event.
        /// </summary>
        /// <param name="sender">The sender parameter.</param>
        /// <param name="e">The event arguments.</param>
        public void HandleDrop(object sender, DragEventArgs e)
        {
            Control uiElement = e.Source as Control;

            if (uiElement == null)
            {
                e.DragEffects = DragDropEffects.None;
                e.Handled = true;
                return;
            }
            CurrentTargetVBDataObject = GetNearestContainer(uiElement);
            IACInteractiveObject dropObject = VBDragDrop.GetDropObject(e);
            if (dropObject == null)
            {
                e.DragEffects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            Global.ElementActionType elementActionType = Global.ElementActionType.Drop;

            if (e.KeyModifiers == KeyModifiers.Shift && DragEnabled == DragMode.EnabledMove)
                elementActionType = Global.ElementActionType.Move;
            switch (elementActionType)
            {
                case Global.ElementActionType.Move: // Vorhandene Elemente verschieben
                    {
                        ACActionArgs actionArgs = new ACActionArgs(dropObject, 0, 0, Global.ElementActionType.Move);
                        ACAction(actionArgs);
                        e.Handled = true;
                    }
                    return;
                case Global.ElementActionType.Drop: // Neue Elemente einfügen
                    {
                        ACActionArgs actionArgs = new ACActionArgs(dropObject, 0, 0, Global.ElementActionType.Drop);
                        ACAction(actionArgs);
                        e.Handled = true;
                    }
                    return;
                default:
                    e.DragEffects = DragDropEffects.None;
                    return;
            }
        }

        private VBTreeViewItem GetNearestContainer(Control element)
        {
            // Walk up the element tree to the nearest tree view item.
            TreeViewItem container = element as TreeViewItem;
            while ((container == null) && (element != null))
            {
                element = VisualTreeHelper.GetParent(element) as Control;
                container = element as TreeViewItem;
            }

            return container as VBTreeViewItem;
        }

        #endregion

        #region IACObject
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
        #endregion

        #region IACMenuBuilder Member
        /// <summary>
        /// Gets the context menu.
        /// </summary>
        /// <param name="vbContent">The vbContent parameter.</param>
        /// <param name="vbControl">The vbControl parameter.</param>
        ///<returns>Returns the list of ACMenu items.</returns>
        public virtual ACMenuItemList GetMenu(string vbContent, string vbControl)
        {
            ACMenuItemList acMenuItemList = new ACMenuItemList();
            AppendMenu(vbContent, vbControl, ref acMenuItemList);
            return acMenuItemList;
        }

        /// <summary>
        /// Appends the context menu.
        /// </summary>
        /// <param name="vbContent">The vbContent parameter.</param>
        /// <param name="vbControl">The vbControl parameter.</param>
        ///<param name="acMenuItemList">The acMenuItemList parameter.</param>
        public virtual void AppendMenu(string vbContent, string vbControl, ref ACMenuItemList acMenuItemList)
        {
            VBLogicalTreeHelper.AppendMenu(this, vbContent, vbControl, ref acMenuItemList);
            this.GetDesignManagerMenu(VBContent, ref acMenuItemList);
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
    }
}
