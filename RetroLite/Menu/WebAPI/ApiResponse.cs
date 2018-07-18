using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroLite.Menu.WebAPI
{
    class ApiResponse
    {
        public string Data { get; }
        public int StatusCode { get; }

        public ApiResponse(string data, int statusCode)
        {
            Data = data;
            StatusCode = statusCode;
        }
    }
}
