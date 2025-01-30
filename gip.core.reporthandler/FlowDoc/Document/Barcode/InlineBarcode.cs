using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BarcodeLib;
using QRCoder.Xaml;

namespace gip.core.reporthandler.Flowdoc
{
    public enum BarcodeType
    {
        UNSPECIFIED = 0,
        UPCA = 1,
        UPCE = 2,
        UPC_SUPPLEMENTAL_2DIGIT = 3,
        UPC_SUPPLEMENTAL_5DIGIT = 4,
        EAN13 = 5,
        EAN8 = 6,
        Interleaved2of5 = 7,
        Interleaved2of5_Mod10 = 8,
        Standard2of5 = 9,
        Standard2of5_Mod10 = 10,
        Industrial2of5 = 11,
        Industrial2of5_Mod10 = 12,
        CODE39 = 13,
        CODE39Extended = 14,
        CODE39_Mod43 = 15,
        Codabar = 16,
        PostNet = 17,
        BOOKLAND = 18,
        ISBN = 19,
        JAN13 = 20,
        MSI_Mod10 = 21,
        MSI_2Mod10 = 22,
        MSI_Mod11 = 23,
        MSI_Mod11_Mod10 = 24,
        Modified_Plessey = 25,
        CODE11 = 26,
        USD8 = 27,
        UCC12 = 28,
        UCC13 = 29,
        LOGMARS = 30,
        CODE128 = 31,
        CODE128A = 32,
        CODE128B = 33,
        CODE128C = 34,
        ITF14 = 35,
        CODE93 = 36,
        TELEPEN = 37,
        FIM = 38,
        PHARMACODE = 39,
        QRCODE = 99
    }

    [ContentProperty(nameof(BarcodeValues))]
    public class InlineBarcode : InlineUIValueBase
    {
        public virtual BarcodeType BarcodeType
        {
            get { return (BarcodeType)GetValue(BarcodeTypeProperty); }
            set { SetValue(BarcodeTypeProperty, value); }
        }
        public static readonly DependencyProperty BarcodeTypeProperty =
            DependencyProperty.Register("BarcodeType", typeof(BarcodeType), typeof(InlineBarcode), new UIPropertyMetadata(BarcodeType.CODE128));


        public virtual int BarcodeWidth
        {
            get { return (int)GetValue(BarcodeWidthProperty); }
            set { SetValue(BarcodeWidthProperty, value); }
        }
        public static readonly DependencyProperty BarcodeWidthProperty =
            DependencyProperty.Register("BarcodeWidth", typeof(int), typeof(InlineBarcode), new UIPropertyMetadata(250));


        public virtual int BarcodeHeight
        {
            get { return (int)GetValue(BarcodeHeightProperty); }
            set { SetValue(BarcodeHeightProperty, value); }
        }
        public static readonly DependencyProperty BarcodeHeightProperty =
            DependencyProperty.Register("BarcodeHeight", typeof(int), typeof(InlineBarcode), new UIPropertyMetadata(100));

        public virtual int QRPixelsPerModule
        {
            get { return (int)GetValue(QRPixelsPerModuleProperty); }
            set { SetValue(QRPixelsPerModuleProperty, value); }
        }
        public static readonly DependencyProperty QRPixelsPerModuleProperty =
            DependencyProperty.Register("QRPixelsPerModule", typeof(int), typeof(InlineBarcode), new UIPropertyMetadata(20));

        public bool DrawQuietZones
        {
            get { return (bool)GetValue(DrawQuietZonesProperty); }
            set { SetValue(DrawQuietZonesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DrawQuietZones.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DrawQuietZonesProperty =
            DependencyProperty.Register("DrawQuietZones", typeof(bool), typeof(InlineBarcode), new PropertyMetadata(true));




        public BarcodeValueCollection BarcodeValues { get;set;} = new BarcodeValueCollection();

        public override object Value
        {
            get
            {
                return (object)GetValue(ValueProperty);
            }
            set
            {
                SetValue(ValueProperty, value);
                RenderBarcode(value);
            }
        }

        private void RenderBarcode(object value)
        {
            if (value == null)
                return;
            if (!(value is System.IConvertible) && !(value is System.IFormattable))
                return;
            string strValue = value as string;
            if (strValue == null)
            {
                if (value is System.IConvertible)
                    strValue = System.Convert.ChangeType(value, typeof(string)) as string;
                else
                    strValue = (value as System.IFormattable).ToString();
            }
            if (string.IsNullOrEmpty(strValue))
                return;

            //System.Drawing.Image img = null;
            System.Windows.Controls.Image wpfImage = new System.Windows.Controls.Image();
            if (BarcodeType == BarcodeType.QRCODE)
            {
                using (QRCoder.QRCodeGenerator qrGenerator = new QRCoder.QRCodeGenerator())
                using (QRCoder.QRCodeData qrCodeData = qrGenerator.CreateQrCode(strValue, QRCoder.QRCodeGenerator.ECCLevel.Q))
                using (XamlQRCode xamlQRCode = new XamlQRCode(qrCodeData))
                {
                    DrawingImage dw = xamlQRCode.GetGraphic(QRPixelsPerModule, DrawQuietZones);
                    wpfImage.Source = dw;
                    wpfImage.MaxHeight = MaxHeight > 0.1 ? MaxHeight : 200;
                    wpfImage.MaxWidth = MaxWidth > 0.1 ? MaxWidth : 200;
                    this.Child = wpfImage;
                }

                //using (QRCoder.QRCodeGenerator qrGenerator = new QRCoder.QRCodeGenerator())
                //using (QRCoder.QRCodeData qrCodeData = qrGenerator.CreateQrCode(strValue, QRCoder.QRCodeGenerator.ECCLevel.Q))
                //using (QRCoder.QRCode qrCode = new QRCoder.QRCode(qrCodeData))
                //{
                //    img = qrCode.GetGraphic(QRPixelsPerModule);
                //    wpfImage.MaxHeight = MaxHeight > 0.1 ? MaxHeight : 200;
                //    wpfImage.MaxWidth = MaxWidth > 0.1 ? MaxWidth : 200;
                //}
            }
            else
            {
                using (Barcode b = new Barcode())
                {
                    //img = b.Encode((TYPE)BarcodeType, strValue, System.Drawing.Color.Black, System.Drawing.Color.White, BarcodeWidth, BarcodeHeight);
                    if (MaxHeight > 0.1)
                        wpfImage.MaxHeight = MaxHeight;
                    if (MaxWidth > 0.1)
                        wpfImage.MaxWidth = MaxWidth;
                    using (System.Drawing.Image img = b.Encode((TYPE)BarcodeType, strValue, System.Drawing.Color.Black, System.Drawing.Color.White, BarcodeWidth, BarcodeHeight))
                    using (var ms = new MemoryStream())
                    {
                        img.Save(ms, ImageFormat.Bmp);
                        ms.Seek(0, SeekOrigin.Begin);

                        var bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.StreamSource = ms;
                        bitmapImage.EndInit();

                        wpfImage.Source = bitmapImage;
                        this.Child = wpfImage;
                    }
                }
            }
        }
    }
}
