using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
#if !EFCR
using System.Data.Objects;
#endif
using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace gip.core.datamodel
{
    /// <summary>
    ///   <para>Implementation of IAccessT&lt;T&gt;.</para>
    ///   <para>This class encapsulates a ACQueryDefinition, that stores a user defined query.<br />
    ///   This user defined query is passed to ACQuery.ACSelect()-Method when the NavSearch()-Method is invoked.<br />
    ///   The ACSelect()-Method builds a dynamic LINQ-expression tree or a Entity-SQL-Expression and returns a IQueryable&lt;T&gt;.<br />
    ///   The result can be read in the NavObjectList or NavList-Property.</para>
    /// </summary>
    /// <typeparam name="T">Type of a EF-Class</typeparam>
    /// <seealso cref="gip.core.datamodel.ACGenericObject"/>
    /// <seealso cref="gip.core.datamodel.IAccessT{T}"/>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACAccess'}de{'ACAccess'}", Global.ACKinds.TACObject, Global.ACStorableTypes.NotStorable, true, false)]
    [ACClassConstructorInfo(
        new object[] 
        { 
            new object[] {"NavACQueryDefinition", Global.ParamOption.Required,  typeof(ACQueryDefinition)},
            new object[] {Const.ACGroup, Global.ParamOption.Required, typeof(String)}
        }
    )]
    public class ACAccess<T> : ACGenericObject, IAccessT<T> where T : class
    {
        public delegate IQueryable<T> NavSearchEventHandler(IQueryable<T> result);
        public delegate void NavSearchExecutedEventHandler(object sender, IList<T> result);

        #region c'tors
        public ACAccess(ACClass acType, IACObject content, IACObjectWithInit parentACObject, ACValueList parameter, string acIdentifier = "") :
            this(acType, content, parentACObject, parameter, acIdentifier, null)
        {
        }

        public ACAccess(ACClass acType, IACObject content, IACObjectWithInit parentACObject, ACValueList parameter, IACObject contextForQuery) :
            this(acType, content, parentACObject, parameter, "", contextForQuery)
        {
        }

        public ACAccess(ACClass acType, IACObject content, IACObjectWithInit parentACObject, ACValueList parameter, string acIdentifier, IACObject contextForQuery) :
            base(acType, content, parentACObject, parameter, acIdentifier)
        {
            if (parameter == null || parameter.Count() < 2)
                throw new Exception("Parameter [acGroup] required!");
            if (parameter == null || parameter.Count() < 1)
                throw new Exception("Parameter [acQueryDefinition] required!");
            NavACQueryDefinition = parameter["NavACQueryDefinition"] as ACQueryDefinition;
            if (contextForQuery != null)
                NavACQueryDefinition.QueryContext = contextForQuery;
            else if (NavACQueryDefinition.QueryContext == null)
                NavACQueryDefinition.QueryContext = parentACObject;
            ACGroup = parameter[Const.ACGroup] as string;
        }

        override public bool ACDeInit(bool deleteACClassTask = false)
        {
            _NavList = null;
            _NavACQueryDefinition = null;
            return base.ACDeInit(deleteACClassTask);
        }
        #endregion


        #region Properties

        public string ACGroup
        {
            get;
            set;
        }

        [ACPropertyInfo(9999)]
        public virtual IACBSO ParentACComponent
        {
            get
            {
                return ParentACObject as IACBSO;
            }
        }


        ACQueryDefinition _NavACQueryDefinition;
        /// <summary>Returns the persistable query</summary>
        /// <value>ACQueryDefinition</value>
        [ACPropertyInfo(9999)]
        public ACQueryDefinition NavACQueryDefinition
        {
            get
            {
                return _NavACQueryDefinition;
            }
            set
            {
                _NavACQueryDefinition = value;
                OnPropertyChanged("NavACQueryDefinition");
            }
        }

        protected IList<T> _NavList = new ObservableCollection<T>();
        /// <summary>
        /// Result of the NavSearch()-Method
        /// </summary>
        /// <value>
        /// Observable-Collection
        /// </value>
        public IList<T> NavList
        {
            get
            {
                return _NavList;
            }
        }

        [ACPropertyInfo(999, "QueryItemsCount", "en{'Query items count'}de{'Abfrageelemente zählen'}")]
        public int QueryItemsCount { get; set; }

        /// <summary>
        /// The Result of the NavSearch() -Method
        /// </summary>
        /// <value>A list of EF-Objects</value>
        public IEnumerable NavObjectList
        {
            get
            {
                return _NavList;
            }
        }

        /// <summary>
        /// Returns the count of objects in NavList.
        /// </summary>
        /// <value>Count of objects in NavList.</value>
        public int NavRowCount
        {
            get
            {
                return _NavList.Count;
            }
        }

        #endregion


        #region Methods

        #region Public
        /// <summary>Executes a Query according to the filter and sort entries in ACQueryDefinition the result is copied to the NavObjectList.</summary>
        /// <param name="context">Reference to a database context</param>
        /// <returns>True, if query was successful</returns>
        public bool NavSearch(IACEntityObjectContext context)
        {
            if (context == null)
                return false;
            return NavSearch(context, context.RecommendedMergeOption);
        }


        /// <summary>Executes a Query according to the filter and sort entries in ACQueryDefinition the result is copied to the NavObjectList.</summary>
        /// <param name="mergeOption">The merge option.</param>
        /// <param name="context">Reference to a database context</param>
        /// <returns>True, if query was successful</returns>
        public bool NavSearch(MergeOption mergeOption, IACEntityObjectContext context)
        {
            if (context == null)
                return false;
            return NavSearch(context, context.RecommendedMergeOption);
        }


        /// <summary>Executes a Query according to the filter and sort entries in ACQueryDefinition the result is copied to the NavObjectList.</summary>
        /// <param name="parentACObject">Reference to a database context</param>
        /// <param name="mergeOption">The merge option.</param>
        /// <returns>True, if query was successful</returns>
        public bool NavSearch(IACObject parentACObject, MergeOption mergeOption)
        {
            if (NavACQueryDefinition.TakeCount <= 0 && Database.Root != null && Database.Root.Environment != null)
                NavACQueryDefinition.TakeCount = Database.Root.Environment.AccessDefaultTakeCount;
            IQueryable<T> result = parentACObject.ACSelect<T>(NavACQueryDefinition, mergeOption);
            if (NavSearchExecuting != null)
                result = NavSearchExecuting(result);
            QueryItemsCount = result.Count();
            if (NavACQueryDefinition.TakeCount > 0)
                result = (IQueryable<T>)result.Take(NavACQueryDefinition.TakeCount);
            if (result != null)
            {
                try
                {
                    _NavList = new ObservableCollection<T>(result);
                }
                catch (Exception e)
                {
                    _NavList = new ObservableCollection<T>();

                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (Database.Root != null && Database.Root.Messages != null)
                        Database.Root.Messages.LogException("ACAccess<T>", "NavSearch", msg);
                }
            }
            else
                _NavList = new ObservableCollection<T>();
            if (NavSearchExecuted != null)
                NavSearchExecuted(this, _NavList);
            OnPropertyChanged("NavList");
            OnPropertyChanged("NavObjectList");
            OnPropertyChanged("QueryItemsCount");
            OnPostNavSearch();
            return result != null;
        }


        /// <summary>
        /// Executes a Query according to the filter and sort entries in ACQueryDefinition. The result is copied to the NavObjectList. The Query-Context is automatically Determined by the local ParentACObject-Member. If you want to specify another Query-Context, then use Method NavSearch(IACObject parentACObject, MergeOption mergeOption = MergeOption.AppendOnly)<br /></summary>
        /// <param name="mergeOption">The merge option.</param>
        /// <returns>True, if query was successful</returns>
        public bool NavSearch(MergeOption mergeOption = MergeOption.AppendOnly)
        {
            bool succ = false;
            if (ParentACComponent != null && NavACQueryDefinition.QueryType != null && NavACQueryDefinition.QueryType.ObjectType != null)
            {
                if (ParentACComponent.Database.GetType().Namespace == NavACQueryDefinition.QueryType.ObjectType.Namespace)
                    succ = NavSearch(ParentACComponent.Database, mergeOption);
                else if (ParentACComponent.Database.ContextIPlus.GetType().Namespace == NavACQueryDefinition.QueryType.ObjectType.Namespace)
                    succ = NavSearch(ParentACComponent.Database.ContextIPlus, mergeOption);
                if (!succ)
                    succ = NavSearch(ParentACComponent, mergeOption);
            }
            else if (ParentACObject != null)
                succ = NavSearch(ParentACObject, mergeOption);
            return succ;
        }


        /// <summary>Executes a Query according to the filter and sort entries in ACQueryDefinition without changing the NavObjectList. The result is returned directly.</summary>
        /// <param name="searchWord">The search word.</param>
        /// <param name="mergeOption">The merge option.</param>
        /// <returns>A IQueryable</returns>
        public IQueryable OneTimeSearch(string searchWord, MergeOption mergeOption = MergeOption.AppendOnly)
        {
            return OneTimeSearchT(searchWord, mergeOption);
        }


        /// <summary>Executes a Query according to the filter and sort entries in ACQueryDefinition without changing the NavObjectList. The result is returned directly.</summary>
        /// <param name="searchWord">The search word.</param>
        /// <param name="mergeOption">The merge option.</param>
        /// <returns>A IQueryableIQueryable{T}</returns>
        public IQueryable<T> OneTimeSearchT(string searchWord, MergeOption mergeOption = MergeOption.AppendOnly)
        {
            if (String.IsNullOrEmpty(searchWord))
                return null;
            try
            {
                IACObject parentACObject = null;
                NavACQueryDefinition.OneTimeSearchWord = searchWord;
                if (ParentACComponent != null && NavACQueryDefinition.QueryType != null && NavACQueryDefinition.QueryType.ObjectType != null)
                {
                    if (ParentACComponent.Database.GetType().Namespace == NavACQueryDefinition.QueryType.ObjectType.Namespace)
                        parentACObject = ParentACComponent.Database;
                    else if (ParentACComponent.Database.ContextIPlus.GetType().Namespace == NavACQueryDefinition.QueryType.ObjectType.Namespace)
                        parentACObject = ParentACComponent.Database.ContextIPlus;
                }
                else if (ParentACObject != null)
                    parentACObject = ParentACObject;
                if (parentACObject != null)
                {
                    return parentACObject.ACSelect<T>(NavACQueryDefinition, mergeOption);
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("ACAccess<T>", "OneTimeSearchT", msg);
            }
            finally
            {
                NavACQueryDefinition.OneTimeSearchWord = null;
            }
            return null;
        }


        /// <summary>Invokes the OneTimeSearchT()-Method an returns the first element in the result</summary>
        /// <param name="searchWord">The search word.</param>
        /// <param name="mergeOption">The merge option.</param>
        /// <returns>The first element oft type T in the result</returns>
        public T OneTimeSearchFirstOrDefaultT(string searchWord, MergeOption mergeOption = MergeOption.AppendOnly)
        {
            IQueryable<T> query = OneTimeSearchT(searchWord, mergeOption);
            if (query == null)
                return null;
            return query.FirstOrDefault();
        }


        /// <summary>Invokes the OneTimeSearch()-Method an returns the first element in the result</summary>
        /// <param name="searchWord">The search word.</param>
        /// <param name="mergeOption">The merge option.</param>
        /// <returns>The first element in the result</returns>
        public object OneTimeSearchFirstOrDefault(string searchWord, MergeOption mergeOption = MergeOption.AppendOnly)
        {
            return OneTimeSearchFirstOrDefaultT(searchWord, mergeOption);
        }


        /// <summary>
        /// Copies the given List into the NavList-Property
        /// </summary>
        /// <param name="list">The list.</param>
        public void ToNavList(IEnumerable<T> list)
        {
            _NavList = new ObservableCollection<T>(list);
            OnPropertyChanged("NavList");
            OnPropertyChanged("NavObjectList");
        }

        /// <summary>
        /// Executes the persisted query in the NavList instead querying database 
        /// </summary>
        /// <returns></returns>
        public bool NavSearchInObjectList()
        {
            List<T> clone = NavList.ToList();
            IQueryable<T> result = clone.ACSelect<T>(NavACQueryDefinition);
            if (NavSearchExecuting != null)
                result = NavSearchExecuting(result);
            if (result != null)
                _NavList = new ObservableCollection<T>(result);
            else
                _NavList = new ObservableCollection<T>();
            if (NavSearchExecuted != null)
                NavSearchExecuted(this, _NavList);
            OnPropertyChanged("NavList");
            OnPropertyChanged("NavObjectList");
            OnPostNavSearch();
            return result != null;
        }

        /// <summary>
        /// Called when NavSearch has copied the result to NavList.
        /// </summary>
        protected virtual void OnPostNavSearch()
        {
        }
        #endregion


        #region GUI-Methods

        /// <summary>Opens the a dialog (VBBSOQueryDialog) on the gui for the manipulation of the ACQueryDefinition.</summary>
        /// <returns>True, if OK-Button was clicked</returns>
        [ACMethodInteraction("Query", "en{'Configuration'}de{'Konfiguration'}", 9999, false)]
        public bool ShowACQueryDialog()
        {
            return (bool)this.ParentACObject.ACUrlCommand("VBBSOQueryDialog!QueryConfigDlg", new object[] { NavACQueryDefinition, true, true, true, true });
        }


        /// <summary>Opens the a dialog (VBBSOQueryDialog) on the gui for chaging the filter values in the ACQueryDefinition.</summary>
        /// <returns>True, if OK-Button was clicked</returns>
        [ACMethodInteraction("Query", "en{'Change values in column'}de{'Ändere Werte in Spalte'}", 9999, false)]
        public bool ShowChangeColumnValuesDialog(ACColumnItem column)
        {
            return (bool)this.ParentACObject.ACUrlCommand("VBBSOQueryDialog!ChangeColumnValues", new object[] { this, column, NavACQueryDefinition });
        }

        #endregion

        #endregion


        #region Events

        /// <summary>
        /// Occurs before query will be executed to the database.
        /// You can extend the passed IQueryable{T} of NavSearchEventHandler and return it again
        /// </summary>
        public event NavSearchEventHandler NavSearchExecuting;

        public event NavSearchExecutedEventHandler NavSearchExecuted;

        /// <summary>
        /// Property-Changed event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}