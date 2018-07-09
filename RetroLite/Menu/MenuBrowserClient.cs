using RetroLite.Scene;
using Xilium.CefGlue;

namespace RetroLite.Menu
{
    public class MenuBrowserClient : CefClient
    {
        private readonly MenuRenderer _renderer;

        public MenuBrowserClient(MenuRenderer menuRenderer)
        {
            _renderer = menuRenderer;
        }

        protected override CefRenderHandler GetRenderHandler()
        {
            return _renderer;
        }

        public void Draw()
        {
            _renderer.Draw();
        }
    }
}