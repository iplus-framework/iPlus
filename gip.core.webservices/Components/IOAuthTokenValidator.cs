using gip.core.datamodel;
using System.Security.Principal;

namespace gip.core.webservices
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'IOAuthTokenValidator'}de{'IOAuthTokenValidator'}", Global.ACKinds.TACInterface)]
    public interface IOAuthTokenValidator : IACComponent
    {
        IPrincipal ValidatePrincipalFromBearerToken(string accessToken);
    }
}
