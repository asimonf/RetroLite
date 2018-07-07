using System;
using System.Runtime.InteropServices;

namespace LibRetro.Types
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct RetroSubsystemInfo
    {
        public IntPtr Desc;

        public IntPtr Ident;

        public RetroSubsystemRomInfo* Roms;

        public uint NumRoms;

        public uint Id;
    }
}