using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using LiteDB;
using Redbus;
using RetroLite.DB.Entity;
using RetroLite.Event;

namespace RetroLite.DB
{
    public class StateManager
    {
        private readonly LiteRepository _db;

        private Task _scanGamesTask = null;
        private Task _initializeTask = null;

        public bool Initialized { get; private set; }
        public bool ScannedForGames { get; private set; }

        public StateManager()
        {
            _db = new LiteRepository(Path.Combine(Environment.CurrentDirectory, "retrolite.db"));
        }

        ~StateManager()
        {
            _db.Dispose();
        }
        
        private void _initialize()
        {
            var systems = Directory.GetDirectories(Path.Combine(Environment.CurrentDirectory, "cores"));
                
            foreach (var systemPath in systems)
            {
                var system = Path.GetFileNameWithoutExtension(systemPath);
                
                _db.Upsert(new Entity.System()
                {
                    Name = system
                });

                var cores = Directory.GetFiles(systemPath, "*.dll");
                    
                foreach (var core in cores)
                {
                    _db.Upsert(new Core()
                    {
                        Name = Path.GetFileNameWithoutExtension(core),
                        Path = core,
                        System = system
                    });
                    Program.EventBus.Publish(new LoadCoreEvent(core, system));
                }
            }

            Initialized = true;
        }

        private void _scanGames(string basePath)
        {
            var systems = Directory.GetDirectories(basePath);
                
            foreach (var systemPath in systems)
            {
                var system = Path.GetFileNameWithoutExtension(systemPath);
                
                _db.Upsert(new Entity.System()
                {
                    Name = system
                });

                var gameFiles = Directory.GetFiles(systemPath);

                foreach (var gameFile in gameFiles)
                {
                    _db.Upsert(new Game()
                    {
                        Name = Path.GetFileNameWithoutExtension(gameFile),
                        Path = gameFile,
                        System = system
                    });
                    
                    Console.WriteLine(gameFile);
                }
            }
        }

        /// <summary>
        /// Scans a path for games, updating the database with anything found on the route
        /// </summary>
        /// <param name="basePath"></param>
        /// <returns>True when scanning started successfully</returns>
        public void ScanForGames(string basePath)
        {
            ScannedForGames = false;

            if (_scanGamesTask != null) return;
            
            _scanGamesTask = new Task(() => _scanGames(basePath));
            _scanGamesTask.Start();
        }

        public void Initialize()
        {
            if (Initialized || _initializeTask != null) return;

            _initializeTask = new Task(_initialize);
            _initializeTask.Start();
        }
        
        public IList<Game> GetGameList()
        {
            return _db.Query<Game>()
                .ToList();
        }
    }
}