using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.layoutengine.EfficientSugiyama
{
    public partial class SugiLayoutAlgorithm<TVertex, TEdge>
    {
        internal class AlternatingLayer : List<IData>, ICloneable
        {
            /// <summary>
            /// This method ensures that the layer is a real alternating
            /// layer: starts with a SegmentContainer followed by a Vertex,
            /// another SegmentContainer, another Vertex, ... ending wiht 
            /// a SegmentContainer.
            /// </summary>
            public void EnsureAlternatingAndPositions()
            {
                bool shouldBeAContainer = true;
                for (int i = 0; i < Count; i++, shouldBeAContainer = !shouldBeAContainer)
                {
                    if (shouldBeAContainer && this[i] is SugiVertex)
                    {
                        Insert(i, new SegmentContainer());
                    }
                    else
                    {
                        while (i < Count && !shouldBeAContainer && this[i] is SegmentContainer)
                        {
                            //the previous one must be a container too
                            var prevContainer = this[i - 1] as SegmentContainer;
                            var actualContainer = this[i] as SegmentContainer;
                            prevContainer.Join(actualContainer);
                            RemoveAt(i);
                        }
                        if (i >= Count)
                            break;
                    }
                }

                if (shouldBeAContainer)
                {
                    //the last element in the alternating layer 
                    //should be a container, but it's not
                    //so add an empty one
                    Add(new SegmentContainer());
                }
            }

            protected void EnsurePositions()
            {
                //assign positions to vertices on the actualLayer (L_i)
                for (int i = 1; i < this.Count; i += 2)
                {
                    var precedingContainer = this[i - 1] as SegmentContainer;
                    var vertex = this[i] as SugiVertex;
                    if (i == 1)
                    {
                        vertex.Position = precedingContainer.Count;
                    }
                    else
                    {
                        var previousVertex = this[i - 2] as SugiVertex;
                        vertex.Position = previousVertex.Position + precedingContainer.Count + 1;
                    }
                }

                //assign positions to containers on the actualLayer (L_i+1)
                for (int i = 0; i < this.Count; i += 2)
                {
                    var container = this[i] as SegmentContainer;
                    if (i == 0)
                    {
                        container.Position = 0;
                    }
                    else
                    {
                        var precedingVertex = this[i - 1] as SugiVertex;
                        container.Position = precedingVertex.Position + 1;
                    }
                }
            }

            public void SetPositions()
            {
                int nextPosition = 0;
                for (int i = 0; i < this.Count; i++)
                {
                    var segmentContainer = this[i] as SegmentContainer;
                    var vertex = this[i] as SugiVertex;
                    if (segmentContainer != null)
                    {
                        segmentContainer.Position = nextPosition;
                        nextPosition += segmentContainer.Count;
                    }
                    else if (vertex != null)
                    {
                        vertex.Position = nextPosition;
                        nextPosition += 1;
                    }
                }
            }

            public AlternatingLayer Clone()
            {
                var clonedLayer = new AlternatingLayer();
                foreach (var item in this)
                {
                    var cloneableItem = item as ICloneable;
                    if (cloneableItem != null)
                        clonedLayer.Add(cloneableItem.Clone() as IData);
                    else
                        clonedLayer.Add(item);
                }
                return clonedLayer;
            }

            #region ICloneable Members

            object ICloneable.Clone()
            {
                return this.Clone();
            }

            #endregion
        }

        internal interface ISegmentContainer : IEnumerable<Segment>, IData, ICloneable
        {
            /// <summary>
            /// Appends the segment <paramref name="s"/> to the end of the 
            /// container.
            /// </summary>
            /// <param name="s">The segment to append.</param>
            void Append(Segment s);

            /// <summary>
            /// Appends all elements of the container <paramref name="sc"/> to 
            /// this container.
            /// </summary>
            /// <param name="sc"></param>
            void Join(ISegmentContainer sc);

            /// <summary>
            /// Split this container at segment <paramref name="s"/> into two contsiners
            /// <paramref name="sc1"/> and <paramref name="sc2"/>. 
            /// All elements less than s are stored in container <paramref name="sc1"/> and
            /// those who are greated than <paramref name="s"/> in <paramref name="sc2"/>.
            /// Element <paramref name="s"/> is neither in <paramref name="sc1"/> or 
            /// <paramref name="sc2"/>.
            /// </summary>
            /// <param name="s">The segment to split at.</param>
            /// <param name="sc1">The container which contains the elements before <paramref name="s"/>.</param>
            /// <param name="sc2">The container which contains the elements after <paramref name="s"/>.</param>
            void Split(Segment s, out ISegmentContainer sc1, out ISegmentContainer sc2);

            /// <summary>
            /// Split the container at position <paramref name="k"/>. The first <paramref name="k"/>
            /// elements of the container are stored in <paramref name="sc1"/> and the remainder
            /// in <paramref name="sc2"/>.
            /// </summary>
            /// <param name="k">The index where the container should be splitted.</param>
            /// <param name="sc1">The container which contains the elements before <paramref name="sc2"/>.</param>
            /// <param name="sc2">The container which contains the elements after <paramref name="sc2"/>.</param>
            void Split(int k, out ISegmentContainer sc1, out ISegmentContainer sc2);

            int Count { get; }
        }

        internal class SegmentContainer : List<Segment>, ISegmentContainer
        {

            public SegmentContainer() { }
            public SegmentContainer(int capacity)
                : base(capacity) { }

            #region ISegmentContainer Members

            public void Append(Segment s)
            {
                Add(s);
            }

            public void Join(ISegmentContainer sc)
            {
                AddRange(sc);
            }

            public void Split(Segment s, out ISegmentContainer sc1, out ISegmentContainer sc2)
            {
                //Contract.Requires(Contains(s));
                //Contract.Ensures(sc1 != null);
                //Contract.Ensures(sc2 != null);

                int index = IndexOf(s);
                Split(index, out sc1, out sc2, false);
            }

            public void Split(int k, out ISegmentContainer sc1, out ISegmentContainer sc2)
            {
                //Contract.Requires(k < Count);
                //Contract.Ensures(sc1 != null);
                //Contract.Ensures(sc2 != null);

                Split(k, out sc1, out sc2, true);
            }

            protected void Split(int k, out ISegmentContainer sc1, out ISegmentContainer sc2, bool keep)
            {
                //Contract.Requires(k < Count);
                //Contract.Ensures(sc1 != null);
                //Contract.Ensures(sc2 != null);

                int sc1Count = k + (keep ? 1 : 0);
                int sc2Count = Count - k - 1;

                sc1 = new SegmentContainer(sc1Count);
                sc2 = new SegmentContainer(sc2Count);

                for (int i = 0; i < sc1Count; i++)
                    sc1.Append(this[i]);

                for (int i = k + 1; i < Count; i++)
                    sc2.Append(this[i]);
            }
            #endregion

            #region IData Members

            //TODO get them from the first element of the container, MAYBE!
            public int Position { get; set; }

            #endregion

            #region ICloneable Members

            public object Clone()
            {
                return this.MemberwiseClone();
            }

            #endregion
        }
    }
}
