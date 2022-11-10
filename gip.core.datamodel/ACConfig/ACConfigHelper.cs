using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


namespace gip.core.datamodel
{
    public static class ACConfigHelper
    {
        /// <summary>
        /// Get single configuration
        /// </summary>
        /// <param name="configItemsSource"></param>
        /// <param name="preConfigACUrl"></param>
        /// <param name="localConfigACUrl"></param>
        /// <param name="fetchDeattached"></param>
        /// <param name="vbiACClassID"></param>
        /// <returns></returns>
        public static IACConfig GetStoreConfiguration(IEnumerable<IACConfig> configItemsSource, string preConfigACUrl, string localConfigACUrl, bool fetchDeattached, Guid? vbiACClassID)
        {
#if !EFCR
            var item = 
                ACConfigQuery<IACConfig>.QueryConfigSource(configItemsSource, preConfigACUrl, localConfigACUrl, vbiACClassID)
                .Where(x => (x as VBEntityObject).EntityState != EntityState.Deleted &&
                    (fetchDeattached || ((x as VBEntityObject).EntityState != EntityState.Detached) )
                )
                .FirstOrDefault();
            if (item == null)
                return null;
            else
                return (IACConfig)item;
#endif
            throw new NotImplementedException();
        }


        /// <summary>
        /// Get configuration for specified propedrties
        /// </summary>
        /// <param name="configItemsSource"></param>
        /// <param name="preConfigACUrl"></param>
        /// <param name="localConfigACUrlList"></param>
        /// <param name="vbiACClassID"></param>
        /// <returns></returns>
        public static List<IACConfig> GetStoreConfigurationList(IEnumerable<IACConfig> configItemsSource, string preConfigACUrl, List<string> localConfigACUrlList, Guid? vbiACClassID)
        {
            if (!configItemsSource.Any()) 
                return null;
#if !EFCR
            return
                ACConfigQuery<IACConfig>.QueryConfigSource(configItemsSource, preConfigACUrl, localConfigACUrlList, vbiACClassID)
                .Where(x =>  (x as VBEntityObject).EntityState != EntityState.Deleted && (x as VBEntityObject).EntityState != EntityState.Detached)
                .ToList();
#endif
            throw new NotImplementedException();
        }

        public static List<IACConfig> GetStoreConfigurationList(IEnumerable<IACConfig> configItemsSource, string preConfigACUrl, string startsWithLocalConfigACUrl, Guid? vbiACClassID)
        {
            if (!configItemsSource.Any()) 
                return null;
#if !EFCR
            return
                ACConfigQuery<IACConfig>.QueryConfigSourceStart(configItemsSource, preConfigACUrl, startsWithLocalConfigACUrl, vbiACClassID)
                .Where(x => (x as VBEntityObject).EntityState != EntityState.Deleted && (x as VBEntityObject).EntityState != EntityState.Detached)
                .ToList();
#endif
            throw new NotImplementedException();
        }
        

        public static ACConfigParam FactoryMachineParam(ACConfigParam originalParam, ACClass vbiACClass)
        {
            ACConfigParam newParam = new ACConfigParam();
            newParam.ACIdentifier = originalParam.ACIdentifier;
            newParam.ACCaption = originalParam.ACCaption;
            newParam.ValueTypeACClassID = originalParam.ValueTypeACClassID;
            newParam.VBiACClassID = vbiACClass.ACClassID;
            newParam.VBACClass = vbiACClass;
            return newParam;
        }
    }
}
