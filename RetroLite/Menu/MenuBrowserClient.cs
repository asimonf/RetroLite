using RetroLite.Scene;
using Xilium.CefGlue;

namespace RetroLite.Menu
{
    public class MenuBrowserClient : CefClient
    {
        private readonly MenuRenderer _renderer;
        private readonly CefRequestHandler _requestHandler;

        public MenuBrowserClient(MenuRenderer menuRenderer, CefRequestHandler requestHandler)
        {
            _renderer = menuRenderer;
            _requestHandler = requestHandler;
        }
        
        protected override CefRenderHandler GetRenderHandler()
        {
            return _renderer;
        }

        protected override CefRequestHandler GetRequestHandler()
        {
            return _requestHandler;
        }

        public void Draw()
        {
            _renderer.Draw();
        }
    }
}