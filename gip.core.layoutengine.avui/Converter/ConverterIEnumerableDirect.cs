using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using gip.core.datamodel;
using System.Windows;

namespace gip.core.layoutengine.avui.VisualControlAnalyser.Converter
{
    //class ConverterIEnumerableDirect : ConverterBase, IMultiValueConverter
    //{
    //    #region c´tors
    //    public ConverterIEnumerableDirect()
    //        : base()
    //    {
    //    }
    //    #endregion

    //    #region IMultiValueConverter Members

    //    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        List<object> parameters = parameter as List<object>;

    //        if (parameters[0].ToString() == "@0") // Wenn nur direkter Wert, dann keine Berechnung
    //        {
    //            if (values[0] == null)
    //                return "";
    //            return values[0].ToString();
    //        }
    //        else
    //        {
    //            object result = null;
    //            if (Calculator.Calculate(values, parameters, out result))
    //            {
    //                return result.ToString();
    //            }

    //            return parameters[0].ToString();
    //        }
    //    }

    //    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //    #endregion
    //}
}
