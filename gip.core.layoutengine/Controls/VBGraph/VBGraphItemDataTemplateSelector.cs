using gip.core.datamodel;
using System.Windows;
using System.Windows.Controls;

namespace gip.core.layoutengine
{
    public class VBGraphItemDataTemplateSelector : DataTemplateSelector
    {

        /*
         * For now no idea how resolve this generic
         * Can be array items used in binding
         */

        #region DataTemplate
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
        public DataTemplate DataTemplate11 { get; set; }
        public DataTemplate DataTemplate12 { get; set; }
        public DataTemplate DataTemplate13 { get; set; }
        public DataTemplate DataTemplate14 { get; set; }
        public DataTemplate DataTemplate15 { get; set; }
        public DataTemplate DataTemplate16 { get; set; }
        public DataTemplate DataTemplate17 { get; set; }
        public DataTemplate DataTemplate18 { get; set; }
        public DataTemplate DataTemplate19 { get; set; }
        public DataTemplate DataTemplate20 { get; set; }

        #endregion

        public string DataTemplateValueACUrl { get; set; }

        #region DataTemplate Value

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
        public string DataTemplate11Value { get; set; }
        public string DataTemplate12Value { get; set; }
        public string DataTemplate13Value { get; set; }
        public string DataTemplate14Value { get; set; }
        public string DataTemplate15Value { get; set; }
        public string DataTemplate16Value { get; set; }
        public string DataTemplate17Value { get; set; }
        public string DataTemplate18Value { get; set; }
        public string DataTemplate19Value { get; set; }
        public string DataTemplate20Value { get; set; }

        #endregion


        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item == null)
                return null;

            IACObject contextObject = item as IACObject;
            IACInteractiveObject acInteractiveObject = item as IACInteractiveObject;
            if (acInteractiveObject != null && acInteractiveObject.ContextACObject != null)
                contextObject = acInteractiveObject.ContextACObject;
            if (contextObject == null)
                return null;

            string stringValue = "";

            if (!string.IsNullOrEmpty(DataTemplateValueACUrl))
                stringValue = contextObject.ACUrlCommand(DataTemplateValueACUrl, item).ToString();
            else
                stringValue = contextObject.ToString();

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

            if (stringValue == DataTemplate10Value)
                return DataTemplate10;

            if (stringValue == DataTemplate11Value)
                return DataTemplate11;

            if (stringValue == DataTemplate12Value)
                return DataTemplate12;

            if (stringValue == DataTemplate13Value)
                return DataTemplate13;

            if (stringValue == DataTemplate14Value)
                return DataTemplate14;

            if (stringValue == DataTemplate15Value)
                return DataTemplate15;

            if (stringValue == DataTemplate16Value)
                return DataTemplate16;

            if (stringValue == DataTemplate17Value)
                return DataTemplate17;

            if (stringValue == DataTemplate18Value)
                return DataTemplate18;

            if (stringValue == DataTemplate19Value)
                return DataTemplate19;

            if (stringValue == DataTemplate20Value)
                return DataTemplate20;

            return DataTemplate1;
        }
    }
}
