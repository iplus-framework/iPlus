// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="IACComponentProcessFunction.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************


namespace gip.core.datamodel
{
    /// <summary>
    /// Interface for PAProcessFunction and PWProcessFunction that executes ACMethod asynchronously
    /// </summary>
    public interface IACComponentProcessFunction :  IACComponent, IACConfigURL
    {
        /// <summary>The virtual method that this function is currently processing.</summary>
        /// <value>ACMethod</value>
        IACContainerTNet<ACMethod> CurrentACMethod { get; set; }
    }
}
