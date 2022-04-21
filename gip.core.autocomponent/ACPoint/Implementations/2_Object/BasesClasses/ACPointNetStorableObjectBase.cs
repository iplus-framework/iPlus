using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACPointNetStorableObjectBase'}de{'ACPointNetStorableObjectBase'}", Global.ACKinds.TACAbstractClass)]
    public abstract class ACPointNetStorableObjectBase<T, W> : ACPointNetStorableBase<T, W>, IACPointNetObject<T>
        where W : ACPointNetWrapObject<T>
        where T : IACObject 
    {
        #region c'tors
        /// <summary>
        /// Constructor for Deserializer
        /// </summary>
        public ACPointNetStorableObjectBase()
            : this(null, null, 0)
        {
        }

        /// <summary>Constructor for Reflection-Instantiation</summary>
        /// <param name="parent"></param>
        /// <param name="acClassProperty"></param>
        /// <param name="maxCapacity"></param>
        public ACPointNetStorableObjectBase(IACComponent parent, IACType acClassProperty, uint maxCapacity)
            : base(parent,acClassProperty, maxCapacity)
        {
            _LocalStorage = new List<W>();
        }
        #endregion

        #region IACConnectionPoint<ACPointRefNetWrapObject<T>> Member

        /// <summary>
        /// Locked query of ConnectionList.Count()
        /// </summary>
        public int ConnectionListCount
        {
            get
            {

                using (ACMonitor.Lock(LockConnectionList_20040))
                {
                    return ConnectionList.Count();
                }
            }
        }

        public virtual new IEnumerable<ACPointNetWrapObject<T>> ConnectionList
        {
            get 
            { 
                return (IEnumerable<ACPointNetWrapObject<T>>)_LocalStorage; 
            }
            set
            {
                //_LocalStorage = (List<W>) value.ToList();
            }
        }

#endregion

#region IACPointRefNet<T,ACPointRefNetWrapObject<T>> Member

        public bool Contains(ACPointNetWrapObject<T> wrapObject)
        {
            return base.Contains((W)wrapObject);
        }

        public bool Remove(ACPointNetWrapObject<T> wrapObject)
        {
            return base.Remove((W)wrapObject);
        }

#endregion

#region IACPointRefNetObject<T> Member

        /// <summary>
        /// Returns a "WrapObject" from the local storage List by addressing it over a acURL.
        /// </summary>
        /// <param name="acURL"></param>
        /// <returns>Returns a "WrapObject"</returns>
        public virtual ACPointNetWrapObject<T> GetWrapObject(string acURL)
        {
            if ((_LocalStorage == null) || (String.IsNullOrEmpty(acURL)))
                return null;

            using (ACMonitor.Lock(LockLocalStorage_20033))
            {
                return _LocalStorage.Where(c => c.GetACUrl() == acURL).FirstOrDefault();
            }
        }

        /// <summary>
        /// Returns a "WrapObject" from the local storage List by addressing it over the "refObject".
        /// </summary>
        /// <param name="refObject"></param>
        /// <returns>Returns a "WrapObject"</returns>
        public virtual ACPointNetWrapObject<T> GetWrapObject(T refObject)
        {
            if ((_LocalStorage == null) || (refObject == null))
                return null;
            if (IsComparableType())
            {

                using (ACMonitor.Lock(LockLocalStorage_20033))
                {
                    return _LocalStorage.Where(c => ((W)c).CompareTo(refObject) == 0).FirstOrDefault();
                }
            }
            else
            {

                using (ACMonitor.Lock(LockLocalStorage_20033))
                {
                    return _LocalStorage.Where(c => c.ValueT != null && c.ValueT.Equals(refObject)).FirstOrDefault();
                }
            }
        }

        /// <summary>
        /// Returns a "refObject" from the local storage List by addressing it over a acURL.
        /// </summary>
        /// <param name="acURL"></param>
        /// <returns>Returns a "refObject"</returns>
        public T GetRefObject(string acURL)
        {
            ACPointNetWrapObject<T> wrapObject = GetWrapObject(acURL);
            if (wrapObject == null)
                return default(T);
            return wrapObject.ValueT;
        }

        /// <summary>
        /// Checks if a "refObject" exists in the local storage List
        /// </summary>
        /// <param name="refObject"></param>
        /// <returns></returns>
        public bool Contains(T refObject)
        {
            ACPointNetWrapObject<T> wrapObject = GetWrapObject(refObject);
            if (wrapObject == null)
                return false;
            return true;
        }

        /// <summary>
        /// Removes a "refObject" from the local storage list including it's wrapping "wrapObject" 
        /// </summary>
        /// <param name="refObject"></param>
        /// <returns>returns true if object existed and was removed</returns>
        public virtual bool Remove(T refObject)
        {
            ACPointNetWrapObject<T> wrapObject = GetWrapObject(refObject);
            if (wrapObject == null)
                return false;
            return Remove(wrapObject);
        }

        /// <summary>
        /// Removes the "wrapObject" from the local storage list including it's wrapped "refObject" 
        /// </summary>
        /// <param name="acURL">Addressing over acURL</param>
        /// <returns>returns true if object existed and was removed</returns>
        public virtual bool Remove(string acURL)
        {
            if (String.IsNullOrEmpty(acURL))
                return false;
            bool removed = false;

            using (ACMonitor.Lock(LockLocalStorage_20033))
            {
                removed = Convert.ToBoolean(_LocalStorage.RemoveAll(c => c.GetACUrl() == acURL));
            }

            if (removed)
                OnLocalStorageListChanged();
            return removed;
        }


        /// <summary>
        /// Adds a "wrapObject" which contains a "refObject" to the List
        /// List could reside on Proxy or Serverside. This depends upon the underlying implementation.
        /// </summary>
        /// <param name="wrapObject"></param>
        public virtual bool Add(ACPointNetWrapObject<T> wrapObject)
        {
            if (!AddToList((W)wrapObject))
                return false;
            OnLocalStorageListChanged();
            return true;
        }

        /// <summary>
        /// Adds a "refObject" to the List by creating a ACPointRefNetWrapObject-Instance and wrapping the "refObject".
        /// List could reside on Proxy or Serverside. This depends upon the underlying implementation.
        /// </summary>
        /// <param name="refObject"></param>
        /// <returns></returns>
        public virtual ACPointNetWrapObject<T> Add(T refObject)
        {
            if (refObject == null)
                return null;

            ACPointNetWrapObject<T> wrapObject = new ACPointNetWrapObject<T>(refObject,this);
            if (wrapObject == null)
                return null;
            if (!AddToList((W)wrapObject))
                return null;
            OnLocalStorageListChanged();
            return wrapObject;
        }

        /// <summary>
        /// Appends a List of "refObjects" to the local storage list by automatically creating ACPointRefNetWrapObject-Instances and wrapping the "refObjects".
        /// </summary>
        /// <param name="refObjects"></param>
        public virtual void Add(IEnumerable<T> refObjects)
        {
            if (refObjects == null)
                return;
            foreach (T refObject in refObjects)
            {
                ACPointNetWrapObject<T> wrapObject = new ACPointNetWrapObject<T>(refObject,this);
                AddToList((W)wrapObject);
            }
            OnLocalStorageListChanged();
        }

        /// <summary>
        /// Replaces the local storage List with the passed List of "refObjects" by automatically creating ACPointRefNetWrapObject-Instances and wrapping the "refObjects".
        /// </summary>
        /// <param name="refObjects"></param>
        public virtual void Set(IEnumerable<T> refObjects)
        {
            _LocalStorage = new List<W>();
            Add(refObjects);
        }

#endregion

#region IACPointRefNet<T,ACPointRefNetWrapObject<T>> Member


        public ACPointNetWrapObject<T> GetWrapObject(ACPointNetWrapObject<T> cloneOrOriginal)
        {
            return base.GetWrapObject((W)cloneOrOriginal);
        }

#endregion

#region IACPointNet<T,ACPointNetWrapObject<T>> Member


        public new ACPointNetWrapObject<T> GetWrapObject(object cloneOrOriginal)
        {
            return base.GetWrapObject(cloneOrOriginal);
        }

#endregion
    }
}

