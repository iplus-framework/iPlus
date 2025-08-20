using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.ext.chart.avui.Charts.Axes
{
	public class IntegerAxisControl : AxisControl<int>
	{
		public IntegerAxisControl()
		{
			LabelProvider = new GenericLabelProvider<int>();
			TicksProvider = new IntegerTicksProvider();
			ConvertToDouble = i => (double)i;
			Range = new Range<int>(0, 1);
		}
	}
}
