using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.ext.chart.Filters;
using System.Windows;

namespace gip.ext.chart.Charts.Filters
{
	public abstract class PointsFilterBase : IPointsFilter
	{
		#region IPointsFilter Members

		public abstract List<Point> Filter(List<Point> points);

		public virtual void SetScreenRect(Rect screenRect) { }

		protected void RaiseChanged()
		{
			Changed.Raise(this);
		}
		public event EventHandler Changed;

		#endregion
	}
}
