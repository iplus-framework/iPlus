using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.tcShared
{
    //TwinCAT data types length in bytes
    public static class TCDataTypeLength
    {
        //Bool
        public static int Bool = 1;

        //ushort
        public static int UInt = 2;

        //short
        public static int Int = 2;

        //int
        public static int DInt = 4;

        //uint
        public static int UDInt = 4;

        //float
        public static int Real = 4;

        //double
        public static int LReal = 8;

        //string
        public static int String(int length = 80)
        {
            return length;
        }

        //TimeSpan
        public static int Time = 4;

        //DateTime
        public static int DT = 4;

        public static int Array(int arrryTypeLen, int arrayLen)
        {
            return arrayLen * arrryTypeLen;
        }
    }
}
