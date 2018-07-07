using System;
using System.Runtime.InteropServices;

namespace LibRetro.Types
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RetroControllerDescription
    {
        public IntPtr Desc;

        public uint Id;
    }
}