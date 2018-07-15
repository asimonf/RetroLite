using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RetroLite.Audio
{
    public class CircularBuffer
    {
        private readonly byte[] _backingBuffer;

        private int _start;
        private int _end;

        public int Capacity => _backingBuffer.Length;
        public int CurrentLength => _end >= _start ? _end - _start : _end + Capacity - _start;
        public int Glitches { get; set; }
        

        /// <summary>
        /// Creates a circular buffer
        /// </summary>
        /// <param name="size">Size of the buffer in bytes</param>
        public CircularBuffer(int size)
        {
            _backingBuffer = new byte[size];
            _start = 0;
            _end = 0;
            Glitches = 0;
        }
        
        /// <summary>
        /// Copy from the array supplied into the circular buffer
        /// </summary>
        /// <param name="arr">Array to copy from</param>
        /// <param name="length">How many bytes to copy from the pointer</param>
        public void CopyFrom(byte[] arr, int length)
        {
            if (_end + length > Capacity)
            {
                var newLength = Capacity - _end;
                var remainder = length - newLength;

                Buffer.BlockCopy(arr, 0, _backingBuffer, _end, newLength);
                Buffer.BlockCopy(arr, newLength, _backingBuffer, 0, remainder);
                _end = remainder;
            }
            else
            {
                Buffer.BlockCopy(arr, 0, _backingBuffer, _end, length);
                _end = (_end + length) % Capacity;
            }
        }

        /// <summary>
        /// Copy to the ptr supplied from the circular buffer
        /// </summary>
        /// <param name="ptr">Pointer to copy to</param>
        /// <param name="length">How many bytes to copy to the pointer</param>
        public void CopyTo(IntPtr ptr, int length)
        {
            // Zero-fill if the request can't be filled with the current buffer contents
            if (length > CurrentLength)
            {
                Glitches++;
                for (var i = 0; i < length; i++) unsafe
                {
                    ((byte*) ptr.ToPointer())[i] = 0;
                }

                return;
            }

            unsafe
            {
                fixed (byte* backingBufferPtr = &_backingBuffer[0])
                {
                    if (_start + length > Capacity)
                    {
                        var newLength = Capacity - _start;
                        var remainder = length - newLength;

                        Buffer.MemoryCopy(backingBufferPtr + _start, ptr.ToPointer(), newLength, newLength);
                        Buffer.MemoryCopy(backingBufferPtr, (byte*) ptr.ToPointer() + newLength, remainder, remainder);

                        _start = remainder;
                    }
                    else if (length > 0)
                    {
                        Buffer.MemoryCopy(backingBufferPtr + _start, ptr.ToPointer(), length, length);
                        _start = (_start + length) % Capacity;
                    }
                }
            }
        }
    }
}