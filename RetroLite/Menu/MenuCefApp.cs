using Xilium.CefGlue;

namespace RetroLite.Menu
{
    public class MenuCefApp : CefApp
    {
        protected override void OnBeforeCommandLineProcessing(string processType, CefCommandLine commandLine)
        {
            commandLine.AppendSwitch("disable-gpu");
            commandLine.AppendSwitch("disable-gpu-compositing");
            commandLine.AppendSwitch("enable-begin-frame-scheduling");
            commandLine.AppendSwitch("off-screen-rendering-enabled");
            commandLine.AppendSwitch("disable-gpu-sandbox");
            commandLine.AppendSwitch("off-screen-frame-rate", "60");
            commandLine.AppendSwitch("use-angle", "gles");
            commandLine.AppendSwitch("use-gl", "swiftshader");
        }

        protected override CefRenderProcessHandler GetRenderProcessHandler()
        {
            return base.GetRenderProcessHandler();
        }
    }
}