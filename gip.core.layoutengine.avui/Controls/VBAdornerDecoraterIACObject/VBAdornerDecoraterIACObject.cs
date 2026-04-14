// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Provides a adorner layer to draw connection line between two IACObjects.
    /// </summary>
    /// <summary xml:lang="de">
    /// Bietet eine Adorner-Schicht mit Verbindungslinie zwischen zwei IACO-Objekten.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBAdornerDecoratorIACObject'}de{'VBAdornerDecoratorIACObject'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBAdornerDecoratorIACObject : AdornerDecorator
    {
        /// <summary>
        /// Creates a new instance of VBAdornerDecoratorIACObject.
        /// </summary>
        public VBAdornerDecoratorIACObject() : base()
        {
           
        }

        /// <summary>
        /// Represents the styled property for Relations.
        /// </summary>
        public static readonly StyledProperty<List<ConnectionIACObject>> RelationsProperty 
            = AvaloniaProperty.Register<VBAdornerDecoratorIACObject, List<ConnectionIACObject>>(nameof(Relations));

        /// <summary>
        /// Gets or sets the Relations.
        /// </summary>
        public List<ConnectionIACObject> Relations
        {
            get { return GetValue(RelationsProperty); }
            set { SetValue(RelationsProperty, value); }
        }

        /// <summary>
        /// Represents the styled property for ConnectionColor.
        /// </summary>
        public static readonly StyledProperty<IBrush> ConnectionColorProperty
            = AvaloniaProperty.Register<VBAdornerDecoratorIACObject, IBrush>(nameof(ConnectionColor), Brushes.Red);

        /// <summary>
        /// Gets or sets the ConnectionColor.
        /// </summary>
        public IBrush ConnectionColor
        {
            get { return GetValue(ConnectionColorProperty); }
            set { SetValue(ConnectionColorProperty, value); }
        }

        /// <summary>
        /// Represents the styled property for ConnectionThickness.
        /// </summary>
        public static readonly StyledProperty<double> ConnectionThicknessProperty
            = AvaloniaProperty.Register<VBAdornerDecoratorIACObject, double>(nameof(ConnectionThickness), 1.5);

        /// <summary>
        /// Gets or sets ConnectionThickness.
        /// </summary>
        public double ConnectionThickness
        {
            get { return GetValue(ConnectionThicknessProperty); }
            set { SetValue(ConnectionThicknessProperty, value); }
        }


        private List<VBVisual> _VBVisualList;
        /// <summary>
        /// Gets the VBVisual list.
        /// </summary>
        public List<VBVisual> VBVisualList
        {
            get 
            {
                if (_VBVisualList == null)
                    _VBVisualList = new List<VBVisual>();
                return _VBVisualList;
            }
        }

        public void RegisterVBVisual(VBVisual vbVisual)
        {
            UnRegisterVBVisual(vbVisual);
            VBVisualList.Add(vbVisual);
            vbWorkflowAdorner.FillUpdateRelationsVisual(Relations, VBVisualList);
        }

        public void UnRegisterVBVisual(VBVisual vbVisual)
        {
            VBVisual vbVisualEx = VBVisualList.FirstOrDefault(c => c.Name == vbVisual.Name);
            if (vbVisualEx != null)
                VBVisualList.Remove(vbVisualEx);
        }

        private VBWorkflowAdorner _vbWorkflowAdorner;
        /// <summary>
        /// Gets the vbWorkflowAdorner.
        /// </summary>
        public VBWorkflowAdorner vbWorkflowAdorner
        {
            get
            {
                if (_vbWorkflowAdorner == null)
                    _vbWorkflowAdorner = new VBWorkflowAdorner(this.Child);
                return _vbWorkflowAdorner;
            }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == RelationsProperty && this.Child != null)
            {
                InsertAdorner();
                vbWorkflowAdorner.FillUpdateRelationsVisual(e.GetNewValue<List<ConnectionIACObject>>(), VBVisualList);
                if (e.GetNewValue<List<ConnectionIACObject>>() != null && e.GetNewValue<List<ConnectionIACObject>>().Any())
                    RefreshAdorner();
                else
                    RemoveAdorner();
            }
        }

        private void InsertAdorner()
        {
            var layer = AdornerLayer;
            if (layer != null)
            {
                var adorners = AdornerLayer.GetAdornerLayer(this.Child);
                if (adorners != null && !adorners.Children.Contains(vbWorkflowAdorner))
                    adorners.Children.Add(vbWorkflowAdorner);
            }
            this.Child.UpdateLayout();
        }

        /// <summary>
        /// Refreshes the Adorner.
        /// </summary>
        public void RefreshAdorner()
        {
            var layer = AdornerLayer;
            if (layer != null)
            {
                var adorners = AdornerLayer.GetAdornerLayer(this.Child);
                if (adorners != null && !adorners.Children.Contains(vbWorkflowAdorner))
                    adorners.Children.Add(vbWorkflowAdorner);
            }
            vbWorkflowAdorner.InvalidateVisual();
        }

        /// <summary>
        /// Removes the Adorner.
        /// </summary>
        public void RemoveAdorner()
        {
            var layer = AdornerLayer;
            if (layer != null)
            {
                var adorners = AdornerLayer.GetAdornerLayer(this.Child);
                if (adorners != null)
                    adorners.Children.Remove(vbWorkflowAdorner);
            }
        }
    }
}
