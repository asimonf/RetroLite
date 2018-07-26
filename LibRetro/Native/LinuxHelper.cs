using System;
using System.Runtime.InteropServices;
using System.Text;

namespace LibRetro.Native
{
    class LinuxHelper : IHelper
    {
        private readonly StringBuilder _sprintfBuffer = new StringBuilder();
        
        public IntPtr LoadLibrary(string fileName) {
            return dlopen(fileName, RtldNow);
        }

        public void FreeLibrary(IntPtr handle) {
            dlclose(handle);
        }

        public IntPtr GetProcAddress(IntPtr dllHandle, string name) {
            // clear previous errors if any
            dlerror();
            var res = dlsym(dllHandle, name);
            var errPtr = dlerror();
            if (errPtr != IntPtr.Zero) {
                throw new Exception("dlsym: " + Marshal.PtrToStringAnsi(errPtr));
            }
            return res;
        }
        
        public void Sprintf(out string buffer, string format, params IntPtr[] args)
        {
            if (args.Length > 12)
            {
                throw new Exception("Too many formatting arguments");
            }
            
            var size = snprintf(
                _sprintfBuffer,
                _sprintfBuffer.Capacity,
                format,
                args.Length >= 1 ? args[0] : IntPtr.Zero,
                args.Length >= 2 ? args[1] : IntPtr.Zero,
                args.Length >= 3 ? args[2] : IntPtr.Zero,
                args.Length >= 4 ? args[3] : IntPtr.Zero,
                args.Length >= 5 ? args[4] : IntPtr.Zero,
                args.Length >= 6 ? args[5] : IntPtr.Zero,
                args.Length >= 7 ? args[6] : IntPtr.Zero,
                args.Length >= 8 ? args[7] : IntPtr.Zero,
                args.Length >= 9 ? args[8] : IntPtr.Zero,
                args.Length >= 10 ? args[9] : IntPtr.Zero,
                args.Length >= 11 ? args[10] : IntPtr.Zero,
                args.Length >= 12 ? args[11] : IntPtr.Zero
            );

            if (size >= _sprintfBuffer.Capacity)
            {
                _sprintfBuffer.EnsureCapacity(size + 1);
                
                snprintf(
                    _sprintfBuffer,
                    _sprintfBuffer.Capacity,
                    format,
                    args.Length >= 1 ? args[0] : IntPtr.Zero,
                    args.Length >= 2 ? args[1] : IntPtr.Zero,
                    args.Length >= 3 ? args[2] : IntPtr.Zero,
                    args.Length >= 4 ? args[3] : IntPtr.Zero,
                    args.Length >= 5 ? args[4] : IntPtr.Zero,
                    args.Length >= 6 ? args[5] : IntPtr.Zero,
                    args.Length >= 7 ? args[6] : IntPtr.Zero,
                    args.Length >= 8 ? args[7] : IntPtr.Zero,
                    args.Length >= 9 ? args[8] : IntPtr.Zero,
                    args.Length >= 10 ? args[9] : IntPtr.Zero,
                    args.Length >= 11 ? args[10] : IntPtr.Zero,
                    args.Length >= 12 ? args[11] : IntPtr.Zero
                );
            }

            buffer = _sprintfBuffer.ToString();
        }

        private const int RtldNow = 2;

        [DllImport("libdl.so", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr dlopen(string fileName, int flags);
        
        [DllImport("libdl.so", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr dlsym(IntPtr handle, string symbol);

        [DllImport("libdl.so", CallingConvention = CallingConvention.Cdecl)]
        private static extern int dlclose(IntPtr handle);

        [DllImport("libdl.so", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr dlerror();
        
        [DllImport("libc.so", CallingConvention = CallingConvention.Cdecl)]
        private static extern int snprintf(
            StringBuilder buffer,
            int maxSize,
            string format,
            IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8,
            IntPtr arg9, IntPtr arg10, IntPtr arg11, IntPtr arg12);
    }
}