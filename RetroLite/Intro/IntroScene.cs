using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using RetroLite.DB;
using RetroLite.Event;
using RetroLite.Menu;
using RetroLite.Scene;
using RetroLite.Video;
using SDL2;

namespace RetroLite.Intro
{
    public class IntroScene : IScene
    {
        private IntPtr _logo;

        private readonly IRenderer _renderer;
        private readonly SceneManager _manager;
        private readonly IScene _nextScene;
        private StateManager _stateManager;

        private Task _initializationTask, _gameScanTask;

        public IntroScene(IRenderer renderer, SceneManager manager, IScene nextScene, StateManager stateManager)
        {
            _renderer = renderer;
            _manager = manager;
            _nextScene = nextScene;
            _stateManager = stateManager;
        }

        public void Draw()
        {
            var destinationRect = new SDL.SDL_Rect()
            {
                y = (_renderer.Height >> 1) - 256,
                x = (_renderer.Width >> 1) - 256,
                h = 512,
                w = 512
            };
            _renderer.RenderCopyDest(_logo, ref destinationRect);
        }

        public void GetAudioData(IntPtr buffer, int frames)
        {
            for(var i=0; i < frames * 2 * 2; i++)
            {
                Marshal.WriteByte(buffer, i, 0);
            }
        }

        public void HandleEvents()
        {
        }

        public void Start()
        {
            _logo = _renderer.LoadTextureFromFile(Path.Combine(Environment.CurrentDirectory, "assets", "logo.png"));
            
            if (_initializationTask == null)
                _initializationTask = _stateManager.Initialize();
        }
        
        public void Stop()
        {
            _renderer.FreeTexture(_logo);
        }

        public void Pause()
        {
        }

        public void Resume()
        {
        }

        public void Update()
        {
            if (_initializationTask == null || !_initializationTask.IsCompleted)
            { 
                return;
            }

            if (_initializationTask.IsCompleted && _gameScanTask == null)
            {
                _gameScanTask = _stateManager.ScanForGames(Path.Combine(Environment.CurrentDirectory, "roms"));
                return;
            }

            if (!_gameScanTask.IsCompleted)
            {
                return;
            }
            
            _manager.ChangeScene(_nextScene);
        }
    }
}