using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.ext.chart.avui.Charts;

namespace gip.ext.chart.avui.Common.Auxiliary
{
	internal static class PlacementExtensions
	{
		public static bool IsHorizontal(this AxisPlacement placement)
		{
			return placement == AxisPlacement.Bottom || placement == AxisPlacement.Top;
		}
	}
}
