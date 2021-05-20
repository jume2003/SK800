namespace SKABO.Camera.DaHeng.Enums
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct GX_FRAME_DATA
    {
        public int nStatus;
        public IntPtr pImgBuf;
        public int nWidth;
        public int nHeight;
        public int nPixelFormat;
        public int nImgSize;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=7)]
        public int[] reserve;
    }
}

