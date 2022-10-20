using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Opc.Ua;
using Opc.Ua.Client;
using System.Security.Cryptography.X509Certificates;


namespace gip.core.communication
{
    public class OPCUAClientSession : Session
    {
        #region c'tors

        public OPCUAClientSession(ITransportChannel channel, ApplicationConfiguration configuration, ConfiguredEndpoint endpoint, X509Certificate2 certificate) 
            : base(channel, configuration, endpoint, certificate)
        {

        }
    
        #endregion

        #region Methods

        public static OPCUAClientSession CreateSession(ApplicationConfiguration configuration, ConfiguredEndpoint endpoint, bool updateBeforeConnect, bool checkDomain)
        {
            endpoint.UpdateBeforeConnect = updateBeforeConnect;

            EndpointDescription endpointDescription = endpoint.Description;

            // create the endpoint configuration (use the application configuration to provide default values).
            EndpointConfiguration endpointConfiguration = endpoint.Configuration;

            if (endpointConfiguration == null)
            {
                endpoint.Configuration = endpointConfiguration = EndpointConfiguration.Create(configuration);
            }

            // create message context.
            ServiceMessageContext messageContext = configuration.CreateMessageContext();

            // update endpoint description using the discovery endpoint.
            if (endpoint.UpdateBeforeConnect)
            {
                endpoint.UpdateFromServer();

                endpointDescription = endpoint.Description;
                endpointConfiguration = endpoint.Configuration;
            }

            // checks the domains in the certificate.
            if (checkDomain && endpoint.Description.ServerCertificate != null && endpoint.Description.ServerCertificate.Length > 0)
            {
                CheckCertificateDomain(endpoint);
            }

            X509Certificate2 clientCertificate = null;
            X509Certificate2Collection clientCertificateChain = null;

            if (endpointDescription.SecurityPolicyUri != SecurityPolicies.None)
            {
                if (configuration.SecurityConfiguration.ApplicationCertificate == null)
                {
                    throw ServiceResultException.Create(StatusCodes.BadConfigurationError, "ApplicationCertificate must be specified.");
                }

                clientCertificate =  configuration.SecurityConfiguration.ApplicationCertificate.Find(true).Result;

                if (clientCertificate == null)
                {
                    throw ServiceResultException.Create(StatusCodes.BadConfigurationError, "ApplicationCertificate cannot be found.");
                }

                // load certificate chain.
                if (configuration.SecurityConfiguration.SendCertificateChain)
                {
                    clientCertificateChain = new X509Certificate2Collection(clientCertificate);
                    List<CertificateIdentifier> issuers = new List<CertificateIdentifier>();
                    configuration.CertificateValidator.GetIssuers(clientCertificate, issuers);

                    for (int i = 0; i < issuers.Count; i++)
                    {
                        clientCertificateChain.Add(issuers[i].Certificate);
                    }
                }
            }

            // initialize the channel which will be created with the server.
            ITransportChannel channel = SessionChannel.Create(
                 configuration,
                 endpointDescription,
                 endpointConfiguration,
                 clientCertificate,
                 clientCertificateChain,
                 messageContext);

            return new OPCUAClientSession(channel, configuration, endpoint, null);
        }

        private static void CheckCertificateDomain(ConfiguredEndpoint endpoint)
        {
            bool domainFound = false;

            X509Certificate2 serverCertificate = new X509Certificate2(endpoint.Description.ServerCertificate);

            // check the certificate domains.
            IList<string> domains = X509Utils.GetDomainsFromCertficate(serverCertificate);

            if (domains != null)
            {
                string hostname;
                string dnsHostName = hostname = endpoint.EndpointUrl.DnsSafeHost;
                bool isLocalHost = false;
                if (endpoint.EndpointUrl.HostNameType == UriHostNameType.Dns)
                {
                    if (dnsHostName.ToLowerInvariant() == "localhost")
                    {
                        isLocalHost = true;
                    }
                    else
                    {   // strip domain names from hostname
                        hostname = dnsHostName.Split('.')[0];
                    }
                }
                else
                {   // dnsHostname is a IPv4 or IPv6 address
                    // normalize ip addresses, cert parser returns normalized addresses
                    hostname = Utils.NormalizedIPAddress(dnsHostName);
                    if (hostname == "127.0.0.1" || hostname == "::1")
                    {
                        isLocalHost = true;
                    }
                }

                if (isLocalHost)
                {
                    dnsHostName = Utils.GetFullQualifiedDomainName();
                    hostname = Utils.GetHostName();
                }

                for (int ii = 0; ii < domains.Count; ii++)
                {
                    if (String.Compare(hostname, domains[ii], StringComparison.OrdinalIgnoreCase) == 0 ||
                        String.Compare(dnsHostName, domains[ii], StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        domainFound = true;
                        break;
                    }
                }
            }

            if (!domainFound)
            {
                string message = Utils.Format(
                    "The domain '{0}' is not listed in the server certificate.",
                    endpoint.EndpointUrl.DnsSafeHost);
                throw new ServiceResultException(StatusCodes.BadCertificateHostNameInvalid, message);
            }
        }
        #endregion
    }

    //public enum OPCUserAuthenticationMode : short
    //{
    //    Anonymous = 0,
    //    Username = 10,
    //    Certificate = 20
    //}
}
