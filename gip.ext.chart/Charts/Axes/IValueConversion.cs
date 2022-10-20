using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.ext.chart.Charts
{
	public interface IValueConversion<T>
	{
		Func<T, double> ConvertToDouble { get; set; }
		Func<double, T> ConvertFromDouble { get; set; }
	}
}
