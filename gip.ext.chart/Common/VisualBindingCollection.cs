using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Diagnostics;

namespace gip.ext.chart.Common
{
	[DebuggerDisplay("Count = {Cache.Count}")]
	public sealed class VisualBindingCollection
	{
		private Dictionary<IPlotterElement, UIElement> cache = new Dictionary<IPlotterElement, UIElement>();

		internal Dictionary<IPlotterElement, UIElement> Cache
		{
			get { return cache; }
		}

		public UIElement this[IPlotterElement element]
		{
			get { return cache[element]; }
		}
	}
}
