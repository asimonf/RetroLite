using System;
using System.Runtime.InteropServices;
using LibArvid;
using NLog;
using NLog.Targets;
using SDL2;

namespace RetroLite.Video
{
    public class RawRenderer
    {
        private readonly ushort[] _framebuffer;

        public bool Initialized { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }
        
        public int Lines { get; private set; }
        
        public float RefreshRate { get; private set; }
        
        public ArvidClient.ArvidClientVmodeInfo[] ModeInfos { get; private set; }
        private ArvidClient.VideoModeInt _selectedMode = ArvidClient.VideoModeInt.Invalid;

        public RawRenderer()
        {
            Initialized = false;
            _framebuffer = new ushort[480000];
            ModeInfos = null;
            
            SDL.SDL_CreateWindow(
                "RetroLite", 
                SDL.SDL_WINDOWPOS_UNDEFINED, 
                SDL.SDL_WINDOWPOS_UNDEFINED, 
                320, 
                252, 
                SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS
            );
        }

        public void Initialize()
        {
            if (Initialized) return;
            
            unsafe
            {
                int count;
                var vmodeInfos = new ArvidClient.ArvidClientVmodeInfo[20];
                fixed (ArvidClient.ArvidClientVmodeInfo* vmodesInfosPtr = vmodeInfos)
                    count = ArvidClient.arvid_client_enum_video_modes(vmodesInfosPtr, 20);
                
                ModeInfos = new ArvidClient.ArvidClientVmodeInfo[count];
                Array.Copy(vmodeInfos, ModeInfos, count);
            }
            
            var res = ArvidClient.arvid_client_set_blit_type(ArvidClient.BlitType.Blocking);
            if (res < 0) throw new Exception("Unable to set Arvid Blit Type");

            Initialized = true;
        }

        public void RenderClear()
        {
            Array.Clear(_framebuffer, 0, _framebuffer.Length);
//            var length = Width * Height;
//            for (var i = 0; i < length; i++)
//            {
//                _framebuffer[i] = (ushort)(i % Width);
//            }
        }

        public void RenderCopy(ushort[] data)
        {
            Array.Copy(data, _framebuffer, Height * Width);
        }

        public unsafe void RenderPresent()
        {
            fixed (ushort* surface = &_framebuffer[0])
            {
                var res = ArvidClient.arvid_client_blit_buffer(
                    surface,
                    Width,
                    Height,
                     Width
                );
                
                if (res != 0)
                {
                    throw new Exception("Error blitting to arvid");
                }
            }
        }

        public void RenderWaitForVsync()
        {
            ArvidClient.arvid_client_wait_for_vsync();
        }

        private ArvidClient.VideoModeInt _findVideoMode(int width)
        {
            for (var i = 0; i < ModeInfos.Length; i++)
            {
                if (ModeInfos[i].width != (ushort) width) continue;

                return (ArvidClient.VideoModeInt) ModeInfos[i].vmode;
            }

            return ArvidClient.VideoModeInt.Invalid;
        }

        public void SetMode(int width, int height)
        {
            Width = width;
            Height = height;
            _selectedMode = _findVideoMode(width);

            var res = ArvidClient.arvid_client_set_video_mode(_selectedMode, Lines);
            if (res < 0) throw new Exception("Unable to set Arvid Mode");
        }

        public void SetMode(int width, int height, float refreshRate)
        {
            Width = width;
            Height = height;
            _selectedMode = _findVideoMode(width);
            
            Lines = ArvidClient.arvid_client_get_video_mode_lines(_selectedMode, refreshRate);
            if (Lines < 0) throw new Exception("Could not find a mode that satisfies the selected refresh rate");

            var res = ArvidClient.arvid_client_set_video_mode(_selectedMode, Lines);
            if (res < 0) throw new Exception("Unable to set Arvid Mode");
            
            RefreshRate = ArvidClient.arvid_client_get_video_mode_refresh_rate(_selectedMode, Lines);
            if (RefreshRate < 0) throw new Exception("Unable to get final refresh rate");
            
            Console.WriteLine("{0}x{1}@{2}", Width, Height, RefreshRate);
        }

        public void SetVMode(int mode)
        {
            var width = ModeInfos[mode].width;
            var height = 224;
            var refreshRate = 60;
            
            SetMode(width, height, refreshRate);
        }

        public void SetInterlacing(bool interlacing)
        {
            ArvidClient.arvid_client_set_interlacing((short)(interlacing ? 1 : 0));
        }
    }
}