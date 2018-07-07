using System;
using System.Runtime.InteropServices;

namespace LibRetro.Types
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RetroFrameTimeDelegate(long usec);
    
    [StructLayout(LayoutKind.Sequential)]
    public struct RetroFrameTimeCallback
    {
        private IntPtr _callback;

        private long _reference;

        public bool HasCallback => _callback != IntPtr.Zero;

        public RetroFrameTimeDelegate Callback => _callback != IntPtr.Zero ? Marshal.GetDelegateForFunctionPointer<RetroFrameTimeDelegate>(_callback) : null;

        public long Reference
        {
            get => _reference;
            set => _reference = value;
        }
    }
}