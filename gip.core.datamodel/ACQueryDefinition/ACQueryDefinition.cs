// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 01-15-2013
// ***********************************************************************
// <copyright file="ACQueryDefinition.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;

namespace gip.core.datamodel
{
    /// <summary>
    /// The ACQueryDefinition is a serializable and persistable class for building and storing user defined queries. 
    /// A ACQueryDefinition can be manipulated on the GUI by changing the entries in the ACFilterColumns- and ACSortColumns-Collection.
    /// The result is a Entity SQL-Statement that can be read in the EntitySQL-Property.
    /// </summary>
    /// <seealso cref="gip.core.datamodel.ACGenericObject" />
    /// <seealso cref="gip.core.datamodel.IVBDataCheckbox" />
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    /// <seealso cref="System.ICloneable" />
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Querydefinition'}de{'Abfragedefinition'}", Global.ACKinds.TACQRY, Global.ACStorableTypes.NotStorable, true, false)]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + ACQueryDefinition.ClassName, "en{'Querydefinition'}de{'Abfragedefinition'}", typeof(ACQueryDefinition), "ACObjectChilds", Const.ACIdentifierPrefix, Const.ACIdentifierPrefix)]
    [ACClassConstructorInfo(
        new object[]
        {
            new object[] {Const.PN_LocalConfigACUrl, Global.ParamOption.Optional, typeof(String) },
            new object[] {"IsLoadConfig", Global.ParamOption.Optional, typeof(Boolean) }
        }
    )]
    [DataContract]
    public class ACQueryDefinition : ACGenericObject, IVBDataCheckbox, INotifyPropertyChanged, ICloneable
    {
        public const string ClassName = "ACQueryDefinition";
        public const string ACSortColumnsPropName = "ACSortColumns";
        public const string ACFilterColumnsPropName = "ACFilterColumns";
        public const string IsUsedPropName = "IsUsed";
        public const string TakeCountPropName = "TakeCount";
        public const string ACColumnsPropName = "ACColumns";
        public const string ACQueryDefinitionChildsPropName = "ACQueryDefinitionChilds";

        #region c´tors

        internal static ACQueryDefinition CreateRootQueryDefinition(ACClass acClassQRYACQueryDefinition)
        {
            ACQueryDefinition qryACQueryDefinition = ACActivator.CreateInstance(acClassQRYACQueryDefinition, acClassQRYACQueryDefinition, null, null) as ACQueryDefinition;
            List<ACColumnItem> acColumns = new List<ACColumnItem>();
            acColumns.Add(new ACColumnItem(Const.ACQueryTypePrefix));
            acColumns.Add(new ACColumnItem(Const.ACQueryRootObjectPrefix));
            acColumns.Add(new ACColumnItem(Const.ChildACUrlPrefix));
            acColumns.Add(new ACColumnItem(ACQueryDefinition.IsUsedPropName));
            acColumns.Add(new ACColumnItem(ACQueryDefinition.TakeCountPropName));
            acColumns.Add(new ACColumnItem(ACQueryDefinition.ACFilterColumnsPropName));
            acColumns.Add(new ACColumnItem(ACQueryDefinition.ACFilterColumnsPropName));
            acColumns.Add(new ACColumnItem(ACQueryDefinition.ACSortColumnsPropName));
            acColumns.Add(new ACColumnItem(ACQueryDefinition.ACQueryDefinitionChildsPropName));
            qryACQueryDefinition.ACColumns = acColumns;
            return qryACQueryDefinition;
        }

        /// <summary>Initializes a new instance of the <see cref="ACQueryDefinition" /> class. Don't use this constructor. 
        /// Use Root.Queries.CreateQuerybyClass()-Method to create a new instance!</summary>
        /// <param name="acType">iPlus-Type-Information for constructing this ACQueryDefinition</param>
        /// <param name="content">Unused. Pass acType.</param>
        /// <param name="parentACObject">The parent ACQueryDefinition. If this ACQueryDefinition is the root then pass a reference to the ACComponent that owns or creates this ACQueryDefinition.</param>
        /// <param name="parameter">Optional: List with "IsLoadConfig" and "LocalConfigACUrl"</param>
        /// <param name="acIdentifier">ACIdentifier of this instance</param>
        public ACQueryDefinition(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "") :
            base(acType, content, parentACObject, parameter, acIdentifier)
        {
            this._ParentACObject = parentACObject;
            IsEnabled = true;
            _content = content;
            if (parameter != null)
            {
                KeyForLocalConfigACUrl = parameter[Const.PN_LocalConfigACUrl] as string;
                if (parameter["IsLoadConfig"] is Boolean)
                    IsLoadConfig = (Boolean)parameter["IsLoadConfig"];
                else
                    IsLoadConfig = parentACObject == null;
            }
            else
            {
                IsLoadConfig = parentACObject == null;
            }

            if (_DefaultTakeCount < 0)
            {
                ACClass typeAsACClass = TypeAsACClass;
                if (typeAsACClass != null)
                {
                    ACClassProperty propTakeCount = typeAsACClass.GetProperty(TakeCountPropName);
                    if (propTakeCount != null && propTakeCount.Value != null)
                    {
                        if ((propTakeCount.Value is String) && (String.IsNullOrEmpty((String)propTakeCount.Value)))
                            _DefaultTakeCount = 0;
                        else
                            _DefaultTakeCount = (Int32)System.Convert.ChangeType(propTakeCount.Value, typeof(Int32));
                    }
                    else
                        _DefaultTakeCount = 0;
                }
                else
                    _DefaultTakeCount = 0;
            }
            // Default sollten ca. maximal 500 Datensätze holen wegen Performance
            if (TakeCount <= 0 && _DefaultTakeCount > 0)
                TakeCount = _DefaultTakeCount;
        }

        /// <summary>
        /// The ACInit method is called directly after construction. 
        /// You can also overwrite the ACInit method. 
        /// When booting or dynamically reloading ACComponent trees, such as loading a workflow, the trees pass through the "Depth-First" + "Pre-Order" algorithm. 
        /// This means that the generation of child ACComponents is always carried out in depth first and then the next ACComponent at the same recursion depth.<para />
        /// The algorithm is executed in the ACInit method of the ACComponent class. 
        /// Therefore, you must always make the base call. 
        /// This means that as soon as you execute your initialization logic after the basic call, you know that the child components were created and that they are in initialization state.
        /// </summary>
        /// <param name="startChildMode">The persisted start mode from database</param>
        /// <returns><c>true</c> if succeeded, <c>false</c> otherwise</returns>
        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            IsQueryForVBControl = startChildMode == Global.ACStartTypes.None;
            if (!base.ACInit(startChildMode))
                return false;

            var acClassPropertyList = this.TypeACClass.Properties.ToList();

            var acPropertyChildACUrl = acClassPropertyList.Where(c => c.ACIdentifier == Const.ChildACUrlPrefix).First() as ACClassProperty;
            if (acPropertyChildACUrl != null)
            {
                ChildACUrl = acPropertyChildACUrl.Value as string;
            }

            var acPropertyQueryType = acClassPropertyList.Where(c => c.ACIdentifier == Const.ACQueryTypePrefix).First() as ACClassProperty;
            if (acPropertyQueryType != null)
            {
                QueryType = this.TypeACClass.GetObjectContext<Database>().ACUrlCommand((acPropertyQueryType.Value as string).Substring(9)) as ACClass;
            }

            var acPropertyRootObject = acClassPropertyList.Where(c => c.ACIdentifier == Const.ACQueryRootObjectPrefix).First() as ACClassProperty;
            if (acPropertyRootObject != null)
            {
                RootObject = TypeAnalyser.GetTypeInAssembly(acPropertyRootObject.Value as string);
            }

            IsUsed = true;

            // Konfiguration wird nur für die oberste ACQueryDefinition verwaltet
            if (!(ParentACObject is ACQueryDefinition) && !string.IsNullOrEmpty(KeyForLocalConfigACUrl) && IsLoadConfig)
            {
                // Compterbezogene Konfiguration laden
                if (!LoadConfig(Global.ConfigSaveModes.Computer))
                {
                    // Benutzerbezogene Konfiguration laden
                    if (!LoadConfig(Global.ConfigSaveModes.User))
                    {
                        // Systemweite Konfiguration laden
                        if (!LoadConfig(Global.ConfigSaveModes.Common))
                        {
                            // Systemweite Konfiguration speichern, falls nicht vorhanden
                            SaveConfig(true, Global.ConfigSaveModes.Common);
                            _RestoredMode = Global.ConfigSaveModes.Common;
                        }
                        else
                            _RestoredMode = Global.ConfigSaveModes.Common;
                    }
                    else
                        _RestoredMode = Global.ConfigSaveModes.User;
                }
                else
                    _RestoredMode = Global.ConfigSaveModes.Computer;
            }

            ResetQueryStrings();

            return true;
        }

        #endregion

        #region Properties

        #region Serializable Properties

        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        [DataMember]
        [ACPropertyInfo(1, "", "en{'Querytype'}de{'Abfragetyp'}")]
        public ACClass QueryType
        {
            get;
            set;
        }


        /// <summary>
        /// Deprecated and unsued.
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        [DataMember]
        [ACPropertyInfo(2, "", "en{'Root Object'}de{'Stammobjekt'}")]
        public Type RootObject
        {
            get;
            set;
        }

