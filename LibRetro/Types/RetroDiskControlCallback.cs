using System;
using System.Runtime.InteropServices;

namespace LibRetro.Types
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool RetroSetEjectState(bool ejected);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool RetroGetEjectState();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate uint RetroGetImageIndex();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool RetroSetImageIndex(uint index);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate uint RetroGetNumImages();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool RetroReplaceImageIndex(uint index);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool RetroAddImageIndex();
        
    [StructLayout(LayoutKind.Sequential)]
    public struct RetroDiskControlCallback
    {
        private IntPtr _setEjectState;
        private IntPtr _getEjectState;

        private IntPtr _getImageIndex;
        private IntPtr _setImageIndex;
        private IntPtr _getNumImages;

        private IntPtr _replaceImageIndex;
        private IntPtr _addImageIndex;

        public RetroSetEjectState SetEjectState
        {
            set => _setEjectState = Marshal.GetFunctionPointerForDelegate(value);
        }

        public RetroGetEjectState GetEjectState
        {
            set => _getEjectState = Marshal.GetFunctionPointerForDelegate(value);
        }

        public RetroGetImageIndex GetImageIndex
        {
            set => _getImageIndex = Marshal.GetFunctionPointerForDelegate(value);
        }

        public RetroSetImageIndex SetImageIndex
        {
            set => _setImageIndex = Marshal.GetFunctionPointerForDelegate(value);
        }

        public RetroGetNumImages GetNumImages
        {
            set => _getNumImages = Marshal.GetFunctionPointerForDelegate(value);
        }

        public RetroReplaceImageIndex ReplaceImageIndex
        {
            set => _replaceImageIndex = Marshal.GetFunctionPointerForDelegate(value);
        }

        public RetroAddImageIndex AddImageIndex
        {
            set => _addImageIndex = Marshal.GetFunctionPointerForDelegate(value);
        }
    }
}