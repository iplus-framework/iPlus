using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace gip.core.layoutengine
{
    public class VBGraphItemDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DataTemplate0 { get; set; }
        public DataTemplate DataTemplate1 { get; set; }
        public DataTemplate DataTemplate2 { get; set; }
        public DataTemplate DataTemplate3 { get; set; }
        public DataTemplate DataTemplate4 { get; set; }
        public DataTemplate DataTemplate5 { get; set; }
        public DataTemplate DataTemplate6 { get; set; }
        public DataTemplate DataTemplate7 { get; set; }
        public DataTemplate DataTemplate8 { get; set; }
        public DataTemplate DataTemplate9 { get; set; }
        public DataTemplate DataTemplate10 { get; set; }

        public string DataTemplateValueACUrl { get; set; }

        public string DataTemplate0Value { get; set; }
        public string DataTemplate1Value { get; set; }
        public string DataTemplate2Value { get; set; }
        public string DataTemplate3Value { get; set; }
        public string DataTemplate4Value { get; set; }
        public string DataTemplate5Value { get; set; }
        public string DataTemplate6Value { get; set; }
        public string DataTemplate7Value { get; set; }
        public string DataTemplate8Value { get; set; }
        public string DataTemplate9Value { get; set; }
        public string DataTemplate10Value { get; set; }


        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item == null)
                return null;

            IVBContent acObject = item as IVBContent;
            if (acObject == null)
                return null;

            string stringValue = "";

            if(!string.IsNullOrEmpty(DataTemplateValueACUrl))
                stringValue = acObject.ContextACObject.GetValue(DataTemplateValueACUrl).ToString();
            else
                stringValue = acObject.ContextACObject.ToString();

            if (stringValue == DataTemplate0Value)
                return DataTemplate0;

            if (stringValue == DataTemplate1Value)
                return DataTemplate1;

            if (stringValue == DataTemplate2Value)
                return DataTemplate2;

            if (stringValue == DataTemplate3Value)
                return DataTemplate3;

            if (stringValue == DataTemplate4Value)
                return DataTemplate4;

            if (stringValue == DataTemplate5Value)
                return DataTemplate5;

            if (stringValue == DataTemplate6Value)
                return DataTemplate6;

            if (stringValue == DataTemplate7Value)
                return DataTemplate7;

            if (stringValue == DataTemplate8Value)
                return DataTemplate8;

            if (stringValue == DataTemplate9Value)
                return DataTemplate9;

            return DataTemplate1;
        }
    }
}
