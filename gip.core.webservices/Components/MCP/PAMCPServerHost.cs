// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using gip.core.autocomponent;
using gip.core.datamodel;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Isam.Esent.Interop;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace gip.core.webservices
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'MCP Server Host'}de{'MCP Server Host'}", Global.ACKinds.TPABGModule, Global.ACStorableTypes.Required, false, false)]
    public class PAMcpServerHost : PAClassAlarmingBase
    {
        #region c'tors
        public PAMcpServerHost(gip.core.datamodel.ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _IsEnabled = new ACPropertyConfigValue<bool>(this, "IsEnabled", false);
            _ServicePort = new ACPropertyConfigValue<int>(this, "ServicePort", 0);
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
            StartMcpServer();
            return base.ACPostInit();
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            StopMcpServer();
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

        private ACPropertyConfigValue<int> _ServicePort;
        [ACPropertyConfig("en{'Service Port number'}de{'Portnummer des Services'}")]
        public int ServicePort
        {
            get => _ServicePort.ValueT;
            set => _ServicePort.ValueT = value;
        }

        [ACPropertyBindingSource(200, "Error", "en{'MCP Server Alarm'}de{'MCP Server Alarm'}", "", false, false)]
        public IACContainerTNet<PANotifyState> IsMcpServerAlarm { get; set; }

        [ACPropertyBindingSource(201, "Error", "en{'Error Text'}de{'Fehlertext'}", "", true, false)]
        public IACContainerTNet<String> ErrorText { get; set; }

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
            switch (acMethodName)
            {
                case nameof(StartMcpServer):
                    StartMcpServer();
                    return true;
                case nameof(IsEnabledStartMcpServer):
                    result = IsEnabledStartMcpServer();
                    return true;
                case nameof(StopMcpServer):
                    StopMcpServer();
                    return true;
                case nameof(IsEnabledStopMcpServer):
                    result = IsEnabledStopMcpServer();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
        #endregion

        [ACMethodInteraction("MCP", "en{'Start MCP Server'}de{'MCP Server starten'}", 200, true)]
        public virtual void StartMcpServer()
        {
            if (!IsEnabledStartMcpServer())
                return;

            StopMcpServer();

            if (this.Root.RootPageWPF != null)
                this.Root.RootPageWPF.SuppressOpenMessageBoxes = true;
            try
            {
                //var builder = Host.CreateApplicationBuilder();
                //builder.Services.AddMcpServer()
                //    .WithStdioServerTransport()
                //    .WithTools<MCPIPlusTools>();
                
                var builder = WebApplication.CreateBuilder();
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

                _McpHost = builder.Build();
                (_McpHost as WebApplication).MapMcp();

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

        public bool IsEnabledStartMcpServer()
        {
            return _McpHost == null && IsEnabled && ServicePort > 0;
        }

        [ACMethodInteraction("MCP", "en{'Stop MCP Server'}de{'MCP Server stoppen'}", 201, true)]
        public void StopMcpServer()
        {
            if (!IsEnabledStopMcpServer())
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

        public bool IsEnabledStopMcpServer()
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

        #endregion
    }
}