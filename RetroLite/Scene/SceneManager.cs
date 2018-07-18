using System;
using System.Collections.Generic;
using System.Diagnostics;
using RetroLite.Input;
using RetroLite.Video;
using Xt;

namespace RetroLite.Scene
{
    public class SceneManager
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly Stack<IScene> _scenes;

        private readonly IRenderer _renderer;
        private readonly EventProcessor _eventProcessor;
        
        private readonly XtAudio _xtAudio;
        private readonly XtDevice _xtDevice;
        private readonly XtStream _xtStream;

        public bool Running { get; set; } = false;
        
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

        ~SceneManager()
        {
            _xtStream.Stop();
            _xtStream.Dispose();
            _xtDevice.Dispose();
            _xtAudio.Dispose();
        }

        public void Cleanup()
        {
            while (_scenes.Count > 0)
            {
                var scene = _scenes.Pop();
                scene.Pause();
                scene.Stop();
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
            _renderer.SetRenderDrawColor(255, 255, 255, 255);
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
            while (_nopTimer.ElapsedTicks < durationTicks)
            {

            }
        }
        
        private void RenderAudioCallback(XtStream stream, object input, object output, int frames, double time,
            ulong position, bool timeValid, ulong error, object user)
        {
            CurrentScene?.GetAudioData(((IntPtr)output), frames);
        }
        
        private void XRunCallback(int index, object user)
        {
            _logger.Warn("Over/Underflow detected");
        }
        
        private void _traceCallback(XtLevel level, string message)
        {
            switch (level)
            {
                case XtLevel.Info:
                    _logger.Info(message);
                    break;
                case XtLevel.Error:
                    _logger.Error(message);
                    break;
                case XtLevel.Fatal:
                    _logger.Fatal(message);
                    break;
            }
        }

        private void _fatalCallback()
        {
            _logger.Fatal("Fatal error in audio driver");
        }
    }
}