        /// <summary>
        /// If this ACQueryDefinition is used to query a child relationship, then the name of the navigation property (EntityCollection {T}, EdmRelationshipNavigationPropertyAttribute) must be specified here, which is declared in the parent entity class.
        /// </summary>
        /// <value>The child AC URL.</value>
        [DataMember]
        [ACPropertyInfo(3, "", "en{'Child ACUrl'}de{'Kind ACUrl'}")]
        public String ChildACUrl
        {
            get;
            set;
        }


        bool _IsUsed;
        /// <summary>
        /// Deprecated and unsued.
        /// </summary>
        /// <value><c>true</c> if this instance is used; otherwise, <c>false</c>.</value>
        [DataMember]
        [ACPropertyInfo(4, "", "en{'Used'}de{'Verwendet'}")]
        public bool IsUsed
        {
            get
            {
                return _IsUsed;
            }
            set
            {
                if (value != this._IsUsed)
                {
                    _IsUsed = value;
                    OnPropertyChanged("IsUsed");
                }
            }
        }


        string _SearchWord;
        /// <summary>
        /// Global search word that is displayed in the ribbon bar and at the top of the query dialog.
        /// </summary>
        /// <value>The global search word.</value>
        [DataMember]
        [ACPropertyInfo(5, "", "en{'Filter'}de{'Filter'}")]
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
                ResetQueryStrings();
            }
        }

        public const Int32 C_DefaultTakeCount = 500;
        private static Int32 _DefaultTakeCount = -1;
        public static Int32 DefaultTakeCount
        {
            get
            {
                return _DefaultTakeCount;
            }
        }

        int _TakeCount;
        /// <summary>
        /// Limits the rows returned in a query result. This value is passed to the Take-Extension-Method and converted to the TOP-clause in SQL.
        /// </summary>
        /// <value>Count of records</value>
        [DataMember]
        [ACPropertyInfo(9, "", "en{'Limit Record Count'}de{'Limit Anzahl Datensätze'}")]
        public int TakeCount
        {
            get
            {
                return _TakeCount;
            }
            set
            {
                _TakeCount = value;
                OnPropertyChanged("TakeCount");
            }
        }


        /// <summary>
        /// List of ACQueryDefinition's that a in a child-relationship to this ACQueryDefinition
        /// </summary>
        /// <value>ACQueryDefinitions for child-relationship</value>
        [DataMember]
        [ACPropertyInfo(8, "", "en{'Queries for child-relationships'}de{'Anfragen für Kind-Beziehungen'}")]
        public IEnumerable<ACQueryDefinition> ACQueryDefinitionChilds
        {
            get
            {
                return ACObjectChilds.Where(c => c is ACQueryDefinition).Select(c => c as ACQueryDefinition);
            }
        }

        #region unnecessary
        //public string ConfigXML
        //{
        //    get
        //    {
        //        return ACSerializer.Serialize(this, Database._QRYACQueryDefinition);
        //    }
        //}

        //static public string KeyACIdentifier
        //{
        //    get { return Const.ACIdentifierPrefix; }
        //}

        //public object GetValue(object acObject, string acUrl)
        //{
        //    try
        //    {
        //        if (acObject == null)
        //            return null;
        //        return acObject.GetValue(acUrl);
        //    }
        //    catch (Exception e)
        //    {
        //        string msg = e.Message;
        //        if (e.InnerException != null && e.InnerException.Message != null)
        //            msg += " Inner:" + e.InnerException.Message;

        //        if (Database.Root != null && Database.Root.Messages != null)
        //            Database.Root.Messages.LogException("ACQueryDefinition", "GetValue", msg);

        //        return "error";
        //    }
        //}

        //public ACQueryDefinition GetACQueryDefinitionByType(Type queryType)
        //{
        //    if (this.QueryType.ObjectType == queryType)
        //        return this;

        //    foreach (var acQueryDefinition in ACQueryDefinitionChilds)
        //    {
        //        var result = acQueryDefinition.GetACQueryDefinitionByType(queryType);
        //        if (result != null)
        //            return result;
        //    }
        //    return null;
        //}
        #endregion

        List<ACColumnItem> _ACColumns = null;
        /// <summary>
        /// Available columns that are displayed in the query dialog on the left.
        /// </summary>
        /// <value>List of ACColumnItem</value>
        [DataMember]
        [ACPropertyInfo(6, "", "en{'Columns'}de{'Spalten'}")]
        public List<ACColumnItem> ACColumns
        {
            get
            {
                if (_ACColumns == null)
                {
                    InitACColumns();
                }

                return _ACColumns;
            }
            set
            {
                _ACColumns = value.ToList();
            }
        }


        [DataMember]
        BindingList<ACFilterItem> _ACFilterColumns = null;
        /// <summary>
        /// Filter list that is displayed in the filter tab in the query dialog.
        /// </summary>
        /// <value>List of ACFilterItem</value>
        [IgnoreDataMember]
        [ACPropertyInfo(7, "", "en{'Filter Columns'}de{'Filterspalten'}")]
        public BindingList<ACFilterItem> ACFilterColumns
        {
            get
            {
                if (_ACFilterColumns == null)
                    InitACFilterColumns();
                if (!_ACFilterColumnsSubscribed)
                {
                    _ACFilterColumns.ListChanged += new ListChangedEventHandler(ACFilterColumns_ListChanged);
                    _ACFilterColumnsSubscribed = true;
                }
                return _ACFilterColumns;
            }
        }


        [DataMember]
        BindingList<ACSortItem> _ACSortColumns = null;
        /// <summary>
        /// Sort list that is displayed in the sort tab in the query dialog.
        /// </summary>
        /// <value>List of ACSortItem</value>
        [IgnoreDataMember]
        [ACPropertyInfo(8, "", "en{'Sorting Columns'}de{'Sortierung'}")]
        public BindingList<ACSortItem> ACSortColumns
        {
            get
            {
                if (_ACSortColumns == null)
                {
                    InitACSortColumns();
                }
                if (!_ACSortColumnsSubscribed)
                {
                    _ACSortColumns.ListChanged += new ListChangedEventHandler(ACSortColumns_ListChanged);
                    _ACSortColumnsSubscribed = true;
                }
                return _ACSortColumns;
            }
        }
        #endregion

        #region Public Properties

        #region Common Properties
        /// <summary>
        /// If this parameter is set to true, the persisted query is read from the database. Otherwise, the filter and sorting conditions remain unchanged.
        /// </summary>
        /// <value>bool</value>
        public bool IsLoadConfig
        {
            get;
            private set;
        }

        /// <summary>
        /// If this parameter is set to true, this query definition will not be serialized and stored to the database (Table ACClassConfig).
        /// </summary>
        public bool SaveToACConfigOff
        {
            get;
            set;
        }


        private string _LocalConfigACUrl;
        /// <summary>
        /// LocalConfigACUrl is the key where the ACQueryDefintion is stored in Table ACClassConfig.
        /// LocalConfigACUrl is build from KeyForLocalConfigACUrl and combined with Global.ConfigSaveModes-Enum.
        /// </summary>
        /// <value>string</value>
        public string LocalConfigACUrl
        {
            get
            {
                return _LocalConfigACUrl;
            }
            private set
            {
                _LocalConfigACUrl = value;
                OnPropertyChanged("LocalConfigACUrl");
            }
        }

        /// <summary>
        /// KeyForLocalConfigACUrl is the prefix for building the LocalConfigACUrl that is used as a key to store the ACQueryDefintion in Table ACClassConfig.
        /// </summary>
        /// <value>string</value>
        public string KeyForLocalConfigACUrl
        {
            get;
            private set;
        }


        private Global.ConfigSaveModes _RestoredMode = Global.ConfigSaveModes.Common;


        bool _IsQueryForVBControl = false;
        /// <summary>
        /// If this Query is created from a VBControl, than the query must be stored with a different LocalConfigACUrl than if it's created from a Business object,
        /// to avoid side effects
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is query for vb control; otherwise, <c>false</c>.
        /// </value>
        public bool IsQueryForVBControl
        {
            get
            {
                return _IsQueryForVBControl;
            }
            set
            {
                _IsQueryForVBControl = value;
                OnPropertyChanged("IsQueryForVBControl");
            }
        }

        private bool _IsLoadMode = false;


        /// <summary>
        /// Gets the root AC query definition.
        /// </summary>
        /// <value>The root AC query definition.</value>
        public ACQueryDefinition RootACQueryDefinition
        {
            get
            {
                if (ParentACObject is ACQueryDefinition)
                    return (ParentACObject as ACQueryDefinition).RootACQueryDefinition;
                return this;
            }
        }


        /// <summary>
        /// Get ACColumns as a comma seperated string
        /// </summary>
        /// <value>The AC columns as string.</value>
        public string ACColumnsAsString
        {
            get
            {
                string columns = "";
                foreach (var acColumn in ACColumns)
                {
                    if (!string.IsNullOrEmpty(columns))
                        columns += ",";
                    columns += acColumn.PropertyName;
                }
                return columns;
            }
        }

        internal IACObject QueryContext
        {
            get;
            set;
        }

        public List<ObjectParameter> FilterParameters
        {
            get
            {
                List<ObjectParameter> collection = new List<ObjectParameter>();
                int index = 0;
                foreach (ACFilterItem filterItem in ACFilterColumns)
                {
                    ObjectParameter parameter = filterItem.GetValueAsObjParameter(this.QueryType.ObjectType, SearchWordForQuery, "p" + index.ToString());
                    if (parameter != null)
                    {
                        collection.Add(parameter);
                        index++;
                    }
                }
                return collection;
            }
        }

        private string _OneTimeSearchWord;
        /// <summary>  Searchword, that is used once. After excecuting the search with ACAccess.OneTimeSearchT() this value will be reset to null.</summary>
        /// <value>The one time search word.</value>
        [IgnoreDataMember]
        public string OneTimeSearchWord
        {
            get
            {
                return _OneTimeSearchWord;
            }
            set
            {
                _OneTimeSearchWord = value;
                OnPropertyChanged("OneTimeSearchWord");
                ResetQueryStrings();
            }
        }

        /// <summary>
        /// Gets the search word for query. If OneTimeSearchWord is not null, than OneTimeSearchWord is returned - else SearchWord;
        /// </summary>
        /// <value>
        /// The search word for query.
        /// </value>
        public string SearchWordForQuery
        {
            get
            {
                if (!String.IsNullOrEmpty(OneTimeSearchWord))
                    return OneTimeSearchWord;
                return SearchWord;
            }
        }

