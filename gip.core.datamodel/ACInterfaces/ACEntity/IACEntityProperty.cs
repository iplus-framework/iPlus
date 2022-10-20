// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="IACEntityProperty.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.core.datamodel
{
    /// <summary>
    /// Interface that implements Entity-Framework-Classes to be able to support "Virtual Properties".
    /// With "virtual properties" you can extend the Entity-Framework-Class with additional properties which are automatically serialized and deserialized from XML.
    /// The XML is stored in the Database-Field "XMLConfig".
    /// "Virtual Properties" are declared via BSOiPlusStudio by adding new ACClassProperty-Entries to the corresponding ACClass, that represents the Entity-Class (respectively the Database-Table).
    /// Virtual Properties are accessed by the this[]-Indexer.
    /// </summary>
    public interface IACEntityProperty
    {
        /// <summary>
        /// Indexer for reading an writing virtual properties.
        /// </summary>
        /// <param name="property">ACIdentifer of the virtual property</param>
        /// <returns>Boxed value</returns>
        object this[string property] { get; set; }


        /// <summary>
        /// Serialized Values of the extended virtual properties.
        /// </summary>
        /// <value>XML-String</value>
        string XMLConfig { get; set; }


        /// <summary>
        /// Method for manually forcing PropertyChanged-Event
        /// Needed for Changes in XML-Config and for forcing invocation of UpdateControlMode()-Method in VB-WPF-Controls
        /// </summary>
        /// <param name="property">Name of a property</param>
        void OnEntityPropertyChanged(string property);


        /// <summary>
        /// Instance, that manages the virtual properties for this entity-object.
        /// </summary>
        ACPropertyManager ACProperties { get; }
    }
}
