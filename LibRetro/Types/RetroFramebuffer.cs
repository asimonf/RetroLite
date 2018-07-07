using System.Runtime.InteropServices;

namespace LibRetro.Types
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct RetroFramebuffer
    {
        public void* Data;
        public uint Width;
        public uint Height;
        public uint Pitch;

        public RetroPixelFormat Format;

        public uint AccessFlags;

        public uint MemoryFlags;
    }
}
