﻿using System;
using RetroLite.Video;
using SDL2;
using Xilium.CefGlue;

namespace RetroLite.Menu
{
    public class MenuRenderer : CefRenderHandler, IDisposable
    {
        private readonly IntPtr _texture;
        private readonly IRenderer _renderer;

        private bool _newFrameReady = false;

        public MenuRenderer(IRenderer renderer)
        {
            _renderer = renderer;

            var format = SDL.SDL_PIXELFORMAT_ARGB8888;

            _texture = _renderer.CreateTexture(
                format,
                SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING,
                _renderer.Width,
                _renderer.Height
            );

            _renderer.SetTextureBlendMode(_texture, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
        }

        protected override bool GetViewRect(CefBrowser browser, ref CefRectangle rect)
        {
            rect = new CefRectangle(0, 0, _renderer.Width, _renderer.Height);

            return true;
        }

        protected override unsafe void OnPaint(CefBrowser browser, CefPaintElementType type, CefRectangle[] dirtyRects,
            IntPtr buffer, int width, int height)
        {
            var length = width * height * 4;

            _renderer.LockTexture(_texture, out var pixels, out var pitch);
            Buffer.MemoryCopy(buffer.ToPointer(), pixels.ToPointer(), length, length);
            _renderer.UnlockTexture(_texture);

            _newFrameReady = true;
        }

        protected override CefAccessibilityHandler GetAccessibilityHandler()
        {
            return null;
        }

        protected override bool GetScreenInfo(CefBrowser browser, CefScreenInfo screenInfo)
        {
            return false;
        }

        protected override void OnPopupSize(CefBrowser browser, CefRectangle rect)
        {
        }

        protected override void OnCursorChange(CefBrowser browser, IntPtr cursorHandle, CefCursorType type, CefCursorInfo customCursorInfo)
        {
        }

        protected override void OnScrollOffsetChanged(CefBrowser browser, double x, double y)
        {
        }

        protected override void OnImeCompositionRangeChanged(CefBrowser browser, CefRange selectedRange, CefRectangle[] characterBounds)
        {
        }
        
        public void Draw()
        {
            if (!_newFrameReady) return;
            
            _renderer.RenderCopy(_texture);
            _newFrameReady = false;
        }

        public void Dispose()
        {
           _renderer.FreeTexture(_texture);
        }
    }
}