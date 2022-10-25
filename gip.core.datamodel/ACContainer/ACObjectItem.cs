// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ACObjectItem.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Transactions;
using System.Runtime.CompilerServices;

namespace gip.core.datamodel
{
    /// <summary>
    /// Container mit Items für IACObjects. Serialisierbar
    /// Verwendung:
    /// -Baum mit Steuerelementen, Properties, Methods und Produktionsstufen beim Designer
    /// -Bei der Berarbeitung von ACQuery´s
    /// -Bearbeitung von Beziehungen ACClassPropertyRelation im BSOiPlusStudio
    /// </summary>
    #if !EFCR
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACObjectItem'}de{'ACObjectItem'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    // 1 ACCaption
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + "ACObjectItem", "en{'ACObjectItem'}de{'ACObjectItem'}", typeof(ACObjectItem), "ACObjectItem", Const.ACCaptionPrefix, Const.ACCaptionPrefix)]
    public class ACObjectItem : IACContainerWithItemsT<ACObjectItem, IACObject>, IACObject, INotifyPropertyChanged
    {
        #region c´tors
        /// <summary>
        /// Initializes a new instance of the <see cref="ACObjectItem"/> class.
        /// </summary>
        /// <param name="caption">The caption.</param>
        /// <param name="acIdentifier">The ac identifier.</param>
        public ACObjectItem(string caption, string acIdentifier = null)
        {
            ACCaption = caption;
            _ACIdentifier = acIdentifier;
            _ACObjectItemList = new ACObjectItemList();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ACObjectItem"/> class.
        /// </summary>
        /// <param name="acObject">The ac object.</param>
        /// <param name="caption">The caption.</param>
        /// <param name="acURLRelative">The ac URL relative.</param>
        public ACObjectItem(IACObject acObject, string caption = null, string acURLRelative = "")
        {
            ACObject = acObject;
            ACCaption = caption;
            ACUrlRelative = acURLRelative;
            _ACObjectItemList = new ACObjectItemList();
        }
        #endregion

        #region BSO->ACProperty
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
            get { return _ACObject; }
            set { _ACObject = value; }
        }

        #region IACContainerWithItems
        /// <summary>Adds the specified child-container</summary>
        /// <param name="child">The child-container</param>
        public void Add(IACContainerWithItems child)
        {
            ACObjectItem acObjectItem = child as ACObjectItem;
            if (acObjectItem != null)
            {
                Add(acObjectItem);
                acObjectItem.ParentACObject = acObjectItem;
            }
        }

        /// <summary>Removes the specified child-container</summary>
        /// <param name="child">The child-container</param>
        /// <returns>true if removed</returns>
        public bool Remove(IACContainerWithItems child)
        {
            return _ACObjectItemList.Remove(child as ACObjectItem);
        }

        /// <summary>Container-Childs</summary>
        /// <value>Container-Childs</value>
        [DataMember]
        public IEnumerable<IACContainerWithItems> Items
        {
            get 
            {
                return _ACObjectItemList.ToArray();
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

        /// <summary>Container-Childs</summary>
        /// <value>Container-Childs</value>
        public IEnumerable<ACObjectItem> ItemsT
        {
            get
            {
                return _ACObjectItemList.ToArray();
            }
        }

        /// <summary>Visible Container-Childs</summary>
        /// <value>Visible Container-Childs</value>
        public IEnumerable<ACObjectItem> VisibleItemsT
        {
            get
            {
                return ItemsT;
            }
        }

        /// <summary>Gets the parent container T.</summary>
        /// <value>The parent container T.</value>
        public ACObjectItem ParentContainerT
        {
            get
            {
                return ParentContainer as ACObjectItem;
            }
        }

        /// <summary>Gets the root container T.</summary>
        /// <value>The root container T.</value>
        public ACObjectItem RootContainerT
        {
            get
            {
                return RootContainer as ACObjectItem;
            }
        }

        private bool _IsVisible = true;
        public bool IsVisible
        {
            get
            {
                return _IsVisible;
            }
            set
            {
                _IsVisible = value;
            }
        }


        /// <summary>Adds the specified child-container</summary>
        /// <param name="child">The child-container</param>
        public void Add(ACObjectItem child)
        {
            if (child == null)
                return;
            _ACObjectItemList.Add(child);
        }


        /// <summary>Removes the specified child-container</summary>
        /// <param name="child">The child-container</param>
        /// <returns>true if removed</returns>
        public bool Remove(ACObjectItem child)
        {
            if (child == null)
                return false;
            return _ACObjectItemList.Remove(child);
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
                ACObject = value as IACObject;
            }
        }

        public IACObject ValueT
        {
            get
            {
                return ACObject;
            }
            set
            {
                ACObject = value;
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

        /// <summary>
        /// The _ AC object item list
        /// </summary>
        ACObjectItemList _ACObjectItemList;
        #endregion

        #region IACObjectWithBinding Member
        /// <summary>
        /// Returns a ACUrl relatively to the passed object.
        /// If the passed object is null then the absolute path is returned
        /// </summary>
        /// <param name="rootACObject">Object for creating a realtive path to it</param>
        /// <returns>ACUrl as string</returns>
        [ACMethodInfo("","", 9999)]
        public string GetACUrl(IACObject rootACObject = null)
        {
            return ACIdentifier;
        }

        string _ACCaption;
        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [ACPropertyInfo(1)]
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
            set { _ACCaption = value; }
        }

        /// <summary>
        /// The _ AC URL relative
        /// </summary>
        string _ACUrlRelative;
        /// <summary>
        /// Gets or sets the AC URL relative.
        /// </summary>
        /// <value>The AC URL relative.</value>
        public string ACUrlRelative
        {
            get
            {
                return _ACUrlRelative;
            }
            set
            {
                _ACUrlRelative = value;
            }
        }

        private string _acurl;
        [ACPropertyInfo(1)]
        public string ACUrl
        {
            get
            {
                return _acurl;
            }
            set
            {
                _acurl = value;
            }
        }

        /// <summary>
        /// The _ AC identifier
        /// </summary>
        string _ACIdentifier;
        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        [ACPropertyInfo(9999)]
        public string ACIdentifier
        {
            get
            {
                if (!string.IsNullOrEmpty(_ACIdentifier))
                    return _ACIdentifier;
                if (_ACObject != null)
                {
                    return _ACObject.ACIdentifier;
                }
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
#if !EFCR
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
#endif
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
            get; set;
        }

        /// <summary>
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
#if !EFCR
        public IEnumerable<IACObject> ACContentList
        {
            get
            {
                return this.ReflectGetACContentList();
            }
        }
#endif
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
        }
        #endregion

    }

#endif
}
