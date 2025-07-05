using System.Runtime.CompilerServices;
// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 12-20-2012
// ***********************************************************************
// <copyright file="ACClassInfoWithItems.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Transactions;

namespace gip.core.datamodel
{
    /// <summary>
    /// Container mit Items für ein ACClass
    /// Verwendung: Für alle Trees in denen Klassenbäumne dargestellt werden u.a. in BSOiPlusStudio, BSOPartslist
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Classinfo'}de{'Klasseninformation'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class ACClassInfoWithItems : IACContainerWithItemsT<ACClassInfoWithItems, ACClass>, IACObject, INotifyPropertyChanged, IVBDataCheckbox, IACClassDesignProvider, IVBIsVisible, ICloneable
    {
        #region c´tors
        /// <summary>
        /// Initializes a new instance of the <see cref="ACClassInfoWithItems"/> class.
        /// </summary>
        public ACClassInfoWithItems()
        {
            IsEnabled = true;
        }

        public ACClassInfoWithItems(ACClass aCClass)
            : this()
        {
            this.ValueT = aCClass;
        }

        public ACClassInfoWithItems(string acCaption)
            : this()
        {
            ACCaption = acCaption;
        }

        /// <summary>
        /// Constructor for Root-Items
        /// </summary>
        /// <param name="acCaption"></param>
        /// <param name="showCaptionInTree"></param>
        /// <param name="visibilityFilter"></param>
        /// <param name="checkHandler"></param>
        public ACClassInfoWithItems(string acCaption, bool showCaptionInTree, VisibilityFilters visibilityFilter, CheckHandler checkHandler)
            : this()
        {
            ACCaption = acCaption;
            SetPresentationOptions(showCaptionInTree, visibilityFilter, checkHandler);
        }

        #endregion

        #region Helper Classes for Filtering and Checking
        public class VisibilityFilters : ICloneable
        {
            // Inclusion-Properties - If nothing set, then everything ist visible. If one ore more are set, then only those are presented.
            public bool IncludeProcessModules { get; set; }
            public bool IncludeProcessFunctions { get; set; }
            public bool IncludeModules { get; set; }
            public bool IncludeBackgroundModules { get; set; }
            public bool IncludeLibraryClasses { get; set; }
            public IEnumerable<Type> IncludeTypes { get; set; }

            // Filter/Exclusion-Properties:
            public ACClass FilterACClass { get; set; }
            public string SearchText { get; set; }
            public string SearchACUrlComponent { get; set; }
            public Func<ACClassInfoWithItems, bool> CustomFilter { get; set; }


            public bool IsVisible(ACClassInfoWithItems item)
            {
                if (item == null)
                    throw new ArgumentNullException();
                if (!IsAnyFilterSet)
                    return true;
                if (item.ValueT == null)
                    return true;

                // Inclusion-Rules
                return (((!IncludeProcessModules && !IncludeProcessFunctions && !IncludeModules && !IncludeBackgroundModules && !IncludeLibraryClasses)
                              || (IncludeProcessModules && item.ValueT.ACKind == Global.ACKinds.TPAProcessModule)
                              || (IncludeProcessFunctions && item.ValueT.ACKind == Global.ACKinds.TPAProcessFunction)
                              || (IncludeModules && item.ValueT.ACKind == Global.ACKinds.TPAModule)
                              || (IncludeBackgroundModules && item.ValueT.ACKind == Global.ACKinds.TPABGModule)
                              || (IncludeLibraryClasses && (item.ValueT.ACKind == Global.ACKinds.TACAbstractClass
                                                                || item.ValueT.ACKind == Global.ACKinds.TPAProcessFunction
                                                                || item.ValueT.ACKind == Global.ACKinds.TPWMethod
                                                                || item.ValueT.ACKind == Global.ACKinds.TPWNode
                                                                || item.ValueT.ACKind == Global.ACKinds.TPWNodeStatic
                                                                || item.ValueT.ACKind == Global.ACKinds.TPWNodeMethod
                                                                || item.ValueT.ACKind == Global.ACKinds.TPWNodeWorkflow
                                                                || item.ValueT.ACKind == Global.ACKinds.TPWGroup
                                                                || item.ValueT.ACKind == Global.ACKinds.TACObject
                                                                || item.ValueT.ACKind == Global.ACKinds.TACVBControl))
                            )
                       // Exclusion/Filter-Rules
                       && (
                                  (String.IsNullOrEmpty(SearchText)
                                    || item.ValueT.ACCaption.IndexOf(SearchText, StringComparison.CurrentCultureIgnoreCase) >= 0
                                    || item.ValueT.ACIdentifier.IndexOf(SearchText, StringComparison.CurrentCultureIgnoreCase) >= 0
                                  )
                              && (     String.IsNullOrEmpty(SearchACUrlComponent)
                                    || item.ValueT.ACUrlComponent.IndexOf(SearchACUrlComponent, StringComparison.CurrentCultureIgnoreCase) >= 0
                                  )
                              && (FilterACClass == null || FilterACClass == item.ValueT)
                              && (CustomFilter == null || CustomFilter(item))
                              && (IncludeTypes == null || IncludeTypes.Where(c => c.IsAssignableFrom(item.ValueT.ObjectType)).Any())
                           )
                      );
            }

            public bool IsAnyFilterSet
            {
                get
                {
                    return IncludeProcessModules
                        || IncludeModules
                        || IncludeBackgroundModules
                        || IncludeProcessFunctions
                        || IncludeLibraryClasses
                        || FilterACClass != null
                        || !String.IsNullOrEmpty(SearchText)
                        || !String.IsNullOrEmpty(SearchACUrlComponent)
                        || CustomFilter != null
                        || IncludeTypes != null;
                }
            }

            public override bool Equals(object obj)
            {
                VisibilityFilters filter = obj as VisibilityFilters;
                if (filter == null)
                    return base.Equals(obj);
                return IncludeProcessModules == filter.IncludeProcessModules
                        && IncludeModules == filter.IncludeModules
                        && IncludeBackgroundModules == filter.IncludeBackgroundModules
                        && IncludeProcessFunctions == filter.IncludeProcessFunctions
                        && IncludeLibraryClasses == filter.IncludeLibraryClasses
                        && SearchText == filter.SearchText
                        && SearchACUrlComponent == filter.SearchACUrlComponent
                        && FilterACClass == filter.FilterACClass
                        && CustomFilter == filter.CustomFilter
                        && IncludeTypes == filter.IncludeTypes;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public object Clone()
            {
                return new VisibilityFilters()
                {
                    IncludeProcessModules = this.IncludeProcessModules,
                    IncludeModules = this.IncludeModules,
                    IncludeBackgroundModules = this.IncludeBackgroundModules,
                    IncludeProcessFunctions = this.IncludeProcessFunctions,
                    IncludeLibraryClasses = this.IncludeLibraryClasses,
                    FilterACClass = this.FilterACClass,
                    SearchText = this.SearchText,
                    SearchACUrlComponent = this.SearchACUrlComponent,
                    CustomFilter = this.CustomFilter,
                    IncludeTypes = this.IncludeTypes
                };
            }
        }

        public class CheckHandler : ICloneable
        {
            public bool QueryRightsFromDB { get; set; }
            public bool IsCheckboxVisible { get; set; }
            public Func<ACClassInfoWithItems, bool> CheckedGetter { get; set; }
            public Action<ACClassInfoWithItems, bool> CheckedSetter { get; set; }
            public Func<ACClassInfoWithItems, bool> CheckIsEnabledGetter { get; set; }

            public override bool Equals(object obj)
            {
                CheckHandler handler = obj as CheckHandler;
                if (handler == null)
                    return base.Equals(obj);
                return IsCheckboxVisible == handler.IsCheckboxVisible
                        && CheckedGetter == handler.CheckedGetter
                        && CheckedSetter == handler.CheckedSetter
                        && CheckIsEnabledGetter == handler.CheckIsEnabledGetter;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public object Clone()
            {
                return new CheckHandler()
                {
                    QueryRightsFromDB = this.QueryRightsFromDB,
                    IsCheckboxVisible = this.IsCheckboxVisible,
                    CheckedGetter = this.CheckedGetter,
                    CheckedSetter = this.CheckedSetter,
                    CheckIsEnabledGetter = this.CheckIsEnabledGetter
                };
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// Tritt ein, wenn sich ein Eigenschaftswert ändert.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region Properties

        #region IACContainer

        ACClass _ValueT = null;

        /// <summary>Gets or sets the encapsulated value of the generic type T.
        /// T is ACClass</summary>
        /// <value>The encapsulated ACClass</value>
        [ACPropertyInfo(9999)]
        public ACClass ValueT
        {
            get
            {
                return _ValueT;
            }
            set
            {
                _ValueT = value;
                OnPropertyChanged("ValueT");
                OnPropertyChanged("Value");
                OnPropertyChanged("ACIdentifier");
                OnPropertyChanged("ACCaption");
                OnPropertyChanged("ItemText");
            }
        }

        /// <summary>Gets or sets the encapsulated value as a boxed type</summary>
        /// <value>The boxed value.</value>
        public object Value
        {
            get
            {
                return ValueT;
            }
            set
            {
                ValueT = value as ACClass;
            }
        }

        /// <summary>Metadata (iPlus-Type) of the Value-Property. ATTENTION: ACClass is a EF-Object. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!</summary>
        /// <value>Metadata (iPlus-Type) of the Value-Property as ACClass</value>
        public ACClass ValueTypeACClass
        {
            get
            {
                return ValueT == null ? null : ValueT.ACType as ACClass;
            }
        }

        #endregion

        #region IACContainerWithItems

        public List<ACClassInfoWithItems> _ItemsT = new List<ACClassInfoWithItems>();
        
        /// <summary>Container-Childs</summary>
        /// <value>Container-Childs</value>
        public IEnumerable<ACClassInfoWithItems> ItemsT
        {
            get
            {
                return _ItemsT;
            }
        }

        /// <summary>Container-Childs</summary>
        /// <value>Container-Childs</value>
        public IEnumerable<IACContainerWithItems> Items
        {
            get
            {
                return ItemsT;
            }
        }

        ACClassInfoWithItems _ParentT;
        /// <summary>Gets the parent container T.</summary>
        /// <value>The parent container T.</value>
        public ACClassInfoWithItems ParentContainerT
        {
            get
            {
                return _ParentT;
            }
            set
            {
                _ParentT = value;
                OnPropertyChanged("ParentT");
                OnPropertyChanged("ParentACObject");
            }
        }

        /// <summary>Gets the parent container.</summary>
        /// <value>The parent container.</value>
        public IACContainerWithItems ParentContainer
        {
            get
            {
                return ParentContainerT;
            }
        }

        /// <summary>
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        public IACObject ParentACObject
        {
            get { return ParentContainerT; }
            set { ParentContainerT = value as ACClassInfoWithItems; }
        }


        /// <summary>Gets the root container T.</summary>
        /// <value>The root container T.</value>
        public ACClassInfoWithItems RootContainerT
        {
            get
            {
                return ParentContainerT != null ? ParentContainerT.RootContainerT : this;
            }
        }


        /// <summary>Gets the root container.</summary>
        /// <value>The root container.</value>
        public IACContainerWithItems RootContainer
        {
            get
            {
                return RootContainerT;
            }
        }
        #endregion

        #region Presentation

        #region Visibility
        private bool? _IsVisible;
        public bool IsVisible
        {
            get
            {
                if (_IsVisible.HasValue)
                    return _IsVisible.Value;
                return SearchClassChildCount > 0
                       || FilterOnParent == null
                       || (!FilterOnParent.IsAnyFilterSet && SearchClassChildCount < 0);
            }
            set
            {
                _IsVisible = value;
            }
        }

        VisibilityFilters _Filter;
        public VisibilityFilters Filter
        {
            get
            {
                return _Filter;
            }
            set
            {
                bool filterChanged = (_Filter == null && value != null)
                                    || (_Filter != null && value == null)
                                    || (_Filter != null && value != null && !_Filter.Equals(value));
                _Filter = value;
                if (filterChanged)
                    ApplyFilterAndUpdateVisibilityInTree();
                OnPropertyChanged("Filter");
            }
        }

        public VisibilityFilters FilterOnParent
        {
            get
            {
                if (_Filter != null)
                    return _Filter;
                if (ParentContainerT == null)
                    return null;
                return ParentContainerT.FilterOnParent;
            }
        }

        public void ApplyFilterAndUpdateVisibilityInTree()
        {
            SearchClassChildCount = -1;

            foreach (var childs in ItemsT)
            {
                childs.ApplyFilterAndUpdateVisibilityInTree();
            }

            // Groups who doesn't have a reference to a Class are only displayed if there are any childs which meets the filter
            if (ValueT != null
                && (FilterOnParent == null || FilterOnParent.IsVisible(this)))
                IncreaseSearchCounter();

            OnPropertyChanged("IsVisible");
            OnPropertyChanged("VisibleItemsT");
        }

        private void IncreaseSearchCounter()
        {
            if (SearchClassChildCount <= 0)
                SearchClassChildCount = 0;
            SearchClassChildCount++;
            if (ParentContainerT != null)
                ParentContainerT.IncreaseSearchCounter();
        }

        /// <summary>Visible Container-Childs</summary>
        /// <value>Visible Container-Childs</value>
        [ACPropertyInfo(9999)]
        public IEnumerable<ACClassInfoWithItems> VisibleItemsT
        {
            get
            {
                return ItemsT.Where(c => c.IsVisible);
            }
        }

        /// <summary>
        /// Gets the control mode.
        /// </summary>
        /// <value>The control mode.</value>
        [ACPropertyInfo(9999)]
        public Global.ControlModes ControlMode
        {
            get
            {
                return IsChecked ? Global.ControlModes.Enabled : Global.ControlModes.Hidden;
            }
        }
        #endregion

        #region Check
        /// <summary>
        /// The _ is checked
        /// </summary>
        bool _IsChecked;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is checked.
        /// </summary>
        /// <value><c>true</c> if this instance is checked; otherwise, <c>false</c>.</value>
        [ACPropertyInfo(9999)]
        public bool IsChecked
        {
            get
            {
                if (HandlerForCheckboxOnParent != null && HandlerForCheckboxOnParent.CheckedGetter != null)
                    return HandlerForCheckboxOnParent.CheckedGetter(this);
                return _IsChecked;
            }
            set
            {
                if (HandlerForCheckboxOnParent != null && HandlerForCheckboxOnParent.CheckedSetter != null)
                    HandlerForCheckboxOnParent.CheckedSetter(this, value);

                _IsChecked = value;
                OnPropertyChanged(IsCheckedPropName);
            }
        }
        public const string IsCheckedPropName = "IsChecked";

        CheckHandler _HandlerForCheckbox;
        public CheckHandler HandlerForCheckbox
        {
            get
            {
                return _HandlerForCheckbox;
            }
            set
            {
                bool handlerChanged = (_HandlerForCheckbox == null && value != null)
                                    || (_HandlerForCheckbox != null && value == null)
                                    || (_HandlerForCheckbox != null && value != null && !_HandlerForCheckbox.Equals(value));
                _HandlerForCheckbox = value;
                if (handlerChanged)
                    ApplyCheckHandlerOnChilds();
                OnPropertyChanged("HandlerForCheckbox");
            }
        }

        public void ApplyCheckHandlerOnChilds()
        {
            foreach (var childs in ItemsT)
            {
                childs.ApplyCheckHandlerOnChilds();
            }
            OnPropertyChanged("DataContentCheckBox");
            OnPropertyChanged(IsCheckedPropName);
        }

        public CheckHandler HandlerForCheckboxOnParent
        {
            get
            {
                if (_HandlerForCheckbox != null)
                    return _HandlerForCheckbox;
                if (ParentContainerT == null)
                    return null;
                return ParentContainerT.HandlerForCheckboxOnParent;
            }
        }


        #endregion

        #endregion

        #region IACObjectWithBinding Member

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

        string _ACCaption;
        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [ACPropertyInfo(9999)]
        public string ACCaption
        {
            get
            {
                if (!String.IsNullOrEmpty(_ACCaption))
                    return _ACCaption;
                return ValueT.ACCaption;
            }
            set
            {
                _ACCaption = value;
                OnPropertyChanged("ACCaption");
                OnPropertyChanged("ItemText");
            }
        }

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        [ACPropertyInfo(9999)]
        public string ACIdentifier
        {
            get
            {
                if (ValueT == null)
                    return ACCaption;
                return ValueT.ACIdentifier;
            }
        }

        [ACPropertyInfo(9999)]
        public string ACUrlComponent
        {
            get
            {
                if (ValueT == null)
                    return ACCaption;
                return ValueT.ACUrlComponent;
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
                return this.ReflectGetACContentList();
            }
        }

        [ACPropertyInfo(9999)]
        public string ItemText
        {
            get
            {
                return ShowCaptionOnParent ? ACCaption : ACIdentifier;
            }
        }


        private bool? _ShowCaption;
        public bool? ShowCaption
        {
            get
            {
                return _ShowCaption;
            }
            set
            {
                bool changed = _ShowCaption != value;
                _ShowCaption = value;
                if (changed)
                    RefreshTreeItemText();
                OnPropertyChanged("ShowCaption");
            }
        }

        private void RefreshTreeItemText()
        {
            foreach (var childs in ItemsT)
            {
                childs.RefreshTreeItemText();
            }
            OnPropertyChanged("ItemText");
        }

        public bool ShowCaptionOnParent
        {
            get
            {
                if (_ShowCaption.HasValue)
                    return _ShowCaption.Value;
                if (ParentContainerT == null)
                    return false;
                return ParentContainerT.ShowCaptionOnParent;
            }
        }
        #endregion

        #region IVBDataCheckbox Member
        private string _DataContentCheckBox;
        /// <summary>
        /// Gets or sets the data content check box.
        /// </summary>
        /// <value>The data content check box.</value>
        public string DataContentCheckBox
        {
            get
            {
                if (!String.IsNullOrEmpty(_DataContentCheckBox))
                    return _DataContentCheckBox;
                if (HandlerForCheckboxOnParent != null && HandlerForCheckboxOnParent.IsCheckboxVisible)
                    return IsCheckedPropName;
                return _DataContentCheckBox;
            }
            set
            {
                _DataContentCheckBox = value;
                OnPropertyChanged("DataContentCheckBox");
            }
        }

        private bool _IsEnabled = false;
        public bool IsEnabled
        {
            get
            {
                if (HandlerForCheckboxOnParent != null && HandlerForCheckboxOnParent.CheckIsEnabledGetter != null)
                    return HandlerForCheckboxOnParent.CheckIsEnabledGetter(this);
                return _IsEnabled;
            }
            set
            {
                _IsEnabled = value;
                OnPropertyChanged("IsEnabled");
            }
        }
        #endregion

        #region Searching
        /// <summary>
        /// The _ search class child count
        /// </summary>
        private int _SearchClassChildCount = -1;
        /// <summary>
        /// Gets or sets the search class child count.
        /// </summary>
        /// <value>The search class child count.</value>
        public int SearchClassChildCount
        {
            get
            {
                return _SearchClassChildCount;
            }
            set
            {
                _SearchClassChildCount = value;
                OnPropertyChanged("SearchClassChildCount");
            }
        }
        #endregion

        #region Misc

        private object _IconState;
        [ACPropertyInfo(999)]
        public object IconState
        {
            get
            {
                return _IconState;
            }
            set
            {
                _IconState = value;
                OnPropertyChanged("IconState");
            }
        }

        #endregion

        #endregion

        #region Methods

        #region IACContainerWithItems
        /// <summary>Adds the specified child-container</summary>
        /// <param name="child">The child-container</param>
        public void Add(IACContainerWithItems child)
        {
            Add(child as ACClassInfoWithItems);
        }

        /// <summary>Adds the specified child-container</summary>
        /// <param name="child">The child-container</param>
        public void Add(ACClassInfoWithItems child)
        {
            if (child == null)
                throw new ArgumentNullException("child is null");
            if (child != null)
                child.ParentContainerT = this;
            _ItemsT.Add(child);
            OnPropertyChanged("ItemsT");
            OnPropertyChanged("Items");
            OnPropertyChanged("VisibleItemsT");
        }

        /// <summary>Removes the specified child-container</summary>
        /// <param name="child">The child-container</param>
        /// <returns>true if removed</returns>
        public bool Remove(IACContainerWithItems child)
        {
            return Remove(child as ACClassInfoWithItems);
        }


        /// <summary>Removes the specified child-container</summary>
        /// <param name="child">The child-container</param>
        /// <returns>true if removed</returns>
        public bool Remove(ACClassInfoWithItems child)
        {
            if (child == null)
                return false;
            bool removed = _ItemsT.Remove(child);
            if (removed)
            {
                OnPropertyChanged("ItemsT");
                OnPropertyChanged("Items");
                OnPropertyChanged("VisibleItemsT");
            }
            return removed;
        }

        public void EliminateTree()
        {
            foreach (var child in ItemsT)
            {
                child.EliminateTree();
            }
            RemoveAllChilds();
            ParentContainerT = null;
            _Filter = null;
            _SearchClassChildCount = -1;
        }

        private void RemoveAllChilds()
        {
            _ItemsT = new List<ACClassInfoWithItems>();
            OnPropertyChanged("ItemsT");
            OnPropertyChanged("Items");
            OnPropertyChanged("VisibleItemsT");
        }


        #endregion

        #region IACObjectWithBinding

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
            if (acUrl == Const.CmdTooltip)
            {
                if (ValueT != null)
                {
                    return ValueT.ACUrlCommand(acUrl, acParameter);
                }
                else
                {
                    return Database.Root.Environment.TranslateText(this, "Group") + " " + ACCaption;
                }
            }

            return this.ReflectACUrlCommand(acUrl, acParameter);
        }


        /// <summary>
        /// This method is called before ACUrlCommand if a method-command was encoded in the ACUrl
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>true if ACUrlCommand can be invoked</returns>
        public bool IsEnabledACUrlCommand(string acUrl, params Object[] acParameter)
        {
            if (acUrl == Const.CmdTooltip)
            {
                if (ValueT != null)
                {
                    return ValueT.IsEnabledACUrlCommand(acUrl, acParameter);
                }
                else
                {
                    return true;
                }
            }

            return this.ReflectIsEnabledACUrlCommand(acUrl, acParameter);
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
            return this.ReflectACUrlBinding(acUrl, ref acTypeInfo, ref source, ref path, ref rightControlMode);
        }

        public bool ACUrlTypeInfo(string acUrl, ref ACUrlTypeInfo acUrlTypeInfo)
        {
            return this.ReflectACUrlTypeInfo(acUrl, ref acUrlTypeInfo);
        }


        /// <summary>
        /// Returns a ACUrl relatively to the passed object.
        /// If the passed object is null then the absolute path is returned
        /// </summary>
        /// <param name="rootACObject">Object for creating a realtive path to it</param>
        /// <returns>ACUrl as string</returns>
        public string GetACUrl(IACObject rootACObject = null)
        {
            if (ValueT == null && Value != null)
                return ((IACObject)Value).GetACUrl(rootACObject);
            else if (ValueT != null)
                return ValueT.GetACUrl(rootACObject);
            return null;
        }

        /// <summary>Returns a ACClassDesign for presenting itself on the gui</summary>
        /// <param name="acUsage">Filter for selecting designs that belongs to this ACUsages-Group</param>
        /// <param name="acKind">Filter for selecting designs that belongs to this ACKinds-Group</param>
        /// <param name="vbDesignName">Optional: The concrete acIdentifier of the design</param>
        /// <returns>ACClassDesign</returns>
        public ACClassDesign GetDesign(Global.ACUsages acUsage, Global.ACKinds acKind, string vbDesignName = "")
        {
            if (ValueT == null)
                return null;
            return ValueT.GetDesign(ValueT, acUsage, acKind, vbDesignName);
        }

        #endregion

        #region Searching & Presentation
        public Action<ACClassInfoWithItems> AllItemsFunction { get; set; }

        public void CallOnAllItems(Action<ACClassInfoWithItems> action)
        {
            action(this);
            if (ItemsT != null)
                foreach (var childItem in ItemsT)
                    childItem.CallOnAllItems(action);
        }

        public void SetPresentationOptions(bool showCaptionInTree, VisibilityFilters visibilityFilter, CheckHandler checkHandler)
        {
            HandlerForCheckbox = checkHandler;
            Filter = visibilityFilter;
            ShowCaption = showCaptionInTree;
        }
        #endregion

        #region Clone
        public object Clone()
        {
            return new ACClassInfoWithItems()
            {
                ValueT = this._ValueT,
                ACCaption = this._ACCaption,
                IsEnabled = this.IsEnabled,
                Filter = this.Filter,
                HandlerForCheckbox = this.HandlerForCheckbox,
                ShowCaption = this.ShowCaption
            };
        }
        #endregion

        #endregion
    }
}
