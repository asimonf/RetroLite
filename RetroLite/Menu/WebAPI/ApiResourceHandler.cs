using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using Xilium.CefGlue;

namespace RetroLite.Menu.WebAPI
{
    internal class ApiResourceHandler : CefResourceHandler
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly ApiRouter _apiRouter;

        private ApiResponse _apiResponse;

        private byte[] _responseBytes;

        private bool _completed;

        private int _totalBytesRead;

        public ApiResourceHandler(ApiRouter apiRouter)
        {
            _apiRouter = apiRouter;
        }

        protected override bool ProcessRequest(CefRequest request, CefCallback callback)
        {
            Task.Run(() =>
            {
                using (callback)
                {
                    try
                    {
                        _apiResponse = _apiRouter.ProcessRequest(request);

                        if (_apiResponse.Data != null)
                        {
                            _logger.Debug(_apiResponse.Data);
                            _responseBytes = Encoding.UTF8.GetBytes(_apiResponse.Data);
                        }
                    }
                    catch (Exception exception)
                    {
                        _logger.Error(exception, exception.Message);
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
            responseLength = _responseBytes?.LongLength ?? 0;
            redirectUrl = null;

            try
            {
                var statusCode = _apiResponse.StatusCode;

                var headers = response.GetHeaderMap();
                headers.Add("Cache-Control", "private");
                headers.Add("Access-Control-Allow-Origin", "*");
                headers.Add("Access-Control-Allow-Methods", "GET,POST");
                headers.Add("Access-Control-Allow-Headers", "Content-Type");
                headers.Add("Content-Type", "application/json; charset=utf-8");
                response.SetHeaderMap(headers);

                response.Status = statusCode;
                response.MimeType = "application/json";
                response.StatusText = (statusCode < 400 && statusCode >= 200) ? "OK" : "Error";
            }
            catch (Exception exception)
            {
                _logger.Error(exception);
            }
        }

        protected override bool ReadResponse(Stream response, int bytesToRead, out int bytesRead, CefCallback callback)
        {
            var currBytesRead = 0;

            try
            {
                if (_completed)
                {
                    bytesRead = 0;
                    _totalBytesRead = 0;
                    _responseBytes = null;
                    _apiResponse = null;

                    return false;
                }

                if (_responseBytes != null)
                {
                    currBytesRead = Math.Min(_responseBytes.Length - _totalBytesRead, bytesToRead);
                    response.Write(_responseBytes, _totalBytesRead, currBytesRead);
                    _totalBytesRead += currBytesRead;

                    if (_totalBytesRead >= _responseBytes.Length)
                    {
                        _completed = true;
                    }
                }
                else
                {
                    bytesRead = 0;
                    _completed = true;
                }
            }
            catch (Exception exception)
            {
                _logger.Error(exception);
            }

            bytesRead = currBytesRead;
            return true;
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
