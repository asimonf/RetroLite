using System.Runtime.InteropServices;

namespace LibRetro.Types
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RetroPerfCounter
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string Ident;
        
        public ulong Start;
        
        public ulong Total;
        
        public ulong CallCount;

        public bool Registered;
    }
}