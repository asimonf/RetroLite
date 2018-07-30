using System;
using System.Collections.Concurrent;
using System.Threading;
using Redbus;
using Redbus.Events;
using SimpleInjector;

namespace RetroLite.Event
{
    public class EventProcessor
    {
        private readonly ConcurrentQueue<Tuple<AutoResetEvent, EventBase>> _eventQueue;
        private readonly EventBus _eventBus;

        public EventProcessor(EventBus eventBus, Container container)
        {
            _eventQueue = new ConcurrentQueue<Tuple<AutoResetEvent, EventBase>>();
            _eventBus = eventBus;
        }

        public AutoResetEvent EnqueueEventForMainThread<T>(T eventBase) where T : EventBase
        {
            var eventHandle = new AutoResetEvent(false);
            var tuple = new Tuple<AutoResetEvent, EventBase>(eventHandle, eventBase);
            _eventQueue.Enqueue(tuple);
            return eventHandle;
        }

        public void HandleEvents()
        {
            while (_eventQueue.Count > 0)
            {
                if (!_eventQueue.TryDequeue(out var pendingEvent)) continue;

                switch (pendingEvent.Item2)
                {
                    case LoadGameEvent e:
                        _eventBus.Publish(e);
                        break;
                    case OpenMenuEvent e:
                        _eventBus.Publish(e);
                        break;
                    case CloseMenuEvent e:
                        _eventBus.Publish(e);
                        break;
                }
                pendingEvent.Item1.Set();
            }
        }
    }
}