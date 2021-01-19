using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using gip.core.datamodel;
using gip.core.autocomponent;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;


namespace gip.core.communication
{
    /// <summary>
    /// Mail-Client
    /// Syntax of mail-Addresses: mailaddr1@domain.com(Display Name 1);mailaddr2@domain.com(Display Name 2);
    /// or: mailaddr1@domain.com;mailaddr2@domain.com;
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACSmtpClient'}de{'ACSmtpClient'}", Global.ACKinds.TACDAClass, Global.ACStorableTypes.Required, false, false)]
    public class ACSmtpClient : PAClassAlarmingBase
    {
        private bool _SmtpRequestActive = false;
        private SmtpClient _SmtpClientSync = null;
        private SmtpClient _SmtpClientAsync = null;

#region Properties

        [ACPropertyBindingSource(201, "Configuration", "en{'Timeout in ms'}de{'Timeout in ms'}", "", true, true)]
        public IACContainerTNet<int> SendTimeOut { get; set; }

        [ACPropertyBindingSource(202, "Configuration", "en{'Smtp-server host'}de{'Smtp-server host'}", "", true, true)]
        public IACContainerTNet<String> SmtpServerHost { get; set; }

        [ACPropertyBindingSource(203, "Configuration", "en{'Smtp-server port'}de{'Smtp-server port'}", "", true, true)]
        public IACContainerTNet<int> SmtpServerPort { get; set; }

        [ACPropertyBindingSource(207, "Configuration", "en{'Use SSL'}de{'Mit SSL'}", "", true, true)]
        public IACContainerTNet<Boolean> SmtpUseSSL { get; set; }

        [ACPropertyBindingSource(207, "Configuration", "en{'Ignore invalid certificate'}de{'Ignorierere ungültiges Zertifikat'}", "", true, true)]
        public IACContainerTNet<Boolean> IgnoreInvalidCertificate { get; set; }

        [ACPropertyBindingSource(204, "Configuration", "en{'User'}de{'Benutzer'}", "", true, true)]
        public IACContainerTNet<String> SmtpAuthUser { get; set; }

        [ACPropertyBindingSource(205, "Configuration", "en{'Password'}de{'Passwort'}", "", true, true)]
        public IACContainerTNet<String> SmtpAuthPassword { get; set; }

        [ACPropertyBindingSource(206, "Configuration", "en{'e-Mail from'}de{'e-Mail von'}", "", true, true)]
        public IACContainerTNet<String> MailAddressFrom { get; set; }

        [ACPropertyBindingSource(9999, "", "en{'Has Error'}de{'Hat Fehler'}", "", true, false, DefaultValue = false)]
        public IACContainerTNet<Boolean> IsError { get; set; }

        [ACPropertyBindingSource(9999, "", "en{'Error-text'}de{'Fehlertext'}", "", true, false, DefaultValue = false)]
        public IACContainerTNet<String> ErrorText { get; set; }

        private ACPropertyConfigValue<string> _MailingList;
        [ACPropertyConfig("en{'Mailing-List'}de{'Verteiler'}")]
        public string MailingList
        {
            get { return _MailingList.ValueT; }
            set { _MailingList.ValueT = value; }
        }

#endregion

#region Constructors

        static ACSmtpClient()
        {
            ACMethod.RegisterVirtualMethod(typeof(ACSmtpClient), "Send", CreateVirtualSendMethod("SendMailSync", "en{'Send mail'}de{'Sende mail'}", true));
            ACMethod.RegisterVirtualMethod(typeof(ACSmtpClient), "SendAsync", CreateVirtualSendMethod("SendMailAsync", "en{'Send mail'}de{'Sende mail'}", false));
            ACMethod.RegisterVirtualMethod(typeof(ACSmtpClient), "SendToMailingList", CreateVirtualSendToListMethod("SendMailToMailingList", "en{'Send mail'}de{'Sende mail'}", true));
            ACMethod.RegisterVirtualMethod(typeof(ACSmtpClient), "SendToMailingListAsync", CreateVirtualSendToListMethod("SendMailToMailingListAsync", "en{'Send mail'}de{'Sende mail'}", false));
        }

