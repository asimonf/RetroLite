using RetroLite.Menu.WebAPI.Action;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xilium.CefGlue;

namespace RetroLite.Menu.WebAPI
{
    class ApiRouter
    {
        private IDictionary<string, IAction> _routeDictionary;

        public ApiRouter(IEnumerable<IAction> actions)
        {
            _routeDictionary = new Dictionary<string, IAction>();

            foreach (var action in actions)
            {
                _routeDictionary.Add(action.Path, action);
            }
        }

        public ApiResponse ProcessRequest(CefRequest request)
        {
            var url = new Uri(request.Url);

            if (!_routeDictionary.ContainsKey(url.AbsolutePath))
            {
                return new ApiResponse("", 404);
            }

            return _routeDictionary[url.AbsolutePath].ProcessRequest(request);
        }
    }
}
