using System;
using System.Runtime.InteropServices;

namespace LibRetro.Types
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate long RetroPerfGetTimeUsec();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate ulong RetroPerfGetCounter();
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate ulong RetroGetCpuFeatures();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RetroPerfLog();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RetroPerfRegister(ref RetroPerfCounter counter);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RetroPerfStart(ref RetroPerfCounter counter);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RetroPerfStop(ref RetroPerfCounter counter);

    [StructLayout(LayoutKind.Sequential)]
    public struct RetroPerfCallback
    {
        private IntPtr _getTimeUsec;
        private IntPtr _getCpuFeatures;

        private IntPtr _perfGetCounter;
        private IntPtr _perfRegister;
        private IntPtr _perfStart;
        private IntPtr _perfStop;
        private IntPtr _perfLog;

        public RetroPerfGetTimeUsec GetTimeUsec
        {
            set => _getTimeUsec = Marshal.GetFunctionPointerForDelegate(value);
        }

        public RetroGetCpuFeatures GetCpuFeatures
        {
            set => _getCpuFeatures = Marshal.GetFunctionPointerForDelegate(value);
        }

        public RetroPerfGetCounter PerfGetCounter
        {
            set => _perfGetCounter = Marshal.GetFunctionPointerForDelegate(value);
        }

        public RetroPerfRegister PerfRegister
        {
            set => _perfRegister = Marshal.GetFunctionPointerForDelegate(value);
        }

        public RetroPerfStart PerfStart
        {
            set => _perfStart = Marshal.GetFunctionPointerForDelegate(value);
        }

        public RetroPerfStop PerfStop
        {
            set => _perfStop = Marshal.GetFunctionPointerForDelegate(value);
        }

        public RetroPerfLog PerfLog
        {
            set => _perfLog = Marshal.GetFunctionPointerForDelegate(value);
        }
    }
}