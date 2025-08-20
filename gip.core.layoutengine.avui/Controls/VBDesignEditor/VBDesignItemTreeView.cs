using System;
using System.Collections.Generic;
using System.Linq;
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
using gip.core.layoutengine.avui.Helperclasses;
using gip.ext.designer.avui.PropertyGrid;
using gip.ext.designer.avui;
using gip.ext.designer.avui.OutlineView;
using gip.ext.design.avui;
using gip.ext.designer.avui.Extensions;
using gip.ext.design.avui.Extensions;
using gip.ext.designer.avui.Controls;

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
        #region c'tors
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "DesignItemTreeViewStyleGip", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBDesignEditor/Themes/DesignItemTreeViewStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "DesignItemTreeViewStyleAero", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBDesignEditor/Themes/DesignItemTreeViewStyleAero.xaml" },
        };

        /// <summary>
        /// Gets the list of custom styles.
        /// </summary>
        public static List<CustomControlStyleInfo> StyleInfoList
        {
            get
            {
                return _styleInfoList;
            }
        }

        /// <summary>
        /// Gets the list of custom styles.
        /// </summary>
        public virtual List<CustomControlStyleInfo> MyStyleInfoList
        {
            get
            {
                return _styleInfoList;
            }
        }

        static VBDesignItemTreeView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBDesignItemTreeView), new FrameworkPropertyMetadata(typeof(VBDesignItemTreeView)));
        }

        bool _themeApplied = false;

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ActualizeTheme(true);
        }

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (!_themeApplied)
                ActualizeTheme(false);
        }

        /// <summary>
        /// Actualizes current theme.
        /// </summary>
        /// <param name="bInitializingCall">Determines is initializing call or not.</param>
        public void ActualizeTheme(bool bInitializingCall)
        {
            _themeApplied = ControlManager.RegisterImplicitStyle(this, MyStyleInfoList, bInitializingCall);
        }
        #endregion

        /// <summary>
        /// Represents the dependency property for BSOACComponent.
        /// </summary>
        public static readonly DependencyProperty BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner(typeof(VBDesignItemTreeView), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
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

        public override void MainHeaderClick(object sender, RoutedEventArgs e)
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
