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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace gip.core.communication
{
    /// <summary>
    /// Rest-Client
    /// </summary>
    [ACSerializeableInfo(new Type[] { typeof(WSResponse<string>) })]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACRestClient'}de{'ACRestClient'}", Global.ACKinds.TACDAClass, Global.ACStorableTypes.Required, false, false)]
    public class ACRestClient : ACSession
    {
        #region Constructors
        static ACRestClient()
        {
            ACMethod.RegisterVirtualMethod(typeof(ACRestClient), "CallWS", CreateVirtualSendMethod("GET", "en{'GET'}de{'GET'}"));
            //ACMethod.RegisterVirtualMethod(typeof(ACRestClient), "CallWSAsync", CreateVirtualSendMethod("GETAsync", "en{'GET async'}de{'GET async'}"));
            ACMethod.RegisterVirtualMethod(typeof(ACRestClient), "CallWS", CreateVirtualSendMethod("POST", "en{'POST'}de{'POST'}"));
            //ACMethod.RegisterVirtualMethod(typeof(ACRestClient), "CallWSAsync", CreateVirtualSendMethod("POSTAsync", "en{'POST async'}de{'POST async'}"));
        }

        public ACRestClient(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            _TaskInvocationPoint = new ACPointAsyncRMI(this, Const.TaskInvocationPoint, 0);
            _TaskInvocationPoint.SetMethod = OnSetTaskInvocationPoint;

            if (_JsonMSDateFormat == null)
                _JsonMSDateFormat = new ACPropertyConfigValue<bool>(this, "JsonMSDateFormat", false);

            bool result = base.ACInit(startChildMode);

            Connect();

            return result;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            bool result = base.ACDeInit(deleteACClassTask);
            return result;
        }

        #endregion


        #region Properties
        public readonly ACMonitorObject _30210_LockValue = new ACMonitorObject(30210);

        HttpClient _Client = null;
        public HttpClient Client
        {
            get
            {
                using (ACMonitor.Lock(_30210_LockValue))
                {
                    return _Client;
                }
            }
        }

        private string _ServiceUrl;
        [ACPropertyInfo(true, 201, "", "en{'Service-Url'}de{'Service-Url'}", "", true)]
        public string ServiceUrl
        {
            get
            {
                return _ServiceUrl;
            }
            set
            {
                _ServiceUrl = value;
                OnPropertyChanged("ServiceUrl");
            }
        }


        private string _User;
        [ACPropertyInfo(true, 202, "", "en{'User'}de{'Benutzer'}", "", true)]
        public string User
        {
            get
            {
                return _User;
            }
            set
            {
                _User = value;
                OnPropertyChanged("User");
            }
        }

        private string _Password;
        [ACPropertyInfo(true, 203, "", "en{'Password'}de{'Passwort'}", "", true)]
        public string Password
        {
            get
            {
                return _Password;
            }
            set
            {
                _Password = value;
                OnPropertyChanged("Password");
            }
        }

        private TimeSpan _TimeOut;
        [ACPropertyInfo(true, 203, "", "en{'TimeOut'}de{'TimeOut'}", "", true)]
        public TimeSpan TimeOut
        {
            get
            {
                return _TimeOut;
            }
            set
            {
                _TimeOut = value;
                OnPropertyChanged("TimeOut");
            }
        }

        private static ACPropertyConfigValue<bool> _JsonMSDateFormat;
        [ACPropertyConfig("en{'Json Microsoft Date-Format'}de{'Json Microsoft Date-Format'}")]
        public bool JsonMSDateFormat
        {
            get { return _JsonMSDateFormat.ValueT; }
            set { _JsonMSDateFormat.ValueT = value; }
        }

        public virtual DateFormatHandling DTFormatHandling
        {
            get
            {
                return JsonMSDateFormat == true ? DateFormatHandling.MicrosoftDateFormat : DateFormatHandling.IsoDateFormat;
            }
        }

        protected JsonSerializerSettings _DefaultJsonSerializerSettings;
        public virtual JsonSerializerSettings DefaultJsonSerializerSettings
        {
            get
            {
                if (_DefaultJsonSerializerSettings != null)
                    return _DefaultJsonSerializerSettings;
                _DefaultJsonSerializerSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    //TypeNameHandling = TypeNameHandling.None,
                    //DefaultValueHandling = DefaultValueHandling.Ignore,
                    Formatting = Formatting.Indented,
                    DateFormatHandling = DTFormatHandling
                };
                return _DefaultJsonSerializerSettings;
            }
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
                case "CallWS":
                    result = CallWS(acParameter[0] as ACMethod);
                    return true;
                //case "CallWSAsync":
                //    result = CallWSAsync(acParameter[0] as ACMethod);
                //    return true;
                case Const.IsEnabledPrefix + "CallWS":
                    result = IsEnabledCallWS(acParameter[0] as ACMethod);
                    return true;
                    //case Const.IsEnabledPrefix + "CallWSAsync":
                    //    result = IsEnabledCallWSAsync(acParameter[0] as ACMethod);
                    //    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
        #endregion

        #region virtual
        protected void OnCreateDefaultRequestHeaders(HttpClient client)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            if (!String.IsNullOrEmpty(User) && !String.IsNullOrEmpty(Password))
            {
                client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue(
                            "Basic", Convert.ToBase64String(
                                System.Text.ASCIIEncoding.ASCII.GetBytes(User + ":" + Password)));
            }
        }

        #endregion

        #region ACMethod
        [ACMethodInfo("", "en{'Call Rest-Service'}de{'Call Rest-Service'}", 200, false)]
        public WSResponse<string> CallWS(ACMethod acMethod)
        {
            if (!IsEnabledCallWS(acMethod))
                return new WSResponse<string>(null, new Msg(eMsgLevel.Error, "Disconnected"));
            string uri = "";
            ACValue pUri = acMethod.ParameterValueList.GetACValue("Uri");
            if (pUri != null)
                uri = pUri.ParamAsString;
            bool isAbsolute = false;
            ACValue pIsAbsoluteUri = acMethod.ParameterValueList.GetACValue("IsAbsoluteUri");
            if (pIsAbsoluteUri != null)
                isAbsolute = pUri.ParamAsBoolean;

            if (acMethod.ACIdentifier == "GET")
            {
                return CallGET(uri, isAbsolute);
            }
            else //if (acMethod.ACIdentifier == "POST")
            {
                string sRequest = null;
                ACValue pRequest = acMethod.ParameterValueList.GetACValue("Request");
                if (pRequest == null || String.IsNullOrEmpty(pRequest.ParamAsString))
                    return new WSResponse<string>(null, new Msg(eMsgLevel.Error, "Request is Empty"));
                else
                    sRequest = pRequest.ParamAsString;
                ACValue pMediaType = acMethod.ParameterValueList.GetACValue("MediaType");
                return CallPOST(uri, isAbsolute, sRequest, pMediaType != null ? pMediaType.ParamAsString : null);
            }
        }

        public bool IsEnabledCallWS(ACMethod acMethod)
        {
            if (!acMethod.IsValid())
                return false;
            if (Client == null)
                return false;
            return true;
        }

        [ACMethodInfo("", "en{'GET'}de{'GET'}", 201, false)]
        public WSResponse<string> CallGET(string uri, bool isAbsoluteUri)
        {
            if (!IsEnabledDisConnect())
                return new WSResponse<string>(null, new Msg(eMsgLevel.Error, "Disconnected"));
            Uri callUri = isAbsoluteUri ? new Uri(uri) : GetUri(uri);
            return Get(callUri);
        }

        [ACMethodInfo("", "en{'POST'}de{'POST'}", 202, false)]
        public WSResponse<string> CallPOST(string uri, bool isAbsoluteUri, string content, string mediaType)
        {
            if (!IsEnabledDisConnect())
                return new WSResponse<string>(null, new Msg(eMsgLevel.Error, "Disconnected"));
            if (String.IsNullOrEmpty(content))
                return new WSResponse<string>(null, new Msg(eMsgLevel.Error, "Request is Empty"));
            StringContent sContent = string.IsNullOrEmpty(mediaType) ? new StringContent(content) : new StringContent(content, Encoding.UTF8, mediaType);
            Uri callUri = isAbsoluteUri ? new Uri(uri) : GetUri(uri);
            return Post(sContent, callUri);
        }


        //[ACMethodAsync("Mail", "en{'Call Rest-Service asynchronous'}de{'Call Rest-Service asynchron'}", 201, false)]
        //public ACMethodEventArgs CallWSAsync(ACMethod acMethod)
        //{
        //    ACMethodEventArgs result = new ACMethodEventArgs(acMethod, Global.ACMethodResultState.Failed);
        //    ACValue succValue = result.GetACValue("Succeeded");
        //    if (succValue == null)
        //    {
        //        succValue = new ACValue("Succeeded", typeof(Boolean), false);
        //        result.Add(succValue);
        //    }

        //    if (!IsEnabledCallWSAsync(acMethod))
        //        return result;

        //    result = new ACMethodEventArgs(acMethod, Global.ACMethodResultState.InProcess);
        //    return result;
        //}

        //public bool IsEnabledCallWSAsync(ACMethod acMethod)
        //{
        //    if (!acMethod.IsValid())
        //        return false;
        //    if (Client == null)
        //        return false;
        //    return true;
        //}

        private static ACMethodWrapper CreateVirtualSendMethod(string acIdentifier, string captionTranslation)
        {
            ACMethod method = new ACMethod(acIdentifier);
            method.ParameterValueList.Add(new ACValue("Uri", typeof(String), null, Global.ParamOption.Optional));
            method.ParameterValueList.Add(new ACValue("IsAbsoluteUri", typeof(bool), false, Global.ParamOption.Optional));
            method.ParameterValueList.Add(new ACValue("Request", typeof(String), null, Global.ParamOption.Optional));
            method.ParameterValueList.Add(new ACValue("MediaType", typeof(String), null, Global.ParamOption.Optional));
            method.ResultValueList.Add(new ACValue("Response", typeof(string), false, Global.ParamOption.Required));
            return new ACMethodWrapper(method, captionTranslation, null);
        }
        #endregion


        #region Connection
        public override bool InitSession()
        {
            if (!IsEnabledInitSession())
                return true;
            using (ACMonitor.Lock(_30210_LockValue))
            {
                if (_Client != null)
                    return true;
                _Client = new HttpClient();
                _Client.Timeout = TimeOut > TimeSpan.MinValue ? TimeOut : new TimeSpan(0, 0, 15);
                _Client.BaseAddress = new Uri(ServiceUrl);
                OnCreateDefaultRequestHeaders(_Client);
            }
            IsConnected.ValueT = true;
            return true;
        }

        public override bool IsEnabledInitSession()
        {
            using (ACMonitor.Lock(_30210_LockValue))
            {
                return _Client == null && !String.IsNullOrEmpty(ServiceUrl);
            }
        }

        public override bool DeInitSession()
        {
            using (ACMonitor.Lock(_30210_LockValue))
            {
                if (_Client == null)
                    return true;
                _Client.Dispose();
                _Client = null;
            }
            IsConnected.ValueT = false;
            return true;
        }

        public override bool IsEnabledDeInitSession()
        {
            using (ACMonitor.Lock(_30210_LockValue))
            {
                return _Client != null;
            }
        }

        public override bool Connect()
        {
            return InitSession();
        }

        public override bool IsEnabledConnect()
        {
            return IsEnabledInitSession();
        }

        public override bool DisConnect()
        {
            return DeInitSession();
        }

        public override bool IsEnabledDisConnect()
        {
            return IsEnabledDeInitSession();
        }

        protected override void StartReconnection()
        {

        }
        #endregion


        #region HTTP

        #region Helper
        public Uri GetUri(string relativeUri)
        {
            if (!String.IsNullOrEmpty(relativeUri))
                return new Uri(relativeUri, UriKind.Relative);
            else
                return new Uri(ServiceUrl, UriKind.Absolute);
        }
        #endregion

        #region GET

        #region Sync
        public WSResponse<string> Get(string relativeUri)
        {
            return Get(GetUri(relativeUri));
        }

        public WSResponse<TResult> Get<TResult>(string relativeUri)
        {
            return Get<TResult>(GetUri(relativeUri));
        }

        public WSResponse<string> Get(Uri uri)
        {
            if (Client == null)
                return new WSResponse<string>(null, new Msg(eMsgLevel.Error, "Disconnected"));
            try
            {
                Task<WSResponse<string>> task = GetAsync(uri);
                return task.Result;
            }
            catch (Exception ex)
            {
                var msg = new Msg(eMsgLevel.Exception, ex.Message);
                Messages.LogException(this.GetACUrl(), "Get(30)", ex);
                OnNewAlarmOccurred("IsConnected", new Msg(eMsgLevel.Exception, ex.Message));
                if (ex is HttpRequestException)
                    IsConnected.ValueT = false;
                return new WSResponse<string>(msg);
            }
        }

        public WSResponse<TResult> Get<TResult>(Uri uri)
        {
            if (Client == null)
                return new WSResponse<TResult>(default(TResult), new Msg(eMsgLevel.Error, "Disconnected"));
            try
            {
                Task<WSResponse<TResult>> task = GetAsync<TResult>(uri);
                return task.Result;
            }
            catch (Exception ex)
            {
                var msg = new Msg(eMsgLevel.Exception, ex.Message);
                Messages.LogException(this.GetACUrl(), "Get(40)", ex);
                OnNewAlarmOccurred("IsConnected", new Msg(eMsgLevel.Exception, ex.Message));
                if (ex is HttpRequestException)
                    IsConnected.ValueT = false;
                return new WSResponse<TResult>(msg);
            }
        }
        #endregion

        #region Async
        public async Task<WSResponse<string>> GetAsync(string relativeUri)
        {
            return await GetAsync(GetUri(relativeUri));
        }

        public async Task<WSResponse<string>> GetAsync(Uri uri)
        {
            HttpClient client = Client;
            if (client == null)
                return await Task.FromResult(new WSResponse<string>(new Msg(eMsgLevel.Error, "Disconnected")));
            HttpResponseMessage response = null;
            response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                IsConnected.ValueT = true;
                string result = await response.Content.ReadAsStringAsync();
                return new WSResponse<string>(result);
            }
            return await Task.FromResult(new WSResponse<string>(new Msg(eMsgLevel.Failure, String.Format("{0},{1}", response.ReasonPhrase, response.StatusCode))));
        }

        public async Task<WSResponse<TResult>> GetAsync<TResult>(string relativeUri)
        {
            return await GetAsync<TResult>(GetUri(relativeUri));
        }

        public async Task<WSResponse<TResult>> GetAsync<TResult>(Uri uri)
        {
            HttpClient client = Client;
            if (client == null)
                return await Task.FromResult(new WSResponse<TResult>(new Msg(eMsgLevel.Error, "Disconnected")));
            HttpResponseMessage response = null;
            response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                IsConnected.ValueT = true;
                string json = await response.Content.ReadAsStringAsync();
                var result = await Task.Run(() => JsonConvert.DeserializeObject<TResult>(json));
                return new WSResponse<TResult>(result);
            }
            return await Task.FromResult(new WSResponse<TResult>(new Msg(eMsgLevel.Failure, String.Format("{0},{1}", response.ReasonPhrase, response.StatusCode))));
        }
        #endregion

        #endregion

        #region POST

        #region Sync
        public WSResponse<string> Post(StringContent content, string relativeUri)
        {
            return Post(content, GetUri(relativeUri));
        }

        public WSResponse<TResult> Post<TResult>(StringContent content, string relativeUri)
        {
            return Post<TResult>(content, GetUri(relativeUri));
        }

        public WSResponse<TResult> Post<TResult, TParam>(TParam item, string relativeUri)
        {
            return Post<TResult, TParam>(item, GetUri(relativeUri));
        }

        public WSResponse<string> Post(StringContent content, Uri uri)
        {
            if (Client == null)
                return new WSResponse<string>(null, new Msg(eMsgLevel.Error, "Disconnected"));
            try
            {
                Task<WSResponse<string>> task = PostAsync(content, uri);
                return task.Result;
            }
            catch (Exception ex)
            {
                string errMsg = "";
                Exception tmpEx = ex;
                while (tmpEx != null)
                {
                    errMsg += tmpEx.Message;
                    tmpEx = tmpEx.InnerException;
                }
                var msg = new Msg(eMsgLevel.Exception, errMsg);
                Messages.LogException(this.GetACUrl(), "Post(20)", ex);
                OnNewAlarmOccurred("IsConnected", new Msg(eMsgLevel.Exception, errMsg));
                if (ex is HttpRequestException)
                    IsConnected.ValueT = false;
                return new WSResponse<string>(null, msg);
            }
        }

        public WSResponse<TResult> Post<TResult>(StringContent content, Uri uri)
        {
            if (Client == null)
                return new WSResponse<TResult>(default(TResult), new Msg(eMsgLevel.Error, "Disconnected"));
            try
            {
                Task<WSResponse<TResult>> task = PostAsync<TResult>(content, uri);
                return task.Result;
            }
            catch (Exception ex)
            {
                var msg = new Msg(eMsgLevel.Exception, ex.Message);
                Messages.LogException(this.GetACUrl(), "Post(30)", ex);
                OnNewAlarmOccurred("IsConnected", new Msg(eMsgLevel.Exception, ex.Message));
                if (ex is HttpRequestException)
                    IsConnected.ValueT = false;
                return new WSResponse<TResult>(msg);
            }
        }

        public WSResponse<TResult> Post<TResult, TParam>(TParam item, Uri uri)
        {
            if (Client == null)
                return new WSResponse<TResult>(default(TResult), new Msg(eMsgLevel.Error, "Disconnected"));
            try
            {
                Task<WSResponse<TResult>> task = PostAsync<TResult, TParam>(item, uri);
                return task.Result;
            }
            catch (Exception ex)
            {
                var msg = new Msg(eMsgLevel.Exception, ex.Message);
                Messages.LogException(this.GetACUrl(), "Post(40)", ex);
                OnNewAlarmOccurred("IsConnected", new Msg(eMsgLevel.Exception, ex.Message));
                if (ex is HttpRequestException)
                    IsConnected.ValueT = false;
                return new WSResponse<TResult>(msg);
            }
        }
        #endregion

        #region Async
        protected async Task<WSResponse<TResult>> PostAsync<TResult, TParam>(TParam item, string uriString)
        {
            return await PostAsync<TResult, TParam>(item, new Uri(uriString, UriKind.Relative));
        }

        protected async Task<WSResponse<TResult>> PostAsync<TResult, TParam>(TParam item, Uri uri)
        {
            var serializedItem = JsonConvert.SerializeObject(item, DefaultJsonSerializerSettings);
            using (StringContent content = new StringContent(serializedItem, Encoding.UTF8, "application/json"))
            {
                HttpResponseMessage response = null;
                response = await _Client.PostAsync(uri, content);
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    var result = await Task.Run(() => JsonConvert.DeserializeObject<TResult>(json));
                    return new WSResponse<TResult>(result);
                }
                return await Task.FromResult(new WSResponse<TResult>(new Msg(eMsgLevel.Failure, String.Format("{0},{1}", response.ReasonPhrase, response.StatusCode))));
            }
        }

        protected async Task<WSResponse<TResult>> PostAsync<TResult>(StringContent content, Uri uri)
        {
            HttpResponseMessage response = null;
            response = await _Client.PostAsync(uri, content);
            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                var result = await Task.Run(() => JsonConvert.DeserializeObject<TResult>(json));
                return new WSResponse<TResult>(result);
            }
            return await Task.FromResult(new WSResponse<TResult>(new Msg(eMsgLevel.Failure, String.Format("{0},{1}", response.ReasonPhrase, response.StatusCode))));
        }

        protected async Task<WSResponse<string>> PostAsync(StringContent content, Uri uri)
        {
            HttpResponseMessage response = null;
            response = await _Client.PostAsync(uri, content);
            if (response.IsSuccessStatusCode)
            {
                string result = await response.Content.ReadAsStringAsync();
                return new WSResponse<string>(result);
            }
            return await Task.FromResult(new WSResponse<string>(new Msg(eMsgLevel.Failure, String.Format("{0},{1}", response.ReasonPhrase, response.StatusCode))));
        }
        #endregion

        #endregion

        #region PUT

        #region Sync
        public WSResponse<string> Put(StringContent content, string relativeUri)
        {
            return Put(content, GetUri(relativeUri));
        }

        public WSResponse<TResult> Put<TResult>(StringContent content, string relativeUri)
        {
            return Put<TResult>(content, GetUri(relativeUri));
        }

        public WSResponse<TResult> Put<TResult, TParam>(TParam item, string relativeUri)
        {
            return Put<TResult, TParam>(item, GetUri(relativeUri));
        }

        public WSResponse<string> Put(StringContent content, Uri uri)
        {
            if (Client == null)
                return new WSResponse<string>(null, new Msg(eMsgLevel.Error, "Disconnected"));
            try
            {
                Task<WSResponse<string>> task = PutAsync(content, uri);
                return task.Result;
            }
            catch (Exception ex)
            {
                var msg = new Msg(eMsgLevel.Exception, ex.Message);
                Messages.LogException(this.GetACUrl(), "Put(20)", ex);
                OnNewAlarmOccurred("IsConnected", new Msg(eMsgLevel.Exception, ex.Message));
                if (ex is HttpRequestException)
                    IsConnected.ValueT = false;
                return new WSResponse<string>(null, msg);
            }
        }

        public WSResponse<TResult> Put<TResult>(StringContent content, Uri uri)
        {
            if (Client == null)
                return new WSResponse<TResult>(default(TResult), new Msg(eMsgLevel.Error, "Disconnected"));
            try
            {
                Task<WSResponse<TResult>> task = PutAsync<TResult>(content, uri);
                return task.Result;
            }
            catch (Exception ex)
            {
                var msg = new Msg(eMsgLevel.Exception, ex.Message);
                Messages.LogException(this.GetACUrl(), "Put(30)", ex);
                OnNewAlarmOccurred("IsConnected", new Msg(eMsgLevel.Exception, ex.Message));
                if (ex is HttpRequestException)
                    IsConnected.ValueT = false;
                return new WSResponse<TResult>(msg);
            }
        }

        public WSResponse<TResult> Put<TResult, TParam>(TParam item, Uri uri)
        {
            if (Client == null)
                return new WSResponse<TResult>(default(TResult), new Msg(eMsgLevel.Error, "Disconnected"));
            try
            {
                Task<WSResponse<TResult>> task = PutAsync<TResult, TParam>(item, uri);
                return task.Result;
            }
            catch (Exception ex)
            {
                var msg = new Msg(eMsgLevel.Exception, ex.Message);
                Messages.LogException(this.GetACUrl(), "Put(40)", ex);
                OnNewAlarmOccurred("IsConnected", new Msg(eMsgLevel.Exception, ex.Message));
                if (ex is HttpRequestException)
                    IsConnected.ValueT = false;
                return new WSResponse<TResult>(msg);
            }
        }
        #endregion

        #region Async
        protected async Task<WSResponse<TResult>> PutAsync<TResult, TParam>(TParam item, string uriString)
        {
            return await PutAsync<TResult, TParam>(item, new Uri(uriString, UriKind.Relative));
        }

        protected async Task<WSResponse<TResult>> PutAsync<TResult, TParam>(TParam item, Uri uri)
        {
            var serializedItem = JsonConvert.SerializeObject(item, DefaultJsonSerializerSettings);
            using (StringContent content = new StringContent(serializedItem, Encoding.UTF8, "application/json"))
            {
                HttpResponseMessage response = null;
                response = await _Client.PutAsync(uri, content);
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    var result = await Task.Run(() => JsonConvert.DeserializeObject<TResult>(json));
                    return new WSResponse<TResult>(result);
                }
                return await Task.FromResult(new WSResponse<TResult>(new Msg(eMsgLevel.Failure, String.Format("{0},{1}", response.ReasonPhrase, response.StatusCode))));
            }
        }

        protected async Task<WSResponse<TResult>> PutAsync<TResult>(StringContent content, Uri uri)
        {
            HttpResponseMessage response = null;
            response = await _Client.PutAsync(uri, content);
            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                var result = await Task.Run(() => JsonConvert.DeserializeObject<TResult>(json));
                return new WSResponse<TResult>(result);
            }
            return await Task.FromResult(new WSResponse<TResult>(new Msg(eMsgLevel.Failure, String.Format("{0},{1}", response.ReasonPhrase, response.StatusCode))));
        }

        protected async Task<WSResponse<string>> PutAsync(StringContent content, Uri uri)
        {
            HttpResponseMessage response = null;
            response = await _Client.PutAsync(uri, content);
            if (response.IsSuccessStatusCode)
            {
                string result = await response.Content.ReadAsStringAsync();
                return new WSResponse<string>(result);
            }
            return await Task.FromResult(new WSResponse<string>(new Msg(eMsgLevel.Failure, String.Format("{0},{1}", response.ReasonPhrase, response.StatusCode))));
        }
        #endregion
        #endregion

        #region DELETE

        #region Sync
        public WSResponse<string> Delete(string relativeUri)
        {
            return Delete(GetUri(relativeUri));
        }

        public WSResponse<TResult> Delete<TResult>(string relativeUri)
        {
            return Delete<TResult>(GetUri(relativeUri));
        }

        public WSResponse<string> Delete(Uri uri)
        {
            if (Client == null)
                return new WSResponse<string>(null, new Msg(eMsgLevel.Error, "Disconnected"));
            try
            {
                Task<WSResponse<string>> task = DeleteAsync(uri);
                return task.Result;
            }
            catch (Exception ex)
            {
                var msg = new Msg(eMsgLevel.Exception, ex.Message);
                Messages.LogException(this.GetACUrl(), "Delete(30)", ex);
                OnNewAlarmOccurred("IsConnected", new Msg(eMsgLevel.Exception, ex.Message));
                if (ex is HttpRequestException)
                    IsConnected.ValueT = false;
                return new WSResponse<string>(msg);
            }
        }

        public WSResponse<TResult> Delete<TResult>(Uri uri)
        {
            if (Client == null)
                return new WSResponse<TResult>(default(TResult), new Msg(eMsgLevel.Error, "Disconnected"));
            try
            {
                Task<WSResponse<TResult>> task = DeleteAsync<TResult>(uri);
                return task.Result;
            }
            catch (Exception ex)
            {
                var msg = new Msg(eMsgLevel.Exception, ex.Message);
                Messages.LogException(this.GetACUrl(), "Delete(40)", ex);
                OnNewAlarmOccurred("IsConnected", new Msg(eMsgLevel.Exception, ex.Message));
                if (ex is HttpRequestException)
                    IsConnected.ValueT = false;
                return new WSResponse<TResult>(msg);
            }
        }
        #endregion

        #region Async
        public async Task<WSResponse<string>> DeleteAsync(string relativeUri)
        {
            return await DeleteAsync(GetUri(relativeUri));
        }

        public async Task<WSResponse<string>> DeleteAsync(Uri uri)
        {
            HttpClient client = Client;
            if (client == null)
                return await Task.FromResult(new WSResponse<string>(new Msg(eMsgLevel.Error, "Disconnected")));
            HttpResponseMessage response = null;
            response = await client.DeleteAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                IsConnected.ValueT = true;
                string result = await response.Content.ReadAsStringAsync();
                return new WSResponse<string>(result);
            }
            return await Task.FromResult(new WSResponse<string>(new Msg(eMsgLevel.Failure, String.Format("{0},{1}", response.ReasonPhrase, response.StatusCode))));
        }

        public async Task<WSResponse<TResult>> DeleteAsync<TResult>(string relativeUri)
        {
            return await DeleteAsync<TResult>(GetUri(relativeUri));
        }

        public async Task<WSResponse<TResult>> DeleteAsync<TResult>(Uri uri)
        {
            HttpClient client = Client;
            if (client == null)
                return await Task.FromResult(new WSResponse<TResult>(new Msg(eMsgLevel.Error, "Disconnected")));
            HttpResponseMessage response = null;
            response = await client.DeleteAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                IsConnected.ValueT = true;
                string json = await response.Content.ReadAsStringAsync();
                var result = await Task.Run(() => JsonConvert.DeserializeObject<TResult>(json));
                return new WSResponse<TResult>(result);
            }
            return await Task.FromResult(new WSResponse<TResult>(new Msg(eMsgLevel.Failure, String.Format("{0},{1}", response.ReasonPhrase, response.StatusCode))));
        }
        #endregion

        #endregion

        #endregion

        #endregion

    }
}
