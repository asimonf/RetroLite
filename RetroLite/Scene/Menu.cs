using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Redbus;
using RetroLite.DB;
using RetroLite.Event;
using RetroLite.Input;
using RetroLite.Menu;
using RetroLite.RetroCore;
using RetroLite.Scene;
using Xilium.CefGlue;

namespace RetroLite.Scene
{
    public class Menu : IScene
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private GameControllerAnalog[] _analogs;
        private const int RepeatFrameLimit = 30;

        private readonly GameControllerButton[] _buttons;
        private readonly List<SubscriptionToken> _eventTokenList;
        private readonly CefBrowser _browser;
        private readonly MenuBrowserClient _browserClient;
        private readonly InputProcessor _inputProcessor;
        private readonly EventBus _eventBus;

        public int Order => 1;

        public Menu(
            CefApp menuCeffApp,
            CefMainArgs mainArgs,
            InputProcessor inputProcessor,
            MenuBrowserClient browserClient,
            EventBus eventBus
        )
        {
            // State Initialization
            _buttons = (GameControllerButton[])Enum.GetValues(typeof(GameControllerButton));
            _analogs = (GameControllerAnalog[])Enum.GetValues(typeof(GameControllerAnalog));
            _eventTokenList = new List<SubscriptionToken>();
            _eventBus = eventBus;

            var settings = new CefSettings
            {
                WindowlessRenderingEnabled = true, 
                LogSeverity = CefLogSeverity.Debug,
                NoSandbox = true
            };

            CefRuntime.Initialize(mainArgs, settings, menuCeffApp, IntPtr.Zero);

            // Instruct CEF to not render to a window at all.
            var cefWindowInfo = CefWindowInfo.Create();
            cefWindowInfo.SetAsWindowless(IntPtr.Zero, false);

            var browserSettings = new CefBrowserSettings()
            {
                WindowlessFrameRate = 60,
            };

            _inputProcessor = inputProcessor;
            _browserClient = browserClient;
            _browser = CefBrowserHost.CreateBrowserSync(
                cefWindowInfo,
                _browserClient,
                browserSettings,
                "http://localhost:4200"
//                "https://browserbench.org/MotionMark/"
            );

            _eventTokenList.Add(_eventBus.Subscribe<OpenMenuEvent>(OnOpenMenuEvent));
            _eventTokenList.Add(_eventBus.Subscribe<CloseMenuEvent>(OnCloseMenuEvent));
        }

        ~Menu()
        {
            foreach (var token in _eventTokenList)
            {
                _eventBus.Unsubscribe(token);
            }

            _eventTokenList.Clear();
            
            CefRuntime.Shutdown();
        }

        private void OnOpenMenuEvent(OpenMenuEvent openMenuEvent)
        {
            _inputProcessor.ResetControllers();
        }
        
        private void OnCloseMenuEvent(CloseMenuEvent closeMenuEvent)
        {
            _inputProcessor.ResetControllers();
        }

        public void HandleEvents()
        {
            var menuController = _inputProcessor[0];

            for (var index = 0; index < _buttons.Length; index++)
            {
                var button = _buttons[index];
                var currentState = menuController.GetButtonState(button);

                if (currentState == GameControllerButtonState.None) continue;

                var eventType = currentState == GameControllerButtonState.Down
                    ? CefKeyEventType.KeyDown
                    : CefKeyEventType.KeyUp;
                var cefKeyEvent = new CefKeyEvent
                {
                    EventType = eventType,
                    WindowsKeyCode = (short) InputProcessor.GetVirtualKey(button)
                };

                _browser.GetHost().SendKeyEvent(cefKeyEvent);

                if (button == GameControllerButton.Start &&
                    currentState == GameControllerButtonState.Up)
                {
                    _browser.GetHost().SendMouseClickEvent(new CefMouseEvent()
                    {
                        X = _inputProcessor.X,
                        Y = _inputProcessor.Y
                    }, CefMouseButtonType.Left, false, 1);
                    
                    _browser.GetHost().SendMouseClickEvent(new CefMouseEvent()
                    {
                        X = _inputProcessor.X,
                        Y = _inputProcessor.Y
                    }, CefMouseButtonType.Left, true, 1);
                }
            }
        }

        public void Update()
        {
            CefRuntime.DoMessageLoopWork();
        }

        public void Draw()
        {
            _browserClient.Draw();
        }

        public float[] GetAudioData(int frames)
        {
            return null;
        }
        
        
        public int CompareTo(IScene other)
        {
            if (other == null) return 1;

            return Order.CompareTo(other.Order);
        }
    }
}