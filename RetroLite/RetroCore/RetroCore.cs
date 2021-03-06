using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using LibRetro;
using LibRetro.Types;
using NLog;
using NLog.Targets;
using RetroLite.Input;
using RetroLite.Video;
using RubberBand;
using SDL2;
using SRC_CS;

namespace RetroLite.RetroCore
{
    public partial class RetroCore : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        private readonly string _coreName;
        private readonly Dictionary<Delegate, GCHandle> _delegateDictionary;
        private readonly string _systemDirectory;
        
        private readonly InputProcessor _inputProcessor;
        private readonly IRenderer _renderer;
        private readonly Core _core;
        private readonly Config _config;

        private readonly List<InputDescriptor> _inputDescriptors;
        private readonly Dictionary<string, CoreVariable> _coreVariables;

        public bool GameLoaded { get; private set; }

        public Core LowLevelCore => _core;

        #region Video Related Fields

        private RetroPixelFormat _pixelFormat = RetroPixelFormat.F_0RGB1555;
        private RetroSystemAvInfo _currentSystemAvInfo;
        private IntPtr _framebuffer;
        private IntPtr _frameField;
        private int _framebufferWidth, _framebufferHeight;
        private bool _videoContextUpdated;
        private bool _interlacing;
        private bool _isOddField;
        
        #endregion

        #region Audio Related Fields

        private double _audioResampleRatio;
        private readonly short[] _temporaryInputAudioBuffer;
        private readonly short[] _temporaryPreconversionAudioBuffer;
        private readonly float[] _temporaryConversionBuffer;
        private readonly float[][] _temporaryStretcherBuffer;
        private readonly float[] _temporaryResampleBuffer;
        private readonly CircularBuffer<float> _audioBuffer;
        private readonly CircularBuffer<short> _temporaryBuffer;
        private readonly float[] _temporaryOutputBuffer;
        private IntPtr _resamplerState;
        private RubberBandStretcher _stretcher;
        private bool _resampleNeeded;
        private long _latencyCounter;
        private bool _dataWasSent;

        #endregion

        #region Callback Delegates

        private RetroFrameTimeCallback _currentFrameTimeCallback;
        private RetroFrameTimeCallback _prevFrameTimeCallback;

        #endregion

        public RetroCore(string dllPath, Config config, InputProcessor inputProcessor, IRenderer renderer)
        {
            _systemDirectory = Path.Combine(
                Environment.CurrentDirectory,
                "system"
            );

            _delegateDictionary = new Dictionary<Delegate, GCHandle>();
            _inputDescriptors = new List<InputDescriptor>();
            _coreVariables = new Dictionary<string, CoreVariable>();
            
            RetroLogCallbackWrapper logCallback = _retroLogCallback;
            _delegateDictionary.Add(logCallback, GCHandle.Alloc(logCallback));

            _core = new Core(dllPath);
            _core.RetroGetSystemInfo(out var systemInfo);
            _coreName = Marshal.PtrToStringAnsi(systemInfo.LibraryName);
            
            unsafe
            {
                _core.SetupCallbacks(
                    _environment,
                    _videoRefresh,
                    _audioSample,
                    _audioSampleBatch,
                    _inputPoll,
                    _inputState,
                    _retroLogCallback
                );                
            }

            GameLoaded = false;

            // Audio Buffers
            _temporaryInputAudioBuffer = new short[8192];
            _temporaryPreconversionAudioBuffer = new short[8192];
            _temporaryConversionBuffer = new float[8192];
            _temporaryStretcherBuffer = new float[2][];
            _temporaryStretcherBuffer[0] = new float[8192];
            _temporaryStretcherBuffer[1] = new float[8192];
            _temporaryResampleBuffer = new float[8192];
            _temporaryOutputBuffer = new float[8192];
            _audioBuffer = new CircularBuffer<float>(8192);
            _temporaryBuffer = new CircularBuffer<short>(8192);

            _config = config;
            _inputProcessor = inputProcessor;
            _renderer = renderer;
            
            _core.RetroInit();
        }

