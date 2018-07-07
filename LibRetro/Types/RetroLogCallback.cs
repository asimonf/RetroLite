using System;
using System.Runtime.InteropServices;

namespace LibRetro.Types
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, SetLastError = false)]
    internal delegate void RetroLogPrintf(RetroLogLevel level, [MarshalAs(UnmanagedType.LPStr)] string fmt,
        IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8,
        IntPtr arg9, IntPtr arg10, IntPtr arg11, IntPtr arg12);

    [StructLayout(LayoutKind.Sequential)]
    internal struct RetroLogCallback
    {
        private IntPtr _log;

        public RetroLogPrintf Log
        {
            set => _log = Marshal.GetFunctionPointerForDelegate(value);
        }
    }
}