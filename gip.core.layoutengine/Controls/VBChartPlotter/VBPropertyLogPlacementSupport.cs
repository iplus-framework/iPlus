// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Windows;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Media;

using gip.ext.design;
using gip.ext.design.Adorners;
using gip.ext.design.Extensions;
using gip.ext.designer.Controls;
using gip.ext.designer.Extensions;

namespace gip.core.layoutengine
{
	/// <summary>
	/// Provides <see cref="IPlacementBehavior"/> behavior for <see cref="VBPropertyLogChart"/>.
	/// </summary>
	[ExtensionFor(typeof(VBPropertyLogChart), OverrideExtension=typeof(DefaultPlacementBehavior))]
    public sealed class VBPropertyLogChartPlacementSupport : SnaplinePlacementBehavior, IPlacementChildGenerator
	{
		VBPropertyLogChart VBPropertyLogChart;
		
        /// <summary>
        /// Handles a on initialized event.
        /// </summary>
		protected override void OnInitialized()
		{
			base.OnInitialized();
			VBPropertyLogChart = (VBPropertyLogChart)this.ExtendedItem.Component;
		}
		
        /// <summary>
        /// Enters a contrainer.
        /// </summary>
        /// <param name="operation">The placement operation.</param>
		public override void EnterContainer(PlacementOperation operation)
		{
			base.EnterContainer(operation);
		}

        /// <summary>
        /// Ends a placement operation.
        /// </summary>
        /// <param name="operation">The placement operation.</param>
        public override void EndPlacement(PlacementOperation operation)
		{
            VBPropertyLogChart.UpdateLayout();
            base.EndPlacement(operation);
		}
		
        /// <summary>
        /// Sets the position.
        /// </summary>
        /// <param name="info"></param>
		public override void SetPosition(PlacementInformation info)
		{
			base.SetPosition(info);
		}

        /// <summary>
        /// Leaves the container.
        /// </summary>
        /// <param name="operation">The placement operation.</param>
        public override void LeaveContainer(PlacementOperation operation)
		{
		}

        /// <summary>
        /// Creates a default instance of new child.
        /// </summary>
        /// <returns>Returns a new VBPropertyLogChartItem.</returns>
        public object CreateDefaultNewChildInstance()
        {
            return new VBPropertyLogChartItem();
        }
    }
}
