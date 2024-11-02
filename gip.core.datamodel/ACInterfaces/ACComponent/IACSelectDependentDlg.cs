// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 11-08-2012
// ***********************************************************************
// <copyright file="IVBContent.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************


namespace gip.core.datamodel
{
    /// <summary>
    /// Interface for selection dependant dialogs. It synchronizes the Content of a selected WPF-Control with the model.
    /// </summary>
    public interface IACSelectDependentDlg : IACBSO
    {
        /// <summary>  Reference to the Selection-Manager-Instance</summary>
        /// <value>  Reference to the Selection-Manager-Instance</value>
        IACComponent VBBSOSelectionManager { get; }


        /// <summary>  The object (model) that is selected on the GUI.</summary>
        /// <value>  The object (model) that is selected on the GUI.</value>
        IACObject CurrentSelection { get; }
    }
}
