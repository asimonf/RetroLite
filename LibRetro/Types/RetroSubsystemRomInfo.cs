using System;
using System.Runtime.InteropServices;

namespace LibRetro.Types
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct RetroSubsystemRomInfo
    {
        public IntPtr Desc;

        public IntPtr ValidExtensions;

        public bool NeedFullpath;

        public bool BlockExtract;

        public bool Required;

        public RetroSubsystemMemoryInfo* Memory;

        public uint NumMemory;
    }
}