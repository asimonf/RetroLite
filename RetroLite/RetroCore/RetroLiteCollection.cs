using System;
using System.Collections.Generic;
using System.IO;
using RetroLite.Input;
using RetroLite.Scene;
using RetroLite.Video;

namespace RetroLite.RetroCore
{
    class RetroLiteCollection : IScene
    {
        private static RetroLiteCollection _instance;
        
        public static RetroLiteCollection GetInstance()
        {
            if (null == _instance)
            {
                _instance = new RetroLiteCollection();
            }

            return _instance;
        }

        private Dictionary<string, RetroLite> _coresByName;
        private Dictionary<string, RetroLite> _coresByExtension;
        private Dictionary<string, List<RetroLite>> _coresBySystem;

        private List<RetroLite> _loadedCores;
        private RetroLite _currentCore;
        
        private readonly SceneManager _manager;
        private readonly EventProcessor _eventProcessor;
        private readonly IRenderer _renderer;

        private RetroLiteCollection()
        {
            _coresBySystem = new Dictionary<string, List<RetroLite>>();
            _coresByExtension = new Dictionary<string, RetroLite>();
            _coresByName = new Dictionary<string, RetroLite>();
            _loadedCores = new List<RetroLite>();
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

        public void HandleEvents()
        {
            _currentCore?.HandleEvents();
        }

        public void Start()
        {
            
        }

        public void LoadGame(string path)
        {
            var system = Path.GetFileNameWithoutExtension(Path.GetDirectoryName(path));
            var core = _coresBySystem[system][0];

            core.LoadGame(path);

            if (!_loadedCores.Contains(core))
            {
                _loadedCores.Add(core);
            }

            _currentCore = core;
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

        public void Add(string dll, string system)
        {
            var name = Path.GetFileNameWithoutExtension(dll);

            if (_coresByName.ContainsKey(name))
            {
                throw new Exception("Dll already loaded");
            }

            var core = new RetroLite(dll, _manager, _eventProcessor, _renderer);
            core.Start();

            //var systemInfo = core.GetSystemInfo();
            //var extensions = systemInfo.GetExtensions();
            
            //foreach (var extension in extensions)
            //{
            //    if (!_coresByExtension.ContainsKey(extension))
            //    {
            //        _coresByExtension.Add(extension, core);
            //    } else
            //    {
            //        // TODO: Make core selection per extension
            //        throw new Exception("A core already exists for extension " + extension);
            //    }
            //}

            _coresByName.Add(name, core);
            
            if (!_coresBySystem.ContainsKey(system))
            {
                _coresBySystem.Add(system, new List<RetroLite>());
            }

            _coresBySystem[system].Add(core);
        }

//        public int GetAudioData(byte[] buffer, int offset, int count)
//        {
//            if (null != _currentCore)
//            {
//                return _currentCore.GetAudioData(buffer, offset, count);
//            }
//            else
//            {
//                Array.Clear(buffer, offset, count);
//
//                return count;
//            }
//        }
    }
}
