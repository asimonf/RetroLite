using System;
using System.Runtime.InteropServices;

namespace LibRetro.Types
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate void RetroKeyboardEvent(bool down, RetroKey keycode, uint character, ushort modifiers);
    
    [StructLayout(LayoutKind.Sequential)]
    public struct RetroKeyboardCallback
    {
        private IntPtr _callback;
        
        public RetroKeyboardEvent Callback
        {
            set => _callback = Marshal.GetFunctionPointerForDelegate(value);
        }
    }
}