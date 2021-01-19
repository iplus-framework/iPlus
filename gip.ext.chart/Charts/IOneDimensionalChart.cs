using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.ext.chart.DataSources;

namespace gip.ext.chart
{
	public interface IOneDimensionalChart
	{
		IPointDataSource DataSource { get; set; }
		event EventHandler DataChanged;
	}
}
