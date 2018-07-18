using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xilium.CefGlue;

namespace RetroLite.Menu.WebAPI.Action
{
    class ListGamesAction : IAction
    {
        public string Path => "/games";

        public string Method => "GET";

        public ApiResponse ProcessRequest(CefRequest request)
        {
            var response = Program.StateManager.GetGameList();

            return new ApiResponse(
                JsonConvert.SerializeObject(response),
                200
            );
        }
    }
}
