// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 11-08-2012
// ***********************************************************************
// <copyright file="ACMenuItem.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace gip.core.datamodel
{
    /// <summary>
    /// Mit ACMenuItem werden hierarchische Menüstrukturen abgebildet werden. Serialisierbar
    /// Diese Klasse wird für die Verwaltung des Hauptmenüs und für PopUp-Menüs eingesetzt.
    /// </summary>
    [DataContract]
    [ACSerializeableInfo]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACMenuItem'}de{'ACMenuItem'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class ACMenuItem : ACCommand, IComparable
    {
        #region c´tors

        /// <summary>
        /// Initializes a new instance of the <see cref="ACMenuItem"/> class.
        /// </summary>
        /// <param name="parentMenuEntry">The parent menu entry.</param>
        /// <param name="acCaption">The ac caption.</param>
        /// <param name="acUrl">The ac URL.</param>
        /// <param name="sortIndex">Index of the sort.</param>
        /// <param name="parameterList">The parameter list.</param>
        /// <param name="isAutoEnabled">if set to <c>true</c> [is auto enabled].</param>
        /// <param name="handlerACElement">The handler AC element.</param>
        /// <param name="useACCaption">if set to <c>true</c> [use AC caption].</param>
        public ACMenuItem(ACMenuItem parentMenuEntry, string acCaption, string acUrl, Int16 sortIndex, ACValueList parameterList, bool isAutoEnabled = false, IACInteractiveObject handlerACElement = null, bool useACCaption = false)
            : base(acCaption, acUrl, parameterList, isAutoEnabled, handlerACElement)
        {
            ParentMenuEntry = parentMenuEntry;
            _MenuItemList = new ACMenuItemList();
            UseACCaption = useACCaption;
            SortIndex = sortIndex;
        }

        public ACMenuItem(string acCaption, string acUrl, Int16 sortIndex, ACValueList parameterList, string categoryIndex = null, bool isAutoEnabled = false, IACInteractiveObject handlerACElement = null, bool useACCaption = false)
            : base(acCaption, acUrl, parameterList, isAutoEnabled, handlerACElement)
        {
            CategoryIndex = categoryIndex;
            _MenuItemList = new ACMenuItemList();
            UseACCaption = useACCaption;
            SortIndex = sortIndex;
        }
        #endregion

        #region IACUrl Member
        /// <summary>
        /// The _ menu item list
        /// </summary>
        ACMenuItemList _MenuItemList;
        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        /// <value>The items.</value>
        [DataMember]
        [ACPropertyInfo(9999)]
        public ACMenuItemList Items
        {
            get { return _MenuItemList; }
            set { _MenuItemList = value; }
        }

        /// <summary>
        /// The _ use AC caption
        /// </summary>
        bool _UseACCaption = false;
        /// <summary>
        /// Gets or sets a value indicating whether [use AC caption].
        /// </summary>
        /// <value><c>true</c> if [use AC caption]; otherwise, <c>false</c>.</value>
        [DataMember]
        [ACPropertyInfo(9999)]
        public bool UseACCaption
        {
            get
            {
                return _UseACCaption;
            }
            set
            {
                _UseACCaption = value;
                OnPropertyChanged("UseACCaption");
            }
        }

        bool _RibbonOff = false;
        [DataMember]
        [ACPropertyInfo(9999)]
        public bool RibbonOff
        {
            get
            {
                return _RibbonOff;
            }
            set
            {
                _RibbonOff = value;
                OnPropertyChanged("RibbonOff");
            }
        }

        public override bool IsAutoEnabled
        {
            get
            {
                if (Items != null && Items.Any())
                    return true;
                return base.IsAutoEnabled;
            }
        }

        [DataMember]
        public string IconACUrl
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the index of the sort.
        /// </summary>
        /// <value>The index of the sort.</value>
        [DataMember]
        public Int16 SortIndex
        {
            get;
            set;
        }

        [DataMember]
        public string CategoryIndex
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        [ACPropertyInfo(9999)]
        public override IACObject ParentACObject
        {
            get
            {
                return ParentMenuEntry;
            }
        }

        public override IACInteractiveObject HandlerACElement
        {
            get
            {
                return base.HandlerACElement;
            }
            set
            {
                base.HandlerACElement = value;
                if (Items != null)
                    Items.ForEach(c => c.HandlerACElement = value);
            }
        }
        #endregion

        #region IMenuItem Member
        /// <summary>
        /// Gets the AC URL command string.
        /// </summary>
        /// <value>The AC URL command string.</value>
        public string ACUrlCommandString
        {
            get
            {
                if (string.IsNullOrEmpty(GetACUrl()))
                    return "";
                switch (GetACUrl()[0])
                {
                    case ACUrlHelper.Delimiter_InvokeMethod:
                    case ACUrlHelper.Delimiter_DirSeperator:
                    case ACUrlHelper.Delimiter_Start:
                        return GetACUrl();
                    case ACUrlHelper.Delimiter_RelativePath:
                        if (GetACUrl()[1] == ACUrlHelper.Delimiter_RelativePath)
                            return GetACUrl();
                        else
                            return GetACUrl().Substring(1);
                    default:
                        return Const.BusinessobjectsACUrl + ACUrlHelper.Delimiter_Start + GetACUrl();
                }
            }
        }

        public string BSOName
        {
            get
            {
                string acUrl = GetACUrl();
                if (String.IsNullOrEmpty(acUrl))
                    return null;
                string bsoName = acUrl;
                int lastIndex = acUrl.LastIndexOfAny(new char[] { '!', '\\' , '#' });
                if (lastIndex > 0)
                    bsoName = acUrl.Substring(lastIndex + 1);
                return bsoName.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the parent menu entry.
        /// </summary>
        /// <value>The parent menu entry.</value>
        [ACPropertyInfo(9999)]
        public ACMenuItem ParentMenuEntry
        {
            get;
            set;
        }
        #endregion

        #region Others

        /// <summary>
        /// Vergleicht die aktuelle Instanz mit einem anderen Objekt vom selben Typ und gibt eine ganze Zahl zurück, die angibt, ob die aktuelle Instanz in der Sortierreihenfolge vor oder nach dem anderen Objekt oder an derselben Position auftritt.
        /// </summary>
        /// <param name="obj">Ein Objekt, das mit dieser Instanz verglichen werden soll.</param>
        /// <returns>Ein Wert, der die relative Reihenfolge der verglichenen Objekte angibt.Der Rückgabewert hat folgende Bedeutung:Wert Bedeutung Kleiner als 0 Diese Instanz ist kleiner als <paramref name="obj" />. 0 Diese Instanz ist gleich <paramref name="obj" />. Größer als 0 Diese Instanz ist größer als <paramref name="obj" />.</returns>
        public int CompareTo(object obj)
        {
            if (obj == null)
                return -1;
            else if (!(obj is ACMenuItem))
                return -1;
            ACMenuItem objToCompare = obj as ACMenuItem;
            if ((objToCompare.ACUrl != this.ACUrl) 
                || (objToCompare.ACIdentifier != this.ACIdentifier)
                || (objToCompare.ACCaption != this.ACCaption) 
                || (objToCompare.ACType != this.ACType) 
                || (objToCompare.ACUrlCommandString != this.ACUrlCommandString))
                return -1;

            if (objToCompare.ParameterList.Count() != this.ParameterList.Count())
                return -1;

            for (int i = 0; i < objToCompare.ParameterList.Count(); i++)
            {
                if (objToCompare.ParameterList[i].ACIdentifier != this.ParameterList[i].ACIdentifier ||
                    objToCompare.ParameterList[i].Value != this.ParameterList[i].Value)
                    return -1;
            }

            return 0;
        }

        public static ACMenuItemList CreateParentACMenuItem(ACClassMethod method, ACMenuItemList acMenuItemList)
        {
            ACMenuItem parentItem = null;
#if !EFCR
            if (method.ContextMenuCategoryIndex != null && method.ContextMenuCategoryIndex != (short)Global.ContextMenuCategory.NoCategory)
            {
                ACValueItem category = Global.ContextMenuCategoryList.FirstOrDefault(c => (short)c.Value == method.ContextMenuCategoryIndex);
                if (category != null)
                {
                    ACMenuItem parentMenuItem = null;
                    if (category.ParentACObject != null && category.ParentACObject is ACValueItem && ((ACValueItem)category.ParentACObject).Value is short)
                    {
                        ACValueItem parentCategory = category.ParentACObject as ACValueItem;
                        parentMenuItem = new ACMenuItem(null, parentCategory.ACCaption, parentCategory.Value.ToString(), parentCategory.SortIndex, null, true);
                        if (!acMenuItemList.Any(c => c.ACUrl == parentCategory.Value.ToString()))
                        {
                            parentMenuItem = new ACMenuItem(null, parentCategory.ACCaption, parentCategory.Value.ToString(), parentCategory.SortIndex, null, true);
                            parentMenuItem.IconACUrl = Global.GetCategoryIconACUrl((Global.ContextMenuCategory)Enum.Parse(typeof(Global.ContextMenuCategory), parentMenuItem.ACUrl));
                            acMenuItemList.Add(parentMenuItem);
                        }
                        else
                            parentMenuItem = acMenuItemList.FirstOrDefault(c => c.ACUrl == parentCategory.Value.ToString());
                    }
                    if (!acMenuItemList.Any(c => c.ACUrl == category.Value.ToString()))
                    {
                        string parentACUrl = null;
                        if (parentMenuItem != null)
                            parentACUrl = parentMenuItem.ACUrl;

                        parentItem = new ACMenuItem(category.ACCaption, category.Value.ToString(), category.SortIndex, null, parentACUrl, true);
                        parentItem.IconACUrl = Global.GetCategoryIconACUrl((Global.ContextMenuCategory)Enum.Parse(typeof(Global.ContextMenuCategory), parentItem.ACUrl));
                        acMenuItemList.Add(parentItem);
                    }
                    else
                        parentItem = acMenuItemList.FirstOrDefault(c => c.ACUrl == category.Value.ToString());
                }
            }
#endif
            return acMenuItemList;
        }

#endregion

#region Overrides
        public override string ToString()
        {
            return string.Format(@"{0}[{1}]", ACUrl, ACIdentifier);
        }
#endregion
    }
}
