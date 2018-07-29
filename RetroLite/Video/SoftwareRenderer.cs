using System;
using SDL2;

namespace RetroLite.Video
{
    public class SoftwareRenderer : IRenderer
    {
        private readonly IntPtr _sdlWindow;
        private readonly IntPtr _sdlRenderer;

        public int Height { get; }
        public int Width { get; }

        public SoftwareRenderer()
        {
            if (SDL.SDL_InitSubSystem(SDL.SDL_INIT_VIDEO) != 0)
            {
                throw new Exception("SDL Video Initialization error");
            }
            
            Width = 800;
            Height = 600;
            
            _sdlWindow = SDL.SDL_CreateWindow(
                "RetroLite", 
                SDL.SDL_WINDOWPOS_UNDEFINED, 
                SDL.SDL_WINDOWPOS_UNDEFINED, 
                Width, 
                Height, 
                SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS
            );
            
            if (_sdlWindow == null)
            {
                throw new Exception("SDL Window Initialization Error");
            }

            _sdlRenderer = SDL.SDL_CreateRenderer(_sdlWindow, -1,
                SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED |
//                SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC | 
                SDL.SDL_RendererFlags.SDL_RENDERER_TARGETTEXTURE);

            if (_sdlRenderer == null)
            {
                throw new Exception("SDL Renderer Initialization Error");
            }

            SetRenderDrawBlendMode(SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
        }

        public void Dispose()
        {
            SDL.SDL_DestroyRenderer(_sdlRenderer);
            SDL.SDL_DestroyWindow(_sdlWindow);
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

        public bool SetRenderDrawBlendMode(SDL.SDL_BlendMode mode)
        {
            return SDL.SDL_SetRenderDrawBlendMode(_sdlRenderer, mode) == 0;
        }
        
        public bool SetTextureBlendMode(IntPtr texture, SDL.SDL_BlendMode mode)
        {
            return SDL.SDL_SetTextureBlendMode(texture, mode) == 0;
        }

        public void Screenshot()
        {
            
        }

        public void SetTitleText(string title)
        {
            SDL.SDL_SetWindowTitle(_sdlWindow, title);
        }
    }
}