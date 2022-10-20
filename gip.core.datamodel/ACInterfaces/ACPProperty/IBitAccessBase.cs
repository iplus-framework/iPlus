// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="IBitAccessBase.cs" company="gip mbh, Oftersheim, Germany">
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
    /// Interface IBitAccessBase
    /// </summary>
    public interface IBitAccessBase : IConvertible
    {
        /// <summary>
        /// Sets from string.
        /// </summary>
        /// <param name="value">The value.</param>
        void SetFromString(string value);
    }
}
