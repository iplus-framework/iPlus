using gip.core.autocomponent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using CoreWCF;
using CoreWCF.Web;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.webservices
{
    public class WSRestAuthorizationManager : ServiceAuthorizationManager
    {
        protected override async ValueTask<bool> CheckAccessCoreAsync(OperationContext operationContext)
        {
            Guid? sessionId = CurrentSessionID;
            // Extract the Authorization header, and parse out the credentials converting the Base64 string:  
            string authHeader = WebOperationContext.Current.IncomingRequest.Headers["Authorization"];
            if (!String.IsNullOrEmpty(authHeader))
            {
                if (sessionId.HasValue)
                    return true;

                var svcCredentials = System.Text.ASCIIEncoding.ASCII
                    .GetString(Convert.FromBase64String(authHeader.Substring(6)))
                    .Split(':');
                var user = new
                {
                    Name = svcCredentials[0],
                    Password = svcCredentials[1]
                };

                ACStartUpRoot startUpRoot = new ACStartUpRoot(null);
                string errorMessage = "";
                bool loggedIn = await Task.Run(() => startUpRoot.CheckLogin(user.Name, user.Password, ref errorMessage)) != null;
                if (loggedIn)
                {
                    if (!sessionId.HasValue)
                        WebOperationContext.Current.OutgoingResponse.Headers[HttpResponseHeader.SetCookie] = CoreWebServiceConst.BuildSessionIdForCookieHeader(Guid.NewGuid());
                }
                return loggedIn;
            }
            else
            {
                // No authorization header was provided, so challenge the client to provide before proceeding:  
                WebOperationContext.Current.OutgoingResponse.Headers.Add("WWW-Authenticate: Basic realm=\"PAJsonServiceHost\"");
                // Throw an exception with the associated HTTP status code equivalent to HTTP status 401  
                throw new WebFaultException(HttpStatusCode.Unauthorized);
            }
        }

        public static Guid? CurrentSessionID
        {
            get
            {
                string cookieHeader = WebOperationContext.Current.IncomingRequest.Headers[HttpRequestHeader.Cookie];
                if (String.IsNullOrEmpty(cookieHeader))
                    cookieHeader = WebOperationContext.Current.OutgoingResponse.Headers[HttpResponseHeader.SetCookie];
                return CoreWebServiceConst.DecodeSessionIdFromCookieHeader(cookieHeader);
            }
        }
    }
}
