using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace gip.core.layoutengine.avui.timeline
{
    public class ScrollViewerSyncer
    {

        private ScrollViewer sv1;
        private ScrollViewer sv2;

        private ScrollBar _sv1HSB;
        private ScrollBar sv1HSB
        {
            get
            {
                if (_sv1HSB == null)
                    _sv1HSB = sv1?.Template.FindName("PART_HorizontalScrollBar", sv1) as ScrollBar;
                return _sv1HSB;
            }
        }
        
        private ScrollBar _sv2HSB;
        private ScrollBar sv2HSB
        {
            get
            {
                if(_sv2HSB == null)
                    _sv2HSB = sv2?.Template.FindName("PART_HorizontalScrollBar", sv2) as ScrollBar;
                return _sv2HSB;
            }
        }

        private bool sv1HorizontalVisible;
        private bool sv2HorizontalVisible;

        public ScrollViewerSyncer(ScrollViewer sv1, ScrollViewer sv2)
        {
            if (sv1 == null) throw new ArgumentNullException("sv1");
            if (sv2 == null) throw new ArgumentNullException("sv2");

            this.sv1 = sv1;
            this.sv2 = sv2;

            sv1HorizontalVisible =
                sv1.ComputedHorizontalScrollBarVisibility == Visibility.Visible;
            sv2HorizontalVisible =
                sv2.ComputedHorizontalScrollBarVisibility == Visibility.Visible;


            sv1.ScrollChanged += new ScrollChangedEventHandler(sv1_ScrollChanged);
            sv2.ScrollChanged += new ScrollChangedEventHandler(sv2_ScrollChanged);
        }

        

        public void DeInitControl()
        {
            if(sv1 != null)
                sv1.ScrollChanged -= new ScrollChangedEventHandler(sv1_ScrollChanged);

            if(sv2 != null)
                sv2.ScrollChanged -= new ScrollChangedEventHandler(sv2_ScrollChanged);

            sv1 = null;
            sv2 = null;
            _sv1HSB = null;
            _sv2HSB = null;
        }

        private void sv2_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.OriginalSource != sv2) return;

            if (e.VerticalChange != 0)
            {
                sv1.ScrollToVerticalOffset(sv2.VerticalOffset);
            }

            bool sv2NewHV = sv2.ComputedHorizontalScrollBarVisibility == Visibility.Visible && sv2HSB != null && sv2HSB.IsEnabled;
            if (sv2HorizontalVisible != sv2NewHV)
            {
                sv2HorizontalVisible = sv2NewHV;
                MatchHeightDifferences();
            }
        }

        private void sv1_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.OriginalSource != sv1) return;

            if (e.VerticalChange != 0)
            {
                sv2.ScrollToVerticalOffset(sv1.VerticalOffset);
            }

            bool sv1NewHV = sv1.ComputedHorizontalScrollBarVisibility == Visibility.Visible && sv1HSB != null && sv1HSB.IsEnabled;
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
                    sv1.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
                }
                else if (sv1HorizontalVisible && !sv2HorizontalVisible)
                {
                    sv2.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
                }
                else if(!sv1HorizontalVisible && !sv2HorizontalVisible)
                {
                    sv1.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                    sv2.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                }

        }
    }
}
