using System;
using System.Runtime.InteropServices;
using System.Text;

namespace LibRetro.Native
{
    class LinuxHelper : IHelper
    {
        public IntPtr LoadLibrary(string fileName) {
            return dlopen(fileName, RTLD_NOW);
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
        
        public int sprintf(out string buffer, string format, params IntPtr[] args)
        {
            if (args.Length > 12)
            {
                throw new Exception("Too many formatting arguments");
            }
            
            var tmpBuffer = new StringBuilder(5);

            var size = snprintf(
                tmpBuffer,
                tmpBuffer.Capacity,
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

            if (size >= tmpBuffer.Capacity)
            {
                tmpBuffer = new StringBuilder(size + 1);
                
                snprintf(
                    tmpBuffer,
                    tmpBuffer.Capacity,
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

            buffer = tmpBuffer.ToString();
            return tmpBuffer.Capacity;
        }

        const int RTLD_NOW = 2;

        [DllImport("libdl.so")]
        private static extern IntPtr dlopen(String fileName, int flags);
        
        [DllImport("libdl.so")]
        private static extern IntPtr dlsym(IntPtr handle, String symbol);

        [DllImport("libdl.so")]
        private static extern int dlclose(IntPtr handle);

        [DllImport("libdl.so")]
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