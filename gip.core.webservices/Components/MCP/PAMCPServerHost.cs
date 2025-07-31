// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using gip.core.autocomponent;
using gip.core.datamodel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Isam.Esent.Interop;
using ModelContextProtocol.Server;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace gip.core.webservices
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'MCP Server Host'}de{'MCP Server Host'}", Global.ACKinds.TPABGModule, Global.ACStorableTypes.Required, false, false)]
    public class PAMcpServerHost : PAWebServiceBase
    {
        #region c'tors
        public PAMcpServerHost(gip.core.datamodel.ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _IsEnabled = new ACPropertyConfigValue<bool>(this, "IsEnabled", false);
            // Command to enable http-Service
            // >netsh http add urlacl url=http://+:ServicePort/ user="\Everyone"
            // https://nodejs.org/en/download

            // Configuration for Claude Desktop
            // https://www.npmjs.com/package/mcp-remote

        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            bool result = base.ACInit(startChildMode);
            _ = IsEnabled;
            _ = ServicePort;
            return result;
        }

        public override bool ACPostInit()
        {
            StartService();
            return base.ACPostInit();
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            StopService();
            bool result = base.ACDeInit(deleteACClassTask);
            return result;
        }
        #endregion

        #region Properties

        private ACPropertyConfigValue<bool> _IsEnabled;
        [ACPropertyConfig("en{'Enable MCP Server'}de{'MCP Server aktivieren'}")]
        public bool IsEnabled
        {
            get => _IsEnabled.ValueT;
            set => _IsEnabled.ValueT = value;
        }


        [ACPropertyBindingSource(200, "Error", "en{'MCP Server Alarm'}de{'MCP Server Alarm'}", "", false, false)]
        public IACContainerTNet<PANotifyState> IsMcpServerAlarm { get; set; }

        public override Type ServiceType => null;

        public override Type ServiceInterfaceType => null;

        private WebApplication _McpHost = null;
        //private IHost _McpHost = null;
        private CancellationTokenSource _CancellationTokenSource = null;
        private Task _HostTask = null;

        #endregion

        #region Methods

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            //switch (acMethodName)
            //{
            //    case nameof(StartMcpServer):
            //        StartMcpServer();
            //        return true;
            //    case nameof(IsEnabledStartMcpServer):
            //        result = IsEnabledStartMcpServer();
            //        return true;
            //    case nameof(StopMcpServer):
            //        StopMcpServer();
            //        return true;
            //    case nameof(IsEnabledStopMcpServer):
            //        result = IsEnabledStopMcpServer();
            //        return true;
            //}
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
        #endregion

        [ACMethodInteraction("MCP", "en{'Start MCP Server'}de{'MCP Server starten'}", 200, true)]
        public override void StartService()
        {
            if (!IsEnabledStartService())
                return;

            StopService();

            if (this.Root.RootPageWPF != null)
                this.Root.RootPageWPF.SuppressOpenMessageBoxes = true;
            try
            {
                //var builder = Host.CreateApplicationBuilder();
                //builder.Services.AddMcpServer()
                //    .WithStdioServerTransport()
                //    .WithTools<MCPIPlusTools>();
                
                var builder = WebApplication.CreateBuilder();

                // Add authentication and authorization
                builder.Services.AddAuthentication()
                    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKey", options => { });

                builder.Services.AddAuthorization(options =>
                {
                    options.AddPolicy("ApiKeyPolicy", policy =>
                    {
                        policy.AuthenticationSchemes.Add("ApiKey");
                        policy.RequireAuthenticatedUser();
                    });
                });

                builder.Services.AddMcpServer()
                    .WithHttpTransport()
                    .WithTools<MCPIPlusTools>()
                    .WithPrompts<MCPIPlusPrompts>();

                builder.Logging.AddConsole(options =>
                {
                    options.LogToStandardErrorThreshold = LogLevel.Information;
                });

                // Register the ACRoot as a singleton service so tools can access it
                builder.Services.AddSingleton<IACComponent>(provider => this);
                builder.Services.AddSingleton<ApiKeyAuthorizationFilter>();
                builder.Services.AddSingleton<IApiKeyValidator, ApiKeyValidator>();
                builder.Services.AddSingleton<PAMcpServerHost>(this);
                builder.Services.AddScoped<IVBUserContextService, VBUserContextService>();

                _McpHost = builder.Build();
                _McpHost.UseAuthentication();
                _McpHost.UseAuthorization();

                (_McpHost as WebApplication).MapMcp().RequireAuthorization("ApiKeyPolicy");

                _CancellationTokenSource = new CancellationTokenSource();

                _HostTask = Task.Run(async () =>
                {
                    try
                    {
                        string strUri = String.Format("http://{0}:{1}", this.Root.Environment.UserInstance.Hostname, ServicePort);
                        //await _McpHost.RunAsync(_CancellationTokenSource.Token);
                        await _McpHost.RunAsync(strUri);
                    }
                    catch (OperationCanceledException)
                    {
                        // Expected when cancellation is requested
                    }
                    catch (Exception ex)
                    {
                        Messages.LogException(GetACUrl(), "MCP Host Error", ex);
                        IsMcpServerAlarm.ValueT = PANotifyState.AlarmOrFault;
                        ErrorText.ValueT = ex.Message;
                        OnNewAlarmOccurred(IsMcpServerAlarm, "MCP Host Error", true);
                    }
                });

                Messages.LogInfo(GetACUrl(), "StartMcpServer", "MCP Server started successfully with prompts");
            }
            catch (Exception ex)
            {
                Messages.LogException(GetACUrl(), "StartMcpServer", ex);
                IsMcpServerAlarm.ValueT = PANotifyState.AlarmOrFault;
                ErrorText.ValueT = ex.Message;
                OnNewAlarmOccurred(IsMcpServerAlarm, "StartMcpServer Error", true);
            }
        }

        public override bool IsEnabledStartService()
        {
            return _McpHost == null && IsEnabled && ServicePort > 0;
        }

        [ACMethodInteraction("MCP", "en{'Stop MCP Server'}de{'MCP Server stoppen'}", 201, true)]
        public override void StopService()
        {
            if (!IsEnabledStopService())
                return;

            try
            {
                _CancellationTokenSource?.Cancel();
                _HostTask?.Wait(TimeSpan.FromSeconds(10));
                //_McpHost?.Dispose();
                _McpHost = null;
                _CancellationTokenSource?.Dispose();
                _CancellationTokenSource = null;
                _HostTask = null;

                Messages.LogInfo(GetACUrl(), "StopMcpServer", "MCP Server stopped successfully");
            }
            catch (Exception ex)
            {
                Messages.LogException(GetACUrl(), "StopMcpServer", ex);
            }
        }

        public override bool IsEnabledStopService()
        {
            return _McpHost != null;
        }

        public override void AcknowledgeAlarms()
        {
            if (!IsEnabledAcknowledgeAlarms())
                return;
            if (IsMcpServerAlarm.ValueT == PANotifyState.AlarmOrFault)
            {
                IsMcpServerAlarm.ValueT = PANotifyState.Off;
                OnAlarmDisappeared(IsMcpServerAlarm);
            }
            base.AcknowledgeAlarms();
        }


        public VBUserRights ResolveUserForSession(IMcpServer server)
        {
            var userContextService = server.Services?.GetService(typeof(IVBUserContextService)) as VBUserContextService;
            if (userContextService == null)
                return null;
            Guid? currentSessionID = userContextService.GetSessionID();
            if (!currentSessionID.HasValue)
                return null;
            return GetRightsForSession(currentSessionID.Value);
        }

        private ConcurrentDictionary<Guid, VBUserRights> _Sessions = new ConcurrentDictionary<Guid, VBUserRights>();

        public void AddSession(VBUserRights vbUserRights)
        {
            if (vbUserRights.SessionID.HasValue)
                _Sessions.TryAdd(vbUserRights.SessionID.Value, vbUserRights);
        }

        public bool RemoveSession(Guid sessionId)
        {
            VBUserRights vBUserRights;
            return _Sessions.TryRemove(sessionId, out vBUserRights);
        }

        public VBUserRights GetRightsForSession(Guid sessionId)
        {
            VBUserRights vBUserRights;
            _Sessions.TryGetValue(sessionId, out vBUserRights);
            return vBUserRights;
        }

        public override IWebHost CreateService()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}