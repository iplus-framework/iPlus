using System.Windows;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Interface for all nodes.
    /// </summary>
    public interface INodeBase
    {
        FrameworkElement NodeFWElement { get; set; }
        Point NodePosition { get; set; }
    }

    public enum EdgeDirection
    {
        TopBottom,
        BottomTop,
        LeftRight,
        RightLeft,
        LeftTopRightBottom,
        RightTopLeftBottom,
        LeftBottomRightTop,
        RightBottomLeftTop
    };

    public enum Side
    {
        Left, Right, Top, Bottom
    };

    /// <summary>
    /// Class for node near edges.
    /// </summary>
    public class Node : INodeBase
    {
        public Node(FrameworkElement node, Point nodePosition, int linesLeft = 0, int linesRight = 0, int linesTop = 0, int linesBottom = 0, Side? sourceTargetConnSide = null)
        {
            NodeFWElement = node;
            NodePosition = nodePosition;
            LinesNearLeft = linesLeft;
            LinesNearRight = linesRight;
            LinesNearTop = linesTop;
            LinesNearBottom = linesBottom;
            SourceConnSide = sourceTargetConnSide;
        }

        public FrameworkElement NodeFWElement { get; set; }

        public Point NodePosition { get; set; }

        public int LinesNearLeft { get; set; }

        public int LinesNearRight { get; set; }

        public int LinesNearTop { get; set; }

        public int LinesNearBottom { get; set; }

        public Side? SourceConnSide { get; set; }

        public Side? TargetConnSide { get; set; }
    }

    /// <summary>
    /// Class for node in collision.
    /// </summary>
    public class NodeInCollision : INodeBase
    {
        public NodeInCollision(FrameworkElement node, Point nodePosition, Point topCollisionPoint, Point bottomCollisionPoint, Point leftCollisionPoint, Point rightCollisionPoint,
                               Point edgeInterStartPoint, Point edgeInterEndPoint, int lineIndex = 0)
        {
            NodeFWElement = node;
            NodePosition = nodePosition;
            TopCollisionPoint = topCollisionPoint;
            BottomCollisionPoint = bottomCollisionPoint;
            LeftCollisionPoint = leftCollisionPoint;
            RightCollisionPoint = rightCollisionPoint;
            EdgeInterStartPoint = edgeInterStartPoint;
            EdgeInterEndPoint = edgeInterEndPoint;
            CollisonOnIndex = lineIndex;
        }

        public FrameworkElement NodeFWElement
        {
            get;
            set;
        }

        public Point NodePosition
        {
            get;
            set;
        }

        public Point TopCollisionPoint
        {
            get;
            set;
        }

        public Point BottomCollisionPoint
        {
            get;
            set;
        }

        public Point LeftCollisionPoint
        {
            get;
            set;
        }

        public Point RightCollisionPoint
        {
            get;
            set;
        }

        public Point EdgeInterStartPoint { get; set; }

        public Point EdgeInterEndPoint { get; set; }

        public int CollisonOnIndex
        {
            get;
            set;
        }

        public double CheckPoints(bool isXAxe, bool isFromTopOrLeft)
        {
            if (!isXAxe && isFromTopOrLeft && TopCollisionPoint.Y != 0)
                return TopCollisionPoint.Y;

            if (!isXAxe && !isFromTopOrLeft && BottomCollisionPoint.Y != 0)
                return BottomCollisionPoint.Y;

            if (!isXAxe && LeftCollisionPoint.Y != 0 && RightCollisionPoint.Y == 0)
                return LeftCollisionPoint.Y;

            if (!isXAxe && RightCollisionPoint.Y != 0 && LeftCollisionPoint.Y == 0)
                return RightCollisionPoint.Y;

            if (isXAxe && isFromTopOrLeft && TopCollisionPoint.X != 0)
                return TopCollisionPoint.X;

            if (isXAxe && !isFromTopOrLeft && BottomCollisionPoint.X != 0)
                return BottomCollisionPoint.X;

            if (isXAxe && isFromTopOrLeft && LeftCollisionPoint.X != 0 && RightCollisionPoint.X != 0)
                return LeftCollisionPoint.X;

            if (isXAxe && !isFromTopOrLeft && LeftCollisionPoint.X != 0 && RightCollisionPoint.X != 0)
                return RightCollisionPoint.X;

            if (isXAxe && LeftCollisionPoint.X != 0 && RightCollisionPoint.X == 0)
                return LeftCollisionPoint.X;

            if (isXAxe && RightCollisionPoint.X != 0 && LeftCollisionPoint.X == 0)
                return RightCollisionPoint.X;

            return 0;
        }

    }

    /// <summary>
    /// Class for source or target node in collision.
    /// </summary>
    public class SourceTargetInCollision : NodeInCollision
    {
        public SourceTargetInCollision(FrameworkElement node, Point nodePosition, Point topCollisionPoint, Point bottomCollisionPoint, Point leftCollisionPoint, Point rightCollisionPoint,
                                        Side edgeExit, int lineIndex = 0)
            : base(node, nodePosition, topCollisionPoint, bottomCollisionPoint, leftCollisionPoint, rightCollisionPoint, new Point(), new Point(), lineIndex = 0)
        {
            EdgeExit = edgeExit;
        }

        public Side EdgeExit
        {
            get;
            set;
        }
    }
}
