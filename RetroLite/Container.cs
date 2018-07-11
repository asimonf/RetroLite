using System;
using RetroLite.Input;
using RetroLite.Intro;
using RetroLite.Menu;
using RetroLite.RetroCore;
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
        public IntroScene IntroScene { get; private set; }
        public MenuScene MenuScene { get; private set; }
        public RetroLiteCollection RetroLiteCollection { get; private set; }

        public CefMainArgs CefMainArgs { get; private set; }
        public MenuBrowserClient MenuBrowserClient { get; private set; }
        public MenuCefApp MenuCefApp { get; private set; }
        public MenuRenderer MenuRenderer { get; private set; }

        public Container(CefMainArgs cefMainArgs)
        {
            CefMainArgs = cefMainArgs;
            MenuCefApp = new MenuCefApp();
            Renderer = new SoftwareRenderer(1024, 768);
            EventProcessor = new EventProcessor();
            SceneManager = new SceneManager(Renderer, EventProcessor);
            MenuRenderer = new MenuRenderer(Renderer);
            MenuBrowserClient = new MenuBrowserClient(MenuRenderer);
            IntroScene = new IntroScene(Renderer);
            MenuScene = new MenuScene(CefMainArgs, SceneManager, EventProcessor, MenuBrowserClient);
            
            RetroLiteCollection = new RetroLiteCollection(SceneManager, EventProcessor, Renderer);
        }

        public void Shutdown()
        {
            CefRuntime.Shutdown();
            SDL.SDL_Quit();
        }
    }
}