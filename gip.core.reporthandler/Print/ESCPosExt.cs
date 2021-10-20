using static ESCPOS.Commands;
using ESCPOS;
using ESCPOS.Utils;
using System.Text;

namespace gip.core.reporthandler
{
    public static class ESCPosExt
    {

        public static byte[] PrintQRCodeExt(string content, QRCodeModel qrCodemodel = QRCodeModel.Model1, QRCodeCorrection qrodeCorrection = QRCodeCorrection.Percent7, QRCodeSizeExt qrCodeSize = QRCodeSizeExt.Six)
        {
            byte[] obj = new byte[9]
            {
                29,
                40,
                107,
                4,
                0,
                49,
                65,
                0,
                0
            };
            obj[7] = (byte)qrCodemodel;
            byte[] array = obj;
            byte[] obj2 = new byte[8]
            {
                29,
                40,
                107,
                3,
                0,
                49,
                67,
                0
            };
            obj2[7] = (byte)qrCodeSize;
            byte[] array2 = obj2;
            byte[] obj3 = new byte[8]
            {
                29,
                40,
                107,
                3,
                0,
                49,
                69,
                0
            };
            obj3[7] = (byte)qrodeCorrection;
            byte[] array3 = obj3;
            int num = content.Length + 3;
            int num2 = num % 256;
            int num3 = num / 256;
            byte[] obj4 = new byte[8]
            {
                29,
                40,
                107,
                0,
                0,
                49,
                80,
                48
            };
            obj4[3] = (byte)num2;
            obj4[4] = (byte)num3;
            byte[] array4 = obj4;
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            byte[] array5 = new byte[8]
            {
                29,
                40,
                107,
                3,
                0,
                49,
                81,
                48
            };
            return array.Add(array2, array3, array4, bytes, array5);
        }
    }
}
