using System;
using System.Reflection;
using NLog;
using NLog.Config;
using NLog.Targets;
using Redbus;
using RetroLite.DB;
using RetroLite.Event;
using RetroLite.Input;
using RetroLite.Menu;
using RetroLite.Menu.WebAPI;
using RetroLite.Menu.WebAPI.Action;
using RetroLite.RetroCore;
using RetroLite.Scene;
using RetroLite.Video;
using SDL2;
using SimpleInjector;
using Xilium.CefGlue;

namespace RetroLite
{
    internal static class Program
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static bool Running { get; set; } = true;
        
        [STAThread]
        public static int Main(string[] args)
        {
            var cefMainArgs = new CefMainArgs(args);
            var cefApp = new MenuCefApp();

            var res = CefRuntime.ExecuteProcess(cefMainArgs, cefApp, IntPtr.Zero);
            
            if (res >= 0)
            {
                return res;
            }

            try
            {
                InitializeLogger();
                
                Logger.Info("Initializing SDL");

                if (SDL.SDL_Init(0) != 0)
                {
                    Logger.Error(new Exception("SDL Initialization error"));
                }
                
                if (SDL_ttf.TTF_Init() != 0)
                {
                    Logger.Error(new Exception("TTF Initialization error"));
                }

                // Container
                using (var container = SetupContainer(cefMainArgs, cefApp))
                {
                    var sceneManager = container.GetInstance<SceneManager>();

                    while (Running)
                    {
                        sceneManager.RunLoop();
                    }
                }

                return 0;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return 1;
            }
            finally
            {
                SDL_ttf.TTF_Quit();
                SDL.SDL_Quit();                
            }
        }

        private static Container SetupContainer(CefMainArgs cefMainArgs, CefApp cefApp)
        {
            var container = new Container();
            
            // Register Cef related instances
            container.RegisterInstance(cefMainArgs);
            container.RegisterInstance(cefApp);

            // Register renderer (TODO: maybe make it configurable?)
            container.Register<IRenderer, SoftwareRenderer>(Lifestyle.Singleton);

            // Register main components
            container.RegisterSingleton<InputProcessor>();
            container.RegisterSingleton<SceneManager>();
            container.RegisterSingleton<MenuRenderer>();
            container.RegisterSingleton<MenuBrowserClient>();
            container.RegisterSingleton<StateManager>();
            container.RegisterSingleton<EventProcessor>();
            container.RegisterSingleton<EventBus>();
            container.RegisterSingleton<Config>();
            container.RegisterSingleton<RetroCoreFactory>();
            
            // Register Scenes
            container.Collection.Register<IScene>(Assembly.GetExecutingAssembly());

            // Register API components
            container.RegisterSingleton<ApiRouter>();
            container.Register<CefRequestHandler, ApiRequestHandler>(Lifestyle.Singleton);
            container.Collection.Register<IAction>(Assembly.GetExecutingAssembly());

            return container;
        }
        
        private static unsafe void InitializeLogger()
        {
            var loggingConfiguration = new LoggingConfiguration();
            var consoleTarget = new ColoredConsoleTarget();
            consoleTarget.Layout = "${date: format = HH\\:MM\\:ss}: ${message}";
            loggingConfiguration.AddTarget("console", consoleTarget);
            var rule1 = new LoggingRule("*", LogLevel.Debug, consoleTarget);
            loggingConfiguration.LoggingRules.Add(rule1);
            LogManager.Configuration = loggingConfiguration;
        }
    }
}