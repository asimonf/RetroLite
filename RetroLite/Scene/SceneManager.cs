using SDL2;
using System;
using System.Collections.Generic;
using RetroLite.Input;
using RetroLite.Video;
using Xilium.CefGlue;

namespace RetroLite.Scene
{
    public class SceneManager
    {
        private readonly Stack<IScene> _scenes;

        private readonly IRenderer _renderer;
        private readonly EventProcessor _eventProcessor;

        public bool Running { get; set; } = false;
        
        public SceneManager(IRenderer renderer, EventProcessor eventProcessor)
        {
            _renderer = renderer;
            _eventProcessor = eventProcessor;
            
            _scenes = new Stack<IScene>();
        }

        public void Cleanup()
        {
            while (_scenes.Count > 0)
            {
                _scenes.Pop().Stop();
            }
        }

        public void ChangeScene(IScene scene)
        {
            _eventProcessor.ResetControllers();
            if (_scenes.Count > 0)
            {
                _scenes.Pop().Stop();
            }

            _scenes.Push(scene);
            scene.Start();
        }

        public void PushScene(IScene scene)
        {
            _eventProcessor.ResetControllers();
            if (_scenes.Count > 0)
            {
                _scenes.Peek().Pause();
            }

            _scenes.Push(scene);
            scene.Start();
            scene.Resume();
        }

        public bool IsCurrentScene(IScene scene)
        {
            return null != scene && scene == _scenes.Peek();
        }

        public void PopScene()
        {
            _eventProcessor.ResetControllers();
            if (_scenes.Count > 0)
            {
                _scenes.Pop().Stop();
            }

            if (_scenes.Count > 0)
            {
                _scenes.Peek().Resume();
            }
        }

        public void HandleEvents()
        {
            _eventProcessor.HandleEvents();
            _scenes.Peek().HandleEvents();
        }

        public void Update()
        {
            _scenes.Peek().Update();
        }

        public void Draw()
        {
            _renderer.SetRenderDrawColor(0, 0, 0, 0);
            _renderer.RenderClear();
            _scenes.Peek().Draw();
            _renderer.RenderPresent();
        }
    }
}