        public void Reset()
        {
            _core.RetroReset();
            _updateAudioContext();
            _videoContextUpdated = true;
        }

        public void LoadGame(string path)
        {
            Logger.Info("Loading game: {0}", path);
            if (GameLoaded)
            {
                _core.RetroUnloadGame();
            }

            _core.RetroGetSystemInfo(out var systemInfo);
            var gameInfo = new RetroGameInfo();
            bool loaded;
            
            if (systemInfo.NeedFullpath)
            {
                var pathPtr = Marshal.StringToHGlobalAnsi(path);
                var metaPtr = IntPtr.Zero;
                gameInfo.Meta = metaPtr;
                gameInfo.Path = pathPtr;
                gameInfo.Data = IntPtr.Zero;
                gameInfo.size = 0;
                loaded = _core.RetroLoadGame(ref gameInfo);
            }
            else
            {
                unsafe
                {
                    var pathPtr = IntPtr.Zero;
                    var metaPtr = IntPtr.Zero;
                    var data = File.ReadAllBytes(path);
                    fixed (byte* dataPtr = &data[0])
                    {
                        gameInfo.Meta = metaPtr;
                        gameInfo.Path = pathPtr;
                        gameInfo.Data = new IntPtr(dataPtr);
                        gameInfo.size = (uint) data.Length;
                        loaded = _core.RetroLoadGame(ref gameInfo);
                    }                    
                }
            }

            if (!loaded)
            {
                Logger.Error("Could not load the game");
                return;
            }
            else
            {
                Logger.Debug("Game succesfully loaded");
            }
            
            GameLoaded = true;
            _core.RetroGetSystemAvInfo(out _currentSystemAvInfo);
            _renderer.SetMode(
                (int)_currentSystemAvInfo.Geometry.BaseWidth,
                (int)_currentSystemAvInfo.Geometry.BaseHeight,
                (float)_currentSystemAvInfo.Timing.Fps
            );
            _config.TargetFps = _currentSystemAvInfo.Timing.Fps;
            
            _videoContextUpdated = true;
            _updateAudioContext();
        }

        public void UnloadGame()
        {
            GameLoaded = false;
            _core.RetroUnloadGame();
        }

        #region Core Callbacks

//        private unsafe void _render(
//            IntPtr data, 
//            uint width, 
//            uint height, 
//            ulong pitch, 
//            uint bytesPerPixel, 
//            byte* destinationPtr
//        )
//        {
//            var destinationPitch = bytesPerPixel * width;
//            var sourcePtr = (byte*)data.ToPointer();
//
//            if (destinationPitch != pitch)
//            {
//                for (ulong i = 0; i < height; i++)
//                {
//                    Buffer.MemoryCopy(
//                        sourcePtr + i * pitch,
//                        destinationPtr + i * destinationPitch, 
//                        destinationPitch,
//                        destinationPitch
//                    );
//                }
//            }
//            else
//            {
//                Buffer.MemoryCopy(
//                    sourcePtr, 
//                    destinationPtr, 
//                    height * pitch, 
//                    height * pitch
//                );
//            }
//        } 
//        
//        private unsafe void _render16bit(IntPtr data, uint width, uint height, ulong pitch)
//        {
//            fixed (ushort* destinationPtr = &_framebuffer16bit[0])
//                _render(data, width, height, pitch, 4, (byte*)destinationPtr);
//
//            // Convert color depth to 15 bit
//            for (var i = 0; i < _framebufferHeight; i++)
//            {
//                for (var j = 0; j < _framebufferWidth; j++)
//                {
//                    var pos = i * _framebufferHeight + j;
//                    var srcPixel = _framebuffer16bit[pos];
//
//                    var R = (srcPixel & 0xF800) >> 11;
//                    var G = (srcPixel & 0x07E0) >> 5;
//                    var B = srcPixel & 0x001F;
//
//                    var dstPixel = (ushort)(R | ((G * 2 + 1) >> 2) << 5 | B);
//
//                    _framebuffer15bit[pos] = dstPixel;
//                }
//            }
//        }
//        
//        private unsafe void _render32bit(IntPtr data, uint width, uint height, ulong pitch)
//        {
//            fixed (uint* destinationPtr = &_framebuffer32bit[0])
//                _render(data, width, height, pitch, 4, (byte*)destinationPtr);
//
//            // Convert color depth to 15 bit
//            for (var i = 0; i < _framebufferHeight; i++)
//            {
//                for (var j = 0; j < _framebufferWidth; j++)
//                {
//                    var pos = i * _framebufferHeight + j;
//                    var srcPixel = _framebuffer32bit[pos];
//
//                    var R = (srcPixel & 0xFF0000) >> 16;
//                    var G = (srcPixel & 0x00FF00) >> 8;
//                    var B = srcPixel & 0x0000FF;
//
//                    var dstPixel = (ushort)(
//                        ((R * 249 + 1024) >> 11) << 10 |
//                        ((G * 249 + 1024) >> 11) << 5  |
//                        ((B * 249 + 1024) >> 11)
//                    );
//
//                    _framebuffer15bit[pos] = dstPixel;
//                }
//            }
//        }

