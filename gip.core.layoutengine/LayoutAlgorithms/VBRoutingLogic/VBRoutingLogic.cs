using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using gip.ext.design;
using System.Windows.Media;
using gip.core.datamodel;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Class for edge routing logic.
    /// </summary>
    public class VBRoutingLogic
    {
        IACComponent _aCComponent = null;
        public VBRoutingLogic(IACComponent datamodel)
        {
            _aCComponent = datamodel;
        }

        public void ClearVB(bool clearStorages = true)
        {
            _EdgeDesignItems = new List<FrameworkElement>();
            _NodeDesignItems = new List<FrameworkElement>();
            //base.Clear(clearStorages);
        }

        public void InitWithDesignItems(DesignItem rootCanvas, List<FrameworkElement> nodes, IEnumerable<FrameworkElement> edges, IEnumerable<DesignItem> designItemEdges = null)
        {
            _RootDesignItem = rootCanvas;
            _NodeDesignItems = nodes;
            _EdgeDesignItems = edges.ToList();
            _DesignItemEdges = designItemEdges;
        }

        List<FrameworkElement> _NodeDesignItems = new List<FrameworkElement>();
        public List<FrameworkElement> NodeDesignItems
        {
            get
            {
                return _NodeDesignItems;
            }
        }

        List<FrameworkElement> _EdgeDesignItems = new List<FrameworkElement>();
        public List<FrameworkElement> EdgeDesignItems
        {
            get
            {
                return _EdgeDesignItems;
            }
        }

        private IEnumerable<DesignItem> _DesignItemEdges;

        DesignItem _RootDesignItem = null;
        public DesignItem RootDesignItem
        {
            get
            {
                return _RootDesignItem;
            }
        }

        List<Node> _ListNodes = new List<Node>();
        List<Node> _ListAllNodes = new List<Node>();
        List<NodeInCollision> _ListNodeInCollison = new List<NodeInCollision>();
        List<SourceTargetInCollision> _ListSourceTargetInCollision = new List<SourceTargetInCollision>();
        List<IEdge> _ListRoutedEdges = new List<IEdge>();
        Point _NullPoint = new Point();
        bool _InvertFromSourceTarget = false;
        private int _SpaceFromNode = 5;

        /// <summary>
        /// Calculate edge route.
        /// </summary>
        /// <param name="rootElement">The root element of all elements.</param>
        public void CalculateEdgeRoute(UIElement rootElement, int spaceFromNode = 5, bool checkEdgeCollision = true)
        {
            InitWithDesignItems(RootDesignItem, NodeDesignItems, EdgeDesignItems.Where(c => ((IEdge)c).SourceElement != null && ((IEdge)c).TargetElement != null), 
                                _DesignItemEdges != null ? _DesignItemEdges.Where(c => ((IEdge)c.View).SourceElement != null && ((IEdge)c.View).TargetElement != null) : null);
            _SpaceFromNode = spaceFromNode;
            _ListAllNodes.Clear();
            _ListRoutedEdges.Clear();
            TransformPoints(rootElement, true);
            PointCollection points = new PointCollection();
            var sortedAsc = EdgeDesignItems.OrderBy(c => CalculateLineLenght(((IEdge)c).Points));
            foreach (FrameworkElement designItem in sortedAsc)
            {
                if (designItem is VBEdge)
                {
                    VBEdge vbEdge = designItem as VBEdge;

                    if (vbEdge.Points != null)
                        vbEdge.Points.Clear();

                    bool terminate = false;
                    points = CalculateEdgeRoute(rootElement, vbEdge, out terminate);
                    if (points != null)
                    {
                        vbEdge.Points = points;
                        _ListRoutedEdges.Add(vbEdge);
                    }
                    if (terminate)
                        break;
                }
            }
            CheckEdgesCollision(rootElement, _ListRoutedEdges, checkEdgeCollision);
        }

        /// <summary>
        /// Calculate bypass edge route.
        /// </summary>
        /// <param name="rootCanvas">The root element(Canvas) for all edges and nodes.</param>
        /// <param name="iEdge">The VBEdge.</param>
        /// <returns>Return point collection of bypass edge.</returns>
        PointCollection CalculateEdgeRoute(UIElement rootCanvas, IEdge iEdge, out bool terminate, bool invert = false)
        {
            terminate = false;
            _ListNodes.Clear();
            PointCollection points = new PointCollection();
            IEnumerable<FrameworkElement> nodes = null;
            bool isSameParent = IsSameParent(iEdge);
            if (isSameParent)
            {
                FrameworkElement parentElement = iEdge.SourceElement.Parent as FrameworkElement;
                nodes = NodeDesignItems.Where(c => c.Parent == parentElement);
            }
            else
            {
                FrameworkElement source = iEdge.SourceElement;
                FrameworkElement target = iEdge.TargetElement;
                try
                {
                    FrameworkElement parentSourceElement = ((FrameworkElement)source.Parent).Parent as FrameworkElement;
                    FrameworkElement parentTargetElement = ((FrameworkElement)target.Parent).Parent as FrameworkElement;

                    FrameworkElement rootVBCanvasInGroup = source.Parent as FrameworkElement;
                    if (rootVBCanvasInGroup == null || ((FrameworkElement)rootVBCanvasInGroup.Parent).Parent != rootCanvas)
                        rootVBCanvasInGroup = target.Parent as FrameworkElement;

                    if (((FrameworkElement)rootVBCanvasInGroup.Parent).Parent != rootCanvas)
                        rootVBCanvasInGroup = ((FrameworkElement)((FrameworkElement)rootVBCanvasInGroup).Parent).Parent as FrameworkElement;

                    if (rootVBCanvasInGroup != null && source.Parent == rootVBCanvasInGroup && parentTargetElement.Parent == rootVBCanvasInGroup)
                        nodes = NodeDesignItems.Where(c => c.Parent == source.Parent || ((FrameworkElement)c.Parent).Parent == parentTargetElement);

                    else if (rootVBCanvasInGroup != null && parentSourceElement.Parent == rootVBCanvasInGroup && target.Parent == rootVBCanvasInGroup)
                        nodes = NodeDesignItems.Where(c => ((FrameworkElement)c.Parent).Parent == parentSourceElement || c.Parent == target.Parent);

                    else if (((FrameworkElement)((FrameworkElement)rootVBCanvasInGroup).Parent).Parent == rootCanvas)
                        nodes = NodeDesignItems.Where(c => c.Parent == rootVBCanvasInGroup && c != parentSourceElement && c != parentTargetElement ||
                                                           ((FrameworkElement)c.Parent).Parent == parentSourceElement ||
                                                           ((FrameworkElement)c.Parent).Parent == parentTargetElement);
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("VBRoutingLogic", "CalculateEdgeRoute", msg);

                    nodes = null;
                }
            }

            if (nodes == null)
                return null;

            foreach (var designItem in nodes)
            {
                FrameworkElement fwElement = designItem as FrameworkElement;
                if(fwElement != null)
                {
                    Point posRelativeToParent = fwElement.TransformToVisual(rootCanvas).Transform(new Point(0, 0));
                    _ListNodes.Add(new Node(fwElement, posRelativeToParent));
                }
            }

            if (IsInCollision(iEdge, rootCanvas, isSameParent))
            {
                int insertIndex = 0, counter = 0;
                while (IsInCollision(iEdge, rootCanvas, isSameParent))
                {
                    if (!iEdge.Points.Any())
                        iEdge.Points = CalculatePoints(rootCanvas, iEdge, out insertIndex, invert);
                    else
                    {
                        var pointsToInsert = CalculatePoints(rootCanvas, iEdge, out insertIndex);
                        foreach (Point point in pointsToInsert)
                        {
                            if (insertIndex > 0)
                            {
                                iEdge.Points.Insert(insertIndex, point);
                                insertIndex++;
                            }
                        }
                    }
                    counter++;
                    if (counter > 18)
                    {
                        _aCComponent?.Messages.MsgAsync(new Msg(eMsgLevel.Warning, "The space between nodes is not enough. If is possible, please increase space between nodes!!!"), Global.MsgResult.OK, eMsgButton.OK);
                        terminate = true;
                        return null;
                    }
                }
                SourceTargetNodeNearEdges(iEdge);
                return iEdge.Points;
            }
            return null;
        }

        /// <summary>
        /// Check edges in collision and try calculate alternative route.
        /// </summary>
        /// <param name="rootCanvas">The root element.</param>
        /// <param name="routedEdges">The routed edges.</param>
        void CheckEdgesCollision(UIElement rootCanvas, List<IEdge> routedEdges, bool checkEdgeCollision)
        {
            TransformPoints(rootCanvas);
            _ListAllNodes.Clear();
            foreach (VBEdge vbEdge in routedEdges)
            {
                vbEdge.RedrawVBEdge(false);
                if (!checkEdgeCollision)
                    continue;

                int nCollPoints = CheckEdgesCollisionDetail(vbEdge, CheckPotentialEdgesCollision(vbEdge, rootCanvas), rootCanvas);
                if (nCollPoints > 0)
                {
                    PointCollection tempPoints = CopyPoints(vbEdge.Points);
                    vbEdge.Points.Clear();
                    bool terminate = false;
                    PointCollection points = CalculateEdgeRoute(rootCanvas, vbEdge, out terminate, true);
                    if (points != null)
                    {
                        vbEdge.Points = points;
                        int nInvertedCollPoints = CheckEdgesCollisionDetail(vbEdge, CheckPotentialEdgesCollision(vbEdge, rootCanvas), rootCanvas);
                        if (nInvertedCollPoints > nCollPoints)
                            vbEdge.Points = tempPoints;
                    }
                    if(terminate)
                    {
                        vbEdge.Points = tempPoints;
                        break;
                    }
                }
            }

            if (_DesignItemEdges == null)
                return;

            foreach (DesignItem edgeDesignItem in _DesignItemEdges)
            {
                IEdge vbEdge = edgeDesignItem.View as IEdge;
                vbEdge.RedrawVBEdge(false);
                DesignItemProperty property = edgeDesignItem.Properties[VBEdge.PointsProperty];
                if (property != null)
                {
                    property.SetValue(vbEdge.Points);
                    property.SetValueOnInstance(vbEdge.Points);
                }
            }
        }

        /// <summary>
        /// Checks edge collsion on first level.
        /// </summary>
        /// <param name="iEdge">The edge.</param>
        /// <param name="rootCanvas">The root element.</param>
        /// <returns>List of edges that is in potential collision.</returns>
        List<IEdge> CheckPotentialEdgesCollision(IEdge iEdge, UIElement rootCanvas)
        {
            List<IEdge> edgesToCheck = new List<IEdge>();

            if (iEdge.Points == null)
                return edgesToCheck;

            Point vbEdgePos = iEdge.TransformToVisual(rootCanvas).Transform(_NullPoint);
            Point widthHeightPoint = GetWidthAndHeight(iEdge.Points);
            
            foreach (FrameworkElement edgeDesignItem in EdgeDesignItems)
            {
                VBEdge vbEdgeForCheck = edgeDesignItem as VBEdge;
                if (iEdge != vbEdgeForCheck && vbEdgeForCheck.Points != null)
                {
                    foreach (Point point in vbEdgeForCheck.Points)
                    {
                        if (point.X >= vbEdgePos.X && point.Y >= vbEdgePos.Y &&
                            point.X <= vbEdgePos.X + widthHeightPoint.X && point.Y <= vbEdgePos.Y + widthHeightPoint.Y)
                        {
                            edgesToCheck.Add(vbEdgeForCheck);
                            break;
                        }
                    }
                }
            }
            return edgesToCheck;
        }

        /// <summary>
        /// Check if edge in collision  with other edges.
        /// </summary>
        /// <param name="iEdge">The iEdge.</param>
        /// <param name="listEdges">The list of edges in potential collision.</param>
        /// <param name="rootCanvas">The root element.</param>
        /// <returns>Return number of collision.</returns>
        int CheckEdgesCollisionDetail(IEdge iEdge, List<IEdge> listEdges, UIElement rootCanvas)
        {
            int countColl = 0;
            var mainEdgeAllPoints = GenerateAllPoints(iEdge.Points);
            Point mainEdgePos = iEdge.TransformToVisual(rootCanvas).Transform(_NullPoint);

            foreach (IEdge edge in listEdges)
                countColl += mainEdgeAllPoints.Count(c => GenerateAllPoints(edge.Points).Any(x => Math.Round(c.X) + Math.Round(mainEdgePos.X, 0) == Math.Round(x.X, 0) &&
                                                                                                  Math.Round(c.Y) + Math.Round(mainEdgePos.Y, 0) == Math.Round(x.Y, 0)));

            return countColl;
        }

        /// <summary>
        /// Generates edge all points.
        /// </summary>
        /// <param name="points">The edge point collection.</param>
        /// <returns>All edge points.</returns>
        List<Point> GenerateAllPoints(PointCollection points)
        {
            List<Point> linePoints = new List<Point>();
            foreach (Point point in points)
            {
                if (points.Count() >= points.IndexOf(point) + 2)
                {
                    Point point1 = point;
                    Point point2 = points[points.IndexOf(point) + 1];
                    linePoints.AddRange((VBEdge.GetBresenhamLine(point1, point2)));
                }
            }
            if (linePoints.Count > 30)
            {
                linePoints.RemoveRange(0, 10);
                linePoints.RemoveRange(linePoints.Count - 11, 10);
            }
            else if (linePoints.Count > 15)
            {
                linePoints.RemoveRange(0, 4);
                linePoints.RemoveRange(linePoints.Count - 5, 4);
            }
            return linePoints;
        }

        /// <summary>
        /// Get width and height for edge.
        /// </summary>
        /// <param name="points">The edge point collection.</param>
        /// <returns>The point where X is edge width and Y is edge height.</returns>
        Point GetWidthAndHeight(PointCollection points)
        {
            double width = 0, height = 0;
            foreach (Point p in points)
            {
                if (p.X > width)
                    width = p.X;
                if (p.Y > height)
                    height = p.Y;
            }
            return new Point(width, height);
        }

        /// <summary>
        /// Copy points.
        /// </summary>
        /// <param name="points">The point collection for copy.</param>
        /// <returns>The point collection of copied points.</returns>
        PointCollection CopyPoints(PointCollection points)
        {
            PointCollection pc = new PointCollection();
            foreach (Point point in points)
                pc.Add(new Point(point.X, point.Y));
            return pc;
        }

        /// <summary>
        /// Transform source and target points to root element for edges where is only 2 points. 
        /// </summary>
        /// <param name="rootCanvas">The root element.</param>
        void TransformPoints(UIElement rootCanvas, bool isReset = false)
        {
            foreach (FrameworkElement edgeDesignItem in EdgeDesignItems)
            {
                VBEdge vbedge = edgeDesignItem as VBEdge;
                if (vbedge.Points != null && (vbedge.Points.Count == 2 || isReset))
                {
                    vbedge.Points.Clear();
                    vbedge.Points.Add(vbedge.GetSourceConnectorPointToContainer(rootCanvas));
                    vbedge.Points.Add(vbedge.GetTargetConnectorPointToContainer(rootCanvas));
                }
            }
        }

        /// <summary>
        /// Checks if VBEdge source element and target element have same parent.
        /// </summary>
        /// <param name="vbEdge">The VBEdge.</param>
        /// <returns>Return true if is same parent, else return false.</returns>
        private bool IsSameParent(IEdge vbEdge)
        {
            FrameworkElement source, target = null;
            source = vbEdge.SourceElement;
            target = vbEdge.TargetElement;
            if (source != null && target != null && source.Parent == target.Parent)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Checks if edge goes over any node.
        /// </summary>
        /// <param name="vbEdge">The VBEdge.</param>
        /// <param name="rootElement">The root element of edges and nodes.</param>
        /// <param name="isSameParent">The VBEdge target and source element have same parent.</param>
        /// <returns>Return true if edge in collision, else return false.</returns>
        private bool IsInCollision(IEdge vbEdge, UIElement rootElement, bool isSameParent)
        {
            var sourceElement = vbEdge.SourceElement;
            var targetElement = vbEdge.TargetElement;

            // Graph hacks - search if relations and elements are not soldved well - missing some elements presented in relations
            // Graph hack #1
            if(sourceElement  == null || targetElement == null)
            {
                //System.Diagnostics.Debugger.Break();
                return false;
            }

            Node sourceNode = _ListNodes.FirstOrDefault(c => c.NodeFWElement == sourceElement);
            Node targetNode = _ListNodes.FirstOrDefault(c => c.NodeFWElement == targetElement);
            
            // Graph hack #2
            if (sourceNode == null || targetNode == null)
            {
                // System.Diagnostics.Debugger.Break();
                return false;
            }

            Point sourceConnectorPosRoot = vbEdge.GetSourceConnectorPointToContainer(rootElement);
            Point targetConnectorPosRoot = vbEdge.GetTargetConnectorPointToContainer(rootElement);

            bool collision = false;
            var otherNodes = _ListNodes.Where(c => c.NodeFWElement != sourceElement && c.NodeFWElement != targetElement).OrderBy(c => c.NodePosition.Y);

            _ListNodeInCollison.Clear();
            _ListSourceTargetInCollision.Clear();

            if (vbEdge.Points.Count > 2)
            {
                PointCollection linePointsSource = VBEdge.GetBresenhamLine(vbEdge.Points[0], vbEdge.Points[1]);
                PointCollection linePointsTarget = VBEdge.GetBresenhamLine(vbEdge.Points[vbEdge.Points.Count - 2], vbEdge.Points[vbEdge.Points.Count - 1]);
                collision = CheckSourceOrTarget(sourceNode, sourceConnectorPosRoot, linePointsSource, collision);
                collision = CheckSourceOrTarget(targetNode, targetConnectorPosRoot, linePointsTarget, collision);

                foreach (Point point in vbEdge.Points)
                {
                    if (vbEdge.Points.Count() >= vbEdge.Points.IndexOf(point) + 2)
                    {
                        PointCollection linePoints = VBEdge.GetBresenhamLine(point, vbEdge.Points[vbEdge.Points.IndexOf(point) + 1]);

                        foreach (var node in _ListNodes.OrderBy(c => c.NodePosition.Y))
                        {
                            collision = CheckCollisionPoints(node, linePoints, rootElement, vbEdge.Points.IndexOf(point) + 1, collision);
                        }
                    }
                }
                return collision;
            }
            else
            {
                PointCollection linePoints = VBEdge.GetBresenhamLine(sourceConnectorPosRoot, targetConnectorPosRoot);

                collision = CheckSourceOrTarget(sourceNode, sourceConnectorPosRoot, linePoints, collision);
                collision = CheckSourceOrTarget(targetNode, targetConnectorPosRoot, linePoints, collision);

                foreach (var node in otherNodes)
                {
                    collision = CheckCollisionPoints(node, linePoints, rootElement, 0, collision);
                }
                return collision;
            }
        }

        /// <summary>
        /// Calculate points for edge in collision.
        /// </summary>
        /// <param name="rootCanvas">The root element of edges.</param>
        /// <param name="vbEdge">The VBEdge.</param>
        /// <param name="insertIndex">Index for insert points in current point collection.</param>
        /// <returns>Return calculated points.</returns>
        private PointCollection CalculatePoints(UIElement rootCanvas, IEdge vbEdge, out int insertIndex, bool invert = false)
        {
            insertIndex = 0;
            PointCollection points = new PointCollection();
            Point sourceConnectorPos = vbEdge.GetSourceConnectorPointToContainer(rootCanvas);
            Point targetConnectorPos = vbEdge.GetTargetConnectorPointToContainer(rootCanvas);

            EdgeDirection edgeDirection = DetermineEdgeDirection(sourceConnectorPos, targetConnectorPos);
            _ListNodeInCollison = SortNodesInCollision(_ListNodeInCollison, edgeDirection);

            if (!vbEdge.Points.Any())
                points.Add(sourceConnectorPos);

            if (_ListSourceTargetInCollision.Any())
            {
                bool isSourceTopOrBottomExit = false;
                foreach (SourceTargetInCollision sourceTargetInCollision in _ListSourceTargetInCollision)
                {
                    bool isSource = false;
                    Node nodeForNearEdges = GetNodeForNearEdges(sourceTargetInCollision);
                    Point connector;
                    if (vbEdge.SourceElement == sourceTargetInCollision.NodeFWElement)
                    {
                        connector = sourceConnectorPos;
                        insertIndex = 1;
                        isSource = true;
                        if (sourceTargetInCollision.EdgeExit == Side.Top || sourceTargetInCollision.EdgeExit == Side.Bottom)
                            isSourceTopOrBottomExit = true;
                    }
                    else
                    {
                        connector = targetConnectorPos;
                        insertIndex = vbEdge.Points.Count - 1;
                    }
                    points = GenerateSourceTargetPoints(nodeForNearEdges, sourceTargetInCollision, points, rootCanvas, connector, isSource);
                }
                if (invert && isSourceTopOrBottomExit)
                    _InvertFromSourceTarget = true;
            }
            else
            {
                NodeInCollision node = _ListNodeInCollison[0];
                edgeDirection = DetermineEdgeDirection(node.EdgeInterStartPoint, node.EdgeInterEndPoint);
                Node nodeForNearEdges = GetNodeForNearEdges(node);
                Side nodeSide = Side.Left;
                insertIndex = node.CollisonOnIndex;
                NodeInCollision nextNode = null;
                if (_ListNodeInCollison.Count > 1)
                    nextNode = _ListNodeInCollison[1];

                #region Vertical line
                if (node.TopCollisionPoint != _NullPoint && node.BottomCollisionPoint != _NullPoint)
                {
                    double diffTopRight = node.NodePosition.X + node.NodeFWElement.ActualWidth - node.TopCollisionPoint.X;
                    double diffBottomLeft = node.BottomCollisionPoint.X - node.NodePosition.X;
                    bool rightSide = Math.Round(diffTopRight, 0) < Math.Round(diffBottomLeft, 0);
                    if (invert || _InvertFromSourceTarget)
                    {
                        rightSide = !rightSide;
                        _InvertFromSourceTarget = false;
                    }

                    //line go around right side of node
                    if (rightSide)
                    {
                        Point point1 = new Point(node.NodeFWElement.ActualWidth + _SpaceFromNode + nodeForNearEdges.LinesNearRight, 
                                                 -_SpaceFromNode - nodeForNearEdges.LinesNearTop);
                        Point point2 = new Point(node.NodeFWElement.ActualWidth + _SpaceFromNode + nodeForNearEdges.LinesNearRight, 
                                                 node.NodeFWElement.Height + (_SpaceFromNode -1) + nodeForNearEdges.LinesNearBottom);
                        if (edgeDirection == EdgeDirection.BottomTop || edgeDirection == EdgeDirection.LeftBottomRightTop || edgeDirection == EdgeDirection.RightBottomLeftTop)
                            ChangePoints(point1, point2, out point1, out point2);
                        nodeSide = Side.Right;
                        points = GeneratePoints(node, points, point1, point2, rootCanvas);
                    }
                    //line go around left side of node
                    else
                    {
                        Point point1 = new Point(-1 - _SpaceFromNode - nodeForNearEdges.LinesNearLeft, 
                                                 -_SpaceFromNode - nodeForNearEdges.LinesNearTop);
                        Point point2 = new Point(-1 - _SpaceFromNode - nodeForNearEdges.LinesNearLeft, 
                                                  node.NodeFWElement.Height + (_SpaceFromNode -1) + nodeForNearEdges.LinesNearBottom);
                        if (edgeDirection == EdgeDirection.BottomTop || edgeDirection == EdgeDirection.LeftBottomRightTop || edgeDirection == EdgeDirection.RightBottomLeftTop)
                            ChangePoints(point1, point2, out point1, out point2);
                        nodeSide = Side.Left;
                        points = GeneratePoints(node, points, point1, point2, rootCanvas);
                    }
                }
                #endregion

                #region Horizontal line
                else if (node.LeftCollisionPoint != _NullPoint && node.RightCollisionPoint != _NullPoint)
                {
                    double diffLeftTop = node.LeftCollisionPoint.Y - node.NodePosition.Y;
                    double diffRightBottom = node.NodePosition.Y + node.NodeFWElement.ActualHeight - node.RightCollisionPoint.Y;

                    if (node.LeftCollisionPoint.Y - node.NodePosition.Y < node.NodePosition.Y + node.NodeFWElement.ActualHeight - node.LeftCollisionPoint.Y
                            && node.RightCollisionPoint.Y - node.NodePosition.Y < node.NodePosition.Y + node.NodeFWElement.ActualHeight - node.RightCollisionPoint.Y
                        || Math.Round(diffLeftTop, 0) < Math.Round(diffRightBottom, 0))
                    {
                        Point point1 = new Point(-_SpaceFromNode, -_SpaceFromNode - nodeForNearEdges.LinesNearTop);
                        Point point2 = new Point(node.NodeFWElement.ActualWidth + _SpaceFromNode, -_SpaceFromNode - nodeForNearEdges.LinesNearTop);
                        if (edgeDirection == EdgeDirection.RightLeft || edgeDirection == EdgeDirection.RightTopLeftBottom || edgeDirection == EdgeDirection.RightBottomLeftTop)
                            ChangePoints(point1, point2, out point1, out point2);
                        nodeSide = Side.Top;
                        points = GeneratePoints(node, points, point1, point2, rootCanvas);
                    }
                    else
                    {
                        Point point1 = new Point(-_SpaceFromNode, node.NodeFWElement.ActualHeight + _SpaceFromNode + nodeForNearEdges.LinesNearBottom);
                        Point point2 = new Point(node.NodeFWElement.ActualWidth + _SpaceFromNode, node.NodeFWElement.ActualHeight + (_SpaceFromNode - 1) + nodeForNearEdges.LinesNearBottom);
                        if (edgeDirection == EdgeDirection.RightLeft || edgeDirection == EdgeDirection.RightTopLeftBottom || edgeDirection == EdgeDirection.RightBottomLeftTop)
                            ChangePoints(point1, point2, out point1, out point2);
                        nodeSide = Side.Bottom;
                        points = GeneratePoints(node, points, point1, point2, rootCanvas);
                    }
                }
                #endregion

                else if (node.TopCollisionPoint != _NullPoint && node.LeftCollisionPoint != _NullPoint)
                {
                    Point point1 = new Point((-2 - _SpaceFromNode) - nodeForNearEdges.LinesNearLeft, -_SpaceFromNode - nodeForNearEdges.LinesNearTop);
                    Point point2 = new Point((-2 - _SpaceFromNode) - nodeForNearEdges.LinesNearLeft, node.NodeFWElement.ActualHeight + (_SpaceFromNode - 1) + nodeForNearEdges.LinesNearBottom);
                    bool _isTwoPointNeeded = IsTwoPointsNeeded(node, nextNode, vbEdge, rootCanvas, edgeDirection);
                    if (edgeDirection == EdgeDirection.LeftBottomRightTop && _isTwoPointNeeded)
                        ChangePoints(point1, point2, out point1, out point2);
                    nodeSide = Side.Left;
                    points = GeneratePoints(node, points, point1, point2, rootCanvas, _isTwoPointNeeded, false);
                }
                else if (node.TopCollisionPoint != _NullPoint && node.RightCollisionPoint != _NullPoint)
                {
                    Point point1 = new Point(node.NodeFWElement.ActualWidth + _SpaceFromNode + nodeForNearEdges.LinesNearRight, -_SpaceFromNode - nodeForNearEdges.LinesNearTop);
                    Point point2 = new Point(node.NodeFWElement.ActualWidth + _SpaceFromNode + nodeForNearEdges.LinesNearRight, node.NodeFWElement.ActualHeight + (_SpaceFromNode-1) + nodeForNearEdges.LinesNearBottom);
                    bool _isTwoPointNeeded = IsTwoPointsNeeded(node, nextNode, vbEdge, rootCanvas, edgeDirection);
                    if (edgeDirection == EdgeDirection.RightBottomLeftTop && _isTwoPointNeeded)
                        ChangePoints(point1, point2, out point1, out point2);
                    nodeSide = Side.Right;
                    points = GeneratePoints(node, points, point1, point2, rootCanvas, _isTwoPointNeeded, false);
                }
                else if (node.BottomCollisionPoint != _NullPoint && node.LeftCollisionPoint != _NullPoint)
                {
                    Point point1 = new Point((-2 - _SpaceFromNode) - nodeForNearEdges.LinesNearLeft, -_SpaceFromNode - nodeForNearEdges.LinesNearTop);
                    Point point2 = new Point((-2 - _SpaceFromNode) - nodeForNearEdges.LinesNearLeft, node.NodeFWElement.ActualHeight + (_SpaceFromNode-1) + nodeForNearEdges.LinesNearBottom);
                    bool _isTwoPointNeeded = IsTwoPointsNeeded(node, nextNode, vbEdge, rootCanvas, edgeDirection);
                    if (edgeDirection == EdgeDirection.RightBottomLeftTop && _isTwoPointNeeded)
                        ChangePoints(point1, point2, out point1, out point2);
                    nodeSide = Side.Left;
                    points = GeneratePoints(node, points, point1, point2, rootCanvas, _isTwoPointNeeded, false);
                }
                else if (node.BottomCollisionPoint != _NullPoint && node.RightCollisionPoint != _NullPoint)
                {
                    Point point1 = new Point(node.NodeFWElement.ActualWidth + _SpaceFromNode + nodeForNearEdges.LinesNearRight, -_SpaceFromNode - nodeForNearEdges.LinesNearTop);
                    Point point2 = new Point(node.NodeFWElement.ActualWidth + _SpaceFromNode + nodeForNearEdges.LinesNearRight, node.NodeFWElement.ActualHeight + _SpaceFromNode-1 + nodeForNearEdges.LinesNearBottom);
                    bool _isTwoPointNeeded = IsTwoPointsNeeded(node, nextNode, vbEdge, rootCanvas, edgeDirection);
                    if (edgeDirection == EdgeDirection.LeftBottomRightTop && _isTwoPointNeeded)
                        ChangePoints(point1, point2, out point1, out point2);
                    nodeSide = Side.Right;
                    points = GeneratePoints(node, points, point1, point2, rootCanvas, _isTwoPointNeeded, false);
                }
                InsertOrUpdateNodeNearEdges(nodeSide, nodeForNearEdges, node.NodeFWElement);
            }
            if (!vbEdge.Points.Any())
                points.Add(targetConnectorPos);
            return points;
        }

        /// <summary>
        /// Checks if are two points needed for node.
        /// </summary>
        /// <param name="node">The node in collision.</param>
        /// <param name="nextNode">The next node in collision.</param>
        /// <param name="vbEdge">The VBEdge.</param>
        /// <param name="rootCanvas">The root element of VBEdges.</param>
        /// <param name="edgeDirection">The edge direction parameter.</param>
        /// <returns>Return true if two points are needed, else return false.</returns>
        private bool IsTwoPointsNeeded(NodeInCollision node, NodeInCollision nextNode, IEdge vbEdge, UIElement rootCanvas, EdgeDirection edgeDirection)
        {
            if (nextNode != null)
            {
                if (edgeDirection == EdgeDirection.LeftTopRightBottom || edgeDirection == EdgeDirection.RightTopLeftBottom)
                {
                    if (node.NodeFWElement.TransformToVisual(rootCanvas).Transform(_NullPoint).Y + node.NodeFWElement.ActualHeight + _SpaceFromNode <
                        nextNode.NodeFWElement.TransformToVisual(rootCanvas).Transform(_NullPoint).Y - _SpaceFromNode
                        && vbEdge.GetSourceConnectorPointToContainer(rootCanvas).Y - _SpaceFromNode -5 <= node.NodePosition.Y)
                    {
                        return true;
                    }
                }
                else if (edgeDirection == EdgeDirection.LeftBottomRightTop || edgeDirection == EdgeDirection.RightBottomLeftTop)
                {
                    if (node.NodeFWElement.TransformToVisual(rootCanvas).Transform(_NullPoint).Y - _SpaceFromNode >
                        nextNode.NodeFWElement.TransformToVisual(rootCanvas).Transform(_NullPoint).Y + nextNode.NodeFWElement.ActualHeight + _SpaceFromNode
                        && vbEdge.GetSourceConnectorPointToContainer(rootCanvas).Y + _SpaceFromNode + 5 >= node.NodePosition.Y + node.NodeFWElement.ActualHeight)
                    {
                        return true;
                    }
                }
            }
            else
            {
                if (edgeDirection == EdgeDirection.LeftTopRightBottom || edgeDirection == EdgeDirection.RightTopLeftBottom)
                {
                    if (node.NodeFWElement.TransformToVisual(rootCanvas).Transform(_NullPoint).Y - _SpaceFromNode > vbEdge.GetSourceConnectorPointToContainer(rootCanvas).Y + _SpaceFromNode &&
                        node.NodeFWElement.TransformToVisual(rootCanvas).Transform(_NullPoint).Y + node.NodeFWElement.ActualHeight + _SpaceFromNode < vbEdge.GetTargetConnectorPointToContainer(rootCanvas).Y - _SpaceFromNode)
                    {
                        return true;
                    }
                }
                else if (edgeDirection == EdgeDirection.LeftBottomRightTop || edgeDirection == EdgeDirection.RightBottomLeftTop)
                {
                    if (node.NodeFWElement.TransformToVisual(rootCanvas).Transform(_NullPoint).Y + node.NodeFWElement.ActualHeight + _SpaceFromNode < vbEdge.GetSourceConnectorPointToContainer(rootCanvas).Y - _SpaceFromNode &&
                        node.NodeFWElement.TransformToVisual(rootCanvas).Transform(_NullPoint).Y - _SpaceFromNode > vbEdge.GetTargetConnectorPointToContainer(rootCanvas).Y + _SpaceFromNode)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Determine edge direction.
        /// </summary>
        /// <param name="source">The point from VBEdge source element.</param>
        /// <param name="target">Ther point from VBEdge target element.</param>
        /// <returns>Return edge direction.</returns>
        private EdgeDirection DetermineEdgeDirection(Point source, Point target)
        {
            if (source.X == target.X && source.Y < target.Y)
                return EdgeDirection.TopBottom;
            else if (source.X == target.X && source.Y > target.Y)
                return EdgeDirection.BottomTop;
            else if (source.Y == target.Y && source.X < target.X)
                return EdgeDirection.LeftRight;
            else if (source.Y == target.Y && source.X > target.X)
                return EdgeDirection.RightLeft;
            else if (source.X < target.X && source.Y < target.Y)
                return EdgeDirection.LeftTopRightBottom;
            else if (source.X < target.X && source.Y > target.Y)
                return EdgeDirection.LeftBottomRightTop;
            else if (source.X > target.X && source.Y < target.Y)
                return EdgeDirection.RightTopLeftBottom;
            else
                return EdgeDirection.RightBottomLeftTop;
        }

        /// <summary>
        /// Sort nodes in collision depends on edge direction.
        /// </summary>
        /// <param name="listNodeInCollison">The list of nodes in collision.</param>
        /// <param name="edgeDirection">The edge direction.</param>
        /// <returns>Return sorted nodes in collision.</returns>
        private List<NodeInCollision> SortNodesInCollision(List<NodeInCollision> listNodeInCollison, EdgeDirection edgeDirection)
        {
            if (edgeDirection == EdgeDirection.TopBottom)
                listNodeInCollison = listNodeInCollison.OrderBy(c => c.CheckPoints(false, true)).ToList();

            else if (edgeDirection == EdgeDirection.BottomTop)
                listNodeInCollison = listNodeInCollison.OrderByDescending(c => c.CheckPoints(false, false)).ToList();

            else if (edgeDirection == EdgeDirection.LeftTopRightBottom)
                listNodeInCollison = listNodeInCollison.OrderBy(c => c.CheckPoints(false, true)).ThenBy(x => x.CheckPoints(true, true)).ToList();

            else if (edgeDirection == EdgeDirection.RightTopLeftBottom)
                listNodeInCollison = listNodeInCollison.OrderBy(c => c.CheckPoints(false, true)).ThenByDescending(x => x.CheckPoints(true, false)).ToList();

            else if (edgeDirection == EdgeDirection.LeftBottomRightTop)
                listNodeInCollison = listNodeInCollison.OrderByDescending(c => c.CheckPoints(false, false)).ThenBy(x => x.CheckPoints(true, true)).ToList();

            else if (edgeDirection == EdgeDirection.RightBottomLeftTop)
                listNodeInCollison = listNodeInCollison.OrderByDescending(c => c.CheckPoints(false, false)).ThenByDescending(x => x.CheckPoints(true, false)).ToList();

            else if (edgeDirection == EdgeDirection.LeftRight)
                listNodeInCollison = listNodeInCollison.OrderBy(c => c.CheckPoints(true, true)).ToList();

            else if (edgeDirection == EdgeDirection.RightLeft)
                listNodeInCollison = listNodeInCollison.OrderByDescending(c => c.CheckPoints(true, false)).ToList();

            return listNodeInCollison;
        }

        /// <summary>
        /// Get node where saved information about near edges.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>Return node with near edges information.</returns>
        private Node GetNodeForNearEdges(INodeBase node)
        {
            if (_ListAllNodes.Any(c => c.NodeFWElement == node.NodeFWElement))
            {
                return _ListAllNodes.FirstOrDefault(c => c.NodeFWElement == node.NodeFWElement);
            }
            else
            {
                return _ListNodes.FirstOrDefault(c => c.NodeFWElement == node.NodeFWElement);
            }
        }

        /// <summary>
        /// Generate points and put it in right place.
        /// </summary>
        /// <param name="nodeInCollision">The node in collison.</param>
        /// <param name="points">The point collection.</param>
        /// <param name="point1">The first point for insert.</param>
        /// <param name="point2">The second point for insert.</param>
        /// <param name="rootCanvas">The root element(Canvas).</param>
        /// <returns>Return generated points.</returns>
        private PointCollection GeneratePoints(NodeInCollision nodeInCollision, PointCollection points,
                                               Point point1, Point point2, UIElement rootCanvas)
        {
            Point firstPoint = nodeInCollision.NodeFWElement.TransformToVisual(rootCanvas).Transform(point1);
            points.Add(firstPoint);
            Point secondPoint = nodeInCollision.NodeFWElement.TransformToVisual(rootCanvas).Transform(point2);
            points.Add(secondPoint);
            return points;
        }

        /// <summary>
        /// Generate points and put it in right place.
        /// </summary>
        /// <param name="nodeInCollision">The node in collision.</param>
        /// <param name="points">The point collection.</param>
        /// <param name="point1">The first point for insert.</param>
        /// <param name="point2">The second point for insert.</param>
        /// <param name="rootCanvas">The root element(Canvas).</param>
        /// <param name="isTwoPointsNeeded">Is two points need insert.</param>
        /// <param name="isPointInsertBefore">Is second point need insert before first point.</param>
        /// <returns>Return generated points.</returns>
        private PointCollection GeneratePoints(NodeInCollision nodeInCollision, PointCollection points, Point point1, Point point2,
                                               UIElement rootCanvas, bool isTwoPointsNeeded, bool isPointInsertBefore)
        {
            if (isPointInsertBefore && isTwoPointsNeeded)
            {
                Point secondPoint = nodeInCollision.NodeFWElement.TransformToVisual(rootCanvas).Transform(point2);
                points.Add(secondPoint);
                Point firstPoint = nodeInCollision.NodeFWElement.TransformToVisual(rootCanvas).Transform(point1);
                points.Add(firstPoint);
            }
            else if (!isPointInsertBefore && isTwoPointsNeeded)
            {
                Point firstPoint = nodeInCollision.NodeFWElement.TransformToVisual(rootCanvas).Transform(point1);
                points.Add(firstPoint);
                Point secondPoint = nodeInCollision.NodeFWElement.TransformToVisual(rootCanvas).Transform(point2);
                points.Add(secondPoint);
            }
            else if (nodeInCollision.TopCollisionPoint != _NullPoint)
            {
                Point firstPoint = nodeInCollision.NodeFWElement.TransformToVisual(rootCanvas).Transform(point1);
                points.Add(firstPoint);
            }
            else if (nodeInCollision.BottomCollisionPoint != _NullPoint)
            {
                Point secondPoint = nodeInCollision.NodeFWElement.TransformToVisual(rootCanvas).Transform(point2);
                points.Add(secondPoint);
            }
            return points;
        }

        /// <summary>
        /// Generate source or target points and insert them on right place.
        /// </summary>
        /// <param name="nodeNearEdges">The node with information about edges near.</param>
        /// <param name="sourceTargetInCollision">The source or target node in collision.</param>
        /// <param name="points">The point collection.</param>
        /// <param name="rootCanvas">The root element(Canvas).</param>
        /// <param name="connector">The connector point on node.</param>
        /// <param name="isSource">Is source node parameter.</param>
        /// <returns>Return generated points.</returns>
        private PointCollection GenerateSourceTargetPoints(Node nodeNearEdges, SourceTargetInCollision sourceTargetInCollision, PointCollection points,
                                                           UIElement rootCanvas, Point connector, bool isSource)
        {
            if (isSource)
                nodeNearEdges.SourceConnSide = sourceTargetInCollision.EdgeExit;
            else
                nodeNearEdges.TargetConnSide = sourceTargetInCollision.EdgeExit;

            Point exitPoint;
            if (sourceTargetInCollision.EdgeExit == Side.Top)
            {
                exitPoint = sourceTargetInCollision.NodeFWElement.TransformToVisual(rootCanvas).Transform(new Point(0, -5 - nodeNearEdges.LinesNearTop));
                exitPoint.X = connector.X;
                points.Add(exitPoint);
            }
            else if (sourceTargetInCollision.EdgeExit == Side.Bottom)
            {
                exitPoint = sourceTargetInCollision.NodeFWElement.TransformToVisual(rootCanvas).Transform(new Point(0, sourceTargetInCollision.NodeFWElement.ActualHeight + 4 + nodeNearEdges.LinesNearBottom));
                exitPoint.X = connector.X;
                points.Add(exitPoint);
            }
            else if (sourceTargetInCollision.EdgeExit == Side.Left)
            {
                exitPoint = sourceTargetInCollision.NodeFWElement.TransformToVisual(rootCanvas).Transform(new Point(-7 - nodeNearEdges.LinesNearLeft, 0));
                exitPoint.Y = connector.Y;
                points.Add(exitPoint);
            }
            else if (sourceTargetInCollision.EdgeExit == Side.Right)
            {
                exitPoint = sourceTargetInCollision.NodeFWElement.TransformToVisual(rootCanvas).Transform(new Point(sourceTargetInCollision.NodeFWElement.ActualWidth + 5 + nodeNearEdges.LinesNearRight, 0));
                exitPoint.Y = connector.Y;
                points.Add(exitPoint);
            }
            return points;
        }

        /// <summary>
        /// Change points.
        /// </summary>
        /// <param name="point1">The first point.</param>
        /// <param name="point2">The second point.</param>
        /// <param name="point1out">The first point out.</param>
        /// <param name="point2out">The second point out.</param>
        private void ChangePoints(Point point1, Point point2, out Point point1out, out Point point2out)
        {
            Point tempPoint = point1;
            point1out = point2;
            point2out = tempPoint;
        }

        /// <summary>
        /// Determine collision points and if node in collision fill _ListNodeInCollision.
        /// </summary>
        /// <param name="node">The node which need check.</param>
        /// <param name="linePoints">The all points of edge.</param>
        /// <param name="collision">The collision parameter</param>
        /// <returns>Retrun true if node and edge in collision, else return false.</returns>
        private bool CheckCollisionPoints(INodeBase node, PointCollection linePoints, bool collision)
        {
            Point lt = new Point(node.NodePosition.X, node.NodePosition.Y);
            Point rt = new Point(node.NodePosition.X + node.NodeFWElement.ActualWidth, node.NodePosition.Y);
            Point lb = new Point(node.NodePosition.X, node.NodePosition.Y + node.NodeFWElement.ActualHeight);
            Point rb = new Point(node.NodePosition.X + node.NodeFWElement.ActualWidth, node.NodePosition.Y + node.NodeFWElement.ActualHeight);

            Point topPoint = linePoints.FirstOrDefault(c => Math.Round(c.Y, 0) == Math.Round(lt.Y, 0) && c.X >= lt.X && c.X <= rt.X);
            Point bottomPoint = linePoints.FirstOrDefault(c => Math.Round(c.Y, 0) == Math.Round(lb.Y, 0) && c.X >= lt.X && c.X <= rt.X);
            Point leftPoint = linePoints.FirstOrDefault(c => Math.Round(c.X, 0) == Math.Round(lt.X, 0) && c.Y >= lt.Y && c.Y <= lb.Y);
            Point rightPoint = linePoints.FirstOrDefault(c => Math.Round(c.X, 0) == Math.Round(rt.X, 0) && c.Y >= lt.Y && c.Y <= lb.Y);

            if (CountNodePoints(topPoint, bottomPoint, leftPoint, rightPoint) == 2)
            {
                _ListNodeInCollison.Add(new NodeInCollision(node.NodeFWElement, node.NodePosition, topPoint, bottomPoint, leftPoint, rightPoint, linePoints.First(), linePoints.Last()));
                collision = true;
            }
            else if (CountNodePoints(topPoint, bottomPoint, leftPoint, rightPoint) == 1)
            {
                _ListSourceTargetInCollision.Add(new SourceTargetInCollision(node.NodeFWElement, node.NodePosition, topPoint, bottomPoint, leftPoint, rightPoint, Side.Top));
                collision = true;
            }
            return collision;
        }

        /// <summary>
        /// Determine collision points and if node in collision fill _ListNodeInCollision.
        /// </summary>
        /// <param name="node">The node which need check.</param>
        /// <param name="linePoints">The all points of edge.</param>
        /// <param name="rootElement">The root element.</param>
        /// <param name="insertIndex">The insert index.</param>
        /// <param name="collision">The collision parameter.</param>
        /// <returns>Retrun true if node and edge in collision, else return false</returns>
        private bool CheckCollisionPoints(INodeBase node, PointCollection linePoints, UIElement rootElement, int insertIndex, bool collision)
        {
            var nodePosToRoot = node.NodeFWElement.TransformToVisual(rootElement).Transform(new Point(0, 0));
            Point lt = new Point(nodePosToRoot.X, nodePosToRoot.Y);
            Point rt = new Point(nodePosToRoot.X + node.NodeFWElement.ActualWidth, nodePosToRoot.Y);
            Point lb = new Point(nodePosToRoot.X, nodePosToRoot.Y + node.NodeFWElement.ActualHeight);
            Point rb = new Point(nodePosToRoot.X + node.NodeFWElement.ActualWidth, nodePosToRoot.Y + node.NodeFWElement.ActualHeight);

            Point topPoint = linePoints.FirstOrDefault(c => Math.Round(c.Y, 0) == Math.Round(lt.Y, 0) && c.X >= lt.X && c.X <= rt.X);
            Point bottomPoint = linePoints.FirstOrDefault(c => Math.Round(c.Y, 0) == Math.Round(lb.Y, 0) && c.X >= lt.X && c.X <= rt.X);
            Point leftPoint = linePoints.FirstOrDefault(c => Math.Round(c.X, 0) == Math.Round(lt.X, 0) && c.Y >= lt.Y && c.Y <= lb.Y);
            Point rightPoint = linePoints.FirstOrDefault(c => Math.Round(c.X, 0) == Math.Round(rt.X, 0) && c.Y >= lt.Y && c.Y <= lb.Y);

            if (CountNodePoints(topPoint, bottomPoint, leftPoint, rightPoint) == 2)
            {
                _ListNodeInCollison.Add(new NodeInCollision(node.NodeFWElement, node.NodePosition, topPoint, bottomPoint, leftPoint, rightPoint, linePoints.First(), linePoints.Last(), insertIndex));
                collision = true;
            }
            return collision;
        }

        /// <summary>
        /// Determine collision points on source or target node and if source or target node in collision, fill _ListSourceTargetInCollision.
        /// </summary>
        /// <param name="node">The source or target node.</param>
        /// <param name="connector">The connector point.</param>
        /// <param name="linePoints">The all points of edge.</param>
        /// <param name="collision">The collision parameter.</param>
        /// <returns>Return true if source or target node in collison, else return false.</returns>
        private bool CheckSourceOrTarget(INodeBase node, Point connector, PointCollection linePoints, bool collision)
        {
            if (node.NodeFWElement.ActualHeight > 12)
            {
                Side side = DetermineSide(node, connector);
                CheckCollisionPoints(node, linePoints, false);
                SourceTargetInCollision sourceTargetInCollision = _ListSourceTargetInCollision.FirstOrDefault(c => c.NodeFWElement == node.NodeFWElement);
                if (sourceTargetInCollision == null)
                    return collision;
                if (sourceTargetInCollision.TopCollisionPoint != _NullPoint && side == Side.Top)
                {
                    _ListSourceTargetInCollision.Remove(sourceTargetInCollision);
                    collision = false;
                }
                else if (sourceTargetInCollision.BottomCollisionPoint != _NullPoint && side == Side.Bottom)
                {
                    _ListSourceTargetInCollision.Remove(sourceTargetInCollision);
                    collision = false;
                }
                else if (sourceTargetInCollision.LeftCollisionPoint != _NullPoint && side == Side.Left)
                {
                    _ListSourceTargetInCollision.Remove(sourceTargetInCollision);
                    collision = false;
                }
                else if (sourceTargetInCollision.RightCollisionPoint != _NullPoint && side == Side.Right)
                {
                    _ListSourceTargetInCollision.Remove(sourceTargetInCollision);
                    collision = false;
                }
                else
                {
                    _ListSourceTargetInCollision.FirstOrDefault(c => c.NodeFWElement == sourceTargetInCollision.NodeFWElement).EdgeExit = side;
                    collision = true;
                }
            }
            return collision;
        }

        /// <summary>
        /// Determine side for edge output on source node or edge input on target node.
        /// </summary>
        /// <param name="node">The source or target node.</param>
        /// <param name="connector">The connector point.</param>
        /// <returns>Return determineted side.</returns>
        private Side DetermineSide(INodeBase node, Point connector)
        {
            Dictionary<Side, double> dict = new Dictionary<Side, double>();
            dict.Add(Side.Top, connector.Y - node.NodePosition.Y);
            dict.Add(Side.Bottom, node.NodePosition.Y + node.NodeFWElement.ActualHeight - connector.Y);
            dict.Add(Side.Left, connector.X - node.NodePosition.X);
            dict.Add(Side.Right, node.NodePosition.X + node.NodeFWElement.ActualWidth - connector.X);
            return dict.OrderBy(c => c.Value).FirstOrDefault().Key;
        }

        /// <summary>
        /// Count how many points in collision.
        /// </summary>
        /// <param name="point1">The first point.</param>
        /// <param name="point2">The second point.</param>
        /// <param name="point3">The third point.</param>
        /// <param name="point4">The fourth point.</param>
        /// <returns>Return 2 if two points in collision on same node, else 1 if one point in collision on same node, else return 0.</returns>
        private int CountNodePoints(Point point1, Point point2, Point point3, Point point4)
        {
            if ((point1 != _NullPoint && point2 != _NullPoint) || (point1 != _NullPoint && point3 != _NullPoint) || (point1 != _NullPoint && point4 != _NullPoint))
                return 2;
            else if ((point2 != _NullPoint && point3 != _NullPoint) || (point2 != _NullPoint && point4 != _NullPoint) || (point3 != _NullPoint && point4 != _NullPoint))
                return 2;
            else if (point1 != _NullPoint && point2 == _NullPoint && point3 == _NullPoint && point4 == _NullPoint)
                return 1;
            else if (point1 == _NullPoint && point2 != _NullPoint && point3 == _NullPoint && point4 == _NullPoint)
                return 1;
            else if (point1 == _NullPoint && point2 == _NullPoint && point3 != _NullPoint && point4 == _NullPoint)
                return 1;
            else if (point1 == _NullPoint && point2 == _NullPoint && point3 == _NullPoint && point4 != _NullPoint)
                return 1;
            return 0;
        }

        /// <summary>
        /// Update information about edges near source or target node.
        /// </summary>
        /// <param name="iEdge">The VBEdge.</param>
        private void SourceTargetNodeNearEdges(IEdge iEdge)
        {
            FrameworkElement source = iEdge.SourceElement;
            FrameworkElement target = iEdge.TargetElement;

            if (_ListAllNodes.Any(c => c.NodeFWElement == source))
            {
                Node node = _ListAllNodes.FirstOrDefault(c => c.NodeFWElement == source);
                if (node.SourceConnSide != null)
                {
                    InsertOrUpdateNodeNearEdges(node.SourceConnSide, node, source);
                }
            }
            if (_ListAllNodes.Any(c => c.NodeFWElement == target))
            {
                Node node = _ListAllNodes.FirstOrDefault(c => c.NodeFWElement == target);
                if (node.TargetConnSide != null)
                {
                    InsertOrUpdateNodeNearEdges(node.TargetConnSide, node, target);
                }
            }
        }

        /// <summary>
        /// Insert or update node near edges on ListAllNodes.
        /// </summary>
        /// <param name="edgeNearNode">The side where edge goes around node.</param>
        /// <param name="nodeNearEdges">The node which edge bypasses and save in ListAllNodes.</param>
        /// <param name="nodeFWElement">The Framework Element of node in collision.</param>
        private void InsertOrUpdateNodeNearEdges(Side? edgeNearNode, Node nodeNearEdges, FrameworkElement nodeFWElement)
        {
            if (!_ListAllNodes.Any(c => c.NodeFWElement == nodeFWElement))
            {
                if (edgeNearNode == Side.Top)
                    nodeNearEdges.LinesNearTop += 5;
                else if (edgeNearNode == Side.Bottom)
                    nodeNearEdges.LinesNearBottom += 5;
                else if (edgeNearNode == Side.Left)
                    nodeNearEdges.LinesNearLeft += 7;
                else if (edgeNearNode == Side.Right)
                    nodeNearEdges.LinesNearRight += 7;
                _ListAllNodes.Add(nodeNearEdges);
            }
            else
            {
                if (edgeNearNode == Side.Top)
                    _ListAllNodes.FirstOrDefault(c => c.NodeFWElement == nodeFWElement).LinesNearTop += 5;
                else if (edgeNearNode == Side.Bottom)
                    _ListAllNodes.FirstOrDefault(c => c.NodeFWElement == nodeFWElement).LinesNearBottom += 5;
                else if (edgeNearNode == Side.Left)
                    _ListAllNodes.FirstOrDefault(c => c.NodeFWElement == nodeFWElement).LinesNearLeft += 7;
                else if (edgeNearNode == Side.Right)
                    _ListAllNodes.FirstOrDefault(c => c.NodeFWElement == nodeFWElement).LinesNearRight += 7;
            }
        }

        /// <summary>
        /// Calculate line lenght between two points.
        /// </summary>
        /// <param name="start">The start point.</param>
        /// <param name="end">The end point.</param>
        /// <returns>Return line lenght.</returns>
        public double CalculateLineLenght(Point start, Point end)
        {
            return Math.Sqrt(Math.Pow((end.Y - start.Y), 2) + Math.Pow((end.X - start.X), 2));
        }

        /// <summary>
        /// Calculate line lenght between two points.
        /// </summary>
        /// <param name="points">The points collection.</param>
        /// <returns>Return line lenght if is two points, else return 0.0</returns>
        public double CalculateLineLenght(PointCollection points)
        {
            if (points != null && points.Count == 2)
            {
                Point start = points[0];
                Point end = points[1];
                return Math.Sqrt(Math.Pow((end.Y - start.Y), 2) + Math.Pow((end.X - start.X), 2));
            }
            return 0.0;
        }
    }
}
