using System;
using Xilium.CefGlue;

namespace RetroLite.Menu.WebAPI
{
    class ApiRequestHandler : CefRequestHandler
    {
        private readonly ApiRouter _apiRouter;

        public ApiRequestHandler(ApiRouter apiRouter)
        {
            _apiRouter = apiRouter;
        }

        protected override CefResourceHandler GetResourceHandler(CefBrowser browser, CefFrame frame, CefRequest request)
        {
            var url = new Uri(request.Url);

            if (url.Host != "retrolite.internal") return null;

            return new ApiResourceHandler(_apiRouter);
        }
    }
}
