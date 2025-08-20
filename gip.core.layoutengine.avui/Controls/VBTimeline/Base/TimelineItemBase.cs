using System.Runtime.CompilerServices;
using gip.core.datamodel;
using gip.core.layoutengine.avui.ganttchart;
using gip.core.layoutengine.avui.Helperclasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace gip.core.layoutengine.avui.timeline
{
    public abstract class TimelineItemBase : ContentControl, INotifyPropertyChanged, IACInteractiveObject, IACObject, IACMenuBuilder
    {
        #region Loaded-Event

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            Loaded += TimelineItem_Loaded;
        }

        internal virtual void TimelineItem_Loaded(object sender, RoutedEventArgs e)
        {
            int toolTipChildrenCount = 0;

            if (VBTimelineChart.container.Children.Count == 0 || (VBTimelineChart.container.Children.Count == 3 && VBTimelineChart.container.Children[2] != contentControl) && contentControl != null)
            {
                VBTimelineChart.container.Children.Clear();
                if (VBTimelineChart.container.Children.Count == 0)
                {
                    if (_ToolTipStatus == null && this.ContentTemplate != null)
                        _ToolTipStatus = this.ContentTemplate.Resources["ToolTipStatus"] as StackPanel;

                    if (_ToolTipStatus != null)
                    {
                        VBTimelineChart.container.Children.Add(_ToolTipStatus);
                        VBTimelineChart.container.Children.Add(new Separator() { Height = 5, Background = Brushes.Transparent });
                        toolTipChildrenCount = 2;
                    }
                }
            }
            else if (contentControl == null)
            {
                Binding bind = new Binding();
                bind.Source = this;
                bind.Path = new PropertyPath("ToolTipContent");
                contentControl = new ContentControl();
                contentControl.SetBinding(ContentControl.ContentProperty, bind);
            }
            if (VBTimelineChart.container.Children.Count == toolTipChildrenCount && contentControl != null)
                VBTimelineChart.container.Children.Add(contentControl);
        }

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            InitVBControl();
        }

        bool _IsInitialized = false;
        
        StackPanel _ToolTipStatus;
        ContentControl contentControl;

        public virtual void InitVBControl()
        {
            if (_IsInitialized)
                return;

            Binding binding = new Binding();
            binding.Source = this.Content;
            binding.Path = new PropertyPath(VBTimelineView.GanttStart.Replace("\\", "."));
            this.SetBinding(TimelinePanel.StartDateProperty, binding);

            Binding binding2 = new Binding();
            binding2.Source = this.Content;
            binding2.Path = new PropertyPath(VBTimelineView.GanttEnd.Replace("\\", "."));
            this.SetBinding(TimelinePanel.EndDateProperty, binding2);

            Binding binding3 = new Binding();
            binding3.Source = this.Content;
            binding3.Path = new PropertyPath("DisplayOrder");
            this.SetBinding(TimelinePanel.RowIndexProperty, binding3);

            _IsInitialized = true;
        }

        #endregion

        #region Properties

        private VBTimelineViewBase _VBTimelineView;
        public VBTimelineViewBase VBTimelineView
        {
            get
            {
                if (_VBTimelineView == null)
                    _VBTimelineView = Helperclasses.VBVisualTreeHelper.FindParentObjectInVisualTree(this, typeof(VBTimelineViewBase)) as VBTimelineViewBase;
                return _VBTimelineView;
            }
        }

        private VBTimelineChartBase _VBTimelineChart;
        public VBTimelineChartBase VBTimelineChart
        {
            get
            {
                if (_VBTimelineChart == null)
                    _VBTimelineChart = WpfUtility.FindVisualParent<VBTimelineChartBase>(this);
                return _VBTimelineChart;
            }
        }

        public VBTreeListViewItem VBTreeListViewItemMap;

        private bool _IsToolTipEnabled = true;
        public bool IsToolTipEnabled
        {
            get => _IsToolTipEnabled;
            set => _IsToolTipEnabled = value;
        }

        #endregion

        #region DependencyProp

        public bool IsCollapsed
        {
            get { return (bool)GetValue(IsCollapsedProperty); }
            set { SetValue(IsCollapsedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsCollapsed.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsCollapsedProperty =
                DependencyProperty.Register("IsCollapsed", typeof(bool), typeof(TimelineItemBase),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsParentMeasure, IsCollapsedChanged));

        private static void IsCollapsedChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            TimelineItemBase item = (TimelineItemBase)o;
            item.OnPropertyChanged("IsCollapsed");
        }

        /// <summary>
        /// Get or set indication that this timeline item should be display
        /// as zero length.
        /// This can be either because the item length is really zero or
        /// because its length fall below the zoom factor.
        /// </summary>
        public bool IsDisplayAsZero
        {
            get { return (bool)GetValue(IsDisplayAsZeroProperty); }
            set { SetValue(IsDisplayAsZeroProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsDisplayAsZero.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDisplayAsZeroProperty =
                DependencyProperty.Register("IsDisplayAsZero", typeof(bool), typeof(TimelineItemBase), new FrameworkPropertyMetadata(false, IsDisplayAsZeroChanged));

        private static void IsDisplayAsZeroChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            TimelineItemBase item = (TimelineItemBase)o;
            item.OnPropertyChanged("IsDisplayAsZero");
        }

        public static readonly DependencyProperty IsSelectedProperty
            = DependencyProperty.Register("IsSelected", typeof(bool), typeof(TimelineItemBase), new PropertyMetadata(false, IsSelectedChanged));
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static void IsSelectedChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            TimelineItemBase item = (TimelineItemBase)o;
            if (item.IsSelected)
            {
                item.VBTimelineChart.SelectedItem = item.DataContext;
                if (Mouse.LeftButton == MouseButtonState.Pressed)
                    item.ScrollToItemStart();
            }
        }

        public static readonly DependencyProperty ToolTipContentProperty = DependencyProperty.Register("ToolTipContent", typeof(StackPanel), typeof(TimelineItemBase));

        internal StackPanel ToolTipContent
        {
            get { return (StackPanel)GetValue(ToolTipContentProperty); }
            set { SetValue(ToolTipContentProperty, value); }
        }

        #endregion

        #region Methods

        public virtual bool DeInitVBControl()
        {
            VBTimelineChart.container.Children.Clear();
            if (contentControl != null)
                BindingOperations.ClearBinding(contentControl, ContentControl.ContentProperty);
            BindingOperations.ClearBinding(this, TimelinePanel.StartDateProperty);
            BindingOperations.ClearBinding(this, TimelinePanel.EndDateProperty);
            BindingOperations.ClearBinding(this, TimelinePanel.RowIndexProperty);
            BindingOperations.ClearBinding(this, ToolTipContentProperty);
            BindingOperations.ClearAllBindings(this);
            ToolTipOpening -= TimelineItem_ToolTipOpening;
            Loaded -= TimelineItem_Loaded;
            VBTreeListViewItemMap = null;
            _IsInitialized = false;
            return true;
        }

        public virtual void SelectTreeItem()
        {
            if (VBTreeListViewItemMap != null)
                VBTreeListViewItemMap.IsSelected = true;
        }

        private void ScrollToItemStart()
        {
            if (VBTimelineChart.IsAncestorOf(this))
            {
                VBTimelineChart.PART_AxesPanel._IsScrollFromZoom = false;
                VBTimelineChart.ScrollViewer.ScrollToHorizontalOffset(VBTimelineChart.ScrollViewer.HorizontalOffset + TransformToAncestor(VBTimelineChart).Transform(new Point()).X - VBTimelineChart.ActualWidth / 2);
            }
        }

        private void ScrollToItemEnd()
        {
            if (VBTimelineChart.IsAncestorOf(this))
            {
                VBTimelineChart.PART_AxesPanel._IsScrollFromZoom = false;
                VBTimelineChart.ScrollViewer.ScrollToHorizontalOffset(VBTimelineChart.ScrollViewer.HorizontalOffset + TransformToAncestor(VBTimelineChart).Transform(new Point()).X + ActualWidth - VBTimelineChart.ActualWidth / 2);
            }
        }

        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            if(!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.LeftShift))
                SelectTreeItem();
            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.LeftShift) && VBTreeListViewItemMap != null)
            {
                VBTreeListViewItemMap.IsSelected = true;
                VBTreeListViewItemMap.IsExpanded = !VBTreeListViewItemMap.IsExpanded;
            }
            base.OnMouseDoubleClick(e);
        }

        protected override void OnPreviewMouseRightButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            if (VBTreeListViewItemMap != null)
                VBTreeListViewItemMap.IsSelected = true;
            Point point = e.GetPosition(this);
            ACActionMenuArgs actionArgs = new ACActionMenuArgs(this, point.X, point.Y, Global.ElementActionType.ContextMenu);
            VBTimelineChart.BSOACComponent.ACAction(actionArgs);
            VBContextMenu vbContextMenu = new VBContextMenu(this, actionArgs.ACMenuItemList);
            this.ContextMenu = vbContextMenu;
            //@ihrastinski NOTE: Remote desktop context menu problem - added placement target
            if (vbContextMenu.PlacementTarget == null)
                vbContextMenu.PlacementTarget = this;
            ContextMenu.IsOpen = true;
            //e.Handled = true;
            base.OnPreviewMouseRightButtonDown(e);
        }

        public ACMenuItemList GetMenu(string vbContent, string vbControl)
        {
            ACMenuItemList acMenuItemList = new ACMenuItemList();
            AppendMenu(vbContent, "TimelineItem", ref acMenuItemList);
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
            this.GetDesignManagerMenu(vbContent, ref acMenuItemList);
        }

        [ACMethodInteraction("", "en{'Go to start'}de{'Zum Start gehen'}", 998, false)]
        public void GoToStart()
        {
            ScrollToItemStart();
        }

        [ACMethodInteraction("", "en{'Go to end'}de{'Zum Ende gehen'}", 999, false)]
        public void GoToEnd()
        {
            ScrollToItemEnd();
        }

        internal virtual void TimelineItem_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            if (!IsToolTipEnabled)
            {
                ToolTip = null;
                return;
            }

            if (ToolTipContent != null)
                foreach (FrameworkElement element in ToolTipContent.Children)
                {
                    element.DataContext = DataContext;
                    element.OnApplyTemplate();
                }
            ToolTip = VBTimelineChart.container;
        }

        internal void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var handlers = PropertyChanged;
            if (handlers != null)
            {
                handlers(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
        /// The ACUrlBinding()-Method returns a Source and a Path which the Control use to create a WPF-Binding to bind the right value and set the WPF-DataContext.
        /// ACUrl's can be either absolute or relative to the DataContext of the parent WPFControl (or the ContextACObject of the parent IACInteractiveObject)</summary>
        /// <value>Relative or absolute ACUrl</value>
        [Category("VBControl")]
        public string VBContent
        {
            get { return VBTimelineChart.VBContent; }
        }

        /// <summary>
        /// ContextACObject is used by WPF-Controls and mostly it equals to the FrameworkElement.DataContext-Property.
        /// IACInteractiveObject-Childs in the logical WPF-tree resolves relative ACUrl's to this ContextACObject-Property.
        /// </summary>
        /// <value>The Data-Context as IACObject</value>
        public IACObject ContextACObject
        {
            get { return this.DataContext as IACObject; }
        }

        /// <summary>
        /// ACAction is called when one IACInteractiveObject (Source) wants to inform another IACInteractiveObject (Target) about an relevant interaction-event.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        public void ACAction(ACActionArgs actionArgs)
        {
            if (actionArgs.ElementAction == Global.ElementActionType.ACCommand)
            {
                var query = actionArgs.DropObject.ACContentList.Where(c => c is ACCommand);
                if (query.Any())
                {
                    ACCommand acCommand = query.First() as ACCommand;
                    if (!acCommand.ParameterList.Any())
                    {
                        ACUrlCommand(acCommand.GetACUrl());
                        return;
                    }
                }
            }
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

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public string ACIdentifier
        {
            get { return this.Name; }
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        public string ACCaption
        {
            get { return ACIdentifier; }
        }

        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        public IACType ACType
        {
            get { return this.ReflectACType(); }
        }

        /// <summary>
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        public IEnumerable<IACObject> ACContentList
        {
            get { return this.ReflectGetACContentList(); }
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
            return false;
        }

        public bool ACUrlTypeInfo(string acUrl, ref ACUrlTypeInfo acUrlTypeInfo)
        {
            return this.ReflectACUrlTypeInfo(acUrl, ref acUrlTypeInfo);
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
