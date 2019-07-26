using System.Collections.Generic;
using Newtonsoft.Json;
using RetroLite.DB;
using Xilium.CefGlue;

namespace RetroLite.Menu.WebAPI.Action
{
    public class ScanGamesAction : IAction
    {
        public string Path => "^/games/scan$|";

        public string Method => "POST";
        
        private readonly StateManager _stateManager;

        public ScanGamesAction(StateManager stateManager)
        {
            _stateManager = stateManager;
        }
        
        public ApiResponse ProcessRequest(CefRequest request, IDictionary<string, string> parameters)
        {
            _stateManager.ScanForGames();
            
            var response = _stateManager.GetGameList();
            
            return new ApiResponse(
                JsonConvert.SerializeObject(response),
                200
            );
        }
    }
}