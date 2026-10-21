using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Batched resolver for VBEdge connector lookups.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A workflow design can contain hundreds of VBEdges. The previous approach gave
    /// every edge its own DispatcherTimer that retried up to 64 times, and every retry
    /// performed a full recursive visual-tree search (VBVisualTreeHelper
    /// .FindChildVBContentObjectInVisualTree). With N edges that meant N × 64 tree walks
    /// — the reason large workflows took a very long time until all edges were drawn.
    /// </para>
    /// <para>
    /// This resolver instead keeps pending edges in one shared queue and uses a SINGLE
    /// DispatcherTimer: on every tick the pending edges are grouped by their parent
    /// container, each container's visual tree is walked ONCE to collect all VBConnectors
    /// into a name dictionary, and all pending edges of that container are resolved
    /// against the dictionary. Cost per tick: one tree walk per container instead of one
    /// per edge. The timer stops itself when the queue is empty.
    /// </para>
    /// <para>
    /// Like the previous per-edge retry loop, resolution keeps retrying until the
    /// connectors appear (they may be created later by incremental design loading) or
    /// the per-edge attempt limit is reached.
    /// </para>
    /// </remarks>
    internal static class VBEdgeConnectorResolver
    {
        private const int ResolveIntervalMs = 10;

        private static readonly object _lock = new object();
        private static readonly List<VBEdge> _pendingEdges = new List<VBEdge>();
        private static DispatcherTimer _resolveTimer;

        /// <summary>
        /// Registers an edge for batched connector resolution. Call when the edge is
        /// loaded and its connectors are not resolvable yet.
        /// </summary>
        internal static void ScheduleResolve(VBEdge edge)
        {
            if (edge == null)
                return;
            lock (_lock)
            {
                if (!_pendingEdges.Contains(edge))
                    _pendingEdges.Add(edge);
                EnsureTimer();
            }
        }

        /// <summary>
        /// Removes an edge from the queue (e.g. when unloaded or destructed).
        /// </summary>
        internal static void Remove(VBEdge edge)
        {
            if (edge == null)
                return;
            lock (_lock)
            {
                _pendingEdges.Remove(edge);
                if (_pendingEdges.Count == 0 && _resolveTimer != null)
                    _resolveTimer.Stop();
            }
        }

        private static void EnsureTimer()
        {
            if (_resolveTimer != null && _resolveTimer.IsEnabled)
                return;
            _resolveTimer = new DispatcherTimer(
                TimeSpan.FromMilliseconds(ResolveIntervalMs), DispatcherPriority.Loaded,
                (s, e) => ResolvePending());
            _resolveTimer.Start();
        }

        private static void ResolvePending()
        {
            List<VBEdge> edges;
            lock (_lock)
            {
                if (_pendingEdges.Count == 0)
                {
                    _resolveTimer.Stop();
                    return;
                }
                edges = new List<VBEdge>(_pendingEdges);
                _pendingEdges.Clear();
            }

            var stillPending = new List<VBEdge>();
            var resolvedAny = false;

            // Group by parent container so each container tree is walked only once.
            foreach (var group in edges.Where(e => e.Parent is Control).GroupBy(e => (Control)e.Parent))
            {
                var connectorDict = new Dictionary<string, VBConnector>(StringComparer.Ordinal);
                CollectConnectors(group.Key, connectorDict);

                foreach (var edge in group)
                {
                    edge.IncrementResolveAttempt();
                    if (edge.ResolveConnectors(connectorDict))
                    {
                        resolvedAny = true;
                    }
                    else if (!edge.ResolveAttemptsExhausted)
                    {
                        stillPending.Add(edge);
                    }
                    // Attempts exhausted: drop silently (same as the old max-attempts cut-off).
                }
            }

            // Edges not yet attached to a parent stay pending (their connectors
            // cannot be searched without a container).
            foreach (var edge in edges.Where(e => !(e.Parent is Control)))
            {
                edge.IncrementResolveAttempt();
                if (!edge.ResolveAttemptsExhausted)
                    stillPending.Add(edge);
            }

            lock (_lock)
            {
                foreach (var edge in stillPending)
                {
                    if (!_pendingEdges.Contains(edge))
                        _pendingEdges.Add(edge);
                }
                if (_pendingEdges.Count == 0)
                {
                    _resolveTimer.Stop();
                }
            }

            if (resolvedAny)
                edgeRedrawInvalidation?.Invoke();
        }

        /// <summary>
        /// Called after any edge was resolved, so containers can invalidate rendering.
        /// Edges invalidate themselves; this hook exists for potential future use.
        /// </summary>
        internal static Action edgeRedrawInvalidation;

        /// <summary>
        /// Walks the visual tree of <paramref name="container"/> once and collects all
        /// VBConnectors by their VBContent name.
        /// </summary>
        private static void CollectConnectors(Visual container, Dictionary<string, VBConnector> dict)
        {
            foreach (Visual child in container.GetVisualChildren())
            {
                if (child is VBConnector connector)
                {
                    string name = connector.VBContent;
                    if (!string.IsNullOrEmpty(name) && !dict.ContainsKey(name))
                        dict[name] = connector;
                }
                CollectConnectors(child, dict);
            }
        }
    }
}
