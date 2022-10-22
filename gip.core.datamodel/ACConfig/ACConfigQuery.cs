using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    public static class ACConfigQuery<T> where T : IACConfig
    {

        #region Query Definition
#if !EFCR
        public static IEnumerable<T> QueryConfigSource(IEnumerable<T> configItemsSource, string preConfigACUrl, string localConfigACUrl, Guid? vbiACClassID)
        {
            
            return configItemsSource.Where(x =>
                       x.LocalConfigACUrl != null
                    && x.LocalConfigACUrl == localConfigACUrl
                    && ( ((x.PreConfigACUrl ?? "") == "") || (x.PreConfigACUrl ?? "") == (preConfigACUrl ?? "")) 
                        && (vbiACClassID == null || (x.VBiACClassID == null || x.VBiACClassID == vbiACClassID))
                ).OrderByDescending(x => x.PreConfigACUrl)
                .ThenByDescending(x => x.VBACClass == null ? "" : x.VBACClass.ACIdentifier);
        }
#endif

#if !EFCR
        public static IEnumerable<T> QueryConfigSource(IEnumerable<T> configItemsSource, string preConfigACUrl, List<string> localConfigACUrlList, Guid? vbiACClassID)
        {
            return configItemsSource.Where(x =>
                    localConfigACUrlList.Contains(x.LocalConfigACUrl) 
                    && ( ((x.PreConfigACUrl ?? "") == "") || (x.PreConfigACUrl ?? "") == (preConfigACUrl ?? "")) 
                        && (vbiACClassID == null || (x.VBiACClassID == null || x.VBiACClassID == vbiACClassID))
                ).OrderByDescending(x => x.PreConfigACUrl)
                .ThenBy(c => c.LocalConfigACUrl)
                .ThenByDescending(x => x.VBACClass == null ? "" : x.VBACClass.ACIdentifier);
        }
#endif

#if !EFCR
        public static IEnumerable<T> QueryConfigSourceStart(IEnumerable<T> configItemsSource, string preConfigACUrl, string startsWithLocalConfigACUrl, Guid? vbiACClassID)
        {
            return configItemsSource.Where(x =>
                   x.LocalConfigACUrl != null
                && x.LocalConfigACUrl.StartsWith(startsWithLocalConfigACUrl)
                && (   ((x.PreConfigACUrl ?? "") == "") || (x.PreConfigACUrl ?? "") == (preConfigACUrl ?? "")) 
                    && (vbiACClassID == null || (x.VBiACClassID == null || x.VBiACClassID == vbiACClassID))
           ).OrderByDescending(x => x.PreConfigACUrl)
            .ThenBy(c => c.LocalConfigACUrl)
            .ThenByDescending(x => x.VBACClass == null ? "" : x.VBACClass.ACIdentifier);
        }
#endif

#endregion
    }
}
