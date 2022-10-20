using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace gip.core.reporthandler.Flowdoc
{
    public class ConfigurationParameter : DependencyObject
    {
        public ConfigurationParameter()
        {

        }

        public string ParameterName
        {
            get { return (string)GetValue(ParameterNameProperty); }
            set { SetValue(ParameterNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ParametarName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ParameterNameProperty =
            DependencyProperty.Register("ParameterName", typeof(string), typeof(ConfigurationParameter));

        
    }
}
