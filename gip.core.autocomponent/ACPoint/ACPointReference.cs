using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Class for managing references from other ACComponent's using ACRef.
    /// Also it manages References from WPFControls which Bind ACComponets via WPF-Binding
    /// </summary>
    public class ACPointReference : ACPointStorableBase<IACObject>, IACPointReference<IACObject> 
    {
        #region c'tors
        public ACPointReference(IACComponent parent, string acPropertyName) 
            : base(parent, acPropertyName, 0)
        {
        }

        /// <summary>
        /// Constructor for automatic Instantiation over Reflection in ACInitACPoint()
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="acClassProperty"></param>
        /// <param name="maxCapacity"></param>
        public ACPointReference(IACComponent parent, ACClassProperty acClassProperty, uint maxCapacity)
            : base(parent, acClassProperty, maxCapacity)
        {
        }
        #endregion


        #region Methods for ACRefs to IACComponents/IACObjects

        /// <summary>
        /// Broadcast a message to all references
        /// </summary>
        /// <param name="acURL"></param>
        /// <param name="acParameter"></param>
        public void Broadcast(string acURL, object[] acParameter)
        {
            using (ACMonitor.Lock(LockConnectionList_20030))
            {
                foreach (var acObject in this.UnsafeConnectionList)
                {
                    acObject.ACUrlCommand(acURL, acParameter);
                }
            }
        }

        /// <summary>
        /// Adds a reference
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override void Add(IACObject item)
        {
            if (ACRef.ValueT != null)
            {
                ACRef.ValueT.OnAddReference(item);
                if (_ACComponent.ValueT.InitState == ACInitState.Initializing
                    || _ACComponent.ValueT.InitState == ACInitState.Initialized
                    || _ACComponent.ValueT.InitState == ACInitState.RecyclingFromPool
                    || _ACComponent.ValueT.InitState == ACInitState.RecycledFromPool
                    || _ACComponent.ValueT.InitState == ACInitState.Reloaded)
                {
                    ACComponentProxy imAProxy = _ACComponent.ValueT as ACComponentProxy;
                    if (imAProxy != null)
                    {
                        IACContainerRef acRefToMe = item as IACContainerRef;
                        if (acRefToMe != null)
                        {
                            if (!acRefToMe.IsWeakReference)
                                imAProxy.SubscribeAtServer(false, true);
                        }
                        else
                            imAProxy.SubscribeAtServer(false, true);
                    }
                }
                else if (_ACComponent.ValueT.InitState == ACInitState.DisposedToPool)
                {
                    // TODO:
                    throw new InvalidOperationException("ACPointReference.Add() not allowed on Component which is disposed to component pool");
                }
                else if (_ACComponent.ValueT.InitState == ACInitState.Destructed)
                {
                    // TODO:
                    throw new InvalidOperationException("ACPointReference.Add() not allowed on Component which is destructed");
                }
                //else if (_ACComponent.Obj.InitState == ACInitState.DisposingToPool
                //    || _ACComponent.Obj.InitState == ACInitState.Destructing
                //    || _ACComponent.Obj.InitState == ACInitState.Reloading
                //    || _ACComponent.Obj.InitState == ACInitState.RecyclingFromPool)
                //{
                //}
            }
            base.Add(item);
        }

        /// <summary>
        /// Removes a reference
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool Remove(IACObject item)
        {
            if (_IsDetachingRef)
                return true;
            if (ACRef.ValueT != null)
                ACRef.ValueT.OnRemoveReference(item);
            if (!base.Remove(item))
                return false;
            if (ACRoot.SRoot.InitState == ACInitState.Initialized
                && !HasStrongReferences)
                GarbageCollector.Instance.AnalyzeAndSweep(_ACComponent.ValueT as ACComponent);
            return true;
        }

        private bool _IsDetachingRef = false;

        internal void DetachAndClear()
        {
            using (ACMonitor.Lock(_20600_WPFReferenceDictLock))
            {
                // Empty all references
                if (_WPFRefDict != null)
                    _WPFRefDict = null;
                if (_WPFRefDictReverse != null)
                    _WPFRefDictReverse = null;

                using (ACMonitor.Lock(LockConnectionList_20030))
                {
                    try
                    {
                        foreach (var acObjectClient in UnsafeConnectionList.ToList())
                        {
                            if (acObjectClient is IACContainerRef)
                            {
                                _IsDetachingRef = true;
                                (acObjectClient as IACContainerRef).Detach();
                                _IsDetachingRef = false;
                            }
                            else
                            {
                                acObjectClient.ACUrlCommand(Const.EventDeInit, this);
                            }
                        }

                        if (_ConnectionList != null)
                            _ConnectionList.Clear();
                    }
                    finally
                    {
                        _IsDetachingRef = false;
                    }
                }
            }
        }
        #endregion


        #region Methods Refs to WPF-Objects
        private readonly ACMonitorObject _20600_WPFReferenceDictLock = new ACMonitorObject(20600);
        private Dictionary<int, WPFReferencesToComp> _WPFRefDictReverse = null;
        private Dictionary<IACComponent, WPFReferencesToComp> _WPFRefDict = null;

        /// <summary>
        /// WPF-Control that register itself and the bounded object (in most cases a ACComponentProxy-Object) to this Reference-Point
        /// </summary>
        /// <param name="hashOfDepObj">Hashcode of the calling WPF-Control</param>
        /// <param name="boundedObject">ACComponent which is bound via WPF-Binding to the WPF-Control</param>
        public void AddWPFRef(int hashOfDepObj, IACObject boundedObject)
        {
            IACComponent boundedComponent = null;
            IACPropertyBase objectIsProperty = boundedObject as IACPropertyBase;
            if (objectIsProperty != null)
                boundedComponent = objectIsProperty.ParentACComponent;
            else
                boundedComponent = boundedObject as IACComponent;
            if (boundedComponent == null)
                return;
            if (ACRef.ValueT == null || !(ACRef.ValueT is IACBSO) || ACRef.ValueT == boundedComponent || !boundedComponent.IsProxy)
                return;
            IACBSO bso = ACRef.ValueT as IACBSO;
            if (bso.InitState != ACInitState.Initialized)
                return;
            WPFReferencesToComp wpfRef = null;

            using (ACMonitor.Lock(_20600_WPFReferenceDictLock))
            {
                if (_WPFRefDictReverse == null)
                    _WPFRefDictReverse = new Dictionary<int, WPFReferencesToComp>();
                if (_WPFRefDict == null)
                    _WPFRefDict = new Dictionary<IACComponent, WPFReferencesToComp>();
                if (_WPFRefDictReverse.TryGetValue(hashOfDepObj, out wpfRef))
                    return;
                if (!_WPFRefDict.TryGetValue(boundedComponent, out wpfRef))
                {
                    wpfRef = new WPFReferencesToComp(boundedComponent, bso);
                    _WPFRefDict.Add(boundedComponent, wpfRef);
                    Add(wpfRef);
                }
                wpfRef.AddWPFRef(hashOfDepObj);
                _WPFRefDictReverse.Add(hashOfDepObj, wpfRef);
            }
        }

        /// <summary>
        /// WPF-Control that removes itself
        /// </summary>
        /// <param name="hashOfDepObj">Hashcode of the calling WPF-Control</param>
        /// <returns></returns>
        public bool RemoveWPFRef(int hashOfDepObj)
        {
            if (ACRef.ValueT == null || !(ACRef.ValueT is IACBSO))
                return false;
            WPFReferencesToComp wpfRef = null;

            using (ACMonitor.Lock(_20600_WPFReferenceDictLock))
            {
                if (_WPFRefDictReverse == null || _WPFRefDict == null)
                    return false;
                if (!_WPFRefDictReverse.TryGetValue(hashOfDepObj, out wpfRef))
                    return false;
                wpfRef.RemoveWPFRef(hashOfDepObj);
                _WPFRefDictReverse.Remove(hashOfDepObj);
            }

            return true;
        }

        internal WPFReferencesToComp GetWPFRefsForComp(IACComponent boundedComponent)
        {
            if (_WPFRefDict == null)
                return null;
            WPFReferencesToComp wpfRef = null;

            using (ACMonitor.Lock(_20600_WPFReferenceDictLock))
            {
                _WPFRefDict.TryGetValue(boundedComponent, out wpfRef);
            }
            return wpfRef;
        }


        /// <summary>
        /// Is any WPF-Control bound to this component?
        /// </summary>
        /// <param name="boundedComponent">Component which ist bound via WPF-Binding to a WPFControl</param>
        /// <returns></returns>
        public bool HasWPFRefsForComp(IACComponent boundedComponent)
        {
            return GetWPFRefsForComp(boundedComponent) != null;
        }

        public void CheckWPFRefsAndDetachUnusedProxies()
        {
            using (ACMonitor.Lock(_20600_WPFReferenceDictLock))
            {
                if (_WPFRefDictReverse == null || _WPFRefDict == null)
                    return;
                foreach (var wpfRef in _WPFRefDict.Values.Where(c => !c.IsReferencedFromWPF).ToArray())
                {
                    if (wpfRef.ReferencedComp != null)
                    {
                        _WPFRefDict.Remove(wpfRef.ReferencedComp);
                        wpfRef.Detach();
                    }
                    Remove(wpfRef);
                }
            }
        }
        #endregion


        #region Properties
        /// <summary>
        /// A strong reference is a normal reference that protects the referred object from collection by the IPlus-GarbageCollector.
        /// Returns if there are any strong references. (Weak references are excluded)
        /// </summary>
        public bool HasStrongReferences
        {
            get
            {
                using (ACMonitor.Lock(LockConnectionList_20030))
                {
                    if (!UnsafeConnectionList.Any())
                        return false;
                    foreach (IACObject acObject in UnsafeConnectionList)
                    {
                        if (acObject is IACContainerRef)
                        {
                            IACContainerRef acRef = acObject as IACContainerRef;
                            if (acRef.IsWeakReference || (acObject is ACPSubscrObjBase))
                                continue;
                            IACMember member = acRef.ParentACObject as IACMember;
                            if (member != null)
                            {
                                if (_ACComponent.ValueT.HasMember(member))
                                    continue;
                            }
                        }
                        return true;
                        // Falls Referenz kein Child-Objekt ist, das durch ACRef eine Parent-Beziehung zu diese ACComponent besitzt,
                        // dann ist Objekt ausserhalb referenziert
                        //if (_ACComponent.Obj.GetMember(acObject.ACIdentifier) == null)
                        //return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// A strong reference is a normal reference that protects the referred object from collection by the IPlus-GarbageCollector.
        /// Returns the count of strong references. (Weak references are excluded)
        /// </summary>
        public int CountStrongReferences
        {
            get
            {
                int count = 0;

                using (ACMonitor.Lock(LockConnectionList_20030)) 
                {
                    if (!UnsafeConnectionList.Any())
                        return count;
                    foreach (IACObject acObject in UnsafeConnectionList)
                    {
                        if (acObject is IACContainerRef)
                        {
                            IACContainerRef acRef = acObject as IACContainerRef;
                            if (acRef.IsWeakReference || (acObject is ACPSubscrObjBase))
                                continue;
                            IACMember member = acRef.ParentACObject as IACMember;
                            if (member != null)
                            {
                                if (_ACComponent.ValueT.HasMember(member))
                                    continue;
                            }
                        }
                        count++;
                        // Falls Referenz kein Child-Objekt ist, das durch ACRef eine Parent-Beziehung zu diese ACComponent besitzt,
                        // dann ist Objekt ausserhalb referenziert
                        //if (_ACComponent.Obj.GetMember(acObject.ACIdentifier) == null)
                        //return true;
                    }
                }
                return count;
            }
        }
        #endregion
    }


    internal class WPFReferencesToComp : ACRef<IACComponent>
    {
        public WPFReferencesToComp(IACComponent referencedComp, IACBSO presentingBSO)
            : base(referencedComp, presentingBSO, false)
        { 
        }

        //private object _Lock;
        private List<int> _HashCodesOfVisual = new List<int>();
        public List<int> HashCodesOfVisuals
        {
            get
            {
                return _HashCodesOfVisual;
            }
        }

        public IACComponent ReferencedComp
        {
            get
            {
                return ValueT;
            }
        }

        public IACBSO PresentingBSO
        {
            get
            {
                return ParentACObject as IACBSO;
            }
        }

        public bool IsReferencedFromWPF
        {
            get
            {
                return _HashCodesOfVisual.Any();
            }
        }

        public void AddWPFRef(int hashCode)
        {
            // Is threadsafe through _20600_WPFReferenceDictLock outside 
            //lock (_Lock)
            //{
            if (!_HashCodesOfVisual.Contains(hashCode))
                    _HashCodesOfVisual.Add(hashCode);
            //}
        }

        public void RemoveWPFRef(int hashCode)
        {
            // Is threadsafe through _20600_WPFReferenceDictLock outside 
            //lock (_Lock)
            //{
            _HashCodesOfVisual.Remove(hashCode);
            //}
        }

        //public void Detach()
        //{
        //    if (_ReferencedComp != null)
        //        _ReferencedComp.Detach();
        //}

        //public void SubscribeAtServer()
        //{
        //    if (_ReferencedComp != null && !_ReferencedComp.IsWeakReference)
        //        (_ReferencedComp.ValueT as ACComponent).SubscribeAtServer(false, true);
        //}
    }
}
