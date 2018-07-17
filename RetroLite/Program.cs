using System;
using Redbus;
using RetroLite.DB;
using RetroLite.Input;
using RetroLite.Intro;
using RetroLite.Menu;
using RetroLite.RetroCore;
using RetroLite.Scene;
using RetroLite.Video;
using SDL2;
using SimpleInjector;
using SRC_CS;
using Xilium.CefGlue;
using Xt;

namespace RetroLite
{
    internal static class Program
    {
        public static NLog.Logger Logger { get; } = NLog.LogManager.GetCurrentClassLogger();
        public static EventBus EventBus { get; } = new EventBus();
        public static StateManager StateManager { get; } = new StateManager();
        
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

            SceneManager sceneManager = null;
            try
            {
                Logger.Info("Initializing SDL");

                if (SDL.SDL_Init(SDL.SDL_INIT_JOYSTICK | SDL.SDL_INIT_VIDEO) != 0)
                {
                    Logger.Error(new Exception("SDL Initialization error"));
                }
                
                // Container
                var container = new Container();
                
                container.RegisterInstance<CefMainArgs>(cefMainArgs);
                container.RegisterInstance<CefApp>(cefApp);
                container.RegisterSingleton<EventProcessor>();
                container.Register<IRenderer, SdlRenderer>(Lifestyle.Singleton);
                container.RegisterSingleton<SceneManager>();
                container.RegisterSingleton<MenuRenderer>();
                container.RegisterSingleton<MenuBrowserClient>();
                container.RegisterSingleton<RetroCoreCollection>();
                container.RegisterSingleton<MenuScene>();

                
                // Initialize Components
//                var eventProcessor = new EventProcessor();
//                IRenderer renderer = new SdlRenderer();
//                sceneManager = new SceneManager(renderer, eventProcessor);
//                var menuRenderer = new MenuRenderer(renderer);
//                var menuBrowserClient = new MenuBrowserClient(menuRenderer);
//                var retroLiteCollection = new RetroCoreCollection(sceneManager, eventProcessor, renderer);
//                var menuScene = new MenuScene(
//                    cefMainArgs, 
//                    sceneManager, 
//                    eventProcessor, 
//                    menuBrowserClient, 
//                    cefApp,
//                    retroLiteCollection);

                sceneManager.ChangeScene(new IntroScene(renderer));
                sceneManager.Running = true;

                while (sceneManager.Running)
                {
                    sceneManager.RunLoop();
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
                sceneManager?.Cleanup();
                SDL.SDL_Quit();                
                CefRuntime.Shutdown();
            }
        }
    }
}