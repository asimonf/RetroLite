using System;
using System.Runtime.InteropServices;

namespace LibRetro.Types
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RetroHwContextReset();
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate UIntPtr RetroHwGetCurrentFramebuffer();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.FunctionPtr)]
    public delegate RetroProcAddress RetroHwGetProcAddress([MarshalAs(UnmanagedType.LPStr)] string sym);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RetroProcAddress();
    
    [StructLayout(LayoutKind.Sequential)]
    public struct RetroHwRenderCallback
    {
        private IntPtr _contextType;

        private IntPtr _contextReset;

        private IntPtr _getCurrentFramebuffer;

        private IntPtr _getProcAddress;

        private bool _depth;

        private bool _stencil;

        private bool _bottomLeftOrigin;

        private uint _versionMajor;

        private uint _versionMinor;

        private bool _cacheContext;

        private IntPtr _contextDestroy;

        private bool _debugContext;

        public RetroHwContextType ContextType
        {
            set => _contextType = Marshal.GetFunctionPointerForDelegate(value);
        }

        public RetroHwContextReset ContextReset
        {
            set => _contextReset = Marshal.GetFunctionPointerForDelegate(value);
        }
        
        public RetroHwGetCurrentFramebuffer GetCurrentFramebuffer
        {
            set => _getCurrentFramebuffer = Marshal.GetFunctionPointerForDelegate(value);
        }
        
        public RetroHwGetProcAddress GetProcAddress
        {
            set => _getProcAddress = Marshal.GetFunctionPointerForDelegate(value);
        }

        public bool Depth
        {
            get => _depth;
            set => _depth = value;
        }

        public bool Stencil
        {
            get => _stencil;
            set => _stencil = value;
        }

        public bool BottomLeftOrigin
        {
            get => _bottomLeftOrigin;
            set => _bottomLeftOrigin = value;
        }

        public uint VersionMajor
        {
            get => _versionMajor;
            set => _versionMajor = value;
        }

        public uint VersionMinor
        {
            get => _versionMinor;
            set => _versionMinor = value;
        }

        public bool CacheContext
        {
            get => _cacheContext;
            set => _cacheContext = value;
        }

        public RetroHwContextReset ContextDestroy
        {
            set => _contextDestroy = Marshal.GetFunctionPointerForDelegate(value);
        }

        public bool DebugContext
        {
            get => _debugContext;
            set => _debugContext = value;
        }
    }
}