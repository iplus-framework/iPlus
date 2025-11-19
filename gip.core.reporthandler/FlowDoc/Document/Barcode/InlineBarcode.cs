using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using gip.core.datamodel;
using QRCoder.Xaml;
using static gip.core.datamodel.GS1;
using System.Linq;
using BarcodeLib;

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
        QRCODE = 99,
        GS1 = 100
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


        #region ESC properties
        //int desiredWidthDots, int heightPx, int minModule = 1, int maxModule = 6
        //int ESCDesiredWidthDots, int ESCHeightPx, int ESCMinModule = 1, int ESCMaxModule = 6

        public virtual int ESCDesiredWidthDots
        {
            get { return (int)GetValue(ESCDesiredWidthDotsProperty); }
            set { SetValue(ESCDesiredWidthDotsProperty, value); }
        }
        public static readonly DependencyProperty ESCDesiredWidthDotsProperty =
            DependencyProperty.Register(nameof(ESCDesiredWidthDots), typeof(int), typeof(InlineBarcode), new UIPropertyMetadata(3));

        public virtual int ESCHeightPx
        {
            get { return (int)GetValue(ESCHeightPxProperty); }
            set { SetValue(ESCHeightPxProperty, value); }
        }
        public static readonly DependencyProperty ESCHeightPxProperty =
            DependencyProperty.Register(nameof(ESCHeightPx), typeof(int), typeof(InlineBarcode), new UIPropertyMetadata(250));

        public virtual int ESCMinModule
        {
            get { return (int)GetValue(ESCMinModuleProperty); }
            set { SetValue(ESCMinModuleProperty, value); }
        }
        public static readonly DependencyProperty ESCMinModuleProperty =
            DependencyProperty.Register(nameof(ESCMinModule), typeof(int), typeof(InlineBarcode), new UIPropertyMetadata(1));

        public virtual int ESCMaxModule
        {
            get { return (int)GetValue(ESCMaxModuleProperty); }
            set { SetValue(ESCMaxModuleProperty, value); }
        }
        public static readonly DependencyProperty ESCMaxModuleProperty =
            DependencyProperty.Register(nameof(ESCMaxModule), typeof(int), typeof(InlineBarcode), new UIPropertyMetadata(6));

        public virtual bool ShowHRI
        {
            get { return (bool)GetValue(ShowHRIProperty); }
            set { SetValue(ShowHRIProperty, value); }
        }
        public static readonly DependencyProperty ShowHRIProperty =
            DependencyProperty.Register(nameof(ShowHRI), typeof(bool), typeof(InlineBarcode), new UIPropertyMetadata(false));


        public virtual bool Rotate90
        {
            get { return (bool)GetValue(Rotate90Property); }
            set { SetValue(Rotate90Property, value); }
        }
        public static readonly DependencyProperty Rotate90Property =
            DependencyProperty.Register(nameof(Rotate90), typeof(bool), typeof(InlineBarcode), new UIPropertyMetadata(false));

        #endregion

        #region GS1 data input

        public string VBShowColumns
        {
            get { return (string)GetValue(VBShowColumnsProperty); }
            set { SetValue(VBShowColumnsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VBShowColumns.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VBShowColumnsProperty =
            DependencyProperty.Register("VBShowColumns", typeof(string), typeof(InlineBarcode), new PropertyMetadata(null));


        public string VBShowColumnsKeys
        {
            get { return (string)GetValue(VBShowColumnsKeysProperty); }
            set { SetValue(VBShowColumnsKeysProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VBShowColumnsKeys.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VBShowColumnsKeysProperty =
            DependencyProperty.Register("VBShowColumnsKeys", typeof(string), typeof(InlineBarcode), new PropertyMetadata(null));
        #endregion



        public BarcodeValueCollection BarcodeValues { get; set; } = new BarcodeValueCollection();

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

        public GS1Model GS1Model { get; private set; }

        private void RenderBarcode(object value)
        {
            if (value == null)
                return;
            string strValue = null;
            if (!string.IsNullOrEmpty(VBShowColumns) && !string.IsNullOrEmpty(VBShowColumnsKeys))
            {
                string[] strColumnKeys = VBShowColumnsKeys.Split(new char[] { ',', ';' }, System.StringSplitOptions.RemoveEmptyEntries);
                string[] strValueIdentifiers = VBShowColumns.Split(new char[] { ',', ';' }, System.StringSplitOptions.RemoveEmptyEntries);

                if (strColumnKeys.Length == strValueIdentifiers.Length)
                {
                    List<(string ai, string val, bool variable)> input = GS1.GetGS1Data(value, strColumnKeys, strValueIdentifiers);
                    GS1Model = GS1.GetGS1Model(strColumnKeys, input);
                    strValue = GS1Model.RawGs1Value;
                    SetValue(ValueProperty, strValue);
                }
            }
            else
            {
                if (!(value is System.IConvertible) && !(value is System.IFormattable))
                    return;
                strValue = value as string;
                if (strValue == null)
                {
                    if (value is System.IConvertible)
                        strValue = System.Convert.ChangeType(value, typeof(string)) as string;
                    else
                        strValue = (value as System.IFormattable).ToString();
                }
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
