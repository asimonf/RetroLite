using System;
using RetroLite.Input;
using RetroLite.Menu;
using RetroLite.Scene;
using RetroLite.Video;
using SDL2;
using Xilium.CefGlue;

namespace RetroLite
{
    public class Container
    {
        public IRenderer Renderer { get; private set; }
        public EventProcessor EventProcessor { get; private set; }
        public SceneManager SceneManager { get; private set; }
        public MenuScene MenuScene { get; private set; }

        public CefMainArgs CefMainArgs { get; private set; }
        public MenuBrowserClient MenuBrowserClient { get; private set; }
        public MenuCefApp MenuCefApp { get; private set; }
        public MenuRenderer MenuRenderer { get; private set; }

        public Container(CefMainArgs cefMainArgs)
        {
            Console.WriteLine("Initializing Video");
            
            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO | SDL.SDL_INIT_GAMECONTROLLER) != 0)
            {
                Console.WriteLine("Init error");
                throw new Exception("SDL Video Initialization error");
            }
            
            CefMainArgs = cefMainArgs;
            MenuCefApp = new MenuCefApp();
            Renderer = new SdlRenderer(1024, 768);
            EventProcessor = new EventProcessor();
            SceneManager = new SceneManager(Renderer, EventProcessor);
            MenuRenderer = new MenuRenderer(Renderer);
            MenuBrowserClient = new MenuBrowserClient(MenuRenderer);
            MenuScene = new MenuScene(CefMainArgs, SceneManager, EventProcessor, MenuBrowserClient);
        }

        public void Shutdown()
        {
            CefRuntime.Shutdown();
            SDL.SDL_Quit();
        }
    }
}