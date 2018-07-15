using System;
using System.IO;
using System.Runtime.InteropServices;
using LibRetro.Types;
using System.Collections.Generic;
using LibRetro.Native;

namespace LibRetro
{
    #region Callbacks

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate bool RetroEnvironment(RetroEnvironmentCommand cmd, void* data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RetroVideoRefresh(IntPtr data, uint width, uint height, ulong pitch);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RetroAudioSample(short left, short right);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate ulong RetroAudioSampleBatch(IntPtr data, ulong frames);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RetroInputPoll();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate short RetroInputState(uint port, RetroDevice device, uint index, uint id);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RetroLogCallbackWrapper(RetroLogLevel severity, string formattedString);

    #endregion

    #region Set Callbacks

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RetroSetEnvironment([MarshalAs(UnmanagedType.FunctionPtr)] RetroEnvironment environment);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RetroSetVideoRefresh([MarshalAs(UnmanagedType.FunctionPtr)] RetroVideoRefresh environment);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RetroSetAudioSample([MarshalAs(UnmanagedType.FunctionPtr)] RetroAudioSample environment);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RetroSetAudioSampleBatch(
        [MarshalAs(UnmanagedType.FunctionPtr)] RetroAudioSampleBatch environment);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RetroSetInputPoll([MarshalAs(UnmanagedType.FunctionPtr)] RetroInputPoll environment);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RetroSetInputState([MarshalAs(UnmanagedType.FunctionPtr)] RetroInputState environment);

    #endregion

    #region Global Initialization/Deinitialization

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RetroInit();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RetroDeinit();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate uint RetroApiVersion();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RetroGetSystemInfo(out RetroSystemInfo info);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RetroGetSystemAvInfo(out RetroSystemAvInfo info);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RetroSetControllerPortDevice(uint port, RetroDevice device);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RetroReset();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RetroRun();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate ulong RetroSerializeSize();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool RetroSerialize(IntPtr data, ulong size);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool RetroUnserialize(IntPtr data, ulong size);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RetroCheatReset();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RetroCheatSet(uint index, bool enabled, IntPtr code);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate bool RetroLoadGame(ref RetroGameInfo game);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool RetroLoadGameSpecial(uint gameType, ref RetroGameInfo game, ulong numInfo);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RetroUnloadGame();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate uint RetroGetRegion();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr RetroGetMemoryData(uint id);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate ulong RetroGetMemorySize(uint id);

    #endregion

    public class Core : IDisposable
    {
        private readonly IntPtr _pDll;
        private readonly List<GCHandle> _delegateHandles;

        #region Core API Callbacks

        private readonly RetroSetEnvironment _retroSetEnvironment;
        private readonly RetroSetVideoRefresh _retroSetVideoRefresh;
        private readonly RetroSetAudioSample _retroSetAudioSample;
        private readonly RetroSetAudioSampleBatch _retroSetAudioSampleBatch;
        private readonly RetroSetInputPoll _retroSetInputPoll;
        private readonly RetroSetInputState _retroSetInputState;

        #endregion

        #region Core API Calls

        public readonly RetroInit RetroInit;
        public readonly RetroDeinit RetroDeinit;
        public readonly RetroApiVersion RetroApiVersion;

        public readonly RetroGetSystemInfo RetroGetSystemInfo;
        public readonly RetroGetSystemAvInfo RetroGetSystemAvInfo;

        public readonly RetroSetControllerPortDevice RetroSetControllerPortDevice;
        public readonly RetroReset RetroReset;
        public readonly RetroRun RetroRun;

        public readonly RetroSerializeSize RetroSerializeSize;
        public readonly RetroSerialize RetroSerialize;
        public readonly RetroUnserialize RetroUnserialize;

        public readonly RetroCheatReset RetroCheatReset;
        public readonly RetroCheatSet RetroCheatSet;

        public readonly RetroLoadGame RetroLoadGame;
        public readonly RetroLoadGameSpecial RetroLoadGameSpecial;

        public readonly RetroUnloadGame RetroUnloadGame;
        public readonly RetroGetRegion RetroGetRegion;

        public readonly RetroGetMemoryData RetroGetMemoryData;
        public readonly RetroGetMemorySize RetroGetMemorySize;

        #endregion

        private RetroEnvironment _wrappedRetroEnvironment;
        private RetroEnvironment _retroEnvironment;
        private RetroLogCallbackWrapper _retroLogCallbackWrapper;
        private RetroLogPrintf _internalRetroLogPrintf;

        public Core(string coreDllPath)
        {
            if (!File.Exists(coreDllPath))
            {
                throw new FileNotFoundException("Core DLL was not found");
            }

            _pDll = NativeHelper.LoadLibrary(coreDllPath);

            if (_pDll == IntPtr.Zero)
            {
                if (!NativeHelper.IsLinux())
                {
                    throw new System.ComponentModel.Win32Exception();
                }
                else
                {
                    throw new Exception("Error loading the core library");                    
                }
            }

            _retroSetEnvironment = LoadFunction<RetroSetEnvironment>("retro_set_environment");
            _retroSetVideoRefresh = LoadFunction<RetroSetVideoRefresh>("retro_set_video_refresh");
            _retroSetAudioSample = LoadFunction<RetroSetAudioSample>("retro_set_audio_sample");
            _retroSetAudioSampleBatch = LoadFunction<RetroSetAudioSampleBatch>("retro_set_audio_sample_batch");
            _retroSetInputPoll = LoadFunction<RetroSetInputPoll>("retro_set_input_poll");
            _retroSetInputState = LoadFunction<RetroSetInputState>("retro_set_input_state");

            RetroInit = LoadFunction<RetroInit>("retro_init");
            RetroDeinit = LoadFunction<RetroDeinit>("retro_deinit");
            RetroApiVersion = LoadFunction<RetroApiVersion>("retro_api_version");

            RetroGetSystemInfo = LoadFunction<RetroGetSystemInfo>("retro_get_system_info");
            RetroGetSystemAvInfo = LoadFunction<RetroGetSystemAvInfo>("retro_get_system_av_info");

            RetroSetControllerPortDevice =
                LoadFunction<RetroSetControllerPortDevice>("retro_set_controller_port_device");
            RetroReset = LoadFunction<RetroReset>("retro_reset");
            RetroRun = LoadFunction<RetroRun>("retro_run");

            RetroSerializeSize = LoadFunction<RetroSerializeSize>("retro_serialize_size");
            RetroSerialize = LoadFunction<RetroSerialize>("retro_serialize");
            RetroUnserialize = LoadFunction<RetroUnserialize>("retro_unserialize");

            RetroCheatReset = LoadFunction<RetroCheatReset>("retro_cheat_reset");
            RetroCheatSet = LoadFunction<RetroCheatSet>("retro_cheat_set");

            RetroLoadGame = LoadFunction<RetroLoadGame>("retro_load_game");
            RetroLoadGameSpecial = LoadFunction<RetroLoadGameSpecial>("retro_load_game_special");

            RetroUnloadGame = LoadFunction<RetroUnloadGame>("retro_unload_game");
            RetroGetRegion = LoadFunction<RetroGetRegion>("retro_get_region");

            RetroGetMemoryData = LoadFunction<RetroGetMemoryData>("retro_get_memory_data");
            RetroGetMemorySize = LoadFunction<RetroGetMemorySize>("retro_get_memory_size");

            _delegateHandles = new List<GCHandle>();
        }

        public void SetupCallbacks(
            RetroEnvironment retroEnvironment,
            RetroVideoRefresh retroVideoRefresh,
            RetroAudioSample retroAudioSample,
            RetroAudioSampleBatch retroAudioSampleBatch,
            RetroInputPoll retroInputPoll,
            RetroInputState retroInputState,
            RetroLogCallbackWrapper retroLogCallbackWrapper
        )
        {
            _wrappedRetroEnvironment = retroEnvironment;
            _retroLogCallbackWrapper = retroLogCallbackWrapper;
            
            unsafe
            {
                _retroEnvironment = _internalRetroEnvironment;
                _internalRetroLogPrintf = _retroLogCallback;
            }

            _delegateHandles.Add(GCHandle.Alloc(_retroEnvironment));
            _delegateHandles.Add(GCHandle.Alloc(_internalRetroLogPrintf));
            _delegateHandles.Add(GCHandle.Alloc(retroVideoRefresh));
            _delegateHandles.Add(GCHandle.Alloc(retroAudioSample));
            _delegateHandles.Add(GCHandle.Alloc(retroAudioSampleBatch));
            _delegateHandles.Add(GCHandle.Alloc(retroInputPoll));
            _delegateHandles.Add(GCHandle.Alloc(retroInputState));

            _retroSetEnvironment(_retroEnvironment);
            _retroSetVideoRefresh(retroVideoRefresh);
            _retroSetAudioSample(retroAudioSample);
            _retroSetAudioSampleBatch(retroAudioSampleBatch);
            _retroSetInputPoll(retroInputPoll);
            _retroSetInputState(retroInputState);
        }

        private unsafe bool _internalRetroEnvironment(RetroEnvironmentCommand cmd, void* data)
        {
            if (cmd == RetroEnvironmentCommand.GetLogInterface)
            {
                var callback = (RetroLogCallback*) data;
                callback->Log = _internalRetroLogPrintf;

                return true;
            }
            else
            {
                return _wrappedRetroEnvironment(cmd, data);
            }
        }

        private T LoadFunction<T>(string functionName)
        {
            var pAddressOfFunctionToCall = NativeHelper.GetProcAddress(_pDll, functionName);

            if (pAddressOfFunctionToCall != IntPtr.Zero)
                return Marshal.GetDelegateForFunctionPointer<T>(pAddressOfFunctionToCall);

            throw new Exception(
                $"Error loading the core library function \'{functionName}\'");
        }

        public void Dispose()
        {
            foreach (var handle in _delegateHandles)
            {
                handle.Free();
            }
            _delegateHandles.Clear();
            NativeHelper.FreeLibrary(_pDll);
        }

        private void _retroLogCallback(RetroLogLevel level, string format,
            IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8,
            IntPtr arg9, IntPtr arg10, IntPtr arg11, IntPtr arg12)
        {
            int argumentsToPush;
            
            try
            {
                argumentsToPush = NativeHelper.GetFormatArgumentCount(format);
            }
            catch (NotImplementedException e)
            {
                _retroLogCallbackWrapper(
                    RetroLogLevel.Warn, e.Message
                );
                return;
            }

            if (argumentsToPush >= 12)
            {
                _retroLogCallbackWrapper(
                    RetroLogLevel.Warn, $"Too many arguments (\'{argumentsToPush.ToString()}\') supplied to retroLogCallback"
                );
            }

            NativeHelper.Sprintf(
                out var formattedData, 
                format,
                argumentsToPush >= 1 ? arg1 : IntPtr.Zero,
                argumentsToPush >= 2 ? arg2 : IntPtr.Zero,
                argumentsToPush >= 3 ? arg3 : IntPtr.Zero,
                argumentsToPush >= 4 ? arg4 : IntPtr.Zero,
                argumentsToPush >= 5 ? arg5 : IntPtr.Zero,
                argumentsToPush >= 6 ? arg6 : IntPtr.Zero,
                argumentsToPush >= 7 ? arg7 : IntPtr.Zero,
                argumentsToPush >= 8 ? arg8 : IntPtr.Zero,
                argumentsToPush >= 9 ? arg9 : IntPtr.Zero,
                argumentsToPush >= 10 ? arg10 : IntPtr.Zero,
                argumentsToPush >= 11 ? arg11 : IntPtr.Zero,
                argumentsToPush >= 12 ? arg12 : IntPtr.Zero
            );

            _retroLogCallbackWrapper(level, formattedData);
        }
    }
}