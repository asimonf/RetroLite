using System.Runtime.InteropServices;

namespace LibRetro.Types
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RetroSystemTiming
    {
        public double Fps;
        public double SampleRate;
    }
}
