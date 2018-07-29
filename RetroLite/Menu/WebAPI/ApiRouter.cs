using RetroLite.Menu.WebAPI.Action;
using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NLog.Fluent;
using Xilium.CefGlue;

namespace RetroLite.Menu.WebAPI
{
    class ApiRouter
    {
        private readonly IDictionary<Tuple<string, string>, RouteDefinition> _routeDictionary;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ApiRouter(IEnumerable<IAction> actions)
        {
            _routeDictionary = new Dictionary<Tuple<string, string>, RouteDefinition>();

            foreach (var action in actions)
            {
                var tuple = new Tuple<string, string>(action.Path, action.Method);
                _routeDictionary.Add(tuple, new RouteDefinition(action));
            }
        }

        public ApiResponse ProcessRequest(CefRequest request)
        {
            var url = new Uri(request.Url);

            var key = new Tuple<string, string>(url.AbsolutePath, request.Method);
            
            if (_routeDictionary.ContainsKey(key)) 
                return _routeDictionary[key].Action.ProcessRequest(request, new Dictionary<string, string>());

            foreach (var route in _routeDictionary)
            {
                // Execute the regex to check whether the uri correspond to the route
                var match = route.Value.RouteRegEx.Match(url.AbsolutePath);

                if (!match.Success) continue;

                // Obtain named groups.
                var parameters = route.Value.RouteRegEx.GetGroupNames().Skip(1) // Skip the "0" group
                    .Where(g => match.Groups[g].Success && match.Groups[g].Captures.Count > 0)
                    .ToDictionary(groupName => groupName, groupName => match.Groups[groupName].Value);

                return route.Value.Action.ProcessRequest(request, parameters);
            }
            
            return new ApiResponse("", 404);

        }
    }
}
