using System;
using System.Runtime.InteropServices;
using System.Text;

namespace LibRetro.Native
{
    class WindowsHelper : IHelper
    {
        void IHelper.FreeLibrary(IntPtr handle) {
            FreeLibrary(handle);
        }

        IntPtr IHelper.GetProcAddress(IntPtr dllHandle, string name) {
            return GetProcAddress(dllHandle, name);
        }

        IntPtr IHelper.LoadLibrary(string fileName) {
            return LoadLibrary(fileName);
        }

        int IHelper.sprintf(out string buffer, string format, params IntPtr[] args)
        {
            var tmpBuffer = new StringBuilder(
                    _scprintf(
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
                    ) + 1
                );

            sprintf(
                tmpBuffer,
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

            buffer = tmpBuffer.ToString();
            return tmpBuffer.Capacity;
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string fileName);

        [DllImport("kernel32.dll")]
        private static extern int FreeLibrary(IntPtr handle);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress (IntPtr handle, string procedureName);
        
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int sprintf(
            StringBuilder buffer,
            string format,
            IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8,
            IntPtr arg9, IntPtr arg10, IntPtr arg11, IntPtr arg12);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int _scprintf(
            string format,
            IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8,
            IntPtr arg9, IntPtr arg10, IntPtr arg11, IntPtr arg12);

    }
}