        public ACSmtpClient(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _MailingList = new ACPropertyConfigValue<string>(this, "MailingList", "");
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            _TaskInvocationPoint = new ACPointAsyncRMI(this, Const.TaskInvocationPoint, 0);
            _TaskInvocationPoint.SetMethod = OnSetTaskInvocationPoint;
            
            bool result = base.ACInit(startChildMode);

            InitAsyncSmtpClient();
            InitSyncSmtpClient();
            return result;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            if (_SmtpClientAsync != null)
            {
                _SmtpClientAsync.Dispose();
                _SmtpClientAsync.SendCompleted -= _SmtpClientAsync_SendCompleted;
                _SmtpClientAsync = null;
            }
            if (_SmtpClientSync != null)
            {
                _SmtpClientSync.Dispose();
                _SmtpClientSync = null;
            }
            bool result = base.ACDeInit(deleteACClassTask);
            return result;
        }

#endregion

#region Points

        protected ACPointAsyncRMI _TaskInvocationPoint;
        [ACPropertyAsyncMethodPoint(9999, false, 0)]
        public ACPointAsyncRMI TaskInvocationPoint
        {
            get
            {
                return _TaskInvocationPoint;
            }
        }

        public bool OnSetTaskInvocationPoint(IACPointNetBase point)
        {
            TaskInvocationPoint.DeQueueInvocationList();
            return true;
        }

#endregion

#region Public 

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "Send":
                    result = Send(acParameter[0] as ACMethod);
                    return true;
                case "SendAsync":
                    result = SendAsync(acParameter[0] as ACMethod);
                    return true;
                case "SendToMailingList":
                    result = SendToMailingList(acParameter[0] as ACMethod);
                    return true;
                case "SendToMailingListAsync":
                    result = SendToMailingListAsync(acParameter[0] as ACMethod);
                    return true;
                case "SendTestMail":
                    SendTestMail();
                    return true;
                case Const.IsEnabledPrefix + "Send":
                    result = IsEnabledSend(acParameter[0] as ACMethod);
                    return true;
                case Const.IsEnabledPrefix + "SendAsync":
                    result = IsEnabledSendAsync(acParameter[0] as ACMethod);
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
        #endregion


        [ACMethodInfo("Mail", "en{'Send mail'}de{'Sende mail'}", 200, false)]
        public Boolean Send(ACMethod acMethod)
        {
            if (!IsEnabledSend(acMethod))
                return false;
            MailMessage message = BuildMailMessage(acMethod);
            if (message == null)
                return false;

            InitSyncSmtpClient();
            try
            {
                _SmtpClientSync.Send(message);
            }
            catch (Exception ex)
            {
                Messages.LogFailure(this.GetACUrl(), "ACSmtpClient.Send(1)", ex.Message);
                return false;
            }

            return true;
        }

        public bool IsEnabledSend(ACMethod acMethod)
        {
            if (!acMethod.IsValid())
                return false;
            if (String.IsNullOrEmpty(SmtpServerHost.ValueT))
                return false;
            if (String.IsNullOrEmpty(SmtpServerHost.ValueT))
                return false;
            String from = GetFrom(acMethod);
            if (String.IsNullOrEmpty(from))
                return false;
            return true;
        }

