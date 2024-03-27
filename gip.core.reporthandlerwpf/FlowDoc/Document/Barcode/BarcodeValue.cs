using System.Windows;

namespace gip.core.reporthandlerwpf.Flowdoc
{
    public class BarcodeValue : InlineDocumentValue
    {

        #region Properties

        public virtual string AI
        {
            get { return (string)GetValue(AIProperty); }
            set { SetValue(AIProperty, value); }
        }
        public static readonly DependencyProperty AIProperty = ReportDocument.StringFormatProperty.AddOwner(typeof(BarcodeValue));


        #endregion

    }
}
