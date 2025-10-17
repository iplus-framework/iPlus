using Avalonia;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.core.layoutengine.avui.timeline
{
	public static class WpfUtility
    {

		public static void WaitForPriority(DispatcherPriority priority)
        {
			DispatcherFrame frame = new DispatcherFrame();
			DispatcherOperation dispatcherOperation = Dispatcher.CurrentDispatcher.BeginInvoke(priority, new DispatcherOperationCallback(ExitFrameOperation), frame);
			Dispatcher.PushFrame(frame);
			if (dispatcherOperation.Status != DispatcherOperationStatus.Completed)
            {
				dispatcherOperation.Abort();
			}
		}

		private static object ExitFrameOperation(object obj)
        {
			((DispatcherFrame)obj).Continue = false;
			return null;
		}
	}
}
