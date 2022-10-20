using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.ext.chart.Common
{
	internal interface INotifyingPanel
	{
		NotifyingUIElementCollection NotifyingChildren { get; }
		event EventHandler ChildrenCreated;
	}
}
