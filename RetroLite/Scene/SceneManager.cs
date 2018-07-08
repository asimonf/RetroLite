using SDL2;
using System;
using System.Collections.Generic;
using RetroLite.Input;
using RetroLite.Video;

namespace RetroLite.Scene
{
    public class SceneManager
    {
        private readonly Stack<IScene> _scenes;

        public IRenderer Renderer { get; private set; }
        public EventProcessor EventProcessor { get; private set; }

        public int Height => Renderer.GetHeight();
        public int Width => Renderer.GetWidth();

        public bool Running { get; set; } = false;

        public SceneManager()
        {
            _scenes = new Stack<IScene>();
            EventProcessor = new EventProcessor(this);
            Renderer = new SdlRenderer(1024, 768);
        }

        public void Init()
        {
            Running = true;

            EventProcessor.Init();
            Renderer.Init();
        }

        public void Cleanup()
        {
            while (_scenes.Count > 0)
            {
                _scenes.Pop().Cleanup();
            }

            EventProcessor.Cleanup();
            Renderer.Cleanup();

            SDL_image.IMG_Quit();
            SDL.SDL_Quit();
        }

        public void ChangeScene(IScene scene)
        {
            EventProcessor.ResetControllers();
            if (_scenes.Count > 0)
            {
                _scenes.Pop().Cleanup();
            }

            _scenes.Push(scene);
            scene.Init(this);
        }

        public void PushScene(IScene scene)
        {
            EventProcessor.ResetControllers();
            if (_scenes.Count > 0)
            {
                _scenes.Peek().Pause();
            }

            _scenes.Push(scene);
            scene.Init(this);
        }

        public void PopScene()
        {
            EventProcessor.ResetControllers();
            if (_scenes.Count > 0)
            {
                _scenes.Pop().Cleanup();
            }

            if (_scenes.Count > 0)
            {
                _scenes.Peek().Resume();
            }
        }

        public void HandleEvents()
        {
            EventProcessor.HandleEvents();
            _scenes.Peek().HandleEvents();
        }

        public void Update()
        {
            _scenes.Peek().Update();
        }

        public void Draw()
        {
            _scenes.Peek().Draw();
        }
    }
}
