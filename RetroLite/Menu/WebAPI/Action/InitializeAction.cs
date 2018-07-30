using System.Collections.Generic;
using RetroLite.DB;
using Xilium.CefGlue;

namespace RetroLite.Menu.WebAPI.Action
{
    public class InitializeAction : IAction
    {
        public string Path => "^/initialize$";

        public string Method => "GET";
        
        private readonly StateManager _stateManager;

        public InitializeAction(StateManager stateManager)
        {
            _stateManager = stateManager;
        }
        
        public ApiResponse ProcessRequest(CefRequest request, IDictionary<string, string> parameters)
        {
            _stateManager.Initialize();
            
            return new ApiResponse(
                null,
                200
            );
        }
    }
}