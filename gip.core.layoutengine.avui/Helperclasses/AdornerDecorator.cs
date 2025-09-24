using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Object which allows elements beneath it in the visual tree to be adorned.
    /// AdornerDecorator has two children.
    /// The first child is the parent of the rest of the visual tree below the AdornerDecorator.
    /// The second child is the AdornerLayer on which adorners are rendered.
    /// 
    /// AdornerDecorator is intended to be used as part of an object's Style.
    /// </summary>
    public class AdornerDecorator : Decorator
    {
        //------------------------------------------------------
        //
        //  Constructors
        //
        //------------------------------------------------------

        #region Constructors


        public AdornerDecorator() : base()
        {
            _adornerLayer = new AdornerLayer();
            AdornerLayer.SetAdornedElement(_adornerLayer, this);
        }

        #endregion Constructors


        #region Public Properties

        public AdornerLayer AdornerLayer
        {
            get
            {
                return _adornerLayer;
            }
        }

        #endregion Public Properties

        #region Protected Methods

        protected override Size MeasureOverride(Size constraint)
        {
            Size desiredSize = base.MeasureOverride(constraint);
            if (_adornerLayer.GetVisualParent() != null)
            {
                // We don't really care about the size of the AdornerLayer-- we'll
                // always just make the AdornerDecorator the full desiredSize.  But
                // we need to measure it anyway, to make sure Adorners render.
                _adornerLayer.Measure(constraint);
            }
            return desiredSize;
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            Size inkSize = base.ArrangeOverride(finalSize);

            if (_adornerLayer.GetVisualParent() != null)
            {
                _adornerLayer.Arrange(new Rect(finalSize));
            }

            return (inkSize);
        }


        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == ChildProperty)
            {
                if (change.NewValue == null)
                {
                    if (VisualChildren != null && VisualChildren.Contains(_adornerLayer))
                        VisualChildren.Remove(_adornerLayer);
                }
                else
                {
                    if (VisualChildren != null && !VisualChildren.Contains(_adornerLayer))
                        VisualChildren.Add(_adornerLayer);
                }
            }
            base.OnPropertyChanged(change);
        }

        #endregion Protected Methods


        #region Private Members

        private readonly AdornerLayer _adornerLayer;

        #endregion Private Members
    }
}
