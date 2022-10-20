// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="IVBSerialize.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml.Linq;

namespace gip.core.datamodel
{
    /// <summary>
    /// Interface IVBSerialize
    /// </summary>
    public interface IVBSerialize
    {
        /// <summary>
        /// Adds the serializable attributes.
        /// </summary>
        /// <param name="xElement">The x element.</param>
        void AddSerializableAttributes(XElement xElement);
    }
}
