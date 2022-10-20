using Abt.Controls.SciChart.Visuals.Axes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gip.core.datamodel;
using System.IO;

namespace gip.core.scichart
{
    public class EnumLabelProvider : NumericLabelProvider
    {
        public Type EnumType
        {
            get;
            set;
        }

        public override string FormatLabel(IComparable dataValue)
        {
            try
            {
                if (dataValue == null)
                    return "";

                if (EnumType == null || !EnumType.IsEnum)
                    return base.FormatLabel(dataValue);

                short dValue;
                double emptyValue;

                if (short.TryParse(dataValue.ToString(), out dValue))
                {
                    string value = Enum.GetName(EnumType, dValue);
                    if (string.IsNullOrEmpty(value))
                        return base.FormatLabel(dataValue);
                    return value;
                }
                else if (double.TryParse(dataValue.ToString(), out emptyValue))
                    return "";

                return base.FormatLabel(dataValue);

            }
            catch (Exception)
            {
                return base.FormatLabel(dataValue);
            }
        }

        public override string FormatCursorLabel(IComparable dataValue)
        {
            return base.FormatCursorLabel(dataValue);
        }
    }
}
