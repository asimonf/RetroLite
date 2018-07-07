using System;
using System.Runtime.InteropServices;

namespace LibRetro.Types
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RetroVariable
    {
        public IntPtr key;
        public IntPtr value;

        public string Key => Marshal.PtrToStringAnsi(key);
        public string Value => Marshal.PtrToStringAnsi(value);
    }
}
