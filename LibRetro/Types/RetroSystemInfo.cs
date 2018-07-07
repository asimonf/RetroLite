using System;
using System.Runtime.InteropServices;

namespace LibRetro.Types
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct RetroSystemInfo
    {
        public IntPtr LibraryName;
        public IntPtr LibraryVersion;
        public IntPtr ValidExtensions;

        public bool NeedFullpath;
        public bool BlockExtract;

        public string[] GetExtensions()
        {
            var extensions = Marshal.PtrToStringAnsi(ValidExtensions);

            return extensions.Split('|');
        }
    }
}
