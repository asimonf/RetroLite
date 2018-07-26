using System;
using System.Reflection;
using NLog;
using NLog.Config;
using NLog.Targets;
using Redbus;
using RetroLite.DB;
using RetroLite.Input;
using RetroLite.Intro;
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
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public static EventBus EventBus { get; } = new EventBus();

        public static bool Running { get; set; } = true;
        
        [STAThread]
        public static int Main(string[] args)
        {
            var cefMainArgs = new CefMainArgs(args);
            var cefApp = new MenuCefApp();

            var res = CefRuntime.ExecuteProcess(cefMainArgs, cefApp, windowsSandboxInfo: IntPtr.Zero);
            
            if (res >= 0)
            {
                return res;
            }

            try
            {
                InitializeLogger();
                
                _logger.Info("Initializing SDL");

                if (SDL.SDL_Init(SDL.SDL_INIT_JOYSTICK | SDL.SDL_INIT_VIDEO) != 0)
                {
                    _logger.Error(new Exception("SDL Initialization error"));
                }
                
                if (SDL_ttf.TTF_Init() != 0)
                {
                    _logger.Error(new Exception("TTF Initialization error"));
                }

                // Container
                using (var container = new Container())
                {
                    SetupContainer(cefMainArgs, cefApp, container);
    
                    var sceneManager = container.GetInstance<SceneManager>();
    
                    sceneManager.ChangeScene(
                        new IntroScene(
                            container.GetInstance<IRenderer>(), 
                            sceneManager, 
                            container.GetInstance<MenuScene>(),
                            container.GetInstance<StateManager>()
                        )
                    );
    
                    while (Running)
                    {
                        sceneManager.RunLoop();
                    }
                }

                return 0;
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return 1;
            }
            finally
            {
                SDL_ttf.TTF_Quit();
                SDL.SDL_Quit();                
            }
        }

        private static void SetupContainer(CefMainArgs cefMainArgs, CefApp cefApp, Container container)
        {
            // Register Cef related instances
            container.RegisterInstance(cefMainArgs);
            container.RegisterInstance(cefApp);

            // Register renderer (TODO: maybe make it configurable?)
            container.Register<IRenderer, SdlRenderer>(Lifestyle.Singleton);

            // Register main components
            container.RegisterSingleton<EventProcessor>();
            container.RegisterSingleton<SceneManager>();
            container.RegisterSingleton<MenuRenderer>();
            container.RegisterSingleton<MenuBrowserClient>();
            container.RegisterSingleton<RetroCoreCollection>();
            container.RegisterSingleton<MenuScene>();
            container.RegisterSingleton<StateManager>();

            // Register API components
            container.RegisterSingleton<ApiRouter>();
            container.Register<CefRequestHandler, ApiRequestHandler>(Lifestyle.Singleton);
            container.Collection.Register<IAction>(Assembly.GetExecutingAssembly());
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