// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="IACConfigProvider.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;

namespace gip.core.datamodel
{
    /// <summary>
    /// The same configuration-value (stored in IACConfig.Value) can be stored in different Config-Tables that are build on each other.
    /// We call this scenario "Config-Parameter overriding" or the principle of "strict entity separation with progressive concretization".
    /// IACConfigProvider manages the insertion, manipulation, read and removal of configurations at the right IACConfigStore.
    /// This interface is implemented from ConfigManagerIPlus or from ConfigManagerIPlusMES. 
    /// If you have a further datamodel, that must be considered for parameter overriding, then extend one of these classes.
    /// Also you have to add your own tables that implement IACConfigStore and IACConfig.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{''}de{''}", Global.ACKinds.TACInterface)]
    public interface IACConfigProvider
    {

        #region Config stores
        /// <summary>
        /// Returns a list of IACConfigStore. The implementation must set the IACConfigStore.OverridingOrder-Property.
        /// </summary>
        /// <param name="callingConfigStoreList"></param>
        /// <returns></returns>
        List<IACConfigStore> GetACConfigStores(List<IACConfigStore> callingConfigStoreList);
       

        #endregion

        #region Configuration Lists

        /// <summary>
        /// get configuration list for specified properties and context
        /// </summary>
        /// <param name="callingConfigStoreList"></param>
        /// <param name="preConfigACUrl"></param>
        /// <param name="localConfigACUrlList"></param>
        /// <param name="fetchFirstConfig"></param>
        /// <returns></returns>
        List<IACConfig> GetConfigurationList(List<IACConfigStore> callingConfigStoreList, string preConfigACUrl, List<string> localConfigACUrlList, Guid? vbiACClassID, bool fetchFirstConfig = true, bool reloadParams = false);

        /// <summary>
        /// Confgiuration list for GUI and VBBSOControlPW
        /// </summary>
        /// <param name="acMethod"></param>
        /// <param name="mandatoryConfigStores"></param>
        /// <param name="preACUrl"></param>
        /// <param name="localACURL"></param>
        /// <returns></returns>
        List<ACConfigParam> GetACConfigParamList(ACMethod acMethod, List<IACConfigStore> mandatoryConfigStores, string preACUrl, string localACURL, bool reloadParams = false);

        /// <summary>
        /// Writes configurations-values to ACMethod.ParameterValueList.
        /// Only the last valid value in the Config-Store hierarchy will be taken.
        /// </summary>
        /// <param name="acMethod"></param>
        /// <param name="mandatoryConfigStores"></param>
        /// <param name="preACUrl"></param>
        /// <param name="localACURL"></param>
        /// <param name="vbiACClassID"></param>
        /// <param name="fetchFirstConfig"></param>
        short WriteConfigIntoACValue(ACMethod acMethod, List<IACConfigStore> mandatoryConfigStores, string preACUrl, string localACURL, Guid? vbiACClassID, bool fetchFirstConfig);
        #endregion

        #region Configuration one

        /// <summary>
        /// Returns the last valid Configuration according to the IACConfigStore.OverridingOrder
        /// </summary>
        /// <param name="callingConfigStoreList"></param>
        /// <param name="preConfigACUrl"></param>
        /// <param name="localConfigACUrl"></param>
        /// <returns></returns>
        IACConfig GetConfiguration(List<IACConfigStore> callingConfigStoreList, string preConfigACUrl, string localConfigACUrl, Guid? vbiACClassID, out int priorityLevel);

        /// <summary>
        ///  Delete all configuration for node by deleting same node
        /// </summary>
        /// <param name="localConfigACUrl"></param>
        void DeleteConfigNode(IACEntityObjectContext db, Guid acClassWFID);

        #endregion

        #region Serialization

        /// <summary>
        /// List for deserialization
        /// </summary>
        /// <param name="db"></param>
        /// <param name="rmiResult"></param>
        /// <returns></returns>
        List<IACConfigStore> DeserializeMandatoryConfigStores(IACEntityObjectContext db, List<ACConfigStoreInfo> rmiResult);

        #endregion

        #region serving rule values
        List<RuleValue> GetDBStoredRuleValueList(List<IACConfigStore> callingConfigStores, string preConfigACUrl, string localConfigACUrl, string configStoreUrl);

        RuleValueList GetRuleValueList(List<IACConfigStore> callingConfigStores, string preConfigACUrl, string localConfigACUrl);
        #endregion

        #region Query configuration from all config stores
        List<IACConfig> QueryAllCOnfigs(IACEntityObjectContext db, IACConfigStore sameConfigStore, string preConfigACUrl, string localConfigACUrl, Guid? vbiACClassID);
        #endregion
    }
}
