namespace SKABO.Camera.DaHeng.Enums
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct GX_ENUM_DESCRIPTION
    {
        public long nValue;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x40)]
        public string szSymbolic;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=8)]
        public int[] reserve;
    }
}

