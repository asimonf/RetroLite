using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using LibRetro.Types;

namespace LibRetro
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RetroVfsFileHandle
    {
        public readonly IntPtr handle;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.LPStr)]
    public delegate string RetroVfsGetPath(ref RetroVfsFileHandle stream);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr RetroVfsOpen([MarshalAs(UnmanagedType.LPStr)] string path, uint mode, uint hints);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int RetroVfsClose(ref RetroVfsFileHandle stream);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate long RetroVfsSize(ref RetroVfsFileHandle stream);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate long RetroVfsTell(ref RetroVfsFileHandle stream);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate long RetroVfsSeek(ref RetroVfsFileHandle stream, long offset, int seek);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate long RetroVfsRead(ref RetroVfsFileHandle stream, IntPtr s, ulong len);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate long RetroVfsWrite(ref RetroVfsFileHandle stream, IntPtr s, ulong len);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int RetroVfsFlush(ref RetroVfsFileHandle stream);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int RetroVfsRemove([MarshalAs(UnmanagedType.LPStr)] string path);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int RetroVfsRename([MarshalAs(UnmanagedType.LPStr)] string oldPath,
        [MarshalAs(UnmanagedType.LPStr)] string newPath);

    public class RetroVfs : IDisposable
    {
        private readonly Dictionary<IntPtr, FileStream> _fileDictionary;

        private readonly RetroVfsGetPath _getPathCallback;
        private readonly RetroVfsOpen _openCallback;
        private readonly RetroVfsClose _closeCallback;
        private readonly RetroVfsSize _sizeCallback;
        private readonly RetroVfsTell _tellCallback;
        private readonly RetroVfsSeek _seekCallback;
        private readonly RetroVfsRead _readCallback;
        private readonly RetroVfsWrite _writeCallback;
        private readonly RetroVfsFlush _flushCallback;
        private readonly RetroVfsRemove _removeCallback;
        private readonly RetroVfsRename _renameCallback;

        public RetroVfs()
        {
            _fileDictionary = new Dictionary<IntPtr, FileStream>();

            _getPathCallback = GetPath;
            _openCallback = Open;
            _closeCallback = Close;
            _sizeCallback = Size;
            _tellCallback = Tell;
            _seekCallback = Seek;
            _readCallback = Read;
            _writeCallback = Write;
            _flushCallback = Flush;
            _removeCallback = Remove;
            _renameCallback = Rename;
        }

        public IntPtr GetInterfacePtr()
        {
            var iface = new RetroVfsInterface
            {
                GetPath = Marshal.GetFunctionPointerForDelegate(_getPathCallback),
                Open = Marshal.GetFunctionPointerForDelegate(_openCallback),
                Close = Marshal.GetFunctionPointerForDelegate(_closeCallback),
                Size = Marshal.GetFunctionPointerForDelegate(_sizeCallback),
                Tell = Marshal.GetFunctionPointerForDelegate(_tellCallback),
                Seek = Marshal.GetFunctionPointerForDelegate(_seekCallback),
                Read = Marshal.GetFunctionPointerForDelegate(_readCallback),
                Write = Marshal.GetFunctionPointerForDelegate(_writeCallback),
                Flush = Marshal.GetFunctionPointerForDelegate(_flushCallback),
                Remove = Marshal.GetFunctionPointerForDelegate(_removeCallback),
                Rename = Marshal.GetFunctionPointerForDelegate(_renameCallback)
            };


            var ret = Marshal.AllocHGlobal(Marshal.SizeOf(iface));
            Marshal.StructureToPtr(iface, ret, false);

            return ret;
        }

        private string GetPath(ref RetroVfsFileHandle stream)
        {
            return !_fileDictionary.ContainsKey(stream.handle) ? null : _fileDictionary[stream.handle].Name;
        }

        private IntPtr Open(string path, uint mode, uint hints)
        {
            FileMode fileMode;

            if ((mode & Constants.RetroVfsFileAccessUpdateExisting) == Constants.RetroVfsFileAccessUpdateExisting)
            {
                fileMode = FileMode.Append;
            }
            else if ((mode & Constants.RetroVfsFileAccessReadWrite) == Constants.RetroVfsFileAccessReadWrite)
            {
                fileMode = FileMode.Create;
            }
            else
            {
                fileMode = FileMode.Open;
            }

            try
            {
                var stream = File.Open(path, fileMode);

                Debug.Assert(stream.SafeFileHandle != null, "stream.SafeFileHandle != null");
                
                var hPtr = stream.SafeFileHandle.DangerousGetHandle();
                _fileDictionary.Add(hPtr, stream);

                return hPtr;
            }
            catch
            {
                return IntPtr.Zero;
            }
        }

        private int Close(ref RetroVfsFileHandle stream)
        {
            var hPtr = stream.handle;

            if (!_fileDictionary.ContainsKey(hPtr))
            {
                return -1;
            }

            try
            {
                using (var fileStream = _fileDictionary[hPtr])
                {
                    if (!_fileDictionary.Remove(hPtr))
                    {
                        return -1;
                    }

                    fileStream.Close();
                    return 0;
                }
            }
            catch
            {
                return -1;
            }
        }

        private long Size(ref RetroVfsFileHandle stream)
        {
            var hPtr = stream.handle;

            if (!_fileDictionary.ContainsKey(hPtr))
            {
                return -1;
            }

            return _fileDictionary[hPtr].Length;
        }

        private long Tell(ref RetroVfsFileHandle stream)
        {
            var hPtr = stream.handle;

            if (!_fileDictionary.ContainsKey(hPtr))
            {
                return -1;
            }

            return _fileDictionary[hPtr].Position;
        }

        private long Seek(ref RetroVfsFileHandle stream, long offset, int seek)
        {
            var hPtr = stream.handle;

            if (!_fileDictionary.ContainsKey(hPtr))
            {
                return -1;
            }

            SeekOrigin seekOrigin;

            switch (seek)
            {
                case 0:
                    seekOrigin = SeekOrigin.Begin;
                    break;
                case 1:
                    seekOrigin = SeekOrigin.Current;
                    break;
                case 2:
                    seekOrigin = SeekOrigin.End;
                    break;
                default:
                    return -1;
            }

            return _fileDictionary[hPtr].Seek(offset, seekOrigin);
        }

        private long Read(ref RetroVfsFileHandle stream, IntPtr s, ulong len)
        {
            var hPtr = stream.handle;

            if (!_fileDictionary.ContainsKey(hPtr) || len > int.MaxValue)
            {
                return -1;
            }

            var data = new byte[len];
            var fileStream = _fileDictionary[hPtr];

            var res = fileStream.Read(data, 0, (int) len);
            Marshal.Copy(data, 0, s, data.Length);

            return res;
        }

        private long Write(ref RetroVfsFileHandle stream, IntPtr s, ulong len)
        {
            var hPtr = stream.handle;

            if (!_fileDictionary.ContainsKey(hPtr) || len > int.MaxValue)
            {
                return -1;
            }

            var data = new byte[len];
            Marshal.Copy(s, data, 0, (int) len);

            _fileDictionary[hPtr].Write(data, 0, (int) len);

            return (int) len;
        }

        private int Flush(ref RetroVfsFileHandle stream)
        {
            var hPtr = stream.handle;

            if (!_fileDictionary.ContainsKey(hPtr))
            {
                return -1;
            }

            _fileDictionary[hPtr].Flush();

            return 0;
        }

        private int Remove([MarshalAs(UnmanagedType.LPStr)] string path)
        {
            try
            {
                File.Delete(path);
                return 0;
            }
            catch
            {
                return -1;
            }
        }

        private int Rename([MarshalAs(UnmanagedType.LPStr)] string oldPath,
            [MarshalAs(UnmanagedType.LPStr)] string newPath)
        {
            try
            {
                File.Move(oldPath, newPath);
                return 0;
            }
            catch
            {
                return -1;
            }
        }

        public void Dispose()
        {
            foreach (var stream in _fileDictionary.Values)
            {
                try
                {
                    stream.Dispose();
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }
    }
}