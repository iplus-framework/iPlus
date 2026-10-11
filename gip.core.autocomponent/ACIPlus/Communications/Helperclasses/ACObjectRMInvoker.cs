// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using gip.core.datamodel;


namespace gip.core.autocomponent
{
    public class ACObjectRMInvoker
    {
        protected readonly ACMonitorObject _20500_LockWaitHandles = new ACMonitorObject(20500);
        List<ACObjectRMIWaitHandle> _waitHandles = new List<ACObjectRMIWaitHandle>();
        private int _RequestCounter = 0;
        private IACComponent _ACComponent = null;

        /// <summary>
        /// Browser (WASM) hook, registered by the UI framework at startup (e.g. the Avalonia
        /// client). A plain blocking <see cref="EventWaitHandle.WaitOne(int)"/> deadlocks the
        /// single-threaded browser UI: the response can only be delivered when the UI thread
        /// keeps pumping its event loop. The hook runs a nested dispatcher pump until the
        /// handle is set or the timeout (ms) elapses; returns true when signaled.
        /// </summary>
        public static Func<EventWaitHandle, int, bool> PumpingWaitHook = null;

        #region c´tors
        public ACObjectRMInvoker(IACComponent acComponent)
        {
            _ACComponent = acComponent;
        }

        public void Close()
        {
            using (ACMonitor.Lock(_20500_LockWaitHandles))
            {
                foreach (ACObjectRMIWaitHandle waitHandle in _waitHandles)
                {
                    try
                    {
                        waitHandle.Set();
                        waitHandle.Close();
                    }
                    catch (Exception)
                    {
                    }
                }
                _waitHandles = new List<ACObjectRMIWaitHandle>();
            }
        }
        #endregion

        #region Messagehandling
        public object ExecuteMethod(string acMethodName, Object[] acParameter)
        {
            string url = _ACComponent.ACUrl;
            //if (_acObject.ACUrl[_acObject.ACUrl.Length - 1] == '\\')
                //url = _acObject.ACUrl.Substring(0, _acObject.ACUrl.Length - 1);
            url += "!" + acMethodName;

            return InvokeRemote(_ACComponent, url, acParameter);
        }

        public object IsEnabledExecuteMethod(string acMethodName, Object[] acParameter)
        {
            string url = "&" + _ACComponent.ACUrl;
            //if (_acObject.ACUrl[_acObject.ACUrl.Length - 1] == '\\')
            //url = _acObject.ACUrl.Substring(0, _acObject.ACUrl.Length - 1);
            url += "!" + acMethodName;

            return InvokeRemote(_ACComponent, url, acParameter);
        }


        public object ExecuteRemote(string ACProjectIdentifier, string acUrl, Object[] acParameter)
        {
            IACComponent projectService = null;
            projectService = _ACComponent.Root.GetChildComponent(ACProjectIdentifier);
            if (projectService == null)
                return null;

            return InvokeRemote(projectService, acUrl, acParameter);
        }

        public object InvokeACUrlCommand(string acUrl, params Object[] acParameter)
        {
            return InvokeRemote(_ACComponent, acUrl, acParameter);
        }

        /// <summary>
        /// Async variant of <see cref="ExecuteMethod"/>. MUST be used on the browser (WASM):
        /// the single UI thread cannot block on a wait handle, because the response can only
        /// be delivered while the browser event loop is running. Awaiting yields control so
        /// the WebSocket receive loop can set the wait handle.
        /// </summary>
        public Task<object> ExecuteMethodAsync(string acMethodName, Object[] acParameter)
        {
            string url = _ACComponent.ACUrl + "!" + acMethodName;
            return InvokeRemoteAsync(_ACComponent, url, acParameter);
        }

        /// <summary>
        /// Async variant of <see cref="IsEnabledExecuteMethod"/>. MUST be used on the
        /// browser (WASM) — see <see cref="ExecuteMethodAsync"/>.
        /// </summary>
        public Task<object> IsEnabledExecuteMethodAsync(string acMethodName, Object[] acParameter)
        {
            string url = "&" + _ACComponent.ACUrl + "!" + acMethodName;
            return InvokeRemoteAsync(_ACComponent, url, acParameter);
        }

        private async Task<object> InvokeRemoteAsync(IACComponent atACComponent, string acUrl, params Object[] acParameter)
        {
            if (atACComponent == null)
                return null;
            if ((atACComponent as ACComponent).ConnectionState == ACObjectConnectionState.DisConnected)
                return null;

            ACObjectRMIWaitHandle newWaitHandle = null;

            using (ACMonitor.Lock(_20500_LockWaitHandles))
            {
                _RequestCounter++;
                newWaitHandle = new ACObjectRMIWaitHandle(false, EventResetMode.AutoReset, _RequestCounter);
                _waitHandles.Add(newWaitHandle);
            }

            try
            {
                _ACComponent.Root.SendACMessage(WCFMessage.NewACMessage(_ACComponent.ACUrl, acUrl, newWaitHandle.RequestID, acParameter), atACComponent);
                // 5000 ms Responsezeit — polled asynchronously so the browser event loop
                // keeps running while we wait (a blocking WaitOne would deadlock WASM).
                DateTime deadline = DateTime.UtcNow.AddMilliseconds(5000);
                while (!newWaitHandle.WaitOne(0) && DateTime.UtcNow < deadline)
                    await Task.Delay(50);
                if (!newWaitHandle.WaitOne(0))
                {
                    if (atACComponent != null)
                        _ACComponent.Messages.LogFailure(_ACComponent.ACUrl, "ACObjectRMInvoker.InvokeRemoteAsync(10)", String.Format("Request {0} [{1}] timed out at component {2}", acUrl, newWaitHandle.RequestID, atACComponent.GetACUrl()));
                    else
                        _ACComponent.Messages.LogFailure(_ACComponent.ACUrl, "ACObjectRMInvoker.InvokeRemoteAsync(10)", String.Format("Request {0} [{1}] timed out", acUrl, newWaitHandle.RequestID));
                    newWaitHandle.TimedOut = true;
                }
            }
            catch (ACWCFException e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ACObjectRMInvoker", "InvokeRemoteAsync", msg);
                return null;
            }
            finally
            {
                using (ACMonitor.Lock(_20500_LockWaitHandles))
                {
                    newWaitHandle.Close();
                    _waitHandles.Remove(newWaitHandle);
                }
            }

            return newWaitHandle.RemoteMethodInvocationResult;
        }

