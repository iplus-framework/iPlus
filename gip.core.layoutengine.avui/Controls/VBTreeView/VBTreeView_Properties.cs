using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
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
    public partial class VBTreeView
    {
        string _DataSource;
        string _DataShowColumns;
        string _DataChilds = "Items";
        bool _isRoot;
        protected bool _Initialized = false;
        bool _Loaded = false;
        private readonly Dictionary<Key, bool> _pressedKeys = new Dictionary<Key, bool>();

        #region Additional Styled Properties
        /// <summary>
        /// Represents the styled property for control mode.
        /// </summary>
        public static readonly StyledProperty<Global.ControlModes> ControlModeProperty = 
            AvaloniaProperty.Register<VBTreeView, Global.ControlModes>(nameof(ControlMode));

        /// <summary>
        /// Gets or sets the Control mode.
        /// </summary>
        public Global.ControlModes ControlMode
        {
            get => GetValue(ControlModeProperty);
            set => SetValue(ControlModeProperty, value);
        }

        #endregion

        /// <summary>
        /// Represents the styled property for ACCaption.
        /// </summary>
        public static readonly StyledProperty<string> ACCaptionProperty =
            AvaloniaProperty.Register<VBTreeView, string>(Const.ACCaptionPrefix);

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [Category("VBControl")]
        public string ACCaption
        {
            get => GetValue(ACCaptionProperty);
            set => SetValue(ACCaptionProperty, value);
        }

        /// <summary>
        /// Represents the styled property for ACCaptionTrans.
        /// </summary>
        public static readonly StyledProperty<string> ACCaptionTransProperty =
            AvaloniaProperty.Register<VBTreeView, string>(nameof(ACCaptionTrans));

        /// <summary>
        /// Gets or sets the ACCaption translation.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt die ACCaption-Übersetzung.
        /// </summary>
        [Category("VBControl")]
        public string ACCaptionTrans
        {
            get => GetValue(ACCaptionTransProperty);
            set => SetValue(ACCaptionTransProperty, value);
        }


        /// <summary>
        /// Represents the styled property for ShowCaption.
        /// </summary>
        public static readonly StyledProperty<bool> ShowCaptionProperty =
            AvaloniaProperty.Register<VBTreeView, bool>(nameof(ShowCaption), true);
        /// <summary>
        /// Determines is control caption shown or not.
        /// </summary>
        /// <summary xml:lang="de">
        /// Legt fest, ob die Steuertitel angezeigt werden oder nicht.
        /// </summary>
        [Category("VBControl")]
        public bool ShowCaption
        {
            get => GetValue(ShowCaptionProperty);
            set => SetValue(ShowCaptionProperty, value);
        }

        /// <summary>
        /// Represents the styled property for DisableContextMenu.
        /// </summary>
        public static readonly StyledProperty<bool> DisableContextMenuProperty = 
            ContentPropertyHandler.DisableContextMenuProperty.AddOwner<VBTreeView>();
        /// <summary>
        /// Determines is context menu disabled or enabled.
        /// </summary>
        /// <summary xml:lang="de">
        /// Ermittelt ist das Kontextmenü deaktiviert oder aktiviert
        /// </summary>
        [Category("VBControl")]
        [ACPropertyInfo(9999)]
        public bool DisableContextMenu
        {
            get => GetValue(DisableContextMenuProperty);
            set => SetValue(DisableContextMenuProperty, value);
        }

        /// <summary>
        /// Represents the styled property for the VBTreeViewExpandMethod
        /// </summary>
        public static readonly StyledProperty<string> VBTreeViewExpandMethodProperty = 
            AvaloniaProperty.Register<VBTreeView, string>(nameof(VBTreeViewExpandMethod));

        /// <summary>
        /// Represents the property where you enter the name of BSO's method, which will be invoked on OnVBTreeViewItemExpand event. It's used for VBTreeView lazy loading.
        /// </summary>
        [Category("VBControl")]
        public string VBTreeViewExpandMethod
        {
            get => GetValue(VBTreeViewExpandMethodProperty);
            set => SetValue(VBTreeViewExpandMethodProperty, value);
        }

        /// <summary>
        /// Determines is tree view check(set tick) items to root when is child item checked.
        /// </summary>
        [Category("VBControl")]
        public bool CheckToRoot
        {
            get => GetValue(CheckToRootProperty);
            set => SetValue(CheckToRootProperty, value);
        }

        /// <summary>
        /// Represents the styled property for the CheckToRoot.
        /// </summary>
        public static readonly StyledProperty<bool> CheckToRootProperty =
            AvaloniaProperty.Register<VBTreeView, bool>(nameof(CheckToRoot), false);
        

        #region IDataField Members

        /// <summary>
        /// Represents the styled property for VBContent.
        /// </summary>
        public static readonly StyledProperty<string> VBContentProperty =
            AvaloniaProperty.Register<VBTreeView, string>(nameof(VBContent));

        /// <summary>
        /// Represents the property where you enter the name of BSO's property which is in tree structure(example: ACClassInfoWithItems) to connect it with tree view.
        /// By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
        /// The ACUrlBinding()-Method returns a Source and a Path which the Control use to create a WPF-Binding to bind the right value and set the WPF-DataContext.
        /// ACUrl's can be either absolute or relative to the DataContext of the parent WPFControl (or the ContextACObject of the parent IACInteractiveObject)</summary>
        /// <value>Relative or absolute ACUrl</value>
        [Category("VBControl")]
        public string VBContent
        {
            get => GetValue(VBContentProperty);
            set => SetValue(VBContentProperty, value);
        }

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public string ACIdentifier
        {
            get
            {
                return Name;
            }
            set
            {
                Name = value;
            }
        }

        /// <summary>
        /// Gets or sets the CheckBox level.
        /// </summary>
        [Category("VBControl")]
        public int CheckBoxLevel
        {
            get;
            set;
        }

        /// <summary>
        /// Represents the styled property for ExpandLevel.
        /// </summary>
        public static readonly StyledProperty<int> ExpandLevelProperty =
            AvaloniaProperty.Register<VBTreeView, int>(nameof(ExpandLevel));

        /// <summary>
        /// Gets or sets the expand level. Determines to which level tree view will be pre-expanded.
        /// </summary>
        [Category("VBControl")]
        public int ExpandLevel
        {
            get => GetValue(ExpandLevelProperty);
            set => SetValue(ExpandLevelProperty, value);
        }
        #endregion

        #region IVBSource Members

        /// <summary>
        /// Represents the property where you enter the name of BSO's property which contains the root item of tree structure.
        /// </summary>
        [Category("VBControl")]
        public string VBSource
        {
            get
            {
                return _DataSource;
            }
            set
            {
                _DataSource = value;
            }
        }

        /// <summary>
        /// Determines which column will be shown. (On example: ACCaption)
        /// </summary>
        [Category("VBControl")]
        public string VBShowColumns
        {
            get
            {
                return _DataShowColumns;
            }
            set
            {
                _DataShowColumns = value;
            }
        }

        /// <summary>
        /// Determines which columns will be disabled.
        /// </summary>
        public string VBDisabledColumns
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the VBChilds.
        /// </summary>
        public string VBChilds
        {
            get
            {
                return _DataChilds;
            }
            set
            {
                _DataChilds = value;
            }
        }

        /// <summary>
        /// Gets or sets the ACQueryDefinition.
        /// </summary>
        public ACQueryDefinition ACQueryDefinition
        {
            get;
            set;
        }

        // TODO: 
        /// <summary>
        /// Gets or sets sort order.
        /// </summary>
        public string SortOrder
        {
            get;
            set;
        }

        #endregion

        #region IDataContent Members
        /// <summary>
        /// ContextACObject is used by WPF-Controls and mostly it equals to the FrameworkElement.DataContext-Property.
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

        /// <summary>
        /// Represents the styled property for BSOACComponent.
        /// </summary>
        public static readonly StyledProperty<IACBSO> BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner<VBTreeView>();
        /// <summary>
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get => GetValue(BSOACComponentProperty);
            set => SetValue(BSOACComponentProperty, value);
        }

        /// <summary>
        /// Represents the styled property for ACCompInitState.
        /// </summary>
        public static readonly StyledProperty<ACInitState> ACCompInitStateProperty = AvaloniaProperty.Register<VBTreeView, ACInitState>(nameof(ACCompInitState));

        /// <summary>
        /// Gets or sets the ACCompInitState.
        /// </summary>
        public ACInitState ACCompInitState
        {
            get => GetValue(ACCompInitStateProperty);
            set => SetValue(ACCompInitStateProperty, value);
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

        /// <summary>
        /// Zielobjekt beim ACDropData
        /// </summary>
        IACInteractiveObject CurrentTargetVBDataObject
        {
            get;
            set;
        }


        string _DblClick = "";
        /// <summary>
        /// Gets or sets the double click ACMethod name.
        /// </summary>
        public string DblClick
        {
            get
            {
                if (string.IsNullOrEmpty(_DblClick) && (BSOACComponent != null))
                {
                    var query = BSOACComponent.ACClassMethods.Where(c => c.InteractionVBContent == VBContent && c.SortIndex == (short)MISort.Load);
                    if (query.Any())
                    {
                        return query.First().ACIdentifier;
                    }
                }
                return _DblClick;
            }
            set
            {
                _DblClick = value;
            }
        }

        /// <summary>
        /// Determines is control enabled or disabled.
        /// </summary>
        public bool Enabled
        {
            get
            {
                return IsEnabled;
            }
            set
            {
                if (value == true)
                {
                    if (ContextACObject == null)
                    {
                        IsEnabled = true;
                    }
                    else
                    {
                        IsEnabled = RightControlMode >= Global.ControlModes.Enabled;
                    }
                }
                else
                {
                    IsEnabled = false;
                }
                ControlModeChanged();
            }
        }

        /// <summary>
        /// Enables or disables auto focus.
        /// </summary>
        /// <summary xml:lang="de">
        /// Aktiviert oder deaktiviert den Autofokus.
        /// </summary>
        [Category("VBControl")]
        public bool AutoFocus { get; set; }

        /// <summary>
        /// Gets or sets DragEnabled.
        /// </summary>
        [Category("VBControl")]
        public DragMode DragEnabled { get; set; }

        IACType _VBContentPropertyInfo = null;
        /// <summary>
        /// Gets the ACClassProperty which describes a bounded property by VBContent.
        /// </summary>
        public ACClassProperty VBContentPropertyInfo
        {
            get
            {
                return _VBContentPropertyInfo as ACClassProperty;
            }
        }


        /// <summary>
        /// Represents the styled property for DisabledModes.
        /// </summary>
        public static readonly StyledProperty<string> DisabledModesProperty =
            AvaloniaProperty.Register<VBTreeView, string>(nameof(DisabledModes));
        /// <summary>
        /// Gets or sets the disabled modes.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt die deaktivierten Modi.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string DisabledModes
        {
            get => GetValue(DisabledModesProperty);
            set => SetValue(DisabledModesProperty, value);
        }
        #endregion

        #region IDataHandling Members
        /// <summary>
        /// Determines is auto load enabled or disabled.
        /// </summary>
        [Category("VBControl")]
        public bool AutoLoad { get; set; }
        #endregion

        #region private methods

        /// <summary>
        /// Represents the styled property for TreeDataSource.
        /// </summary>
        public static readonly StyledProperty<object> TreeDataSourceProperty =
            AvaloniaProperty.Register<VBTreeView, object>(nameof(TreeDataSource));

        /// <summary>
        /// Gets or sets the TreeDataSource.
        /// </summary>
        public object TreeDataSource
        {
            get => GetValue(TreeDataSourceProperty);
            set => SetValue(TreeDataSourceProperty, value);
        }

        #endregion

        #region DragAndDrop

        /// <summary>
        /// Represents the styled property for TreeItemClicked.
        /// </summary>
        public static readonly StyledProperty<int> TreeItemClickedProperty =
            AvaloniaProperty.Register<VBTreeView, int>(nameof(TreeItemClicked));

        /// <summary>
        /// Gets or sets the TreeItemClicked.
        /// </summary>
        public int TreeItemClicked
        {
            get => GetValue(TreeItemClickedProperty);
            set => SetValue(TreeItemClickedProperty, value);
        }
        #endregion

        /// <summary>
        /// Gets or sets the right control mode.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt den richtigen Kontrollmodus.
        /// </summary>
        [Category("VBControl")]
        public Global.ControlModes RightControlMode
        {
            get;
            set;
        }

        /// <summary>
        /// Represents the styled property for the TreeValue.
        /// </summary>
        public static readonly StyledProperty<IACObject> TreeValueProperty =
            AvaloniaProperty.Register<VBTreeView, IACObject>(nameof(TreeValue));

        /// <summary>
        /// Gets or sets the TreeValue (the selected model object).
        /// </summary>
        public IACObject TreeValue
        {
            get => GetValue(TreeValueProperty);
            set => SetValue(TreeValueProperty, value);
        }

        /// <summary>
        /// Gets or sets the ChangeInfo.
        /// </summary>
        public ChangeInfo ChangeInfo
        {
            get => GetValue(ChangeInfoProperty);
            set => SetValue(ChangeInfoProperty, value);
        }

        /// <summary>
        /// Represents the styled property for the ChangeInfo.
        /// </summary>
        public static readonly StyledProperty<ChangeInfo> ChangeInfoProperty =
            AvaloniaProperty.Register<VBTreeView, ChangeInfo>(nameof(ChangeInfo));


        #region IACObject
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
        #endregion
    }
}
