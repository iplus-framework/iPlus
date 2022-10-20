using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace gip.ext.chart.Common.Auxiliary
{
	public static class DoubleCollectionHelper
	{
		public static DoubleCollection Create(params double[] collection)
		{
			return new DoubleCollection(collection);
		}
	}
}
