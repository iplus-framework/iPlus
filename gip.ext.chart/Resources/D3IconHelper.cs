using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Reflection;

namespace gip.ext.chart
{
	public static class D3IconHelper
	{
		private static BitmapFrame icon = null;
		public static BitmapFrame DynamicDataDisplayIcon
		{
			get
			{
				if (icon == null)
				{
					Assembly currentAssembly = typeof(D3IconHelper).Assembly;
                    icon = BitmapFrame.Create(currentAssembly.GetManifestResourceStream("gip.ext.chart.Resources.D3-icon.ico"));
				}
				return icon;
			}
		}

		private static BitmapFrame whiteIcon = null;
		public static BitmapFrame DynamicDataDisplayWhiteIcon
		{
			get
			{
				if (whiteIcon == null)
				{
					Assembly currentAssembly = typeof(D3IconHelper).Assembly;
                    whiteIcon = BitmapFrame.Create(currentAssembly.GetManifestResourceStream("gip.ext.chart.Resources.D3-icon-white.ico"));
				}

				return whiteIcon;
			}
		}
	}
}
