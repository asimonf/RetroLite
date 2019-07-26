using System;
    
namespace RetroLite.RetroCore
{
    public class CircularBuffer<T>
    {
        private readonly T[] _backingBuffer;

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
            _backingBuffer = new T[size];
            _start = 0;
            _end = 0;
            Glitches = 0;
        }

        public void AddSample(T sample)
        {
            if (_end == Capacity)
            {
                _backingBuffer[0] = sample;

                _end = 1;
            }
            else
            {
                
                _end += 1;
            }
        }
        
        public void CopyFrom(T[] arr, int length)
        {
            if (_end + length > Capacity)
            {
                var newLength = Capacity - _end;
                var remainder = length - newLength;

                Array.Copy(arr, 0, _backingBuffer, _end, newLength);
                Array.Copy(arr, newLength, _backingBuffer, 0, remainder);

                _end = remainder;
            }
            else
            {
                Array.Copy(arr, 0, _backingBuffer, _end, length);
                _end = (_end + length) % Capacity;
            }
        }

        public void CopyTo(T[] destination, int length)
        {
            // Zero-fill if the request can't be filled with the current buffer contents
            if (length > CurrentLength)
            {
                Glitches++;
                Console.Write(CurrentLength);
                Console.Write(',');
                Console.Write(length);
                Console.Write('.');

                return;
            }

            if (_start + length > Capacity)
            {
                var newLength = Capacity - _start;
                var remainder = length - newLength;

                Array.Copy(_backingBuffer, _start, destination, 0, newLength);
                Array.Copy(_backingBuffer, 0, destination, newLength, remainder);

                _start = remainder;
            }
            else if (length > 0)
            {
                Array.Copy(_backingBuffer, _start, destination, 0, length);

                _start = (_start + length) % Capacity;
            }
        }
    }
}