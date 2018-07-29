using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RetroLite.RetroCore
{
    public class CircularBuffer
    {
        private readonly float[] _backingBuffer;

        private int _start;
        private int _end;

        public int Capacity => _backingBuffer.Length;
        public int CurrentLength => _end >= _start ? _end - _start : _end + Capacity - _start;
        public int Glitches { get; set; }
        

        /// <summary>
        /// Creates a circular buffer
        /// </summary>
        /// <param name="size">Size of the buffer in samples</param>
        public CircularBuffer(int size)
        {
            _backingBuffer = new float[size];
            _start = 0;
            _end = 0;
            Glitches = 0;
        }
        
        public void CopyFrom(float[] arr, int length)
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

        public void CopyTo(float[] destination, int length)
        {
            // Zero-fill if the request can't be filled with the current buffer contents
            if (length > CurrentLength)
            {
                Glitches++;
                for (var i = 0; i < length; i++)
                {
                    destination[i] = 0;
                }

                return;
            }

            if (_start + length > Capacity)
            {
                var newLength = Capacity - _start;
                var remainder = length - newLength;

                Buffer.BlockCopy(_backingBuffer, _start, destination, 0, newLength);
                Buffer.BlockCopy(_backingBuffer, 0, destination, newLength, remainder);

                _start = remainder;
            }
            else if (length > 0)
            {
                Buffer.BlockCopy(_backingBuffer, _start, destination, 0, length);

                _start = (_start + length) % Capacity;
            }
        }
    }
}