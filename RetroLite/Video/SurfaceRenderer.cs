using System;
using SDL2;

namespace RetroLite.Video
{
    public class SurfaceRenderer : IRenderer
    {
        private readonly IntPtr _sdlSurface;
        private readonly IntPtr _sdlRenderer;

        public int Height { get; }
        public int Width { get; }

        public SurfaceRenderer(int width, int height)
        {
            if (SDL.SDL_InitSubSystem(SDL.SDL_INIT_VIDEO) != 0)
            {
                throw new Exception("SDL Video Initialization error");
            }
            
            Width = width;
            Height = height;
            
            _sdlSurface = SDL.SDL_CreateRGBSurface(0, Width, Height, 32, 0, 0, 0, 0);

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

        public void Dispose()
        {
            SDL.SDL_DestroyRenderer(_sdlRenderer);
            SDL.SDL_FreeSurface(_sdlSurface);
            SDL.SDL_QuitSubSystem(SDL.SDL_INIT_VIDEO);
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

        public void RenderCopy(IntPtr texture)
        {
            SDL.SDL_RenderCopy(_sdlRenderer, texture, IntPtr.Zero, IntPtr.Zero);
        }

        public void RenderPresent()
        {
            SDL.SDL_RenderPresent(_sdlRenderer);
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
    }
}