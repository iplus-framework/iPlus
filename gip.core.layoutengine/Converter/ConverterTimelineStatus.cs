using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using gip.core.datamodel;

namespace gip.core.layoutengine
{
    public class ConverterTimelineStatus : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            List<Global.TimelineItemStatus> status = value as List<Global.TimelineItemStatus>;
            Global.TimelineItemStatus icon;
            Enum.TryParse(parameter.ToString(), out icon);
            if (status.Contains(icon))
                return true;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
