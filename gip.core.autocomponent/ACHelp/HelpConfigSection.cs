// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{
    public class HelpConfigSection : ConfigurationSection
    {
        private static HelpConfigSection settings = ConfigurationManager.GetSection("HelpConfigSection") as HelpConfigSection;

        public static HelpConfigSection Settings
        {
            get
            {
                return settings;
            }
        }

        [ConfigurationProperty("HelpPageRootURL", DefaultValue = "https://www.gip-automation.de", IsRequired = true)]
        public string HelpPageRootURL
        {
            get { return this["HelpPageRootURL"].ToString(); }
            set { this["HelpPageRootURL"] = value; }
        }

        [ConfigurationProperty("SearchRelativeURL", DefaultValue = "/{lang}/Search/Index/Json", IsRequired = true)]
        public string SearchRelativeURL
        {
            get { return this["SearchRelativeURL"].ToString(); }
            set { this["SearchRelativeURL"] = value; }
        }

        [ConfigurationProperty("MediaRelativeURL", DefaultValue = "/{lang}/documentation/Media/Get/Json", IsRequired = true)]
        public string MediaRelativeURL
        {
            get { return this["MediaRelativeURL"].ToString(); }
            set { this["MediaRelativeURL"] = value; }
        }

        [ConfigurationProperty("LoginRelativeURL", DefaultValue = "/{lang}/Login/Login/Json", IsRequired = true)]
        public string LoginRelativeURL
        {
            get { return this["LoginRelativeURL"].ToString(); }
            set { this["LoginRelativeURL"] = value; }
        }

        [ConfigurationProperty("RegisterRelativeURL", DefaultValue = "/{lang}/Register/Register/Json", IsRequired = true)]
        public string RegisterRelativeURL
        {
            get { return this["RegisterRelativeURL"].ToString(); }
            set { this["RegisterRelativeURL"] = value; }
        }
    }
}
