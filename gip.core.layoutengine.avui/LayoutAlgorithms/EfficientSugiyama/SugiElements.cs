using Avalonia;
using QuickGraph;
using System.Diagnostics;

namespace gip.core.layoutengine.avui.EfficientSugiyama
{
    public partial class SugiLayoutAlgorithm<TVertex, TEdge>
    {
        internal enum VertexTypes
        {
            Original,
            PVertex,
            QVertex,
            RVertex
        }

        internal enum EdgeTypes
        {
            NonInnerSegment,
            InnerSegment
        }

        internal interface IData
        {
            int Position { get; set; }
        }

        internal abstract class Data : IData
        {
            public int Position { get; set; }

            /* Used by horizontal assignment */
            public readonly Data[] Sinks = new Data[4];
            public readonly double[] Shifts = new double[4] { double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity };
        }

        internal abstract class SugiVertex<TVertexC> : Data
        {
            public TVertexC OriginalVertex;
            public VertexTypes Type;
            public Segment Segment;
            public int LayerIndex { get; set; }
            public double MeasuredPosition { get; set; }

            public SugiVertex() { }

            public SugiVertex(TVertexC originalVertex)
            {
                OriginalVertex = originalVertex;
                Type = VertexTypes.Original;
                Segment = null;
            }
        }

        [DebuggerDisplay("{Type}: {OriginalVertex} - {Position} ; {MeasuredPosition} on layer {LayerIndex}")]
        internal class SugiVertex : SugiVertex<TVertex>
        {
            public readonly double[] HorizontalPositions = new double[4] { double.NaN, double.NaN, double.NaN, double.NaN };
            public double HorizontalPosition = double.NaN;
            public double VerticalPosition = double.NaN;
            public readonly SugiVertex[] Roots = new SugiVertex[4];
            public readonly SugiVertex[] Aligns = new SugiVertex[4];
            public readonly double[] BlockWidths = new double[4] { double.NaN, double.NaN, double.NaN, double.NaN };
            public int IndexInsideLayer;
            public int PermutationIndex;
            public int TempPosition;
            public bool DoNotOpt;
            public readonly Size Size;

            public void AddHorizontalPosition(int modeIndex, double value)
            {
                HorizontalPositions[modeIndex] = value;
            }

            public SugiVertex()
            {
                Size = new Size();
            }

            public SugiVertex(TVertex originalVertex, Size size)
                : base(originalVertex)
            {
                Size = size;
            }

            public void SavePositionToTemp()
            {
                TempPosition = Position;
            }

            public void LoadPositionFromTemp()
            {
                Position = TempPosition;
            }
        }

        internal class Segment : Data
        {
            /// <summary>
            /// Gets or sets the p-vertex of the segment.
            /// </summary>
            public SugiVertex PVertex;

            /// <summary>
            /// Gets or sets the q-vertex of the segment.
            /// </summary>
            public SugiVertex QVertex;
        }

        internal class SugiEdge : TaggedEdge<SugiVertex, TEdge>
        {
            public SugiEdge(TEdge originalEdge, SugiVertex source, SugiVertex target)
                : base(source, target, originalEdge) { }

            /// <summary>
            /// Gets the original edge of this SugiEdge.
            /// </summary>
            public TEdge OriginalEdge { get { return this.Tag; } }

            /// <summary>
            /// Gets or sets that the edge is included in a 
            /// type 1 conflict as a non-inner segment (true) or not (false).
            /// </summary>
            public bool Marked = false;

            public bool TempMark = false;

            public void SaveMarkedToTemp()
            {
                TempMark = Marked;
            }

            public void LoadMarkedFromTemp()
            {
                Marked = TempMark;
            }
        }
    }
}
