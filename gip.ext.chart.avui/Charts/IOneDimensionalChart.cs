using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.ext.chart.avui.DataSources;

namespace gip.ext.chart.avui
{
	public interface IOneDimensionalChart
	{
		IPointDataSource DataSource { get; set; }
		event EventHandler DataChanged;
	}
}
