using System;
using Redbus;
using RetroLite.Input;
using RetroLite.Intro;
using RetroLite.Menu;
using RetroLite.RetroCore;
using RetroLite.Scene;
using RetroLite.Video;
using SDL2;
using SRC_CS;
using Xilium.CefGlue;
using Xt;

namespace RetroLite
{
    internal static class Program
    {
        public static EventBus EventBus { get; } = new EventBus();
        
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
                Console.WriteLine("Initializing SDL");

                if (SDL.SDL_Init(SDL.SDL_INIT_JOYSTICK | SDL.SDL_INIT_VIDEO) != 0)
                {
                    Console.WriteLine("SDL Init error");
                    throw new Exception("SDL Initialization error");
                }
                
                // Initialize Components
                var eventProcessor = new EventProcessor();
                IRenderer renderer = new SdlRenderer(1024, 768);
                sceneManager = new SceneManager(renderer, eventProcessor);
                var menuRenderer = new MenuRenderer(renderer);
                var menuBrowserClient = new MenuBrowserClient(menuRenderer);
                var retroLiteCollection = new RetroLiteCollection(sceneManager, eventProcessor, renderer);
                var menuScene = new MenuScene(
                    cefMainArgs, 
                    sceneManager, 
                    eventProcessor, 
                    menuBrowserClient, 
                    cefApp,
                    retroLiteCollection);

                sceneManager.ChangeScene(new IntroScene(renderer));
                sceneManager.Running = true;

                while (sceneManager.Running)
                {
                    sceneManager.HandleEvents();
                    sceneManager.Update();
                    sceneManager.Draw();
                }
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error!!!");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
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