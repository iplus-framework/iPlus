﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using gip.core.datamodel;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Controls;

namespace gip.core.reporthandlerwpf.Flowdoc
{
    public class ConfigurationMethod : ItemsControl
    {
        public ConfigurationMethod()
        {
            
        }

        public string VBContent
        {
            get { return (string)GetValue(VBContentProperty); }
            set { SetValue(VBContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VBContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VBContentProperty =
            DependencyProperty.Register("VBContent", typeof(string), typeof(ConfigurationMethod));


        public string MethodName
        {
            get
            {
                return VBContent.Split('\\').Last().Split('(').Last().Split(')').First();
            }
        }

    }
}
