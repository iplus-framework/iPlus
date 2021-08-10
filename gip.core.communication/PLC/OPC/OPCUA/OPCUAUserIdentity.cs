using System;
using System.Collections.Generic;
using Opc.Ua;
using Opc.Ua.Server;
using System.Security.Cryptography.X509Certificates;
using gip.core.datamodel;
using System.Collections.Concurrent;

namespace gip.core.communication
{
    public class OPCUAUserIdentity : UserIdentity
    {
        public OPCUAUserIdentity() : base()
        {
        }

        public OPCUAUserIdentity(IssuedIdentityToken issuedToken, VBUser vbUser) : base(issuedToken)
        {
            _VBUser = vbUser;
        }

        public OPCUAUserIdentity(CertificateIdentifier certificateId, VBUser vbUser) : base(certificateId)
        {
            _VBUser = vbUser;
        }

        public OPCUAUserIdentity(X509Certificate2 certificate, VBUser vbUser) : base(certificate)
        {
            _VBUser = vbUser;
        }

        public OPCUAUserIdentity(UserIdentityToken token, VBUser vbUser) : base(token)
        {
            _VBUser = vbUser;
        }

        public OPCUAUserIdentity(string username, string password, VBUser vbUser) : base(username, password)
        {
            _VBUser = vbUser;
        }

        private VBUser _VBUser = null;
        public VBUser VBUser
        {
            get
            {
                return _VBUser;
            }
        }
    }
}