        private void _videoRefresh(IntPtr data, uint width, uint height, ulong pitch)
        {
            if (IntPtr.Zero == _framebuffer || _videoContextUpdated || height != _framebufferHeight || width != _framebufferWidth)
            {
                _videoContextUpdated = false;
                _framebufferHeight = (int) height;
                _framebufferWidth = (int) width;
                _recreateFramebuffer();
            }

            if (IntPtr.Zero == data || IntPtr.Zero == _framebuffer)
            {
                Logger.Warn($"Invalid framebuffer in '{_coreName}'");
                return;
            }

            _renderer.LockTexture(_framebuffer, out var destination, out var destinationPitch);

            unsafe
            {
                if ((ulong) destinationPitch != pitch)
                {
                    var bytesPerPixel = _pixelFormat == RetroPixelFormat.F_XRGB8888 ? 4 : 2;

                    for (ulong i = 0; i < height; i++)
                    {
                        Buffer.MemoryCopy((byte*) data.ToPointer() + i * pitch,
                            (byte*) destination.ToPointer() + i * (ulong) destinationPitch, width * bytesPerPixel,
                            width * bytesPerPixel);
                    }
                }
                else
                {
                    Buffer.MemoryCopy(data.ToPointer(), destination.ToPointer(), height * pitch, height * pitch);
                }
            }

            _renderer.UnlockTexture(_framebuffer);
        }

        private void _audioSample(short left, short right)
        {
            _dataWasSent = true;
            _temporaryBuffer.AddSample(left);
            _temporaryBuffer.AddSample(right);
        }

        private ulong _audioSampleBatch(IntPtr data, ulong frames)
        {
            _dataWasSent = true;
            var size = (int) frames * 2;
            Marshal.Copy(data, _temporaryInputAudioBuffer, 0, size);
            _temporaryBuffer.CopyFrom(_temporaryInputAudioBuffer, size);

            return frames;
        }

        private void _inputPoll()
        {
//            _inputProcessor.Poll();
        }

