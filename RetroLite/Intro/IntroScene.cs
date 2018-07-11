using System;
using System.IO;
using System.Threading.Tasks;
using RetroLite.Event;
using RetroLite.Menu;
using RetroLite.Scene;
using RetroLite.Video;
using SDL2;

namespace RetroLite.Intro
{
    public class IntroScene : IScene
    {
        private bool _initialized = false;
        private IntPtr _logo;
        private Task _initTask;

        private readonly IRenderer _renderer;

        public IntroScene(IRenderer renderer)
        {
            _renderer = renderer;
        }

        public void Draw()
        {
            var destinationRect = new SDL.SDL_Rect()
            {
                y = (_renderer.GetHeight() >> 1) - 256,
                x = (_renderer.GetWidth() >> 1) - 256,
                h = 512,
                w = 512
            };
            _renderer.RenderCopyDest(_logo, ref destinationRect);
        }

        public void HandleEvents()
        {
        }

        public void Start()
        {
            if (_initialized)
            {
                throw new Exception("Already initialized");
            }

            _logo = _renderer.LoadTextureFromFile(Path.Combine(Environment.CurrentDirectory, "assets", "logo.png"));

            _initTask = new Task(() =>
            {
                var systems = Directory.GetDirectories(Path.Combine(Environment.CurrentDirectory, "cores"));

                foreach (var systemPath in systems)
                {
                    var system = Path.GetFileNameWithoutExtension(systemPath);

                    var cores = Directory.GetFiles(systemPath, "*.dll");
                    
                    foreach (var core in cores)
                    {
                        Program.EventBus.Publish(new LoadCoreEvent(core, system));
                    }
                }
            });
            _initTask.Start();

            _initialized = true;
        }
        
        public void Stop()
        {
            _renderer.FreeTexture(_logo);
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
            
            Program.EventBus.Publish(new IntroFinishedEvent());
        }
    }
}