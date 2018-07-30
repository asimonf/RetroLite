using System.Text.RegularExpressions;
using RetroLite.Menu.WebAPI.Action;

namespace RetroLite.Menu.WebAPI
{
    public class RouteDefinition
    {
        public IAction Action { get; }
        public Regex RouteRegEx { get; }

        public RouteDefinition(IAction action)
        {
            Action = action;

            // Build RegEx from route (:foo to named group (?<foo>[a-z0-9]+)).
            var routeFormat = new Regex("(:([a-z]+))\\b").Replace(action.Path, "(?<$2>[a-z0-9A-Z\\-]+)");

            // Build the match uri parameter to that regex.
            RouteRegEx = new Regex(routeFormat);
        }
    }
}