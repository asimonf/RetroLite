using System;
using System.Runtime.InteropServices;

namespace LibRetro.Types
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct RetroGameInfo
    {
        public IntPtr Path;

        public IntPtr Data;

        public ulong size;

        public IntPtr Meta;
    }
}
