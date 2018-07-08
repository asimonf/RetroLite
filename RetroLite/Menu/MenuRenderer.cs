using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using RetroLite.Scene;
using RetroLite.Video;
using SDL2;
using Xilium.CefGlue;

namespace RetroLite.Menu
{
    public class MenuRenderer : CefRenderHandler, IDisposable
    {
        private IntPtr _texture;
        private readonly SceneManager _manager;
        private IRenderer Renderer => _manager.Renderer;

        public MenuRenderer(SceneManager manager)
        {
            _manager = manager;
            _resize();
        }

        private void _resize()
        {
            if (_texture != IntPtr.Zero)
            {
                Renderer.FreeTexture(_texture);
            }

            var format = SDL.SDL_PIXELFORMAT_ARGB8888;

            _texture = Renderer.CreateTexture(
                format,
                SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING,
                Renderer.GetWidth(),
                Renderer.GetHeight()
            );
        }

        protected override bool GetViewRect(CefBrowser browser, ref CefRectangle rect)
        {
            rect = new CefRectangle(0, 0, Renderer.GetWidth(), Renderer.GetHeight());
            return true;
        }

        protected override unsafe void OnPaint(CefBrowser browser, CefPaintElementType type, CefRectangle[] dirtyRects,
            IntPtr buffer, int width, int height)
        {
            if (_texture == IntPtr.Zero) return;

            var length = width * height * 4;
            
            Renderer.LockTexture(_texture, null, out var textureData, out var texturePitch);
            Buffer.MemoryCopy(buffer.ToPointer(), _texture.ToPointer(), length, length);
            Renderer.UnlockTexture(_texture);
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

        public void Dispose()
        {
            if (_texture != IntPtr.Zero)
            {
                Renderer.FreeTexture(_texture);
            }
        }
    }
}