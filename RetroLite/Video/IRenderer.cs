﻿using System;
using SDL2;

namespace RetroLite.Video
{
    public interface IRenderer
    {
        void Init();
        void Cleanup();

        int GetWidth();
        int GetHeight();

        IntPtr LoadTextureFromFile(string path);
        IntPtr CreateTexture(uint format, SDL.SDL_TextureAccess access, int width, int height);
        int LockTexture(
            IntPtr texture,
            in SDL.SDL_Rect? rect,
            out IntPtr pixels,
            out int pitch
        );
        void UnlockTexture(IntPtr texture);
        void FreeTexture(IntPtr texture);

        void GetRenderDrawColor(out byte r, out byte g, out byte b, out byte a);
        void SetRenderDrawColor(byte r, byte b, byte g, byte a);

        void RenderClear();
        void RenderCopy(IntPtr texture, in SDL.SDL_Rect? src, in SDL.SDL_Rect? dest);
        void RenderPresent();
    }
}