using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using LibRetro;
using LibRetro.Types;
using NLog;
using RetroLite.Input;
using RetroLite.Video;
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

        private RetroPixelFormat _pixelFormat;
        private RetroSystemAvInfo _currentSystemAvInfo;
        private IntPtr _framebuffer;
        private int _framebufferWidth, _framebufferHeight;
        private bool _videoContextUpdated;
        
        #endregion

        #region Audio Related Fields

        private double _audioResampleRatio;
        private int _temporaryAudioBufferPosition = 0;
        private readonly short[] _temporaryAudioBuffer;
        private readonly float[] _temporaryConversionBuffer;
        private readonly float[] _temporaryResampleBuffer;
        private readonly float[] _temporaryOutputBuffer;
        private IntPtr _resamplerState;
        private readonly CircularBuffer _audioBuffer;
        private bool _resampleNeeded;

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
            // TODO: tweak buffer sizes to minimize memory consumption
            _temporaryAudioBuffer = new short[8192];
            _temporaryConversionBuffer = new float[8192];
            _temporaryResampleBuffer = new float[8192];
            _temporaryOutputBuffer = new float[8192];
            _audioBuffer = new CircularBuffer(8192);

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
            Logger.Debug("Loading game: {0}", path);
            if (GameLoaded)
            {
                _core.RetroUnloadGame();
            }

            _core.RetroGetSystemInfo(out var systemInfo);
            var gameInfo = new RetroGameInfo();
            if (systemInfo.NeedFullpath)
            {
                var pathPtr = Marshal.StringToHGlobalAnsi(path);
                var metaPtr = IntPtr.Zero;
                gameInfo.Meta = metaPtr;
                gameInfo.Path = pathPtr;
                gameInfo.Data = IntPtr.Zero;
                gameInfo.size = 0;
                _core.RetroLoadGame(ref gameInfo);
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
                        _core.RetroLoadGame(ref gameInfo);
                    }                    
                }
            }

            GameLoaded = true;
            _core.RetroGetSystemAvInfo(out _currentSystemAvInfo);
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
            _temporaryAudioBuffer[_temporaryAudioBufferPosition] = left;
            _temporaryAudioBuffer[_temporaryAudioBufferPosition + 1] = right;
            _temporaryAudioBufferPosition += 2;
        }

        private ulong _audioSampleBatch(IntPtr data, ulong frames)
        {
            var size = (int) frames * 2;
            Marshal.Copy(data, _temporaryAudioBuffer, _temporaryAudioBufferPosition, size);
            _temporaryAudioBufferPosition += size;

            return frames;
        }

        private void _inputPoll()
        {
            //TODO: implement
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

        public void Draw()
        {
            if (GameLoaded && _framebuffer != IntPtr.Zero)
            {
                _renderer.RenderCopy(_framebuffer);

                if (_audioBuffer.Glitches > 0)
                {
                    // TODO: Do something with this info, maybe log it? maybe paint a pixel?
                    _audioBuffer.Glitches = 0;
                }
            }
        }

        public float[] GetAudioData(int frames)
        {
            // One frame is 2 samples 
            _audioBuffer.CopyTo(_temporaryOutputBuffer, frames * 2);

            return _temporaryOutputBuffer;
        }

        #endregion

        #region Helpers
        private void _updateAudioContext()
        {
            if (_resamplerState != IntPtr.Zero)
            {
                SampleRate.src_delete(_resamplerState);
            }

            if (_config.SampleRate != (int) _currentSystemAvInfo.Timing.SampleRate)
            {
                _resampleNeeded = true;
                _audioResampleRatio = _config.SampleRate / _currentSystemAvInfo.Timing.SampleRate;
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
                var frames = _temporaryAudioBufferPosition / 2;
                fixed (float* conversionPtr = &_temporaryConversionBuffer[0])
                {
                    fixed (short* shortDataPtr = &_temporaryAudioBuffer[0])
                    {
                        SampleRate.src_short_to_float_array(shortDataPtr, conversionPtr,
                            _temporaryAudioBufferPosition);
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

                            if (res != 0)
                            {
                                Logger.Error(SampleRate.src_strerror(res));
                            }
                            else
                            {
                                _audioBuffer.CopyFrom(_temporaryResampleBuffer, convert.output_frames_gen * 2);
                            }   
                        }
                    }
                    else
                    {
                        _audioBuffer.CopyFrom(_temporaryConversionBuffer, frames * 2);
                    }
                }
            }
            _temporaryAudioBufferPosition = 0;
        }

        private void _recreateFramebuffer()
        {
            if (_framebuffer != IntPtr.Zero)
            {
                _renderer.FreeTexture(_framebuffer);
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

            _framebuffer = _renderer.CreateTexture(
                format,
                SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING,
                _framebufferWidth,
                _framebufferHeight
            );
            
            _renderer.SetTextureBlendMode(_framebuffer, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
        }
        #endregion

        public override string ToString()
        {
            return _coreName;
        }
    }
}