﻿using QuickGraph;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace gip.core.layoutengine.EfficientSugiyama
{
    public partial class SugiLayoutAlgorithm<TVertex, TEdge>
    {
        private readonly IMutableBidirectionalGraph<Data, IEdge<Data>> _sparseCompactionGraph
            = new BidirectionalGraph<Data, IEdge<Data>>();

        private double[] _layerHeights;
        private double[] _layerPositions;

        private enum LeftRightMode : byte
        {
            Left = 0,
            Right = 1
        }

        private enum UpperLowerEdges : byte
        {
            Upper = 0,
            Lower = 1
        }

        private void CalculatePositions()
        {
            PutbackIsolatedVertices();
            CalculateLayerHeightsAndPositions();
            CalculateVerticalPositions();

            if (_PositionMode < 0 || _PositionMode == 0)
                CalculateHorizontalPositions(LeftRightMode.Left, UpperLowerEdges.Upper);
            if (_PositionMode < 0 || _PositionMode == 1)
                CalculateHorizontalPositions(LeftRightMode.Right, UpperLowerEdges.Upper);
            if (_PositionMode < 0 || _PositionMode == 2)
                CalculateHorizontalPositions(LeftRightMode.Left, UpperLowerEdges.Lower);
            if (_PositionMode < 0 || _PositionMode == 3)
                CalculateHorizontalPositions(LeftRightMode.Right, UpperLowerEdges.Lower);

            CalculateRealPositions();
            //DoEdgeRouting();

            SavePositions();
        }

        private void DoEdgeRouting()
        {
            DoTraditionalEdgeRouting();
        }

        private void DoTraditionalEdgeRouting()
        {
            foreach (var kvp in _dummyVerticesOfEdges)
            {
                var routePoints = new Point[kvp.Value.Count];
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    var vertex = kvp.Value[i];
                    routePoints[i] = new Point(vertex.HorizontalPosition, vertex.VerticalPosition);
                }
                _edgeRoutingPoints[kvp.Key] = routePoints;
            }
        }

        private void SavePositions()
        {
            foreach (var vertex in _Graph.Vertices)
            {
                if (vertex.Type == VertexTypes.Original)
                    VertexPositions[vertex.OriginalVertex] = new Point(vertex.HorizontalPosition, vertex.VerticalPosition);
            }

        }

        private void PutbackIsolatedVertices()
        {
            _sparseCompactionGraph.AddVertexRange(_isolatedVertices.OfType<Data>());
            _Graph.AddVertexRange(_isolatedVertices);
            int layer = 0;
            SugiVertex prevIsolatedVertex = null;
            foreach (var isolatedVertex in _isolatedVertices)
            {
                var lastOnLayer = _sparseCompactionByLayerBackup[layer].LastOrDefault();
                _layers[layer].Add(isolatedVertex);
                isolatedVertex.LayerIndex = layer;
                isolatedVertex.Position = _layers[layer].Count - 1;
                if (lastOnLayer != null)
                {
                    var edge = new Edge<Data>(lastOnLayer.Target, isolatedVertex);
                    _sparseCompactionByLayerBackup[layer].Add(edge);
                    _sparseCompactionGraph.AddEdge(edge);
                }
                if (layer > 0 && prevIsolatedVertex != null)
                {
                    _Graph.AddEdge(new SugiEdge(default(TEdge), prevIsolatedVertex, isolatedVertex));
                }
                layer = (layer + 1) % _layers.Count;
                prevIsolatedVertex = isolatedVertex;
            }
        }

        private void CalculateLayerHeightsAndPositions()
        {
            _layerHeights = new double[_layers.Count];
            for (int i = 0; i < _layers.Count; i++)
                _layerHeights[i] = _layers[i].Max(v => v.Size.Height);

            double layerDistance = _LayerDistance;
            _layerPositions = new double[_layers.Count];
            _layerPositions[0] = 0;
            for (int i = 1; i < _layers.Count; i++)
                _layerPositions[i] = _layerPositions[i - 1] + _layerHeights[i - 1] + layerDistance;

        }

        private void CalculateRealPositions()
        {
            foreach (var vertex in _Graph.Vertices)
            {
#if DEBUG
                Debug.WriteLine(string.Format("{0}:\t{1}\t{2}\t{3}\t{4}",
                    vertex.OriginalVertex,
                    vertex.HorizontalPositions[0],
                    vertex.HorizontalPositions[1],
                    vertex.HorizontalPositions[2],
                    vertex.HorizontalPositions[3]));
#endif
                if (_PositionMode < 0)
                {
                    vertex.HorizontalPosition =
                        (vertex.HorizontalPositions[0] + vertex.HorizontalPositions[1]
                         + vertex.HorizontalPositions[2] + vertex.HorizontalPositions[3]) / 4.0;
                }
                else
                {
                    vertex.HorizontalPosition = vertex.HorizontalPositions[_PositionMode];
                }
            }
        }

        private void CalculateVerticalPositions()
        {
            foreach (var vertex in _Graph.Vertices)
            {
                vertex.VerticalPosition = _layerPositions[vertex.LayerIndex] + (vertex.Size.Height <= 0 ? _layerHeights[vertex.LayerIndex] : vertex.Size.Height) / 2.0;
            }
        }

        /// <summary>
        /// Calculates the horizontal positions based on the selected modes.
        /// </summary>
        /// <param name="leftRightMode">Mode of the vertical alignment.</param>
        /// <param name="upperLowerEdges">Alignment based on which edges (upper or lower ones).</param>
        private void CalculateHorizontalPositions(LeftRightMode leftRightMode, UpperLowerEdges upperLowerEdges)
        {
            int modeIndex = (byte)upperLowerEdges * 2 + (byte)leftRightMode;
            InitializeRootsAndAligns(modeIndex);
            DoAlignment(modeIndex, leftRightMode, upperLowerEdges);
            WriteOutAlignment(modeIndex);
            InitializeSinksAndShifts(modeIndex);
            DoHorizontalCompaction(modeIndex, leftRightMode, upperLowerEdges);
        }

        private void WriteOutAlignment(int modeIndex)
        {
#if DEBUG
            Debug.WriteLine(string.Format("Alignment for {0}", modeIndex));
            foreach (var vertex in _Graph.Vertices)
                Debug.WriteLine(string.Format("{0},{1},{2}: Root {3},{4},{5}\tAlign {6},{7},{8}",
                    vertex.OriginalVertex,
                    vertex.LayerIndex,
                    vertex.Type,
                    vertex.Roots[modeIndex].OriginalVertex,
                    vertex.Roots[modeIndex].LayerIndex,
                    vertex.Roots[modeIndex].Type,
                    vertex.Aligns[modeIndex].OriginalVertex,
                    vertex.Aligns[modeIndex].LayerIndex,
                    vertex.Aligns[modeIndex].Type));
#endif
        }

        private void InitializeSinksAndShifts(int modeIndex)
        {
            foreach (var vertex in _Graph.Vertices)
            {
                vertex.Sinks[modeIndex] = vertex;
                vertex.Shifts[modeIndex] = double.PositiveInfinity;
                vertex.HorizontalPositions[modeIndex] = double.NaN;
            }
        }

        private void PlaceBlock(int modeIndex, LeftRightMode leftRightMode, UpperLowerEdges upperLowerEdges, SugiVertex v)
        {
            if (!double.IsNaN(v.HorizontalPositions[modeIndex]))
                return;

            double delta = _VertexDistance;
            v.HorizontalPositions[modeIndex] = 0;
            Data w = v;
            do
            {
                SugiVertex wVertex = w as SugiVertex;
                Segment wSegment = w as Segment;
                if (_sparseCompactionGraph.ContainsVertex(w) &&
                    ((leftRightMode == LeftRightMode.Left && _sparseCompactionGraph.InDegree(w) > 0)
                      || (leftRightMode == LeftRightMode.Right && _sparseCompactionGraph.OutDegree(w) > 0)))
                {
                    var edges = leftRightMode == LeftRightMode.Left
                        ? _sparseCompactionGraph.InEdges(w)
                        : _sparseCompactionGraph.OutEdges(w);
                    foreach (var edge in edges)
                    {
                        SugiVertex u = null;
                        Data pred = leftRightMode == LeftRightMode.Left ? edge.Source : edge.Target;
                        if (pred is SugiVertex)
                            u = ((SugiVertex)pred).Roots[modeIndex];
                        else
                        {
                            var segment = (Segment)pred;
                            u = upperLowerEdges == UpperLowerEdges.Upper ? segment.PVertex.Roots[modeIndex] : segment.QVertex.Roots[modeIndex];
                        }
                        PlaceBlock(modeIndex, leftRightMode, upperLowerEdges, u);
                        if (v.Sinks[modeIndex] == v)
                            v.Sinks[modeIndex] = u.Sinks[modeIndex];
                        //var xDelta = delta + (v.Roots[modeIndex].BlockWidths[modeIndex] + u.BlockWidths[modeIndex]) / 2.0;
                        var xDelta = delta + ((wVertex != null ? wVertex.Size.Width : 0.0) + ((pred is SugiVertex) ? ((SugiVertex)pred).Size.Width : u.BlockWidths[modeIndex])) / 2.0;
                        if (v.Sinks[modeIndex] != u.Sinks[modeIndex])
                        {
                            var s = leftRightMode == LeftRightMode.Left
                                ? v.HorizontalPositions[modeIndex] - u.HorizontalPositions[modeIndex] - xDelta
                                : u.HorizontalPositions[modeIndex] - v.HorizontalPositions[modeIndex] - xDelta;

                            u.Sinks[modeIndex].Shifts[modeIndex] = leftRightMode == LeftRightMode.Left
                                ? Math.Min(u.Sinks[modeIndex].Shifts[modeIndex], s)
                                : Math.Max(u.Sinks[modeIndex].Shifts[modeIndex], s);
                        }
                        else
                        {
                            v.AddHorizontalPosition(modeIndex, leftRightMode == LeftRightMode.Left
                                    ? Math.Max(v.HorizontalPositions[modeIndex], u.HorizontalPositions[modeIndex] + xDelta)
                                    : Math.Min(v.HorizontalPositions[modeIndex], u.HorizontalPositions[modeIndex] - xDelta));
                        }
                    }
                }
                if (wSegment != null)
                    w = (upperLowerEdges == UpperLowerEdges.Upper) ? wSegment.QVertex : wSegment.PVertex;
                else if (wVertex.Type == VertexTypes.PVertex && upperLowerEdges == UpperLowerEdges.Upper)
                    w = wVertex.Segment;
                else if (wVertex.Type == VertexTypes.QVertex && upperLowerEdges == UpperLowerEdges.Lower)
                    w = wVertex.Segment;
                else
                    w = wVertex.Aligns[modeIndex];
            } while (w != v);
        }

        private void DoHorizontalCompaction(int modeIndex, LeftRightMode leftRightMode, UpperLowerEdges upperLowerEdges)
        {
            foreach (var vertex in _Graph.Vertices)
            {
                if (vertex.Roots[modeIndex] == vertex)
                    PlaceBlock(modeIndex, leftRightMode, upperLowerEdges, vertex);
            }

            foreach (var vertex in _Graph.Vertices)
            {
                vertex.AddHorizontalPosition(modeIndex, vertex.Roots[modeIndex].HorizontalPositions[modeIndex]);
                if (vertex.Roots[modeIndex].Sinks[modeIndex].Shifts[modeIndex] < double.PositiveInfinity &&
                    vertex.Roots[modeIndex] == vertex)
                    vertex.HorizontalPositions[modeIndex] += vertex.Roots[modeIndex].Sinks[modeIndex].Shifts[modeIndex];
            }
        }

        private void DoAlignment(int modeIndex, LeftRightMode leftRightMode, UpperLowerEdges upperLowerEdges)
        {
            int layerStart, layerEnd, layerStep;
            if (upperLowerEdges == UpperLowerEdges.Upper)
            {
                layerStart = 0;
                layerEnd = _layers.Count;
                layerStep = 1;
            }
            else
            {
                layerStart = _layers.Count - 1;
                layerEnd = -1;
                layerStep = -1;
            }
            for (int i = layerStart; i != layerEnd; i += layerStep)
            {
                int r = leftRightMode == LeftRightMode.Left ? int.MinValue : int.MaxValue;
                var layer = _layers[i];
                int vertexStart, vertexEnd, vertexStep;
                if (leftRightMode == LeftRightMode.Left)
                {
                    vertexStart = 0;
                    vertexEnd = layer.Count;
                    vertexStep = 1;
                }
                else
                {
                    vertexStart = layer.Count - 1;
                    vertexEnd = -1;
                    vertexStep = -1;
                }
                for (int j = vertexStart; j != vertexEnd; j += vertexStep)
                {
                    var vertex = layer[j];
                    if (vertex.Type == VertexTypes.Original
                        || vertex.Type == VertexTypes.RVertex
                        || (vertex.Type == VertexTypes.PVertex && upperLowerEdges == UpperLowerEdges.Upper)
                        || (vertex.Type == VertexTypes.QVertex && upperLowerEdges == UpperLowerEdges.Lower))
                    {
                        List<SugiEdge> neighbourEdges = null;
                        neighbourEdges = upperLowerEdges == UpperLowerEdges.Upper
                            ? _Graph.InEdges(vertex).OrderByDescending(e => e.Source.Position).ToList()
                            : _Graph.OutEdges(vertex).OrderByDescending(e => e.Target.Position).ToList();
                        if (neighbourEdges.Count <= 0)
                            continue;

                        int c1 = (int)Math.Floor((neighbourEdges.Count + 1) / 2.0) - 1;
                        int c2 = (int)Math.Ceiling((neighbourEdges.Count + 1) / 2.0) - 1;
                        int[] medians = null;
                        if (c1 == c2)
                        {
                            medians = new int[1] { c1 };
                        }
                        else
                        {
                            medians = leftRightMode == LeftRightMode.Left
                                ? new int[2] { c1, c2 }
                                : new int[2] { c2, c1 };
                        }
                        for (int m = 0; m < medians.Length; m++)
                        {
                            if (vertex.Aligns[modeIndex] != vertex)
                                continue;
                            var edge = neighbourEdges[medians[m]];
#if DEBUG
                            if (edge.Marked)
                                Debug.WriteLine("Edge marked: " + edge.Source.OriginalVertex + ", " + edge.Target.OriginalVertex);
#endif
                            var neighbour = edge.OtherVertex(vertex);
                            if (!edge.Marked &&
                                ((leftRightMode == LeftRightMode.Left && r < neighbour.Position)
                                    || (leftRightMode == LeftRightMode.Right && r > neighbour.Position)))
                            {
                                neighbour.Aligns[modeIndex] = vertex;
                                neighbour.BlockWidths[modeIndex] = Math.Max(neighbour.BlockWidths[modeIndex], vertex.Size.Width);
                                vertex.Roots[modeIndex] = neighbour.Roots[modeIndex];
                                vertex.Aligns[modeIndex] = vertex.Roots[modeIndex];
                                r = neighbour.Position;
                            }
                        }
                    }
                    else if (vertex.Type == VertexTypes.PVertex /*&& upperLowerEdges == UpperLowerEdges.Lower*/)
                    {
                        //align the segment of the PVertex
                        vertex.Roots[modeIndex] = vertex.Segment.QVertex.Roots[modeIndex];
                        vertex.Aligns[modeIndex] = vertex.Roots[modeIndex];
                        r = vertex.Segment.Position;
                    }
                    else if (vertex.Type == VertexTypes.QVertex /*&& upperLowerEdges == UpperLowerEdges.Upper*/)
                    {
                        //align the segment of the QVertex
                        vertex.Roots[modeIndex] = vertex.Segment.PVertex.Roots[modeIndex];
                        vertex.Aligns[modeIndex] = vertex.Roots[modeIndex];
                        r = vertex.Segment.Position;
                    }
                }
            }
        }

        private void InitializeRootsAndAligns(int modeIndex)
        {
            foreach (var vertex in _Graph.Vertices)
            {
                vertex.Roots[modeIndex] = vertex;
                vertex.Aligns[modeIndex] = vertex;
                vertex.BlockWidths[modeIndex] = vertex.Size.Width;
            }
        }
    }
}
