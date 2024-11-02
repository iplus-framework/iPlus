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
// <copyright file="ACActionArgs.cs" company="gip mbh, Oftersheim, Germany">
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
    public delegate void ACActionEventHandler(object sender, ACActionArgs e);

    /// <summary>
    /// Class ACActionArgs
    /// </summary>
    public class ACActionArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ACActionArgs"/> class.
        /// </summary>
        public ACActionArgs()
        {
            Handled = false;
            ElementAction = Global.ElementActionType.Drop;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ACActionArgs"/> class.
        /// </summary>
        /// <param name="dropObject">The drop object.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="elementAction">The element action.</param>
        public ACActionArgs(IACInteractiveObject dropObject, double x, double y, Global.ElementActionType elementAction = Global.ElementActionType.Drop)
        {
            Handled = false;
            DropObject = dropObject;
            X = x;
            Y = y;
            ElementAction = elementAction;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ACActionArgs"/> is handled.
        /// </summary>
        /// <value><c>true</c> if handled; otherwise, <c>false</c>.</value>
        public bool Handled { get; set; }

        /// <summary>
        /// Gets or sets the drop object.
        /// </summary>
        /// <value>The drop object.</value>
        public IACInteractiveObject DropObject { get; set; }

        /// <summary>
        /// Gets or sets the X.
        /// </summary>
        /// <value>The X.</value>
        public double X { get; set; }

        /// <summary>
        /// Gets or sets the Y.
        /// </summary>
        /// <value>The Y.</value>
        public double Y { get; set; }

        /// <summary>
        /// Gets or sets the element action.
        /// </summary>
        /// <value>The element action.</value>
        public Global.ElementActionType ElementAction { get; set; }
    }

    /// <summary>
    /// Class ACActionMenuArgs
    /// </summary>
    public class ACActionMenuArgs : ACActionArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ACActionMenuArgs"/> class.
        /// </summary>
        public ACActionMenuArgs() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ACActionMenuArgs"/> class.
        /// </summary>
        /// <param name="dropObject">The drop object.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="elementAction">The element action.</param>
        public ACActionMenuArgs(IACInteractiveObject dropObject, double x, double y, Global.ElementActionType elementAction = Global.ElementActionType.Drop)
            : base(dropObject, x, y, elementAction)
        {
        }

        /// <summary>
        /// Gets or sets the AC menu item list.
        /// </summary>
        /// <value>The AC menu item list.</value>
        public IEnumerable<ACMenuItem> ACMenuItemList
        {
            get;
            set;
        }
    }
}
