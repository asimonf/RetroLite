using System;
using SDL2;

namespace RetroLite.Video
{
    public delegate void OnVideoSetHandler(int newWidth, int newHeight, float newRefreshRate, IRenderer self);
    
    public interface IRenderer : IDisposable
    {
        int Width { get; }
        int Height { get; }
        int Lines { get; }
        float RefreshRate { get; }
        bool Initialized { get; }

        void Initialize();

        IntPtr LoadTextureFromFile(string path);
        IntPtr CreateTexture(uint format, SDL.SDL_TextureAccess access, int width, int height);
        int LockTexture(
            IntPtr texture,
            ref SDL.SDL_Rect rect,
            out IntPtr pixels,
            out int pitch
        );
        int LockTexture(
            IntPtr texture,
            out IntPtr pixels,
            out int pitch
        );
        void UnlockTexture(IntPtr texture);
        void FreeTexture(IntPtr texture);

        void GetRenderDrawColor(out byte r, out byte g, out byte b, out byte a);
        void SetRenderDrawColor(byte r, byte b, byte g, byte a);

        void RenderClear();
        void RenderCopy(IntPtr texture);
        void RenderCopy(IntPtr texture, ref SDL.SDL_Rect src, ref SDL.SDL_Rect dest);
        void RenderCopyDest(IntPtr texture, ref SDL.SDL_Rect dest);
        void RenderCopySrc(IntPtr texture, ref SDL.SDL_Rect src);
        void RenderPresent();

        void RenderWaitForVsync();

        bool SetRenderDrawBlendMode(SDL.SDL_BlendMode mode);
        bool SetTextureBlendMode(IntPtr texture, SDL.SDL_BlendMode mode);

        void SetMode(int width, int height);
        void SetMode(int width, int height, float refreshRate);

        void SetInterlacing(bool interlacing);
    }
}