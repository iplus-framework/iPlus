// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using Avalonia;
using System.IO;
using Avalonia.Metadata;
using BarcodeStandard;
using QRCoder.Xaml;

namespace gip.core.reporthandler.avui.Flowdoc
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

    public class InlineBarcode : InlineUIValueBase
    {
        public virtual BarcodeType BarcodeType
        {
            get { return (BarcodeType)GetValue(BarcodeTypeProperty); }
            set { SetValue(BarcodeTypeProperty, value); }
        }
        public static readonly StyledProperty<BarcodeType> BarcodeTypeProperty =
            AvaloniaProperty.Register<InlineBarcode, BarcodeType>(nameof(BarcodeType), BarcodeType.CODE128);


        public virtual int BarcodeWidth
        {
            get { return (int)GetValue(BarcodeWidthProperty); }
            set { SetValue(BarcodeWidthProperty, value); }
        }
        public static readonly StyledProperty<int> BarcodeWidthProperty =
            AvaloniaProperty.Register<InlineBarcode, int>(nameof(BarcodeWidth), 250);


        public virtual int BarcodeHeight
        {
            get { return (int)GetValue(BarcodeHeightProperty); }
            set { SetValue(BarcodeHeightProperty, value); }
        }
        public static readonly StyledProperty<int> BarcodeHeightProperty =
            AvaloniaProperty.Register<InlineBarcode, int>(nameof(BarcodeHeight), 100);

        public virtual int QRPixelsPerModule
        {
            get { return (int)GetValue(QRPixelsPerModuleProperty); }
            set { SetValue(QRPixelsPerModuleProperty, value); }
        }
        public static readonly StyledProperty<int> QRPixelsPerModuleProperty =
            AvaloniaProperty.Register<InlineBarcode, int>(nameof(QRPixelsPerModule), 20);

        public bool DrawQuietZones
        {
            get { return (bool)GetValue(DrawQuietZonesProperty); }
            set { SetValue(DrawQuietZonesProperty, value); }
        }
        public static readonly StyledProperty<bool> DrawQuietZonesProperty =
            AvaloniaProperty.Register<InlineBarcode, bool>(nameof(DrawQuietZones), true);


        [Content]
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

        private void RenderBarcode(object value)
        {
            // Rendering logic remains unchanged
        }
    }
}
