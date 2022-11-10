// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 11-07-2012
// ***********************************************************************
// <copyright file="XMLToObjectConverter.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Runtime.Serialization;
using System.Data;
using System.Globalization;
using System.Threading;

namespace gip.core.datamodel
{
    /// <summary>
    /// Class XMLToObjectConverter
    /// </summary>
    public static class XMLToObjectConverter
    {
        /// <summary>
        /// XMLs to object.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="valueXML">The value XML.</param>
        /// <param name="invariantCulture">invariantCulture</param>
        /// <param name="database">The database.</param>
        /// <returns>System.Object.</returns>
        static public object XMLToObject(Type objectType, string valueXML, bool invariantCulture, IACEntityObjectContext database)
        {
            return ACConvert.ChangeType(valueXML, null, objectType, invariantCulture, database);
        }

        /// <summary>
        /// Objects to XML.
        /// </summary>
        /// <param name="valueObject">The value object.</param>
        /// <param name="invariantCulture">invariantCulture</param>
        /// <param name="entityAsEntityKey">if set to <c>true</c> [entity as entity key].</param>
        /// <param name="xmlIndented">if set to <c>true</c> [XML indented].</param>
        /// <returns>System.String.</returns>
        static public string ObjectToXML(object valueObject, bool invariantCulture, bool entityAsEntityKey = false, bool xmlIndented = false)
        {
            return ACConvert.ChangeType(valueObject, null, typeof(string), invariantCulture, null, entityAsEntityKey, xmlIndented) as string;
        }
    }
}
