using System.Runtime.InteropServices;

namespace LibRetro.Types
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct RetroControllerInfo
    {
        public RetroControllerDescription* Types;
        public uint NumTypes;
    }
}