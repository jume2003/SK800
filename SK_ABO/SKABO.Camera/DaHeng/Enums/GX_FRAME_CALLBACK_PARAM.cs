namespace SKABO.Camera.DaHeng.Enums
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct GX_FRAME_CALLBACK_PARAM
    {
        public IntPtr pUserParam;
        public int status;
        public IntPtr pImgBuf;
        public int nImgSize;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=8)]
        public int[] reserve;
    }
}

