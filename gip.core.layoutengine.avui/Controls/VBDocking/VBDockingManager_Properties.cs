using Avalonia;
using Avalonia.Controls;
using Avalonia.Metadata;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace gip.core.layoutengine.avui
{
    public partial class VBDockingManager
    {

        #region PART's
        private Border _PART_BorderFreeze;
        public Border PART_BorderFreeze
        {
            get
            {
                return _PART_BorderFreeze;
            }
        }

        private StackPanel _PART_btnPanelLeft;
        public StackPanel PART_btnPanelLeft
        {
            get
            {
                return _PART_btnPanelLeft;
            }
        }

        private StackPanel _PART_btnPanelRight;
        public StackPanel PART_btnPanelRight
        {
            get
            {
                return _PART_btnPanelRight;
            }
        }

        private StackPanel _PART_btnPanelTop;
        public StackPanel PART_btnPanelTop
        {
            get
            {
                return _PART_btnPanelTop;
            }
        }

        private StackPanel _PART_btnPanelBottom;
        public StackPanel PART_btnPanelBottom
        {
            get
            {
                return _PART_btnPanelBottom;
            }
        }

        private VBDockingGrid _PART_gridDocking;
        public VBDockingGrid PART_gridDocking
        {
            get
            {
                return _PART_gridDocking;
            }
        }

        private DockPanel _PART_panelFront;
        public DockPanel PART_panelFront
        {
            get
            {
                return _PART_panelFront;
            }
        }

        private StackPanel _PART_AvInvisibleInitDummy;
        /// <summary>
        /// Invisible Helper-Element for temporary adding new created VBDockingPanelToolWindow into logical tree in order to call Initialize and ApplyTemplate() Method
        /// </summary>
        public StackPanel PART_AvInvisibleInitDummy
        {
            get
            {
                return _PART_AvInvisibleInitDummy;
            }
        }

        #endregion

        #region Deklarative Properties (XAML)
        public static readonly AttachedProperty<Global.VBDesignContainer> ContainerProperty =
                               AvaloniaProperty.RegisterAttached<VBDockingManager, Control, Global.VBDesignContainer>(
                                   "Container", Global.VBDesignContainer.TabItem);

        public static readonly AttachedProperty<Global.VBDesignDockState> DockStateProperty =
                               AvaloniaProperty.RegisterAttached<VBDockingManager, Control, Global.VBDesignDockState>(
                                   "DockState", Global.VBDesignDockState.Tabbed);

        public static readonly AttachedProperty<Global.VBDesignDockPosition> DockPositionProperty =
                               AvaloniaProperty.RegisterAttached<VBDockingManager, Control, Global.VBDesignDockPosition>(
                                   "DockPosition", Global.VBDesignDockPosition.Bottom);

        public static readonly AttachedProperty<Global.ControlModes> RibbonBarVisibilityProperty =
                               AvaloniaProperty.RegisterAttached<VBDockingManager, Control, Global.ControlModes>(
                                   "RibbonBarVisibility", Global.ControlModes.Hidden);

        public static readonly AttachedProperty<bool> IsCloseableBSORootProperty =
                               AvaloniaProperty.RegisterAttached<VBDockingManager, Control, bool>(
                                   "IsCloseableBSORoot", false);

        public static readonly AttachedProperty<bool> DisableDockingOnClickProperty =
                               AvaloniaProperty.RegisterAttached<VBDockingManager, Control, bool>(
                                   "DisableDockingOnClick", false);

        public static readonly AttachedProperty<Size> WindowSizeProperty =
                               AvaloniaProperty.RegisterAttached<VBDockingManager, Control, Size>(
                                   "WindowSize", new Size());

        public static readonly AttachedProperty<String> WindowTitleProperty =
                               AvaloniaProperty.RegisterAttached<VBDockingManager, Control, String>(
                                   "WindowTitle", string.Empty);

        public static readonly AttachedProperty<Global.ControlModes> PART_closeButtonVisibilityProperty =
                               AvaloniaProperty.RegisterAttached<VBDockingManager, Control, Global.ControlModes>(
                                   "PART_closeButtonVisibility", Global.ControlModes.Hidden);

        public static readonly AttachedProperty<Global.ControlModes> CloseButtonVisibilityProperty =
                               AvaloniaProperty.RegisterAttached<VBDockingManager, Control, Global.ControlModes>(
                                   "CloseButtonVisibility", Global.ControlModes.Hidden);

        public static readonly AttachedProperty<string> TabVisibilityACUrlProperty =
                               AvaloniaProperty.RegisterAttached<VBDockingManager, Control, string>(
                                   "TabVisibilityACUrl", string.Empty);

        public static readonly StyledProperty<double> TabItemMinHeightProperty =
                               AvaloniaProperty.Register<VBDockingManager, double>(
                                   nameof(TabItemMinHeight), 0.0);


        public string ACClassUrl
        {
            get;
            set;
        }

        public List<Control> _VBDesignList = new List<Control>();
        [Content]
        public List<Control> VBDesignList
        {
            get
            {
                return _VBDesignList;
            }
            set
            {
                _VBDesignList = value;
            }
        }


        #endregion

        #region AccessMethods Attached Properties
        public static void SetContainer(Control element, Global.VBDesignContainer value)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            element.SetValue(VBDockingManager.ContainerProperty, value);
        }

        public static Global.VBDesignContainer GetContainer(Control element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            return element.GetValue(VBDockingManager.ContainerProperty);
        }

        public static void SetDockState(Control element, Global.VBDesignDockState value)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            element.SetValue(VBDockingManager.DockStateProperty, value);
        }

        public static Global.VBDesignDockState GetDockState(Control element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            return element.GetValue(VBDockingManager.DockStateProperty);
        }

        public static void SetDockPosition(Control element, Global.VBDesignDockPosition value)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            element.SetValue(VBDockingManager.DockPositionProperty, value);
        }

        public static Global.VBDesignDockPosition GetDockPosition(Control element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            return element.GetValue(VBDockingManager.DockPositionProperty);
        }

        public static void SetRibbonBarVisibility(Control element, Global.ControlModes value)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            element.SetValue(VBDockingManager.RibbonBarVisibilityProperty, value);
        }

        public static Global.ControlModes GetRibbonBarVisibility(Control element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            return element.GetValue(VBDockingManager.RibbonBarVisibilityProperty);
        }

        public static void SetIsCloseableBSORoot(Control element, bool value)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            element.SetValue(VBDockingManager.IsCloseableBSORootProperty, value);
        }

        public static bool GetIsCloseableBSORoot(Control element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            return element.GetValue(VBDockingManager.IsCloseableBSORootProperty);
        }

        public static void SetDisableDockingOnClick(Control element, bool value)
        {
            if (element == null)
            {
                return;
                //throw new ArgumentNullException("element");
            }
            element.SetValue(VBDockingManager.DisableDockingOnClickProperty, value);
        }

        public static bool GetDisableDockingOnClick(Control element)
        {
            if (element == null)
            {
                return false;
                //throw new ArgumentNullException("element");
            }
            return element.GetValue(VBDockingManager.DisableDockingOnClickProperty);
        }


        public static void SetWindowSize(Control element, Size value)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            element.SetValue(VBDockingManager.WindowSizeProperty, value);
        }

        public static Size GetWindowSize(Control element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            return element.GetValue(VBDockingManager.WindowSizeProperty);
        }


        public static void SetWindowTitle(Control element, String value)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            element.SetValue(VBDockingManager.WindowTitleProperty, value);
        }

        public static String GetWindowTitle(Control element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            return element.GetValue(VBDockingManager.WindowTitleProperty);
        }

        public static void SetCloseButtonVisibility(Control element, Global.ControlModes value)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            element.SetValue(VBDockingManager.CloseButtonVisibilityProperty, value);
        }

        public static Global.ControlModes GetCloseButtonVisibility(Control element)
        {
            if (element == null)
            {
                return Global.ControlModes.Enabled;
                //throw new ArgumentNullException("element");
            }
            return element.GetValue(VBDockingManager.CloseButtonVisibilityProperty);
        }


        public static void SetTabVisibilityACUrl(Control element, string value)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            element.SetValue(VBDockingManager.TabVisibilityACUrlProperty, value);
        }

        public static string GetTabVisibilityACUrl(Control element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            return element.GetValue(VBDockingManager.TabVisibilityACUrlProperty);
        }

        public double TabItemMinHeight
        {
            set { SetValue(TabItemMinHeightProperty, value); }
            get { return GetValue(TabItemMinHeightProperty); }
        }

        #endregion

        #region IACUrl Member

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public string ACIdentifier
        {
            get { return ACUrlHelper.BuildACNameForGUI(this, Name); }
        }

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

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [Category("VBControl")]
        public string ACCaption
        {
            get
            {
                return null;
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
                return null;
            }
        }
        #endregion

        #region public members

        /// <summary>
        /// ContextACObject is used by WPF-Controls and mostly it equals to the Control.DataContext-Property.
        /// IACInteractiveObject-Childs in the logical WPF-tree resolves relative ACUrl's to this ContextACObject-Property.
        /// </summary>
        /// <value>The Data-Context as IACObject</value>
        public IACObject ContextACObject
        {
            get
            {
                return DataContext as IACObject;
            }
        }

        public static readonly StyledProperty<bool> IsBSOManagerProperty
            = AvaloniaProperty.Register<VBDockingManager, bool>(nameof(IsBSOManager));
        [Category("VBControl")]
        public bool IsBSOManager
        {
            get { return GetValue(IsBSOManagerProperty); }
            set { SetValue(IsBSOManagerProperty, value); }
        }

        /// <summary>
        /// List of managed contents (hiddens too)
        /// </summary>
        List<VBDockingContainerBase> ToolWindowContainerList = new List<VBDockingContainerBase>();

        /// <summary>
        /// Returns a documents list
        /// </summary>
        public VBDockingContainerTabbedDoc[] TabbedDocContainerList
        {
            get
            {
                if (PART_gridDocking == null)
                    return new VBDockingContainerTabbedDoc[0];
                int diff = PART_gridDocking.vbDockingPanelTabbedDoc.Documents.Count - PART_gridDocking.vbDockingPanelTabbedDoc.ContainerToolWindowsList.Count;
                if (diff <= 0)
                    return new VBDockingContainerTabbedDoc[0];
                VBDockingContainerTabbedDoc[] docs = new VBDockingContainerTabbedDoc[diff];
                int i = 0;
                foreach (VBDockingContainerBase content in PART_gridDocking.vbDockingPanelTabbedDoc.Documents)
                {
                    if (content is VBDockingContainerTabbedDoc)
                        docs[i++] = content as VBDockingContainerTabbedDoc;
                }

                return docs;
            }
        }

        /// <summary>
        /// Return active document. Return Selected Item in TabControl
        /// </summary>
        /// <remarks>If no document is present or a dockable content is active in the Documents pane return null</remarks>
        public VBDockingContainerTabbedDoc ActiveDocument
        {
            get
            {
                if (vbDockingPanelTabbedDoc == null)
                    return null;
                return vbDockingPanelTabbedDoc.ActiveDocument;
            }
        }


        #region FocusView

        public static readonly StyledProperty<string> FocusViewProperty = AvaloniaProperty.Register<VBDockingManager, string>(nameof(FocusView));

        [Category("VBControl")]
        public string FocusView
        {
            get { return GetValue(FocusViewProperty); }
            set { SetValue(FocusViewProperty, value); }
        }

        #endregion

        /// <summary>
        /// Returns currently active documents pane (at the moment this is only one per DockManager control)
        /// </summary>
        /// <returns>The DocumentsPane</returns>
        internal VBDockingPanelTabbedDoc vbDockingPanelTabbedDoc
        {
            get
            {
                if (PART_gridDocking == null)
                    return null;
                return PART_gridDocking.vbDockingPanelTabbedDoc;
            }
        }


        private bool _SelectionChangedHandlerAdded = false;
        private void AddSelectionChangedHandler()
        {
            if (_SelectionChangedHandlerAdded)
                return;
            if (vbDockingPanelTabbedDoc_TabControl == null)
                return;
            vbDockingPanelTabbedDoc_TabControl.SelectionChanged += vbDockingPanelTabbedDoc_TabControl_SelectionChanged;
            _SelectionChangedHandlerAdded = true;
        }


        private void RemoveSelectionChangedHandler()
        {
            if (!_SelectionChangedHandlerAdded)
                return;
            if (vbDockingPanelTabbedDoc_TabControl == null)
                return;
            vbDockingPanelTabbedDoc_TabControl.SelectionChanged -= vbDockingPanelTabbedDoc_TabControl_SelectionChanged;
            _SelectionChangedHandlerAdded = false;
        }

        void vbDockingPanelTabbedDoc_TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TabbedDocSelectionChanged != null)
            {
                TabbedDocSelectionChanged(sender, e);
            }
        }

        public event EventHandler<SelectionChangedEventArgs> TabbedDocSelectionChanged;
        public VBTabControl vbDockingPanelTabbedDoc_TabControl
        {
            get
            {
                if (vbDockingPanelTabbedDoc == null)
                    return null;
                return vbDockingPanelTabbedDoc.TabControl;
            }
        }
        #endregion

        #region Persistence
        public static readonly StyledProperty<bool> FreezeActiveProperty = AvaloniaProperty.Register<VBDockingManager, bool>(nameof(FreezeActive));
        public bool FreezeActive
        {
            get { return GetValue(FreezeActiveProperty); }
            set { SetValue(FreezeActiveProperty, value); }
        }

        public static readonly StyledProperty<WPFControlSelectionEventArgs> MasterPageFreezeProperty = AvaloniaProperty.Register<VBDockingManager, WPFControlSelectionEventArgs>(nameof(MasterPageFreeze));

        public WPFControlSelectionEventArgs MasterPageFreeze
        {
            get { return GetValue(MasterPageFreezeProperty); }
            set { SetValue(MasterPageFreezeProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ACUrlCmdMessage.
        /// </summary>
        public static readonly StyledProperty<ACUrlCmdMessage> ACUrlCmdMessageProperty = AvaloniaProperty.Register<VBDockingManager, ACUrlCmdMessage>(nameof(ACUrlCmdMessage));

        /// <summary>
        /// Gets or sets the ACUrlCmdMessage.
        /// </summary>
        public ACUrlCmdMessage ACUrlCmdMessage
        {
            get { return GetValue(ACUrlCmdMessageProperty); }
            set { SetValue(ACUrlCmdMessageProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ACCompInitState.
        /// </summary>
        public static readonly StyledProperty<ACInitState> ACCompInitStateProperty = AvaloniaProperty.Register<VBDockingManager, ACInitState>(nameof(ACCompInitState));

        /// <summary>
        /// Gets or sets the ACCompInitState.
        /// </summary>
        public ACInitState ACCompInitState
        {
            get { return GetValue(ACCompInitStateProperty); }
            set { SetValue(ACCompInitStateProperty, value); }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == MasterPageFreezeProperty)
                RootPageWPF_VBDockingManagerFreezingEvent();
            else if (change.Property == ACUrlCmdMessageProperty)
                OnACUrlMessageReceived();
            else if (change.Property == ACCompInitStateProperty)
                InitStateChanged();
            else if (change.Property == FocusViewProperty)
            {
                if (vbDockingPanelTabbedDoc != null && change.NewValue != null)
                    vbDockingPanelTabbedDoc.FocusView = change.NewValue.ToString();
            }
            else if (change.Property == DataContextProperty)
            {
                if (change.NewValue == null && change.OldValue != null)
                {
                    IACBSO bso = change.OldValue as IACBSO;
                    if (bso != null)
                        DeInitVBControl(bso);
                }
            }
            base.OnPropertyChanged(change);

        }
        #endregion

        /// <summary>By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
        /// The ACUrlBinding()-Method returns a Source and a Path which the Control use to create a WPF-Binding to bind the right value and set the WPF-DataContext.
        /// ACUrl's can be either absolute or relative to the DataContext of the parent WPFControl (or the ContextACObject of the parent IACInteractiveObject)</summary>
        /// <value>Relative or absolute ACUrl</value>
        [Category("VBControl")]
        public string VBContent
        {
            get;
            set;
        }

    }

    /// <summary>
    /// The settings for window position of the VBDesign.
    /// </summary>
    [Serializable]
    public class SettingsVBDesignWndPos
    {
        /// <summary>
        /// Creates a new instance of SettingsWndPos.
        /// </summary>
        public SettingsVBDesignWndPos()
        {
        }

        /// <summary>
        /// Creates a new instance of SettingsWndPos.
        /// </summary>
        /// <param name="acIdentifier">The acIdentifier parameter.</param>
        /// <param name="wndRect">The widnow rectangle parameter.</param>
        public SettingsVBDesignWndPos(string acIdentifier, Rect wndRect)
        {
            ACIdentifier = acIdentifier;
            WndRect = wndRect;
        }

        /// <summary>
        /// Gets or sets the ACIdentifier.
        /// </summary>
        public string ACIdentifier
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the WndRect.
        /// </summary>
        public Rect WndRect
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Represents the list of settings for window position of VBDesign.
    /// </summary>
    [Serializable]
    public class SettingsVBDesignWndPosList : List<SettingsVBDesignWndPos>
    {
    }

}
