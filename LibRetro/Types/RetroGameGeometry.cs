using System.Runtime.InteropServices;

namespace LibRetro.Types
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RetroGameGeometry
    {
        public uint BaseWidth;
        public uint BaseHeight;
        public uint MaxWidth;
        public uint MaxHeight;

        public float AspectRatio;

        public bool Equals(RetroGameGeometry geometry)
        {
            return BaseWidth == geometry.BaseWidth &&
                BaseHeight == geometry.BaseHeight &&
                MaxWidth == geometry.MaxWidth &&
                MaxHeight == geometry.MaxHeight &&
                AspectRatio == geometry.AspectRatio;
        }
    }
}
