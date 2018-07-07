using System.Runtime.InteropServices;

namespace LibRetro.Types
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct RetroVfsInterfaceInfo
    {
        public uint requiredInterfaceVersion;

        public RetroVfsInterface* iface;
    }
}
