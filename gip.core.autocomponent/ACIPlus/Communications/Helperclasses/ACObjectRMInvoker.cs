using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Threading;
using gip.core.datamodel;


namespace gip.core.autocomponent
{
    public class ACObjectRMInvoker
    {
        protected readonly ACMonitorObject _20500_LockWaitHandles = new ACMonitorObject(20500);
        List<ACObjectRMIWaitHandle> _waitHandles = new List<ACObjectRMIWaitHandle>();
        private int _RequestCounter = 0;
        private IACComponent _ACComponent = null;

        #region c´tors
        public ACObjectRMInvoker(IACComponent acComponent)
        {
            _ACComponent = acComponent;
        }

        public void Close()
        {
            foreach (ACObjectRMIWaitHandle waitHandle in _waitHandles)
            {
                waitHandle.Set();
                waitHandle.Close();
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
                if (!newWaitHandle.WaitOne(5000))
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

            return newWaitHandle.RemoteMethodInvocationResult;
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
            if (!request.WaitOne(5000))
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
