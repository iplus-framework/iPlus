using gip.core.autocomponent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.webservices
{
    public class WSRestAuthorizationManager : ServiceAuthorizationManager
    {
        protected override bool CheckAccessCore(OperationContext operationContext)
        {
            Guid? sessionId = CurrentSessionID;
            //Extract the Authorization header, and parse out the credentials converting the Base64 string:  
            string authHeader = WebOperationContext.Current.IncomingRequest.Headers["Authorization"];

            if (!String.IsNullOrEmpty(authHeader))
            {
                if (sessionId.HasValue)
                    return true;

#if DEBUG
                try
                {
                    ACRoot.SRoot.Messages.LogDebug("WSRestAuthorizationManager", "CheckAccessCore()", String.Format("{0}, {1}, {2}", operationContext.Channel.LocalAddress.ToString(), operationContext.Host.Description.Name, authHeader));
                }
                catch
                {
                }
#endif
                var svcCredentials = System.Text.UTF8Encoding.UTF8
                    .GetString(Convert.FromBase64String(authHeader.Substring(6)))
                    .Split(':');
                var user = new
                {
                    Name = svcCredentials[0],
                    Password = svcCredentials[1]
                };


                ACStartUpRoot startUpRoot = new ACStartUpRoot();
                string errorMessage = "";
                bool loggedIn = startUpRoot.CheckLogin(user.Name, user.Password, ref errorMessage) != null;
                if (loggedIn)
                {
                    if (!sessionId.HasValue)
                        WebOperationContext.Current.OutgoingResponse.Headers[HttpResponseHeader.SetCookie] = CoreWebServiceConst.BuildSessionIdForCookieHeader(Guid.NewGuid());
                }
                return loggedIn;
            }
            else
            {
#if DEBUG
                try
                {
                    ACRoot.SRoot.Messages.LogDebug("WSRestAuthorizationManager", "CheckAccessCore()", String.Format("{0}, {1}, {2}", operationContext.Channel.LocalAddress.ToString(), operationContext.Host.Description.Name, "Authorization Header is empty"));
                }
                catch
                {
                }
#endif
                //No authorization header was provided, so challenge the client to provide before proceeding:  
                WebOperationContext.Current.OutgoingResponse.Headers.Add("WWW-Authenticate: Basic realm=\"PAJsonServiceHost\"");
                //Throw an exception with the associated HTTP status code equivalent to HTTP status 401  
                throw new WebFaultException(HttpStatusCode.Unauthorized);
            }
        }

        public static Guid? CurrentSessionID
        {
            get
            {
                // Prüfe zuerst HttpListener-Kontext (für nicht-WCF)
                if (HttpListenerWebContext.CurrentSessionID.HasValue)
                    return HttpListenerWebContext.CurrentSessionID;

                // Fallback auf WCF WebOperationContext
                if (WebOperationContext.Current == null)
                    return null;

                string cookieHeader = WebOperationContext.Current.IncomingRequest.Headers[HttpRequestHeader.Cookie];
                if (String.IsNullOrEmpty(cookieHeader))
                    cookieHeader = WebOperationContext.Current.OutgoingResponse.Headers[HttpResponseHeader.SetCookie];
                return CoreWebServiceConst.DecodeSessionIdFromCookieHeader(cookieHeader);
            }
        }

        public static int? ServicePort
        {
            get
            {
                int? servicePort = null;
                // Prüfe zuerst HttpListener-Kontext (für nicht-WCF)
                if (HttpListenerWebContext.CurrentServicePort.HasValue)
                    servicePort = HttpListenerWebContext.CurrentServicePort;
                else
                    // Fallback auf WCF WebOperationContext
                    servicePort = WebOperationContext.Current?.IncomingRequest?.UriTemplateMatch?.BaseUri.Port;
                // If Port forwarding activated and (URL in the mobile application contains a portnumber > 10000 e.g. x.x.x.x:18730 
                if (servicePort.HasValue && servicePort >= 10000)
                    servicePort -= 10000;
                return servicePort;
            }
        }
    }
}
