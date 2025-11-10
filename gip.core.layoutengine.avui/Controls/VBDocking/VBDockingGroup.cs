using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Xml;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// How group panes are splitted.
    /// </summary>
    public enum VBDockSplitOrientation : short
    {
        Horizontal,
        Vertical
    }

    /// <summary>
    /// Group of one or more child groups.
    /// </summary>
    class VBDockingGroup : IVBDockLayoutSerializable
    {
        VBDockingPanelBase _attachedVBDockingPanel;
 
        /// <summary>
        /// Pane directly attached
        /// </summary>
        public VBDockingPanelBase AttachedVBDockingPanel {get{return _attachedVBDockingPanel;}}

        VBDockingGroup _firstChildGroup;

        public VBDockingGroup FirstChildGroup
        {
            get { return _firstChildGroup; }
            set
            {
                _firstChildGroup = value;
                value._parentGroup = this;
            }
        }

        VBDockingGroup _secondChildGroup;

        public VBDockingGroup SecondChildGroup
        {
            get { return _secondChildGroup; }
            set
            {
                _secondChildGroup = value;
                value._parentGroup = this;
            }
        }


        VBDockingGroup _parentGroup;

        public VBDockingGroup ParentGroup
        {
            get { return _parentGroup; }
            internal set 
            {
                _parentGroup = value;
            }

        }

        Avalonia.Controls.Dock _dock;

        public Avalonia.Controls.Dock Dock
        {
            get { return _dock; }
        }

        /// <summary>
        /// Needed only for deserialization
        /// </summary>
        public VBDockingGroup()
        { }

        /// <summary>
        /// Create a group with a single pane
        /// </summary>
        /// <param name="panel">Attached panel</param>
        public VBDockingGroup(VBDockingPanelBase panel)
        {
            _attachedVBDockingPanel = panel;
        }

        /// <summary>
        /// Create a group with no panes
        /// </summary>
        public VBDockingGroup(VBDockingGroup firstGroup, VBDockingGroup secondGroup, Avalonia.Controls.Dock groupDock)
        {
            FirstChildGroup = firstGroup;
            SecondChildGroup = secondGroup;
            _dock = groupDock;
        }

        public VBDockingPanelBase GetDockingPanelFromContainer(VBDockingContainerBase container)
        {
            if (AttachedVBDockingPanel != null && AttachedVBDockingPanel.ContainerToolWindowsList.Contains(container))
                return AttachedVBDockingPanel;

            if (FirstChildGroup != null)
            {
                VBDockingPanelBase pane = FirstChildGroup.GetDockingPanelFromContainer(container);
                if (pane != null)
                    return pane;
            }

            if (SecondChildGroup != null)
                return SecondChildGroup.GetDockingPanelFromContainer(container);

            return null;
        }

        bool IsHidden
        {
            get 
            {
                if (AttachedVBDockingPanel != null)
                    return AttachedVBDockingPanel.IsHidden;

                return FirstChildGroup.IsHidden && SecondChildGroup.IsHidden;
            }
        }

        GridLength GroupWidth
        {
            get
            {
                if (AttachedVBDockingPanel != null)
                    return new GridLength(AttachedVBDockingPanel.PaneWidth, GridUnitType.Pixel);
                else
                {
                    if (Dock == Avalonia.Controls.Dock.Left || Dock == Avalonia.Controls.Dock.Right)
                        return new GridLength(FirstChildGroup.GroupWidth.Value+SecondChildGroup.GroupWidth.Value+4, GridUnitType.Pixel);
                    else
                        return FirstChildGroup.GroupWidth;
                }
            }
        }

        GridLength GroupHeight
        {
            get
            {
                if (AttachedVBDockingPanel != null)
                    return new GridLength(AttachedVBDockingPanel.PaneHeight, GridUnitType.Pixel);
                else
                {
                    if (Dock == Avalonia.Controls.Dock.Top || Dock == Avalonia.Controls.Dock.Bottom)
                        return new GridLength(FirstChildGroup.GroupHeight.Value + SecondChildGroup.GroupHeight.Value + 4, GridUnitType.Pixel);
                    else
                        return FirstChildGroup.GroupHeight;
                }
            }
        }

        public void Arrange(Grid grid)
        {
            if (AttachedVBDockingPanel != null)//AttachedPane.IsHidden)
            {
                /*grid.RowDefinitions.Add(new RowDefinition());
                grid.RowDefinitions.Add(new RowDefinition());
                grid.RowDefinitions[0].Height = new GridLength(1,GridUnitType.Star);
                grid.RowDefinitions[1].Height = new GridLength(1);

                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.ColumnDefinitions[0].Width = new GridLength(1,GridUnitType.Star);
                grid.ColumnDefinitions[1].Width = new GridLength(1);*/

                grid.Children.Add(AttachedVBDockingPanel);
            }
            else if (FirstChildGroup.IsHidden && !SecondChildGroup.IsHidden)
                SecondChildGroup.Arrange(grid);
            else if (!FirstChildGroup.IsHidden && SecondChildGroup.IsHidden)
                FirstChildGroup.Arrange(grid);
            else
            {
                if (Dock == Avalonia.Controls.Dock.Left || Dock == Avalonia.Controls.Dock.Right)
                {
                    grid.RowDefinitions.Add(new RowDefinition());
                    grid.ColumnDefinitions.Add(new ColumnDefinition());
                    grid.ColumnDefinitions.Add(new ColumnDefinition());
                    //grid.ColumnDefinitions[0].Width = (Dock == Dock.Left) ? new GridLength(AttachedPane.PaneWidth) : new GridLength(1, GridUnitType.Star);
                    //grid.ColumnDefinitions[1].Width = (Dock == Dock.Right) ? new GridLength(AttachedPane.PaneWidth) : new GridLength(1, GridUnitType.Star);
                    grid.ColumnDefinitions[0].Width = (Dock == Avalonia.Controls.Dock.Left) ? FirstChildGroup.GroupWidth : new GridLength(1, GridUnitType.Star);
                    grid.ColumnDefinitions[1].Width = (Dock == Avalonia.Controls.Dock.Right) ? SecondChildGroup.GroupWidth : new GridLength(1, GridUnitType.Star);

                    //grid.ColumnDefinitions[0].MinWidth = 50;
                    //grid.ColumnDefinitions[1].MinWidth = 50;


                    Grid firstChildGrid = new Grid();
                    firstChildGrid.SetValue(Grid.ColumnProperty, 0);
                    firstChildGrid.Margin = new Thickness(0, 0, 4, 0);
                    FirstChildGroup.Arrange(firstChildGrid);
                    grid.Children.Add(firstChildGrid);

                    Grid secondChildGrid = new Grid();
                    secondChildGrid.SetValue(Grid.ColumnProperty, 1);
                    //secondChildGrid.Margin = (Dock == Dock.Right) ? new Thickness(0, 0, 4, 0) : new Thickness();
                    SecondChildGroup.Arrange(secondChildGrid);
                    grid.Children.Add(secondChildGrid);

                    //AttachedPane.SetValue(Grid.ColumnProperty, (Dock == Dock.Right) ? 1 : 0);
                    //AttachedPane.Margin = (Dock == Dock.Left) ? new Thickness(0, 0, 4, 0) : new Thickness();
                    //grid.Children.Add(AttachedPane);

                    GridSplitter splitter = new GridSplitter();
                    splitter.Width = 4;
                    splitter.HorizontalAlignment = HorizontalAlignment.Right;
                    splitter.VerticalAlignment = VerticalAlignment.Stretch;
                    splitter.BorderBrush = new SolidColorBrush(Colors.Transparent);
                    splitter.Background = new SolidColorBrush(Colors.Transparent);
                    grid.Children.Add(splitter);
                }
                else // if (Dock == Dock.Top || Dock == Dock.Bottom)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition());
                    grid.RowDefinitions.Add(new RowDefinition());
                    grid.RowDefinitions.Add(new RowDefinition());
                    //grid.RowDefinitions[0].Height = (Dock == Dock.Top) ? new GridLength(AttachedPane.PaneHeight) : new GridLength(1, GridUnitType.Star);
                    //grid.RowDefinitions[1].Height = (Dock == Dock.Bottom) ? new GridLength(AttachedPane.PaneHeight) : new GridLength(1, GridUnitType.Star);
                    grid.RowDefinitions[0].Height = (Dock == Avalonia.Controls.Dock.Top) ? FirstChildGroup.GroupHeight : new GridLength(1, GridUnitType.Star);
                    grid.RowDefinitions[1].Height = (Dock == Avalonia.Controls.Dock.Bottom) ? SecondChildGroup.GroupHeight : new GridLength(1, GridUnitType.Star);

                    grid.RowDefinitions[0].MinHeight = 50;
                    grid.RowDefinitions[1].MinHeight = 50;

                    Grid firstChildGrid = new Grid();
                    //firstChildGrid.SetValue(Grid.RowProperty, (Dock == Dock.Bottom) ? 1 : 0);
                    firstChildGrid.SetValue(Grid.RowProperty, 0);
                    //firstChildGrid.Margin = (Dock == Dock.Bottom) ? new Thickness(0, 0, 0, 4) : new Thickness();
                    firstChildGrid.Margin = new Thickness(0, 0, 0, 4);
                    FirstChildGroup.Arrange(firstChildGrid);
                    grid.Children.Add(firstChildGrid);

                    Grid secondChildGrid = new Grid();
                    //secondChildGrid.SetValue(Grid.RowProperty, (Dock == Dock.Top) ? 1 : 0);
                    secondChildGrid.SetValue(Grid.RowProperty, 1);
                    //secondChildGrid.Margin = (Dock == Dock.Bottom) ? new Thickness(0, 0, 0, 4) : new Thickness();
                    SecondChildGroup.Arrange(secondChildGrid);
                    grid.Children.Add(secondChildGrid);

                    //AttachedPane.SetValue(Grid.RowProperty, (Dock == Dock.Bottom) ? 1 : 0);
                    //AttachedPane.Margin = (Dock == Dock.Top) ? new Thickness(0, 0, 0, 4) : new Thickness();
                    //grid.Children.Add(AttachedPane);

                    GridSplitter splitter = new GridSplitter();
                    splitter.Height = 4;
                    splitter.HorizontalAlignment = HorizontalAlignment.Stretch;
                    splitter.VerticalAlignment = VerticalAlignment.Bottom;
                    splitter.BorderBrush = new SolidColorBrush(Colors.Transparent);
                    splitter.Background = new SolidColorBrush(Colors.Transparent);
                    grid.Children.Add(splitter);
                }
            }
        }

        public void ReplaceChildGroup(VBDockingGroup groupToFind, VBDockingGroup groupToReplace)
        {
            if (FirstChildGroup == groupToFind)
                FirstChildGroup = groupToReplace;
            else if (SecondChildGroup == groupToFind)
                SecondChildGroup = groupToReplace;
            else
            {
                System.Diagnostics.Debug.Assert(false);
            }
        }


        public VBDockingGroup AddVBDockingPanelToolWindow(VBDockingPanelToolWindow panel)
        {
            switch (panel.Dock)
            {
                case Avalonia.Controls.Dock.Right:
                case Avalonia.Controls.Dock.Bottom:
                    return new VBDockingGroup(this, new VBDockingGroup(panel), panel.Dock);

                case Avalonia.Controls.Dock.Left:
                case Avalonia.Controls.Dock.Top:
                    return new VBDockingGroup(new VBDockingGroup(panel), this, panel.Dock);
            }

            return null;
        }

  
        public VBDockingGroup RemoveVBDockingPanelToolWindow(VBDockingPanelToolWindow panel)
        {
            if (AttachedVBDockingPanel != null)
                return null;

            if (FirstChildGroup.AttachedVBDockingPanel == panel)
            {
                return SecondChildGroup;
            }
            else if (SecondChildGroup.AttachedVBDockingPanel == panel)
            {
                return FirstChildGroup;
            }
            else
            {
                VBDockingGroup group = FirstChildGroup.RemoveVBDockingPanelToolWindow(panel);

                if (group != null)
                {
                    FirstChildGroup = group;
                    group._parentGroup = this;
                    return null;
                }


                group = SecondChildGroup.RemoveVBDockingPanelToolWindow(panel);

                if (group != null)
                {
                    SecondChildGroup = group;
                    group._parentGroup = this;
                    return null;
                }
            }

            return null;
        }


        public VBDockingGroup GetVBDockingGroup(VBDockingPanelBase pane)
        {
            if (AttachedVBDockingPanel == pane)
                return this;
            
            if (FirstChildGroup != null)
            {
                VBDockingGroup paneGroup = FirstChildGroup.GetVBDockingGroup(pane);
                if (paneGroup!=null)
                    return paneGroup;
            }
            if (SecondChildGroup != null)
            {
                VBDockingGroup paneGroup = SecondChildGroup.GetVBDockingGroup(pane);
                if (paneGroup!=null)
                    return paneGroup;
            }

            return null;
        }


        #region IVBDockLayoutSerializable Members
        public void PersistStateToVBDesignContent()
        {
            if (AttachedVBDockingPanel != null)
            {
                AttachedVBDockingPanel.PersistStateToVBDesignContent();
            }
            else
            {
                FirstChildGroup.PersistStateToVBDesignContent();
                SecondChildGroup.PersistStateToVBDesignContent();
            }
        }

        public void Serialize(XmlDocument doc, XmlNode parentNode)
        {
            parentNode.Attributes.Append(doc.CreateAttribute("Dock"));
            parentNode.Attributes["Dock"].Value = _dock.ToString();

            if (AttachedVBDockingPanel != null)
            {
                XmlNode nodeAttachedPane = null;

                if (AttachedVBDockingPanel is VBDockingPanelToolWindow)
                    nodeAttachedPane = doc.CreateElement("VBDockingPanelToolWindow");
                else if (AttachedVBDockingPanel is VBDockingPanelTabbedDoc)
                    nodeAttachedPane = doc.CreateElement("VBDockingPanelTabbedDoc");

                AttachedVBDockingPanel.Serialize(doc, nodeAttachedPane);

                parentNode.AppendChild(nodeAttachedPane);
            }
            else
            {
                XmlNode nodeChildGroups = doc.CreateElement("ChildGroups");

                XmlNode nodeFirstChildGroup = doc.CreateElement("FirstChildGroup");
                FirstChildGroup.Serialize(doc, nodeFirstChildGroup);
                nodeChildGroups.AppendChild(nodeFirstChildGroup);

                XmlNode nodeSecondChildGroup = doc.CreateElement("SecondChildGroup");
                SecondChildGroup.Serialize(doc, nodeSecondChildGroup);
                nodeChildGroups.AppendChild(nodeSecondChildGroup);

                parentNode.AppendChild(nodeChildGroups);
            }
        }

        public void Deserialize(VBDockingManagerOldWPF managerToAttach, System.Xml.XmlNode node, GetContentFromTypeString getObjectHandler)
        {
            _dock = (Avalonia.Controls.Dock)Enum.Parse(typeof(Avalonia.Controls.Dock), node.Attributes["Dock"].Value);

            if (node.ChildNodes[0].Name == "VBDockingPanelToolWindow")
            {
                VBDockingPanelToolWindow pane = new VBDockingPanelToolWindow(managerToAttach);
                pane.Deserialize(managerToAttach, node.ChildNodes[0], getObjectHandler);
                _attachedVBDockingPanel = pane;
            }
            else if (node.ChildNodes[0].Name == "VBDockingPanelTabbedDoc")
            {
                VBDockingPanelTabbedDoc pane = managerToAttach.vbDockingPanelTabbedDoc;
                pane.Deserialize(managerToAttach, node.ChildNodes[0], getObjectHandler);
                _attachedVBDockingPanel = pane;
            }
            else
            {
                _firstChildGroup = new VBDockingGroup();
                _firstChildGroup._parentGroup = this;
                _firstChildGroup.Deserialize(managerToAttach, node.ChildNodes[0].ChildNodes[0], getObjectHandler);

                _secondChildGroup = new VBDockingGroup();
                _secondChildGroup._parentGroup = this;
                _secondChildGroup.Deserialize(managerToAttach, node.ChildNodes[0].ChildNodes[1], getObjectHandler);
            }
        }

        #endregion
    }
}
