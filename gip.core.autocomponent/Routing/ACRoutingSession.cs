using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Converters;
using gip.core.datamodel;
using Microsoft.Isam.Esent.Interop;

namespace gip.core.autocomponent
{
    public class ACRoutingSession
    {
        #region c'tors

        public ACRoutingSession(ACRoutingService service)
        {
            _ACRoutingService = service;
        }

        #endregion

        #region Private members

        /// <summary>
        /// Collections of Sidetracks in graph, ordered by total delta
        /// </summary>
        private PriorityQueue<ST_Node> PathsHeap;

        /// <summary>
        /// Flag to indicate if shortest paths have been recalculated
        /// </summary>
        private bool Ready;

        private Dictionary<IACComponent, ACRoutingVertex> _RoutingVertexList;
        private Dictionary<IACComponent, ACRoutingVertex> RoutingVertexList
        {
            get
            {
                if (_RoutingVertexList == null)
                    _RoutingVertexList = new Dictionary<IACComponent, ACRoutingVertex>();
                return _RoutingVertexList;
            }
        }

        private List<ACRoutingPath> _RoutingPaths;
        private List<ACRoutingPath> RoutingPaths
        {
            get
            {
                if (_RoutingPaths == null)
                    _RoutingPaths = new List<ACRoutingPath>();
                return _RoutingPaths;
            }
        }

        private List<ACRoutingPath> checkedEdges = new List<ACRoutingPath>();

        private string _SelectionRuleID;
        private object[] _SelectionRuleParams = new object[] { };

        private int _MaxRouteAlternatives = 1;
        private int _MaxRouteLoopDepth = 2;

        bool _anyLoop = false;

        private ACRoutingService _ACRoutingService;
        private string _DiagnosticData = "";

        private class LoopItem
        {
            public LoopItem(PAEdge e)
            {
                Edge = e;
                LoopCounter = 1;
            }

            public PAEdge Edge
            {
                get;
                set;
            }

            public int LoopCounter
            {
                get;
                set;
            }
        }

        Route _PreviousRoute = null;

        ACRoutingParameters _RoutingParameters = null;

        #endregion

        #region Properties

        public ACRoutingVertex Source
        {
            get;
            set;
        }

        public ACRoutingVertex Target
        {
            get;
            set;
        }

        #endregion

        #region Methods

        public Tuple<List<ACRoutingVertex>, PriorityQueue<ST_Node>> BuildRoutes(ACRoutingVertex startVertex, ACRoutingVertex endVertex, int maxRouteAlternatives,
                                                                               bool includeReserved, bool includeAllocated, bool isForEditor = false, string selectionRuleID = null, object[] selectionRuleParams = null)
        {
            _SelectionRuleID = selectionRuleID;
            _SelectionRuleParams = selectionRuleParams != null ? selectionRuleParams : new object[] { };

            BuildAvailableRoutes(startVertex, endVertex, maxRouteAlternatives, includeReserved, includeAllocated, isForEditor);
            return new Tuple<List<ACRoutingVertex>, PriorityQueue<ST_Node>>(RoutingVertexList.Values.Where(c => c.Distance != int.MinValue).ToList(), PathsHeap);
        }

        public RoutingResult FindAvailableComponents(ACRoutingVertex startVertex, string selectionRuleID, RouteDirections direction, object[] selectionRuleParams, bool includeReserved, 
                                                     bool includeAllocated, bool includeFullRoute = false)
        {
            _SelectionRuleID = selectionRuleID;
            _SelectionRuleParams = selectionRuleParams != null ? selectionRuleParams : new object[] { };
            if (direction == RouteDirections.Backwards)
            {
                Target = startVertex;
                return FindComponents(includeReserved, includeAllocated, direction, includeFullRoute);
            }
            else
            {
                Source = startVertex;
                return FindComponents(includeReserved, includeAllocated, direction, includeFullRoute);
            }
        }

        public RoutingResult FindRoute(IEnumerable<ACRoutingVertex> startVertices, IEnumerable<ACRoutingVertex> endVertices, ACRoutingParameters routingParameters)
        {
            RoutingPaths.Clear();
            _MaxRouteAlternatives = routingParameters.MaxRouteAlternativesInLoop;
            _SelectionRuleID = routingParameters.SelectionRuleID;
            _SelectionRuleParams = routingParameters.SelectionRuleParams;
            _PreviousRoute = routingParameters.PreviousRoute;
            _MaxRouteLoopDepth = routingParameters.MaxRouteLoopDepth;

            _RoutingParameters = routingParameters;

            foreach (ACRoutingVertex start in startVertices)
            {
                Source = start;
                foreach (ACRoutingVertex end in endVertices)
                {
                    Target = end;
                    List<ACRoutingPath> tempPaths = new List<ACRoutingPath>();
                    ACRoutingPath tempPath = FindShortestPath(routingParameters.IncludeReserved, routingParameters.IncludeAllocated);
                    if (tempPath.Any())
                        RoutingPaths.Add(tempPath);
                    while (tempPath.IsValid)
                    {
                        tempPath = FindNextShortestPath();
                        if (tempPath.Any() && tempPath.End.ParentACComponent == Target.Component.ValueT)
                            RoutingPaths.Add(tempPath);
                    }
                }
            }

            var groupBySourceAndTarget = RoutingPaths.GroupBy(c => new Tuple<IACComponent, IACComponent>(c.Start.ParentACComponent, c.End.ParentACComponent));

            if ((RoutingPaths.Count > 1 && (startVertices.Count() > 1 || endVertices.Count() > 1) && groupBySourceAndTarget.Count() > 1))
            {
                var result = CheckMultipleSourcesTargets();
                if (result == null)
                {
                    Msg msg = new Msg("Missing a common component in multiple sources or/and targets mode. Route must have at least a one common component.", _ACRoutingService,
                                      eMsgLevel.Error, "ACRoutingSession", "FindRoute(10)", 137);
                    return new RoutingResult(null, false, msg);
                }
                return CreateRouteFromMultipleSourcesTargets(result);
            }
            else if (RoutingPaths.Any())
            {
                Msg msg = null;
                if (!string.IsNullOrEmpty(_DiagnosticData))
                {
                    msg = new Msg(_DiagnosticData, _ACRoutingService, eMsgLevel.Warning, "ACRoutingSession", "FindRoute(10)", 152);
                }

                return new RoutingResult(new Route[] { CreateRoute(RoutingPaths) }, false, msg);
            }

            string startCompsACUrl = string.Join(", ", startVertices.Select(c => c.Component.GetACUrl()));
            string endCompsACUrl = string.Join(", ", endVertices.Select(c => c.Component.GetACUrl()));

            string message = string.Format("Route is not found between start components with ACUrl {0} and end components with ACUrl {1}", startCompsACUrl, endCompsACUrl);
            if (!string.IsNullOrEmpty(_DiagnosticData))
            {
                message += System.Environment.NewLine + _DiagnosticData;
            }

            return new RoutingResult(null, false, new Msg(message, _ACRoutingService, eMsgLevel.Error, "ACRoutingSession", "FindRoute(20)", 161));
        }

