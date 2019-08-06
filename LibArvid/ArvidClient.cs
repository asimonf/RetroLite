using System;
using System.Runtime.InteropServices;

namespace LibArvid
{
    public static unsafe class ArvidClient
    {
        public enum VideoModeShort: short
        {
            Invalid = -1,
            Arvid320 = 0,
            Arvid256 = 1,
            Arvid288 = 2,
            Arvid384 = 3,
            Arvid240 = 4,
            Arvid392 = 5,
            Arvid400 = 6,
            Arvid292 = 7,
            Arvid336 = 8,
            Arvid416 = 9,
            Arvid448 = 10,
            Arvid512 = 11,
            Arvid640 = 12,
            Count = 12
        }
        
        public enum VideoModeInt: int
        {
            Invalid = -1,
            Arvid320 = 0,
            Arvid256 = 1,
            Arvid288 = 2,
            Arvid384 = 3,
            Arvid240 = 4,
            Arvid392 = 5,
            Arvid400 = 6,
            Arvid292 = 7,
            Arvid336 = 8,
            Arvid416 = 9,
            Arvid448 = 10,
            Arvid512 = 11,
            Arvid640 = 12,
            Count = 12
        }
        
        public enum BlitType: int
        {
            Blocking    = 0,
            NonBlocking = 1,
        }
        
        [StructLayout(LayoutKind.Sequential)]
        public struct ArvidClientVmodeInfo
        {
            public ushort width;
            public VideoModeShort vmode;
        }
        
        private const string nativeLibName = "arvid_client.dll";

        public static bool IsConnected { get; private set; }

        public static bool Connect(string ip)
        {
            if (IsConnected)
            {
                return false;
            }
            
            var ipPtr = Marshal.StringToHGlobalAnsi(ip);

            if (arvid_client_connect(ipPtr) < 0)
            {
                return false;
            }
            
            IsConnected = true;
            return true;
        }

        public static bool Close()
        {
            if (!IsConnected)
            {
                return false;
            }

            return arvid_client_close() == 0;
        }
        
        /// <summary>
        /// connects the client to the arvid-server
        /// </summary>
        /// <param name="serverAddress"></param>
        /// <returns></returns>
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int arvid_client_connect(IntPtr serverAddress);
        
        /// <summary>
        /// closes arvid
        /// </summary>
        /// <returns></returns>
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int arvid_client_close();
        
        /// <summary>
        /// closes arvid
        /// </summary>
        /// <returns></returns>
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int arvid_client_set_blit_type(BlitType type);

        /// <summary>
        /// sends the video frame to hidden arvid buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="stride"></param>
        /// <returns>returns 0 on success, -1 on failure</returns>
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int arvid_client_blit_buffer(ushort* buffer, int width, int height,  int stride);
        
        /// <summary>
        /// returns current frame number
        /// </summary>
        /// <returns></returns>
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint arvid_client_get_frame_number();

        /// <summary>
        /// waits for a vsync then returns current frame number
        /// </summary>
        /// <returns></returns>
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint arvid_client_wait_for_vsync();

        /// <summary>
        /// sets arvid video mode
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="lines"></param>
        /// <returns>return 0 on success</returns>
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int arvid_client_set_video_mode(VideoModeInt mode, int lines);

        /// <summary>
        /// returns the number of video lines (vertical resolution) of specified videomode
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="frequency"></param>
        /// <returns></returns>
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int arvid_client_get_video_mode_lines(VideoModeInt mode, float frequency);

        /// <summary>
        /// returns the screen refresh rate for particular video mode and number of lines of that video mode
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="lines"></param>
        /// <returns></returns>
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float arvid_client_get_video_mode_refresh_rate(VideoModeInt mode, int lines);

        /// <summary>
        /// enumerates available video modes
        /// vmodes and maxItem must not be NULL
        /// maxItem must be set to number of items the vmodes buffer can hold.
        /// </summary>
        /// <param name="vmodes"></param>
        /// <param name="maxItem"></param>
        /// <returns>returns number of videomodes on success, -1 on error</returns>
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int arvid_client_enum_video_modes(ArvidClientVmodeInfo* vmodes, int maxItem);
        
        /// <summary>
        /// get the width in pixels of current video mode or negative number on error
        /// </summary>
        /// <returns></returns>
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int arvid_client_get_width();

        /// <summary>
        /// get number of lines of current video mode or negative number on error
        /// </summary>
        /// <returns></returns>
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int arvid_client_get_height();

        /// <summary>
        /// get button state (coins and switches)
        /// </summary>
        /// <returns></returns>
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int arvid_client_get_button_status();

        /// <summary>
        /// Gets a number of bytes transferred to server in between the calls of this function
        /// </summary>
        /// <returns></returns>
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint arvid_client_get_stat_transferred_size();
        
        /// <summary>
        /// set the line position modifier
        /// </summary>
        /// <param name="mod"></param>
        /// <returns></returns>
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int arvid_client_set_line_pos_mod(short mod);
        
        /// <summary>
        /// set interlacing mode
        /// </summary>
        /// <param name="enabled"></param>
        /// <returns></returns>
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int arvid_client_set_interlacing(short enabled);

        /// <summary>
        /// get the line position modifier
        /// </summary>
        /// <returns></returns>
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int arvid_client_get_line_pos_mod();

        /// <summary>
        /// set virtual vsync line. use -1 to disable
        /// </summary>
        /// <param name="vsyncLine"></param>
        /// <returns></returns>
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int arvid_client_set_virtual_vsync(int vsyncLine);

        /// <summary>
        /// sends an update file to the server
        /// </summary>
        /// <param name="updateData"></param>
        /// <param name="updateSize"></param>
        /// <returns></returns>
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int arvid_client_update_server(IntPtr updateData, int updateSize);

        /// <summary>
        /// Issues power-off command to the arvid server.
        /// This closes arvid connection and casues arvid server to power off.  
        /// </summary>
        /// <returns></returns>
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int arvid_client_power_off_server();
    }
}