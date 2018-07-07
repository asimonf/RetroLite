using System.Runtime.InteropServices;

namespace LibRetro.Types
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RetroSystemAvInfo
    {
        public RetroGameGeometry Geometry;
        public RetroSystemTiming Timing;
    }
}
