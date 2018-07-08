using System;
using SDL2;

namespace RetroLite.Video
{
    public class SdlRenderer : IRenderer
    {
        private IntPtr _sdlWindow;
        private IntPtr _sdlRenderer;
        private IntPtr _framebuffer;

        private int _height;
        private int _width;

        public SdlRenderer(int width, int height)
        {
            _width = width;
            _height = height;
        }

        public void Init()
        {
            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) != 0)
            {
                Console.WriteLine("Init error");
                throw new Exception("SDL Video Initialization error");
            }
            
            _sdlWindow = SDL.SDL_CreateWindow(
                "RetroLite", 
                SDL.SDL_WINDOWPOS_UNDEFINED, 
                SDL.SDL_WINDOWPOS_UNDEFINED, 
                _height, 
                _width, 
                SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE
            );

            if (_sdlWindow == null)
            {
                throw new Exception("SDL Window Initialization Error");
            }

            _framebuffer = SDL.SDL_GetWindowSurface(_sdlWindow);
            _sdlRenderer = SDL.SDL_CreateRenderer(_sdlWindow, -1,
                SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);

            if (_sdlRenderer == null)
            {
                throw new Exception("SDL Renderer Initialization Error");
            }
        }

        public void Cleanup()
        {
            SDL.SDL_DestroyRenderer(_sdlRenderer);
            SDL.SDL_DestroyWindow(_sdlWindow);

            _sdlRenderer = IntPtr.Zero;
            _sdlWindow = IntPtr.Zero;
        }

        public int GetWidth()
        {
            return _width;
        }

        public int GetHeight()
        {
            return _height;
        }

        public IntPtr LoadTextureFromFile(string path)
        {
            return SDL_image.IMG_LoadTexture(_sdlRenderer, path);
        }

        public IntPtr CreateTexture(uint format, SDL.SDL_TextureAccess access, int width, int height)
        {
            return SDL.SDL_CreateTexture(_sdlRenderer, format, (int)access, width, height);
        }

        public int LockTexture(IntPtr texture, in SDL.SDL_Rect? rect, out IntPtr pixels, out int pitch)
        {
            return SDL.SDL_LockTexture(_sdlRenderer, in rect, out pixels, out pitch);
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

        public void RenderCopy(IntPtr texture, in SDL.SDL_Rect? src, in SDL.SDL_Rect? dest)
        {
            SDL.SDL_RenderCopy(_sdlRenderer, texture, in src, in dest);
        }

        public void RenderPresent()
        {
            SDL.SDL_RenderPresent(_sdlRenderer);
        }
    }
}