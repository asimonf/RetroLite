using System.Collections.Generic;
using RetroLite.DB;
using Xilium.CefGlue;

namespace RetroLite.Menu.WebAPI.Action
{
    public class ScanGamesAction : IAction
    {
        public string Path => "^/games/scan$|";

        public string Method => "GET";
        
        private readonly StateManager _stateManager;

        public ScanGamesAction(StateManager stateManager)
        {
            _stateManager = stateManager;
        }
        
        public ApiResponse ProcessRequest(CefRequest request, IDictionary<string, string> parameters)
        {
            _stateManager.ScanForGames();
            
            return new ApiResponse(
                null,
                200
            );
        }
    }
}