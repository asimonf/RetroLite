﻿using System;
using System.Runtime.InteropServices;
using SRC_STATE_PTR = System.IntPtr;

namespace SRC_CS
{
    public static unsafe class SampleRate
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct SRC_DATA
        {
            public float* data_in;
            public float* data_out;

            public int input_frames;
            public int output_frames;
            public int input_frames_used;
            public int output_frames_gen;

            public int end_of_input;

            public double src_ratio;
        }

        private const string nativeLibName = "libsamplerate-0.dll";

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate bool src_callback_t(void* cb_data, float** data);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern SRC_STATE_PTR src_new(Quality converter_type, int channels, out int error);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern SRC_STATE_PTR src_clone(SRC_STATE_PTR orig, int* error);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern SRC_STATE_PTR src_callback_new(src_callback_t func, Quality converter_type, int channels,
                int* error, void* cb_data);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern SRC_STATE_PTR src_delete(SRC_STATE_PTR state);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int src_process(SRC_STATE_PTR state, ref SRC_DATA data);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern long src_callback_read(SRC_STATE_PTR state, double src_ratio, long frames, float* data);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int src_simple(ref SRC_DATA data, Quality converter_type, int channels);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr src_get_name(Quality converter_type);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr src_get_description(Quality converter_type);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr src_get_version();

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int src_set_ratio(SRC_STATE_PTR state, double new_ratio);

        /*
        **	Get the current channel count.
        **	Returns negative on error, positive channel count otherwise
        */

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int src_get_channels(SRC_STATE_PTR state);

        /*
        **	Reset the internal SRC state.
        **	Does not modify the quality settings.
        **	Does not free any memory allocations.
        **	Returns non zero on error.
        */

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int src_reset(SRC_STATE_PTR state);

        /*
        ** Return TRUE if ratio is a valid conversion ratio, FALSE
        ** otherwise.
        */

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int src_is_valid_ratio(double ratio);

        /*
        **	Return an error number.
        */

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int src_error(SRC_STATE_PTR state);

        /*
        **	Convert the error number into a string.
        */
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "src_strerror")]
        private static extern IntPtr internal_src_strerror (int error) ;

        public static string src_strerror(int error)
        {
            return Marshal.PtrToStringAnsi(internal_src_strerror(error));
        }

        /*
        ** The following enums can be used to set the interpolator type
        ** using the function src_set_converter().
        */

        public enum Quality : int
        {
            SRC_SINC_BEST_QUALITY = 0,
            SRC_SINC_MEDIUM_QUALITY = 1,
            SRC_SINC_FASTEST = 2,
            SRC_ZERO_ORDER_HOLD = 3,
            SRC_LINEAR = 4,
        };

        /*
        ** Extra helper functions for converting from short to float and
        ** back again.
        */

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void src_short_to_float_array(short* _in, float* _out, int len) ;
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void src_float_to_short_array(float* _in, short* _out, int len) ;

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void src_int_to_float_array(int* _in, float* _out, int len) ;
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void src_float_to_int_array(float* _in, int* _out, int len) ;
    }
}