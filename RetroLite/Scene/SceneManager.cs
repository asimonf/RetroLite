using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using RetroLite.Event;
using RetroLite.Input;
using RetroLite.Video;
using Xt;

namespace RetroLite.Scene
{
    public class SceneManager : IDisposable
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IScene[] _scenes;

        private readonly IRenderer _renderer;
        private readonly InputProcessor _inputProcessor;
        private readonly EventProcessor _eventProcessor;
        private readonly Config _config;
        
        private readonly XtAudio _xtAudio;
        private readonly XtDevice _xtDevice;
        private readonly XtStream _xtStream;
        private readonly float[] _tmpAudioBuffer;

        private readonly Stopwatch _nopTimer;

        public SceneManager(
            IRenderer renderer, 
            InputProcessor inputProcessor, 
            EventProcessor eventProcessor, 
            IEnumerable<IScene> scenes,
            Config config
        )
        {
            _renderer = renderer;
            _inputProcessor = inputProcessor;
            _eventProcessor = eventProcessor;

            _scenes = scenes.ToArray();
            Array.Sort(_scenes);

            _config = config;
            
            _nopTimer = new Stopwatch();
            _nopTimer.Start();
            
            // Audio Related Stuff
            _tmpAudioBuffer = new float[8192];
            _xtAudio = new XtAudio(null, IntPtr.Zero, _traceCallback, _fatalCallback);
            var xtService = XtAudio.GetServiceBySetup(XtSetup.SystemAudio);
            var audioFormat = new XtFormat(new XtMix(_config.SampleRate, XtSample.Float32), 0, 0, 2, 0);
            _xtDevice = xtService.OpenDefaultDevice(true);
            _xtStream = _xtDevice.OpenStream(audioFormat, true, true, 16, RenderAudioCallback, XRunCallback, null);
            _xtStream.Start();
            
        }

        public void Dispose()
        {
            _xtStream.Stop();
            _xtStream.Dispose();
            _xtDevice.Dispose();
            _xtAudio.Dispose();
        }
        
        private void HandleEvents()
        {
            _inputProcessor.HandleEvents();
            _eventProcessor.HandleEvents();
            foreach (var scene in _scenes)
                scene.HandleEvents();
        }

        private void Update()
        {
            foreach (var scene in _scenes)
                scene.Update();
        }

        private void Draw()
        {
            _renderer.SetRenderDrawColor(0, 0, 0, 255);
            _renderer.RenderClear();
            foreach (var scene in _scenes)
                scene.Draw();
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
            var targetFrametime = (1000.0 / _config.TargetFps);
            
            if (!(targetFrametime > elapsedTime)) return;
            
            var durationTicks = (targetFrametime - elapsedTime) * (Stopwatch.Frequency / 1000.0);

            // Busy loop. This is to increase accuracy of the timing function
            _nopTimer.Restart();
            while (_nopTimer.ElapsedTicks < durationTicks) ;
        }
        
        private void RenderAudioCallback(XtStream stream, object input, object output, int frames, double time,
            ulong position, bool timeValid, ulong error, object user)
        {
            var sampleCount = frames * 2; // 2 channels per frame, interleaved
            
            if (_scenes.Length == 0)
            {
                for (var i = 0; i < sampleCount; i++) unsafe
                {
                    ((float*) ((IntPtr)output).ToPointer())[i] = 0;
                }

                return;
            }

            var firstSceneFrames = _scenes[0].GetAudioData(frames);

            if (firstSceneFrames == null)
            {
                Array.Clear(_tmpAudioBuffer, 0, sampleCount);
            }
            else
            {
                Array.Copy(firstSceneFrames, _tmpAudioBuffer, sampleCount);
            }

            for (var i = 1; i < _scenes.Length; i++)
            {
                var sceneFrames = _scenes[i].GetAudioData(frames);

                if (null == sceneFrames) continue;

                // Actual mixing. TODO: Optimize using SIMD intrinsics
                for (var j = 0; j < sampleCount; j++)
                {
                    var a = sceneFrames[j];
                    var b = _tmpAudioBuffer[j];

                    _tmpAudioBuffer[j] = a + b - a * b;
                }
            }
            
            Marshal.Copy(_tmpAudioBuffer, 0, (IntPtr)output, sampleCount);
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
