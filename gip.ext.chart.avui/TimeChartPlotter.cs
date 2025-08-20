using System;
using gip.ext.chart.avui;
using System.Windows;
using gip.ext.chart.avui.Navigation;
using gip.ext.chart.avui.Common;
using System.Windows.Controls;
using gip.ext.chart.avui.Charts;


namespace gip.ext.chart.avui
{
	public class TimeChartPlotter : ChartPlotter
	{
		public TimeChartPlotter()
		{
			HorizontalAxis = new HorizontalDateTimeAxis();
		}

		public void SetHorizontalAxisMapping(Func<double, DateTime> fromDouble, Func<DateTime, double> toDouble)
		{
			if (fromDouble == null)
				throw new ArgumentNullException("fromDouble");
			if (toDouble == null)
				throw new ArgumentNullException("toDouble");
	

			HorizontalDateTimeAxis axis = (HorizontalDateTimeAxis)HorizontalAxis;
			axis.ConvertFromDouble = fromDouble;
			axis.ConvertToDouble = toDouble;
		}

		public void SetHorizontalAxisMapping(double min, DateTime minDate, double max, DateTime maxDate) {
			HorizontalDateTimeAxis axis = (HorizontalDateTimeAxis)HorizontalAxis;
			
			axis.SetConversion(min, minDate, max, maxDate);
		}
	}
}
