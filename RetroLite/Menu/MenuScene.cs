using System;
using System.Diagnostics;
using System.IO;
using NLog;
using RetroLite.Input;
using RetroLite.Scene;
using Xilium.CefGlue;

namespace RetroLite.Menu
{
    public class MenuScene: IScene
    {
        private static MenuScene _instance = null;
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        
        private GameController _prevControllerState;
        private GameControllerButton[] _buttons;
        private GameControllerAnalog[] _analogs;
        private int[] _buttonRepeatCounter;
        private const int RepeatFrameLimit = 30;

        private readonly MenuCefApp _menuCefApp;
        private CefBrowser _browser = null;
        private MenuBrowserClient _browserClient = null;
        private SceneManager _manager = null;
        
        private MenuScene()
        {
            _prevControllerState = new GameController();
            _buttons = (GameControllerButton[]) Enum.GetValues(typeof(GameControllerButton));
            _analogs = (GameControllerAnalog[]) Enum.GetValues(typeof(GameControllerAnalog));
            _buttonRepeatCounter = new int[_buttons.Length];
            _menuCefApp = new MenuCefApp();
            
            var mainArgs = new CefMainArgs(new string[]
            {
                "--disable-gpu",
                "--disable-gpu-compositing",
                "--enable-begin-frame-scheduling"
            });

            CefRuntime.ExecuteProcess(mainArgs, _menuCefApp, IntPtr.Zero);
            
            var settings = new CefSettings()
            {
                //By default CefSharp will use an in-memory cache, you need to specify a Cache Folder to persist data
                CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache"),
                WindowlessRenderingEnabled = true,
                LogSeverity = CefLogSeverity.Info,
                LogFile = "cef.log"
            };
            
            CefRuntime.Initialize(mainArgs, settings, _menuCefApp, IntPtr.Zero);
        }
        
        public static MenuScene GetInstance()
        {
            return _instance ?? (_instance = new MenuScene());
        }
        
        public void Init(SceneManager manager)
        {
            Debug.Assert(_manager == null);
            _manager = manager;
            
            // Instruct CEF to not render to a window at all.
            var cefWindowInfo = CefWindowInfo.Create();
            cefWindowInfo.SetAsWindowless(IntPtr.Zero, true);

            var browserSettings = new CefBrowserSettings()
            {
                WindowlessFrameRate = 60
            };
            
            _browserClient = new MenuBrowserClient(manager);
            _browser = CefBrowserHost.CreateBrowserSync(cefWindowInfo, _browserClient, browserSettings, "http://www.google.com");
        }

        public void Cleanup()
        {
            _browser.Dispose();
            _browser = null;
            _browserClient = null;
            _manager = null;
        }

        public void Pause()
        {
        }

        public void Resume()
        {
        }

        public void HandleEvents()
        {
            var menuController = _manager.EventProcessor[0];

            foreach (var button in _buttons)
            {
                var currentState = menuController.GetButtonState(button);
                var transitioning = currentState != _prevControllerState.GetButtonState(button);
                var eventType = currentState ? CefKeyEventType.KeyDown : CefKeyEventType.KeyUp;

                if (!transitioning)
                {
                    if (!currentState) continue;
                    
                    _buttonRepeatCounter[(int) button] = (_buttonRepeatCounter[(int) button] + 1) % RepeatFrameLimit;

                    if (_buttonRepeatCounter[(int) button] != 0) continue;
                    
                    var keyEvent = new CefKeyEvent();
                    keyEvent.EventType = eventType;
                    keyEvent.WindowsKeyCode = (short) _manager.EventProcessor.GetVirtualKey(button);
                    _browser.GetHost().SendKeyEvent(keyEvent);
                }
                else
                {
                    _buttonRepeatCounter[(int) button] = 0;
                    var keyEvent = new CefKeyEvent();
                    keyEvent.EventType = eventType;
                    keyEvent.WindowsKeyCode = (short) _manager.EventProcessor.GetVirtualKey(button);
                    _browser.GetHost().SendKeyEvent(keyEvent);                    
                }
            }
            
            _prevControllerState.CopyStateFrom(menuController);
        }

        public void Update()
        {
            CefRuntime.DoMessageLoopWork();
        }

        public void Draw()
        {
            throw new System.NotImplementedException();
        }
    }
}