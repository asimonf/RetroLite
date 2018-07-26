using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using NLog;
using RetroLite.DB.Entity;
using RetroLite.Event;
using RetroLite.Input;
using RetroLite.Scene;
using RetroLite.Video;
using Logger = NLog.Logger;

namespace RetroLite.DB
{
    public class StateManager : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly LiteRepository _db;
        private readonly Dictionary<string, Dictionary<string, RetroCore.RetroCore>> _retroCoresBySystem;
        private readonly SceneManager _sceneManager;

        public StateManager(SceneManager manager)
        {
            _db = new LiteRepository(Path.Combine(Environment.CurrentDirectory, "retrolite.db"));
            _retroCoresBySystem = new Dictionary<string, Dictionary<string, RetroCore.RetroCore>>();
            _sceneManager = manager;
        }

        public void Dispose()
        {
            _db.Dispose();
        }
        
        private void _initialize()
        {
            var systems = Directory.GetDirectories(Path.Combine(Environment.CurrentDirectory, "cores"));
                
            foreach (var systemPath in systems)
            {
                var system = Path.GetFileNameWithoutExtension(systemPath);

                if (null == system) continue;

                var systemCoresList = new Dictionary<string, RetroCore.RetroCore>();
                
                var cores = Directory.GetFiles(systemPath, "*.dll");

                var extensions = new List<string>();
                    
                foreach (var core in cores)
                {
                    var retroCore = _sceneManager.CreateRetroCore(core, system);
                    retroCore.LowLevelCore.RetroGetSystemInfo(out var systemInfo);

                    var name = Path.GetFileNameWithoutExtension(core);

                    var coreExtensions = systemInfo.GetExtensions();
                    
                    _db.Upsert(new Core()
                    {
                        Name = name,
                        LibraryName = systemInfo.GetLibraryName(),
                        LibraryVersion = systemInfo.GetLibraryVersion(),
                        ValidExtensions = coreExtensions,
                        Path = core,
                        System = system,
                    });
                    
                    extensions.AddRange(coreExtensions);
                    
                    systemCoresList.Add(name, retroCore);
                }
                
                _db.Upsert(new Entity.System()
                {
                    Name = system,
                    ValidExtensions = extensions.ToArray(),
                });
                _retroCoresBySystem.Add(system, systemCoresList);
            }
        }

        

        private void _scanGames(string basePath)
        {
            var systems = Directory.GetDirectories(basePath);

            var systemCollection = _db.Database.GetCollection<Entity.System>();

            Parallel.ForEach(systems, (systemPath) =>
            {
                var systemName = Path.GetFileNameWithoutExtension(systemPath);

                var system = systemCollection.FindById(systemName);

                if (null == system)
                {
                    Logger.Warn($"No core loaded for system '{system}'");
                    return;
                }

                var gameFiles = Directory.EnumerateFiles(systemPath);

                Parallel.ForEach(gameFiles, (gameFile) =>
                {
                    var extension = Path.GetExtension(gameFile)?.Replace(".", "");

                    if (null == extension || !system.ValidExtensions.Contains(extension))
                    {
                        Logger.Debug($"Skipping file '{gameFile}'");
                        return;
                    }

                    _db.Upsert(new Game()
                    {
                        Id = new Guid(),
                        Name = Path.GetFileNameWithoutExtension(gameFile),
                        Path = gameFile,
                        System = systemName
                    });
                });
            });
        }

        /// <summary>
        /// Scans a path for games, updating the database with anything found on the route
        /// </summary>
        /// <param name="basePath"></param>
        /// <returns>The task object</returns>
        public Task ScanForGames(string basePath)
        {
            return Task.Factory.StartNew(() =>
            {
                _db.Delete<Game>(x => true);
                _scanGames(basePath);
            });
        }

        public Task Initialize()
        {
            return Task.Factory.StartNew(_initialize);
        }

        public RetroCore.RetroCore GetDefaultRetroCoreForSystem(string system)
        {
            return _retroCoresBySystem.ContainsKey(system) ? _retroCoresBySystem[system].Values.FirstOrDefault() : null;
        }
        
        public IList<Game> GetGameList()
        {
            return _db.Database.GetCollection<Game>()
                .FindAll()
                .OrderBy(x => x.Name)
                .ToList();
        }

        public IList<Core> GetCoreList()
        {
            return _db.Query<Core>()
                .ToList();
        }
        
        public IList<Entity.System> GetSystemList()
        {
            return _db.Query<Entity.System>()
                .ToList();
        }
    }
}