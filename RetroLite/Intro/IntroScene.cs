﻿using System;
using System.IO;
using System.Runtime.InteropServices;
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
            Program.StateManager.Initialize();
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
            if (!Program.StateManager.Initialized)
            {
                return;
            }
            
            Program.EventBus.Publish(new IntroFinishedEvent());
        }
    }
}