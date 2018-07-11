using Redbus.Events;
using RetroLite.Scene;

namespace RetroLite.Event
{
    public class OpenMenuEvent : EventBase
    {
        public IScene CalleeScene { get; }
        
        public OpenMenuEvent(IScene calleeScene)
        {
            CalleeScene = calleeScene;
        }
    }
}