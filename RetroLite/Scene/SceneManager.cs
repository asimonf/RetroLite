using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using RetroLite.Input;
using RetroLite.Video;
using Xt;

namespace RetroLite.Scene
{
    public class SceneManager : IDisposable
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly Stack<IScene> _scenes;

        private readonly IRenderer _renderer;
        private readonly EventProcessor _eventProcessor;
        
        private readonly XtAudio _xtAudio;
        private readonly XtDevice _xtDevice;
        private readonly XtStream _xtStream;

        public XtFormat AudioFormat { get; }
        public IScene CurrentScene => _scenes.Count > 0 ? _scenes.Peek() : null;

        private Stopwatch _nopTimer;
        
        public double TargetFps { get; set; } = 60;

        public SceneManager(IRenderer renderer, EventProcessor eventProcessor)
        {
            _renderer = renderer;
            _eventProcessor = eventProcessor;
            
            _scenes = new Stack<IScene>();
            
            _xtAudio = new XtAudio(null, IntPtr.Zero, _traceCallback, _fatalCallback);
            var xtService = XtAudio.GetServiceBySetup(XtSetup.SystemAudio);
            AudioFormat = new XtFormat(new XtMix(48000, XtSample.Int16), 0, 0, 2, 0);
            _xtDevice = xtService.OpenDefaultDevice(true);
            _xtStream = _xtDevice.OpenStream(AudioFormat, interleaved: true, raw: true, bufferSize: 16, RenderAudioCallback, XRunCallback, null);
            _xtStream.Start();
            
            _nopTimer = new Stopwatch();
            _nopTimer.Start();
        }

        public void Dispose()
        {
            while (_scenes.Count > 0)
            {
                var scene = _scenes.Pop();
                scene.Pause();
                scene.Stop();
            }
            _xtStream.Stop();
            _xtStream.Dispose();
            _xtDevice.Dispose();
            _xtAudio.Dispose();
            
            Console.WriteLine("Disposed");
        }
        
        public RetroCore.RetroCore CreateRetroCore(string dll, string system)
        {
            Logger.Debug($"Loading core {dll}");
            var name = Path.GetFileNameWithoutExtension(dll);

            Debug.Assert(name != null, nameof(name) + " != null");

            try
            {
                var core = new RetroCore.RetroCore(dll, this, _eventProcessor, _renderer);
                core.Start();

                Logger.Debug($"Core '{name}' for system '{system}' loaded.");

                return core;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error loading core...");

                return null;
            }
        }

        public void ChangeScene(IScene scene)
        {
            _eventProcessor.ResetControllers();    
            if (_scenes.Count > 0)
            {
                var poppedScene = _scenes.Pop();
                poppedScene.Pause();
                poppedScene.Stop();
            }

            _scenes.Push(scene);
            scene.Start();
            scene.Resume();
        }

        public void PushScene(IScene scene)
        {
            _eventProcessor.ResetControllers();
            CurrentScene?.Pause();

            _scenes.Push(scene);
            scene.Start();
            scene.Resume();
        }

        public void PopScene()
        {
            _eventProcessor.ResetControllers();
            if (_scenes.Count > 0)
            {
                _scenes.Pop().Stop();
            }

            CurrentScene?.Resume();
        }

        private void HandleEvents()
        {
            _eventProcessor.HandleEvents();
            
            CurrentScene?.HandleEvents();
        }

        private void Update()
        {
            CurrentScene?.Update();
        }

        private void Draw()
        {
            _renderer.SetRenderDrawColor(0, 0, 0, 255);
            _renderer.RenderClear();
            CurrentScene?.Draw();
            _renderer.RenderPresent();
        }

        public void RunLoop()
        {
            var frameStart = Stopwatch.GetTimestamp();
            HandleEvents();
            Update();
            Draw();
            var frameTicks = Stopwatch.GetTimestamp() - frameStart;
            var elapsedTime = frameTicks * (1000.0 / Stopwatch.Frequency);
            var targetFrametime = (1000.0 / TargetFps);
            
            if (!(targetFrametime > elapsedTime)) return;
            
            var durationTicks = (targetFrametime - elapsedTime) * (Stopwatch.Frequency / 1000.0);

            // Busy loop. This is to increase accuracy of the timing function
            _nopTimer.Restart();
            while (_nopTimer.ElapsedTicks < durationTicks) ;
        }
        
        private void RenderAudioCallback(XtStream stream, object input, object output, int frames, double time,
            ulong position, bool timeValid, ulong error, object user)
        {
            CurrentScene?.GetAudioData(((IntPtr)output), frames);
        }
        
        private void XRunCallback(int index, object user)
        {
            Logger.Warn("Over/Underflow detected");
        }
        
        private void _traceCallback(XtLevel level, string message)
        {
            switch (level)
            {
                case XtLevel.Info:
                    Logger.Info(message);
                    break;
                case XtLevel.Error:
                    Logger.Error(message);
                    break;
                case XtLevel.Fatal:
                    Logger.Fatal(message);
                    break;
            }
        }

        private void _fatalCallback()
        {
            Logger.Fatal("Fatal error in audio driver");
        }
    }
}
