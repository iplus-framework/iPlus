// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System.Collections.Generic;
using System.Reflection;

namespace gip.core.reporthandlerwpf.Flowdoc
{
    /// <summary>
    /// Static cache class for report paginator
    /// </summary>
    internal static class ReportPaginatorStaticCache
    {
        private static Dictionary<string, ReportContextValueType> _reportContextValueTypes;

        /// <summary>
        /// Static constructor
        /// </summary>
        static ReportPaginatorStaticCache() 
        {
            // add static cache for report context value names
            _reportContextValueTypes = new Dictionary<string, ReportContextValueType>(20);
            foreach (FieldInfo fi in typeof(ReportContextValueType).GetFields())
            {
                if (((int)fi.Attributes & (int)FieldAttributes.Static) == 0) continue;
                _reportContextValueTypes.Add(fi.Name.ToLowerInvariant(), (ReportContextValueType)fi.GetRawConstantValue());
            }
        }

        /// <summary>
        /// Gets a report context value type by name
        /// </summary>
        /// <param name="name">name of report context value</param>
        /// <returns>null, if it does not exist</returns>
        public static ReportContextValueType? GetReportContextValueTypeByName(string name)
        {
            if (name == null) return null;
            name = name.ToLowerInvariant();
            if (!_reportContextValueTypes.ContainsKey(name)) return null;
            return _reportContextValueTypes[name];
        }
    }
}
