using Xilium.CefGlue;

namespace RetroLite.Menu
{
    public class MenuCefApp : CefApp
    {
        protected override void OnBeforeCommandLineProcessing(string processType, CefCommandLine commandLine)
        {
            commandLine.AppendSwitch("off-screen-frame-rate", "60");
            commandLine.AppendSwitch("disable-gpu");
            commandLine.AppendSwitch("disable-gpu-compositing");
            commandLine.AppendSwitch("enable-begin-frame-scheduling");
            commandLine.AppendSwitch("off-screen-rendering-enabled");
//            commandLine.AppendSwitch("disable-gpu-vsync");
//            commandLine.AppendSwitch("use-angle", "warp");
            commandLine.AppendSwitch("show-fps-counter");
        }
    }
}