        [ACMethodAsync("Mail", "en{'Send mail asynchronous'}de{'Sende mail asynchron'}", 201, false)]
        public ACMethodEventArgs SendAsync(ACMethod acMethod)
        {
            ACMethodEventArgs result = new ACMethodEventArgs(acMethod, Global.ACMethodResultState.Failed);
            ACValue succValue = result.GetACValue("Succeeded");
            if (succValue == null)
            {
                succValue = new ACValue("Succeeded", typeof(Boolean), false);
                result.Add(succValue);
            }

            if (!IsEnabledSendAsync(acMethod))
                return result;

            MailMessage message = BuildMailMessage(acMethod);
            if (message == null)
                return result;

            InitAsyncSmtpClient();
            try
            {
                _SmtpRequestActive = true;
                _SmtpClientAsync.SendAsync(message,
                                acMethod.ACRequestID.ToString());
            }
            catch (Exception ex)
            {
                _SmtpClientAsync = null;
                Messages.LogFailure(this.GetACUrl(), "ACSmtpClient.SendAsync(1)", ex.Message);
                result = new ACMethodEventArgs(acMethod, Global.ACMethodResultState.Failed);
                return result;
            }

            result = new ACMethodEventArgs(acMethod, Global.ACMethodResultState.InProcess);
            return result;
        }

        public bool IsEnabledSendAsync(ACMethod acMethod)
        {
            if (_SmtpRequestActive)
                return false;
            if (!IsEnabledSend(acMethod))
                return false;
            return true;
        }

        [ACMethodInfo("Mail", "en{'Send to mailinglist'}de{'Sende an Verteilerliste'}", 202, false)]
        public Boolean SendToMailingList(ACMethod acMethod)
        {
            if (String.IsNullOrEmpty(MailingList))
                return false;
            ACMethod acMethod2 = ACUrlACTypeSignature("!SendMailSync", Database.ContextIPlus);
            acMethod2.ParameterValueList["Recipients"] = MailingList;
            acMethod2.ParameterValueList["Subject"] = acMethod.ParameterValueList["Subject"];
            acMethod2.ParameterValueList["Body"] = acMethod.ParameterValueList["Body"];
            return Send(acMethod2);
        }

        [ACMethodAsync("Mail", "en{'Send to mailinglist async.'}de{'Sende an Verteilerliste async.'}", 203, false)]
        public ACMethodEventArgs SendToMailingListAsync(ACMethod acMethod)
        {
            if (String.IsNullOrEmpty(MailingList))
            {
                ACMethodEventArgs result = new ACMethodEventArgs(acMethod, Global.ACMethodResultState.Failed);
                ACValue durationValue = result.GetACValue("Succeeded");
                if (durationValue == null)
                {
                    durationValue = new ACValue("Succeeded", typeof(Boolean), false);
                    result.Add(durationValue);
                }
                return result;
            }
            ACMethod acMethod2 = ACUrlACTypeSignature("!SendMailAsync", Database.ContextIPlus);
            acMethod2.ParameterValueList["Recipients"] = MailingList;
            acMethod2.ParameterValueList["Subject"] = acMethod.ParameterValueList["Subject"];
            acMethod2.ParameterValueList["Body"] = acMethod.ParameterValueList["Body"];
            return SendAsync(acMethod2);
        }

        [ACMethodInteraction("Mail", "en{'Send testmail to mailinglist'}de{'Sende Testmail an Verteilerliste'}", 210, true)]
        public void SendTestMail()
        {
            ACMethod acMethod = ACUrlACTypeSignature("!SendToMailingList", gip.core.datamodel.Database.GlobalDatabase); // Immer Globalen context um Deadlock zu vermeiden 
            acMethod.ParameterValueList["Subject"] = "Test";
            acMethod.ParameterValueList["Body"] = "Testmail from variobatch";
            SendToMailingList(acMethod);
        }

#endregion 

#region Private 

        private string GetFrom(ACMethod acMethod)
        {
            String from = acMethod.ParameterValueList["From"] as string;
            if (String.IsNullOrEmpty(from))
                from = MailAddressFrom.ValueT;
            return from;
        }

