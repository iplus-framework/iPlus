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
        public const string ClassName = nameof(Route);
        #endregion

        #region Constructors

        public Route(RouteItem item)
        {
            _Items = new List<RouteItem>(new RouteItem[] { item });
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

        [DataMember]
        private bool _IsPredefinedRoute = false;
        [IgnoreDataMember]
        public bool IsPredefinedRoute
        {
            get => _IsPredefinedRoute;
            set => _IsPredefinedRoute = value;
        }

        [DataMember]
        private bool? _HasAnyReserved;
        [IgnoreDataMember]
        public bool? HasAnyReserved
        {
            get
            {
                return _HasAnyReserved;
            }
            set
            {
                _HasAnyReserved = value;
            }
        }

        [DataMember]
        private bool? _HasAnyAllocated;
        [IgnoreDataMember]
        public bool? HasAnyAllocated
        {
            get
            {
                return _HasAnyAllocated;
            }
            set
            {
                _HasAnyAllocated = value;
            }
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

        public static Route IntersectRoutesGetDiff(Route routeA, Route routeB_forRemovingItemsA, bool removeIfSourceOrTarget = false)
        {
            if (routeA == null || routeB_forRemovingItemsA == null)
                return null;

            Route intersectedRoute = (Route)routeA.Clone();
            List<RouteItem> diffItems = intersectedRoute.ToList();
            foreach (var itemInA in intersectedRoute)
            {
                if (routeB_forRemovingItemsA.Where(c => (!removeIfSourceOrTarget && c.SourceKey == itemInA.SourceKey && c.TargetKey == itemInA.TargetKey)
                                                     || (removeIfSourceOrTarget && (c.SourceKey == itemInA.SourceKey || c.TargetKey == itemInA.TargetKey)))
                                            .Any())
                      diffItems.Remove(itemInA);
            }
            return new Route(diffItems);
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

        public static IEnumerable<Route> SplitRoute(Route route)
        {
            List<Route> result = new List<Route>();
            var groups = route.GroupBy(c => c.RouteNo);
            foreach (var group in groups)
                result.Add(new Route(group));
            return result;
        }

        public static IEnumerable<Route> SplitRouteWithDuplicates(Route route)
        {
            List<Route> result = new List<Route>();
            var groups = route.GroupBy(c => c.RouteNo);

            var sources = route.GetRouteSources();
            var targets = route.GetRouteTargets();

            Route mainRoute = null;
            IGrouping<int, RouteItem> mainGroup = null;

            foreach (var source in sources)
            {
                foreach (var target in targets)
                {
                    mainGroup = groups.FirstOrDefault(c => c.Key == source.RouteNo && c.Key == target.RouteNo);
                    mainRoute = new Route(mainGroup);

                    break;
                }

                if (mainRoute != null)
                    break;
            }

            foreach (var group in groups)
            {
                if (group == mainGroup)
                    continue;

                Route r = new Route(group);

                var tempSource = r.GetRouteSource();
                var tempTarget = r.GetRouteTarget();

                //var startFromMainRoute = 


            }

            return result;
        }

        /// <summary>
        /// Compares two routes. Returns true, if both are equal or if Branch found
        /// </summary>
        /// <param name="oldRoute">Secnond rout to compare</param>
        /// <param name="branchesAt">Component where both routes start to be different</param>
        /// <returns>True if equal or if branch found</returns>
        public bool Compare(Route oldRoute, out IACComponent branchesAt)
        {
            branchesAt = null;
            if (oldRoute == null)
                return false;

            IEnumerable<RouteItem> newSources = this.GetRouteSources();
            IEnumerable<RouteItem> oldSources = oldRoute.GetRouteSources();

            foreach (RouteItem newSource in newSources)
            {
                RouteItem oldSource = oldSources.FirstOrDefault(o => o.Equals(newSource));
                if (oldSource != null)
                {
                    var key = FindFirstDifference(newSource.SourceKey, oldSource.SourceKey, oldRoute);
                    if (key == null)
                        return true;
                    var rItem = this.FirstOrDefault(c => c.SourceKey == key);
                    if (rItem != null)
                        branchesAt = rItem.SourceACComponent;
                    else
                    {
                        rItem = this.FirstOrDefault(c => c.TargetKey == key);
                        if (rItem == null)
                            return false;
                        branchesAt = rItem.TargetACComponent;
                    }
                    return true;
                }
            }
            return false;
        }

        private EntityKey FindFirstDifference(EntityKey newSource, EntityKey oldSource, Route oldRoute)
        {
            var newSourceItems = this.Where(c => c.SourceKey == newSource).OrderBy(c => c.SourceGuid).ThenBy(c => c.TargetGuid);
            var oldSourceItems = oldRoute.Where(c => c.SourceKey == oldSource).OrderBy(c => c.SourceGuid).ThenBy(c => c.TargetGuid);

            if (!newSourceItems.SequenceEqual(oldSourceItems))
                return newSource;

            var newTargetItems = newSourceItems.Select(t => t.TargetKey);
            var oldTargetItems = oldSourceItems.Select(t => t.TargetKey);

            foreach (var newTarget in newTargetItems)
            {
                var oldTarget = oldTargetItems.FirstOrDefault(c => c == newTarget);
                if (oldTarget == null)
                    return newSource;

                var newTargetSources = this.Where(c => c.TargetKey == newTarget).OrderBy(c => c.SourceGuid).ThenBy(c => c.TargetGuid);
                var oldTargetSources = oldRoute.Where(c => c.TargetKey == oldTarget).OrderBy(c => c.SourceGuid).ThenBy(c => c.TargetGuid);

                if (!newTargetSources.SequenceEqual(oldTargetSources))
                    return newTarget;

                return FindFirstDifference(newTarget, oldTarget, oldRoute);
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
                RouteItem source = this.GetRouteSource();
                RouteItem target = this.GetRouteTarget();
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
        
        public int GetRouteHash()
        {
            var sources = this.GetRouteSources().ToList();
            var targets = this.GetRouteTargets().ToList();

            foreach (var source in sources)
                this.Items.Remove(source);

            foreach (var target in targets)
                this.Items.Remove(target);

            List<Guid> guids = null;

            if (!this.Items.Any())
            {
                guids = new List<Guid>(targets.Select(c => c.SourceGuid));
            }
            else
            {
                var newTargets = this.GetRouteTargets().ToList();

                guids = this.Select(c => c.SourceGuid).ToList();
                foreach (var target in newTargets)
                {
                    guids.Add(target.TargetGuid);
                }
            }

            this.Items.InsertRange(0, sources);
            this.Items.AddRange(targets);

            if (guids == null || !guids.Any())
                return 0;

            return string.Join("", guids).GetHashCode();
        }

        public string GetRouteItemsHash()
        {
            var targets = this.GetRouteTargets().ToList();

            List<Guid> guids = this.Select(c => c.SourceGuid).ToList();
            foreach (var target in targets)
            {
                guids.Add(target.TargetGuid);
            }

            string result = "";

            foreach (Guid guid in guids)
            {
                result += guid.GetHashCode() + ",";
            }

            return result;
        }

        public string GetRouteItemsGuid()
        {
            var targets = this.GetRouteTargets().ToList();

            List<Guid> guids = this.Select(c => c.SourceGuid).ToList();
            foreach (var target in targets)
            {
                guids.Add(target.TargetGuid);
            }

            string result = "";

            foreach (Guid guid in guids)
            {
                result += guid + ",";
            }

            return result;
        }

        public (bool reserved, bool allocated) GetReservedAndAllocated(IACComponent acRoutingService)
        {
            if (Count == 0)
            {
                _HasAnyReserved = false;
                _HasAnyAllocated = false;
            }

            if (_HasAnyReserved.HasValue && _HasAnyAllocated.HasValue)
                return (_HasAnyReserved.Value, _HasAnyAllocated.Value);

            if (acRoutingService == null)
                return (false, false);

            Route route = acRoutingService.ExecuteMethod("GetAllocatedAndReserved", this) as Route;
            if (route != null)
            {
                _HasAnyReserved = route.HasAnyReserved;
                _HasAnyAllocated = route.HasAnyAllocated;
            }

            return (HasAnyReserved.HasValue? HasAnyReserved.Value : false, HasAnyAllocated.HasValue? HasAnyAllocated.Value : false);
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
        [IgnoreDataMember]
        public bool IsAttached
        {
            get { return !_Items.Where(c => !c.IsAttached).Any(); }
        }

        public void DetachEntitesFromDbContext()
        {
            _Items.ForEach(c => c.DetachEntitesFromDbContext());
        }

        [IgnoreDataMember]
        public bool IsDetachedFromDbContext
        {
            get { return !_Items.Where(c => !c.IsDetachedFromDBContext).Any(); }
        }

        [IgnoreDataMember]
        public bool AreACUrlInfosSet
        {
            get { return !_Items.Where(c => !c.AreACUrlInfosSet).Any(); }
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
            return new Route(this._Items.Select(c => c.Clone() as RouteItem)) { IsPredefinedRoute = this.IsPredefinedRoute };
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
