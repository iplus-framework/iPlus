using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace gip.ext.chart.avui.ViewportRestrictions
{
	public class MinimalSizeRestriction : ViewportRestrictionBase
	{
		private const double minSize = 1E-11;
		public override Rect Apply(Rect oldDataRect, Rect newDataRect, Viewport2D viewport)
		{
			if (newDataRect.Width < minSize || newDataRect.Height < minSize)
				return oldDataRect;

			return newDataRect;
		}
	}
}
