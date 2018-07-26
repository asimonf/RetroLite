using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RetroLite.DB;
using Xilium.CefGlue;

namespace RetroLite.Menu.WebAPI.Action
{
    internal class ListGamesAction : IAction
    {
        public string Path => "/games";

        public string Method => "GET";
        
        private StateManager _stateManager;

        public ListGamesAction(StateManager stateManager)
        {
            _stateManager = stateManager;
        }

        public ApiResponse ProcessRequest(CefRequest request)
        {
            var response = _stateManager.GetGameList();

            return new ApiResponse(
                JsonConvert.SerializeObject(response),
                200
            );
        }
    }
}
