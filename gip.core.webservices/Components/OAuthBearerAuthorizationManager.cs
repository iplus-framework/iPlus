using System;
using System.Security.Principal;
using CoreWCF;
using CoreWCF.Channels;
using System.Threading;

namespace gip.core.webservices
{
    public class OAuthBearerAuthorizationManager : ServiceAuthorizationManager
    {
        private readonly Func<string, IPrincipal> _tokenValidator;

        public OAuthBearerAuthorizationManager(Func<string, IPrincipal> tokenValidator)
        {
            _tokenValidator = tokenValidator;
        }

        protected override bool CheckAccessCore(OperationContext operationContext)
        {
            if (operationContext == null || _tokenValidator == null)
                return false;

            HttpRequestMessageProperty httpRequest = operationContext.IncomingMessageProperties[HttpRequestMessageProperty.Name] as HttpRequestMessageProperty;
            if (httpRequest == null)
                return false;

            string authHeader = httpRequest.Headers["Authorization"];
            if (String.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return false;

            string token = authHeader.Substring("Bearer ".Length).Trim();
            if (String.IsNullOrWhiteSpace(token))
                return false;

            IPrincipal principal = _tokenValidator(token);
            if (principal == null || principal.Identity == null || !principal.Identity.IsAuthenticated)
                return false;

            Thread.CurrentPrincipal = principal;
            return true;
        }
    }

}
