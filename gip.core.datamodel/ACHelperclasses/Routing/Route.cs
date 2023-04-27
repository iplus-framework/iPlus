using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace gip.core.datamodel
{
    [ACSerializeableInfo]
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Route'}de{'Route'}", Global.ACKinds.TACSimpleClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class Route : IReadOnlyList<RouteItem>, IACAttach, ICloneable
    {
        #region const
        public const string ClassName = "Route";
        #endregion

        #region private fields

        [DataMember]
        private List<RouteItem> _Items;

        #endregion

        #region Properties 

        [IgnoreDataMember]
        public int Count
        {
            get { return _Items.Count; }
        }

        public RouteItem this[int index]
        {
            get { return _Items[index]; }
        }

        public List<RouteItem> Items
        {
            get
            {
                return _Items;
            }
        }

        [IgnoreDataMember]
        [ACPropertyInfo(999,"","en{'Route'}de{'Route'}")]
        public string RouteName
        {
            get { return ToString(); }
        }

        #endregion

        #region Constructors

        public Route(RouteItem item)
        {
            _Items = new List<RouteItem>(new RouteItem[] {item});
        }

        public Route(IEnumerable<RouteItem> items)
        {
            _Items = new List<RouteItem>(items);
        }


        public Route()
        {
            _Items = new List<RouteItem>();
        }

        #endregion

        #region Methods

        #region public

        public IEnumerator<RouteItem> GetEnumerator()
        {
            return _Items.GetEnumerator();
        }

        public IEnumerable<IACComponent> GetSourceComponentsOfRouteSources()
        {
            if (!IsAttached)
                return null;

            return this.Where(s => !this.Any(t => t.TargetKey == s.SourceKey)).Select(c => c.SourceACComponent);
        }

        //public IACComponent GetTargetComponent(Database db)
        //{
        //    RouteItem targetRouteItem = this.LastOrDefault();
        //    if (targetRouteItem == null)
        //        return null;
        //    if (!targetRouteItem.IsAttached && db != null)
        //        targetRouteItem.AttachTo(db);
        //    return targetRouteItem.TargetACComponent;
        //}

        public static Route MergeRoutes(IEnumerable<Route> routes)
        {
            List<RouteItem> routeItems = new List<RouteItem>();
            int routeNo = 0;
            foreach (var route in routes)
            {
                foreach (var routeItem in route)
                {
                    routeItem.RouteNo = routeNo;
                    routeItems.Add(routeItem);
                }
                routeNo++;
            }
            return new Route(routeItems);
        }

        public static Route MergeRoutesWithoutDuplicates(IEnumerable<Route> routes)
        {
            if (routes == null)
                return null;

            List<RouteItem> routeItems = new List<RouteItem>();
            int routeNo = 0;
            foreach (var route in routes)
            {
                foreach (var routeItem in route)
                {
                    if (!routeItems.Any(c => c.SourceKey == routeItem.SourceKey && c.TargetKey == routeItem.TargetKey))
                    {
                        routeItem.RouteNo = routeNo;
                        routeItems.Add(routeItem);
                    }
                }
                routeNo++;
            }
            return new Route(routeItems);
        }

        public bool TryGetFirstDifferentComponent(Route oldRoute, out IACComponent diffComponent)
        {
            diffComponent = null;

            if (oldRoute == null)
                return false;

            IEnumerable<RouteItem> newSources = this.GetRouteSources();
            IEnumerable<RouteItem> oldSources = this.GetRouteSources();

            foreach(RouteItem newSource in newSources)
            {
                RouteItem oldSource = oldSources.FirstOrDefault(o => o.Equals(newSource));
                if (oldSource != null)
                {
                    var key = CheckForFirstDifferentComponent(newSource.SourceKey, oldSource.SourceKey, oldRoute);
                    if (key == null)
                        return true;

                    var rItem = this.FirstOrDefault(c => c.SourceKey == key);
                    if (rItem != null)
                        diffComponent = rItem.SourceACComponent;
                    else
                    {
                        rItem = this.FirstOrDefault(c => c.TargetKey == key);
                        if (rItem == null)
                            return false;
                        diffComponent = rItem.TargetACComponent;
                    }
                    return true;
                }
            }
            return false;
        }

        private System.Data.EntityKey CheckForFirstDifferentComponent(System.Data.EntityKey newSource, System.Data.EntityKey oldSource, Route oldRoute)
        {
            var newSourceItems = this.Where(c => c.SourceKey == newSource);
            var oldSourceItems = oldRoute.Where(c => c.SourceKey == oldSource);

            if (!newSourceItems.SequenceEqual(oldSourceItems))
                return newSource;

            var newTargetItems = newSourceItems.Select(t => t.TargetKey);
            var oldTargetItems = oldSourceItems.Select(t => t.TargetKey);

            foreach(var newTarget in newTargetItems)
            {
                var oldTarget = oldTargetItems.FirstOrDefault(c => c == newTarget);
                if (oldTarget == null)
                    return newSource;

                var newTargetSources = this.Where(c => c.TargetKey == newTarget);
                var oldTargetSources = oldRoute.Where(c => c.TargetKey == oldTarget);

                if (!newTargetSources.SequenceEqual(oldTargetSources))
                    return newTarget;

                return CheckForFirstDifferentComponent(newTarget, oldTarget, oldRoute);
            }
            return null;
        }

        public RouteItem GetRouteSource()
        {
            return this.FirstOrDefault(r => !this.Any(t => r.SourceKey == t.TargetKey));
        }

        public IEnumerable<RouteItem> GetRouteSources()
        {
            return this.Where(r => !this.Any(t => r.SourceKey == t.TargetKey));
        }

        public RouteItem GetRouteTarget()
        {
            return this.LastOrDefault(r => !this.Any(t => r.TargetKey == t.SourceKey));
        }

        public IEnumerable<RouteItem> GetRouteTargets()
        {
            return this.Where(r => !this.Any(t => r.TargetKey == t.SourceKey)).ToList();
        }

        public override string ToString()
        {
            var groups = this.GroupBy(c => c.RouteNo);
            var last = groups.LastOrDefault();
            string result = "";
            foreach (var group in groups)
            {
                RouteItem source = this.FirstOrDefault();
                RouteItem target = this.LastOrDefault();
                if (source == null || target == null)
                    continue;
                var sourceComp = source.SourceACComponent;
                var targetComp = target.TargetACComponent;
                result += string.Format(" {0} -> {1}{2}", sourceComp != null ? sourceComp.GetACUrl() : "NULL", targetComp != null ? targetComp.GetACUrl() : "NULL", group.Key == last.Key ? "" : ", ");
            }
            return string.IsNullOrEmpty(result) ? base.ToString() : result;

            //RouteItem source = this.FirstOrDefault();
            //RouteItem target = this.LastOrDefault();
            //if (source == null || target == null)
            //    return base.ToString();
            //var sourceComp = source.SourceACComponent;
            //var targetComp = target.TargetACComponent;
            //if (sourceComp != null && targetComp != null)
            //{
            //    return sourceComp.GetACUrl() + " -> " + targetComp.GetACUrl();
            //}
        }

        #endregion

        #region Private

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region IAttach

        /// <summary>Attaches the deserialized encapuslated objects to the parent context.</summary>
        /// <param name="parentACObject">The parent context. Normally this is a EF-Context (IACEntityObjectContext).</param>
        public void AttachTo(IACObject parentACObject)
        {
            IACEntityObjectContext context = parentACObject as IACEntityObjectContext;
            if (context == null)
                return;
            _Items.ForEach(c => c.AttachTo(context));
            if (ObjectAttached != null)
                ObjectAttached(this, new EventArgs());
        }

        /// <summary>Detaches the encapuslated objects from the parent context.</summary>
        /// <param name="detachFromContext">If attached object is a Entity object, then it will be detached from Change-Tracking if this parameter is set to true.</param>
        public void Detach(bool detachFromContext = false)
        {
            if (ObjectDetaching != null)
                ObjectDetaching(this, new EventArgs());
            _Items.ForEach(c => c.Detach());
            if (ObjectDetached != null)
                ObjectDetached(this, new EventArgs());
        }

        /// <summary>Gets a value indicating whether the encapuslated objects are attached.</summary>
        /// <value>
        ///   <c>true</c> if the encapuslated objects are attached; otherwise, <c>false</c>.</value>
        public bool IsAttached
        {
            get { return !_Items.Where(c => !c.IsAttached).Any(); }
        }

        public void DetachEntitesFromDbContext()
        {
            _Items.ForEach(c => c.DetachEntitesFromDbContext());
        }

        public bool IsDetachedFromDbContext
        {
            get { return !_Items.Where(c => !c.IsDetachedFromDBContext).Any(); }
        }

        /// <summary>
        /// Occurs when encapuslated objects were detached.
        /// </summary>
        public event EventHandler ObjectDetached;

        /// <summary>
        /// Occurs before the deserialized content will be attached to be able to access the encapuslated objects later.
        /// </summary>
        public event EventHandler ObjectDetaching;

        /// <summary>
        /// Occurs when encapuslated objects were attached.
        /// </summary>
        public event EventHandler ObjectAttached;

        #endregion

        #endregion

        #region Cloning

        public object Clone()
        {
            return new Route(this._Items.Select(c => c.Clone() as RouteItem));
        }

        public Route Clone(IACEntityObjectContext attachTo)
        {
            Route clone = this.Clone() as Route;
            if (attachTo != null)
                clone.AttachTo(attachTo);
            return clone;
        }

        #endregion
    }
}
