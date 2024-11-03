// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 11-07-2012
// ***********************************************************************
// <copyright file="ACFSItem.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;

namespace gip.core.datamodel
{

    public delegate void ACFSItemChange(ACFSItem aCFSItem, string propertyName, IACObject aCObject, string acObjectPropertyName);
    /// <summary>
    /// Container mit Verzeichnisinformationen (Verzeichnisse/Dateien) Serialisierbar
    /// Verwendung: Für alle Trees in denen Verzeichnisse dargestellt werden u.a. in BSOIPlusInstall
    /// </summary>
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACFSItem'}de{'ACFSItem'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    // 1 ACCaption
    // 2 ACUrlFS
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + "ACFSItem", "en{'ACFSItem'}de{'ACFSItem'}", typeof(ACFSItem), "ACFSItem", Const.ACCaptionPrefix, Const.ACCaptionPrefix)]
    public class ACFSItem : IACContainerWithItems, IACObject, INotifyPropertyChanged, IVBDataCheckbox, IVBIsVisible
    {
        #region event
        public event ACFSItemChange OnACFSItemChange;
        #endregion

        #region c´tors

        /// <summary>
        /// ACFSItem
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="container"></param>
        /// <param name="acObject"></param>
        /// <param name="caption"></param>
        /// <param name="resourceType"></param>
        /// <param name="acURLFS"></param>
        public ACFSItem(IResources resource, ACFSItemContainer container, IACObject acObject, string caption, ResourceTypeEnum resourceType, string acURLFS = "")
        {
            Resource = resource;
            ACObject = acObject;
            Container = container;
            container.AddStack(this);
            ACCaption = caption;
            _ResourceType = resourceType;
            DataContentCheckBox = "IsChecked";
            ACUrlFS = acURLFS;
            _ACObjectItemList = new ObservableCollection<IACContainerWithItems>();
            IsEnabled = true;
            IsVisible = true;
        }

        #endregion

        #region BSO->ACProperty
        /// <summary>
        /// The _ is checked
        /// </summary>
        bool _IsChecked = true;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is checked.
        /// </summary>
        /// <value><c>true</c> if this instance is checked; otherwise, <c>false</c>.</value>
        [ACPropertyInfo(9999, "IsChecked", "en{'Selected'}de{'Ausgewählt'}")]
        public bool IsChecked
        {
            get
            {
                return _IsChecked;
            }
            set
            {
                if (_IsChecked != value)
                {
                    _IsChecked = value;
                    IsCheckedChildrenProcess(value);
                    OnPropertyChanged("IsChecked");
                }
            }
        }

        private void IsCheckedChildrenProcess(bool value)
        {
            if (Items != null)
            {
                foreach (IACObject item in Items)
                {
                    if (item != null && item is ACFSItem)
                        (item as ACFSItem).IsChecked = value;
                }
            }
        }

        [ACPropertyInfo(9999, "IsNew", "en{'New'}de{'Neu'}")]
        public bool? IsNew
        {
            get
            {
                if (ACObject == null) return null;
                return (ACObject as VBEntityObject).EntityState == EntityState.Added;
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// The _ is directory
        /// </summary>
        ResourceTypeEnum _ResourceType = ResourceTypeEnum.Folder;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is directory.
        /// </summary>
        /// <value><c>true</c> if this instance is directory; otherwise, <c>false</c>.</value>
        [ACPropertyInfo(9999)]
        public ResourceTypeEnum ResourceType
        {
            get
            {
                return _ResourceType;
            }
            set
            {
                _ResourceType = value;
                OnPropertyChanged("ResourceType");
            }
        }

        /// <summary>
        /// The _ AC object
        /// </summary>
        IACObject _ACObject;
        /// <summary>
        /// Gets or sets the AC object.
        /// </summary>
        /// <value>The AC object.</value>
        [ACPropertyInfo(9999, "", "", "", false)]
        public IACObject ACObject
        {
            get
            {
                return _ACObject;
            }
            set
            {
                if (_ACObject != value)
                {
                    _ACObject = value;
                    if (value != null)
                        ACObjectACUrl = _ACObject.GetACUrl();
                    OnPropertyChanged("ACObject");
                    if (_ACObject != null)
                        if (_ACObject is INotifyPropertyChanged)
                            (_ACObject as INotifyPropertyChanged).PropertyChanged += ACFSItem_PropertyChanged;
                }
            }
        }

        private void ACFSItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (OnACFSItemChange != null)
                OnACFSItemChange(this, "ACObject", ACObject, e.PropertyName);
        }

        public IResources Resource { get; set; }

        public XElement XNode { get; set; }

        public IACObject OuterIACObject { get; set; }

        public string ACObjectACUrl { get; set; }

        private ACFSItemContainer _Container;
        public ACFSItemContainer Container
        {
            get
            {
                return _Container;
            }
            set
            {
                _Container = value;
            }
        }

#endregion

#region IACContainerWithItems

        ObservableCollection<IACContainerWithItems> _ACObjectItemList;
        /// <summary>Container-Childs</summary>
        /// <value>Container-Childs</value>
        [DataMember]
        [ACPropertyInfo(9999)]
        public IEnumerable<IACContainerWithItems> Items
        {
            get
            {
                return _ACObjectItemList.Select(c => c);
            }
        }

        /// <summary>Visible Container-Childs</summary>
        /// <value>Visible Container-Childs</value>
        [ACPropertyInfo(9999)]
        public IEnumerable<IACContainerWithItems> VisibleItemsT
        {
            get
            {
                return Items.Where(c => (c as IVBIsVisible).IsVisible);
            }
        }


        /// <summary>Gets the parent container.</summary>
        /// <value>The parent container.</value>
        public IACContainerWithItems ParentContainer
        {
            get
            {
                return this.ParentACObject as IACContainerWithItems;
            }
        }

        /// <summary>Gets the root container.</summary>
        /// <value>The root container.</value>
        public IACContainerWithItems RootContainer
        {
            get
            {
                if (this.ParentContainer == null)
                    return this;
                return ParentContainer.RootContainer;
            }
        }

        /// <summary>Adds the specified child-container</summary>
        /// <param name="child">The child-container</param>
        public void Add(IACContainerWithItems child)
        {
            _ACObjectItemList.Add(child);
            if (child is ACObjectItem)
            {
                (child as ACObjectItem).ParentACObject = this;
            }
            if (child is ACFSItem)
            {
                (child as ACFSItem).ParentACObject = this;
            }
            OnPropertyChanged("ACObjectItemList");
            OnPropertyChanged("Items");
        }

        public void AddItemChange(ACFSItemChanges item)
        {
            ACFSItemChanges oldItem = ItemChangesList.FirstOrDefault(x => x.PropertyName == item.PropertyName);
            if (oldItem != null)
                ItemChangesList.Remove(oldItem);
            ItemChangesList.Add(item);
        }

        /// <summary>Removes the specified child-container</summary>
        /// <param name="child">The child-container</param>
        /// <returns>true if removed</returns>
        public bool Remove(IACContainerWithItems child)
        {
            bool result = _ACObjectItemList.Remove(child);
            OnPropertyChanged("ACObjectItemList");
            OnPropertyChanged("Items");
            return result;
        }

#endregion

#region IACObjectWithBinding Member
        /// <summary>
        /// Returns a ACUrl relatively to the passed object.
        /// If the passed object is null then the absolute path is returned
        /// </summary>
        /// <param name="rootACObject">Object for creating a realtive path to it</param>
        /// <returns>ACUrl as string</returns>
        [ACMethodInfo("", "", 9999)]
        public string GetACUrl(IACObject rootACObject = null)
        {
            return ACIdentifier;
        }

        string _ACCaption;
        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [ACPropertyInfo(1, "", "en{'Name'}de{'Name'}")]
        public string ACCaption
        {
            get
            {
                if (!string.IsNullOrEmpty(_ACCaption))
                    return _ACCaption;
                if (_ACObject != null)
                    return _ACObject.ACIdentifier;
                return "unknown";
            }
            set
            {
                if (_ACCaption != value)
                {
                    _ACCaption = value;
                    OnPropertyChanged("ACCaption");
                }
            }
        }

        /// <summary>
        /// The _ AC URL FS
        /// </summary>
        string _ACUrlFS;
        /// <summary>
        /// Gets or sets the AC URL FS.
        /// </summary>
        /// <value>The AC URL FS.</value>
        [ACPropertyInfo(2, "", "en{'ACUrl'}de{'ACUrl'}")]
        public string ACUrlFS
        {
            get
            {
                return _ACUrlFS;
            }
            set
            {
                _ACUrlFS = value;
            }
        }

        /// <summary>
        /// Gets the folderpath.
        /// </summary>
        /// <value>The folderpath.</value>
        [ACPropertyInfo(999, "", "en{'Filepath'}de{'Dateipfad'}")]
        public string Path
        {
            get
            {
                string filepath = _ACUrlFS;
                if (filepath.StartsWith("\\Resources\\"))
                {
                    filepath = filepath.Substring(11);
                }
                return filepath;
            }
        }


        /// <summary>
        /// Gets the folderpath.
        /// </summary>
        /// <value>The folderpath.</value>
        [ACPropertyInfo(999, "", "en{'Folderpath'}de{'Ordnerpfad'}")]
        public string Folderpath
        {
            get
            {
                if (_ACUrlFS == null) return null;
                string folderpath = _ACUrlFS;
                if (folderpath.StartsWith(@"\Resources\"))
                {
                    folderpath = folderpath.Substring(@"\Resources\".Length);
                }
                if (folderpath.StartsWith(@"\XML\"))
                {
                    folderpath = folderpath.Substring(@"\XML\".Length);
                }
                if (ResourceType != ResourceTypeEnum.Folder) // Falls Direktory, dann Dateiname entfernen
                {
                    int pos = folderpath.LastIndexOf('\\');
                    if (pos != -1)
                    {
                        folderpath = folderpath.Substring(0, pos);
                    }
                }
                return folderpath;
            }
        }

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        [ACPropertyInfo(9999)]
        public string ACIdentifier
        {
            get
            {
                return this.ReflectGetACIdentifier();
            }
        }

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
        public virtual object ACUrlCommand(string acUrl, params Object[] acParameter)
        {
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
        public virtual bool ACUrlBinding(string acUrl, ref IACType acTypeInfo, ref object source, ref string path, ref Global.ControlModes rightControlMode)
        {
            return this.ReflectACUrlBinding(acUrl, ref acTypeInfo, ref source, ref path, ref rightControlMode);
        }

#endregion

#region IACObjectEntity Members
        /// <summary>
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        [ACPropertyInfo(9999)]
        public IACObject ParentACObject
        {
            get;
            set;
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

#endregion

#region static Properties
        /// <summary>
        /// Gets the key AC identifier.
        /// </summary>
        /// <value>The key AC identifier.</value>
        static public string KeyACIdentifier
        {
            get
            {
                return Const.ACCaptionPrefix;
            }
        }
#endregion

#region INotifyPropertyChanged Members
        /// <summary>
        /// Tritt ein, wenn sich ein Eigenschaftswert ändert.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="name">The name.</param>
        public void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }

            if (OnACFSItemChange != null)
                OnACFSItemChange(this, name, ACObject, null);
        }
#endregion

#region IACValue
        /// <summary>Gets or sets the encapsulated value as a boxed type</summary>
        /// <value>The boxed value.</value>
        public object Value
        {
            get
            {
                return ACObject;
            }
            set
            {
            }
        }

        /// <summary>Metadata (iPlus-Type) of the Value-Property. ATTENTION: ACClass is a EF-Object. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!</summary>
        /// <value>Metadata (iPlus-Type) of the Value-Property as ACClass</value>
        public ACClass ValueTypeACClass
        {
            get
            {
                return ACObject == null ? null : ACObject.ACType as ACClass;
            }
        }
#endregion

#region IVBDataCheckbox Member
        /// <summary>
        /// Gets or sets the data content check box.
        /// </summary>
        /// <value>The data content check box.</value>
        public string DataContentCheckBox
        {
            get;
            set;
        }

        public bool IsEnabled { get; set; }
#endregion

#region Additional members

        public bool IsVisible
        {
            get;
            set;
        }

        private ACFSItemChanges _SelectedItemChanges;
        [ACPropertySelected(999, "ItemChanges")]
        public ACFSItemChanges SelectedItemChanges
        {
            get
            {
                return _SelectedItemChanges;
            }
            set
            {
                if (_SelectedItemChanges != value)
                {
                    _SelectedItemChanges = value;
                    OnPropertyChanged("SelectedItemChanges");
                }
            }
        }

        private List<ACFSItemChanges> _ItemChanges;
        [ACPropertyList(999, "ItemChanges")]
        public List<ACFSItemChanges> ItemChangesList
        {
            get
            {
                if (_ItemChanges == null)
                    _ItemChanges = new List<ACFSItemChanges>();
                return _ItemChanges;
            }
            set
            {
                _ItemChanges = value;
            }
        }
#endregion

#region Recursive mehtods

        /// <summary>
        /// Generating child folder item for URL folder structure (zips)
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="path"></param>
        /// <param name="create"></param>
        /// <returns></returns>
        public ACFSItem GetChildFolderItem(IResources resource, string path, bool create = false)
        {
            ACFSItem childItem = null;
            List<string> members = path.Split('\\').ToList();
            childItem = Items.FirstOrDefault(x => x.ACCaption == members.FirstOrDefault()) as ACFSItem;
            if (childItem == null && create)
            {
                childItem = new ACFSItem(resource, Container, null, members.FirstOrDefault(), ResourceTypeEnum.Folder, ACUrlFS + @"\" + members.FirstOrDefault());
                Add(childItem);
            }
            members.RemoveAt(0);
            if (members.Any())
            {
                childItem = childItem.GetChildFolderItem(resource, string.Join(@"\", members), create);
            }
            return childItem;
        }

        public void CallAction(Action<ACFSItem, object[]> action, params object[] args)
        {
            action(this, args);
            if (Items != null && Items.Any())
            {
                foreach (var item in Items)
                {
                    (item as ACFSItem).CallAction(action, args);
                }
            }
        }

        public bool UpdateDateFail { get; set; }

        public void DeatachAll()
        {
            if (ACObject != null && ACObject is VBEntityObject)
            {
                IACEntityObjectContext context = (ACObject as VBEntityObject).GetObjectContext();
                if (context == null)
                    context = ACObjectContextManager.GetContextFromACUrl(null, ACObject.ACType.ObjectFullType.FullName);
                EntityState objectEntityState = (ACObject as VBEntityObject).EntityState;
                if (context != null)
                {
                    if (objectEntityState != EntityState.Detached)
                    {
                        context.Detach(ACObject);
                    }
                }
            }

            foreach (var item in Items)
            {
                (item as ACFSItem).DeatachAll();
            }
        }

        /// <summary>
        /// Build list of contexts used in tree
        /// </summary>
        /// <param name="inputList"></param>
        public void CollectContext(List<IACEntityObjectContext> inputList)
        {
            if (ACObject != null)
            {
                IACEntityObjectContext context = null;
                if (ACObject is IACEntityObjectContext)
                {
                    context = ACObject as IACEntityObjectContext;
                }
                else if(ACObject is VBEntityObject)
                {
                    context = (ACObject as VBEntityObject).GetObjectContext();
                }
                if (context != null && !inputList.Contains(context))
                    inputList.Add(context);
            }
            foreach (var item in Items)
            {
                (item as ACFSItem).CollectContext(inputList);
            }
        }

        /// <summary>
        /// Recursive properties setup
        /// </summary>
        /// <param name="msgList"></param>
        public void SetupProperties(List<Msg> msgList)
        {
            // @aagincic TODO: place for check UpdateDate in database and update date in imported element - for avoid 
            if (ACObject != null && (XNode != null || OuterIACObject != null))
                ACObjectSerialHelper.SetIACObjectProperties(Resource, (ACObject as VBEntityObject).GetObjectContext(), Container, this, false, msgList);
            foreach (var child in Items)
            {
                ACFSItem childFsItem = child as ACFSItem;
                childFsItem.SetupProperties(msgList);
            }
        }


        /// <summary>
        /// Tree used operation - for hidden list show first as visible - enable tree collapse option
        /// </summary>
        public void ShowFirst()
        {
            int i = 0;
            if (Items != null)
                foreach (var item in Items)
                {
                    if (i == 0)
                        (item as ACFSItem).IsVisible = true;
                    i++;
                    (item as ACFSItem).ShowFirst();
                }
        }

#endregion

#region Operational methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        public void SetItems(List<ACFSItem> items)
        {
            _ACObjectItemList = new ObservableCollection<IACContainerWithItems>(items);
        }

        /// <summary>
        /// Trimming URL
        /// </summary>
        /// <returns></returns>
        public string TrimACUrlFS()
        {
            string filename = ACUrlFS;
            if (!string.IsNullOrEmpty(ACUrlFS))
            {
                filename = filename.Replace("\\Resources\\", "");
                filename = filename.Replace("\\List\\", "");
                filename = filename.Replace("\\ZIP\\", "");
                filename = filename.Replace("\\XML\\", "");
            }
            return filename;
        }

        /// <summary>
        /// Read value for recursive post-load
        /// Loading children after loading parents
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <param name="aCObjectPropertyHandlingTypesEnum"></param>
        /// <returns>KeyValuePair</returns>
        public KeyValuePair<bool, string> ReadPropertyValue(PropertyInfo propertyInfo, ACObjectSerialPropertyHandlingTypesEnum aCObjectPropertyHandlingTypesEnum)
        {
            string propertyValue = null;
            if (XNode != null)
            {
                XElement xProperty = XNode.Element(propertyInfo.Name);
                if (xProperty == null)
                    return new KeyValuePair<bool, string>(false, null);
                propertyValue = ACObjectSerialHelper.ReplaceLegacyNames(xProperty.Value);
            }
            if (OuterIACObject != null)
            {
                object objectValue = propertyInfo.GetValue(OuterIACObject);
                if (objectValue != null)
                    switch (aCObjectPropertyHandlingTypesEnum)
                    {
                        case ACObjectSerialPropertyHandlingTypesEnum.DateTime:
                            propertyValue = (objectValue as DateTime?).Value.ToString("o");
                            break;
                        //case ACObjectPropertyHandlingTypesEnum.ACClassDesignByte:
                        //    break;
                        case ACObjectSerialPropertyHandlingTypesEnum.IACObject:
                            propertyValue = (objectValue as IACObject).GetACUrl();
                            break;
                        default:
                            propertyValue = objectValue.ToString();
                            break;
                    }
            }

            return new KeyValuePair<bool, string>(true, propertyValue);
        }

#endregion

        public override string ToString()
        {
            string ts = "[";
            ts += ResourceType.ToString();
            ts += "]";
            if (ACObject != null)
                ts += ACObjectACUrl;
            else if (!ACUrlFS.Contains("\\"))
                ts += ACUrlFS;
            else
                ts += ACUrlFS.Substring(ACUrlFS.LastIndexOf("\\"));
            return ts;
        }

    }

    public enum ACFSItemEntityOperation
    {
        None,
        Attach,
        Deattach
    }

}
