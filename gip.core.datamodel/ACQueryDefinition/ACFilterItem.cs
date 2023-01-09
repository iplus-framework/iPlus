// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ACFilterItem.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Transactions;
using System.Reflection;
using System.ComponentModel;
using Microsoft.Data.SqlClient;

namespace gip.core.datamodel
{
    /// <summary>
    /// ACFilterItem corresponds to a filter line in the ACQueryDefinition.ACFilterColumns list.
    /// </summary>
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACFilterItem'}de{'ACFilterItem'}", Global.ACKinds.TACSimpleClass, Global.ACStorableTypes.NotStorable, true, false)]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + "ACFilterItem", "en{'ACFilterItem'}de{'ACFilterItem'}", typeof(ACFilterItem), "ACFilterItem", "PropertyName", "PropertyName")]
    public class ACFilterItem : IACObject, INotifyPropertyChanged, ICloneable, IComparable
    {
        #region c´tors
        /// <summary>
        /// Initializes a new instance of the <see cref="ACFilterItem"/> class.
        /// </summary>
        public ACFilterItem()
        {
            FilterType = Global.FilterTypes.filter;
            LogicalOperator = Global.LogicalOperators.contains;
            Operator = Global.Operators.or;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ACFilterItem"/> class.
        /// </summary>
        /// <param name="filterType">Type of the filter.</param>
        /// <param name="logicalConnective">Th logical connective</param>
        public ACFilterItem(Global.FilterTypes filterType, Global.Operators logicalConnective)
        {
            //FilterItemID = Guid.NewGuid();
            FilterType = filterType;
            LogicalOperator = Global.LogicalOperators.none;
            Operator = logicalConnective;
            UsedInGlobalSearch = false;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ACFilterItem"/> class.
        /// </summary>
        /// <param name="filterType">Type of the filter.</param>
        /// <param name="properyName">Name of the propery.</param>
        /// <param name="relationalOperator">The relational operator</param>
        /// <param name="logicalConnective">The logical connective.</param>
        /// <param name="searchWord">The search word.</param>
        /// <param name="isConfiguration">if set to <c>true</c> [is configuration].</param>
        /// <param name="usedInGlobalSearch">usedInGlobalSearch</param>
        public ACFilterItem(Global.FilterTypes filterType, string properyName, Global.LogicalOperators relationalOperator, Global.Operators logicalConnective, string searchWord, bool isConfiguration, bool usedInGlobalSearch = false)
        {
            FilterType = filterType;
            PropertyName = properyName;
            LogicalOperator = relationalOperator;
            Operator = logicalConnective;
            SearchWord = searchWord;
            IsConfiguration = isConfiguration;
            UsedInGlobalSearch = usedInGlobalSearch;
        }
        #endregion

        #region Properties

        #region Serializable

        /// <summary>
        /// The _ filter type
        /// </summary>
        Global.FilterTypes _FilterType;
        /// <summary>
        /// Describes whether the FilterItem is is a search condition or a opening bracket or a clsoing bracket.
        /// </summary>
        /// <value>Global.FilterTypes</value>
        [DataMember]
        [ACPropertyInfo(1,"", "en{'Filtertype'}de{'Filterart'}")]
        public Global.FilterTypes FilterType
        {
            get
            {
                return _FilterType;
            }
            set
            {
                _FilterType = value;
                OnPropertyChanged("FilterType");
            }
        }

        private string _PropertyName;
        /// <summary>
        /// Relative path (separated by a dot) to the property (or table field) to be filtered for.
        /// </summary>
        /// <value>The name of the property.</value>
        [DataMember]
        [ACPropertyInfo(2, "", "en{'Field'}de{'Feld'}")]
        public string PropertyName
        {
            get
            {
                return _PropertyName;
            }

            set
            {
                _PropertyName = value;
                OnPropertyChanged("PropertyName");
            }
        }


        private Global.LogicalOperators _LogicalOperator;
        /// <summary>
        /// Relational operators and logical operators (IS NOT NULL, IS NULL) 
        /// </summary>
        /// <value>Relational operator</value>
        [DataMember]
        [ACPropertyInfo(3, "", "en{'Relational operator'}de{'Vergleichsoperator'}")]
        public Global.LogicalOperators LogicalOperator
        {
            get
            {
                return _LogicalOperator;
            }

            set
            {
                _LogicalOperator = value;
                OnPropertyChanged("LogicalOperator");
            }
        }

        private Global.Operators _Operator;
        /// <summary>
        /// Logical connective AND / OR
        /// </summary>
        /// <value>Logical connective</value>
        [DataMember]
        [ACPropertyInfo(5, "", "en{'Logical connective'}de{'Logische Verknüpfung'}")]
        public Global.Operators Operator
        {
            get
            {
                return _Operator;
            }

            set
            {
                _Operator = value;
                OnPropertyChanged("Operator");
            }
        }


        private string _SearchWord;
        /// <summary>
        ///   <para>
        /// String that should be used for filtering. Use the following methods to set or read the filter value: </para>
        ///   <para>T GetSearchValue&lt;T&gt;() </para>
        ///   <para>void SetSearchValue&lt;T&gt;(T value) </para>
        ///   <para>T specifies the data types of the property referred to by "PropertyName". </para>
        /// </summary>
        /// <value>The search word.</value>
        [DataMember]
        [ACPropertyInfo(4, "", "en{'Filter'}de{'Filter'}")]
        public string SearchWord
        {
            get
            {
                return _SearchWord;
            }

            set
            {
                _SearchWord = value;
                OnPropertyChanged("SearchWord");
            }
        }

        private bool? _UsedInGlobalSearch;
        /// <summary>Indicator whether the global search word (gip.core.datamodel.ACQueryDefinition.SearchWord) should be used as a search word in this filter line.</summary>
        /// <value>
        ///   <c>true</c> if [used in global search]; otherwise, <c>false</c>.</value>
        [DataMember(Name = "UIGS")]
        [ACPropertyInfo(7, "", "en{'Used for global search word'}de{'Wird für globales Suchwort verwendet'}")]
        public bool UsedInGlobalSearch
        {
            get
            {
                if (!_UsedInGlobalSearch.HasValue)
                    return true;
                return _UsedInGlobalSearch.Value;
            }

            set
            {
                _UsedInGlobalSearch = value;
                OnPropertyChanged("UsedInGlobalSearch");
            }
        }

        /// <summary>
        /// Converts the string of the SearchWord-Property to the requested Type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetSearchValue<T>()
        {
            return (T) ACConvert.ChangeType(SearchWord, typeof(T), false, Database.GlobalDatabase);
        }

        /// <summary>
        /// Converts the passed value T to string and sets the SearchWord-Property
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        public void SetSearchValue<T>(T value)
        {
            SearchWord = (string) ACConvert.ChangeType(value, typeof(string), false, Database.GlobalDatabase);
        }


        private bool _IsConfiguration;
        /// <summary>
        /// Indicator whether the filter line was created from the program code. If "False", the user created the fit line using the GUI.
        /// </summary>
        /// <value><c>true</c> if this instance is configuration; otherwise, <c>false</c>.</value>
        [DataMember]
        [ACPropertyInfo(6, "", "en{'Configuration'}de{'Konfiguration'}")]
        public bool IsConfiguration
        {
            get
            {
                return _IsConfiguration;
            }

            set
            {
                _IsConfiguration = value;
                OnPropertyChanged("IsConfiguration");
            }
        }
        #endregion

        #region IACObject Member
        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        public string ACCaption
        {
            get { return PropertyName; }
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
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        public IACObject ParentACObject
        {
            get { return null; }
        }

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public string ACIdentifier
        {
            get { return PropertyName; }
        }
        #endregion

        #region Static

        /// <summary>
        /// Gets the key AC identifier.
        /// </summary>
        /// <value>The key AC identifier.</value>
        static public string KeyACIdentifier
        {
            get
            {
                return "PropertyName";
            }
        }

        #endregion

        #endregion

        #region Methods

        #region Clone
        public virtual void CopyFrom(ACFilterItem from)
        {
            if (from == null)
                return;
            this.FilterType = from.FilterType;
            this.PropertyName = from.PropertyName;
            this.LogicalOperator = from.LogicalOperator;
            this.SearchWord = from.SearchWord;
            this.Operator = from.Operator;
            this.IsConfiguration = from.IsConfiguration;
            this._UsedInGlobalSearch = from._UsedInGlobalSearch;
        }

        public virtual object Clone()
        {
            ACFilterItem clone = new ACFilterItem();
            clone.CopyFrom(this);
            return clone;
        }
        #endregion

#region ObjectParameter
        public ObjectParameter GetValueAsObjParameter(Type typeOfACQueryDef, string globalSearchWord, string parameterName)
        {
            if (LogicalOperator == Global.LogicalOperators.isNull || LogicalOperator == Global.LogicalOperators.isNotNull)
                return null;
            if (this.FilterType != Global.FilterTypes.filter)
                return null;
            if (!UsedInGlobalSearch && string.IsNullOrEmpty(SearchWord))
                return null;
            string searchWord = string.IsNullOrEmpty(SearchWord) ? globalSearchWord : SearchWord;
            if (String.IsNullOrEmpty(searchWord))
                return null;

            Type t = typeOfACQueryDef;
            PropertyInfo pi = null;
            string[] memberList = PropertyName.Split('\\');
            if (memberList.Count() > 1)
            {
                for (int i = 0; i < memberList.Count(); i++)
                {
                    pi = t.GetProperty(memberList[i]);
                    if (pi == null)
                        return null;
                    t = pi.PropertyType;
                }
            }
            else
            {
                pi = t.GetProperty(PropertyName);
            }
            if (pi == null)
                return null;


            t = pi.PropertyType;
            if (pi.PropertyType.IsGenericType)
                t = pi.PropertyType.GetGenericArguments()[0];

            ObjectParameter parameter = new ObjectParameter(parameterName, ACConvert.ChangeType(searchWord, t, false, Database.GlobalDatabase));
            return parameter;

        }
#endregion

#region Static
        /// <summary>
        /// Creates the filter item open.
        /// </summary>
        /// <param name="isConfiguration">if set to <c>true</c> [is configuration].</param>
        /// <returns>ACFilterItem.</returns>
        public static ACFilterItem CreateFilterItemOpen(bool isConfiguration)
        {
            return new ACFilterItem() { FilterType = Global.FilterTypes.parenthesisOpen, IsConfiguration = true, LogicalOperator = Global.LogicalOperators.none, Operator = Global.Operators.none, UsedInGlobalSearch = false };
        }

        /// <summary>
        /// Creates the filter item close.
        /// </summary>
        /// <param name="verknuepfungsOperator">The verknuepfungs operator.</param>
        /// <param name="isConfiguration">if set to <c>true</c> [is configuration].</param>
        /// <returns>ACFilterItem.</returns>
        public static ACFilterItem CreateFilterItemClose(Global.Operators verknuepfungsOperator, bool isConfiguration)
        {
            return new ACFilterItem() { FilterType = Global.FilterTypes.parenthesisClose, LogicalOperator = Global.LogicalOperators.none, Operator = verknuepfungsOperator, IsConfiguration = isConfiguration, UsedInGlobalSearch = false };
        }

        /// <summary>
        /// Creates the filter item.
        /// </summary>
        /// <param name="feld">The feld.</param>
        /// <param name="vergleichsOperator">The vergleichs operator.</param>
        /// <param name="verknuepfungsOperator">The verknuepfungs operator.</param>
        /// <param name="defaultFilter">The default filter.</param>
        /// <param name="isConfiguration">if set to <c>true</c> [is configuration].</param>
        /// <returns>ACFilterItem.</returns>
        public static ACFilterItem CreateFilterItem(string feld, Global.LogicalOperators vergleichsOperator, Global.Operators verknuepfungsOperator, string defaultFilter, bool isConfiguration)
        {
            return new ACFilterItem(Global.FilterTypes.filter, feld, vergleichsOperator, verknuepfungsOperator, defaultFilter, isConfiguration);
        }
#endregion

#region IACObject Member
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
        /// Returns a ACUrl relatively to the passed object.
        /// If the passed object is null then the absolute path is returned
        /// </summary>
        /// <param name="rootACObject">Object for creating a realtive path to it</param>
        /// <returns>ACUrl as string</returns>
        public string GetACUrl(IACObject rootACObject = null)
        {
            return ACIdentifier;
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

#endregion

#endregion

#region Event handler
        /// <summary>
        /// Notifies the property changed.
        /// </summary>
        /// <param name="info">The info.</param>
        private void OnPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public int CompareTo(object obj)
        {
            return CompareTo(obj, true);
        }

        public int CompareTo(object obj, bool includeSearchWord)
        {
            ACFilterItem acFilterItem = obj as ACFilterItem;
            if (acFilterItem == null)
                return -1;
            if (this.PropertyName == acFilterItem.PropertyName
                && this.FilterType == acFilterItem.FilterType
                && this.LogicalOperator == acFilterItem.LogicalOperator
                && this.Operator == acFilterItem.Operator
                && (!includeSearchWord || this.SearchWord == acFilterItem.SearchWord)
                && this.IsConfiguration == acFilterItem.IsConfiguration
                && this.UsedInGlobalSearch == acFilterItem.UsedInGlobalSearch)
            {
                return 0;
            }
            return -1;
        }

        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
#endregion

    }
}
