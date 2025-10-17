using gip.core.datamodel;
using QuickGraph.Algorithms.Search;
using System.Collections.Generic;
using System.Linq;

namespace gip.core.layoutengine.avui.EfficientSugiyama
{
    public partial class SugiLayoutAlgorithm<TVertex, TEdge> where TVertex : IACObject where TEdge : IACEdge
    {
        private void DoPreparing()
        {
            RemoveCycles();
            RemoveLoops();
            RemoveIsolatedVertices(); //it must run after the two method above           
        }

        private void RemoveIsolatedVertices()
        {
            var _isolatedVertices = _Graph.Vertices.Where(v => _Graph.Degree(v) == 0).ToList();
            foreach (var isolatedVertex in _isolatedVertices)
                _Graph.RemoveVertex(isolatedVertex);
        }

        /// <summary>
        /// Removes the edges which source and target is the same vertex.
        /// </summary>
        private void RemoveLoops()
        {
            _Graph.RemoveEdgeIf(edge => edge.Source == edge.Target);
        }

        /// <summary>
        /// Removes the cycles from the original graph with simply reverting
        /// some edges.
        /// </summary>
        private void RemoveCycles()
        {
            //find the cycle edges with dfs
            var cycleEdges = new List<SugiEdge>();
            var dfsAlgo = new DepthFirstSearchAlgorithm<SugiVertex, SugiEdge>(_Graph);
            dfsAlgo.BackEdge += cycleEdges.Add;
            dfsAlgo.Compute();

            //and revert them
            foreach (var edge in cycleEdges)
            {
                _Graph.RemoveEdge(edge);
                _Graph.AddEdge(new SugiEdge(edge.OriginalEdge, edge.Target, edge.Source));
            }
        }
    }
}