        private short _inputState(uint port, RetroDevice device, uint index, uint id)
        {
            if (_inputProcessor[(int) port] == null)
            {
                return 0;
            }

            switch (device)
            {
                case RetroDevice.Joypad:
                    switch ((RetroDeviceIdJoypad) id)
                    {
                        case RetroDeviceIdJoypad.A:
                            return _inputProcessor[(int) port].B == GameControllerButtonState.Down ? short.MaxValue : (short) 0;
                        case RetroDeviceIdJoypad.B:
                            return _inputProcessor[(int) port].A == GameControllerButtonState.Down ? short.MaxValue : (short) 0;
                        case RetroDeviceIdJoypad.DOWN:
                            return _inputProcessor[(int) port].DpadDown == GameControllerButtonState.Down ? short.MaxValue : (short) 0;
                        case RetroDeviceIdJoypad.L:
                            return _inputProcessor[(int) port].LeftShoulder == GameControllerButtonState.Down ? short.MaxValue : (short) 0;
                        case RetroDeviceIdJoypad.L2:
                            return _inputProcessor[(int) port].LeftTrigger;
                        case RetroDeviceIdJoypad.L3:
                            return _inputProcessor[(int) port].LeftStick == GameControllerButtonState.Down ? short.MaxValue : (short) 0;
                        case RetroDeviceIdJoypad.LEFT:
                            return _inputProcessor[(int) port].DpadLeft == GameControllerButtonState.Down ? short.MaxValue : (short) 0;
                        case RetroDeviceIdJoypad.R:
                            return _inputProcessor[(int) port].RightShoulder == GameControllerButtonState.Down ? short.MaxValue : (short) 0;
                        case RetroDeviceIdJoypad.R2:
                            return _inputProcessor[(int) port].RightTrigger;
                        case RetroDeviceIdJoypad.R3:
                            return _inputProcessor[(int) port].RightStick == GameControllerButtonState.Down ? short.MaxValue : (short) 0;
                        case RetroDeviceIdJoypad.RIGHT:
                            return _inputProcessor[(int) port].DpadRight == GameControllerButtonState.Down ? short.MaxValue : (short) 0;
                        case RetroDeviceIdJoypad.SELECT:
                            return _inputProcessor[(int) port].Back == GameControllerButtonState.Down ? short.MaxValue : (short) 0;
                        case RetroDeviceIdJoypad.START:
                            return _inputProcessor[(int) port].Start == GameControllerButtonState.Down ? short.MaxValue : (short) 0;
                        case RetroDeviceIdJoypad.UP:
                            return _inputProcessor[(int) port].DpadUp == GameControllerButtonState.Down ? short.MaxValue : (short) 0;
                        case RetroDeviceIdJoypad.X:
                            return _inputProcessor[(int) port].Y == GameControllerButtonState.Down ? short.MaxValue : (short) 0;
                        case RetroDeviceIdJoypad.Y:
                            return _inputProcessor[(int) port].X == GameControllerButtonState.Down ? short.MaxValue : (short) 0;
                    }
                    break;
            }

            return 0;
        }

        private void _retroLogCallback(RetroLogLevel level, string formattedString)
        {
            const string message = "Core '{1}':\n---\n{0}\n---\n";

            switch (level)
            {
                case RetroLogLevel.Debug:
                    Logger.Debug(message, formattedString, _coreName);
                    break;
                case RetroLogLevel.Info:
                    Logger.Info(message, formattedString, _coreName);
                    break;
                case RetroLogLevel.Warn:
                    Logger.Warn(message, formattedString, _coreName);
                    break;
                case RetroLogLevel.Error:
                    Logger.Error(message, formattedString, _coreName);
                    break;
            }
        }

        private void _keyboardEvent(bool down, RetroKey keycode, uint character, ushort modifiers)
        {
            throw new NotImplementedException();
        }

        private bool _addImageIndex()
        {
            throw new NotImplementedException();
        }

        private bool _getEjectState()
        {
            throw new NotImplementedException();
        }

        private uint _getImageIndex()
        {
            throw new NotImplementedException();
        }

        private uint _getNumImages()
        {
            throw new NotImplementedException();
        }

        private bool _replaceImageIndex(uint index)
        {
            throw new NotImplementedException();
        }

        private bool _setEjectState(bool ejected)
        {
            throw new NotImplementedException();
        }

        private bool _setImageIndex(uint index)
        {
            throw new NotImplementedException();
        }

        private long _getTimeUsec()
        {
            throw new NotImplementedException();
        }

        private ulong _getCpuFeatures()
        {
            throw new NotImplementedException();
        }

        private ulong _perfGetCounter()
        {
            throw new NotImplementedException();
        }

        private void _perfRegister(ref RetroPerfCounter counter)
        {
            throw new NotImplementedException();
        }

        private void _perfStart(ref RetroPerfCounter counter)
        {
            throw new NotImplementedException();
        }

        private void _perfStop(ref RetroPerfCounter counter)
        {
            throw new NotImplementedException();
        }

