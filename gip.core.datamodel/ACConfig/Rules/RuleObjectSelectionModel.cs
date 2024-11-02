// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System.Collections.Generic;

namespace gip.core.datamodel
{
    public class RuleObjectSelectionModel
    {
        public RuleInfo RuleInfo { get; set; }
        public string ConfigStoreURL { get; set; }
        public string PreACUrl { get; set; }
        public string PAFRulePropertyACUrl { get; set; }

        private List<object> _AvailableValues;
        public List<object> AvailableValues
        {
            get
            {
                return _AvailableValues;
            }
            set
            {
                _AvailableValues = value;
            }
        }

        private List<object> _SelectedValues;
        public List<object> SelectedValues 
        {
            get
            {
                return _SelectedValues;
            }
            set
            {
                _SelectedValues = value;
            }
        }
    }
}
