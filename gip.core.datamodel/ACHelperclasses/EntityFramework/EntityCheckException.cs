// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="DataManipulationException.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace gip.core.datamodel
{
    public class EntityCheckException : Exception
    {
        public EntityCheckException(string message, IList<Msg> subMessages)
            : base(message)
        {
            SubMessages = subMessages;
        }

        public IList<Msg> SubMessages
        {
            get;
            set;
        }
    }
}