        private void _perfLog()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IScene Methods
        public void Dispose()
        {
            _core.RetroDeinit();
            if (_resamplerState != IntPtr.Zero)
            {
                Logger.Debug("Freeing resampler state");
                SampleRate.src_delete(_resamplerState);
            }

            Logger.Debug("Freeing libretro core");
            _core.Dispose();

            Logger.Debug("Freeing callback handles");
            foreach (var handle in _delegateDictionary.Values)
            {
                handle.Free();
            }
        }

        public void Update()
        {
            if (GameLoaded)
            {
                _core.RetroRun();
                _resampleAudioData();
            }
        }

        public unsafe void Draw()
        {
            if (GameLoaded && _framebuffer != IntPtr.Zero)
            {
                if (!_interlacing)
                {
                    var destRect = new SDL.SDL_Rect()
                    {
                        h = _framebufferHeight,
                        w = _framebufferWidth,
                        x = 0,
                        y = 0
                    };
                    _renderer.RenderCopyDest(_framebuffer, ref destRect);                    
                }
                else
                {
                    try
                    {
                        _renderer.LockTexture(_framebuffer, out var framebufferPixels, out var framebufferPitch);
                        _renderer.LockTexture(_frameField, out var frameFieldPixels, out var frameFieldPitch);

                        for (var i = 0; i < _renderer.Height; i++)
                        {
                            var originScanline = _isOddField ? i * 2 : i * 2 + 1;
                        
                            Buffer.MemoryCopy(
                                (byte*)framebufferPixels.ToPointer() + originScanline * framebufferPitch, 
                                (byte*)frameFieldPixels.ToPointer() + i * frameFieldPitch, 
                                frameFieldPitch, 
                                framebufferPitch
                            );
                        }                            
                    }
                    finally
                    {
                        _renderer.UnlockTexture(_framebuffer);
                        _renderer.UnlockTexture(_frameField);
                    }

                    _isOddField = !_isOddField;
                    
                    var destRect = new SDL.SDL_Rect()
                    {
                        h = _framebufferHeight >> 1,
                        w = _framebufferWidth,
                        x = 0,
                        y = 0
                    };
                    _renderer.RenderCopyDest(_frameField, ref destRect);
                }
            }
        }

        public float[] GetAudioData(int frames)
        {
            var samples = frames * 2;
            if (samples <= _audioBuffer.CurrentLength)
                _audioBuffer.CopyTo(_temporaryOutputBuffer, samples);
            else
                Array.Clear(_temporaryOutputBuffer, 0, samples);

            return _temporaryOutputBuffer;
        }

        #endregion

        #region Helpers
        private void _updateAudioContext()
        {
            _dataWasSent = false;
            _stretcher?.Dispose();
            var speedRatio = (_renderer.RefreshRate / _config.TargetFps);
            Console.Write(speedRatio);
            _stretcher = new RubberBandStretcher(
                _config.SampleRate,
                2,
                RubberBandStretcher.Options.ProcessRealTime,
                1 / speedRatio,
                speedRatio 
            );

            _latencyCounter = _stretcher.GetLatency();
            
            if (_resamplerState != IntPtr.Zero)
            {
                SampleRate.src_delete(_resamplerState);
            }

            if (_config.SampleRate != (int) _currentSystemAvInfo.Timing.SampleRate)
            {
                _resampleNeeded = true;
                _audioResampleRatio = (_config.SampleRate / _currentSystemAvInfo.Timing.SampleRate);
                _resamplerState = SampleRate.src_new(SampleRate.Quality.SRC_SINC_BEST_QUALITY, 2, out var error);
    
                if (error > 0)
                {
                    Logger.Error("Error initializing resampler: '{0}'", SampleRate.src_strerror(error));
                }
    
                Logger.Debug("Audio Resample Ratio: {0}", _audioResampleRatio);
            }
            else
            {
                _resampleNeeded = false;
                Logger.Debug("Resampling not needed");
            }
        }

