using System;
using System.Runtime.InteropServices;

namespace LibRetro.Types
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RetroSubsystemMemoryInfo
    {
        public IntPtr Extension;

        public uint Type;
    }
}