using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gip.core.datamodel;

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

        bool _anyLoop = false;

        private ACRoutingService _ACRoutingService;
        private string _DiagnosticData = "";

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

        public Tuple<List<ACRoutingVertex>,PriorityQueue<ST_Node>> BuildRoutes(ACRoutingVertex startVertex, ACRoutingVertex endVertex, int maxRouteAlternatives,
                                                                               bool includeReserved, bool includeAllocated, bool isForEditor = false)
        {
            BuildAvailableRoutes(startVertex, endVertex, maxRouteAlternatives, includeReserved, includeAllocated, isForEditor);
            return new Tuple<List<ACRoutingVertex>, PriorityQueue<ST_Node>>(RoutingVertexList.Values.Where(c => c.Distance != int.MinValue).ToList(), PathsHeap);
        }

        public RoutingResult FindAvailableComponents(ACRoutingVertex startVertex, string selectionRuleID, RouteDirections direction, object[] selectionRuleParams, bool includeReserved, bool includeAllocated)
        {
            _SelectionRuleID = selectionRuleID;
            _SelectionRuleParams = selectionRuleParams != null ? selectionRuleParams : new object[] { };
            if (direction == RouteDirections.Backwards)
            {
                Target = startVertex;
                 return FindComponents(includeReserved, includeAllocated, direction);
            }
            else
            {
                Source = startVertex;
                return FindComponents(includeReserved, includeAllocated, direction);
            }
        }

        public RoutingResult FindRoute(IEnumerable<ACRoutingVertex> startVertices, IEnumerable<ACRoutingVertex> endVertices, string selectionRuleID, object[] selectionRuleParams, 
                                       int maxRouteAlternatives, bool includeReserved, bool includeAllocated)
        {
            RoutingPaths.Clear();
            _MaxRouteAlternatives = maxRouteAlternatives;
            _SelectionRuleID = selectionRuleID;
            _SelectionRuleParams = selectionRuleParams;
            foreach (ACRoutingVertex start in startVertices)
            {
                Source = start;
                foreach (ACRoutingVertex end in endVertices)
                {
                    Target = end;
                    List<ACRoutingPath> tempPaths = new List<ACRoutingPath>();
                    ACRoutingPath tempPath = FindShortestPath(includeReserved, includeAllocated);
                    if (tempPath.Any())
                        RoutingPaths.Add(tempPath);
                    while (tempPath.IsValid)
                    {
                        tempPath = FindNextShortestPath();
                        if (tempPath.Any())
                            RoutingPaths.Add(tempPath);
                    }
                }
            }

            if ((RoutingPaths.Count > 1 && (startVertices.Count() > 1 || endVertices.Count() > 1)))
            {
                var result = CheckMultipleSourcesTargets();
                if (result == null)
                {
                    Msg msg = new Msg("Missing a common component in multiple sources or/and targets mode. Route must have at least a one common component.", _ACRoutingService,
                                      eMsgLevel.Error, "ACRoutingSession", "FindRoute(10)", 137);
                    return new RoutingResult(null, false, msg);
                }
                return CreateRouteFromMultipeSourcesTargets(result);
            }
            else if (RoutingPaths.Any())
            {
                Msg msg = null;
                if(!string.IsNullOrEmpty(_DiagnosticData))
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

            return new RoutingResult(null, false, new Msg(message, _ACRoutingService, eMsgLevel.Error, "ACRoutingSession","FindRoute(20)", 161));
        }

        public RoutingResult FindSuccessors(ACRoutingVertex startVertex, string selectionRuleID, RouteDirections direction, object[] selectionRuleParams, int maxRouteAlternatives,
                                            bool includeReserved, bool includeAllocated)
        {
            RoutingResult rResult = FindAvailableComponents(startVertex, selectionRuleID, direction, selectionRuleParams, includeReserved, includeAllocated);
            if (rResult != null && rResult.Message != null && rResult.Message.MessageLevel > eMsgLevel.Warning)
                return rResult;

            if (rResult == null || !rResult.Components.Any())
            {
                string message = "Can not find a successors according selection rule (" + selectionRuleID + ")";
                if (rResult != null && rResult.Message != null && rResult.Message.MessageLevel == eMsgLevel.Warning)
                    message += "Details: " + rResult.Message.Message;

                Msg msg = new Msg(message, _ACRoutingService, eMsgLevel.Error, "ACRoutingSession", "FindSuccesors(10)", 165);
                return new RoutingResult(null, false, msg);
            }

            IEnumerable<ACRoutingVertex> foundSuccessors = rResult.Components.Select(c => new ACRoutingVertex(c.ValueT));

            RoutingResult routingResult = null;

            if (direction == RouteDirections.Backwards)
            {
                routingResult = FindRoute(foundSuccessors, new ACRoutingVertex[] { startVertex }, selectionRuleID, selectionRuleParams, maxRouteAlternatives, 
                                                        includeReserved, includeAllocated);
            }
            else
            {
                routingResult = FindRoute(new ACRoutingVertex[] { startVertex }, foundSuccessors, selectionRuleID, selectionRuleParams, maxRouteAlternatives, 
                                                        includeReserved, includeAllocated);
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

            foreach(var route in RoutingPaths)
            {
                var searchableRoutes = RoutingPaths.Where(c => c.Start.ParentACComponent != route.Start.ParentACComponent || c.End.ParentACComponent != route.End.ParentACComponent);

                if(searchableRoutes.Any(r => r.AllPoints.Select(x => x.ParentACComponent).Intersect(route.AllPoints.Select(c => c.ParentACComponent)).Any()))
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

            //ACRoutingPath currentPath = routingPaths.Where(c => !c.Any(x => x.IsAllocated(x == c.LastOrDefault())))
            //                                        .OrderBy(k => k.DeltaWeight).FirstOrDefault();

            ACRoutingPath currentPath = routingPaths.OrderBy(k => k.DeltaWeight).FirstOrDefault();

            if (currentPath == null || !currentPath.Any())
                currentPath = routingPaths.OrderBy(c => c.DeltaWeight).FirstOrDefault();

            List<RouteItem> tempList = new List<RouteItem>();
            foreach(var item in currentPath)
                tempList.Add(new RouteItem(item.Relation));

            return new Route(tempList);
        }

        private RoutingResult CreateRouteFromMultipeSourcesTargets(IEnumerable<ACRoutingPath> routingPaths)
        {
            var groups = routingPaths.GroupBy(x => new { start = x.Start.ParentACComponent, end = x.End.ParentACComponent });
            List<Route> routes = new List<Route>();
            string msgHeader = "Routing group is empty in multiple start or/and end components mode:"+System.Environment.NewLine;
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
            return RebuildPath(node.Sidetracks, this.Source.Component.ValueT, RoutingVertexList.Values.ToList());
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
            if(RoutingVertexList.TryGetValue(Source.Component.ValueT, out tempSource))
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
                        if (!ValidateRouteRelation(edge, includeReserved, includeAllocated, isForEditor))
                            continue;

                        ACRoutingVertex currentVertex; 
                        if (!RoutingVertexList.TryGetValue(edge.TargetParentComponent, out currentVertex))
                        {
                            if (edge.TargetParentComponent == Target.Component.ValueT)
                                continue;

                            currentVertex = new ACRoutingVertex(edge.Target.ParentACComponent as ACComponent);
                            RoutingVertexList.Add(edge.TargetParentComponent, currentVertex);
                        }

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

            if(!result && diagnosticData != null)
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
            AddSidetracks(empty, this.Source);
            if (_anyLoop)
                this.PathsHeap.Take(_MaxRouteAlternatives + 1);
        }

        private int _AddedSidetracks = 0;

        /// <summary>
        /// Adds sidetracks recursively for specified vertex and new vertices in shortest path
        /// </summary>
        /// <param name="_p">Previous sidetrack collection</param>
        /// <param name="_v">Vertex to evalueate</param>
        private void AddSidetracks(ACRoutingPath _p, ACRoutingVertex routingVertex)
        {
            if (routingVertex == null)
                return;

            var edges = routingVertex.RelatedEdges.Where(c => c.SourceParentComponent == routingVertex.Component.ValueT).ToArray();

            foreach (PAEdge edge in OrderEdges(edges))
            {
                ACRoutingVertex targetVertex;
                if (!RoutingVertexList.TryGetValue(edge.TargetParentComponent, out targetVertex))
                    continue;

                if (edge.IsSidetrackOf(routingVertex) 
                    && (targetVertex.EdgeToPath != null || edge.Target.ParentACComponent == this.Target.Component.ValueT) 
                    && edge.Source.ParentACComponent != this.Target.Component.ValueT )
                {
                    ACRoutingPath p = new ACRoutingPath();
                    p.AddRange(_p);
                    p.Add(edge);

                    if (!CheckLoop(p) || edge.TargetParent == this.Source.Component.ValueT)
                        continue;
                    this.PathsHeap.Enqueue(new ST_Node(p));
                    _AddedSidetracks++;

                    if (edge.Target.ParentACComponent != routingVertex.Component.ValueT)  // This avoids infinite cycling
                        AddSidetracks(p, targetVertex);
                }
            }
            if (routingVertex.Next != null && routingVertex.Distance > 0 && !(_anyLoop && _AddedSidetracks >= _MaxRouteAlternatives))
                AddSidetracks(_p, RoutingVertexList[routingVertex.Next.ValueT]);
        }

        private IEnumerable<PAEdge> OrderEdges(IEnumerable<PAEdge> edges)
        {
            Dictionary<PAEdge, int> result = new Dictionary<PAEdge, int>();
            foreach(var edge in edges)
            {
                if (!result.ContainsKey(edge))
                {
                    ACRoutingVertex v;
                    if (RoutingVertexList.TryGetValue(edge.TargetParentComponent, out v))
                    {
                        if(v.Distance == int.MinValue)
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

        /// <summary>
        /// Reconstructs path from sidetracks
        /// </summary>
        /// <param name="_sidetracks">Sidetracks collections for this path, could be empty for shortest</param>
        /// <returns>Full path reconstructed from s to t, crossing sidetracks</returns>
        public static ACRoutingPath RebuildPath(ACRoutingPath _sidetracks, IACComponent sourceComponent, List<ACRoutingVertex> vertexList)
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
                    if (tempVertex != null && tempVertex.EdgeToPath == null || (path.Any(c => c.SourceParent == sourceComponent) && path.Any(c => c.TargetParent == sourceComponent)))
                        break;
                    path.Add(tempVertex.EdgeToPath);
                    v = tempVertex.Next.ValueT;
                    //if (tempVertex.DistanceInLoop > 0)
                    //{
                    //    tempVertex.EdgeToPath = null;
                    //}
                }
            }
            return path;
        }

        private RoutingResult FindComponents(bool includeReserved, bool includeAllocated, RouteDirections routeDirection)
        {
            SelectionRule selectionRule = ACRoutingService.GetSelectionQuery(_SelectionRuleID);
            if (selectionRule == null)
            {
                Msg msg = new Msg(string.Format("Can not find a selection rule: {0}", _SelectionRuleID), _ACRoutingService, eMsgLevel.Error, "ACRoutingSession", "FindComponents(10)", 555);
                return new RoutingResult(null, false, msg);
            }

            ACRoutingVertex routingTargetVertex;
            List<ACRoutingVertex> result = new List<ACRoutingVertex>();

            if (Source != null)
                routingTargetVertex = Source;
            else
                routingTargetVertex = Target;

            if (routingTargetVertex == null)
                return new RoutingResult(null, false, new Msg() { Source="ACRoutingSession", Message = String.Format("Routing target component not exist!") });

            if (RoutingVertexList.Any(c => c.Key == routingTargetVertex.Component.ValueT))
                RoutingVertexList.Remove(routingTargetVertex.Component.ValueT);

            RoutingVertexList.Add(routingTargetVertex.Component.ValueT, routingTargetVertex);

            PriorityQueue<SP_Node> fringe = new PriorityQueue<SP_Node>();
            int loopCounter = 0;

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
                        if (!ValidateRouteRelation(edge, includeReserved, includeAllocated, false))
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
                    RoutingVertexList.Add(Source != null ? e.TargetParentComponent : e.SourceParentComponent, routingTargetVertex);
                }
                else
                {
                    //TODO Ivan: Improve this
                    loopCounter++;
                    if (loopCounter > 20)
                    {
                        excludedByLoop += routingTargetVertex.Component.ACUrl + "; ";
                        routingTargetVertex = null;
                        loopCounter = 0;
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

            return new RoutingResult(null, false, msg1, result.Select(c => c.Component));
        }

        private bool ValidateRouteRelation(PAEdge edge, bool includeReserved, bool includeAllocated, bool isForEditor)
        {
            if (isForEditor)
                return true;

            if (edge.IsDeactivated || (edge.SourceParent is PAClassPhysicalBase && ((PAClassPhysicalBase)edge.SourceParent).OperatingMode.ValueT > Global.OperatingMode.Manual) ||
                                      (edge.TargetParent is PAClassPhysicalBase && ((PAClassPhysicalBase)edge.TargetParent).OperatingMode.ValueT > Global.OperatingMode.Manual))
                return false;

            BitAccessForAllocatedByWay allocatedSource = edge.IsAllocated(false);
            BitAccessForAllocatedByWay allocatedTarget = edge.IsAllocated(true);

            if (allocatedSource.ValueT == 0 && allocatedTarget.ValueT == 0)
                return true;

            if ((!includeReserved && allocatedSource.Bit00_Reserved) || (!includeAllocated && allocatedSource.Bit01_Allocated))
                return false;

            if ((!includeReserved && allocatedTarget.Bit00_Reserved) || (!includeAllocated && allocatedTarget.Bit01_Allocated))
                return false;

            return true;
        }

        #endregion
    }
}
