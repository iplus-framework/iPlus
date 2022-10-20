// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ACSerializeableInfo.cs" company="gip mbh, Oftersheim, Germany">
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
    /// Class ACSerializeableInfo
    /// </summary>
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Enum)]
    public class ACSerializeableInfo : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ACSerializeableInfo"/> class.
        /// </summary>
        public ACSerializeableInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ACSerializeableInfo"/> class.
        /// </summary>
        /// <param name="typeList">The type list.</param>
        public ACSerializeableInfo(Type[] typeList)
        {
            TypeList = typeList;
        }

        /// <summary>
        /// Gets or sets the type list.
        /// </summary>
        /// <value>The type list.</value>
        public Type[] TypeList
        {
            get;
            set;
        }

    }
}
