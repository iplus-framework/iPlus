using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Controls;

namespace gip.core.reporthandlerwpf.Flowdoc
{
    /// <summary>
    /// THe delegate type of the event that will be raised
    /// </summary>
    public delegate void DocumentVisitedEventHandler(object sender, DocumentVisitedEventArgs e);
    public class DocumentVisitedEventArgs : EventArgs
    {
        public DocumentVisitedEventArgs(object visitedObject, object parentObject, bool start, int recursionDepth)
        {
            _VisitedObject = visitedObject;
            _ParentObject = parentObject;
            _Start = start;
            _RecursionDepth = recursionDepth;
        }

        private object _VisitedObject;
        public object VisitedObject
        {
            get
            {
                return _VisitedObject;
            }
        }

        private object _ParentObject;
        public object ParentObject
        {
            get
            {
                return _ParentObject;
            }
        }

        private bool _Start;
        public bool Start
        {
            get
            {
                return _Start;
            }
        }

        private int _RecursionDepth;
        public int RecursionDepth
        {
            get
            {
                return _RecursionDepth;
            }
        }

        private bool _Handled;
        public bool Handled
        {
            get
            {
                return _Handled;
            }
            set
            {
                _Handled = value;
            }
        }
    }

    /// <summary>
    /// The Document walker class enables a traversal of the flow document tree and raises an event for each node 
    /// in the document tree. I used it to find all instances of a FormattedRun control in the flowdocument defintion. 
    /// It is pretty straightforward, so I will just show the code itself. 
    /// </summary>
    public class DocumentWalker
    {
        private object _tag = null;
        /// <summary>
        /// Gets or sets the tag associated to this walker
        /// </summary>
        public object Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        /// <summary>
        /// This is the event to hook on.
        /// </summary>
        public event DocumentVisitedEventHandler VisualVisited;

        /// <summary>
        /// Traverses whole document
        /// </summary>
        /// <param name="fd">FlowDocument</param>
        /// <returns>list of inlines</returns>
        public List<Inline> Walk(FlowDocument fd)
        {
            return TraverseBlockCollection<Inline>(fd.Blocks, 0);
        }

        /// <summary>
        /// Traverses whole document
        /// </summary>
        /// <param name="fd">FlowDocument</param>
        /// <returns>list of inlines</returns>
        public List<T> Walk<T>(FlowDocument fd) where T : class
        {
            return TraverseBlockCollection<T>(fd.Blocks, 0);
        }

        /// <summary>
        /// Traverses an InlineCollection
        /// </summary>
        /// <param name="inlines">InlineCollection to be traversed</param>
        /// <returns>list of inlines</returns>
        public List<T> TraverseInlines<T>(InlineCollection inlines, int recursionDepth) where T : class
        {
            recursionDepth++;
            List<T> res = new List<T>();
            if (inlines != null && inlines.Count > 0)
            {
                Inline il = inlines.FirstInline;
                while (il != null)
                {
                    if (il is T) res.Add(il as T);

                    Run r = il as Run;
                    if (r != null)
                    {
                        if (VisualVisited != null)
                        {
                            DocumentVisitedEventArgs eventArgs = new DocumentVisitedEventArgs(r, inlines, true, recursionDepth);
                            VisualVisited(this, eventArgs);
                            if (eventArgs.Handled)
                                break;
                        }
                        il = il.NextInline;
                        continue;
                    }


                    Span sp = il as Span;
                    if (sp != null)
                    {
                        if (VisualVisited != null)
                        {
                            DocumentVisitedEventArgs eventArgs = new DocumentVisitedEventArgs(sp, inlines, true, recursionDepth);
                            VisualVisited(this, eventArgs);
                            if (eventArgs.Handled)
                                break;
                        }

                        res.AddRange(TraverseInlines<T>(sp.Inlines, recursionDepth));
                        il = il.NextInline;
                        continue;
                    }

                    InlineUIContainer uc = il as InlineUIContainer;
                    if (uc != null)
                    {
                        if (VisualVisited != null)
                        {
                            DocumentVisitedEventArgs eventArgs = new DocumentVisitedEventArgs(uc, inlines, true, recursionDepth);
                            VisualVisited(this, eventArgs);
                            if (eventArgs.Handled)
                                break;
                        }
                        if (uc.Child != null)
                        {
                            if (uc.Child is T)
                                res.Add(uc.Child as T);
                            TextBlock tb = uc.Child as TextBlock;
                            if (tb != null)
                                res.AddRange(TraverseInlines<T>(tb.Inlines, recursionDepth));
                        }

                        il = il.NextInline;
                        continue;
                    }
                    Figure fg = il as Figure;
                    if (fg != null)
                    {
                        if (VisualVisited != null)
                        {
                            DocumentVisitedEventArgs eventArgs = new DocumentVisitedEventArgs(fg, inlines, true, recursionDepth);
                            VisualVisited(this, eventArgs);
                            if (eventArgs.Handled)
                                break;
                        }
                        res.AddRange(TraverseBlockCollection<T>(fg.Blocks, recursionDepth));
                    }
                    il = il.NextInline;
                }
            }
            return res;
        }


