using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using gip.core.datamodel;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Represents a docking panel for tabbed document. Use with <see cref="VBDockingGrid"/>
    /// </summary>

    partial class VBDockingPanelTabbedDoc : VBDockingPanelBase
    {
        public readonly List<VBDockingContainerBase> Documents = new List<VBDockingContainerBase>();

        //public VBDockingPanelTabbedDoc()
        //    : base(null)
        //{
        //    InitializeComponent();
        //}

        public VBDockingPanelTabbedDoc(VBDockingManager dockManager)
            : base(dockManager)
        {
            InitializeComponent();
        }

        internal override void DeInitVBControl(IACComponent bso = null)
        {
            base.DeInitVBControl(bso);
            if (tbcDocuments != null)
            {
                if (tbcDocuments.Items != null)
                    tbcDocuments.Items.Clear();
            }
            if (Documents != null)
            {
                Documents.Clear();
            }
            if (cxDocumentSwitchMenu != null)
                cxDocumentSwitchMenu = null;
        }


        public VBDockingContainerTabbedDoc ActiveDocument
        {
            get
            {
                if (tbcDocuments.SelectedContent == null)
                    return null;

                return Documents[tbcDocuments.SelectedIndex] as VBDockingContainerTabbedDoc;
            }
        }

        public override VBDockingContainerBase ActiveContent
        {
            get
            {
                if (tbcDocuments.SelectedContent == null)
                    return null;
                if (tbcDocuments.SelectedIndex < 0 || Documents.Count < (tbcDocuments.SelectedIndex+1))
                    return null;
                return Documents[tbcDocuments.SelectedIndex];
            }
            set
            {
                if (Documents.Count > 1)
                {
                    tbcDocuments.SelectedIndex = Documents.IndexOf(value);
                }
            }
        }

        private string _FocusView;
        public string FocusView
        {
            get
            {
                return _FocusView;
            }
            set
            {
                if (_FocusView != value)
                {
                    _FocusView = value;
                    if (value != null)
                        foreach (var tabItem in tbcDocuments.Items)
                        {
                            VBTabItem vbTabItem = tabItem as VBTabItem;
                            if (vbTabItem != null && vbTabItem.FocusNames != null)
                                if (vbTabItem.FocusNames.Contains(value))
                                {
                                    vbTabItem.Focus();
                                    break;
                                }
                        }
                }
            }
        }

        //void OnUnloaded(object sender, EventArgs e)
        //{
        //    foreach (ManagedContent content in Documents)
        //        content.Close();


        //    Documents.Clear();
        //}


        #region Add and Remove DockingContainer

        #region Add and Remove DockingContainerToolWindow
        public override void AddDockingContainerToolWindow(VBDockingContainerBase container)
        {
            System.Diagnostics.Debug.Assert(container != null);
            if (container == null)
                return;

            Documents.Add(container);
            AddDockingContainer(container);

            base.AddDockingContainerToolWindow(container);
        }


        public override void RemoveDockingContainerToolWindow(VBDockingContainerBase container)
        {
            System.Diagnostics.Debug.Assert(container != null);
            if (container == null)
                return;

            RemoveDockingContainer(container);
            Documents.Remove(container);
            DockManager.RemoveDockingContainerToolWindow(container);

            base.RemoveDockingContainerToolWindow(container);
        }
        #endregion


        #region Add and Remove DockingContainerTabbedDoc
        public void AddDockingContainerTabbedDoc(VBDockingContainerTabbedDoc container)
        {
            System.Diagnostics.Debug.Assert(container != null);
            if (container == null)
                return;

            Documents.Add(container);
            AddDockingContainer(container);
        }


        public void RemoveDockingContainerTabbedDoc(VBDockingContainerTabbedDoc container)
        {
            System.Diagnostics.Debug.Assert(container != null);
            if (container == null)
                return;

            RemoveDockingContainer(container);
            Documents.Remove(container);
            container.Close();
        }
        #endregion


        #region Add and Remove DockingContainer (private)
        protected virtual void AddDockingContainer(VBDockingContainerBase container)
        {
            VBTabItem vbTabItem = new VBTabItem();
            InitDesignContentOfTabItem(container, vbTabItem, true);
        }

        void container_VBDesignLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is VBDockingContainerBase)
            {
                VBTabItem tabItem = tbcDocuments.Items[Documents.IndexOf(sender as VBDockingContainerBase)] as VBTabItem;
                tabItem.Header = (sender as VBDockingContainerBase).Title;
            }
        }

        protected virtual void RemoveDockingContainer(VBDockingContainerBase container)
        {
            VBTabItem vbTabItem = tbcDocuments.Items[Documents.IndexOf(container)] as VBTabItem;
            container.VBDesignLoaded -= container_VBDesignLoaded;
            container.OnRemovedPanelTabbedDoc(vbTabItem, this);
            vbTabItem.Loaded -= OnTabItemLoaded;
            vbTabItem.Unloaded -= OnTabItemUnLoaded;
            UnsubscribeTabItemBorderEvents(vbTabItem);

            vbTabItem.Content = null;

            tbcDocuments.Items.Remove(vbTabItem);

            if (tbcDocuments.Items.Count == 1)
                ((VBTabItem)tbcDocuments.Items[0]).ShowCaption = false;

            /*if (tbcDocuments.Items.Count == 0)
                tbcDocuments.Visibility = Visibility.Collapsed;*/
        }
        #endregion

        #endregion

        public override void Show(VBDockingContainerBase container)
        {
            if (!ContainerToolWindowsList.Contains(container))
                AddDockingContainerToolWindow(container);
            else
            {
                VBTabItem vbTabItem = tbcDocuments.Items[Documents.IndexOf(container)] as VBTabItem;
                if (vbTabItem != null)
                    InitDesignContentOfTabItem(container, vbTabItem, false);
            }

            base.Show(container);
        }

        public void InitDesignContentOfTabItem(VBDockingContainerBase container, VBTabItem vbTabItem, bool isNewTabItem)
        {
            if (container == null || vbTabItem == null)
                return;
            VBDesign designContent = container.VBDesignContent as VBDesign;
            if (designContent != null)
            {
                if (vbTabItem.ShowCaption != designContent.ShowCaption)
                    vbTabItem.ShowCaption = designContent.ShowCaption;
                if (vbTabItem.VBContent != designContent.VBContent)
                    vbTabItem.VBContent = designContent.VBContent;
                if (vbTabItem.FocusNames != designContent.FocusNames)
                    vbTabItem.FocusNames = designContent.FocusNames;
            }

            if (container is VBDockingContainerToolWindow)
                vbTabItem.IsDragable = true;
            if (container.VBDesignContent != null)
            {
                vbTabItem.TabVisibilityACUrl = VBDockingManager.GetTabVisibilityACUrl(container.VBDesignContent);
                if (VBDockingManager.GetIsCloseableBSORoot(container.VBDesignContent))
                    vbTabItem.WithVisibleCloseButton = true;
                if (VBDockingManager.GetRibbonBarVisibility(container.VBDesignContent) != datamodel.Global.ControlModes.Hidden)
                    vbTabItem.ShowRibbonBar = true;
                if (vbTabItem.WithVisibleCloseButton == true)
                    VBDockingManager.SetCloseButtonVisibility(container.VBDesignContent, Global.ControlModes.Enabled);
            }

            if (isNewTabItem)
            {
                vbTabItem.Loaded += new RoutedEventHandler(OnTabItemLoaded);
                vbTabItem.Unloaded += new RoutedEventHandler(OnTabItemUnLoaded);
                if (   container.OnAddedToPanelTabbedDoc(vbTabItem, this)
                    || vbTabItem.Content == null)
                {
                    DockPanel tabPanel = new DockPanel();
                    vbTabItem.Content = new ContentPresenter();
                    (vbTabItem.Content as ContentPresenter).Content = container.Content;
                }
            }
            else if (vbTabItem.Content is ContentPresenter)
            {
                if ((vbTabItem.Content as ContentPresenter).Content != container.Content)
                    (vbTabItem.Content as ContentPresenter).Content = container.Content;
            }

            string title = null;
            if (vbTabItem.Header != null && vbTabItem.Header is String)
                title = (vbTabItem.Header as String);
            if (title != container.Title)
                vbTabItem.Header = container.Title;
            if (isNewTabItem)
            {
                container.VBDesignLoaded += new RoutedEventHandler(container_VBDesignLoaded);
                tbcDocuments.Items.Add(vbTabItem);
                if ((container.VBDesignContent != null) && VBDockingManager.GetIsCloseableBSORoot(container.VBDesignContent))
                    tbcDocuments.SelectedItem = vbTabItem;

                if (tbcDocuments.Items.Count == 1)
                    tbcDocuments.Visibility = Visibility.Visible;

                if (tbcDocuments.Items.Count > 1)
                    ((VBTabItem)tbcDocuments.Items[0]).ShowCaption = true;
            }
            if (container.DockManager != null && container.DockManager.TabItemMinHeight > 0.1)
                vbTabItem.MinHeight = container.DockManager.TabItemMinHeight;
        }


        public override void RefreshTitle()
        {
            if (ActiveContent == null || Documents.Count <= 0)
            {
                base.RefreshTitle();
                return;
            }
            VBTabItem tabItem = tbcDocuments.Items[Documents.IndexOf(ActiveContent)] as VBTabItem;
            tabItem.Header = ActiveContent.Title;

            base.RefreshTitle();
        }

        public VBTabControl TabControl
        {
            get
            {
                return tbcDocuments;
            }
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if (this.ActiveContent != null && this.ActiveContent.ContextACObject != null)
            {
                this.Root().RootPageWPF.CurrentACComponent = this.ActiveContent.ContextACObject as IACComponent;
            }
            base.OnPreviewMouseDown(e);
        }

        void tbcDocuments_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ActiveContent != null)
            {
                if (this.ActiveContent.ContextACObject != null)
                {
                    this.Root().RootPageWPF.CurrentACComponent = this.ActiveContent.ContextACObject as IACComponent;
                }
            }
            else
            {
                this.Root().RootPageWPF.CurrentACComponent = null;
            }
        }

        void OnTabItemLoaded(object sender, RoutedEventArgs e)
        {
            VBTabItem item = sender as VBTabItem;
            if (item.PART_TabItemBorder == null)
                item.ApplyTemplate();
            if (item.PART_TabItemBorder != null && !item._TabItemBorderEventsSubscr)
            {
                item.PART_TabItemBorder.PreviewMouseDown += new MouseButtonEventHandler(OnTabItemMouseDown);
                item.PART_TabItemBorder.MouseMove += new MouseEventHandler(OnTabItemMouseMove);
                item.PART_TabItemBorder.MouseUp += new MouseButtonEventHandler(OnTabItemMouseUp);
                item._TabItemBorderEventsSubscr = true;
            }
        }

        void OnTabItemUnLoaded(object sender, RoutedEventArgs e)
        {
            //VBTabItem item = sender as VBTabItem;
            //UnsubscribeTabItemBorderEvents(item);
        }

        void UnsubscribeTabItemBorderEvents(VBTabItem item)
        {
            if (item.PART_TabItemBorder != null && item._TabItemBorderEventsSubscr)
            {
                item.PART_TabItemBorder.PreviewMouseDown -= OnTabItemMouseDown;
                item.PART_TabItemBorder.MouseMove -= OnTabItemMouseMove;
                item.PART_TabItemBorder.MouseUp -= OnTabItemMouseUp;
                item._TabItemBorderEventsSubscr = false;
            }
        }

        #region Drag inner contents
        Point ptStartDrag;
        
        void OnTabItemMouseDown(object sender, MouseButtonEventArgs e)
        {
            VBDockingContainerBase clickedContent = this.ActiveContent;
            FrameworkElement senderElement = (sender as FrameworkElement);
            VBTabItem item = senderElement.TemplatedParent as VBTabItem;
            if (!item.IsSelected)
            {
                int index = TabControl.Items.IndexOf(item);
                if (index >= 0)
                    clickedContent = Documents[index];
            }

            // Falls Schliessen-Button oder Ribbon-Button im Tab gedr�ckt, leite Info an entdprechendes Objekt weiter
            if ((clickedContent != null) && (e.Source is Button))
            {
                if (clickedContent is VBDockingContainerToolWindowVB)
                {
                    (clickedContent as VBDockingContainerToolWindowVB).OnTabItemMouseDown(sender, e);
                    return;
                }
                else if (clickedContent is VBDockingContainerTabbedDoc)
                {
                    (clickedContent as VBDockingContainerTabbedDoc).OnTabItemMouseDown(sender, e);
                    return;
                }
            }
            if (!senderElement.IsMouseCaptured)
            {
                ptStartDrag = e.GetPosition(this);
                senderElement.CaptureMouse();
            }
        }

        void OnTabItemMouseMove(object sender, MouseEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            VBTabItem item = senderElement.TemplatedParent as VBTabItem;
            if (senderElement.IsMouseCaptured && Math.Abs(ptStartDrag.X - e.GetPosition(this).X) > 4)
            {
                senderElement.ReleaseMouseCapture();
                int index = tbcDocuments.Items.IndexOf(item);
                VBDockingContainerToolWindow contentToDrag = null;
                //foreach (DockableContent content in Contents)
                //    if (content.Content == (item.Content as ContentPresenter).Content)
                //        contentToDrag = content;

                contentToDrag = Documents[index] as VBDockingContainerToolWindow;
                
                if (contentToDrag != null && !ControlManager.TouchScreenMode)
                    DragContent(contentToDrag, e.GetPosition(DockManager), e.GetPosition(item));
                
                e.Handled = true;
            }
        }
        void OnTabItemMouseUp(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            VBTabItem item = senderElement.TemplatedParent as VBTabItem;
            senderElement.ReleaseMouseCapture();

        }
        #endregion

        #region TabControl commands (switch menu / close current content)
        void OnDocumentSwitch(object sender, EventArgs e)
        {
            int index = cxDocumentSwitchMenu.Items.IndexOf((sender as MenuItem));

            VBTabItem tabItem = tbcDocuments.Items[index] as VBTabItem;
            tbcDocuments.Items.RemoveAt(index);
            tbcDocuments.Items.Insert(0, tabItem);
            tbcDocuments.SelectedIndex = 0;

            VBDockingContainerBase contentToSwap = Documents[index];
            Documents.RemoveAt(index);
            Documents.Insert(0, contentToSwap);

            foreach(MenuItem item in cxDocumentSwitchMenu.Items)
                item.Click -= new RoutedEventHandler(OnDocumentSwitch);
        }

        ContextMenu cxDocumentSwitchMenu;

        void OnBtnDocumentsMenu(object sender, MouseButtonEventArgs e)
        {
            if (Documents.Count <= 1)
                return;

            cxDocumentSwitchMenu = new ContextMenu();

            foreach (VBDockingContainerBase content in Documents)
            {
                MenuItem item = new MenuItem();
                Image imgIncon = new Image();
                imgIncon.Source = content.Icon;
                item.Icon = imgIncon;
                item.Header = content.Title;
                item.Click += new RoutedEventHandler(OnDocumentSwitch);
                cxDocumentSwitchMenu.Items.Add(item);
            }

            cxDocumentSwitchMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.AbsolutePoint;
            cxDocumentSwitchMenu.PlacementRectangle = new Rect(PointToScreen(e.GetPosition(this)), new Size(0, 0));
            cxDocumentSwitchMenu.PlacementTarget = this;
            cxDocumentSwitchMenu.IsOpen = true;
        }

        void OnBtnDocumentClose(object sender, MouseButtonEventArgs e)
        {
            if (ActiveContent == null)
                return;

            if (ActiveContent is VBDockingContainerToolWindow)
                RemoveDockingContainerToolWindow(ActiveContent as VBDockingContainerToolWindow);
            else 
                RemoveDockingContainerTabbedDoc(ActiveDocument);
        }

        //void OnShowDocumentsMenu(object sender, DependencyPropertyChangedEventArgs e)
        //{ 
            
        //}
        #endregion
    }
}