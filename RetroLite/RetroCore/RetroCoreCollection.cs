using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using NLog;
using NLog.Targets;
using Redbus;
using RetroLite.DB;
using RetroLite.Event;
using RetroLite.Input;
using RetroLite.Scene;
using RetroLite.Video;

namespace RetroLite.RetroCore
{
    public class RetroCoreCollection : IScene
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        
        private RetroCore _currentCore;
        private StateManager _stateManager;
        private List<SubscriptionToken> _eventTokens;

        public RetroCoreCollection(StateManager stateManager)
        {
            _eventTokens = new List<SubscriptionToken>();
            _stateManager = stateManager;
        }

        ~RetroCoreCollection()
        {
            foreach (var token in _eventTokens)
            {
                Program.EventBus.Unsubscribe(token);
            }
        }

        public void Stop()
        {
            _currentCore?.Stop();
        }

        public void Draw()
        {
            _currentCore?.Draw();
        }

        public void GetAudioData(IntPtr buffer, int frames)
        {
            _currentCore?.GetAudioData(buffer, frames);
        }

        public void HandleEvents()
        {
            _currentCore?.HandleEvents();
        }

        public void Start()
        {
            _currentCore?.Start();
        }

        public bool LoadGame(string path)
        {
            if (!File.Exists(path)) return false;
            
            var system = Path.GetFileNameWithoutExtension(Path.GetDirectoryName(path));
            
            // TODO: Do something here when it fails to find anything
            if (system == null) return false;
            
            var core = _stateManager.GetDefaultRetroCoreForSystem(system);

            core.LoadGame(path);

            _currentCore = core;

            return true;
        }

        public void Pause()
        {
            _currentCore?.Pause();
        }

        public void Resume()
        {
            _currentCore?.Resume();
        }

        public void Update()
        {
            _currentCore?.Update();
        }
    }
}
