using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    public interface IAppManager : IACComponent
    {
        string RoutingServiceACUrl
        {
            get;
        }

        string[] FindMatchingUrls(FindMatchingUrlsParam queryParam);

        Dictionary<string, object> GetACComponentACMemberValues(Dictionary<string, string> acUrl_AcMemberIdentifiers);
    }
}
