using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xilium.CefGlue;

namespace RetroLite.Menu.WebAPI
{
    class ResourceHandler : CefResourceHandler
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private ApiRouter _apiRouter;

        public ResourceHandler(ApiRouter apiRouter)
        {
            _apiRouter = apiRouter;
        }

        private ApiResponse _apiResponse;

        /// <summary>
        /// The response in bytes.
        /// </summary>
        private byte[] _responseBytes;

        /// <summary>
        /// The completed flag.
        /// </summary>
        private bool _completed;

        /// <summary>
        /// The total bytes read.
        /// </summary>
        private int _totalBytesRead;

        protected override bool ProcessRequest(CefRequest request, CefCallback callback)
        {
            if (!request.Url.StartsWith("retrolite://"))
            {
                callback.Dispose();
                return false;
            }

            Task.Run(() =>
            {
                using (callback)
                {
                    try
                    {
                        _apiResponse = _apiRouter.ProcessRequest(request);
                        _responseBytes = Encoding.UTF8.GetBytes(_apiResponse.Data);
                    }
                    catch (Exception exception)
                    {
                        _logger.Error(exception, "Error processing a request");
                        _apiResponse = new ApiResponse(exception.Message, 500);
                    }
                    finally
                    {
                        callback.Continue();
                    }
                }
            });

            return true;
        }

        protected override void GetResponseHeaders(CefResponse response, out long responseLength, out string redirectUrl)
        {
            throw new NotImplementedException();
        }

        protected override bool ReadResponse(Stream response, int bytesToRead, out int bytesRead, CefCallback callback)
        {
            throw new NotImplementedException();
        }

        protected override void Cancel()
        {
        }

        protected override bool CanGetCookie(CefCookie cookie)
        {
            return false;
        }

        protected override bool CanSetCookie(CefCookie cookie)
        {
            return false;
        }
    }
}
