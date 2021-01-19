using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.ext.chart.Charts
{
	public class HorizontalTimeSpanAxis : TimeSpanAxis
	{
		public HorizontalTimeSpanAxis()
		{
			Placement = AxisPlacement.Bottom;
		}

		protected override void ValidatePlacement(AxisPlacement newPlacement)
		{
			if (newPlacement == AxisPlacement.Left || newPlacement == AxisPlacement.Right)
                throw new ArgumentException(gip.ext.chart.Properties.Resources.HorizontalAxisCannotBeVertical);
		}
	}
}
