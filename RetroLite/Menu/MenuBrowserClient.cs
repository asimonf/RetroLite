using RetroLite.Scene;
using Xilium.CefGlue;

namespace RetroLite.Menu
{
    internal class MenuBrowserClient : CefClient
    {
        private readonly MenuRenderer _renderer;

        public MenuBrowserClient(SceneManager manager)
        {
            _renderer = new MenuRenderer(manager);
        }

        protected override CefRenderHandler GetRenderHandler()
        {
            return _renderer;
        }

        protected override void Dispose(bool disposing)
        {
            _renderer.Dispose();
            base.Dispose(disposing);
        }
    }
}