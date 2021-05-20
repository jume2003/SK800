namespace SKABO.Camera.DaHeng.Enums
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct GX_OPEN_PARAM
    {
        public string pszContent;
        public GX_OPEN_MODE openMode;
        public GX_ACCESS_MODE accessMode;
    }
}

