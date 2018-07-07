using System.Runtime.InteropServices;

namespace LibRetro.Types
{
    [StructLayout(LayoutKind.Sequential)]
    struct RetroHwRenderInterface
    {
        public RetroHwRenderInterfaceType InterfaceType;
        public uint InterfaceVersion;
    }
}
