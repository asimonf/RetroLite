using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Redbus;
using RetroLite.Event;
using RetroLite.Input;
using RetroLite.RetroCore;
using RetroLite.Scene;
using Xilium.CefGlue;

namespace RetroLite.Menu
{
    public enum BrowserEvent
    {
        OpenMenu,
        CloseMenu
    }
    
    public class MenuScene : IScene
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private GameControllerAnalog[] _analogs;
        private const int RepeatFrameLimit = 30;

        private readonly GameControllerButton[] _buttons;
        private readonly List<SubscriptionToken> _eventTokenList;
        private readonly CefBrowser _browser;
        private readonly MenuBrowserClient _browserClient;
        private readonly SceneManager _manager;
        private readonly EventProcessor _eventProcessor;
        private readonly RetroCoreCollection _coreCollection;

        public bool IsLoaded => !_browser.IsLoading;

        private bool _isCoreStarted;
        private bool _isCoreRunning;
        private bool _isMenuOpen;

        public MenuScene(
            CefMainArgs mainArgs,
            SceneManager manager,
            EventProcessor eventProcessor,
            MenuBrowserClient browserClient,
            CefApp menuCeffApp,
            RetroCoreCollection coreCollection)
        {
            // State Initialization
            _buttons = (GameControllerButton[])Enum.GetValues(typeof(GameControllerButton));
            _analogs = (GameControllerAnalog[])Enum.GetValues(typeof(GameControllerAnalog));
            _eventTokenList = new List<SubscriptionToken>();

            var settings = new CefSettings
            {
                WindowlessRenderingEnabled = true, 
                LogSeverity = CefLogSeverity.Disable,
                NoSandbox = true,
                SingleProcess = true
            };

            CefRuntime.Initialize(mainArgs, settings, menuCeffApp, windowsSandboxInfo: IntPtr.Zero);

            // Instruct CEF to not render to a window at all.
            var cefWindowInfo = CefWindowInfo.Create();
            cefWindowInfo.SetAsWindowless(parentHandle: IntPtr.Zero, transparent: false);

            var browserSettings = new CefBrowserSettings()
            {
                WindowlessFrameRate = 60,
            };

            _manager = manager;
            _eventProcessor = eventProcessor;
            _browserClient = browserClient;
            _coreCollection = coreCollection;
            _browser = CefBrowserHost.CreateBrowserSync(
                cefWindowInfo,
                _browserClient,
                browserSettings,
                "http://localhost:4200"
//                "https://browserbench.org/MotionMark/"
            );

            _eventTokenList.Add(Program.EventBus.Subscribe<OpenMenuEvent>(OnOpenMenuEvent));
        }

        ~MenuScene()
        {
            foreach (var token in _eventTokenList)
            {
                Program.EventBus.Unsubscribe(token);
            }

            _eventTokenList.Clear();
        }

        private void OnOpenMenuEvent(OpenMenuEvent openMenuEvent)
        {
            if (!_isMenuOpen)
            {
                _isCoreRunning = false;
                _isMenuOpen = true;
                _eventProcessor.ResetControllers();
                _sendBrowserEvent(BrowserEvent.OpenMenu);
            }
        }

        public void Start()
        {
            _isMenuOpen = true;
        }

        public void Stop()
        {
            _isMenuOpen = false;
            _isCoreStarted = false;
            _isCoreRunning = false;
        }

        public void Pause()
        {
        }

        public void Resume()
        {
        }

        public void HandleEvents()
        {
            if (_isCoreRunning)
            {
                _coreCollection.HandleEvents();
            }
            else
            {
                var menuController = _eventProcessor[0];

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
                        WindowsKeyCode = (short) EventProcessor.GetVirtualKey(button)
                    };

                    _browser.GetHost().SendKeyEvent(cefKeyEvent);
                    
                    if (!_isCoreStarted &&
                        button == GameControllerButton.A &&
                        currentState == GameControllerButtonState.Up)
                    {
                        Program.StateManager.ScanForGames(Path.Combine(Environment.CurrentDirectory, "roms"));
                    }

                    if (!_isCoreStarted &&
                        button == GameControllerButton.B &&
                        currentState == GameControllerButtonState.Up &&
                        _coreCollection.LoadGame(Path.Combine(Environment.CurrentDirectory,
                            Program.StateManager.GetGameList()[3].Path)))
                    {
                        _isCoreStarted = true;
                        _isCoreRunning = true;
                        _isMenuOpen = false;
                        _eventProcessor.ResetControllers();
                        _sendBrowserEvent(BrowserEvent.CloseMenu);
                    }

                    if (button == GameControllerButton.Guide &&
                        currentState == GameControllerButtonState.Up &&
                        _isMenuOpen &&
                        _isCoreStarted)
                    {
                        _isMenuOpen = false;
                        _isCoreRunning = true;
                        _logger.Debug("Closing Menu");
                        _eventProcessor.ResetControllers();
                        _sendBrowserEvent(BrowserEvent.CloseMenu);
                    }

                    if (button == GameControllerButton.Start &&
                        currentState == GameControllerButtonState.Up)
                    {
                        _browser.GetHost().SendMouseClickEvent(new CefMouseEvent()
                        {
                            X = _eventProcessor.X,
                            Y = _eventProcessor.Y
                        }, CefMouseButtonType.Left, false, 1);
                        
                        _browser.GetHost().SendMouseClickEvent(new CefMouseEvent()
                        {
                            X = _eventProcessor.X,
                            Y = _eventProcessor.Y
                        }, CefMouseButtonType.Left, true, 1);
                    }
                }
            }
        }

        public void Update()
        {
            CefRuntime.DoMessageLoopWork();
            if (_isCoreRunning) _coreCollection.Update();
        }

        public void Draw()
        {
            if (_isCoreStarted) _coreCollection.Draw();
            _browserClient.Draw();
        }

        public void GetAudioData(IntPtr buffer, int frames)
        {
            if (_isCoreRunning) _coreCollection.GetAudioData(buffer, frames);
            else for(var i=0; i < frames * 2 * 2; i++)
            {
                Marshal.WriteByte(buffer, i, 0);
            }
        }

        private void _sendBrowserEvent(BrowserEvent browserEvent)
        {
            var frame = _browser.GetMainFrame();
            switch (browserEvent)
            {
                case BrowserEvent.OpenMenu:
                    frame.ExecuteJavaScript("setTimeout(() => document.dispatchEvent(new Event('openmenu')), 0)", frame.Url, 0);
                    break;
                case BrowserEvent.CloseMenu:
                    frame.ExecuteJavaScript("setTimeout(() => document.dispatchEvent(new Event('closemenu')), 0)", frame.Url, 0);
                    break;
            }
        }
    }
}