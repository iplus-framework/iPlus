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
// <copyright file="IVBGui.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    /// <summary>
    /// Interface IVBGui
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'IVBGui'}de{'IVBGui'}", Global.ACKinds.TACInterface)]
    public interface IVBGui : IACObject
    {

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="forObject">For object.</param>
        /// <param name="acClassDesignName">Name of the ac class design.</param>
        /// <param name="acCaption">The ac caption.</param>
        /// <param name="isClosableBSORoot">if set to <c>true</c> [is closable BSO root].</param>
        /// <param name="ribbonVisibility">The ribbon visibility.</param>
        /// <param name="closeButtonVisibility">visibility of the close button</param>
        void ShowDialog(IACComponent forObject, string acClassDesignName, string acCaption = "", bool isClosableBSORoot = false,
            Global.ControlModes ribbonVisibility = Global.ControlModes.Hidden, Global.ControlModes closeButtonVisibility = Global.ControlModes.Enabled);

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="forObject">For object.</param>
        /// <param name="acClassDesignName">Name of the ac class design.</param>
        /// <param name="acCaption">The ac caption.</param>
        /// <param name="isClosableBSORoot">if set to <c>true</c> [is closable BSO root].</param>
        /// <param name="ribbonVisibility">The ribbon visibility.</param>
        /// <param name="closeButtonVisibility">visibility of the close button</param>
        Task ShowDialogAsync(IACComponent forObject, string acClassDesignName, string acCaption = "", bool isClosableBSORoot = false,
            Global.ControlModes ribbonVisibility = Global.ControlModes.Hidden, Global.ControlModes closeButtonVisibility = Global.ControlModes.Enabled);

        /// <summary>
        /// Shows the window.
        /// </summary>
        /// <param name="forObject">For object.</param>
        /// <param name="acClassDesignName">Name of the ac class design.</param>
        /// <param name="isClosableBSORoot">if set to <c>true</c> [is closable BSO root].</param>
        /// <param name="containerType">Type of the container.</param>
        /// <param name="dockState">State of the dock.</param>
        /// <param name="dockPosition">The dock position.</param>
        /// <param name="ribbonVisibility">The ribbon visibility.</param>
        /// <param name="closeButtonVisibility">visibility of the close button</param>
        void ShowWindow(IACComponent forObject, string acClassDesignName, bool isClosableBSORoot, Global.VBDesignContainer containerType, Global.VBDesignDockState dockState,
            Global.VBDesignDockPosition dockPosition, Global.ControlModes ribbonVisibility, Global.ControlModes closeButtonVisibility = Global.ControlModes.Enabled);

        /// <summary>
        /// Closes the top dialog.
        /// </summary>
        void CloseTopDialog();

        /// <summary>
        /// Gets the dialog stack.
        /// </summary>
        /// <value>The dialog stack.</value>
        List<IVBDialog> DialogStack { get; }
    }
}
