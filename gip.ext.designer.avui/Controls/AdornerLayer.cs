// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

//#define DEBUG_ADORNERLAYER

using System;
using System.Collections.Generic;
using System.Diagnostics;
using gip.ext.design.avui.Adorners;
using gip.ext.design.avui;
using Avalonia.Controls;
using Avalonia;
using Avalonia.VisualTree;
using Avalonia.Media;
using System.Linq;

namespace gip.ext.designer.avui.Controls
{
    /// <summary>
    /// A control that displays adorner panels.
    /// </summary>
    public sealed class AdornerLayer : Panel, IAdornerLayer
    {
        #region AdornerPanelCollection
        internal sealed class AdornerPanelCollection : ICollection<AdornerPanel>, IReadOnlyCollection<AdornerPanel>
        {
            readonly AdornerLayer _layer;

            public AdornerPanelCollection(AdornerLayer layer)
            {
                this._layer = layer;
            }

            public int Count
            {
                get { return _layer.Children.Count; }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public void Add(AdornerPanel item)
            {
                if (item == null)
                    throw new ArgumentNullException("item");

                _layer.AddAdorner(item);
            }

            public void Clear()
            {
                _layer.ClearAdorners();
            }

            public bool Contains(AdornerPanel item)
            {
                if (item == null)
                    throw new ArgumentNullException("item");

                return VisualTreeHelper.GetParent(item) == _layer;
            }

            public void CopyTo(AdornerPanel[] array, int arrayIndex)
            {
                foreach (AdornerPanel panel in this)
                    array[arrayIndex++] = panel;
            }

            public bool Remove(AdornerPanel item)
            {
                if (item == null)
                    throw new ArgumentNullException("item");

                return _layer.RemoveAdorner(item);
            }

            public IEnumerator<AdornerPanel> GetEnumerator()
            {
                foreach (AdornerPanel panel in _layer.Children)
                {
                    yield return panel;
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }
        #endregion

        AdornerPanelCollection _adorners;
        readonly Control _designPanel;

#if DEBUG_ADORNERLAYER
		int _totalAdornerCount;
#endif

        internal AdornerLayer(Control designPanel)
        {
            this._designPanel = designPanel;

            this.LayoutUpdated += OnLayoutUpdated;

            _adorners = new AdornerPanelCollection(this);
        }

        void OnLayoutUpdated(object sender, EventArgs e)
        {
            UpdateAllAdorners(false);
#if DEBUG_ADORNERLAYER
			Debug.WriteLine("Adorner LayoutUpdated. AdornedElements=" + _dict.Count +
			                ", visible adorners=" + VisualChildrenCount + ", total adorners=" + (_totalAdornerCount));
#endif
        }

        protected override void OnSizeChanged(SizeChangedEventArgs sizeInfo)
        {
            base.OnSizeChanged(sizeInfo);
            UpdateAllAdorners(true);
        }

        internal AdornerPanelCollection Adorners
        {
            get
            {
                return _adorners;
            }
        }

        sealed class AdornerInfo
        {
            internal readonly List<AdornerPanel> adorners = new List<AdornerPanel>();
            internal bool isVisible;
            internal Rect position;
        }

        // adorned element => AdornerInfo
        Dictionary<Control, AdornerInfo> _dict = new Dictionary<Control, AdornerInfo>();

        void ClearAdorners()
        {
            if (_dict.Count == 0)
                return; // already empty

            this.Children.Clear();
            _dict = new Dictionary<Control, AdornerInfo>();

#if DEBUG_ADORNERLAYER
			_totalAdornerCount = 0;
			Debug.WriteLine("AdornerLayer cleared.");
#endif
        }

        AdornerInfo GetOrCreateAdornerInfo(Control adornedElement)
        {
            AdornerInfo info;
            if (!_dict.TryGetValue(adornedElement, out info))
            {
                info = _dict[adornedElement] = new AdornerInfo();
                info.isVisible = adornedElement.IsDescendantOf(_designPanel);
            }
            return info;
        }

        AdornerInfo GetExistingAdornerInfo(Control adornedElement)
        {
            AdornerInfo info;
            _dict.TryGetValue(adornedElement, out info);
            return info;
        }

        void AddAdorner(AdornerPanel adornerPanel)
        {
            if (adornerPanel.AdornedElement == null)
                throw new DesignerException("adornerPanel.AdornedElement must be set");

            AdornerInfo info = GetOrCreateAdornerInfo(adornerPanel.AdornedElement);
            info.adorners.Add(adornerPanel);

            if (info.isVisible)
            {
                AddAdornerToChildren(adornerPanel);
            }

#if DEBUG_ADORNERLAYER
			Debug.WriteLine("Adorner added. AdornedElements=" + _dict.Count +
			                ", visible adorners=" + VisualChildrenCount + ", total adorners=" + (++_totalAdornerCount));
#endif
        }

        void AddAdornerToChildren(AdornerPanel adornerPanel)
        {
            Avalonia.Controls.Controls children = this.Children;
            int i = 0;
            for (i = 0; i < children.Count; i++)
            {
                AdornerPanel p = (AdornerPanel)children[i];
                if (p.Order > adornerPanel.Order)
                {
                    break;
                }
            }
            children.Insert(i, adornerPanel);
        }


        protected override Size MeasureOverride(Size availableSize)
        {
            Size infiniteSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
            foreach (AdornerPanel adorner in this.Children)
            {
                adorner.Measure(infiniteSize);
            }
            return new Size(0, 0);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (AdornerPanel adorner in this.Children)
            {
                if (adorner.AdornedElement.IsDescendantOf(_designPanel))
                {
                    var transform = adorner.AdornedElement.TransformToAncestor(_designPanel);
                    var rt = transform as MatrixTransform;
                    if (rt != null && adorner.AdornedDesignItem != null && adorner.AdornedDesignItem.Parent != null && adorner.AdornedDesignItem.Parent.View is Canvas && adorner.AdornedElement.Bounds.Height == 0 && adorner.AdornedElement.Bounds.Width == 0)
                    {
                        var width = ((Control)adorner.AdornedElement).Width;
                        width = width > 0 ? width : 2.0;
                        var height = ((Control)adorner.AdornedElement).Height;
                        height = height > 0 ? height : 2.0;
                        var xOffset = rt.Matrix.M31 - (width / 2);
                        var yOffset = rt.Matrix.M32 - (height / 2);
                        rt = new MatrixTransform(new Matrix(rt.Matrix.M11, rt.Matrix.M12, rt.Matrix.M21, rt.Matrix.M22, xOffset, yOffset));
                    }
                    else if (transform is TransformGroup)
                    {
                        //var intTrans = ((GeneralTransformGroup) transform).Children.FirstOrDefault(x => x.GetType().Name == "GeneralTransform2DTo3DTo2D");
                        //var prp = intTrans.GetType().GetField("_worldTransformation", BindingFlags.Instance | BindingFlags.NonPublic);
                        //var mtx = (Matrix3D) prp.GetValue(intTrans);
                        //var mtx2D = new Matrix(mtx.M11, mtx.M12, mtx.M21, mtx.M22, mtx.OffsetX, mtx.OffsetY);
                        //rt = new MatrixTransform(mtx2D);
                        rt = ((TransformGroup)transform).Children.OfType<MatrixTransform>().LastOrDefault();
                    }


                    adorner.RenderTransform = rt;
                }

                adorner.Arrange(new Rect(new Point(0, 0), adorner.DesiredSize));
            }
            return finalSize;
        }

        bool RemoveAdorner(AdornerPanel adornerPanel)
        {
            if (adornerPanel.AdornedElement == null)
                return false;

            AdornerInfo info = GetExistingAdornerInfo(adornerPanel.AdornedElement);
            if (info == null)
                return false;

            if (info.adorners.Remove(adornerPanel))
            {
                if (info.isVisible)
                {
                    this.Children.Remove(adornerPanel);
                }

                if (info.adorners.Count == 0)
                {
                    _dict.Remove(adornerPanel.AdornedElement);
                }

#if DEBUG_ADORNERLAYER
				Debug.WriteLine("Adorner removed. AdornedElements=" + _dict.Count +
				                ", visible adorners=" + VisualChildrenCount + ", total adorners=" + (--_totalAdornerCount));
#endif

                return true;
            }
            else
            {
                return false;
            }
        }

        public void UpdateAdornersForElement(Control element, bool forceInvalidate)
        {
            AdornerInfo info = GetExistingAdornerInfo(element);
            if (info != null)
            {
                UpdateAdornersForElement(element, info, forceInvalidate);
            }
        }

        Rect GetPositionCache(Control element)
        {
            Matrix? transform = element.TransformToVisual(_designPanel);
            if (transform != null)
            {
                var p = transform.Value.Transform(new Point(0, 0));
                return new Rect(p, element.Bounds.Size);
            }
            return new Rect(0, 0, element.Bounds.Width, element.Bounds.Height);
        }

        void UpdateAdornersForElement(Control element, AdornerInfo info, bool forceInvalidate)
        {
            if (element.IsDescendantOf(_designPanel))
            {
                if (!info.isVisible)
                {
                    info.isVisible = true;
                    // make adorners visible:
                    info.adorners.ForEach(AddAdornerToChildren);
                }
                Rect c = GetPositionCache(element);
                if (forceInvalidate || !info.position.Equals(c))
                {
                    info.position = c;
                    foreach (AdornerPanel p in info.adorners)
                    {
                        p.InvalidateMeasure();
                    }
                    this.InvalidateArrange();
                }
            }
            else
            {
                if (info.isVisible)
                {
                    info.isVisible = false;
                    // make adorners invisible:
                    foreach (Control ctl in this.Children.ToList())
                    {
                        this.Children.Remove(ctl);
                    }
                }
            }
        }

        void UpdateAllAdorners(bool forceInvalidate)
        {
            foreach (KeyValuePair<Control, AdornerInfo> pair in _dict)
            {
                UpdateAdornersForElement(pair.Key, pair.Value, forceInvalidate);
            }
        }
    }
}
