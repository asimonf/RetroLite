using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using LibRetro;
using LibRetro.Types;
using NLog;
using RetroLite.Audio;
using RetroLite.Event;
using RetroLite.Input;
using RetroLite.Menu;
using RetroLite.Scene;
using RetroLite.Video;
using SDL2;
using SRC_CS;

namespace RetroLite.RetroCore
{
    partial class RetroLite : IDisposable, IScene
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        
        private readonly string _coreName;
        private readonly Dictionary<Delegate, GCHandle> _delegateDictionary;
        private readonly string _systemDirectory;
        
        private readonly SceneManager _manager;
        private readonly EventProcessor _eventProcessor;
        private readonly IRenderer _renderer;
        private readonly Core _core;

        private readonly List<InputDescriptor> _inputDescriptors;
        private readonly Dictionary<string, CoreVariable> _coreVariables;

        public bool GameLoaded { get; private set; }

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
        private readonly byte[] _temporaryAudioBuffer;
        private readonly float[] _temporaryConversionBuffer;
        private readonly float[] _temporaryResampleBuffer;
        private IntPtr _resamplerState;
        private readonly CircularBuffer _audioBuffer;

        #endregion

        #region Callback Delegates

        private RetroFrameTimeCallback _currentFrameTimeCallback;
        private RetroFrameTimeCallback _prevFrameTimeCallback;

        #endregion

        public RetroLite(string dllPath, SceneManager manager, EventProcessor eventProcessor, IRenderer renderer)
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
            _temporaryAudioBuffer = new byte[8192];
            _temporaryConversionBuffer = new float[8192];
            _temporaryResampleBuffer = new float[8192];
            _audioBuffer = new CircularBuffer(8192 * 4);

            _manager = manager;
            _eventProcessor = eventProcessor;
            _renderer = renderer;
            
