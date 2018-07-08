using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace LibRetro.Types
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RetroSystemInfo
    {
        public IntPtr LibraryName;
        public IntPtr LibraryVersion;
        public IntPtr ValidExtensions;

        public bool NeedFullpath;
        public bool BlockExtract;

        public string[] GetExtensions()
        {
            var extensions = Marshal.PtrToStringAnsi(ValidExtensions);

            Debug.Assert(extensions != null, nameof(extensions) + " != null");
            return extensions.Split('|');
        }
    }
}
