using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.ext.chart.Common.UndoSystem
{
	public abstract class UndoableAction
	{
		public abstract void Do();
		public abstract void Undo();
	}
}
