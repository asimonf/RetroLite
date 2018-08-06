using System.Collections.Generic;
using RetroLite.DB;
using Xilium.CefGlue;

namespace RetroLite.Menu.WebAPI.Action
{
    public class QuitAction : IAction
    {
        public string Path => "^/quit$";

        public string Method => "POST";
        
        public ApiResponse ProcessRequest(CefRequest request, IDictionary<string, string> parameters)
        {
            Program.Running = false;
            
            return new ApiResponse(
                null,
                200
            );
        }
    }
}