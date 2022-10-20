using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.ext.chart.Charts.Axes
{
	public class VerticalIntegerAxis : IntegerAxis
	{
		public VerticalIntegerAxis()
		{
			Placement = AxisPlacement.Left;
		}

		protected override void ValidatePlacement(AxisPlacement newPlacement)
		{
			if (newPlacement == AxisPlacement.Bottom || newPlacement == AxisPlacement.Top)
                throw new ArgumentException(gip.ext.chart.Properties.Resources.VerticalAxisCannotBeHorizontal);
		}
	}
}
