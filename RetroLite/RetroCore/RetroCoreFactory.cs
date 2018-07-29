using System;
using System.Diagnostics;
using System.IO;
using NLog;
using RetroLite.Input;
using RetroLite.Video;

namespace RetroLite.RetroCore
{
    public class RetroCoreFactory
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly InputProcessor _inputProcessor;
        private readonly IRenderer _renderer;
        private readonly Config _config;

        public RetroCoreFactory(InputProcessor inputProcessor, IRenderer renderer, Config config)
        {
            _inputProcessor = inputProcessor;
            _renderer = renderer;
            _config = config;
        }
        
        public RetroCore CreateRetroCore(string dll, string system)
        {
            Logger.Debug($"Loading core {dll}");
            var name = Path.GetFileNameWithoutExtension(dll);

            Debug.Assert(name != null, nameof(name) + " != null");

            try
            {
                var core = new RetroCore(dll, _config, _inputProcessor, _renderer);

                Logger.Debug($"Core '{name}' for system '{system}' loaded.");

                return core;
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);

                return null;
            }
        }
    }
}