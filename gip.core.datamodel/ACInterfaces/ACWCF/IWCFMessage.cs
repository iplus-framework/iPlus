// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="IWCFMessage.cs" company="gip mbh, Oftersheim, Germany">
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
    /// Interface IWCFMessage
    /// </summary>
    public interface IWCFMessage
    {
        /// <summary>
        /// Gets or sets the AC URL.
        /// </summary>
        /// <value>The AC URL.</value>
        string ACUrl { get; set; }

        /// <summary>
        /// Gets or sets the AC parameter.
        /// </summary>
        /// <value>The AC parameter.</value>
        Object[] ACParameter { get; set; }

        /// <summary>
        /// Gets or sets the method invoke request ID.
        /// </summary>
        /// <value>The method invoke request ID.</value>
        int MethodInvokeRequestID { get; set; }
    }
}
