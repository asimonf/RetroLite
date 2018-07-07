using System;
using System.Runtime.InteropServices;

namespace LibRetro.Types
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.FunctionPtr)]
    public delegate RetroGetProcAddress RetroGetProcAddress([MarshalAs(UnmanagedType.LPStr)]string sym);
    
    [StructLayout(LayoutKind.Sequential)]
    public struct RetroGetProcAddressInterface
    {
        public IntPtr GetProcAddress;
    }
}