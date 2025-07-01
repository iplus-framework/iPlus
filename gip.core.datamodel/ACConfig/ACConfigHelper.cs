// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
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
            var item = 
                ACConfigQuery<IACConfig>.QueryConfigSource(configItemsSource, preConfigACUrl, localConfigACUrl, vbiACClassID)
                .Where(x =>    (x as VBEntityObject).EntityState != EntityState.Deleted 
                            && (fetchDeattached || ((x as VBEntityObject).EntityState != EntityState.Detached)))
                .FirstOrDefault();
            if (item == null)
                return null;
            else
                return (IACConfig)item;
        }


        /// <summary>
        /// Get configuration for specified propedrties
        /// </summary>
        /// <param name="configItemsSource"></param>
        /// <param name="preConfigACUrl"></param>
        /// <param name="localConfigACUrlList"></param>
        /// <param name="fetchDeattached"></param>
        /// <param name="vbiACClassID"></param>
        /// <returns></returns>
        public static List<IACConfig> GetStoreConfigurationList(IEnumerable<IACConfig> configItemsSource, string preConfigACUrl, List<string> localConfigACUrlList, bool fetchDeattached, Guid? vbiACClassID)
        {
            if (!configItemsSource.Any()) 
                return null;
            return
                ACConfigQuery<IACConfig>.QueryConfigSource(configItemsSource, preConfigACUrl, localConfigACUrlList, vbiACClassID)
                .Where(x => (x as VBEntityObject).EntityState != EntityState.Deleted
                            && (fetchDeattached || ((x as VBEntityObject).EntityState != EntityState.Detached)))
                .ToList();

        }

        public static List<IACConfig> GetStoreConfigurationList(IEnumerable<IACConfig> configItemsSource, string preConfigACUrl, string startsWithLocalConfigACUrl, bool fetchDeattached, Guid? vbiACClassID)
        {
            if (!configItemsSource.Any()) 
                return null;
            return
                ACConfigQuery<IACConfig>.QueryConfigSourceStart(configItemsSource, preConfigACUrl, startsWithLocalConfigACUrl, vbiACClassID)
                .Where(x => (x as VBEntityObject).EntityState != EntityState.Deleted
                            && (fetchDeattached || ((x as VBEntityObject).EntityState != EntityState.Detached)))
                .ToList();

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
