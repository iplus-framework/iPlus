#region Copyright and License Information
// This is a modification for iplus-framework of the Fluent Ribbon Control Suite
// https://github.com/fluentribbon/Fluent.Ribbon
// Copyright © Degtyarev Daniel, Rikker Serg. 2009-2010.  All rights reserved.
// 
// This code was originally distributed under the Microsoft Public License (Ms-PL). The modifications by gipSoft d.o.o. are now distributed under GPLv3.
// The license is available online https://github.com/fluentribbon/Fluent.Ribbonlicense
#endregion

using System.Windows.Controls;
using System.Windows.Media;

namespace Fluent
{
    /// <summary>
    /// Represents ScrollViewer with modified hit test
    /// </summary>
    public class RibbonScrollViewer : ScrollViewer
    {
        #region Overrides

        /// <summary>
        /// Performs a hit test to determine whether the specified 
        /// points are within the bounds of this ScrollViewer
        /// </summary>
        /// <returns>The result of the hit test</returns>
        /// <param name="hitTestParameters">The parameters for hit testing within a visual object</param>
        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            if (VisualChildrenCount > 0)
            {
                return VisualTreeHelper.HitTest(GetVisualChild(0), hitTestParameters.HitPoint);
            }
            return base.HitTestCore(hitTestParameters);
        }

        #endregion
    }
}
