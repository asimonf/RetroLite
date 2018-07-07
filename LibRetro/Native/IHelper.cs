using System;
using System.Text;

namespace LibRetro.Native
{
    interface IHelper
    {
        IntPtr LoadLibrary(string fileName);
        void FreeLibrary(IntPtr handle);
        IntPtr GetProcAddress(IntPtr dllHandle, string name);
        
        int sprintf(
            out string buffer,
            string format,
            params IntPtr[] args);
    }
}