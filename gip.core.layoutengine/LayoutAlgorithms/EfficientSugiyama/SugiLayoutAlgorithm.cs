using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using gip.core.datamodel;
using gip.core.layoutengine;
using QuickGraph;

namespace gip.core.layoutengine.EfficientSugiyama
{
    public partial class SugiLayoutAlgorithm<TVertex,TEdge> where TVertex : IACObject where TEdge : IACEdge
    {
        public SugiLayoutAlgorithm(IEnumerable<TVertex> vertices, IEnumerable<TEdge> edges, Dictionary<TVertex,FrameworkElement> visualsMap, IEnumerable<VBEdge> vbEdges)
        {
            _OriginalVertices = vertices;
            _OriginalEdges = edges;
            _VisualsMap = visualsMap;
            _OriginalVisualEdges = vbEdges;
            InitGraph();
        }

        #region PrivateMembers

        IEnumerable<TVertex> _OriginalVertices;

        IEnumerable<TEdge> _OriginalEdges;

        private IMutableBidirectionalGraph<SugiVertex, SugiEdge> _Graph;

        private Dictionary<TVertex, SugiVertex> _vertexMap = new Dictionary<TVertex, SugiVertex>();

        private Dictionary<TVertex, FrameworkElement> _VisualsMap;

        private IEnumerable<VBEdge> _OriginalVisualEdges;

        private readonly IList<IList<SugiVertex>> _layers = new List<IList<SugiVertex>>();

        private readonly IDictionary<TEdge, IList<SugiVertex>> _dummyVerticesOfEdges = new Dictionary<TEdge, IList<SugiVertex>>();

        private readonly IDictionary<TEdge, Point[]> _edgeRoutingPoints = new Dictionary<TEdge, Point[]>();

        private Dictionary<TVertex, Point> _VertexPositions;

        private List<SugiVertex> _isolatedVertices;

        private double _WidthPerHeight = 1.0;
        private double _LayerDistance = 60.0;
        private double _VertexDistance = 80.0;
        private bool _MinimizeEdgeLength = true;
        private int _PositionMode = -1;

        #endregion

        #region Properties

        public IDictionary<TVertex, Point> VertexPositions
        {
            get { return _VertexPositions; }
        }

        #endregion

        private void InitGraph()
        {
            _Graph = new BidirectionalGraph<SugiVertex, SugiEdge>();
            foreach (TVertex vertex in _OriginalVertices)
            {
                Size size = new Size();
                FrameworkElement frameworkElement;
                if(_VisualsMap.TryGetValue(vertex, out frameworkElement))
                {
                    size.Height = frameworkElement.Height;
                    size.Width = frameworkElement.Width;
                }

                var vertexWrapper = new SugiVertex(vertex, size);
                _Graph.AddVertex(vertexWrapper);
                _vertexMap.Add(vertex, vertexWrapper);
            }

            foreach (var edge in _OriginalEdges)
            {
                var edgeWrapper = new SugiEdge(edge, _vertexMap[(TVertex)edge.SourceParent], _vertexMap[(TVertex)edge.TargetParent]);
                _Graph.AddEdge(edgeWrapper);
            }

            _VertexPositions = new Dictionary<TVertex, Point>();
            _isolatedVertices = new List<SugiVertex>();
        }

        public void LayoutElements(out Dictionary<TVertex, Point> vertexPositions, out IDictionary<TEdge, Point[]> edgePositions, int modeIndex = -1)
        {
            _PositionMode = modeIndex;
            DoPreparing();
            BuildSparseNormalizedGraph();
            DoCrossingMinimizations();
            CalculatePositions();
            vertexPositions = _VertexPositions;
            edgePositions = _edgeRoutingPoints;
        }
    }
}
