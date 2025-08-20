using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using gip.ext.chart.avui.Charts;

namespace gip.ext.chart.avui
{
	internal static class RangeExtensions
	{
		public static double GetLength(this Range<Point> range)
		{
			Point p1 = range.Min;
			Point p2 = range.Max;

			return (p1 - p2).Length;
		}
	}
}
