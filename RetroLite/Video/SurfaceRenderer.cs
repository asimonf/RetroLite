using System;
using System.Runtime.InteropServices;
using LibArvid;
using NLog.Targets;
using SDL2;

namespace RetroLite.Video
{
    public class SurfaceRenderer : IRenderer
    {
        private readonly IntPtr _sdlSurface;
        private readonly IntPtr _sdlRenderer;
        private ushort[] _tempDestination;

        public int Height { get; private set; }
        public int Width { get; private set; }
        public int Lines { get; private set; }
        public float RefreshRate { get; private set; }
        
        public bool Interlacing { get; private set; }
        
        public bool Initialized { get; private set; }
        
        public ArvidClient.ArvidClientVmodeInfo[] ModeInfos { get; private set; }
        
        public SurfaceRenderer()
        {
            Initialized = false;
            
            _sdlSurface = SDL.SDL_CreateRGBSurfaceWithFormat(
                0, 
                640, 
                240, 
                16, 
                SDL.SDL_PIXELFORMAT_RGB555
            );
            
            if (_sdlSurface == null)
                throw new Exception("SDL Software Surface Initialization Error");

            _sdlRenderer = SDL.SDL_CreateSoftwareRenderer(_sdlSurface);

            if (_sdlRenderer == null)
                throw new Exception("SDL Renderer Initialization Error");

            _tempDestination = new ushort[640 * 240];
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

        public void Dispose()
        {
            SDL.SDL_DestroyRenderer(_sdlRenderer);
            SDL.SDL_FreeSurface(_sdlSurface);
        }

        public IntPtr LoadTextureFromFile(string path)
        {
            return SDL_image.IMG_LoadTexture(_sdlRenderer, path);
        }

        public IntPtr CreateTexture(uint format, SDL.SDL_TextureAccess access, int width, int height)
        {
            return SDL.SDL_CreateTexture(_sdlRenderer, format, (int)access, width, height);
        }

        public int LockTexture(IntPtr texture, ref SDL.SDL_Rect rect, out IntPtr pixels, out int pitch)
        {
            return SDL.SDL_LockTexture(texture, ref rect, out pixels, out pitch);
        }

        public int LockTexture(IntPtr texture, out IntPtr pixels, out int pitch)
        {
            return SDL.SDL_LockTexture(texture, IntPtr.Zero, out pixels, out pitch);
        }

        public void UnlockTexture(IntPtr texture)
        {
            SDL.SDL_UnlockTexture(texture);
        }

        public void FreeTexture(IntPtr texture)
        {
            SDL.SDL_DestroyTexture(texture);
        }

        public void GetRenderDrawColor(out byte r, out byte g, out byte b, out byte a)
        {
            SDL.SDL_GetRenderDrawColor(_sdlRenderer, out r, out g, out b, out a);
        }

        public void SetRenderDrawColor(byte r, byte g, byte b, byte a)
        {
            SDL.SDL_SetRenderDrawColor(_sdlRenderer, r, g, b, a);
        }

        public void RenderClear()
        {
            SDL.SDL_RenderClear(_sdlRenderer);
        }

        public void RenderCopy(IntPtr texture, ref SDL.SDL_Rect src, ref SDL.SDL_Rect dest)
        {
            SDL.SDL_RenderCopy(_sdlRenderer, texture, ref src, ref dest);
        }

        public void RenderCopyDest(IntPtr texture, ref SDL.SDL_Rect dest)
        {
            SDL.SDL_RenderCopy(_sdlRenderer, texture, IntPtr.Zero, ref dest);
        }

        public void RenderCopySrc(IntPtr texture, ref SDL.SDL_Rect src)
        {
            SDL.SDL_RenderCopy(_sdlRenderer, texture, ref src, IntPtr.Zero);
        }

        public void RenderCopy(IntPtr texturePtr)
        {
            SDL.SDL_RenderCopy(_sdlRenderer, texturePtr, IntPtr.Zero, IntPtr.Zero);
        }

        public void RenderPresent()
        {
            SDL.SDL_RenderPresent(_sdlRenderer);

            try
            {
                SDL.SDL_LockSurface(_sdlSurface);

                unsafe
                {
                    fixed (ushort* tempDestPtr = &_tempDestination[0])
                    {
                        var surface = (SDL.SDL_Surface*)_sdlSurface.ToPointer();
                        for (var i = 0; i < Height; i++)
                        {
                            Buffer.MemoryCopy(
                                (ushort*)(surface->pixels + i * surface->pitch),
                                tempDestPtr + i * Width,
                                Width * 2,
                                Width * 2
                            );
                        }

                        var res = ArvidClient.arvid_client_blit_buffer(
                            tempDestPtr,
                            Width,
                            Height,
                            Width
                        );

                        if (res != 0)
                            throw new Exception("Error blitting to arvid");
                    }                    
                }
                
            }
            finally
            {
                SDL.SDL_UnlockSurface(_sdlSurface);
            }
        }
        
        public void RenderWaitForVsync()
        {
            ArvidClient.arvid_client_wait_for_vsync();
        }
        
        public bool SetRenderDrawBlendMode(SDL.SDL_BlendMode mode)
        {
            return SDL.SDL_SetRenderDrawBlendMode(_sdlRenderer, mode) == 0;
        }
        
        public bool SetTextureBlendMode(IntPtr texture, SDL.SDL_BlendMode mode)
        {
            return SDL.SDL_SetTextureBlendMode(texture, mode) == 0;
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
        
        public void SetMode(int width, int height, float refreshRate)
        {
            Width = width;
            Height = height;
            var selectedMode = _findVideoMode(width);
            
            Lines = ArvidClient.arvid_client_get_video_mode_lines(selectedMode, refreshRate) - 1;
            if (Lines < 0) throw new Exception("Could not find a mode that satisfies the selected refresh rate");

            var res = ArvidClient.arvid_client_set_video_mode(selectedMode, Lines);
            if (res < 0) throw new Exception("Unable to set Arvid Mode");
            
            RefreshRate = ArvidClient.arvid_client_get_video_mode_refresh_rate(selectedMode, Lines);
            if (RefreshRate < 0) throw new Exception("Unable to get final refresh rate");
            
            ArvidClient.arvid_client_set_virtual_vsync(Height - 15);
            
            Console.WriteLine("{0}x{1}@{2}. Asked for {3}", Width, Height, RefreshRate, refreshRate);
        }
        
        public void SetMode(int width, int height)
        {
            Width = width;
            Height = height;
            var selectedMode = _findVideoMode(width);

            var res = ArvidClient.arvid_client_set_video_mode(selectedMode, Lines);
            if (res < 0) throw new Exception("Unable to set Arvid Mode");
            
            ArvidClient.arvid_client_set_virtual_vsync(Height - 15);
            
//            if (IntPtr.Zero != _tempDestination) FreeTexture(_tempDestination);
//            _tempDestination = CreateTexture(
//                SDL.SDL_PIXELFORMAT_RGB555,
//                SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING,
//                Width,
//                Height
//            );
        }

        public void SetInterlacing(bool interlacing)
        {
            Interlacing = interlacing;
            ArvidClient.arvid_client_set_interlacing((short)(interlacing ? 1 : 0));
        }
    }
}