using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics; 
using System.Security.Permissions; 
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    //[HostProtection(SharedState = true)] 
    [Serializable]
    public class SafeBindingList<T> : SafeList<T>, IBindingList, ICancelAddNew, IRaiseItemChangedEvents 
    {
        private readonly string _ExSource = "gip.core.datamodel.SafeBindingList<" + typeof(T).Name + ">.";

        private int addNewPos = -1;
        private bool raiseListChangedEvents = true; 
        private bool raiseItemChangedEvents = false;
 
        [NonSerialized()]
        private PropertyDescriptorCollection itemTypeProperties = null; 
 
        [NonSerialized()] 
        private PropertyChangedEventHandler propertyChangedEventHandler = null; 
 
        [NonSerialized()] 
        private AddingNewEventHandler onAddingNew;
 
        [NonSerialized()]
        private ListChangedEventHandler onListChanged; 
 
        [NonSerialized()] 
        private int lastChangeIndex = -1; 
 
        private bool allowNew = true; 
        private bool allowEdit = true;
        private bool allowRemove = true;
        private bool userSetAllowNew = false;
  
        #region Constructors
  
        public SafeBindingList() : base() {
            Initialize();
        } 
 
        public SafeBindingList(IList<T> list) : base(list) {
            Initialize();
        }
  
        private void Initialize() {
            // Set the default value of AllowNew based on whether type T has a default constructor 
            this.allowNew = ItemTypeHasDefaultConstructor; 
 
            // Check for INotifyPropertyChanged 
            if (typeof(INotifyPropertyChanged).IsAssignableFrom(typeof(T))) {
                // Supports INotifyPropertyChanged
                this.raiseItemChangedEvents = true;

                RWLock.EnterReadLock();
                try
                {
                    // Loop thru the items already in the collection and hook their change notification.
                    foreach (T item in this._Items)
                    {
                        HookPropertyChanged(item);
                    }
                }
                finally
                {
                    RWLock.ExitReadLock();
                }

            } 
        }
 
        private bool ItemTypeHasDefaultConstructor {
            get { 
                Type itemType = typeof(T);
  
                if (itemType.IsPrimitive) { 
                    return true;
                } 
 
                if (itemType.GetConstructor(BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance, null, new Type[0], null) != null) {
                    return true;
                } 
 
                return false; 
            } 
        }
  
        #endregion
 
        #region AddingNew event
  
        public event AddingNewEventHandler AddingNew { 
            add {
                bool allowNewWasTrue = AllowNew;
                onAddingNew += value;
                if (allowNewWasTrue != AllowNew) { 
                    FireListChanged(ListChangedType.Reset, -1);
                } 
            } 
            remove {
                bool allowNewWasTrue = AllowNew; 
                onAddingNew -= value;
                if (allowNewWasTrue != AllowNew) {
                    FireListChanged(ListChangedType.Reset, -1);
                } 
            }
        } 
  
        protected virtual void OnAddingNew(AddingNewEventArgs e) {
            if (onAddingNew != null) { 
                onAddingNew(this, e);
            } 
        } 
 
        // Private helper method 
        private object FireAddingNew() {
            AddingNewEventArgs e = new AddingNewEventArgs(null);
            OnAddingNew(e);
            return e.NewObject; 
        }
  
        #endregion 
 
        #region ListChanged event 
 
        public event ListChangedEventHandler ListChanged { 
            add { 
                onListChanged += value;
            } 
            remove {
                onListChanged -= value;
            }
        } 
 
        protected virtual void OnListChanged(ListChangedEventArgs e) {
            if (onListChanged != null) {
                onListChanged(this, e);
            } 
        }
  
        public bool RaiseListChangedEvents {
            get { 
                return this.raiseListChangedEvents;
            }
 
            set { 
                if (this.raiseListChangedEvents != value) {
                    this.raiseListChangedEvents = value; 
                } 
            }
        } 
 
        public void ResetBindings() {
            FireListChanged(ListChangedType.Reset, -1); 
        } 
 
        public void ResetItem(int position) {
            FireListChanged(ListChangedType.ItemChanged, position); 
        }
  
        // Private helper method 
        private void FireListChanged(ListChangedType type, int index) {
            if (this.raiseListChangedEvents) { 
                OnListChanged(new ListChangedEventArgs(type, index));
            }
        }
  
        #endregion
  
        #region Collection<t> overrides 
 
        // Collection<t> funnels all list changes through the four virtual methods below. 
        // We override these so that we can commit any pending new item and fire the proper ListChanged events.
 
        protected override void ClearItems() {
            EndNew(addNewPos); 
 
            if (this.raiseItemChangedEvents) 
            {
                RWLock.EnterReadLock();
                try
                {
                    foreach (T item in this._Items)
                    {
                        UnhookPropertyChanged(item);
                    }
                }
                finally
                {
                    RWLock.ExitReadLock();
                }
            }
 
            base.ClearItems();
            FireListChanged(ListChangedType.Reset, -1); 
        }

        protected override void InsertItem(int index, T item, int timeout, bool withlock = true)
        { 
            EndNew(addNewPos);
            base.InsertItem(index, item, timeout, withlock); 
 
            if (this.raiseItemChangedEvents) {
                HookPropertyChanged(item);
            } 
 
            FireListChanged(ListChangedType.ItemAdded, index); 
        }

        protected override void RemoveItem(int index, int timeout, bool withlock = true)
        { 
            // Need to all RemoveItem if this on the AddNew item
            if (!this.allowRemove && !(this.addNewPos >= 0 && this.addNewPos == index)) {
                throw new NotSupportedException();
            } 
 
            EndNew(addNewPos); 
  
            if (this.raiseItemChangedEvents) {
                UnhookPropertyChanged(this[index]); 
            }

            base.RemoveItem(index, timeout, withlock);
            FireListChanged(ListChangedType.ItemDeleted, index); 
        }

        protected override void SetItem(int index, T item, int timeout)
        { 
 
            if (this.raiseItemChangedEvents) { 
                UnhookPropertyChanged(this[index]);
            }

            base.SetItem(index, item, timeout); 
 
            if (this.raiseItemChangedEvents) { 
                HookPropertyChanged(item); 
            }
  
            FireListChanged(ListChangedType.ItemChanged, index);
        }

        public override int RemoveAll(Predicate<T> match, int timeout)
        {
            int removed = 0;
            if (!RWLock.TryEnterWriteLock(timeout))
                throw new TimeoutException("Write lock timeout.") { Source = _ExSource + "InsertItem" };
            try
            {
                var tArr = _Items.ToArray();
                for (int i = 0; i < tArr.Count(); i++)
                {
                    var item = tArr[i];
                    if (match(item))
                    {
                        RemoveItem(i, timeout, false);
                        removed++;
                    }
                }
            }
            finally
            {
                RWLock.ExitWriteLock();
            } 
            return removed;
        }

 
        #endregion 
 
        #region ICancelAddNew interface 
  
        public virtual void CancelNew(int itemIndex)
        { 
            if (addNewPos >= 0 && addNewPos == itemIndex) {
                RemoveItem(addNewPos, -1); 
                addNewPos = -1; 
            }
        } 
 
        public virtual void EndNew(int itemIndex) 
        { 
            if (addNewPos >= 0 && addNewPos == itemIndex) {
                addNewPos = -1; 
            }
        }
 
        #endregion 
 
        #region IBindingList interface 
  
        public T AddNew() { 
            return (T)((this as IBindingList).AddNew());
        }
 
        object IBindingList.AddNew() { 
            // Create new item and add it to list
            object newItem = AddNewCore(); 
  
            // Record position of new item (to support cancellation later on)
            addNewPos = (newItem != null) ? IndexOf((T) newItem) : -1; 
 
            // Return new item to caller
            return newItem;
        } 
 
        private bool AddingNewHandled { 
            get { 
                return onAddingNew != null && onAddingNew.GetInvocationList().Length > 0;
            } 
        }
 
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2113:SecureLateBindingMethods")]
        protected virtual object AddNewCore() {
            // Allow event handler to supply the new item for us 
            object newItem = FireAddingNew();
  
            // If event hander did not supply new item, create one ourselves 
            if (newItem == null) {
  
                Type type = typeof(T);
                newItem = Activator.CreateInstance(type, null);
                //newItem = SecurityUtils.SecureCreateInstance(type);
            }
  
            // Add item to end of list. Note: If event handler returned an item not of type T,
            // the cast below will trigger an InvalidCastException. This is by design. 
            Add((T) newItem); 
 
            // Return new item to caller 
            return newItem;
        }
 
        public bool AllowNew { 
            get {
                //If the user set AllowNew, return what they set.  If we have a default constructor, allowNew will be 
                //true and we should just return true.
                if (userSetAllowNew || allowNew)
                {
                    return this.allowNew; 
                }
                //Even if the item doesn't have a default constructor, the user can hook AddingNew to provide an item. 
                //If there's a handler for this, we should allow new. 
                return AddingNewHandled;
            } 
            set {
                bool oldAllowNewValue = AllowNew;
                userSetAllowNew = true;
                //Note that we don't want to set allowNew only if AllowNew didn't match value, 
                //since AllowNew can depend on onAddingNew handler
                this.allowNew = value; 
                if (oldAllowNewValue != value) { 
                    FireListChanged(ListChangedType.Reset, -1);
                } 
            }
        }
 
        /* private */ bool IBindingList.AllowNew { 
            get {
                return AllowNew; 
            } 
        }
  
        public bool AllowEdit { 
            get {
                return this.allowEdit; 
            } 
            set {
                if (this.allowEdit != value) { 
                    this.allowEdit = value;
                    FireListChanged(ListChangedType.Reset, -1);
                }
            } 
        }
  
        /* private */ bool IBindingList.AllowEdit { 
            get {
                return AllowEdit; 
            }
        }
 
        public bool AllowRemove { 
            get {
                return this.allowRemove; 
            }
            set {
                if (this.allowRemove != value) {
                    this.allowRemove = value; 
                    FireListChanged(ListChangedType.Reset, -1);
                } 
            } 
        }
  
        /* private */ bool IBindingList.AllowRemove {
            get {
                return AllowRemove;
            } 
        }
  
        bool IBindingList.SupportsChangeNotification { 
            get {
                return SupportsChangeNotificationCore; 
            }
        }
 
        protected virtual bool SupportsChangeNotificationCore { 
            get {
                return true; 
            } 
        }
  
        bool IBindingList.SupportsSearching {
            get {
                return SupportsSearchingCore;
            } 
        }
  
        protected virtual bool SupportsSearchingCore { 
            get {
                return false; 
            }
        }
 
        bool IBindingList.SupportsSorting { 
            get {
                return SupportsSortingCore; 
            } 
        }
  
        protected virtual bool SupportsSortingCore {
            get {
                return false;
            } 
        }
  
        bool IBindingList.IsSorted { 
            get {
                return IsSortedCore; 
            }
        }
 
        protected virtual bool IsSortedCore { 
            get {
                return false; 
            } 
        }
  
        PropertyDescriptor IBindingList.SortProperty {
            get {
                return SortPropertyCore;
            } 
        }
  
        protected virtual PropertyDescriptor SortPropertyCore { 
            get {
                return null; 
            }
        }
 
        ListSortDirection IBindingList.SortDirection { 
            get {
                return SortDirectionCore; 
            } 
        }
  
        protected virtual ListSortDirection SortDirectionCore {
            get {
                return ListSortDirection.Ascending;
            } 
        }
  
        void IBindingList.ApplySort(PropertyDescriptor prop, ListSortDirection direction) { 
            ApplySortCore(prop, direction);
        } 
 
        protected virtual void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction) {
            throw new NotSupportedException();
        } 
 
        void IBindingList.RemoveSort() { 
            RemoveSortCore(); 
        }
  
        protected virtual void RemoveSortCore() {
            throw new NotSupportedException();
        }
  
        int IBindingList.Find(PropertyDescriptor prop, object key) {
            return FindCore(prop, key); 
        } 
 
        protected virtual int FindCore(PropertyDescriptor prop, object key) { 
            throw new NotSupportedException();
        }
 
        void IBindingList.AddIndex(PropertyDescriptor prop) { 
            // Not supported
        } 
  
        void IBindingList.RemoveIndex(PropertyDescriptor prop) {
            // Not supported 
        }
 
        #endregion
  
        #region Property Change Support
  
        private void HookPropertyChanged(T item) { 
            INotifyPropertyChanged inpc = (item as INotifyPropertyChanged);
  
            // Note: inpc may be null if item is null, so always check.
            if (null != inpc) {
                if (propertyChangedEventHandler == null) {
                    propertyChangedEventHandler = new PropertyChangedEventHandler(Child_PropertyChanged); 
                }
                inpc.PropertyChanged += propertyChangedEventHandler; 
            } 
        }
  
        private void UnhookPropertyChanged(T item) {
            INotifyPropertyChanged inpc = (item as INotifyPropertyChanged);
 
            // Note: inpc may be null if item is null, so always check. 
            if (null != inpc && null != propertyChangedEventHandler) {
                inpc.PropertyChanged -= propertyChangedEventHandler; 
            } 
        }
  
        void Child_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (this.RaiseListChangedEvents) {
                if (sender == null || e == null || string.IsNullOrEmpty(e.PropertyName)) {
                    // Fire reset event (per INotifyPropertyChanged spec) 
                    ResetBindings();
                } 
                else { 
                    // The change event is broken should someone pass an item to us that is not
                    // of type T.  Still, if they do so, detect it and ignore.  It is an incorrect 
                    // and rare enough occurrence that we do not want to slow the mainline path
                    // with "is" checks.
                    T item;
  
                    try {
                        item = (T)sender; 
                    } 
                    catch(InvalidCastException ec)
                    {
                        string msg = ec.Message;
                        if (ec.InnerException != null && ec.InnerException.Message != null)
                            msg += " Inner:" + ec.InnerException.Message;

                        if (Database.Root != null && Database.Root.Messages != null)
                            Database.Root.Messages.LogException("SafeBindingList", "Child_PropertyChanged", msg);

                        ResetBindings(); 
                        return;
                    }
 
                    // Find the position of the item.  This should never be -1.  If it is, 
                    // somehow the item has been removed from our list without our knowledge.
                    int pos = lastChangeIndex; 
  
                    if (pos < 0 || pos >= Count || !this[pos].Equals(item)) {
                        pos = this.IndexOf(item); 
                        lastChangeIndex = pos;
                    }
 
                    if (pos == -1) { 
                        Debug.Fail("Item is no longer in our list but we are still getting change notifications.");
                        UnhookPropertyChanged(item); 
                        ResetBindings(); 
                    }
                    else { 
                        // Get the property descriptor
                        if (null == this.itemTypeProperties) {
                            // Get Shape
                            itemTypeProperties = TypeDescriptor.GetProperties(typeof(T)); 
                            Debug.Assert(itemTypeProperties != null);
                        } 
  
                        PropertyDescriptor pd = itemTypeProperties.Find(e.PropertyName, true);
  
                        // Create event args.  If there was no matching property descriptor,
                        // we raise the list changed anyway.
                        ListChangedEventArgs args = new ListChangedEventArgs(ListChangedType.ItemChanged, pos, pd);
  
                        // Fire the ItemChanged event
                        OnListChanged(args); 
                    } 
                }
            } 
        }
 
        #endregion
  
        #region IRaiseItemChangedEvents interface
  
        bool IRaiseItemChangedEvents.RaisesItemChangedEvents { 
            get {
                return this.raiseItemChangedEvents; 
            } 
        }
  
        #endregion
 
    }
} 
 