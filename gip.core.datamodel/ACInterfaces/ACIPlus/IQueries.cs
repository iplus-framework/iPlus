// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="IQueries.cs" company="gip mbh, Oftersheim, Germany">
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
    /// Interface IQueries
    /// </summary>
    public interface IQueries : IACComponent
    {
        /// <summary>
        /// Creates the query.
        /// </summary>
        /// <param name="acComponentParent">The ac component parent.</param>
        /// <param name="qryACName">Name of the qry AC.</param>
        /// <param name="acKey">The ac key.</param>
        /// <param name="forVBControl"></param>
        /// <returns>ACQueryDefinition.</returns>
        ACQueryDefinition CreateQuery(IACComponent acComponentParent, string qryACName, string acKey, bool forVBControl = false);

        /// <summary>
        /// Creates the queryby class.
        /// </summary>
        /// <param name="acComponentParent">optional</param>
        /// <param name="qryACClass">The qry AC class.</param>
        /// <param name="acKey">optional Schlüssel für Konfiguration</param>
        /// <param name="forVBControl"></param>
        /// <returns>ACQueryDefinition.</returns>
        ACQueryDefinition CreateQueryByClass(IACComponent acComponentParent, ACClass qryACClass, string acKey, bool forVBControl = false);

        /// <summary>
        /// Creates the queryby class with config.
        /// </summary>
        /// <param name="acComponentParent">The ac component parent.</param>
        /// <param name="qryACClass">The qry AC class.</param>
        /// <param name="configXML">The config XML.</param>
        /// <param name="forVBControl"></param>
        /// <returns>ACQueryDefinition.</returns>
        ACQueryDefinition CreateQueryByClassWithConfig(IACComponent acComponentParent, ACClass qryACClass, string configXML, bool forVBControl = false);
    }
}
