// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using gip.core.autocomponent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using CoreWCF;
using CoreWCF.Web;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Text.Encodings.Web;
using System.Security.Claims;

namespace gip.core.webservices
{
    public class ApiKeyAttribute : ServiceFilterAttribute
    {
        public ApiKeyAttribute()
            : base(typeof(ApiKeyAuthorizationFilter))
        {
        }
    }

    public class ApiKeyAuthorizationFilter : IAuthorizationFilter
    {
        public const string ApiKeyHeaderName = "Authorization";
        public const char CredentialSeparator = '#';

        private readonly IApiKeyValidator _apiKeyValidator;

        public ApiKeyAuthorizationFilter(IApiKeyValidator apiKeyValidator)
        {
            _apiKeyValidator = apiKeyValidator;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            string apiKey = context.HttpContext.Request.Headers[ApiKeyHeaderName];
            if (!_apiKeyValidator.IsValid(apiKey, context.HttpContext))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Register cleanup when request completes
            context.HttpContext.Response.OnCompleted(() =>
            {
                CleanupSession(context.HttpContext);
                return Task.CompletedTask;
            });
        }

        private static void CleanupSession(HttpContext context)
        {
            try
            {
                if (context.Items.TryGetValue(VBUserContextService.SessionIDKey, out object sessionId) && sessionId is Guid guid)
                {
                    var serviceHost = PAWebServiceBase.FindPAWebService<PAMcpServerHost>(context.Connection.LocalPort);
                    if (serviceHost != null)
                        serviceHost.RemoveSession(guid);
                }
            }
            catch (Exception ex)
            {
                ACRoot.SRoot.Messages.LogException("ApiKeyAuthorizationFilter", "CleanupSession", ex);
            }
        }
    }

    public class ApiKeyValidator : IApiKeyValidator
    {
        public bool IsValid(string apiKey, HttpContext context)
        {
            PAMcpServerHost myServiceHost = PAWebServiceBase.FindPAWebService<PAMcpServerHost>(context.Connection.LocalPort);
            if (myServiceHost == null)
                return false;
            try
            {
                var svcCredentials = apiKey.Split(ApiKeyAuthorizationFilter.CredentialSeparator);
                var user = new
                {
                    Name = svcCredentials[0],
                    Password = svcCredentials[1]
                };

                ACStartUpRoot startUpRoot = new ACStartUpRoot(null);
                string errorMessage = "";
                gip.core.datamodel.VBUser vbUser = startUpRoot.CheckLogin(user.Name, user.Password, ref errorMessage);
                if (vbUser != null)
                {
                    // Create a unique session ID for this request
                    Guid sessionId = Guid.NewGuid();
                    myServiceHost.AddSession(new VBUserRights()
                    {
                        UserName = user.Name,
                        SessionID = sessionId,
                        VBUser = vbUser
                    });
                    context.Items[VBUserContextService.SessionIDKey] = sessionId;
                    return true;
                }
            }
            catch (Exception ex)
            {
                ACRoot.SRoot.Messages.LogException("ApiKeyValidator", "IsValid", ex);
            }
            return false;

        }


        public static int? ServicePort
        {
            get
            {
                return WebOperationContext.Current?.IncomingRequest?.UriTemplateMatch?.BaseUri.Port;
            }
        }
    }

    public interface IApiKeyValidator
    {
        bool IsValid(string apiKey, HttpContext context);
    }


    public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IApiKeyValidator _apiKeyValidator;
        private readonly TimeProvider _timeProvider;

        public ApiKeyAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger, UrlEncoder encoder, TimeProvider timeProvider,
            IApiKeyValidator apiKeyValidator)
            : base(options, logger, encoder)
        {
            _apiKeyValidator = apiKeyValidator;
            _timeProvider = timeProvider;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string apiKey = Context.Request.Headers[ApiKeyAuthorizationFilter.ApiKeyHeaderName];

            if (string.IsNullOrEmpty(apiKey))
            {
                return Task.FromResult(AuthenticateResult.Fail("Missing API Key"));
            }

            if (!_apiKeyValidator.IsValid(apiKey, Context))
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));
            }

            // Register cleanup when request completes
            Context.Response.OnCompleted(() =>
            {
                CleanupSession(Context);
                return Task.CompletedTask;
            });

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "API User"),
                new Claim(ClaimTypes.NameIdentifier, apiKey)
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        private static void CleanupSession(HttpContext context)
        {
            try
            {
                if (context.Items.TryGetValue(VBUserContextService.SessionIDKey, out object sessionId) && sessionId is Guid guid)
                {
                    var serviceHost = PAWebServiceBase.FindPAWebService<PAMcpServerHost>(context.Connection.LocalPort);
                    if (serviceHost != null)
                        serviceHost.RemoveSession(guid);
                }
            }
            catch (Exception ex)
            {
                ACRoot.SRoot.Messages.LogException("ApiKeyAuthenticationHandler", "CleanupSession", ex);
            }
        }
    }

    public interface IVBUserContextService
    {
        Guid? GetSessionID();
    }

    public class VBUserContextService : IVBUserContextService
    {
        public const string SessionIDKey = "VBUserSessionID";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly PAMcpServerHost _mcpServerHost;

        public VBUserContextService(IHttpContextAccessor httpContextAccessor, PAMcpServerHost mcpServerHost)
        {
            _httpContextAccessor = httpContextAccessor;
            _mcpServerHost = mcpServerHost;
        }

        public Guid? GetSessionID()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context?.Items.TryGetValue(SessionIDKey, out object sessionId) == true)
            {
                return (Guid)sessionId;
            }
            return null;
        }
    }

}
