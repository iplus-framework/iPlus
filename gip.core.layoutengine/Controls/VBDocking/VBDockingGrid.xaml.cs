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

namespace gip.core.layoutengine
{
    /// <summary>
    /// Represents a grid for documents docking.
    /// </summary>
    public partial class VBDockingGrid : UserControl, IVBDockLayoutSerializable
    {
        public VBDockingGrid()
        {
            InitializeComponent();

        }

        internal void AttachDockManager(VBDockingManager dockManager)
        {
            _vbDockingPanelTabbedDoc = new VBDockingPanelTabbedDoc(dockManager);
            _rootGroup = new VBDockingGroup(vbDockingPanelTabbedDoc);
            ArrangeLayout();
        }

        internal void DeInitVBControl(IACComponent bso = null)
        {
            Clear(_panel);
            _panel = null;
            _rootGroup = null;
            _vbDockingPanelTabbedDoc = null;
        }
        
        //Creates a root group with a DocumentsPane
        VBDockingGroup _rootGroup;

        VBDockingPanelTabbedDoc _vbDockingPanelTabbedDoc;

        public VBDockingPanelTabbedDoc vbDockingPanelTabbedDoc
        {
            get { return _vbDockingPanelTabbedDoc; }
        }

        internal void AttachDockingPanelToolWindowEvents(VBDockingPanelToolWindow panel)
        {
            panel.OnStateChanged += new EventHandler(PanelToolWindow_OnStateChanged);
            panel.OnDockChanged += new EventHandler(PanelToolWindow_OnDockChanged);
        }

        internal void DetachDockingPanelToolWindowEvents(VBDockingPanelToolWindow panel)
        {
            panel.OnStateChanged -= new EventHandler(PanelToolWindow_OnStateChanged);
            panel.OnDockChanged -= new EventHandler(PanelToolWindow_OnDockChanged);
        }

        void PanelToolWindow_OnDockChanged(object sender, EventArgs e)
        {
            VBDockingPanelToolWindow panel = sender as VBDockingPanelToolWindow;
            Remove(panel);
            Add(panel);
        }

        void PanelToolWindow_OnStateChanged(object sender, EventArgs e)
        {
            VBDockingPanelToolWindow pane = sender as VBDockingPanelToolWindow;

            //if (pane.State == PaneState.FloatingWindow)
            //    Remove(pane);
            //else
                ArrangeLayout();
        }


        public void Add(VBDockingPanelToolWindow panel)
        {
            _rootGroup = _rootGroup.AddVBDockingPanelToolWindow(panel);
            ArrangeLayout();
        }

#if DEBUG
        void Dump(VBDockingGroup group, int indent)
        {
            if (indent == 0)
                Console.WriteLine("Dump()");
            for (int i = 0; i < indent; i++)
                Console.Write("-");
            Console.Write(">");
            if (group.AttachedVBDockingPanel == null)
            {
                Console.WriteLine(group.Dock);
                Dump(group.FirstChildGroup, indent + 4);
                Console.WriteLine();
                Dump(group.SecondChildGroup, indent + 4);
            }
            else if (group.AttachedVBDockingPanel.ActiveContent!=null)
                Console.WriteLine(group.AttachedVBDockingPanel.ActiveContent.Title);
            else
                Console.WriteLine(group.AttachedVBDockingPanel.ToString() + " {null}");
        }
#endif

        public void Add(VBDockingPanelToolWindow panel, VBDockingPanelBase relativePanel, Dock relativeDock)
        {
            //Console.WriteLine("Add(...)");
            AttachDockingPanelToolWindowEvents(panel);
            VBDockingGroup group = GetVBDockingGroup(relativePanel);

            switch (relativeDock)
            {
                case Dock.Right:
                case Dock.Bottom:
                    {
                        if (group == _rootGroup)
                        {
                            _rootGroup = new VBDockingGroup(group, new VBDockingGroup(panel), relativeDock);
                        }
                        else
                        {
                            VBDockingGroup parentGroup = group.ParentGroup;
                            VBDockingGroup newChildGroup = new VBDockingGroup(group, new VBDockingGroup(panel), relativeDock);
                            parentGroup.ReplaceChildGroup(group, newChildGroup);
                        }
                    }
                    break;
                case Dock.Left:
                case Dock.Top:
                    {
                        if (group == _rootGroup)
                        {
                            _rootGroup = new VBDockingGroup(new VBDockingGroup(panel), group, relativeDock);
                        }
                        else
                        {
                            VBDockingGroup parentGroup = group.ParentGroup;
                            VBDockingGroup newChildGroup = new VBDockingGroup(new VBDockingGroup(panel), group, relativeDock);
                            parentGroup.ReplaceChildGroup(group, newChildGroup);
                        }
                    }
                    break;
            }

            ArrangeLayout();
            
        }


