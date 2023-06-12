using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace gip.core.layoutengine
{
    public class MultiConverterComparable : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Any(c => c == null))
                return false;

            object firstValue = values.FirstOrDefault();
            if (firstValue == null)
                return false;

            string firstValueString = firstValue as string;
            if (firstValueString != null)
            {
                if (string.IsNullOrEmpty(firstValueString))
                    return false;
            }

            if (values.All(c => c.Equals(firstValue)))
                return true;

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