        public RoutingResult FindSuccessors(ACRoutingVertex startVertex, ACRoutingParameters rp, RouteResultMode resultMode = RouteResultMode.FullRoute)
        {
            RoutingResult rResult = FindAvailableComponents(startVertex, rp.SelectionRuleID, rp.Direction, rp.SelectionRuleParams, rp.IncludeReserved, rp.IncludeAllocated, resultMode == RouteResultMode.FullRouteFromFindComp);
            if (rResult != null && rResult.Message != null && rResult.Message.MessageLevel > eMsgLevel.Warning)
                return rResult;

            if (rResult == null || !rResult.Components.Any())
            {
                string message = "Can't find successors according selection rule (" + rp.SelectionRuleID + ")";
                if (rResult != null && rResult.Message != null && rResult.Message.MessageLevel == eMsgLevel.Warning)
                    message += "Details: " + rResult.Message.Message;

                Msg msg = new Msg(message, _ACRoutingService, eMsgLevel.Error, "ACRoutingSession", "FindSuccesors(10)", 165);
                return new RoutingResult(null, false, msg);
            }

            IEnumerable<ACRoutingVertex> foundSuccessors = rResult.Components.Select(c => new ACRoutingVertex(c.ValueT));

            RoutingResult routingResult = null;
            bool runFullRoute = resultMode == RouteResultMode.FullRoute;

            if (rp.Direction == RouteDirections.Backwards)
            {
                if (resultMode == RouteResultMode.ShortRoute)
                {
                    List<Route> tempResult = new List<Route>();
                    PAEdge toEdge = startVertex.ToEdges.FirstOrDefault();
                    if (toEdge != null && toEdge.Relation != null)
                    {
                        foreach (var successor in foundSuccessors)
                        {
                            PAEdge fromEdge = successor.FromEdges.FirstOrDefault();
                            if (fromEdge == null || fromEdge.Relation == null)
                                continue;

                            RouteItem rItem = new RouteItem(fromEdge.Relation.SourceACClass, fromEdge.Relation.SourceACClassProperty, toEdge.Relation.TargetACClass, toEdge.Relation.TargetACClassProperty);
                            tempResult.Add(new Route(rItem));
                        }
                        routingResult = new RoutingResult(tempResult, false, null);
                    }
                    else
                        runFullRoute = true;
                }
                else if (resultMode == RouteResultMode.FullRouteFromFindComp)
                {
                    if (rResult.Routes != null && rResult.Routes.Any())
                        routingResult = rResult;
                    else
                        runFullRoute = true;
                }

                if (runFullRoute)
                {
                    routingResult = FindRoute(foundSuccessors, new ACRoutingVertex[] { startVertex }, rp);
                }
            }
            else
            {
                if (resultMode == RouteResultMode.ShortRoute)
                {
                    List<Route> tempResult = new List<Route>();
                    PAEdge fromEdge = startVertex.FromEdges.FirstOrDefault();
                    if (fromEdge != null && fromEdge.Relation != null)
                    {
                        foreach (var successor in foundSuccessors)
                        {
                            PAEdge toEdge = successor.ToEdges.FirstOrDefault();
                            if (toEdge == null || toEdge.Relation == null)
                                continue;

                            RouteItem rItem = new RouteItem(fromEdge.Relation.SourceACClass, fromEdge.Relation.SourceACClassProperty, toEdge.Relation.TargetACClass, toEdge.Relation.TargetACClassProperty);
                            tempResult.Add(new Route(rItem));
                        }

                        routingResult = new RoutingResult(tempResult, false, null);
                    }
                    else
                        runFullRoute = true;
                }
                else if (resultMode == RouteResultMode.FullRouteFromFindComp)
                {
                    if (rResult.Routes != null && rResult.Routes.Any())
                        routingResult = rResult;
                    else
                        runFullRoute = true;
                }


                if (runFullRoute)
                {
                    routingResult = FindRoute(new ACRoutingVertex[] { startVertex }, foundSuccessors, rp);
                }
            }

            if (routingResult != null && routingResult.Message == null && rResult != null && rResult.Message != null && rResult.Message.MessageLevel == eMsgLevel.Warning)
            {
                routingResult.Message = rResult.Message;
            }

            return routingResult;
        }

