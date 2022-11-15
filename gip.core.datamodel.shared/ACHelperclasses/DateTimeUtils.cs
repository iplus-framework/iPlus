// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 01-17-2013
// ***********************************************************************
// <copyright file="Translator.cs" company="gip mbh, Oftersheim, Germany">
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
    public static class DateTimeUtils
    {
        /// <summary>
        /// Returns the local time without daylight saving time
        /// </summary>
        public static DateTime NowDST
        {
            get
            {
                DateTime dt = DateTime.Now;
                if (dt.IsDaylightSavingTime())
                    dt = dt.AddHours(-1);
                return dt;
            }
        }

        /// <summary>
        /// Returns the local time with offest to utc without daylight saving time
        /// </summary>
        public static DateTimeOffset NowOffDST
        {
            get
            {
                return new DateTimeOffset(NowDST);
            }
        }
    }
}
