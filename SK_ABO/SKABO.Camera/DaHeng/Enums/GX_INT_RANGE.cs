namespace SKABO.Camera.DaHeng.Enums
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct GX_INT_RANGE
    {
        public long nMin;
        public long nMax;
        public long nInc;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=8)]
        public int[] reserve;
    }
}

