﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace gip.tool.installerAndUpdater
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            TextResources.TextResource.Culture = System.Globalization.CultureInfo.CurrentUICulture;
            //TextResources.TextResource.Culture = new System.Globalization.CultureInfo("de-DE");
        }
    }
}
