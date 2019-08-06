using System;
using RetroLite.Video;
using SDL2;
using Xilium.CefGlue;

namespace RetroLite.Menu
{
    public class MenuRenderer : CefRenderHandler, IDisposable
    {
        private IntPtr _texture = IntPtr.Zero;
        private IRenderer _renderer;

        public MenuRenderer(
            IRenderer renderer
        )
        {
            _renderer = renderer;
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

            if (_texture == IntPtr.Zero) return;
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
        }

        public void Dispose()
        {
        }
    }
}