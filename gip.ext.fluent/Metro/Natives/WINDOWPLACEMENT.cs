using System;
using System.Runtime.InteropServices;

namespace Fluent.Metro.Native
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct WINDOWPLACEMENT
    {
        public int length;
        public int flags;
        public int showCmd;
        public POINT minPosition;
        public POINT maxPosition;
        [NonSerialized]
        public RECT normalPosition;
    }
}