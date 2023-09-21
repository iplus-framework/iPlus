// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="IVBSource.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Reflection;

namespace gip.core.datamodel
{
    public interface IWPFServices
    {
        IVBDesignerService DesignerService { get; }

        IVBMediaControllerService VBMediaControllerService { get; }

        IVBWFLayoutCalculatorService WFLayoutCalculatorService { get; }

        void AddXamlNamespacesFromAssembly(Assembly classAssembly);
    }
}
