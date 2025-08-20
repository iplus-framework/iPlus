using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace gip.core.layoutengine.avui
{
    public class ProgramLogViewItemMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter == null)
                parameter = "L";

            switch (parameter.ToString().ToUpper())
            {
                case "T": { return new Thickness(0, (double)value, 0, 0); }
                case "R": { return new Thickness(0, 0, (double)value, 0); }
                case "B": { return new Thickness(0, 0, 0, (double)value); }
                default: { return new Thickness((double)value, 0, 0, 0); }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter == null)
                parameter = "L";

            switch (parameter.ToString().ToUpper())
            {
                case "T": { return ((Thickness)value).Top; }
                case "R": { return ((Thickness)value).Right; }
                case "B": { return ((Thickness)value).Bottom; }
                default: { return ((Thickness)value).Left; }
            }
        }
    }
}
