using Avalonia;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.layoutengine.avui.timeline
{
	public static class WpfUtility
    {

        public static async Task WaitForPriorityAsync(DispatcherPriority priority)
        {
            await Dispatcher.UIThread.InvokeAsync(() => { }, priority);
        }

        public static void WaitForPriority(DispatcherPriority priority)
        {
            Dispatcher.UIThread.Invoke(() => { }, priority);
        }

  //      public static void WaitForPriority(DispatcherPriority priority)
  //      {
		//	DispatcherFrame frame = new DispatcherFrame();
  //          DispatcherOperation dispatcherOperation = Dispatcher.UIThread.Invoke(priority, new DispatcherOperationCallback(ExitFrameOperation), frame);
  //          Dispatcher.UIThread.PushFrame(frame);
		//	if (dispatcherOperation.Status != DispatcherOperationStatus.Completed)
  //          {
		//		dispatcherOperation.Abort();
		//	}
		//}

		//private static object ExitFrameOperation(object obj)
  //      {
		//	((DispatcherFrame)obj).Continue = false;
		//	return null;
		//}
	}
}
