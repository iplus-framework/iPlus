using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace gip.ext.chart.avui.Common.Palettes
{
	public abstract class PaletteBase : IPalette
	{
		#region IPalette Members

		public abstract Color GetColor(double t);

		protected void RaiseChanged()
		{
			Changed.Raise(this);
		}
		public event EventHandler Changed;

		#endregion
	}
}
