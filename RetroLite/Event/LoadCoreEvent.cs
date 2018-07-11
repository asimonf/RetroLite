using Redbus.Events;

namespace RetroLite.Event
{
    public class LoadCoreEvent : EventBase
    {
        public string Dll { get; }
        public string System { get; }

        public LoadCoreEvent(string dll, string system)
        {
            Dll = dll;
            System = system;
        }
    }
}