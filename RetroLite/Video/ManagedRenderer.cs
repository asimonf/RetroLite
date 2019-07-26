using System;
using NLog;
using SDL2;

namespace RetroLite.Video
{
    public class ManagedRenderer: IRenderer
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        
        private IRenderer _surfaceRenderer;
        private IntPtr _sdlWindow;
        
        
        public int Width => _surfaceRenderer?.Width ?? 0;
        public int Height => _surfaceRenderer?.Height ?? 0;
        public float RefreshRate => _surfaceRenderer?.RefreshRate ?? 0f;
        
        public event OnVideoSetHandler OnVideoSet;
        
        public object Sync { get; }

        public ManagedRenderer()
        {
            Sync = new object();

            SetMode(512, 240, 60.0f);
            
            _sdlWindow = SDL.SDL_CreateWindow(
                "RetroLite", 
                SDL.SDL_WINDOWPOS_UNDEFINED, 
                SDL.SDL_WINDOWPOS_UNDEFINED, 
                320, 
                252, 
                SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS
            );
        }

        public void Dispose()
        {
            _surfaceRenderer?.Dispose();
            SDL.SDL_DestroyWindow(_sdlWindow);
        }

        public IntPtr LoadTextureFromFile(string path)
        {
            return _surfaceRenderer.LoadTextureFromFile(path);
        }

        public IntPtr CreateTexture(uint format, SDL.SDL_TextureAccess access, int width, int height)
        {
            return _surfaceRenderer.CreateTexture(format, access, width, height);
        }

        public int LockTexture(IntPtr texture, ref SDL.SDL_Rect rect, out IntPtr pixels, out int pitch)
        {
            return _surfaceRenderer.LockTexture(texture, ref rect, out pixels, out pitch);
        }

        public int LockTexture(IntPtr texture, out IntPtr pixels, out int pitch)
        {
            return _surfaceRenderer.LockTexture(texture, out pixels, out pitch);
        }

        public void UnlockTexture(IntPtr texture)
        {
            _surfaceRenderer.UnlockTexture(texture);
        }

        public void FreeTexture(IntPtr texture)
        {
            _surfaceRenderer.FreeTexture(texture);
        }

        public void GetRenderDrawColor(out byte r, out byte g, out byte b, out byte a)
        {
            _surfaceRenderer.GetRenderDrawColor(out r, out g, out b, out a);
        }

        public void SetRenderDrawColor(byte r, byte b, byte g, byte a)
        {
            _surfaceRenderer.SetRenderDrawColor(r, b, g, a);
        }

        public void RenderClear()
        {
            _surfaceRenderer.RenderClear();
        }

        public void RenderCopy(IntPtr texture)
        {
            _surfaceRenderer.RenderCopy(texture);
        }

        public void RenderCopy(IntPtr texture, ref SDL.SDL_Rect src, ref SDL.SDL_Rect dest)
        {
            _surfaceRenderer.RenderCopy(texture, ref src, ref dest);
        }

        public void RenderCopyDest(IntPtr texture, ref SDL.SDL_Rect dest)
        {
            _surfaceRenderer.RenderCopyDest(texture, ref dest);
        }

        public void RenderCopySrc(IntPtr texture, ref SDL.SDL_Rect src)
        {
            _surfaceRenderer.RenderCopySrc(texture, ref src);
        }

        public void RenderPresent()
        {
            _surfaceRenderer.RenderPresent();
        }

        public bool SetRenderDrawBlendMode(SDL.SDL_BlendMode mode)
        {
            var res2 = _surfaceRenderer.SetRenderDrawBlendMode(mode);

            return true && res2;
        }

        public bool SetTextureBlendMode(IntPtr texture, SDL.SDL_BlendMode mode)
        {
            return _surfaceRenderer.SetTextureBlendMode(texture, mode);
        }

        public void Screenshot()
        {
        }

        public void SetTitleText(string title)
        {
        }

        public void SetMode(int width, int height, float refreshRate)
        {
            if (width == Width && height == Height && Math.Abs(refreshRate - RefreshRate) <= 0.001f) return;

            lock (Sync)
            {
                var newRenderer = new SurfaceRenderer(width, height, refreshRate);
            
                _logger.Info(
                    "Video-Mode set to {width}x{height}@{refreshRate}",
                    newRenderer.Width,
                    newRenderer.Height,
                    newRenderer.RefreshRate
                );

                OnVideoSet?.Invoke(Width, Height, RefreshRate, newRenderer);

                _surfaceRenderer = newRenderer;
            }
        }
    }
}