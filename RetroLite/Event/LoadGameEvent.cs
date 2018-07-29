using Redbus.Events;
using RetroLite.DB.Entity;

namespace RetroLite.Event
{
    public class LoadGameEvent : EventBase
    {
        public Game Game { get; }

        public LoadGameEvent(Game game)
        {
            Game = game;
        }
    }
}