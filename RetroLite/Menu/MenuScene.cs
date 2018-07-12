using System;
using System.Collections.Generic;
using System.IO;
using Redbus;
using RetroLite.Event;
using RetroLite.Input;
using RetroLite.RetroCore;
using RetroLite.Scene;
using Xilium.CefGlue;

namespace RetroLite.Menu
{
    public class MenuScene : IScene
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private GameControllerAnalog[] _analogs;
        private const int RepeatFrameLimit = 30;

        private readonly int[] _buttonRepeatCounter;
        private readonly GameControllerButton[] _buttons;
        private readonly GameController _prevControllerState;
        private readonly List<SubscriptionToken> _eventTokenList;
        private readonly CefBrowser _browser;
        private readonly MenuBrowserClient _browserClient;
        private readonly SceneManager _manager;
        private readonly EventProcessor _eventProcessor;
        private readonly RetroLiteCollection _coreCollection;

        public bool IsLoaded => !_browser.IsLoading;

        private bool _isCoreStarted = false;
        private bool _isCoreRunning = false;
        private bool _isMenuOpen = false;

        public MenuScene(
            CefMainArgs mainArgs,
            SceneManager manager,
            EventProcessor eventProcessor,
            MenuBrowserClient browserClient,
            CefApp menuCeffApp,
            RetroLiteCollection coreCollection)
        {
            // State Initialization
            _buttons = (GameControllerButton[]) Enum.GetValues(typeof(GameControllerButton));
            _analogs = (GameControllerAnalog[]) Enum.GetValues(typeof(GameControllerAnalog));
            _prevControllerState = new GameController();
            _buttonRepeatCounter = new int[_buttons.Length];
            _eventTokenList = new List<SubscriptionToken>();

            var settings = new CefSettings {WindowlessRenderingEnabled = true, LogSeverity = CefLogSeverity.Default};

            CefRuntime.Initialize(mainArgs, settings, menuCeffApp, windowsSandboxInfo: IntPtr.Zero);

            // Instruct CEF to not render to a window at all.
            var cefWindowInfo = CefWindowInfo.Create();
            cefWindowInfo.SetAsWindowless(parentHandle: IntPtr.Zero, transparent: true);

            var browserSettings = new CefBrowserSettings()
            {
                WindowlessFrameRate = 60
            };

            _manager = manager;
            _eventProcessor = eventProcessor;
            _browserClient = browserClient;
            _coreCollection = coreCollection;
            _browser = CefBrowserHost.CreateBrowserSync(
                cefWindowInfo,
                _browserClient,
                browserSettings,
                "http://unixpapa.com/js/testkey.html"
            );

            _eventTokenList.Add(Program.EventBus.Subscribe<OpenMenuEvent>(OnOpenMenuEvent));
            _eventTokenList.Add(Program.EventBus.Subscribe<IntroFinishedEvent>(OnIntroFinishedEvent));
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
            }
        }

        private void OnIntroFinishedEvent(IntroFinishedEvent introFinishedEvent)
        {
            _manager.ChangeScene(this);
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

                foreach (var button in _buttons)
                {
                    var currentState = menuController.GetButtonState(button);
                    
                    if (currentState == GameControllerButtonState.None) continue;
                    
                    var eventType = currentState == GameControllerButtonState.Down ? 
                        CefKeyEventType.KeyDown : CefKeyEventType.KeyUp;
                    var cefKeyEvent = new CefKeyEvent
                    {
                        EventType = eventType,
                        WindowsKeyCode = (short) EventProcessor.GetVirtualKey(button)
                    };

                    _browser.GetHost().SendKeyEvent(cefKeyEvent);
                    
                    if (!_isCoreStarted && 
                        button == GameControllerButton.B && 
                        currentState == GameControllerButtonState.Up &&
                        _coreCollection.LoadGame(Path.Combine(Environment.CurrentDirectory, "roms/snes/240pSuite.sfc")))
                    {
                        _isCoreStarted = true;
                        _isCoreRunning = true;
                        _isMenuOpen = false;
                    }

                    if (button == GameControllerButton.Guide && 
                        currentState == GameControllerButtonState.Up &&
                        _isMenuOpen && 
                        _isCoreStarted)
                    {
                        _isMenuOpen = false;
                        _isCoreRunning = true;
                        Console.WriteLine("Closing Menu");
                    } 
                }
            }
        }

        public void Update()
        {
            if (_isCoreRunning) _coreCollection.Update();
            CefRuntime.DoMessageLoopWork();
        }

        public void Draw()
        {
            if (_isCoreStarted) _coreCollection.Draw();
            if (_isMenuOpen) _browserClient.Draw();
        }
    }
}