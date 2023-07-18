using gip.core.autocomponent;
using gip.core.datamodel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CoreWCF;
using CoreWCF.Channels;
using CoreWCF.Description;
using CoreWCF.Dispatcher;
using CoreWCF.Web;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;
using System.Diagnostics;
using CoreWCF.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using System.Reflection;
using System.Security.Policy;

namespace gip.core.webservices
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Json Host iPlus'}de{'Json Host iPlus'}", Global.ACKinds.TPABGModule, Global.ACStorableTypes.Required, false, false)]
    public class PAJsonServiceHost : PAWebServiceBase
    {
        #region c´tors
        public PAJsonServiceHost(gip.core.datamodel.ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }
        #endregion


        #region Implementation

        public override IWebHost CreateService()
        {
            // Kommando um http-Service bei Windows freizuschalten
            // >netsh http add urlacl url=http://+:8730/ user="\Everyone"

            // Kommando um http-Service bei Windows zu deaktiveren
            // >netsh http delete urlacl url=http://+:8730/

            // Kommando um alle Einträge anzuzeigen:
            // >netsh http show urlacl

            Host = WebHost.CreateDefaultBuilder()
                .UseKestrel()
                .ConfigureServices(services =>
                {
                    services.AddServiceModelServices()
                    .AddServiceModelMetadata();
                })
                .Configure(app =>
                {
                    app.UseServiceModel(builder =>
                    {
                        int servicePort = ServicePort;
                        if (servicePort <= 0)
                        {
                            servicePort = 8730;
                            ServicePort = servicePort;
                        }

                        string strUri = String.Format("http://{0}:{1}/", this.Root.Environment.UserInstance.Hostname, servicePort);
                        Uri uri = new Uri(strUri);
                        //builder.Authorization.ServiceAuthorizationManager = new WSRestAuthorizationManager();
                        WebHttpBinding httpBinding = new WebHttpBinding() { ContentTypeMapper = new WSJsonServiceContentTypeMapper() };
                        //Can't enable AllowCookies in httpBinding, not updated in CoreWCF; AllowCookies = true
                        httpBinding.MaxReceivedMessageSize = int.MaxValue;
                        httpBinding.ReaderQuotas.MaxStringContentLength = 1000000;
                        httpBinding.MaxBufferSize = int.MaxValue;
                        httpBinding.MaxBufferPoolSize = int.MaxValue;
                        builder.AddService(ServiceType);

                        MethodInfo addServiceEndpointMethod = typeof(IServiceBuilder)
                            .GetMethods()
                            .FirstOrDefault(m =>
                                m.Name == "AddServiceEndpoint" &&
                                m.GetParameters().Length == 3 &&
                                m.GetParameters()[0].ParameterType == typeof(Type) &&
                                m.GetParameters()[1].ParameterType == typeof(Binding) &&
                                m.GetParameters()[2].ParameterType == typeof(Uri))
                            ?.MakeGenericMethod(ServiceType);

                        var serviceEndpoint = addServiceEndpointMethod.Invoke(builder, new object[] { ServiceInterfaceType, httpBinding, uri });

                        builder.ConfigureServiceHostBase<CoreWebService>(serviceHostBase =>
                        {
                            serviceHostBase.Description.Behaviors.Add(new ServiceAuthorizationBehavior { ServiceAuthorizationManager = new WSRestAuthorizationManager() });
                            if (serviceEndpoint != null)
                            {
                                foreach (var endpoint in serviceHostBase.Description.Endpoints)
                                {
                                    endpoint.EndpointBehaviors.Add(new PAWebServiceBaseErrorBehavior(this.GetACUrl()));
                                }
                            }

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
                })
                .Build();

            return Host;
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

        public override object GetWebServiceInstance()
        {
            return new CoreWebService();
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
