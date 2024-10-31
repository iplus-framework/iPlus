// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace gip.ext.widgets
{
	public static class ColorHelper
	{
		public static Color ColorFromString(string s)
		{
			if (string.IsNullOrEmpty(s)) {
				return Colors.White;
			}
			if (s[0] != '#') s = "#" + s;
			try {
				return (Color)ColorConverter.ConvertFromString(s);
			}
			catch (Exception)
            {
                return Colors.White;
			}
		}

		public static string StringFromColor(Color c)
		{
			return c.ToString().Substring(1);
		}

		public static Color ColorFromHsv(double h, double s, double v)
		{
			double r, g, b;
			RgbFromHsv(h, s, v, out r, out g, out b);
            byte br = 0;
            byte bg = 0;
            byte bb = 0;
            if (r > 0)
                br = (byte)(r * 255);
            if (g > 0)
                bg = (byte)(g * 255);
            if (b > 0)
                bb = (byte)(b * 255);
            return Color.FromRgb(br, bg, bb);

		}

		public static void HsvFromColor(Color c, out double h, out double s, out double v)
		{
			HsvFromRgb(c.R / 255.0, c.G / 255.0, c.B / 255.0, out h, out s, out v);
		}

		// http://en.wikipedia.org/wiki/HSV_color_space
		public static void HsvFromRgb(double r, double g, double b, out double h, out double s, out double v)
		{
			var max = Math.Max(r, Math.Max(g, b));
			var min = Math.Min(r, Math.Min(g, b));

			if (max == min) {
				h = 0;
			}
			else if (max == r) {
				h = (60 * (g - b) / (max - min)) % 360;
			}
			else if (max == g) {
				h = 60 * (b - r) / (max - min) + 120;
			}
			else {
				h = 60 * (r - g) / (max - min) + 240;
			}

			if (max == 0) {
				s = 0;
			}
			else {
				s = 1 - min / max;
			}

			v = max;
		}

		// http://en.wikipedia.org/wiki/HSV_color_space
		public static void RgbFromHsv(double h, double s, double v, out double r, out double g, out double b)
		{
			h = h % 360;
			int hi = (int)(h / 60) % 6;
			var f = h / 60 - (int)(h / 60);
			var p = v * (1 - s);
			var q = v * (1 - f * s);
			var t = v * (1 - (1 - f) * s);

			switch (hi) {
				case 0: r = v; g = t; b = p; break;
				case 1: r = q; g = v; b = p; break;
				case 2: r = p; g = v; b = t; break;
				case 3: r = p; g = q; b = v; break;
				case 4: r = t; g = p; b = v; break;
				default: r = v; g = p; b = q; break;
			}
		}
	}
}
