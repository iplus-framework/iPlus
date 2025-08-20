using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.ext.chart.avui.Charts.Axes
{
	public class HorizontalIntegerAxis : IntegerAxis
	{
		public HorizontalIntegerAxis()
		{
			Placement = AxisPlacement.Bottom;
		}

		protected override void ValidatePlacement(AxisPlacement newPlacement)
		{
			if (newPlacement == AxisPlacement.Left || newPlacement == AxisPlacement.Right)
                throw new ArgumentException(gip.ext.chart.avui.Properties.Resources.HorizontalAxisCannotBeVertical);
		}
	}
}
