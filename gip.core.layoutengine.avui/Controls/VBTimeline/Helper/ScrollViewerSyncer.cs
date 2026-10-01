using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.core.layoutengine.avui.timeline
{
    public class ScrollViewerSyncer
    {
        // Both sides consist of NESTED ItemsControls - the actual scrolling may
        // happen in ANY of the inner ScrollViewers, so all of them are hooked and
        // the one that really scrolls drives the counterpart.
        private readonly List<ScrollViewer> _sv1List = new List<ScrollViewer>();
        private readonly List<ScrollViewer> _sv2List = new List<ScrollViewer>();
        private bool _isSyncing;

        public ScrollViewerSyncer(IEnumerable<ScrollViewer> sv1List, IEnumerable<ScrollViewer> sv2List)
        {
            if (sv1List == null) throw new ArgumentNullException("sv1List");
            if (sv2List == null) throw new ArgumentNullException("sv2List");

            _sv1List.AddRange(sv1List);
            _sv2List.AddRange(sv2List);

            foreach (var sv in _sv1List)
                sv.ScrollChanged += sv1_ScrollChanged;
            foreach (var sv in _sv2List)
                sv.ScrollChanged += sv2_ScrollChanged;
        }

        //private ScrollBar _sv1HSB;
        //private ScrollBar sv1HSB
        //{
        //    get
        //    {
        //        if (_sv1HSB == null)
        //            _sv1HSB = _sv1?.Template.FindName("PART_HorizontalScrollBar", _sv1) as ScrollBar;
        //        return _sv1HSB;
        //    }
        //}
        
        //private ScrollBar _sv2HSB;
        //private ScrollBar sv2HSB
        //{
        //    get
        //    {
        //        if(_sv2HSB == null)
        //            _sv2HSB = _sv2?.Template.FindName("PART_HorizontalScrollBar", _sv2) as ScrollBar;
        //        return _sv2HSB;
        //    }
        //}

        public void DeInitControl()
        {
            foreach (var sv in _sv1List)
                sv.ScrollChanged -= sv1_ScrollChanged;
            foreach (var sv in _sv2List)
                sv.ScrollChanged -= sv2_ScrollChanged;
            _sv1List.Clear();
            _sv2List.Clear();
        }

        private void sv2_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_isSyncing || e.OffsetDelta.Y == 0)
                return;
            Sync(_sv2List, _sv1List);
        }

        private void sv1_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_isSyncing || e.OffsetDelta.Y == 0)
                return;
            Sync(_sv1List, _sv2List);
        }

        private void Sync(List<ScrollViewer> fromList, List<ScrollViewer> toList)
        {
            // The driver is the viewer that actually scrolled.
            ScrollViewer driver = fromList.FirstOrDefault(sv => sv.Offset.Y != 0)
                                  ?? fromList.FirstOrDefault();
            if (driver == null)
                return;
            _isSyncing = true;
            try
            {
                foreach (var target in toList)
                {
                    if (driver.Offset.Y != target.Offset.Y)
                        target.Offset = new Avalonia.Vector(target.Offset.X, driver.Offset.Y);
                }
            }
            finally
            {
                _isSyncing = false;
            }
        }
    }
}
