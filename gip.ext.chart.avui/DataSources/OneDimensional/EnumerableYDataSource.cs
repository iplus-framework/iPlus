using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.ext.chart.avui.DataSources
{
	internal sealed class EnumerableYDataSource<T> : EnumerableDataSource<T>
	{
		public EnumerableYDataSource(IEnumerable<T> data) : base(data) { }
	}
}