        /// <summary>
        /// Traverses only passed paragraph
        /// </summary>
        /// <param name="p">paragraph</param>
        /// <returns>list of inlines</returns>
        public List<T> TraverseParagraph<T>(Paragraph p, int recursionDepth) where T : class
        {
            return TraverseInlines<T>(p.Inlines, recursionDepth);
        }

        /// <summary>
        /// Traverses passed block collection
        /// </summary>
        /// <param name="blocks">blocks to be traversed</param>
        /// <returns>list of inlines</returns>
        ///    
        public List<T> TraverseBlockCollection<T>(IEnumerable<Block> blocks, int recursionDepth) where T : class
        {
            recursionDepth++;
            List<T> res = new List<T>();
            foreach (Block b in blocks)
            {
                if (b is T)
                {
                    if (VisualVisited != null)
                    {
                        DocumentVisitedEventArgs eventArgs = new DocumentVisitedEventArgs(b, blocks, true, recursionDepth);
                        VisualVisited(this, eventArgs);
                        if (eventArgs.Handled)
                            break;
                    }
                    res.Add(b as T);
                }

                Paragraph p = b as Paragraph;
                if (p != null)
                {
                    if (VisualVisited != null)
                    {
                        DocumentVisitedEventArgs eventArgs = new DocumentVisitedEventArgs(b, blocks, true, recursionDepth);
                        VisualVisited(this, eventArgs);
                        if (eventArgs.Handled)
                            break;
                    }
                    res.AddRange(TraverseParagraph<T>(p, recursionDepth));
                    continue;
                }

                BlockUIContainer bui = b as BlockUIContainer;
                if (bui != null)
                {
                    if (VisualVisited != null)
                    {
                        DocumentVisitedEventArgs eventArgs = new DocumentVisitedEventArgs(bui.Child, blocks, true, recursionDepth);
                        VisualVisited(this, eventArgs);
                        if (eventArgs.Handled)
                            break;
                    }
                    continue;
                }

                Section s = b as Section;
                if (s != null)
                {
                    if (VisualVisited != null)
                    {
                        DocumentVisitedEventArgs eventArgs = new DocumentVisitedEventArgs(s, blocks, true, recursionDepth);
                        VisualVisited(this, eventArgs);
                        if (eventArgs.Handled)
                            break;
                    }
                    res.AddRange(TraverseBlockCollection<T>(s.Blocks, recursionDepth));
                    continue;
                }

                Table t = b as Table;
                if (t != null)
                {
                    if (VisualVisited != null)
                    {
                        DocumentVisitedEventArgs eventArgs = new DocumentVisitedEventArgs(t, blocks, true, recursionDepth);
                        VisualVisited(this, eventArgs);
                        if (eventArgs.Handled)
                            break;
                    }
                    foreach (TableRowGroup trg in t.RowGroups)
                    {
                        if (VisualVisited != null)
                        {
                            DocumentVisitedEventArgs eventArgs = new DocumentVisitedEventArgs(trg, blocks, true, recursionDepth);
                            VisualVisited(this, eventArgs);
                            if (eventArgs.Handled)
                                break;
                        }
                        foreach (TableRow tr in trg.Rows)
                        {
                            if (VisualVisited != null)
                            {
                                DocumentVisitedEventArgs eventArgs = new DocumentVisitedEventArgs(tr, blocks, true, recursionDepth);
                                VisualVisited(this, eventArgs);
                                if (eventArgs.Handled)
                                    break;
                            }
                            if (tr is T)
                                res.Add(tr as T);

                            foreach (TableCell tc in tr.Cells)
                            {
                                if (VisualVisited != null)
                                {
                                    DocumentVisitedEventArgs eventArgs = new DocumentVisitedEventArgs(tc, blocks, true, recursionDepth);
                                    VisualVisited(this, eventArgs);
                                    if (eventArgs.Handled)
                                        break;
                                }
                                res.AddRange(TraverseBlockCollection<T>(tc.Blocks, recursionDepth));
                            }
                        }
                    }
                    continue;
                }
            }
            return res;
        }
    }
}