        private void BuildAvailableRoutes(ACRoutingVertex startVertex, ACRoutingVertex endVertex, int maxRouteAlternatives,
                                          bool includeReserved, bool includeAllocated, bool isForEditor = false)
        {
            Source = startVertex;
            Target = endVertex;
            _MaxRouteAlternatives = maxRouteAlternatives;
            BuildPaths(includeReserved, includeAllocated, isForEditor);
        }

        private IEnumerable<ACRoutingPath> CheckMultipleSourcesTargets()
        {
            var result = new List<ACRoutingPath>();

            foreach (var route in RoutingPaths)
            {
                var searchableRoutes = RoutingPaths.Where(c => c.Start.ParentACComponent != route.Start.ParentACComponent || c.End.ParentACComponent != route.End.ParentACComponent);

                if (searchableRoutes.Any(r => r.AllPoints.Select(x => x.ParentACComponent).Intersect(route.AllPoints.Select(c => c.ParentACComponent)).Any()))
                {
                    if (!result.Contains(route))
                        result.Add(route);
                }
            }

            return result.Any() ? result : null;
        }

        private Route CreateRoute(IEnumerable<ACRoutingPath> routingPaths)
        {
            if (!routingPaths.Any())
                return null;

            List<ACRoutingPath> paths = new List<ACRoutingPath>();
            List<RouteHashItem> routeHashItems = null;

            //PREVIOUS ROUTE
            if (_PreviousRoute != null)
            {
                List<Tuple<ACRoutingPath, int>> diffList = new List<Tuple<ACRoutingPath, int>>();

                foreach (ACRoutingPath path in routingPaths)
                {
                    if (!path.Any())
                        continue;

                    List<PAEdge> diffItems = path.ToList();
                    foreach (var itemInA in path)
                    {
                        ACClassPropertyRelation relation = itemInA.Relation;

                        if (_PreviousRoute.Where(c => (c.SourceGuid == relation.SourceACClassID || c.TargetGuid == relation.TargetACClassID)).Any())
                            diffItems.Remove(itemInA);
                    }

                    diffList.Add(new Tuple<ACRoutingPath, int>(path, diffItems.Count));
                }

                var groupedItem = diffList.GroupBy(c => c.Item2).OrderBy(c => c.Key).ToArray().FirstOrDefault();

                //Difference max
                if (groupedItem.Key != _PreviousRoute.Count)
                {
                    paths.AddRange(groupedItem.Select(c => c.Item1));
                }
            }

            bool runPreferences = _RoutingParameters == null || !_RoutingParameters.IgnorePreferences;

            //RULES (Parallel / NonParallel)
            if (runPreferences && !paths.Any() && routingPaths.Any(c => c.RouteItemsMode.Any()))
            {
                try
                {
                    List<ACRoutingPath> tempPaths = new List<ACRoutingPath>();
                    List<Tuple<IACObject, RouteItemModeEnum>> routeItemsMode = routingPaths.SelectMany(c => c.RouteItemsMode).Distinct().ToList();

                    int routingPathNo = 1;

                    foreach (ACRoutingPath path in routingPaths.OrderBy(x => x.DeltaWeight).Where(c => c.Any(x => routeItemsMode.Any(k => k.Item1 == x.SourceParent))))
                    {
                        ACRoutingPath newPath = new ACRoutingPath();
                        path.RoutingPathNo = routingPathNo;
                        newPath.RoutingPathNo = routingPathNo;
                        newPath.HasAnyAllocated = path.HasAnyAllocated;
                        newPath.HasAnyReserved = path.HasAnyReserved;

                        foreach (PAEdge edge in path)
                        {
                            if (!tempPaths.Any(x => x.Any(c => edge == c)))
                                newPath.Add(edge);
                        }

                        if (newPath.Any())
                        {
                            tempPaths.Add(newPath);
                            routingPathNo++;
                        }
                    }

                    ACRoutingPath workingPath = tempPaths.FirstOrDefault();
                    paths.Add(workingPath);

                    foreach (ACRoutingPath path in tempPaths)
                    {
                        if (path == workingPath)
                            continue;

                        ACRoutingPath mainPath = path;
                        int loopCounter = 0;

                        while (true)
                        {
                            if (loopCounter > 500)
                                break;

                            loopCounter++;

                            mainPath = tempPaths.FirstOrDefault(c => c != path && c.Any(k => k.SourceParent == mainPath.FirstOrDefault().SourceParent));

                            if (mainPath == null)
                                break;

                            int index = mainPath.IndexWhere(c => c.SourceParent == path.Start.ParentACObject);
                            if (index < 0 || index + 1 > mainPath.Count)
                                continue;

                            var mode = routeItemsMode.Where(c => mainPath.Take(index + 1).Any(x => x.SourceParent == c.Item1)).LastOrDefault();
                            if (mode != null)
                            {
                                if (mode.Item2 == RouteItemModeEnum.RouteItemsNextParallel)
                                {
                                    ACRoutingPath pathToAdd = routingPaths.FirstOrDefault(c => c.RoutingPathNo == path.RoutingPathNo);
                                    paths.Add(pathToAdd);
                                }
                            }

                            if (mainPath == workingPath)
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    _ACRoutingService.Messages.LogException(_ACRoutingService.GetACUrl(), nameof(ACRoutingSession) + "." + nameof(ACRoutingSession.CreateRoute), e);
                }
            }

            //ROUTE USAGE 
            if (!paths.Any())
            {
                if (runPreferences && _ACRoutingService.SelectRouteDependUsage)
                {
                    List<Guid> targetIDs = routingPaths.Select(c => c.LastOrDefault().Relation.SourceACClassID).Distinct().ToList();
                    routeHashItems = _ACRoutingService.GetMostUsedRouteHash(targetIDs);
                }

                List<int> tempHashCodeItems = new List<int>();
                if (routeHashItems != null && routeHashItems.Any())
                {
                    List<int> hashCodes = new List<int>();

                    foreach (var itemPath in routingPaths)
                    {
                        PAEdge source = itemPath.FirstOrDefault();
                        PAEdge target = itemPath.LastOrDefault();

                        itemPath.Remove(source);
                        itemPath.Remove(target);

                        List<Guid> temp = null;
                        if (itemPath.Any())
                        {
                            temp = itemPath.Select(c => c.Relation.SourceACClassID).ToList();
                            temp.Add(itemPath.LastOrDefault().Relation.TargetACClassID);

                        }
                        else
                            temp = new List<Guid>() { target.SourceParentComponent.ComponentClass.ACClassID };

                        int hash = string.Join("", temp).GetHashCode();

                        itemPath.Insert(0, source);
                        itemPath.Add(target);

                        var groupedByKey = routeHashItems.GroupBy(c => c.UseFactor).OrderByDescending(c => c.Key).FirstOrDefault();

                        if (groupedByKey.Any(c => c.RouteHashCodes.Any(x => x == hash)))
                            paths.Add(itemPath);
                    }
                }
            }

            if (paths == null || !paths.Any())
            {
                ACRoutingPath currentPath = routingPaths.OrderBy(k => k.DeltaWeight).FirstOrDefault();

                if (currentPath == null || !currentPath.Any())
                    currentPath = routingPaths.OrderBy(c => c.DeltaWeight).FirstOrDefault();

                paths.Add(currentPath);
            }

            List<RouteItem> tempList = new List<RouteItem>();

            bool hasAnyReserved = false, hasAnyAllocated = false;

            foreach (ACRoutingPath routingPath in paths)
            {
                if (routingPath.HasAnyAllocated)
                    hasAnyAllocated = true;

                if (routingPath.HasAnyReserved)
                    hasAnyReserved = true;

                int i = 0;
                foreach (PAEdge pathEdge in routingPath)
                    tempList.Add(new RouteItem(pathEdge.Relation) { RouteNo = i });

                i++;
            }

            return new Route(tempList) { HasAnyReserved = hasAnyReserved, HasAnyAllocated = hasAnyAllocated };
        }

        private RoutingResult CreateRouteFromMultipleSourcesTargets(IEnumerable<ACRoutingPath> routingPaths)
        {
            var groups = routingPaths.GroupBy(x => new { start = x.Start.ParentACComponent, end = x.End.ParentACComponent });
            List<Route> routes = new List<Route>();
            string msgHeader = "Routing group is empty in multiple start or/and end components mode:" + System.Environment.NewLine;
            string message = null;
            eMsgLevel messageLevel = eMsgLevel.Warning;

            foreach (var group in groups)
            {
                var route = CreateRoute(group);
                if (route != null)
                    routes.Add(route);
                else
                    message = group.Key.start.GetACUrl() + " -> " + group.Key.end.GetACUrl();
            }

            Msg msg = null;
            if (!string.IsNullOrEmpty(message))
                messageLevel = eMsgLevel.Error;

            if (!string.IsNullOrEmpty(_DiagnosticData))
                message += messageLevel == eMsgLevel.Error ? System.Environment.NewLine : "" + "Excluded by deselector :" + _DiagnosticData;

            if (!string.IsNullOrEmpty(message))
            {
                msg = new Msg(message, _ACRoutingService, messageLevel, "ACRoutingSession", "CreateRouteFromMultipeSourcesTargets(10)", 287);
            }

            return new RoutingResult(routes, false, msg);
        }

        #endregion

        #region Methods routing

        private void BuildPaths(bool includeReserved, bool includeAllocated, bool isForEditor = false)
        {
            Ready = false;
            BuildShortestPathTree(includeReserved, includeAllocated, isForEditor);
            BuildSidetracksHeap();
            Ready = true;
        }

        private ACRoutingPath FindShortestPath(bool includeReserved, bool includeAllocated)
        {
            Ready = false;

            // Builds shortest path tree for all vertices to t, according to Dijkstra,
            // storing distance to endpoint information on vertices, as described in Eppstein
            BuildShortestPathTree(includeReserved, includeAllocated, false, _DiagnosticData);
            // Fills a heap with all possible tracks from s to t, as described in Eppstein
            // Paths are defined uniquely by sidetrack collections (edges not in shortest paths) 
            BuildSidetracksHeap();
            // Flag to indicate that shortest paths have been calculated
            Ready = true;

            return FindNextShortestPath();
        }

        /// <summary>
        /// Recover next pre-calculated shortest path
        /// </summary>
        /// <returns>Directional path if available, empty path if not remaining paths</returns>
        private ACRoutingPath FindNextShortestPath()
        {
            if (!Ready)
                return new ACRoutingPath();  // Invalid path

            // Pick next track from heap, it is ordered from shortest path to longest
            ST_Node node = this.PathsHeap.Dequeue();
            if (node == null)
                return new ACRoutingPath(); // Invalid path

            // Returns path reconstructed from sidetracks
            return RebuildPath(node.Sidetracks, this.Source.Component.ValueT, RoutingVertexList.Values.ToList(), _ACRoutingService.GetRouteItemsMode());
        }

        /// <summary>
        /// Clears pointers to next edge in shortest path for all vertices
        /// Clears distances to endpoint in shortet path vor all vertices
        /// </summary>
        private void ResetGraphState()
        {
            foreach (var item in RoutingVertexList.Values)
                item.ResetRoutingState();
        }

        /// <summary>
        /// Builds the shortest path tree using a priority queue for given vertex
        /// </summary>
        /// <remarks>Negative edges are ignored</remarks>
        private void BuildShortestPathTree(bool includeReserved, bool includeAllocated, bool isForEditor = false, string diagnosticData = null)
        {
            SelectionRule selectionRule = ACRoutingService.GetSelectionQuery(_SelectionRuleID);

            ResetGraphState();  // Reset all distances to endpoint and previous shortest path

            ACRoutingVertex routingTargetVertex = this.Target;
            if (routingTargetVertex == null)
                return;

            routingTargetVertex.Distance = 0;   // Set distance to 0 for endpoint vertex
            routingTargetVertex.DistanceInLoop = 0;

            bool isSourceAndTargetSame = this.Source.Component.ValueT == this.Target.Component.ValueT;

            ACRoutingVertex tempSource; // = RoutingVertexList[Source.Component.ValueT];
            if (RoutingVertexList.TryGetValue(Source.Component.ValueT, out tempSource))
            {
                RoutingVertexList.Remove(Source.Component.ValueT);
                //RoutingVertexList.Remove(tempSource);
                tempSource = null;
            }
            RoutingVertexList.Add(Source.Component.ValueT, Source);

            ACRoutingVertex tempTarget; // = RoutingVertexList[Target.Component.ValueT]; //RoutingVertexList.FirstOrDefault(c => c.Component.ValueT == Target.Component.ValueT);
            if (RoutingVertexList.TryGetValue(Target.Component.ValueT, out tempTarget))
            {
                RoutingVertexList.Remove(Target.Component.ValueT);
                tempTarget = null;
            }
            RoutingVertexList.Add(Target.Component.ValueT, Target);

            // Creates a fringe (queue) for storing edge pending to be processed
            PriorityQueue<SP_Node> fringe = new PriorityQueue<SP_Node>();

            // Main loop
            do
            {
                if (routingTargetVertex != null)
                {
                    var edges = routingTargetVertex.RelatedEdges.Where(c => c.TargetParentComponent == routingTargetVertex.Component.ValueT).OrderBy(c => c.Weight).ToArray();
                    foreach (PAEdge edge in edges)
                    {
                        bool hasReserved, hasAllocated;

                        if (!ValidateRouteRelation(edge, includeReserved, includeAllocated, isForEditor, out hasAllocated, out hasReserved))
                            continue;

                        ACRoutingVertex currentVertex;
                        if (!RoutingVertexList.TryGetValue(edge.TargetParentComponent, out currentVertex))
                        {
                            if (edge.TargetParentComponent == Target.Component.ValueT)
                                continue;

                            currentVertex = new ACRoutingVertex(edge.Target.ParentACComponent as ACComponent);
                            RoutingVertexList.Add(edge.TargetParentComponent, currentVertex);
                        }

                        currentVertex.HasAnyAllocated = hasAllocated;
                        currentVertex.HasAnyReserved = hasReserved;

                        if (currentVertex.Component.ValueT == routingTargetVertex.Component.ValueT && edge.Weight >= 0)  // Ignore negative edges
                            fringe.Enqueue(new SP_Node(edge, edge.Weight + currentVertex.Distance));
                    }
                }

                SP_Node node = fringe.Dequeue();  // Extracts next element in queue
                if (node == null)  // No pending edges to evaluate, finished
                    break;

                PAEdge e = node.Edge;
                bool existInList = true;
                if (!RoutingVertexList.TryGetValue(e.SourceParentComponent, out routingTargetVertex))
                {
                    routingTargetVertex = new ACRoutingVertex(e.Source.ParentACComponent as ACComponent);
                    RoutingVertexList.Add(e.SourceParentComponent, routingTargetVertex);
                    existInList = false;
                }

                int distance = 0;
                if (existInList && routingTargetVertex.Distance != int.MinValue)
                {
                    ACRoutingVertex tempVertex = RoutingVertexList[e.TargetParentComponent];
                    if (tempVertex != null)
                        distance = e.Weight + tempVertex.Distance;
                }

                if ((routingTargetVertex.Distance == int.MinValue || (distance > 0 && distance < routingTargetVertex.Distance)
                      || VerifySourceTargetLoop(routingTargetVertex, distance, isSourceAndTargetSame))
                      && VerifyDeselector(selectionRule, routingTargetVertex, diagnosticData)) // Vertex distance to endpoint not calculated yet
                {
                    ACRoutingVertex tempVertex = RoutingVertexList[e.TargetParentComponent];
                    if (tempVertex == null)
                        tempVertex = new ACRoutingVertex(e.Target.ParentACComponent as ACComponent);

                    if (routingTargetVertex.Distance == 0)
                    {
                        routingTargetVertex.DistanceInLoop = distance > 0 ? distance : e.Weight + tempVertex.Distance;
                        if (isSourceAndTargetSame)
                        {
                            this.Source.Distance = routingTargetVertex.DistanceInLoop.Value;
                            this.Source.EdgeToPath = e;
                        }
                    }
                    else
                        routingTargetVertex.Distance = distance > 0 ? distance : e.Weight + tempVertex.Distance;
                    routingTargetVertex.EdgeToPath = e;
                }
                else
                    routingTargetVertex = null;
            } while (true);
        }

        private bool VerifyDeselector(SelectionRule selectionRule, ACRoutingVertex routingTargetVertex, string diagnosticData = null)
        {
            bool result = selectionRule == null || selectionRule.DeSelector == null
                          || !(this.Source.Component.ValueT != routingTargetVertex.Component.ValueT && selectionRule.DeSelector(routingTargetVertex, _SelectionRuleParams));

            if (!result && diagnosticData != null)
            {
                diagnosticData += routingTargetVertex.Component.ACUrl + "; ";
            }

            return result;
        }

        private bool VerifySourceTargetLoop(ACRoutingVertex routingTargetVertex, int distance, bool isSourceAndTargetSame)
        {
            return isSourceAndTargetSame && routingTargetVertex.Distance == 0 && routingTargetVertex.DistanceInLoop != null && routingTargetVertex.DistanceInLoop < distance;
        }

        /// <summary>
        /// Creates all posible paths by describing only the sidetracks for each path
        /// </summary>
        /// <returns></returns>
        private void BuildSidetracksHeap()
        {
            this.PathsHeap = new PriorityQueue<ST_Node>();
            ACRoutingPath empty = new ACRoutingPath();
            this.PathsHeap.Enqueue(new ST_Node(empty));
            AddSidetracks(empty, this.Source,0);
            if (_anyLoop)
                this.PathsHeap.Take(_MaxRouteAlternatives + 1);
        }

        private int _AddedSidetracks = 0;

        /// <summary>Adds sidetracks recursively for specified vertex and new vertices in shortest path</summary>
        /// <param name="rp">Previous sidetrack collection</param>
        /// <param name="routingVertex"></param>
        private void AddSidetracks(ACRoutingPath rp, ACRoutingVertex routingVertex, int depth)
        {
            if (routingVertex == null || depth > _MaxRouteLoopDepth)
                return;

            var edges = routingVertex.RelatedEdges.Where(c => c.SourceParentComponent == routingVertex.Component.ValueT).ToArray();

            foreach (PAEdge edge in OrderEdges(edges))
            {
                ACRoutingVertex targetVertex;
                if (!RoutingVertexList.TryGetValue(edge.TargetParentComponent, out targetVertex))
                    continue;

                if (edge.IsSidetrackOf(routingVertex)
                    && (targetVertex.EdgeToPath != null || edge.Target.ParentACComponent == this.Target.Component.ValueT)
                    && edge.Source.ParentACComponent != this.Target.Component.ValueT)
                {
                    ACRoutingPath p = new ACRoutingPath();
                    p.AddRange(rp);
                    p.Add(edge);

                    if (!CheckLoop(p) || edge.TargetParent == this.Source.Component.ValueT)
                        continue;
                    this.PathsHeap.Enqueue(new ST_Node(p));
                    _AddedSidetracks++;

                    if (edge.Target.ParentACComponent != routingVertex.Component.ValueT)  // This avoids infinite cycling
                        AddSidetracks(p, targetVertex, depth + 1);
                }
            }
            if (routingVertex.Next != null && routingVertex.Distance > 0 && !(_anyLoop && _AddedSidetracks >= _MaxRouteAlternatives))
                AddSidetracks(rp, RoutingVertexList[routingVertex.Next.ValueT], depth);
        }

        private IEnumerable<PAEdge> OrderEdges(IEnumerable<PAEdge> edges)
        {
            Dictionary<PAEdge, int> result = new Dictionary<PAEdge, int>();
            foreach (var edge in edges)
            {
                if (!result.ContainsKey(edge))
                {
                    ACRoutingVertex v;
                    if (RoutingVertexList.TryGetValue(edge.TargetParentComponent, out v))
                    {
                        if (v.Distance == int.MinValue)
                            result.Add(edge, int.MaxValue);
                        else
                            result.Add(edge, v.Distance);
                    }
                    else
                        result.Add(edge, int.MaxValue);
                }
            }

            return result.OrderBy(c => c.Value).ToList().Select(x => x.Key);
        }

        private bool CheckLoop(ACRoutingPath path)
        {
            if (_MaxRouteAlternatives == 0)
                return false;

            if (checkedEdges.Any(c => c.SequenceEqual(path.Distinct())) || (_anyLoop && this.PathsHeap.Count > _MaxRouteAlternatives))
            {
                _anyLoop = true;
                return false;
            }
            checkedEdges.Add(path);
            return true;
        }

        /// <summary>Reconstructs path from sidetracks</summary>
        /// <param name="_sidetracks">Sidetracks collections for this path, could be empty for shortest</param>
        /// <param name="sourceComponent"></param>
        /// <param name="vertexList"></param>
        /// <returns>Full path reconstructed from s to t, crossing sidetracks</returns>
        public static ACRoutingPath RebuildPath(ACRoutingPath _sidetracks, IACComponent sourceComponent, List<ACRoutingVertex> vertexList, Dictionary<Guid,RouteItemModeEnum> routeModeItems)
        {
            ACRoutingPath path = new ACRoutingPath();
            IACComponent v = sourceComponent;
            int i = 0;

            // Start from s, following shortest path or sidetracks
            while (v != null)
            {
                // if current vertex is conected to next sidetrack, cross it
                if (i < _sidetracks.Count && _sidetracks[i].Source.ACRef.ValueT == v)
                {
                    path.Add(_sidetracks[i]);
                    v = _sidetracks[i++].Target.ACRef.ValueT as ACComponent;
                }
                else // else continue walking on shortest path
                {
                    ACRoutingVertex tempVertex = vertexList.FirstOrDefault(c => c.Component.ValueT == v);

                    if (tempVertex != null)
                    {
                        if (tempVertex.HasAnyAllocated)
                            path.HasAnyAllocated = tempVertex.HasAnyAllocated;

                        if (tempVertex.HasAnyReserved)
                            path.HasAnyReserved = tempVertex.HasAnyReserved;
                    }

                    if (tempVertex != null && tempVertex.EdgeToPath == null || (path.Any(c => c.SourceParent == sourceComponent) && path.Any(c => c.TargetParent == sourceComponent)))
                        break;
                    path.Add(tempVertex.EdgeToPath);
                    v = tempVertex.Next.ValueT;

                    if (routeModeItems != null && routeModeItems.Any())
                    {
                        Guid tempVertexID = tempVertex.Component.ValueT.ComponentClass.ACClassID;

                        RouteItemModeEnum mode = RouteItemModeEnum.None;

                        if (routeModeItems.TryGetValue(tempVertexID, out mode))
                        {
                            if (mode != RouteItemModeEnum.None)
                            {
                                path.RouteItemsMode.Add(new Tuple<IACObject, RouteItemModeEnum>(tempVertex.EdgeToPath.SourceParent, mode));
                            }
                        }
                    }
                }
            }
            return path;
        }

        private RoutingResult FindComponents(bool includeReserved, bool includeAllocated, RouteDirections routeDirection, bool includeFullRoute = false)
        {
            SelectionRule selectionRule = ACRoutingService.GetSelectionQuery(_SelectionRuleID);
            if (selectionRule == null)
            {
                Msg msg = new Msg(string.Format("Can't find a selection rule: {0}", _SelectionRuleID), _ACRoutingService, eMsgLevel.Error, "ACRoutingSession", "FindComponents(10)", 555);
                return new RoutingResult(null, false, msg);
            }

            ACRoutingVertex routingTargetVertex;
            List<ACRoutingVertex> result = new List<ACRoutingVertex>();

            if (Source != null)
                routingTargetVertex = Source;
            else
                routingTargetVertex = Target;

            if (routingTargetVertex == null)
                return new RoutingResult(null, false, new Msg() { Source = "ACRoutingSession", Message = String.Format("Routing target component not exist!") });

            routingTargetVertex.Distance = 0;
            routingTargetVertex.DistanceInLoop = 0;

            if (RoutingVertexList.Any(c => c.Key == routingTargetVertex.Component.ValueT))
                RoutingVertexList.Remove(routingTargetVertex.Component.ValueT);

            RoutingVertexList.Add(routingTargetVertex.Component.ValueT, routingTargetVertex);

            PriorityQueue<SP_Node> fringe = new PriorityQueue<SP_Node>();
            List<LoopItem> loopCounter = new List<LoopItem>();

            string excludedByDeselector = "", excludedByLoop = "";

            // Main loop
            do
            {
                if (routingTargetVertex != null)
                {
                    IEnumerable<PAEdge> relatedEdges = routeDirection == RouteDirections.Forwards ? routingTargetVertex.FromEdges : routingTargetVertex.ToEdges;
                    if (relatedEdges == null)
                        relatedEdges = routingTargetVertex.RelatedEdges;

                    foreach (PAEdge edge in relatedEdges)
                    {
                        bool hasReserved, hasAllocated;

                        if (!ValidateRouteRelation(edge, includeReserved, includeAllocated, false, out hasAllocated, out hasReserved))
                            continue;

                        ACRoutingVertex currentVertex;
                        if (!RoutingVertexList.TryGetValue(Source != null ? edge.SourceParentComponent : edge.TargetParentComponent, out currentVertex))
                        {
                            if (Source != null)
                                currentVertex = new ACRoutingVertex(edge.SourceParent as ACComponent);
                            else
                                currentVertex = new ACRoutingVertex(edge.TargetParent as ACComponent);

                            RoutingVertexList.Add(Source != null ? edge.SourceParentComponent : edge.TargetParentComponent, currentVertex);
                        }

                        currentVertex.HasAnyAllocated = hasAllocated;
                        currentVertex.HasAnyReserved = hasReserved;

                        if (currentVertex.Component.ValueT == routingTargetVertex.Component.ValueT && edge.Weight >= 0)  // Ignore negative edges
                            fringe.Enqueue(new SP_Node(edge, edge.Weight + currentVertex.Distance));
                    }
                }

                SP_Node node = fringe.Dequeue();  // Extracts next element in queue
                if (node == null)  // No pending edges to evaluate, finished
                    break;

                PAEdge e = node.Edge;
                if (!RoutingVertexList.TryGetValue(Source != null ? e.TargetParentComponent : e.SourceParentComponent, out routingTargetVertex))
                {
                    routingTargetVertex = new ACRoutingVertex(Source != null ? e.TargetParent as ACComponent : e.SourceParent as ACComponent);
                    routingTargetVertex.EdgeToPath = e;
                    routingTargetVertex.Distance = node.Weight + e.Weight;
                    RoutingVertexList.Add(Source != null ? e.TargetParentComponent : e.SourceParentComponent, routingTargetVertex);
                }
                else
                {
                    var currentLoopEdge = loopCounter.FirstOrDefault(c => c.Edge == e);
                    if (currentLoopEdge != null)
                    {
                        currentLoopEdge.LoopCounter++;
                    }
                    else
                    {
                        currentLoopEdge = new LoopItem(e);
                        loopCounter.Add(currentLoopEdge);
                    }

                    if (currentLoopEdge.LoopCounter > 15)
                    {
                        excludedByLoop += routingTargetVertex.Component.ACUrl + "; ";
                        routingTargetVertex = null;
                        continue;
                    }
                }


                if (selectionRule.Selector != null && selectionRule.Selector(routingTargetVertex, _SelectionRuleParams))
                {
                    if (!result.Any(c => c.Component.ValueT == routingTargetVertex.Component.ValueT))
                        result.Add(routingTargetVertex);
                    routingTargetVertex = null;
                }
                else if (selectionRule.DeSelector != null && selectionRule.DeSelector(routingTargetVertex, _SelectionRuleParams))
                {
                    excludedByDeselector += routingTargetVertex.Component.ACUrl + "; ";
                    routingTargetVertex.EdgeToPath = null;
                    routingTargetVertex = null;
                }

            } while (true);

            Msg msg1 = null;
            string warning = "";

            if (!string.IsNullOrEmpty(excludedByDeselector))
            {
                warning = "Components excluded by deselector: " + excludedByDeselector + System.Environment.NewLine; ;
            }

            if (!string.IsNullOrEmpty(excludedByLoop))
            {
                warning += "Components excluded by detected loop: " + excludedByLoop;
            }

            if (!string.IsNullOrEmpty(warning))
            {
                msg1 = new Msg(warning, _ACRoutingService, eMsgLevel.Warning, "ACRoutingSession", "FindComponents(30)", 655);
            }

            List<Route> routeResult = null;

            if (includeFullRoute)
            {
                routeResult = new List<Route>();

                foreach (ACRoutingVertex vertex in result)
                {
                    ACRoutingVertex tempVertex = vertex;
                    PAEdge edge = tempVertex.EdgeToPath;
                    List<PAEdge> resultList = new List<PAEdge>();
                    List<RouteItem> routeItems = new List<RouteItem>();

                    routeItems.Add(new RouteItem(edge.Relation));

                    bool hasAnyReserved = tempVertex.HasAnyReserved;
                    bool hasAnyAllocated = tempVertex.HasAnyAllocated;

                    do
                    {
                        if (edge == null)
                            break;

                        resultList.Add(edge);

                        IACComponent edgeComponent = routeDirection == RouteDirections.Forwards ? edge.SourceParentComponent : edge.TargetParentComponent;
                        if (edgeComponent == null)
                            break;

                        if (RoutingVertexList.TryGetValue(edgeComponent, out tempVertex))
                        {
                            edge = tempVertex.EdgeToPath;
                            if (edge == null)
                                break;
                            routeItems.Insert(0, new RouteItem(edge.Relation));

                            if (tempVertex.HasAnyReserved)
                                hasAnyReserved = true;

                            if (tempVertex.HasAnyAllocated)
                                hasAnyAllocated = true;
                        }
                        else
                            break;
                    }
                    while (true);

                    routeResult.Add(new Route(routeItems) { HasAnyReserved = hasAnyReserved, HasAnyAllocated = hasAnyAllocated });
                }
            }

            return new RoutingResult(routeResult, false, msg1, result.Select(c => c.Component));
        }

        private bool ValidateRouteRelation(PAEdge edge, bool includeReserved, bool includeAllocated, bool isForEditor, out bool allocated, out bool reserved)
        {
            allocated = false;
            reserved = false;

            if (isForEditor)
                return true;

            if (   edge.IsDeactivated 
                || (    edge.SourceParent is PAClassPhysicalBase 
                    && (   ((PAClassPhysicalBase)edge.SourceParent).OperatingMode.ValueT == Global.OperatingMode.Maintenance 
                        || (!_ACRoutingService.IgnoreInactiveModules && ((PAClassPhysicalBase)edge.SourceParent).OperatingMode.ValueT == Global.OperatingMode.Inactive)
                       )
                   )
                || (    edge.TargetParent is PAClassPhysicalBase 
                    && (   ((PAClassPhysicalBase)edge.TargetParent).OperatingMode.ValueT == Global.OperatingMode.Maintenance
                        || (!_ACRoutingService.IgnoreInactiveModules && ((PAClassPhysicalBase)edge.TargetParent).OperatingMode.ValueT == Global.OperatingMode.Inactive)
                       )
                   )
                )
                return false;

            BitAccessForAllocatedByWay allocatedSource = edge.GetAllocationState(false);
            BitAccessForAllocatedByWay allocatedTarget = edge.GetAllocationState(true);

            if (allocatedSource.ValueT == 0 && allocatedTarget.ValueT == 0)
                return true;

            reserved = allocatedSource.Bit00_Reserved || allocatedTarget.Bit00_Reserved;
            allocated = allocatedSource.Bit01_Allocated || allocatedTarget.Bit01_Allocated;

            if (_PreviousRoute != null && (reserved || allocated))
            {
                ACClassPropertyRelation relation = edge.Relation;
                
                if (relation != null && _PreviousRoute.Any(c => c.SourceGuid == relation.SourceACClassID || c.TargetGuid == relation.TargetACClassID))
                    return true;
            }

            if ((!includeReserved && allocatedSource.Bit00_Reserved) || (!includeAllocated && allocatedSource.Bit01_Allocated))
                return false;

            if ((!includeReserved && allocatedTarget.Bit00_Reserved) || (!includeAllocated && allocatedTarget.Bit01_Allocated))
                return false;


            return true;
        }

        #endregion
    }
}
