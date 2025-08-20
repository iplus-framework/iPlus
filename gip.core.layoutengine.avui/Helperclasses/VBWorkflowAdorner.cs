using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;

namespace gip.core.layoutengine.avui
{
    public class VBWorkflowAdorner : Adorner
    {
        public VBWorkflowAdorner(UIElement uiElement) : base(uiElement)
        {
        }

        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            if (RelationsVisual == null || !RelationsVisual.Any())
                return;
            VBAdornerDecoratorIACObject VBAdornerDecorator = VBVisualTreeHelper.FindParentObjectInVisualTree(this, typeof(VBAdornerDecoratorIACObject)) as VBAdornerDecoratorIACObject;
            foreach (Tuple<VBVisualAdorner, VBVisualAdorner> visuals in RelationsVisual)
            {
                if (VBAdornerDecorator.IsAncestorOf(visuals.Item1) && VBAdornerDecorator.IsAncestorOf(visuals.Item2))
                {
                    Point from = visuals.Item1.TransformToVisual(VBAdornerDecorator).Transform(new Point(visuals.Item1.ActualWidth-1, visuals.Item1.ActualHeight / 2));
                    Point to = visuals.Item2.TransformToVisual(VBAdornerDecorator).Transform(new Point(0, visuals.Item2.ActualHeight / 2));
                    drawingContext.DrawLine(new Pen(VBAdornerDecorator.ConnectionColor, VBAdornerDecorator.ConnectionThickness), from, to);
                }
            }
            base.OnRender(drawingContext); 
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
                    al1.Remove(item.Item1);

                AdornerLayer al2 = AdornerLayer.GetAdornerLayer(item.Item2.AdornedElement);
                if(al2 != null)
                    al2.Remove(item.Item2);
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
                adornerLayerFrom.Add(adornerFrom);
                adornerLayerFrom.Update();

                AdornerLayer adornerLayerTo = AdornerLayer.GetAdornerLayer(visualTo);
                VBVisualAdorner adornerTo = new VBVisualAdorner(visualTo);
                adornerLayerTo.Add(adornerTo);

                RelationsVisual.Add(new Tuple<VBVisualAdorner, VBVisualAdorner>(adornerFrom, adornerTo));
            }
        }
    }

    public class VBVisualAdorner : Adorner
    {
        public VBVisualAdorner(UIElement uiElement) : base(uiElement)
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
