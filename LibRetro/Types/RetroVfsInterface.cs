using System;
using System.Runtime.InteropServices;

namespace LibRetro.Types
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RetroVfsInterface
    {
        public IntPtr GetPath;
        public IntPtr Open;
        public IntPtr Close;
        public IntPtr Size;
        public IntPtr Tell;
        public IntPtr Seek;
        public IntPtr Read;
        public IntPtr Write;
        public IntPtr Flush;
        public IntPtr Remove;
        public IntPtr Rename;
    }
}
