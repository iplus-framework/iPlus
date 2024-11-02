// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;

namespace gip.core.reporthandlerwpf
{
    public class LinxByteMappingAttribute : Attribute
    {
        public int Order { get; set; }
        public int Length { get; set; }

        /// <summary>
        ///  Avoid null value problem by attribute
        /// </summary>
        public int? _DefaultValue { get; set; }
        public int DefaultValue
        {
            get
            {
                return _DefaultValue ?? 0;
            }
            set
            {
                _DefaultValue = value;
            }
        }
    }
}
