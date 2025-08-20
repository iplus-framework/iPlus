using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.ext.chart.avui.Common.UndoSystem
{
	public abstract class UndoableAction
	{
		public abstract void Do();
		public abstract void Undo();
	}
}
