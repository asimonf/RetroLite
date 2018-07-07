using System;
using System.Runtime.InteropServices;

namespace LibRetro.Types
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RetroMessage
    {
        private IntPtr _message;
        private uint _frames;

        public string Message => Marshal.PtrToStringAnsi(_message);
        public uint Frames => _frames;
    }
}
