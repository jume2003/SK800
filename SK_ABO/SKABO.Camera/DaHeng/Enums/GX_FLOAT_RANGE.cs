namespace SKABO.Camera.DaHeng.Enums
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct GX_FLOAT_RANGE
    {
        public double dMin;
        public double dMax;
        public double dInc;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=8)]
        public string szUnit;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=8)]
        public int[] reserve;
    }
}

