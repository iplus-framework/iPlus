using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace gip.ext.chart.Common.Auxiliary
{
	public sealed class DisposableTimer : IDisposable
	{
		private readonly string name;
		Stopwatch timer;
		public DisposableTimer(string name)
		{
			this.name = name;
			timer = Stopwatch.StartNew();
		}

		#region IDisposable Members

		public void Dispose()
		{
			var duration = timer.ElapsedMilliseconds;
#if DEBUG
            Debug.WriteLine(name + ": elapsed " + duration + " ms.");
#endif
            timer.Stop();
		}

		#endregion
	}
}
