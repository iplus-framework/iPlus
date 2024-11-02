// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gip.core.datamodel;
using gip.core.layoutengine.Helperclasses;
using System.Windows.Controls;


namespace gip.core.layoutengine
{
    /// <summary>
    /// Provides a adorner layer with connection line between two IACObjects.
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
        /// Represents the dependency property for Relations.
        /// </summary>
        public static readonly DependencyProperty RelationsProperty 
            = DependencyProperty.Register("Relations", typeof(List<ConnectionIACObject>), typeof(VBAdornerDecoratorIACObject), new PropertyMetadata(new PropertyChangedCallback(OnDepPropChanged)));

        /// <summary>
        /// Gets or sets the Relations.
        /// </summary>
        public List<ConnectionIACObject> Relations
        {
            get { return (List<ConnectionIACObject>)GetValue(RelationsProperty); }
            set{ SetValue(RelationsProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ConnectionColor.
        /// </summary>
        public static readonly DependencyProperty ConnectionColorProperty
            = DependencyProperty.Register("ConnectionColor", typeof(SolidColorBrush), typeof(VBAdornerDecoratorIACObject), new PropertyMetadata(Brushes.Red));

        /// <summary>
        /// Gets or sets the ConnectionColor.
        /// </summary>
        public SolidColorBrush ConnectionColor
        {
            get { return (SolidColorBrush)GetValue(ConnectionColorProperty); }
            set { SetValue(ConnectionColorProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ConnctionThickness.
        /// </summary>
        public static readonly DependencyProperty ConnectionThicknessProperty
            = DependencyProperty.Register("ConnectionThickness", typeof(double), typeof(VBAdornerDecoratorIACObject), new PropertyMetadata(1.5));

        /// <summary>
        /// Gets or sets ConnectionThickness.
        /// </summary>
        public double ConnectionThickness
        {
            get { return (double)GetValue(ConnectionThicknessProperty); }
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

        private static void OnDepPropChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            VBAdornerDecoratorIACObject vbAdornerDecorator = dependencyObject as VBAdornerDecoratorIACObject;
            if (vbAdornerDecorator.Child != null)
            {
                vbAdornerDecorator.InsertAdorner();
                vbAdornerDecorator.vbWorkflowAdorner.FillUpdateRelationsVisual(args.NewValue as List<ConnectionIACObject>, vbAdornerDecorator.VBVisualList);
                if (args.NewValue != null && ((List<ConnectionIACObject>)args.NewValue).Any())
                    vbAdornerDecorator.RefreshAdorner();
                else
                    vbAdornerDecorator.RemoveAdorner();
            }
        }

        private void InsertAdorner()
        {
            if (AdornerLayer.GetAdornerLayer(this.Child) == null)
                AdornerLayer.Add(vbWorkflowAdorner);
            this.Child.UpdateLayout();
        }

        /// <summary>
        /// Refreshes the Adorner.
        /// </summary>
        public void RefreshAdorner()
        {
            var adoners = AdornerLayer.GetAdorners(this.Child);
            if (adoners == null)
                AdornerLayer.Add(vbWorkflowAdorner);
            else if (adoners != null && !adoners.Contains(vbWorkflowAdorner))
                AdornerLayer.Add(vbWorkflowAdorner);
            vbWorkflowAdorner.InvalidateVisual();
        }

        /// <summary>
        /// Removes the Adorner.
        /// </summary>
        public void RemoveAdorner()
        {
            if (AdornerLayer.GetAdorners(this.Child) != null)
                AdornerLayer.Remove(vbWorkflowAdorner);
        }

    }
}
