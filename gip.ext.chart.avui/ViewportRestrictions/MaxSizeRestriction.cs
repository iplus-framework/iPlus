using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace gip.ext.chart.avui.ViewportRestrictions
{
	public class MaxSizeRestriction : ViewportRestrictionBase
	{
		private const double maxSize = 1000;
		public override Rect Apply(Rect oldDataRect, Rect newDataRect, Viewport2D viewport)
		{
			if (newDataRect.Width > maxSize || newDataRect.Height > maxSize)
				return oldDataRect;

			return newDataRect;
		}
	}
}
