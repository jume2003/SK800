namespace SKABO.Camera.DaHeng.Enums
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct GX_DEVICE_BASE_INFO
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x20)]
        public string szVendorName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x20)]
        public string szModelName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x20)]
        public string szSN;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x40)]
        public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x200)]
        public string reserved;
    }
}

