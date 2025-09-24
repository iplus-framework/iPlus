using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.VisualTree;
using System;
using System.Linq;

namespace gip.core.layoutengine.avui
{
    public abstract class Adorner : TemplatedControl
    {
        protected Adorner(Control adornedElement)
        {
            ArgumentNullException.ThrowIfNull(adornedElement);

            _adornedElement = adornedElement;
            _isClipEnabled = false;
        }
        #region Protected Methods

        /// <summary>
        /// Measure adorner. Default behavior is to size to match the adorned element.
        /// </summary>
        protected override Size MeasureOverride(Size availableSize)
        {
            Size desiredSize;

            // Use Bounds instead of RenderSize in Avalonia
            desiredSize = new Size(AdornedElement.Bounds.Width, AdornedElement.Bounds.Height);

            // In Avalonia, we iterate through visual children differently
            foreach (Control child in this.GetVisualChildren().OfType<Control>())
            {
                child?.Measure(desiredSize);
            }

            return desiredSize;
        }

        /// <summary>
        /// Override of GetLayoutClip. In Avalonia, this is handled differently.
        /// </summary>
        /// <returns>null</returns>
        /// <remarks>
        /// Clip gets generated before transforms are applied, which means that
        /// Adorners can get inappropriately clipped if they draw outside of the bounding rect
        /// of the element they're adorning. This is against the Adorner philosophy of being
        /// topmost, so we choose to ignore clip instead.</remarks>
        protected virtual Geometry GetLayoutClip(Size layoutSlotSize)
        {
            return null;
        }

        #endregion Protected Methods

        #region Public Methods

        /// <summary>
        /// Adorners don't always want to be transformed in the same way as the elements they
        /// adorn. Adorners which adorn points, such as resize handles, want to be translated
        /// and rotated but not scaled. Adorners adorning an object, like a marquee, may want
        /// all transforms. This method is called by AdornerLayer to allow the adorner to
        /// filter out the transforms it doesn't want and return a new transform with just the
        /// transforms it wants applied. An adorner can also add an additional translation
        /// transform at this time, allowing it to be positioned somewhere other than the upper
        /// left corner of its adorned element.
        /// </summary>
        /// <param name="transform">The transform applied to the object the adorner adorns</param>
        /// <returns>Transform to apply to the adorner</returns>
        public virtual ITransform GetDesiredTransform(ITransform transform)
        {
            return transform;
        }

        #endregion Public Methods

        #region Public Properties

        /// <summary>
        /// Gets or sets the clip of this Visual.
        /// Needed by AdornerLayer
        /// </summary>
        internal Geometry AdornerClip
        {
            get
            {
                return Clip;
            }
            set
            {
                Clip = value;
            }
        }

        /// <summary>
        /// Gets or sets the transform of this Visual.
        /// Needed by AdornerLayer
        /// </summary>
        internal ITransform AdornerTransform
        {
            get
            {
                return RenderTransform;
            }
            set
            {
                RenderTransform = value;
            }
        }

        /// <summary>
        /// Control this Adorner adorns.
        /// </summary>
        public Control AdornedElement
        {
            get { return _adornedElement; }
        }

        /// <summary>
        /// If set to true, the adorner will be clipped using the same clip geometry as the
        /// AdornedElement. This is expensive, and therefore should not normally be used.
        /// Defaults to false.
        /// </summary>
        public bool IsClipEnabled
        {
            get
            {
                return _isClipEnabled;
            }

            set
            {
                _isClipEnabled = value;
                InvalidateArrange();
                // In Avalonia, we need to get the AdornerLayer differently
                // This is a simplified approach - in a full implementation you might need
                // to traverse the visual tree to find the AdornerLayer
                var adornerLayer = this.FindAncestorOfType<gip.ext.designer.avui.Controls.AdornerLayer>();
                adornerLayer?.InvalidateArrange();
            }
        }

        #endregion Public Properties

        //------------------------------------------------------
        //
        //  Private Methods
        //
        //------------------------------------------------------

        #region Private Methods

        private void CreateFlowDirectionBinding()
        {
            // In Avalonia, we create bindings differently
            var binding = new Binding
            {
                Source = AdornedElement,
                Path = nameof(Control.FlowDirection),
                Mode = BindingMode.OneWay
            };
            this.Bind(Control.FlowDirectionProperty, binding);
        }

        internal virtual bool NeedsUpdate(Size oldSize)
        {
            // In Avalonia, we compare sizes differently
            return !oldSize.Equals(AdornedElement.Bounds.Size);
        }

        #endregion Private Methods

        #region Private Fields

        private readonly Control _adornedElement;
        private bool _isClipEnabled;

        #endregion Private Fields
    }
}
