using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.Templates;
using gip.core.datamodel;


namespace gip.core.layoutengine.avui
{
    public class VBGraphItemDataTemplateSelector : IDataTemplate
    {

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


        public Control Build(object param)
        {
            if (param == null)
                return null;

            IACObject contextObject = param as IACObject;
            IACInteractiveObject acInteractiveObject = param as IACInteractiveObject;
            if (acInteractiveObject != null && acInteractiveObject.ContextACObject != null)
                contextObject = acInteractiveObject.ContextACObject;
            if (contextObject == null)
                return null;

            string stringValue = "";

            if(!string.IsNullOrEmpty(DataTemplateValueACUrl))
                stringValue = contextObject.ACUrlCommand(DataTemplateValueACUrl, param).ToString();
            else
                stringValue = contextObject.ToString();

            return stringValue switch
            {
                var value when value == DataTemplate0Value => DataTemplate0.Build(param),
                var value when value == DataTemplate1Value => DataTemplate1.Build(param),
                var value when value == DataTemplate2Value => DataTemplate2.Build(param),
                var value when value == DataTemplate3Value => DataTemplate3.Build(param),
                var value when value == DataTemplate4Value => DataTemplate4.Build(param),
                var value when value == DataTemplate5Value => DataTemplate5.Build(param),
                var value when value == DataTemplate6Value => DataTemplate6.Build(param),
                var value when value == DataTemplate7Value => DataTemplate7.Build(param),
                var value when value == DataTemplate8Value => DataTemplate8.Build(param),
                var value when value == DataTemplate9Value => DataTemplate9.Build(param),
                var value when value == DataTemplate10Value => DataTemplate10.Build(param),
                var value when value == DataTemplate11Value => DataTemplate11.Build(param),
                var value when value == DataTemplate12Value => DataTemplate12.Build(param),
                var value when value == DataTemplate13Value => DataTemplate13.Build(param),
                var value when value == DataTemplate14Value => DataTemplate14.Build(param),
                var value when value == DataTemplate15Value => DataTemplate15.Build(param),
                var value when value == DataTemplate16Value => DataTemplate16.Build(param),
                var value when value == DataTemplate17Value => DataTemplate17.Build(param),
                var value when value == DataTemplate18Value => DataTemplate18.Build(param),
                var value when value == DataTemplate19Value => DataTemplate19.Build(param),
                var value when value == DataTemplate20Value => DataTemplate20.Build(param),
                _ => DataTemplate1.Build(param)
            };
        }

        public bool Match(object data)
        {
            if (data == null)
                return false;

            IACObject contextObject = data as IACObject;
            IACInteractiveObject acInteractiveObject = data as IACInteractiveObject;
            if (acInteractiveObject != null && acInteractiveObject.ContextACObject != null)
                contextObject = acInteractiveObject.ContextACObject;
            if (contextObject == null)
                return false;

            string stringValue = "";

            if (!string.IsNullOrEmpty(DataTemplateValueACUrl))
                stringValue = contextObject.ACUrlCommand(DataTemplateValueACUrl, data).ToString();
            else
                stringValue = contextObject.ToString();

            return !string.IsNullOrEmpty(stringValue);
        }
    }
}
