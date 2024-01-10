using System.Windows;

namespace gip.core.reporthandler.Flowdoc
{
    public class BarcodeValue : InlineUIValueBase
    {

        public virtual string AI
        {
            get { return (string)GetValue(AIProperty); }
            set { SetValue(AIProperty, value); }
        }
        public static readonly DependencyProperty AIProperty = ReportDocument.StringFormatProperty.AddOwner(typeof(BarcodeValue));
    }
}
