﻿using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace gip.core.layoutengine
{
    /// <summary>
    /// The adorner manager for VBControls
    /// </summary>
    public class AdornerVBControlManager
    {
        #region ctor's

        /// <summary>
        /// Creates a new instance of AdornerVBControlManager.
        /// </summary>
        /// <param name="vbControlToAdorn">The VB control which should be adorned.</param>
        /// <param name="color">The color of adorner.</param>
        public AdornerVBControlManager(IVBContent vbControlToAdorn, Color color)
        {
            ControlToAdorn = vbControlToAdorn;
            AddAdornerToElement(color);
        }
        #endregion

        #region properties

        /// <summary>
        /// Gets or sets the control to adorn.
        /// </summary>
        public IVBContent ControlToAdorn { get; set; }

        /// <summary>
        /// Gets or sets the adorner layer of design.
        /// </summary>
        public AdornerLayer AdornerLayerOfDesign { get; set; }

        /// <summary>
        /// Gets or sets the adorner of control.
        /// </summary>
        public Adorner AdornerOfControl { get; set; }

        /// <summary>
        /// Gets the last used color.
        /// </summary>
        public Color LastUsedColor
        {
            get
            {
                return lastUsedColor;
            }
        }

        #endregion

        #region methods

        private void RemoveAdornerFromChildElements(AdornerLayer adornerLayer, UIElement vbUiControlToAdorn)
        {
            var childs2 = adornerLayer.GetAdorners(vbUiControlToAdorn);
            if (childs2 != null)
            {
                foreach (var childAdorner in childs2)
                {
                    if (childAdorner is VBDesignSelectionAdorner)
                        AdornerLayerOfDesign.Remove(childAdorner);
                }
            }
        }

        /// <summary>
        /// Adds adorner to element.
        /// </summary>
        /// <param name="color">The adorner color.</param>
        public void AddAdornerToElement(Color color)
        {
            lastUsedColor = color;
            UIElement vbUiControlToAdorn = ControlToAdorn as UIElement;

            if (vbUiControlToAdorn == null)
                return;

            AdornerLayerOfDesign = AdornerLayer.GetAdornerLayer(vbUiControlToAdorn);

            if (AdornerLayerOfDesign == null)
                return;

            RemoveAdornerFromChildElements(AdornerLayerOfDesign, vbUiControlToAdorn);

            AdornerOfControl = new VBDesignSelectionAdorner(vbUiControlToAdorn, color);
            AdornerLayerOfDesign.Add(AdornerOfControl);
        }

        /// <summary>
        /// Removes a adorner from the element.
        /// </summary>
        public void RemoveAdornerFromElement()
        {
            if (AdornerOfControl != null)
            {
                var layerOfLast = AdornerLayer.GetAdornerLayer(AdornerOfControl.AdornedElement);
                if (layerOfLast != null)
                {
                    var childs = layerOfLast.GetAdorners(AdornerOfControl.AdornedElement);
                    if (childs != null)
                    {
                        foreach (var childAdorner in childs)
                        {
                            if(childAdorner is VBDesignSelectionAdorner)
                                layerOfLast.Remove(childAdorner);
                        }
                    }
                    if (AdornerOfControl is VBDesignSelectionAdorner)
                        layerOfLast.Remove(AdornerOfControl);
                }
                if (AdornerOfControl is VBDesignSelectionAdorner)
                    AdornerLayerOfDesign.Remove(AdornerOfControl);
            }
            AdornerOfControl = null;
            AdornerLayerOfDesign = null;
        }

        #endregion

        #region private members
        private Color lastUsedColor;
        #endregion
    }
}
