using RetroLite.Menu.WebAPI.Action;
using System;
using System.Collections.Generic;
using Xilium.CefGlue;

namespace RetroLite.Menu.WebAPI
{
    class ApiRouter
    {
        private IDictionary<Tuple<string, string>, IAction> _routeDictionary;

        public ApiRouter(IEnumerable<IAction> actions)
        {
            _routeDictionary = new Dictionary<Tuple<string, string>, IAction>();

            foreach (var action in actions)
            {
                var tuple = new Tuple<string, string>(action.Path, action.Method);
                _routeDictionary.Add(tuple, action);
            }
        }

        public ApiResponse ProcessRequest(CefRequest request)
        {
            var url = new Uri(request.Url);

            var key = new Tuple<string, string>(url.AbsolutePath, request.Method);

            if (!_routeDictionary.ContainsKey(key))
            {
                return new ApiResponse("", 404);
            }

            return _routeDictionary[key].ProcessRequest(request);
        }
    }
}
