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
                y = (_renderer.Height >> 1) - 256,
                x = (_renderer.Width >> 1) - 256,
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
            Console.WriteLine(Environment.CurrentDirectory);
            _logo = _renderer.LoadTextureFromFile(Path.Combine(Environment.CurrentDirectory, "assets", "logo.png"));

            _initTask = new Task(() =>
            {
                var systems = Directory.GetDirectories(Path.Combine(Environment.CurrentDirectory, "cores"));
                
                foreach (var systemPath in systems)
                {
                    Console.WriteLine(systemPath);
                    var system = Path.GetFileNameWithoutExtension(systemPath);

                    var cores = Directory.GetFiles(systemPath, "*.dll");
                    
                    foreach (var core in cores)
                    {
                        Console.WriteLine(core);
                        Program.EventBus.Publish(new LoadCoreEvent(core, system));
                    }
                }
            });
            
            _initTask.Start();
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
            
            _renderer.Screenshot();
            Program.EventBus.Publish(new IntroFinishedEvent());
        }
    }
}