using gip.core.autocomponent;
using gip.core.datamodel;
using System;
using System.Security.Principal;

namespace gip.core.webservices
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'DemoOAuthTokenValidator'}de{'DemoOAuthTokenValidator'}", Global.ACKinds.TPABGModule, Global.ACStorableTypes.NotStorable, true, false)]
    public class DemoOAuthTokenValidator :  PAClassAlarmingBase, IOAuthTokenValidator
    {
        #region 

        public DemoOAuthTokenValidator(gip.core.datamodel.ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
        : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _BearerToken = new ACPropertyConfigValue<string>(this, nameof(BearerToken), "demo-token");
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            bool baseACInit =  base.ACInit(startChildMode);
            _ = BearerToken;
            return baseACInit;
        }

        #endregion

        #region Config
        private ACPropertyConfigValue<string> _BearerToken;
        [ACPropertyConfig("en{'Use Custom http listener'}de{'Verwende eigenen http listener'}")]
        public string BearerToken
        {
            get => _BearerToken.ValueT;
            set => _BearerToken.ValueT = value;
        }
        #endregion

        #region Methods

        public IPrincipal ValidatePrincipalFromBearerToken(string accessToken)
        {
            if (!String.Equals(accessToken, BearerToken, StringComparison.Ordinal))
                return null;

            GenericIdentity identity = new GenericIdentity("oauth-demo-user", "Bearer");
            return new GenericPrincipal(identity, new[] { "ApiUser" });
        }

        public string test {  get; set; }

        #endregion
    }
}