        private void InitAsyncSmtpClient()
        {
            if (_SmtpClientAsync == null)
            {
                if (SmtpServerPort.ValueT > 0)
                    _SmtpClientAsync = new SmtpClient(SmtpServerHost.ValueT, SmtpServerPort.ValueT);
                else
                    _SmtpClientAsync = new SmtpClient(SmtpServerHost.ValueT);
                _SmtpClientAsync.SendCompleted += new SendCompletedEventHandler(_SmtpClientAsync_SendCompleted);
            }
            if (_SmtpClientAsync.Host != SmtpServerHost.ValueT)
                _SmtpClientAsync.Host = SmtpServerHost.ValueT;
            if (SmtpServerPort.ValueT > 0 && _SmtpClientAsync.Port != SmtpServerPort.ValueT)
                _SmtpClientAsync.Port = SmtpServerPort.ValueT;
            if (SendTimeOut.ValueT > 0 && _SmtpClientAsync.Timeout != SendTimeOut.ValueT)
                _SmtpClientAsync.Timeout = SendTimeOut.ValueT;

            if (SmtpUseSSL.ValueT)
                _SmtpClientAsync.EnableSsl = true;
            else
                _SmtpClientAsync.EnableSsl = false;
            if (_SmtpClientAsync.Credentials == null)
            {
                if (!String.IsNullOrEmpty(SmtpAuthUser.ValueT) && !String.IsNullOrEmpty(SmtpAuthPassword.ValueT))
                    _SmtpClientAsync.Credentials = new NetworkCredential(SmtpAuthUser.ValueT, SmtpAuthPassword.ValueT);
            }
            else
            {
                NetworkCredential credential = _SmtpClientAsync.Credentials as NetworkCredential;
                if (credential.Password != SmtpAuthPassword.ValueT || credential.UserName != SmtpAuthUser.ValueT)
                {
                    if (!String.IsNullOrEmpty(SmtpAuthUser.ValueT) && !String.IsNullOrEmpty(SmtpAuthPassword.ValueT))
                        _SmtpClientAsync.Credentials = new NetworkCredential(SmtpAuthUser.ValueT, SmtpAuthPassword.ValueT);
                    else
                        _SmtpClientAsync.Credentials = null;
                }
            }
            if (IgnoreInvalidCertificate.ValueT)
                ServicePointManager.ServerCertificateValidationCallback = delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
        }

        private void InitSyncSmtpClient()
        {
            if (_SmtpClientSync == null)
            {
                if (SmtpServerPort.ValueT > 0)
                    _SmtpClientSync = new SmtpClient(SmtpServerHost.ValueT, SmtpServerPort.ValueT);
                else
                    _SmtpClientSync = new SmtpClient(SmtpServerHost.ValueT);
            }
            if (_SmtpClientSync.Host != SmtpServerHost.ValueT)
                _SmtpClientSync.Host = SmtpServerHost.ValueT;
            if (SmtpServerPort.ValueT > 0 && _SmtpClientSync.Port != SmtpServerPort.ValueT)
                _SmtpClientSync.Port = SmtpServerPort.ValueT;
            if (SmtpUseSSL.ValueT)
                _SmtpClientSync.EnableSsl = true;
            else
                _SmtpClientSync.EnableSsl = false;
            if (_SmtpClientSync.Credentials == null)
            {
                if (!String.IsNullOrEmpty(SmtpAuthUser.ValueT) && !String.IsNullOrEmpty(SmtpAuthPassword.ValueT))
                    _SmtpClientSync.Credentials = new NetworkCredential(SmtpAuthUser.ValueT, SmtpAuthPassword.ValueT);
            }
            else
            {
                NetworkCredential credential = _SmtpClientSync.Credentials as NetworkCredential;
                if (credential.Password != SmtpAuthPassword.ValueT || credential.UserName != SmtpAuthUser.ValueT)
                {
                    if (!String.IsNullOrEmpty(SmtpAuthUser.ValueT) && !String.IsNullOrEmpty(SmtpAuthPassword.ValueT))
                        _SmtpClientSync.Credentials = new NetworkCredential(SmtpAuthUser.ValueT, SmtpAuthPassword.ValueT);
                    else
                        _SmtpClientSync.Credentials = null;
                }
            }
        }

