using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xilium.CefGlue;

namespace RetroLite.Menu.WebAPI.Action
{
    internal class ListCoresAction : IAction
    {
        public string Path => "/cores";

        public string Method => "GET";

        public ApiResponse ProcessRequest(CefRequest request)
        {
            var response = Program.StateManager.GetCoreList();

            return new ApiResponse(
                JsonConvert.SerializeObject(response),
                200
            );
        }
    }
}