        private object InvokeRemote(IACComponent atACComponent, string acUrl, params Object[] acParameter)
        {
            if (atACComponent == null)
                return null;
            if ((atACComponent as ACComponent).ConnectionState == ACObjectConnectionState.DisConnected)
                return null;

            ACObjectRMIWaitHandle newWaitHandle = null;

            using (ACMonitor.Lock(_20500_LockWaitHandles))
            {
                _RequestCounter++;
                newWaitHandle = new ACObjectRMIWaitHandle(false, EventResetMode.AutoReset, _RequestCounter);
                _waitHandles.Add(newWaitHandle);
            }

            try
            {
                _ACComponent.Root.SendACMessage(WCFMessage.NewACMessage(_ACComponent.ACUrl, acUrl, newWaitHandle.RequestID, acParameter), atACComponent);
                // 5000 ms Responsezeit
                bool signaled;
                if (OperatingSystem.IsBrowser())
                {
                    if (PumpingWaitHook != null)
                        signaled = PumpingWaitHook(newWaitHandle, 5000);
                    else
                    {
                        // No pump hook registered: a blocking wait would deadlock the
                        // single-threaded browser UI forever. Fail fast instead; callers
                        // that need the result must use the async path
                        // (ExecuteMethodAsync / ACComponent.ACActionAsync).
                        signaled = false;
                    }
                }
                else
                    signaled = newWaitHandle.WaitOne(5000);
                if (!signaled)
                {
                    if (atACComponent != null)
                        _ACComponent.Messages.LogFailure(_ACComponent.ACUrl, "ACObjectRMInvoker.InvokeRemote(10)", String.Format("Request {0} [{1}] timed out at component {2}", acUrl, newWaitHandle.RequestID, atACComponent.GetACUrl()));
                    else
                        _ACComponent.Messages.LogFailure(_ACComponent.ACUrl, "ACObjectRMInvoker.InvokeRemote(10)", String.Format("Request {0} [{1}] timed out", acUrl, newWaitHandle.RequestID));
                    newWaitHandle.TimedOut = true;
                }
            }
            catch (ACWCFException e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ACObjectRMInvoker", "InvokeRemote", msg);
                return null;
            }


            using (ACMonitor.Lock(_20500_LockWaitHandles))
            {
                newWaitHandle.Close();
                _waitHandles.Remove(newWaitHandle);
            }

            object res = newWaitHandle.RemoteMethodInvocationResult;
            return res;
        }

        internal void OnMethodExecuted(int MethodInvokeRequestID, object MethodResult)
        {

            using (ACMonitor.Lock(_20500_LockWaitHandles))
            {
                try
                {
                    ACObjectRMIWaitHandle waitHandle = _waitHandles.Where(c => c.RequestID == MethodInvokeRequestID).FirstOrDefault();
                    if (waitHandle != null)
                    {
                        waitHandle.RemoteMethodInvocationResult = MethodResult;
                        waitHandle.Set();
                    }
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("ACObjectRMInvoker", "OnACMethodExecuted", msg);
                }
            }
        }

        internal ACObjectRMIWaitHandle NewSynchronousRequest()
        {
            ACObjectRMIWaitHandle newWaitHandle = null;

            using (ACMonitor.Lock(_20500_LockWaitHandles))
            {
                _RequestCounter++;
                newWaitHandle = new ACObjectRMIWaitHandle(false, EventResetMode.AutoReset, _RequestCounter);
            }
            return newWaitHandle;
        }

        internal bool WaitOnSynchronousRequest(ACObjectRMIWaitHandle request)
        {
            if (request == null)
                return false;

            using (ACMonitor.Lock(_20500_LockWaitHandles))
            {
                _waitHandles.Add(request);
            }

            // 5000 ms Responsezeit
            bool signaled;
            if (OperatingSystem.IsBrowser())
            {
                if (PumpingWaitHook != null)
                    signaled = PumpingWaitHook(request, 5000);
                else
                    signaled = false; // fail fast — see InvokeRemote
            }
            else
                signaled = request.WaitOne(5000);
            if (!signaled)
            {
                request.TimedOut = true;
                if (_ACComponent != null)
                    _ACComponent.Messages.LogFailure(_ACComponent.ACUrl, "ACObjectRMInvoker.WaitOnSynchronousRequest(10)", String.Format("Request {0} timed out", request.RequestID));
            }


            using (ACMonitor.Lock(_20500_LockWaitHandles))
            {
                request.Close();
                _waitHandles.Remove(request);
            }
            return !request.TimedOut;
        }

        internal void SignalSynchronousRequest(int RequestID)
        {
            OnMethodExecuted(RequestID, null);
        }

#endregion



    }
}
