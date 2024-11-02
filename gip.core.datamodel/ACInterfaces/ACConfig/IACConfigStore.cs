// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="IACConfigStore.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Collections.Generic;

namespace gip.core.datamodel
{
    /// <summary>
    /// Mode for ValidateConfigurationEntriesWithDB
    /// </summary>
    public enum ConfigEntriesValidationMode
    {
        /// <summary>
        /// If ConfigurationEntries (cache) has not any entries, then check if there are any in database
        /// </summary>
        AnyCheck,
        
        /// <summary>
        /// Compare the count in ConfigurationEntries (cache) with database
        /// </summary>
        CompareCount,
        
        /// <summary>
        /// Compare the content of the cache with database
        /// </summary>
        CompareContent
    }

    /// <summary>
    /// The IACConfigStore interface allows you to store and read any data (even complex ones) for objects that are related to each other. (see summary of IACConfig for more informations)
    /// IACConfigStore implements EF-Classes that represent a application context (e.g. for production orders, BOMs, purchase orders...).
    /// The Entity-Class that represents the "main table" in group of semantically related tables must implement the interface IACConfigStore. 
    /// Other tables inside this group can also implement IACConfigStore if they want save additional data for itself.
    /// Via IACConfigStore you can access the corresponding Config-Table (IACConfig) on an abstract way.
    /// The following Entity-Classes implement this interface: ACClass, ACClassDesign, ACClassMethod, ACClassProperty, ACProgram, Material, MaterialWF, Partslist, ProdOrderPartslist...
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{''}de{''}", Global.ACKinds.TACInterface)]
    public interface IACConfigStore : IACObject
    {
        /// <summary>
        /// A human readable text of the "ConfigStore"
        /// </summary>
        /// <value>
        /// A human readable text of the "ConfigStore".
        /// </value>
        [ACPropertyInfo(2, "ConfigStoreName", "en{'Config Store Name'}de{'Konfigurationspeicher Name'}")]
        string ConfigStoreName { get; }


        /// <summary>
        /// ACConfigKeyACUrl returns the relative Url to the "main table" in group a group of semantically related tables.
        /// This property is used when NewACConfig() is called. NewACConfig() creates a new IACConfig-Instance and set the IACConfig.KeyACUrl-Property with this ACConfigKeyACUrl.
        /// </summary>
        string ACConfigKeyACUrl { get; }


        /// <summary>
        /// The same configuration-value (stored in IACConfig.Value) can be stored in different Config-Tables that are build on each other.
        /// We call this scenario "Config-Parameter overriding" or the principle of "strict entity separation with progressive concretization".
        /// With this property the "Overriding order" will be defined. This parameter is no persisted and will automatically be recalculated and set
        /// from classes which implement the interface IACConfigProvider (ConfigManagerIPlus and ConfigManagerIPlusMES).
        /// </summary>
        /// <value>
        /// "Overriding order"
        /// </value>
        decimal OverridingOrder { get; set; }


        /// <summary>
        /// Creates and adds a new IACConfig-Entry to ConfigurationEntries.
        /// The implementing class creates a new entity object an add it to its "own Configuration-Table".
        /// It sets automatically the IACConfig.KeyACUrl-Property with this ACConfigKeyACUrl.
        /// </summary>
        /// <param name="acObject">Optional: Reference to another Entity-Object that should be related for this new configuration entry.</param>
        /// <param name="valueTypeACClass">The iPlus-Type of the "Value"-Property.</param>
        /// <param name="localConfigACUrl"></param>
        /// <returns>IACConfig as a new entry</returns>
        IACConfig NewACConfig(IACObjectEntity acObject = null, gip.core.datamodel.ACClass valueTypeACClass = null, string localConfigACUrl = null);


        /// <summary>Removes a configuration from ConfigurationEntries and the database-context.</summary>
        /// <param name="acObject">Entry as IACConfig</param>
        void RemoveACConfig(IACConfig acObject);


        /// <summary>
        /// Deletes all IACConfig-Entries in the Database-Context as well as in ConfigurationEntries.
        /// </summary>
        void DeleteAllConfig();


        /// <summary>
        /// A thread-safe and cached list of Configuration-Values of type IACConfig.
        /// </summary>
        IEnumerable<IACConfig> ConfigurationEntries { get; }


        /// <summary>
        /// Clears the cache of configuration entries. (ConfigurationEntries)
        /// Re-accessing the ConfigurationEntries property rereads all configuration entries from the database.
        /// </summary>
        void ClearCacheOfConfigurationEntries();

        /// <summary>
        /// Checks if cached configuration entries are loaded from database successfully
        /// </summary>
        bool ValidateConfigurationEntriesWithDB(ConfigEntriesValidationMode mode = ConfigEntriesValidationMode.AnyCheck);
    }
}
