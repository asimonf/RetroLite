using System;
using System.Reflection;
using LibArvid;
using NLog;
using NLog.Config;
using NLog.Targets;
using Redbus;
using Redbus.Configuration;
using Redbus.Interfaces;
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
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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
                
                Logger.Info("Initializing Arvid");

                while (!ArvidClient.Connect("192.168.2.101"))
                {
                    Logger.Error("Could not connect to Arvid");
                }

                Logger.Info("Initializing SDL");

                if (SDL.SDL_Init(SDL.SDL_INIT_JOYSTICK | SDL.SDL_INIT_VIDEO) != 0)
                {
                    Logger.Error(new Exception("SDL Initialization error"));
                }

#if(DEBUG)
                SDL.SDL_GetVersion(out var version);
                Logger.Debug($"SDL: {version.major}.{version.minor}.{version.patch}");
#endif

                if (SDL_ttf.TTF_Init() != 0)
                {
                    Logger.Error(new Exception("TTF Initialization error"));
                }
                
                SDL.SDL_CreateWindow(
                    "RetroLite", 
                    SDL.SDL_WINDOWPOS_UNDEFINED, 
                    SDL.SDL_WINDOWPOS_UNDEFINED, 
                    320, 
                    240, 
                    SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS
                );

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
                if (ArvidClient.IsConnected) ArvidClient.arvid_client_close();
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
            container.RegisterInstance<IEventBusConfiguration>(
                new EventBusConfiguration
                {
                    ThrowSubscriberException = false
                }
            );

            // Register renderer (TODO: maybe make it configurable?)
            container.Register<IRenderer, SurfaceRenderer>(Lifestyle.Singleton);

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
            container.RegisterSingleton<RawRenderer>();

            // Register Scenes
//            container.Collection.Register<IScene>(Assembly.GetExecutingAssembly());

//            container.RegisterSingleton<RetroCoreManager>();
            
            container.Collection.Register<IScene>(typeof(RetroCoreManager));

            
            // Register API components
            container.RegisterSingleton<ApiRouter>();
            container.Register<CefRequestHandler, ApiRequestHandler>(Lifestyle.Singleton);
            container.Collection.Register<IAction>(Assembly.GetExecutingAssembly());

            return container;
        }

        private static void InitializeLogger()
        {
            var loggingConfiguration = new LoggingConfiguration();
            var consoleTarget = new ColoredConsoleTarget();
            consoleTarget.Layout = "${date: format = HH\\:MM\\:ss}: ${message}";
            loggingConfiguration.AddTarget("console", consoleTarget);
#if(DEBUG)
            var logLevel = LogLevel.Debug;
#else
            var logLevel = LogLevel.Error;
#endif
            var rule1 = new LoggingRule("*", logLevel, consoleTarget);
            loggingConfiguration.LoggingRules.Add(rule1);
            LogManager.Configuration = loggingConfiguration;
        }
    }
}