#endregion

#region Static Properties

        static ACClass _ACClassACAccessNav = null;
        private static ACClass ACClassACAccessNav
        {
            get
            {
                if (_ACClassACAccessNav != null)
                    return _ACClassACAccessNav;
                if (_ACClassACAccessNav == null)
                    _ACClassACAccessNav = Database.GlobalDatabase.GetACType(typeof(ACAccessNav<>)) as ACClass;
                return _ACClassACAccessNav;
            }
        }

        static ACClass _ACClassACAccess = null;
        private static ACClass ACClassACAccess
        {
            get
            {
                if (_ACClassACAccess != null)
                    return _ACClassACAccess;
                if (_ACClassACAccess == null)
                    _ACClassACAccess = Database.GlobalDatabase.GetACType(typeof(ACAccess<>)) as ACClass;
                return _ACClassACAccess;
            }
        }

#endregion

#region LINQ-Predicate Properties
        private string _LINQPredicateFilterFromEdit = null;

        /// <summary>LINQ Where-Clause, that can be manipulated in the Query-Dialog by the user</summary>
        /// <value>LINQ-string</value>
        [ACPropertyInfo(9999, "", "en{'Edit LINQ Where-Clause'}de{'Editiere LINQ Where-Anweisung'}")]
        public string LINQPredicateWhere_FromEdit
        {
            get
            {
                return _LINQPredicateFilterFromEdit;
            }
            set
            {
                _LINQPredicateFilterFromEdit = value;
                OnPropertyChanged("LINQPredicateWhere_FromEdit");
            }
        }

        private string _LINQPredicateOrderBy_FromEdit = null;
        /// <summary>LINQ Order by-Clause, that can be manipulated in the Query-Dialog by the user</summary>
        /// <value>LINQ-string</value>
        [ACPropertyInfo(9999, "", "en{'Edit LINQ OrderBy-Clause'}de{'Editiere LINQ OrderBy-Anweisung'}")]
        public string LINQPredicateOrderBy_FromEdit
        {
            get
            {
                return _LINQPredicateOrderBy_FromEdit;
            }
            set
            {
                _LINQPredicateOrderBy_FromEdit = value;
                OnPropertyChanged("LINQPredicateOrderBy_FromEdit");
            }
        }

        private string _LINQPredicateWhere_FromItems = null;
        /// <summary>LINQ Where-Clause, that is generated from ACFilterColumns</summary>
        /// <value>LINQ-string</value>
        [ACPropertyInfo(9999, "", "en{'Generated LINQ Where-Clause'}de{'Generierte LINQ Where-Anweisung'}")]
        public string LINQPredicateWhere_FromItems
        {
            get
            {
                if (_LINQPredicateWhere_FromItems != null)
                    return _LINQPredicateWhere_FromItems;
                RebuildLINQPredicateFromItems();
                return _LINQPredicateWhere_FromItems;
            }
        }

        private string _LINQPredicateOrderBy_FromItems = null;
        /// <summary>LINQ Order by-Clause, that is generated from ACSortColumns</summary>
        /// <value>LINQ-string</value>
        [ACPropertyInfo(9999, "", "en{'Generated LINQ OrderBy-Clause'}de{'Generierte LINQ OrderBy-Anweisung'}")]
        public string LINQPredicateOrderBy_FromItems
        {
            get
            {
                if (_LINQPredicateOrderBy_FromItems != null)
                    return _LINQPredicateOrderBy_FromItems;
                RebuildLINQPredicateFromItems();
                return _LINQPredicateOrderBy_FromItems;
            }
        }

        /// <summary>LINQ Where-Clause, that is used for exceuting query</summary>
        /// <value>LINQ-string</value>
        [ACPropertyInfo(9999, "", "en{'Active LINQ Where-Clause'}de{'Aktive LINQ Where-Anweisung'}")]
        public string LINQPredicateWhere
        {
            get
            {
                if (!String.IsNullOrEmpty(LINQPredicateWhere_FromEdit))
                    return LINQPredicateWhere_FromEdit;
                return LINQPredicateWhere_FromItems;
            }
        }

        /// <summary>LINQ OrderBy-Clause, that is used for exceuting query</summary>
        /// <value>LINQ-string</value>
        [ACPropertyInfo(9999, "", "en{'Active LINQ OrderBy-Clause'}de{'Aktive LINQ OrderBy-Anweisung'}")]
        public string LINQPredicateOrderBy
        {
            get
            {
                if (!String.IsNullOrEmpty(LINQPredicateOrderBy_FromEdit))
                    return LINQPredicateOrderBy_FromEdit;
                return LINQPredicateOrderBy_FromItems;
            }
        }
#endregion

#region Entity-SQL Properties
        public string SelectPartOfEntitySQL
        {
            get
            {
                //string queryString = "SELECT VALUE c FROM " + this.ChildACUrl + " AS c ";
                //ObjectContext context = QueryContext as ObjectContext;
                //if (context != null)
                //    queryString = "SELECT VALUE c FROM " + context.DefaultContainerName + "." + this.ChildACUrl + " AS c ";
                //else
                //    throw new MissingMemberException("QueryContext in ACQueryDefinition is null or ist not a ObjectContext");
                //return queryString;
                if (this.QueryContext == null)
                    return "";
                DbContext context = QueryContext as DbContext;
                if (context == null)
                    return "";
#if !EFCR
                string queryString = "SELECT VALUE c FROM " + context.DefaultContainerName + "." + this.ChildACUrl + " AS c ";
#endif
                string queryString = null;
                return queryString;
            }
        }

        private string _EntitySQL_FromEdit = null;
        /// <summary>Entity-SQL Statemenet, that can be manipulated in the Query-Dialog by the user</summary>
        /// <value>Entity-SQL-string</value>
        [ACPropertyInfo(9999, "", "en{'Edit SQL Statement'}de{'Editiere SQL Anweisung'}")]
        public string EntitySQL_FromEdit
        {
            get
            {
                return _EntitySQL_FromEdit;
            }
            set
            {
                _EntitySQL_FromEdit = value;
                OnPropertyChanged("EntitySQL_FromEdit");
                OnPropertyChanged("EntitySQL");
            }
        }

        private string _EntitySQL_FromItems = null;
        /// <summary>Entity-SQL Where-Clause, that is generated from ACFilterColumns and ACSortColumns</summary>
        /// <value>Entity-SQL-string</value>
        [ACPropertyInfo(9999, "", "en{'Generated SQL Statement'}de{'Generierte SQL Anweisung'}")]
        public string EntitySQL_FromItems
        {
            get
            {
                if (!String.IsNullOrEmpty(_EntitySQL_FromItems) && _EntitySQL_FromItems.IndexOf("SELECT VALUE c FROM") >= 0)
                    return _EntitySQL_FromItems;
                if (String.IsNullOrEmpty(SelectPartOfEntitySQL))
                    return "";
                RebuildEntitySQLFromItems();
                return _EntitySQL_FromItems;
            }
        }

        /// <summary>Entity-SQL statemenet, that is used for exceuting query</summary>
        /// <value>Entity-SQL-string</value>
        [ACPropertyInfo(9999, "", "en{'Active SQL Statement'}de{'Aktive SQL Anweisung'}")]
        public string EntitySQL
        {
            get
            {
                if (!String.IsNullOrEmpty(EntitySQL_FromEdit))
                {
                    if (EntitySQL_FromEdit.StartsWith(SelectPartOfEntitySQL))
                        return EntitySQL_FromEdit;
                    return SelectPartOfEntitySQL + EntitySQL_FromEdit;
                }
                return EntitySQL_FromItems;
            }
        }
#endregion

#endregion

#region Private Properties
        /// <summary>
        /// The _content
        /// </summary>
        private IACObject _content;

#endregion