        public void Remove(VBDockingPanelToolWindow panel)
        {
            VBDockingGroup groupToAttach = _rootGroup.RemoveVBDockingPanelToolWindow(panel);
            if (groupToAttach != null)
            {
                _rootGroup = groupToAttach;
                _rootGroup.ParentGroup = null;
            }

            ArrangeLayout();
        }


        public void MoveTo(VBDockingPanelToolWindow sourcePanel, VBDockingPanelBase destinationPanel, Dock relativeDock)
        {
            Remove(sourcePanel);
            Add(sourcePanel, destinationPanel, relativeDock);
        }


        public void MoveInto(VBDockingPanelToolWindow sourcePanel, VBDockingPanelBase destinationPanel)
        {
            Remove(sourcePanel);
            while (sourcePanel.ContainerToolWindowsList.Count > 0)
            {
                VBDockingContainerBase content = sourcePanel.ContainerToolWindowsList[0];
                sourcePanel.RemoveDockingContainerToolWindow(content);
                destinationPanel.AddDockingContainerToolWindow(content);
                destinationPanel.Show(content);
            }
            sourcePanel.Close();
        }


        public VBDockingPanelBase GetVBDockingPanelFromContainer(VBDockingContainerBase container)
        {
            return _rootGroup.GetDockingPanelFromContainer(container);
        }


        VBDockingGroup GetVBDockingGroup(VBDockingPanelBase panel)
        {
            return _rootGroup.GetVBDockingGroup(panel);
        }

        internal void ArrangeLayout()
        {
            //_rootGroup.SaveChildPanesSize();
            Clear(_panel);
            _rootGroup.Arrange(_panel);

            /*RowDefinition lastRow = new RowDefinition();
            lastRow.Height = new GridLength(1);
            _panel.RowDefinitions.Add(lastRow);

            ColumnDefinition lastCol = new ColumnDefinition();
            lastCol.Width = new GridLength(1);
            _panel.ColumnDefinitions.Add(lastCol);*/

            //Dump(_rootGroup, 0);
        }

      
        private void Clear(Grid grid)
        {
            foreach (UIElement child in grid.Children)
            {
                if (child is Grid)
                    Clear(child as Grid);
            }

            grid.Children.Clear();
            grid.ColumnDefinitions.Clear();
            grid.RowDefinitions.Clear();
        }


        #region IVBDockLayoutSerializable Members
        public void PersistStateToVBDesignContent()
        {
            _rootGroup.PersistStateToVBDesignContent();
        }

        public void Serialize(XmlDocument doc, XmlNode parentNode)
        {
            XmlNode node_rootGroup = doc.CreateElement("_rootGroup");

            _rootGroup.Serialize(doc, parentNode);

            parentNode.AppendChild(node_rootGroup);
        }

        public void Deserialize(VBDockingManager managerToAttach, XmlNode node, GetContentFromTypeString getObjectHandler)
        {
            _rootGroup = new VBDockingGroup();
            _rootGroup.Deserialize(managerToAttach, node, getObjectHandler);

            //_docsPane = FindDocumentsPane(_rootGroup);
            
            ArrangeLayout();
        }

        VBDockingPanelTabbedDoc FindVBDockingPanelTabbedDocInGroup(VBDockingGroup group)
        {
            if (group == null)
                return null;

            if (group.AttachedVBDockingPanel is VBDockingPanelTabbedDoc)
                return group.AttachedVBDockingPanel as VBDockingPanelTabbedDoc;
            else
            {
                VBDockingPanelTabbedDoc docsPane = FindVBDockingPanelTabbedDocInGroup(group.FirstChildGroup);
                if (docsPane != null)
                    return docsPane;

                docsPane = FindVBDockingPanelTabbedDocInGroup(group.SecondChildGroup);
                if (docsPane != null)
                    return docsPane;
            }

            return null;
        }

        #endregion
    }
}