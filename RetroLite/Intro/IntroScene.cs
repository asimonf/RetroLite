using System;
using System.IO;
using System.Threading.Tasks;
using RetroLite.Scene;
using SDL2;

namespace RetroLite.Intro
{
    public class IntroScene : IScene
    {
        private bool _initialized = false;
        private IntPtr _logo;
        private int _transitionTick = 0;
        private Task _initTask;
        private SceneManager _manager;

        public void Draw()
        {
            SDL.SDL_Rect? destinationRect = new SDL.SDL_Rect()
            {
                x = (_manager.Height >> 1) - 128,
                y = (_manager.Width >> 1) - 128,
                h = 256,
                w = 256
            };
            
            _manager.Renderer.SetRenderDrawColor(0, 0, 0, 0);
            _manager.Renderer.RenderClear();
            _manager.Renderer.RenderCopy(_logo, null, in destinationRect);
            _manager.Renderer.RenderPresent();
        }

        public void HandleEvents()
        {
        }

        public void Init(SceneManager manager)
        {
            if (_initialized)
            {
                throw new Exception("Already initialized");
            }

            _manager = manager;

            _logo = manager.Renderer.LoadTextureFromFile(Path.Combine(Environment.CurrentDirectory, "assets", "logo2.png"));
//            MenuScene.GetInstance();

            _initTask = new Task(() =>
            {
//                var collection = RetroLiteCollection.GetInstance();
//
//                var systems = Directory.GetDirectories(Path.Combine(Environment.CurrentDirectory, "cores"));
//
//                foreach (var systemPath in systems)
//                {
//                    var system = Path.GetFileNameWithoutExtension(systemPath);
//
//                    var cores = Directory.GetFiles(systemPath, "*.dll");
//
//                    foreach (var core in cores)
//                    {
//                        collection.Add(core, system, manager);
//                    }
//                }
            });
            _initTask.Start();

            _initialized = true;
        }
        
        public void Cleanup()
        {
            _manager.Renderer.FreeTexture(_logo);
            _initTask.Dispose();
            _initTask = null;
        }

        public void Pause()
        {
        }

        public void Resume()
        {
        }

        public void Update()
        {
            if (!_initTask.IsCompleted)
            {
                return;
            }

//            if (MenuScene.GetInstance().IsBrowserInitialized)
//            {
//                _transitionTick++;
//            }

            if (_transitionTick >= 60)
            {
//                manager.ChangeScene(MenuScene.GetInstance());
            }
        }
    }
}