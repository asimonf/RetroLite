using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Redbus.Events;
using RetroLite.DB;
using RetroLite.DB.Entity;
using RetroLite.Event;
using Xilium.CefGlue;

namespace RetroLite.Menu.WebAPI.Action
{
    internal class LoadGameAction : IAction
    {
        public string Path => "^/games/:id/load$";

        public string Method => "POST";

        private readonly EventProcessor _eventProcessor;
        private readonly StateManager _stateManager;

        public LoadGameAction(EventProcessor eventProcessor, StateManager stateManager)
        {
            _eventProcessor = eventProcessor;
            _stateManager = stateManager;
        }

        public ApiResponse ProcessRequest(CefRequest request, IDictionary<string, string> parameters)
        {
            var gameId = parameters["id"];

            var game = _stateManager.GetGameById(new Guid(gameId));
            
            if (null == game)
            {
                return new ApiResponse(null, 404);
            }
            
            using (var handle = _eventProcessor.EnqueueEventForMainThread(new LoadGameEvent(game)))
            {
                handle.WaitOne();
            }
            
            return new ApiResponse(null, 204);
        }
    }
}
