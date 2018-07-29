using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Redbus.Events;
using RetroLite.DB;
using RetroLite.Event;
using Xilium.CefGlue;

namespace RetroLite.Menu.WebAPI.Action
{
    internal class CloseMenuAction : IAction
    {
        public string Path => "/event/close-menu";

        public string Method => "GET";

        private readonly EventProcessor _eventProcessor;

        public CloseMenuAction(EventProcessor eventProcessor)
        {
            _eventProcessor = eventProcessor;
        }

        public ApiResponse ProcessRequest(CefRequest request, IDictionary<string, string> parameters)
        {
            using (var handle = _eventProcessor.EnqueueEventForMainThread(new CloseMenuEvent()))
            {
                handle.WaitOne();
            }
            
            return new ApiResponse(
                null,
                200
            );
        }
    }
}