            _core.RetroInit();
        }

        public void Start()
        {
            
        }

        public void Reset()
        {
            _core.RetroReset();
            _updateAudioContext();
            _videoContextUpdated = true;
        }

        public void LoadGame(string path)
        {
            _logger.Debug("Loading game: {0}", path);
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
            if (_videoContextUpdated || height != _framebufferHeight || width != _framebufferWidth)
            {
                _videoContextUpdated = false;
                _framebufferHeight = (int) height;
                _framebufferWidth = (int) width;
                _recreateFramebuffer();
            }

            if (IntPtr.Zero == data)
            {
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
            _temporaryAudioBuffer[_temporaryAudioBufferPosition] = (byte) (left & 0xFF);
            _temporaryAudioBuffer[_temporaryAudioBufferPosition + 1] = (byte) (left >> 8);
            _temporaryAudioBuffer[_temporaryAudioBufferPosition + 2] = (byte) (right & 0xFF);
            _temporaryAudioBuffer[_temporaryAudioBufferPosition + 3] = (byte) (right >> 8);
            _temporaryAudioBufferPosition += 4;
        }

        private ulong _audioSampleBatch(IntPtr data, ulong frames)
        {
            var size = (int) frames * 4;
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
            if (_eventProcessor[(int) port] == null)
            {
                return 0;
            }

            switch (device)
            {
                case RetroDevice.Joypad:
                    switch ((RetroDeviceIdJoypad) id)
                    {
                        case RetroDeviceIdJoypad.A:
                            return _eventProcessor[(int) port].B == GameControllerButtonState.Down ? short.MaxValue : (short) 0;
                        case RetroDeviceIdJoypad.B:
                            return _eventProcessor[(int) port].A == GameControllerButtonState.Down ? short.MaxValue : (short) 0;
                        case RetroDeviceIdJoypad.DOWN:
                            return _eventProcessor[(int) port].DpadDown == GameControllerButtonState.Down ? short.MaxValue : (short) 0;
                        case RetroDeviceIdJoypad.L:
                            return _eventProcessor[(int) port].LeftShoulder == GameControllerButtonState.Down ? short.MaxValue : (short) 0;
                        case RetroDeviceIdJoypad.L2:
                            return _eventProcessor[(int) port].LeftTrigger;
                        case RetroDeviceIdJoypad.L3:
                            return _eventProcessor[(int) port].LeftStick == GameControllerButtonState.Down ? short.MaxValue : (short) 0;
                        case RetroDeviceIdJoypad.LEFT:
                            return _eventProcessor[(int) port].DpadLeft == GameControllerButtonState.Down ? short.MaxValue : (short) 0;
                        case RetroDeviceIdJoypad.R:
                            return _eventProcessor[(int) port].RightShoulder == GameControllerButtonState.Down ? short.MaxValue : (short) 0;
                        case RetroDeviceIdJoypad.R2:
                            return _eventProcessor[(int) port].RightTrigger;
                        case RetroDeviceIdJoypad.R3:
                            return _eventProcessor[(int) port].RightStick == GameControllerButtonState.Down ? short.MaxValue : (short) 0;
                        case RetroDeviceIdJoypad.RIGHT:
                            return _eventProcessor[(int) port].DpadRight == GameControllerButtonState.Down ? short.MaxValue : (short) 0;
                        case RetroDeviceIdJoypad.SELECT:
                            return _eventProcessor[(int) port].Back == GameControllerButtonState.Down ? short.MaxValue : (short) 0;
                        case RetroDeviceIdJoypad.START:
                            return _eventProcessor[(int) port].Start == GameControllerButtonState.Down ? short.MaxValue : (short) 0;
                        case RetroDeviceIdJoypad.UP:
                            return _eventProcessor[(int) port].DpadUp == GameControllerButtonState.Down ? short.MaxValue : (short) 0;
                        case RetroDeviceIdJoypad.X:
                            return _eventProcessor[(int) port].Y == GameControllerButtonState.Down ? short.MaxValue : (short) 0;
                        case RetroDeviceIdJoypad.Y:
                            return _eventProcessor[(int) port].X == GameControllerButtonState.Down ? short.MaxValue : (short) 0;
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
                    _logger.Debug(message, formattedString, _coreName);
                    break;
                case RetroLogLevel.Info:
                    _logger.Info(message, formattedString, _coreName);
                    break;
                case RetroLogLevel.Warn:
                    _logger.Warn(message, formattedString, _coreName);
                    break;
                case RetroLogLevel.Error:
                    _logger.Error(message, formattedString, _coreName);
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
            if (_resamplerState != IntPtr.Zero)
            {
                _logger.Debug("Freeing resampler state");
                SampleRate.src_delete(_resamplerState);
            }

            _logger.Debug("Freeing libretro core");
            _core.Dispose();

            _logger.Debug("Freeing callback handles");
            foreach (var handle in _delegateDictionary.Values)
            {
                handle.Free();
            }
        }

        public void Stop()
        {
        }

        public void Pause()
        {
        }

        public void Resume()
        {
            _videoContextUpdated = true;
        }

        public void HandleEvents()
        {
            for (var port = 0; port < EventProcessor.MaxPorts; port++)
            {
                // Ignore if there's no device connected
                if (_eventProcessor[port] == null) continue;
                
                // Switch to menu if guide is pressed
                if (_eventProcessor[port].Guide == GameControllerButtonState.Up)
                {
                    Program.EventBus.Publish(new OpenMenuEvent(this));
                    break;                    
                }
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
                    Console.Write("x");
                    _audioBuffer.Glitches = 0;
                }
            }
        }

        public void GetAudioData(IntPtr buffer, int frames)
        {
            // One frame is 2 samples of 2 bytes each (short) 
            _audioBuffer.CopyTo(buffer, frames  * 2 * 2);
        }

        #endregion

        #region Helpers
        private void _updateAudioContext()
        {
            if (_resamplerState != IntPtr.Zero)
            {
                SampleRate.src_delete(_resamplerState);
            }

            _resamplerState = SampleRate.src_new(SampleRate.Quality.SRC_SINC_BEST_QUALITY, 2, out var error);

            if (error > 0)
            {
                _logger.Error("Error initializing resampler: '{0}'", SampleRate.src_strerror(error));
            }

            _audioResampleRatio = _manager.AudioFormat.mix.rate / _currentSystemAvInfo.Timing.SampleRate;
            _logger.Debug("Audio Resample Ratio: {0}", _audioResampleRatio);
        }

        private void _resampleAudioData()
        {
            unsafe
            {
                var frames = _temporaryAudioBufferPosition / 4;
                fixed (byte* dataPtr = &_temporaryAudioBuffer[0])
                {
                    var shortDataPtr = (short*) dataPtr;
                    fixed (float* conversionPtr = &_temporaryConversionBuffer[0], resamplePtr =
                        &_temporaryResampleBuffer[0])
                    {
                        SampleRate.src_short_to_float_array(shortDataPtr, conversionPtr, _temporaryAudioBufferPosition);

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
                            _logger.Error(SampleRate.src_strerror(res));
                        }
                        else
                        {
                            SampleRate.src_float_to_short_array(resamplePtr, shortDataPtr,
                                convert.output_frames_gen * 2);
                            var size = convert.output_frames_gen * 4;
                            _audioBuffer.CopyFrom(_temporaryAudioBuffer, size);
                        }
                    }
                }

                _temporaryAudioBufferPosition = 0;
            }
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