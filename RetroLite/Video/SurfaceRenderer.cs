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

        public int Height { get; set; }
        public int Width { get; }
        public float RefreshRate { get; }
        
        public event OnVideoSetHandler OnVideoSet;
        
        public object Sync { get; }

        public SurfaceRenderer(int width, int height, float refreshRate)
        {
            Width = width;
            Height = height;
            RefreshRate = SetArvidMode(width, height, refreshRate);
            Sync = new object();

                _sdlSurface = SDL.SDL_CreateRGBSurfaceWithFormat(
                0, 
                Width, 
                Height, 
                16, 
                SDL.SDL_PIXELFORMAT_RGB555
            );

            if (_sdlSurface == null)
            {
                throw new Exception("SDL Software Surface Initialization Error");
            }

            _sdlRenderer = SDL.SDL_CreateSoftwareRenderer(_sdlSurface);

            if (_sdlRenderer == null)
            {
                throw new Exception("SDL Renderer Initialization Error");
            }
        }

        private unsafe float SetArvidMode(int width, int height, float refreshRate)
        {
            if (ArvidClient.IsConnected)
            {
                var vmodeInfos = new ArvidClient.ArvidClientVmodeInfo[20];

                fixed (ArvidClient.ArvidClientVmodeInfo* vmodesInfosPtr = vmodeInfos)
                {
                    var count = ArvidClient.arvid_client_enum_video_modes(vmodesInfosPtr, 20);
                    
                    Console.WriteLine(count);

                    for (var i = 0; i < count; i++)
                    {
                        if (vmodesInfosPtr[i].width != (ushort) width) continue;

                        var mode = (ArvidClient.VideoModeInt) vmodesInfosPtr[i].vmode;

                        var lines = ArvidClient.arvid_client_get_video_mode_lines(mode, refreshRate);
                        
                        if (lines < 0) throw new Exception("Could not find a mode that satisfies the selected refresh rate");

                        var res = ArvidClient.arvid_client_set_video_mode(mode, lines);
                        
                        if (res < 0) throw new Exception("Unable to set Arvid Mode");

                        res = ArvidClient.arvid_client_set_blit_type(ArvidClient.BlitType.Blocking);
                        
                        if (res < 0) throw new Exception("Unable to set Arvid Blit Type");

                        var finalRefreshRate = ArvidClient.arvid_client_get_video_mode_refresh_rate(mode, lines);
                        
                        if (finalRefreshRate < 0) throw new Exception("Unable to get final refresh rate");

                        return finalRefreshRate;
                    }
                }
            }

            throw new Exception("Not connected to Arvid");
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
            var destRect = new SDL.SDL_Rect()
            {
                h = Height,
                w = Width,
                x = 0,
                y = 0
            };
            SDL.SDL_RenderCopy(_sdlRenderer, texturePtr, IntPtr.Zero, ref destRect);
        }

        public void RenderPresent()
        {
            SDL.SDL_RenderPresent(_sdlRenderer);

            try
            {
                SDL.SDL_LockSurface(_sdlSurface);

                unsafe
                {
                    var surface = (SDL.SDL_Surface*)_sdlSurface.ToPointer();
                    ArvidClient.arvid_client_wait_for_vsync();
                    var res = ArvidClient.arvid_client_blit_buffer(
                        (ushort*)surface->pixels,
                        Width,
                        Height,
                        surface->pitch / 2
                    );

                    if (res != 0)
                    {
                        throw new Exception("Error blitting to arvid");
                    }
                }
            }
            finally
            {
                SDL.SDL_UnlockSurface(_sdlSurface);
            }                
        }

        public void Screenshot()
        {
        }
        
        public bool SetRenderDrawBlendMode(SDL.SDL_BlendMode mode)
        {
            return SDL.SDL_SetRenderDrawBlendMode(_sdlRenderer, mode) == 0;
        }
        
        public bool SetTextureBlendMode(IntPtr texture, SDL.SDL_BlendMode mode)
        {
            return SDL.SDL_SetTextureBlendMode(texture, mode) == 0;
        }

        public void SetTitleText(string title)
        {
            
        }

        public void SetMode(int width, int height, float refreshRate)
        {
            
        }

        public void SetInterlacing(bool interlacing)
        {
            ArvidClient.arvid_client_set_interlacing((short)(interlacing ? 1 : 0));
        }
    }
}