        private MailMessage BuildMailMessage(ACMethod acMethod)
        {
            MailMessage message = new MailMessage();
            message.From = new MailAddress(GetFrom(acMethod));
            string recipients = acMethod.ParameterValueList["Recipients"] as string;
            string[] arrRec = recipients.Split(';');
            foreach (string email in arrRec)
            {
                if (email.IndexOf('@') <= 0)
                    continue;
                String name = "";
                string addr = email;
                int openBracket = email.IndexOf('(');
                int closeBracket = email.IndexOf(')');
                if (openBracket > 2 && closeBracket > 2 && closeBracket > (openBracket + 1))
                    name = email.Substring(openBracket + 1, closeBracket - openBracket - 1);
                if (openBracket > 0 || closeBracket > 0)
                {
                    int min = closeBracket > openBracket ? openBracket : closeBracket;
                    addr = email.Substring(0, min);
                }
                else if (openBracket >= 0 || closeBracket >= 0)
                    continue;
                message.To.Add(new MailAddress(addr, name));
            }

            message.Subject = acMethod.ParameterValueList["Subject"] as string;
            message.Body = acMethod.ParameterValueList["Body"] as string;
            return message;
        }

        void _SmtpClientAsync_SendCompleted(object sender, AsyncCompletedEventArgs e)
        {
            _SmtpRequestActive = false;
            // Get the unique identifier for this asynchronous operation.
            String token = (string)e.UserState;
            if (e.Cancelled)
            {
                Messages.LogFailure(this.GetACUrl(), "ACSmtpClient._SmtpClientAsync_SendCompleted(Send canceled)", token);
            }
            if (e.Error != null)
            {
                IsError.ValueT = true;
                ErrorText.ValueT = e.Error.ToString();
                Messages.LogFailure(this.GetACUrl(), "ACSmtpClient._SmtpClientAsync_SendCompleted(" + token + ")", e.Error.ToString());
                // Callback
                if (!TaskInvocationPoint.CurrentAsyncRMI.CallbackIsPending)
                {
                    TaskInvocationPoint.InvokeCallbackDelegate(new ACMethodEventArgs(TaskInvocationPoint.CurrentAsyncRMI.RequestID, new ACValueList(), Global.ACMethodResultState.Failed));
                }
            }
            else
            {
                IsError.ValueT = false;
                ErrorText.ValueT = "";
                // Callback
                if (!TaskInvocationPoint.CurrentAsyncRMI.CallbackIsPending)
                {
                    TaskInvocationPoint.InvokeCallbackDelegate(new ACMethodEventArgs(TaskInvocationPoint.CurrentAsyncRMI.RequestID, new ACValueList(), Global.ACMethodResultState.Succeeded));
                }
            }
        }

        private static ACMethodWrapper CreateVirtualSendMethod(string acIdentifier, string captionTranslation, bool includeResponse)
        {
            ACMethod method = new ACMethod(acIdentifier);

            method.ParameterValueList.Add(new ACValue("From", typeof(String), null, Global.ParamOption.Optional));
            method.ParameterValueList.Add(new ACValue("Recipients", typeof(String), null, Global.ParamOption.Required));
            method.ParameterValueList.Add(new ACValue("Subject", typeof(String), null, Global.ParamOption.Required));
            method.ParameterValueList.Add(new ACValue("Body", typeof(String), null, Global.ParamOption.Required));

            if (includeResponse)
                method.ResultValueList.Add(new ACValue("Succeeded", typeof(bool), false, Global.ParamOption.Required));

            return new ACMethodWrapper(method, captionTranslation, null);
        }

        private static ACMethodWrapper CreateVirtualSendToListMethod(string acIdentifier, string captionTranslation, bool includeResponse)
        {
            ACMethod method = new ACMethod(acIdentifier);

            method.ParameterValueList.Add(new ACValue("Subject", typeof(String), null, Global.ParamOption.Required));
            method.ParameterValueList.Add(new ACValue("Body", typeof(String), null, Global.ParamOption.Required));

            if (includeResponse)
                method.ResultValueList.Add(new ACValue("Succeeded", typeof(bool), false, Global.ParamOption.Required));

            return new ACMethodWrapper(method, captionTranslation, null);
        }
#endregion

    }
}
