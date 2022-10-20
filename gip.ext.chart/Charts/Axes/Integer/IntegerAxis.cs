using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.ext.chart.Charts.Axes
{
	public class IntegerAxis : AxisBase<int>
	{
		public IntegerAxis()
			: base(new IntegerAxisControl(),
				d => (int)d,
				i => (double)i)
		{

		}
	}
}
