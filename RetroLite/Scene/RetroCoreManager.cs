using System.Collections.Generic;
using System.IO;
using NLog;
using Redbus;
using RetroLite.DB;
using RetroLite.Event;
using RetroLite.Video;

namespace RetroLite.Scene
{
    public class RetroCoreManager : IScene
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        
        private RetroCore.RetroCore _currentCore;
        private readonly List<SubscriptionToken> _eventTokens;
        private readonly StateManager _stateManager;
        private readonly EventBus _eventBus;
        private bool _running;

        public int Order => 0;

        public RetroCoreManager(
            StateManager stateManager, 
            EventBus eventBus
        )
        {
            _eventTokens = new List<SubscriptionToken>();
            _stateManager = stateManager;
            _eventBus = eventBus;
            
            _eventTokens.Add(_eventBus.Subscribe<LoadGameEvent>(OnLoadGameEvent));
            _eventTokens.Add(_eventBus.Subscribe<OpenMenuEvent>(OnOpenMenuEvent));
            _eventTokens.Add(_eventBus.Subscribe<CloseMenuEvent>(OnCloseMenuEvent));
        }
        
        private void OnOpenMenuEvent(OpenMenuEvent openMenuEvent)
        {
            _running = false;
        }
        
        private void OnCloseMenuEvent(CloseMenuEvent closeMenuEvent)
        {
            _running = true;
        }
        
        public int CompareTo(IScene other)
        {
            if (other == null) return 1;

            return Order.CompareTo(other.Order);
        }

        ~RetroCoreManager()
        {
            foreach (var token in _eventTokens)
            {
                _eventBus.Unsubscribe(token);
            }
        }
        
        private void OnLoadGameEvent(LoadGameEvent loadGameEvent)
        {
            if (_currentCore != null)
            {
                _currentCore.UnloadGame();
                _currentCore = null;
            }
            
            string path = loadGameEvent.Game.Path;
            
            if (!File.Exists(path)) return;
        
            var system = Path.GetFileNameWithoutExtension(Path.GetDirectoryName(path));
        
            // TODO: Do something here when it fails to find anything
            if (system == null) return;
        
            var core = _stateManager.GetDefaultRetroCoreForSystem(system);

            core.LoadGame(path);

            _currentCore = core;
            _running = true;
        }

        public void Draw()
        {
            _currentCore?.Draw();
        }

        public float[] GetAudioData(int frames)
        {
            return _running ? _currentCore?.GetAudioData(frames) : null;
        }

        public void HandleEvents()
        {
        }

        public void Update()
        {
            if (_running)
                _currentCore?.Update();
        }
        
        
    }
}
