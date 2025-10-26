// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using gip.ext.design.avui;
using gip.ext.design.avui.Extensions;
using gip.ext.designer.avui.Extensions;

namespace gip.core.layoutengine.avui
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