#region IVBDataCheckbox Member
        /// <summary>
        /// Gets the data content check box.
        /// </summary>
        /// <value>The data content check box.</value>
        public string DataContentCheckBox
        {
            get
            {
                return IsUsedPropName;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is checked.
        /// </summary>
        /// <value><c>true</c> if this instance is checked; otherwise, <c>false</c>.</value>
        public bool IsChecked
        {
            get;
            set;
        }

        public bool IsEnabled { get; set; }

#endregion

#endregion

#region Methods

#region public

#region Filter and Sort Mainpulation
        /// <summary>
        /// Gets the AC columns.
        /// </summary>
        /// <param name="acColumns">The ac columns.</param>
        /// <returns>List{ACColumnItem}.</returns>
        public List<ACColumnItem> GetACColumns(string acColumns)
        {
            if (string.IsNullOrEmpty(acColumns))
                return ACColumns;
            return BuildACColumnsFromVBSource(acColumns);
        }

        public static List<ACColumnItem> BuildACColumnsFromVBSource(string vbSource)
        {
            if (string.IsNullOrEmpty(vbSource))
                return null;
            List<ACColumnItem> acColumnList = new List<ACColumnItem>();
            string[] aACColumns = vbSource.Split(',');
            foreach (var column in aACColumns)
            {
                acColumnList.Add(new ACColumnItem(column));
            }
            return acColumnList;
        }

        /// <summary>
        /// Removes entries from ACFilterColumns
        /// </summary>
        public void ClearFilter(bool bForceDeletePreDefinedFilter = false)
        {
            foreach (var acFilterColumn in ACFilterColumns.ToList())
            {
                if (!acFilterColumn.IsConfiguration || bForceDeletePreDefinedFilter)
                    _ACFilterColumns.Remove(acFilterColumn);
            }
        }


        /// <summary>
        /// Removes entries from ACSortColumns
        /// </summary>
        public void ClearSort(bool bForceDeletePreDefinedSort = false)
        {
            foreach (var acSortColumn in ACSortColumns.ToList())
            {
                if (!acSortColumn.IsConfiguration || bForceDeletePreDefinedSort)
                    _ACSortColumns.Remove(acSortColumn);
            }
        }


        /// <summary>
        /// Direct factory filter columns
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="searchWord"></param>
        public void FactoryACFilterItem(string propertyName, string searchWord)
        {
            ACFilterItem filter = ACFilterColumns.FirstOrDefault(c => c.PropertyName == propertyName);
            if (filter == null)
            {
                filter = new ACFilterItem();
                filter.PropertyName = propertyName;
                ACFilterColumns.Add(filter);
            }
            filter.SearchWord = searchWord;
        }

#endregion

#region Load/Save

        /// <summary>
        /// Loads the stored query from ACClassConfig
        /// </summary>
        /// <param name="acClassConfig">The ac class config.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise</returns>
        public bool LoadConfig(ACClassConfig acClassConfig)
        {
            this.CopyFrom(acClassConfig);
            LocalConfigACUrl = acClassConfig.LocalConfigACUrl;
            return true;
        }


        /// <summary>
        /// Loads the stored query from the database
        /// </summary>
        /// <param name="configSaveMode">ConfigSaveModes controls under which key (LocalConfigACUrl) a ACQueryDefinition is stored in the ACClassConfig-Table.</param>
        /// <param name="configName">Name that is needed if configSaveMode is Configurationname</param>
        /// <returns><c>true</c> if succeeded, <c>false</c> otherwise</returns>
        public bool LoadConfig(Global.ConfigSaveModes configSaveMode, string configName = "")
        {
            if (ParentACObject is ACQueryDefinition)
                return (ParentACObject as ACQueryDefinition).LoadConfig(configSaveMode, configName);

            if (Database._QRYACQueryDefinition == null)
                return true;

            string comment = "";
            string localConfigACUrl = BuildLocalConfigACUrl(out comment, configSaveMode, configName);
            if (String.IsNullOrEmpty(localConfigACUrl))
                return false;

            ACClassConfig config = this.TypeACClass.ConfigurationEntries.Where(c => c.LocalConfigACUrl == localConfigACUrl).FirstOrDefault() as ACClassConfig;
            if (config == null)
                return false;
            return LoadConfig(config);
        }


        /// <summary>
        /// Saves the stored query to the database under the LocalConfigACUrl as it was loaded first.
        /// </summary>
        /// <param name="withFilter">if set to <c>true</c> [with filter].</param>
        /// <param name="configName">Name of the configuration.</param>
        /// <returns></returns>
        public bool SaveConfig(bool withFilter, string configName = "")
        {
            return SaveConfig(withFilter, _RestoredMode, configName);
        }


        /// <summary>
        /// Saves the stored query to the database.
        /// </summary>
        /// <param name="withFilter">If set, then the SearchWords in ACFilterColumens are stored as well</param>
        /// <param name="configSaveMode">ConfigSaveModes controls under which key (LocalConfigACUrl) a ACQueryDefinition is stored in the ACClassConfig-Table.</param>
        /// <param name="configName">Name that is needed if configSaveMode is Configurationname</param>
        /// <returns><c>true</c> if succeeded, <c>false</c> otherwise</returns>
        public bool SaveConfig(bool withFilter, Global.ConfigSaveModes configSaveMode, string configName = "")
        {
            if (ParentACObject is ACQueryDefinition)
            {
                return (ParentACObject as ACQueryDefinition).SaveConfig(withFilter, configSaveMode, configName);
            }

            string comment = "";
            string localConfigACUrl = BuildLocalConfigACUrl(out comment, configSaveMode, configName);
            if (String.IsNullOrEmpty(localConfigACUrl))
                return false;

            ACQueryDefinition copy;
            if (withFilter)
                copy = this;
            else
            {
                copy = this.Clone() as ACQueryDefinition;
                copy.IsLoadConfig = false;
            }

            if (Database._QRYACQueryDefinition != null)
            {
                BuildLocalConfigACUrl(out comment, configSaveMode, configName);
                IACConfig acConfig = this.TypeACClass.ConfigurationEntries.Where(c => c.LocalConfigACUrl == localConfigACUrl).FirstOrDefault();

                if (acConfig == null)
                {
                    acConfig = this.TypeACClass.NewACConfig(null, TypeACClass);
                    acConfig.LocalConfigACUrl = localConfigACUrl;
                }
                acConfig.Value = copy;
                acConfig.Comment = comment;

                LocalConfigACUrl = acConfig.LocalConfigACUrl;

                ((VBEntityObject)acConfig).GetObjectContext().ACSaveChanges();
                return true;
            }
            return false;
        }


        /// <summary>
        /// Builds the the key (LocalConfigACUrl) where this ACQueryDefinition should be stored or loaded.
        /// </summary>
        /// <param name="comment">returns a comment</param>
        /// <param name="configSaveMode">ConfigSaveModes controls under which key (LocalConfigACUrl) a ACQueryDefinition is stored in the ACClassConfig-Table.</param>
        /// <param name="configName">Name that is needed if configSaveMode is Configurationname</param>
        /// <param name="ignoreLastKeyPart">Builds the LocalConfigACUrl without a user- oder computername</param>
        /// <returns></returns>
        public string BuildLocalConfigACUrl(out string comment, Global.ConfigSaveModes configSaveMode, string configName = "", bool ignoreLastKeyPart = false)
        {
            comment = "";
            string localConfigACUrl = KeyForLocalConfigACUrl;
            if (IsQueryForVBControl)
                localConfigACUrl += "_1";
            else
                localConfigACUrl += "_N";

            switch (configSaveMode)
            {
                case Global.ConfigSaveModes.Configurationname:
                    if (string.IsNullOrEmpty(configName))
                        return null;
                    localConfigACUrl += "_N";
                    if (!ignoreLastKeyPart)
                        localConfigACUrl += "_" + configName;
                    comment = configName;
                    break;
                case Global.ConfigSaveModes.User:
                    localConfigACUrl += "_U";
                    if (!ignoreLastKeyPart)
                        localConfigACUrl += "_" + Database.Root.Environment.User.VBUserName;
                    comment = Database.Root.Environment.User.VBUserName;
                    break;
                case Global.ConfigSaveModes.Common:
                    localConfigACUrl += "_S";
                    comment = "Standard";
                    break;
                case Global.ConfigSaveModes.Computer:
                    localConfigACUrl += "_C";
                    if (!ignoreLastKeyPart)
                        localConfigACUrl += "_" + Database.Root.Environment.ComputerName;
                    comment = Database.Root.Environment.ComputerName;
                    break;
            }

            return localConfigACUrl;
        }
#endregion

#region Clone-Methoden

        public virtual object Clone()
        {
            ACQueryDefinition clone = new ACQueryDefinition(this.ACType as ACClass, this._content, this._ParentACObject, null);
            clone.CopyFrom(this, true, true, true);
            return clone;
        }

        public virtual void CopyFrom(ACQueryDefinition from, bool withColumns, bool withFilterColumns, bool withSortColumns)
        {
            if (from == null)
                return;

            //this.ConfigXML = from.ConfigXML;
            this._ACIdentifier = from._ACIdentifier;
            this._ACType = from._ACType;

            // private Properties
            this._content = from._content;
            this.IsChecked = from.IsChecked;
            this._ACFilterColumnsSubscribed = false;
            this._ACSortColumnsSubscribed = false;

            // Local public
            this.IsLoadConfig = from.IsLoadConfig;
            this.LocalConfigACUrl = from.LocalConfigACUrl;
            this.KeyForLocalConfigACUrl = from.KeyForLocalConfigACUrl;
            this._RestoredMode = from._RestoredMode;
            this.IsQueryForVBControl = from.IsQueryForVBControl;
            this._IsLoadMode = from._IsLoadMode;
            this.QueryContext = from.QueryContext;

            // Serializable
            this.QueryType = from.QueryType;
            this.RootObject = from.RootObject;
            this.ChildACUrl = from.ChildACUrl;
            this.IsUsed = from.IsUsed;
            this.SearchWord = from.SearchWord;
            this.TakeCount = from.TakeCount;

            // LINQ/Entity SQL
            this.LINQPredicateWhere_FromEdit = from.LINQPredicateWhere_FromEdit;
            this.LINQPredicateOrderBy_FromEdit = from.LINQPredicateOrderBy_FromEdit;
            this._LINQPredicateWhere_FromItems = from._LINQPredicateWhere_FromItems;
            this._LINQPredicateOrderBy_FromItems = from._LINQPredicateOrderBy_FromItems;
            this.EntitySQL_FromEdit = from.EntitySQL_FromEdit;
            this._EntitySQL_FromItems = from._EntitySQL_FromItems;

            // Lists
            if (withColumns)
                this.ACColumns = from.ACColumns;

            if (withFilterColumns)
            {
                if (this._ACFilterColumns == null)
                    this._ACFilterColumns = new BindingList<ACFilterItem>();

                CopyFilterList(from.ACFilterColumns, this._ACFilterColumns);

                //else
                //    this._ACFilterColumns.Clear();
                //foreach (ACFilterItem filterItem in from.ACFilterColumns)
                //{
                //    this._ACFilterColumns.Add(filterItem.Clone() as ACFilterItem);
                //}
                _ACFilterColumns.ListChanged -= new ListChangedEventHandler(ACFilterColumns_ListChanged);
                _ACFilterColumns.ListChanged += new ListChangedEventHandler(ACFilterColumns_ListChanged);
                _ACFilterColumnsSubscribed = true;
            }

            if (withSortColumns)
            {
                if (this._ACSortColumns == null)
                    this._ACSortColumns = new BindingList<ACSortItem>();

                CopySortList(from.ACSortColumns, this._ACSortColumns);

                _ACSortColumns.ListChanged -= new ListChangedEventHandler(ACSortColumns_ListChanged);
                _ACSortColumns.ListChanged += new ListChangedEventHandler(ACSortColumns_ListChanged);
                _ACSortColumnsSubscribed = true;
            }

            // TODO: Nach Norberts programmierung werden Childs gar nicht serialisiert, Was soll das?
            foreach (var fromChild in from.ACQueryDefinitionChilds)
            {
                ACQueryDefinition toChild = ACQueryDefinitionChilds.Where(c => c.ACIdentifier == fromChild.ACIdentifier).FirstOrDefault();
                if (toChild != null)
                    toChild.CopyFrom(fromChild, withColumns, withFilterColumns, withSortColumns);
                else
                {
                    toChild = fromChild.Clone() as ACQueryDefinition;
                    toChild._ParentACObject = this;
                    (this.ACObjectChilds as List<IACObject>).Add(toChild);
                }
            }
        }

        private void CopyFilterList(BindingList<ACFilterItem> source, BindingList<ACFilterItem> target)
        {
            if (source.Count() == 0)
                target.Clear();
            else
            {
                List<ACFilterItem> removedItems = target.Where(c =>!string.IsNullOrEmpty(c.PropertyName) && !source.Select(x => x.PropertyName).Where(x => !string.IsNullOrEmpty(x)).Contains(c.PropertyName)).ToList();
                foreach (ACFilterItem removedItem in removedItems)
                    target.Remove(removedItem);
                target.Clear();
                foreach (ACFilterItem item in source)
                    target.Add(item);
            }
        }

        private void CopySortList(BindingList<ACSortItem> source, BindingList<ACSortItem> target)
        {
            if (source.Count() == 0)
                target.Clear();
            else
            {
                List<ACSortItem> removedItems = target.Where(c => !source.Select(x => x.PropertyName).Where(x => !string.IsNullOrEmpty(x)).Contains(c.PropertyName)).ToList();
                foreach (ACSortItem removedItem in removedItems)
                    target.Remove(removedItem);
                target.Clear();
                foreach (ACSortItem item in source)
                    target.Add(item);
            }
        }

        public void CopyFrom(IACConfig config)
        {
            _IsLoadMode = true;
            try
            {
                string configXML = "";
                ACPropertyExt propertyExt = config.ACProperties.Properties.FirstOrDefault(x => x.ACIdentifier == Const.Value);
                if (propertyExt != null)
                {
                    configXML = propertyExt.XMLValue;
                    ACSerializer.Deserialize(this, Database._QRYACQueryDefinition, configXML);
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("ACQueryDefinition", "CopyFrom", msg);
            }
            _IsLoadMode = false;
        }

#endregion

#region Query-Builder-Methods (LINQ-Predicate / Entity-SQL)

        /// <summary>
        /// Rebuilds the LINQPredicateWhere_FromItems, LINQPredicateOrderBy_FromItems, LINQPredicateWhere and LINQPredicateOrderBy-Properties
        /// </summary>
        public void RebuildLINQPredicateFromItems()
        {
            _LINQPredicateWhere_FromItems = "";
            _LINQPredicateOrderBy_FromItems = "";
#if !EFCR
            List<ObjectParameter> filterValues = null;
            BuildFilter(ref _LINQPredicateWhere_FromItems, ref filterValues, ref _LINQPredicateOrderBy_FromItems, false);
#endif
            OnPropertyChanged("LINQPredicateWhere_FromItems");
            OnPropertyChanged("LINQPredicateOrderBy_FromItems");
            OnPropertyChanged("LINQPredicateWhere");
            OnPropertyChanged("LINQPredicateOrderBy");
        }

        /// <summary>
        /// Rebuilds the EntitySQL_FromItems and EntitySQL-Properties
        /// </summary>
        public void RebuildEntitySQLFromItems()
        {
            string filter = "";
#if !EFCR
            List<ObjectParameter> filterValues = null;
            string sortOrder = "";
            BuildFilter(ref filter, ref filterValues, ref sortOrder, true);
            int parameterCount = 0;
            if (filterValues != null)
                parameterCount = filterValues.Count;
#endif
            _EntitySQL_FromItems = SelectPartOfEntitySQL;
            if (!string.IsNullOrEmpty(filter))
                _EntitySQL_FromItems += "WHERE " + filter;
#if !EFCR
            _EntitySQL_FromItems += sortOrder;
#endif
            OnPropertyChanged("EntitySQL_FromItems");
            OnPropertyChanged("EntitySQL");
        }

        public void ResetQueryStrings()
        {
            LINQPredicateWhere_FromEdit = null;
            LINQPredicateOrderBy_FromEdit = null;
            _LINQPredicateWhere_FromItems = null;
            OnPropertyChanged("LINQPredicateWhere_FromItems");
            _LINQPredicateOrderBy_FromItems = null;
            OnPropertyChanged("LINQPredicateOrderBy_FromItems");
            OnPropertyChanged("LINQPredicateWhere");
            OnPropertyChanged("LINQPredicateOrderBy");
            _EntitySQL_FromEdit = null;
            _EntitySQL_FromItems = null;
            OnPropertyChanged("EntitySQL_FromItems");
            OnPropertyChanged("EntitySQL");
        }

#if !EFCR
        /// <summary>
        /// Builds a either a LINQ-Statement or a Entity-SQL-Statement from the ACFilterColumns and ACSortColumns
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="filterValues">The filter values.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="forEntitySQL"></param>
        protected void BuildFilter(ref string filter, ref List<ObjectParameter> filterValues, ref string sortOrder, bool forEntitySQL = false)
        {
            sortOrder = GetOrderExpression(forEntitySQL);
            filterValues = new List<ObjectParameter>();
            filter = CreateFilterPart(0, ACFilterColumns.Count() - 1, ref filterValues, forEntitySQL);
        }
#endif


#if !EFCR
        private string CreateFilterPart(int indexFrom, int indexTo, ref List<ObjectParameter> filterValueList, bool forEntitySQL = false)
        {
            string filterPart = "";
            string nextConnector = "";

            for (int i = indexFrom; i < indexTo + 1; i++)
            {
                var filterItem = ACFilterColumns[i];

                switch (filterItem.FilterType)
                {
                    case Global.FilterTypes.parenthesisOpen:
                        // Wenn Klammer auf, dann die dazugehörende Klammer zu suchen und Teilbereich bearbeiten
                        // Das ganze funktioniert rekursiv
                        // v                              v
                        //  v            v    v          v
                        // (( <Ausdruck> ) && (<Ausdruck>))
                        int indexFrom1 = i + 1;
                        int indexTo1 = indexFrom1;
                        int klammerCount = 1;
                        for (int j = indexFrom1; j < indexTo + 1; j++)
                        {
                            var filterItem1 = ACFilterColumns[j];
                            if (filterItem1.FilterType == Global.FilterTypes.parenthesisOpen)
                                klammerCount++;
                            if (filterItem1.FilterType == Global.FilterTypes.parenthesisClose)
                                klammerCount--;
                            if (klammerCount == 0)
                            {
                                indexTo1 = j - 1;
                                break;
                            }
                        }
                        string part = CreateFilterPart(indexFrom1, indexTo1, ref filterValueList, forEntitySQL);
                        if (!string.IsNullOrEmpty(part))
                        {
                            filterPart += nextConnector;
                            filterPart += "(" + part + ")";
                            nextConnector = CreateFilterConnector(filterItem);

                            //if (indexTo1 + 2 < FilterItems.Count()) // Am Ende kein Verknüpfungsoperator
                            //    filterPart += FilterItems[indexTo1 + 1].VerknuepfungsOperator;
                        }
                        i = indexTo1 + 1;
                        break;
                    case Global.FilterTypes.filter:
                        {
                            string filterstring = CreateFilterExpression(filterItem, ref filterValueList, forEntitySQL);
                            if (!string.IsNullOrEmpty(filterstring))
                            {
                                filterPart += nextConnector;
                                filterPart += filterstring;
                                if (i + 1 >= ACFilterColumns.Count()) // Am Ende kein Verknüpfungsoperator
                                    break;
                                if (ACFilterColumns[i + 1].FilterType == Global.FilterTypes.parenthesisClose)
                                    continue;
                                nextConnector = CreateFilterConnector(filterItem);
                            }
                        }
                        break;
                }
            }
            return filterPart;
        }
#endif


#if !EFCR
        /// <summary>
        /// Creates a expression from a fiteritem: [Variable] [Operator] [Wert]
        /// NULL is returned if expression coud not be ctreadted
        /// </summary>
        /// <param name="filterItem">The filter item.</param>
        /// <param name="filterValueList">The filter value list.</param>
        /// <param name="forEntitySQL">The search word.</param>
        /// <returns>System.String.</returns>
        private string CreateFilterExpression(ACFilterItem filterItem, ref List<ObjectParameter> filterValueList, bool forEntitySQL = false)
        {
            string filterExpression;
            string index = filterValueList.Count().ToString();
            ObjectParameter objectParameter = filterItem.GetValueAsObjParameter(QueryType.ObjectType, SearchWordForQuery, "p" + index);
            if (objectParameter == null)
            {
                if (filterItem.LogicalOperator != Global.LogicalOperators.isNull &&
                    filterItem.LogicalOperator != Global.LogicalOperators.isNotNull)
                    return null;
            }

            if (String.IsNullOrEmpty(filterItem.PropertyName))
                return null;

            string[] memberList = filterItem.PropertyName.Split('\\');
            if (memberList.Count() > 1)
            {
                filterExpression = "";
                for (int i = 0; i < memberList.Count(); i++)
                {
                    if (!string.IsNullOrEmpty(filterExpression))
                        filterExpression += ".";
                    filterExpression += memberList[i];
                }
                if (forEntitySQL && !String.IsNullOrEmpty(filterExpression))
                    filterExpression = "c." + filterExpression;
            }
            else
            {
                if (forEntitySQL)
                    filterExpression = "c." + filterItem.PropertyName;
                else
                    filterExpression = filterItem.PropertyName;
            }

            if (filterItem.LogicalOperator == Global.LogicalOperators.contains
                || filterItem.LogicalOperator == Global.LogicalOperators.endsWith
                || filterItem.LogicalOperator == Global.LogicalOperators.startsWith)
            {
                bool likeOperator = false;
                IACType typeOfColumn = QueryType;
                object acSource = null;
                string acPath = "";
                Global.ControlModes acControlMode = Global.ControlModes.Disabled;
                if (QueryType != null && QueryType.ACUrlBinding(filterItem.PropertyName, ref typeOfColumn, ref acSource, ref acPath, ref acControlMode))
                {
                    if (typeOfColumn != null && typeOfColumn.ObjectType != null)
                        likeOperator = typeOfColumn.ObjectType.IsAssignableFrom(typeof(string));
                }

                if (!likeOperator)
                {
                    filterItem.LogicalOperator = Global.LogicalOperators.equal;
                }
            }


            switch (filterItem.LogicalOperator)
            {
                case Global.LogicalOperators.equal:
                    {
                        filterValueList.Add(objectParameter);
                        if (forEntitySQL)
                            filterExpression += " = @p" + index;
                        else
                            filterExpression += " = @" + index;
                        break;
                    }
                case Global.LogicalOperators.greaterThan:
                    {
                        filterValueList.Add(objectParameter);
                        if (forEntitySQL)
                            filterExpression += " > @p" + index;
                        else
                            filterExpression += " > @" + index;
                        break;
                    }
                case Global.LogicalOperators.greaterThanOrEqual:
                    {
                        filterValueList.Add(objectParameter);
                        if (forEntitySQL)
                            filterExpression += " >= @p" + index;
                        else
                            filterExpression += " >= @" + index;
                        break;
                    }
                case Global.LogicalOperators.lessThan:
                    {
                        filterValueList.Add(objectParameter);
                        if (forEntitySQL)
                            filterExpression += " < @p" + index;
                        else
                            filterExpression += " < @" + index;
                        break;
                    }
                case Global.LogicalOperators.lessThanOrEqual:
                    {
                        filterValueList.Add(objectParameter);
                        if (forEntitySQL)
                            filterExpression += " <= @p" + index;
                        else
                            filterExpression += " <= @" + index;
                        break;
                    }
                case Global.LogicalOperators.notEqual:
                    {
                        filterValueList.Add(objectParameter);
                        if (forEntitySQL)
                            filterExpression += " != @p" + index;
                        else
                            filterExpression += " != @" + index;
                        break;
                    }
                case Global.LogicalOperators.contains:
                    {
                        filterValueList.Add(objectParameter);
                        if (forEntitySQL)
                            filterExpression += " LIKE '%' + @p" + index + " + '%'";
                        else
                            filterExpression += ".Contains(@" + index + ")";
                        break;
                    }
                case Global.LogicalOperators.notContains:
                    {
                        filterValueList.Add(objectParameter);
                        if (forEntitySQL)
                            filterExpression += " NOT LIKE '%' + @p" + index + " + '%'";
                        else
                            filterExpression = "!" + filterExpression + ".Contains(@" + index + ")";
                        break;
                    }
                case Global.LogicalOperators.endsWith:
                    {
                        filterValueList.Add(objectParameter);
                        if (forEntitySQL)
                            filterExpression += " LIKE '%' + @p" + index;
                        else
                            filterExpression += ".EndsWith(@" + index + ")";
                        break;
                    }
                case Global.LogicalOperators.startsWith:
                    {
                        filterValueList.Add(objectParameter);
                        if (forEntitySQL)
                            filterExpression += " LIKE @p" + index + " + '%'";
                        else
                            filterExpression += ".StartsWith(@" + index + ")";
                        break;
                    }
                case Global.LogicalOperators.isNotNull:
                    {
                        if (forEntitySQL)
                            filterExpression += " IS NOT NULL";
                        else
                            filterExpression += " != null";
                        break;
                    }
                case Global.LogicalOperators.isNull:
                    {
                        if (forEntitySQL)
                            filterExpression += " IS NULL";
                        else
                            filterExpression += " == null";
                        break;
                    }
                default:
                    break;
            }
            return filterExpression;
        }
#endif

        /// <summary>
        /// Creates a logical connective for a expression
        /// </summary>
        /// <param name="filterItem">The filter item.</param>
        /// <returns>System.String.</returns>
        static string CreateFilterConnector(ACFilterItem filterItem)
        {
            switch (filterItem.Operator)
            {
                case Global.Operators.and:
                    return " && ";
                case Global.Operators.or:
                    return " || ";
                default:
                    return null;
            }
        }

        /// <summary>
        /// Gets the order expression.
        /// </summary>
        /// <param name="forEntitySQL">The query definition.</param>
        /// <returns>System.String.</returns>
        private string GetOrderExpression(bool forEntitySQL = false)
        {
            string orderExpression = "";
            foreach (var orderItem in ACSortColumns)
            {
                if (String.IsNullOrEmpty(orderItem.PropertyName))
                    continue;
                if (!string.IsNullOrEmpty(orderExpression))
                {
                    orderExpression += ",";
                }
                else if (string.IsNullOrEmpty(orderExpression) && forEntitySQL)
                    orderExpression = " ORDER BY ";

                if (forEntitySQL)
                    orderExpression += "c." + orderItem.PropertyName.Replace('\\', '.');
                else
                    orderExpression += orderItem.PropertyName.Replace('\\', '.');

                switch (orderItem.SortDirection)
                {
                    case Global.SortDirections.ascending:
                        {
                            if (forEntitySQL)
                                orderExpression += " ASC";
                            else
                                orderExpression += " ascending";
                            break;
                        }
                    case Global.SortDirections.descending:
                        {
                            if (forEntitySQL)
                                orderExpression += " DESC";
                            else
                                orderExpression += " descending";
                            break;
                        }
                    default:
                        break;
                }
            }
            return orderExpression;
        }
#endregion

#region Value-Methods        
        /// <summary>
        /// Reads the SearchWord from a ACFilterItem that matches the propertyname and converts it to the generic datataype.
        /// </summary>
        /// <typeparam name="T">Datatype</typeparam>
        /// <param name="filterPropertyName">Name of the filter property.</param>
        /// <returns></returns>
        public T GetSearchValue<T>(string filterPropertyName)
        {
            ACFilterItem filterItem = this.ACFilterColumns.Where(c => c.PropertyName == filterPropertyName).FirstOrDefault();
            if (filterItem == null)
                return default(T);
            return filterItem.GetSearchValue<T>();
        }

        /// <summary>
        /// Sets the SearchWord at a ACFilterItem that matches the propertyname and converts it from the generic datataype to string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filterPropertyName">Name of the filter property.</param>
        /// <param name="value">The value.</param>
        public void SetSearchValue<T>(string filterPropertyName, T value)
        {
            ACFilterItem filterItem = this.ACFilterColumns.Where(c => c.PropertyName == filterPropertyName).FirstOrDefault();
            if (filterItem != null)
                filterItem.SetSearchValue<T>(value);
        }

        /// <summary>
        /// Reads the SearchWord from a ACFilterItem that matches the propertyname and the passed operator and converts it to the generic datataype.
        /// This method is helpful if ther are more ACFilterItem's with the same name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filterPropertyName"></param>
        /// <param name="logicalOperator"></param>
        /// <returns></returns>
        public T GetSearchValue<T>(string filterPropertyName, Global.LogicalOperators logicalOperator)
        {
            ACFilterItem filterItem = this.ACFilterColumns.Where(c => c.PropertyName == filterPropertyName && c.LogicalOperator == logicalOperator).FirstOrDefault();
            if (filterItem == null)
                return default(T);
            return filterItem.GetSearchValue<T>();
        }

        /// <summary>
        /// Sets the SearchWord at a ACFilterItem that matches the propertyname and the passed operator and converts it from the generic datataype to string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filterPropertyName"></param>
        /// <param name="logicalOperator"></param>
        /// <param name="value"></param>
        public void SetSearchValue<T>(string filterPropertyName, Global.LogicalOperators logicalOperator, T value)
        {
            ACFilterItem filterItem = this.ACFilterColumns.Where(c => c.PropertyName == filterPropertyName && c.LogicalOperator == logicalOperator).FirstOrDefault();
            if (filterItem != null)
                filterItem.SetSearchValue<T>(value);
        }


        /// <summary>Compares passed filter- and sortlists. If anything is different, then the Items in this query definition are replaced with the passed ones.</summary>
        /// <param name="filterItems">The filter items that should be compared with ACFilterColumns</param>
        /// <param name="sortItems">The sort items that should be compared with ACSortColumns</param>
        /// <param name="onlyDefaultParameters">if set to <c>true</c> only items with IsConfiguration=true are compared.</param>
        /// <param name="compareSearchWords">if set to <c>true</c> the the SearchWord-Property will also be compared</param>
        /// <param name="autoSaveConfig">if set to <c>true</c> then SaveConfig() will be called if something was changed</param>
        /// <returns>true, if something was changed</returns>
        public bool CheckAndReplaceColumnsIfDifferent(IEnumerable<ACFilterItem> filterItems, IEnumerable<ACSortItem> sortItems, bool onlyDefaultParameters = true, bool compareSearchWords = false, bool autoSaveConfig = true)
        {
            bool changed = false;
            if (CheckAndReplaceFilterColumnsIfDifferent(filterItems, onlyDefaultParameters, compareSearchWords, false))
                changed = true;
            if (CheckAndReplaceSortColumnsIfDifferent(sortItems, onlyDefaultParameters, false))
                changed = true;
            if (changed && autoSaveConfig)
                SaveConfig(false);
            return changed;
        }


        /// <summary>Compares passed filter- and sortlists.</summary>
        /// <param name="filterItems">The filter items that should be compared with ACFilterColumns</param>
        /// <param name="sortItems">The sort-list that should be comapred with ACSortColumns.</param>
        /// <param name="onlyDefaultParameters">if set to <c>true</c> only items with IsConfiguration=true are compared</param>
        /// <param name="compareSearchWords">if set to <c>true</c> the the SearchWord-Property will also be compared.</param>
        /// <returns>true, if something is different</returns>
        public bool CompareColumns(IEnumerable<ACFilterItem> filterItems, IEnumerable<ACSortItem> sortItems, bool onlyDefaultParameters = true, bool compareSearchWords = false)
        {
            return    CompareFilterColumns(filterItems, onlyDefaultParameters, compareSearchWords)
                   && CompareSortColumns(sortItems, onlyDefaultParameters);
        }


        /// <summary>Replaces the entries in ACFilterColumns with the entries from parameter filterItems and replaces the entries in ACSortColumns with the entries from parameter sortItems</summary>
        /// <param name="filterItems">The filter items.</param>
        /// <param name="sortItems">The sort items.</param>
        public void ReplaceColumns(IEnumerable<ACFilterItem> filterItems, IEnumerable<ACSortItem> sortItems)
        {
            ReplaceFilterColumns(filterItems);
            ReplaceSortColumns(sortItems);
        }


        /// <summary>Compares the passed filter-list with ACFilterColumns. If anything is different, then the Items in this query definition are replaced with the passed ones.</summary>
        /// <param name="filterItems">The filter items that should be compared with ACFilterColumns</param>
        /// <param name="onlyDefaultParameters">if set to <c>true</c> only items with IsConfiguration=true are compared.</param>
        /// <param name="compareSearchWords">if set to <c>true</c> the the SearchWord-Property will also be compared</param>
        /// <param name="autoSaveConfig">if set to <c>true</c> then SaveConfig() will be called if something was changed</param>
        /// <returns>true, if something was changed</returns>
        public bool CheckAndReplaceFilterColumnsIfDifferent(IEnumerable<ACFilterItem> filterItems, bool onlyDefaultParameters = true, bool compareSearchWords = false, bool autoSaveConfig = true)
        {
            bool changed = false;
            if (!CompareFilterColumns(filterItems, onlyDefaultParameters, compareSearchWords))
            {
                ReplaceFilterColumns(filterItems);
                changed = true;
            }
            if (changed && autoSaveConfig)
                SaveConfig(false);
            return changed;
        }


        /// <summary>Compares the passed filter-list with ACFilterColumns.</summary>
        /// <param name="filterItems">The filter items that should be compared with ACFilterColumns</param>
        /// <param name="onlyDefaultParameters">if set to <c>true</c> only items with IsConfiguration=true are compared</param>
        /// <param name="compareSearchWords">if set to <c>true</c> the the SearchWord-Property will also be compared</param>
        /// <returns>true, if something is different</returns>
        public bool CompareFilterColumns(IEnumerable<ACFilterItem> filterItems, bool onlyDefaultParameters = true, bool compareSearchWords = false)
        {
            List<ACFilterItem> filtersThis = null;
            List<ACFilterItem> filters2Compare = null;
            if (onlyDefaultParameters)
            {
                filtersThis = this.ACFilterColumns.Where(c => c.IsConfiguration).ToList();
                if (filterItems != null && filterItems.Any())
                    filters2Compare = filterItems.Where(c => c.IsConfiguration).ToList();
                else
                    filters2Compare = new List<ACFilterItem>();
            }
            else
            {
                filtersThis = this.ACFilterColumns.ToList();
                if (filterItems != null && filterItems.Any())
                    filters2Compare = filterItems.ToList();
                else
                    filters2Compare = new List<ACFilterItem>();
            }

            if (filtersThis.Count != filters2Compare.Count)
                return false;
            if (filtersThis.Count == 0)
                return true;
            int i = 0;
            foreach (var thisFilter in filtersThis)
            {
                if (filters2Compare[i].CompareTo(thisFilter, compareSearchWords) != 0)
                    return false;
                i++;
            }
            return true;
        }


        /// <summary>Replaces the entries in ACFilterColumns with the entries from parameter filterItems</summary>
        /// <param name="filterItems">The filter items.</param>
        public void ReplaceFilterColumns(IEnumerable<ACFilterItem> filterItems)
        {
            ACFilterColumns.Clear();
            if (filterItems == null || !filterItems.Any())
                return;
            foreach (ACFilterItem filterItem in filterItems)
            {
                ACFilterColumns.Add(filterItem);
            }
        }

        /// <summary>Compares passed sort-list with ACSortColumns. If anything is different, then the Items in this query definition are replaced with the passed ones.</summary>
        /// <param name="sortItems">The sort-list that should be comapred with ACSortColumns.</param>
        /// <param name="onlyDefaultParameters">if set to <c>true</c> only items with IsConfiguration=true are compared.</param>
        /// <param name="autoSaveConfig">if set to <c>true</c> then SaveConfig() will be called if something was changed</param>
        /// <returns>true, if something was changed</returns>
        public bool CheckAndReplaceSortColumnsIfDifferent(IEnumerable<ACSortItem> sortItems, bool onlyDefaultParameters = true, bool autoSaveConfig = true)
        {
            bool changed = false;
            if (!CompareSortColumns(sortItems, onlyDefaultParameters))
            {
                ReplaceSortColumns(sortItems);
                changed = true;
            }
            if (changed && autoSaveConfig)
                SaveConfig(false);
            return changed;
        }


        /// <summary>Compares passed sort-list with ACSortColumns.</summary>
        /// <param name="sortItems">The sort-list that should be comapred with ACSortColumns.</param>
        /// <param name="onlyDefaultParameters">if set to <c>true</c> only items with IsConfiguration=true are compared</param>
        /// <returns>true, if something is different</returns>
        public bool CompareSortColumns(IEnumerable<ACSortItem> sortItems, bool onlyDefaultParameters = true)
        {
            List<ACSortItem> sortsThis = null;
            List<ACSortItem> sorts2Compare = null;
            if (onlyDefaultParameters)
            {
                sortsThis = this.ACSortColumns.Where(c => c.IsConfiguration).ToList();
                if (sortItems != null && sortItems.Any())
                    sorts2Compare = sortItems.Where(c => c.IsConfiguration).ToList();
                else
                    sorts2Compare = new List<ACSortItem>();
            }
            else
            {
                sortsThis = this.ACSortColumns.ToList();
                if (sortItems != null && sortItems.Any())
                    sorts2Compare = sortItems.ToList();
                else
                    sorts2Compare = new List<ACSortItem>();
            }

            if (sortsThis.Count != sorts2Compare.Count)
                return false;
            if (sortsThis.Count == 0)
                return true;
            int i = 0;
            foreach (var thisFilter in sortsThis)
            {
                if (sorts2Compare[i].CompareTo(thisFilter) != 0)
                    return false;
                i++;
            }
            return true;
        }


        /// <summary>Replaces the entries in ACSortColumns with the entries from parameter sortItems</summary>
        /// <param name="sortItems">The sort items</param>
        public void ReplaceSortColumns(IEnumerable<ACSortItem> sortItems)
        {
            ACSortColumns.Clear();
            if (sortItems == null || !sortItems.Any())
                return;
            foreach (ACSortItem sortItem in sortItems)
            {
                ACSortColumns.Add(sortItem);
            }
        }

#endregion

#region ACAccess

        /// <summary>
        /// Creates a Access-instance for executing the query and navigating. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="acGroup">The ac group.</param>
        /// <param name="acComponentParent">The ac component parent.</param>
        /// <param name="contextForQuery">The context for query.</param>
        /// <returns></returns>
        /// <exception cref="FieldAccessException"></exception>
        /// <exception cref="InvalidCastException"></exception>
        public ACAccessNav<T> NewAccessNav<T>(string acGroup, IACObjectWithInit acComponentParent, IACObject contextForQuery = null) where T : class
        {
            if (QueryType == null)
                throw new FieldAccessException(Const.ACQueryTypePrefix);

            if (QueryType != null)
            {
                Type typeT = typeof(T);
                if (QueryType.ObjectFullType != typeof(T))
                    throw new InvalidCastException(String.Format("Type of QueryType {0} is different to Generic-Type {1}.", QueryType.ObjectFullType.FullName, typeT.FullName));
            }

            if (contextForQuery != null)
                this.QueryContext = contextForQuery;

            return new ACAccessNav<T>(ACClassACAccessNav, ACClassACAccessNav, acComponentParent, GetParameterACAccess(acGroup), contextForQuery);
        }


        /// <summary>
        /// Creates a Access-instance for executing the query and navigating. 
        /// </summary>
        /// <param name="acGroup">The ac group.</param>
        /// <param name="acComponentParent">The ac component parent.</param>
        /// <param name="contextForQuery">The context for query.</param>
        /// <returns></returns>
        /// <exception cref="FieldAccessException"></exception>
        public IAccessNav NewAccessNav(string acGroup, IACObjectWithInit acComponentParent, IACObject contextForQuery = null)
        {
            if (QueryType == null)
                throw new FieldAccessException(Const.ACQueryTypePrefix);

            if (contextForQuery != null)
                this.QueryContext = contextForQuery;

            Type typeAccess = typeof(ACAccessNav<>);
            Type typeToCreate = typeAccess.MakeGenericType(QueryType.ObjectFullType);
            return Activator.CreateInstance(typeToCreate, new Object[] { ACClassACAccessNav, (IACObject)ACClassACAccessNav, (IACObjectWithInit)acComponentParent, GetParameterACAccess(acGroup), "", contextForQuery }) as IAccessNav;
        }


        /// <summary>
        /// Creates a Access-instance for executing the query without navigation-feature.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="acGroup"></param>
        /// <param name="acComponentParent"></param>
        /// <param name="contextForQuery"></param>
        /// <returns></returns>
        public ACAccess<T> NewAccess<T>(string acGroup, IACObjectWithInit acComponentParent, IACObject contextForQuery = null) where T : class
        {
            if (QueryType == null)
                throw new FieldAccessException(Const.ACQueryTypePrefix);

            if (QueryType != null)
            {
                Type typeT = typeof(T);
                if (QueryType.ObjectFullType != typeof(T))
                    throw new InvalidCastException(String.Format("Type of QueryType {0} is different to Generic-Type {1}.", QueryType.ObjectFullType.FullName, typeT.FullName));
            }

            if (contextForQuery != null)
                this.QueryContext = contextForQuery;

            return new ACAccess<T>(ACClassACAccess, ACClassACAccess, acComponentParent, GetParameterACAccess(acGroup), contextForQuery);
        }


        /// <summary>
        /// Creates a Access-instance for executing the query without navigation-feature.
        /// </summary>
        /// <param name="acGroup"></param>
        /// <param name="acComponentParent"></param>
        /// <param name="contextForQuery"></param>
        /// <returns></returns>
        public IAccess NewAccess(string acGroup, IACObjectWithInit acComponentParent, IACObject contextForQuery = null)
        {
            if (QueryType == null)
                throw new FieldAccessException(Const.ACQueryTypePrefix);

            if (contextForQuery != null)
                this.QueryContext = contextForQuery;

            Type typeAccess = typeof(ACAccess<>);
            Type typeToCreate = typeAccess.MakeGenericType(QueryType.ObjectFullType);
            return Activator.CreateInstance(typeToCreate, new Object[] { ACClassACAccess, (IACObject)ACClassACAccess, (IACObjectWithInit)acComponentParent, GetParameterACAccess(acGroup), "", contextForQuery }) as IAccess;
        }

#endregion

#endregion

#region private

#region Init-Methods
        /// <summary>
        /// Inits the AC columns.
        /// </summary>
        private void InitACColumns()
        {
            _ACColumns = new List<ACColumnItem>();
            if (!_IsLoadMode && this.QueryType != null)
            {
                _ACColumns = this.QueryType.GetColumns();
            }
        }


        /// <summary>
        /// Inits the AC filter columns.
        /// </summary>
        private void InitACFilterColumns()
        {
            if (_ACFilterColumns != null)
            {
                if (_ACFilterColumnsSubscribed)
                {
                    _ACFilterColumns.ListChanged -= ACFilterColumns_ListChanged;
                    _ACFilterColumnsSubscribed = false;
                }
            }

            _ACFilterColumns = new BindingList<ACFilterItem>();

            if (!_IsLoadMode)
            {
                _ACFilterColumns.Clear();
                string[] filterColumns = null;
                if (!String.IsNullOrEmpty(TypeACClass.ACFilterColumns))
                    filterColumns = TypeACClass.ACFilterColumns.Split(',');
                else
                {
                    var acColumnItem = TypeACClass.GetColumns().FirstOrDefault();
                    if (acColumnItem != null)
                        filterColumns = new string[] { acColumnItem.PropertyName };
                }

                if (filterColumns != null && filterColumns.Any())
                {
                    foreach (var filterColumn in filterColumns)
                    {
                        bool likeOperator = false;
                        IACType typeOfColumn = QueryType;
                        object acSource = null;
                        string acPath = "";
                        Global.ControlModes acControlMode = Global.ControlModes.Disabled;
                        if (QueryType != null && QueryType.ACUrlBinding(filterColumn, ref typeOfColumn, ref acSource, ref acPath, ref acControlMode))
                        {
                            if (typeOfColumn != null && typeOfColumn.ObjectType != null)
                                likeOperator = typeOfColumn.ObjectType.IsAssignableFrom(typeof(string));
                        }

                        if (likeOperator)
                        {
                            _ACFilterColumns.Add(new ACFilterItem(Global.FilterTypes.filter, filterColumn, Global.LogicalOperators.contains,
                                        Global.Operators.or, "", true, true));
                        }
                        else
                        {
                            _ACFilterColumns.Add(new ACFilterItem(Global.FilterTypes.filter, filterColumn, Global.LogicalOperators.equal,
                                        Global.Operators.or, "", true, true));
                        }
                    }
                }
            }

            if (!_ACFilterColumnsSubscribed)
            {
                _ACFilterColumns.ListChanged += new ListChangedEventHandler(ACFilterColumns_ListChanged);
                _ACFilterColumnsSubscribed = true;
            }
        }


        /// <summary>
        /// Inits the AC sort columns.
        /// </summary>
        public void InitACSortColumns()
        {
            if (_ACSortColumns != null)
            {
                if (_ACSortColumnsSubscribed)
                {
                    _ACSortColumns.ListChanged -= ACSortColumns_ListChanged;
                    _ACSortColumnsSubscribed = false;
                }
            }

            _ACSortColumns = new BindingList<ACSortItem>();
            if (!_IsLoadMode)
            {
                _ACSortColumns.Clear();
                if (!String.IsNullOrEmpty(TypeACClass.ACSortColumns))
                {
                    string[] sortOrders = TypeACClass.ACSortColumns.Split(',');
                    foreach (var sortOrder in sortOrders)
                    {
                        _ACSortColumns.Add(new ACSortItem(sortOrder, Global.SortDirections.ascending, true));
                    }
                }
                else
                {
                    var acColumnItem = TypeACClass.GetColumns().FirstOrDefault();
                    if (acColumnItem != null)
                        _ACSortColumns.Add(new ACSortItem(acColumnItem.PropertyName, Global.SortDirections.ascending, true));
                }
            }

            if (!_ACSortColumnsSubscribed)
            {
                _ACSortColumns.ListChanged += new ListChangedEventHandler(ACSortColumns_ListChanged);
                _ACSortColumnsSubscribed = true;
            }
        }

#endregion

#region ACAccess
        private ACValueList GetParameterACAccess(string acGroup)
        {
            ACValueList acValueList = new ACValueList();
            acValueList.Add(new ACValue("NavACQueryDefinition", ACQueryDefinition.ClassName, "", this));
            acValueList.Add(new ACValue(Const.ACGroup, Const.TNameString, "", acGroup));
            return acValueList;
        }
#endregion

#endregion

#endregion

#region Eventhandling

#region PropertyChanged
        /// <summary>
        /// Notifies the property changed.
        /// </summary>
        /// <param name="info">The info.</param>
        private void OnPropertyChanged(String info)
        {
            if (SaveToACConfigOff)
                return;
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _ACFilterColumnsSubscribed = false;
        protected virtual void ACFilterColumns_ListChanged(object sender, ListChangedEventArgs e)
        {
            // Bei Änderung des Suchbgriffs keine automatische Aktualisierung/Serialisierung in der ACClassConfig, damit nicht ständig ACSaveChanges aufgerufen wird
            if (e.PropertyDescriptor != null && e.PropertyDescriptor.DisplayName != "SearchWord")
                OnPropertyChanged(ACQueryDefinition.ACFilterColumnsPropName); // Benachrichtige ACClassConfig für Serialisierung
            if (e.PropertyDescriptor != null && e.PropertyDescriptor.DisplayName == "IsConfiguration")
                return;
            ResetQueryStrings();
        }

        private bool _ACSortColumnsSubscribed = false;
        protected virtual void ACSortColumns_ListChanged(object sender, ListChangedEventArgs e)
        {
            OnPropertyChanged(ACQueryDefinition.ACSortColumnsPropName);
            ResetQueryStrings();
        }

#endregion

#endregion

    }
}
