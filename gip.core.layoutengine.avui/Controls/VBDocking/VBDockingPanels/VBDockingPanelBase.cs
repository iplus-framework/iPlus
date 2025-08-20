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
using System.Xml;
using gip.core.datamodel;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Base class for docking panels.
    /// </summary>
    public abstract class VBDockingPanelBase : UserControl , IVBDockDropSurface, IVBDockLayoutSerializable
    {
        public const double PaneDefaultWidth = 220;
        public double PaneWidth = PaneDefaultWidth;
        public const double PaneDefaultHeight= 150;
        public double PaneHeight = PaneDefaultHeight;

        public readonly List<VBDockingContainerBase> ContainerToolWindowsList = new List<VBDockingContainerBase>();

        #region c´tors

        public VBDockingPanelBase(VBDockingManager dockManager)
            : this(dockManager, null)
        {
        }

        public VBDockingPanelBase(VBDockingManager dockManager, VBDockingContainerToolWindow content)
        {
            _dockManager = dockManager;

            if (content != null)
                AddDockingContainerToolWindow(content);
        }

        internal virtual void DeInitVBControl(IACComponent bso = null)
        {
            ActiveContent = null;
            _dockManager = null;
        }

        #endregion

        VBDockingManager _dockManager;
        public virtual VBDockingManager DockManager
        {
            get { return _dockManager; }
            //set { _dockManager = value; }
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
        }

        public virtual void AddDockingContainerToolWindow(VBDockingContainerBase container)
        {
            if (DockManager != null)
            {
                DockManager.AddDockingContainerToolWindow(container);
            }
            SetDefaultWithFromVBDesign(container);
            container.SetDockingPanel(this);
            ContainerToolWindowsList.Add(container);
        }

        public void SetDefaultWithFromVBDesign(VBDockingContainerBase container)
        {
            if (DockManager != null)
            {
                if (container.VBDesignContent != null)
                {
                    Size lastStoredSize = VBDockingManager.GetWindowSize(container.VBDesignContent);
                    if ((lastStoredSize.Width > 0) && (lastStoredSize.Height > 0) && (Math.Abs(PaneWidth - PaneDefaultWidth) <= Double.Epsilon) && (Math.Abs(PaneHeight - PaneDefaultHeight) <= Double.Epsilon))
                    {
                        PaneWidth = lastStoredSize.Width;
                        PaneHeight = lastStoredSize.Height;
                    }
                }
            }
        }

        public virtual void RemoveDockingContainerToolWindow(VBDockingContainerBase content)
        {
            if (DockManager != null)
                DockManager.RemoveDockingContainerToolWindow(content);
            content.SetDockingPanel(null);
            ContainerToolWindowsList.Remove(content);
        }

        public virtual void Show(VBDockingContainerBase container)
        {
            //System.Diagnostics.Debug.Assert(ContainerToolWindowsList.Contains(content));
        }

        public virtual void Hide(VBDockingContainerBase container)
        { 
        
        }

        public virtual void Show()
        {
            if ((DockManager == null) || (DockManager.DragPanelServices == null))
                return;
            DockManager.DragPanelServices.Register(this);
        }

        public virtual void Hide()
        {
            if ((DockManager == null) || (DockManager.DragPanelServices == null))
                return;
            DockManager.DragPanelServices.Unregister(this);
        }

        public virtual void Close()
        {
            if ((DockManager == null) || (DockManager.DragPanelServices == null))
                return;
            DockManager.DragPanelServices.Unregister(this);
        }

        public virtual void Close(VBDockingContainerBase content)
        {

        }

        public virtual bool CloseWindow()
        {
            return false;
        }
        
        public virtual bool IsHidden
        {
            get { return false; }
        }

        public virtual void SaveSize()
        {

        }


        public virtual VBDockingContainerBase ActiveContent
        {
            get { return null; }
            set { }
        }

        protected virtual void DragContent(VBDockingContainerToolWindow contentToDrag, Point startDragPoint, Point offset)
        {
            if (DockManager == null)
                return;

            RemoveDockingContainerToolWindow(contentToDrag);
            VBDockingPanelToolWindow panel = new VBDockingPanelToolWindow(DockManager);
            panel.AddDockingContainerToolWindow(contentToDrag);
            panel.ChangeState(VBDockingPanelState.DockableWindow);
            //panel.Show();
            DockManager.AddDockingPanelToolWindow(panel);
            VBWindowDockingUndocked wnd = new VBWindowDockingUndocked(panel);
            panel.ChangeState(VBDockingPanelState.DockableWindow);
            DockManager.Drag(wnd, startDragPoint, offset);
        }

        public virtual void RefreshTitle()
        { 
        
        }

        #region IDropSurface 

        public Rect SurfaceRectangle
        {
            get 
            {
                if (IsHidden)
                    return new Rect();
                try
                {
                    return new Rect(PointToScreen(new Point(0, 0)), new Size(ActualWidth, ActualHeight));
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("VBDockingPanelBase", "SurfaceRectangle", msg);

                    return new Rect();
                }
            }
        }

        public virtual void OnDockDragEnter(Point point)
        {
            if ((DockManager == null) || (DockManager.OverlayWindow == null))
                return;
            DockManager.OverlayWindow.ShowOverlayPaneDockingOptions(this);
        }

        public virtual void OnDockDragOver(Point point)
        {
            
        }

        public virtual void OnDockDragLeave(Point point)
        {
            if ((DockManager == null) || (DockManager.OverlayWindow == null))
                return;
            DockManager.OverlayWindow.HideOverlayPaneDockingOptions(this);
        }

        public virtual bool OnDockDrop(Point point)
        {
            return false;
        }

        #endregion

        #region IVBDockLayoutSerializable
        public void PersistStateToVBDesignContent()
        {
            foreach (VBDockingContainerBase content in ContainerToolWindowsList)
            {
                VBDockingContainerToolWindow dockableContent = content as VBDockingContainerToolWindow;
                if (dockableContent != null)
                    dockableContent.PersistStateToVBDesignContent();
            }
        }

        public virtual void Serialize(XmlDocument doc, XmlNode parentNode)
        {
            parentNode.Attributes.Append(doc.CreateAttribute("Width"));
            parentNode.Attributes["Width"].Value = PaneWidth.ToString();
            parentNode.Attributes.Append(doc.CreateAttribute("Height"));
            parentNode.Attributes["Height"].Value = PaneHeight.ToString();

            foreach (VBDockingContainerBase content in ContainerToolWindowsList)
            {
                VBDockingContainerToolWindow dockableContent = content as VBDockingContainerToolWindow;
                if (dockableContent != null)
                {
                    XmlNode nodeDockableContent = doc.CreateElement(dockableContent.GetType().ToString());
                    parentNode.AppendChild(nodeDockableContent);
                }
            }
        }

        public virtual void Deserialize(VBDockingManager managerToAttach, XmlNode node, GetContentFromTypeString getObjectHandler)
        {
            _dockManager = managerToAttach;

            PaneWidth = double.Parse(node.Attributes["Width"].Value);
            PaneHeight = double.Parse(node.Attributes["Height"].Value);

            foreach (XmlNode nodeDockableContent in node.ChildNodes)
            {
                VBDockingContainerToolWindow content = getObjectHandler(nodeDockableContent.Name);
                AddDockingContainerToolWindow(content);
                Show(content);
            }

            DockManager.DragPanelServices.Register(this);
        }

        #endregion

    }
}
