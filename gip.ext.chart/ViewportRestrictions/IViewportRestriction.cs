﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace gip.ext.chart.ViewportRestrictions
{
	public interface IViewportRestriction
	{
		Rect Apply(Rect oldVisible, Rect newVisible, Viewport2D viewport);
		event EventHandler Changed;
	}
}
