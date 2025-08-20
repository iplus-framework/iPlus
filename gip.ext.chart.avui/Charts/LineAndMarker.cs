using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.ext.chart.avui
{
	public sealed class LineAndMarker<T>
	{
		public LineGraph LineGraph { get; set; }
		public T MarkerGraph { get; set; }
	}
}