        private void _resampleAudioData()
        {
            unsafe
            {
                var sampleCount = _temporaryBuffer.CurrentLength;
                var frames = sampleCount / 2;
                fixed (float* conversionPtr = &_temporaryConversionBuffer[0])
                {
                    _temporaryBuffer.CopyTo(_temporaryPreconversionAudioBuffer, sampleCount);
                    fixed (short* shortDataPtr = &_temporaryPreconversionAudioBuffer[0])
                    {
                        SampleRate.src_short_to_float_array(shortDataPtr, conversionPtr,
                            sampleCount);
                    }

                    if (_resampleNeeded)
                    {
                        fixed (float* resamplePtr =
                            &_temporaryResampleBuffer[0])
                        {
                            var convert = new SampleRate.SRC_DATA()
                            {
                                data_in = conversionPtr,
                                data_out = resamplePtr,
                                input_frames = frames,
                                output_frames = frames * 2,
                                src_ratio = _audioResampleRatio
                            };
                            var res = SampleRate.src_process(_resamplerState, ref convert);

                            if (res != 0) Logger.Error(SampleRate.src_strerror(res));
                            else
                            {
                                for (var i = 0; i < convert.output_frames_gen; i++)
                                {
                                    _temporaryStretcherBuffer[0][i] = _temporaryResampleBuffer[i * 2];
                                    _temporaryStretcherBuffer[1][i] = _temporaryResampleBuffer[i * 2 + 1];
                                }
                    
                                _stretcher.Process(
                                    _temporaryStretcherBuffer,
                                    convert.output_frames_gen,
                                    false
                                );
                    
                                if (_stretcher.GetSamplesRequired() > 0) return;
                                
                                var availableFrames = _stretcher.Available();
                                _stretcher.Retrieve(_temporaryStretcherBuffer, availableFrames);
                                
                                for (var i = 0; i < availableFrames; i++)
                                {
                                    _temporaryResampleBuffer[i * 2] = _temporaryStretcherBuffer[0][i];
                                    _temporaryResampleBuffer[i * 2 + 1] = _temporaryStretcherBuffer[1][i];
                                }

                                _audioBuffer.CopyFrom(_temporaryResampleBuffer, availableFrames * 2);
                            }   
                        }
                    }
                    else
                    {
                        Console.WriteLine("else");
//                        _audioBuffer.CopyFrom(_temporaryConversionBuffer, frames * 2);
                    }
                }
            }
        }

        private void _recreateFramebuffer()
        {
            if (_framebuffer != IntPtr.Zero) _renderer.FreeTexture(_framebuffer);
            if (_frameField != IntPtr.Zero)
            {
                _renderer.FreeTexture(_frameField);
                _frameField = IntPtr.Zero;
            }

            var format = SDL.SDL_PIXELFORMAT_RGB888;

            switch (_pixelFormat)
            {
                case RetroPixelFormat.F_0RGB1555:
                    format = SDL.SDL_PIXELFORMAT_ARGB1555;
                    break;
                case RetroPixelFormat.F_RGB565:
                    format = SDL.SDL_PIXELFORMAT_RGB565;
                    break;
            }
            
            _isOddField = false;
            _interlacing = _framebufferHeight >= _renderer.Lines;

            _framebuffer = _renderer.CreateTexture(
                format,
                SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING,
                _framebufferWidth,
                _framebufferHeight
            );
            _frameField = _renderer.CreateTexture(
                format,
                SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING,
                _framebufferWidth,
                _framebufferHeight >> 1
            );
            
            _renderer.SetTextureBlendMode(_framebuffer, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
            _renderer.SetTextureBlendMode(_frameField, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
            
            Logger.Info(
                "New FB Size: {0}x{1} mode: {2}", 
                _framebufferWidth, 
                _framebufferHeight,
                !_interlacing ? "Progressive" : "Interlaced"
            );
            
            _renderer.SetMode(_framebufferWidth, _interlacing ? _framebufferHeight >> 1 : _framebufferHeight);
            _renderer.SetInterlacing(_interlacing);
        }
        #endregion

        public override string ToString()
        {
            return _coreName;
        }
    }
}