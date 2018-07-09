using System;
using RetroLite.Intro;
using RetroLite.Menu;
using RetroLite.Scene;
using SDL2;
using SRC_CS;
using Xilium.CefGlue;
using Xt;

namespace RetroLite
{
    internal class Program
    {
        [STAThread]
        public static unsafe int Main(string[] args)
        {
            var cefMainArgs = new CefMainArgs(args);

            var res = CefRuntime.ExecuteProcess(cefMainArgs, null, IntPtr.Zero);
            
            if (res >= 0)
            {
                return res;
            }
            
            var container = new Container(cefMainArgs);
            var manager = container.SceneManager;
            manager.ChangeScene(container.MenuScene);
            manager.Running = true;

            while (container.SceneManager.Running)
            {
                manager.HandleEvents();
                manager.Update();
                manager.Draw();
            }
            
            manager.Cleanup();
            container.Shutdown();
            
            return 0;
        }
    }
}