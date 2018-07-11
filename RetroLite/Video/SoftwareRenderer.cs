using System;
using SDL2;

namespace RetroLite.Video
{
    public class SoftwareRenderer : IRenderer
    {
        private IntPtr _sdlSurface;
        private IntPtr _sdlRenderer;
        private IntPtr _framebuffer;

        private int _height;
        private int _width;

        public SoftwareRenderer(int width, int height)
        {
            _width = width;
            _height = height;
            
            Console.WriteLine("Initializing Video");
            
            if (SDL.SDL_InitSubSystem(SDL.SDL_INIT_VIDEO) != 0)
            {
                Console.WriteLine("Init error");
                throw new Exception("SDL Video Initialization error");
            }
            
            _sdlSurface = SDL.SDL_CreateRGBSurface(
                0, 
                _width, 
                _height, 
                32,
                0,
                0,
                0,
                0
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

        ~SoftwareRenderer()
        {
            SDL.SDL_DestroyRenderer(_sdlRenderer);
            SDL.SDL_QuitSubSystem(SDL.SDL_INIT_VIDEO);

            _sdlRenderer = IntPtr.Zero;
            _framebuffer = IntPtr.Zero;
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
            SDL.SDL_Delay(5);
        }

        public void Screenshot()
        {
            SDL.SDL_SaveBMP(_sdlSurface, "screenshot.bmp");
        }
    }
}