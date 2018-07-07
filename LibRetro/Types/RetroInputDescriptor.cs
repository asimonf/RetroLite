using System;
using System.Runtime.InteropServices;

namespace LibRetro.Types
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RetroInputDescriptor
    {
        private uint _port;
        private RetroDevice _device;
        private uint _index;
        private uint _id;

        private IntPtr _description;
        
        public uint Port => _port;
        public RetroDevice Device => _device;
        public uint Index => _index;
        public uint Id => _id;

        public string Description => Marshal.PtrToStringAnsi(_description);
    }
}
