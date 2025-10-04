using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using gip.ext.designer.avui.PropertyGrid;
using gip.ext.designer.avui;
using gip.ext.designer.avui.OutlineView;
using gip.ext.design.avui;
using gip.ext.designer.avui.Extensions;
using gip.ext.design.avui.Extensions;
using gip.ext.designer.avui.Controls;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia;

namespace gip.core.layoutengine.avui
{
    //TODO:
    /// <summary>
    /// Represents a design item for treeview.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt ein Design Item für die Baumansicht dar.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBDesignItemTreeView'}de{'VBDesignItemTreeView'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBDesignItemTreeView : DesignItemTreeView
    {
        /// <summary>
        /// Represents the dependency property for BSOACComponent.
        /// </summary>
        public static readonly AttachedProperty<IACBSO> BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner<VBDesignItemTreeView>();
        /// <summary>
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get { return (IACBSO)GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }

        public override OutlineNode CreateOutlineNode()
        {
            return new VBOutlineNode(RootItem);
        }

        protected override ContextMenu BuildContextMenu()
        {
            return new VBContextMenu();
        }
    }


    /// <summary>
    /// Represents a outline node.
    /// </summary>
    public class VBOutlineNode : OutlineNode
    {
        public static new VBOutlineNode Create(DesignItem designItem)
        {
            OutlineNode node;
            if (!outlineNodes.TryGetValue(designItem, out node))
            {
                node = new VBOutlineNode(designItem);
                outlineNodes[designItem] = node;
            }
            return node as VBOutlineNode;
        }


        public VBOutlineNode(DesignItem designItem)
            : base(designItem)
        {
        }

        protected override OutlineNodeBase OnCreateChildrenNode(DesignItem child)
        {
            return VBOutlineNode.Create(child);
        }

        public override QuickOperationMenu CreateMainMenu()
        {
            return new VBQuickOperationMenu() { MainHeader = CreateMenuItem("Operations") };
        }

        public override MenuItem CreateMenuItem(string header)
        {
            return new VBMenuItem() { Header = header };
        }

        public override Separator CreateSeparator()
        {
            return new VBMenuSeparator();
        }

        public override void MainHeader_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            VBQuickOperationMenuExtension.MainHeaderClick(sender, e, this.DesignItem, _menu, DockingManager, WindowTitle);
        }

        protected string WindowTitle
        {
            get
            {
                return VBQuickOperationMenuExtension.GetWindowTitle(this.DesignItem);
            }
        }

        private VBDockingManager _DockingManager;
        protected VBDockingManager DockingManager
        {
            get
            {
                if (_DockingManager != null)
                    return _DockingManager;
                _DockingManager = VBQuickOperationMenuExtension.GetDockingManager(this.DesignItem);
                return _DockingManager;
            }
        }

    }
}
