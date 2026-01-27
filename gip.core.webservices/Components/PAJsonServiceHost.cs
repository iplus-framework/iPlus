// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using CoreWCF;
using CoreWCF.Channels;
using CoreWCF.Configuration;
using CoreWCF.Description;
using CoreWCF.Dispatcher;
using CoreWCF.Web;
﻿using gip.core.autocomponent;
using gip.core.datamodel;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace gip.core.webservices
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Json Host iPlus'}de{'Json Host iPlus'}", Global.ACKinds.TPABGModule, Global.ACStorableTypes.Required, false, false)]
    public partial class PAJsonServiceHost : PAWebServiceBase
    {
        #region c´tors
        public PAJsonServiceHost(gip.core.datamodel.ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _UseCustomHttpListener = new ACPropertyConfigValue<bool>(this, nameof(UseCustomHttpListener), false);
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (UseCustomHttpListener)
                InitPAJsonServiceHostListener();
            return base.ACInit(startChildMode);
        }
        #endregion

        #region Config
        private ACPropertyConfigValue<bool> _UseCustomHttpListener;
        [ACPropertyConfig("en{'Use Custom http listener'}de{'Verwende eigenen http listener'}")]
        public bool UseCustomHttpListener
        {
            get => _UseCustomHttpListener.ValueT;
            set => _UseCustomHttpListener.ValueT = value;
        }
        #endregion

        #region Implementation

        public override IHost CreateService()
        {
            if (UseCustomHttpListener)
                return CreateHttpListenerService();
            else
                return CreateWCFHttpService();
        }

        protected IHost CreateWCFHttpService()
        {
            // Kommando um http-Service bei Windows freizuschalten
            // >netsh http add urlacl url=http://+:8730/ user="\Everyone"

            // Kommando um http-Service bei Windows zu deaktiveren
            // >netsh http delete urlacl url=http://+:8730/

            // Kommando um alle Einträge anzuzeigen:
            // >netsh http show urlacl

            var builder = WebApplication.CreateBuilder();
            
            int servicePort = ServicePort;
            if (servicePort <= 0)
            {
                servicePort = 8730;
                ServicePort = servicePort;
            }

            builder.WebHost.UseKestrel(options =>
            {
                options.ListenAnyIP(servicePort);
                //options.ListenAnyIP(8731, listenOptions =>
                //{
                //    listenOptions.UseHttps();
                //});
            });

            builder.Services.AddServiceModelWebServices(o =>
            {
                o.Title = "Mobile Service API";
                o.Version = "1";
                o.Description = "API Description";
                o.TermsOfService = new("https://github.com/iplus-framework/iPlus");
                o.ContactName = "Contact";
                o.ContactEmail = "info@iplus-framework.com";
                //o.ContactUrl = new("http://example.com/contact");
                //o.ExternalDocumentUrl = new("http://example.com/doc.pdf");
                //o.ExternalDocumentDescription = "Documentation";
            });

            builder.Services.AddSingleton(new SwaggerOptions());

            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();
            ((IApplicationBuilder)app).UseServiceModel(serviceBuilder =>
            {
                string strUri = String.Format("http://{0}:{1}/", this.Root.Environment.UserInstance.Hostname, servicePort);
                Uri uri = new Uri(strUri);
                WebHttpBinding httpBinding = new WebHttpBinding() { ContentTypeMapper = GetContentTypeMapper() };
                httpBinding.MaxReceivedMessageSize = int.MaxValue;
                httpBinding.ReaderQuotas.MaxStringContentLength = 1000000;
                httpBinding.MaxBufferSize = int.MaxValue;
                httpBinding.MaxBufferPoolSize = int.MaxValue;
                serviceBuilder.AddService(ServiceType);
                serviceBuilder.AddServiceWebEndpoint(ServiceType, ServiceInterfaceType, httpBinding, uri, null);

                serviceBuilder.ConfigureAllServiceHostBase((serviceHostBase) =>
                {
                    // Set the authorization manager
                    serviceHostBase.Authorization.ServiceAuthorizationManager = new WSRestAuthorizationManager();

                    ServiceMetadataBehavior metad = serviceHostBase.Description.Behaviors.Find<ServiceMetadataBehavior>();
                    if (metad == null)
                    {
                        metad = new ServiceMetadataBehavior();
                        serviceHostBase.Description.Behaviors.Add(metad);
                    }
                    metad.HttpGetEnabled = true;
                    foreach (CoreWCF.Description.ServiceEndpoint endpoint in serviceHostBase.Description.Endpoints)
                    {
                        foreach (OperationDescription opDescr in endpoint.Contract.Operations)
                        {
                            OnAddKnownTypesToOperationContract(endpoint, opDescr);
                        }
                    }
                });
            });

            return app;
        }

        public virtual WebContentTypeMapper GetContentTypeMapper()
        {
            return new WSJsonServiceContentTypeMapper();
        }

        public override Type ServiceType
        {
            get
            {
                return typeof(CoreWebService);
            }
        }

        public override Type ServiceInterfaceType
        {
            get
            {
                return typeof(ICoreWebService);
            }
        }

        private CoreWebService _serviceInstance;
        public override object GetWebServiceInstance()
        {
            if (_serviceInstance == null)
                _serviceInstance = new CoreWebService();
            return _serviceInstance;
        }

        private ConcurrentDictionary<Guid, VBUserRights> _Sessions = new ConcurrentDictionary<Guid, VBUserRights>();

        public void AddSession(VBUserRights vbUserRights)
        {
            if (vbUserRights.SessionID.HasValue)
                _Sessions.TryAdd(vbUserRights.SessionID.Value, vbUserRights);
        }

        public bool RemoveSession(Guid guid)
        {
            VBUserRights vBUserRights;
            return _Sessions.TryRemove(guid, out vBUserRights);
        }

        public VBUserRights GetRightsForSession(Guid guid)
        {
            VBUserRights vBUserRights;
            _Sessions.TryGetValue(guid, out vBUserRights);
            return vBUserRights;
        }

        #endregion

    }

    public class WSJsonServiceContentTypeMapper : WebContentTypeMapper
    {
        public override WebContentFormat GetMessageFormatForContentType(string contentType)
        {
            if (contentType == "text/javascript"
                || contentType == "text/plain"
                || contentType == "application/x-www-form-urlencoded")
            {
                return WebContentFormat.Json;
            }
            else
            {
                return WebContentFormat.Default;
            }
        }
    }
}
