using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using gip.ext.design.avui;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gip.core.layoutengine.avui
{
    public class VBWorkflowAdorner : Adorner
    {
        public VBWorkflowAdorner(Control Control) : base(Control)
        {
        }

        public override void Render(DrawingContext context)
        {
            if (RelationsVisual == null || !RelationsVisual.Any())
                return;
            VBAdornerDecoratorIACObject VBAdornerDecorator = VBVisualTreeHelper.FindParentObjectInVisualTree(this, typeof(VBAdornerDecoratorIACObject)) as VBAdornerDecoratorIACObject;
            foreach (Tuple<VBVisualAdorner, VBVisualAdorner> visuals in RelationsVisual)
            {
                if (visuals.Item1.IsDescendantOf(VBAdornerDecorator) && visuals.Item2.IsDescendantOf(VBAdornerDecorator))
                {
                    Point from = visuals.Item1.TransformToVisual(VBAdornerDecorator)?.Transform(new Point(visuals.Item1.Bounds.Width - 1, visuals.Item1.Bounds.Height / 2)) ?? new Point();
                    Point to = visuals.Item2.TransformToVisual(VBAdornerDecorator)?.Transform(new Point(0, visuals.Item2.Bounds.Height / 2)) ?? new Point();
                    context.DrawLine(new Pen(VBAdornerDecorator.ConnectionColor, VBAdornerDecorator.ConnectionThickness), from, to);
                }
            }
            base.Render(context);
        }

        private List<Tuple<VBVisualAdorner, VBVisualAdorner>> _RelationsVisual;
        public List<Tuple<VBVisualAdorner, VBVisualAdorner>> RelationsVisual
        {
            get
            {
                if (_RelationsVisual == null)
                    _RelationsVisual = new List<Tuple<VBVisualAdorner, VBVisualAdorner>>();
                return _RelationsVisual;
            }
        }

        public void ClearAdoners()
        {
            foreach (var item in RelationsVisual)
            {
                AdornerLayer al1 = AdornerLayer.GetAdornerLayer(item.Item1.AdornedElement);
                if (al1 != null)
                    al1.Children.Remove(item.Item1);

                AdornerLayer al2 = AdornerLayer.GetAdornerLayer(item.Item2.AdornedElement);
                if (al2 != null)
                    al2.Children.Remove(item.Item2);
            }
        }

        public void FillUpdateRelationsVisual(List<ConnectionIACObject> connectionIACObjectList, List<VBVisual> vbVisualList)
        {
            ClearAdoners();
            RelationsVisual.Clear();
            if (connectionIACObjectList == null || !vbVisualList.Any())
                return;

            foreach (ConnectionIACObject connectionIACObject in connectionIACObjectList)
            {
                string nodeFromName = ((IACWorkflowNode)connectionIACObject.Item1).XName;
                string nodeToName = ((IACWorkflowNode)connectionIACObject.Item2).XName;

                VBVisual visualFrom = vbVisualList.Where(c => c.ACCompInitState == ACInitState.Initialized).FirstOrDefault(c => c.Name == nodeFromName);
                VBVisual visualTo = vbVisualList.Where(c => c.ACCompInitState == ACInitState.Initialized).FirstOrDefault(c => c.Name == nodeToName);

                if (visualFrom == null || visualTo == null)
                    return;
                visualFrom.ApplyTemplate();
                visualTo.ApplyTemplate();

                AdornerLayer adornerLayerFrom = AdornerLayer.GetAdornerLayer(visualFrom);
                VBVisualAdorner adornerFrom = new VBVisualAdorner(visualFrom);
                AdornerLayer.SetAdornedElement(adornerFrom, visualFrom);

                AdornerLayer adornerLayerTo = AdornerLayer.GetAdornerLayer(visualTo);
                VBVisualAdorner adornerTo = new VBVisualAdorner(visualTo);
                AdornerLayer.SetAdornedElement(adornerTo, visualTo);

                RelationsVisual.Add(new Tuple<VBVisualAdorner, VBVisualAdorner>(adornerFrom, adornerTo));
            }
        }
    }

    public class VBVisualAdorner : Adorner
    {
        public VBVisualAdorner(Control Control) : base(Control)
        {
        }

        private VBAdornerDecoratorIACObject _VBAdornerDecorator;
        public VBAdornerDecoratorIACObject VBAdornerDecorator
        {
            get 
            { 
                if(_VBAdornerDecorator == null)
                    _VBAdornerDecorator = VBVisualTreeHelper.FindParentObjectInVisualTree(this.Parent, typeof(VBAdornerDecoratorIACObject)) as VBAdornerDecoratorIACObject;
                return _VBAdornerDecorator;
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            VBAdornerDecorator.RefreshAdorner();
            return base.ArrangeOverride(finalSize);
        }
    }
}
