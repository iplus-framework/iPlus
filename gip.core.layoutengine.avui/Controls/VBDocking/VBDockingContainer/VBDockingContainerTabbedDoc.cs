using System;
using System.Collections.Generic;
using System.Text;
using gip.core.datamodel;
using Avalonia.Input;
using Avalonia.Controls;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represent a container for tabbed documents. Use with <see cref="VBDockingManager"/>.
    /// </summary>
    public class VBDockingContainerTabbedDoc : VBDockingContainerBase
    {
        #region c'tors
        public VBDockingContainerTabbedDoc()
        {
        }

        public VBDockingContainerTabbedDoc(VBDockingManager manager)
            : base(manager)
        {
        }

        public VBDockingContainerTabbedDoc(VBDockingManager manager, Control vbDesignContent)
            : base(manager, vbDesignContent)
        {
        }

        internal override void DeInitVBControl(IACComponent bso = null)
        {
            base.DeInitVBControl(bso);
        }

        #endregion

        #region Methods Docking-Framework
        public virtual new void Show()
        {
            DockManager.AddDockingContainerTabbedDoc_GetDockingPanel(this);
        }

        public virtual void CloseTab()
        {
            if (DockManager != null)
                if (DockManager.RemoveDockingContainerTabbedDoc(this))
                {
                    //ReleaseACObjectRef();
                    if (DockManager != null)
                        DockManager.CloseAndRemoveVBDesign(this.VBDesignContent);
                    DeInitVBControl();
                }
        }
        #endregion

        #region methods
        bool _SelectionChangedSubscribed = false;
        public override bool OnAddedToPanelTabbedDoc(VBTabItem vbTabItem, VBDockingPanelBase panel)
        {
            if (ContextACObject == null || VBDesignContent == null)
                return false; // Neues Tab nicht einem Parent-DockPanel unter VBDockingPanelTabbedDoc.AddDockingContainer() zuordnen!!

            VBDockPanel dockPanel = new VBDockPanel();
            dockPanel.DataContext = ContextACObject;

            GenerateContentLayout(dockPanel);
            vbTabItem.Content = dockPanel;
            vbTabItem.TranslationOff = true;
            vbTabItem.ACCaption = ACCaption;

            if (panel is VBDockingPanelTabbedDoc)
            {
                (panel as VBDockingPanelTabbedDoc).TabControl.SelectionChanged += tabControl_SelectionChanged;
                _SelectionChangedSubscribed = true;
            }
            return false; // Neues Tab nicht einem Parent-DockPanel unter VBDockingPanelTabbedDoc.AddDockingContainer() zuordnen!!
        }

        public override void OnRemovedPanelTabbedDoc(VBTabItem vbTabItem, VBDockingPanelBase panel)
        {
            if (panel is VBDockingPanelTabbedDoc && _SelectionChangedSubscribed)
            {
                (panel as VBDockingPanelTabbedDoc).TabControl.SelectionChanged -= tabControl_SelectionChanged;
                _SelectionChangedSubscribed = false;
            }
        }


        public void OnTabItemMouseDown(object sender, PointerPressedEventArgs e)
        {
            if ((_VBRibbon != null) && (e.Source is Button))
            {
                Button button = (Button)e.Source;
                if (button.Name == "PART_RibbonSwitchButton")
                {
                    //string xaml = XamlWriter.Save(DockManager);
                    if (!_VBRibbon.IsVisible)
                    {
                        _VBRibbon.IsVisible = true;
                        // Call SetRibbonBarVisibility for persistance of user-Design
                        if (VBDesignContent != null)
                            VBDockingManager.SetRibbonBarVisibility(VBDesignContent, Global.ControlModes.Enabled);
                    }
                    else
                    {
                        _VBRibbon.IsVisible = false;
                        // Call SetRibbonBarVisibility for persistance of user-Design
                        if (VBDesignContent != null)
                            VBDockingManager.SetRibbonBarVisibility(VBDesignContent, Global.ControlModes.Collapsed);
                    }
                }
                else if ((button.Name == "PART_CloseButton") && (VBDesignContent != null))
                {
                    if (VBDockingManager.GetIsCloseableBSORoot(VBDesignContent))
                    {
                        CloseTab();
                    }
                }
            }
        }

        void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VBTabItem vbTabitemAdded = null;
            foreach (var added in e.AddedItems)
            {
                if (added is VBTabItem)
                {
                    vbTabitemAdded = added as VBTabItem;
                    break;
                }
            }

            if (DockManager != null && DockManager.ContextACObject != null)
            {
                if (vbTabitemAdded != null)
                {
                    foreach (var removed in e.RemovedItems)
                    {
                        if (removed is VBTabItem)
                        {
                            VBTabItem vbTabitemRemoved = removed as VBTabItem;
                            (DockManager.ContextACObject as IACComponent).ACAction(new ACActionArgs(vbTabitemRemoved, 0, 0, Global.ElementActionType.TabItemDeActivated));
                            break;
                        }
                    }
                    IACInteractiveObject interactionObject = DockManager.ContextACObject as IACInteractiveObject;
                    if (interactionObject != null)
                        interactionObject.ACAction(new ACActionArgs(vbTabitemAdded, 0, 0, Global.ElementActionType.TabItemActivated));
                }
            }
        }
        #endregion

    }
}
