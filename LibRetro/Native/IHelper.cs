﻿using System;

namespace LibRetro.Native
{
    internal interface IHelper
    {
        IntPtr LoadLibrary(string fileName);
        void FreeLibrary(IntPtr handle);
        IntPtr GetProcAddress(IntPtr dllHandle, string name);
        
        int Sprintf(
            out string buffer,
            string format,
            params IntPtr[] args);
    }
}