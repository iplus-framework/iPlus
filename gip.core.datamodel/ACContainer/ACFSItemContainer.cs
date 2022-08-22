using System.Collections.Generic;
using System.Linq;

namespace gip.core.datamodel
{

    public delegate void ReportAddFsItem(ACFSItemContainer cFSItemContainer, ACFSItem aCFSItem);

    public class ACFSItemContainer
    {
        public event ReportAddFsItem OnReportAddFsItem;

        #region private members

        #endregion

        #region ctor's

        public ACFSItemContainer(IACEntityObjectContext db, bool buildCache = false)
        {
            DB = db;
            if (buildCache)
                BuildCache();
        }


        #endregion

        #region Properties
        public bool CheckUpdateDate { get; set; }

        private List<ACFSItem> stack;

        public List<ACFSItem> Stack
        {
            get
            {
                if (stack == null)
                    stack = new List<ACFSItem>();
                return stack;
            }
        }


        public IACEntityObjectContext DB { get; set; }

        private Dictionary<string, IACObject> _CachedIACObjects;
        public Dictionary<string, IACObject> CachedIACObjects
        {
            get
            {
                if (_CachedIACObjects == null)
                    _CachedIACObjects = new Dictionary<string, IACObject>();
                return _CachedIACObjects;
            }
        }

        public IACObject ACUrlCommandCached(string acURL)
        {
            IACObject fetchedObject = null;

            if (CachedIACObjects.Keys.Contains(acURL))
                fetchedObject = CachedIACObjects[acURL];
            
            return fetchedObject;
        }
        #endregion

        #region Methods

        public void BuildCache()
        {
            _CachedIACObjects = 
                DB
                .ContextIPlus
                .ACClass
                .ToDictionary(key => key.GetACUrl().Replace(Const.ContextDatabase + "\\", ""), val => val as IACObject);
            
            ACProject[] acProjects = DB.ContextIPlus.ACProject.ToArray();
            foreach(ACProject aCProject in acProjects)
                _CachedIACObjects.Add(aCProject.GetACUrl().Replace(Const.ContextDatabase + "\\", ""), aCProject);
        }


        public void AddStack(ACFSItem aCFSItem)
        {
            Stack.Add(aCFSItem);
            if (OnReportAddFsItem != null)
                OnReportAddFsItem(this, aCFSItem);
        }

        #endregion

    }
}
