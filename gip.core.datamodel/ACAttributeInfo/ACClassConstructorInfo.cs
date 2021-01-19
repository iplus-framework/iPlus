// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 01-15-2013
// ***********************************************************************
// <copyright file="ACClassConstructorInfo.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace gip.core.datamodel
{
    /// <summary>
    /// Class ACClassConstructorInfo
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ACClassConstructorInfo : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ACClassConstructorInfo"/> class.
        /// </summary>
        /// <param name="acParameters">The ac parameters.</param>
        public ACClassConstructorInfo(object[] acParameters = null)
        {
            ACParameters = acParameters;
        }

        /// <summary>
        /// Gets or sets the AC parameters.
        /// </summary>
        /// <value>The AC parameters.</value>
        public object[] ACParameters
        {
            get;
            set;
        }
    }
}
