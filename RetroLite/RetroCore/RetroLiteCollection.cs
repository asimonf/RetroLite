using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using NLog.Targets;
using Redbus;
using RetroLite.Event;
using RetroLite.Input;
using RetroLite.Scene;
using RetroLite.Video;

namespace RetroLite.RetroCore
{
    public class RetroLiteCollection : IScene
    {
        private readonly Dictionary<string, RetroLite> _coresByName;
        private readonly Dictionary<string, List<RetroLite>> _coresBySystem;

        private readonly List<RetroLite> _loadedCores;
        private readonly List<SubscriptionToken> _eventTokens;
        private RetroLite _currentCore;
        
        private readonly SceneManager _manager;
        private readonly EventProcessor _eventProcessor;
        private readonly IRenderer _renderer;

        public RetroLiteCollection(SceneManager manager, EventProcessor eventProcessor, IRenderer renderer)
        {
            _manager = manager;
            _eventProcessor = eventProcessor;
            _renderer = renderer;
            
            _coresBySystem = new Dictionary<string, List<RetroLite>>();
            _coresByName = new Dictionary<string, RetroLite>();
            _loadedCores = new List<RetroLite>();
            _eventTokens = new List<SubscriptionToken>();

            _eventTokens.Add(Program.EventBus.Subscribe<LoadCoreEvent>(OnLoadCoreEvent));
        }

        ~RetroLiteCollection()
        {
            foreach (var token in _eventTokens)
            {
                Program.EventBus.Unsubscribe(token);
            }
        }

        public void Stop()
        {
            foreach (var core in _coresByName.Values)
            {
                core.Stop();
            }
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
            
        }

        public bool LoadGame(string path)
        {
            if (!File.Exists(path)) return false;
            
            var system = Path.GetFileNameWithoutExtension(Path.GetDirectoryName(path));
            
            // TODO: Do something here when it fails to find anything
            if (system == null) return false;
            
            var core = _coresBySystem[system][0];

            core.LoadGame(path);

            if (!_loadedCores.Contains(core))
            {
                _loadedCores.Add(core);
            }

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

        /// <summary>
        /// Executes when a core load event is fired
        /// </summary>
        /// <param name="loadCoreEvent"></param>
        /// <exception cref="Exception"></exception>
        private void OnLoadCoreEvent(LoadCoreEvent loadCoreEvent)
        {
            string dll = loadCoreEvent.Dll, system = loadCoreEvent.System;
            Console.WriteLine($"Loading core {dll}");
            var name = Path.GetFileNameWithoutExtension(dll);

            Debug.Assert(name != null, nameof(name) + " != null");
            
            if (_coresByName.ContainsKey(name))
            {
                throw new Exception("Dll already loaded");
            }

            var core = new RetroLite(dll, _manager, _eventProcessor, _renderer);
            core.Start();

            _coresByName.Add(name, core);
            
            if (!_coresBySystem.ContainsKey(system))
            {
                _coresBySystem.Add(system, new List<RetroLite>());
            }

            Console.WriteLine($"Core '{name}' for system '{system}' loaded.");
            _coresBySystem[system].Add(core);
        }
    }
}
