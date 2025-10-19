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

        private ScrollViewer _sv1;
        private ScrollViewer _sv2;

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

        private bool sv1HorizontalVisible;
        private bool sv2HorizontalVisible;

        public ScrollViewerSyncer(ScrollViewer sv1, ScrollViewer sv2)
        {
            if (sv1 == null) throw new ArgumentNullException("sv1");
            if (sv2 == null) throw new ArgumentNullException("sv2");

            this._sv1 = sv1;
            this._sv2 = sv2;

            sv1HorizontalVisible =
                sv1.HorizontalScrollBarVisibility == ScrollBarVisibility.Visible;
            sv2HorizontalVisible =
                sv2.HorizontalScrollBarVisibility == ScrollBarVisibility.Visible;


            sv1.ScrollChanged += sv1_ScrollChanged;
            sv2.ScrollChanged += sv2_ScrollChanged;
        }

        

        public void DeInitControl()
        {
            if (_sv1 != null)
                _sv1.ScrollChanged -= sv1_ScrollChanged;

            if (_sv2 != null)
                _sv2.ScrollChanged -= sv2_ScrollChanged;

            _sv1 = null;
            _sv2 = null;
            //_sv1HSB = null;
            //_sv2HSB = null;
        }

        private void sv2_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.Source != _sv2) 
                return;

            if (e.OffsetDelta != null && e.OffsetDelta.Y != 0)
            {
                _sv1.Offset = _sv2.Offset;
            }

            bool sv2NewHV = _sv2.HorizontalScrollBarVisibility == ScrollBarVisibility.Visible;// && sv2HSB != null && sv2HSB.IsEnabled;
            if (sv2HorizontalVisible != sv2NewHV)
            {
                sv2HorizontalVisible = sv2NewHV;
                MatchHeightDifferences();
            }
        }

        private void sv1_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.Source != _sv1) 
                return;

            if (e.OffsetDelta != null && e.OffsetDelta.Y != 0)
            {
                _sv2.Offset = _sv1.Offset;
            }

            bool sv1NewHV = _sv1.HorizontalScrollBarVisibility == ScrollBarVisibility.Visible;// && sv1HSB != null && sv1HSB.IsEnabled;
            if (sv1HorizontalVisible != sv1NewHV)
            {
                sv1HorizontalVisible = sv1NewHV;
                MatchHeightDifferences();
            }
        }


        private void MatchHeightDifferences()
        {
                if (!sv1HorizontalVisible && sv2HorizontalVisible)
                {
                    _sv1.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
                }
                else if (sv1HorizontalVisible && !sv2HorizontalVisible)
                {
                    _sv2.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
                }
                else if(!sv1HorizontalVisible && !sv2HorizontalVisible)
                {
                    _sv1.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                    _sv2.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                }

        }
    }
}
