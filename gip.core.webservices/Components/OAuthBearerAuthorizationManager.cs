using System;
using System.Security.Principal;
using CoreWCF;
using CoreWCF.Channels;
using System.Threading;
using System.Threading.Tasks;

namespace gip.core.webservices
{
    public class OAuthBearerAuthorizationManager : ServiceAuthorizationManager
    {
        private readonly Func<string, IPrincipal> _tokenValidator;

        public OAuthBearerAuthorizationManager(Func<string, IPrincipal> tokenValidator)
        {
            _tokenValidator = tokenValidator;
        }

        protected override ValueTask<bool> CheckAccessCoreAsync(OperationContext operationContext)
        {
            if (operationContext == null || _tokenValidator == null)
                return new ValueTask<bool>(false);

            HttpRequestMessageProperty httpRequest = operationContext.IncomingMessageProperties[HttpRequestMessageProperty.Name] as HttpRequestMessageProperty;
            if (httpRequest == null)
                return new ValueTask<bool>(false);

            string authHeader = httpRequest.Headers["Authorization"];
            if (String.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return new ValueTask<bool>(false);

            string token = authHeader.Substring("Bearer ".Length).Trim();
            if (String.IsNullOrWhiteSpace(token))
                return new ValueTask<bool>(false);

            IPrincipal principal = _tokenValidator(token);
            if (principal == null || principal.Identity == null || !principal.Identity.IsAuthenticated)
                return new ValueTask<bool>(false);

            Thread.CurrentPrincipal = principal;
            return new ValueTask<bool>(true);
        }
    }

}
